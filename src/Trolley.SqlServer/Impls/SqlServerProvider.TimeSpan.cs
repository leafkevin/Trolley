using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.SqlServer;

partial class SqlServerProvider
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
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) => target.Change(TimeSpan.MinValue, true));
                    result = true;
                    break;
                case "MaxValue":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) => target.Change(TimeSpan.MaxValue, true));
                    result = true;
                    break;
                case "Zero":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) => target.Change(TimeSpan.Zero, true));
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
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).Ticks);

                        return targetSegment.Change($"DATEDIFF_BIG(MICROSECOND,CAST('00:00:00' AS TIME),{targetSegment})*10", false, false, true);
                    });
                    result = true;
                    break;
                case "Days":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).Days);

                        return targetSegment.Change($"DATEPART(DAY,{targetSegment})", false, false, false, true);
                    });
                    result = true;
                    break;
                case "Hours":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).Hours);

                        return targetSegment.Change($"DATEPART(HOUR,{targetSegment})", false, false, false, true);
                    });
                    result = true;
                    break;
                case "Milliseconds":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).Milliseconds);

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        return targetSegment.Change($"DATEPART(MILLISECOND,{targetArgument})", false, false, false, true);
                    });
                    result = true;
                    break;
                case "Minutes":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).Minutes);

                        return targetSegment.Change($"DATEPART(MINUTE,{targetSegment})", false, false, false, true);
                    });
                    result = true;
                    break;
                case "Seconds":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).Seconds);

                        return targetSegment.Change($"DATEPART(SECOND,{targetSegment})", false, false, false, true);
                    });
                    result = true;
                    break;
                case "TotalDays":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).TotalDays);

                        return targetSegment.Change($"DATEDIFF_BIG(SECOND,'00:00:00',{targetSegment})/CAST({3600 * 24} AS FLOAT)", false, false, true);
                    });
                    result = true;
                    break;
                case "TotalHours":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).TotalHours);

                        return targetSegment.Change($"DATEDIFF_BIG(SECOND,'00:00:00',{targetSegment})/CAST(3600 AS FLOAT)", false, false, true);
                    });
                    result = true;
                    break;
                case "TotalMilliseconds":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).TotalMilliseconds);

                        return targetSegment.Change($"CAST(DATEDIFF_BIG(MILLISECOND,'00:00:00',{targetSegment}) AS FLOAT)", false, false, false, true);
                    });
                    result = true;
                    break;
                case "TotalMinutes":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).TotalMinutes);

                        return targetSegment.Change($"DATEDIFF_BIG(SECOND,'00:00:00',{targetSegment})/CAST(60 AS FLOAT)", false, false, true);
                    });
                    result = true;
                    break;
                case "TotalSeconds":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).TotalSeconds);

                        return targetSegment.Change($"CAST(DATEDIFF_BIG(SECOND,'00:00:00',{targetSegment}) AS FLOAT)", false, false, false, true);
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
                        var leftSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[1] });

                        var leftArgument = visitor.GetQuotedValue(leftSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return leftSegment.Merge(rightSegment, $"CASE WHEN ({leftArgument}={rightArgument} THEN 0 WHEN ({leftArgument}>{rightArgument})=1 THEN 1 ELSE -1 END", false, false, true);
                    });
                    result = true;
                    break;
                case "Equals":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var leftSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[1] });

                        var leftArgument = visitor.GetQuotedValue(leftSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return leftSegment.Merge(rightSegment, $"{leftArgument}={rightArgument}", false, false, true);
                    });
                    result = true;
                    break;
                case "FromDays":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if (valueSegment.IsConstant || valueSegment.IsVariable)
                            return valueSegment.Change(TimeSpan.FromDays(Convert.ToDouble(valueSegment.Value)));

                        return valueSegment.Change($"CAST(DATEADD(MILLISECOND,CAST({valueSegment}*{(long)1000 * 60 * 60 * 24} AS BIGINT),'00:00:00') AS TIME)", false, false, false, true);
                    });
                    result = true;
                    break;
                case "FromHours":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if (valueSegment.IsConstant || valueSegment.IsVariable)
                            return valueSegment.Change(TimeSpan.FromHours(Convert.ToDouble(valueSegment.Value)));
                        return valueSegment.Change($"CAST(DATEADD(MILLISECOND,CAST({valueSegment}*{(long)1000 * 60 * 60} AS BIGINT),'00:00:00') AS TIME)", false, false, false, true);
                    });
                    result = true;
                    break;
                case "FromMilliseconds":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if (valueSegment.IsConstant || valueSegment.IsVariable)
                            return valueSegment.Change(TimeSpan.FromMilliseconds(Convert.ToDouble(valueSegment.Value)));
                        return valueSegment.Change($"CAST(DATEADD(MILLISECOND,{valueSegment},'00:00:00') AS TIME)", false, false, false, true);
                    });
                    result = true;
                    break;
                case "FromMinutes":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if (valueSegment.IsConstant || valueSegment.IsVariable)
                            return valueSegment.Change(TimeSpan.FromMinutes(Convert.ToDouble(valueSegment.Value)));
                        return valueSegment.Change($"CAST(DATEADD(MILLISECOND,CAST({valueSegment}*{(long)1000 * 60} AS BIGINT),'00:00:00') AS TIME)", false, false, false, true);
                    });
                    result = true;
                    break;
                case "FromSeconds":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if (valueSegment.IsConstant || valueSegment.IsVariable)
                            return valueSegment.Change(TimeSpan.FromSeconds(Convert.ToDouble(valueSegment.Value)));
                        return valueSegment.Change($"CAST(DATEADD(MILLISECOND,CAST({valueSegment}*1000 AS BIGINT),'00:00:00') AS TIME)", false, false, false, true);
                    });
                    result = true;
                    break;
                case "FromTicks":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if (valueSegment.IsConstant || valueSegment.IsVariable)
                            return valueSegment.Change(TimeSpan.FromTicks(Convert.ToInt64(valueSegment.Value)));
                        return valueSegment.Change($"CAST(DATEADD(MILLISECOND,{valueSegment}/{TimeSpan.TicksPerMillisecond},'00:00:00') AS TIME)", false, false, false, true);
                    });
                    result = true;
                    break;
                case "Parse":
                case "TryParse":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if (valueSegment.IsConstant || valueSegment.IsVariable)
                            return valueSegment.Change(TimeSpan.Parse(valueSegment.ToString()));
                        return valueSegment.Change($"CAST({valueSegment} AS TIME)", false, false, false, true);
                    });
                    result = true;
                    break;
                case "ParseExact":
                case "TryParseExact":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        var formatSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[1] });
                        if ((valueSegment.IsConstant || valueSegment.IsVariable)
                            && (formatSegment.IsConstant || formatSegment.IsVariable))
                            return valueSegment.Merge(formatSegment, TimeSpan.ParseExact(valueSegment.ToString(), formatSegment.ToString(), CultureInfo.InvariantCulture));

                        return valueSegment.Merge(formatSegment, $"CAST({valueSegment} AS TIME)", false, false, false, true);
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
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(rightSegment, $"CASE WHEN {targetArgument}={rightArgument} THEN 0 WHEN {targetArgument}>{rightArgument} THEN 1 ELSE -1 END", false, false, true);
                    });
                    result = true;
                    break;
                case "Equals":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(rightSegment, $"{targetArgument}={rightArgument}", false, false, true);
                    });
                    result = true;
                    break;
                case "Add":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });

                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return targetSegment.Merge(rightSegment, ((TimeSpan)targetSegment.Value).Add((TimeSpan)rightSegment.Value));

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        if (rightSegment.IsConstant || rightSegment.IsVariable)
                        {
                            var builder = new StringBuilder();
                            var timeSpan = (TimeSpan)rightSegment.Value;
                            if (timeSpan.Days > 0)
                            {
                                builder.Append($"DATEADD(DAY,{timeSpan.Days},{targetArgument})");
                                timeSpan = timeSpan.Subtract(TimeSpan.FromDays(timeSpan.Days));
                            }
                            if (timeSpan.Ticks > 0)
                            {
                                if (builder.Length > 0) builder.Insert(0, $"DATEADD(MILLISECOND,{timeSpan.TotalMilliseconds},");
                                else builder.Append($"DATEADD(MILLISECOND,{timeSpan.TotalMilliseconds},");
                                builder.Append($"{targetArgument})");
                            }
                            return targetSegment.Merge(rightSegment, builder.ToString(), false, false, true);
                        }
                        var rightArgument = $"DATEDIFF_BIG(MILLISECOND,'00:00:00',{rightSegment}";
                        return targetSegment.Merge(rightSegment, $"DATEADD(MILLISECOND,-{rightArgument},{targetArgument})", false, false, false, true);
                    });
                    result = true;
                    break;
                case "Subtract":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return targetSegment.Merge(rightSegment, ((TimeSpan)targetSegment.Value).Subtract((TimeSpan)rightSegment.Value));

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(rightSegment, $"CAST(DATEADD(MILLISECOND,DATEDIFF(MILLISECOND,'00:00:00',{targetArgument})-DATEDIFF(MILLISECOND,'00:00:00',{rightArgument}),'00:00:00') AS TIME)", false, false, false, true);
                    });
                    result = true;
                    break;
                case "Multiply":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return targetSegment.Merge(rightSegment, ((TimeSpan)targetSegment.Value).Multiply((double)rightSegment.Value));

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(rightSegment, $"CAST(DATEADD(MILLISECOND,DATEDIFF(MILLISECOND,'00:00:00',{targetArgument})*{rightArgument},'00:00:00') AS TIME)", false, false, false, true);
                    });
                    result = true;
                    break;
                case "Divide":
                    if (parameterInfos[0].ParameterType == typeof(double))
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                            var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (rightSegment.IsConstant || rightSegment.IsVariable))
                                return targetSegment.Merge(rightSegment, ((TimeSpan)targetSegment.Value).Divide((double)rightSegment.Value));

                            var targetArgument = visitor.GetQuotedValue(targetSegment);
                            var rightArgument = visitor.GetQuotedValue(rightSegment);
                            return targetSegment.Merge(rightSegment, $"CAST(DATEADD(MILLISECOND,DATEDIFF(MILLISECOND,'00:00:00',{targetArgument})/{rightArgument},'00:00:00') AS TIME)", false, false, false, true);
                        });
                        result = true;
                    }
                    if (parameterInfos[0].ParameterType == typeof(TimeSpan))
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                            var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (rightSegment.IsConstant || rightSegment.IsVariable))
                                return targetSegment.Merge(rightSegment, ((TimeSpan)targetSegment.Value).Divide((TimeSpan)rightSegment.Value));

                            var targetArgument = visitor.GetQuotedValue(targetSegment);
                            var rightArgument = visitor.GetQuotedValue(rightSegment);
                            return targetSegment.Merge(rightSegment, $"CAST(DATEADD(MILLISECOND,DATEDIFF(MILLISECOND,'00:00:00',{targetArgument})/DATEDIFF(MILLISECOND,'00:00:00',{rightArgument}),'00:00:00') AS TIME)", false, false, false, true);
                        });
                        result = true;
                    }
                    break;
            }
        }
        return result;
    }
}
