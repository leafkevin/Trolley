using MySqlConnector;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Trolley.MySqlConnector;

public partial class MySqlProvider : BaseOrmProvider
{
    private static Dictionary<object, Type> defaultMapTypes = new();
    private static Dictionary<Type, object> defaultDbTypes = new();
    private static Dictionary<Type, string> castTos = new();
    protected readonly ConcurrentDictionary<Type, ITypeHandler> typeHandlers = new();
    protected readonly ConcurrentDictionary<Type, ITypeHandler> defaultTypeHandlers = new();
    protected readonly ConcurrentDictionary<int, ITypeHandler> typeHandlerMap = new();

    public override OrmProviderType OrmProviderType => OrmProviderType.MySql;
    public override Type NativeDbTypeType => typeof(MySqlDbType);
    public override ICollection<ITypeHandler> TypeHandlers => this.typeHandlers.Values;

    static MySqlProvider()
    {
        defaultMapTypes[MySqlDbType.Bit] = typeof(bool);
        defaultMapTypes[MySqlDbType.Bool] = typeof(bool);
        defaultMapTypes[MySqlDbType.Byte] = typeof(sbyte);
        defaultMapTypes[MySqlDbType.UByte] = typeof(byte);
        defaultMapTypes[MySqlDbType.Int16] = typeof(short);
        defaultMapTypes[MySqlDbType.UInt16] = typeof(ushort);
        defaultMapTypes[MySqlDbType.Int24] = typeof(int);
        defaultMapTypes[MySqlDbType.UInt24] = typeof(uint);
        defaultMapTypes[MySqlDbType.Int32] = typeof(int);
        defaultMapTypes[MySqlDbType.UInt32] = typeof(uint);
        defaultMapTypes[MySqlDbType.Int64] = typeof(long);
        defaultMapTypes[MySqlDbType.UInt64] = typeof(ulong);
        defaultMapTypes[MySqlDbType.Float] = typeof(float);
        defaultMapTypes[MySqlDbType.Double] = typeof(double);
        defaultMapTypes[MySqlDbType.NewDecimal] = typeof(decimal);
        defaultMapTypes[MySqlDbType.Decimal] = typeof(decimal);
        defaultMapTypes[MySqlDbType.String] = typeof(string);
        defaultMapTypes[MySqlDbType.VarString] = typeof(string);
        defaultMapTypes[MySqlDbType.VarChar] = typeof(string);
        defaultMapTypes[MySqlDbType.TinyText] = typeof(string);
        defaultMapTypes[MySqlDbType.MediumText] = typeof(string);
        defaultMapTypes[MySqlDbType.LongText] = typeof(string);
        defaultMapTypes[MySqlDbType.Text] = typeof(string);
        defaultMapTypes[MySqlDbType.JSON] = typeof(string);
        defaultMapTypes[MySqlDbType.DateTime] = typeof(DateTime);
        defaultMapTypes[MySqlDbType.Newdate] = typeof(DateTime);
        defaultMapTypes[MySqlDbType.Timestamp] = typeof(DateTime);
        defaultMapTypes[MySqlDbType.Date] = typeof(DateOnly);
        defaultMapTypes[MySqlDbType.Time] = typeof(TimeOnly);
        defaultMapTypes[MySqlDbType.TinyBlob] = typeof(byte[]);
        defaultMapTypes[MySqlDbType.MediumBlob] = typeof(byte[]);
        defaultMapTypes[MySqlDbType.LongBlob] = typeof(byte[]);
        defaultMapTypes[MySqlDbType.Blob] = typeof(byte[]);
        defaultMapTypes[MySqlDbType.Binary] = typeof(byte[]);
        defaultMapTypes[MySqlDbType.VarBinary] = typeof(byte[]);
        defaultMapTypes[MySqlDbType.Guid] = typeof(Guid);

        defaultDbTypes[typeof(bool)] = MySqlDbType.Bool;
        defaultDbTypes[typeof(sbyte)] = MySqlDbType.Byte;
        defaultDbTypes[typeof(byte)] = MySqlDbType.UByte;
        defaultDbTypes[typeof(short)] = MySqlDbType.Int16;
        defaultDbTypes[typeof(ushort)] = MySqlDbType.UInt16;
        defaultDbTypes[typeof(int)] = MySqlDbType.Int32;
        defaultDbTypes[typeof(uint)] = MySqlDbType.UInt32;
        defaultDbTypes[typeof(long)] = MySqlDbType.Int64;
        defaultDbTypes[typeof(ulong)] = MySqlDbType.UInt64;
        defaultDbTypes[typeof(float)] = MySqlDbType.Float;
        defaultDbTypes[typeof(double)] = MySqlDbType.Double;
        defaultDbTypes[typeof(decimal)] = MySqlDbType.Decimal;
        defaultDbTypes[typeof(string)] = MySqlDbType.VarChar;
        defaultDbTypes[typeof(DateTime)] = MySqlDbType.DateTime;
        defaultDbTypes[typeof(DateOnly)] = MySqlDbType.Date;
        defaultDbTypes[typeof(TimeOnly)] = MySqlDbType.Time;
        defaultDbTypes[typeof(Guid)] = MySqlDbType.Guid;
        defaultDbTypes[typeof(byte[])] = MySqlDbType.VarBinary;

        defaultDbTypes[typeof(bool?)] = MySqlDbType.Bool;
        defaultDbTypes[typeof(sbyte?)] = MySqlDbType.Byte;
        defaultDbTypes[typeof(byte?)] = MySqlDbType.UByte;
        defaultDbTypes[typeof(short?)] = MySqlDbType.Int16;
        defaultDbTypes[typeof(ushort?)] = MySqlDbType.UInt16;
        defaultDbTypes[typeof(int?)] = MySqlDbType.Int32;
        defaultDbTypes[typeof(uint?)] = MySqlDbType.UInt32;
        defaultDbTypes[typeof(long?)] = MySqlDbType.Int64;
        defaultDbTypes[typeof(ulong?)] = MySqlDbType.UInt64;
        defaultDbTypes[typeof(float?)] = MySqlDbType.Float;
        defaultDbTypes[typeof(double?)] = MySqlDbType.Double;
        defaultDbTypes[typeof(decimal?)] = MySqlDbType.Decimal;
        defaultDbTypes[typeof(DateTime?)] = MySqlDbType.DateTime;
        defaultDbTypes[typeof(DateOnly?)] = MySqlDbType.Date;
        defaultDbTypes[typeof(TimeOnly?)] = MySqlDbType.Time;
        defaultDbTypes[typeof(Guid?)] = MySqlDbType.Guid;

        castTos[typeof(string)] = "CHAR";
        castTos[typeof(bool)] = "SIGNED";
        castTos[typeof(byte)] = "UNSIGNED";
        castTos[typeof(sbyte)] = "SIGNED";
        castTos[typeof(short)] = "SIGNED";
        castTos[typeof(ushort)] = "UNSIGNED";
        castTos[typeof(int)] = "SIGNED";
        castTos[typeof(uint)] = "UNSIGNED";
        castTos[typeof(long)] = "SIGNED";
        castTos[typeof(ulong)] = "UNSIGNED";
        castTos[typeof(float)] = "FLOAT";
        castTos[typeof(double)] = "DOUBLE";
        castTos[typeof(decimal)] = "DECIMAL(36,18)";
        castTos[typeof(DateTime)] = "DATETIME";
        castTos[typeof(DateOnly)] = "DATE";
        castTos[typeof(TimeOnly)] = "TIME";

        castTos[typeof(bool?)] = "SIGNED";
        castTos[typeof(byte?)] = "UNSIGNED";
        castTos[typeof(sbyte?)] = "SIGNED";
        castTos[typeof(short?)] = "SIGNED";
        castTos[typeof(ushort?)] = "UNSIGNED";
        castTos[typeof(int?)] = "SIGNED";
        castTos[typeof(uint?)] = "UNSIGNED";
        castTos[typeof(long?)] = "SIGNED";
        castTos[typeof(ulong?)] = "UNSIGNED";
        castTos[typeof(float?)] = "FLOAT";
        castTos[typeof(double?)] = "DOUBLE";
        castTos[typeof(decimal?)] = "DECIMAL(36,18)";
        castTos[typeof(DateTime?)] = "DATETIME";
        castTos[typeof(DateOnly?)] = "DATE";
        castTos[typeof(TimeOnly?)] = "TIME";
    }
    public MySqlProvider() => this.RegisterTypeHandlers();
    public override IDbConnection CreateConnection(string connectionString)
        => new MySqlConnection(connectionString);
    public override IDbDataParameter CreateParameter(string parameterName, object value)
        => new MySqlParameter(parameterName, value);
    public override IDbDataParameter CreateParameter(string parameterName, object nativeDbType, object value)
    {
        var parameter = new MySqlParameter(parameterName, (MySqlDbType)nativeDbType);
        parameter.Value = value;
        return parameter;
    }
    public override string GetTableName(string entityName) => "`" + entityName + "`";
    public override string GetFieldName(string fieldName) => "`" + fieldName + "`";
    public override object GetNativeDbType(Type fieldType)
    {
        if (!defaultDbTypes.TryGetValue(fieldType, out var dbType))
            throw new Exception($"类型{fieldType.FullName}没有对应的MySqlConnector.MySqlDbType映射类型");
        return dbType;
    }
    public override Type MapDefaultType(object nativeDbType)
    {
        if (defaultMapTypes.TryGetValue(nativeDbType, out var result))
            return result;
        return typeof(object);
    }
    public override string CastTo(Type type, object value)
        => $"CAST({value} AS {castTos[type]})";

