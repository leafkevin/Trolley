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
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) => target.Change(TimeOnly.MinValue));
                    result = true;
                    break;
                case "MaxValue":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) => target.Change(TimeOnly.MaxValue));
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
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((TimeOnly)targetSegment.Value).Ticks);

                        var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                        return visitor.Change(targetSegment, $"DATEDIFF_BIG(MICROSECOND,CAST('00:00:00' AS TIME),{targetArgument})*10", true, false);
                    });
                    result = true;
                    break;
                case "Hour":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((TimeOnly)targetSegment.Value).Hour);

                        var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                        return visitor.Change(targetSegment, $"DATEPART(HOUR,{targetArgument})", false, true);
                    });
                    result = true;
                    break;
                case "Millisecond":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((TimeOnly)targetSegment.Value).Millisecond);

                        var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                        return visitor.Change(targetSegment, $"DATEPART(MILLISECOND,{targetArgument})", false, true);
                    });
                    result = true;
                    break;
                case "Minute":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((TimeOnly)targetSegment.Value).Minute);

                        var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                        return visitor.Change(targetSegment, $"DATEPART(MINUTE,{targetArgument})", false, true);
                    });
                    result = true;
                    break;
                case "Second":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((TimeOnly)targetSegment.Value).Second);

                        var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                        return visitor.Change(targetSegment, $"DATEPART(SECOND,{targetArgument})", false, true);
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
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if (valueSegment.IsConstant || valueSegment.IsVariable)
                            return visitor.Change(valueSegment, TimeOnly.FromTimeSpan((TimeSpan)valueSegment.Value));

                        var valueArgument = this.GetQuotedValue(visitor.Change(valueSegment));
                        return visitor.Change(valueSegment, $"CAST({valueArgument} AS TIME)", false, true);
                    });
                    result = true;
                    break;
                case "FromDateTime":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if (valueSegment.IsConstant || valueSegment.IsVariable)
                            return visitor.Change(valueSegment, TimeOnly.FromDateTime((DateTime)valueSegment.Value));

                        var valueArgument = this.GetQuotedValue(visitor.Change(valueSegment));
                        return visitor.Change(valueSegment, $"CAST({valueArgument} AS TIME)", false, true);
                    });
                    result = true;
                    break;
                case "Parse":
                case "TryParse":
                    if (parameterInfos.Length >= 1 && parameterInfos[0].ParameterType == typeof(string))
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var valueSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                            if (valueSegment.IsConstant || valueSegment.IsVariable)
                                return visitor.Change(valueSegment, TimeOnly.Parse(valueSegment.ToString()));

                            var valueArgument = this.GetQuotedValue(visitor.Change(valueSegment));
                            return visitor.Change(valueSegment, $"CAST({valueArgument} AS TIME)", false, true);
                        });
                        result = true;
                    }
                    break;
                case "ParseExact":
                case "TryParseExact":
                    if (parameterInfos.Length >= 2 && parameterInfos[0].ParameterType == typeof(string) && parameterInfos[1].ParameterType == typeof(string))
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var valueSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                            var formatSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[1] });
                            if ((valueSegment.IsConstant || valueSegment.IsVariable)
                                && (formatSegment.IsConstant || formatSegment.IsVariable))
                                return visitor.Merge(valueSegment, formatSegment, TimeOnly.ParseExact(valueSegment.ToString(), formatSegment.ToString()));

                            var valueArgument = this.GetQuotedValue(visitor.Change(valueSegment));
                            return visitor.Merge(valueSegment, formatSegment, $"CAST({valueArgument} AS TIME)", false, true);
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
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return visitor.Merge(targetSegment, rightSegment, ((TimeOnly)targetSegment.Value).Add((TimeSpan)rightSegment.Value));

                        var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                        var rightArgument = this.GetQuotedValue(visitor.Change(rightSegment));
                        return visitor.Merge(targetSegment, rightSegment, $"CAST(DATEADD(SECOND,DATEDIFF(SECOND,'00:00:00',{targetArgument})+DATEDIFF(SECOND,'00:00:00',{rightArgument}),'00:00:00') AS TIME)", false, true);
                    });
                    result = true;
                    break;
                case "AddHours":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return visitor.Merge(targetSegment, rightSegment, ((TimeOnly)targetSegment.Value).AddHours((double)rightSegment.Value));

                        var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                        var rightArgument = this.GetQuotedValue(visitor.Change(rightSegment));
                        return visitor.Merge(targetSegment, rightSegment, $"CAST(DATEADD(HOUR,{rightArgument},{targetArgument}) AS TIME)", false, true);
                    });
                    result = true;
                    break;
                case "AddMinutes":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return visitor.Merge(targetSegment, rightSegment, ((TimeOnly)targetSegment.Value).AddMinutes((double)rightSegment.Value));

                        var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                        var rightArgument = this.GetQuotedValue(visitor.Change(rightSegment));
                        return visitor.Merge(targetSegment, rightSegment, $"CAST(DATEADD(MINUTE,{rightArgument},{targetArgument}) AS TIME)", false, true);
                    });
                    result = true;
                    break;
                case "CompareTo":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(targetSegment.Clone(args[0]));
                        var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                        var rightArgument = this.GetQuotedValue(visitor.Change(rightSegment));
                        return visitor.Merge(targetSegment, rightSegment, $"CASE WHEN ({targetArgument}={rightArgument} THEN 0 WHEN ({targetArgument}>{rightArgument})=1 THEN 1 ELSE -1 END", true, false);
                    });
                    result = true;
                    break;
                case "Equals":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(targetSegment.Clone(args[0]));
                        var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                        var rightArgument = this.GetQuotedValue(visitor.Change(rightSegment));
                        return visitor.Merge(targetSegment, rightSegment, $"{targetArgument}={rightArgument}", true, false);
                    });
                    result = true;
                    break;
                case "ToTimeSpan":
                    if (parameterInfos[0].ParameterType == typeof(double))
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                            if (targetSegment.IsConstant && targetSegment.IsVariable)
                                return visitor.Change(targetSegment, ((TimeOnly)targetSegment.Value).ToTimeSpan());

                            return visitor.Change(targetSegment, this.GetQuotedValue(visitor.Change(targetSegment)));
                        });
                        result = true;
                    }
                    break;
            }
        }
        return result;
    }
}
