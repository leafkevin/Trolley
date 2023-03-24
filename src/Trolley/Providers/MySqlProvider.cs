using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace Trolley;

public partial class MySqlProvider : BaseOrmProvider
{
    private static Func<string, IDbConnection> createNativeConnectonDelegate = null;
    private static Func<string, object, IDbDataParameter> createDefaultNativeParameterDelegate = null;
    private static Func<string, object, object, IDbDataParameter> createNativeParameterDelegate = null;
    private static ConcurrentDictionary<MemberInfo, MemberAccessSqlFormatter> memberAccessSqlFormatterCahe = new();
    private static ConcurrentDictionary<MethodInfo, MethodCallSqlFormatter> methodCallSqlFormatterCahe = new();
    private static Dictionary<Type, object> defaultDbTypes = new();
    private static Dictionary<int, object> nativeDbTypes = new();
    private static Dictionary<Type, string> castTos = new();

    public override DatabaseType DatabaseType => DatabaseType.MySql;
    public override string SelectIdentitySql => " RETURNING {0}";
    static MySqlProvider()
    {
        var connectionType = Type.GetType("MySqlConnector.MySqlConnection, MySqlConnector, Version=2.0.0.0, Culture=neutral, PublicKeyToken=d33d3e53aa5f8c92");
        createNativeConnectonDelegate = BaseOrmProvider.CreateConnectionDelegate(connectionType);
        var dbTypeType = Type.GetType("MySqlConnector.MySqlDbType, MySqlConnector, Version=2.0.0.0, Culture=neutral, PublicKeyToken=d33d3e53aa5f8c92");
        var dbParameterType = Type.GetType("MySqlConnector.MySqlParameter, MySqlConnector, Version=2.0.0.0, Culture=neutral, PublicKeyToken=d33d3e53aa5f8c92");
        var valuePropertyInfo = dbParameterType.GetProperty("Value");
        createDefaultNativeParameterDelegate = BaseOrmProvider.CreateDefaultParameterDelegate(dbParameterType);
        createNativeParameterDelegate = BaseOrmProvider.CreateParameterDelegate(dbTypeType, dbParameterType, valuePropertyInfo);


        defaultDbTypes[typeof(bool)] = Enum.ToObject(dbTypeType, -1);
        defaultDbTypes[typeof(sbyte)] = Enum.ToObject(dbTypeType, 1);
        defaultDbTypes[typeof(short)] = Enum.ToObject(dbTypeType, 2);
        defaultDbTypes[typeof(int)] = Enum.ToObject(dbTypeType, 3);
        defaultDbTypes[typeof(long)] = Enum.ToObject(dbTypeType, 8);
        defaultDbTypes[typeof(float)] = Enum.ToObject(dbTypeType, 4);
        defaultDbTypes[typeof(double)] = Enum.ToObject(dbTypeType, 5);
        defaultDbTypes[typeof(TimeSpan)] = Enum.ToObject(dbTypeType, 11);
        defaultDbTypes[typeof(DateTime)] = Enum.ToObject(dbTypeType, 12);
        defaultDbTypes[typeof(string)] = Enum.ToObject(dbTypeType, 253);
        defaultDbTypes[typeof(byte)] = Enum.ToObject(dbTypeType, 501);
        defaultDbTypes[typeof(ushort)] = Enum.ToObject(dbTypeType, 502);
        defaultDbTypes[typeof(uint)] = Enum.ToObject(dbTypeType, 503);
        defaultDbTypes[typeof(ulong)] = Enum.ToObject(dbTypeType, 508);
        defaultDbTypes[typeof(Guid)] = Enum.ToObject(dbTypeType, 253);
        defaultDbTypes[typeof(decimal)] = Enum.ToObject(dbTypeType, 0);
        defaultDbTypes[typeof(byte[])] = Enum.ToObject(dbTypeType, 601);

        defaultDbTypes[typeof(bool?)] = Enum.ToObject(dbTypeType, -1);
        defaultDbTypes[typeof(sbyte?)] = Enum.ToObject(dbTypeType, 1);
        defaultDbTypes[typeof(short?)] = Enum.ToObject(dbTypeType, 2);
        defaultDbTypes[typeof(int?)] = Enum.ToObject(dbTypeType, 3);
        defaultDbTypes[typeof(long?)] = Enum.ToObject(dbTypeType, 8);
        defaultDbTypes[typeof(float?)] = Enum.ToObject(dbTypeType, 4);
        defaultDbTypes[typeof(double?)] = Enum.ToObject(dbTypeType, 5);
        defaultDbTypes[typeof(TimeSpan?)] = Enum.ToObject(dbTypeType, 11);
        defaultDbTypes[typeof(DateTime?)] = Enum.ToObject(dbTypeType, 12);
        defaultDbTypes[typeof(string)] = Enum.ToObject(dbTypeType, 253);
        defaultDbTypes[typeof(byte?)] = Enum.ToObject(dbTypeType, 501);
        defaultDbTypes[typeof(ushort?)] = Enum.ToObject(dbTypeType, 502);
        defaultDbTypes[typeof(uint?)] = Enum.ToObject(dbTypeType, 503);
        defaultDbTypes[typeof(ulong?)] = Enum.ToObject(dbTypeType, 508);
        defaultDbTypes[typeof(Guid?)] = Enum.ToObject(dbTypeType, 253);
        defaultDbTypes[typeof(decimal?)] = Enum.ToObject(dbTypeType, 0);

        nativeDbTypes[-1] = Enum.ToObject(dbTypeType, -1);
        nativeDbTypes[1] = Enum.ToObject(dbTypeType, 1);
        nativeDbTypes[2] = Enum.ToObject(dbTypeType, 2);
        nativeDbTypes[3] = Enum.ToObject(dbTypeType, 3);
        nativeDbTypes[8] = Enum.ToObject(dbTypeType, 8);
        nativeDbTypes[4] = Enum.ToObject(dbTypeType, 4);
        nativeDbTypes[5] = Enum.ToObject(dbTypeType, 5);
        nativeDbTypes[11] = Enum.ToObject(dbTypeType, 11);
        nativeDbTypes[12] = Enum.ToObject(dbTypeType, 12);
        nativeDbTypes[253] = Enum.ToObject(dbTypeType, 253);
        nativeDbTypes[501] = Enum.ToObject(dbTypeType, 501);
        nativeDbTypes[502] = Enum.ToObject(dbTypeType, 502);
        nativeDbTypes[503] = Enum.ToObject(dbTypeType, 503);
        nativeDbTypes[508] = Enum.ToObject(dbTypeType, 508);
        nativeDbTypes[0] = Enum.ToObject(dbTypeType, 0);

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
    }
    public MySqlProvider()
    {
        memberAccessSqlFormatterCahe.TryAdd(typeof(string).GetMember(nameof(string.Empty))[0], target => new SqlSegment { Value = "''", IsConstantValue = true });
        memberAccessSqlFormatterCahe.TryAdd(typeof(string).GetProperty(nameof(string.Length)), target => target.Change($"CHAR_LENGTH({this.GetQuotedValue(target)})", false, true));

        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetMember(nameof(DateTime.Now))[0], target => new SqlSegment { Value = "NOW()", IsConstantValue = false, IsExpression = true });
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetMember(nameof(DateTime.UtcNow))[0], target => new SqlSegment { Value = "UTC_TIMESTAMP()", IsConstantValue = false, IsExpression = true });
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetMember(nameof(DateTime.Today))[0], target => new SqlSegment { Value = "CURDATE()", IsConstantValue = false, IsExpression = true });
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetMember(nameof(DateTime.MinValue))[0], target => new SqlSegment { Value = $"'{DateTime.MinValue:yyyy-MM-dd HH:mm:ss}'", IsConstantValue = false });
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetMember(nameof(DateTime.MaxValue))[0], target => new SqlSegment { Value = $"'{DateTime.MinValue:yyyy-MM-dd HH:mm:ss}'", IsConstantValue = false });

        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.Date)), target => target.Change($"CAST(DATE_FORMAT({this.GetQuotedValue(target)},'%Y-%m-%d') AS DATETIME)", false, true));
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.Day)), target => target.Change($"DAYOFMONTH({this.GetQuotedValue(target)})", false, true));
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.DayOfWeek)), target => target.Change($"(DAYOFWEEK({this.GetQuotedValue(target)})-1)", false, true));
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.DayOfYear)), target => target.Change($"DAYOFYEAR({this.GetQuotedValue(target)})", false, true));
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.Hour)), target => target.Change($"HOUR({this.GetQuotedValue(target)})", false, true));
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.Millisecond)), target => target.Change($"FLOOR(MICROSECOND({this.GetQuotedValue(target)})/1000)", false, true));
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.Minute)), target => target.Change($"MINUTE({this.GetQuotedValue(target)})", false, true));
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.Month)), target => target.Change($"MONTH({this.GetQuotedValue(target)})", false, true));
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.Second)), target => target.Change($"SECOND({this.GetQuotedValue(target)})", false, true));
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.Ticks)), target => target.Change($"(TIMESTAMPDIFF(MICROSECOND, '0001-1-1', {this.GetQuotedValue(target)})*10", false, true));
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.TimeOfDay)), target => target.Change($"TIMESTAMPDIFF(MICROSECOND, DATE_FORMAT({this.GetQuotedValue(target)},'%Y-%m-%d'), {this.GetQuotedValue(target)})", false, true));
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.Year)), target => target.Change($"YEAR({this.GetQuotedValue(target)})", false, true));

        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetMember(nameof(TimeSpan.Zero))[0], target => new SqlSegment { Value = "0", IsConstantValue = true });
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetMember(nameof(TimeSpan.MinValue))[0], target => new SqlSegment { Value = $"{long.MinValue}", IsConstantValue = true });
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetMember(nameof(TimeSpan.MaxValue))[0], target => new SqlSegment { Value = $"{long.MaxValue}", IsConstantValue = true });

        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetProperty(nameof(TimeSpan.Days)), target => target.Change($"(({this.GetQuotedValue(target)}) DIV {(long)1000000 * 60 * 60 * 24})", false, true));
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetProperty(nameof(TimeSpan.Hours)), target => target.Change($"(({this.GetQuotedValue(target)}) DIV {(long)1000000 * 60 * 60} MOD 24)", false, true));
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetProperty(nameof(TimeSpan.Milliseconds)), target => target.Change($"(({this.GetQuotedValue(target)}) DIV 1000 MOD 1000)", false, true));
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetProperty(nameof(TimeSpan.Minutes)), target => target.Change($"(({this.GetQuotedValue(target)}) DIV {(long)1000000 * 60} MOD 60)", false, true));
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetProperty(nameof(TimeSpan.Seconds)), target => target.Change($"(({this.GetQuotedValue(target)}) DIV 1000000 MOD 60)", false, true));
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetProperty(nameof(TimeSpan.Ticks)), target => target.Change($"(({this.GetQuotedValue(target)}) * 10)", false, true));
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetProperty(nameof(TimeSpan.TotalDays)), target => target.Change($"(({this.GetQuotedValue(target)}) / {(long)1000000 * 60 * 60 * 24})", false, true));
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetProperty(nameof(TimeSpan.TotalHours)), target => target.Change($"(({this.GetQuotedValue(target)}) / {(long)1000000 * 60 * 60})", false, true));
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetProperty(nameof(TimeSpan.TotalMilliseconds)), target => target.Change($"(({this.GetQuotedValue(target)}) / 1000)", false, true));
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetProperty(nameof(TimeSpan.TotalMinutes)), target => target.Change($"(({this.GetQuotedValue(target)}) / {(long)1000000 * 60})", false, true));
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetProperty(nameof(TimeSpan.TotalSeconds)), target => target.Change($"(({this.GetQuotedValue(target)}) / 1000000)", false, true));
    }
    public override IDbConnection CreateConnection(string connectionString)
        => createNativeConnectonDelegate.Invoke(connectionString);
    public override IDbDataParameter CreateParameter(string parameterName, object value)
        => createDefaultNativeParameterDelegate.Invoke(parameterName, value);
    public override IDbDataParameter CreateParameter(string parameterName, object nativeDbType, object value)
        => createNativeParameterDelegate.Invoke(parameterName, nativeDbType, value);
    public override string GetTableName(string entityName) => "`" + entityName + "`";
    public override string GetFieldName(string propertyName) => "`" + propertyName + "`";
    public override object GetNativeDbType(Type type)
    {
        if (!defaultDbTypes.TryGetValue(type, out var dbType))
            throw new Exception($"类型{type.FullName}没有对应的MySqlConnector.MySqlDbType映射类型");
        return dbType;
    }
    public override object GetNativeDbType(int nativeDbType)
    {
        if (nativeDbTypes.TryGetValue(nativeDbType, out var result))
            return result;
        var dbTypeType = Type.GetType("MySqlConnector.MySqlDbType, MySqlConnector, Version=2.0.0.0, Culture=neutral, PublicKeyToken=d33d3e53aa5f8c92");
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
        throw new Exception($"数值{nativeDbType}没有对应的MySqlConnector.MySqlDbType映射类型");
    }
    public override bool IsStringDbType(int nativeDbType)
    {
        if (nativeDbType == 15 || nativeDbType == 253 || nativeDbType == 254)
            return true;
        if (nativeDbType >= 749 && nativeDbType <= 752)
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
        => memberAccessSqlFormatterCahe.TryGetValue(memberExpr.Member, out formatter);
    public override bool TryGetMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter)
    {
        var methodInfo = methodCallExpr.Method;
        var parameterInfos = methodInfo.GetParameters();
        if (!methodCallSqlFormatterCahe.TryGetValue(methodInfo, out formatter))
        {
            bool result = false;
            if (methodInfo.DeclaringType == typeof(string) && this.TryGetStringMethodCallSqlFormatter(methodCallExpr, out formatter))
                return true;
            if (methodInfo.DeclaringType == typeof(DateTime) && this.TryGetDateTimeMethodCallSqlFormatter(methodCallExpr, out formatter))
                return true;
            if (this.TryGetIEnumerableMethodCallSqlFormatter(methodCallExpr, out formatter))
                return true;

            switch (methodInfo.Name)
            {
                case "Equals":
                    if (!methodInfo.IsStatic && parameterInfos.Length == 1)
                    {
                        methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            var rightSegment = visitor.VisitAndDeferred(target);
                            return targetSegment.Change($"{targetSegment}={rightSegment}", false, true);
                        });
                        result = true;
                    }
                    break;
                case "CompareTo":
                    if (!methodInfo.IsStatic && parameterInfos.Length == 1)
                    {
                        methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            var rightSegment = visitor.VisitAndDeferred(target);
                            return targetSegment.Change($"(CASE WHEN {targetSegment}={rightSegment} THEN 0 WHEN {targetSegment}>{rightSegment} THEN 1 ELSE -1 END)", false, true);
                        });
                        result = true;
                    }
                    break;
                case "ToString":
                    if (!methodInfo.IsStatic && parameterInfos.Length == 0)
                    {
                        methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            if (targetSegment.IsConstantValue)
                                return targetSegment.ChangeValue(targetSegment.ToString());
                            return targetSegment.Change($"CAST({targetSegment} AS {this.CastTo(methodInfo.DeclaringType)})", false, true);
                        });
                        result = true;
                    }
                    break;
                case "Parse":
                case "TryParse":
                    if (!methodInfo.IsStatic && parameterInfos.Length == 1)
                    {
                        methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                        {
                            args[0] = visitor.VisitAndDeferred(args[0]);
                            if (args[0].IsConstantValue)
                                return args[0].Change(this.GetQuotedValue(methodInfo.DeclaringType, args[0]));
                            return args[0].Change($"CAST({args[0]} AS {this.CastTo(methodInfo.DeclaringType)})", false, true);
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
