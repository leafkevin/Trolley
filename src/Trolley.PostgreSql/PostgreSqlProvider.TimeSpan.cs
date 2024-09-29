using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.PostgreSql;

partial class PostgreSqlProvider
{
    public override bool TryGetTimeSpanMemberAccessSqlFormatter(MemberExpression memberExpr, out MemberAccessSqlFormatter formatter)
    {
        bool result = false;
        formatter = null;
        var memberInfo = memberExpr.Member;
        var cacheKey = RepositoryHelper.GetCacheKey(memberInfo.DeclaringType, memberInfo);
        if (memberExpr.Expression == null)
        {
            switch (memberInfo.Name)
            {
                //静态成员访问，理论上没有target对象，为了不再创建sqlSegment对象，外层直接把对象传了进来
                case "MinValue":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) => target.ChangeValue(TimeSpan.MinValue, true));
                    result = true;
                    break;
                case "MaxValue":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) => target.ChangeValue(TimeSpan.MaxValue, true));
                    result = true;
                    break;
                case "Zero":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) => target.ChangeValue(TimeSpan.Zero, true));
                    result = true;
                    break;
            }
        }
        else
        {
            switch (memberInfo.Name)
            {
                case "Ticks":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((TimeSpan)targetSegment.Value).Ticks);

                        return targetSegment.Change($"(EXTRACT(EPOCH FROM {targetSegment.Body})*10000000)::INT8");
                    });
                    result = true;
                    break;
                case "Days":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((TimeSpan)targetSegment.Value).Days);

                        return targetSegment.Change($"EXTRACT(DAY FROM {targetSegment.Body})::INT4");
                    });
                    result = true;
                    break;
                case "Hours":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((TimeSpan)targetSegment.Value).Hours);

                        return targetSegment.Change($"EXTRACT(HOUR FROM {targetSegment.Body})::INT4");
                    });
                    result = true;
                    break;
                case "Milliseconds":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((TimeSpan)targetSegment.Value).Milliseconds);

                        return targetSegment.Change($"(EXTRACT(SECOND FROM {targetSegment.Body})*1000)::INT4");
                    });
                    result = true;
                    break;
                case "Minutes":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((TimeSpan)targetSegment.Value).Minutes);

                        return targetSegment.Change($"EXTRACT(MINUTE FROM {targetSegment.Body})::INT4");
                    });
                    result = true;
                    break;
                case "Seconds":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((TimeSpan)targetSegment.Value).Seconds);

                        return targetSegment.Change($"EXTRACT(SECOND FROM {targetSegment.Body})::INT4");
                    });
                    result = true;
                    break;
                case "TotalDays":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((TimeSpan)targetSegment.Value).TotalDays);

                        return targetSegment.Change($"(EXTRACT(EPOCH FROM {targetSegment.Body})/{3600 * 24})::FLOAT8");
                    });
                    result = true;
                    break;
                case "TotalHours":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((TimeSpan)targetSegment.Value).TotalHours);

                        return targetSegment.Change($"(EXTRACT(EPOCH FROM {targetSegment.Body})/3600)::FLOAT8");
                    });
                    result = true;
                    break;
                case "TotalMilliseconds":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((TimeSpan)targetSegment.Value).TotalMilliseconds);

                        return targetSegment.Change($"(EXTRACT(EPOCH FROM {targetSegment.Body})*1000)::FLOAT8");
                    });
                    result = true;
                    break;
                case "TotalMinutes":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((TimeSpan)targetSegment.Value).TotalMinutes);

                        return targetSegment.Change($"(EXTRACT(EPOCH FROM {targetSegment.Body})/60)::FLOAT8");
                    });
                    result = true;
                    break;
                case "TotalSeconds":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((TimeSpan)targetSegment.Value).TotalSeconds);

                        return targetSegment.Change($"EXTRACT(EPOCH FROM {targetSegment.Body})::FLOAT8", false, true);
                    });
                    result = true;
                    break;
            }
        }
        return result;
    }
    public override bool TryGetTimeSpanMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter)
    {
        var result = false;
        formatter = null;
        var methodInfo = methodCallExpr.Method;
        var parameterInfos = methodInfo.GetParameters();
        var cacheKey = RepositoryHelper.GetCacheKey(methodInfo.DeclaringType, methodInfo);
        if (methodInfo.IsStatic)
        {
            switch (methodInfo.Name)
            {
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
                case "Equals":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var leftSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        var rightSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[1] });

                        var leftArgument = visitor.GetQuotedValue(leftSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return leftSegment.Merge(rightSegment, $"{leftArgument}={rightArgument}");
                    });
                    result = true;
                    break;
                case "FromDays":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        if (valueSegment.IsConstant || valueSegment.IsVariable)
                            return valueSegment.ChangeValue(TimeSpan.FromDays(Convert.ToDouble(valueSegment.Value)));

                        return valueSegment.Change($"INTERVAL '1D'*{valueSegment.ToExprWrap()}");
                    });
                    result = true;
                    break;
                case "FromHours":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        if (valueSegment.IsConstant || valueSegment.IsVariable)
                            return valueSegment.ChangeValue(TimeSpan.FromHours(Convert.ToDouble(valueSegment.Value)));

                        return valueSegment.Change($"INTERVAL '1H'*{valueSegment.ToExprWrap()}");
                    });
                    result = true;
                    break;
                case "FromMilliseconds":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        if (valueSegment.IsConstant || valueSegment.IsVariable)
                            return valueSegment.ChangeValue(TimeSpan.FromMilliseconds(Convert.ToDouble(valueSegment.Value)));

                        return valueSegment.Change($"INTERVAL '1S'*{valueSegment.ToExprWrap()}/1000");
                    });
                    result = true;
                    break;
                case "FromMinutes":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        if (valueSegment.IsConstant || valueSegment.IsVariable)
                            return valueSegment.ChangeValue(TimeSpan.FromMinutes(Convert.ToDouble(valueSegment.Value)));

                        return valueSegment.Change($"INTERVAL '1M'*{valueSegment.ToExprWrap()}");
                    });
                    result = true;
                    break;
                case "FromSeconds":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        if (valueSegment.IsConstant || valueSegment.IsVariable)
                            return valueSegment.ChangeValue(TimeSpan.FromSeconds(Convert.ToDouble(valueSegment.Value)));

                        return valueSegment.Change($"INTERVAL '1S'*{valueSegment.ToExprWrap()}");
                    });
                    result = true;
                    break;
                case "FromTicks":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        if (valueSegment.IsConstant || valueSegment.IsVariable)
                            return valueSegment.ChangeValue(TimeSpan.FromTicks(Convert.ToInt64(valueSegment.Value)));

                        return valueSegment.Change($"INTERVAL '1S'*{valueSegment.ToExprWrap()}/{TimeSpan.TicksPerSecond}");
                    });
                    result = true;
                    break;
                case "Parse":
                case "TryParse":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        if (valueSegment.IsConstant || valueSegment.IsVariable)
                            return valueSegment.ChangeValue(TimeSpan.Parse(valueSegment.Value.ToString()));

                        return valueSegment.Change($"'{valueSegment.Body}'::INTERVAL");
                    });
                    result = true;
                    break;
                case "ParseExact":
                case "TryParseExact":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        var formatSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[1] });
                        if ((valueSegment.IsConstant || valueSegment.IsVariable)
                            && (formatSegment.IsConstant || formatSegment.IsVariable))
                            return valueSegment.MergeValue(formatSegment, TimeSpan.ParseExact(valueSegment.Value.ToString(), formatSegment.Value.ToString(), CultureInfo.InvariantCulture));

                        string formatArgument = null;
                        if (formatSegment.IsConstant || formatSegment.IsVariable)
                        {
                            formatArgument = $"'{formatSegment.Value}'";

                            if (formatArgument.Contains("HH"))
                                formatArgument = formatArgument.NextReplace("HH", "HH24");
                            else if (formatArgument.Contains("H"))
                                formatArgument = formatArgument.NextReplace("H", "FMHH24");
                            else if (formatArgument.Contains("hh"))
                                formatArgument = formatArgument.NextReplace("hh", "HH12");
                            else if (formatArgument.Contains("h"))
                                formatArgument = formatArgument.NextReplace("h", "FMHH12");

                            if (formatArgument.Contains("mm"))
                                formatArgument = formatArgument.NextReplace("mm", "MI");
                            else formatArgument = formatArgument.NextReplace("m", "FMMI");

                            if (formatArgument.Contains("ss"))
                                formatArgument = formatArgument.NextReplace("ss", "SS");
                            else if (formatArgument.Contains("s"))
                                formatArgument = formatArgument.NextReplace("s", "FMSS");

                            if (formatArgument.Contains("tt"))
                                formatArgument = formatArgument.NextReplace("tt", "AM");
                            else if (formatArgument.Contains("t"))
                                formatArgument = formatArgument.NextReplace("t", "AM");

                            if (formatArgument.Contains("FFFFFF"))
                                formatArgument = formatArgument.NextReplace("FFFFFF", "US");
                            else if (formatArgument.Contains("FFFFF"))
                                formatArgument = formatArgument.NextReplace("FFFFF", "FMUS");
                            else if (formatArgument.Contains("FFFF"))
                                formatArgument = formatArgument.NextReplace("FFFF", "FMUS");
                            else if (formatArgument.Contains("ffffff"))
                                formatArgument = formatArgument.NextReplace("ffffff", "US");
                            else if (formatArgument.Contains("fffff"))
                                formatArgument = formatArgument.NextReplace("fffff", "FMUS");
                            else if (formatArgument.Contains("ffff"))
                                formatArgument = formatArgument.NextReplace("ffff", "FMUS");

                            if (formatArgument.Contains("FFF"))
                                formatArgument = formatArgument.NextReplace("FFF", "MS");
                            else if (formatArgument.Contains("FF"))
                                formatArgument = formatArgument.NextReplace("FF", "FMMS");
                            else if (formatArgument.Contains("F"))
                                formatArgument = formatArgument.NextReplace("F", "FMMS");
                            else if (formatArgument.Contains("fff"))
                                formatArgument = formatArgument.NextReplace("fff", "MS");
                            else if (formatArgument.Contains("ff"))
                                formatArgument = formatArgument.NextReplace("ff", "FMMS");
                            else if (formatArgument.Contains("f"))
                                formatArgument = formatArgument.NextReplace("f", "FMMS");
                        }
                        else formatArgument = visitor.GetQuotedValue(formatSegment);
                        return valueSegment.Change($"'{formatArgument}'::INTERVAL");
                    });
                    result = true;
                    break;
            }
        }
        else
        {
            switch (methodInfo.Name)
            {
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
                case "Add":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });

                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return targetSegment.MergeValue(rightSegment, ((TimeSpan)targetSegment.Value).Add((TimeSpan)rightSegment.Value));

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(rightSegment, $"{targetArgument}+{rightArgument}");
                    });
                    result = true;
                    break;
                case "Subtract":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return targetSegment.MergeValue(rightSegment, ((TimeSpan)targetSegment.Value).Subtract((TimeSpan)rightSegment.Value));

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(rightSegment, $"{targetArgument}-{rightArgument}");
                    });
                    result = true;
                    break;
