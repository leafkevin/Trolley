using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Data;
using System.Reflection;
using System.Text;
using Thea.Orm;
using Trolley.Interfaces;

namespace Trolley;.Providers;

public class NpgSqlProvider : BaseOrmProvider
{
    private static Func<string, IDbConnection> createNativeConnectonDelegate = null;
    private static Func<string, int, object, IDbDataParameter> createNativeParameterDelegate = null;
    private static ConcurrentDictionary<MethodInfo, MethodCallSqlFormatter> methodCallSqlFormatterCahe = new();

    public override string SelectIdentitySql => " RETURNING {0}";
    public NpgSqlProvider()
    {
        var connectionType = Type.GetType("Npgsql.NpgsqlConnection, Npgsql, Culture=neutral, PublicKeyToken=5d8b90d52f46fda7");
        createNativeConnectonDelegate = base.CreateConnectionDelegate(connectionType);
        var dbTypeType = Type.GetType("Npgsql.NpgsqlParameter, Npgsql, Culture=neutral, PublicKeyToken=5d8b90d52f46fda7");
        var dbParameterType = Type.GetType("Npgsql.NpgsqlParameter, Npgsql, Culture=neutral, PublicKeyToken=5d8b90d52f46fda7");
        var dbTypePropertyInfo = dbParameterType.GetProperty("NpgsqlDbType");
        createNativeParameterDelegate = base.CreateParameterDelegate(dbTypeType, dbParameterType, dbTypePropertyInfo);
    }

