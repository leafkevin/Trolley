using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley.PostgreSql;

public partial class PostgreSqlProvider : BaseOrmProvider
{
    private static readonly Regex NpgsqlBoxRegex = new Regex("\\((-?\\d+.?\\d*),(-?\\d+.?\\d*)\\),\\((-?\\d+.?\\d*),(-?\\d+.?\\d*)\\)");
    private static readonly Regex NpgsqlCircleRegex = new Regex("<\\((-?\\d+.?\\d*),(-?\\d+.?\\d*)\\),(\\d+.?\\d*)>");
    private static readonly Regex NpgsqlLineRegex = new Regex("\\{(-?\\d+.?\\d*),(-?\\d+.?\\d*),(-?\\d+.?\\d*)\\}");
    private static readonly Regex NpgsqlLSegRegex = new Regex("\\[\\((-?\\d+.?\\d*),(-?\\d+.?\\d*)\\),\\((-?\\d+.?\\d*),(-?\\d+.?\\d*)\\)\\]");
    private static readonly Regex NpgsqlPointRegex = new Regex("\\((-?\\d+.?\\d*),(-?\\d+.?\\d*)\\)");
    private static Dictionary<Type, Func<object, object>> selfTypeParsers = new();

    private readonly static Dictionary<object, Type> defaultMapTypes = new();
    private readonly static Dictionary<Type, object> defaultDbTypes = new();
    private readonly static Dictionary<Type, string> castTos = new();
    private readonly static List<Type> selfTypes = new() { typeof(NpgsqlInet), typeof(IPAddress),
        typeof(PhysicalAddress), typeof(NpgsqlPoint), typeof(NpgsqlLine), typeof(NpgsqlLSeg),
        typeof(NpgsqlBox), typeof(NpgsqlPath), typeof(NpgsqlPolygon), typeof(NpgsqlCircle) };

