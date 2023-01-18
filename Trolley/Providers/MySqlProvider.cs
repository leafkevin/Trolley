using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

namespace Trolley;

public class MySqlProvider : BaseOrmProvider
{
    private static CreateNativeDbConnectionDelegate createNativeConnectonDelegate = null;
    private static CreateDefaultNativeParameterDelegate createDefaultNativeParameterDelegate = null;
    private static CreateNativeParameterDelegate createNativeParameterDelegate = null;
    private static ConcurrentDictionary<MemberInfo, MemberAccessSqlFormatter> memberAccessSqlFormatterCahe = new();
    private static ConcurrentDictionary<MethodInfo, MethodCallSqlFormatter> methodCallSqlFormatterCahe = new();
    private static Dictionary<Type, int> nativeDbTypes = new();
    private static Dictionary<Type, string> castTos = new();

    public override DatabaseType DatabaseType => DatabaseType.MySql;
    public override string SelectIdentitySql => " RETURNING {0}";

    public MySqlProvider()
    {
        var connectionType = Type.GetType("MySqlConnector.MySqlConnection, MySqlConnector, Version=2.0.0.0, Culture=neutral, PublicKeyToken=d33d3e53aa5f8c92");
        createNativeConnectonDelegate = base.CreateConnectionDelegate(connectionType);
        var dbTypeType = Type.GetType("MySqlConnector.MySqlDbType, MySqlConnector, Version=2.0.0.0, Culture=neutral, PublicKeyToken=d33d3e53aa5f8c92");
        var dbParameterType = Type.GetType("MySqlConnector.MySqlParameter, MySqlConnector, Version=2.0.0.0, Culture=neutral, PublicKeyToken=d33d3e53aa5f8c92");
        var dbTypePropertyInfo = dbParameterType.GetProperty("MySqlDbType");
        createDefaultNativeParameterDelegate = base.CreateDefaultParameterDelegate(dbParameterType);
        createNativeParameterDelegate = base.CreateParameterDelegate(dbTypeType, dbParameterType, dbTypePropertyInfo);

        nativeDbTypes[typeof(bool)] = -1;
        nativeDbTypes[typeof(sbyte)] = 1;
        nativeDbTypes[typeof(short)] = 2;
        nativeDbTypes[typeof(int)] = 3;
        nativeDbTypes[typeof(long)] = 8;
        nativeDbTypes[typeof(float)] = 4;
        nativeDbTypes[typeof(double)] = 5;
        nativeDbTypes[typeof(TimeSpan)] = 11;
        nativeDbTypes[typeof(DateTime)] = 12;
        nativeDbTypes[typeof(string)] = 253;
        nativeDbTypes[typeof(byte)] = 501;
        nativeDbTypes[typeof(ushort)] = 502;
        nativeDbTypes[typeof(uint)] = 503;
        nativeDbTypes[typeof(ulong)] = 508;
        nativeDbTypes[typeof(Guid)] = 253;
        nativeDbTypes[typeof(decimal)] = 0;
        nativeDbTypes[typeof(byte[])] = 601;

        nativeDbTypes[typeof(bool?)] = -1;
        nativeDbTypes[typeof(sbyte?)] = 1;
        nativeDbTypes[typeof(short?)] = 2;
        nativeDbTypes[typeof(int?)] = 3;
        nativeDbTypes[typeof(long?)] = 8;
        nativeDbTypes[typeof(float?)] = 4;
        nativeDbTypes[typeof(double?)] = 5;
        nativeDbTypes[typeof(TimeSpan?)] = 11;
        nativeDbTypes[typeof(DateTime?)] = 12;
        nativeDbTypes[typeof(string)] = 253;
        nativeDbTypes[typeof(byte?)] = 501;
        nativeDbTypes[typeof(ushort?)] = 502;
        nativeDbTypes[typeof(uint?)] = 503;
        nativeDbTypes[typeof(ulong?)] = 508;
        nativeDbTypes[typeof(Guid?)] = 253;
        nativeDbTypes[typeof(decimal?)] = 0;


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

        memberAccessSqlFormatterCahe.TryAdd(typeof(string).GetMember(nameof(string.Empty))[0], target => "''");
        memberAccessSqlFormatterCahe.TryAdd(typeof(string).GetProperty(nameof(string.Length)), target => $"CHAR_LENGTH({this.GetQuotedValue(target)})");

        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetMember(nameof(DateTime.Now))[0], target => "NOW()");
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetMember(nameof(DateTime.UtcNow))[0], target => "UTC_TIMESTAMP()");
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetMember(nameof(DateTime.Today))[0], target => "CURDATE()");
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetMember(nameof(DateTime.MinValue))[0], target => "'1753-01-01 00:00:00'");
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetMember(nameof(DateTime.MaxValue))[0], target => "'9999-12-31 23:59:59'");

        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.Date)), target => $"CAST(DATE_FORMAT({this.GetQuotedValue(target)},'%Y-%m-%d') AS DATETIME)");
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.Day)), target => $"DAYOFMONTH({this.GetQuotedValue(target)})");
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.DayOfWeek)), target => $"(DAYOFWEEK({this.GetQuotedValue(target)})-1)");
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.DayOfYear)), target => $"DAYOFYEAR({this.GetQuotedValue(target)})");
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.Hour)), target => $"HOUR({this.GetQuotedValue(target)})");
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.Millisecond)), target => $"FLOOR(MICROSECOND({this.GetQuotedValue(target)})/1000)");
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.Minute)), target => $"MINUTE({this.GetQuotedValue(target)})");
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.Month)), target => $"MONTH({this.GetQuotedValue(target)})");
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.Second)), target => $"SECOND({this.GetQuotedValue(target)})");
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.Ticks)), target => $"(TIMESTAMPDIFF(MICROSECOND, '0001-1-1', {this.GetQuotedValue(target)})*10");
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.TimeOfDay)), target => $"TIMESTAMPDIFF(MICROSECOND, DATE_FORMAT({this.GetQuotedValue(target)},'%Y-%m-%d'), {this.GetQuotedValue(target)})");
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.Year)), target => $"YEAR({this.GetQuotedValue(target)})");

        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetMember(nameof(TimeSpan.Zero))[0], target => "0");
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetMember(nameof(TimeSpan.MinValue))[0], target => "-922337203685477580");
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetMember(nameof(TimeSpan.MaxValue))[0], target => "922337203685477580");

        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetProperty(nameof(TimeSpan.Days)), target => $"(({this.GetQuotedValue(target)}) DIV {(long)1000000 * 60 * 60 * 24})");
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetProperty(nameof(TimeSpan.Hours)), target => $"(({this.GetQuotedValue(target)}) DIV {(long)1000000 * 60 * 60} MOD 24)");
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetProperty(nameof(TimeSpan.Milliseconds)), target => $"(({this.GetQuotedValue(target)}) DIV 1000 MOD 1000)");
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetProperty(nameof(TimeSpan.Minutes)), target => $"(({this.GetQuotedValue(target)}) DIV {(long)1000000 * 60} MOD 60)");
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetProperty(nameof(TimeSpan.Seconds)), target => $"(({this.GetQuotedValue(target)}) DIV 1000000 MOD 60)");
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetProperty(nameof(TimeSpan.Ticks)), target => $"(({this.GetQuotedValue(target)}) * 10)");
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetProperty(nameof(TimeSpan.TotalDays)), target => $"(({this.GetQuotedValue(target)}) / {(long)1000000 * 60 * 60 * 24})");
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetProperty(nameof(TimeSpan.TotalHours)), target => $"(({this.GetQuotedValue(target)}) / {(long)1000000 * 60 * 60})");
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetProperty(nameof(TimeSpan.TotalMilliseconds)), target => $"(({this.GetQuotedValue(target)}) / 1000)");
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetProperty(nameof(TimeSpan.TotalMinutes)), target => $"(({this.GetQuotedValue(target)}) / {(long)1000000 * 60})");
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetProperty(nameof(TimeSpan.TotalSeconds)), target => $"(({this.GetQuotedValue(target)}) / 1000000)");
    }
    public override IDbConnection CreateConnection(string connectionString)
        => createNativeConnectonDelegate.Invoke(connectionString);
    public override IDbDataParameter CreateParameter(string parameterName, object value)
        => createDefaultNativeParameterDelegate.Invoke(parameterName, value);
    public override IDbDataParameter CreateParameter(string parameterName, int nativeDbType, object value)
        => createNativeParameterDelegate.Invoke(parameterName, nativeDbType, value);
    public override string GetTableName(string entityName) => "`" + entityName + "`";
    public override string GetFieldName(string propertyName) => "`" + propertyName + "`";
    public override int GetNativeDbType(Type type)
    {
        if (nativeDbTypes.TryGetValue(type, out var dbType))
            return dbType;
        return 0;
    }
    public override string CastTo(Type type)
    {
        if (castTos.TryGetValue(type, out var dbType))
            return dbType;
        return type.ToString().ToLower();
    }
    public override bool TryGetMemberAccessSqlFormatter(SqlSegment originalSegment, MemberInfo memberInfo, out MemberAccessSqlFormatter formatter)
        => memberAccessSqlFormatterCahe.TryGetValue(memberInfo, out formatter);
    public override bool TryGetMethodCallSqlFormatter(SqlSegment originalSegment, MethodInfo methodInfo, out MethodCallSqlFormatter formatter)
    {
        if (!methodCallSqlFormatterCahe.TryGetValue(methodInfo, out formatter))
        {
            bool result = false;
            var parameterInfos = methodInfo.GetParameters();
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
                    if (!methodInfo.IsStatic && parameterInfos.Length >= 1 && typeof(string).IsAssignableFrom(methodInfo.DeclaringType))
                    {
                        methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (target, deferExprs, args) =>
                        {
                            var leftField = this.GetQuotedValue(target);
                            var rightValue = $"'%{args[0]}%'";

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
                        foreach (var arg in args)
                        {
                            if (arg is IEnumerable enumerable && arg is not string)
                            {
                                foreach (var element in enumerable)
                                {
                                    if (builder.Length > 0)
                                        builder.Append(',');

                                    if (element is SqlSegment sqlSegment && !sqlSegment.IsConstantValue)
                                        builder.Append(sqlSegment);
                                    else builder.Append(this.GetQuotedValue(typeof(string), element));
                                }
                            }
                            else
                            {
                                if (builder.Length > 0)
                                    builder.Append(',');
                                builder.Append(this.GetQuotedValue(arg));
                            }
                        }
                        if (builder.Length > 0)
                        {
                            builder.Insert(0, "CONCAT(");
                            builder.Append(')');
                        }
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

                            var methodInfo = typeof(string).GetMethod(nameof(string.Concat), new Type[] { typeof(string).MakeArrayType() });
                            this.TryGetMethodCallSqlFormatter(originalSegment, methodInfo, out var concatFormater);
                            result = concatFormater.Invoke(null, null, concatParameters);
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
                        formatter = (target, deferExprs, args) => $"SUBSTRING({target} FROM {(int)(args[0]) + 1} FOR {args[1]})";
                    else formatter = (target, deferExprs, args) => $"SUBSTRING({target} FROM {(int)(args[0]) + 1}";
                    result = true;
                    break;
                case "ToString":
                    if (methodInfo.IsStatic)
                    {
                        formatter = (target, deferExprs, args) =>
                        {
                            if (args[0] is SqlSegment sqlSegment && !sqlSegment.IsConstantValue)
                                return $"CAST({sqlSegment} AS {this.CastTo(typeof(string))})";
                            return args[0].ToString();
                        };
                    }
                    else
                    {
                        formatter = (target, deferExprs, args) =>
                        {
                            if (target is SqlSegment sqlSegment && !sqlSegment.IsConstantValue)
                                return $"CAST({sqlSegment} AS {this.CastTo(typeof(string))})";
                            return target.ToString();
                        };
                    }
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
                    formatter = (target, deferExprs, args) => $"CAST({args[0]} AS {this.CastTo(typeof(bool))})";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "ToByte":
                    formatter = (target, deferExprs, args) => $"CAST({args[0]} AS {this.CastTo(typeof(byte))})";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "ToChar":
                    formatter = (target, deferExprs, args) => $"CAST({args[0]} AS {this.CastTo(typeof(char))})";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "ToDateTime":
                    formatter = (target, deferExprs, args) => $"CAST({args[0]} AS {this.CastTo(typeof(DateTime))})";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "ToDouble":
                    formatter = (target, deferExprs, args) => $"CAST({args[0]} AS {this.CastTo(typeof(double))})";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "ToInt16":
                    formatter = (target, deferExprs, args) => $"CAST({args[0]} AS {this.CastTo(typeof(short))})";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "ToInt32":
                    formatter = (target, deferExprs, args) => $"CAST({args[0]} AS {this.CastTo(typeof(int))})";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "ToInt64":
                    formatter = (target, deferExprs, args) => $"CAST({args[0]} AS {this.CastTo(typeof(long))})";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "ToSByte":
                    formatter = (target, deferExprs, args) => $"CAST({args[0]} AS {this.CastTo(typeof(sbyte))})";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "ToSingle":
                    formatter = (target, deferExprs, args) => $"CAST({args[0]} AS {this.CastTo(typeof(float))})";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "ToUInt16":
                    formatter = (target, deferExprs, args) => $"CAST({args[0]} AS {this.CastTo(typeof(ushort))})";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "ToUInt32":
                    formatter = (target, deferExprs, args) => $"CAST({args[0]} AS {this.CastTo(typeof(uint))})";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "ToUInt64":
                    formatter = (target, deferExprs, args) => $"CAST({args[0]} AS {this.CastTo(typeof(ulong))})";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "ToDecimal":
                    formatter = (target, deferExprs, args) => $"CAST({args[0]} AS {this.CastTo(typeof(decimal))})";
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
                    formatter = (target, deferExprs, args) => $"TRUNCATE({args[0]}, 0)";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;

                //DateTime方法
                case "DaysInMonth":
                    formatter = (target, deferExprs, args) => $"DAYOFMONTH(LAST_DAY(CONCAT({args[0]},'-',{args[1]},'-01')))";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "IsLeapYear":
                    formatter = (target, deferExprs, args) => $"(({args[0]})%4=0 AND ({args[0]})%100<>0 OR ({args[0]})%400=0)"; ;
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "ParseExact":
                case "TryParseExact":
                    formatter = (target, deferExprs, args) => $"CAST({args[0]} AS {this.CastTo(methodInfo.DeclaringType)})";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "Add":
                    formatter = (target, deferExprs, args) => $"DATE_ADD({args[0]},INTERVAL({args[1]}) MICROSECOND)";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "AddDays":
                    formatter = (target, deferExprs, args) => $"DATE_ADD({args[0]},INTERVAL({args[1]}) DAY)";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "AddHours":
                    formatter = (target, deferExprs, args) => $"DATE_ADD({args[0]},INTERVAL({args[1]}) HOUR)";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "AddMilliseconds":
                    formatter = (target, deferExprs, args) => $"DATE_ADD({args[0]},INTERVAL({args[1]})*1000 MICROSECOND)";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "AddMinutes":
                    formatter = (target, deferExprs, args) => $"DATE_ADD({args[0]},INTERVAL({args[1]}) MINUTE)";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "AddMonths":
                    formatter = (target, deferExprs, args) => $"DATE_ADD({args[0]},INTERVAL({args[1]}) MONTH)";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "AddSeconds":
                    formatter = (target, deferExprs, args) => $"DATE_ADD({args[0]},INTERVAL({args[1]}) SECOND)";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "AddTicks":
                    formatter = (target, deferExprs, args) => $"DATE_ADD({args[0]},INTERVAL({args[1]})/10 MICROSECOND)";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "AddYears":
                    formatter = (target, deferExprs, args) => $"DATE_ADD({args[0]},INTERVAL({args[1]}) YEAR)";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "Subtract":
                    switch (originalSegment.Expression.Type.FullName)
                    {
                        case "System.DateTime":
                            formatter = (target, deferExprs, args) => $"TIMESTAMPDIFF(MICROSECOND,{args[1]},{args[0]})";
                            methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                            result = true;
                            break;
                        case "System.TimeSpan":
                            formatter = (target, deferExprs, args) => $"DATE_SUB({args[0]},INTERVAL({args[1]}) MICROSECOND)";
                            methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                            result = true;
                            break;
                    }
                    break;
                default: formatter = null; result = false; break;
            }
            return result;
        }
        return true;
    }
}
