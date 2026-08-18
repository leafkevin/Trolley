//#if NET6_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
//#endif
using System.Linq.Expressions;

namespace Trolley.MySqlConnector;

partial class MySqlProvider
{
    public override bool TryGetDateOnlyMemberAccessSqlFormatter(MemberExpression memberExpr, out Func<ISqlVisitor, SqlSegment, SqlSegment> formatter)
    {
        bool result = false;
        formatter = null;
        //#if NET6_0_OR_GREATER
        var memberInfo = memberExpr.Member;
        var cacheKey = HashCode.Combine(memberInfo.DeclaringType, memberInfo);
        if (memberExpr.Expression == null)
        {
            switch (memberInfo.Name)
            {
                //静态成员访问，理论上没有target对象，为了不再创建sqlSegment对象，外层直接把对象传了进来
                case "MinValue":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) => target.Change(DateOnly.MinValue, SqlType.Constant));
                    result = true;
                    break;
                case "MaxValue":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) => target.Change(DateOnly.MaxValue, SqlType.Constant));
                    result = true;
                    break;
            }
        }
        else
        {
            switch (memberInfo.Name)
            {
                case "Day":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        SqlSegment targetSegment = default;
                        if (target.Expression is MethodCallExpression callExpr && callExpr.Object == null
                            && TryGetDateOnlyMethodCallSqlFormatter(callExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, callExpr, null);
                        else targetSegment = visitor.Visit(target);

                        if (targetSegment.IsValue)
                            return targetSegment.Change(((DateOnly)targetSegment.Value).Day);

                        return targetSegment.Change($"DAYOFMONTH({targetSegment.Value})", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "DayOfWeek":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        SqlSegment targetSegment = default;
                        if (target.Expression is MethodCallExpression callExpr && callExpr.Object == null
                            && TryGetDateOnlyMethodCallSqlFormatter(callExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, callExpr, null);
                        else targetSegment = visitor.Visit(target);

                        if (targetSegment.IsValue)
                            return targetSegment.Change(((DateOnly)targetSegment.Value).DayOfWeek);

                        return targetSegment.Change($"DAYOFWEEK({targetSegment.Value})-1", SqlType.Expression);
                    });
                    result = true;
                    break;
                case "DayOfYear":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlSegment targetSegment = default;
                        if (target.Expression is MethodCallExpression callExpr && callExpr.Object == null
                            && TryGetDateOnlyMethodCallSqlFormatter(callExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, callExpr, null);
                        else targetSegment = visitor.Visit(target);

                        if (targetSegment.IsValue)
                            return targetSegment.Change(((DateOnly)targetSegment.Value).DayOfYear);

                        return targetSegment.Change($"DAYOFYEAR({targetSegment.Value})", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "Month":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlSegment targetSegment = default;
                        if (target.Expression is MethodCallExpression callExpr && callExpr.Object == null
                            && TryGetDateOnlyMethodCallSqlFormatter(callExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, callExpr, null);
                        else targetSegment = visitor.Visit(target);

                        if (targetSegment.IsValue)
                            return targetSegment.Change(((DateOnly)targetSegment.Value).Month);

                        return targetSegment.Change($"MONTH({targetSegment.Value})", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "Year":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlSegment targetSegment = default;
                        if (target.Expression is MethodCallExpression callExpr && callExpr.Object == null
                            && TryGetDateOnlyMethodCallSqlFormatter(callExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, callExpr, null);
                        else targetSegment = visitor.Visit(target);

                        if (targetSegment.IsValue)
                            return targetSegment.Change(((DateOnly)targetSegment.Value).Year);

                        return targetSegment.Change($"YEAR({targetSegment.Value})", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "DayNumber":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlSegment targetSegment = default;
                        if (target.Expression is MethodCallExpression callExpr && callExpr.Object == null
                            && TryGetDateOnlyMethodCallSqlFormatter(callExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, callExpr, null);
                        else targetSegment = visitor.Visit(target);

                        if (targetSegment.IsValue)
                            return targetSegment.Change(((DateOnly)targetSegment.Value).DayNumber);

                        return targetSegment.Change($"DATEDIFF({targetSegment.Value},'0001-01-01')", SqlType.MethodCall);
                    });
                    result = true;
                    break;
            }
        }
        //#endif
        return result;
    }
    public override bool TryGetDateOnlyMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out Func<ISqlVisitor, MethodCallExpression, Stack<DeferredOperation>, SqlSegment> formatter)
    {
        var result = false;
        formatter = null;
        //#if NET6_0_OR_GREATER
        var methodInfo = methodCallExpr.Method;
        var parameterInfos = methodInfo.GetParameters();
        var cacheKey = HashCode.Combine(methodInfo.DeclaringType, methodInfo);
        if (methodInfo.IsStatic)
        {
            switch (methodInfo.Name)
            {
                case "FromDateTime":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var valueSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if (valueSegment.IsValue)
                            return valueSegment.Change(DateOnly.FromDateTime((DateTime)valueSegment.Value));

                        return valueSegment.Change($"DATE({valueSegment.Value})", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "FromDayNumber":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var valueSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if (valueSegment.IsValue)
                            return valueSegment.Change(DateOnly.FromDayNumber(Convert.ToInt32(valueSegment.Value)));

                        return valueSegment.Change($"DATE_ADD('0001-01-01',INTERVAL {valueSegment.Value} DAY)", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "Parse":
                case "TryParse":
                    if (parameterInfos.Length == 3)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var valueSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            var providerSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[1] });
                            var styleSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[2] });

                            if (valueSegment.IsValue && providerSegment.IsValue && styleSegment.IsValue)
                                return valueSegment.Change(DateOnly.Parse(valueSegment.Value.ToString(), (IFormatProvider)providerSegment.Value, (DateTimeStyles)styleSegment.Value));

                            return valueSegment.Change($"CAST({valueSegment.Value} AS DATE)", SqlType.MethodCall);
                        });
                    }
                    else if (parameterInfos.Length == 2)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var valueSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            var providerSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[1] });

                            if ((valueSegment.IsValue)
                                && providerSegment.IsValue)
                                return valueSegment.Change(DateOnly.Parse(valueSegment.Value.ToString(), (IFormatProvider)providerSegment.Value));

                            return valueSegment.Change($"CAST({valueSegment.Value} AS DATE)", SqlType.MethodCall);
                        });
                    }
                    else
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var valueSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            if (valueSegment.IsValue)
                                return valueSegment.Change(DateOnly.Parse(valueSegment.Value.ToString()));

                            return valueSegment.Change($"CAST({valueSegment.Value} AS DATE)", SqlType.MethodCall);
                        });
                    }
                    result = true;
                    if (methodInfo.IsStatic && parameterInfos.Length >= 3 && parameterInfos[0].ParameterType == typeof(ReadOnlySpan<char>))
                        throw new NotSupportedException("DateOnly.Parse方法暂时不支持ReadOnlySpan<char>类型参数的解析，请转换成String类型");
                    break;
                case "ParseExact":
                case "TryParseExact":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var valueSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        var formatSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[1] });
                        var providerSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[2] });

                        if (valueSegment.IsValue && formatSegment.IsValue && providerSegment.IsValue)
                            return valueSegment.MergeValue(formatSegment, DateOnly.ParseExact(valueSegment.Value.ToString(), formatSegment.Value.ToString(), (IFormatProvider)providerSegment.Value));

                        string formatArgument = null;
                        if (formatSegment.IsConstant)
                        {
                            formatArgument = $"'{formatSegment.Value}'";

                            if (formatArgument.Contains("mm"))
                                formatArgument = formatArgument.Replace("mm", "%i");
                            else formatArgument = formatArgument.Replace("m", "%i");

                            if (formatArgument.Contains("yyyy"))
                                formatArgument = formatArgument.Replace("yyyy", "%Y");
                            else if (formatArgument.Contains("yyy"))
                                formatArgument = formatArgument.Replace("yyy", "%Y");
                            else if (formatArgument.Contains("yy"))
                                formatArgument = formatArgument.Replace("yy", "%y");

                            if (formatArgument.Contains("MMMM"))
                                formatArgument = formatArgument.Replace("MMMM", "%M");
                            else if (formatArgument.Contains("MMM"))
                                formatArgument = formatArgument.Replace("MMM", "%b");
                            else if (formatArgument.Contains("MM"))
                                formatArgument = formatArgument.Replace("MM", "%m");
                            else if (formatArgument.Contains("M"))
                                formatArgument = formatArgument.Replace("M", "%c");

                            if (formatArgument.Contains("dddd"))
                                formatArgument = formatArgument.Replace("dddd", "%W");
                            else if (formatArgument.Contains("ddd"))
                                formatArgument = formatArgument.Replace("ddd", "%a");
                            else if (formatArgument.Contains("dd"))
                                formatArgument = formatArgument.Replace("dd", "%d");
                            else if (formatArgument.Contains("d"))
                                formatArgument = formatArgument.Replace("d", "%e");
                        }
                        else formatArgument = visitor.GetQuotedValue(formatSegment);
                        var valueArgument = visitor.GetQuotedValue(valueSegment);
                        return valueSegment.Merge(formatSegment, $"STR_TO_DATE({valueArgument},{formatArgument})", SqlType.MethodCall);
                    });
                    result = true;
                    if (methodInfo.IsStatic && parameterInfos.Length >= 1 && parameterInfos[0].ParameterType == typeof(ReadOnlySpan<char>))
                        throw new NotSupportedException($"DateOnly.{methodInfo.Name}方法暂时不支持ReadOnlySpan<char>类型参数的解析，请转换成String类型");
                    break;
                case "Compare":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var leftSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[1] });

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
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                        var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if (targetSegment.IsValue && rightSegment.IsValue)
                            return targetSegment.MergeValue(rightSegment, ((DateOnly)targetSegment.Value).AddDays(Convert.ToInt32(rightSegment.Value)));

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(rightSegment, $"DATE_ADD({targetArgument},INTERVAL {rightArgument} DAY)", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "AddMonths":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                        var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if (targetSegment.IsValue && rightSegment.IsValue)
                            return targetSegment.MergeValue(rightSegment, ((DateOnly)targetSegment.Value).AddMonths(Convert.ToInt32(rightSegment.Value)));

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(rightSegment, $"DATE_ADD({targetArgument},INTERVAL {rightArgument} MONTH)", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "AddYears":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                        var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if (targetSegment.IsValue && rightSegment.IsValue)
                            return targetSegment.MergeValue(rightSegment, ((DateOnly)targetSegment.Value).AddDays(Convert.ToInt32(rightSegment.Value)));

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(rightSegment, $"DATE_ADD({targetArgument},INTERVAL {rightArgument} YEAR)", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "Equals":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                        var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(rightSegment, $"{targetArgument}={rightArgument}");
                    });
                    result = true;
                    break;
                case "CompareTo":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                        var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(rightSegment, $"CASE WHEN {targetArgument}={rightArgument} THEN 0 WHEN {targetArgument}>{rightArgument} THEN 1 ELSE -1 END");
                    });
                    result = true;
                    break;
                case "ToString":
                    if (parameterInfos.Length == 0)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                            if (targetSegment.IsValue)
                                return targetSegment.Change(targetSegment.Value.ToString());

                            return targetSegment.Change($"DATE_FORMAT({targetSegment.Value},'%Y-%m-%d')", SqlType.MethodCall);
                        });
                        result = true;
                    }
                    if (parameterInfos.Length == 1)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                            var formatSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });

                            string formatArgument = null;
                            if formatSegment.IsValue
                            {
                                formatArgument = $"'{formatSegment.Value}'";

                                if (formatArgument.Contains("yyyy"))
                                    formatArgument = formatArgument.Replace("yyyy", "YYYY");
                                else if (formatArgument.Contains("yyy"))
                                    formatArgument = formatArgument.Replace("yyy", "YYY");
                                else if (formatArgument.Contains("yy"))
                                    formatArgument = formatArgument.Replace("yy", "YY");

                                if (formatArgument.Contains("MMMM"))
                                    formatArgument = formatArgument.Replace("MMMM", "Month");
                                else if (formatArgument.Contains("MMM"))
                                    formatArgument = formatArgument.Replace("MMM", "Mon");
                                else if (formatArgument.Contains("M") && !formatArgument.Contains("MM"))
                                    formatArgument = formatArgument.Replace("M", "FMMM");

                                if (formatArgument.Contains("dddd"))
                                    formatArgument = formatArgument.Replace("dddd", "Day");
                                else if (formatArgument.Contains("ddd"))
                                    formatArgument = formatArgument.Replace("ddd", "DY");
                                else if (formatArgument.Contains("dd"))
                                    formatArgument = formatArgument.Replace("dd", "DD");
                                else if (formatArgument.Contains("d"))
                                    formatArgument = formatArgument.Replace("d", "FMDD");
                            }
                            else formatArgument = visitor.GetQuotedValue(formatSegment);

                            if ((targetSegment.IsValue)
                                && formatSegment.IsValue)
                                return targetSegment.MergeValue(formatSegment, ((DateOnly)targetSegment.Value).ToString(formatSegment.Value.ToString()));

                            var targetArgument = visitor.GetQuotedValue(targetSegment);
                            return targetSegment.Merge(formatSegment, $"DATE_FORMAT({targetArgument},{formatArgument})", SqlType.MethodCall);
                        });
                        result = true;
                    }
                    break;
                case "ToDateTime":
                    if (parameterInfos.Length > 1)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                            var valueSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            var kindSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[1] });
                            if (!kindSegment.IsConstant && !kindSegment.IsVariable)
                                throw new NotSupportedException($"DateOnly.{methodInfo.Name}方法暂时仅支持第二个参数是常量或是变量的解析");

                            if ((targetSegment.IsValue)
                                && (valueSegment.IsValue))
                                return targetSegment.MergeValue(valueSegment, ((DateOnly)targetSegment.Value).ToDateTime((TimeOnly)valueSegment.Value, (DateTimeKind)kindSegment.Value));

                            var targetArgument = visitor.GetQuotedValue(targetSegment);
                            var valueArgument = visitor.GetQuotedValue(valueSegment);
                            var timezone = $"TIMESTAMP({targetArgument},{valueArgument})";
                            if ((DateTimeKind)kindSegment.Value == DateTimeKind.Utc)
                                timezone = $"CONVERT_TZ({timezone},'SYSTEM','UTC')";
                            return targetSegment.Merge(valueSegment, timezone, false, true);
                        });
                    }
                    else
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                            var valueSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            if ((targetSegment.IsValue)
                                && (valueSegment.IsValue))
                                return targetSegment.MergeValue(valueSegment, ((DateOnly)targetSegment.Value).ToDateTime((TimeOnly)valueSegment.Value));

                            var targetArgument = visitor.GetQuotedValue(targetSegment);
                            var valueArgument = visitor.GetQuotedValue(valueSegment);
                            return targetSegment.Merge(valueSegment, $"TIMESTAMP({targetArgument},{valueArgument})", SqlType.MethodCall);
                        });
                    }
                    result = true;
                    break;
            }
        }
        //#endif
        return result;
    }
}