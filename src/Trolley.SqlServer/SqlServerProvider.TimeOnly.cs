using System;
using System.Linq.Expressions;

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
                case "MinValue":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) => target.ChangeValue(TimeOnly.MinValue, true));
                    result = true;
                    break;
                case "MaxValue":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) => target.ChangeValue(TimeOnly.MaxValue, true));
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
                            return targetSegment.ChangeValue(((TimeOnly)targetSegment.Value).Ticks);

                        return targetSegment.Change($"DATEDIFF_BIG(MICROSECOND,CAST('00:00:00' AS TIME),{targetSegment.Body})*10");
                    });
                    result = true;
                    break;
                case "Hour":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((TimeOnly)targetSegment.Value).Hour);

                        return targetSegment.Change($"DATEPART(HOUR,{targetSegment.Body})", false, true);
                    });
                    result = true;
                    break;
                case "Millisecond":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((TimeOnly)targetSegment.Value).Millisecond);

                        return targetSegment.Change($"DATEPART(MILLISECOND,{targetSegment.Body})", false, true);
                    });
                    result = true;
                    break;
                case "Minute":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((TimeOnly)targetSegment.Value).Minute);

                        return targetSegment.Change($"DATEPART(MINUTE,{targetSegment.Body})", false, true);
                    });
                    result = true;
                    break;
                case "Second":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((TimeOnly)targetSegment.Value).Second);

                        return targetSegment.Change($"DATEPART(SECOND,{targetSegment.Body})", false, true);
                    });
                    result = true;
                    break;
            }
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
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        if (valueSegment.IsConstant || valueSegment.IsVariable)
                            return valueSegment.ChangeValue(TimeOnly.FromTimeSpan((TimeSpan)valueSegment.Value));

                        return valueSegment.Change($"CAST({valueSegment.Body} AS TIME)", false, true);
                    });
                    result = true;
                    break;
                case "FromDateTime":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        if (valueSegment.IsConstant || valueSegment.IsVariable)
                            return valueSegment.ChangeValue(TimeOnly.FromDateTime((DateTime)valueSegment.Value));

                        return valueSegment.Change($"CAST({valueSegment.Body} AS TIME)", false, true);
                    });
                    result = true;
                    break;
                case "Parse":
                case "TryParse":
                    if (parameterInfos.Length >= 1 && parameterInfos[0].ParameterType == typeof(string))
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var valueSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                            if (valueSegment.IsConstant || valueSegment.IsVariable)
                                return valueSegment.ChangeValue(TimeOnly.Parse(valueSegment.Value.ToString()));

                            return valueSegment.Change($"CAST({valueSegment.Body} AS TIME)", false, true);
                        });
                        result = true;
                    }
                    break;
                case "ParseExact":
                case "TryParseExact":
                    if (parameterInfos.Length >= 2 && parameterInfos[0].ParameterType == typeof(string) && parameterInfos[1].ParameterType == typeof(string))
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var valueSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                            var formatSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[1] });
                            if ((valueSegment.IsConstant || valueSegment.IsVariable)
                                && (formatSegment.IsConstant || formatSegment.IsVariable))
                                return valueSegment.MergeValue(formatSegment, TimeOnly.ParseExact(valueSegment.Value.ToString(), formatSegment.Value.ToString()));

                            var valueArgument = visitor.GetQuotedValue(valueSegment);
                            return valueSegment.Merge(formatSegment, $"CAST({valueArgument} AS TIME)", false, true);
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
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return targetSegment.MergeValue(rightSegment, ((TimeOnly)targetSegment.Value).Add((TimeSpan)rightSegment.Value));

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(rightSegment, $"CAST(DATEADD(SECOND,DATEDIFF(SECOND,'00:00:00',{targetArgument})+DATEDIFF(SECOND,'00:00:00',{rightArgument}),'00:00:00') AS TIME)", false, true);
                    });
                    result = true;
                    break;
                case "AddHours":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return targetSegment.MergeValue(rightSegment, ((TimeOnly)targetSegment.Value).AddHours((double)rightSegment.Value));

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(rightSegment, $"CAST(DATEADD(HOUR,{rightArgument},{targetArgument}) AS TIME)", false, true);
                    });
                    result = true;
                    break;
                case "AddMinutes":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return targetSegment.MergeValue(rightSegment, ((TimeOnly)targetSegment.Value).AddMinutes((double)rightSegment.Value));

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(rightSegment, $"CAST(DATEADD(MINUTE,{rightArgument},{targetArgument}) AS TIME)", false, true);
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
                        return targetSegment.Merge(rightSegment, $"CASE WHEN ({targetArgument}={rightArgument} THEN 0 WHEN {targetArgument}>{rightArgument} THEN 1 ELSE -1 END");
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
                case "ToTimeSpan":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                        if (targetSegment.IsConstant && targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((TimeOnly)targetSegment.Value).ToTimeSpan());

                        return targetSegment.Change(this.CastTo(typeof(TimeSpan), targetSegment.Body), false, true);
                    });
                    result = true;
                    break;
            }
        }
        return result;
    }
}
