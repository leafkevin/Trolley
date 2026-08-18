using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.PostgreSql;

partial class PostgreSqlProvider
{
    public override bool TryGetStringMemberAccessSqlFormatter(MemberExpression memberExpr, out Func<ISqlVisitor, SqlSegment, SqlSegment> formatter)
    {
        bool result = false;
        formatter = null;
        var memberInfo = memberExpr.Member;
        var cacheKey = HashCode.Combine(memberInfo.DeclaringType, memberInfo);
        if (memberExpr.Expression == null)
        {
            switch (memberInfo.Name)
            {
                //静态成员访问，理论上没有target对象，为了不再创建sqlSegment对象，外层直接把对象传了进来
                case "Empty":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) => target.Change("''", true));
                    result = true;
                    break;
            }
            return result;
        }
        switch (memberInfo.Name)
        {
            case "Length":
                formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                {
                    var targetSegment = visitor.Visit(target);
                    if (targetSegment.IsValue)
                        return targetSegment.Change(((string)targetSegment.Value).Length);

                    return targetSegment.Change($"LENGTH({targetSegment.Value})", SqlType.MethodCall);
                });
                result = true;
                break;
        }
        return result;
    }
    public override bool TryGetStringMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out Func<ISqlVisitor, MethodCallExpression, Stack<DeferredOperation>, SqlSegment> formatter)
    {
        var result = false;
        formatter = null;
        var methodInfo = methodCallExpr.Method;
        var parameterInfos = methodInfo.GetParameters();
        var cacheKey = HashCode.Combine(methodInfo.DeclaringType, methodInfo);
        if (methodInfo.IsStatic)
        {
            switch (methodInfo.Name)
            {
                case "Concat":
                    //public static String Concat(IEnumerable<String?> values);
                    //public static String Concat(params String?[] values);
                    //public static String Concat<T>(IEnumerable<T> values);
                    //public static String Concat(params object?[] args);
                    //public static String Concat(object? arg0);
                    //public static String Concat(object? arg0, object? arg1, object? arg2);
                    //public static String Concat(String? str0, String? str1, String? str2, String? str3);
                    //public static String Concat(ReadOnlySpan<char> str0, ReadOnlySpan<char> str1, ReadOnlySpan<char> str2, ReadOnlySpan<char> str3);
                    if (parameterInfos.Length >= 1)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var builder = new StringBuilder();
                            var constBuilder = new StringBuilder();
                            var concatExprs = visitor.SplitConcatList(args);
                            SqlSegment resultSegment = default;

                            bool isDeferredFields = false;
                            var sqlSegments = new List<SqlSegment>();
                            for (var i = 0; i < concatExprs.Count; i++)
                            {
                                //可能是一个sqlSegment，也可能是多个List<sqlSegment>
                                var sqlSegment = visitor.Visit(new SqlSegment { Expression = concatExprs[i] });
                                //获取枚举名称，根据数据库的字段类型来处理
                                if (sqlSegment.SegmentType.IsEnum && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall)
                                    visitor.ToEnumString(sqlSegment);

                                //先不处理类型，都解析完毕后，最后处理类型，转换成字符串
                                sqlSegments.Add(sqlSegment);
                                if (sqlSegment.IsDeferredFields)
                                {
                                    isDeferredFields = true;
                                    resultSegment = sqlSegment;
                                    break;
                                }
                            }
                            if (isDeferredFields)
                            {
                                if (!visitor.IsSelect)
                                    throw new NotSupportedException($"不支持的方法调用：{orgExpr}");
                                //.NET解析 f.TotalAmount.ToString("C") 语句后，会更改methodCallExpr的内容，此处使用原始表达式
                                return visitor.BuildDeferredSqlSegment(orgExpr as MethodCallExpression, resultSegment);
                            }

                            resultSegment = sqlSegments[0];
                            for (var i = 0; i < concatExprs.Count; i++)
                            {
                                //可能是一个sqlSegment，也可能是多个List<sqlSegment>
                                var sqlSegment = sqlSegments[i];
                                if (sqlSegment.IsConstant)
                                {
                                    constBuilder.Append(sqlSegment.Value.ToString());
                                    continue;
                                }
                                if (constBuilder.Length > 0)
                                {
                                    if (builder.Length > 0)
                                        builder.Append(',');
                                    builder.Append($"'{constBuilder}'");
                                    constBuilder.Clear();
                                }
                                if (builder.Length > 0)
                                    builder.Append(',');

                                string body = visitor.WrapSql(sqlSegment);
                                if (sqlSegment.SegmentType != typeof(string))
                                {
                                    if (sqlSegment.HasField || sqlSegment.IsExpression || sqlSegment.IsMethodCall)
                                        body = this.CastTo(typeof(string), sqlSegment.Value);
                                    //变量场景
                                    else body = visitor.ChangeParameterValue(sqlSegment, typeof(string));
                                }
                                builder.Append(body);
                            }
                            if (builder.Length > 0)
                            {
                                if (constBuilder.Length > 0)
                                {
                                    builder.Append($",'{constBuilder}'");
                                    constBuilder.Clear();
                                }
                                builder.Insert(0, "CONCAT(");
                                builder.Append(')');
                                return resultSegment.Change(builder.ToString(), false, true);
                            }
                            return resultSegment.Change(constBuilder.ToString(), true);
                        });
                        result = true;
                    }
                    break;
                case "Format":
                    //public static String Format(String format, object? arg0);
                    //public static String Format(String format, object? arg0, object? arg1); 
                    //public static String Format(String format, object? arg0, object? arg1, object? arg2); 
                    //public static String Format(String format, params object?[] args);
                    if (parameterInfos.Length >= 2)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var builder = new StringBuilder();
                            var constBuilder = new StringBuilder();
                            //已经被分割成了多个SqlSegment
                            var concatExprs = visitor.ConvertFormatToConcatList(args);
                            SqlSegment resultSegment = default;

                            //123_{0}_345_{1}{2}_etr_{3}_fdr, 111,@p1,@p2,e4re
                            bool isDeferredFields = false;
                            var sqlSegments = new List<SqlSegment>();
                            for (var i = 0; i < concatExprs.Count; i++)
                            {
                                //可能是一个sqlSegment，也可能是多个List<sqlSegment>
                                var sqlSegment = visitor.Visit(new SqlSegment { Expression = concatExprs[i] });
                                //获取枚举名称，根据数据库的字段类型来处理
                                if (sqlSegment.SegmentType.IsEnum && !sqlSegment.IsExpression && !sqlSegment.IsMethodCall)
                                    visitor.ToEnumString(sqlSegment);
                                sqlSegments.Add(sqlSegment);
                                if (sqlSegment.IsDeferredFields)
                                {
                                    isDeferredFields = true;
                                    resultSegment = sqlSegment;
                                    break;
                                }
                            }
                            if (isDeferredFields)
                            {
                                if (!visitor.IsSelect)
                                    throw new NotSupportedException($"不支持的方法调用：{orgExpr}");
                                //.NET解析 f.TotalAmount.ToString("C") 语句后，会更改methodCallExpr的内容，此处使用原始表达式
                                return visitor.BuildDeferredSqlSegment(orgExpr as MethodCallExpression, resultSegment);
                            }

                            resultSegment = sqlSegments[0];
                            for (var i = 0; i < concatExprs.Count; i++)
                            {
                                //可能是一个sqlSegment，也可能是多个List<sqlSegment>
                                var sqlSegment = sqlSegments[i];
                                if (sqlSegment.IsConstant)
                                {
                                    constBuilder.Append(sqlSegment.Value.ToString());
                                    continue;
                                }
                                if (constBuilder.Length > 0)
                                {
                                    if (builder.Length > 0)
                                        builder.Append(',');
                                    builder.Append($"'{constBuilder}'");
                                    constBuilder.Clear();
                                }
                                if (builder.Length > 0)
                                    builder.Append(',');

                                string body = visitor.WrapSql(sqlSegment);
                                if (sqlSegment.SegmentType != typeof(string))
                                {
                                    if (sqlSegment.HasField || sqlSegment.IsExpression || sqlSegment.IsMethodCall)
                                        body = this.CastTo(typeof(string), sqlSegment.Value);
                                    //变量场景
                                    else body = visitor.ChangeParameterValue(sqlSegment, typeof(string));
                                }
                                builder.Append(body);
                            }

                            if (builder.Length > 0)
                            {
                                if (constBuilder.Length > 0)
                                {
                                    builder.Append($",'{constBuilder}'");
                                    constBuilder.Clear();
                                }
                                builder.Insert(0, "CONCAT(");
                                builder.Append(')');
                                return resultSegment.Change(builder.ToString(), false, true);
                            }
                            return resultSegment.Change(constBuilder.ToString(), true);
                        });
                        result = true;
                    }
                    break;
                case "Compare":
                case "CompareOrdinal":
                    //String.Compare  不区分大小写
                    //public static int Compare(String? strA, String? strB);
                    //public static int Compare(String? strA, String? strB, bool ignoreCase);
                    //public static int Compare(String? strA, String? strB, bool ignoreCase, CultureInfo? culture);
                    if (parameterInfos.Length >= 2)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var leftSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[1] });
                            var leftArgument = visitor.WrapSql(leftSegment);
                            var rightArgument = visitor.WrapSql(rightSegment);
                            return leftSegment.Change($"CASE WHEN {leftArgument}={rightArgument} THEN 0 WHEN {leftArgument}>{rightArgument} THEN 1 ELSE -1 END");
                        });
                        result = true;
                    }
                    break;
                case "IsNullOrEmpty":
                    if (parameterInfos.Length == 1)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var valueSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            var valueArgument = visitor.WrapSql(valueSegment, true);
                            return valueSegment.Change($"({valueArgument} IS NULL OR {valueArgument}='')", SqlType.MethodCall);
                        });
                        result = true;
                    }
                    break;
                case "IsNullOrWhiteSpace":
                    if (parameterInfos.Length == 1)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            var targetArgument = visitor.WrapSql(targetSegment, true);
                            return targetSegment.Change($"({targetArgument} IS NULL OR {targetArgument}='' OR TRIM({targetArgument})='')", SqlType.MethodCall);
                        });
                        result = true;
                    }
                    break;
                case "Join":
                    if (parameterInfos.Length == 2)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var separatorSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            var valuesSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[1] });

                            if (!separatorSegment.IsConstant)
                                throw new NotSupportedException("暂时不支持分隔符是非常量的表达式解析，可以考虑在表达式外Join后再进行查询");

                            if (valuesSegment.IsConstant || valuesSegment.IsVariable)
                                return valuesSegment.Change(string.Join(separatorSegment.Value.ToString(), valuesSegment.Value as IEnumerable));

                            var resultSegment = valuesSegment;
                            var separatorAugment = separatorSegment.Value.ToString();
                            var enumerable = valuesSegment.Value as IEnumerable;
                            var builder = new StringBuilder();
                            var constBuilder = new StringBuilder();

                            int index = 0;
                            foreach (var item in enumerable)
                            {
                                if (item is SqlSegment elementSegment)
                                {
                                    if (elementSegment.IsConstant)
                                    {
                                        constBuilder.Append(elementSegment.Value.ToString());
                                        continue;
                                    }
                                    if (builder.Length > 0)
                                        builder.Append(',');
                                    if (constBuilder.Length > 0)
                                    {
                                        builder.Append($"'{constBuilder}'");
                                        constBuilder.Clear();
                                    }
                                    builder.Append(',');

                                    string body = visitor.WrapSql(elementSegment);
                                    if (elementSegment.SegmentType != typeof(string))
                                    {
                                        if (elementSegment.HasField || elementSegment.IsExpression || elementSegment.IsMethodCall)
                                            body = this.CastTo(typeof(string), elementSegment.Value);
                                        //变量场景
                                        else body = visitor.ChangeParameterValue(elementSegment, typeof(string));
                                    }
                                    builder.Append(body);
                                }
                                else constBuilder.Append(item.ToString());
                                index++;
                            }
                            if (builder.Length > 0)
                            {
                                if (constBuilder.Length > 0)
                                {
                                    builder.Append($",'{constBuilder}'");
                                    constBuilder.Clear();
                                }
                                builder.Insert(0, "CONCAT(");
                                builder.Append(')');
                                return resultSegment.Change(builder.ToString(), false, true);
                            }
                            return resultSegment.Change(constBuilder.ToString(), true);
                        });
                        result = true;
                    }
                    if (parameterInfos.Length > 2)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var separatorSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            var valuesSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[1] });
                            var startIndex = visitor.Evaluate<int>(args[2]);
                            var length = visitor.Evaluate<int>(args[3]);

                            if (!separatorSegment.IsConstant)
                                throw new NotSupportedException("暂时不支持分隔符是非常量的表达式解析，可以考虑在表达式外Join后再进行查询");

                            if (separatorSegment.IsConstant && (valuesSegment.IsConstant || valuesSegment.IsVariable))
                                return valuesSegment.Change(string.Join(separatorSegment.Value.ToString(), valuesSegment.Value as List<SqlSegment>, startIndex, length));

                            var resultSegment = valuesSegment;
                            var separatorAugment = separatorSegment.Value.ToString();
                            var enumerable = valuesSegment.Value as IEnumerable;
                            var builder = new StringBuilder();
                            var constBuilder = new StringBuilder();
                            int index = 0, count = startIndex + length;
                            foreach (var item in enumerable)
                            {
                                if (index < startIndex)
                                {
                                    index++;
                                    continue;
                                }
                                if (index >= count) break;

                                if (item is SqlSegment elementSegment)
                                {
                                    if (elementSegment.IsConstant)
                                    {
                                        constBuilder.Append(elementSegment.Value.ToString());
                                        continue;
                                    }
                                    if (builder.Length > 0)
                                        builder.Append(',');
                                    if (constBuilder.Length > 0)
                                    {
                                        builder.Append($"'{constBuilder}'");
                                        constBuilder.Clear();
                                    }
                                    builder.Append(',');

                                    string body = visitor.WrapSql(elementSegment);
                                    if (elementSegment.SegmentType != typeof(string))
                                    {
                                        if (elementSegment.HasField || elementSegment.IsExpression || elementSegment.IsMethodCall)
                                            body = this.CastTo(typeof(string), elementSegment.Value);
                                        //变量场景
                                        else body = visitor.ChangeParameterValue(elementSegment, typeof(string));
                                    }
                                    builder.Append(body);
                                }
                                else constBuilder.Append(item.ToString());
                                index++;
                            }
                            if (builder.Length > 0)
                            {
                                if (constBuilder.Length > 0)
                                {
                                    builder.Append($",'{constBuilder}'");
                                    constBuilder.Clear();
                                }
                                builder.Insert(0, "CONCAT(");
                                builder.Append(')');
                                return resultSegment.Change(builder.ToString(), false, true);
                            }
                            return resultSegment.Change(constBuilder.ToString(), true);
                        });
                        result = true;
                    }
                    break;
                case "Equals":
                    if (parameterInfos.Length >= 2)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var leftSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[1] });

                            var leftArgument = visitor.WrapSql(leftSegment);
                            var rightArgument = visitor.WrapSql(rightSegment);

                            string equalsString = deferExprs.IsDeferredNot() ? "<>" : "=";
                            return leftSegment.Change($"{leftArgument}{equalsString}{rightArgument}");
                        });
                        result = true;
                    }
                    break;
            }
        }
        else
        {
            switch (methodInfo.Name)
            {
                case "Contains":
                    //String
                    //public bool Contains(char value);
                    //public bool Contains(char value, StringComparison comparisonType);
                    //public bool Contains(String value);
                    //public bool Contains(String value, StringComparison comparisonType);
                    if (parameterInfos.Length >= 1)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                            var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            var targetArgument = visitor.WrapSql(targetSegment);
                            string body = null;
                            if (visitor.IsSelect)
                            {
                                var notString = deferExprs.IsDeferredNot() ? "<0" : ">0";
                                if (rightSegment.IsConstant)
                                    body = $"POSITION('{rightSegment.Value}' IN {targetArgument}){notString}";
                                else body = $"POSITION({visitor.WrapSql(rightSegment)} IN {targetArgument}){notString}";
                            }
                            else
                            {
                                var notString = deferExprs.IsDeferredNot() ? "NOT " : "";
                                if (rightSegment.IsConstant)
                                    body = $"{targetArgument}{notString} LIKE '%{rightSegment.Value}%'";
                                else body = $"{targetArgument}{notString} LIKE CONCAT('%',{visitor.WrapSql(rightSegment)},'%')";
                            }
                            return targetSegment.Change(body);
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
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                        var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        var targetArgument = visitor.WrapSql(targetSegment);
                        var rightArgument = visitor.WrapSql(rightSegment);
                        return targetSegment.Change($"CASE WHEN {targetArgument}={rightArgument} THEN 0 WHEN {targetArgument}>{rightArgument} THEN 1 ELSE -1 END");
                    });
                    result = true;
                    break;
                case "Trim":
                    if (parameterInfos.Length == 0)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                            if (targetSegment.IsValue)
                                return targetSegment.Change(((string)targetSegment.Value).Trim());

                            return targetSegment.Change($"TRIM({targetSegment.Value})", SqlType.MethodCall);
                        });
                        result = true;
                    }
                    if (parameterInfos.Length == 1 && parameterInfos[0].ParameterType == typeof(char))
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                            var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            if (targetSegment.IsValue
                                && (rightSegment.IsValue))
                                return targetSegment.Change(((string)targetSegment.Value).Trim((char)rightSegment.Value));

                            var targetArgument = visitor.WrapSql(targetSegment);
                            var rightArgument = visitor.WrapSql(rightSegment);
                            return targetSegment.Change($"TRIM(BOTH {rightArgument} FROM {targetArgument})", SqlType.MethodCall);
                        });
                        result = true;
                    }
                    if (parameterInfos.Length == 1 && parameterInfos[0].ParameterType == typeof(char[]))
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                            var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            if (targetSegment.IsValue
                                && (rightSegment.IsValue))
                                return targetSegment.Change(((string)targetSegment.Value).Trim((char[])rightSegment.Value));

                            throw new NotSupportedException("暂时只支持Trim方法的参数是常量或变量的表达式解析");
                        });
                        result = true;
                    }
                    break;
                case "TrimStart":
                    if (parameterInfos.Length == 0)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                            if (targetSegment.IsValue)
                                return targetSegment.Change(((string)targetSegment.Value).TrimStart());

                            return targetSegment.Change($"LTRIM({targetSegment.Value})", SqlType.MethodCall);
                        });
                        result = true;
                    }
                    if (parameterInfos.Length == 1 && parameterInfos[0].ParameterType == typeof(char))
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                            var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            if (targetSegment.IsValue
                                && (rightSegment.IsValue))
                                return targetSegment.Change(((string)targetSegment.Value).TrimStart((char)rightSegment.Value));

                            var targetArgument = visitor.WrapSql(targetSegment);
                            var rightArgument = visitor.WrapSql(rightSegment);
                            return targetSegment.Change($"TRIM(LEADING {rightArgument} FROM {targetArgument})", SqlType.MethodCall);
                        });
                        result = true;
                    }
                    if (parameterInfos.Length == 1 && parameterInfos[0].ParameterType == typeof(char[]))
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                            var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            if (targetSegment.IsValue
                                && (rightSegment.IsValue))
                            {
                                //.NET Standard 2.0 framework场景会走到这里
                                if (rightSegment.Value is List<object> charArray && charArray.Count == 0)
                                    return targetSegment.Change(((string)targetSegment.Value).TrimStart());
                                return targetSegment.Change(((string)targetSegment.Value).TrimStart((char[])rightSegment.Value));
                            }
                            //.NET Standard 2.0 framework场景会走到这里
                            else if (rightSegment.Value is List<object> charArray && charArray.Count == 0)
                                return targetSegment.Change($"LTRIM({targetSegment.Value})", SqlType.MethodCall);
                            throw new NotSupportedException("暂时只支持TrimStart方法的参数是常量或变量的表达式解析");
                        });
                        result = true;
                    }
                    break;
                case "TrimEnd":
                    if (parameterInfos.Length == 0)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                            if (targetSegment.IsValue)
                                return targetSegment.Change(((string)targetSegment.Value).TrimEnd());

                            return targetSegment.Change($"RTRIM({targetSegment.Value})", SqlType.MethodCall);
                        });
                        result = true;
                    }
                    if (parameterInfos.Length == 1 && parameterInfos[0].ParameterType == typeof(char))
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                            var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            if (targetSegment.IsValue
                                && (rightSegment.IsValue))
                                return targetSegment.Change(((string)targetSegment.Value).TrimEnd((char)rightSegment.Value));

                            var targetArgument = visitor.WrapSql(targetSegment);
                            var rightArgument = visitor.WrapSql(rightSegment);
                            return targetSegment.Change($"TRIM(TRAILING {rightArgument} FROM {targetArgument})", SqlType.MethodCall);
                        });
                        result = true;
                    }
                    if (parameterInfos.Length == 1 && parameterInfos[0].ParameterType == typeof(char[]))
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                            var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            if (targetSegment.IsValue
                                && (rightSegment.IsValue))
                            {
                                //.NET Standard 2.0 framework场景会走到这里
                                if (rightSegment.Value is List<object> charArray && charArray.Count == 0)
                                    return targetSegment.Change(((string)targetSegment.Value).TrimEnd());
                                return targetSegment.Change(((string)targetSegment.Value).TrimEnd((char[])rightSegment.Value));
                            }
                            //.NET Standard 2.0 framework场景会走到这里
                            else if (rightSegment.Value is List<object> charArray && charArray.Count == 0)
                                return targetSegment.Change($"RTRIM({targetSegment.Value})", SqlType.MethodCall);
                            throw new NotSupportedException("暂时只支持TrimEnd方法的参数是常量或变量的表达式解析");
                        });
                        result = true;
                    }
                    break;
                case "ToUpper":
                    if (parameterInfos.Length >= 0)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                            if (targetSegment.IsValue)
                                return targetSegment.Change(((string)targetSegment.Value).ToUpper());

                            return targetSegment.Change($"UPPER({targetSegment.Value})", SqlType.MethodCall);
                        });
                        result = true;
                    }
                    break;
                case "ToLower":
                    if (parameterInfos.Length >= 0)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                            if (targetSegment.IsValue)
                                return targetSegment.Change(((string)targetSegment.Value).ToLower());

                            return targetSegment.Change($"LOWER({targetSegment.Value})", SqlType.MethodCall);
                        });
                        result = true;
                    }
                    break;
                case "Equals":
                    //各种类型都有Equals方法
                    //public bool Equals(Boolean value);
                    //public bool Equals(Int32 value);
                    //public bool Equals(Double value);
                    //public bool Equals(DateTime value);
                    //public bool Equals(object? value);
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                        var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        var targetArgument = visitor.WrapSql(targetSegment);
                        var rightArgument = visitor.WrapSql(rightSegment);

                        var equalsString = deferExprs.IsDeferredNot() ? "<>" : "=";
                        return targetSegment.Change($"{targetArgument}{equalsString}{rightArgument}");
                    });
                    result = true;
                    break;
                case "StartsWith":
                    if (parameterInfos.Length >= 1)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                            var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            var targetArgument = visitor.WrapSql(targetSegment);

                            string rightArgument = null;
                            if (rightSegment.IsConstant)
                                rightArgument = $"'{rightSegment.Value}%'";
                            else rightArgument = $"CONCAT({visitor.WrapSql(rightSegment)},'%')";

                            var notString = deferExprs.IsDeferredNot() ? "NOT " : "";
                            return targetSegment.Change($"{targetArgument}{notString} LIKE {rightArgument}");
                        });
                        result = true;
                    }
                    break;
                case "EndsWith":
                    if (parameterInfos.Length >= 1)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                            var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            var targetArgument = visitor.WrapSql(targetSegment);

                            string rightArgument = null;
                            if (rightSegment.IsConstant)
                                rightArgument = $"'%{rightSegment.Value}'";
                            else rightArgument = $"CONCAT('%',{visitor.WrapSql(rightSegment)})";

                            var notString = deferExprs.IsDeferredNot() ? "NOT " : "";
                            return targetSegment.Change($"{targetArgument}{notString} LIKE {rightArgument}");
                        });
                        result = true;
                    }
                    break;
                case "Substring":
                    if (parameterInfos.Length > 1)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                            var indexSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            var lengthSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[1] });

                            if (targetSegment.IsValue
                                && indexSegment.IsValue
                                && lengthSegment.IsValue)
                                return targetSegment.Change( targetSegment.Value.ToString().Substring(Convert.ToInt32(indexSegment.Value), Convert.ToInt32(lengthSegment.Value)));

                            var targetArgument = visitor.WrapSql(targetSegment);
                            indexSegment.Value = visitor.GetQuotedValue<int>(indexSegment) + 1;
                            var indexArgument = visitor.WrapSql(indexSegment);
                            var lengthArgument = visitor.WrapSql(lengthSegment);
                            return targetSegment.Change( $"SUBSTRING({targetArgument},{indexArgument},{lengthArgument})", SqlType.MethodCall);
                        });
                        result = true;
                    }
                    else
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                            var indexSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });

                            if (targetSegment.IsValue
                                && indexSegment.IsValue)
                                return targetSegment.Change(targetSegment.Value.ToString().Substring(Convert.ToInt32(indexSegment.Value)));

                            var targetArgument = visitor.WrapSql(targetSegment);
                            indexSegment.Value = visitor.GetQuotedValue<int>(indexSegment) + 1;
                            var indexArgument = visitor.WrapSql(indexSegment);
                            return targetSegment.Change( $"SUBSTRING({targetArgument},{indexArgument})", SqlType.MethodCall);
                        });
                        result = true;
                    }
                    break;
                case "ToString":
                    if (parameterInfos.Length >= 0)
                    {
                        //int.ToString();
                        //int.ToString(IFormatProvider);
                        //double.ToString();
                        //double.ToString(IFormatProvider);
                        //DateTime.ToString();
                        if (parameterInfos.Length == 0 || (parameterInfos.Length == 1 && typeof(IFormatProvider).IsAssignableFrom(parameterInfos[0].ParameterType)))
                        {
                            formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                            {
                                var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                                if (targetSegment.IsValue)
                                    return targetSegment.Change(targetSegment.Value.ToString());

                                return targetSegment.Change(this.CastTo(typeof(string), targetSegment.Value), false, true);
                            });
                            result = true;
                        }
                        //放到其他类型的方法中实现
                        //int.ToString(string format);
                        //double.ToString(string format);
                        //DateTime.ToString(string format);
                    }
                    break;
                case "IndexOf":
                    if (parameterInfos.Length == 1)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                            var valueSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            if (targetSegment.IsValue
                                && valueSegment.IsValue)
                                return targetSegment.Change(methodInfo.Invoke(targetSegment.Value, new object[] { valueSegment.Value }));

                            var targetArgument = visitor.WrapSql(targetSegment);
                            var valueArgument = visitor.WrapSql(valueSegment);
                            return targetSegment.Change($"POSITION({valueArgument} IN {targetArgument})-1");
                        });
                        result = true;
                    }
                    if (parameterInfos.Length > 1 && parameterInfos[1].ParameterType == typeof(int))
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                            var valueSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            var startIndexSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[1] });
                            if (targetSegment.IsValue
                                && valueSegment.IsValue
                                && (startIndexSegment.IsConstant || startIndexSegment.IsVariable))
                                return targetSegment.Change(startIndexSegment, methodInfo.Invoke(targetSegment.Value, new object[] { valueSegment.Value, startIndexSegment.Value }));

                            string indexArgument = null;
                            if (startIndexSegment.IsConstant)
                                indexArgument = $"{(int)startIndexSegment.Value + 1}";
                            else indexArgument = $"{visitor.WrapSql(startIndexSegment)}+1";
                            var targetArgument = visitor.WrapSql(targetSegment);
                            var valueArgument = visitor.WrapSql(valueSegment);
                            return targetSegment.Change(startIndexSegment, $"POSITION({valueArgument} IN SUBSTRING({targetArgument},{indexArgument}))-1");
                        });
                        result = true;
                    }
                    break;
                case "PadLeft":
                    if (parameterInfos.Length == 1)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                            var widthSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            if (targetSegment.IsValue
                                && widthSegment.IsValue)
                                return targetSegment.Change(((string)targetSegment.Value).PadLeft((int)widthSegment.Value));

                            var targetArgument = visitor.WrapSql(targetSegment);
                            var widthArgument = visitor.WrapSql(widthSegment);
                            return targetSegment.Change($"LPAD({targetArgument},{widthArgument})", SqlType.MethodCall);
                        });
                        result = true;
                    }
                    else
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                            var widthSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            var paddingSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[1] });
                            if (targetSegment.IsValue
                                && widthSegment.IsValue
                                && paddingSegment.IsValue)
                                return targetSegment.Change( ((string)targetSegment.Value).PadLeft((int)widthSegment.Value, (char)paddingSegment.Value));

                            var targetArgument = visitor.WrapSql(targetSegment);
                            var widthArgument = visitor.WrapSql(widthSegment);
                            var paddingArgument = visitor.WrapSql(paddingSegment);
                            return targetSegment.Change( $"LPAD({targetArgument},{widthArgument},{paddingArgument})", SqlType.MethodCall);
                        });
                        result = true;
                    }
                    break;
                case "PadRight":
                    if (parameterInfos.Length == 1)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                            var widthSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            if (targetSegment.IsValue
                                && widthSegment.IsValue)
                                return targetSegment.Change(((string)targetSegment.Value).PadRight((int)widthSegment.Value));

                            var targetArgument = visitor.WrapSql(targetSegment);
                            var widthArgument = visitor.WrapSql(widthSegment);
                            return targetSegment.Change($"RPAD({targetArgument},{widthArgument})", SqlType.MethodCall);
                        });
                        result = true;
                    }
                    else
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                            var widthSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            var paddingSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[1] });
                            if (targetSegment.IsValue
                                && widthSegment.IsValue
                                && paddingSegment.IsValue)
                                return targetSegment.Change( ((string)targetSegment.Value).PadRight((int)widthSegment.Value, (char)paddingSegment.Value));

                            var targetArgument = visitor.WrapSql(targetSegment);
                            var widthArgument = visitor.WrapSql(widthSegment);
                            var paddingArgument = visitor.WrapSql(paddingSegment);
                            return targetSegment.Change( $"RPAD({targetArgument},{widthArgument},{paddingArgument})", SqlType.MethodCall);
                        });
                        result = true;
                    }
                    break;
                case "Replace":
                    if (parameterInfos.Length > 2 && parameterInfos[0].ParameterType == typeof(char))
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                            var oldSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            var newSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[1] });
                            if (targetSegment.IsValue
                                && oldSegment.IsValue
                                && newSegment.IsValue)
                                return targetSegment.Change(((string)targetSegment.Value).Replace((char)oldSegment.Value, (char)newSegment.Value));

                            var targetArgument = visitor.WrapSql(targetSegment);
                            var oldArgument = visitor.WrapSql(oldSegment);
                            var newArgument = visitor.WrapSql(newSegment);
                            return targetSegment.Change( $"REPLACE({targetArgument},{oldArgument},{newArgument})", SqlType.MethodCall);
                        });
                        result = true;
                    }
                    if (parameterInfos.Length > 2 && parameterInfos[0].ParameterType == typeof(string))
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                            var oldSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            var newSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[1] });
                            if (targetSegment.IsValue
                                && oldSegment.IsValue
                                && newSegment.IsValue)
                                return targetSegment.Change(((string)targetSegment.Value).Replace((string)oldSegment.Value, (string)newSegment.Value));

                            var targetArgument = visitor.WrapSql(targetSegment);
                            var oldArgument = visitor.WrapSql(oldSegment);
                            var newArgument = visitor.WrapSql(newSegment);
                            return targetSegment.Change( $"REPLACE({targetArgument},{oldArgument},{newArgument})", SqlType.MethodCall);
                        });
                        result = true;
                    }
                    break;
            }
        }
        return result;
    }
}