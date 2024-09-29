using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Trolley;

public abstract partial class BaseOrmProvider : IOrmProvider
{
    protected static readonly ConcurrentDictionary<int, MemberAccessSqlFormatter> memberAccessSqlFormatterCache = new();
    protected static readonly ConcurrentDictionary<int, MethodCallSqlFormatter> methodCallSqlFormatterCache = new();
    protected static readonly ConcurrentDictionary<int, Delegate> methodCallCache = new();
    protected static readonly ConcurrentDictionary<Type, ITypeHandler> typeHandlers = new();
    protected static readonly ConcurrentDictionary<int, Func<object, object>> parameterValueGetters = new();
    protected static readonly ConcurrentDictionary<int, Func<object, object>> readerValueGetters = new();
    static BaseOrmProvider()
    {
        typeHandlers.TryAdd(typeof(JsonTypeHandler), Activator.CreateInstance(typeof(JsonTypeHandler)) as ITypeHandler);
        typeHandlers.TryAdd(typeof(ToStringTypeHandler), Activator.CreateInstance(typeof(ToStringTypeHandler)) as ITypeHandler);
    }
    public virtual OrmProviderType OrmProviderType => OrmProviderType.Basic;
    public virtual string ParameterPrefix => "@";
    public abstract Type NativeDbTypeType { get; }
    public virtual string DefaultTableSchema { get; }
    public virtual ICollection<ITypeHandler> TypeHandlers => typeHandlers.Values;
    public abstract ITheaConnection CreateConnection(string dbKey, string connectionString);
    public abstract IDbCommand CreateCommand();
    public abstract IDbDataParameter CreateParameter(string parameterName, object value);
    public abstract IDbDataParameter CreateParameter(string parameterName, object nativeDbType, object value);
    public abstract void ChangeParameter(object dbParameter, Type targetType, object value);
    public abstract IRepository CreateRepository(DbContext dbContext);

