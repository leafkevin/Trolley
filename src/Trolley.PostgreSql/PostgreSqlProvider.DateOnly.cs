#if NET6_0_OR_GREATER
using System;
using System.Globalization;
using System.Linq;
#endif
using System.Linq.Expressions;

namespace Trolley.PostgreSql;

partial class PostgreSqlProvider
{
    public override bool TryGetDateOnlyMemberAccessSqlFormatter(MemberExpression memberExpr, out Func<ISqlVisitor, SqlSegment, SqlSegment> formatter)
    {
        bool result = false;
        formatter = null;
#if NET6_0_OR_GREATER
        var memberInfo = memberExpr.Member;
        var cacheKey = HashCode.Combine(memberInfo.DeclaringType, memberInfo);
        if (memberExpr.Expression == null)
        {
            switch (memberInfo.Name)
            {
                //静态成员访问，理论上没有target对象，为了不再创建sqlSegment对象，外层直接把对象传了进来
                case "MinValue":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) => target.ChangeValue(DateOnly.MinValue, true));
                    result = true;
                    break;
                case "MaxValue":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) => target.ChangeValue(DateOnly.MaxValue, true));
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
                            targetSegment = exprFormatter.Invoke(visitor, callExpr,  null);
                        else targetSegment = visitor.Visit(target);

                        if (targetSegment.IsValue)
                            return targetSegment.Change(((DateOnly)targetSegment.Value).Day);

                        return targetSegment.Change($"EXTRACT(DAY FROM {targetSegment.Value})::INT4");
                    });
                    result = true;
                    break;
                case "DayOfWeek":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        SqlSegment targetSegment = default;
                        if (target.Expression is MethodCallExpression callExpr && callExpr.Object == null
                            && TryGetDateOnlyMethodCallSqlFormatter(callExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, callExpr,  null);
                        else targetSegment = visitor.Visit(target);

                        if (targetSegment.IsValue)
                            return targetSegment.Change(((DateOnly)targetSegment.Value).DayOfWeek);

                        return targetSegment.Change($"EXTRACT(DOW FROM {targetSegment.Value})::INT4");
                    });
                    result = true;
                    break;
                case "DayOfYear":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        SqlSegment targetSegment = default;
                        if (target.Expression is MethodCallExpression callExpr && callExpr.Object == null
                            && TryGetDateOnlyMethodCallSqlFormatter(callExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, callExpr,  null);
                        else targetSegment = visitor.Visit(target);

                        if (targetSegment.IsValue)
                            return targetSegment.Change(((DateOnly)targetSegment.Value).DayOfYear);

                        return targetSegment.Change($"EXTRACT(DOY FROM {targetSegment.Value})::INT4");
                    });
                    result = true;
                    break;
                case "Month":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        SqlSegment targetSegment = default;
                        if (target.Expression is MethodCallExpression callExpr && callExpr.Object == null
                            && TryGetDateOnlyMethodCallSqlFormatter(callExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, callExpr,  null);
                        else targetSegment = visitor.Visit(target);

                        if (targetSegment.IsValue)
                            return targetSegment.Change(((DateOnly)targetSegment.Value).Month);

                        return targetSegment.Change($"EXTRACT(MONTH FROM {targetSegment.Value})::INT4");
                    });
                    result = true;
                    break;
                case "Year":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        SqlSegment targetSegment = default;
                        if (target.Expression is MethodCallExpression callExpr && callExpr.Object == null
                            && TryGetDateOnlyMethodCallSqlFormatter(callExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, callExpr,  null);
                        else targetSegment = visitor.Visit(target);

                        if (targetSegment.IsValue)
                            return targetSegment.Change(((DateOnly)targetSegment.Value).Year);

                        return targetSegment.Change($"EXTRACT(YEAR FROM {targetSegment.Value})::INT4");
                    });
                    result = true;
                    break;
                case "DayNumber":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        SqlSegment targetSegment = default;
                        if (target.Expression is MethodCallExpression callExpr && callExpr.Object == null
                            && TryGetDateOnlyMethodCallSqlFormatter(callExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, callExpr,  null);
                        else targetSegment = visitor.Visit(target);

                        if (targetSegment.IsValue)
                            return targetSegment.Change(((DateOnly)targetSegment.Value).DayNumber);

                        return targetSegment.Change($"{targetSegment.Value}-DATE '0001-01-01'");
                    });
                    result = true;
                    break;
            }
        }
