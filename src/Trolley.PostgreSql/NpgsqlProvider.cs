using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Trolley;

public class NpgSqlProvider : BaseOrmProvider
{
    private static Func<string, IDbConnection> createNativeConnectonDelegate = null;
    private static Func<string, object, IDbDataParameter> createDefaultNativeParameterDelegate = null;
    private static Func<string, object, object, IDbDataParameter> createNativeParameterDelegate = null;
    private static ConcurrentDictionary<MemberInfo, MemberAccessSqlFormatter> memberAccessSqlFormatterCahe = new();
    private static ConcurrentDictionary<MethodInfo, MethodCallSqlFormatter> methodCallSqlFormatterCahe = new();
    private static Dictionary<object, Type> defaultMapTypes = new();
    private static Dictionary<Type, object> defaultDbTypes = new();
    private static Dictionary<int, object> nativeDbTypes = new();
    private static Dictionary<Type, string> castTos = new();

    public override DatabaseType DatabaseType => DatabaseType.Postgresql;
    public override string SelectIdentitySql => " RETURNING {0}";
    static NpgSqlProvider()
    {
        var connectionType = Type.GetType("Npgsql.NpgsqlConnection, Npgsql, Culture=neutral, PublicKeyToken=5d8b90d52f46fda7");
        createNativeConnectonDelegate = BaseOrmProvider.CreateConnectionDelegate(connectionType);
        var dbTypeType = Type.GetType("NpgsqlTypes.NpgsqlDbType, Npgsql, Culture=neutral, PublicKeyToken=5d8b90d52f46fda7");
        var dbParameterType = Type.GetType("Npgsql.NpgsqlParameter, Npgsql, Culture=neutral, PublicKeyToken=5d8b90d52f46fda7");
        var valuePropertyInfo = dbParameterType.GetProperty("Value");
        createDefaultNativeParameterDelegate = BaseOrmProvider.CreateDefaultParameterDelegate(dbParameterType);
        createNativeParameterDelegate = BaseOrmProvider.CreateParameterDelegate(dbTypeType, dbParameterType, valuePropertyInfo);

        defaultMapTypes[Enum.Parse(dbTypeType, "Bigint")] = typeof(long);
        defaultMapTypes[Enum.Parse(dbTypeType, "Boolean")] = typeof(bool);
        defaultMapTypes[Enum.Parse(dbTypeType, "Bytea")] = typeof(byte[]);
        defaultMapTypes[Enum.Parse(dbTypeType, "Char")] = typeof(string);
        defaultMapTypes[Enum.Parse(dbTypeType, "Date")] = typeof(DateTime);
        defaultMapTypes[Enum.Parse(dbTypeType, "Double")] = typeof(double);
        defaultMapTypes[Enum.Parse(dbTypeType, "Integer")] = typeof(int);
        defaultMapTypes[Enum.Parse(dbTypeType, "Money")] = typeof(decimal);
        defaultMapTypes[Enum.Parse(dbTypeType, "Numeric")] = typeof(decimal);
        defaultMapTypes[Enum.Parse(dbTypeType, "Real")] = typeof(float);
        defaultMapTypes[Enum.Parse(dbTypeType, "Smallint")] = typeof(short);
        defaultMapTypes[Enum.Parse(dbTypeType, "Text")] = typeof(string);
        defaultMapTypes[Enum.Parse(dbTypeType, "Time")] = typeof(TimeSpan);
        defaultMapTypes[Enum.Parse(dbTypeType, "Timestamp")] = typeof(DateTime);
        defaultMapTypes[Enum.Parse(dbTypeType, "Varchar")] = typeof(string);
        defaultMapTypes[Enum.Parse(dbTypeType, "Bit")] = typeof(byte[]);
        defaultMapTypes[Enum.Parse(dbTypeType, "TimestampTz")] = typeof(DateTimeOffset);
        defaultMapTypes[Enum.Parse(dbTypeType, "TimestampTZ")] = typeof(DateTimeOffset);
        defaultMapTypes[Enum.Parse(dbTypeType, "Uuid")] = typeof(Guid);
        defaultMapTypes[Enum.Parse(dbTypeType, "Xml")] = typeof(string);
        defaultMapTypes[Enum.Parse(dbTypeType, "Interval")] = typeof(TimeSpan);
        defaultMapTypes[Enum.Parse(dbTypeType, "TimeTz")] = typeof(TimeSpan);
        defaultMapTypes[Enum.Parse(dbTypeType, "TimeTZ")] = typeof(TimeSpan);
        defaultMapTypes[Enum.Parse(dbTypeType, "Json")] = typeof(string);
        defaultMapTypes[Enum.Parse(dbTypeType, "Jsonb")] = typeof(string);
        defaultMapTypes[Enum.Parse(dbTypeType, "InternalChar")] = typeof(string);
        defaultMapTypes[Enum.Parse(dbTypeType, "Varbit")] = typeof(byte[]);
        defaultMapTypes[Enum.Parse(dbTypeType, "Oid")] = typeof(uint);
        defaultMapTypes[Enum.Parse(dbTypeType, "Xid")] = typeof(uint);
        defaultMapTypes[Enum.Parse(dbTypeType, "Cid")] = typeof(uint);
        defaultMapTypes[Enum.Parse(dbTypeType, "Xid8")] = typeof(ulong);
        //defaultMapTypes[Enum.Parse(dbTypeType, "BigIntMultirange")] = typeof(long);
        //defaultMapTypes[Enum.Parse(dbTypeType, "DateMultirange")] = typeof(DateTime);
        //defaultMapTypes[Enum.Parse(dbTypeType, "IntegerMultirange")] = typeof(int);
        //defaultMapTypes[Enum.Parse(dbTypeType, "NumericMultirange")] = typeof(decimal);
        //defaultMapTypes[Enum.Parse(dbTypeType, "TimestampMultirange")] = typeof(DateTime);
        //defaultMapTypes[Enum.Parse(dbTypeType, "TimestampTzMultirange")] = typeof(DateTimeOffset);
        //defaultMapTypes[Enum.Parse(dbTypeType, "Range")] = typeof("Range");
        defaultMapTypes[Enum.Parse(dbTypeType, "BigIntRange")] = typeof(long);
        defaultMapTypes[Enum.Parse(dbTypeType, "DateRange")] = typeof(DateTime);
        defaultMapTypes[Enum.Parse(dbTypeType, "IntegerRange")] = typeof(int);
        defaultMapTypes[Enum.Parse(dbTypeType, "NumericRange")] = typeof(decimal);
        defaultMapTypes[Enum.Parse(dbTypeType, "TimestampRange")] = typeof(DateTime);
        defaultMapTypes[Enum.Parse(dbTypeType, "TimestampTzRange")] = typeof(DateTimeOffset);
        //defaultMapTypes[Enum.Parse(dbTypeType, "Array")] = typeof("Array");

        //Npgsql支持数据类型，值为各自值|int.MinValue
        //如, int[]类型: int.MinValue | NpgsqlDbType.Integer
        defaultMapTypes[Enum.ToObject(dbTypeType, (int)Enum.Parse(dbTypeType, "Array") | (int)Enum.Parse(dbTypeType, "Bigint"))] = typeof(long[]);
        defaultMapTypes[Enum.ToObject(dbTypeType, (int)Enum.Parse(dbTypeType, "Array") | (int)Enum.Parse(dbTypeType, "Boolean"))] = typeof(bool[]);
        defaultMapTypes[Enum.ToObject(dbTypeType, (int)Enum.Parse(dbTypeType, "Array") | (int)Enum.Parse(dbTypeType, "Char"))] = typeof(string[]);
        defaultMapTypes[Enum.ToObject(dbTypeType, (int)Enum.Parse(dbTypeType, "Array") | (int)Enum.Parse(dbTypeType, "Date"))] = typeof(DateTime[]);
        defaultMapTypes[Enum.ToObject(dbTypeType, (int)Enum.Parse(dbTypeType, "Array") | (int)Enum.Parse(dbTypeType, "Double"))] = typeof(double[]);
        defaultMapTypes[Enum.ToObject(dbTypeType, (int)Enum.Parse(dbTypeType, "Array") | (int)Enum.Parse(dbTypeType, "Integer"))] = typeof(int[]);
        defaultMapTypes[Enum.ToObject(dbTypeType, (int)Enum.Parse(dbTypeType, "Array") | (int)Enum.Parse(dbTypeType, "Money"))] = typeof(decimal[]);
        defaultMapTypes[Enum.ToObject(dbTypeType, (int)Enum.Parse(dbTypeType, "Array") | (int)Enum.Parse(dbTypeType, "Numeric"))] = typeof(decimal[]);
        defaultMapTypes[Enum.ToObject(dbTypeType, (int)Enum.Parse(dbTypeType, "Array") | (int)Enum.Parse(dbTypeType, "Real"))] = typeof(float[]);
        defaultMapTypes[Enum.ToObject(dbTypeType, (int)Enum.Parse(dbTypeType, "Array") | (int)Enum.Parse(dbTypeType, "Smallint"))] = typeof(short[]);
        defaultMapTypes[Enum.ToObject(dbTypeType, (int)Enum.Parse(dbTypeType, "Array") | (int)Enum.Parse(dbTypeType, "Text"))] = typeof(string[]);
        defaultMapTypes[Enum.ToObject(dbTypeType, (int)Enum.Parse(dbTypeType, "Array") | (int)Enum.Parse(dbTypeType, "Time"))] = typeof(TimeSpan[]);
        defaultMapTypes[Enum.ToObject(dbTypeType, (int)Enum.Parse(dbTypeType, "Array") | (int)Enum.Parse(dbTypeType, "Timestamp"))] = typeof(DateTime[]);
        defaultMapTypes[Enum.ToObject(dbTypeType, (int)Enum.Parse(dbTypeType, "Array") | (int)Enum.Parse(dbTypeType, "Varchar"))] = typeof(string[]);
        defaultMapTypes[Enum.ToObject(dbTypeType, (int)Enum.Parse(dbTypeType, "Array") | (int)Enum.Parse(dbTypeType, "TimestampTz"))] = typeof(DateTimeOffset[]);
        defaultMapTypes[Enum.ToObject(dbTypeType, (int)Enum.Parse(dbTypeType, "Array") | (int)Enum.Parse(dbTypeType, "TimestampTZ"))] = typeof(DateTimeOffset[]);
        defaultMapTypes[Enum.ToObject(dbTypeType, (int)Enum.Parse(dbTypeType, "Array") | (int)Enum.Parse(dbTypeType, "Uuid"))] = typeof(Guid[]);
        defaultMapTypes[Enum.ToObject(dbTypeType, (int)Enum.Parse(dbTypeType, "Array") | (int)Enum.Parse(dbTypeType, "Xml"))] = typeof(string[]);
        defaultMapTypes[Enum.ToObject(dbTypeType, (int)Enum.Parse(dbTypeType, "Array") | (int)Enum.Parse(dbTypeType, "Interval"))] = typeof(TimeSpan[]);
        defaultMapTypes[Enum.ToObject(dbTypeType, (int)Enum.Parse(dbTypeType, "Array") | (int)Enum.Parse(dbTypeType, "TimeTz"))] = typeof(TimeSpan[]);
        defaultMapTypes[Enum.ToObject(dbTypeType, (int)Enum.Parse(dbTypeType, "Array") | (int)Enum.Parse(dbTypeType, "TimeTZ"))] = typeof(TimeSpan[]);
        defaultMapTypes[Enum.ToObject(dbTypeType, (int)Enum.Parse(dbTypeType, "Array") | (int)Enum.Parse(dbTypeType, "Json"))] = typeof(string[]);
        defaultMapTypes[Enum.ToObject(dbTypeType, (int)Enum.Parse(dbTypeType, "Array") | (int)Enum.Parse(dbTypeType, "Jsonb"))] = typeof(string[]);
        defaultMapTypes[Enum.ToObject(dbTypeType, (int)Enum.Parse(dbTypeType, "Array") | (int)Enum.Parse(dbTypeType, "InternalChar"))] = typeof(string[]);
        defaultMapTypes[Enum.ToObject(dbTypeType, (int)Enum.Parse(dbTypeType, "Array") | (int)Enum.Parse(dbTypeType, "Oid"))] = typeof(uint[]);
        defaultMapTypes[Enum.ToObject(dbTypeType, (int)Enum.Parse(dbTypeType, "Array") | (int)Enum.Parse(dbTypeType, "Xid"))] = typeof(uint[]);
        defaultMapTypes[Enum.ToObject(dbTypeType, (int)Enum.Parse(dbTypeType, "Array") | (int)Enum.Parse(dbTypeType, "Cid"))] = typeof(uint[]);
        defaultMapTypes[Enum.ToObject(dbTypeType, (int)Enum.Parse(dbTypeType, "Array") | (int)Enum.Parse(dbTypeType, "Xid8"))] = typeof(ulong[]);

        defaultDbTypes[typeof(long)] = Enum.Parse(dbTypeType, "Bigint");
        defaultDbTypes[typeof(bool)] = Enum.Parse(dbTypeType, "Boolean");
        defaultDbTypes[typeof(double)] = Enum.Parse(dbTypeType, "Double");
        defaultDbTypes[typeof(int)] = Enum.Parse(dbTypeType, "Integer");
        defaultDbTypes[typeof(decimal)] = Enum.Parse(dbTypeType, "Numeric");
        defaultDbTypes[typeof(float)] = Enum.Parse(dbTypeType, "Real");
        defaultDbTypes[typeof(short)] = Enum.Parse(dbTypeType, "Smallint");
        defaultDbTypes[typeof(TimeSpan)] = Enum.Parse(dbTypeType, "Time");
        defaultDbTypes[typeof(string)] = Enum.Parse(dbTypeType, "Varchar");
        defaultDbTypes[typeof(DateTimeOffset)] = Enum.Parse(dbTypeType, "TimestampTz");
        defaultDbTypes[typeof(Guid)] = Enum.Parse(dbTypeType, "Uuid");
        defaultDbTypes[typeof(uint)] = Enum.Parse(dbTypeType, "Oid");
        defaultDbTypes[typeof(ulong)] = Enum.Parse(dbTypeType, "Xid8");
        defaultDbTypes[typeof(byte[])] = Enum.Parse(dbTypeType, "Bytea");

        defaultDbTypes[typeof(long?)] = Enum.Parse(dbTypeType, "Bigint");
        defaultDbTypes[typeof(bool?)] = Enum.Parse(dbTypeType, "Boolean");
        defaultDbTypes[typeof(double?)] = Enum.Parse(dbTypeType, "Double");
        defaultDbTypes[typeof(int?)] = Enum.Parse(dbTypeType, "Integer");
        defaultDbTypes[typeof(decimal?)] = Enum.Parse(dbTypeType, "Numeric");
        defaultDbTypes[typeof(float?)] = Enum.Parse(dbTypeType, "Real");
        defaultDbTypes[typeof(short?)] = Enum.Parse(dbTypeType, "Smallint");
        defaultDbTypes[typeof(TimeSpan?)] = Enum.Parse(dbTypeType, "Time");
        defaultDbTypes[typeof(DateTimeOffset?)] = Enum.Parse(dbTypeType, "TimestampTz");
        defaultDbTypes[typeof(Guid?)] = Enum.Parse(dbTypeType, "Uuid");
        defaultDbTypes[typeof(uint?)] = Enum.Parse(dbTypeType, "Oid");
        defaultDbTypes[typeof(ulong?)] = Enum.Parse(dbTypeType, "Xid8");

        //Npgsql支持数据类型，值为各自值|int.MinValue
        //如, int[]类型: int.MinValue | NpgsqlDbType.Integer
        defaultDbTypes[typeof(long[])] = Enum.ToObject(dbTypeType, (int)Enum.Parse(dbTypeType, "Bigint") | (int)Enum.Parse(dbTypeType, "Array"));
        defaultDbTypes[typeof(bool[])] = Enum.ToObject(dbTypeType, (int)Enum.Parse(dbTypeType, "Boolean") | (int)Enum.Parse(dbTypeType, "Array"));
        defaultDbTypes[typeof(double[])] = Enum.ToObject(dbTypeType, (int)Enum.Parse(dbTypeType, "Double") | (int)Enum.Parse(dbTypeType, "Array"));
        defaultDbTypes[typeof(int[])] = Enum.ToObject(dbTypeType, (int)Enum.Parse(dbTypeType, "Integer") | (int)Enum.Parse(dbTypeType, "Array"));
        defaultDbTypes[typeof(decimal[])] = Enum.ToObject(dbTypeType, (int)Enum.Parse(dbTypeType, "Numeric") | (int)Enum.Parse(dbTypeType, "Array"));
        defaultDbTypes[typeof(float[])] = Enum.ToObject(dbTypeType, (int)Enum.Parse(dbTypeType, "Real") | (int)Enum.Parse(dbTypeType, "Array"));
        defaultDbTypes[typeof(short[])] = Enum.ToObject(dbTypeType, (int)Enum.Parse(dbTypeType, "Smallint") | (int)Enum.Parse(dbTypeType, "Array"));
        defaultDbTypes[typeof(TimeSpan[])] = Enum.ToObject(dbTypeType, (int)Enum.Parse(dbTypeType, "Time") | (int)Enum.Parse(dbTypeType, "Array"));
        defaultDbTypes[typeof(string[])] = Enum.ToObject(dbTypeType, (int)Enum.Parse(dbTypeType, "Varchar") | (int)Enum.Parse(dbTypeType, "Array"));
        defaultDbTypes[typeof(DateTimeOffset[])] = Enum.ToObject(dbTypeType, (int)Enum.Parse(dbTypeType, "TimestampTz") | (int)Enum.Parse(dbTypeType, "Array"));
        defaultDbTypes[typeof(Guid[])] = Enum.ToObject(dbTypeType, (int)Enum.Parse(dbTypeType, "Uuid") | (int)Enum.Parse(dbTypeType, "Array"));
        defaultDbTypes[typeof(uint[])] = Enum.ToObject(dbTypeType, (int)Enum.Parse(dbTypeType, "Oid") | (int)Enum.Parse(dbTypeType, "Array"));
        defaultDbTypes[typeof(ulong[])] = Enum.ToObject(dbTypeType, (int)Enum.Parse(dbTypeType, "Xid8") | (int)Enum.Parse(dbTypeType, "Array"));

        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Bigint")] = Enum.Parse(dbTypeType, "Bigint");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Boolean")] = Enum.Parse(dbTypeType, "Boolean");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Box")] = Enum.Parse(dbTypeType, "Box");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Bytea")] = Enum.Parse(dbTypeType, "Bytea");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Circle")] = Enum.Parse(dbTypeType, "Circle");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Char")] = Enum.Parse(dbTypeType, "Char");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Date")] = Enum.Parse(dbTypeType, "Date");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Double")] = Enum.Parse(dbTypeType, "Double");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Integer")] = Enum.Parse(dbTypeType, "Integer");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Line")] = Enum.Parse(dbTypeType, "Line");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "LSeg")] = Enum.Parse(dbTypeType, "LSeg");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Money")] = Enum.Parse(dbTypeType, "Money");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Numeric")] = Enum.Parse(dbTypeType, "Numeric");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Path")] = Enum.Parse(dbTypeType, "Path");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Point")] = Enum.Parse(dbTypeType, "Point");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Polygon")] = Enum.Parse(dbTypeType, "Polygon");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Real")] = Enum.Parse(dbTypeType, "Real");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Smallint")] = Enum.Parse(dbTypeType, "Smallint");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Text")] = Enum.Parse(dbTypeType, "Text");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Time")] = Enum.Parse(dbTypeType, "Time");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Timestamp")] = Enum.Parse(dbTypeType, "Timestamp");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Varchar")] = Enum.Parse(dbTypeType, "Varchar");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Refcursor")] = Enum.Parse(dbTypeType, "Refcursor");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Inet")] = Enum.Parse(dbTypeType, "Inet");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Bit")] = Enum.Parse(dbTypeType, "Bit");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "TimestampTz")] = Enum.Parse(dbTypeType, "TimestampTz");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "TimestampTZ")] = Enum.Parse(dbTypeType, "TimestampTZ");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Uuid")] = Enum.Parse(dbTypeType, "Uuid");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Xml")] = Enum.Parse(dbTypeType, "Xml");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Oidvector")] = Enum.Parse(dbTypeType, "Oidvector");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Interval")] = Enum.Parse(dbTypeType, "Interval");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "TimeTz")] = Enum.Parse(dbTypeType, "TimeTz");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "TimeTZ")] = Enum.Parse(dbTypeType, "TimeTZ");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Name")] = Enum.Parse(dbTypeType, "Name");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Abstime")] = Enum.Parse(dbTypeType, "Abstime");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "MacAddr")] = Enum.Parse(dbTypeType, "MacAddr");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Json")] = Enum.Parse(dbTypeType, "Json");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Jsonb")] = Enum.Parse(dbTypeType, "Jsonb");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Hstore")] = Enum.Parse(dbTypeType, "Hstore");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "InternalChar")] = Enum.Parse(dbTypeType, "InternalChar");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Varbit")] = Enum.Parse(dbTypeType, "Varbit");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Unknown")] = Enum.Parse(dbTypeType, "Unknown");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Oid")] = Enum.Parse(dbTypeType, "Oid");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Xid")] = Enum.Parse(dbTypeType, "Xid");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Cid")] = Enum.Parse(dbTypeType, "Cid");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Cidr")] = Enum.Parse(dbTypeType, "Cidr");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "TsVector")] = Enum.Parse(dbTypeType, "TsVector");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "TsQuery")] = Enum.Parse(dbTypeType, "TsQuery");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Regtype")] = Enum.Parse(dbTypeType, "Regtype");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Geometry")] = Enum.Parse(dbTypeType, "Geometry");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Citext")] = Enum.Parse(dbTypeType, "Citext");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Int2Vector")] = Enum.Parse(dbTypeType, "Int2Vector");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Tid")] = Enum.Parse(dbTypeType, "Tid");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "MacAddr8")] = Enum.Parse(dbTypeType, "MacAddr8");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Geography")] = Enum.Parse(dbTypeType, "Geography");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Regconfig")] = Enum.Parse(dbTypeType, "Regconfig");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "JsonPath")] = Enum.Parse(dbTypeType, "JsonPath");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "PgLsn")] = Enum.Parse(dbTypeType, "PgLsn");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "LTree")] = Enum.Parse(dbTypeType, "LTree");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "LQuery")] = Enum.Parse(dbTypeType, "LQuery");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "LTxtQuery")] = Enum.Parse(dbTypeType, "LTxtQuery");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Xid8")] = Enum.Parse(dbTypeType, "Xid8");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Multirange")] = Enum.Parse(dbTypeType, "Multirange");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "BigIntMultirange")] = Enum.Parse(dbTypeType, "BigIntMultirange");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "DateMultirange")] = Enum.Parse(dbTypeType, "DateMultirange");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "IntegerMultirange")] = Enum.Parse(dbTypeType, "IntegerMultirange");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "NumericMultirange")] = Enum.Parse(dbTypeType, "NumericMultirange");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "TimestampMultirange")] = Enum.Parse(dbTypeType, "TimestampMultirange");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "TimestampTzMultirange")] = Enum.Parse(dbTypeType, "TimestampTzMultirange");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Range")] = Enum.Parse(dbTypeType, "Range");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "BigIntRange")] = Enum.Parse(dbTypeType, "BigIntRange");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "DateRange")] = Enum.Parse(dbTypeType, "DateRange");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "IntegerRange")] = Enum.Parse(dbTypeType, "IntegerRange");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "NumericRange")] = Enum.Parse(dbTypeType, "NumericRange");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "TimestampRange")] = Enum.Parse(dbTypeType, "TimestampRange");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "TimestampTzRange")] = Enum.Parse(dbTypeType, "TimestampTzRange");
        nativeDbTypes[(int)Enum.Parse(dbTypeType, "Array")] = Enum.Parse(dbTypeType, "Array");

        castTos[typeof(string)] = "VARCHAR";
        castTos[typeof(sbyte)] = "SMALLINT";
        castTos[typeof(byte)] = "SMALLINT";
        castTos[typeof(short)] = "SMALLINT";
        castTos[typeof(ushort)] = "SMALLINT";
        castTos[typeof(int)] = "INTEGER";
        castTos[typeof(uint)] = "INTEGER";
        castTos[typeof(long)] = "BIGINT";
        castTos[typeof(ulong)] = "BIGINT";
        castTos[typeof(float)] = "DECIMAL";
        castTos[typeof(double)] = "DECIMAL";
        castTos[typeof(decimal)] = "DECIMAL";
        castTos[typeof(bool)] = "BOOLEAN";
        castTos[typeof(DateTime)] = "TIMESTAMP";

        castTos[typeof(sbyte?)] = "SMALLINT";
        castTos[typeof(byte?)] = "SMALLINT";
        castTos[typeof(short?)] = "SMALLINT";
        castTos[typeof(ushort?)] = "SMALLINT";
        castTos[typeof(int?)] = "INTEGER";
        castTos[typeof(uint?)] = "INTEGER";
        castTos[typeof(long?)] = "BIGINT";
        castTos[typeof(ulong?)] = "BIGINT";
        castTos[typeof(float?)] = "DECIMAL";
        castTos[typeof(double?)] = "DECIMAL";
        castTos[typeof(decimal?)] = "DECIMAL";
        castTos[typeof(bool?)] = "BOOLEAN";
        castTos[typeof(DateTime?)] = "TIMESTAMP";
    }
    public NpgSqlProvider()
    {
        memberAccessSqlFormatterCahe.TryAdd(typeof(string).GetMember(nameof(string.Empty))[0], target => new SqlSegment { Value = "''", IsConstantValue = true });
        memberAccessSqlFormatterCahe.TryAdd(typeof(string).GetProperty(nameof(string.Length)), target => target.Change($"LENGTH({this.GetQuotedValue(target)})", false, true));

        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetMember(nameof(DateTime.Now))[0], target => new SqlSegment { Value = "NOW()", IsExpression = true });
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetMember(nameof(DateTime.UtcNow))[0], target => new SqlSegment { Value = "NOW() AT TIME ZONE 'UTC'", IsExpression = true });
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetMember(nameof(DateTime.Today))[0], target => new SqlSegment { Value = "CURRENT_DATE", IsExpression = true });
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetMember(nameof(DateTime.MinValue))[0], target => new SqlSegment { Value = $"'{DateTime.MinValue:yyyy-MM-dd HH:mm:ss}'" });
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetMember(nameof(DateTime.MaxValue))[0], target => new SqlSegment { Value = $"'{DateTime.MinValue:yyyy-MM-dd HH:mm:ss}'" });

        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.Date)), target => target.Change($"({this.GetQuotedValue(target)})::DATE", false, true));
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.TimeOfDay)), target => target.Change($"(EXTRACT(EPOCH FROM({this.GetQuotedValue(target)})::TIME)*1000000)", false, true));
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.DayOfWeek)), target => target.Change($"EXTRACT(DOW FROM({this.GetQuotedValue(target)})::TIMESTAMP)", false, true));
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.Day)), target => target.Change($"EXTRACT(DAY FROM({this.GetQuotedValue(target)})::TIMESTAMP)", false, true));
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.DayOfYear)), target => target.Change($"EXTRACT(DOY FROM({this.GetQuotedValue(target)})::TIMESTAMP)", false, true));
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.Month)), target => target.Change($"EXTRACT(MONTH FROM({this.GetQuotedValue(target)})::TIMESTAMP)", false, true));
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.Year)), target => target.Change($"EXTRACT(YEAR FROM({this.GetQuotedValue(target)})::TIMESTAMP)", false, true));
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.Hour)), target => target.Change($"EXTRACT(HOUR FROM({this.GetQuotedValue(target)})::TIMESTAMP)", false, true));
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.Minute)), target => target.Change($"EXTRACT(MINUTE FROM({this.GetQuotedValue(target)})::TIMESTAMP)", false, true));
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.Second)), target => target.Change($"EXTRACT(SECOND FROM({this.GetQuotedValue(target)})::TIMESTAMP)", false, true));
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.Millisecond)), target => target.Change($"(EXTRACT(MILLISECONDS FROM({this.GetQuotedValue(target)})::TIMESTAMP)-EXTRACT(SECOND FROM({this.GetQuotedValue(target)})::TIMESTAMP)*1000)", false, true));
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.Ticks)), target => target.Change($"(EXTRACT(EPOCH FROM({this.GetQuotedValue(target)})::TIMESTAMP)*10000000+621355968000000000)", false, true));

        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetMember(nameof(TimeSpan.Zero))[0], target => new SqlSegment { Value = "0", IsConstantValue = true });
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetMember(nameof(TimeSpan.MinValue))[0], target => new SqlSegment { Value = $"{long.MinValue}", IsConstantValue = true });
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetMember(nameof(TimeSpan.MaxValue))[0], target => new SqlSegment { Value = $"{long.MaxValue}", IsConstantValue = true });

        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetProperty(nameof(TimeSpan.Days)), target => target.Change($"FLOOR(({this.GetQuotedValue(target)})/{(long)1000000 * 60 * 60 * 24})", false, true));
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetProperty(nameof(TimeSpan.Hours)), target => target.Change($"FLOOR(({this.GetQuotedValue(target)})/{(long)1000000 * 60 * 60}%24)", false, true));
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetProperty(nameof(TimeSpan.Milliseconds)), target => target.Change($"(FLOOR(({this.GetQuotedValue(target)})/1000)::INT8%1000)", false, true));
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetProperty(nameof(TimeSpan.Minutes)), target => target.Change($"(FLOOR(({this.GetQuotedValue(target)})/{(long)1000000 * 60})::INT8%60)", false, true));
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetProperty(nameof(TimeSpan.Seconds)), target => target.Change($"(FLOOR(({this.GetQuotedValue(target)})/1000000)::INT8%60)", false, true));
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetProperty(nameof(TimeSpan.Ticks)), target => target.Change($"(({this.GetQuotedValue(target)})*10)", false, true));
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetProperty(nameof(TimeSpan.TotalDays)), target => target.Change($"(({this.GetQuotedValue(target)})/{(long)1000000 * 60 * 60 * 24})", false, true));
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetProperty(nameof(TimeSpan.TotalHours)), target => target.Change($"(({this.GetQuotedValue(target)})/{(long)1000000 * 60 * 60})", false, true));
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetProperty(nameof(TimeSpan.TotalMilliseconds)), target => target.Change($"(({this.GetQuotedValue(target)})/1000)", false, true));
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetProperty(nameof(TimeSpan.TotalMinutes)), target => target.Change($"(({this.GetQuotedValue(target)})/{(long)1000000 * 60})", false, true));
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetProperty(nameof(TimeSpan.TotalSeconds)), target => target.Change($"(({this.GetQuotedValue(target)})/1000000)", false, true));
    }
    public override IDbConnection CreateConnection(string connectionString)
        => createNativeConnectonDelegate.Invoke(connectionString);
    public override IDbDataParameter CreateParameter(string parameterName, object value)
        => createDefaultNativeParameterDelegate.Invoke(parameterName, value);
    public override IDbDataParameter CreateParameter(string parameterName, object nativeDbType, object value)
        => createNativeParameterDelegate.Invoke(parameterName, nativeDbType, value);
    public override string GetFieldName(string propertyName) => "\"" + propertyName + "\"";
    public override string GetTableName(string entityName) => "\"" + entityName + "\"";
    public override object GetNativeDbType(Type type)
    {
        if (defaultDbTypes.TryGetValue(type, out var dbType))
            return dbType;

        //数组类型
        if (typeof(IEnumerable).IsAssignableFrom(type))
        {
            lock (this)
            {
                var genericType = type.GetGenericArguments().FirstOrDefault();
                if (defaultDbTypes.TryGetValue(genericType, out dbType))
                    return dbType;
                defaultDbTypes.TryAdd(type, dbType = this.GetNativeDbType(int.MinValue | (int)dbType));
            }
            return dbType;
        }
        throw new Exception($"类型{type.FullName}没有对应的NpgsqlTypes.NpgsqlDbType映射类型");
    }
    public override object GetNativeDbType(int nativeDbType)
    {
        if (nativeDbTypes.TryGetValue(nativeDbType, out var result))
            return result;
        var dbTypeType = Type.GetType("NpgsqlTypes.NpgsqlDbType, Npgsql, Culture=neutral, PublicKeyToken=5d8b90d52f46fda7");
        result = Enum.ToObject(dbTypeType, nativeDbType);
        if (result != null)
        {
            lock (this)
            {
                if (nativeDbTypes.TryGetValue(nativeDbType, out result))
                    return result;
                result = Enum.ToObject(dbTypeType, nativeDbType);
                nativeDbTypes.TryAdd(nativeDbType, result);
            }
            return result;
        }
        throw new Exception($"数值{nativeDbType}没有对应的NpgsqlTypes.NpgsqlDbType映射类型");
    }
    public override Type MapDefaultType(object nativeDbType)
    {
        if (defaultMapTypes.TryGetValue(nativeDbType, out var result))
            return result;
        return typeof(object);
    }
    public override string CastTo(Type type)
    {
        if (castTos.TryGetValue(type, out var dbType))
            return dbType;
        return type.ToString().ToLower();
    }
    public override bool TryGetMemberAccessSqlFormatter(MemberExpression memberExpr, ISqlVisitor sqlVisitor, out MemberAccessSqlFormatter formatter)
       => memberAccessSqlFormatterCahe.TryGetValue(memberExpr.Member, out formatter);
    public override bool TryGetMethodCallSqlFormatter(MethodCallExpression methodCallExpr, ISqlVisitor sqlVisitor, out MethodCallSqlFormatter formatter)
    {
        var methodInfo = methodCallExpr.Method;
        var parameterInfos = methodInfo.GetParameters();
        if (!methodCallSqlFormatterCahe.TryGetValue(methodInfo, out formatter))
        {
            bool result = false;
            switch (methodInfo.Name)
            {
                case "Contains":
                    //public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value);
                    //public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value, IEqualityComparer<TSource>? comparer);
                    if (methodInfo.IsStatic && parameterInfos.Length >= 2 && parameterInfos[0].ParameterType.GenericTypeArguments.Length > 0
                        && typeof(IEnumerable<>).MakeGenericType(parameterInfos[0].ParameterType.GenericTypeArguments[0]).IsAssignableFrom(parameterInfos[0].ParameterType))
                    {
                        //数组调用
                        methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (target, deferExprs, args) =>
                        {
                            var builder = new StringBuilder();
                            IEnumerable enumerable = null;
                            if (args[0] is SqlSegment argsSegment)
                                enumerable = argsSegment.Value as IEnumerable;
                            else enumerable = args[0] as IEnumerable;

                            foreach (var element in enumerable)
                            {
                                if (builder.Length > 0)
                                    builder.Append(',');
                                //目前数组元素有SqlSegment包装
                                builder.Append(this.GetQuotedValue(element));
                            }

                            var fieldName = this.GetQuotedValue(args[1]);
                            int notIndex = 0;

                            if (deferExprs != null && deferExprs.Count > 0)
                            {
                                while (deferExprs.TryPop(f => f.OperationType == OperationType.Not, out var deferrdExpr))
                                {
                                    notIndex++;
                                }
                            }
                            string notString = notIndex % 2 > 0 ? " NOT" : "";

                            if (builder.Length > 0)
                            {
                                builder.Insert(0, fieldName + $"{notString} IN (");
                                builder.Append(')');
                            }
                            //TODO:如果数组没有数据，空数据
                            else builder.Append(fieldName + " IN (NULL)");
                            return builder.ToString();
                        });
                        result = true;
                    }
                    //IEnumerable<T>,List<T>
                    //public bool Contains(T item);
                    if (!methodInfo.IsStatic && parameterInfos.Length == 1 && methodInfo.DeclaringType.GenericTypeArguments.Length > 0
                        && typeof(IEnumerable<>).MakeGenericType(methodInfo.DeclaringType.GenericTypeArguments[0]).IsAssignableFrom(methodInfo.DeclaringType))
                    {
                        methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (target, deferExprs, args) =>
                        {
                            var builder = new StringBuilder();
                            IEnumerable enumerable = null;
                            if (target is SqlSegment argsSegment)
                                enumerable = argsSegment.Value as IEnumerable;
                            else enumerable = target as IEnumerable;

                            foreach (var element in enumerable)
                            {
                                if (builder.Length > 0)
                                    builder.Append(',');
                                //目前数组元素是原来的值，没有SqlSegment包装
                                builder.Append(this.GetQuotedValue(element));
                            }
                            var fieldName = this.GetQuotedValue(args[0]);
                            int notIndex = 0;

                            if (deferExprs != null && deferExprs.Count > 0)
                            {
                                while (deferExprs.TryPop(f => f.OperationType == OperationType.Not, out var deferrdExpr))
                                {
                                    notIndex++;
                                }
                            }
                            string notString = notIndex % 2 > 0 ? " NOT" : "";

                            if (builder.Length > 0)
                            {
                                builder.Insert(0, fieldName + $"{notString} IN (");
                                builder.Append(')');
                            }
                            //TODO:如果数组没有数据，空数据
                            else builder.Append(fieldName + " IN (NULL)");
                            return builder.ToString();
                        });
                        return true;
                    }
                    //String
                    //public bool Contains(char value);
                    //public bool Contains(char value, StringComparison comparisonType);
                    //public bool Contains(String value);
                    //public bool Contains(String value, StringComparison comparisonType);
                    if (!methodInfo.IsStatic && parameterInfos.Length >= 1 && methodInfo.DeclaringType.IsAssignableFrom(typeof(string)))
                    {
                        methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (target, deferExprs, args) =>
                        {
                            var leftField = this.GetQuotedValue(target);
                            string rightValue = null;
                            if (args[0] is SqlSegment rightSegment && rightSegment.IsParameter)
                            {
                                var concatMethodInfo = typeof(string).GetMethod("Concat", BindingFlags.Public | BindingFlags.Static, new Type[] { typeof(string), typeof(string), typeof(string) });
                                //if (this.TryGetMethodCallSqlFormatter(originalSegment, concatMethodInfo, out var concatFormatter))
                                //    //自己调用字符串连接，参数直接是字符串
                                //    rightValue = concatFormatter.Invoke(null, deferExprs, "'%'", rightSegment.Value.ToString(), "'%'");
                            }
                            else rightValue = $"'%{args[0]}%'";

                            int notIndex = 0;
                            if (deferExprs != null && deferExprs.Count > 0)
                            {
                                while (deferExprs.TryPop(f => f.OperationType == OperationType.Not, out var deferrdExpr))
                                {
                                    notIndex++;
                                }
                            }
                            string notString = notIndex % 2 > 0 ? " NOT" : "";
                            return $"{leftField}{notString} LIKE {rightValue}";
                        });
                        result = true;
                    }
                    break;
                case "Concat":
                    //public static String Concat(IEnumerable<String?> values);
                    //public static String Concat(params String?[] values);
                    //public static String Concat<T>(IEnumerable<T> values);
                    //public static String Concat(params object?[] args);
                    //public static String Concat(object? arg0);
                    //public static String Concat(object? arg0, object? arg1, object? arg2);
                    //public static String Concat(String? str0, String? str1, String? str2, String? str3);
                    //public static String Concat(ReadOnlySpan<char> str0, ReadOnlySpan<char> str1, ReadOnlySpan<char> str2, ReadOnlySpan<char> str3);
                    //public static IEnumerable<TSource> Concat<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second);
                    //TODO:测试一下IEnumerable<TSource> Concat<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (target, deferExprs, args) =>
                    {
                        var builder = new StringBuilder();
                        //foreach (var arg in args)
                        //{
                        //    if (arg is IEnumerable enumerable && arg is not string)
                        //    {
                        //        foreach (var element in enumerable)
                        //        {
                        //            if (builder.Length > 0)
                        //                builder.Append(" || ");

                        //            //连接符是||，不是字符串类型，无法连接，需要转换
                        //            if (element is SqlSegment sqlSegment && !sqlSegment.IsConstantValue)
                        //            {
                        //                if (sqlSegment.Expression.Type != typeof(string))
                        //                    builder.Append($"{sqlSegment.Value}::text");
                        //                else builder.Append(sqlSegment.Value.ToString());
                        //            }
                        //            else builder.Append(this.GetQuotedValue(typeof(string), element));
                        //        }
                        //    }
                        //    else
                        //    {
                        //        if (builder.Length > 0)
                        //            builder.Append(" || ");
                        //        builder.Append(this.GetQuotedValue(arg));
                        //    }
                        //}
                        return builder.ToString();
                    });
                    result = true;
                    break;
                case "Format":
                    //public static String Format(String format, object? arg0);
                    //public static String Format(String format, object? arg0, object? arg1); 
                    //public static String Format(String format, object? arg0, object? arg1, object? arg2); 
                    //public static String Format(String format, params object?[] args);
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (target, deferExprs, args) =>
                    {
                        var parameters = new List<object>(args);
                        parameters.RemoveAt(0);
                        var result = args[0].ToString();
                        var concatIndices = new List<int>();
                        var count = parameters.Count;
                        for (int i = 0; i < count; i++)
                        {
                            if (parameters[i] is SqlSegment sqlSegment && sqlSegment.IsConstantValue)
                            {
                                string strValue = null;
                                if (sqlSegment != SqlSegment.Null)
                                {
                                    if (sqlSegment.Value is IEnumerable enumerable && sqlSegment.Value is not string)
                                    {
                                        parameters.RemoveAt(i);
                                        int eleIndex = 0;
                                        foreach (var element in enumerable)
                                        {
                                            if (element is SqlSegment eleSegment && eleSegment.IsConstantValue)
                                            {
                                                strValue = eleSegment.ToString();
                                                result = result.Replace("{" + eleIndex + "}", strValue);
                                            }
                                            else concatIndices.Add(eleIndex);
                                            parameters.Add(element);
                                            eleIndex++;
                                        }
                                    }
                                    else
                                    {
                                        strValue = this.GetQuotedValue(sqlSegment);
                                        result = result.Replace("{" + i + "}", strValue);
                                    }
                                }
                            }
                            else concatIndices.Add(i);
                        }
                        if (concatIndices.Count > 0)
                        {
                            int index = 0;
                            int lastIndex = 0;
                            var concatParameters = new List<object>();
                            var formatSpan = result.AsSpan();
                            foreach (var concatIndex in concatIndices)
                            {
                                index = formatSpan.IndexOf('{');
                                if (index > 0)
                                {
                                    var concatParameter = formatSpan.Slice(0, index);
                                    concatParameters.Add(concatParameter.ToString());
                                }
                                concatParameters.Add(parameters[concatIndex]);
                                lastIndex = formatSpan.IndexOf('}') + 1;
                                formatSpan = formatSpan.Slice(lastIndex);
                            }
                            if (formatSpan.Length > 0)
                                concatParameters.Add(formatSpan.ToString());

                            //var methodInfo = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string).MakeArrayType() });
                            //this.TryGetMethodCallSqlFormatter(originalSegment, methodInfo, out var concatFormater);
                            //result = concatFormater.Invoke(null, null, concatParameters);
                        }
                        return result;
                    });
                    result = true;
                    break;
                case "Compare":
                case "CompareOrdinal":
                    //String.Compare  不区分大小写
                    //public static int Compare(String? strA, String? strB);
                    //public static int Compare(String? strA, String? strB, bool ignoreCase);
                    //public static int Compare(String? strA, String? strB, bool ignoreCase, CultureInfo? culture);
                    if (parameterInfos.Length >= 2)
                    {
                        methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (target, deferExprs, args) =>
                        {
                            var leftArgument = this.GetQuotedValue(args[0]);
                            var rightArgument = this.GetQuotedValue(args[1]);
                            return $"(CASE WHEN {leftArgument}={rightArgument} THEN 0 WHEN {leftArgument}>{rightArgument} THEN 1 ELSE -1 END)";
                        });
                        result = true;
                    }
                    break;
                case "CompareTo":
                    //各种类型都有CompareTo方法
                    //public int CompareTo(Boolean value);
                    //public int CompareTo(Int32 value);
                    //public int CompareTo(Double value);
                    //public int CompareTo(DateTime value);
                    //public int CompareTo(object? value);                       
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (target, deferExprs, args) =>
                    {
                        var leftArgument = this.GetQuotedValue(target);
                        var rightArgument = this.GetQuotedValue(args[0]);
                        return $"(CASE WHEN {leftArgument}={rightArgument} THEN 0 WHEN {leftArgument}>{rightArgument} THEN 1 ELSE -1 END)";
                    });
                    result = true;
                    break;
                case "Trim":
                    if (parameterInfos.Length == 0)
                    {
                        formatter = (target, deferExprs, args) =>
                        {
                            if (target is SqlSegment sqlSegment && !sqlSegment.IsConstantValue)
                                return $"LTRIM(RTRIM({target}))";
                            else return $"LTRIM(RTRIM('{target}'))";
                        };
                        methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                        result = true;
                    }
                    break;
                case "TrimStart":
                    if (parameterInfos.Length == 0)
                    {
                        formatter = (target, deferExprs, args) =>
                        {
                            if (target is SqlSegment sqlSegment && !sqlSegment.IsConstantValue)
                                return $"LTRIM({target})";
                            else return $"LTRIM('{target}')";
                        };
                        methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                        result = true;
                    }
                    break;
                case "TrimEnd":
                    if (parameterInfos.Length == 0)
                    {
                        formatter = (target, deferExprs, args) =>
                        {
                            if (target is SqlSegment sqlSegment && !sqlSegment.IsConstantValue)
                                return $"RTRIM({target})";
                            else return $"RTRIM('{target}')";
                        };
                        methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                        result = true;
                    }
                    break;
                case "ToUpper":
                    formatter = (target, deferExprs, args) =>
                    {
                        if (target is SqlSegment sqlSegment && !sqlSegment.IsConstantValue)
                            return $"UPPER({target})";
                        else return $"UPPER('{target}')";
                    };
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "ToLower":
                    formatter = (target, deferExprs, args) =>
                    {
                        if (target is SqlSegment sqlSegment && !sqlSegment.IsConstantValue)
                            return $"LOWER({target})";
                        else return $"LOWER('{target}')";
                    };
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "Equals":
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (target, deferExprs, args) =>
                    {
                        var leftTarget = this.GetQuotedValue(target);
                        var rightValue = this.GetQuotedValue(args[0]);
                        int notIndex = 0;

                        if (deferExprs != null && deferExprs.Count > 0)
                        {
                            while (deferExprs.TryPop(f => f.OperationType == OperationType.Not, out var deferrdExpr))
                            {
                                notIndex++;
                            }
                        }
                        string equalsString = notIndex % 2 > 0 ? "<>" : "=";
                        return $"{leftTarget}{equalsString}{rightValue}";
                    });
                    result = true;
                    break;
                case "StartsWith":
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (target, deferExprs, args) =>
                    {
                        var leftField = this.GetQuotedValue(target);
                        var rightValue = $"'{args[0]}%'";
                        int notIndex = 0;

                        if (deferExprs != null && deferExprs.Count > 0)
                        {
                            while (deferExprs.TryPop(f => f.OperationType == OperationType.Not, out var deferrdExpr))
                            {
                                notIndex++;
                            }
                        }
                        string notString = notIndex % 2 > 0 ? " NOT" : "";
                        return $"{leftField}{notString} LIKE {rightValue}";
                    });
                    result = true;
                    break;
                case "EndsWith":
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (target, deferExprs, args) =>
                    {
                        var leftField = this.GetQuotedValue(target);
                        var rightValue = $"'%{args[0]}'";
                        int notIndex = 0;

                        if (deferExprs != null && deferExprs.Count > 0)
                        {
                            while (deferExprs.TryPop(f => f.OperationType == OperationType.Not, out var deferrdExpr))
                            {
                                notIndex++;
                            }
                        }
                        string notString = notIndex % 2 > 0 ? " NOT" : "";
                        return $"{leftField}{notString} LIKE {rightValue}";
                    });
                    result = true;
                    break;
                case "Substring":
                    if (parameterInfos.Length > 1)
                        formatter = (target, deferExprs, args) => $"SUBSTR({target},{args[0]},{args[1]})";
                    else formatter = (target, deferExprs, args) => $"SUBSTR({target},{args[0]})";
                    result = true;
                    break;
                case "ToString":
                    if (methodInfo.IsStatic)
                        formatter = (target, deferExprs, args) => $"{args[0]}::text";
                    else formatter = (target, deferExprs, args) => $"{target}::text";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "Parse":
                case "TryParse":
                    formatter = (target, deferExprs, args) => $"CAST('{args[0]}' AS {this.CastTo(methodInfo.DeclaringType)})";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;

                case "ToBoolean":
                    formatter = (target, deferExprs, args) => $"{args[0]}::bool";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "ToByte":
                    formatter = (target, deferExprs, args) => $"({args[0]})::int2";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "ToChar":
                    formatter = (target, deferExprs, args) => $"SUBSTR(({args[0]})::char,1,1)";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "ToDateTime":
                    formatter = (target, deferExprs, args) => $"({args[0]})::timestamp";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "ToDecimal":
                    formatter = (target, deferExprs, args) => $"({args[0]})::numeric";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "ToDouble":
                    formatter = (target, deferExprs, args) => $"({args[0]})::float8";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "ToInt16":
                    formatter = (target, deferExprs, args) => $"({args[0]})::int2";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "ToInt32":
                    formatter = (target, deferExprs, args) => $"({args[0]})::int4";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "ToInt64":
                    formatter = (target, deferExprs, args) => $"({args[0]})::int8";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "ToSByte":
                    formatter = (target, deferExprs, args) => $"({args[0]})::int2";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "ToSingle":
                    formatter = (target, deferExprs, args) => $"({args[0]})::float4";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "ToUInt16":
                    formatter = (target, deferExprs, args) => $"({args[0]})::int2";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "ToUInt32":
                    formatter = (target, deferExprs, args) => $"({args[0]})::int4";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "ToUInt64":
                    formatter = (target, deferExprs, args) => $"({args[0]})::int8";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;

                case "Abs":
                    formatter = (target, deferExprs, args) => $"ABS({args[0]})";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "Sign":
                    formatter = (target, deferExprs, args) => $"SIGN({args[0]})";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "Floor":
                    formatter = (target, deferExprs, args) => $"FLOOR({args[0]})";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "Ceiling":
                    formatter = (target, deferExprs, args) => $"CEILING({args[0]})";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "Round":
                    if (parameterInfos.Length > 1 && parameterInfos[1].ParameterType == typeof(int))
                        formatter = (target, deferExprs, args) => $"ROUND({args[0]},{args[1]})";
                    formatter = (target, deferExprs, args) => $"ROUND({args[0]})";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "Exp":
                    formatter = (target, deferExprs, args) => $"EXP({args[0]})";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "Log":
                    formatter = (target, deferExprs, args) => $"LOG({args[0]})";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "Log10":
                    formatter = (target, deferExprs, args) => $"LOG10({args[0]})";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "Pow":
                    formatter = (target, deferExprs, args) => $"POW({args[0]},{args[1]})";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "Sqrt":
                    formatter = (target, deferExprs, args) => $"SQRT({args[0]})";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "Cos":
                    formatter = (target, deferExprs, args) => $"COS({args[0]})";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "Sin":
                    formatter = (target, deferExprs, args) => $"SIN({args[0]})";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "Tan":
                    formatter = (target, deferExprs, args) => $"TAN({args[0]})";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "Acos":
                    formatter = (target, deferExprs, args) => $"ACOS({args[0]})";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "Asin":
                    formatter = (target, deferExprs, args) => $"ASIN({args[0]})";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "Atan":
                    formatter = (target, deferExprs, args) => $"ATAN({args[0]})";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "Atan2":
                    formatter = (target, deferExprs, args) => $"ATAN2({args[0]},{args[1]})";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "Truncate":
                    formatter = (target, deferExprs, args) => $"TRUNC({args[0]},0)";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;

                default: formatter = null; result = false; break;
            }
            return result;
        }
        return true;
    }
}