    public virtual void RegisterTypeHandlers()
    {
        this.AddTypeHandler(new BooleanAsIntTypeHandler());
        this.AddTypeHandler(new NullableBooleanAsIntTypeHandler());

        this.AddTypeHandler(new NumberTypeHandler());
        this.AddTypeHandler(new NullableNumberTypeHandler());
        this.AddTypeHandler(new ConvertNumberTypeHandler());
        this.AddTypeHandler(new NullableConvertNumberTypeHandler());

        this.AddTypeHandler(new EnumTypeHandler());
        this.AddTypeHandler(new NullableEnumTypeHandler());
        this.AddTypeHandler(new EnumAsStringTypeHandler());
        this.AddTypeHandler(new NullableEnumAsStringTypeHandler());

        this.AddTypeHandler(new CharTypeHandler());
        this.AddTypeHandler(new NullableCharTypeHandler());
        this.AddTypeHandler(new StringTypeHandler());
        this.AddTypeHandler(new NullableStringTypeHandler());

        this.AddTypeHandler(new DateTimeOffsetTypeHandler());
        this.AddTypeHandler(new NullableDateTimeOffsetTypeHandler());

        this.AddTypeHandler(new DateTimeTypeHandler());
        this.AddTypeHandler(new NullableDateTimeTypeHandler());
        this.AddTypeHandler(new DateTimeAsStringTypeHandler());
        this.AddTypeHandler(new NullableDateTimeAsStringTypeHandler());

        this.AddTypeHandler(new DateOnlyTypeHandler());
        this.AddTypeHandler(new NullableDateOnlyTypeHandler());
        this.AddTypeHandler(new DateOnlyAsStringTypeHandler());
        this.AddTypeHandler(new NullableDateOnlyAsStringTypeHandler());

        this.AddTypeHandler(new TimeSpanTypeHandler());
        this.AddTypeHandler(new NullableTimeSpanTypeHandler());
        this.AddTypeHandler(new TimeSpanAsStringTypeHandler());
        this.AddTypeHandler(new NullableTimeSpanAsStringTypeHandler());
        this.AddTypeHandler(new TimeSpanAsLongTypeHandler());
        this.AddTypeHandler(new NullableTimeSpanAsLongTypeHandler());

        this.AddTypeHandler(new TimeOnlyTypeHandler());
        this.AddTypeHandler(new NullableTimeOnlyTypeHandler());
        this.AddTypeHandler(new TimeOnlyAsStringTypeHandler());
        this.AddTypeHandler(new NullableTimeOnlyAsStringTypeHandler());
        this.AddTypeHandler(new TimeOnlyAsLongTypeHandler());
        this.AddTypeHandler(new NullableTimeOnlyAsLongTypeHandler());

        this.AddTypeHandler(new GuidTypeHandler());
        this.AddTypeHandler(new NullableGuidTypeHandler());
        this.AddTypeHandler(new GuidAsStringTypeHandler());
        this.AddTypeHandler(new NullableGuidAsStringTypeHandler());

        this.AddTypeHandler(new JsonTypeHandler());
        this.AddTypeHandler(new ToStringTypeHandler());


        this.defaultTypeHandlers.TryAdd(typeof(bool), this.typeHandlers[typeof(BooleanAsIntTypeHandler)]);
        this.defaultTypeHandlers.TryAdd(typeof(bool?), this.typeHandlers[typeof(NullableBooleanAsIntTypeHandler)]);
        this.defaultTypeHandlers.TryAdd(typeof(string), this.typeHandlers[typeof(NullableStringTypeHandler)]);
        this.defaultTypeHandlers.TryAdd(typeof(DateTimeOffset), this.typeHandlers[typeof(DateTimeOffsetTypeHandler)]);
        this.defaultTypeHandlers.TryAdd(typeof(DateTimeOffset?), this.typeHandlers[typeof(NullableDateTimeOffsetTypeHandler)]);
        this.defaultTypeHandlers.TryAdd(typeof(byte), this.typeHandlers[typeof(NumberTypeHandler)]);
        this.defaultTypeHandlers.TryAdd(typeof(byte?), this.typeHandlers[typeof(NullableNumberTypeHandler)]);
        this.defaultTypeHandlers.TryAdd(typeof(sbyte), this.typeHandlers[typeof(NumberTypeHandler)]);
        this.defaultTypeHandlers.TryAdd(typeof(sbyte?), this.typeHandlers[typeof(NullableNumberTypeHandler)]);
        this.defaultTypeHandlers.TryAdd(typeof(short), this.typeHandlers[typeof(NumberTypeHandler)]);
        this.defaultTypeHandlers.TryAdd(typeof(ushort?), this.typeHandlers[typeof(NullableNumberTypeHandler)]);
        this.defaultTypeHandlers.TryAdd(typeof(char), this.typeHandlers[typeof(CharTypeHandler)]);
        this.defaultTypeHandlers.TryAdd(typeof(char?), this.typeHandlers[typeof(NullableCharTypeHandler)]);
        this.defaultTypeHandlers.TryAdd(typeof(int), this.typeHandlers[typeof(NumberTypeHandler)]);
        this.defaultTypeHandlers.TryAdd(typeof(uint?), this.typeHandlers[typeof(NullableNumberTypeHandler)]);
        this.defaultTypeHandlers.TryAdd(typeof(long), this.typeHandlers[typeof(NumberTypeHandler)]);
        this.defaultTypeHandlers.TryAdd(typeof(ulong?), this.typeHandlers[typeof(NullableNumberTypeHandler)]);
        this.defaultTypeHandlers.TryAdd(typeof(float), this.typeHandlers[typeof(NumberTypeHandler)]);
        this.defaultTypeHandlers.TryAdd(typeof(float?), this.typeHandlers[typeof(NullableNumberTypeHandler)]);
        this.defaultTypeHandlers.TryAdd(typeof(double), this.typeHandlers[typeof(NumberTypeHandler)]);
        this.defaultTypeHandlers.TryAdd(typeof(double?), this.typeHandlers[typeof(NullableNumberTypeHandler)]);
        this.defaultTypeHandlers.TryAdd(typeof(decimal), this.typeHandlers[typeof(NumberTypeHandler)]);
        this.defaultTypeHandlers.TryAdd(typeof(decimal?), this.typeHandlers[typeof(NullableNumberTypeHandler)]);
        this.defaultTypeHandlers.TryAdd(typeof(DateTime), this.typeHandlers[typeof(DateTimeTypeHandler)]);
        this.defaultTypeHandlers.TryAdd(typeof(DateTime?), this.typeHandlers[typeof(NullableDateTimeTypeHandler)]);
        this.defaultTypeHandlers.TryAdd(typeof(DateOnly), this.typeHandlers[typeof(DateOnlyTypeHandler)]);
        this.defaultTypeHandlers.TryAdd(typeof(DateOnly?), this.typeHandlers[typeof(NullableDateOnlyTypeHandler)]);
        this.defaultTypeHandlers.TryAdd(typeof(TimeSpan), this.typeHandlers[typeof(TimeSpanTypeHandler)]);
        this.defaultTypeHandlers.TryAdd(typeof(TimeSpan?), this.typeHandlers[typeof(NullableTimeSpanTypeHandler)]);
        this.defaultTypeHandlers.TryAdd(typeof(TimeOnly), this.typeHandlers[typeof(TimeOnlyTypeHandler)]);
        this.defaultTypeHandlers.TryAdd(typeof(TimeOnly?), this.typeHandlers[typeof(NullableTimeOnlyTypeHandler)]);
        this.defaultTypeHandlers.TryAdd(typeof(Guid), this.typeHandlers[typeof(GuidTypeHandler)]);
        this.defaultTypeHandlers.TryAdd(typeof(Guid?), this.typeHandlers[typeof(NullableGuidTypeHandler)]);
        this.defaultTypeHandlers.TryAdd(typeof(object), this.typeHandlers[typeof(JsonTypeHandler)]);
    }
    public override void AddTypeHandler(ITypeHandler typeHandler)
        => this.typeHandlers.TryAdd(typeHandler.GetType(), typeHandler);
    public override ITypeHandler GetTypeHandler(Type targetType, Type fieldType, bool isRequired)
    {
        var hashKey = HashCode.Combine(targetType, fieldType, isRequired);
        return this.typeHandlerMap.GetOrAdd(hashKey, f =>
        {
            Type handlerType = null;
            if (targetType.IsNullableType(out var underlyingType))
            {
                if (underlyingType.IsEnumType(out _))
                {
                    if (fieldType == typeof(string))
                        handlerType = typeof(NullableEnumAsStringTypeHandler);
                    else handlerType = typeof(NullableEnumTypeHandler);
                }
                else if (underlyingType == typeof(Guid))
                {
                    if (fieldType == typeof(string))
                        handlerType = typeof(NullableGuidAsStringTypeHandler);
                    else handlerType = typeof(NullableGuidTypeHandler);
                }
                else if (underlyingType == typeof(DateTimeOffset))
                {
                    if (fieldType == typeof(string))
                        handlerType = typeof(NullableDateTimeOffsetAsStringTypeHandler);
                    else handlerType = typeof(NullableDateTimeOffsetTypeHandler);
                }
                else if (underlyingType == typeof(DateOnly))
                {
                    if (fieldType == typeof(string))
                        handlerType = typeof(NullableDateOnlyAsStringTypeHandler);
                    else handlerType = typeof(NullableDateOnlyTypeHandler);
                }
                else if (underlyingType == typeof(TimeSpan))
                {
                    if (fieldType == typeof(long))
                        handlerType = typeof(NullableTimeSpanAsLongTypeHandler);
                    else if (fieldType == typeof(string))
                        handlerType = typeof(NullableTimeSpanAsStringTypeHandler);
                    else handlerType = typeof(NullableTimeSpanTypeHandler);
                }
                else if (underlyingType == typeof(TimeOnly))
                {
                    if (fieldType == typeof(long))
                        handlerType = typeof(NullableTimeOnlyAsLongTypeHandler);
                    else if (fieldType == typeof(string))
                        handlerType = typeof(NullableTimeOnlyAsStringTypeHandler);
                    else handlerType = typeof(NullableTimeOnlyTypeHandler);
                }
                else
                {
                    switch (Type.GetTypeCode(underlyingType))
                    {
                        case TypeCode.Boolean:
                            handlerType = typeof(NullableBooleanAsIntTypeHandler);
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
                                handlerType = typeof(NullableNumberTypeHandler);
                            else handlerType = typeof(NullableConvertNumberTypeHandler);
                            break;
                        case TypeCode.Char:
                            handlerType = typeof(NullableCharTypeHandler);
                            break;
                        case TypeCode.DateTime:
                            if (fieldType == typeof(string))
                                handlerType = typeof(NullableDateTimeAsStringTypeHandler);
                            else handlerType = typeof(NullableDateTimeTypeHandler);
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
            }
            else
            {
                if (underlyingType.IsEnumType(out _))
                {
                    if (fieldType == typeof(string))
                        handlerType = typeof(EnumAsStringTypeHandler);
                    else handlerType = typeof(EnumTypeHandler);
                }
                else if (underlyingType == typeof(Guid))
                {
                    if (fieldType == typeof(string))
                        handlerType = typeof(GuidAsStringTypeHandler);
                    else handlerType = typeof(GuidTypeHandler);
                }
                else if (underlyingType == typeof(DateTimeOffset))
                {
                    if (fieldType == typeof(string))
                        handlerType = typeof(DateTimeOffsetAsStringTypeHandler);
                    else handlerType = typeof(DateTimeOffsetTypeHandler);
                }
                else if (underlyingType == typeof(DateOnly))
                {
                    if (fieldType == typeof(string))
                        handlerType = typeof(DateOnlyAsStringTypeHandler);
                    else handlerType = typeof(DateOnlyTypeHandler);
                }
                else if (underlyingType == typeof(TimeSpan))
                {
                    if (fieldType == typeof(long))
                        handlerType = typeof(TimeSpanAsLongTypeHandler);
                    else if (fieldType == typeof(string))
                        handlerType = typeof(TimeSpanAsStringTypeHandler);
                    else handlerType = typeof(TimeSpanTypeHandler);
                }
                else if (underlyingType == typeof(TimeOnly))
                {
                    if (fieldType == typeof(long))
                        handlerType = typeof(TimeOnlyAsLongTypeHandler);
                    else if (fieldType == typeof(string))
                        handlerType = typeof(TimeOnlyAsStringTypeHandler);
                    else handlerType = typeof(TimeOnlyTypeHandler);
                }
                else
                {
                    switch (Type.GetTypeCode(underlyingType))
                    {
                        case TypeCode.Boolean:
                            handlerType = typeof(BooleanAsIntTypeHandler);
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
                                handlerType = typeof(NumberTypeHandler);
                            else handlerType = typeof(ConvertNumberTypeHandler);
                            break;
                        case TypeCode.Char:
                            handlerType = typeof(CharTypeHandler);
                            break;
                        case TypeCode.String:
                            if (isRequired) handlerType = typeof(StringTypeHandler);
                            else handlerType = typeof(NullableStringTypeHandler);
                            break;
                        case TypeCode.DateTime:
                            if (fieldType == typeof(string))
                                handlerType = typeof(DateTimeAsStringTypeHandler);
                            else handlerType = typeof(DateTimeTypeHandler);
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
            }
            if (this.typeHandlers.TryGetValue(handlerType, out var typeHandler))
                return typeHandler;
            return null;
        });
    }
    public override string GetQuotedValue(Type expectType, object value)
    {
        if (value == null) return "NULL";
        if (this.defaultTypeHandlers.TryGetValue(expectType, out var typeHandler))
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
    public override bool TryGetMyMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter)
    {
        var methodInfo = methodCallExpr.Method;
        var parameterInfos = methodInfo.GetParameters();
        switch (methodInfo.Name)
        {
            case "Values":
                var genericArgumentTypes = methodInfo.DeclaringType.GetGenericArguments();
                if (genericArgumentTypes.Length == 1 && methodInfo.DeclaringType == typeof(IMySqlCreateDuplicateKeyUpdate<>).MakeGenericType(genericArgumentTypes[0]))
                {
                    var genericArgumentType = methodInfo.GetGenericArguments()[0];
                    var cacheKey = HashCode.Combine(typeof(IMySqlCreateDuplicateKeyUpdate<>), methodInfo);
                    //.Set(f => new { TotalAmount = f.TotalAmount + x.Values(f.TotalAmount) })
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var myVisitor = visitor as MySqlCreateVisitor;
                        if (args[0] is not MemberExpression memberExpr)
                            throw new NotSupportedException($"不支持的表达式访问，类型{methodInfo.DeclaringType.FullName}.Values方法，只支持MemberAccess访问，如：.Set(f =&gt; new {{TotalAmount = x.Values(f.TotalAmount)}})");
                        if (!myVisitor.Tables[0].Mapper.TryGetMemberMap(memberExpr.Member.Name, out var memberMapper))
                            throw new MissingMemberException($"类{myVisitor.Tables[0].EntityType.FullName}未找到成员{memberExpr.Member.Name}");

                        //使用别名，一定要先使用，后使用的话，存在表达式计算场景无法解析，如：.Set(f => new { TotalAmount = f.TotalAmount + x.Values(f.TotalAmount) })
                        var fieldName = this.GetFieldName(memberMapper.FieldName);
                        if (myVisitor.IsSetAlias) fieldName = myVisitor.SetRowAlias + "." + fieldName;
                        else fieldName = $"VALUES({fieldName})";
                        return new SqlSegment
                        {
                            MemberMapper = memberMapper,
                            FromMember = memberMapper.Member,
                            HasField = true,
                            Value = fieldName
                        };
                    });
                    return true;
                }
                break;
        }
        formatter = null;
        return false;
    }
}
