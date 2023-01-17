using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Trolley;

public class NpgSqlProvider : BaseOrmProvider
{
    private static CreateNativeDbConnectionDelegate createNativeConnectonDelegate = null;
    private static CreateDefaultNativeParameterDelegate createDefaultNativeParameterDelegate = null;
    private static CreateNativeParameterDelegate createNativeParameterDelegate = null;
    private static ConcurrentDictionary<MemberInfo, MemberAccessSqlFormatter> memberAccessSqlFormatterCahe = new();
    private static ConcurrentDictionary<MethodInfo, MethodCallSqlFormatter> methodCallSqlFormatterCahe = new();
    private static Dictionary<Type, int> nativeDbTypes = new();
    private static Dictionary<Type, string> castTos = new();

    public override DatabaseType DatabaseType => DatabaseType.Postgresql;
    public override string SelectIdentitySql => " RETURNING {0}";
    public NpgSqlProvider()
    {
        var connectionType = Type.GetType("Npgsql.NpgsqlConnection, Npgsql, Culture=neutral, PublicKeyToken=5d8b90d52f46fda7");
        createNativeConnectonDelegate = base.CreateConnectionDelegate(connectionType);
        var dbTypeType = Type.GetType("Npgsql.NpgsqlParameter, Npgsql, Culture=neutral, PublicKeyToken=5d8b90d52f46fda7");
        var dbParameterType = Type.GetType("Npgsql.NpgsqlParameter, Npgsql, Culture=neutral, PublicKeyToken=5d8b90d52f46fda7");
        var dbTypePropertyInfo = dbParameterType.GetProperty("NpgsqlDbType");
        createDefaultNativeParameterDelegate = base.CreateDefaultParameterDelegate(dbParameterType);
        createNativeParameterDelegate = base.CreateParameterDelegate(dbTypeType, dbParameterType, dbTypePropertyInfo);


        nativeDbTypes[typeof(bool)] = 2;
        nativeDbTypes[typeof(sbyte)] = 18;
        nativeDbTypes[typeof(byte)] = 18;
        nativeDbTypes[typeof(char)] = 6;
        nativeDbTypes[typeof(short)] = 18;
        nativeDbTypes[typeof(int)] = 9;
        nativeDbTypes[typeof(uint)] = 41;//NpgsqlDbType.Oid
        nativeDbTypes[typeof(long)] = 1;
        nativeDbTypes[typeof(ulong)] = 1;
        nativeDbTypes[typeof(float)] = 17;
        nativeDbTypes[typeof(double)] = 8;
        nativeDbTypes[typeof(TimeSpan)] = 20;
        nativeDbTypes[typeof(DateTime)] = 21;
        nativeDbTypes[typeof(string)] = 22;
        nativeDbTypes[typeof(Guid)] = 27;
        nativeDbTypes[typeof(decimal)] = 13;
        nativeDbTypes[typeof(byte[])] = 4;

        nativeDbTypes[typeof(bool?)] = 2;
        nativeDbTypes[typeof(sbyte?)] = 18;
        nativeDbTypes[typeof(byte?)] = 18;
        nativeDbTypes[typeof(char?)] = 6;
        nativeDbTypes[typeof(ushort?)] = 18;
        nativeDbTypes[typeof(short?)] = 18;
        nativeDbTypes[typeof(int?)] = 9;
        nativeDbTypes[typeof(uint?)] = 41;
        nativeDbTypes[typeof(long?)] = 1;
        nativeDbTypes[typeof(ulong?)] = 1;
        nativeDbTypes[typeof(float?)] = 17;
        nativeDbTypes[typeof(double?)] = 8;
        nativeDbTypes[typeof(TimeSpan?)] = 20;
        nativeDbTypes[typeof(DateTime?)] = 21;
        nativeDbTypes[typeof(Guid?)] = 27;
        nativeDbTypes[typeof(decimal?)] = 13;

        //Npgsql支持数据类型，值为各自值|int.MinValue
        //如, int[]类型: int.MinValue | NpgsqlDbType.Integer

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

        memberAccessSqlFormatterCahe.TryAdd(typeof(string).GetMember(nameof(string.Empty))[0], target => "''");
        memberAccessSqlFormatterCahe.TryAdd(typeof(string).GetProperty(nameof(string.Length)), target => $"LENGTH({this.GetQuotedValue(target)})");

        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetMember(nameof(DateTime.Now))[0], target => "NOW()");
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetMember(nameof(DateTime.UtcNow))[0], target => "NOW() at time zone 'utc'");
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetMember(nameof(DateTime.Today))[0], target => "CURRENT_DATE");
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetMember(nameof(DateTime.MinValue))[0], target => "'0001-01-01 00:00:00'");
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetMember(nameof(DateTime.MaxValue))[0], target => "'9999-12-31 23:59:59'");

        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.Date)), target => $"({this.GetQuotedValue(target)})::DATE");
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.TimeOfDay)), target => $"(EXTRACT(EPOCH FROM({this.GetQuotedValue(target)})::TIME)*1000000)");
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.DayOfWeek)), target => $"EXTRACT(DOW FROM({this.GetQuotedValue(target)})::TIMESTAMP)");
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.Day)), target => $"EXTRACT(DAY FROM({this.GetQuotedValue(target)})::TIMESTAMP)");
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.DayOfYear)), target => $"EXTRACT(DOY FROM({this.GetQuotedValue(target)})::TIMESTAMP)");
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.Month)), target => $"EXTRACT(MONTH FROM({this.GetQuotedValue(target)})::TIMESTAMP)");
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.Year)), target => $"EXTRACT(YEAR FROM({this.GetQuotedValue(target)})::TIMESTAMP)");
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.Hour)), target => $"EXTRACT(HOUR FROM({this.GetQuotedValue(target)})::TIMESTAMP)");
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.Minute)), target => $"EXTRACT(MINUTE FROM({this.GetQuotedValue(target)})::TIMESTAMP)");
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.Second)), target => $"EXTRACT(SECOND FROM({this.GetQuotedValue(target)})::TIMESTAMP)");
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.Millisecond)), target => $"(EXTRACT(MILLISECONDS FROM({this.GetQuotedValue(target)})::TIMESTAMP)-EXTRACT(SECOND FROM({this.GetQuotedValue(target)})::TIMESTAMP)*1000)");
        memberAccessSqlFormatterCahe.TryAdd(typeof(DateTime).GetProperty(nameof(DateTime.Ticks)), target => $"(EXTRACT(EPOCH FROM({this.GetQuotedValue(target)})::TIMESTAMP)*10000000+621355968000000000)");

        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetMember(nameof(TimeSpan.Zero))[0], target => "0");
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetMember(nameof(TimeSpan.MinValue))[0], target => "-922337203685477580");
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetMember(nameof(TimeSpan.MaxValue))[0], target => "922337203685477580");

        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetProperty(nameof(TimeSpan.Days)), target => $"floor(({this.GetQuotedValue(target)})/{(long)1000000 * 60 * 60 * 24})");
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetProperty(nameof(TimeSpan.Hours)), target => $"floor(({this.GetQuotedValue(target)})/{(long)1000000 * 60 * 60}%24)");
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetProperty(nameof(TimeSpan.Milliseconds)), target => $"(floor(({this.GetQuotedValue(target)})/1000)::int8%1000)");
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetProperty(nameof(TimeSpan.Minutes)), target => $"(floor(({this.GetQuotedValue(target)})/{(long)1000000 * 60})::int8%60)");
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetProperty(nameof(TimeSpan.Seconds)), target => $"(floor(({this.GetQuotedValue(target)})/1000000)::int8%60)");
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetProperty(nameof(TimeSpan.Ticks)), target => $"(({this.GetQuotedValue(target)})*10)");
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetProperty(nameof(TimeSpan.TotalDays)), target => $"(({this.GetQuotedValue(target)})/{(long)1000000 * 60 * 60 * 24})");
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetProperty(nameof(TimeSpan.TotalHours)), target => $"(({this.GetQuotedValue(target)})/{(long)1000000 * 60 * 60})");
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetProperty(nameof(TimeSpan.TotalMilliseconds)), target => $"(({this.GetQuotedValue(target)})/1000)");
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetProperty(nameof(TimeSpan.TotalMinutes)), target => $"(({this.GetQuotedValue(target)})/{(long)1000000 * 60})");
        memberAccessSqlFormatterCahe.TryAdd(typeof(TimeSpan).GetProperty(nameof(TimeSpan.TotalSeconds)), target => $"(({this.GetQuotedValue(target)})/1000000)");
    }

    public override IDbConnection CreateConnection(string connectionString)
        => createNativeConnectonDelegate.Invoke(connectionString);
    public override IDbDataParameter CreateParameter(string parameterName, object value)
        => createDefaultNativeParameterDelegate.Invoke(parameterName, value);
    public override IDbDataParameter CreateParameter(string parameterName, int nativeDbType, object value)
        => createNativeParameterDelegate.Invoke(parameterName, nativeDbType, value);
    public override string GetFieldName(string propertyName) => "\"" + propertyName + "\"";
    public override string GetTableName(string entityName) => "\"" + entityName + "\"";
    public override int GetNativeDbType(Type type)
    {
        if (nativeDbTypes.TryGetValue(type, out var dbType))
            return dbType;

        //数组类型
        if (typeof(IEnumerable).IsAssignableFrom(type))
        {
            var genericType = type.GetGenericArguments().FirstOrDefault();
            if (nativeDbTypes.TryGetValue(genericType, out dbType))
                return int.MinValue | dbType;
        }
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
                    if (!methodInfo.IsStatic && parameterInfos.Length >= 1 && methodInfo.DeclaringType.IsAssignableFrom(typeof(string)))
                    {
                        methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (target, deferExprs, args) =>
                        {
                            var leftField = this.GetQuotedValue(target);
                            string rightValue = null;
                            if (args[0] is SqlSegment rightSegment && rightSegment.IsParameter)
                            {
                                var concatMethodInfo = typeof(string).GetMethod("Concat", BindingFlags.Public | BindingFlags.Static, new Type[] { typeof(string), typeof(string), typeof(string) });
                                if (this.TryGetMethodCallSqlFormatter(originalSegment, concatMethodInfo, out var concatFormatter))
                                    //自己调用字符串连接，参数直接是字符串
                                    rightValue = concatFormatter.Invoke(null, deferExprs, "'%'", rightSegment.Value.ToString(), "'%'");
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
                        foreach (var arg in args)
                        {
                            if (arg is IEnumerable enumerable && arg is not string)
                            {
                                foreach (var element in enumerable)
                                {
                                    if (builder.Length > 0)
                                        builder.Append(" || ");

                                    //连接符是||，不是字符串类型，无法连接，需要转换
                                    if (element is SqlSegment sqlSegment && !sqlSegment.IsConstantValue)
                                    {
                                        if (sqlSegment.Expression.Type != typeof(string))
                                            builder.Append($"{sqlSegment.Value}::text");
                                        else builder.Append(sqlSegment.Value.ToString());
                                    }
                                    else builder.Append(this.GetQuotedValue(typeof(string), element));
                                }
                            }
                            else
                            {
                                if (builder.Length > 0)
                                    builder.Append(" || ");
                                builder.Append(this.GetQuotedValue(arg));
                            }
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
                    if (parameterInfos.Length >= 2 && parameterInfos.Length <= 4)
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
                    if (methodInfo.GetParameters().Length == 0)
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
                    else result = false;
                    break;
                case "TrimStart":
                    if (methodInfo.GetParameters().Length == 0)
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
                    else result = false;
                    break;
                case "TrimEnd":
                    if (methodInfo.GetParameters().Length == 0)
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
                    else result = false;
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