    public override OrmProviderType OrmProviderType => OrmProviderType.PostgreSql;
    public override Type NativeDbTypeType => typeof(NpgsqlDbType);
    public override string DefaultTableSchema => "public";
    static PostgreSqlProvider()
    {
        defaultMapTypes[NpgsqlDbType.Bit] = typeof(BitArray);
        defaultMapTypes[NpgsqlDbType.Boolean] = typeof(bool);
        defaultMapTypes[NpgsqlDbType.Smallint] = typeof(short);
        defaultMapTypes[NpgsqlDbType.Integer] = typeof(int);
        defaultMapTypes[NpgsqlDbType.Bigint] = typeof(long);
        defaultMapTypes[NpgsqlDbType.Real] = typeof(float);
        defaultMapTypes[NpgsqlDbType.Double] = typeof(double);
        defaultMapTypes[NpgsqlDbType.Money] = typeof(decimal);
        defaultMapTypes[NpgsqlDbType.Numeric] = typeof(decimal);
        defaultMapTypes[NpgsqlDbType.Char] = typeof(string);
        defaultMapTypes[NpgsqlDbType.Varchar] = typeof(string);
        defaultMapTypes[NpgsqlDbType.Text] = typeof(string);
        defaultMapTypes[NpgsqlDbType.Json] = typeof(string);
        defaultMapTypes[NpgsqlDbType.Jsonb] = typeof(string);
        defaultMapTypes[NpgsqlDbType.Xml] = typeof(string);
        defaultMapTypes[NpgsqlDbType.Timestamp] = typeof(DateTime);
        defaultMapTypes[NpgsqlDbType.TimestampTz] = typeof(DateTimeOffset);
#if NET6_0_OR_GREATER
        defaultMapTypes[NpgsqlDbType.Date] = typeof(DateOnly);
        defaultMapTypes[NpgsqlDbType.Time] = typeof(TimeOnly);
        defaultMapTypes[NpgsqlDbType.TimeTz] = typeof(TimeOnly);
#else
        defaultMapTypes[NpgsqlDbType.Date] = typeof(DateTime);
        defaultMapTypes[NpgsqlDbType.Time] = typeof(TimeSpan);
        defaultMapTypes[NpgsqlDbType.TimeTz] = typeof(TimeSpan);
#endif
        defaultMapTypes[NpgsqlDbType.Interval] = typeof(TimeSpan);
        defaultMapTypes[NpgsqlDbType.Bytea] = typeof(byte[]);
        defaultMapTypes[NpgsqlDbType.Varbit] = typeof(BitArray);
        defaultMapTypes[NpgsqlDbType.Uuid] = typeof(Guid);
        defaultMapTypes[NpgsqlDbType.Hstore] = typeof(Dictionary<string, string>);

        defaultMapTypes[NpgsqlDbType.Name] = typeof(string);
        defaultMapTypes[NpgsqlDbType.Citext] = typeof(string);

        defaultMapTypes[NpgsqlDbType.Oid] = typeof(uint);
        defaultMapTypes[NpgsqlDbType.Xid] = typeof(uint);
        defaultMapTypes[NpgsqlDbType.Cid] = typeof(uint);
        defaultMapTypes[NpgsqlDbType.Oidvector] = typeof(uint[]);

        defaultMapTypes[NpgsqlDbType.Cidr] = typeof(NpgsqlInet);
        defaultMapTypes[NpgsqlDbType.Inet] = typeof(IPAddress);
        defaultMapTypes[NpgsqlDbType.MacAddr] = typeof(PhysicalAddress);

        defaultMapTypes[NpgsqlDbType.Point] = typeof(NpgsqlPoint);
        defaultMapTypes[NpgsqlDbType.Line] = typeof(NpgsqlLine);
        defaultMapTypes[NpgsqlDbType.LSeg] = typeof(NpgsqlLSeg);
        defaultMapTypes[NpgsqlDbType.Box] = typeof(NpgsqlBox);
        defaultMapTypes[NpgsqlDbType.Path] = typeof(NpgsqlPath);
        defaultMapTypes[NpgsqlDbType.Polygon] = typeof(NpgsqlPolygon);
        defaultMapTypes[NpgsqlDbType.Circle] = typeof(NpgsqlCircle);

        defaultMapTypes[NpgsqlDbType.Bit | NpgsqlDbType.Array] = typeof(BitArray[]);
        defaultMapTypes[NpgsqlDbType.Boolean | NpgsqlDbType.Array] = typeof(bool[]);
        defaultMapTypes[NpgsqlDbType.Smallint | NpgsqlDbType.Array] = typeof(short[]);
        defaultMapTypes[NpgsqlDbType.Integer | NpgsqlDbType.Array] = typeof(int[]);
        defaultMapTypes[NpgsqlDbType.Bigint | NpgsqlDbType.Array] = typeof(long[]);
        defaultMapTypes[NpgsqlDbType.Real | NpgsqlDbType.Array] = typeof(float[]);
        defaultMapTypes[NpgsqlDbType.Double | NpgsqlDbType.Array] = typeof(double[]);
        defaultMapTypes[NpgsqlDbType.Money | NpgsqlDbType.Array] = typeof(decimal[]);
        defaultMapTypes[NpgsqlDbType.Numeric | NpgsqlDbType.Array] = typeof(decimal[]);
        defaultMapTypes[NpgsqlDbType.Varchar | NpgsqlDbType.Array] = typeof(string[]);
        defaultMapTypes[NpgsqlDbType.Text | NpgsqlDbType.Array] = typeof(string[]);

        defaultMapTypes[NpgsqlDbType.Timestamp | NpgsqlDbType.Array] = typeof(DateTime[]);
        defaultMapTypes[NpgsqlDbType.TimestampTz | NpgsqlDbType.Array] = typeof(DateTimeOffset[]);
        defaultMapTypes[NpgsqlDbType.Interval | NpgsqlDbType.Array] = typeof(TimeSpan[]);
#if NET6_0_OR_GREATER
        defaultMapTypes[NpgsqlDbType.Date | NpgsqlDbType.Array] = typeof(DateOnly[]);
        defaultMapTypes[NpgsqlDbType.Time | NpgsqlDbType.Array] = typeof(TimeOnly[]);
        defaultMapTypes[NpgsqlDbType.TimeTz | NpgsqlDbType.Array] = typeof(TimeOnly[]);
#else
        defaultMapTypes[NpgsqlDbType.Date | NpgsqlDbType.Array] = typeof(DateTime[]);
        defaultMapTypes[NpgsqlDbType.Time | NpgsqlDbType.Array] = typeof(TimeSpan[]);
        defaultMapTypes[NpgsqlDbType.TimeTz | NpgsqlDbType.Array] = typeof(TimeSpan[]);
#endif
        defaultMapTypes[NpgsqlDbType.Bytea | NpgsqlDbType.Array] = typeof(byte[][]);
        defaultMapTypes[NpgsqlDbType.Varbit | NpgsqlDbType.Array] = typeof(BitArray[]);
        defaultMapTypes[NpgsqlDbType.Uuid | NpgsqlDbType.Array] = typeof(Guid[]);
        defaultMapTypes[NpgsqlDbType.Hstore | NpgsqlDbType.Array] = typeof(Dictionary<string, string>[]);

        defaultMapTypes[NpgsqlDbType.Cidr | NpgsqlDbType.Array] = typeof(NpgsqlInet[]);
        defaultMapTypes[NpgsqlDbType.Inet | NpgsqlDbType.Array] = typeof(IPAddress[]);
        defaultMapTypes[NpgsqlDbType.MacAddr | NpgsqlDbType.Array] = typeof(PhysicalAddress[]);

        defaultMapTypes[NpgsqlDbType.Point | NpgsqlDbType.Array] = typeof(NpgsqlPoint[]);
        defaultMapTypes[NpgsqlDbType.Line | NpgsqlDbType.Array] = typeof(NpgsqlLine[]);
        defaultMapTypes[NpgsqlDbType.LSeg | NpgsqlDbType.Array] = typeof(NpgsqlLSeg[]);
        defaultMapTypes[NpgsqlDbType.Box | NpgsqlDbType.Array] = typeof(NpgsqlBox[]);
        defaultMapTypes[NpgsqlDbType.Path | NpgsqlDbType.Array] = typeof(NpgsqlPath[]);
        defaultMapTypes[NpgsqlDbType.Polygon | NpgsqlDbType.Array] = typeof(NpgsqlPolygon[]);
        defaultMapTypes[NpgsqlDbType.Circle | NpgsqlDbType.Array] = typeof(NpgsqlCircle[]);

        defaultMapTypes[NpgsqlDbType.Integer | NpgsqlDbType.Range] = typeof(NpgsqlRange<int>);
        defaultMapTypes[NpgsqlDbType.Bigint | NpgsqlDbType.Range] = typeof(NpgsqlRange<long>);
        defaultMapTypes[NpgsqlDbType.Numeric | NpgsqlDbType.Range] = typeof(NpgsqlRange<decimal>);
        defaultMapTypes[NpgsqlDbType.Date | NpgsqlDbType.Range] = typeof(NpgsqlRange<DateTime>);
        defaultMapTypes[NpgsqlDbType.Timestamp | NpgsqlDbType.Range] = typeof(NpgsqlRange<DateTime>);
        defaultMapTypes[NpgsqlDbType.TimestampTz | NpgsqlDbType.Range] = typeof(NpgsqlRange<DateTime>);

        defaultMapTypes[NpgsqlDbType.Integer | NpgsqlDbType.Range | NpgsqlDbType.Array] = typeof(NpgsqlRange<int>[]);
        defaultMapTypes[NpgsqlDbType.Bigint | NpgsqlDbType.Range | NpgsqlDbType.Array] = typeof(NpgsqlRange<long>[]);
        defaultMapTypes[NpgsqlDbType.Numeric | NpgsqlDbType.Range | NpgsqlDbType.Array] = typeof(NpgsqlRange<decimal>[]);
        defaultMapTypes[NpgsqlDbType.Date | NpgsqlDbType.Range | NpgsqlDbType.Array] = typeof(NpgsqlRange<DateTime>[]);
        defaultMapTypes[NpgsqlDbType.Timestamp | NpgsqlDbType.Range | NpgsqlDbType.Array] = typeof(NpgsqlRange<DateTime>[]);
        defaultMapTypes[NpgsqlDbType.TimestampTz | NpgsqlDbType.Range | NpgsqlDbType.Array] = typeof(NpgsqlRange<DateTime>[]);


        defaultDbTypes[typeof(bool)] = NpgsqlDbType.Boolean;
        defaultDbTypes[typeof(sbyte)] = NpgsqlDbType.Smallint;
        defaultDbTypes[typeof(byte)] = NpgsqlDbType.Smallint;
        defaultDbTypes[typeof(short)] = NpgsqlDbType.Smallint;
        defaultDbTypes[typeof(ushort)] = NpgsqlDbType.Integer;
        defaultDbTypes[typeof(int)] = NpgsqlDbType.Integer;
        defaultDbTypes[typeof(uint)] = NpgsqlDbType.Bigint;
        defaultDbTypes[typeof(long)] = NpgsqlDbType.Bigint;
        defaultDbTypes[typeof(ulong)] = NpgsqlDbType.Numeric;
        defaultDbTypes[typeof(float)] = NpgsqlDbType.Real;
        defaultDbTypes[typeof(double)] = NpgsqlDbType.Double;
        defaultDbTypes[typeof(decimal)] = NpgsqlDbType.Numeric;
        defaultDbTypes[typeof(string)] = NpgsqlDbType.Varchar;
        defaultDbTypes[typeof(DateTime)] = NpgsqlDbType.Timestamp;
        defaultDbTypes[typeof(TimeSpan)] = NpgsqlDbType.Interval;
        defaultDbTypes[typeof(DateTimeOffset)] = NpgsqlDbType.TimestampTz;
#if NET6_0_OR_GREATER
        defaultDbTypes[typeof(DateOnly)] = NpgsqlDbType.Date;
        defaultDbTypes[typeof(TimeOnly)] = NpgsqlDbType.Time;
#endif        
        defaultDbTypes[typeof(Guid)] = NpgsqlDbType.Uuid;
        defaultDbTypes[typeof(byte[])] = NpgsqlDbType.Bytea;

        defaultDbTypes[typeof(bool?)] = NpgsqlDbType.Boolean;
        defaultDbTypes[typeof(sbyte?)] = NpgsqlDbType.Smallint;
        defaultDbTypes[typeof(byte?)] = NpgsqlDbType.Smallint;
        defaultDbTypes[typeof(short?)] = NpgsqlDbType.Smallint;
        defaultDbTypes[typeof(ushort?)] = NpgsqlDbType.Integer;
        defaultDbTypes[typeof(int?)] = NpgsqlDbType.Integer;
        defaultDbTypes[typeof(uint?)] = NpgsqlDbType.Bigint;
        defaultDbTypes[typeof(long?)] = NpgsqlDbType.Bigint;
        defaultDbTypes[typeof(ulong?)] = NpgsqlDbType.Numeric;

        defaultDbTypes[typeof(float?)] = NpgsqlDbType.Real;
        defaultDbTypes[typeof(double?)] = NpgsqlDbType.Double;
        defaultDbTypes[typeof(decimal?)] = NpgsqlDbType.Numeric;
        defaultDbTypes[typeof(DateTime?)] = NpgsqlDbType.Timestamp;
        defaultDbTypes[typeof(TimeSpan?)] = NpgsqlDbType.Interval;
        defaultDbTypes[typeof(DateTimeOffset?)] = NpgsqlDbType.TimestampTz;
#if NET6_0_OR_GREATER
        defaultDbTypes[typeof(DateOnly?)] = NpgsqlDbType.Date;
        defaultDbTypes[typeof(TimeOnly?)] = NpgsqlDbType.Time;
#endif
        defaultDbTypes[typeof(Guid?)] = NpgsqlDbType.Uuid;
        defaultDbTypes[typeof(byte[])] = NpgsqlDbType.Bytea;

        defaultDbTypes[typeof(bool[])] = NpgsqlDbType.Boolean | NpgsqlDbType.Array;
        defaultDbTypes[typeof(short[])] = NpgsqlDbType.Smallint | NpgsqlDbType.Array;
        defaultDbTypes[typeof(int[])] = NpgsqlDbType.Integer | NpgsqlDbType.Array;
        defaultDbTypes[typeof(long[])] = NpgsqlDbType.Bigint | NpgsqlDbType.Array;
        defaultDbTypes[typeof(float[])] = NpgsqlDbType.Real | NpgsqlDbType.Array;
        defaultDbTypes[typeof(double[])] = NpgsqlDbType.Double | NpgsqlDbType.Array;
        defaultDbTypes[typeof(decimal[])] = NpgsqlDbType.Numeric | NpgsqlDbType.Array;
#if NET6_0_OR_GREATER
        defaultDbTypes[typeof(DateOnly[])] = NpgsqlDbType.Date | NpgsqlDbType.Array;
        defaultDbTypes[typeof(TimeOnly[])] = NpgsqlDbType.Time | NpgsqlDbType.Array;
#endif
        defaultDbTypes[typeof(DateTime[])] = NpgsqlDbType.Timestamp | NpgsqlDbType.Array;
        defaultDbTypes[typeof(TimeSpan[])] = NpgsqlDbType.Interval | NpgsqlDbType.Array;
        defaultDbTypes[typeof(string[])] = NpgsqlDbType.Varchar | NpgsqlDbType.Array;
        defaultDbTypes[typeof(DateTimeOffset[])] = NpgsqlDbType.TimestampTz | NpgsqlDbType.Array;
        defaultDbTypes[typeof(Guid[])] = NpgsqlDbType.Uuid | NpgsqlDbType.Array;
        defaultDbTypes[typeof(BitArray[])] = NpgsqlDbType.Varbit | NpgsqlDbType.Array;
        defaultDbTypes[typeof(Dictionary<string, string>[])] = NpgsqlDbType.Hstore | NpgsqlDbType.Array;
        defaultDbTypes[typeof(byte[][])] = NpgsqlDbType.Bytea | NpgsqlDbType.Array;

        defaultDbTypes[typeof(NpgsqlInet[])] = NpgsqlDbType.Cidr | NpgsqlDbType.Array;
        defaultDbTypes[typeof(IPAddress[])] = NpgsqlDbType.Inet | NpgsqlDbType.Array;
        defaultDbTypes[typeof(PhysicalAddress[])] = NpgsqlDbType.MacAddr | NpgsqlDbType.Array;
        defaultDbTypes[typeof(NpgsqlPoint[])] = NpgsqlDbType.Point | NpgsqlDbType.Array;
        defaultDbTypes[typeof(NpgsqlLine[])] = NpgsqlDbType.Line | NpgsqlDbType.Array;
        defaultDbTypes[typeof(NpgsqlLSeg[])] = NpgsqlDbType.LSeg | NpgsqlDbType.Array;
        defaultDbTypes[typeof(NpgsqlBox[])] = NpgsqlDbType.Box | NpgsqlDbType.Array;
        defaultDbTypes[typeof(NpgsqlPath[])] = NpgsqlDbType.Path | NpgsqlDbType.Array;
        defaultDbTypes[typeof(NpgsqlPolygon[])] = NpgsqlDbType.Polygon | NpgsqlDbType.Array;
        defaultDbTypes[typeof(NpgsqlCircle[])] = NpgsqlDbType.Circle | NpgsqlDbType.Array;


        defaultDbTypes[typeof(NpgsqlRange<int>)] = NpgsqlDbType.Integer | NpgsqlDbType.Range;
        defaultDbTypes[typeof(NpgsqlRange<long>)] = NpgsqlDbType.Bigint | NpgsqlDbType.Range;
        defaultDbTypes[typeof(NpgsqlRange<decimal>)] = NpgsqlDbType.Numeric | NpgsqlDbType.Range;
        defaultDbTypes[typeof(NpgsqlRange<DateTime>)] = NpgsqlDbType.Timestamp | NpgsqlDbType.Range;

        defaultDbTypes[typeof(NpgsqlRange<int>[])] = NpgsqlDbType.Integer | NpgsqlDbType.Range | NpgsqlDbType.Array; ;
        defaultDbTypes[typeof(NpgsqlRange<long>[])] = NpgsqlDbType.Bigint | NpgsqlDbType.Range | NpgsqlDbType.Array;
        defaultDbTypes[typeof(NpgsqlRange<decimal>[])] = NpgsqlDbType.Numeric | NpgsqlDbType.Range | NpgsqlDbType.Array;
        defaultDbTypes[typeof(NpgsqlRange<DateTime>[])] = NpgsqlDbType.Timestamp | NpgsqlDbType.Range | NpgsqlDbType.Array;


        castTos[typeof(string)] = "VARCHAR";
        castTos[typeof(sbyte)] = "SMALLINT";
        castTos[typeof(byte)] = "SMALLINT";
        castTos[typeof(short)] = "SMALLINT";
        castTos[typeof(ushort)] = "INTEGER";
        castTos[typeof(int)] = "INTEGER";
        castTos[typeof(uint)] = "BIGINT";
        castTos[typeof(long)] = "BIGINT";
        castTos[typeof(ulong)] = "DECIMAL";
        castTos[typeof(float)] = "REAL";
        castTos[typeof(double)] = "FLOAT";
        castTos[typeof(decimal)] = "DECIMAL";
        castTos[typeof(bool)] = "BOOLEAN";
        castTos[typeof(DateTime)] = "TIMESTAMP";
        castTos[typeof(TimeSpan)] = "INTERVAL";
#if NET6_0_OR_GREATER
        castTos[typeof(DateOnly)] = "DATE";
        castTos[typeof(TimeOnly)] = "TIME";
#endif
        castTos[typeof(sbyte?)] = "SMALLINT";
        castTos[typeof(byte?)] = "SMALLINT";
        castTos[typeof(short?)] = "SMALLINT";
        castTos[typeof(ushort?)] = "INTEGER";
        castTos[typeof(int?)] = "INTEGER";
        castTos[typeof(uint?)] = "INTEGER";
        castTos[typeof(long?)] = "BIGINT";
        castTos[typeof(ulong?)] = "DECIMAL";
        castTos[typeof(float?)] = "REAL";
        castTos[typeof(double?)] = "FLOAT";
        castTos[typeof(decimal?)] = "DECIMAL";
        castTos[typeof(bool?)] = "BOOLEAN";
        castTos[typeof(DateTime?)] = "TIMESTAMP";
        castTos[typeof(TimeSpan?)] = "INTERVAL";
#if NET6_0_OR_GREATER
        castTos[typeof(DateOnly?)] = "DATE";
        castTos[typeof(TimeOnly?)] = "TIME";
#endif
        selfTypeParsers.Add(typeof(NpgsqlInet), GetSelfTypeParserFunc(typeof(NpgsqlInet)));
        selfTypeParsers.Add(typeof(NpgsqlPoint), GetSelfTypeParserFunc(typeof(NpgsqlPoint)));
        selfTypeParsers.Add(typeof(NpgsqlLine), GetSelfTypeParserFunc(typeof(NpgsqlLine)));
        selfTypeParsers.Add(typeof(NpgsqlLSeg), GetSelfTypeParserFunc(typeof(NpgsqlLSeg)));
        selfTypeParsers.Add(typeof(NpgsqlBox), GetSelfTypeParserFunc(typeof(NpgsqlBox)));
        selfTypeParsers.Add(typeof(NpgsqlPath), GetSelfTypeParserFunc(typeof(NpgsqlPath)));
        selfTypeParsers.Add(typeof(NpgsqlPolygon), GetSelfTypeParserFunc(typeof(NpgsqlPolygon)));
        selfTypeParsers.Add(typeof(NpgsqlCircle), GetSelfTypeParserFunc(typeof(NpgsqlCircle)));
        selfTypeParsers.Add(typeof(IPAddress), GetSelfTypeParserFunc(typeof(IPAddress)));
        selfTypeParsers.Add(typeof(PhysicalAddress), GetSelfTypeParserFunc(typeof(PhysicalAddress)));

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    public override ITheaConnection CreateConnection(string dbKey, string connectionString)
        => new PostgreSqlTheaConnection(dbKey, connectionString);
    public override IDbCommand CreateCommand() => new NpgsqlCommand();
    public override IDbDataParameter CreateParameter(string parameterName, object value)
        => new NpgsqlParameter(parameterName, value);
    public override IDbDataParameter CreateParameter(string parameterName, object nativeDbType, object value)
        => new NpgsqlParameter(parameterName, (NpgsqlDbType)nativeDbType) { Value = value };
    public override void ChangeParameter(object dbParameter, Type targetType, object value)
    {
        var fieldValue = Convert.ChangeType(value, targetType);
        var myDbParameter = dbParameter as NpgsqlParameter;
        var nativeDbType = (NpgsqlDbType)this.GetNativeDbType(targetType);
        myDbParameter.NpgsqlDbType = nativeDbType;
        myDbParameter.Value = fieldValue;
    }
    public override string GetTableName(string tableName)
    {
        if (string.IsNullOrEmpty(tableName))
            throw new ArgumentNullException(nameof(tableName));
        if (tableName.Contains('.'))
        {
            var tableNames = tableName.Split('.');
            if (tableNames[0] == this.DefaultTableSchema)
                return "\"" + tableNames[1] + "\"";
            return $"\"{tableNames[0]}\".\"{tableNames[1]}\"";
        }
        return "\"" + tableName + "\"";
    }
    public override string GetFieldName(string fieldName) => "\"" + fieldName + "\"";
    public override object GetNativeDbType(Type fieldType)
    {
        if (!defaultDbTypes.TryGetValue(fieldType, out var dbType))
            throw new Exception($"类型{fieldType.FullName}没有对应的NpgsqlTypes.NpgsqlDbType映射类型");
        return dbType;
    }
    public override Type MapDefaultType(object nativeDbType)
    {
        if (nativeDbType == null)
            throw new ArgumentNullException(nameof(nativeDbType));

        if (defaultMapTypes.TryGetValue(nativeDbType, out var result))
            return result;

        if (nativeDbType is NpgsqlDbType dbType)
        {
            var elementDbType = dbType & ~NpgsqlDbType.Array;
            if (defaultMapTypes.TryGetValue(elementDbType, out var elementType))
                return elementType.MakeArrayType();
        }
        if (nativeDbType is int iDbType)
        {
            var elementDbType = (NpgsqlDbType)(iDbType & ~(int)NpgsqlDbType.Array);
            if (defaultMapTypes.TryGetValue(elementDbType, out result))
                return result.MakeArrayType();
        }
        return typeof(object);
    }
    public override Type MapDefaultType(MemberMap memberMappper)
    {
        if (memberMappper.NativeDbType is NpgsqlDbType nativeDbType && nativeDbType == NpgsqlDbType.Bit)
        {
            if (memberMappper.MaxLength > 1)
                return typeof(BitArray);
            else return typeof(bool);
        }
        return this.MapDefaultType(memberMappper.NativeDbType);
    }
    public override string CastTo(Type type, object value, string characterSetOrCollation = null)
        => $"CAST({value} AS {castTos[type]})";
    public override string GetIdentitySql(string keyField) => $" RETURNING {keyField}";
    public override string GetQuotedValue(Type expectType, object value)
    {
        if (value == null) return "NULL";
        switch (expectType)
        {
            case Type factType when factType == typeof(bool):
                return Convert.ToBoolean(value) ? "TRUE" : "FALSE";
            case Type factType when factType == typeof(string):
                return $"'{Convert.ToString(value).Replace("'", @"\'")}'";
            case Type factType when factType == typeof(Guid):
                return $"'{value}'::UUID";
            case Type factType when factType == typeof(DateTime):
                return $"TIMESTAMP '{Convert.ToDateTime(value):yyyy\\-MM\\-dd\\ HH\\:mm\\:ss\\.fff}'";
            case Type factType when factType == typeof(DateTimeOffset):
                return $"TIMESTAMPTZ '{(DateTimeOffset)value:yyyy\\-MM\\-dd\\ HH\\:mm\\:ss\\.fffZ}'";
#if NET6_0_OR_GREATER
            case Type factType when factType == typeof(DateOnly):
                return $"DATE '{(DateOnly)value:yyyy\\-MM\\-dd}'";
#endif
            case Type factType when factType == typeof(TimeSpan):
                {
                    var factValue = (TimeSpan)value;
                    if (factValue.TotalDays > 1 || factValue.TotalDays < -1)
                        return $"INTERVAL '{(int)factValue.TotalDays}D {factValue:hh\\:mm\\:ss\\.ffffff}'";
                    return $"INTERVAL '{factValue:hh\\:mm\\:ss\\.ffffff}'";
                }
#if NET6_0_OR_GREATER
            case Type factType when factType == typeof(TimeOnly): return $"TIME '{(TimeOnly)value:hh\\:mm\\:ss\\.ffffff}'";
#endif
            case Type factType when factType == typeof(BitArray):
                {
                    var bitArray = value as BitArray;
                    if (bitArray.Length > 0)
                    {
                        var builder = new StringBuilder("'");
                        int index = 0;
                        while (index < bitArray.Length)
                        {
                            var bitValue = bitArray.Get(index);
                            builder.Append(bitValue ? "1" : "0");
                        }
                        builder.Append("'::bit");
                        return builder.ToString();
                    }
                    return null;
                }
            case Type factType when selfTypes.Contains(factType):
                return $"'{value}'";
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
    public override Func<object, object> GetParameterValueGetter(Type fromType, Type fieldType, bool isNullable, OrmDbFactoryOptions options)
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
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return null;
                                    return RepositoryHelper.ToArray(elelmentTypes[0], value);
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
                    else if (selfTypes.Contains(fieldType))
                    {
                        if (underlyingType == typeof(string))
                        {
                            var parser = selfTypeParsers[fieldType];
                            if (isNullableType || isNullable)
                            {
                                typeHandler = value =>
                                {
                                    if (value == null) return DBNull.Value;
                                    return parser(value);
                                };
                            }
                            else typeHandler = value => parser(value);
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
    public override Func<object, object> GetReaderValueGetter(Type targetType, Type fieldType, OrmDbFactoryOptions options)
    {
        var hashKey = RepositoryHelper.GetCacheKey(targetType, fieldType, options.DefaultDateTimeKind);
        return readerValueGetters.GetOrAdd(hashKey, f =>
        {
            var underlyingType = Nullable.GetUnderlyingType(targetType);
            var isNullableType = underlyingType != null || targetType.IsClass;
            underlyingType ??= targetType;
            Func<object, object> typeHandler = null;
            if (targetType == fieldType || underlyingType == fieldType)
            {
                var valueExpr = Expression.Parameter(typeof(object), "value");
                var blockBodies = new List<Expression>();
                var resultExpr = Expression.Variable(typeof(object), "result");
                var isDbNullExpr = Expression.TypeIs(valueExpr, typeof(DBNull));
                var setDefaultExpr = Expression.Assign(resultExpr, Expression.Convert(Expression.Default(targetType), typeof(object)));

                Expression typedValueExpr = null;
                if (underlyingType == typeof(DateTime) || underlyingType == typeof(DateTimeOffset))
                {
                    MethodInfo methodInfo;
                    if (options.DefaultDateTimeKind == DateTimeKind.Utc)
                    {
                        typedValueExpr = Expression.Convert(valueExpr, underlyingType);
                        methodInfo = typeof(RepositoryHelper).GetMethod(nameof(RepositoryHelper.ToUtcTime), BindingFlags.Public | BindingFlags.Static, null, [underlyingType], null);
                        typedValueExpr = Expression.Call(methodInfo, typedValueExpr);
                        typedValueExpr = Expression.Convert(typedValueExpr, typeof(object));
                    }
                    else if (options.DefaultDateTimeKind == DateTimeKind.Local)
                    {
                        typedValueExpr = Expression.Convert(valueExpr, underlyingType);
                        methodInfo = typeof(RepositoryHelper).GetMethod(nameof(RepositoryHelper.ToLocalTime), BindingFlags.Public | BindingFlags.Static, null, [underlyingType], null);
                        typedValueExpr = Expression.Call(methodInfo, typedValueExpr);
                        typedValueExpr = Expression.Convert(typedValueExpr, typeof(object));
                    }
                    else typedValueExpr = valueExpr;
                }
                else typedValueExpr = valueExpr;
                var setTypedValueExpr = Expression.Assign(resultExpr, typedValueExpr);
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
                                || underlyingType.IsInterface)
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return null;
                                    return RepositoryHelper.CreateListInstance(elelmentTypes[0], value);
                                };
                            }
                            else if (underlyingType == typeof(Collection<>).MakeGenericType(elelmentTypes))
                            {
                                typeHandler = value =>
                                {
                                    if (value is DBNull) return null;
                                    return RepositoryHelper.CreateCollectionInstance(elelmentTypes[0], value);
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
                                if (options.DefaultDateTimeKind == DateTimeKind.Utc)
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return null;
                                        return RepositoryHelper.ToUtcTime(DateTimeOffset.Parse((string)value));
                                    };
                                }
                                else if (options.DefaultDateTimeKind == DateTimeKind.Local)
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return null;
                                        return RepositoryHelper.ToLocalTime(DateTimeOffset.Parse((string)value));
                                    };
                                }
                                else
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return null;
                                        return DateTimeOffset.Parse((string)value);
                                    };
                                }
                            }
                            else
                            {
                                if (options.DefaultDateTimeKind == DateTimeKind.Utc)
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return DateTimeOffset.MinValue;
                                        return RepositoryHelper.ToUtcTime(DateTimeOffset.Parse((string)value));
                                    };
                                }
                                else if (options.DefaultDateTimeKind == DateTimeKind.Local)
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return DateTimeOffset.MinValue;
                                        return RepositoryHelper.ToLocalTime(DateTimeOffset.Parse((string)value));
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
                        }
                        else if (fieldType == typeof(DateTime))
                        {
                            if (isNullableType)
                            {
                                if (options.DefaultDateTimeKind == DateTimeKind.Utc)
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return null;
                                        return RepositoryHelper.ToUtcTime(new DateTimeOffset(((DateTime)value)));
                                    };
                                }
                                else if (options.DefaultDateTimeKind == DateTimeKind.Local)
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return null;
                                        return RepositoryHelper.ToLocalTime(new DateTimeOffset((DateTime)value));
                                    };
                                }
                                else
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return null;
                                        return new DateTimeOffset((DateTime)value);
                                    };
                                }
                            }
                            else
                            {
                                if (options.DefaultDateTimeKind == DateTimeKind.Utc)
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return DateTimeOffset.MinValue;
                                        return RepositoryHelper.ToUtcTime(new DateTimeOffset(((DateTime)value)));
                                    };
                                }
                                else if (options.DefaultDateTimeKind == DateTimeKind.Local)
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return DateTimeOffset.MinValue;
                                        return new DateTimeOffset(RepositoryHelper.ToLocalTime((DateTime)value));
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
                    }
                    else if (underlyingType == typeof(DateTime))
                    {
                        if (fieldType == typeof(string))
                        {
                            if (isNullableType)
                            {
                                if (options.DefaultDateTimeKind == DateTimeKind.Utc)
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return null;
                                        return RepositoryHelper.ToUtcTime(DateTime.Parse((string)value));
                                    };
                                }
                                else if (options.DefaultDateTimeKind == DateTimeKind.Local)
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return null;
                                        return RepositoryHelper.ToLocalTime(DateTime.Parse((string)value));
                                    };
                                }
                                else
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return null;
                                        return DateTime.Parse((string)value);
                                    };
                                }
                            }
                            else
                            {
                                if (options.DefaultDateTimeKind == DateTimeKind.Utc)
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return DateTime.MinValue;
                                        return RepositoryHelper.ToUtcTime(DateTime.Parse((string)value));
                                    };
                                }
                                else if (options.DefaultDateTimeKind == DateTimeKind.Utc)
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return DateTime.MinValue;
                                        return RepositoryHelper.ToLocalTime(DateTime.Parse((string)value));
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
                                if (options.DefaultDateTimeKind == DateTimeKind.Utc)
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return null;
                                        return RepositoryHelper.ToUtcTime((DateTimeOffset)value);
                                    };
                                }
                                else if (options.DefaultDateTimeKind == DateTimeKind.Local)
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return null;
                                        return RepositoryHelper.ToLocalTime((DateTimeOffset)value);
                                    };
                                }
                                else
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return null;
                                        return ((DateTimeOffset)value).DateTime;
                                    };
                                }
                            }
                            else
                            {
                                if (options.DefaultDateTimeKind == DateTimeKind.Utc)
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return DateTime.MinValue;
                                        return RepositoryHelper.ToUtcTime((DateTimeOffset)value);
                                    };
                                }
                                else if (options.DefaultDateTimeKind == DateTimeKind.Local)
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return DateTime.MinValue;
                                        return RepositoryHelper.ToLocalTime((DateTimeOffset)value);
                                    };
                                }
                                else
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return DateTime.MinValue;
                                        return ((DateTimeOffset)value).DateTime;
                                    };
                                }
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
                                if (options.DefaultDateTimeKind == DateTimeKind.Utc)
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return null;
                                        return DateOnly.FromDateTime(RepositoryHelper.ToUtcTime((DateTime)value));
                                    };
                                }
                                else if (options.DefaultDateTimeKind == DateTimeKind.Local)
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return null;
                                        return DateOnly.FromDateTime(RepositoryHelper.ToLocalTime((DateTime)value));
                                    };
                                }
                                else
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return null;
                                        return DateOnly.FromDateTime(((DateTime)value));
                                    };
                                }
                            }
                            else
                            {
                                if (options.DefaultDateTimeKind == DateTimeKind.Utc)
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return DateOnly.MinValue;
                                        return DateOnly.FromDateTime(RepositoryHelper.ToUtcTime((DateTime)value));
                                    };
                                }
                                else if (options.DefaultDateTimeKind == DateTimeKind.Local)
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return DateOnly.MinValue;
                                        return DateOnly.FromDateTime(RepositoryHelper.ToLocalTime((DateTime)value));
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
                        }
                        else if (fieldType == typeof(DateTimeOffset))
                        {
                            if (isNullableType)
                            {
                                if (options.DefaultDateTimeKind == DateTimeKind.Utc)
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return null;
                                        return DateOnly.FromDateTime(RepositoryHelper.ToUtcTime((DateTimeOffset)value).DateTime);
                                    };
                                }
                                else if (options.DefaultDateTimeKind == DateTimeKind.Local)
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return null;
                                        return DateOnly.FromDateTime(RepositoryHelper.ToLocalTime((DateTimeOffset)value).DateTime);
                                    };
                                }
                                else
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return null;
                                        return DateOnly.FromDateTime(((DateTimeOffset)value).DateTime);
                                    };
                                }
                            }
                            else
                            {
                                if (options.DefaultDateTimeKind == DateTimeKind.Utc)
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return DateOnly.MinValue;
                                        return DateOnly.FromDateTime(RepositoryHelper.ToUtcTime((DateTimeOffset)value).DateTime);
                                    };
                                }
                                else if (options.DefaultDateTimeKind == DateTimeKind.Local)
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return DateOnly.MinValue;
                                        return DateOnly.FromDateTime(RepositoryHelper.ToLocalTime((DateTimeOffset)value).DateTime);
                                    };
                                }
                                else
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return DateOnly.MinValue;
                                        return DateOnly.FromDateTime(((DateTimeOffset)value).DateTime);
                                    };
                                }
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
                                if (options.DefaultDateTimeKind == DateTimeKind.Utc)
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return null;
                                        return RepositoryHelper.ToUtcTime((DateTime)value).TimeOfDay;
                                    };
                                }
                                else if (options.DefaultDateTimeKind == DateTimeKind.Local)
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return null;
                                        return RepositoryHelper.ToLocalTime((DateTime)value).TimeOfDay;
                                    };
                                }
                                else
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return null;
                                        return ((DateTime)value).TimeOfDay;
                                    };
                                }
                            }
                            else
                            {
                                if (options.DefaultDateTimeKind == DateTimeKind.Utc)
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return TimeSpan.MinValue;
                                        return RepositoryHelper.ToUtcTime((DateTime)value).TimeOfDay;
                                    };
                                }
                                else if (options.DefaultDateTimeKind == DateTimeKind.Local)
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return TimeSpan.MinValue;
                                        return RepositoryHelper.ToLocalTime((DateTime)value).TimeOfDay;
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
                        }
                        else if (fieldType == typeof(DateTimeOffset))
                        {
                            if (isNullableType)
                            {
                                if (options.DefaultDateTimeKind == DateTimeKind.Utc)
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return null;
                                        return RepositoryHelper.ToUtcTime((DateTimeOffset)value).TimeOfDay;
                                    };
                                }
                                else if (options.DefaultDateTimeKind == DateTimeKind.Local)
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return null;
                                        return RepositoryHelper.ToLocalTime((DateTimeOffset)value).TimeOfDay;
                                    };
                                }
                                else
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return null;
                                        return ((DateTimeOffset)value).DateTime.TimeOfDay;
                                    };
                                }
                            }
                            else
                            {
                                if (options.DefaultDateTimeKind == DateTimeKind.Utc)
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return TimeSpan.MinValue;
                                        return RepositoryHelper.ToUtcTime((DateTimeOffset)value).TimeOfDay;
                                    };
                                }
                                else if (options.DefaultDateTimeKind == DateTimeKind.Local)
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return TimeSpan.MinValue;
                                        return RepositoryHelper.ToLocalTime((DateTimeOffset)value).TimeOfDay;
                                    };
                                }
                                else
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return TimeSpan.MinValue;
                                        return ((DateTimeOffset)value).DateTime.TimeOfDay;
                                    };
                                }
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
                                if (options.DefaultDateTimeKind == DateTimeKind.Utc)
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return null;
                                        return TimeOnly.FromTimeSpan(RepositoryHelper.ToUtcTime((DateTime)value).TimeOfDay);
                                    };
                                }
                                else if (options.DefaultDateTimeKind == DateTimeKind.Local)
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return null;
                                        return TimeOnly.FromTimeSpan(RepositoryHelper.ToLocalTime((DateTime)value).TimeOfDay);
                                    };
                                }
                                else
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return null;
                                        return TimeOnly.FromTimeSpan(((DateTime)value).TimeOfDay);
                                    };
                                }
                            }
                            else
                            {
                                if (options.DefaultDateTimeKind == DateTimeKind.Utc)
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return TimeOnly.MinValue;
                                        return TimeOnly.FromTimeSpan(RepositoryHelper.ToUtcTime((DateTime)value).TimeOfDay);
                                    };
                                }
                                else if (options.DefaultDateTimeKind == DateTimeKind.Local)
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return TimeOnly.MinValue;
                                        return TimeOnly.FromTimeSpan(RepositoryHelper.ToLocalTime((DateTime)value).TimeOfDay);
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
                        }
                        else if (fieldType == typeof(DateTimeOffset))
                        {
                            if (isNullableType)
                            {
                                if (options.DefaultDateTimeKind == DateTimeKind.Utc)
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return null;
                                        return TimeOnly.FromTimeSpan(RepositoryHelper.ToUtcTime((DateTimeOffset)value).TimeOfDay);
                                    };
                                }
                                else if (options.DefaultDateTimeKind == DateTimeKind.Local)
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return null;
                                        return TimeOnly.FromTimeSpan(RepositoryHelper.ToLocalTime((DateTimeOffset)value).TimeOfDay);
                                    };
                                }
                                else
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return null;
                                        return TimeOnly.FromTimeSpan(((DateTimeOffset)value).DateTime.TimeOfDay);
                                    };
                                }
                            }
                            else
                            {
                                if (options.DefaultDateTimeKind == DateTimeKind.Utc)
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return TimeOnly.MinValue;
                                        return TimeOnly.FromTimeSpan(RepositoryHelper.ToUtcTime((DateTimeOffset)value).TimeOfDay);
                                    };
                                }
                                else if (options.DefaultDateTimeKind == DateTimeKind.Local)
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return TimeOnly.MinValue;
                                        return TimeOnly.FromTimeSpan(RepositoryHelper.ToLocalTime((DateTimeOffset)value).TimeOfDay);
                                    };
                                }
                                else
                                {
                                    typeHandler = value =>
                                    {
                                        if (value is DBNull) return TimeOnly.MinValue;
                                        return TimeOnly.FromTimeSpan(((DateTimeOffset)value).DateTime.TimeOfDay);
                                    };
                                }
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
                        Type[] supportedTypes = [typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong)];
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
                        Type[] supportedTypes = [ typeof(bool), typeof(char), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double)
