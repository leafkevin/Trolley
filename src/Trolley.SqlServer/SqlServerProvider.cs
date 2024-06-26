﻿using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.SqlServer;

public partial class SqlServerProvider : BaseOrmProvider
{
    private static Dictionary<object, Type> defaultMapTypes = new();
    private static Dictionary<Type, object> defaultDbTypes = new();
    private static Dictionary<Type, string> castTos = new();
    private static Dictionary<Type, ITypeHandler> defaultTypeHandlers = new();

    public override OrmProviderType OrmProviderType => OrmProviderType.SqlServer;
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
        defaultMapTypes[SqlDbType.Timestamp] = typeof(byte[]);
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
        defaultDbTypes[typeof(sbyte)] = SqlDbType.TinyInt;
        defaultDbTypes[typeof(short)] = SqlDbType.SmallInt;
        defaultDbTypes[typeof(ushort)] = SqlDbType.SmallInt;
        defaultDbTypes[typeof(int)] = SqlDbType.Int;
        defaultDbTypes[typeof(uint)] = SqlDbType.Int;
        defaultDbTypes[typeof(long)] = SqlDbType.BigInt;
        defaultDbTypes[typeof(ulong)] = SqlDbType.BigInt;
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
        defaultDbTypes[typeof(sbyte?)] = SqlDbType.TinyInt;
        defaultDbTypes[typeof(short?)] = SqlDbType.SmallInt;
        defaultDbTypes[typeof(ushort?)] = SqlDbType.SmallInt;
        defaultDbTypes[typeof(int?)] = SqlDbType.Int;
        defaultDbTypes[typeof(uint?)] = SqlDbType.Int;
        defaultDbTypes[typeof(long?)] = SqlDbType.BigInt;
        defaultDbTypes[typeof(ulong?)] = SqlDbType.BigInt;
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
        => new SqlConnection(connectionString);
    public override IDbDataParameter CreateParameter(string parameterName, object value)
        => new SqlParameter(parameterName, value);
    public override IDbDataParameter CreateParameter(string parameterName, object nativeDbType, object value)
    {
        var parameter = new SqlParameter(parameterName, (SqlDbType)nativeDbType);
        parameter.Value = value;
        return parameter;
    }
    //public override IQueryVisitor NewQueryVisitor(IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p", IDataParameterCollection dbParameters = null)
    //    => new SqlServerQueryVisitor(this, mapProvider, isParameterized, tableAsStart, parameterPrefix, dbParameters);
    //public override ICreateVisitor NewCreateVisitor(IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
    //    => new SqlServerCreateVisitor(this, mapProvider, isParameterized, tableAsStart, parameterPrefix);
    //public override IUpdateVisitor NewUpdateVisitor(IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
    //    => new SqlServerUpdateVisitor(this, mapProvider, isParameterized, tableAsStart, parameterPrefix);
    public override string GetTableName(string tableName) => "[" + tableName + "]";
    public override string GetFieldName(string fieldName) => "[" + fieldName + "]";
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
    public override string GetIdentitySql(Type entityType) => ";SELECT SCOPE_IDENTITY()";
    public override string CastTo(Type type, object value)
        => $"CAST({value} AS {castTos[type]})";
    public override bool TryGetDefaultTypeHandler(Type targetType, out ITypeHandler typeHandler)
        => defaultTypeHandlers.TryGetValue(targetType, out typeHandler);
    public override bool TryGetMyMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter)
    {
        var methodInfo = methodCallExpr.Method;
        var parameterInfos = methodInfo.GetParameters();
        int cacheKey = 0;
        switch (methodInfo.Name)
        {
            case "IsNull":
                cacheKey = HashCode.Combine(typeof(Sql), methodInfo);
                formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                {
                    var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                    var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[1] });
                    var targetArgument = visitor.GetQuotedValue(targetSegment);
                    var rightArgument = visitor.GetQuotedValue(rightSegment);
                    return targetSegment.Merge(rightSegment, $"ISNULL({targetArgument},{rightArgument})", false, false, false, true);
                });
                return true;
        }
        formatter = null;
        return false;
    }
}