    public virtual string GetTableName(string entityName) => entityName;
    public virtual string GetFieldName(string fieldName) => fieldName;
    public virtual string GetPagingTemplate(int? skip, int? limit, string orderBy = null)
    {
        var builder = new StringBuilder("SELECT /**fields**/ FROM /**tables**/ /**others**/");
        if (!String.IsNullOrEmpty(orderBy)) builder.Append($" {orderBy}");
        if (limit.HasValue) builder.Append($" LIMIT {limit}");
        if (skip.HasValue && skip.Value > 0) builder.Append($" OFFSET {skip}");
        return builder.ToString();
    }
    public abstract object GetNativeDbType(Type type);
    public abstract Type MapDefaultType(object nativeDbType);
    public abstract string CastTo(Type type, object value, string characterSetOrCollation = null);
    public virtual string GetIdentitySql(string keyField) => ";SELECT @@IDENTITY";
    public virtual string GetQuotedValue(Type expectType, object value)
    {
        if (value == null) return "NULL";
        switch (expectType)
        {
            case Type factType when factType == typeof(bool):
                return Convert.ToBoolean(value) ? "1" : "0";
            case Type factType when factType == typeof(string):
                return $"'{Convert.ToString(value).Replace("'", @"\'")}'";
            case Type factType when factType == typeof(Guid):
                return $"'{value}'";
            case Type factType when factType == typeof(DateTime):
                return $"'{Convert.ToDateTime(value):yyyy\\-MM\\-dd\\ HH\\:mm\\:ss\\.fff}'";
            case Type factType when factType == typeof(DateTimeOffset):
                return $"'{(DateTimeOffset)value:yyyy\\-MM\\-dd\\ HH\\:mm\\:ss\\.fffZ}'";
#if NET6_0_OR_GREATER
            case Type factType when factType == typeof(DateOnly):
                return $"'{(DateOnly)value:yyyy\\-MM\\-dd}'";
#endif
            case Type factType when factType == typeof(TimeSpan):
                {
                    var factValue = (TimeSpan)value;
                    if (factValue.TotalDays > 1 || factValue.TotalDays < -1)
                        return $"'{(int)factValue.TotalDays}.{factValue:hh\\:mm\\:ss\\.ffffff}'";
                    return $"'{factValue:hh\\:mm\\:ss\\.ffffff}'";
                }
#if NET6_0_OR_GREATER
            case Type factType when factType == typeof(TimeOnly): return $"'{(TimeOnly)value:hh\\:mm\\:ss\\.ffffff}'";
#endif
            case Type factType when factType == typeof(SqlFieldSegment):
                {
                    var sqlSegment = value as SqlFieldSegment;
                    if (sqlSegment.IsConstant || sqlSegment.IsVariable)
                        return this.GetQuotedValue(sqlSegment.Value);
                    return sqlSegment.Body;
                }
            default: return value.ToString();
        }
    }
    public virtual string GetBinaryOperator(ExpressionType nodeType) =>
        nodeType switch
        {
            ExpressionType.Equal => "=",
            ExpressionType.NotEqual => "<>",
            ExpressionType.GreaterThan => ">",
            ExpressionType.GreaterThanOrEqual => ">=",
            ExpressionType.LessThan => "<",
            ExpressionType.LessThanOrEqual => "<=",
            ExpressionType.AndAlso => "AND",
            ExpressionType.OrElse => "OR",
            ExpressionType.Add => "+",
            ExpressionType.Subtract => "-",
            ExpressionType.Multiply => "*",
            ExpressionType.Divide => "/",
            ExpressionType.Modulo => "%",
            ExpressionType.Coalesce => "COALESCE",
            ExpressionType.And => "&",
            ExpressionType.Or => "|",
            ExpressionType.ExclusiveOr => "^",
            ExpressionType.LeftShift => "<<",
            ExpressionType.RightShift => ">>",
            _ => nodeType.ToString()
        };
    public virtual Func<object, object> GetParameterValueGetter(Type fromType, Type fieldType, bool isNullable)
    {
        var hashKey = RepositoryHelper.GetCacheKey(fromType, fieldType, isNullable);
        return parameterValueGetters.GetOrAdd(hashKey, f =>
        {
            var underlyingType = Nullable.GetUnderlyingType(fromType);
            var isNullableType = underlyingType != null;
            underlyingType ??= fromType;
            Func<object, object> typeHandler = null;

            if (fromType == fieldType && fromType.IsValueType || fromType == typeof(DBNull))
                typeHandler = value => value;
            else if (underlyingType == fieldType)
            {
                if (isNullable || !fromType.IsValueType)
                {
                    typeHandler = value =>
                    {
                        if (value == null) return DBNull.Value;
                        return value;
                    };
                }
                else typeHandler = value => value;
            }
            else
            {
                //当前参数类型是非空类型，尽管数据库可为null，当作非空类型处理
                if (underlyingType.IsArray && fieldType == typeof(Array))
                {
                    typeHandler = value =>
                    {
                        if (value is DBNull) return null;
                        return Convert.ChangeType(value, underlyingType);
                    };
                }
                else if (underlyingType.IsEnumType(out _))
                {
                    var enumUnderlyingType = Enum.GetUnderlyingType(underlyingType);
                    var supportedTypes = new Type[] { typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) };
                    if (fieldType == typeof(string))
                    {
                        //参数类型可为null，数据库一定可为null
                        if (isNullableType && isNullable)
                        {
                            typeHandler = value =>
                            {
                                if (value == null) return DBNull.Value;
                                return Enum.GetName(underlyingType, value);
                            };
                        }
                        else typeHandler = value => Enum.GetName(underlyingType, value);
                    }
                    else if (enumUnderlyingType != fieldType && supportedTypes.Contains(fieldType))
                    {
                        if (isNullableType && isNullable)
                        {
                            typeHandler = value =>
                            {
                                if (value == null) return DBNull.Value;
                                var numberValue = Convert.ChangeType(value, enumUnderlyingType);
                                return Convert.ChangeType(numberValue, fieldType);
                            };
                        }
                        else typeHandler = value =>
                        {
                            var numberValue = Convert.ChangeType(value, enumUnderlyingType);
                            return Convert.ChangeType(numberValue, fieldType);
                        };
                    }
                    else
                    {
                        if (isNullableType && isNullable)
                        {
                            typeHandler = value =>
                            {
                                if (value == null) return DBNull.Value;
                                return Convert.ChangeType(value, enumUnderlyingType);
                            };
                        }
                        else typeHandler = value => Convert.ChangeType(value, enumUnderlyingType);
                    }
                }
                else
                {
                    if (fieldType == typeof(Guid))
                    {
                        if (underlyingType == typeof(string))
                        {
                            if (isNullableType && isNullable)
                            {
                                typeHandler = value =>
                                {
                                    if (value == null) return DBNull.Value;
                                    return new Guid((string)value);
                                };
                            }
                            else typeHandler = value => new Guid((string)value);
                        }
                        else if (underlyingType == typeof(byte[]))
                        {
                            if (isNullableType && isNullable)
                            {
                                typeHandler = value =>
                                {
                                    if (value == null) return DBNull.Value;
                                    return new Guid((byte[])value);
                                };
                            }
                            else typeHandler = value => new Guid((byte[])value);
                        }
                    }
                    else if (fieldType == typeof(DateTimeOffset))
                    {
                        if (underlyingType == typeof(string))
                        {
                            if (isNullableType && isNullable)
                            {
                                typeHandler = value =>
                                {
                                    if (value == null) return DBNull.Value;
                                    return DateTimeOffset.Parse((string)value);
                                };
                            }
                            else typeHandler = value => DateTimeOffset.Parse((string)value);
                        }
                        else if (underlyingType == typeof(DateTime))
                        {
                            if (isNullableType && isNullable)
                            {
                                typeHandler = value =>
                                {
                                    if (value == null) return DBNull.Value;
                                    return new DateTimeOffset((DateTime)value);
                                };
                            }
                            else typeHandler = value => new DateTimeOffset((DateTime)value);
                        }
                    }
                    else if (fieldType == typeof(DateTime))
                    {
                        if (underlyingType == typeof(string))
                        {
                            if (isNullableType && isNullable)
                            {
                                typeHandler = value =>
                                {
                                    if (value == null) return DBNull.Value;
                                    return DateTime.Parse((string)value);
                                };
                            }
                            else typeHandler = value => DateTime.Parse((string)value);
                        }
#if NET6_0_OR_GREATER
                        else if (underlyingType == typeof(DateOnly))
                        {
                            if (isNullableType && isNullable)
                            {
                                typeHandler = value =>
                                {
                                    if (value == null) return DBNull.Value;
                                    return ((DateOnly)value).ToDateTime(TimeOnly.MinValue);
                                };
                            }
                            else typeHandler = value => ((DateOnly)value).ToDateTime(TimeOnly.MinValue);
                        }
#endif
                        else if (underlyingType == typeof(DateTimeOffset))
                        {
                            if (isNullableType && isNullable)
                            {
                                typeHandler = value =>
                                {
                                    if (value == null) return DBNull.Value;
                                    return ((DateTimeOffset)value).LocalDateTime;
                                };
                            }
                            else typeHandler = value => ((DateTimeOffset)value).LocalDateTime;
                        }
                    }
#if NET6_0_OR_GREATER
                    else if (fieldType == typeof(DateOnly))
                    {
                        if (underlyingType == typeof(string))
                        {
                            if (isNullableType && isNullable)
                            {
                                typeHandler = value =>
                                {
                                    if (value == null) return DBNull.Value;
                                    return DateOnly.Parse((string)value);
                                };
                            }
                            else typeHandler = value => DateOnly.Parse((string)value);
                        }
                        else if (underlyingType == typeof(DateTime))
                        {
                            if (isNullableType && isNullable)
                            {
                                typeHandler = value =>
                                {
                                    if (value == null) return DBNull.Value;
                                    return DateOnly.FromDateTime((DateTime)value);
                                };
                            }
                            else typeHandler = value => DateOnly.FromDateTime((DateTime)value);
                        }
                        else if (underlyingType == typeof(DateTimeOffset))
                        {
                            if (isNullableType && isNullable)
                            {
                                typeHandler = value =>
                                {
                                    if (value == null) return DBNull.Value;
                                    return DateOnly.FromDateTime(((DateTimeOffset)value).LocalDateTime);
                                };
                            }
                            else typeHandler = value => DateOnly.FromDateTime(((DateTimeOffset)value).LocalDateTime);
                        }
                    }
#endif
                    else if (fieldType == typeof(TimeSpan))
                    {
                        if (underlyingType == typeof(long))
                        {
                            if (isNullableType && isNullable)
                            {
                                typeHandler = value =>
                                {
                                    if (value == null) return DBNull.Value;
                                    return TimeSpan.FromTicks((long)value);
                                };
                            }
                            else typeHandler = value => TimeSpan.FromTicks((long)value);
                        }
                        else if (underlyingType == typeof(string))
                        {
                            if (isNullableType && isNullable)
                            {
                                typeHandler = value =>
                                {
                                    if (value == null) return DBNull.Value;
                                    return TimeSpan.Parse((string)value);
                                };
                            }
                            else typeHandler = value => TimeSpan.Parse((string)value);
                        }
#if NET6_0_OR_GREATER
                        else if (underlyingType == typeof(TimeOnly))
                        {
                            if (isNullableType && isNullable)
                            {
                                typeHandler = value =>
                                {
                                    if (value == null) return DBNull.Value;
                                    return ((TimeOnly)value).ToTimeSpan();
                                };
                            }
                            else typeHandler = value => ((TimeOnly)value).ToTimeSpan();
                        }
#endif
                        else if (underlyingType == typeof(DateTime))
                        {
                            if (isNullableType && isNullable)
                            {
                                typeHandler = value =>
                                {
                                    if (value == null) return DBNull.Value;
                                    return ((DateTime)value).TimeOfDay;
                                };
                            }
                            else typeHandler = value => ((DateTime)value).TimeOfDay;
                        }
                        else if (underlyingType == typeof(DateTimeOffset))
                        {
                            if (isNullableType && isNullable)
                            {
                                typeHandler = value =>
                                {
                                    if (value == null) return DBNull.Value;
                                    return ((DateTimeOffset)value).LocalDateTime.TimeOfDay;
                                };
                            }
                            else typeHandler = value => ((DateTimeOffset)value).LocalDateTime.TimeOfDay;
                        }
                    }
#if NET6_0_OR_GREATER
                    else if (fieldType == typeof(TimeOnly))
                    {
                        if (underlyingType == typeof(long))
                        {
                            if (isNullableType && isNullable)
                            {
                                typeHandler = value =>
                                {
                                    if (value == null) return DBNull.Value;
                                    return new TimeOnly((long)value);
                                };
                            }
                            else typeHandler = value => new TimeOnly((long)value);
                        }
                        else if (underlyingType == typeof(string))
                        {
                            if (isNullableType && isNullable)
                            {
                                typeHandler = value =>
                                {
                                    if (value == null) return DBNull.Value;
                                    return TimeOnly.Parse((string)value);
                                };
                            }
                            else typeHandler = value => TimeOnly.Parse((string)value);
                        }
                        else if (underlyingType == typeof(TimeSpan))
                        {
                            if (isNullableType && isNullable)
                            {
                                typeHandler = value =>
                                {
                                    if (value == null) return DBNull.Value;
                                    return TimeOnly.FromTimeSpan((TimeSpan)value);
                                };
                            }
                            else typeHandler = value => TimeOnly.FromTimeSpan((TimeSpan)value);
                        }
                        else if (underlyingType == typeof(DateTime))
                        {
                            if (isNullableType && isNullable)
                            {
                                typeHandler = value =>
                                {
                                    if (value == null) return DBNull.Value;
                                    return TimeOnly.FromTimeSpan(((DateTime)value).TimeOfDay);
                                };
                            }
                            else typeHandler = value => TimeOnly.FromTimeSpan(((DateTime)value).TimeOfDay);
                        }
                        else if (underlyingType == typeof(DateTimeOffset))
                        {
                            if (isNullableType && isNullable)
                            {
                                typeHandler = value =>
                                {
                                    if (value == null) return DBNull.Value;
                                    return TimeOnly.FromTimeSpan(((DateTimeOffset)value).LocalDateTime.TimeOfDay);
                                };
                            }
                            else typeHandler = value => TimeOnly.FromTimeSpan(((DateTimeOffset)value).LocalDateTime.TimeOfDay);
                        }
                    }
#endif
                    else if (fieldType == typeof(string))
                    {
                        if (isNullable)
                        {
                            typeHandler = value =>
                            {
                                if (value == null) return DBNull.Value;
                                return Convert.ToString(value);
                            };
                        }
                        else typeHandler = value => Convert.ToString(value);
                    }
                    else if (fieldType == typeof(bool))
                    {
                        var supportedTypes = new Type[] { typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong) };
                        if (supportedTypes.Contains(underlyingType))
                        {
                            if (isNullableType && isNullable)
                            {
                                typeHandler = value =>
                                {
                                    if (value == null) return DBNull.Value;
                                    return Convert.ToInt32(value) != 0;
                                };
                            }
                            else typeHandler = value => Convert.ToInt32(value) != 0;
                        }
                    }
                    else if (fieldType == typeof(byte[]))
                    {
                        var supportedTypes = new Type[] { typeof(bool), typeof(char), typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) 
#if NET6_0_OR_GREATER
                            , typeof(Half)
#endif
                        };
                        if (supportedTypes.Contains(underlyingType))
                        {
                            switch (underlyingType)
                            {
                                case Type factType when factType == typeof(bool):
                                    if (isNullableType && isNullable)
                                    {
                                        typeHandler = value =>
                                        {
                                            if (value == null) return DBNull.Value;
                                            return BitConverter.GetBytes((bool)value);
                                        };
                                    }
                                    else typeHandler = value => BitConverter.GetBytes((bool)value);
                                    break;
                                case Type factType when factType == typeof(char):
                                    if (isNullableType && isNullable)
                                    {
                                        typeHandler = value =>
                                        {
                                            if (value == null) return DBNull.Value;
                                            return BitConverter.GetBytes((char)value);
                                        };
                                    }
                                    else typeHandler = value => BitConverter.GetBytes((char)value);
                                    break;
                                case Type factType when factType == typeof(short):
                                    if (isNullableType && isNullable)
                                    {
                                        typeHandler = value =>
                                        {
                                            if (value == null) return DBNull.Value;
                                            return BitConverter.GetBytes((short)value);
                                        };
                                    }
                                    else typeHandler = value => BitConverter.GetBytes((short)value);
                                    break;
                                case Type factType when factType == typeof(ushort):
                                    if (isNullableType && isNullable)
                                    {
                                        typeHandler = value =>
                                        {
                                            if (value == null) return DBNull.Value;
                                            return BitConverter.GetBytes((ushort)value);
                                        };
                                    }
                                    else typeHandler = value => BitConverter.GetBytes((ushort)value);
                                    break;
                                case Type factType when factType == typeof(int):
                                    if (isNullableType && isNullable)
                                    {
                                        typeHandler = value =>
                                        {
                                            if (value == null) return DBNull.Value;
                                            return BitConverter.GetBytes((int)value);
                                        };
                                    }
                                    else typeHandler = value => BitConverter.GetBytes((int)value);
                                    break;
                                case Type factType when factType == typeof(uint):
                                    if (isNullableType && isNullable)
                                    {
                                        typeHandler = value =>
                                        {
                                            if (value == null) return DBNull.Value;
                                            return BitConverter.GetBytes((uint)value);
                                        };
                                    }
                                    else typeHandler = value => BitConverter.GetBytes((uint)value);
                                    break;
                                case Type factType when factType == typeof(long):
                                    if (isNullableType && isNullable)
                                    {
                                        typeHandler = value =>
                                        {
                                            if (value == null) return DBNull.Value;
                                            return BitConverter.GetBytes((long)value);
                                        };
                                    }
                                    else typeHandler = value => BitConverter.GetBytes((long)value);
                                    break;
                                case Type factType when factType == typeof(ulong):
                                    if (isNullableType && isNullable)
                                    {
                                        typeHandler = value =>
                                        {
                                            if (value == null) return DBNull.Value;
                                            return BitConverter.GetBytes((ulong)value);
                                        };
                                    }
                                    else typeHandler = value => BitConverter.GetBytes((ulong)value);
                                    break;
                                case Type factType when factType == typeof(float):
                                    if (isNullableType && isNullable)
                                    {
                                        typeHandler = value =>
                                        {
                                            if (value == null) return DBNull.Value;
                                            return BitConverter.GetBytes((float)value);
                                        };
                                    }
                                    else typeHandler = value => BitConverter.GetBytes((float)value);
                                    break;
                                case Type factType when factType == typeof(double):
                                    if (isNullableType && isNullable)
                                    {
                                        typeHandler = value =>
                                        {
                                            if (value == null) return DBNull.Value;
                                            return BitConverter.GetBytes((double)value);
                                        };
                                    }
                                    else typeHandler = value => BitConverter.GetBytes((double)value);
                                    break;
#if NET6_0_OR_GREATER
                                case Type factType when factType == typeof(Half):
                                    if (isNullableType && isNullable)
                                    {
                                        typeHandler = value =>
                                        {
                                            if (value == null) return DBNull.Value;
                                            return BitConverter.GetBytes((Half)value);
                                        };
                                    }
                                    else typeHandler = value => BitConverter.GetBytes((Half)value);
                                    break;
#endif
                            }
                        }
                    }
                    else if (fieldType == typeof(char))
                    {
                        var supportedTypes = new Type[] { typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong) };
                        if (underlyingType == typeof(string))
                        {
                            if (isNullableType && isNullable)
                            {
                                typeHandler = value =>
                                {
                                    if (value == null) return DBNull.Value;
                                    return ((string)value)[0];
                                };
                            }
                            else typeHandler = value => ((string)value)[0];
                        }
                        else if (supportedTypes.Contains(underlyingType))
                        {
                            if (isNullableType && isNullable)
                            {
                                typeHandler = value =>
                                {
                                    if (value == null) return DBNull.Value;
                                    return Convert.ToChar(value);
                                };
                            }
                            else typeHandler = value => Convert.ToChar(value);
                        }
                    }
                    else
                    {
                        switch (Type.GetTypeCode(fieldType))
                        {
                            case TypeCode.Byte:
                            case TypeCode.SByte:
                            case TypeCode.Int16:
                            case TypeCode.UInt16:
                            case TypeCode.Int32:
                            case TypeCode.UInt32:
                            case TypeCode.Int64:
                            case TypeCode.UInt64:
                            case TypeCode.Single:
                            case TypeCode.Double:
                            case TypeCode.Decimal:
                                if (isNullableType && isNullable)
                                {
                                    typeHandler = value =>
                                    {
                                        if (value == null) return DBNull.Value;
                                        return Convert.ChangeType(value, fieldType);
                                    };
                                }
                                else typeHandler = value => Convert.ChangeType(value, fieldType);
                                break;
                        }
                    }
                }
            }
            if (typeHandler == null) throw new Exception($"不存在类型{fromType.FullName}->{fieldType.FullName}转换TypeHandler");
            return typeHandler;
        });
    }
    public virtual Func<object, object> GetReaderValueGetter(Type targetType, Type fieldType)
    {
        var hashKey = RepositoryHelper.GetCacheKey(targetType, fieldType);
        return readerValueGetters.GetOrAdd(hashKey, f =>
        {
            var underlyingType = Nullable.GetUnderlyingType(targetType);
            var isNullableType = underlyingType != null;
            underlyingType ??= targetType;
            Func<object, object> typeHandler = null;
            if (targetType == fieldType || underlyingType == fieldType)
            {
                var valueExpr = Expression.Parameter(typeof(object), "value");
                var blockBodies = new List<Expression>();
                var resultExpr = Expression.Variable(typeof(object), "result");
                var isDbNullExpr = Expression.TypeIs(valueExpr, typeof(DBNull));
                var setDefaultExpr = Expression.Assign(resultExpr, Expression.Convert(Expression.Default(targetType), typeof(object)));
                var setTypedValueExpr = Expression.Assign(resultExpr, valueExpr);
                blockBodies.Add(Expression.IfThenElse(isDbNullExpr, setDefaultExpr, setTypedValueExpr));
                var resultLabelExpr = Expression.Label(typeof(object));
                blockBodies.Add(Expression.Return(resultLabelExpr, resultExpr));
                blockBodies.Add(Expression.Label(resultLabelExpr, Expression.Default(typeof(object))));
                var bodyExpr = Expression.Block([resultExpr], blockBodies);
                typeHandler = Expression.Lambda<Func<object, object>>(bodyExpr, valueExpr).Compile();
            }
            else
            {
                //当前参数类型是非空类型，尽管数据库可为null，当作非空类型处理
                if (underlyingType.IsEnumType(out _))
                {
                    var enumUnderlyingType = Enum.GetUnderlyingType(underlyingType);
                    var supportedTypes = new Type[] { typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal) };
                    if (fieldType == typeof(string))
                    {
                        //参数类型可为null，数据库一定可为null
                        if (isNullableType)
                        {
                            typeHandler = value =>
                            {
                                if (value is DBNull) return null;
                                return Enum.Parse(underlyingType, (string)value);
                            };
                        }
                        else
                        {
                            typeHandler = value =>
                            {
                                if (value is DBNull) return Enum.ToObject(underlyingType, 0);
                                return Enum.Parse(underlyingType, (string)value);
                            };
                        }
                    }
                    else if (enumUnderlyingType != fieldType && supportedTypes.Contains(fieldType))
                    {
                        if (isNullableType)
                        {
                            typeHandler = value =>
                            {
                                if (value is DBNull) return null;
                                var numberValue = Convert.ChangeType(value, enumUnderlyingType);
                                return Enum.ToObject(underlyingType, numberValue);
                            };
                        }
                        else
                        {
                            typeHandler = value =>
                            {
                                if (value is DBNull) return Enum.ToObject(underlyingType, 0);
                                var numberValue = Convert.ChangeType(value, enumUnderlyingType);
                                return Enum.ToObject(underlyingType, numberValue);
                            };
                        }
                    }
                    else
                    {
                        if (isNullableType)
                        {
                            typeHandler = value =>
                            {
                                if (value is DBNull) return null;
                                return Enum.ToObject(underlyingType, value);
                            };
                        }
                        else
                        {
                            typeHandler = value =>
                            {
                                if (value is DBNull) return Enum.ToObject(underlyingType, 0);
                                return Enum.ToObject(underlyingType, value);
                            };
                        }
                    }
                }
                else
                {
                    if (underlyingType == typeof(Guid))
                    {
                        if (fieldType == typeof(string))
                        {
                            if (isNullableType)
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return null;
                                    return new Guid((string)value);
                                };
                            }
                            else
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return Guid.Empty;
                                    return new Guid((string)value);
                                };
                            }
                        }
                        else if (fieldType == typeof(byte[]))
                        {
                            if (isNullableType)
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return null;
                                    return new Guid((byte[])value);
                                };
                            }
                            else
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return Guid.Empty;
                                    return new Guid((byte[])value);
                                };
                            }
                        }
                    }
                    else if (underlyingType == typeof(DateTimeOffset))
                    {
                        if (fieldType == typeof(string))
                        {
                            if (isNullableType)
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return null;
                                    return DateTimeOffset.Parse((string)value);
                                };
                            }
                            else
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return DateTimeOffset.MinValue;
                                    return DateTimeOffset.Parse((string)value);
                                };
                            }
                        }
                        else if (fieldType == typeof(DateTime))
                        {
                            if (isNullableType)
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return null;
                                    return new DateTimeOffset((DateTime)value);
                                };
                            }
                            else
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return DateTimeOffset.MinValue;
                                    return new DateTimeOffset((DateTime)value);
                                };
                            }
                        }
                    }
                    else if (underlyingType == typeof(DateTime))
                    {
                        if (fieldType == typeof(string))
                        {
                            if (isNullableType)
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return null;
                                    return DateTime.Parse((string)value);
                                };
                            }
                            else
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return DateTime.MinValue;
                                    return DateTime.Parse((string)value);
                                };
                            }
                        }
