using Microsoft.Data.SqlClient;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.SqlServer;

public partial class SqlServerProvider : BaseOrmProvider
{
    private static ConcurrentDictionary<int, MemberAccessSqlFormatter> memberAccessSqlFormatterCahe = new();
    private static ConcurrentDictionary<int, MethodCallSqlFormatter> methodCallSqlFormatterCahe = new();
    private static Dictionary<object, Type> defaultMapTypes = new();
    private static Dictionary<Type, object> defaultDbTypes = new();
    private static Dictionary<Type, string> castTos = new();

    public override DatabaseType DatabaseType => DatabaseType.SqlServer;
    public override string SelectIdentitySql => ";SELECT SCOPE_IDENTITY()";
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
        defaultMapTypes[SqlDbType.Date] = typeof(DateTime);
        defaultMapTypes[SqlDbType.SmallDateTime] = typeof(DateTime);
        defaultMapTypes[SqlDbType.DateTime] = typeof(DateTime);
        defaultMapTypes[SqlDbType.Timestamp] = typeof(DateTime);
        defaultMapTypes[SqlDbType.DateTime2] = typeof(DateTime);
        defaultMapTypes[SqlDbType.DateTimeOffset] = typeof(DateTimeOffset);
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
    public override object GetNativeDbType(Type type)
    {
        if (!defaultDbTypes.TryGetValue(type, out var dbType))
            throw new Exception($"类型{type.FullName}没有对应的System.Data.SqlDbType映射类型");
        return dbType;
    }
    public override Type MapDefaultType(object nativeDbType)
    {
        if (defaultMapTypes.TryGetValue(nativeDbType, out var result))
            return result;
        return typeof(object);
    }
    public override string GetQuotedValue(Type expectType, object value)
    {
        if (expectType == typeof(TimeSpan) && value is TimeSpan timeSpan)
            return $"'{timeSpan.ToString("d\\ hh\\:mm\\:ss\\.fffffff")}'";
        if (expectType == typeof(TimeOnly) && value is TimeOnly timeOnly)
            return $"'{timeOnly.ToString("hh\\:mm\\:ss\\.fffffff")}'";
        return base.GetQuotedValue(expectType, value);
    }
    public override string CastTo(Type type, object value)
        => $"CAST({value} AS {castTos[type]})";
    public override bool TryGetMemberAccessSqlFormatter(MemberExpression memberExpr, out MemberAccessSqlFormatter formatter)
    {
        var memberInfo = memberExpr.Member;
        var cacheKey = HashCode.Combine(memberInfo.DeclaringType, memberInfo);
        if (!memberAccessSqlFormatterCahe.TryGetValue(cacheKey, out formatter))
        {
            bool result = false;
            if (memberInfo.DeclaringType == typeof(DateTime) && this.TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out formatter))
                return true;
            if (memberInfo.DeclaringType == typeof(TimeSpan) && this.TryGetTimeSpanMemberAccessSqlFormatter(memberExpr, out formatter))
                return true;
            return result;
        }
        return true;
    }
    public override bool TryGetMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter)
    {
        var methodInfo = methodCallExpr.Method;
        var parameterInfos = methodInfo.GetParameters();
        var cacheKey = HashCode.Combine(methodInfo.DeclaringType, methodInfo);
        if (!methodCallSqlFormatterCahe.TryGetValue(cacheKey, out formatter))
        {
            bool result = false;
            if (methodInfo.DeclaringType == typeof(string) && this.TryGetStringMethodCallSqlFormatter(methodCallExpr, out formatter))
                return true;
            if (methodInfo.DeclaringType == typeof(DateTime) && this.TryGetDateTimeMethodCallSqlFormatter(methodCallExpr, out formatter))
                return true;
            if (methodInfo.DeclaringType == typeof(TimeSpan) && this.TryGetTimeSpanMethodCallSqlFormatter(methodCallExpr, out formatter))
                return true;
            if (methodInfo.DeclaringType == typeof(Convert) && this.TryGetConvertMethodCallSqlFormatter(methodCallExpr, out formatter))
                return true;
            if (this.TryGetIEnumerableMethodCallSqlFormatter(methodCallExpr, out formatter))
                return true;
            if (methodInfo.DeclaringType == typeof(Math) && this.TryGetMathMethodCallSqlFormatter(methodCallExpr, out formatter))
                return true;
            switch (methodInfo.Name)
            {
                case "Equals":
                    if (!methodInfo.IsStatic && parameterInfos.Length == 1)
                    {
                        methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            var rightSegment = visitor.VisitAndDeferred(target);
                            targetSegment.Merge(rightSegment);
                            return targetSegment.Change($"{this.GetQuotedValue(targetSegment)}={this.GetQuotedValue(rightSegment)}", false, true);
                        });
                        result = true;
                    }
                    break;
                case "Compare":
                    if (methodInfo.IsStatic && parameterInfos.Length == 2)
                    {
                        methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var leftSegment = visitor.VisitAndDeferred(args[0]);
                            var rightSegment = visitor.VisitAndDeferred(args[1]);

                            leftSegment.Merge(rightSegment);
                            return leftSegment.Change($"(CASE WHEN {this.GetQuotedValue(leftSegment)}={this.GetQuotedValue(rightSegment)} THEN 0 WHEN {this.GetQuotedValue(leftSegment)}>{this.GetQuotedValue(rightSegment)} THEN 1 ELSE -1 END)", false, true);
                        });
                        result = true;
                    }
                    break;
                case "CompareTo":
                    if (!methodInfo.IsStatic && parameterInfos.Length == 1)
                    {
                        methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            var rightSegment = visitor.VisitAndDeferred(args[0]);

                            targetSegment.Merge(rightSegment);
                            return targetSegment.Change($"(CASE WHEN {this.GetQuotedValue(targetSegment)}={this.GetQuotedValue(rightSegment)} THEN 0 WHEN {this.GetQuotedValue(targetSegment)}>{this.GetQuotedValue(rightSegment)} THEN 1 ELSE -1 END)", false, true);
                        });
                        result = true;
                    }
                    break;
                case "ToString":
                    if (!methodInfo.IsStatic && parameterInfos.Length == 0)
                    {
                        methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            if (targetSegment.IsConstantValue)
                                return targetSegment.Change(targetSegment.ToString());
                            return targetSegment.Change(this.CastTo(typeof(string), this.GetQuotedValue(targetSegment)), false, true);
                        });
                        result = true;
                    }
                    break;
                case "Parse":
                case "TryParse":
                    if (!methodInfo.IsStatic && parameterInfos.Length == 1)
                    {
                        methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            args[0] = visitor.VisitAndDeferred(args[0]);
                            if (args[0].IsConstantValue)
                                return args[0].Change(this.GetQuotedValue(methodInfo.DeclaringType, args[0]));
                            return args[0].Change(this.CastTo(methodInfo.DeclaringType, this.GetQuotedValue(args[0])), false, true);
                        });
                        result = true;
                    }
                    break;
                case "get_Item":
                    if (!methodInfo.IsStatic && parameterInfos.Length > 0)
                    {
                        methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            var isConstantValue = targetSegment.IsConstantValue;
                            for (int i = 0; i < args.Length; i++)
                            {
                                args[i] = visitor.VisitAndDeferred(args[i]);
                                isConstantValue = isConstantValue && args[i].IsConstantValue;
                                targetSegment.Merge(args[i]);
                            }
                            if (isConstantValue)
                                return targetSegment.Change(methodInfo.Invoke(targetSegment.Value, args.Select(f => f.Value).ToArray()));

                            throw new NotSupportedException($"不支持的方法调用,{methodInfo.DeclaringType.FullName}.{methodInfo.Name}");
                        });
                        result = true;
                    }
                    break;
            }
            return result;
        }
        return true;
    }
}
