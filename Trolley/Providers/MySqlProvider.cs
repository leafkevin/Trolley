using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Trolley.Providers;

public class MySqlProvider : BaseOrmProvider
{
    private static CreateNativeDbConnectionDelegate createNativeConnectonDelegate = null;
    private static CreateNativeParameterDelegate createNativeParameterDelegate = null;
    private static ConcurrentDictionary<MemberInfo, MemberAccessSqlFormatter> memberAccessSqlFormatterCahe = new();
    private static ConcurrentDictionary<MethodInfo, MethodCallSqlFormatter> methodCallSqlFormatterCahe = new();
    private static Dictionary<Type, int> nativeDbTypes = new();
    private static Dictionary<Type, string> castTos = new();
    public override string SelectIdentitySql => ";SELECT LAST_INSERT_ID()";
    public MySqlProvider()
    {
        var connectionType = Type.GetType("MySqlConnector.MySqlConnection, MySqlConnector, Version=2.0.0.0, Culture=neutral, PublicKeyToken=d33d3e53aa5f8c92");
        createNativeConnectonDelegate = base.CreateConnectionDelegate(connectionType);
        var dbTypeType = Type.GetType("MySqlConnector.MySqlDbType, MySqlConnector, Version=2.0.0.0, Culture=neutral, PublicKeyToken=d33d3e53aa5f8c92");
        var dbParameterType = Type.GetType("MySqlConnector.MySqlParameter, MySqlConnector, Version=2.0.0.0, Culture=neutral, PublicKeyToken=d33d3e53aa5f8c92");
        var dbTypePropertyInfo = dbParameterType.GetProperty("MySqlDbType");
        createNativeParameterDelegate = base.CreateParameterDelegate(dbTypeType, dbParameterType, dbTypePropertyInfo);

        nativeDbTypes[typeof(bool)] = -1;
        nativeDbTypes[typeof(bool?)] = -1;
        nativeDbTypes[typeof(string)] = 253;
        nativeDbTypes[typeof(DateTime)] = 12;
        nativeDbTypes[typeof(DateTime?)] = 12;

        castTos[typeof(long)] = "bigint";
        castTos[typeof(long?)] = "bigint";
        castTos[typeof(short)] = "smallint";
        castTos[typeof(short?)] = "smallint";
        castTos[typeof(byte)] = "tinyint";
        castTos[typeof(byte?)] = "tinyint";

        methodCallSqlFormatterCahe.TryAdd(typeof(IAggregateSelect).GetMethod("Count", Type.EmptyTypes), (target, deferredExprs, arguments) => "COUNT(1)");
        methodCallSqlFormatterCahe.TryAdd(typeof(IAggregateSelect).GetMethod("LongCount", Type.EmptyTypes), (target, deferredExprs, arguments) => "COUNT(1)");
        methodCallSqlFormatterCahe.TryAdd(typeof(IAggregateSelect).GetMethod("Count", 1, new Type[] { Type.MakeGenericMethodParameter(0) }), (target, deferredExprs, arguments) => $"COUNT({arguments[0]})");
        methodCallSqlFormatterCahe.TryAdd(typeof(IAggregateSelect).GetMethod("CountDistinct", 1, new Type[] { Type.MakeGenericMethodParameter(0) }), (target, deferredExprs, arguments) => $"COUNT(DISTINCT {arguments[0]})");
        methodCallSqlFormatterCahe.TryAdd(typeof(IAggregateSelect).GetMethod("LongCount", 1, new Type[] { Type.MakeGenericMethodParameter(0) }), (target, deferredExprs, arguments) => $"COUNT({arguments[0]})");
        methodCallSqlFormatterCahe.TryAdd(typeof(IAggregateSelect).GetMethod("LongCountDistinct", 1, new Type[] { Type.MakeGenericMethodParameter(0) }), (target, deferredExprs, arguments) => $"COUNT(DISTINCT {arguments[0]})");
        methodCallSqlFormatterCahe.TryAdd(typeof(IAggregateSelect).GetMethod("Sum", 1, new Type[] { Type.MakeGenericMethodParameter(0) }), (target, deferredExprs, arguments) => $"SUM({arguments[0]})");
        methodCallSqlFormatterCahe.TryAdd(typeof(IAggregateSelect).GetMethod("SumAs", 2, new Type[] { Type.MakeGenericMethodParameter(0), Type.MakeGenericMethodParameter(1) }), (target, deferredExprs, arguments) => $"SUM({arguments[0]})");
        methodCallSqlFormatterCahe.TryAdd(typeof(IAggregateSelect).GetMethod("Avg", 1, new Type[] { Type.MakeGenericMethodParameter(0) }), (target, deferredExprs, arguments) => $"AVG({arguments[0]})");
        methodCallSqlFormatterCahe.TryAdd(typeof(IAggregateSelect).GetMethod("AvgAs", 2, new Type[] { Type.MakeGenericMethodParameter(0), Type.MakeGenericMethodParameter(1) }), (target, deferredExprs, arguments) => $"AVG({arguments[0]})");
        methodCallSqlFormatterCahe.TryAdd(typeof(IAggregateSelect).GetMethod("Max", 1, new Type[] { Type.MakeGenericMethodParameter(0) }), (target, deferredExprs, arguments) => $"MAX({arguments[0]})");
        methodCallSqlFormatterCahe.TryAdd(typeof(IAggregateSelect).GetMethod("MaxAs", 2, new Type[] { Type.MakeGenericMethodParameter(0), Type.MakeGenericMethodParameter(1) }), (target, deferredExprs, arguments) => $"MAX({arguments[0]})");
        methodCallSqlFormatterCahe.TryAdd(typeof(IAggregateSelect).GetMethod("Min", 1, new Type[] { Type.MakeGenericMethodParameter(0) }), (target, deferredExprs, arguments) => $"MIN({arguments[0]})");
        methodCallSqlFormatterCahe.TryAdd(typeof(IAggregateSelect).GetMethod("MinAs", 2, new Type[] { Type.MakeGenericMethodParameter(0), Type.MakeGenericMethodParameter(1) }), (target, deferredExprs, arguments) => $"MIN({arguments[0]})");
    }
    public override IDbConnection CreateConnection(string connectionString)
        => createNativeConnectonDelegate.Invoke(connectionString);
    public override IDbDataParameter CreateParameter(string parameterName, object value)
    {
        var dbType = this.GetNativeDbType(value.GetType());
        return createNativeParameterDelegate.Invoke(parameterName, dbType, value);
    }
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
    public override bool TryGetMemberAccessSqlFormatter(MemberInfo memberInfo, out MemberAccessSqlFormatter formatter)
    {
        if (!memberAccessSqlFormatterCahe.TryGetValue(memberInfo, out formatter))
        {
            bool result = false;
            switch (memberInfo.Name)
            {
                case "Empty":
                    //String.Empty
                    if (memberInfo.DeclaringType == typeof(string))
                    {
                        memberAccessSqlFormatterCahe.TryAdd(memberInfo, formatter = target => "''");
                        result = true;
                    }
                    break;
                case "Length":
                    //String.Length
                    if (memberInfo.DeclaringType == typeof(string))
                    {
                        memberAccessSqlFormatterCahe.TryAdd(memberInfo, formatter = target =>
                        {
                            string fieldSql = null;
                            if (target is SqlSegment sqlSegment)
                            {
                                if (!sqlSegment.IsParameter && !sqlSegment.HasField)
                                    fieldSql = this.GetQuotedValue(sqlSegment);
                                else fieldSql = sqlSegment.ToString();
                            }
                            else fieldSql = this.GetQuotedValue(target);
                            return $"CHAR_LENGTH({fieldSql})";
                        });
                        result = true;
                    }
                    break;
            }
            return result;
        }
        return true;
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
                    if (methodInfo.IsStatic && parameterInfos.Length >= 2 && parameterInfos[0].ParameterType.GenericTypeArguments.Length > 0
                        && parameterInfos[0].ParameterType.IsAssignableFrom(typeof(IEnumerable<>).MakeGenericType(parameterInfos[0].ParameterType.GenericTypeArguments[0])))
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

                            string fieldName = null;
                            if (args[1] is SqlSegment targetSegment)
                            {
                                if (targetSegment.HasField || targetSegment.IsParameter)
                                    fieldName = targetSegment.ToString();
                                else fieldName = this.GetQuotedValue(targetSegment.Value);
                            }
                            else fieldName = this.GetQuotedValue(args[1]);

                            int notIndex = 0;
                            if (deferExprs != null)
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
                            //TODO:如果数组没有数据，抛出异常
                            //else builder.Append(fieldName + " IN (NULL)");
                            return builder.ToString();
                        });
                        result = true;
                    }
                    //IEnumerable<T>,List<T>
                    //public bool Contains(T item);
                    if (!methodInfo.IsStatic && parameterInfos.Length == 1 && methodInfo.DeclaringType.GenericTypeArguments.Length > 0
                        && methodInfo.DeclaringType.IsAssignableFrom(typeof(IEnumerable<>).MakeGenericType(methodInfo.DeclaringType.GenericTypeArguments[0])))
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

                            foreach (var element in enumerable)
                            {
                                if (builder.Length > 0)
                                    builder.Append(',');
                                builder.Append(element);
                            }
                            string fieldName = null;
                            if (args[0] is SqlSegment fieldSegment)
                            {
                                if (fieldSegment.HasField || fieldSegment.IsParameter)
                                    fieldName = fieldSegment.ToString();
                                else fieldName = this.GetQuotedValue(fieldSegment.Value);
                            }
                            else fieldName = this.GetQuotedValue(args[0]);

                            int notIndex = 0;
                            if (deferExprs != null)
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
                    if (!methodInfo.IsStatic && parameterInfos.Length >= 1 && methodInfo.DeclaringType.IsAssignableFrom(typeof(string)))
                    {
                        methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (target, deferExprs, args) =>
                        {
                            string leftField = null;
                            if (target is SqlSegment leftSegment)
                            {
                                if (leftSegment.HasField || leftSegment.IsParameter)
                                    leftField = leftSegment.ToString();
                                else leftField = this.GetQuotedValue(leftSegment.Value);
                            }
                            else leftField = target.ToString();

                            string rightValue = null;
                            if (args[0] is SqlSegment rightSegment && rightSegment.IsParameter)
                            {
                                var concatMethodInfo = typeof(string).GetMethod("Concat", BindingFlags.Public | BindingFlags.Static, new Type[] { typeof(string), typeof(string), typeof(string) });
                                if (this.TryGetMethodCallSqlFormatter(concatMethodInfo, out var concatFormatter))
                                    //自己调用字符串连接，参数直接是字符串
                                    rightValue = concatFormatter.Invoke(null, deferExprs, "'%'", rightSegment.Value.ToString(), "'%'");
                            }
                            else rightValue = $"'%{args[0]}%'";

                            int notIndex = 0;
                            if (deferExprs != null)
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
                                            builder.Append(", ");

                                        if (element is SqlSegment sqlSegment)
                                        {
                                            if (sqlSegment.HasField || sqlSegment.IsParameter)
                                                builder.Append(sqlSegment.Value);
                                            else builder.Append(this.GetQuotedValue(sqlSegment.Value));
                                        }
                                        else builder.Append(element);
                                    }
                                }
                                else
                                {
                                    if (builder.Length > 0)
                                        builder.Append(", ");

                                    if (arg is SqlSegment sqlSegment)
                                    {
                                        if (sqlSegment.HasField || sqlSegment.IsParameter)
                                            builder.Append(sqlSegment.Value);
                                        else builder.Append(this.GetQuotedValue(sqlSegment.Value));
                                    }
                                    else builder.Append(arg);
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
                    }
                    break;
                case "Format":
                    if (methodInfo.IsStatic && methodInfo.DeclaringType == typeof(string))
                    {
                        //public static String Format(String format, object? arg0);
                        //public static String Format(String format, object? arg0, object? arg1); 
                        //public static String Format(String format, object? arg0, object? arg1, object? arg2); 
                        //public static String Format(String format, params object?[] args);
                        methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (target, deferExprs, args) =>
                        {
                            var parameters = new List<object>(args);
                            parameters.RemoveAt(0);

                            return string.Format(args[0] as string, parameters.ToArray());
                        });
                        result = true;
                    }
                    break;
                case "Compare":
                    if (methodInfo.IsStatic && methodInfo.DeclaringType == typeof(string))
                    {
                        //String.Compare  不区分大小写
                        //public static int Compare(String? strA, String? strB);
                        //public static int Compare(String? strA, String? strB, bool ignoreCase);
                        //public static int Compare(String? strA, String? strB, bool ignoreCase, CultureInfo? culture);
                        if (parameterInfos.Length >= 2 && parameterInfos.Length <= 4)
                        {
                            methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (target, deferExprs, args) =>
                            {
                                string leftArgument = null;
                                if (args[0] is SqlSegment leftSegment)
                                {
                                    if (leftSegment.HasField || leftSegment.IsParameter)
                                        leftArgument = leftSegment.ToString();
                                    else leftArgument = this.GetQuotedValue(leftSegment.Value);
                                }
                                else leftArgument = this.GetQuotedValue(args[0]);

                                string rightArgument = null;
                                if (args[1] is SqlSegment rightSegment)
                                {
                                    if (rightSegment.HasField || rightSegment.IsParameter)
                                        rightArgument = rightSegment.ToString();
                                    else rightArgument = this.GetQuotedValue(rightSegment.Value);
                                }
                                else rightArgument = this.GetQuotedValue(args[1]);

                                return $"(CASE WHEN {leftArgument}={rightArgument} THEN 0 WHEN {leftArgument}>{rightArgument} THEN 1 ELSE -1 END)";
                            });
                            result = true;
                        }
                    }
                    break;
                case "CompareOrdinal":
                    if (methodInfo.IsStatic && methodInfo.DeclaringType == typeof(string))
                    {
                        //public static int CompareOrdinal(String? strA, String? strB);
                        if (parameterInfos.Length == 2)
                        {
                            methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (target, deferExprs, args) =>
                            {
                                string leftArgument = null;
                                if (args[0] is SqlSegment leftSegment)
                                {
                                    if (leftSegment.HasField || leftSegment.IsParameter)
                                        leftArgument = leftSegment.ToString();
                                    else leftArgument = this.GetQuotedValue(leftSegment.Value);
                                }
                                else leftArgument = this.GetQuotedValue(args[0]);

                                string rightArgument = null;
                                if (args[1] is SqlSegment rightSegment)
                                {
                                    if (rightSegment.HasField || rightSegment.IsParameter)
                                        rightArgument = rightSegment.ToString();
                                    else rightArgument = this.GetQuotedValue(rightSegment.Value);
                                }
                                else rightArgument = this.GetQuotedValue(args[1]);

                                return $"(CASE WHEN {leftArgument}={rightArgument} THEN 0 WHEN {leftArgument}>{rightArgument} THEN 1 ELSE -1 END)";
                            });
                            result = true;
                        }
                    }
                    break;
                case "CompareTo":
                    if (!methodCallSqlFormatterCahe.TryGetValue(methodInfo, out formatter))
                    {
                        //各种类型都有CompareTo方法
                        //public int CompareTo(Boolean value);
                        //public int CompareTo(Int32 value);
                        //public int CompareTo(Double value);
                        //public int CompareTo(DateTime value);
                        //public int CompareTo(object? value);
                        methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (target, deferExprs, args) =>
                        {
                            string leftArgument = null;
                            if (target is SqlSegment leftSegment)
                            {
                                if (leftSegment.HasField || leftSegment.IsParameter)
                                    leftArgument = leftSegment.ToString();
                                else leftArgument = this.GetQuotedValue(leftSegment.Value);
                            }
                            else leftArgument = this.GetQuotedValue(target);

                            string rightArgument = null;
                            if (args[0] is SqlSegment rightSegment)
                            {
                                if (rightSegment.HasField || rightSegment.IsParameter)
                                    rightArgument = rightSegment.ToString();
                                else rightArgument = this.GetQuotedValue(rightSegment.Value);
                            }
                            else rightArgument = this.GetQuotedValue(args[0]);

                            return $"(CASE WHEN {leftArgument}={rightArgument} THEN 0 WHEN {leftArgument}>{rightArgument} THEN 1 ELSE -1 END)";
                        });
                        result = true;
                    }
                    break;
                case "Trim":
                    formatter = (target, deferExprs, args) => $"ltrim(rtrim({target}))";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "LTrim":
                    formatter = (target, deferExprs, args) => $"ltrim({target})";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "RTrim":
                    formatter = (target, deferExprs, args) => $"rtrim({target})";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "ToUpper":
                    formatter = (target, deferExprs, args) => $"upper({target})";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "ToLower":
                    formatter = (target, deferExprs, args) => $"lower({target})";
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter);
                    result = true;
                    break;
                case "Equals":
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (target, deferExprs, args) =>
                    {
                        string leftTarget = null;
                        if (target is SqlSegment targetSeqment)
                        {
                            if (targetSeqment.HasField || targetSeqment.IsParameter)
                                leftTarget = targetSeqment.ToString();
                            else leftTarget = this.GetQuotedValue(targetSeqment.Value);
                        }
                        else leftTarget = this.GetQuotedValue(target);

                        string rightValue = null;
                        if (args[0] is SqlSegment rightSeqment)
                        {
                            if (rightSeqment.HasField || rightSeqment.IsParameter)
                                rightValue = rightSeqment.ToString();
                            else rightValue = this.GetQuotedValue(rightSeqment.Value);
                        }
                        else rightValue = this.GetQuotedValue(args[0]);

                        int notIndex = 0;
                        if (deferExprs != null)
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
                        string leftField = null;
                        if (target is SqlSegment leftSegment)
                        {
                            if (leftSegment.HasField || leftSegment.IsParameter)
                                leftField = leftSegment.ToString();
                            else leftField = this.GetQuotedValue(leftSegment.Value);
                        }
                        else leftField = target.ToString();

                        string rightValue = null;
                        if (args[0] is SqlSegment rightSegment && rightSegment.IsParameter)
                        {
                            var concatMethodInfo = typeof(string).GetMethod("Concat", BindingFlags.Public | BindingFlags.Static, new Type[] { typeof(string), typeof(string), typeof(string) });
                            if (this.TryGetMethodCallSqlFormatter(concatMethodInfo, out var concatFormatter))
                                //自己调用字符串连接，参数直接是字符串
                                rightValue = concatFormatter.Invoke(null, deferExprs, rightSegment.Value.ToString(), "'%'");
                        }
                        else rightValue = $"'{args[0]}%'";

                        int notIndex = 0;
                        if (deferExprs != null)
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
                        string leftField = null;
                        if (target is SqlSegment leftSegment)
                        {
                            if (leftSegment.HasField || leftSegment.IsParameter)
                                leftField = leftSegment.ToString();
                            else leftField = this.GetQuotedValue(leftSegment.Value);
                        }
                        else leftField = target.ToString();

                        string rightValue = null;
                        if (args[0] is SqlSegment rightSegment && rightSegment.IsParameter)
                        {
                            var concatMethodInfo = typeof(string).GetMethod("Concat", BindingFlags.Public | BindingFlags.Static, new Type[] { typeof(string), typeof(string), typeof(string) });
                            if (this.TryGetMethodCallSqlFormatter(concatMethodInfo, out var concatFormatter))
                                //自己调用字符串连接，参数直接是字符串
                                rightValue = concatFormatter.Invoke(null, deferExprs, "'%'", rightSegment.Value.ToString());
                        }
                        else rightValue = $"'%{args[0]}'";

                        int notIndex = 0;
                        if (deferExprs != null)
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
                        formatter = (target, deferExprs, args) => $"substring({target} from {(int)(args[0]) + 1} for {args[1]})";
                    else formatter = (target, deferExprs, args) => $"substring({target} from {(int)(args[0]) + 1}";
                    result = true;
                    break;
                case "ToString":
                    if (methodInfo.DeclaringType == typeof(string))
                        formatter = (target, deferExprs, args) => target.ToString();
                    else formatter = (target, deferExprs, args) => $"CAST({target} AS {this.CastTo(typeof(string))})";
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