#if NET6_0_OR_GREATER
                        else if (fieldType == typeof(DateOnly))
                        {
                            if (isNullableType)
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return null;
                                    return ((DateOnly)value).ToDateTime(TimeOnly.MinValue);
                                };
                            }
                            else
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return DateTime.MinValue;
                                    return ((DateOnly)value).ToDateTime(TimeOnly.MinValue);
                                };
                            }
                        }
#endif
                        else if (fieldType == typeof(DateTimeOffset))
                        {
                            if (isNullableType)
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return null;
                                    return ((DateTimeOffset)value).LocalDateTime;
                                };
                            }
                            else
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return DateTime.MinValue;
                                    return ((DateTimeOffset)value).LocalDateTime;
                                };
                            }
                        }
                    }
#if NET6_0_OR_GREATER
                    else if (underlyingType == typeof(DateOnly))
                    {
                        if (fieldType == typeof(string))
                        {
                            if (isNullableType)
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return null;
                                    return DateOnly.Parse((string)value);
                                };
                            }
                            else
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return DateOnly.MinValue;
                                    return DateOnly.Parse((string)value);
                                };
                            }
                        }
                        else if (fieldType == typeof(DateTime))
                        {
                            if (isNullableType)
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return null;
                                    return DateOnly.FromDateTime((DateTime)value);
                                };
                            }
                            else
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return DateOnly.MinValue;
                                    return DateOnly.FromDateTime((DateTime)value);
                                };
                            }
                        }
                        else if (fieldType == typeof(DateTimeOffset))
                        {
                            if (isNullableType)
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return null;
                                    return DateOnly.FromDateTime(((DateTimeOffset)value).LocalDateTime);
                                };
                            }
                            else
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return DateOnly.MinValue;
                                    return DateOnly.FromDateTime(((DateTimeOffset)value).LocalDateTime);
                                };
                            }
                        }
                    }
