using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

public abstract partial class BaseOrmProvider : IOrmProvider
{
    protected static readonly ConcurrentDictionary<int, Func<ISqlVisitor, SqlSegment, SqlSegment>> memberAccessSqlFormatterCache = new();
    protected static readonly ConcurrentDictionary<int, Func<ISqlVisitor, MethodCallExpression, Stack<DeferredOperation>, SqlSegment>> methodCallSqlFormatterCache = new();
    protected static readonly ConcurrentDictionary<int, Delegate> methodCallCache = new();
    protected static readonly ConcurrentDictionary<Type, ITypeHandler> typeHandlers = new();
    protected static readonly ConcurrentDictionary<int, Func<object, object>> parameterValueGetters = new();
    protected static readonly ConcurrentDictionary<int, Func<object, object>> readerValueGetters = new();

    static BaseOrmProvider()
    {
        typeHandlers.TryAdd(typeof(JsonTypeHandler), new JsonTypeHandler());
        typeHandlers.TryAdd(typeof(ToStringTypeHandler), new ToStringTypeHandler());
    }
    public virtual OrmProviderType OrmProviderType => OrmProviderType.Basic;
    public virtual string ParameterPrefix => "@";
    public abstract Type NativeDbTypeType { get; }
    public virtual ICollection<ITypeHandler> TypeHandlers => typeHandlers.Values;
    public abstract string GetDefaultSchema(string connectionString);
    public abstract ITheaConnection CreateConnection(string dbKey, string connectionString);
    //public abstract ITheaCommand CreateCommand();
    public abstract IDbDataParameter CreateParameter(string parameterName, object value);
    public abstract IDbDataParameter CreateParameter(string parameterName, object nativeDbType, object value);
    public abstract void ChangeParameter(object dbParameter, Type targetType, object value);
    public abstract IRepository CreateRepository(DbContext dbContext);

    public virtual string GetTableName(string entityName) => entityName;
    public virtual string GetFieldName(string fieldName) => fieldName;
    public virtual string GetPagingTemplate(int? skip, int? limit, string orderBy = null)
    {
        var builder = new StringBuilder("SELECT /**fields**/ FROM /**tables**/ /**others**/");
        if (!string.IsNullOrEmpty(orderBy)) builder.Append($" {orderBy}");
        if (limit.HasValue) builder.Append($" LIMIT {limit}");
        if (skip.HasValue && skip.Value > 0) builder.Append($" OFFSET {skip}");
        return builder.ToString();
    }
    public abstract object GetNativeDbType(Type type);
    public abstract Type MapDefaultType(MemberMap memberMappper);
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
            //case Type factType when factType == typeof(SqlSegment):
            //    {
            //        var sqlSegment = value as SqlSegment;
            //        if (sqlSegment.IsValue)
            //            return this.GetQuotedValue(sqlSegment.Value);
            //        return sqlSegment.Value;
            //    }
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
    public virtual Func<object, object> GetParameterValueGetter(Type fromType, Type fieldType, bool isNullable, OrmDbFactoryOptions options)
    {
        var hashKey = HashCode.Combine(fromType, fieldType, isNullable);
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
                if (fieldType == typeof(Array))
                {
                    //数组类支持一元的，多元建议用json
                    if (underlyingType.IsArray)
                    {
                        typeHandler = value =>
                        {
                            if (value is DBNull) return null;
                            return Convert.ChangeType(value, underlyingType);
                        };
                    }
                    else
                    {
                        if (underlyingType.IsGenericType)
                        {
                            var elelmentTypes = underlyingType.GetGenericArguments();
                            if (elelmentTypes.Length > 1)
                                throw new NotSupportedException("暂时不支持多维数组的数据类型");

                            if (underlyingType == typeof(List<>).MakeGenericType(elelmentTypes)
                                || underlyingType == typeof(Collection<>).MakeGenericType(elelmentTypes)
                                || underlyingType.IsInterface)
                            {
                                var enumerableExpr = Expression.Parameter(typeof(object), "enumerable");
                                var parametersType = typeof(IEnumerable<>).MakeGenericType(elelmentTypes[0]);
                                var typedEnumerableExpr = Expression.Convert(enumerableExpr, parametersType);
                                var methodInfo = typeof(Enumerable).GetMethod(nameof(Enumerable.ToArray), [parametersType]);
                                var bodyExpr = Expression.Call(methodInfo, typedEnumerableExpr);
                                var arrayCreater = Expression.Lambda<Func<object, object>>(bodyExpr, enumerableExpr).Compile();
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return null;
                                    return arrayCreater.Invoke(value);
                                };
                            }
                        }
                    }
                }
                else if (underlyingType.IsEnumType(out _))
                {
                    var enumUnderlyingType = Enum.GetUnderlyingType(underlyingType);
                    Type[] supportedTypes = [typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal)];
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
                        Type[] supportedTypes = [typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong)];
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
                        Type[] supportedTypes = [ typeof(bool), typeof(char), typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal)
