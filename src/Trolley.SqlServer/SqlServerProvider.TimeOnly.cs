using System;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.SqlServer;

partial class SqlServerProvider
{
    public override bool TryGetTimeOnlyMemberAccessSqlFormatter(MemberExpression memberExpr, out MemberAccessSqlFormatter formatter)
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
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) => target.Change(TimeOnly.MinValue));
                    result = true;
                    break;
                case "MaxValue":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) => target.Change(TimeOnly.MaxValue));
                    result = true;
                    break;
            }
            return result;
        }
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
            case "Hour":
                memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                {
                    var targetSegment = visitor.VisitAndDeferred(target);
                    if (targetSegment.IsConstant)
                        return targetSegment.Change(((TimeOnly)targetSegment.Value).Hour);

                    return targetSegment.Change($"DATEPART(HH,{this.GetQuotedValue(targetSegment)})", false, false, true);
                });
                result = true;
                break;
            case "Millisecond":
                memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                {
                    var targetSegment = visitor.VisitAndDeferred(target);
                    if (targetSegment.IsConstant)
                        return targetSegment.Change(((TimeOnly)targetSegment.Value).Millisecond);

                    return targetSegment.Change($"DATEPART(MS,{this.GetQuotedValue(targetSegment)})", false, false, true);
                });
                result = true;
                break;
            case "Minute":
                memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                {
                    var targetSegment = visitor.VisitAndDeferred(target);
                    if (targetSegment.IsConstant)
                        return targetSegment.Change(((TimeOnly)targetSegment.Value).Minute);

                    return targetSegment.Change($"DATEPART(MI,{this.GetQuotedValue(targetSegment)})", false, false, true);
                });
                result = true;
                break;
            case "Second":
                memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                {
                    var targetSegment = visitor.VisitAndDeferred(target);
                    if (targetSegment.IsConstant)
                        return targetSegment.Change(((TimeOnly)targetSegment.Value).Second);

                    return targetSegment.Change($"DATEPART(SS,{this.GetQuotedValue(targetSegment)})", false, false, true);
                });
                result = true;
                break;
        }
        return result;
    }
    public override bool TryGetTimeOnlyMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter)
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
                case "FromTimeSpan":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(args[0]);
                        if (valueSegment.IsConstant)
                            valueSegment.Change(TimeOnly.FromTimeSpan((TimeSpan)valueSegment.Value));

                        return valueSegment.Change($"CAST({this.GetQuotedValue(valueSegment)} AS TIME(7))", false, false, true);
                    });
                    result = true;
                    break;
                case "FromDateTime":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(args[0]);
                        if (valueSegment.IsConstant)
                            valueSegment.Value = TimeOnly.FromDateTime((DateTime)valueSegment.Value);

                        return valueSegment.Change($"CAST({this.GetQuotedValue(valueSegment)} AS TIME(7))", false, false, true);
                    });
                    result = true;
                    break;
                case "Parse":
                case "TryParse":
                    if (parameterInfos.Length >= 1 && parameterInfos[0].ParameterType == typeof(string))
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var valueSegment = visitor.VisitAndDeferred(args[0]);
                            if (valueSegment.IsConstant)
                                return valueSegment.Change(TimeOnly.Parse(valueSegment.ToString()));

                            return valueSegment.Change($"CAST({this.GetQuotedValue(valueSegment)} AS TIME(7))", false, false, true);
                        });
                        result = true;
                    }
                    break;
                case "ParseExact":
                case "TryParseExact":
                    if (parameterInfos.Length >= 2 && parameterInfos[0].ParameterType == typeof(string) && parameterInfos[1].ParameterType == typeof(string))
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var valueSegment = visitor.VisitAndDeferred(args[0]);
                            var formatSegment = visitor.VisitAndDeferred(args[1]);

                            if (valueSegment.IsConstant && formatSegment.IsConstant)
                                valueSegment.Change(TimeOnly.ParseExact(valueSegment.ToString(), formatSegment.ToString()));

                            return valueSegment.Change($"CAST({this.GetQuotedValue(valueSegment)} AS TIME(7))", false, false, true);
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
                case "Add":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);

                        if (targetSegment.IsConstant && rightSegment.IsConstant)
                            return targetSegment.Change(((TimeOnly)targetSegment.Value).Add((TimeSpan)rightSegment.Value));

                        targetSegment.Merge(rightSegment);
                        return targetSegment.Change($"TIME('{targetSegment}+{rightSegment}')", false, false, true);
                    });
                    result = true;
                    break;
                case "CompareTo":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);

                        targetSegment.Merge(rightSegment);
                        return targetSegment.Change($"(CASE WHEN ({this.GetQuotedValue(targetSegment)}={this.GetQuotedValue(rightSegment)} THEN 0 WHEN ({this.GetQuotedValue(targetSegment)}>{this.GetQuotedValue(rightSegment)})=1 THEN 1 ELSE -1 END)", false, false, true);
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

                    //case "Subtract":
                    //    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    //    {
                    //        var targetSegment = visitor.VisitAndDeferred(target);
                    //        var rightSegment = visitor.VisitAndDeferred(args[0]);
                    //        if (targetSegment.IsConstant && rightSegment.IsConstant)
                    //            return targetSegment.Change(((TimeOnly)targetSegment.Value).Subtract((TimeOnly)rightSegment.Value));

                    //        targetSegment.Merge(rightSegment);
                    //        return targetSegment.Change($"{targetSegment}-{rightSegment}", false, true);
                    //    });
                    //    result = true;
                    //    break;
                    //case "Multiply":
                    //    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    //    {
                    //        var targetSegment = visitor.VisitAndDeferred(target);
                    //        var rightSegment = visitor.VisitAndDeferred(args[0]);
                    //        if (targetSegment.IsConstant && rightSegment.IsConstant)
                    //            return targetSegment.Change(((TimeOnly)targetSegment.Value).Multiply((double)rightSegment.Value));

                    //        targetSegment.Merge(rightSegment);
                    //        return targetSegment.Change($"{targetSegment}*{rightSegment}", false, true);
                    //    });
                    //    result = true;
                    //    break;
                    //case "Divide":
                    //    if (parameterInfos[0].ParameterType == typeof(double))
                    //    {
                    //        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    //        {
                    //            var targetSegment = visitor.VisitAndDeferred(target);
                    //            var rightSegment = visitor.VisitAndDeferred(args[0]);
                    //            if (targetSegment.IsConstant && rightSegment.IsConstant)
                    //                return targetSegment.Change(((TimeOnly)targetSegment.Value).Divide((double)rightSegment.Value));

                    //            targetSegment.Merge(rightSegment);
                    //            return targetSegment.Change($"{targetSegment}/{rightSegment}", false, true);
                    //        });
                    //        result = true;
                    //    }
                    //    if (parameterInfos[0].ParameterType == typeof(TimeOnly))
                    //    {
                    //        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    //        {
                    //            var targetSegment = visitor.VisitAndDeferred(target);
                    //            var rightSegment = visitor.VisitAndDeferred(args[0]);
                    //            if (targetSegment.IsConstant && rightSegment.IsConstant)
                    //                return targetSegment.Change(((TimeOnly)targetSegment.Value).Divide((TimeOnly)rightSegment.Value));

                    //            targetSegment.Merge(rightSegment);
                    //            return targetSegment.Change($"{targetSegment}/{rightSegment}", false, true);
                    //        });
                    //        result = true;
                    //    }
                    //    break;
            }
        }
        return result;
    }
}