#if NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
                case "Multiply":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return targetSegment.MergeValue(rightSegment, ((TimeSpan)targetSegment.Value).Multiply((double)rightSegment.Value));

                        var targetArgument = visitor.GetQuotedValue(targetSegment, true);
                        var rightArgument = visitor.GetQuotedValue(rightSegment, true);
                        return targetSegment.Merge(rightSegment, $"{targetArgument}*{rightArgument}");
                    });
                    result = true;
                    break;
                case "Divide":
                    if (parameterInfos[0].ParameterType == typeof(double))
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                            var rightSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (rightSegment.IsConstant || rightSegment.IsVariable))
                                return targetSegment.MergeValue(rightSegment, ((TimeSpan)targetSegment.Value).Divide((double)rightSegment.Value));

                            var targetArgument = visitor.GetQuotedValue(targetSegment, true);
                            var rightArgument = visitor.GetQuotedValue(rightSegment, true);
                            return targetSegment.Merge(rightSegment, $"{targetArgument}/{rightArgument}");
                        });
                        result = true;
                    }
                    if (parameterInfos[0].ParameterType == typeof(TimeSpan))
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                            var rightSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (rightSegment.IsConstant || rightSegment.IsVariable))
                                return targetSegment.MergeValue(rightSegment, ((TimeSpan)targetSegment.Value).Divide((TimeSpan)rightSegment.Value));

                            var targetArgument = visitor.GetQuotedValue(targetSegment, true);
                            var rightArgument = visitor.GetQuotedValue(rightSegment, true);
                            return targetSegment.Merge(rightSegment, $"{targetArgument}/{rightArgument}");
                        });
                        result = true;
                    }
                    break;
#endif
            }
        }
        return result;
    }
}