#if NET6_0_OR_GREATER
                            , typeof(Half)
#endif
                        ];
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
                        Type[] supportedTypes = [typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong)];
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
    public virtual Func<object, object> GetReaderValueGetter(Type targetType, Type fieldType, OrmDbFactoryOptions options)
    {
        var hashKey = HashCode.Combine(targetType, fieldType, options.DefaultDateTimeKind);
        return readerValueGetters.GetOrAdd(hashKey, f =>
        {
            targetType = targetType.ToUnderlyingType();
            Func<object, object> typeHandler = null;
            if (targetType == fieldType)
            {
                if (targetType == typeof(DateTime))
                {
                    if (options.DefaultDateTimeKind == DateTimeKind.Utc)
                        typeHandler = value => DateTime.SpecifyKind((DateTime)value, DateTimeKind.Utc);
                    else if (options.DefaultDateTimeKind == DateTimeKind.Local)
                        typeHandler = value => DateTime.SpecifyKind((DateTime)value, DateTimeKind.Local);
                    else typeHandler = value => value;
                }
                else typeHandler = value => value;
            }
            else
            {
                //当前参数类型是非空类型，尽管数据库可为null，当作非空类型处理
                if (fieldType == typeof(Array))
                {
                    //数组类支持一元的，多元建议用json
                    if (targetType.IsArray)
                        typeHandler = value => Convert.ChangeType(value, targetType);
                    else if (targetType.IsGenericType)
                    {
                        var elelmentTypes = targetType.GetGenericArguments();
                        if (elelmentTypes.Length > 1)
                            throw new NotSupportedException("暂时不支持多维数组的数据类型");

                        if (targetType == typeof(List<>).MakeGenericType(elelmentTypes)
                            || targetType.IsInterface)
                        {
                            var collectionExpr = Expression.Parameter(typeof(object), "collection");
                            var parametersType = typeof(IEnumerable<>).MakeGenericType(elelmentTypes[0]);
                            var typedCollectionExpr = Expression.Convert(collectionExpr, parametersType);
                            var listType = typeof(List<>).MakeGenericType(elelmentTypes[0]);
                            var bodyExpr = Expression.New(listType.GetConstructor([parametersType]), typedCollectionExpr);
                            var listCreater = Expression.Lambda<Func<object, object>>(bodyExpr, collectionExpr).Compile();
                            typeHandler = value => listCreater.Invoke(value);
                        }
                        else if (targetType == typeof(Collection<>).MakeGenericType(elelmentTypes))
                        {
                            var listExpr = Expression.Parameter(typeof(object), "collection");
                            var parametersType = typeof(IList<>).MakeGenericType(elelmentTypes[0]);
                            var typedListExpr = Expression.Convert(listExpr, parametersType);
                            var collectionType = typeof(Collection<>).MakeGenericType(elelmentTypes[0]);
                            var bodyExpr = Expression.New(collectionType.GetConstructor([parametersType]), typedListExpr);
                            var listCreater = Expression.Lambda<Func<object, object>>(bodyExpr, listExpr).Compile();
                            typeHandler = value => listCreater.Invoke(value);
                        }
                    }
                }
                else if (fieldType == typeof(byte[]))
                {
                    //兼容某些分布式数据库，bit[n]类型转换为string类型，比如：polardb
                    if (targetType == typeof(string))
                        typeHandler = value => Encoding.UTF8.GetString((byte[])value);
                    //兼容某些数据库，bit[1]类型转换为bool类型
                    else if (targetType == typeof(bool))
                        typeHandler = value => Convert.ToInt32(((byte[])value)[0]) != 0;
                }
                else
                {
                    if (targetType.IsEnumType(out var enumUnderlyingType))
                    {
                        Type[] supportedTypes = [typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal)];
                        //参数类型可为null，数据库一定可为null
                        if (fieldType == typeof(string))
                            typeHandler = value => Enum.Parse(targetType, (string)value, true);
                        else if (enumUnderlyingType != fieldType && supportedTypes.Contains(fieldType))
                        {
                            typeHandler = value =>
                            {
                                var numberValue = Convert.ChangeType(value, enumUnderlyingType);
                                return Enum.ToObject(targetType, numberValue);
                            };
                        }
                        else
                        {
                            typeHandler = value =>
                            {
                                var numberValue = Convert.ChangeType(value, enumUnderlyingType);
                                return Enum.ToObject(targetType, numberValue);
                            };
                        }
                    }
                    else if (targetType == typeof(Guid))
                    {
                        if (fieldType == typeof(string))
                            typeHandler = value => new Guid((string)value);
                        else if (fieldType == typeof(byte[]))
                            typeHandler = value => new Guid((byte[])value);
                    }
                    else if (targetType == typeof(DateTimeOffset))
                    {
                        if (fieldType == typeof(string))
                        {
                            if (options.DefaultDateTimeKind == DateTimeKind.Utc)
                                typeHandler = value => new DateTimeOffset(DateTime.Parse((string)value), TimeSpan.Zero);
                            else typeHandler = value => DateTimeOffset.Parse((string)value);
                        }
                        else if (fieldType == typeof(DateTime))
                        {
                            if (options.DefaultDateTimeKind == DateTimeKind.Utc)
                                typeHandler = value => new DateTimeOffset((DateTime)value, TimeSpan.Zero);
                            else
                            {
                                typeHandler = value =>
                                {
                                    var dt = (DateTime)value;
                                    if (dt == DateTime.MinValue) return DateTimeOffset.MinValue;
                                    if (dt == DateTime.MaxValue) return DateTimeOffset.MaxValue;
                                    return new DateTimeOffset(dt);
                                };
                            }
                        }
                    }
                    else if (targetType == typeof(DateTime))
                    {
                        if (fieldType == typeof(string))
                        {
                            if (options.DefaultDateTimeKind == DateTimeKind.Utc)
                                typeHandler = value => DateTime.SpecifyKind(DateTime.Parse((string)value), DateTimeKind.Utc);
                            else if (options.DefaultDateTimeKind == DateTimeKind.Local)
                                typeHandler = value => DateTime.SpecifyKind(DateTime.Parse((string)value), DateTimeKind.Local);
                            else typeHandler = value => DateTime.Parse((string)value);
                        }
#if NET6_0_OR_GREATER
                        else if (fieldType == typeof(DateOnly))
                            typeHandler = value => ((DateOnly)value).ToDateTime(TimeOnly.MinValue);                       
#endif
                    }
#if NET6_0_OR_GREATER
                    else if (targetType == typeof(DateOnly))
                    {
                        if (fieldType == typeof(string))
                            typeHandler = value => DateOnly.Parse((string)value);
                        else if (fieldType == typeof(DateTime))
                            typeHandler = value => DateOnly.FromDateTime((DateTime)value);
                    }
#endif
                    else if (targetType == typeof(TimeSpan))
                    {
                        if (fieldType == typeof(long))
                            typeHandler = value => TimeSpan.FromTicks((long)value);
                        else if (fieldType == typeof(string))
                            typeHandler = value => TimeSpan.Parse((string)value);
#if NET6_0_OR_GREATER
                        else if (fieldType == typeof(TimeOnly))
                            typeHandler = value => ((TimeOnly)value).ToTimeSpan();
#endif
                        else if (fieldType == typeof(DateTime))
                            typeHandler = value => ((DateTime)value).TimeOfDay;
                    }
#if NET6_0_OR_GREATER
                    else if (targetType == typeof(TimeOnly))
                    {
                        if (fieldType == typeof(long))
                            typeHandler = value => new TimeOnly((long)value);
                        else if (fieldType == typeof(string))
                            typeHandler = value => TimeOnly.FromTimeSpan(TimeSpan.Parse((string)value));
                        else if (fieldType == typeof(TimeSpan))
                            typeHandler = value => TimeOnly.FromTimeSpan((TimeSpan)value);
                        else if (fieldType == typeof(DateTime))
                            typeHandler = value => TimeOnly.FromTimeSpan(((DateTime)value).TimeOfDay);
                    }
#endif
                    else if (targetType == typeof(string))
                        typeHandler = value => value.ToString();
                    else if (targetType == typeof(bool))
                    {
                        Type[] supportedTypes = [typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong)];
                        if (supportedTypes.Contains(fieldType))
                            typeHandler = value => Convert.ToInt32(value) != 0;
                    }
                    else if (targetType == typeof(byte[]))
                    {
                        Type[] supportedTypes = [ typeof(bool), typeof(char), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double)
#if NET6_0_OR_GREATER
                            , typeof(Half)
#endif
                        ];
                        if (supportedTypes.Contains(fieldType))
                        {
                            var valueExpr = Expression.Parameter(typeof(object), "value");
                            var typedValueExpr = Expression.Convert(valueExpr, fieldType);
                            var methodInfo = typeof(BitConverter).GetMethod(nameof(BitConverter.GetBytes), [fieldType]);
                            typeHandler = Expression.Lambda<Func<object, object>>(Expression.Call(methodInfo, typedValueExpr), valueExpr).Compile();
                        }
                    }
                    else if (targetType == typeof(char))
                    {
                        Type[] supportedTypes = [typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong)];
                        if (fieldType == typeof(string))
                            typeHandler = value => ((string)value)[0];
                        else if (supportedTypes.Contains(targetType))
                        {
                            var valueExpr = Expression.Parameter(typeof(object), "value");
                            var typedValueExpr = Expression.Convert(valueExpr, fieldType);
                            var methodInfo = typeof(Convert).GetMethod(nameof(Convert.ToChar), [fieldType]);
                            typeHandler = Expression.Lambda<Func<object, object>>(Expression.Call(methodInfo, typedValueExpr), valueExpr).Compile();
                        }
                    }
                    else
                    {
                        switch (Type.GetTypeCode(targetType))
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
                                typeHandler = value => Convert.ChangeType(value, targetType);
                                break;
                            case TypeCode.Object:
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return null;
                                    return value;
                                };
                                break;
                        }
                    }
                }
            }
            if (typeHandler == null) throw new Exception($"不存在类型{fieldType.FullName}->{targetType.FullName}转换的TypeHandler");
            return typeHandler;
        });
    }
    public virtual ITypeHandler GetTypeHandler(Type typeHandlerType)
    {
        if (!typeHandlers.TryGetValue(typeHandlerType, out var typeHandler))
            typeHandlers.TryAdd(typeHandlerType, typeHandler = RepositoryHelper.CreateInstance(typeHandlerType) as ITypeHandler);
        return typeHandler;
    }
    public abstract object MapNativeDbType(DbColumnInfo columnInfo);
    public abstract bool MapTables(string connectionString, IEntityMapProvider entityMapProvider, OrmDbFactoryOptions options);
    public virtual bool TryGetMemberAccessSqlFormatter(MemberExpression memberExpr, out Func<ISqlVisitor, SqlSegment, SqlSegment> formatter)
    {
        var memberInfo = memberExpr.Member;
        var cacheKey = HashCode.Combine(memberInfo.DeclaringType, memberInfo);
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
    public virtual bool TryGetMyMemberAccessSqlFormatter(MemberExpression memberExpr, out Func<ISqlVisitor, SqlSegment, SqlSegment> formatter)
    {
        formatter = null;
        return false;
    }
    public virtual bool TryGetMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out Func<ISqlVisitor, MethodCallExpression, Stack<DeferredOperation>, SqlSegment> formatter)
    {
        var methodInfo = methodCallExpr.Method;
        var cacheKey = HashCode.Combine(methodInfo.DeclaringType, methodInfo);
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
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var leftSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                        var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });

                        var leftArgument = visitor.WrapSql(leftSegment);
                        var rightArgument = visitor.WrapSql(rightSegment);
                        return leftSegment.Change($"{leftArgument}={rightArgument}", SqlType.Expression);
                    });
                    return true;
                }
                break;
            case "Compare":
                if (methodInfo.IsStatic && parameterInfos.Length == 2)
                {
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var leftSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[1] });

                        var leftArgument = visitor.WrapSql(leftSegment);
                        var rightArgument = visitor.WrapSql(rightSegment);
                        return leftSegment.Change($"CASE WHEN {leftArgument}={rightArgument} THEN 0 WHEN {leftArgument}>{rightArgument} THEN 1 ELSE -1 END", SqlType.Expression);
                    });
                    return true;
                }
                break;
            case "CompareTo":
                if (!methodInfo.IsStatic && parameterInfos.Length == 1)
                {
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                        var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });

                        var targetArgument = visitor.WrapSql(targetSegment);
                        var rightArgument = visitor.WrapSql(rightSegment);
                        return targetSegment.Change($"CASE WHEN {targetArgument}={rightArgument} THEN 0 WHEN {targetArgument}>{rightArgument} THEN 1 ELSE -1 END", SqlType.Expression);
                    });
                    return true;
                }
                break;
            case "ToString":
                if (!methodInfo.IsStatic && parameterInfos.Length == 0)
                {
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                        if (targetSegment.IsValue) return targetSegment.Change(targetSegment.Value.ToString());
                        if (targetSegment.SqlType == SqlType.OnlyField && targetSegment.IsEnum)
                            return visitor.ToEnumString(targetSegment);
                        return targetSegment.Change(this.CastTo(methodInfo.ReturnType, targetSegment.Value), SqlType.MethodCall);
                    });
                    return true;
                }
                if (!methodInfo.IsStatic && parameterInfos.Length == 1 && (parameterInfos[0].ParameterType == typeof(string)
                    || typeof(IFormatProvider).IsAssignableFrom(parameterInfos[0].ParameterType)))
                {
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                        var args0Segment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if (targetSegment.IsValue && args0Segment.IsValue)
                            return targetSegment.Change(methodInfo.Invoke(targetSegment.Value, new object[] { args0Segment.Value }));
                        return visitor.VisitDeferredSqlSegment(targetSegment.Next(methodCallExpr));
                    });
                    return true;
                }
                if (!methodInfo.IsStatic && parameterInfos.Length == 2 && parameterInfos[0].ParameterType == typeof(string) && typeof(IFormatProvider).IsAssignableFrom(parameterInfos[1].ParameterType))
                {
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                        var args0Segment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        var args1Segment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[1] });
                        if (targetSegment.IsValue)
                            return targetSegment.Change(methodInfo.Invoke(targetSegment.Value, new object[] { args0Segment.Value, args1Segment.Value }));
                        return visitor.VisitDeferredSqlSegment(targetSegment.Next(methodCallExpr));
                    });
                    return true;
                }
                break;
            case "Parse":
                //Enum.Parse
                if (methodInfo.IsStatic && methodInfo.DeclaringType == typeof(Enum))
                {
                    var genericArguments = methodInfo.GetGenericArguments();
                    if (genericArguments.Length > 0)
                    {
                        var enumType = methodInfo.GetGenericArguments()[0];
                        if (parameterInfos.Length == 1)
                        {
                            formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                            {
                                var args0Segment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                                if (!args0Segment.IsValue)
                                    throw new NotSupportedException("不支持的表达式访问，Enum.Parse方法只支持常量、变量参数");
                                return args0Segment.Change(Enum.Parse(enumType, args0Segment.Value.ToString(), true));
                            });
                        }
                        else
                        {
                            formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                            {
                                var args0Segment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                                var args1Segment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[1] });
                                if (!args0Segment.IsValue || !args1Segment.IsValue)
                                    throw new NotSupportedException("不支持的表达式访问，Enum.Parse方法只支持常量、变量参数");
                                var ignoreCase = (bool)args1Segment.Value;
                                return args0Segment.Change(Enum.Parse(enumType, args0Segment.Value.ToString(), ignoreCase));
                            });
                        }
                        return true;
                    }
                    else
                    {
                        if (parameterInfos.Length == 2)
                        {
                            formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                            {
                                var args0Segment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                                var args1Segment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[1] });
                                if (!args0Segment.IsValue || !args1Segment.IsValue)
                                    throw new NotSupportedException("不支持的表达式访问，Enum.Parse方法只支持常量、变量参数");
                                var enumType = args0Segment.Value as Type;
                                return args0Segment.Change(Enum.Parse(enumType, args1Segment.Value.ToString(), true));
                            });
                        }
                        else
                        {
                            formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                            {
                                var args0Segment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                                var args1Segment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[1] });
                                var args2Segment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[2] });
                                if (!args0Segment.IsValue || !args1Segment.IsValue || !args2Segment.IsValue)
                                    throw new NotSupportedException("不支持的表达式访问，Enum.Parse方法只支持常量、变量参数");
                                var enumType = args0Segment.Value as Type;
                                var ignoreCase = (bool)args2Segment.Value;
                                return args0Segment.Change(Enum.Parse(enumType, args1Segment.Value.ToString(), ignoreCase));
                            });
                        }
                        return true;
                    }
                }
                //int.Parse,double.Parse
                if (methodInfo.IsStatic && parameterInfos.Length >= 1)
                {
                    if (parameterInfos[0].ParameterType == typeof(string))
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var args0Segment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            var arguments = new List<object>();
                            for (int i = 1; i < methodCallExpr.Arguments.Count; i++)
                            {
                                var sqlSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[i] });
                                if (!sqlSegment.IsValue)
                                    throw new NotSupportedException($"不支持的表达式访问，{methodInfo.DeclaringType}.Parse方法，第2个以后的参数只支持常量、变量");
                                arguments.Add(sqlSegment.Value);
                            }
                            if (args0Segment.IsValue)
                                return args0Segment.Change(methodInfo.Invoke(null, arguments.ToArray()));
                            return args0Segment.Change(this.CastTo(methodInfo.DeclaringType, arguments[0]), SqlType.MethodCall);
                        });
                        return true;
                    }
                }
                break;
            case "get_Item":
                if (!methodInfo.IsStatic && parameterInfos.Length > 0)
                {
                    if (parameterInfos.Length > 1)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                            if (!targetSegment.IsValue)
                                throw new NotSupportedException("不支持的表达式访问，get_Item索引方法只支持常量、变量参数");
                            var arguments = new List<object>();
                            for (int i = 0; i < methodCallExpr.Arguments.Count; i++)
                            {
                                var argumentSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[i] });
                                if (!argumentSegment.IsValue)
                                    throw new NotSupportedException("不支持的表达式访问，get_Item索引方法只支持常量、变量参数");
                                arguments.Add(argumentSegment.Value);
                            }
                            return targetSegment.Change(methodInfo.Invoke(targetSegment.Value, arguments.ToArray()));
                        });
                    }
                    else
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                            var argumentSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            if (!targetSegment.IsValue || !argumentSegment.IsValue)
                                throw new NotSupportedException("不支持的表达式访问，get_Item索引方法只支持常量、变量参数");
                            return targetSegment.Change(methodInfo.Invoke(targetSegment.Value, [argumentSegment.Value]));
                        });
                    }
                    return true;
                }
                break;
        }
        return false;
    }
    public virtual bool TryGetMyMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out Func<ISqlVisitor, MethodCallExpression, Stack<DeferredOperation>, SqlSegment> formatter)
    {
        formatter = null;
        return false;
    }
    public abstract bool TryGetStringMemberAccessSqlFormatter(MemberExpression memberExpr, out Func<ISqlVisitor, SqlSegment, SqlSegment> formatter);
    public abstract bool TryGetStringMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out Func<ISqlVisitor, MethodCallExpression, Stack<DeferredOperation>, SqlSegment> formatter);
    public abstract bool TryGetDateTimeMemberAccessSqlFormatter(MemberExpression memberExpr, out Func<ISqlVisitor, SqlSegment, SqlSegment> formatter);
    public abstract bool TryGetDateTimeMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out Func<ISqlVisitor, MethodCallExpression, Stack<DeferredOperation>, SqlSegment> formatter);
    public abstract bool TryGetTimeSpanMemberAccessSqlFormatter(MemberExpression memberExpr, out Func<ISqlVisitor, SqlSegment, SqlSegment> formatter);
    public abstract bool TryGetTimeSpanMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out Func<ISqlVisitor, MethodCallExpression, Stack<DeferredOperation>, SqlSegment> formatter);
    public abstract bool TryGetDateOnlyMemberAccessSqlFormatter(MemberExpression memberExpr, out Func<ISqlVisitor, SqlSegment, SqlSegment> formatter);
    public abstract bool TryGetDateOnlyMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out Func<ISqlVisitor, MethodCallExpression, Stack<DeferredOperation>, SqlSegment> formatter);
    public abstract bool TryGetTimeOnlyMemberAccessSqlFormatter(MemberExpression memberExpr, out Func<ISqlVisitor, SqlSegment, SqlSegment> formatter);
    public abstract bool TryGetTimeOnlyMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out Func<ISqlVisitor, MethodCallExpression, Stack<DeferredOperation>, SqlSegment> formatter);
    public virtual bool TryGetConvertMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out Func<ISqlVisitor, MethodCallExpression, Stack<DeferredOperation>, SqlSegment> formatter)
    {
        var result = false;
        formatter = null;
        var methodInfo = methodCallExpr.Method;
        var parameterInfos = methodInfo.GetParameters();
        var cacheKey = HashCode.Combine(methodInfo.DeclaringType, methodInfo);
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
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var args0Segment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if (args0Segment.IsValue)
                            return args0Segment.Change(methodInfo.Invoke(null, [args0Segment.Value]));
                        return args0Segment.Change(this.CastTo(methodCallExpr.Type, args0Segment.Value), SqlType.MethodCall);
                    });
                    return true;
                }
                break;
            case "ToString":
                if (parameterInfos.Length == 1)
                {
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var args0Segment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if (args0Segment.IsValue)
                            return args0Segment.Change(methodInfo.Invoke(null, new object[] { args0Segment.Value }));
                        if (args0Segment.IsEnum && args0Segment.SqlType == SqlType.OnlyField)
                            return visitor.ToEnumString(args0Segment);
                        return args0Segment.Change(this.CastTo(methodCallExpr.Type, args0Segment.Value), SqlType.MethodCall);
                    });
                    result = true;
                }
                break;
        }
        return result;
    }
    public virtual bool TryGetIEnumerableMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out Func<ISqlVisitor, MethodCallExpression, Stack<DeferredOperation>, SqlSegment> formatter)
    {
        var result = false;
        formatter = null;
        var methodInfo = methodCallExpr.Method;
        var parameterInfos = methodInfo.GetParameters();
        var cacheKey = HashCode.Combine(methodInfo.DeclaringType, methodInfo);
        switch (methodInfo.Name)
        {
            case "Contains":
                //public static bool Contains<T>(this ReadOnlySpan<T> span, T value) where T : IEquatable<T>
                //public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value);
                //public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value, IEqualityComparer<TSource>? comparer);
                if (methodInfo.IsStatic)
                {
                    if (parameterInfos.Length > 2)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var enumerableOrSapnSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            var elementSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[1] });
                            var comparerSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[2] });

                            if (enumerableOrSapnSegment.IsValue && elementSegment.IsValue && comparerSegment.IsValue)
                            {
                                var isContains = (bool)methodInfo.Invoke(null, [enumerableOrSapnSegment.Value, elementSegment.Value, comparerSegment.Value]);
                                var wrapSql = isContains && !deferredOperations.HasNotOperation(out _) ? "1=1" : "1=0";
                                return enumerableOrSapnSegment.Change(wrapSql);
                            }
                            throw new NotSupportedException("不支持的表达式访问，Contains不支持3个以上的非常量、变量参数的表达式访问");
                        });
                    }
                    else
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var enumerableOrSapnSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            var elementSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[1] });

                            if (enumerableOrSapnSegment.IsValue && elementSegment.IsValue)
                            {
                                var isContains = (bool)methodInfo.Invoke(null, [enumerableOrSapnSegment.Value, elementSegment.Value]);
                                var wrapSql = isContains && !deferredOperations.HasNotOperation(out _) ? "1=1" : "1=0";
                                return enumerableOrSapnSegment.Change(wrapSql);
                            }

                            var enumerable = enumerableOrSapnSegment.Value as IEnumerable;
                            Func<object, string> valueFormater = enumerableOrSapnSegment.SqlType == SqlType.Variable ?
                                value => visitor.WrapParameterSql(value) : value => this.GetQuotedValue(value);
                            var builder = new StringBuilder();
                            foreach (var item in enumerable)
                            {
                                if (builder.Length > 0)
                                    builder.Append(',');
                                builder.Append(valueFormater(item));
                            }

                            if (builder.Length > 0)
                            {
                                string elementArgument = visitor.WrapSql(elementSegment);
                                var notString = deferredOperations.HasNotOperation(out _) ? "NOT " : "";
                                return elementSegment.Change($"{elementArgument} {notString}IN ({builder})", SqlType.Expression);
                            }
                            else return elementSegment.Change("1=0", SqlType.Expression);
                        });
                    }
                    result = true;
                }
                //IEnumerable<T>,List<T>
                //public bool Contains(T item);
                if (!methodInfo.IsStatic && parameterInfos.Length == 1 && methodInfo.DeclaringType.GenericTypeArguments.Length > 0
                     && typeof(IEnumerable<>).MakeGenericType(methodInfo.DeclaringType.GenericTypeArguments[0]).IsAssignableFrom(methodInfo.DeclaringType))
                {
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var elementSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });

                        var enumerable = targetSegment.Value as IEnumerable;
                        Func<object, string> valueFormater = targetSegment.SqlType == SqlType.Variable ?
                            value => visitor.WrapParameterSql(value) : value => this.GetQuotedValue(value);
                        var builder = new StringBuilder();
                        foreach (var item in enumerable)
                        {
                            if (builder.Length > 0)
                                builder.Append(',');
                            builder.Append(valueFormater(item));
                        }

                        if (builder.Length > 0)
                        {
                            string elementArgument = visitor.WrapSql(elementSegment);
                            var notString = deferredOperations.HasNotOperation(out _) ? "NOT " : "";
                            return elementSegment.Change($"{elementArgument} {notString}IN ({builder})", SqlType.Expression);
                        }
                        else return elementSegment.Change("1=0", SqlType.Expression);
                    });
                    return true;
                }
                break;
            case "Reverse":
                if (!methodInfo.IsStatic && parameterInfos.Length == 1 && methodInfo.DeclaringType.GenericTypeArguments.Length > 0
                    && methodInfo.DeclaringType.GenericTypeArguments[0] == typeof(char))
                {
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var args0Segment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if (args0Segment.IsValue)
                            return args0Segment.Change(methodInfo.Invoke(args0Segment.Value, null));
                        return args0Segment.Change($"REVERSE({args0Segment.Value})", SqlType.MethodCall);
                    });
                    result = true;
                }
                break;
        }
        return result;
    }
    public abstract bool TryGetMathMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out Func<ISqlVisitor, MethodCallExpression, Stack<DeferredOperation>, SqlSegment> formatter);
    public (string, string) GetFullTableName(string tableName)
    {
        if (tableName.Contains('.'))
        {
            var myTableNames = tableName.Split('.');
            return (myTableNames[0], myTableNames[1]);
        }
        return (null, tableName);
    }
}