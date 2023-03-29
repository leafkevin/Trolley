using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

public partial class SqlServerProvider : BaseOrmProvider
{
    private static Func<string, IDbConnection> createNativeConnectonDelegate = null;
    private static Func<string, object, IDbDataParameter> createDefaultNativeParameterDelegate = null;
    private static Func<string, object, object, IDbDataParameter> createNativeParameterDelegate = null;
    private static ConcurrentDictionary<int, MemberAccessSqlFormatter> memberAccessSqlFormatterCahe = new();
    private static ConcurrentDictionary<int, MethodCallSqlFormatter> methodCallSqlFormatterCahe = new();
    private static Dictionary<Type, object> defaultDbTypes = new();
    private static Dictionary<int, object> nativeDbTypes = new();
    private static Dictionary<Type, string> castTos = new();

    public override DatabaseType DatabaseType => DatabaseType.SqlServer;
    public override string SelectIdentitySql => ";SELECT SCOPE_IDENTITY()";
    static SqlServerProvider()
    {
        var connectionType = Type.GetType("Microsoft.Data.SqlClient.SqlConnection, Microsoft.Data.SqlClient, Culture=neutral, PublicKeyToken=23ec7fc2d6eaa4a5");
        createNativeConnectonDelegate = BaseOrmProvider.CreateConnectionDelegate(connectionType);
        var dbTypeType = Type.GetType("System.Data.SqlDbType, System.Data.Common, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
        var dbParameterType = Type.GetType("Microsoft.Data.SqlClient.SqlParameter, Microsoft.Data.SqlClient, Culture=neutral, PublicKeyToken=23ec7fc2d6eaa4a5");
        var valuePropertyInfo = dbParameterType.GetProperty("Value");
        createDefaultNativeParameterDelegate = BaseOrmProvider.CreateDefaultParameterDelegate(dbParameterType);
        createNativeParameterDelegate = BaseOrmProvider.CreateParameterDelegate(dbTypeType, dbParameterType, valuePropertyInfo);


        defaultDbTypes[typeof(bool)] = Enum.ToObject(dbTypeType, 2);
        defaultDbTypes[typeof(sbyte)] = Enum.ToObject(dbTypeType, 20);
        defaultDbTypes[typeof(short)] = Enum.ToObject(dbTypeType, 16);
        defaultDbTypes[typeof(int)] = Enum.ToObject(dbTypeType, 8);
        defaultDbTypes[typeof(long)] = Enum.ToObject(dbTypeType, 0);
        defaultDbTypes[typeof(float)] = Enum.ToObject(dbTypeType, 13);
        defaultDbTypes[typeof(double)] = Enum.ToObject(dbTypeType, 6);
        defaultDbTypes[typeof(TimeSpan)] = Enum.ToObject(dbTypeType, 32);
        defaultDbTypes[typeof(DateTime)] = Enum.ToObject(dbTypeType, 4);
        defaultDbTypes[typeof(DateTimeOffset)] = Enum.ToObject(dbTypeType, 34);
        defaultDbTypes[typeof(string)] = Enum.ToObject(dbTypeType, 12);
        defaultDbTypes[typeof(byte)] = Enum.ToObject(dbTypeType, 20);
        defaultDbTypes[typeof(ushort)] = Enum.ToObject(dbTypeType, 16);
        defaultDbTypes[typeof(uint)] = Enum.ToObject(dbTypeType, 8);
        defaultDbTypes[typeof(ulong)] = Enum.ToObject(dbTypeType, 0);
        defaultDbTypes[typeof(Guid)] = Enum.ToObject(dbTypeType, 14);
        defaultDbTypes[typeof(decimal)] = Enum.ToObject(dbTypeType, 5);
        defaultDbTypes[typeof(byte[])] = Enum.ToObject(dbTypeType, 1);

        defaultDbTypes[typeof(bool?)] = Enum.ToObject(dbTypeType, 2);
        defaultDbTypes[typeof(sbyte?)] = Enum.ToObject(dbTypeType, 20);
        defaultDbTypes[typeof(short?)] = Enum.ToObject(dbTypeType, 16);
        defaultDbTypes[typeof(int?)] = Enum.ToObject(dbTypeType, 8);
        defaultDbTypes[typeof(long?)] = Enum.ToObject(dbTypeType, 0);
        defaultDbTypes[typeof(float?)] = Enum.ToObject(dbTypeType, 13);
        defaultDbTypes[typeof(double?)] = Enum.ToObject(dbTypeType, 6);
        defaultDbTypes[typeof(TimeSpan?)] = Enum.ToObject(dbTypeType, 32);
        defaultDbTypes[typeof(DateTime?)] = Enum.ToObject(dbTypeType, 4);
        defaultDbTypes[typeof(DateTimeOffset?)] = Enum.ToObject(dbTypeType, 34);
        defaultDbTypes[typeof(byte?)] = Enum.ToObject(dbTypeType, 20);
        defaultDbTypes[typeof(ushort?)] = Enum.ToObject(dbTypeType, 16);
        defaultDbTypes[typeof(uint?)] = Enum.ToObject(dbTypeType, 8);
        defaultDbTypes[typeof(ulong?)] = Enum.ToObject(dbTypeType, 0);
        defaultDbTypes[typeof(Guid?)] = Enum.ToObject(dbTypeType, 14);
        defaultDbTypes[typeof(decimal?)] = Enum.ToObject(dbTypeType, 5);

        nativeDbTypes[2] = Enum.ToObject(dbTypeType, 2);
        nativeDbTypes[20] = Enum.ToObject(dbTypeType, 20);
        nativeDbTypes[16] = Enum.ToObject(dbTypeType, 16);
        nativeDbTypes[8] = Enum.ToObject(dbTypeType, 8);
        nativeDbTypes[0] = Enum.ToObject(dbTypeType, 0);
        nativeDbTypes[13] = Enum.ToObject(dbTypeType, 13);
        nativeDbTypes[6] = Enum.ToObject(dbTypeType, 6);
        nativeDbTypes[32] = Enum.ToObject(dbTypeType, 32);
        nativeDbTypes[4] = Enum.ToObject(dbTypeType, 4);
        nativeDbTypes[34] = Enum.ToObject(dbTypeType, 34);
        nativeDbTypes[14] = Enum.ToObject(dbTypeType, 14);
        nativeDbTypes[5] = Enum.ToObject(dbTypeType, 5);


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
        castTos[typeof(TimeSpan)] = "TIME";
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
        castTos[typeof(TimeSpan?)] = "TIME";
        castTos[typeof(Guid?)] = "UNIQUEIDENTIFIER";
    } 
    public override IDbConnection CreateConnection(string connectionString)
        => createNativeConnectonDelegate.Invoke(connectionString);
    public override IDbDataParameter CreateParameter(string parameterName, object value)
        => createDefaultNativeParameterDelegate.Invoke(parameterName, value);
    public override IDbDataParameter CreateParameter(string parameterName, object nativeDbType, object value)
        => createNativeParameterDelegate.Invoke(parameterName, nativeDbType, value);
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
    public override object GetNativeDbType(int nativeDbType)
    {
        if (nativeDbTypes.TryGetValue(nativeDbType, out var result))
            return result;
        var dbTypeType = Type.GetType("System.Data.SqlDbType, System.Data.Common, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
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
        throw new Exception($"数值{nativeDbType}没有对应的System.Data.SqlDbType映射类型");
    }
    public override bool IsStringDbType(int nativeDbType)
    {
        if (nativeDbType == 3 || nativeDbType == 10 || nativeDbType == 11 || nativeDbType == 12 || nativeDbType == 18 || nativeDbType == 22)
            return true;
        return false;
    }
    public override string CastTo(Type type)
    {
        if (castTos.TryGetValue(type, out var dbType))
            return dbType;
        return type.ToString().ToLower();
    }
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
                            var rightSegment = visitor.VisitAndDeferred(target);

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
                                return targetSegment.Change(this.GetQuotedValue(targetSegment.ToString()));
                            return targetSegment.Change($"CAST({this.GetQuotedValue(targetSegment)} AS {this.CastTo(typeof(string))})", false, true);
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
                            return args[0].Change($"CAST({this.GetQuotedValue(args[0])} AS {this.CastTo(methodInfo.DeclaringType)})", false, true);
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