#endif
                    else if (underlyingType == typeof(TimeSpan))
                    {
                        if (fieldType == typeof(long))
                        {
                            if (isNullableType)
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return null;
                                    return TimeSpan.FromTicks((long)value);
                                };
                            }
                            else
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return TimeSpan.MinValue;
                                    return TimeSpan.FromTicks((long)value);
                                };
                            }
                        }
                        else if (fieldType == typeof(string))
                        {
                            if (isNullableType)
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return null;
                                    return TimeSpan.Parse((string)value);
                                };
                            }
                            else
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return TimeSpan.MinValue;
                                    return TimeSpan.Parse((string)value);
                                };
                            }
                        }
#if NET6_0_OR_GREATER
                        else if (fieldType == typeof(TimeOnly))
                        {
                            if (isNullableType)
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return null;
                                    return ((TimeOnly)value).ToTimeSpan();
                                };
                            }
                            else
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return TimeSpan.MinValue;
                                    return ((TimeOnly)value).ToTimeSpan();
                                };
                            }
                        }
#endif
                        else if (fieldType == typeof(DateTime))
                        {
                            if (isNullableType)
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return null;
                                    return ((DateTime)value).TimeOfDay;
                                };
                            }
                            else
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return TimeSpan.MinValue;
                                    return ((DateTime)value).TimeOfDay;
                                };
                            }
                        }
                        else if (fieldType == typeof(DateTimeOffset))
                        {
                            if (isNullableType)
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return null;
                                    return ((DateTimeOffset)value).LocalDateTime.TimeOfDay;
                                };
                            }
                            else
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return TimeSpan.MinValue;
                                    return ((DateTimeOffset)value).LocalDateTime.TimeOfDay;
                                };
                            }
                        }
                    }
