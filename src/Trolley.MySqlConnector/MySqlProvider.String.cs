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
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) => target.Change("''"));
                    result = true;
                    break;
            }
            return result;
        }
        switch (memberInfo.Name)
        {
            case "Length":
                memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                {
                    var targetSegment = visitor.VisitAndDeferred(target);
                    if (targetSegment.IsConstant || targetSegment.IsVariable)
                        return visitor.Change(targetSegment, ((string)targetSegment.Value).Length);

                    var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                    return visitor.Change(targetSegment, $"CHAR_LENGTH({targetArgument})", false, true);
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
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var builder = new StringBuilder();
                            var constBuilder = new StringBuilder();
                            var concatExprs = visitor.SplitConcatList(args);
                            SqlSegment resultSegment = null;

                            for (var i = 0; i < concatExprs.Count; i++)
                            {
                                //可能是一个sqlSegment，也可能是多个List<sqlSegment>
                                var sqlSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = concatExprs[i] });
                                if (i == 0) resultSegment = sqlSegment;
                                else resultSegment.Merge(sqlSegment);

                                if (sqlSegment.IsConstant)
                                {
                                    constBuilder.Append(sqlSegment.ToString());
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
                                sqlSegment.Value = sqlSegment.Value.ToString();
                                builder.Append(visitor.Change(sqlSegment));
                                resultSegment.IsParameter = resultSegment.IsParameter || sqlSegment.IsParameter;
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
                                return resultSegment.Change(builder.ToString(), false, false, true);
                            }
                            return resultSegment.Change(constBuilder.ToString());
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
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var builder = new StringBuilder();
                            var constBuilder = new StringBuilder();
                            //已经被分割成了多个SqlSegment
                            var concatSegments = visitor.ConvertFormatToConcatList(args);
                            SqlSegment resultSegment = null;

                            //123_{0}_345_{1}{2}_etr_{3}_fdr, 111,@p1,@p2,e4re
                            for (var i = 0; i < concatSegments.Count; i++)
                            {
                                //可能是一个sqlSegment，也可能是多个List<sqlSegment>
                                var sqlSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = concatSegments[i] });
                                if (i == 0) resultSegment = sqlSegment;
                                else resultSegment.Merge(sqlSegment);

                                if (sqlSegment.IsConstant)
                                {
                                    constBuilder.Append(sqlSegment.ToString());
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
                                sqlSegment.Value = sqlSegment.Value.ToString();
                                builder.Append(visitor.Change(sqlSegment));
                                resultSegment.IsParameter = resultSegment.IsParameter || sqlSegment.IsParameter;
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
                                return resultSegment.Change(builder.ToString(), false, false, true);
                            }
                            return resultSegment.Change(constBuilder.ToString());
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
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var leftSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                            var rightSegment = visitor.VisitAndDeferred(leftSegment.Clone(args[1]));
                            var leftArgument = this.GetQuotedValue(visitor.Change(leftSegment));
                            var rightArgument = this.GetQuotedValue(visitor.Change(rightSegment));
                            return visitor.Merge(leftSegment, rightSegment, $"CASE WHEN {leftArgument}={rightArgument} THEN 0 WHEN {leftArgument}>{rightArgument} THEN 1 ELSE -1 END", true, false);
                        });
                        result = true;
                    }
                    break;
                case "IsNullOrEmpty":
                    if (parameterInfos.Length == 1)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var valueSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                            var valueArgument = this.GetQuotedValue(visitor.Change(valueSegment));
                            return visitor.Change(valueSegment, $"({valueArgument} IS NULL OR {valueArgument}='')", false, true);
                        });
                        result = true;
                    }
                    break;
                case "IsNullOrWhiteSpace":
                    if (parameterInfos.Length == 1)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                            var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                            return visitor.Change(targetSegment, $"({targetArgument} IS NULL OR {targetArgument}='' OR TRIM({targetArgument})='')", false, true);
                        });
                        result = true;
                    }
                    break;
                case "Join":
                    if (parameterInfos.Length == 2)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var separatorSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                            var valuesSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[1] });

                            if (!separatorSegment.IsConstant)
                                throw new NotSupportedException("暂时不支持分隔符是非常量的表达式解析，可以考虑在表达式外Join后再进行查询");

                            if (separatorSegment.IsConstant && (valuesSegment.IsConstant || valuesSegment.IsVariable))
                                return visitor.Merge(valuesSegment, separatorSegment, string.Join(separatorSegment.ToString(), valuesSegment.Value as IEnumerable));

                            var resultSegment = valuesSegment;
                            var separatorAugment = separatorSegment.ToString();
                            var enumerable = valuesSegment.Value as IEnumerable;
                            var builder = new StringBuilder();
                            var constBuilder = new StringBuilder();

                            int index = 0;
                            foreach (var item in enumerable)
                            {
                                if (item is SqlSegment elementSegment)
                                {
                                    resultSegment.Merge(elementSegment);
                                    if (elementSegment.IsConstant)
                                    {
                                        constBuilder.Append(elementSegment.ToString());
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
                                    elementSegment.Value = elementSegment.Value.ToString();
                                    builder.Append(visitor.Change(elementSegment));
                                    resultSegment.IsParameter = resultSegment.IsParameter || elementSegment.IsParameter;
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
                                return resultSegment.Change(builder.ToString(), false, false, true);
                            }
                            return resultSegment.Change(constBuilder.ToString());
                        });
                        result = true;
                    }
                    if (parameterInfos.Length > 2)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var separatorSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                            var valuesSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[1] });
                            var startIndex = visitor.Evaluate<int>(args[2]);
                            var length = visitor.Evaluate<int>(args[3]);

                            if (!separatorSegment.IsConstant)
                                throw new NotSupportedException("暂时不支持分隔符是非常量的表达式解析，可以考虑在表达式外Join后再进行查询");

                            if (separatorSegment.IsConstant && (valuesSegment.IsConstant || valuesSegment.IsVariable))
                                return valuesSegment.Change(string.Join(separatorSegment.ToString(), valuesSegment.Value as List<SqlSegment>, startIndex, length));

                            var resultSegment = valuesSegment;
                            var separatorAugment = separatorSegment.ToString();
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
                                    resultSegment.Merge(elementSegment);
                                    if (elementSegment.IsConstant)
                                    {
                                        constBuilder.Append(elementSegment.ToString());
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
                                    elementSegment.Value = elementSegment.Value.ToString();
                                    builder.Append(visitor.Change(elementSegment));
                                    resultSegment.IsParameter = resultSegment.IsParameter || elementSegment.IsParameter;
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
                                return resultSegment.Change(builder.ToString(), false, false, true);
                            }
                            return resultSegment.Change(constBuilder.ToString());
                        });
                        result = true;
                    }
                    break;
                case "Equals":
                    if (parameterInfos.Length >= 2)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var leftSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                            var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[1] });
                            var leftArgument = this.GetQuotedValue(visitor.Change(leftSegment));
                            var rightArgument = this.GetQuotedValue(visitor.Change(rightSegment));

                            int notIndex = 0;
                            if (deferExprs != null && deferExprs.Count > 0)
                            {
                                while (deferExprs.TryPop(out var deferredExpr))
                                {
                                    switch (deferredExpr.OperationType)
                                    {
                                        case OperationType.Equal:
                                            continue;
                                        case OperationType.Not:
                                            notIndex++;
                                            break;
                                    }
                                }
                            }
                            string equalsString = notIndex % 2 > 0 ? "<>" : "=";
                            return visitor.Merge(leftSegment, rightSegment, $"{leftArgument}{equalsString}{rightArgument}", true, false);
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
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                            var rightSegment = visitor.VisitAndDeferred(targetSegment.Clone(args[0]));

                            visitor.Change(targetSegment);
                            visitor.Change(rightSegment);
                            string rightArgument = null;
                            if (rightSegment.IsConstant)
                                rightArgument = $"'%{rightSegment}%'";
                            else rightArgument = $"CONCAT('%',{visitor.Change(rightSegment)},'%')";

                            int notIndex = 0;
                            if (deferExprs != null && deferExprs.Count > 0)
                            {
                                while (deferExprs.TryPop(out var deferredExpr))
                                {
                                    switch (deferredExpr.OperationType)
                                    {
                                        case OperationType.Equal:
                                            continue;
                                        case OperationType.Not:
                                            notIndex++;
                                            break;
                                    }
                                }
                            }
                            string notString = notIndex % 2 > 0 ? " NOT" : "";
                            var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                            return visitor.Merge(targetSegment, rightSegment, $"{targetArgument}{notString} LIKE {rightArgument}", true, false);
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
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(targetSegment.Clone(args[0]));
                        var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                        var rightArgument = this.GetQuotedValue(visitor.Change(rightSegment));
                        return visitor.Merge(targetSegment, rightSegment, $"CASE WHEN {targetArgument}={rightArgument} THEN 0 WHEN {targetArgument}>{rightArgument} THEN 1 ELSE -1 END", true, false);
                    });
                    result = true;
                    break;
                case "Trim":
                    if (parameterInfos.Length == 0)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                            if (targetSegment.IsConstant || targetSegment.IsVariable)
                                return visitor.Change(targetSegment, ((string)targetSegment.Value).Trim());

                            var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                            return visitor.Change(targetSegment, $"TRIM({targetArgument})", false, true);
                        });
                        result = true;
                    }
                    if (parameterInfos.Length == 1 && parameterInfos[0].ParameterType == typeof(char))
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                            var rightSegment = visitor.VisitAndDeferred(targetSegment.Clone(args[0]));
                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (rightSegment.IsConstant || rightSegment.IsVariable))
                                return visitor.Merge(targetSegment, rightSegment, ((string)targetSegment.Value).Trim((char)rightSegment.Value));

                            var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                            var rightArgument = this.GetQuotedValue(visitor.Change(rightSegment));
                            return visitor.Merge(targetSegment, rightSegment, $"TRIM(BOTH {rightArgument} FROM {targetArgument})", false, true);
                        });
                        result = true;
                    }
                    if (parameterInfos.Length == 1 && parameterInfos[0].ParameterType == typeof(char[]))
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                            var rightSegment = visitor.VisitAndDeferred(targetSegment.Clone(args[0]));
                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (rightSegment.IsConstant || rightSegment.IsVariable))
                                return visitor.Merge(targetSegment, rightSegment, ((string)targetSegment.Value).Trim((char[])rightSegment.Value));

                            throw new NotSupportedException("暂时只支持Trim方法的参数是常量或变量的表达式解析");
                        });
                        result = true;
                    }
                    break;
                case "TrimStart":
                    if (parameterInfos.Length == 0)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                            if (targetSegment.IsConstant || targetSegment.IsVariable)
                                return visitor.Change(targetSegment, ((string)targetSegment.Value).TrimStart());

                            var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                            return visitor.Change(targetSegment, $"LTRIM({targetArgument})", false, true);
                        });
                        result = true;
                    }
                    if (parameterInfos.Length == 1 && parameterInfos[0].ParameterType == typeof(char))
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                            var rightSegment = visitor.VisitAndDeferred(targetSegment.Clone(args[0]));
                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (rightSegment.IsConstant || rightSegment.IsVariable))
                                return visitor.Merge(targetSegment, rightSegment, ((string)targetSegment.Value).TrimStart((char)rightSegment.Value));

                            var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                            var rightArgument = this.GetQuotedValue(visitor.Change(rightSegment));
                            return visitor.Merge(targetSegment, rightSegment, $"TRIM(LEADING {rightArgument} FROM {targetArgument})", false, true);
                        });
                        result = true;
                    }
                    if (parameterInfos.Length == 1 && parameterInfos[0].ParameterType == typeof(char[]))
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                            var rightSegment = visitor.VisitAndDeferred(targetSegment.Clone(args[0]));
                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (rightSegment.IsConstant || rightSegment.IsVariable))
                                return visitor.Merge(targetSegment, rightSegment, ((string)targetSegment.Value).TrimStart((char[])rightSegment.Value));

                            throw new NotSupportedException("暂时只支持TrimStart方法的参数是常量或变量的表达式解析");
                        });
                        result = true;
                    }
                    break;
                case "TrimEnd":
                    if (parameterInfos.Length == 0)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                            if (targetSegment.IsConstant || targetSegment.IsVariable)
                                return visitor.Change(targetSegment, ((string)targetSegment.Value).TrimEnd());

                            var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                            return visitor.Change(targetSegment, $"RTRIM({targetArgument})", false, true);
                        });
                        result = true;
                    }
                    if (parameterInfos.Length == 1 && parameterInfos[0].ParameterType == typeof(char))
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                            var rightSegment = visitor.VisitAndDeferred(targetSegment.Clone(args[0]));
                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (rightSegment.IsConstant || rightSegment.IsVariable))
                                return visitor.Merge(targetSegment, rightSegment, ((string)targetSegment.Value).TrimEnd((char)rightSegment.Value));

                            var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                            var rightArgument = this.GetQuotedValue(visitor.Change(rightSegment));
                            return visitor.Merge(targetSegment, rightSegment, $"TRIM(TRAILING {rightArgument} FROM {targetArgument})", false, true);
                        });
                        result = true;
                    }
                    if (parameterInfos.Length == 1 && parameterInfos[0].ParameterType == typeof(char[]))
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                            var rightSegment = visitor.VisitAndDeferred(targetSegment.Clone(args[0]));
                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (rightSegment.IsConstant || rightSegment.IsVariable))
                                return visitor.Merge(targetSegment, rightSegment, ((string)targetSegment.Value).TrimEnd((char[])rightSegment.Value));

                            throw new NotSupportedException("暂时只支持TrimEnd方法的参数是常量或变量的表达式解析");
                        });
                        result = true;
                    }
                    break;
                case "ToUpper":
                    if (parameterInfos.Length >= 0)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                            if (targetSegment.IsConstant || targetSegment.IsVariable)
                                return visitor.Change(targetSegment, ((string)targetSegment.Value).ToUpper());

                            var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                            return visitor.Change(targetSegment, $"UPPER({targetArgument})", false, true);
                        });
                        result = true;
                    }
                    break;
                case "ToLower":
                    if (parameterInfos.Length >= 0)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                            if (targetSegment.IsConstant || targetSegment.IsVariable)
                                return visitor.Change(targetSegment, ((string)targetSegment.Value).ToLower());

                            var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                            return visitor.Change(targetSegment, $"LOWER({targetArgument})", false, true);
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
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(targetSegment.Clone(args[0]));
                        var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                        var rightArgument = this.GetQuotedValue(visitor.Change(rightSegment));

                        int notIndex = 0;
                        if (deferExprs != null && deferExprs.Count > 0)
                        {
                            while (deferExprs.TryPop(out var deferredExpr))
                            {
                                switch (deferredExpr.OperationType)
                                {
                                    case OperationType.Equal:
                                        continue;
                                    case OperationType.Not:
                                        notIndex++;
                                        break;
                                }
                            }
                        }
                        string equalsString = notIndex % 2 > 0 ? "<>" : "=";
                        return visitor.Merge(targetSegment, rightSegment, $"{targetArgument}{equalsString}{rightArgument}", true, false);
                    });
                    result = true;
                    break;
                case "StartsWith":
                    if (parameterInfos.Length >= 1)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                            var rightSegment = visitor.VisitAndDeferred(targetSegment.Clone(args[0]));
                            var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));

                            string rightArgument = null;
                            if (rightSegment.IsConstant)
                                rightArgument = $"'{rightSegment}%'";
                            else rightArgument = $"CONCAT({visitor.Change(rightSegment)},'%')";
                            int notIndex = 0;

                            if (deferExprs != null && deferExprs.Count > 0)
                            {
                                while (deferExprs.TryPop(out var deferredExpr))
                                {
                                    switch (deferredExpr.OperationType)
                                    {
                                        case OperationType.Equal:
                                            continue;
                                        case OperationType.Not:
                                            notIndex++;
                                            break;
                                    }
                                }
                            }
                            string notString = notIndex % 2 > 0 ? " NOT" : "";
                            return visitor.Merge(targetSegment, rightSegment.ToParameter(visitor), $"{targetArgument}{notString} LIKE {rightArgument}", false, true);
                        });
                        result = true;
                    }
                    break;
                case "EndsWith":
                    if (parameterInfos.Length >= 1)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                            var rightSegment = visitor.VisitAndDeferred(targetSegment.Clone(args[0]));
                            var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));

                            string rightArgument = null;
                            if (rightSegment.IsConstant)
                                rightArgument = $"'%{rightSegment}'";
                            else rightArgument = $"CONCAT('%',{visitor.Change(rightSegment)})";
                            int notIndex = 0;

                            if (deferExprs != null && deferExprs.Count > 0)
                            {
                                while (deferExprs.TryPop(out var deferredExpr))
                                {
                                    switch (deferredExpr.OperationType)
                                    {
                                        case OperationType.Equal:
                                            continue;
                                        case OperationType.Not:
                                            notIndex++;
                                            break;
                                    }
                                }
                            }
                            string notString = notIndex % 2 > 0 ? " NOT" : "";
                            return visitor.Merge(targetSegment, rightSegment.ToParameter(visitor), $"{targetSegment}{notString} LIKE {rightArgument}", true, false);
                        });
                        result = true;
                    }
                    break;
                case "Substring":
                    if (parameterInfos.Length > 1)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                            var indexSegment = visitor.VisitAndDeferred(targetSegment.Clone(args[0]));
                            var lengthSegment = visitor.VisitAndDeferred(targetSegment.Clone(args[1]));

                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (indexSegment.IsConstant || indexSegment.IsVariable)
                                && (lengthSegment.IsConstant || lengthSegment.IsVariable))
                                return visitor.Merge(targetSegment, indexSegment, lengthSegment, targetSegment.Value.ToString().Substring(Convert.ToInt32(indexSegment.Value), Convert.ToInt32(lengthSegment.Value)));

                            var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                            var indexArgument = this.GetQuotedValue(visitor.Change(indexSegment));
                            var lengthArgument = this.GetQuotedValue(visitor.Change(lengthSegment));
                            return visitor.Merge(targetSegment, indexSegment, lengthSegment, $"SUBSTR({targetArgument},{indexArgument}+1,{lengthArgument})", false, true);
                        });
                        result = true;
                    }
                    else
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                            var indexSegment = visitor.VisitAndDeferred(targetSegment.Clone(args[0]));

                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (indexSegment.IsConstant || indexSegment.IsVariable))
                                return visitor.Merge(targetSegment, indexSegment, targetSegment.Value.ToString().Substring(Convert.ToInt32(indexSegment.Value)));

                            var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                            var indexArgument = this.GetQuotedValue(visitor.Change(indexSegment));
                            return visitor.Merge(targetSegment, indexSegment, $"SUBSTR({targetArgument},{indexArgument}+1)", false, true);
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
                            methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                            {
                                var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                                if (targetSegment.IsConstant || targetSegment.IsVariable)
                                    return visitor.Change(targetSegment, targetSegment.Value.ToString());

                                var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                                targetSegment.Type = methodInfo.ReturnType;
                                return visitor.Change(targetSegment, this.CastTo(typeof(string), targetArgument), false, true);
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
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                            var valueSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (valueSegment.IsConstant || valueSegment.IsVariable))
                                return visitor.Merge(targetSegment, valueSegment, visitor.Evaluate(orgExpr));

                            var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                            var valueArgument = this.GetQuotedValue(visitor.Change(valueSegment));
                            return visitor.Merge(targetSegment, valueSegment, $"LOCATE({valueArgument},{targetArgument})-1", true, false);
                        });
                        result = true;
                    }
                    if (parameterInfos.Length > 1 && parameterInfos[1].ParameterType == typeof(int))
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                            var valueSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                            var startIndexSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[1] });
                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (valueSegment.IsConstant || valueSegment.IsVariable)
                                && (startIndexSegment.IsConstant || startIndexSegment.IsVariable))
                                return visitor.Merge(targetSegment, valueSegment, startIndexSegment, visitor.Evaluate(orgExpr));

                            string indexArgument = null;
                            if (startIndexSegment.IsConstant)
                                indexArgument = $"{(int)startIndexSegment.Value + 1}";
                            else indexArgument = $"{visitor.Change(startIndexSegment)}+1";
                            var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                            var valueArgument = this.GetQuotedValue(visitor.Change(valueSegment));
                            return visitor.Merge(targetSegment, valueSegment, startIndexSegment.ToParameter(visitor), $"LOCATE({valueArgument},{targetArgument},{indexArgument})-1", true, false);
                        });
                        result = true;
                    }
                    break;
                case "PadLeft":
                    if (parameterInfos.Length == 1)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                            var widthSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (widthSegment.IsConstant || widthSegment.IsVariable))
                                return visitor.Merge(targetSegment, widthSegment, ((string)targetSegment.Value).PadLeft((int)widthSegment.Value));

                            var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                            var widthArgument = this.GetQuotedValue(visitor.Change(widthSegment));
                            return visitor.Merge(targetSegment, widthSegment, $"LPAD({targetArgument},{widthArgument})", false, true);
                        });
                        result = true;
                    }
                    if (parameterInfos.Length > 1 && parameterInfos[1].ParameterType == typeof(int))
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                            var widthSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                            var paddingSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[1] });
                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (widthSegment.IsConstant || widthSegment.IsVariable)
                                && (paddingSegment.IsConstant || paddingSegment.IsVariable))
                                return visitor.Merge(targetSegment, widthSegment, paddingSegment, ((string)targetSegment.Value).PadLeft((int)widthSegment.Value, (char)paddingSegment.Value));

                            var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                            var widthArgument = this.GetQuotedValue(visitor.Change(widthSegment));
                            var paddingArgument = this.GetQuotedValue(visitor.Change(paddingSegment));
                            return visitor.Merge(targetSegment, widthSegment, paddingSegment, $"LPAD({targetArgument},{widthArgument},{paddingArgument})", false, true);
                        });
                        result = true;
                    }
                    break;
                case "PadRight":
                    if (parameterInfos.Length == 1)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                            var widthSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (widthSegment.IsConstant || widthSegment.IsVariable))
                                return visitor.Merge(targetSegment, widthSegment, ((string)targetSegment.Value).PadRight((int)widthSegment.Value));

                            var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                            var widthArgument = this.GetQuotedValue(visitor.Change(widthSegment));
                            return visitor.Merge(targetSegment, widthSegment, $"RPAD({targetArgument},{widthArgument})", false, true);
                        });
                        result = true;
                    }
                    if (parameterInfos.Length > 1 && parameterInfos[1].ParameterType == typeof(int))
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                            var widthSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                            var paddingSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[1] });
                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (widthSegment.IsConstant || widthSegment.IsVariable)
                                && (paddingSegment.IsConstant || paddingSegment.IsVariable))
                                return visitor.Merge(targetSegment, widthSegment, paddingSegment, ((string)targetSegment.Value).PadRight((int)widthSegment.Value, (char)paddingSegment.Value));

                            var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                            var widthArgument = this.GetQuotedValue(visitor.Change(widthSegment));
                            var paddingArgument = this.GetQuotedValue(visitor.Change(paddingSegment));
                            return visitor.Merge(targetSegment, widthSegment, paddingSegment, $"RPAD({targetArgument},{widthArgument},{paddingArgument})", false, true);
                        });
                        result = true;
                    }
                    break;
                case "Replace":
                    if (parameterInfos.Length > 2 && parameterInfos[0].ParameterType == typeof(char))
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                            var oldSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                            var newSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[1] });
                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                    && (oldSegment.IsConstant || oldSegment.IsVariable)
                                    && (newSegment.IsConstant || newSegment.IsVariable))
                                return visitor.Merge(targetSegment, oldSegment, newSegment, ((string)targetSegment.Value).Replace((char)oldSegment.Value, (char)newSegment.Value));

                            var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                            var oldArgument = this.GetQuotedValue(visitor.Change(oldSegment));
                            var newArgument = this.GetQuotedValue(visitor.Change(newSegment));
                            return visitor.Merge(targetSegment, oldSegment, newSegment, $"REPLACE({targetArgument},{oldArgument},{newArgument})", false, true);
                        });
                        result = true;
                    }
                    if (parameterInfos.Length > 2 && parameterInfos[0].ParameterType == typeof(string))
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                            var oldSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                            var newSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[1] });
                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (oldSegment.IsConstant || oldSegment.IsVariable)
                                && (newSegment.IsConstant || newSegment.IsVariable))
                                return visitor.Merge(targetSegment, oldSegment, newSegment, ((string)targetSegment.Value).Replace((string)oldSegment.Value, (string)newSegment.Value));

                            var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                            var oldArgument = this.GetQuotedValue(visitor.Change(oldSegment));
                            var newArgument = this.GetQuotedValue(visitor.Change(newSegment));
                            return visitor.Merge(targetSegment, oldSegment, newSegment, $"REPLACE({targetArgument},{oldArgument},{newArgument})", false, true);
                        });
                        result = true;
                    }
                    break;
            }
        }
        return result;
    }
}
