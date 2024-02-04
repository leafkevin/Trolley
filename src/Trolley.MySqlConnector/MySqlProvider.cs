using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Trolley.MySqlConnector;

public partial class MySqlProvider : BaseOrmProvider
{
    private static Dictionary<object, Type> defaultMapTypes = new();
    private static Dictionary<Type, object> defaultDbTypes = new();
    private static Dictionary<Type, string> castTos = new();
    private static Dictionary<Type, ITypeHandler> defaultTypeHandlers = new();

    public override OrmProviderType OrmProviderType => OrmProviderType.MySql;
    public override Type NativeDbTypeType => typeof(MySqlDbType);

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
        defaultDbTypes[typeof(DateTimeOffset)] = MySqlDbType.Timestamp;
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
        defaultDbTypes[typeof(DateTimeOffset?)] = MySqlDbType.Timestamp;
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

        defaultTypeHandlers[typeof(bool)] = typeHandlers[typeof(BooleanAsIntTypeHandler)];
        defaultTypeHandlers[typeof(bool?)] = typeHandlers[typeof(NullableBooleanAsIntTypeHandler)];
        defaultTypeHandlers[typeof(string)] = typeHandlers[typeof(NullableStringTypeHandler)];
        defaultTypeHandlers[typeof(DateTimeOffset)] = typeHandlers[typeof(DateTimeOffsetTypeHandler)];
        defaultTypeHandlers[typeof(DateTimeOffset?)] = typeHandlers[typeof(NullableDateTimeOffsetTypeHandler)];
        defaultTypeHandlers[typeof(byte)] = typeHandlers[typeof(NumberTypeHandler)];
        defaultTypeHandlers[typeof(byte?)] = typeHandlers[typeof(NullableNumberTypeHandler)];
        defaultTypeHandlers[typeof(sbyte)] = typeHandlers[typeof(NumberTypeHandler)];
        defaultTypeHandlers[typeof(sbyte?)] = typeHandlers[typeof(NullableNumberTypeHandler)];
        defaultTypeHandlers[typeof(short)] = typeHandlers[typeof(NumberTypeHandler)];
        defaultTypeHandlers[typeof(ushort?)] = typeHandlers[typeof(NullableNumberTypeHandler)];
        defaultTypeHandlers[typeof(char)] = typeHandlers[typeof(CharTypeHandler)];
        defaultTypeHandlers[typeof(char?)] = typeHandlers[typeof(NullableCharTypeHandler)];
        defaultTypeHandlers[typeof(int)] = typeHandlers[typeof(NumberTypeHandler)];
        defaultTypeHandlers[typeof(uint?)] = typeHandlers[typeof(NullableNumberTypeHandler)];
        defaultTypeHandlers[typeof(long)] = typeHandlers[typeof(NumberTypeHandler)];
        defaultTypeHandlers[typeof(ulong?)] = typeHandlers[typeof(NullableNumberTypeHandler)];
        defaultTypeHandlers[typeof(float)] = typeHandlers[typeof(NumberTypeHandler)];
        defaultTypeHandlers[typeof(float?)] = typeHandlers[typeof(NullableNumberTypeHandler)];
        defaultTypeHandlers[typeof(double)] = typeHandlers[typeof(NumberTypeHandler)];
        defaultTypeHandlers[typeof(double?)] = typeHandlers[typeof(NullableNumberTypeHandler)];
        defaultTypeHandlers[typeof(decimal)] = typeHandlers[typeof(NumberTypeHandler)];
        defaultTypeHandlers[typeof(decimal?)] = typeHandlers[typeof(NullableNumberTypeHandler)];
        defaultTypeHandlers[typeof(DateTime)] = typeHandlers[typeof(DateTimeTypeHandler)];
        defaultTypeHandlers[typeof(DateTime?)] = typeHandlers[typeof(NullableDateTimeTypeHandler)];
        defaultTypeHandlers[typeof(DateOnly)] = typeHandlers[typeof(DateOnlyTypeHandler)];
        defaultTypeHandlers[typeof(DateOnly?)] = typeHandlers[typeof(NullableDateOnlyTypeHandler)];
        defaultTypeHandlers[typeof(TimeSpan)] = typeHandlers[typeof(TimeSpanTypeHandler)];
        defaultTypeHandlers[typeof(TimeSpan?)] = typeHandlers[typeof(NullableTimeSpanTypeHandler)];
        defaultTypeHandlers[typeof(TimeOnly)] = typeHandlers[typeof(TimeOnlyTypeHandler)];
        defaultTypeHandlers[typeof(TimeOnly?)] = typeHandlers[typeof(NullableTimeOnlyTypeHandler)];
        defaultTypeHandlers[typeof(Guid)] = typeHandlers[typeof(GuidTypeHandler)];
        defaultTypeHandlers[typeof(Guid?)] = typeHandlers[typeof(NullableGuidTypeHandler)];
        defaultTypeHandlers[typeof(object)] = typeHandlers[typeof(JsonTypeHandler)];
    }
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
    public override bool TryGetDefaultTypeHandler(Type targetType, out ITypeHandler typeHandler)
        => defaultTypeHandlers.TryGetValue(targetType, out typeHandler);
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
