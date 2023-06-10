using System;
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
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) => target.Change(TimeSpan.MinValue));
                    result = true;
                    break;
                case "MaxValue":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) => target.Change(TimeSpan.MaxValue));
                    result = true;
                    break;
                case "Zero":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) => target.Change(TimeSpan.Zero));
                    result = true;
                    break;
            }
        }
        else
        {
            switch (memberInfo.Name)
            {
                case "Ticks":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).Ticks);
                        var targetArgument = this.GetQuotedValue(targetSegment);
                        var builder = new StringBuilder();
                        builder.Append($"CAST(DATEPART(DD,{targetArgument}) AS BIGINT)*24*60*60*10000000");
                        builder.Append($"+CAST(DATEPART(HH,{targetArgument}) AS BIGINT)*60*60*10000000");
                        builder.Append($"+CAST(DATEPART(MI,{targetArgument}) AS BIGINT)*60*10000000");
                        builder.Append($"+CAST(DATEPART(SS,{targetArgument}) AS BIGINT)*10000000");
                        builder.Append($"+CAST(DATEPART(NS,{targetArgument}) AS BIGINT)/100");
                        return targetSegment.Change($"({builder})", false, false, true);
                    });
                    result = true;
                    break;
                case "Days":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).Days);

                        return targetSegment.Change($"DATEPART(DD,{this.GetQuotedValue(targetSegment)})", false, false, true);
                    });
                    result = true;
                    break;
                case "Hours":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).Hours);

                        return targetSegment.Change($"DATEPART(HH,{this.GetQuotedValue(targetSegment)})", false, false, true);
                    });
                    result = true;
                    break;
                case "Milliseconds":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).Milliseconds);

                        return targetSegment.Change($"DATEPART(MS,{this.GetQuotedValue(targetSegment)})", false, false, true);
                    });
                    result = true;
                    break;
                case "Minutes":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).Minutes);

                        return targetSegment.Change($"DATEPART(MI,{this.GetQuotedValue(targetSegment)})", false, false, true);
                    });
                    result = true;
                    break;
                case "Seconds":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).Seconds);

                        return targetSegment.Change($"DATEPART(SS,{this.GetQuotedValue(targetSegment)})", false, false, true);
                    });
                    result = true;
                    break;
                case "TotalDays":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).TotalDays);

                        var builder = new StringBuilder();
                        builder.Append($"CAST(DATEPART(DD,{this.GetQuotedValue(targetSegment)}) AS REAL)");
                        builder.Append($"+CAST(CAST(DATEPART(HH,{this.GetQuotedValue(targetSegment)}) AS REAL)/24 AS REAL)");
                        builder.Append($"+CAST(CAST(DATEPART(MI,{this.GetQuotedValue(targetSegment)}) AS REAL)/24/60 AS REAL)");
                        return targetSegment.Change($"({builder})", false, false, true);
                    });
                    result = true;
                    break;
                case "TotalHours":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).TotalHours);

                        var builder = new StringBuilder();
                        builder.Append($"CAST(DATEPART(DD,{this.GetQuotedValue(targetSegment)}) AS REAL)*24");
                        builder.Append($"+CAST(DATEPART(HH,{this.GetQuotedValue(targetSegment)}) AS REAL)");
                        builder.Append($"+CAST(CAST(DATEPART(MI,{this.GetQuotedValue(targetSegment)}) AS REAL)/60 AS REAL)");
                        builder.Append($"+CAST(CAST(DATEPART(SS,{this.GetQuotedValue(targetSegment)}) AS REAL)/60/60 AS REAL)");
                        return targetSegment.Change($"({builder})", false, false, true);
                    });
                    result = true;
                    break;
                case "TotalMilliseconds":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).TotalMilliseconds);

                        var builder = new StringBuilder();
                        builder.Append($"CAST(DATEPART(DD,{this.GetQuotedValue(targetSegment)}) AS REAL)*24*60*60*1000");
                        builder.Append($"+CAST(DATEPART(HH,{this.GetQuotedValue(targetSegment)}) AS REAL)*60*60*1000");
                        builder.Append($"+CAST(DATEPART(MI,{this.GetQuotedValue(targetSegment)}) AS REAL)*60*1000");
                        builder.Append($"+CAST(DATEPART(SS,{this.GetQuotedValue(targetSegment)}) AS REAL)*1000");
                        builder.Append($"+CAST(DATEPART(MS,{this.GetQuotedValue(targetSegment)}) AS REAL)");
                        builder.Append($"+CAST(CAST(DATEPART(MCS,{this.GetQuotedValue(targetSegment)}) AS REAL)/1000 AS REAL)");
                        builder.Append($"+CAST(CAST(DATEPART(NS,{this.GetQuotedValue(targetSegment)}) AS REAL)/1000/1000 AS REAL)");
                        return targetSegment.Change($"({builder})", false, false, true);
                    });
                    result = true;
                    break;
                case "TotalMinutes":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).TotalMinutes);

                        var builder = new StringBuilder();
                        builder.Append($"CAST(DATEPART(DD,{this.GetQuotedValue(targetSegment)}) AS REAL)*24*60");
                        builder.Append($"+CAST(DATEPART(HH,{this.GetQuotedValue(targetSegment)}) AS REAL)*60");
                        builder.Append($"+CAST(DATEPART(MI,{this.GetQuotedValue(targetSegment)}) AS REAL)");
                        builder.Append($"+CAST(CAST(DATEPART(SS,{this.GetQuotedValue(targetSegment)}) AS REAL)/60 AS REAL)");
                        builder.Append($"+CAST(CAST(DATEPART(MS,{this.GetQuotedValue(targetSegment)}) AS REAL)/1000 AS REAL)");
                        builder.Append($"+CAST(CAST(DATEPART(MCS,{this.GetQuotedValue(targetSegment)}) AS REAL)/1000/1000 AS REAL)");
                        return targetSegment.Change($"({builder})", false, false, true);
                    });
                    result = true;
                    break;
                case "TotalSeconds":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).TotalSeconds);

                        var builder = new StringBuilder();
                        builder.Append($"CAST(DATEPART(DD,{this.GetQuotedValue(targetSegment)}) AS REAL)*24*60*60");
                        builder.Append($"+CAST(DATEPART(HH,{this.GetQuotedValue(targetSegment)}) AS REAL)*60*60");
                        builder.Append($"+CAST(DATEPART(MI,{this.GetQuotedValue(targetSegment)}) AS REAL)*60");
                        builder.Append($"+CAST(DATEPART(SS,{this.GetQuotedValue(targetSegment)}) AS REAL)");
                        builder.Append($"+CAST(CAST(DATEPART(MS,{this.GetQuotedValue(targetSegment)}) AS REAL)/1000 AS REAL)");
                        builder.Append($"+CAST(CAST(DATEPART(MCS,{this.GetQuotedValue(targetSegment)}) AS REAL)/1000/1000 AS REAL)");
                        return targetSegment.Change($"({builder})", false, false, true);
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
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var leftSegment = visitor.VisitAndDeferred(args[0]);
                        var rightSegment = visitor.VisitAndDeferred(args[1]);

                        leftSegment.Merge(rightSegment);
                        return args[0].Change($"(CASE WHEN DATEDIFF(MICROSECOND,{this.GetQuotedValue(leftSegment)}={this.GetQuotedValue(rightSegment)} THEN 0 WHEN {leftSegment}>{rightSegment} THEN 1 ELSE -1 END)", false, false, true);
                    });
                    result = true;
                    break;
                case "Equals":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var leftSegment = visitor.VisitAndDeferred(args[0]);
                        var rightSegment = visitor.VisitAndDeferred(args[1]);

                        leftSegment.Merge(rightSegment);
                        return leftSegment.Change($"{leftSegment}={rightSegment}", false, true, false);
                    });
                    result = true;
                    break;
                case "FromDays":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(args[0]);
                        if (valueSegment.IsConstant)
                            valueSegment.ChangeValue(this.GetQuotedValue(valueSegment));
                        return args[0].Change(this.CastTo(typeof(string), this.GetQuotedValue(valueSegment)), false, false, true);
                    });
                    result = true;
                    break;
                case "FromHours":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(args[0]);
                        return valueSegment.Change($"{valueSegment}*{(long)1000000 * 60 * 60}", false, true, false);
                    });
                    result = true;
                    break;
                case "FromMilliseconds":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(args[0]);
                        return valueSegment.Change($"{valueSegment}*1000", false, true, false);
                    });
                    result = true;
                    break;
                case "FromMinutes":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(args[0]);
                        return valueSegment.Change($"{valueSegment}*{(long)1000000 * 60}", false, true, false);
                    });
                    result = true;
                    break;
                case "FromSeconds":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(args[0]);
                        return valueSegment.Change($"{valueSegment}*1000000", false, true, false);
                    });
                    result = true;
                    break;
                case "FromTicks":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(args[0]);
                        return valueSegment.Change($"{valueSegment}/10", false, true, false);
                    });
                    result = true;
                    break;
                case "Parse":
                case "TryParse":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(args[0]);
                        if (valueSegment.IsConstant)
                            return valueSegment.Change(TimeSpan.Parse(valueSegment.ToString()));

                        return valueSegment.Change($"CAST({this.GetQuotedValue(valueSegment)} AS TIME(7))", false, false, true);
                    });
                    result = true;
                    break;
                case "ParseExact":
                case "TryParseExact":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(args[0]);
                        if (valueSegment.IsConstant)
                            return valueSegment.Change(TimeSpan.Parse(valueSegment.ToString()));

                        return valueSegment.Change($"CAST({this.GetQuotedValue(valueSegment)} AS TIME(7))", false, false, true);
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
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);

                        targetSegment.Merge(rightSegment);
                        return targetSegment.Change($"(CASE WHEN {targetSegment}={rightSegment} THEN 0 WHEN {targetSegment}>{rightSegment} THEN 1 ELSE -1 END)", false, false, true);
                    });
                    result = true;
                    break;
                case "Equals":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);

                        targetSegment.Merge(rightSegment);
                        return targetSegment.Change($"{this.GetQuotedValue(targetSegment)}={this.GetQuotedValue(rightSegment)}", false, true, false);
                    });
                    result = true;
                    break;
                case "Add":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);

                        if (targetSegment.IsConstant && rightSegment.IsConstant)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).Add((TimeSpan)rightSegment.Value));

                        if (!rightSegment.IsConstant)
                            throw new NotSupportedException("TimeSpan.Add方法，只支持常量参数解析");
                        var targetArgument = this.GetQuotedValue(targetSegment);
                        var rightArgument = this.GetQuotedValue(rightSegment);

                        targetSegment.Merge(rightSegment);
                        var builder = new StringBuilder();
                        builder.Append($"DATEADD(DD,DATEPART(DD,{rightArgument}),{targetArgument})");
                        builder.Append($"+DATEADD(HH,DATEPART(HH,{rightArgument}),{targetArgument})");
                        builder.Append($"+DATEADD(MI,DATEPART(MI,{rightArgument}),{targetArgument})");
                        builder.Append($"+DATEADD(SS,DATEPART(SS,{rightArgument}),{targetArgument})");
                        builder.Append($"+DATEADD(MS,DATEPART(MS,{rightArgument}),{targetArgument})");
                        builder.Append($"+DATEADD(MCS,DATEPART(MCS,{rightArgument}),{targetArgument})");
                        builder.Append($"+DATEADD(NS,DATEPART(NS,{rightArgument}),{targetArgument})");
                        return targetSegment.Change($"({builder})", false, false, true);
                    });
                    result = true;
                    break;
                case "Subtract":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);
                        if (targetSegment.IsConstant && rightSegment.IsConstant)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).Subtract((TimeSpan)rightSegment.Value));

                        if (!rightSegment.IsConstant)
                            throw new NotSupportedException("TimeSpan.Add方法，只支持常量参数解析");
                        var targetArgument = this.GetQuotedValue(targetSegment);
                        var rightArgument = this.GetQuotedValue(rightSegment);

                        targetSegment.Merge(rightSegment);
                        var builder = new StringBuilder();
                        builder.Append($"DATEADD(DD,-DATEPART(DD,{rightArgument}),{targetArgument})");
                        builder.Append($"+DATEADD(HH,-DATEPART(HH,{rightArgument}),{targetArgument})");
                        builder.Append($"+DATEADD(MI,-DATEPART(MI,{rightArgument}),{targetArgument})");
                        builder.Append($"+DATEADD(SS,-DATEPART(SS,{rightArgument}),{targetArgument})");
                        builder.Append($"+DATEADD(MS,-DATEPART(MS,{rightArgument}),{targetArgument})");
                        builder.Append($"+DATEADD(MCS,-DATEPART(MCS,{rightArgument}),{targetArgument})");
                        builder.Append($"+DATEADD(NS,DATEPART(NS,{rightArgument}),{targetArgument})");
                        return targetSegment.Change($"({builder})", false, false, true);
                    });
                    result = true;
                    break;
                case "Multiply":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);
                        if (targetSegment.IsConstant && rightSegment.IsConstant)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).Multiply((double)rightSegment.Value));

                        targetSegment.Merge(rightSegment);
                        return targetSegment.Change($"{targetSegment}*{rightSegment}", false, false, true);
                    });
                    result = true;
                    break;
                case "Divide":
                    if (parameterInfos[0].ParameterType == typeof(double))
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            var rightSegment = visitor.VisitAndDeferred(args[0]);
                            if (targetSegment.IsConstant && rightSegment.IsConstant)
                                return targetSegment.Change(((TimeSpan)targetSegment.Value).Divide((double)rightSegment.Value));

                            targetSegment.Merge(rightSegment);
                            return targetSegment.Change($"{targetSegment}/{rightSegment}", false, false, true);
                        });
                        result = true;
                    }
                    if (parameterInfos[0].ParameterType == typeof(TimeSpan))
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            var rightSegment = visitor.VisitAndDeferred(args[0]);
                            if (targetSegment.IsConstant && rightSegment.IsConstant)
                                return targetSegment.Change(((TimeSpan)targetSegment.Value).Divide((TimeSpan)rightSegment.Value));

                            targetSegment.Merge(rightSegment);
                            return targetSegment.Change($"{targetSegment}/{rightSegment}", false, false, true);
                        });
                        result = true;
                    }
                    break;
            }
        }
        return result;
    }
}
