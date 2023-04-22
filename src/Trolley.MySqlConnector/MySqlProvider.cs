using MySqlConnector;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace Trolley;

public partial class MySqlProvider : BaseOrmProvider
{
    private static ConcurrentDictionary<int, MemberAccessSqlFormatter> memberAccessSqlFormatterCahe = new();
    private static ConcurrentDictionary<int, MethodCallSqlFormatter> methodCallSqlFormatterCahe = new();
    private static Dictionary<object, Type> defaultMapTypes = new();
    private static Dictionary<Type, object> defaultDbTypes = new();
    private static Dictionary<Type, string> castTos = new();

    public override DatabaseType DatabaseType => DatabaseType.MySql;
    public override string SelectIdentitySql => " RETURNING {0}";
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
        defaultMapTypes[MySqlDbType.Date] = typeof(DateTime);
        defaultMapTypes[MySqlDbType.DateTime] = typeof(DateTime);
        defaultMapTypes[MySqlDbType.Newdate] = typeof(DateTime);
        defaultMapTypes[MySqlDbType.Timestamp] = typeof(DateTime);
        defaultMapTypes[MySqlDbType.Time] = typeof(TimeSpan);
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
        defaultDbTypes[typeof(TimeSpan)] = MySqlDbType.Time;
        defaultDbTypes[typeof(DateTime)] = MySqlDbType.DateTime;
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
        defaultDbTypes[typeof(TimeSpan?)] = MySqlDbType.Time;
        defaultDbTypes[typeof(DateTime?)] = MySqlDbType.DateTime;
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
    public override string GetQuotedValue(Type expectType, object value)
    {
        if (expectType == typeof(TimeSpan) && value is TimeSpan timeSpan)
            return $"TIME('{timeSpan.ToString("d\\ hh\\:mm\\:ss\\.fffffff")}')";
        if (expectType == typeof(TimeOnly) && value is TimeOnly timeOnly)
            return $"TIME('{timeOnly.ToString("hh\\:mm\\:ss\\.fffffff")}')";
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
                return true; if (this.TryGetIEnumerableMethodCallSqlFormatter(methodCallExpr, out formatter))
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
