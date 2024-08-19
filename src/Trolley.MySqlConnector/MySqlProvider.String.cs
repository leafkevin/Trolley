using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.MySqlConnector;

partial class MySqlProvider
{
    public override bool TryGetStringMemberAccessSqlFormatter(MemberExpression memberExpr, out MemberAccessSqlFormatter formatter)
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
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) => target.Change("''", true));
                    result = true;
                    break;
            }
            return result;
        }
        switch (memberInfo.Name)
        {
            case "Length":
                formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                {
                    var targetSegment = visitor.VisitAndDeferred(target);
                    if (targetSegment.IsConstant || targetSegment.IsVariable)
                        return targetSegment.ChangeValue(((string)targetSegment.Value).Length);

                    return targetSegment.Change($"CHAR_LENGTH({targetSegment.Body})", false, true);
                });
                result = true;
                break;
        }
        return result;
    }
    public override bool TryGetStringMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter)
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
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var builder = new StringBuilder();
                            var constBuilder = new StringBuilder();
                            var concatExprs = visitor.SplitConcatList(args);
                            SqlFieldSegment resultSegment = null;

                            bool isDeferredFields = false;
                            var sqlSegments = new List<SqlFieldSegment>();
                            for (var i = 0; i < concatExprs.Count; i++)
                            {
                                //可能是一个sqlSegment，也可能是多个List<sqlSegment>
                                var sqlSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = concatExprs[i] });
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
                                    throw new NotSupportedException($"不支持的方法调用：{methodCallExpr}");

                                return visitor.BuildDeferredSqlSegment(methodCallExpr, resultSegment);
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

                                string body = visitor.GetQuotedValue(sqlSegment);
                                if (sqlSegment.SegmentType != typeof(string))
                                {
                                    if (sqlSegment.HasField || sqlSegment.IsExpression || sqlSegment.IsMethodCall)
                                        body = this.CastTo(typeof(string), sqlSegment.Body);
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
                            return resultSegment.ChangeValue(constBuilder.ToString(), true);
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
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var builder = new StringBuilder();
                            var constBuilder = new StringBuilder();
                            //已经被分割成了多个SqlSegment
                            var concatExprs = visitor.ConvertFormatToConcatList(args);
                            SqlFieldSegment resultSegment = null;

                            //123_{0}_345_{1}{2}_etr_{3}_fdr, 111,@p1,@p2,e4re
                            bool isDeferredFields = false;
                            var sqlSegments = new List<SqlFieldSegment>();
                            for (var i = 0; i < concatExprs.Count; i++)
                            {
                                //可能是一个sqlSegment，也可能是多个List<sqlSegment>
                                var sqlSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = concatExprs[i] });
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
                                    throw new NotSupportedException($"不支持的方法调用：{methodCallExpr}");

                                return visitor.BuildDeferredSqlSegment(methodCallExpr, resultSegment);
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

                                string body = visitor.GetQuotedValue(sqlSegment);
                                if (sqlSegment.SegmentType != typeof(string))
                                {
                                    if (sqlSegment.HasField || sqlSegment.IsExpression || sqlSegment.IsMethodCall)
                                        body = this.CastTo(typeof(string), sqlSegment.Body);
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
                            return resultSegment.ChangeValue(constBuilder.ToString(), true);
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
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var leftSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                            var rightSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[1] });
                            var leftArgument = visitor.GetQuotedValue(leftSegment);
                            var rightArgument = visitor.GetQuotedValue(rightSegment);
                            return leftSegment.Merge(rightSegment, $"CASE WHEN {leftArgument}={rightArgument} THEN 0 WHEN {leftArgument}>{rightArgument} THEN 1 ELSE -1 END");
                        });
                        result = true;
                    }
                    break;
                case "IsNullOrEmpty":
                    if (parameterInfos.Length == 1)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var valueSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                            var valueArgument = visitor.GetQuotedValue(valueSegment, true);
                            return valueSegment.Change($"({valueArgument} IS NULL OR {valueArgument}='')", false, true);
                        });
                        result = true;
                    }
                    break;
                case "IsNullOrWhiteSpace":
                    if (parameterInfos.Length == 1)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                            var targetArgument = visitor.GetQuotedValue(targetSegment, true);
                            return targetSegment.Change($"({targetArgument} IS NULL OR {targetArgument}='' OR TRIM({targetArgument})='')", false, true);
                        });
                        result = true;
                    }
                    break;
                case "Join":
                    if (parameterInfos.Length == 2)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var separatorSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                            var valuesSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[1] });

                            if (!separatorSegment.IsConstant)
                                throw new NotSupportedException("暂时不支持分隔符是非常量的表达式解析，可以考虑在表达式外Join后再进行查询");

                            if (valuesSegment.IsConstant || valuesSegment.IsVariable)
                                return valuesSegment.ChangeValue(string.Join(separatorSegment.Value.ToString(), valuesSegment.Value as IEnumerable));

                            var resultSegment = valuesSegment;
                            var separatorAugment = separatorSegment.Value.ToString();
                            var enumerable = valuesSegment.Value as IEnumerable;
                            var builder = new StringBuilder();
                            var constBuilder = new StringBuilder();

                            int index = 0;
                            foreach (var item in enumerable)
                            {
                                if (item is SqlFieldSegment elementSegment)
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

                                    string body = visitor.GetQuotedValue(elementSegment);
                                    if (elementSegment.SegmentType != typeof(string))
                                    {
                                        if (elementSegment.HasField || elementSegment.IsExpression || elementSegment.IsMethodCall)
                                            body = this.CastTo(typeof(string), elementSegment.Body);
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
                            return resultSegment.ChangeValue(constBuilder.ToString(), true);
                        });
                        result = true;
                    }
                    if (parameterInfos.Length > 2)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var separatorSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                            var valuesSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[1] });
                            var startIndex = visitor.Evaluate<int>(args[2]);
                            var length = visitor.Evaluate<int>(args[3]);

                            if (!separatorSegment.IsConstant)
                                throw new NotSupportedException("暂时不支持分隔符是非常量的表达式解析，可以考虑在表达式外Join后再进行查询");

                            if (separatorSegment.IsConstant && (valuesSegment.IsConstant || valuesSegment.IsVariable))
                                return valuesSegment.ChangeValue(string.Join(separatorSegment.Value.ToString(), valuesSegment.Value as List<SqlFieldSegment>, startIndex, length));

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

                                if (item is SqlFieldSegment elementSegment)
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

                                    string body = visitor.GetQuotedValue(elementSegment);
                                    if (elementSegment.SegmentType != typeof(string))
                                    {
                                        if (elementSegment.HasField || elementSegment.IsExpression || elementSegment.IsMethodCall)
                                            body = this.CastTo(typeof(string), elementSegment.Body);
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
                            return resultSegment.ChangeValue(constBuilder.ToString(), true);
                        });
                        result = true;
                    }
                    break;
                case "Equals":
                    if (parameterInfos.Length >= 2)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var leftSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                            var rightSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[1] });

                            var leftArgument = visitor.GetQuotedValue(leftSegment);
                            var rightArgument = visitor.GetQuotedValue(rightSegment);

                            string equalsString = deferExprs.IsDeferredNot() ? "<>" : "=";
                            return leftSegment.Merge(rightSegment, $"{leftArgument}{equalsString}{rightArgument}");
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
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                            var rightSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                            var targetArgument = visitor.GetQuotedValue(targetSegment);
                            string body = null;
                            if (visitor.IsSelect)
                            {
                                var notString = deferExprs.IsDeferredNot() ? "<0" : ">0";
                                if (rightSegment.IsConstant)
                                    body = $"INSTR('{rightSegment.Value}',{targetArgument}){notString}";
                                else body = $"INSTR({visitor.GetQuotedValue(rightSegment)},{targetArgument}){notString}";
                            }
                            else
                            {
                                var notString = deferExprs.IsDeferredNot() ? "NOT " : "";
                                if (rightSegment.IsConstant)
                                    body = $"{targetArgument}{notString} LIKE '%{rightSegment.Value}%'";
                                else body = $"{targetArgument}{notString} LIKE CONCAT('%',{visitor.GetQuotedValue(rightSegment)},'%')";
                            }
                            return targetSegment.Merge(rightSegment, body);
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
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(rightSegment, $"CASE WHEN {targetArgument}={rightArgument} THEN 0 WHEN {targetArgument}>{rightArgument} THEN 1 ELSE -1 END");
                    });
                    result = true;
                    break;
                case "Trim":
                    if (parameterInfos.Length == 0)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                            if (targetSegment.IsConstant || targetSegment.IsVariable)
                                return targetSegment.ChangeValue(((string)targetSegment.Value).Trim());

                            return targetSegment.Change($"TRIM({targetSegment.Body})", false, true);
                        });
                        result = true;
                    }
                    if (parameterInfos.Length == 1 && parameterInfos[0].ParameterType == typeof(char))
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                            var rightSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (rightSegment.IsConstant || rightSegment.IsVariable))
                                return targetSegment.MergeValue(rightSegment, ((string)targetSegment.Value).Trim((char)rightSegment.Value));

                            var targetArgument = visitor.GetQuotedValue(targetSegment);
                            var rightArgument = visitor.GetQuotedValue(rightSegment);
                            return targetSegment.Merge(rightSegment, $"TRIM(BOTH {rightArgument} FROM {targetArgument})", false, true);
                        });
                        result = true;
                    }
                    if (parameterInfos.Length == 1 && parameterInfos[0].ParameterType == typeof(char[]))
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                            var rightSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (rightSegment.IsConstant || rightSegment.IsVariable))
                                return targetSegment.MergeValue(rightSegment, ((string)targetSegment.Value).Trim((char[])rightSegment.Value));

                            throw new NotSupportedException("暂时只支持Trim方法的参数是常量或变量的表达式解析");
                        });
                        result = true;
                    }
                    break;
                case "TrimStart":
                    if (parameterInfos.Length == 0)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                            if (targetSegment.IsConstant || targetSegment.IsVariable)
                                return targetSegment.ChangeValue(((string)targetSegment.Value).TrimStart());

                            return targetSegment.Change($"LTRIM({targetSegment.Body})", false, true);
                        });
                        result = true;
                    }
                    if (parameterInfos.Length == 1 && parameterInfos[0].ParameterType == typeof(char))
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                            var rightSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (rightSegment.IsConstant || rightSegment.IsVariable))
                                return targetSegment.MergeValue(rightSegment, ((string)targetSegment.Value).TrimStart((char)rightSegment.Value));

                            var targetArgument = visitor.GetQuotedValue(targetSegment);
                            var rightArgument = visitor.GetQuotedValue(rightSegment);
                            return targetSegment.Merge(rightSegment, $"TRIM(LEADING {rightArgument} FROM {targetArgument})", false, true);
                        });
                        result = true;
                    }
                    if (parameterInfos.Length == 1 && parameterInfos[0].ParameterType == typeof(char[]))
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                            var rightSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (rightSegment.IsConstant || rightSegment.IsVariable))
                                return targetSegment.MergeValue(rightSegment, ((string)targetSegment.Value).TrimStart((char[])rightSegment.Value));

                            throw new NotSupportedException("暂时只支持TrimStart方法的参数是常量或变量的表达式解析");
                        });
                        result = true;
                    }
                    break;
                case "TrimEnd":
                    if (parameterInfos.Length == 0)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                            if (targetSegment.IsConstant || targetSegment.IsVariable)
                                return targetSegment.ChangeValue(((string)targetSegment.Value).TrimEnd());

                            return targetSegment.Change($"RTRIM({targetSegment.Body})", false, true);
                        });
                        result = true;
                    }
                    if (parameterInfos.Length == 1 && parameterInfos[0].ParameterType == typeof(char))
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                            var rightSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (rightSegment.IsConstant || rightSegment.IsVariable))
                                return targetSegment.MergeValue(rightSegment, ((string)targetSegment.Value).TrimEnd((char)rightSegment.Value));

                            var targetArgument = visitor.GetQuotedValue(targetSegment);
                            var rightArgument = visitor.GetQuotedValue(rightSegment);
                            return targetSegment.Merge(rightSegment, $"TRIM(TRAILING {rightArgument} FROM {targetArgument})", false, true);
                        });
                        result = true;
                    }
                    if (parameterInfos.Length == 1 && parameterInfos[0].ParameterType == typeof(char[]))
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                            var rightSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (rightSegment.IsConstant || rightSegment.IsVariable))
                                return targetSegment.MergeValue(rightSegment, ((string)targetSegment.Value).TrimEnd((char[])rightSegment.Value));

                            throw new NotSupportedException("暂时只支持TrimEnd方法的参数是常量或变量的表达式解析");
                        });
                        result = true;
                    }
                    break;
                case "ToUpper":
                    if (parameterInfos.Length >= 0)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                            if (targetSegment.IsConstant || targetSegment.IsVariable)
                                return targetSegment.ChangeValue(((string)targetSegment.Value).ToUpper());

                            return targetSegment.Change($"UPPER({targetSegment.Body})", false, true);
                        });
                        result = true;
                    }
                    break;
                case "ToLower":
                    if (parameterInfos.Length >= 0)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                            if (targetSegment.IsConstant || targetSegment.IsVariable)
                                return targetSegment.ChangeValue(((string)targetSegment.Value).ToLower());

                            return targetSegment.Change($"LOWER({targetSegment.Body})", false, true);
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
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);

                        var equalsString = deferExprs.IsDeferredNot() ? "<>" : "=";
                        return targetSegment.Merge(rightSegment, $"{targetArgument}{equalsString}{rightArgument}");
                    });
                    result = true;
                    break;
                case "StartsWith":
                    if (parameterInfos.Length >= 1)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                            var rightSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                            var targetArgument = visitor.GetQuotedValue(targetSegment);

                            string rightArgument = null;
                            if (rightSegment.IsConstant)
                                rightArgument = $"'{rightSegment.Value}%'";
                            else rightArgument = $"CONCAT({visitor.GetQuotedValue(rightSegment)},'%')";

                            var notString = deferExprs.IsDeferredNot() ? "NOT " : "";
                            return targetSegment.Merge(rightSegment, $"{targetArgument}{notString} LIKE {rightArgument}");
                        });
                        result = true;
                    }
                    break;
                case "EndsWith":
                    if (parameterInfos.Length >= 1)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                            var rightSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                            var targetArgument = visitor.GetQuotedValue(targetSegment);

                            string rightArgument = null;
                            if (rightSegment.IsConstant)
                                rightArgument = $"'%{rightSegment.Value}'";
                            else rightArgument = $"CONCAT('%',{visitor.GetQuotedValue(rightSegment)})";

                            var notString = deferExprs.IsDeferredNot() ? "NOT " : "";
                            return targetSegment.Merge(rightSegment, $"{targetArgument}{notString} LIKE {rightArgument}");
                        });
                        result = true;
                    }
                    break;
                case "Substring":
                    if (parameterInfos.Length > 1)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                            var indexSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                            var lengthSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[1] });

                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (indexSegment.IsConstant || indexSegment.IsVariable)
                                && (lengthSegment.IsConstant || lengthSegment.IsVariable))
                                return targetSegment.MergeValue(indexSegment, lengthSegment, targetSegment.Value.ToString().Substring(Convert.ToInt32(indexSegment.Value), Convert.ToInt32(lengthSegment.Value)));

                            var targetArgument = visitor.GetQuotedValue(targetSegment);
                            var indexArgument = visitor.GetQuotedValue(indexSegment);
                            var lengthArgument = visitor.GetQuotedValue(lengthSegment);
                            return targetSegment.Merge(indexSegment, lengthSegment, $"SUBSTR({targetArgument},{indexArgument}+1,{lengthArgument})", false, true);
                        });
                        result = true;
                    }
                    else
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                            var indexSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });

                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (indexSegment.IsConstant || indexSegment.IsVariable))
                                return targetSegment.MergeValue(indexSegment, targetSegment.Value.ToString().Substring(Convert.ToInt32(indexSegment.Value)));

                            var targetArgument = visitor.GetQuotedValue(targetSegment);
                            var indexArgument = visitor.GetQuotedValue(indexSegment);
                            return targetSegment.Merge(indexSegment, $"SUBSTR({targetArgument},{indexArgument}+1)", false, true);
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
                            formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                            {
                                var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                                if (targetSegment.IsConstant || targetSegment.IsVariable)
                                    return targetSegment.ChangeValue(targetSegment.Value.ToString());

                                return targetSegment.Change(this.CastTo(typeof(string), targetSegment.Body), false, true);
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
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                            var valueSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (valueSegment.IsConstant || valueSegment.IsVariable))
                                return targetSegment.MergeValue(valueSegment, methodInfo.Invoke(targetSegment.Value, new object[] { valueSegment.Value }));

                            var targetArgument = visitor.GetQuotedValue(targetSegment);
                            var valueArgument = visitor.GetQuotedValue(valueSegment);
                            return targetSegment.Merge(valueSegment, $"LOCATE({valueArgument},{targetArgument})-1");
                        });
                        result = true;
                    }
                    if (parameterInfos.Length > 1 && parameterInfos[1].ParameterType == typeof(int))
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                            var valueSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                            var startIndexSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[1] });
                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (valueSegment.IsConstant || valueSegment.IsVariable)
                                && (startIndexSegment.IsConstant || startIndexSegment.IsVariable))
                                return targetSegment.MergeValue(valueSegment, startIndexSegment, methodInfo.Invoke(targetSegment.Value, new object[] { valueSegment.Value, startIndexSegment.Value }));

                            string indexArgument = null;
                            if (startIndexSegment.IsConstant)
                                indexArgument = $"{(int)startIndexSegment.Value + 1}";
                            else indexArgument = $"{visitor.GetQuotedValue(startIndexSegment)}+1";
                            var targetArgument = visitor.GetQuotedValue(targetSegment);
                            var valueArgument = visitor.GetQuotedValue(valueSegment);
                            return targetSegment.Merge(valueSegment, startIndexSegment, $"LOCATE({valueArgument},{targetArgument},{indexArgument})-1");
                        });
                        result = true;
                    }
                    break;
                case "PadLeft":
                    if (parameterInfos.Length == 1)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                            var widthSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (widthSegment.IsConstant || widthSegment.IsVariable))
                                return targetSegment.MergeValue(widthSegment, ((string)targetSegment.Value).PadLeft((int)widthSegment.Value));

                            var targetArgument = visitor.GetQuotedValue(targetSegment);
                            var widthArgument = visitor.GetQuotedValue(widthSegment);
                            return targetSegment.Merge(widthSegment, $"LPAD({targetArgument},{widthArgument})", false, true);
                        });
                        result = true;
                    }
                    else
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                            var widthSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                            var paddingSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[1] });
                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (widthSegment.IsConstant || widthSegment.IsVariable)
                                && (paddingSegment.IsConstant || paddingSegment.IsVariable))
                                return targetSegment.MergeValue(widthSegment, paddingSegment, ((string)targetSegment.Value).PadLeft((int)widthSegment.Value, (char)paddingSegment.Value));

                            var targetArgument = visitor.GetQuotedValue(targetSegment);
                            var widthArgument = visitor.GetQuotedValue(widthSegment);
                            var paddingArgument = visitor.GetQuotedValue(paddingSegment);
                            return targetSegment.Merge(widthSegment, paddingSegment, $"LPAD({targetArgument},{widthArgument},{paddingArgument})", false, true);
                        });
                        result = true;
                    }
                    break;
                case "PadRight":
                    if (parameterInfos.Length == 1)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                            var widthSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (widthSegment.IsConstant || widthSegment.IsVariable))
                                return targetSegment.MergeValue(widthSegment, ((string)targetSegment.Value).PadRight((int)widthSegment.Value));

                            var targetArgument = visitor.GetQuotedValue(targetSegment);
                            var widthArgument = visitor.GetQuotedValue(widthSegment);
                            return targetSegment.Merge(widthSegment, $"RPAD({targetArgument},{widthArgument})", false, true);
                        });
                        result = true;
                    }
                    else
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                            var widthSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                            var paddingSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[1] });
                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (widthSegment.IsConstant || widthSegment.IsVariable)
                                && (paddingSegment.IsConstant || paddingSegment.IsVariable))
                                return targetSegment.MergeValue(widthSegment, paddingSegment, ((string)targetSegment.Value).PadRight((int)widthSegment.Value, (char)paddingSegment.Value));

                            var targetArgument = visitor.GetQuotedValue(targetSegment);
                            var widthArgument = visitor.GetQuotedValue(widthSegment);
                            var paddingArgument = visitor.GetQuotedValue(paddingSegment);
                            return targetSegment.Merge(widthSegment, paddingSegment, $"RPAD({targetArgument},{widthArgument},{paddingArgument})", false, true);
                        });
                        result = true;
                    }
                    break;
                case "Replace":
                    if (parameterInfos.Length > 2 && parameterInfos[0].ParameterType == typeof(char))
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                            var oldSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                            var newSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[1] });
                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (oldSegment.IsConstant || oldSegment.IsVariable)
                                && (newSegment.IsConstant || newSegment.IsVariable))
                                return targetSegment.MergeValue(oldSegment, newSegment, ((string)targetSegment.Value).Replace((char)oldSegment.Value, (char)newSegment.Value));

                            var targetArgument = visitor.GetQuotedValue(targetSegment);
                            var oldArgument = visitor.GetQuotedValue(oldSegment);
                            var newArgument = visitor.GetQuotedValue(newSegment);
                            return targetSegment.Merge(oldSegment, newSegment, $"REPLACE({targetArgument},{oldArgument},{newArgument})", false, true);
                        });
                        result = true;
                    }
                    if (parameterInfos.Length > 2 && parameterInfos[0].ParameterType == typeof(string))
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                            var oldSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                            var newSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[1] });
                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (oldSegment.IsConstant || oldSegment.IsVariable)
                                && (newSegment.IsConstant || newSegment.IsVariable))
                                return targetSegment.MergeValue(oldSegment, newSegment, ((string)targetSegment.Value).Replace((string)oldSegment.Value, (string)newSegment.Value));

                            var targetArgument = visitor.GetQuotedValue(targetSegment);
                            var oldArgument = visitor.GetQuotedValue(oldSegment);
                            var newArgument = visitor.GetQuotedValue(newSegment);
                            return targetSegment.Merge(oldSegment, newSegment, $"REPLACE({targetArgument},{oldArgument},{newArgument})", false, true);
                        });
                        result = true;
                    }
                    break;
            }
        }
        return result;
    }
}