#endif
        return result;
    }
    public override bool TryGetDateOnlyMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out Func<ISqlVisitor, MethodCallExpression, Stack<DeferredOperation>, SqlSegment> formatter)
    {
        var result = false;
        formatter = null;
#if NET6_0_OR_GREATER
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

                        return valueSegment.Change($"{valueSegment.Value}::DATE");
                    });
                    result = true;
                    break;
                case "FromDayNumber":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var valueSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if (valueSegment.IsValue)
                            return valueSegment.Change(DateOnly.FromDayNumber(Convert.ToInt32(valueSegment.Value)));
                        var valueArgument = valueSegment.ToExprWrap();
                        return valueSegment.Change($"DATE '0001-01-01'+{valueArgument})");
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
                            var styleSegment = visitor.Visit(new SqlSegment { Expression = args[2] });

                            if (valueSegment.IsValue
                                && providerSegment.IsValue
                                &&styleSegment.IsValue)
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

                            if (valueSegment.IsValue
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
                        var providerSegment = visitor.Visit(new SqlSegment { Expression = args[2] });

                        if (valueSegment.IsValue
                            && formatSegment.IsValue
                            && providerSegment.IsValue)
                            return valueSegment.Change(DateOnly.ParseExact(valueSegment.Value.ToString(), formatSegment.Value.ToString(), (IFormatProvider)providerSegment.Value));

                        string formatArgument = null;
                        if (formatSegment.IsConstant)
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
                        else formatArgument = visitor.WrapSql(formatSegment);
                        var valueArgument = visitor.WrapSql(valueSegment);
                        return valueSegment.Change($"TO_DATE({valueArgument},{formatArgument})", SqlType.MethodCall);
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

                        var leftArgument = visitor.WrapSql(leftSegment);
                        var rightArgument = visitor.WrapSql(rightSegment);
                        return leftSegment.Change($"CASE WHEN {leftArgument}={rightArgument} THEN 0 WHEN {leftArgument}>{rightArgument} THEN 1 ELSE -1 END");
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
                        if (targetSegment.IsValue
                            && (rightSegment.IsValue))
                            return targetSegment.Change(((DateOnly)targetSegment.Value).AddDays(Convert.ToInt32(rightSegment.Value)));

                        var targetArgument = visitor.WrapSql(targetSegment, true);
                        var rightArgument = visitor.WrapSql(rightSegment, true);
                        return targetSegment.Change($"{targetArgument}+{rightArgument}");
                    });
                    result = true;
                    break;
                case "AddMonths":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                        var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if (targetSegment.IsValue
                           	&& (rightSegment.IsValue))
                            return targetSegment.Change(((DateOnly)targetSegment.Value).AddMonths(Convert.ToInt32(rightSegment.Value)));

                        var targetArgument = visitor.WrapSql(targetSegment, true);
                        var rightArgument = visitor.WrapSql(rightSegment, true);
                        return targetSegment.Change($"({targetArgument}+INTERVAL '1 MON'*{rightArgument})::DATE");
                    });
                    result = true;
                    break;
                case "AddYears":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                        var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if (targetSegment.IsValue
                           	&& (rightSegment.IsValue))
                            return targetSegment.Change(((DateOnly)targetSegment.Value).AddDays(Convert.ToInt32(rightSegment.Value)));

                        var targetArgument = visitor.WrapSql(targetSegment, true);
                        var rightArgument = visitor.WrapSql(rightSegment, true);
                        return targetSegment.Change($"({targetArgument}+INTERVAL '1Y'*{rightArgument})::DATE");
                    });
                    result = true;
                    break;
                case "Equals":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                        var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });

                        var targetArgument = visitor.WrapSql(targetSegment);
                        var rightArgument = visitor.WrapSql(rightSegment);
                        return targetSegment.Change($"{targetArgument}={rightArgument}");
                    });
                    result = true;
                    break;
                case "CompareTo":
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
                case "ToString":
                    if (parameterInfos.Length == 0)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                            if (targetSegment.IsValue)
                                return targetSegment.Change(targetSegment.Value.ToString());

                            return targetSegment.Change($"TO_CHAR({targetSegment.Value},'YYYY-MM-DD')", SqlType.MethodCall);
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
                            else formatArgument = visitor.WrapSql(formatSegment);

                            if (targetSegment.IsValue
                                && formatSegment.IsValue)
                                return targetSegment.Change(((DateOnly)targetSegment.Value).ToString(formatSegment.Value.ToString()));

                            var targetArgument = visitor.WrapSql(targetSegment);
                            return targetSegment.Change($"TO_CHAR({targetArgument},{formatArgument})", SqlType.MethodCall);
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

                            if (targetSegment.IsValue
                                && valueSegment.IsValue)
                                return targetSegment.Change(((DateOnly)targetSegment.Value).ToDateTime((TimeOnly)valueSegment.Value, (DateTimeKind)kindSegment.Value));

                            var targetArgument = visitor.WrapSql(targetSegment, true);
                            var valueArgument = visitor.WrapSql(valueSegment, true);
                            var timezone = (DateTimeKind)kindSegment.Value == DateTimeKind.Utc ? " AT TIME ZONE 'UTC'" : string.Empty;
                            return targetSegment.Change($"{targetArgument}+{valueArgument}{timezone}");
                        });
                    }
                    else
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                            var valueSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            if (targetSegment.IsValue
                                && valueSegment.IsValue)
                                return targetSegment.Change(((DateOnly)targetSegment.Value).ToDateTime((TimeOnly)valueSegment.Value));

                            var targetArgument = visitor.WrapSql(targetSegment, true);
                            var valueArgument = visitor.WrapSql(valueSegment, true);
                            return targetSegment.Change($"{targetArgument}+{valueArgument}");
                        });
                    }
                    result = true;
                    break;
            }
        }
#endif
        return result;
    }
}