#if NET6_0_OR_GREATER
                    else if (underlyingType == typeof(TimeOnly))
                    {
                        if (fieldType == typeof(long))
                        {
                            if (isNullableType)
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return null;
                                    return new TimeOnly((long)value);
                                };
                            }
                            else
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return TimeOnly.MinValue;
                                    return new TimeOnly((long)value);
                                };
                            }
                        }
                        else if (fieldType == typeof(string))
                        {
                            if (isNullableType)
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return null;
                                    return TimeOnly.FromTimeSpan(TimeSpan.Parse((string)value));
                                };
                            }
                            else
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return TimeOnly.MinValue;
                                    return TimeOnly.FromTimeSpan(TimeSpan.Parse((string)value));
                                };
                            }
                        }
                        else if (fieldType == typeof(TimeSpan))
                        {
                            if (isNullableType)
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return null;
                                    return TimeOnly.FromTimeSpan((TimeSpan)value);
                                };
                            }
                            else
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return TimeOnly.MinValue;
                                    return TimeOnly.FromTimeSpan((TimeSpan)value);
                                };
                            }
                        }
                        else if (fieldType == typeof(DateTime))
                        {
                            if (isNullableType)
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return null;
                                    return TimeOnly.FromTimeSpan(((DateTime)value).TimeOfDay);
                                };
                            }
                            else
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return TimeOnly.MinValue;
                                    return TimeOnly.FromTimeSpan(((DateTime)value).TimeOfDay);
                                };
                            }
                        }
                        else if (fieldType == typeof(DateTimeOffset))
                        {
                            if (isNullableType)
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return null;
                                    return TimeOnly.FromTimeSpan(((DateTimeOffset)value).LocalDateTime.TimeOfDay);
                                };
                            }
                            else
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return TimeOnly.MinValue;
                                    return TimeOnly.FromTimeSpan(((DateTimeOffset)value).LocalDateTime.TimeOfDay);
                                };
                            }
                        }
                    }
#endif
                    else if (underlyingType == typeof(string))
                    {
                        typeHandler = value =>
                        {
                            if (value is DBNull) return null;
                            return Convert.ToString(value);
                        };
                    }
                    else if (underlyingType == typeof(bool))
                    {
                        var supportedTypes = new Type[] { typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong) };
                        if (supportedTypes.Contains(fieldType))
                        {
                            if (isNullableType)
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return null;
                                    return Convert.ToInt32(value) != 0;
                                };
                            }
                            else
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return false;
                                    return Convert.ToInt32(value) != 0;
                                };
                            }
                        }
                    }
                    else if (underlyingType == typeof(byte[]))
                    {
                        var supportedTypes = new Type[] { typeof(bool), typeof(char), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double)
#if NET6_0_OR_GREATER
                            , typeof(Half) 
#endif
                        };
                        if (supportedTypes.Contains(fieldType))
                        {
                            switch (fieldType)
                            {
                                case Type factType when factType == typeof(bool):
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return null;
                                        return BitConverter.GetBytes((bool)value);
                                    };
                                    break;
                                case Type factType when factType == typeof(char):
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return null;
                                        return BitConverter.GetBytes((char)value);
                                    };
                                    break;
                                case Type factType when factType == typeof(short):
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return null;
                                        return BitConverter.GetBytes((short)value);
                                    };
                                    break;
                                case Type factType when factType == typeof(ushort):
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return null;
                                        return BitConverter.GetBytes((ushort)value);
                                    };
                                    break;
                                case Type factType when factType == typeof(int):
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return null;
                                        return BitConverter.GetBytes((int)value);
                                    };
                                    break;
                                case Type factType when factType == typeof(uint):
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return null;
                                        return BitConverter.GetBytes((uint)value);
                                    };
                                    break;
                                case Type factType when factType == typeof(long):
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return null;
                                        return BitConverter.GetBytes((long)value);
                                    };
                                    break;
                                case Type factType when factType == typeof(ulong):
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return null;
                                        return BitConverter.GetBytes((ulong)value);
                                    };
                                    break;
                                case Type factType when factType == typeof(float):
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return null;
                                        return BitConverter.GetBytes((float)value);
                                    };
                                    break;
                                case Type factType when factType == typeof(double):
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return null;
                                        return BitConverter.GetBytes((double)value);
                                    };
                                    break;
#if NET6_0_OR_GREATER
                                case Type factType when factType == typeof(Half):
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return null;
                                        return BitConverter.GetBytes((Half)value);
                                    };
                                    break;