    public override IDbConnection CreateConnection(string connectionString)
        => createNativeConnectonDelegate.Invoke(connectionString);
    public override IDbDataParameter CreateParameter(string parameterName, object value)
    {
        var dbType = this.GetNativeDbType(value.GetType());
        return createNativeParameterDelegate.Invoke(parameterName, dbType, value);
    }
    public override string GetFieldName(string propertyName) => "\"" + propertyName + "\"";
    public override string GetTableName(string entityName) => "\"" + entityName + "\"";
    public override int GetNativeDbType(Type itemType)
    {
        var typeCode = Type.GetTypeCode(itemType);
        switch (typeCode)
        {
            case TypeCode.Boolean: return int.MinValue | 2;//boolean
            case TypeCode.Byte: return int.MinValue | 18;//smallint
            case TypeCode.SByte: return int.MinValue | 18;//smallint
            case TypeCode.Int16: return int.MinValue | 18;//smallint
            case TypeCode.UInt16: return int.MinValue | 9;//integer
            case TypeCode.Int32: return int.MinValue | 9;//integer
            case TypeCode.UInt32: return int.MinValue | 1;//bigint
            case TypeCode.Int64: return int.MinValue | 1;//bigint
            case TypeCode.UInt64: return int.MinValue | 8;//double TODO
            case TypeCode.Single: return int.MinValue | 17;//real
            case TypeCode.Double: return int.MinValue | 8;//double
            case TypeCode.Decimal: return int.MinValue | 13;//numeric
            case TypeCode.DateTime: return int.MinValue | 21;//timestamp
            case TypeCode.String: return int.MinValue | 22;//varchar
            default:
                if (itemType == typeof(DateTimeOffset)) return int.MinValue | 26;//timestamptz
                if (itemType == typeof(Guid)) return int.MinValue | 27;//uuid
                if (itemType == typeof(TimeSpan)) return int.MinValue | 20;//time
                return int.MinValue | 40;//unknown 
        }
    }
    public override bool TryGetMethodCallSqlFormatter(MethodInfo methodInfo, out MethodCallSqlFormatter formatter)
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
                    if (methodInfo.IsStatic && parameterInfos.Length >= 2 && parameterInfos[0].ParameterType.IsAssignableFrom(typeof(IEnumerable)))
                    {
                        //数组调用                        
                        methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (target, args) =>
                        {
                            var builder = new StringBuilder();
                            var enumerable = args[0] as IEnumerable;
                            foreach (var element in enumerable)
                            {
                                if (builder.Length > 0)
                                    builder.Append(",");
                                builder.Append(element);
                            }
                            var fieldName = args[1] as string;
                            if (builder.Length > 0)
                            {
                                builder.Insert(0, fieldName + " IN (");
                                builder.Append(")");
                            }
                            //TODO:如果数组没有数据，抛出异常
                            //else builder.Append(fieldName + " IN (NULL)");
                            return builder.ToString();
                        });
                        result = true;
                    }
                    //IEnumerable<T>,List<T>
                    //public bool Contains(T item);
                    if (!methodInfo.IsStatic && parameterInfos.Length == 1 && methodInfo.DeclaringType.IsAssignableFrom(typeof(IEnumerable)))
                    {
                        methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (target, args) =>
                        {
                            var builder = new StringBuilder();
                            var enumerable = target as IEnumerable;
                            foreach (var element in enumerable)
                            {
                                if (builder.Length > 0)
                                    builder.Append(",");
                                builder.Append(element);
                            }
                            var fieldName = args[0] as string;
                            if (builder.Length > 0)
                            {
                                builder.Insert(0, fieldName + " IN (");
                                builder.Append(")");
                            }
                            //TODO:如果数组没有数据，抛出异常
                            //else builder.Append(fieldName + " IN (NULL)");
                            return builder.ToString();
                        });
                        return true;
                    }
                    //String
                    //public bool Contains(char value);
                    //public bool Contains(char value, StringComparison comparisonType);
                    //public bool Contains(String value);
                    //public bool Contains(String value, StringComparison comparisonType);
                    if (!methodInfo.IsStatic && parameterInfos.Length == 1 && methodInfo.DeclaringType.IsAssignableFrom(typeof(string)))
                    {
                        methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (target, args) =>
                            $"{target} LIKE '%{args[0]}%'"
                        );
                        result = true;
                    }
                    break;
                case "Concat":
                    if (methodInfo.IsStatic && methodInfo.DeclaringType == typeof(string))
                    {
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
                        methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (target, args) =>
                        {
                            var builder = new StringBuilder();
                            foreach (var arg in args)
                            {
                                if (arg is IEnumerable enumerable && arg is not string)
                                {
                                    foreach (var element in enumerable)
                                    {
                                        if (builder.Length > 0)
                                            builder.Append(",");
                                        builder.Append(element);
                                    }
                                }
                                else builder.Append(arg);
                            }
                            if (builder.Length > 0)
                            {
                                builder.Insert(0, "CONCAT(");
                                builder.Append(")");
                            }
                            return builder.ToString();
                        });
                        result = true;
                    }
                    break;
                case "Compare":
                    //String.Compare  不区分大小写
                    if (methodInfo.IsStatic && parameterInfos.Length >= 2 && parameterInfos.Length <= 4)
                    {
                        //public static int Compare(String? strA, String? strB, bool ignoreCase, CultureInfo? culture);
                        //public static int Compare(String? strA, String? strB, bool ignoreCase);
                        //public static int Compare(String? strA, String? strB);
                        formatter = (target, args) => $"(CASE WHEN {args[0]}={args[1]} THEN 0 WHEN {args[0]}>{args[1]} THEN 1 ELSE -1 END)";
                        methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                        result = true;
                    }
                    break;
                case "CompareOrdinal":
                    if (methodInfo.IsStatic && parameterInfos.Length == 2)
                    {
                        //public static int CompareOrdinal(String? strA, String? strB);
                        formatter = (target, args) => $"(CASE WHEN {args[0]}={args[1]} THEN 0 WHEN {args[0]}>{args[1]} THEN 1 ELSE -1 END)";
                        methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                        result = true;
                    }
                    break;
                case "CompareTo":
                    if (!methodCallSqlFormatterCahe.TryGetValue(methodInfo, out formatter))
                    {
                        //public static int CompareOrdinal(String? strA, String? strB);
                        formatter = (target, args) => $"(CASE WHEN {target}={args[0]} THEN 0 WHEN {target}>{args[0]} THEN 1 ELSE -1 END)";
                        methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                        result = true;
                    }
                    break;
                case "Trim":
                    formatter = (target, args) => $"ltrim(rtrim({target}))";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "LTrim":
                    formatter = (target, args) => $"ltrim({target})";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "RTrim":
                    formatter = (target, args) => $"rtrim({target})";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "ToUpper":
                    formatter = (target, args) => $"upper({target})";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "ToLower":
                    formatter = (target, args) => $"lower({target})";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "Equals":
                    formatter = (target, args) => $"{target}={this.GetQuotedValue(args[0])}";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "StartsWith":
                    formatter = (target, args) => $"{target} LIKE '{args[0]}%'";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "EndsWith":
                    formatter = (target, args) => $"{target} LIKE '%{args[0]}'";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "Substring":
                    if (parameterInfos.Length > 1)
                        formatter = (target, args) => $"substring({target} from {(int)(args[0]) + 1} for {args[1]})";
                    else formatter = (target, args) => $"substring({target} from {(int)(args[0]) + 1}";
                    result = true;
                    break;
                case "ToString":
                    if (methodInfo.DeclaringType == typeof(string))
                        formatter = (target, args) => target.ToString();
                    else formatter = (target, args) => $"CAST({target} AS VARCHAR(1000))";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
            }
            return result;
        }
        formatter = null;
        return false;
    }
}

