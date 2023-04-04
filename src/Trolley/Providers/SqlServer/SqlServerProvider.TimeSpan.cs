using System;
using System.Linq.Expressions;

namespace Trolley;

partial class SqlServerProvider
{
    public virtual bool TryGetTimeSpanMemberAccessSqlFormatter(MemberExpression memberExpr, out MemberAccessSqlFormatter formatter)
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
                    memberAccessSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target) => target.Change(TimeSpan.MinValue));
                    result = true;
                    break;
                case "MaxValue":
                    memberAccessSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target) => target.Change(TimeSpan.MaxValue));
                    result = true;
                    break;
                case "Zero":
                    memberAccessSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target) => target.Change(TimeSpan.Zero));
                    result = true;
                    break;
            }
            return result;
        }
        switch (memberInfo.Name)
        {
            case "Ticks":
                memberAccessSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target) =>
                {
                    var targetSegment = visitor.VisitAndDeferred(target);
                    if (targetSegment.IsConstantValue)
                        return targetSegment.Change(((TimeSpan)targetSegment.Value).Ticks);

                    return targetSegment.Change($"{targetSegment} * 10", false, true);
                });
                result = true;
                break;
            case "Days":
                memberAccessSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target) =>
                {
                    var targetSegment = visitor.VisitAndDeferred(target);
                    if (targetSegment.IsConstantValue)
                        return targetSegment.Change(((TimeSpan)targetSegment.Value).Days);

                    return targetSegment.Change($"({targetSegment} DIV {(long)1000000 * 60 * 60 * 24})", false, true);
                });
                result = true;
                break;
            case "Hours":
                memberAccessSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target) =>
                {
                    var targetSegment = visitor.VisitAndDeferred(target);
                    if (targetSegment.IsConstantValue)
                        return targetSegment.Change(((TimeSpan)targetSegment.Value).Hours);

                    return targetSegment.Change($"({targetSegment} DIV {(long)1000000 * 60 * 60} MOD 24)", false, true);
                });
                result = true;
                break;
            case "Milliseconds":
                memberAccessSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target) =>
                {
                    var targetSegment = visitor.VisitAndDeferred(target);
                    if (targetSegment.IsConstantValue)
                        return targetSegment.Change(((TimeSpan)targetSegment.Value).Milliseconds);

                    return targetSegment.Change($"({targetSegment} DIV 1000 MOD 1000)", false, true);
                });
                result = true;
                break;
            case "Minutes":
                memberAccessSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target) =>
                {
                    var targetSegment = visitor.VisitAndDeferred(target);
                    if (targetSegment.IsConstantValue)
                        return targetSegment.Change(((TimeSpan)targetSegment.Value).Minutes);

                    return targetSegment.Change($"({targetSegment} DIV {(long)1000000 * 60} MOD 60)", false, true);
                });
                result = true;
                break;
            case "Seconds":
                memberAccessSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target) =>
                {
                    var targetSegment = visitor.VisitAndDeferred(target);
                    if (targetSegment.IsConstantValue)
                        return targetSegment.Change(((TimeSpan)targetSegment.Value).Seconds);

                    return targetSegment.Change($"({targetSegment} DIV 1000000 MOD 60)", false, true);
                });
                result = true;
                break;
            case "TotalDays":
                memberAccessSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target) =>
                {
                    var targetSegment = visitor.VisitAndDeferred(target);
                    if (targetSegment.IsConstantValue)
                        return targetSegment.Change(((TimeSpan)targetSegment.Value).TotalDays);

                    return targetSegment.Change($"{targetSegment}/{(long)1000000 * 60 * 60 * 24}", false, true);
                });
                result = true;
                break;
            case "TotalHours":
                memberAccessSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target) =>
                {
                    var targetSegment = visitor.VisitAndDeferred(target);
                    if (targetSegment.IsConstantValue)
                        return targetSegment.Change(((TimeSpan)targetSegment.Value).TotalHours);

                    return targetSegment.Change($"{targetSegment}/{(long)1000000 * 60 * 60}", false, true);
                });
                result = true;
                break;
            case "TotalMilliseconds":
                memberAccessSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target) =>
                {
                    var targetSegment = visitor.VisitAndDeferred(target);
                    if (targetSegment.IsConstantValue)
                        return targetSegment.Change(((TimeSpan)targetSegment.Value).TotalMilliseconds);

                    return targetSegment.Change($"{targetSegment}/1000", false, true);
                });
                result = true;
                break;
            case "TotalMinutes":
                memberAccessSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target) =>
                {
                    var targetSegment = visitor.VisitAndDeferred(target);
                    if (targetSegment.IsConstantValue)
                        return targetSegment.Change(((TimeSpan)targetSegment.Value).TotalMinutes);

                    return targetSegment.Change($"{targetSegment}/{(long)1000000 * 60}", false, true);
                });
                result = true;
                break;
            case "TotalSeconds":
                memberAccessSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target) =>
                {
                    var targetSegment = visitor.VisitAndDeferred(target);
                    if (targetSegment.IsConstantValue)
                        return targetSegment.Change(((TimeSpan)targetSegment.Value).TotalSeconds);

                    return targetSegment.Change($"{targetSegment}/1000000", false, true);
                });
                result = true;
                break;
        }
        return result;
    }
    public virtual bool TryGetTimeSpanMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter)
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
                    methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var leftSegment = visitor.VisitAndDeferred(args[0]);
                        var rightSegment = visitor.VisitAndDeferred(args[1]);

                        leftSegment.Merge(rightSegment);
                        return args[0].Change($"(CASE WHEN DATEDIFF(MICROSECOND,{this.GetQuotedValue(leftSegment)}={this.GetQuotedValue(rightSegment)} THEN 0 WHEN {leftSegment}>{rightSegment} THEN 1 ELSE -1 END)", false, true);
                    });
                    result = true;
                    break;
                case "Equals":
                    methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var leftSegment = visitor.VisitAndDeferred(args[0]);
                        var rightSegment = visitor.VisitAndDeferred(args[1]);

                        leftSegment.Merge(rightSegment);
                        return leftSegment.Change($"{leftSegment}={rightSegment}", false, true);
                    });
                    result = true;
                    break;
                case "FromDays":
                    methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(args[0]);
                        return args[0].Change($"{valueSegment}*{(long)1000000 * 60 * 60 * 24}", false, true);
                    });
                    result = true;
                    break;
                case "FromHours":
                    methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(args[0]);
                        return valueSegment.Change($"{valueSegment}*{(long)1000000 * 60 * 60}", false, true);
                    });
                    result = true;
                    break;
                case "FromMilliseconds":
                    methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(args[0]);
                        return valueSegment.Change($"{valueSegment}*1000", false, true);
                    });
                    result = true;
                    break;
                case "FromMinutes":
                    methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(args[0]);
                        return valueSegment.Change($"{valueSegment}*{(long)1000000 * 60}", false, true);
                    });
                    result = true;
                    break;
                case "FromSeconds":
                    methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(args[0]);
                        return valueSegment.Change($"{valueSegment}*1000000", false, true);
                    });
                    result = true;
                    break;
                case "FromTicks":
                    methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(args[0]);
                        return valueSegment.Change($"{valueSegment}/10", false, true);
                    });
                    result = true;
                    break;
                case "Parse":
                case "TryParse":
                    methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(args[0]);
                        if (valueSegment.IsConstantValue)
                            return valueSegment.Change(TimeSpan.Parse(valueSegment.ToString()));

                        return valueSegment.Change($"CAST({this.GetQuotedValue(valueSegment)} AS SIGNED)", false, true);
                    });
                    result = true;
                    break;
                case "ParseExact":
                case "TryParseExact":
                    methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(args[0]);
                        if (valueSegment.IsConstantValue)
                            return valueSegment.Change(TimeSpan.Parse(valueSegment.ToString()));

                        return valueSegment.Change($"CAST({this.GetQuotedValue(valueSegment)} AS SIGNED)", false, true);
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
                    methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);

                        targetSegment.Merge(rightSegment);
                        return targetSegment.Change($"(CASE WHEN {targetSegment}={rightSegment} THEN 0 WHEN {targetSegment}>{rightSegment} THEN 1 ELSE -1 END)", false, true);
                    });
                    result = true;
                    break;
                case "Equals":
                    methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);

                        targetSegment.Merge(rightSegment);
                        return targetSegment.Change($"{this.GetQuotedValue(targetSegment)}={this.GetQuotedValue(rightSegment)}", false, true);
                    });
                    result = true;
                    break;
                case "Add":
                    methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);

                        if (targetSegment.IsConstantValue && rightSegment.IsConstantValue)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).Add((TimeSpan)rightSegment.Value));

                        targetSegment.Merge(rightSegment);
                        return targetSegment.Change($"{targetSegment}+{rightSegment}", false, true);
                    });
                    result = true;
                    break;
                case "Subtract":
                    methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);
                        if (targetSegment.IsConstantValue && rightSegment.IsConstantValue)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).Subtract((TimeSpan)rightSegment.Value));

                        targetSegment.Merge(rightSegment);
                        return targetSegment.Change($"{targetSegment}-{rightSegment}", false, true);
                    });
                    result = true;
                    break;
                case "Multiply":
                    methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);
                        if (targetSegment.IsConstantValue && rightSegment.IsConstantValue)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).Multiply((double)rightSegment.Value));

                        targetSegment.Merge(rightSegment);
                        return targetSegment.Change($"{targetSegment}*{rightSegment}", false, true);
                    });
                    result = true;
                    break;
                case "Divide":
                    if (parameterInfos[0].ParameterType == typeof(double))
                    {
                        methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            var rightSegment = visitor.VisitAndDeferred(args[0]);
                            if (targetSegment.IsConstantValue && rightSegment.IsConstantValue)
                                return targetSegment.Change(((TimeSpan)targetSegment.Value).Divide((double)rightSegment.Value));

                            targetSegment.Merge(rightSegment);
                            return targetSegment.Change($"{targetSegment}/{rightSegment}", false, true);
                        });
                        result = true;
                    }
                    if (parameterInfos[0].ParameterType == typeof(TimeSpan))
                    {
                        methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            var rightSegment = visitor.VisitAndDeferred(args[0]);
                            if (targetSegment.IsConstantValue && rightSegment.IsConstantValue)
                                return targetSegment.Change(((TimeSpan)targetSegment.Value).Divide((TimeSpan)rightSegment.Value));

                            targetSegment.Merge(rightSegment);
                            return targetSegment.Change($"{targetSegment}/{rightSegment}", false, true);
                        });
                        result = true;
                    }
                    break;
            }
        }
        return result;
    }
}
