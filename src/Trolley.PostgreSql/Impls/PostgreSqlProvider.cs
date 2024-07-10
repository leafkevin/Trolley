﻿using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace Trolley.PostgreSql;

public partial class PostgreSqlProvider : BaseOrmProvider
{
    private static Dictionary<object, Type> defaultMapTypes = new();
    private static Dictionary<Type, object> defaultDbTypes = new();
    private static Dictionary<Type, string> castTos = new();
    private static Dictionary<Type, ITypeHandler> defaultTypeHandlers = new();

    public override OrmProviderType OrmProviderType => OrmProviderType.PostgreSql;
    public override Type NativeDbTypeType => typeof(NpgsqlDbType);
    static PostgreSqlProvider()
    {
        defaultMapTypes[NpgsqlDbType.Bit] = typeof(byte[]);
        defaultMapTypes[NpgsqlDbType.Boolean] = typeof(bool);
        defaultMapTypes[NpgsqlDbType.Smallint] = typeof(short);
        defaultMapTypes[NpgsqlDbType.Integer] = typeof(int);
        defaultMapTypes[NpgsqlDbType.Bigint] = typeof(long);
        defaultMapTypes[NpgsqlDbType.Real] = typeof(float);
        defaultMapTypes[NpgsqlDbType.Double] = typeof(double);
        defaultMapTypes[NpgsqlDbType.Money] = typeof(decimal);
        defaultMapTypes[NpgsqlDbType.Numeric] = typeof(decimal);
        defaultMapTypes[NpgsqlDbType.Varchar] = typeof(string);
        defaultMapTypes[NpgsqlDbType.Text] = typeof(string);
        defaultMapTypes[NpgsqlDbType.Json] = typeof(string);
        defaultMapTypes[NpgsqlDbType.Jsonb] = typeof(string);
        defaultMapTypes[NpgsqlDbType.Xml] = typeof(string);
        defaultMapTypes[NpgsqlDbType.Timestamp] = typeof(DateTime);
        defaultMapTypes[NpgsqlDbType.TimestampTz] = typeof(DateTimeOffset);
        defaultMapTypes[NpgsqlDbType.Date] = typeof(DateOnly);
        defaultMapTypes[NpgsqlDbType.Time] = typeof(TimeOnly);
        defaultMapTypes[NpgsqlDbType.Bytea] = typeof(byte[]);
        defaultMapTypes[NpgsqlDbType.Varbit] = typeof(byte[]);
        defaultMapTypes[NpgsqlDbType.Uuid] = typeof(Guid);
        defaultMapTypes[NpgsqlDbType.Hstore] = typeof(Dictionary<string, string>);

        defaultDbTypes[typeof(bool)] = NpgsqlDbType.Boolean;
        defaultDbTypes[typeof(sbyte)] = NpgsqlDbType.Smallint;
        defaultDbTypes[typeof(byte)] = NpgsqlDbType.Smallint;
        defaultDbTypes[typeof(short)] = NpgsqlDbType.Smallint;
        defaultDbTypes[typeof(ushort)] = NpgsqlDbType.Smallint;
        defaultDbTypes[typeof(int)] = NpgsqlDbType.Integer;
        defaultDbTypes[typeof(uint)] = NpgsqlDbType.Bigint;
        defaultDbTypes[typeof(long)] = NpgsqlDbType.Bigint;
        defaultDbTypes[typeof(ulong)] = NpgsqlDbType.Numeric;
        defaultDbTypes[typeof(float)] = NpgsqlDbType.Real;
        defaultDbTypes[typeof(double)] = NpgsqlDbType.Double;
        defaultDbTypes[typeof(decimal)] = NpgsqlDbType.Numeric;
        defaultDbTypes[typeof(string)] = NpgsqlDbType.Varchar;
        defaultDbTypes[typeof(DateTime)] = NpgsqlDbType.Timestamp;
        defaultDbTypes[typeof(DateTimeOffset)] = NpgsqlDbType.TimestampTz;
        defaultDbTypes[typeof(DateOnly)] = NpgsqlDbType.Date;
        defaultDbTypes[typeof(TimeOnly)] = NpgsqlDbType.Time;
        defaultDbTypes[typeof(Guid)] = NpgsqlDbType.Uuid;
        defaultDbTypes[typeof(byte[])] = NpgsqlDbType.Bytea;

        defaultDbTypes[typeof(bool?)] = NpgsqlDbType.Boolean;
        defaultDbTypes[typeof(sbyte?)] = NpgsqlDbType.Smallint;
        defaultDbTypes[typeof(byte?)] = NpgsqlDbType.Smallint;
        defaultDbTypes[typeof(short?)] = NpgsqlDbType.Smallint;
        defaultDbTypes[typeof(ushort?)] = NpgsqlDbType.Smallint;
        defaultDbTypes[typeof(int?)] = NpgsqlDbType.Integer;
        defaultDbTypes[typeof(uint?)] = NpgsqlDbType.Bigint;
        defaultDbTypes[typeof(long?)] = NpgsqlDbType.Bigint;
        defaultDbTypes[typeof(ulong?)] = NpgsqlDbType.Numeric;
        defaultDbTypes[typeof(float?)] = NpgsqlDbType.Real;
        defaultDbTypes[typeof(double?)] = NpgsqlDbType.Double;
        defaultDbTypes[typeof(decimal?)] = NpgsqlDbType.Numeric;
        defaultDbTypes[typeof(DateTime?)] = NpgsqlDbType.Timestamp;
        defaultDbTypes[typeof(DateTimeOffset?)] = NpgsqlDbType.TimestampTz;
        defaultDbTypes[typeof(DateOnly?)] = NpgsqlDbType.Date;
        defaultDbTypes[typeof(TimeOnly?)] = NpgsqlDbType.Time;
        defaultDbTypes[typeof(Guid?)] = NpgsqlDbType.Uuid;
        defaultDbTypes[typeof(byte[])] = NpgsqlDbType.Bytea;

        //PostgreSql支持数据类型，值为各自值|int.MinValue
        //如, int[]类型: int.MinValue | NpgsqlDbType.Integer
        defaultDbTypes[typeof(long[])] = NpgsqlDbType.BigIntRange;
        defaultDbTypes[typeof(bool[])] = NpgsqlDbType.Boolean | NpgsqlDbType.Range;
        defaultDbTypes[typeof(short[])] = NpgsqlDbType.Smallint | NpgsqlDbType.Range;
        defaultDbTypes[typeof(int[])] = NpgsqlDbType.Integer | NpgsqlDbType.Range;
        defaultDbTypes[typeof(float[])] = NpgsqlDbType.Real | NpgsqlDbType.Range;
        defaultDbTypes[typeof(double[])] = NpgsqlDbType.Double | NpgsqlDbType.Range;
        defaultDbTypes[typeof(decimal[])] = NpgsqlDbType.Numeric | NpgsqlDbType.Range;
        defaultDbTypes[typeof(DateOnly[])] = NpgsqlDbType.DateRange;
        defaultDbTypes[typeof(TimeOnly[])] = NpgsqlDbType.Time | NpgsqlDbType.Range;
        defaultDbTypes[typeof(TimeSpan[])] = NpgsqlDbType.Interval | NpgsqlDbType.Range;
        defaultDbTypes[typeof(string[])] = NpgsqlDbType.Varchar | NpgsqlDbType.Range;
        defaultDbTypes[typeof(DateTimeOffset[])] = NpgsqlDbType.TimestampTzRange;
        defaultDbTypes[typeof(Guid[])] = NpgsqlDbType.Uuid | NpgsqlDbType.Range;

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
        castTos[typeof(DateOnly)] = "DATE";
        castTos[typeof(TimeOnly)] = "TIME";

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
        castTos[typeof(DateOnly?)] = "DATE";
        castTos[typeof(TimeOnly?)] = "TIME";

        //添加默认的类型处理器
        var typeHandlerTypes = typeof(PostgreSqlProvider).Assembly.GetExportedTypes()
            .Where(f => typeof(ITypeHandler).IsAssignableFrom(f))
            .ToList();
        foreach (var typeHandlerType in typeHandlerTypes)
        {
            if (!typeHandlerType.IsClass) continue;
            typeHandlers.TryAdd(typeHandlerType, Activator.CreateInstance(typeHandlerType) as ITypeHandler);
        }

        defaultTypeHandlers[typeof(bool)] = typeHandlers[typeof(BooleanTypeHandler)];
        defaultTypeHandlers[typeof(bool?)] = typeHandlers[typeof(NullableBooleanTypeHandler)];
        defaultTypeHandlers[typeof(string)] = typeHandlers[typeof(NullableStringTypeHandler)];
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
        defaultTypeHandlers[typeof(DateTime)] = typeHandlers[typeof(PostgreSqlDateTimeTypeHandler)];
        defaultTypeHandlers[typeof(DateTime?)] = typeHandlers[typeof(PostgreSqlNullableDateTimeTypeHandler)];
        defaultTypeHandlers[typeof(DateTimeOffset)] = typeHandlers[typeof(PostgreSqlDateTimeOffsetTypeHandler)];
        defaultTypeHandlers[typeof(DateTimeOffset?)] = typeHandlers[typeof(PostgreSqlNullableDateTimeOffsetTypeHandler)];
        defaultTypeHandlers[typeof(DateOnly)] = typeHandlers[typeof(PostgreSqlDateOnlyTypeHandler)];
        defaultTypeHandlers[typeof(DateOnly?)] = typeHandlers[typeof(PostgreSqlNullableDateOnlyTypeHandler)];
        defaultTypeHandlers[typeof(TimeSpan)] = typeHandlers[typeof(PostgreSqlTimeSpanTypeHandler)];
        defaultTypeHandlers[typeof(TimeSpan?)] = typeHandlers[typeof(PostgreSqlNullableTimeSpanTypeHandler)];
        defaultTypeHandlers[typeof(TimeOnly)] = typeHandlers[typeof(PostgreSqlTimeOnlyTypeHandler)];
        defaultTypeHandlers[typeof(TimeOnly?)] = typeHandlers[typeof(PostgreSqlNullableTimeOnlyTypeHandler)];
        defaultTypeHandlers[typeof(Guid)] = typeHandlers[typeof(GuidTypeHandler)];
        defaultTypeHandlers[typeof(Guid?)] = typeHandlers[typeof(NullableGuidTypeHandler)];
    }
    public override IDbConnection CreateConnection(string connectionString)
        => new NpgsqlConnection(connectionString);
    public override IDbDataParameter CreateParameter(string parameterName, object value)
        => new NpgsqlParameter(parameterName, value);
    public override IDbDataParameter CreateParameter(string parameterName, object nativeDbType, object value)
    {
        var parameter = new NpgsqlParameter(parameterName, (NpgsqlDbType)nativeDbType);
        parameter.Value = value;
        return parameter;
    }
    public override string GetTableName(string tableName) => "\"" + tableName + "\"";
    public override string GetFieldName(string fieldName) => "\"" + fieldName + "\"";
    public override object GetNativeDbType(Type fieldType)
    {
        if (!defaultDbTypes.TryGetValue(fieldType, out var dbType))
            throw new Exception($"类型{fieldType.FullName}没有对应的NpgsqlTypes.NpgsqlDbType映射类型");
        return dbType;
    }
    public override Type MapDefaultType(object nativeDbType)
    {
        if (defaultMapTypes.TryGetValue(nativeDbType, out var result))
            return result;
        return typeof(object);
    }
    public override string CastTo(Type type, object value)
        => $"{value}::{castTos[type]}";
    public override bool TryGetDefaultTypeHandler(Type targetType, out ITypeHandler typeHandler)
        => defaultTypeHandlers.TryGetValue(targetType, out typeHandler);
    private static SqlSegment DoNothingSegment = new SqlSegment { Value = "DO NOTHING" };
    public override bool TryGetMyMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter)
    {
        var methodInfo = methodCallExpr.Method;
        var parameterInfos = methodInfo.GetParameters();
        var genericArgumentTypes = methodInfo.DeclaringType.GetGenericArguments();
        int cacheKey = 0;
        switch (methodInfo.Name)
        {
            case "DoNothing":
                if (genericArgumentTypes.Length == 1 && methodInfo.DeclaringType == typeof(IPostgreSqlCreateConflictDoUpdate<>).MakeGenericType(genericArgumentTypes[0]))
                {
                    cacheKey = HashCode.Combine(typeof(IPostgreSqlCreateConflictDoUpdate<>), methodInfo);
                    //.OnConflict(f => f.DoNothing })
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) => DoNothingSegment);
                    return true;
                }
                break;
            case "Excluded":
                if (genericArgumentTypes.Length == 1 && methodInfo.DeclaringType == typeof(IPostgreSqlCreateConflictDoUpdate<>).MakeGenericType(genericArgumentTypes[0]))
                {
                    cacheKey = HashCode.Combine(typeof(IPostgreSqlCreateConflictDoUpdate<>), methodInfo.GetGenericMethodDefinition());
                    //.OnConflict(x => x.Set(f => new { TotalAmount = f.TotalAmount + x.Excluded(f.TotalAmount) }) ... )
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var myVisitor = visitor as PostgreSqlCreateVisitor;
                        if (args[0] is not MemberExpression memberExpr)
                            throw new NotSupportedException($"不支持的表达式访问，类型{methodInfo.DeclaringType.FullName}.Excluded方法，只支持MemberAccess访问，如：.Set(f =&gt; new {{TotalAmount = x.Excluded(f.TotalAmount)}})");
                        if (!myVisitor.Tables[0].Mapper.TryGetMemberMap(memberExpr.Member.Name, out var memberMapper))
                            throw new MissingMemberException($"类{myVisitor.Tables[0].EntityType.FullName}未找到成员{memberExpr.Member.Name}");

                        var fieldName = $"EXCLUDED.{this.GetFieldName(memberMapper.FieldName)}";
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
            case "IsNull":
                cacheKey = HashCode.Combine(typeof(Sql), methodInfo.GetGenericMethodDefinition());
                formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                {
                    var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                    var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[1] });
                    var targetArgument = visitor.GetQuotedValue(targetSegment);
                    var rightArgument = visitor.GetQuotedValue(rightSegment);
                    return targetSegment.Merge(rightSegment, $"COALESCE({targetArgument},{rightArgument})", false, false, false, true);
                });
                return true;
        }
        formatter = null;
        return false;
    }
}