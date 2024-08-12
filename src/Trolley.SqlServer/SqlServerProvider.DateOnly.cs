using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace Trolley.SqlServer;

partial class SqlServerProvider
{
    public override bool TryGetDateOnlyMemberAccessSqlFormatter(MemberExpression memberExpr, out MemberAccessSqlFormatter formatter)
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
                case "MinValue":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) => target.ChangeValue(DateOnly.MinValue, true));
                    result = true;
                    break;
                case "MaxValue":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) => target.ChangeValue(DateOnly.MaxValue, true));
                    result = true;
                    break;
            }
        }
        else
        {
            switch (memberInfo.Name)
            {
                case "Day":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlFieldSegment targetSegment = null;
                        if (target.Expression is MethodCallExpression callExpr && callExpr.Object == null
                            && TryGetDateOnlyMethodCallSqlFormatter(callExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, callExpr, callExpr.Object, null, callExpr.Arguments.ToArray());
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((DateOnly)targetSegment.Value).Day);

                        return targetSegment.Change($"DATEPART(DAY,{targetSegment.Body})", false, true);
                    });
                    result = true;
                    break;
                case "DayOfWeek":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlFieldSegment targetSegment = null;
                        if (target.Expression is MethodCallExpression callExpr && callExpr.Object == null
                            && TryGetDateOnlyMethodCallSqlFormatter(callExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, callExpr, callExpr.Object, null, callExpr.Arguments.ToArray());
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((DateOnly)targetSegment.Value).DayOfWeek);

                        return targetSegment.Change($"DATEPART(WEEKDAY,{targetSegment.Body})-1");
                    });
                    result = true;
                    break;
                case "DayOfYear":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlFieldSegment targetSegment = null;
                        if (target.Expression is MethodCallExpression callExpr && callExpr.Object == null
                            && TryGetDateOnlyMethodCallSqlFormatter(callExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, callExpr, callExpr.Object, null, callExpr.Arguments.ToArray());
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((DateOnly)targetSegment.Value).DayOfYear);

                        return targetSegment.Change($"DATEPART(DAYOFYEAR,{targetSegment.Body})", false, true);
                    });
                    result = true;
                    break;
                case "Month":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlFieldSegment targetSegment = null;
                        if (target.Expression is MethodCallExpression callExpr && callExpr.Object == null
                            && TryGetDateOnlyMethodCallSqlFormatter(callExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, callExpr, callExpr.Object, null, callExpr.Arguments.ToArray());
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((DateOnly)targetSegment.Value).Month);

                        return targetSegment.Change($"DATEPART(MONTH,{targetSegment.Body})", false, true);
                    });
                    result = true;
                    break;
                case "Year":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlFieldSegment targetSegment = null;
                        if (target.Expression is MethodCallExpression callExpr && callExpr.Object == null
                            && TryGetDateOnlyMethodCallSqlFormatter(callExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, callExpr, callExpr.Object, null, callExpr.Arguments.ToArray());
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((DateOnly)targetSegment.Value).Year);

                        return targetSegment.Change($"DATEPART(YEAR,{targetSegment.Body})", false, true);
                    });
                    result = true;
                    break;
                case "DayNumber":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlFieldSegment targetSegment = null;
                        if (target.Expression is MethodCallExpression callExpr && callExpr.Object == null
                            && TryGetDateOnlyMethodCallSqlFormatter(callExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, callExpr, callExpr.Object, null, callExpr.Arguments.ToArray());
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((DateOnly)targetSegment.Value).DayNumber);

                        return targetSegment.Change($"DATEDIFF(DAY,'0001-01-01',{targetSegment.Body})", false, true);
                    });
                    result = true;
                    break;
            }
        }
        return result;
    }
    public override bool TryGetDateOnlyMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter)
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
                case "FromDateTime":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        if (valueSegment.IsConstant || valueSegment.IsVariable)
                            return valueSegment.ChangeValue(DateOnly.FromDateTime((DateTime)valueSegment.Value));

                        return valueSegment.Change($"CAST({valueSegment.Body} AS DATE)", false, true);
                    });
                    result = true;
                    break;
                case "FromDayNumber":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        if (valueSegment.IsConstant || valueSegment.IsVariable)
                            return valueSegment.ChangeValue(DateOnly.FromDayNumber(Convert.ToInt32(valueSegment.Value)));

                        return valueSegment.Change($"CAST(DATEADD(DAY,{valueSegment.Body},'0001-01-01') AS DATE)", false, true);
                    });
                    result = true;
                    break;
                case "Parse":
                case "TryParse":
                    if (parameterInfos.Length == 3)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var valueSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                            var providerSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[1] });
                            var styleSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[2] });

                            if ((valueSegment.IsConstant || valueSegment.IsVariable)
                                && (providerSegment.IsConstant || providerSegment.IsVariable)
                                && (styleSegment.IsConstant || styleSegment.IsVariable))
                                return valueSegment.ChangeValue(DateOnly.Parse(valueSegment.Value.ToString(), (IFormatProvider)providerSegment.Value, (DateTimeStyles)styleSegment.Value));

                            return valueSegment.Change($"CAST({valueSegment.Body} AS DATE)", false, true);
                        });
                    }
                    else if (parameterInfos.Length == 2)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var valueSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                            var providerSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[1] });

                            if ((valueSegment.IsConstant || valueSegment.IsVariable)
                                && (providerSegment.IsConstant || providerSegment.IsVariable))
                                return valueSegment.ChangeValue(DateOnly.Parse(valueSegment.Value.ToString(), (IFormatProvider)providerSegment.Value));

                            return valueSegment.Change($"CAST({valueSegment.Body} AS DATE)", false, true);
                        });
                    }
                    else
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var valueSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                            if (valueSegment.IsConstant || valueSegment.IsVariable)
                                return valueSegment.ChangeValue(DateOnly.Parse(valueSegment.Value.ToString()));

                            return valueSegment.Change($"CAST({valueSegment.Body} AS DATE)", false, true);
                        });
                    }
                    result = true;
                    if (methodInfo.IsStatic && parameterInfos.Length >= 3 && parameterInfos[0].ParameterType == typeof(ReadOnlySpan<char>))
                        throw new NotSupportedException("DateOnly.Parse方法暂时不支持ReadOnlySpan<char>类型参数的解析，请转换成String类型");
                    break;
                case "ParseExact":
                case "TryParseExact":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        var formatSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[1] });
                        var providerSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[2] });

                        if ((valueSegment.IsConstant || valueSegment.IsVariable)
                            && (formatSegment.IsConstant || formatSegment.IsVariable)
                            && (providerSegment.IsConstant || providerSegment.IsVariable))
                            return valueSegment.MergeValue(formatSegment, DateOnly.ParseExact(valueSegment.Value.ToString(), formatSegment.Value.ToString(), (IFormatProvider)providerSegment.Value));

                        if (!(formatSegment.IsConstant || formatSegment.IsVariable))
                            throw new NotSupportedException($"方法DateOnly.{methodInfo.Name}格式化字符串，暂时不支持非常量、变量的解析");

                        var valueArgument = visitor.GetQuotedValue(valueSegment);
                        var format = formatSegment.Value.ToString();
                        string formatValue = null;
                        switch (format)
                        {
                            case "mm/dd/yyyy": formatValue = $"CONVERT(DATE,{valueArgument},101)"; break;
                            case "yyyy.mm.dd": formatValue = $"CONVERT(DATE,{valueArgument},102)"; break;
                            case "dd/mm/yyyy": formatValue = $"CONVERT(DATE,{valueArgument},103)"; break;
                            case "dd.mm.yyyy": formatValue = $"CONVERT(DATE,{valueArgument},104)"; break;
                            case "dd-mm-yyyy": formatValue = $"CONVERT(DATE,{valueArgument},105)"; break;
                            case "dd mon yyyy": formatValue = $"CONVERT(DATE,{valueArgument},106)"; break;
                            case "mon dd, yyyy": formatValue = $"CONVERT(DATE,{valueArgument},107)"; break;
                            case "mm-dd-yyyy": formatValue = $"CONVERT(DATE,{valueArgument},110)"; break;
                            case "yyyy/mm/dd": formatValue = $"CONVERT(DATE,{valueArgument},111)"; break;
                            case "yyyymmdd": formatValue = $"CONVERT(DATE,{valueArgument},112)"; break;
                            default: formatValue = $"CAST({valueArgument} AS DATE)"; break;
                        }
                        return valueSegment.Merge(formatSegment, formatValue, false, true);
                    });
                    result = true;
                    if (methodInfo.IsStatic && parameterInfos.Length >= 1 && parameterInfos[0].ParameterType == typeof(ReadOnlySpan<char>))
                        throw new NotSupportedException($"DateOnly.{methodInfo.Name}方法暂时不支持ReadOnlySpan<char>类型参数的解析，请转换成String类型");
                    break;
                case "Compare":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var leftSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        var rightSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[1] });

                        var leftArgument = visitor.GetQuotedValue(leftSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return leftSegment.Merge(rightSegment, $"CASE WHEN {leftArgument}={rightArgument} THEN 0 WHEN {leftArgument}>{rightArgument} THEN 1 ELSE -1 END");
                    });
                    result = true;
                    break;
            }
        }
        else
        {
            switch (methodInfo.Name)
            {
                case "AddDays":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return targetSegment.MergeValue(rightSegment, ((DateOnly)targetSegment.Value).AddDays(Convert.ToInt32(rightSegment.Value)));

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(rightSegment, $"DATEADD(DAY,{rightArgument},{targetArgument})", false, true);
                    });
                    result = true;
                    break;
                case "AddMonths":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return targetSegment.MergeValue(rightSegment, ((DateOnly)targetSegment.Value).AddMonths(Convert.ToInt32(rightSegment.Value)));

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(rightSegment, $"DATEADD(MONTH,{rightArgument},{targetArgument})", false, true);
                    });
                    result = true;
                    break;
                case "AddYears":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return targetSegment.MergeValue(rightSegment, ((DateOnly)targetSegment.Value).AddDays(Convert.ToInt32(rightSegment.Value)));

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(rightSegment, $"DATEADD(YEAR,{rightArgument},{targetArgument})", false, true);
                    });
                    result = true;
                    break;
                case "Equals":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(rightSegment, $"{targetArgument}={rightArgument}");
                    });
                    result = true;
                    break;
                case "CompareTo":
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
                case "ToString":
                    if (parameterInfos.Length == 0)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                            if (targetSegment.IsConstant || targetSegment.IsVariable)
                                return targetSegment.ChangeValue(targetSegment.Value.ToString());

                            return targetSegment.Change($"CONVERT({targetSegment.Body},'YYYY-MM-DD')", false, true);
                        });
                        result = true;
                    }
                    if (parameterInfos.Length == 1)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                            var formatSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });

                            string formatArgument = null;
                            if (formatSegment.IsConstant || formatSegment.IsVariable)
                            {
                                formatArgument = $"'{formatSegment.Value}'";

                                if (formatArgument.Contains("yyyy"))
                                    formatArgument = formatArgument.NextReplace("yyyy", "YYYY");
                                else if (formatArgument.Contains("yyy"))
                                    formatArgument = formatArgument.NextReplace("yyy", "YYY");
                                else if (formatArgument.Contains("yy"))
                                    formatArgument = formatArgument.NextReplace("yy", "YY");

                                if (formatArgument.Contains("MMMM"))
                                    formatArgument = formatArgument.NextReplace("MMMM", "Month");
                                else if (formatArgument.Contains("MMM"))
                                    formatArgument = formatArgument.NextReplace("MMM", "Mon");
                                else if (formatArgument.Contains("M") && !formatArgument.Contains("MM"))
                                    formatArgument = formatArgument.NextReplace("M", "FMMM");

                                if (formatArgument.Contains("dddd"))
                                    formatArgument = formatArgument.NextReplace("dddd", "Day");
                                else if (formatArgument.Contains("ddd"))
                                    formatArgument = formatArgument.NextReplace("ddd", "DY");
                                else if (formatArgument.Contains("dd"))
                                    formatArgument = formatArgument.NextReplace("dd", "DD");
                                else if (formatArgument.Contains("d"))
                                    formatArgument = formatArgument.NextReplace("d", "FMDD");
                            }
                            else formatArgument = visitor.GetQuotedValue(formatSegment);

                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (formatSegment.IsConstant || formatSegment.IsVariable))
                                return targetSegment.MergeValue(formatSegment, ((DateOnly)targetSegment.Value).ToString(formatSegment.Value.ToString()));

                            var targetArgument = visitor.GetQuotedValue(targetSegment);
                            if (formatSegment.IsConstant || formatSegment.IsVariable)
                                return targetSegment.Merge(formatSegment, $"FORMAT({targetArgument},{formatArgument})", false, true);

                            throw new NotSupportedException("DateOnly类型暂时不支持非常量的格式化字符串");
                        });
                        result = true;
                    }
                    break;
                case "ToDateTime":
                    if (parameterInfos.Length > 1)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                            var valueSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                            var kindSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[1] });
                            if (!kindSegment.IsConstant && !kindSegment.IsVariable)
                                throw new NotSupportedException($"DateOnly.{methodInfo.Name}方法暂时仅支持第二个参数是常量或是变量的解析");

                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (valueSegment.IsConstant || valueSegment.IsVariable))
                                return targetSegment.MergeValue(valueSegment, ((DateOnly)targetSegment.Value).ToDateTime((TimeOnly)valueSegment.Value, (DateTimeKind)kindSegment.Value));

                            throw new NotSupportedException("DateOnly类型ToDateTime方法暂时不支持非常量的参数解析");
                        });
                    }
                    else
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                            var valueSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (valueSegment.IsConstant || valueSegment.IsVariable))
                                return targetSegment.MergeValue(valueSegment, ((DateOnly)targetSegment.Value).ToDateTime((TimeOnly)valueSegment.Value));

                            throw new NotSupportedException("DateOnly类型ToDateTime方法暂时不支持非常量的参数解析");
                        });
                    }
                    result = true;
                    break;
            }
        }
        return result;
    }
}