#endif
                            }
                        }
                    }
                    else if (underlyingType == typeof(char))
                    {
                        var supportedTypes = new Type[] { typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong) };
                        if (fieldType == typeof(string))
                        {
                            if (isNullableType)
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return null;
                                    return ((string)value)[0];
                                };
                            }
                            else
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return default(char);
                                    return ((string)value)[0];
                                };
                            }
                        }
                        else if (supportedTypes.Contains(underlyingType))
                        {
                            if (isNullableType)
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return null;
                                    return Convert.ToChar(value);
                                };
                            }
                            else
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return default(char);
                                    return Convert.ToChar(value);
                                };
                            }
                        }
                    }
                    else
                    {
                        switch (Type.GetTypeCode(underlyingType))
                        {
                            case TypeCode.Byte:
                            case TypeCode.SByte:
                            case TypeCode.Int16:
                            case TypeCode.UInt16:
                            case TypeCode.Int32:
                            case TypeCode.UInt32:
                            case TypeCode.Int64:
                            case TypeCode.UInt64:
                            case TypeCode.Single:
                            case TypeCode.Double:
                            case TypeCode.Decimal:
                                if (isNullableType)
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return null;
                                        return Convert.ChangeType(value, underlyingType);
                                    };
                                }
                                else
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return Convert.ChangeType(0, underlyingType);
                                        return Convert.ChangeType(value, underlyingType);
                                    };
                                }
                                break;
                        }
                    }
                }
            }
            if (typeHandler == null) throw new Exception($"不存在类型{fieldType.FullName}->{targetType.FullName}转换TypeHandler");
            return typeHandler;
        });
    }
    public virtual ITypeHandler GetTypeHandler(Type typeHandlerType)
    {
        if (!typeHandlers.TryGetValue(typeHandlerType, out var typeHandler))
            typeHandlers.TryAdd(typeHandlerType, typeHandler = Activator.CreateInstance(typeHandlerType) as ITypeHandler);
        return typeHandler;
    }
    public abstract object MapNativeDbType(DbColumnInfo columnInfo);
    public abstract void MapTables(string connectionString, IEntityMapProvider mapProvider);
    public virtual bool TryGetMemberAccessSqlFormatter(MemberExpression memberExpr, out MemberAccessSqlFormatter formatter)
    {
        var memberInfo = memberExpr.Member;
        var cacheKey = RepositoryHelper.GetCacheKey(memberInfo.DeclaringType, memberInfo);
        if (memberAccessSqlFormatterCache.TryGetValue(cacheKey, out formatter))
            return true;

        if (memberInfo.DeclaringType == typeof(string) && this.TryGetStringMemberAccessSqlFormatter(memberExpr, out formatter))
            return true;
        if (memberInfo.DeclaringType == typeof(DateTime) && this.TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out formatter))
            return true;
#if NET6_0_OR_GREATER
        if (memberInfo.DeclaringType == typeof(DateOnly) && this.TryGetDateOnlyMemberAccessSqlFormatter(memberExpr, out formatter))
            return true;
#endif
        if (memberInfo.DeclaringType == typeof(TimeSpan) && this.TryGetTimeSpanMemberAccessSqlFormatter(memberExpr, out formatter))
            return true;
#if NET6_0_OR_GREATER
        if (memberInfo.DeclaringType == typeof(TimeOnly) && this.TryGetTimeOnlyMemberAccessSqlFormatter(memberExpr, out formatter))
            return true;
