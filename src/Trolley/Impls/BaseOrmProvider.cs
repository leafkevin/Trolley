using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

public abstract partial class BaseOrmProvider : IOrmProvider
{
    protected static readonly ConcurrentDictionary<int, MemberAccessSqlFormatter> memberAccessSqlFormatterCache = new();
    protected static readonly ConcurrentDictionary<int, MethodCallSqlFormatter> methodCallSqlFormatterCache = new();
    protected static readonly ConcurrentDictionary<int, Delegate> methodCallCache = new();
    protected static readonly ConcurrentDictionary<Type, ITypeHandler> typeHandlers = new();
    protected static readonly ConcurrentDictionary<int, ITypeHandler> typeHandlerMap = new();

    static BaseOrmProvider()
    {
        typeHandlers[typeof(BooleanAsIntTypeHandler)] = new BooleanAsIntTypeHandler();
        typeHandlers[typeof(NullableBooleanAsIntTypeHandler)] = new NullableBooleanAsIntTypeHandler();

        typeHandlers[typeof(NumberTypeHandler)] = new NumberTypeHandler();
        typeHandlers[typeof(NullableNumberTypeHandler)] = new NullableNumberTypeHandler();

        typeHandlers.TryAdd(typeof(ConvertNumberTypeHandler<byte>), new ConvertNumberTypeHandler<byte>());
        typeHandlers.TryAdd(typeof(ConvertNumberTypeHandler<sbyte>), new ConvertNumberTypeHandler<sbyte>());
        typeHandlers.TryAdd(typeof(ConvertNumberTypeHandler<short>), new ConvertNumberTypeHandler<short>());
        typeHandlers.TryAdd(typeof(ConvertNumberTypeHandler<ushort>), new ConvertNumberTypeHandler<ushort>());
        typeHandlers.TryAdd(typeof(ConvertNumberTypeHandler<int>), new ConvertNumberTypeHandler<int>());
        typeHandlers.TryAdd(typeof(ConvertNumberTypeHandler<uint>), new ConvertNumberTypeHandler<uint>());
        typeHandlers.TryAdd(typeof(ConvertNumberTypeHandler<long>), new ConvertNumberTypeHandler<long>());
        typeHandlers.TryAdd(typeof(ConvertNumberTypeHandler<ulong>), new ConvertNumberTypeHandler<ulong>());

        typeHandlers.TryAdd(typeof(NullableConvertNumberTypeHandler<byte>), new NullableConvertNumberTypeHandler<byte>());
        typeHandlers.TryAdd(typeof(NullableConvertNumberTypeHandler<sbyte>), new NullableConvertNumberTypeHandler<sbyte>());
        typeHandlers.TryAdd(typeof(NullableConvertNumberTypeHandler<short>), new NullableConvertNumberTypeHandler<short>());
        typeHandlers.TryAdd(typeof(NullableConvertNumberTypeHandler<ushort>), new NullableConvertNumberTypeHandler<ushort>());
        typeHandlers.TryAdd(typeof(NullableConvertNumberTypeHandler<int>), new NullableConvertNumberTypeHandler<int>());
        typeHandlers.TryAdd(typeof(NullableConvertNumberTypeHandler<uint>), new NullableConvertNumberTypeHandler<uint>());
        typeHandlers.TryAdd(typeof(NullableConvertNumberTypeHandler<long>), new NullableConvertNumberTypeHandler<long>());
        typeHandlers.TryAdd(typeof(NullableConvertNumberTypeHandler<ulong>), new NullableConvertNumberTypeHandler<ulong>());

        typeHandlers.TryAdd(typeof(EnumTypeHandler<byte>), new EnumTypeHandler<byte>());
        typeHandlers.TryAdd(typeof(EnumTypeHandler<sbyte>), new EnumTypeHandler<sbyte>());
        typeHandlers.TryAdd(typeof(EnumTypeHandler<short>), new EnumTypeHandler<short>());
        typeHandlers.TryAdd(typeof(EnumTypeHandler<ushort>), new EnumTypeHandler<ushort>());
        typeHandlers.TryAdd(typeof(EnumTypeHandler<int>), new EnumTypeHandler<int>());
        typeHandlers.TryAdd(typeof(EnumTypeHandler<uint>), new EnumTypeHandler<uint>());
        typeHandlers.TryAdd(typeof(EnumTypeHandler<long>), new EnumTypeHandler<long>());
        typeHandlers.TryAdd(typeof(EnumTypeHandler<ulong>), new EnumTypeHandler<ulong>());

        typeHandlers.TryAdd(typeof(NullableEnumTypeHandler<byte>), new NullableEnumTypeHandler<byte>());
        typeHandlers.TryAdd(typeof(NullableEnumTypeHandler<sbyte>), new NullableEnumTypeHandler<sbyte>());
        typeHandlers.TryAdd(typeof(NullableEnumTypeHandler<short>), new NullableEnumTypeHandler<short>());
        typeHandlers.TryAdd(typeof(NullableEnumTypeHandler<ushort>), new NullableEnumTypeHandler<ushort>());
        typeHandlers.TryAdd(typeof(NullableEnumTypeHandler<int>), new NullableEnumTypeHandler<int>());
        typeHandlers.TryAdd(typeof(NullableEnumTypeHandler<uint>), new NullableEnumTypeHandler<uint>());
        typeHandlers.TryAdd(typeof(NullableEnumTypeHandler<long>), new NullableEnumTypeHandler<long>());
        typeHandlers.TryAdd(typeof(NullableEnumTypeHandler<ulong>), new NullableEnumTypeHandler<ulong>());

        typeHandlers.TryAdd(typeof(ConvertEnumTypeHandler<byte>), new ConvertEnumTypeHandler<byte>());
        typeHandlers.TryAdd(typeof(ConvertEnumTypeHandler<sbyte>), new ConvertEnumTypeHandler<sbyte>());
        typeHandlers.TryAdd(typeof(ConvertEnumTypeHandler<short>), new ConvertEnumTypeHandler<short>());
        typeHandlers.TryAdd(typeof(ConvertEnumTypeHandler<ushort>), new ConvertEnumTypeHandler<ushort>());
        typeHandlers.TryAdd(typeof(ConvertEnumTypeHandler<int>), new ConvertEnumTypeHandler<int>());
        typeHandlers.TryAdd(typeof(ConvertEnumTypeHandler<uint>), new ConvertEnumTypeHandler<uint>());
        typeHandlers.TryAdd(typeof(ConvertEnumTypeHandler<long>), new ConvertEnumTypeHandler<long>());
        typeHandlers.TryAdd(typeof(ConvertEnumTypeHandler<ulong>), new ConvertEnumTypeHandler<ulong>());

        typeHandlers.TryAdd(typeof(NullableConvertEnumTypeHandler<byte>), new NullableConvertEnumTypeHandler<byte>());
        typeHandlers.TryAdd(typeof(NullableConvertEnumTypeHandler<sbyte>), new NullableConvertEnumTypeHandler<sbyte>());
        typeHandlers.TryAdd(typeof(NullableConvertEnumTypeHandler<short>), new NullableConvertEnumTypeHandler<short>());
        typeHandlers.TryAdd(typeof(NullableConvertEnumTypeHandler<ushort>), new NullableConvertEnumTypeHandler<ushort>());
        typeHandlers.TryAdd(typeof(NullableConvertEnumTypeHandler<int>), new NullableConvertEnumTypeHandler<int>());
        typeHandlers.TryAdd(typeof(NullableConvertEnumTypeHandler<uint>), new NullableConvertEnumTypeHandler<uint>());
        typeHandlers.TryAdd(typeof(NullableConvertEnumTypeHandler<long>), new NullableConvertEnumTypeHandler<long>());
        typeHandlers.TryAdd(typeof(NullableConvertEnumTypeHandler<ulong>), new NullableConvertEnumTypeHandler<ulong>());

        typeHandlers[typeof(EnumAsStringTypeHandler)] = new EnumAsStringTypeHandler();
        typeHandlers[typeof(NullableEnumAsStringTypeHandler)] = new NullableEnumAsStringTypeHandler();

        typeHandlers[typeof(CharTypeHandler)] = new CharTypeHandler();
        typeHandlers[typeof(NullableCharTypeHandler)] = new NullableCharTypeHandler();
        typeHandlers[typeof(StringTypeHandler)] = new StringTypeHandler();
        typeHandlers[typeof(NullableStringTypeHandler)] = new NullableStringTypeHandler();

        typeHandlers[typeof(DateTimeOffsetTypeHandler)] = new DateTimeOffsetTypeHandler();
        typeHandlers[typeof(NullableDateTimeOffsetTypeHandler)] = new NullableDateTimeOffsetTypeHandler();

        typeHandlers[typeof(DateTimeTypeHandler)] = new DateTimeTypeHandler();
        typeHandlers[typeof(NullableDateTimeTypeHandler)] = new NullableDateTimeTypeHandler();
        typeHandlers[typeof(DateTimeAsStringTypeHandler)] = new DateTimeAsStringTypeHandler();
        typeHandlers[typeof(NullableDateTimeAsStringTypeHandler)] = new NullableDateTimeAsStringTypeHandler();

        typeHandlers[typeof(DateOnlyTypeHandler)] = new DateOnlyTypeHandler();
        typeHandlers[typeof(NullableDateOnlyTypeHandler)] = new NullableDateOnlyTypeHandler();
        typeHandlers[typeof(DateOnlyAsStringTypeHandler)] = new DateOnlyAsStringTypeHandler();
        typeHandlers[typeof(NullableDateOnlyAsStringTypeHandler)] = new NullableDateOnlyAsStringTypeHandler();

        typeHandlers[typeof(TimeSpanTypeHandler)] = new TimeSpanTypeHandler();
        typeHandlers[typeof(NullableTimeSpanTypeHandler)] = new NullableTimeSpanTypeHandler();
        typeHandlers[typeof(TimeSpanAsStringTypeHandler)] = new TimeSpanAsStringTypeHandler();
        typeHandlers[typeof(NullableTimeSpanAsStringTypeHandler)] = new NullableTimeSpanAsStringTypeHandler();
        typeHandlers[typeof(TimeSpanAsLongTypeHandler)] = new TimeSpanAsLongTypeHandler();
        typeHandlers[typeof(NullableTimeSpanAsLongTypeHandler)] = new NullableTimeSpanAsLongTypeHandler();

        typeHandlers[typeof(TimeOnlyTypeHandler)] = new TimeOnlyTypeHandler();
        typeHandlers[typeof(NullableTimeOnlyTypeHandler)] = new NullableTimeOnlyTypeHandler();
        typeHandlers[typeof(TimeOnlyAsStringTypeHandler)] = new TimeOnlyAsStringTypeHandler();
        typeHandlers[typeof(NullableTimeOnlyAsStringTypeHandler)] = new NullableTimeOnlyAsStringTypeHandler();
        typeHandlers[typeof(TimeOnlyAsLongTypeHandler)] = new TimeOnlyAsLongTypeHandler();
        typeHandlers[typeof(NullableTimeOnlyAsLongTypeHandler)] = new NullableTimeOnlyAsLongTypeHandler();

        typeHandlers[typeof(GuidTypeHandler)] = new GuidTypeHandler();
        typeHandlers[typeof(NullableGuidTypeHandler)] = new NullableGuidTypeHandler();
        typeHandlers[typeof(GuidAsStringTypeHandler)] = new GuidAsStringTypeHandler();
        typeHandlers[typeof(NullableGuidAsStringTypeHandler)] = new NullableGuidAsStringTypeHandler();

        typeHandlers[typeof(JsonTypeHandler)] = new JsonTypeHandler();
        typeHandlers[typeof(ToStringTypeHandler)] = new ToStringTypeHandler();
    }
    public virtual OrmProviderType OrmProviderType => OrmProviderType.Basic;
    public virtual string ParameterPrefix => "@";
    public abstract Type NativeDbTypeType { get; }
    public virtual ICollection<ITypeHandler> TypeHandlers => typeHandlers.Values;
    public abstract IDbConnection CreateConnection(string connectionString);
    public abstract IDbDataParameter CreateParameter(string parameterName, object value);
    public abstract IDbDataParameter CreateParameter(string parameterName, object nativeDbType, object value);
    public abstract IRepository CreateRepository(DbContext dbContext);

