using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.MySqlConnector;

partial class MySqlProvider
{
    public override bool TryGetTimeSpanMemberAccessSqlFormatter(MemberExpression memberExpr, out MemberAccessSqlFormatter formatter)
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

                        return targetSegment.Change($"TIME_TO_SEC({targetSegment.Body})*10000000");
                    });
                    result = true;
                    break;
                case "Days":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((TimeSpan)targetSegment.Value).Days);

                        throw new NotSupportedException("暂时不支持非常量、变量TimeSpan类型返回天数");
                    });
                    result = true;
                    break;
                case "Hours":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((TimeSpan)targetSegment.Value).Hours);

                        return targetSegment.Change($"HOUR({targetSegment.Body})", false, true);
                    });
                    result = true;
                    break;
                case "Milliseconds":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((TimeSpan)targetSegment.Value).Milliseconds);

                        return targetSegment.Change($"MICROSECOND({targetSegment.Body}) DIV 1000 MOD 1000");
                    });
                    result = true;
                    break;
                case "Minutes":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((TimeSpan)targetSegment.Value).Minutes);

                        return targetSegment.Change($"MINUTE({targetSegment.Body})", false, true);
                    });
                    result = true;
                    break;
                case "Seconds":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((TimeSpan)targetSegment.Value).Seconds);

                        return targetSegment.Change($"SECOND({targetSegment.Body})", false, true);
                    });
                    result = true;
                    break;
                case "TotalDays":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((TimeSpan)targetSegment.Value).TotalDays);

                        return targetSegment.Change($"TIME_TO_SEC({targetSegment.Body})/{3600 * 24}");
                    });
                    result = true;
                    break;
                case "TotalHours":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((TimeSpan)targetSegment.Value).TotalHours);

                        return targetSegment.Change($"TIME_TO_SEC({targetSegment.Body})/3600");
                    });
                    result = true;
                    break;
                case "TotalMilliseconds":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((TimeSpan)targetSegment.Value).TotalMilliseconds);

                        return targetSegment.Change($"TIME_TO_SEC({targetSegment.Body})*1000");
                    });
                    result = true;
                    break;
                case "TotalMinutes":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((TimeSpan)targetSegment.Value).TotalMinutes);

                        return targetSegment.Change($"TIME_TO_SEC({targetSegment.Body})/60");
                    });
                    result = true;
                    break;
                case "TotalSeconds":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((TimeSpan)targetSegment.Value).TotalSeconds);

                        return targetSegment.Change($"TIME_TO_SEC({targetSegment.Body})", false, true);
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
        var cacheKey = HashCode.Combine(methodInfo.DeclaringType, methodInfo);
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

                        return valueSegment.Change($"ADDTIME('00:00:00',SEC_TO_TIME({valueSegment.ToExprWrap()}*{24 * 3600}))", false, true);
                    });
                    result = true;
                    break;
                case "FromHours":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        if (valueSegment.IsConstant || valueSegment.IsVariable)
                            return valueSegment.ChangeValue(TimeSpan.FromHours(Convert.ToDouble(valueSegment.Value)));
                        return valueSegment.Change($"ADDTIME('00:00:00',SEC_TO_TIME({valueSegment.ToExprWrap()}*3600))", false, true);
                    });
                    result = true;
                    break;
                case "FromMilliseconds":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        if (valueSegment.IsConstant || valueSegment.IsVariable)
                            return valueSegment.ChangeValue(TimeSpan.FromMilliseconds(Convert.ToDouble(valueSegment.Value)));
                        return valueSegment.Change($"ADDTIME('00:00:00',SEC_TO_TIME({valueSegment.ToExprWrap()}/1000))", false, true);
                    });
                    result = true;
                    break;
                case "FromMinutes":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        if (valueSegment.IsConstant || valueSegment.IsVariable)
                            return valueSegment.ChangeValue(TimeSpan.FromMinutes(Convert.ToDouble(valueSegment.Value)));
                        return valueSegment.Change($"ADDTIME('00:00:00',SEC_TO_TIME({valueSegment.ToExprWrap()}*60))", false, true);
                    });
                    result = true;
                    break;
                case "FromSeconds":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        if (valueSegment.IsConstant || valueSegment.IsVariable)
                            return valueSegment.ChangeValue(TimeSpan.FromSeconds(Convert.ToDouble(valueSegment.Value)));
                        return valueSegment.Change($"ADDTIME('00:00:00',SEC_TO_TIME({valueSegment.ToExprWrap()}))", false, true);
                    });
                    result = true;
                    break;
                case "FromTicks":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        if (valueSegment.IsConstant || valueSegment.IsVariable)
                            return valueSegment.ChangeValue(TimeSpan.FromTicks(Convert.ToInt64(valueSegment.Value)));
                        return valueSegment.Change($"ADDTIME('00:00:00',SEC_TO_TIME({valueSegment.ToExprWrap()}/{TimeSpan.TicksPerSecond}))", false, true);
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
                        return valueSegment.Change($"CAST({valueSegment.Body} AS TIME)", false, true);
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

                        return valueSegment.Merge(formatSegment, $"CAST({valueSegment.Body} AS TIME)", false, true);
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
                        if (rightSegment.IsConstant || rightSegment.IsVariable)
                        {
                            var builder = new StringBuilder();
                            var timeSpan = (TimeSpan)rightSegment.Value;
                            if (timeSpan.Days > 0)
                            {
                                builder.Append($"DATE_ADD({targetArgument},INTERVAL {timeSpan.Days} DAY)");
                                timeSpan = timeSpan.Subtract(TimeSpan.FromDays(timeSpan.Days));
                            }
                            if (timeSpan.Ticks > 0)
                            {
                                if (builder.Length > 0) builder.Insert(0, $"ADDTIME(");
                                else builder.Append($"ADDTIME({targetArgument}");
                                builder.Append($",{this.GetQuotedValue(timeSpan)})");
                            }
                            return targetSegment.Merge(rightSegment, builder.ToString(), false, true);
                        }

                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(rightSegment, $"ADDTIME({targetArgument},{rightArgument})", false, true);
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
                        return targetSegment.Merge(rightSegment, $"TIMEDIFF({targetArgument},{rightArgument})", false, true);
                    });
                    result = true;
                    break;
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
            }
        }
        return result;
    }
}