#endif
        //自定义成员访问解析
        if (this.TryGetMyMemberAccessSqlFormatter(memberExpr, out formatter))
            return true;
        return false;
    }
    public virtual bool TryGetMyMemberAccessSqlFormatter(MemberExpression memberExpr, out MemberAccessSqlFormatter formatter)
    {
        formatter = null;
        return false;
    }
    public virtual bool TryGetMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter)
    {
        var methodInfo = methodCallExpr.Method;
        var cacheKey = RepositoryHelper.GetCacheKey(methodInfo.DeclaringType, methodInfo);
        if (methodCallSqlFormatterCache.TryGetValue(cacheKey, out formatter))
            return true;

        if (methodInfo.DeclaringType == typeof(string) && this.TryGetStringMethodCallSqlFormatter(methodCallExpr, out formatter))
            return true;
        if (methodInfo.DeclaringType == typeof(DateTime) && this.TryGetDateTimeMethodCallSqlFormatter(methodCallExpr, out formatter))
            return true;
#if NET6_0_OR_GREATER
        if (methodInfo.DeclaringType == typeof(DateOnly) && this.TryGetDateOnlyMethodCallSqlFormatter(methodCallExpr, out formatter))
            return true;
#endif
        if (methodInfo.DeclaringType == typeof(TimeSpan) && this.TryGetTimeSpanMethodCallSqlFormatter(methodCallExpr, out formatter))
            return true;
#if NET6_0_OR_GREATER
        if (methodInfo.DeclaringType == typeof(TimeOnly) && this.TryGetTimeOnlyMethodCallSqlFormatter(methodCallExpr, out formatter))
            return true;
#endif
        if (methodInfo.DeclaringType == typeof(Convert) && this.TryGetConvertMethodCallSqlFormatter(methodCallExpr, out formatter))
            return true;
        if (this.TryGetIEnumerableMethodCallSqlFormatter(methodCallExpr, out formatter))
            return true;
        if (methodInfo.DeclaringType == typeof(Math) && this.TryGetMathMethodCallSqlFormatter(methodCallExpr, out formatter))
            return true;
        //自定义函数解析
        if (this.TryGetMyMethodCallSqlFormatter(methodCallExpr, out formatter))
            return true;

        //兜底函数解析
        var parameterInfos = methodInfo.GetParameters();
        switch (methodInfo.Name)
        {
            case "Equals":
                if (!methodInfo.IsStatic && parameterInfos.Length == 1)
                {
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(targetSegment, rightSegment, $"{targetArgument}={rightArgument}");
                    });
                    return true;
                }
                break;
            case "Compare":
                if (methodInfo.IsStatic && parameterInfos.Length == 2)
                {
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var leftSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        var rightSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[1] });

                        var leftArgument = visitor.GetQuotedValue(leftSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return leftSegment.Merge(rightSegment, $"CASE WHEN {leftArgument}={rightArgument} THEN 0 WHEN {leftArgument}>{rightArgument} THEN 1 ELSE -1 END");
                    });
                    return true;
                }
                break;
            case "CompareTo":
                if (!methodInfo.IsStatic && parameterInfos.Length == 1)
                {
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(rightSegment, $"CASE WHEN {targetArgument}={rightArgument} THEN 0 WHEN {targetArgument}>{rightArgument} THEN 1 ELSE -1 END");
                    });
                    return true;
                }
                break;
            case "ToString":
                if (!methodInfo.IsStatic && parameterInfos.Length == 0)
                {
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(targetSegment.Value.ToString());

                        if (targetSegment.SegmentType.IsEnum && !targetSegment.IsExpression && !targetSegment.IsMethodCall)
                            visitor.ToEnumString(targetSegment);
                        if (targetSegment.SegmentType != methodInfo.ReturnType)
                        {
                            targetSegment.Body = this.CastTo(methodInfo.ReturnType, targetSegment.Body);
                            targetSegment.IsMethodCall = true;
                        }
                        return targetSegment;
                    });
                    return true;
                }
                if (!methodInfo.IsStatic && parameterInfos.Length == 1 && (parameterInfos[0].ParameterType == typeof(string) || typeof(IFormatProvider).IsAssignableFrom(parameterInfos[0].ParameterType)))
                {
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                        var args0Segment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(methodInfo.Invoke(targetSegment.Value, new object[] { args0Segment.Value }));

                        //f.Balance.ToString("C")
                        //args0.ToString("C")
                        //(args0)=>{args0.ToString("C")}

                        //f.Balance.ToString(new CultureInfo("en-US"))
                        //args.ToString(new CultureInfo("en-US"))
                        //(args)=>{args.ToString(new CultureInfo("en-US"))}
                        if (visitor.IsSelect && (args0Segment.IsConstant || args0Segment.IsVariable))
                            return visitor.BuildDeferredSqlSegment(methodCallExpr, targetSegment);

                        throw new NotSupportedException("不支持的方法调用，方法.ToString(string format)只支持常量或是变量的解析");
                    });
                    return true;
                }
                if (!methodInfo.IsStatic && parameterInfos.Length == 2 && parameterInfos[0].ParameterType == typeof(string) && typeof(IFormatProvider).IsAssignableFrom(parameterInfos[1].ParameterType))
                {
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                        var args0Segment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        var args1Segment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[1] });
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(methodInfo.Invoke(targetSegment.Value, new object[] { args0Segment.Value, args1Segment.Value }));
                        //f.Balance.ToString("C", new CultureInfo("en-US"))
                        //args.ToString("C", new CultureInfo("en-US"))
                        //(args)=>{args.ToString("C", new CultureInfo("en-US"))}
                        if (visitor.IsSelect && (args0Segment.IsConstant || args0Segment.IsVariable) && (args1Segment.IsConstant || args1Segment.IsVariable))
                            return visitor.BuildDeferredSqlSegment(methodCallExpr, targetSegment);
                        throw new NotSupportedException("不支持的方法调用，方法.ToString(string format, IFormatProvider provider)只支持常量或是变量的解析");
                    });
                    return true;
                }
                break;
            case "Parse":
                if (methodInfo.IsStatic && methodInfo.DeclaringType == typeof(Enum))
                {
                    if (parameterInfos.Length == 1 || parameterInfos[0].ParameterType != typeof(Type))
                    {
                        var enumType = methodInfo.GetGenericArguments()[0];
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var args0Segment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                            if (args0Segment.IsConstant || args0Segment.IsVariable)
                                return args0Segment.ChangeValue(Enum.Parse(enumType, args0Segment.Value.ToString()));

                            throw new NotSupportedException("不支持的表达式访问，Enum.Parse方法只支持常量、变量参数");
                        });
                        return true;
                    }
                    if (parameterInfos.Length > 1 && parameterInfos[0].ParameterType == typeof(Type))
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            SqlFieldSegment resultSegment = null;
                            var arguments = new List<object>();
                            Array.ForEach(args, f =>
                            {
                                var sqlSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = f });
                                if (sqlSegment.IsConstant || sqlSegment.IsVariable)
                                    arguments.Add(sqlSegment.Value);
                                else arguments.Add(sqlSegment.Body);
                                if (resultSegment == null) resultSegment = sqlSegment;
                                else resultSegment.Merge(sqlSegment);
                            });
                            if (resultSegment.IsConstant || resultSegment.IsVariable)
                                return resultSegment.ChangeValue(methodInfo.Invoke(null, arguments.ToArray()));

                            throw new NotSupportedException("不支持的表达式访问，Enum.Parse方法只支持常量、变量参数");
                        });
                        return true;
                    }
                }
                if (methodInfo.IsStatic && parameterInfos.Length >= 1)
                {
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        SqlFieldSegment resultSegment = null;
                        var arguments = new List<object>();
                        Array.ForEach(args, f =>
                        {
                            var sqlSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = f });
                            if (sqlSegment.IsConstant || sqlSegment.IsVariable)
                                arguments.Add(sqlSegment.Value);
                            else arguments.Add(sqlSegment.Body);
                            if (resultSegment == null) resultSegment = sqlSegment;
                            else resultSegment.Merge(sqlSegment);
                        });
                        if (resultSegment.IsConstant || resultSegment.IsVariable)
                            return resultSegment.ChangeValue(methodInfo.Invoke(null, arguments.ToArray()));
                        return resultSegment.Change(this.CastTo(methodInfo.ReturnType, arguments[0]), false, true);
                    });
                    return true;
                }
                break;
            case "TryParse":
                if (methodInfo.IsStatic && methodInfo.DeclaringType == typeof(Enum))
                {
                    if (parameterInfos.Length == 1 || parameterInfos[0].ParameterType != typeof(Type))
                    {
                        var enumType = methodInfo.GetGenericArguments()[0];
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var args0Segment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                            if (args0Segment.IsConstant || args0Segment.IsVariable)
                                return args0Segment.ChangeValue(Enum.Parse(enumType, args0Segment.Value.ToString()));

                            throw new NotSupportedException("不支持的表达式访问，Enum.Parse方法只支持常量、变量参数");
                        });
                        return true;
                    }
                    if (parameterInfos.Length > 1 && parameterInfos[0].ParameterType == typeof(Type))
                    {
                        var enumType = parameterInfos[0].ParameterType;
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            SqlFieldSegment resultSegment = null;
                            var arguments = new List<object>();
                            for (int i = 0; i < args.Length - 1; i++)
                            {
                                var sqlSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[i] });
                                if (sqlSegment.IsConstant || sqlSegment.IsVariable)
                                    arguments.Add(sqlSegment.Value);
                                else arguments.Add(sqlSegment.Body);
                                if (resultSegment == null) resultSegment = sqlSegment;
                                else resultSegment.Merge(sqlSegment);
                            }
                            if (resultSegment.IsConstant || resultSegment.IsVariable)
                                return resultSegment.ChangeValue(methodInfo.Invoke(null, arguments.ToArray()));

                            throw new NotSupportedException("不支持的表达式访问，Enum.Parse方法只支持常量、变量参数");
                        });
                        return true;
                    }
                }
                if (methodInfo.IsStatic && parameterInfos.Length >= 1)
                {
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        SqlFieldSegment resultSegment = null;
                        var arguments = new List<object>();
                        for (int i = 0; i < args.Length - 1; i++)
                        {
                            var sqlSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[i] });
                            if (sqlSegment.IsConstant || sqlSegment.IsVariable)
                                arguments.Add(sqlSegment.Value);
                            else arguments.Add(sqlSegment.Body);
                            if (resultSegment == null) resultSegment = sqlSegment;
                            else resultSegment.Merge(sqlSegment);
                        }
                        if (resultSegment.IsConstant || resultSegment.IsVariable)
                            return resultSegment.ChangeValue(methodInfo.Invoke(null, arguments.ToArray()));
                        return resultSegment.Change(this.CastTo(methodInfo.ReturnType, arguments[0]), false, true);
                    });
                    return true;
                }
                break;
            case "get_Item":
                if (!methodInfo.IsStatic && parameterInfos.Length > 0)
                {
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                        var arguments = new List<object>();
                        for (int i = 0; i < args.Length; i++)
                        {
                            var argumentSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[i] });
                            if (argumentSegment.IsConstant || argumentSegment.IsVariable)
                                arguments.Add(argumentSegment.Value);
                            else arguments.Add(argumentSegment.Body);
                            targetSegment.Merge(argumentSegment);
                        }
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(methodInfo.Invoke(targetSegment.Value, arguments.ToArray()));

                        throw new NotSupportedException("不支持的表达式访问，get_Item索引方法只支持常量、变量参数");
                    });
                    return true;
                }
                break;
                //case "Exists":
                //    if (parameterInfos.Length == 1 && methodInfo.DeclaringType.GenericTypeArguments.Length > 0
                //        && parameterInfos[0].ParameterType == typeof(Predicate<>).MakeGenericType(methodInfo.DeclaringType.GenericTypeArguments[0]))
                //    {
                //        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                //        {
                //            args[0].GetParameters(out var parameters);
                //            var lambdaExpr = args[0] as LambdaExpression;
                //            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                //            if (!targetSegment.IsConstant && !targetSegment.IsVariable)
                //                throw new NotSupportedException($"不支持的表达式访问，Exists方法只支持常量和变量的解析，Path:{orgExpr}");
                //            if (parameters.Count > 1)
                //            {
                //                lambdaExpr = Expression.Lambda(lambdaExpr.Body, parameters);
                //                var targetArray = targetSegment.Value as IEnumerable;                            
                //            }

                //            var args0Segment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                //            if (args0Segment.IsConstant || args0Segment.IsVariable)
                //                return args0Segment.Change(methodInfo.Invoke(args0Segment.Value, null));
                //            return args0Segment.Change($"REVERSE({args0Segment})", false, false, false, true);
                //        });
                //        result = true;
                //    }
                //    break;
        }
        return false;
    }
    public virtual bool TryGetMyMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter)
    {
        formatter = null;
        return false;
    }
    public abstract bool TryGetStringMemberAccessSqlFormatter(MemberExpression memberExpr, out MemberAccessSqlFormatter formatter);
    public abstract bool TryGetStringMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter);
    public abstract bool TryGetDateTimeMemberAccessSqlFormatter(MemberExpression memberExpr, out MemberAccessSqlFormatter formatter);
    public abstract bool TryGetDateTimeMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter);
    public abstract bool TryGetTimeSpanMemberAccessSqlFormatter(MemberExpression memberExpr, out MemberAccessSqlFormatter formatter);
    public abstract bool TryGetTimeSpanMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter);
    public abstract bool TryGetDateOnlyMemberAccessSqlFormatter(MemberExpression memberExpr, out MemberAccessSqlFormatter formatter);
    public abstract bool TryGetDateOnlyMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter);
    public abstract bool TryGetTimeOnlyMemberAccessSqlFormatter(MemberExpression memberExpr, out MemberAccessSqlFormatter formatter);
    public abstract bool TryGetTimeOnlyMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter);
    public virtual bool TryGetConvertMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter)
    {
        var result = false;
        formatter = null;
        var methodInfo = methodCallExpr.Method;
        var parameterInfos = methodInfo.GetParameters();
        var cacheKey = RepositoryHelper.GetCacheKey(methodInfo.DeclaringType, methodInfo);
        switch (methodInfo.Name)
        {
            case "ToBoolean":
            case "ToByte":
            case "ToChar":
            case "ToDateTime":
            case "ToDouble":
            case "ToInt16":
            case "ToInt32":
            case "ToInt64":
            case "ToSByte":
            case "ToSingle":
            case "ToUInt16":
            case "ToUInt32":
            case "ToUInt64":
            case "ToDecimal":
                if (parameterInfos.Length == 1)
                {
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var args0Segment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        if (args0Segment.IsConstant || args0Segment.IsVariable)
                            return args0Segment.ChangeValue(methodInfo.Invoke(null, new object[] { args0Segment.Value }));
                        if (args0Segment.SegmentType != methodInfo.ReturnType)
                            return args0Segment.Change(this.CastTo(methodCallExpr.Type, args0Segment.Body), false, true);
                        return args0Segment;
                    });
                    result = true;
                }
                break;
            case "ToString":
                if (parameterInfos.Length == 1)
                {
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var args0Segment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        if (args0Segment.IsConstant || args0Segment.IsVariable)
                            return args0Segment.ChangeValue(methodInfo.Invoke(null, new object[] { args0Segment.Value }));
                        if (args0Segment.SegmentType != methodInfo.ReturnType)
                        {
                            if (args0Segment.SegmentType.IsEnum && !args0Segment.IsExpression && !args0Segment.IsMethodCall)
                                visitor.ToEnumString(args0Segment);
                            else
                            {
                                args0Segment.Body = this.CastTo(methodCallExpr.Type, args0Segment.Body);
                                args0Segment.IsMethodCall = true;
                            }
                        }
                        return args0Segment;
                    });
                    result = true;
                }
                break;
        }
        return result;
    }
    public virtual bool TryGetIEnumerableMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter)
    {
        var result = false;
        formatter = null;
        var methodInfo = methodCallExpr.Method;
        var parameterInfos = methodInfo.GetParameters();
        var cacheKey = RepositoryHelper.GetCacheKey(methodInfo.DeclaringType, methodInfo);
        switch (methodInfo.Name)
        {
            case "Contains":
                //public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value);
                //public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value, IEqualityComparer<TSource>? comparer);
                if (methodInfo.IsStatic && parameterInfos.Length >= 2)
                {
                    //数组调用
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var builder = new StringBuilder();
                        var elementSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[1] });
                        var arraySegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });

                        var enumerable = arraySegment.Value as IEnumerable;
                        foreach (var item in enumerable)
                        {
                            if (builder.Length > 0)
                                builder.Append(',');
                            var sqlArgument = visitor.GetQuotedValue(item, arraySegment, elementSegment);
                            builder.Append(sqlArgument);
                        }
                        if (builder.Length > 0)
                        {
                            var notString = deferExprs.IsDeferredNot() ? "NOT " : "";
                            var elementArgument = visitor.GetQuotedValue(elementSegment);
                            return elementSegment.Merge(arraySegment, $"{elementArgument} {notString}IN ({builder})");
                        }
                        else return elementSegment.Change("1<>0");
                    });
                    result = true;
                }

                //IEnumerable<T>,List<T>
                //public bool Contains(T item);
                if (!methodInfo.IsStatic && parameterInfos.Length == 1 && methodInfo.DeclaringType.GenericTypeArguments.Length > 0
                     && typeof(IEnumerable<>).MakeGenericType(methodInfo.DeclaringType.GenericTypeArguments[0]).IsAssignableFrom(methodInfo.DeclaringType))
                {
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var builder = new StringBuilder();
                        var elementSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });

                        var enumerable = targetSegment.Value as IEnumerable;
                        foreach (var item in enumerable)
                        {
                            if (builder.Length > 0)
                                builder.Append(',');
                            var sqlArgument = visitor.GetQuotedValue(item, targetSegment, elementSegment);
                            builder.Append(sqlArgument);
                        }
                        if (builder.Length > 0)
                        {
                            string elementArgument = visitor.GetQuotedValue(elementSegment);
                            var notString = deferExprs.IsDeferredNot() ? "NOT " : "";
                            return elementSegment.Merge(targetSegment, $"{elementArgument} {notString}IN ({builder})");
                        }
                        else return elementSegment.Change("1<>0");
                    });
                    return true;
                }
                break;
            case "Reverse":
                if (!methodInfo.IsStatic && parameterInfos.Length == 1 && methodInfo.DeclaringType.GenericTypeArguments.Length > 0
                    && methodInfo.DeclaringType.GenericTypeArguments[0] == typeof(char))
                {
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var args0Segment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        if (args0Segment.IsConstant || args0Segment.IsVariable)
                            return args0Segment.ChangeValue(methodInfo.Invoke(args0Segment.Value, null));
                        return args0Segment.Change($"REVERSE({args0Segment.Body})", false, true);
                    });
                    result = true;
                }
                break;
        }
        return result;
    }
    public abstract bool TryGetMathMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter);
    protected (string, string) GetFullTableName(string tableName)
    {
        if (tableName.Contains('.'))
        {
            var myTableNames = tableName.Split('.');
            return (myTableNames[0], myTableNames[1]);
        }
        return (this.DefaultTableSchema, tableName);
    }
}