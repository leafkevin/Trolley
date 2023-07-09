using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;

namespace Trolley.MySqlConnector;

public partial class MySqlProvider : BaseOrmProvider
{
    private static TimeSpan MaxTimeSpan = TimeSpan.Parse("00:59:59").Add(TimeSpan.FromHours(838));
    private static TimeSpan MinTimeSpan = -TimeSpan.Parse("00:59:59").Add(TimeSpan.FromHours(838));
    private static Dictionary<object, Type> defaultMapTypes = new();
    private static Dictionary<Type, object> defaultDbTypes = new();
    private static Dictionary<Type, string> castTos = new();

    public override string SelectIdentitySql => ";SELECT LAST_INSERT_ID()";
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
        castTos[typeof(decimal?)] = "DECIMAL(36,18)";
        castTos[typeof(DateTime?)] = "DATETIME";
        castTos[typeof(DateOnly?)] = "DATE";
        castTos[typeof(TimeOnly?)] = "TIME";
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
    public override IQueryVisitor NewQueryVisitor(string dbKey, IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
        => new MySqlQueryVisitor(dbKey, this, mapProvider, isParameterized, tableAsStart, parameterPrefix);
    public override IUpdateVisitor NewUpdateVisitor(string dbKey, IEntityMapProvider mapProvider, Type entityType, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
        => new MySqlUpdateVisitor(dbKey, this, mapProvider, entityType, isParameterized, tableAsStart, parameterPrefix);

    public override string GetTableName(string entityName) => "`" + entityName + "`";
    public override string GetFieldName(string propertyName) => "`" + propertyName + "`";
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
    public override string GetQuotedValue(Type expectType, object value)
    {
        if (expectType == typeof(TimeSpan) && value is TimeSpan timeSpan)
        {
            //在SELECT的场景才会用到
            if (timeSpan > MaxTimeSpan)
                return "'34.22:59:59.0000000";
            if (timeSpan < MinTimeSpan)
                return "'-34.22:59:59.0000000";
            if (timeSpan > TimeSpan.FromDays(1) || timeSpan < -TimeSpan.FromDays(1))
                return $"'{(int)timeSpan.TotalDays}.{timeSpan.ToString("hh\\:mm\\:ss\\.fffffff")}'";
            return $"'{timeSpan.ToString("hh\\:mm\\:ss\\.fffffff")}'";
        }
        return base.GetQuotedValue(expectType, value);
    }
}