#if NET6_0_OR_GREATER
                            , typeof(Half)
#endif
                        ];
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
                        Type[] supportedTypes = [typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong)];
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
                    else if (selfTypes.Contains(underlyingType))
                    {
                        if (fieldType == typeof(string))
                        {
                            var parser = selfTypeParsers[underlyingType];
                            if (isNullableType)
                            {
                                typeHandler = value =>
                                {
                                    if (value == null) return DBNull.Value;
                                    return parser(value);
                                };
                            }
                            else typeHandler = value => parser(value);
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

    public override object MapNativeDbType(DbColumnInfo columnInfo)
    {
        var dataType = columnInfo.DataType;
        if (columnInfo.ArrayDimens > 0)
            dataType = dataType.Substring(1);
        NpgsqlDbType result = default;
        switch (dataType)
        {
            case "bool": result = NpgsqlDbType.Boolean; break;

            case "int2": result = NpgsqlDbType.Smallint; break;
            case "int4": result = NpgsqlDbType.Integer; break;
            case "int8": result = NpgsqlDbType.Bigint; break;
            case "float4": result = NpgsqlDbType.Real; break;
            case "float8": result = NpgsqlDbType.Double; break;

            case "oid": result = NpgsqlDbType.Oid; break;
            case "cid": result = NpgsqlDbType.Cid; break;
            case "xid": result = NpgsqlDbType.Xid; break;

            case "numeric": result = NpgsqlDbType.Numeric; break;
            case "money": result = NpgsqlDbType.Money; break;

            case "char":
            case "bpchar": result = NpgsqlDbType.Char; break;
            case "varchar": result = NpgsqlDbType.Varchar; break;
            case "text": result = NpgsqlDbType.Text; break;

            case "date": result = NpgsqlDbType.Date; break;
            case "timestamp": result = NpgsqlDbType.Timestamp; break;
            case "timestamptz": result = NpgsqlDbType.TimestampTz; break;

            case "time": result = NpgsqlDbType.Time; break;
            case "timetz": result = NpgsqlDbType.TimeTz; break;
            case "interval": result = NpgsqlDbType.Interval; break;

            case "bit": result = NpgsqlDbType.Bit; break;
            case "bytea": result = NpgsqlDbType.Bytea; break;
            case "varbit": result = NpgsqlDbType.Varbit; break;

            case "point": result = NpgsqlDbType.Point; break;
            case "line": result = NpgsqlDbType.Line; break;
            case "lseg": result = NpgsqlDbType.LSeg; break;
            case "box": result = NpgsqlDbType.Box; break;
            case "path": result = NpgsqlDbType.Path; break;
            case "polygon": result = NpgsqlDbType.Polygon; break;
            case "circle": result = NpgsqlDbType.Circle; break;

            case "cidr": result = NpgsqlDbType.Cidr; break;
            case "inet": result = NpgsqlDbType.Inet; break;
            case "macaddr": result = NpgsqlDbType.MacAddr; break;
            case "macaddr8": result = NpgsqlDbType.MacAddr8; break;

            case "json": result = NpgsqlDbType.Json; break;
            case "jsonb": result = NpgsqlDbType.Jsonb; break;

            case "uuid": result = NpgsqlDbType.Uuid; break;

            case "oidvector": result = NpgsqlDbType.Oidvector; break;
            case "citext": result = NpgsqlDbType.Citext; break;
            case "tsvector": result = NpgsqlDbType.TsVector; break;
            case "tsquery": result = NpgsqlDbType.TsQuery; break;
            case "regconfig": result = NpgsqlDbType.Regconfig; break;

            case "int4range": result = NpgsqlDbType.Integer | NpgsqlDbType.Range; break;
            case "int8range": result = NpgsqlDbType.Bigint | NpgsqlDbType.Range; break;
            case "numrange": result = NpgsqlDbType.Numeric | NpgsqlDbType.Range; break;
            case "tsrange": result = NpgsqlDbType.Timestamp | NpgsqlDbType.Range; break;
            case "tstzrange": result = NpgsqlDbType.TimestampTz | NpgsqlDbType.Range; break;
            case "daterange": result = NpgsqlDbType.Date | NpgsqlDbType.Range; break;

            case "hstore": result = NpgsqlDbType.Hstore; break;

            case "geometry": result = NpgsqlDbType.Geometry; break;
        }
        if (columnInfo.ArrayDimens > 0)
            result = result | NpgsqlDbType.Array;
        return result;
    }
    public override void MapTables(string connectionString, IEntityMapProvider mapProvider)
    {
        var tableNames = mapProvider.EntityMaps.Where(f => !f.IsMapped).Select(f => f.TableName).ToList();
        if (tableNames == null || tableNames.Count == 0)
            return;
        var sql = @"SELECT b.nspname,a.relname,c.attname,c.attndims,d.typname,CASE WHEN c.atttypmod>0 AND c.atttypmod<32767 THEN c.atttypmod-4 ELSE c.attlen END,e.description,pg_get_expr(g.adbin,g.adrelid),
f.conname IS NOT NULL,h.refobjid IS NOT NULL,c.attnotnull,c.attnum FROM pg_class a INNER JOIN pg_namespace b ON a.relnamespace = b.oid INNER JOIN pg_attribute c ON a.oid = c.attrelid AND c.attnum>0
INNER JOIN pg_type d ON c.atttypid=d.oid LEFT JOIN pg_description e ON e.objoid=c.attrelid AND e.objsubid=c.attnum LEFT JOIN pg_constraint f ON a.oid=f.conrelid AND f.contype='p' and f.conkey @> array[c.attnum] 
LEFT JOIN pg_attrdef g ON a.oid=g.adrelid AND c.attnum=g.adnum LEFT JOIN (select dp.refobjid,dp.refobjsubid FROM pg_depend dp,pg_class cs WHERE dp.objid=cs.oid AND cs.relkind='S') h ON a.oid=h.refobjid
AND c.attnum=h.refobjsubid WHERE a.relkind='r' AND {0} ORDER BY b.nspname,a.relname,c.attnum asc";
        var tableBuilders = new Dictionary<string, StringBuilder>();
        foreach (var tableName in tableNames)
        {
            StringBuilder builder = null;
            string myTableName = null;
            if (tableName.Contains('.'))
            {
                var myTableNames = tableName.Split('.');
                var tableSchema = myTableNames[0];
                myTableName = myTableNames[1];
                if (!tableBuilders.TryGetValue(tableSchema, out builder))
                    tableBuilders.Add(tableSchema, builder = new StringBuilder());
            }
            else
            {
                var tableSchema = this.DefaultTableSchema;
                if (!tableBuilders.TryGetValue(tableSchema, out builder))
                    tableBuilders.Add(tableSchema, builder = new StringBuilder());
                myTableName = tableName;
            }
            if (builder.Length > 0)
                builder.Append(',');
            builder.Append($"'{myTableName}'");
        }
        var sqlBuilder = new StringBuilder();
        foreach (var tableBuilder in tableBuilders)
        {
            if (sqlBuilder.Length > 0)
                sqlBuilder.Append(" OR ");

            sqlBuilder.Append($"b.nspname='{tableBuilder.Key}' AND a.relname IN ({tableBuilder.Value.ToString()})");
        }
        sql = string.Format(sql, sqlBuilder.ToString());
        var entityMappers = mapProvider.EntityMaps.ToList();
        var tableInfos = new List<DbTableInfo>();
        using var connection = new NpgsqlConnection(connectionString);
        using var command = new NpgsqlCommand(sql, connection);
        connection.Open();
        using var reader = command.ExecuteReader(CommandBehavior.SequentialAccess);

        DbTableInfo tableInfo = null;
        while (reader.Read())
        {
            var tableSchema = reader.ToFieldValue<string>(0);
            var tableName = reader.ToFieldValue<string>(1);
            if (tableInfo == null || tableInfo.TableSchema != tableSchema || tableInfo.TableName != tableName)
            {
                tableInfo = new DbTableInfo
                {
                    TableSchema = tableSchema,
                    TableName = tableName,
                    Columns = new List<DbColumnInfo>()
                };
                tableInfos.Add(tableInfo);
            }
            var fieldName = reader.ToFieldValue<string>(2);
            var arrayDimens = reader.ToFieldValue<int>(3);
            var dataType = reader.ToFieldValue<string>(4);
            var length = reader.ToFieldValue<int>(5);
            var scale = (length >> 16) & 0xFFFF;
            var precision = length & 0xFFFF;
            var lengthTypes = new[] { "bool", "name", "bit", "varbit", "char", "bpchar", "varchar", "bytea", "text", "uuid" };
            if (length > 0 && !lengthTypes.Contains(dataType))
                length *= 8;
            var needLengthTypes = new[] { "char", "bpchar", "varchar", "bytea", "bit", "varbit" };
            if (dataType == "bpchar")
                dataType = "char";
            var columnType = dataType;
            if (needLengthTypes.Contains(dataType))
                columnType += $"({length})";
            if (arrayDimens > 0)
            {
                sqlBuilder.Clear();
                sqlBuilder.Append(dataType.Substring(1));
                for (int i = 0; i < arrayDimens; i++)
                    sqlBuilder.Append("[]");
                columnType = sqlBuilder.ToString();
            }
            tableInfo.Columns.Add(new DbColumnInfo
            {
                FieldName = fieldName,
                DataType = dataType,
                DbColumnType = columnType,
                MaxLength = length,
                Scale = scale,
                Precision = precision,
                ArrayDimens = arrayDimens,
                Description = reader.ToFieldValue<string>(6),
                DefaultValue = reader.ToFieldValue<string>(7),
                IsPrimaryKey = reader.ToFieldValue<bool>(8),
                IsAutoIncrement = reader.ToFieldValue<bool>(9),
                IsNullable = !reader.ToFieldValue<bool>(10),
                Position = reader.ToFieldValue<int>(11)
            });
        }
        reader.Close();
        connection.Close();

        var fieldMapHandler = mapProvider.FieldMapHandler;
        foreach (var entityMapper in entityMappers)
        {
            (var tableSchema, var tableName) = this.GetFullTableName(entityMapper.TableName);
            tableInfo = tableInfos.Find(f => f.TableSchema == tableSchema && f.TableName == tableName);
            if (tableInfo == null)
                continue;

            var memberInfos = entityMapper.EntityType.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                .Where(f => f.MemberType == MemberTypes.Property || f.MemberType == MemberTypes.Field).ToList();

            var mappedMappers = new List<MemberMap>();
            foreach (var columnInfo in tableInfo.Columns)
            {
                if (fieldMapHandler.TryFindMember(columnInfo.FieldName, entityMapper.MemberMaps, out var memberMapper))
                {
                    memberMapper.DbColumnType = columnInfo.DbColumnType;
                    memberMapper.IsKey = columnInfo.IsPrimaryKey;
                    memberMapper.IsAutoIncrement = columnInfo.IsAutoIncrement;
                    memberMapper.IsRequired = !columnInfo.IsNullable;
                    memberMapper.MaxLength = columnInfo.MaxLength;
                    memberMapper.NativeDbType = this.MapNativeDbType(columnInfo);
                    memberMapper.Position = columnInfo.Position;
                }
                else
                {
                    if (!fieldMapHandler.TryFindMember(columnInfo.FieldName, memberInfos, out var memberInfo))
                    {
                        if (columnInfo.IsNullable)
                            continue;
                        throw new Exception($"表{tableName}非空字段{columnInfo.FieldName}在实体{entityMapper.EntityType.FullName}中没有对应映射成员或是不满足默认字段映射处理器DefaultFieldMapHandler规则，可手动配置映射字段如：.Member(f => f.XxxMember).Field(\"xxxField\")，如果是RowVersion字段，需要手动指定，如：.Member(f => f.XxxMember).Field(\"xxxField\").RowVersion()");
                    }
                    entityMapper.AddMemberMap(memberInfo.Name, memberMapper = new MemberMap(entityMapper, memberInfo)
                    {
                        FieldName = columnInfo.FieldName,
                        DbColumnType = columnInfo.DbColumnType,
                        IsKey = columnInfo.IsPrimaryKey,
                        IsAutoIncrement = columnInfo.IsAutoIncrement,
                        IsRequired = !columnInfo.IsNullable,
                        MaxLength = columnInfo.MaxLength,
                        NativeDbType = this.MapNativeDbType(columnInfo),
                        Position = columnInfo.Position
                    });
                }
                if (memberMapper.TypeHandler == null && !memberMapper.IsIgnore)
                {
                    //允许自定义TypeHandlerType设置，默认设置，刨除内置的支持类型
                    if ((memberMapper.UnderlyingType.IsClass && memberMapper.UnderlyingType != typeof(string) || memberMapper.UnderlyingType.IsEntityType(out _))
                        && this.MapDefaultType(memberMapper.NativeDbType) == typeof(string) && !selfTypes.Contains(memberMapper.UnderlyingType))
                        memberMapper.TypeHandlerType = typeof(JsonTypeHandler);

                    //object类型
                    if (memberMapper.MemberType == typeof(object) && this.MapDefaultType(memberMapper) == typeof(string))
                        memberMapper.TypeHandlerType = typeof(ToStringTypeHandler);

                    if (memberMapper.TypeHandlerType != null)
                        memberMapper.TypeHandler = this.GetTypeHandler(memberMapper.TypeHandlerType);
                }
                mappedMappers.Add(memberMapper);
            }
            var ignoreMappers = entityMapper.MemberMaps.Except(mappedMappers).ToList();
            if (ignoreMappers.Count > 0)
            {
                foreach (var memberMapper in ignoreMappers)
                {
                    if (memberMapper.IsNavigation || memberMapper.IsRowVersion)
                        continue;
                    memberMapper.IsIgnore = true;
                }
            }

            //非默认TableSchema表名就不变更了
            if (tableSchema != this.DefaultTableSchema)
                entityMapper.TableSchema = tableSchema;
            else entityMapper.TableName = tableName;
            entityMapper.IsMapped = true;
        }
    }
    public override bool TryGetMyMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter)
    {
        var methodInfo = methodCallExpr.Method;
        var parameterInfos = methodInfo.GetParameters();
        int cacheKey = 0;
        switch (methodInfo.Name)
        {
            case "Excluded":
                var genericArgumentTypes = methodInfo.DeclaringType.GetGenericArguments();
                if (genericArgumentTypes.Length == 1 && methodInfo.DeclaringType == typeof(IPostgreSqlCreateConflictDoUpdate<>).MakeGenericType(genericArgumentTypes[0]))
                {
                    cacheKey = RepositoryHelper.GetCacheKey(typeof(IPostgreSqlCreateConflictDoUpdate<>), methodInfo.GetGenericMethodDefinition());
                    //.OnConflict(x => x.UseKeys().Set(f => new { TotalAmount = f.TotalAmount + x.Excluded(f.TotalAmount) }) ... )
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var myVisitor = visitor as PostgreSqlCreateVisitor;
                        if (args[0] is not MemberExpression memberExpr)
                            throw new NotSupportedException($"不支持的表达式访问，类型{methodInfo.DeclaringType.FullName}.Excluded方法，只支持MemberAccess访问，如：.Set(f =&gt; new {{TotalAmount = x.Excluded(f.TotalAmount)}})");
                        if (!myVisitor.Tables[0].Mapper.TryGetMemberMap(memberExpr.Member.Name, out var memberMapper))
                            throw new MissingMemberException($"类{myVisitor.Tables[0].EntityType.FullName}未找到成员{memberExpr.Member.Name}");

                        var fieldName = $"EXCLUDED.{this.GetFieldName(memberMapper.FieldName)}";
                        return new SqlFieldSegment
                        {
                            HasField = true,
                            FromMember = memberMapper.Member,
                            SegmentType = memberMapper.MemberType,
                            NativeDbType = memberMapper.NativeDbType,
                            TypeHandler = memberMapper.TypeHandler,
                            Body = fieldName
                        };
                    });
                    return true;
                }
                break;
            case "IsNull":
                cacheKey = RepositoryHelper.GetCacheKey(typeof(Sql), methodInfo.GetGenericMethodDefinition());
                formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                {
                    var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                    var rightSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[1] });
                    var targetArgument = visitor.GetQuotedValue(targetSegment);
                    var rightArgument = visitor.GetQuotedValue(rightSegment);
                    return targetSegment.Merge(rightSegment, $"COALESCE({targetArgument},{rightArgument})", false, true);
                });
                return true;
        }
        formatter = null;
        return false;
    }
    public virtual List<string> GetShardingTableNames<TEntity>(DbContext dbContext, Func<string, bool> tableNameSelector, string tableSchema = null)
    {
        var entityMapper = dbContext.MapProvider.GetEntityMap(typeof(TEntity));
        var orgTableName = entityMapper.TableName;
        tableSchema ??= dbContext.DefaultTableSchema;
        var sql = $"SELECT a.relname FROM pg_class a,pg_namespace b WHERE a.relnamespace=b.oid AND a.relkind='r' AND a.relname LIKE '{orgTableName}_%' AND b.nspname='{tableSchema}'";
        var tableNames = dbContext.Query<string>(sql);
        return tableNames.FindAll(f => tableNameSelector(f));
    }
    public virtual async Task<List<string>> GetShardingTableNamesAsync<TEntity>(DbContext dbContext, Func<string, bool> tableNameSelector, string tableSchema = null, CancellationToken cancellationToken = default)
    {
        var entityMapper = dbContext.MapProvider.GetEntityMap(typeof(TEntity));
        var orgTableName = entityMapper.TableName;
        tableSchema ??= dbContext.DefaultTableSchema;
        var sql = $"SELECT a.relname FROM pg_class a,pg_namespace b WHERE a.relnamespace=b.oid AND a.relkind='r' AND a.relname LIKE '{orgTableName}_%' AND b.nspname='{tableSchema}'";
        var tableNames = await dbContext.QueryAsync<string>(sql);
        return tableNames.FindAll(f => tableNameSelector(f));
    }
    public int ExecuteBulkCopy(bool isUpdate, DbContext dbContext, SqlVisitor visitor, ITheaConnection connection, Type insertObjType, IEnumerable insertObjs, string tableName = null)
    {
        var entityMapper = visitor.Tables[0].Mapper;
        var memberMappers = visitor.GetRefMemberMappers(insertObjType, entityMapper, isUpdate);

        connection.Open();
        var fromMapper = visitor.Tables[0].Mapper;
        int index = 0;
        tableName ??= fromMapper.TableName;
        var builder = new StringBuilder($"COPY {this.GetTableName(tableName)}(");
        foreach ((var refMemberMapper, _) in memberMappers)
        {
            if (index > 0) builder.Append(',');
            builder.Append(this.GetFieldName(refMemberMapper.FieldName));
            index++;
        }
        builder.Append(") FROM STDIN BINARY");
        var dbConnection = connection.BaseConnection as NpgsqlConnection;
        var transaction = dbContext.Transaction?.BaseTransaction as NpgsqlTransaction;
        var createdAt = DateTime.Now;
        dbContext.DbInterceptors.OnCommandExecuting?.Invoke(new CommandEventArgs
        {
            DbKey = dbContext.DbKey,
            ConnectionString = connection.ConnectionString,
            SqlType = CommandSqlType.BulkCopyInsert
        });
        int recordsAffected = 0;
        bool isSuccess = true;
        Exception exception = null;
        try
        {
            using var writer = dbConnection.BeginBinaryImport(builder.ToString());
            foreach (var insertObj in insertObjs)
            {
                writer.StartRow();
                foreach ((var refMemberMapper, var valueGetter) in memberMappers)
                {
                    object fieldValue = valueGetter.Invoke(insertObj);
                    writer.Write(fieldValue, (NpgsqlDbType)refMemberMapper.NativeDbType);
                }
                recordsAffected++;
            }
            writer.Complete();
        }
        catch (Exception ex)
        {
            exception = ex;
            isSuccess = false;
        }
        finally
        {
            var elapsed = DateTime.Now.Subtract(createdAt).TotalMilliseconds;
            dbContext.DbInterceptors.OnCommandExecuted?.Invoke(new CommandCompletedEventArgs
            {
                DbKey = dbContext.DbKey,
                ConnectionString = connection.ConnectionString,
                SqlType = CommandSqlType.BulkCopyInsert,
                IsSuccess = isSuccess,
                Exception = exception,
                Elapsed = (int)elapsed
            });
            builder.Clear();
        }
        if (!isSuccess)
        {
            if (transaction == null) connection.Close();
            throw exception;
        }
        return recordsAffected;
    }
    public async Task<int> ExecuteBulkCopyAsync(bool isUpdate, DbContext dbContext, SqlVisitor visitor, ITheaConnection connection, Type insertObjType, IEnumerable insertObjs, CancellationToken cancellationToken = default, string tableName = null)
    {
        var entityMapper = visitor.Tables[0].Mapper;
        var memberMappers = visitor.GetRefMemberMappers(insertObjType, entityMapper, isUpdate);

        await connection.OpenAsync(cancellationToken);
        var fromMapper = visitor.Tables[0].Mapper;
        int index = 0;
        tableName ??= fromMapper.TableName;
        var builder = new StringBuilder($"COPY {this.GetTableName(tableName)}(");
        foreach ((var refMemberMapper, _) in memberMappers)
        {
            if (index > 0) builder.Append(',');
            builder.Append(this.GetFieldName(refMemberMapper.FieldName));
            index++;
        }
        builder.Append(") FROM STDIN BINARY");
        var dbConnection = connection.BaseConnection as NpgsqlConnection;
        var transaction = dbContext.Transaction?.BaseTransaction as NpgsqlTransaction;
        var createdAt = DateTime.Now;
        dbContext.DbInterceptors.OnCommandExecuting?.Invoke(new CommandEventArgs
        {
            DbKey = dbContext.DbKey,
            ConnectionString = connection.ConnectionString,
            SqlType = CommandSqlType.BulkCopyInsert
        });
        int recordsAffected = 0;
        bool isSuccess = true;
        Exception exception = null;
        try
        {
            using var writer = await dbConnection.BeginBinaryImportAsync(builder.ToString(), cancellationToken);
            foreach (var insertObj in insertObjs)
            {
                await writer.StartRowAsync(cancellationToken);
                foreach ((var refMemberMapper, var valueGetter) in memberMappers)
                {
                    object fieldValue = valueGetter.Invoke(insertObj);
                    await writer.WriteAsync(fieldValue, (NpgsqlDbType)refMemberMapper.NativeDbType, cancellationToken);
                }
                recordsAffected++;
            }
            await writer.CompleteAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            exception = ex;
            isSuccess = false;
        }
        finally
        {
            var elapsed = DateTime.Now.Subtract(createdAt).TotalMilliseconds;
            dbContext.DbInterceptors.OnCommandExecuted?.Invoke(new CommandCompletedEventArgs
            {
                DbKey = dbContext.DbKey,
                ConnectionString = connection.ConnectionString,
                SqlType = CommandSqlType.BulkCopyInsert,
                IsSuccess = isSuccess,
                Exception = exception,
                Elapsed = (int)elapsed
            });
            builder.Clear();
        }
        if (!isSuccess)
        {
            if (transaction == null) connection.Close();
            throw exception;
        }
        return recordsAffected;
    }

    public static NpgsqlBox ParseBox(string strValue)
    {
        var match = NpgsqlBoxRegex.Match(strValue);
        return new NpgsqlBox(new NpgsqlPoint(double.Parse(match.Groups[1].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat), double.Parse(match.Groups[2].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat)), new NpgsqlPoint(double.Parse(match.Groups[3].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat), double.Parse(match.Groups[4].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat)));
    }
    public static NpgsqlCircle ParseCircle(string strValue)
    {
        var match = NpgsqlCircleRegex.Match(strValue);
        if (!match.Success)
            throw new FormatException("Not a valid circle: " + strValue);
        return new NpgsqlCircle(double.Parse(match.Groups[1].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat), double.Parse(match.Groups[2].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat), double.Parse(match.Groups[3].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat));
    }
    public static NpgsqlLine ParseLine(string strValue)
    {
        var match = NpgsqlLineRegex.Match(strValue);
        if (!match.Success)
            throw new FormatException("Not a valid line: " + strValue);
        return new NpgsqlLine(double.Parse(match.Groups[1].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat), double.Parse(match.Groups[2].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat), double.Parse(match.Groups[3].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat));
    }
    public static NpgsqlLSeg ParseLSeg(string strValue)
    {
        var match = NpgsqlLSegRegex.Match(strValue);
        if (!match.Success)
            throw new FormatException("Not a valid line: " + strValue);
        return new NpgsqlLSeg(double.Parse(match.Groups[1].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat), double.Parse(match.Groups[2].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat), double.Parse(match.Groups[3].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat), double.Parse(match.Groups[4].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat));
    }
    public static NpgsqlPath ParsePath(string strValue)
    {
        var result = new NpgsqlPath(strValue[0] switch
        {
            '[' => true,
            '(' => false,
            _ => throw new Exception("Invalid path string: " + strValue),
        });
        int num = 1;
        while (true)
        {
            int num2 = strValue.IndexOf(')', num);
            result.Add(ParsePoint(strValue.Substring(num, num2 - num + 1)));
            if (strValue[num2 + 1] != ',')
                break;
            num = num2 + 2;
        }
        return result;
    }
    public static NpgsqlPoint ParsePoint(string strValue)
    {
        var match = NpgsqlPointRegex.Match(strValue);
        if (!match.Success)
            throw new FormatException("Not a valid point: " + strValue);
        return new NpgsqlPoint(double.Parse(match.Groups[1].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat), double.Parse(match.Groups[2].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture.NumberFormat));
    }
    public static NpgsqlPolygon ParsePolygon(string strValue)
    {
        var list = new List<NpgsqlPoint>();
        int num = 1;
        while (true)
        {
            int num2 = strValue.IndexOf(')', num);
            list.Add(ParsePoint(strValue.Substring(num, num2 - num + 1)));
            if (strValue[num2 + 1] != ',')
                break;
            num = num2 + 2;
        }
        return new NpgsqlPolygon(list);
    }
    public static NpgsqlInet ParseInet(string strValue) => new NpgsqlInet(strValue);
    public static Func<object, object> GetSelfTypeParserFunc(Type selfType)
    {
        MethodInfo methodInfo = null;
        switch (selfType)
        {
            case Type factType when factType == typeof(NpgsqlInet):
                methodInfo = typeof(PostgreSqlProvider).GetMethod(nameof(ParseInet));
                break;
            case Type factType when factType == typeof(NpgsqlPoint):
                methodInfo = typeof(PostgreSqlProvider).GetMethod(nameof(ParsePoint));
                break;
            case Type factType when factType == typeof(NpgsqlLine):
                methodInfo = typeof(PostgreSqlProvider).GetMethod(nameof(ParseLine));
                break;
            case Type factType when factType == typeof(NpgsqlLSeg):
                methodInfo = typeof(PostgreSqlProvider).GetMethod(nameof(ParseLSeg));
                break;
            case Type factType when factType == typeof(NpgsqlBox):
                methodInfo = typeof(PostgreSqlProvider).GetMethod(nameof(ParseBox));
                break;
            case Type factType when factType == typeof(NpgsqlPath):
                methodInfo = typeof(PostgreSqlProvider).GetMethod(nameof(ParsePath));
                break;
            case Type factType when factType == typeof(NpgsqlPolygon):
                methodInfo = typeof(PostgreSqlProvider).GetMethod(nameof(ParsePolygon));
                break;
            case Type factType when factType == typeof(NpgsqlCircle):
                methodInfo = typeof(PostgreSqlProvider).GetMethod(nameof(ParseCircle));
                break;
            case Type factType when factType == typeof(IPAddress):
                methodInfo = typeof(IPAddress).GetMethod(nameof(IPAddress.Parse), [typeof(string)]);
                break;
            case Type factType when factType == typeof(PhysicalAddress):
                methodInfo = typeof(PhysicalAddress).GetMethod(nameof(PhysicalAddress.Parse), [typeof(string)]);
                break;
        }
        var valueExpr = Expression.Parameter(typeof(object), "value");
        var bodyExpr = Expression.Convert(Expression.Call(methodInfo, Expression.Convert(valueExpr, typeof(string))), typeof(object));
        return Expression.Lambda<Func<object, object>>(bodyExpr, valueExpr).Compile();
    }
}