    public virtual string GetTableName(string entityName) => entityName;
    public virtual string GetFieldName(string fieldName) => fieldName;
    public virtual string GetPagingTemplate(int? skip, int? limit, string orderBy = null)
    {
        var builder = new StringBuilder("SELECT /**fields**/ FROM /**tables**/ /**others**/");
        if (!String.IsNullOrEmpty(orderBy)) builder.Append($" {orderBy}");
        if (limit.HasValue) builder.Append($" LIMIT {limit}");
        if (skip.HasValue) builder.Append($" OFFSET {skip}");
        return builder.ToString();
    }
    public abstract object GetNativeDbType(Type type);
    public abstract Type MapDefaultType(object nativeDbType);
    public abstract string CastTo(Type type, object value);
    public virtual string GetIdentitySql(Type entityType) => ";SELECT @@IDENTITY";
    public virtual string GetQuotedValue(Type expectType, object value)
    {
        if (value == null) return "NULL";
        expectType ??= value.GetType();
        if (this.TryGetDefaultTypeHandler(expectType, out var typeHandler))
            return typeHandler.GetQuotedValue(this, expectType, value);
        if (value is SqlSegment sqlSegment)
        {
            if (sqlSegment == SqlSegment.Null || !sqlSegment.IsConstant)
                return sqlSegment.ToString();
            //此处不应出现变量的情况，应该在此之前把变量都已经变成了参数
            //if (sqlSegment.IsVariable) throw new Exception("此处不应出现变量的情况，先调用ISqlVisitor.Change方法把变量都变成参数后，再调用本方法");
            return this.GetQuotedValue(sqlSegment.Value);
        }
        return value.ToString();
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
    public virtual ITypeHandler CreateTypeHandler(Type typeHandlerType)
    {
        if (typeHandlers.TryGetValue(typeHandlerType, out var typeHandler))
            return typeHandler;
        return Activator.CreateInstance(typeHandlerType) as ITypeHandler;
    }
    public virtual ITypeHandler GetTypeHandler(Type targetType, Type fieldType, bool isRequired)
    {
        var hashKey = HashCode.Combine(targetType, fieldType, isRequired);
        return typeHandlerMap.GetOrAdd(hashKey, f =>
        {
            Type handlerType = null;
            Type underlyingType = null;
            var isNullable = targetType.IsNullableType(out underlyingType);
            if (underlyingType.IsEnumType(out _))
            {
                if (fieldType == typeof(string))
                    handlerType = isNullable ? typeof(NullableEnumAsStringTypeHandler) : typeof(EnumAsStringTypeHandler);
                else if (Enum.GetUnderlyingType(underlyingType) != fieldType)
                {
                    handlerType = isNullable ? typeof(NullableConvertEnumTypeHandler<>) : typeof(ConvertEnumTypeHandler<>);
                    handlerType = handlerType.MakeGenericType(fieldType);
                }
                else
                {
                    handlerType = isNullable ? typeof(NullableEnumTypeHandler<>) : typeof(EnumTypeHandler<>);
                    handlerType = handlerType.MakeGenericType(fieldType);
                }
            }
            else if (underlyingType == typeof(Guid))
            {
                if (fieldType == typeof(string))
                    handlerType = isNullable ? typeof(NullableGuidAsStringTypeHandler) : typeof(GuidAsStringTypeHandler);
                else handlerType = isNullable ? typeof(GuidTypeHandler) : typeof(NullableGuidTypeHandler);
            }
            else if (underlyingType == typeof(DateTimeOffset))
            {
                if (fieldType == typeof(string))
                    handlerType = isNullable ? typeof(NullableDateTimeOffsetAsStringTypeHandler) : typeof(DateTimeOffsetAsStringTypeHandler);
                else handlerType = isNullable ? typeof(NullableDateTimeOffsetTypeHandler) : typeof(DateTimeOffsetTypeHandler);
            }
            else if (underlyingType == typeof(DateOnly))
            {
                if (fieldType == typeof(string))
                    handlerType = isNullable ? typeof(NullableDateOnlyAsStringTypeHandler) : typeof(DateOnlyAsStringTypeHandler);
                else handlerType = isNullable ? typeof(NullableDateOnlyTypeHandler) : typeof(DateOnlyTypeHandler);
            }
            else if (underlyingType == typeof(TimeSpan))
            {
                if (fieldType == typeof(long))
                    handlerType = isNullable ? typeof(NullableTimeSpanAsLongTypeHandler) : typeof(TimeSpanAsLongTypeHandler);
                else if (fieldType == typeof(string))
                    handlerType = isNullable ? typeof(NullableTimeSpanAsStringTypeHandler) : typeof(TimeSpanAsStringTypeHandler);
                else handlerType = isNullable ? typeof(NullableTimeSpanTypeHandler) : typeof(TimeSpanTypeHandler);
            }
            else if (underlyingType == typeof(TimeOnly))
            {
                if (fieldType == typeof(long))
                    handlerType = isNullable ? typeof(NullableTimeOnlyAsLongTypeHandler) : typeof(TimeOnlyAsLongTypeHandler);
                else if (fieldType == typeof(string))
                    handlerType = isNullable ? typeof(NullableTimeOnlyAsStringTypeHandler) : typeof(TimeOnlyAsStringTypeHandler);
                else handlerType = isNullable ? typeof(NullableTimeOnlyTypeHandler) : typeof(TimeOnlyTypeHandler);
            }
            else
            {
                switch (Type.GetTypeCode(underlyingType))
                {
                    case TypeCode.Boolean:
                        handlerType = isNullable ? typeof(NullableBooleanAsIntTypeHandler) : typeof(BooleanAsIntTypeHandler);
                        break;
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
                        if (fieldType == underlyingType)
                            handlerType = isNullable ? typeof(NullableNumberTypeHandler) : typeof(NumberTypeHandler);
                        else
                        {
                            handlerType = isNullable ? typeof(NullableConvertNumberTypeHandler<>) : typeof(ConvertNumberTypeHandler<>);
                            handlerType = handlerType.MakeGenericType(fieldType);
                        }
                        break;
                    case TypeCode.Char:
                        handlerType = typeof(CharTypeHandler);
                        break;
                    case TypeCode.String:
                        handlerType = isRequired ? typeof(StringTypeHandler) : typeof(NullableStringTypeHandler);
                        break;
                    case TypeCode.DateTime:
                        if (fieldType == typeof(string))
                            handlerType = isNullable ? typeof(NullableDateTimeAsStringTypeHandler) : typeof(DateTimeAsStringTypeHandler);
                        else handlerType = isNullable ? typeof(NullableDateTimeTypeHandler) : typeof(DateTimeTypeHandler);
                        break;
                    case TypeCode.Object:
                        if (fieldType == typeof(string))
                            handlerType = typeof(JsonTypeHandler);
                        break;
                    default:
                        handlerType = typeof(ToStringTypeHandler);
                        break;
                }
            }
            if (typeHandlers.TryGetValue(handlerType, out var typeHandler))
                return typeHandler;
            return null;
        });
    }
    public abstract bool TryGetDefaultTypeHandler(Type targetType, out ITypeHandler typeHandler);
    public virtual bool TryGetMemberAccessSqlFormatter(MemberExpression memberExpr, out MemberAccessSqlFormatter formatter)
    {
        var memberInfo = memberExpr.Member;
        var cacheKey = HashCode.Combine(memberInfo.DeclaringType, memberInfo);
        if (memberAccessSqlFormatterCache.TryGetValue(cacheKey, out formatter))
            return true;

        if (memberInfo.DeclaringType == typeof(string) && this.TryGetStringMemberAccessSqlFormatter(memberExpr, out formatter))
            return true;
        if (memberInfo.DeclaringType == typeof(DateTime) && this.TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out formatter))
            return true;
        if (memberInfo.DeclaringType == typeof(TimeSpan) && this.TryGetTimeSpanMemberAccessSqlFormatter(memberExpr, out formatter))
            return true;
        if (memberInfo.DeclaringType == typeof(TimeOnly) && this.TryGetTimeOnlyMemberAccessSqlFormatter(memberExpr, out formatter))
            return true;
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
        var cacheKey = HashCode.Combine(methodInfo.DeclaringType, methodInfo);
        if (methodCallSqlFormatterCache.TryGetValue(cacheKey, out formatter))
            return true;

        if (methodInfo.DeclaringType == typeof(string) && this.TryGetStringMethodCallSqlFormatter(methodCallExpr, out formatter))
            return true;
        if (methodInfo.DeclaringType == typeof(DateTime) && this.TryGetDateTimeMethodCallSqlFormatter(methodCallExpr, out formatter))
            return true;
        if (methodInfo.DeclaringType == typeof(TimeSpan) && this.TryGetTimeSpanMethodCallSqlFormatter(methodCallExpr, out formatter))
            return true;
        if (methodInfo.DeclaringType == typeof(TimeOnly) && this.TryGetTimeOnlyMethodCallSqlFormatter(methodCallExpr, out formatter))
            return true;
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
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        visitor.ChangeSameType(targetSegment, rightSegment);
                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(targetSegment, rightSegment, $"{targetArgument}={rightArgument}", false, false, true);
                    });
                    return true;
                }
                break;
            case "Compare":
                if (methodInfo.IsStatic && parameterInfos.Length == 2)
                {
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var leftSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[1] });
                        visitor.ChangeSameType(leftSegment, rightSegment);
                        var leftArgument = visitor.GetQuotedValue(leftSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return leftSegment.Merge(rightSegment, $"CASE WHEN {leftArgument}={rightArgument} THEN 0 WHEN {leftArgument}>{rightArgument} THEN 1 ELSE -1 END", false, false, true);
                    });
                    return true;
                }
                break;
            case "CompareTo":
                if (!methodInfo.IsStatic && parameterInfos.Length == 1)
                {
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        visitor.ChangeSameType(targetSegment, rightSegment);
                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(rightSegment, $"CASE WHEN {targetArgument}={rightArgument} THEN 0 WHEN {targetArgument}>{rightArgument} THEN 1 ELSE -1 END", false, false, true);
                    });
                    return true;
                }
                break;
            case "ToString":
                if (!methodInfo.IsStatic && parameterInfos.Length == 0)
                {
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                        {
                            targetSegment.ExpectType = methodInfo.ReturnType;
                            return targetSegment.Change(targetSegment.ToString());
                        }
                        targetSegment.ExpectType = methodInfo.ReturnType;
                        return targetSegment.Change(this.CastTo(typeof(string), targetSegment.Value), false, false, false, true);
                    });
                    return true;
                }
                if (!methodInfo.IsStatic && parameterInfos.Length == 1 && (parameterInfos[0].ParameterType == typeof(string) || typeof(IFormatProvider).IsAssignableFrom(parameterInfos[0].ParameterType)))
                {
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var args0Segment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                        {
                            targetSegment.ExpectType = methodInfo.ReturnType;
                            return targetSegment.Change(methodInfo.Invoke(targetSegment.Value, new object[] { args0Segment.Value }));
                        }
                        //f.Balance.ToString("C")
                        //args0.ToString("C")
                        //(args0)=>{args0.ToString("C")}

                        //f.Balance.ToString(new CultureInfo("en-US"))
                        //args.ToString(new CultureInfo("en-US"))
                        //(args)=>{args.ToString(new CultureInfo("en-US"))}
                        if (visitor.IsSelect && (args0Segment.IsConstant || args0Segment.IsVariable))
                        {
                            var argsExpr = Expression.Parameter(target.Type, "args");
                            var newCallExpr = Expression.Call(argsExpr, methodInfo, args);
                            var deferredDelegate = Expression.Lambda(newCallExpr, argsExpr);
                            var fieldName = targetSegment.Value.ToString();
                            return targetSegment.Change(new ReaderField
                            {
                                FieldType = ReaderFieldType.DeferredFields,
                                Body = fieldName,
                                DeferredDelegate = deferredDelegate,
                                ReaderFields = new List<ReaderField>
                                {
                                    new ReaderField
                                    {
                                        FieldType = ReaderFieldType.Field,
                                        FromMember = targetSegment.FromMember,
                                        TargetMember = targetSegment.FromMember,
                                        TargetType = methodInfo.ReturnType,
                                        NativeDbType = targetSegment.NativeDbType,
                                        TypeHandler = targetSegment.TypeHandler,
                                        Body = fieldName
                                    }
                                }
                            });
                        }
                        throw new NotSupportedException("不支持的方法调用，方法.ToString(string format)只支持常量或是变量的解析");
                    });
                    return true;
                }
                if (!methodInfo.IsStatic && parameterInfos.Length == 2 && parameterInfos[0].ParameterType == typeof(string) && typeof(IFormatProvider).IsAssignableFrom(parameterInfos[1].ParameterType))
                {
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var args0Segment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        var args1Segment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[1] });
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                        {
                            targetSegment.ExpectType = methodInfo.ReturnType;
                            return targetSegment.Change(methodInfo.Invoke(targetSegment.Value, new object[] { args0Segment.Value, args1Segment.Value }));
                        }
                        //f.Balance.ToString("C", new CultureInfo("en-US"))
                        //args.ToString("C", new CultureInfo("en-US"))
                        //(args)=>{args.ToString("C", new CultureInfo("en-US"))}
                        if (visitor.IsSelect && (args0Segment.IsConstant || args0Segment.IsVariable) && (args1Segment.IsConstant || args1Segment.IsVariable))
                        {
                            var argsExpr = Expression.Parameter(target.Type, "args");
                            var newCallExpr = Expression.Call(argsExpr, methodInfo, args);
                            var deferredDelegate = Expression.Lambda(newCallExpr, argsExpr);
                            var fieldName = targetSegment.Value.ToString();
                            return targetSegment.Change(new ReaderField
                            {
                                FieldType = ReaderFieldType.DeferredFields,
                                Body = fieldName,
                                DeferredDelegate = deferredDelegate,
                                ReaderFields = new List<ReaderField>
                                {
                                    new ReaderField
                                    {
                                        FieldType = ReaderFieldType.Field,
                                        FromMember = targetSegment.FromMember,
                                        TargetMember = targetSegment.FromMember,
                                        TargetType = methodInfo.ReturnType,
                                        NativeDbType = targetSegment.NativeDbType,
                                        TypeHandler = targetSegment.TypeHandler,
                                        Body = fieldName
                                    }
                                }
                            });
                        }
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
                            var args0Segment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                            if (args0Segment.IsConstant || args0Segment.IsVariable)
                                return args0Segment.Change(Enum.Parse(enumType, args0Segment.Value.ToString()));

                            throw new NotSupportedException("不支持的表达式访问，Enum.Parse方法只支持常量、变量参数");
                        });
                        return true;
                    }
                    if (parameterInfos.Length > 1 && parameterInfos[0].ParameterType == typeof(Type))
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            SqlSegment resultSegment = null;
                            var arguments = new List<object>();
                            Array.ForEach(args, f =>
                            {
                                var sqlSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = f });
                                arguments.Add(sqlSegment.Value);
                                if (resultSegment == null) resultSegment = sqlSegment;
                                else resultSegment.Merge(sqlSegment);
                            });
                            if (resultSegment.IsConstant || resultSegment.IsVariable)
                                return resultSegment.Change(methodInfo.Invoke(null, arguments.ToArray()));

                            throw new NotSupportedException("不支持的表达式访问，Enum.Parse方法只支持常量、变量参数");
                        });
                        return true;
                    }
                }
                if (methodInfo.IsStatic && parameterInfos.Length >= 1)
                {
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        SqlSegment resultSegment = null;
                        var arguments = new List<object>();
                        Array.ForEach(args, f =>
                        {
                            var sqlSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = f });
                            arguments.Add(sqlSegment);
                            if (resultSegment == null) resultSegment = sqlSegment;
                            else resultSegment.Merge(sqlSegment);
                        });
                        if (resultSegment.IsConstant || resultSegment.IsVariable)
                            return resultSegment.Change(methodInfo.Invoke(null, arguments.ToArray()));

                        throw new NotSupportedException("不支持的表达式访问，Enum.Parse方法只支持常量、变量参数");
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
                            var args0Segment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                            if (args0Segment.IsConstant || args0Segment.IsVariable)
                                return args0Segment.Change(Enum.Parse(enumType, args0Segment.Value.ToString()));

                            throw new NotSupportedException("不支持的表达式访问，Enum.Parse方法只支持常量、变量参数");
                        });
                        return true;
                    }
                    if (parameterInfos.Length > 1 && parameterInfos[0].ParameterType == typeof(Type))
                    {
                        var enumType = parameterInfos[0].ParameterType;
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            SqlSegment resultSegment = null;
                            var arguments = new List<object>();
                            for (int i = 0; i < args.Length - 1; i++)
                            {
                                var sqlSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[i] });
                                arguments.Add(sqlSegment.Value);
                                if (resultSegment == null) resultSegment = sqlSegment;
                                else resultSegment.Merge(sqlSegment);
                            }
                            if (resultSegment.IsConstant || resultSegment.IsVariable)
                                return resultSegment.Change(methodInfo.Invoke(null, arguments.ToArray()));

                            throw new NotSupportedException("不支持的表达式访问，Enum.Parse方法只支持常量、变量参数");
                        });
                        return true;
                    }
                }
                if (methodInfo.IsStatic && parameterInfos.Length >= 1)
                {
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        SqlSegment resultSegment = null;
                        var arguments = new List<object>();
                        for (int i = 0; i < args.Length - 1; i++)
                        {
                            var sqlSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[i] });
                            arguments.Add(sqlSegment);
                            if (resultSegment == null) resultSegment = sqlSegment;
                            else resultSegment.Merge(sqlSegment);
                        }
                        if (resultSegment.IsConstant || resultSegment.IsVariable)
                            return resultSegment.Change(methodInfo.Invoke(null, arguments.ToArray()));

                        throw new NotSupportedException("不支持的表达式访问，Enum.Parse方法只支持常量、变量参数");
                    });
                    return true;
                }
                break;
            case "get_Item":
                if (!methodInfo.IsStatic && parameterInfos.Length > 0)
                {
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var arguments = new List<object>();
                        for (int i = 0; i < args.Length; i++)
                        {
                            var argumentSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[i] });
                            targetSegment.Merge(argumentSegment);
                            arguments.Add(argumentSegment.Value);
                        }
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.Change(methodInfo.Invoke(targetSegment.Value, arguments.ToArray()));

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
    public abstract bool TryGetTimeOnlyMemberAccessSqlFormatter(MemberExpression memberExpr, out MemberAccessSqlFormatter formatter);
    public abstract bool TryGetTimeOnlyMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter);
    public abstract bool TryGetConvertMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter);
    public abstract bool TryGetIEnumerableMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter);
    public abstract bool TryGetMathMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter);
}
