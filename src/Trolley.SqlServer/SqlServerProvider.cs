using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Trolley.SqlServer;

public partial class SqlServerProvider : BaseOrmProvider
{
    private static Dictionary<object, Type> defaultMapTypes = new();
    private static Dictionary<Type, object> defaultDbTypes = new();
    private static Dictionary<Type, string> castTos = new();

    public override Type NativeDbTypeType => typeof(SqlDbType);
    static SqlServerProvider()
    {
        defaultMapTypes[SqlDbType.Bit] = typeof(bool);
        defaultMapTypes[SqlDbType.TinyInt] = typeof(byte);
        defaultMapTypes[SqlDbType.SmallInt] = typeof(short);
        defaultMapTypes[SqlDbType.Int] = typeof(int);
        defaultMapTypes[SqlDbType.BigInt] = typeof(long);
        defaultMapTypes[SqlDbType.Real] = typeof(float);
        defaultMapTypes[SqlDbType.Float] = typeof(double);
        defaultMapTypes[SqlDbType.Decimal] = typeof(decimal);
        defaultMapTypes[SqlDbType.Money] = typeof(decimal);
        defaultMapTypes[SqlDbType.SmallMoney] = typeof(decimal);
        defaultMapTypes[SqlDbType.Char] = typeof(string);
        defaultMapTypes[SqlDbType.NChar] = typeof(string);
        defaultMapTypes[SqlDbType.VarChar] = typeof(string);
        defaultMapTypes[SqlDbType.NVarChar] = typeof(string);
        defaultMapTypes[SqlDbType.Text] = typeof(string);
        defaultMapTypes[SqlDbType.NText] = typeof(string);
        defaultMapTypes[SqlDbType.SmallDateTime] = typeof(DateTime);
        defaultMapTypes[SqlDbType.DateTime] = typeof(DateTime);
        defaultMapTypes[SqlDbType.Timestamp] = typeof(DateTime);
        defaultMapTypes[SqlDbType.DateTime2] = typeof(DateTime);
        defaultMapTypes[SqlDbType.DateTimeOffset] = typeof(DateTimeOffset);
        defaultMapTypes[SqlDbType.Date] = typeof(DateOnly);
        defaultMapTypes[SqlDbType.Time] = typeof(TimeOnly);
        defaultMapTypes[SqlDbType.Image] = typeof(byte[]);
        defaultMapTypes[SqlDbType.Binary] = typeof(byte[]);
        defaultMapTypes[SqlDbType.VarBinary] = typeof(byte[]);
        defaultMapTypes[SqlDbType.UniqueIdentifier] = typeof(Guid);

        defaultDbTypes[typeof(bool)] = SqlDbType.Bit;
        defaultDbTypes[typeof(byte)] = SqlDbType.TinyInt;
        defaultDbTypes[typeof(short)] = SqlDbType.SmallInt;
        defaultDbTypes[typeof(int)] = SqlDbType.Int;
        defaultDbTypes[typeof(long)] = SqlDbType.BigInt;
        defaultDbTypes[typeof(float)] = SqlDbType.Real;
        defaultDbTypes[typeof(double)] = SqlDbType.Float;
        defaultDbTypes[typeof(decimal)] = SqlDbType.Decimal;
        defaultDbTypes[typeof(string)] = SqlDbType.NVarChar;
        defaultDbTypes[typeof(DateTime)] = SqlDbType.DateTime;
        defaultDbTypes[typeof(DateTimeOffset)] = SqlDbType.DateTimeOffset;
        defaultDbTypes[typeof(DateOnly)] = SqlDbType.Date;
        defaultDbTypes[typeof(TimeOnly)] = SqlDbType.Time;
        defaultDbTypes[typeof(byte[])] = SqlDbType.VarBinary;
        defaultDbTypes[typeof(Guid)] = SqlDbType.UniqueIdentifier;

        defaultDbTypes[typeof(bool?)] = SqlDbType.Bit;
        defaultDbTypes[typeof(byte?)] = SqlDbType.TinyInt;
        defaultDbTypes[typeof(short?)] = SqlDbType.SmallInt;
        defaultDbTypes[typeof(int?)] = SqlDbType.Int;
        defaultDbTypes[typeof(long?)] = SqlDbType.BigInt;
        defaultDbTypes[typeof(float?)] = SqlDbType.Real;
        defaultDbTypes[typeof(double?)] = SqlDbType.Float;
        defaultDbTypes[typeof(decimal?)] = SqlDbType.Decimal;
        defaultDbTypes[typeof(DateTime?)] = SqlDbType.DateTime;
        defaultDbTypes[typeof(DateTimeOffset?)] = SqlDbType.DateTimeOffset;
        defaultDbTypes[typeof(DateOnly?)] = SqlDbType.Date;
        defaultDbTypes[typeof(TimeOnly?)] = SqlDbType.Time;
        defaultDbTypes[typeof(Guid?)] = SqlDbType.UniqueIdentifier;


        castTos[typeof(string)] = "NVARCHAR(MAX)";
        castTos[typeof(byte)] = "TINYINT";
        castTos[typeof(sbyte)] = "TINYINT";
        castTos[typeof(short)] = "SMALLINT";
        castTos[typeof(ushort)] = "SMALLINT";
        castTos[typeof(int)] = "INT";
        castTos[typeof(uint)] = "INT";
        castTos[typeof(long)] = "BIGINT";
        castTos[typeof(ulong)] = "BIGINT";
        castTos[typeof(float)] = "REAL";
        castTos[typeof(double)] = "FLOAT";
        castTos[typeof(decimal)] = "DECIMAL(36,18)";
        castTos[typeof(bool)] = "BIT";
        castTos[typeof(DateTime)] = "DATETIME";
        castTos[typeof(DateOnly)] = "DATE";
        castTos[typeof(TimeOnly)] = "TIME";
        castTos[typeof(Guid)] = "UNIQUEIDENTIFIER";

        castTos[typeof(string)] = "NVARCHAR(MAX)";
        castTos[typeof(byte?)] = "TINYINT";
        castTos[typeof(sbyte?)] = "TINYINT";
        castTos[typeof(short?)] = "SMALLINT";
        castTos[typeof(ushort?)] = "SMALLINT";
        castTos[typeof(int?)] = "INT";
        castTos[typeof(uint?)] = "INT";
        castTos[typeof(long?)] = "BIGINT";
        castTos[typeof(ulong?)] = "BIGINT";
        castTos[typeof(float?)] = "REAL";
        castTos[typeof(double?)] = "FLOAT";
        castTos[typeof(decimal?)] = "DECIMAL(36,18)";
        castTos[typeof(bool?)] = "BIT";
        castTos[typeof(DateTime?)] = "DATETIME";
        castTos[typeof(DateOnly?)] = "DATE";
        castTos[typeof(TimeOnly?)] = "TIME";
        castTos[typeof(Guid?)] = "UNIQUEIDENTIFIER";
    }
    public override IDbConnection CreateConnection(string connectionString)
        => new SqlConnection(connectionString);
    public override IDbDataParameter CreateParameter(string parameterName, object value)
        => new SqlParameter(parameterName, value);
    public override IDbDataParameter CreateParameter(string parameterName, object nativeDbType, object value)
    {
        var parameter = new SqlParameter(parameterName, (SqlDbType)nativeDbType);
        parameter.Value = value;
        return parameter;
    }
    public override IQueryVisitor NewQueryVisitor(string dbKey, IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p", IDataParameterCollection dbParameters = null)
        => new SqlServerQueryVisitor(dbKey, this, mapProvider, isParameterized, tableAsStart, parameterPrefix, dbParameters);
    public override ICreateVisitor NewCreateVisitor(string dbKey, IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
        => new SqlServerCreateVisitor(dbKey, this, mapProvider, isParameterized, tableAsStart, parameterPrefix);
    public override IUpdateVisitor NewUpdateVisitor(string dbKey, IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
        => new SqlServerUpdateVisitor(dbKey, this, mapProvider, isParameterized, tableAsStart, parameterPrefix);

    public override string GetFieldName(string propertyName) => "[" + propertyName + "]";
    public override string GetTableName(string entityName) => "[" + entityName + "]";
    public override string GetPagingTemplate(int? skip, int? limit, string orderBy = null)
    {
        var builder = new StringBuilder("SELECT ");
        if (skip.HasValue && limit.HasValue)
        {
            if (string.IsNullOrEmpty(orderBy)) throw new ArgumentNullException("orderBy");
            builder.Append("/**fields**/ FROM /**tables**/ /**others**/");
            if (!String.IsNullOrEmpty(orderBy)) builder.Append($" {orderBy}");
            builder.Append($" OFFSET {skip} ROWS");
            builder.AppendFormat($" FETCH NEXT {limit} ROWS ONLY", limit);
        }
        else if (!skip.HasValue && limit.HasValue)
        {
            builder.Append($"TOP {limit} ");
            builder.Append("/**fields**/ FROM /**tables**/ /**others**/");
            if (!String.IsNullOrEmpty(orderBy)) builder.Append($" {orderBy}");
        }
        return builder.ToString();
    }
    public override object GetNativeDbType(Type fieldType)
    {
        if (!defaultDbTypes.TryGetValue(fieldType, out var dbType))
            throw new Exception($"类型{fieldType.FullName}没有对应的System.Data.SqlDbType映射类型");
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
        if (expectType == typeof(DateTime) && value is DateTime dateTime)
            return $"'{dateTime:yyyy-MM-dd HH:mm:ss.fff}'";
        if (expectType == typeof(TimeSpan) && value is TimeSpan timeSpan)
        {
            //在SELECT的场景才会用到
            if (timeSpan > TimeSpan.FromDays(1) || timeSpan < -TimeSpan.FromDays(1))
                return $"'{(int)timeSpan.TotalDays}.{timeSpan.ToString("hh\\:mm\\:ss\\.fff")}'";
            return $"'{timeSpan.ToString("hh\\:mm\\:ss\\.fff")}'";
        }
        if (expectType == typeof(TimeOnly) && value is TimeOnly timeOnly)
            return $"'{timeOnly.ToString("hh\\:mm\\:ss\\.fff")}'";
        return base.GetQuotedValue(expectType, value);
    }
}
