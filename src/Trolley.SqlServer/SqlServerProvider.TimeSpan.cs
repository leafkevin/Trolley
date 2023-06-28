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
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((TimeSpan)targetSegment.Value).Ticks);
                        var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                        return visitor.Change(targetSegment, $"DATEDIFF_BIG(MICROSECOND,CAST('00:00:00' AS TIME),{targetArgument})*10", true, false);
                    });
                    result = true;
                    break;
                case "Days":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((TimeSpan)targetSegment.Value).Days);

                        var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                        return visitor.Change(targetSegment, $"DATEPART(DAY,{targetArgument})", false, true);
                    });
                    result = true;
                    break;
                case "Hours":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((TimeSpan)targetSegment.Value).Hours);

                        var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                        return visitor.Change(targetSegment, $"DATEPART(HOUR,{targetArgument})", false, true);
                    });
                    result = true;
                    break;
                case "Milliseconds":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((TimeSpan)targetSegment.Value).Milliseconds);

                        var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                        return visitor.Change(targetSegment, $"DATEPART(MILLISECOND,{targetArgument})", false, true);
                    });
                    result = true;
                    break;
                case "Minutes":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((TimeSpan)targetSegment.Value).Minutes);

                        var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                        return visitor.Change(targetSegment, $"DATEPART(MINUTE,{targetArgument})", false, true);
                    });
                    result = true;
                    break;
                case "Seconds":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((TimeSpan)targetSegment.Value).Seconds);

                        var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                        return visitor.Change(targetSegment, $"DATEPART(SECOND,{targetArgument})", false, true);
                    });
                    result = true;
                    break;
                case "TotalDays":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((TimeSpan)targetSegment.Value).TotalDays);

                        var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                        return visitor.Change(targetSegment, $"DATEDIFF_BIG(SECOND,'00:00:00',{targetArgument})/CAST({3600 * 24} AS FLOAT)", true, false);
                    });
                    result = true;
                    break;
                case "TotalHours":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((TimeSpan)targetSegment.Value).TotalHours);

                        var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                        return visitor.Change(targetSegment, $"DATEDIFF_BIG(SECOND,'00:00:00',{targetArgument})/CAST(3600 AS FLOAT)", true, false);
                    });
                    result = true;
                    break;
                case "TotalMilliseconds":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((TimeSpan)targetSegment.Value).TotalMilliseconds);

                        var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                        return visitor.Change(targetSegment, $"CAST(DATEDIFF_BIG(MILLISECOND,'00:00:00',{targetArgument}) AS FLOAT)", false, true);
                    });
                    result = true;
                    break;
                case "TotalMinutes":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((TimeSpan)targetSegment.Value).TotalMinutes);

                        var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                        return visitor.Change(targetSegment, $"DATEDIFF_BIG(SECOND,'00:00:00',{targetArgument})/CAST(60 AS FLOAT)", true, false);
                    });
                    result = true;
                    break;
                case "TotalSeconds":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((TimeSpan)targetSegment.Value).TotalSeconds);

                        var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                        return visitor.Change(targetSegment, $"CAST(DATEDIFF_BIG(SECOND,'00:00:00',{targetArgument}) AS FLOAT)", false, true);
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
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var leftSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        var rightSegment = visitor.VisitAndDeferred(leftSegment.Clone(args[1]));

                        var leftArgument = this.GetQuotedValue(visitor.Change(leftSegment));
                        var rightArgument = this.GetQuotedValue(visitor.Change(rightSegment));
                        return visitor.Merge(leftSegment, rightSegment, $"CASE WHEN ({leftArgument}={rightArgument} THEN 0 WHEN ({leftArgument}>{rightArgument})=1 THEN 1 ELSE -1 END", true, false);
                    });
                    result = true;
                    break;
                case "Equals":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var leftSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        var rightSegment = visitor.VisitAndDeferred(leftSegment.Clone(args[1]));

                        var leftArgument = this.GetQuotedValue(visitor.Change(leftSegment));
                        var rightArgument = this.GetQuotedValue(visitor.Change(rightSegment));
                        return visitor.Merge(leftSegment, rightSegment, $"{leftArgument}={rightArgument}", true, false);
                    });
                    result = true;
                    break;
                case "FromDays":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if (valueSegment.IsConstant || valueSegment.IsVariable)
                            return visitor.Change(valueSegment, TimeSpan.FromDays((double)valueSegment.Value));

                        var valueArgument = this.GetQuotedValue(visitor.Change(valueSegment));
                        return visitor.Change(valueSegment, $"CAST(DATEADD(MILLISECOND,CAST({valueArgument}*{(long)1000 * 60 * 60 * 24} AS BIGINT),'00:00:00') AS TIME)", false, true);
                    });
                    result = true;
                    break;
                case "FromHours":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if (valueSegment.IsConstant || valueSegment.IsVariable)
                            return visitor.Change(valueSegment, TimeSpan.FromHours((double)valueSegment.Value));
                        var valueArgument = this.GetQuotedValue(visitor.Change(valueSegment));
                        return visitor.Change(valueSegment, $"CAST(DATEADD(MILLISECOND,CAST({valueArgument}*{(long)1000 * 60 * 60} AS BIGINT),'00:00:00') AS TIME)", false, true);
                    });
                    result = true;
                    break;
                case "FromMilliseconds":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if (valueSegment.IsConstant || valueSegment.IsVariable)
                            return visitor.Change(valueSegment, TimeSpan.FromMilliseconds((double)valueSegment.Value));
                        var valueArgument = this.GetQuotedValue(visitor.Change(valueSegment));
                        return visitor.Change(valueSegment, $"CAST(DATEADD(MILLISECOND,{valueArgument},'00:00:00') AS TIME)", false, true);
                    });
                    result = true;
                    break;
                case "FromMinutes":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if (valueSegment.IsConstant || valueSegment.IsVariable)
                            return visitor.Change(valueSegment, TimeSpan.FromMinutes((double)valueSegment.Value));
                        var valueArgument = this.GetQuotedValue(visitor.Change(valueSegment));
                        return visitor.Change(valueSegment, $"CAST(DATEADD(MILLISECOND,CAST({valueArgument}*{(long)1000 * 60} AS BIGINT),'00:00:00') AS TIME)", false, true);
                    });
                    result = true;
                    break;
                case "FromSeconds":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if (valueSegment.IsConstant || valueSegment.IsVariable)
                            return visitor.Change(valueSegment, TimeSpan.FromSeconds((double)valueSegment.Value));
                        var valueArgument = this.GetQuotedValue(visitor.Change(valueSegment));
                        return visitor.Change(valueSegment, $"CAST(DATEADD(MILLISECOND,CAST({valueArgument}*1000 AS BIGINT),'00:00:00') AS TIME)", false, true);
                    });
                    result = true;
                    break;
                case "FromTicks":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if (valueSegment.IsConstant || valueSegment.IsVariable)
                            return visitor.Change(valueSegment, TimeSpan.FromTicks((long)valueSegment.Value));
                        var valueArgument = this.GetQuotedValue(visitor.Change(valueSegment));
                        return visitor.Change(valueSegment, $"CAST(DATEADD(MILLISECOND,{valueArgument}/{TimeSpan.TicksPerMillisecond},'00:00:00') AS TIME)", false, true);
                    });
                    result = true;
                    break;
                case "Parse":
                case "TryParse":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if (valueSegment.IsConstant || valueSegment.IsVariable)
                            return visitor.Change(valueSegment, TimeSpan.Parse(valueSegment.ToString()));
                        var valueArgument = this.GetQuotedValue(visitor.Change(valueSegment));
                        return visitor.Change(valueSegment, $"CAST({valueArgument} AS TIME)", false, true);
                    });
                    result = true;
                    break;
                case "ParseExact":
                case "TryParseExact":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        var formatSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[1] });
                        if ((valueSegment.IsConstant || valueSegment.IsVariable)
                            && (formatSegment.IsConstant || formatSegment.IsVariable))
                            return visitor.Merge(valueSegment, formatSegment, TimeSpan.ParseExact(valueSegment.ToString(), formatSegment.ToString(), CultureInfo.InvariantCulture));

                        var valueArgument = this.GetQuotedValue(visitor.Change(valueSegment));
                        return visitor.Merge(valueSegment, formatSegment, $"CAST({valueArgument} AS TIME)", false, true);
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
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(targetSegment.Clone(args[0]));

                        var targetArgument = this.GetQuotedValue(visitor.Change(targetSegment));
                        var rightArgument = this.GetQuotedValue(visitor.Change(rightSegment));
                        return visitor.Merge(targetSegment, rightSegment, $"CASE WHEN {targetArgument}={rightArgument} THEN 0 WHEN {targetArgument}>{rightArgument} THEN 1 ELSE -1 END", true, false);
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
                case "Add":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(targetSegment.Clone(args[0]));

                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return visitor.Merge(targetSegment, rightSegment, ((TimeSpan)targetSegment.Value).Add((TimeSpan)rightSegment.Value));

                        var targetArgument = this.GetQuotedValue(targetSegment);
                        var rightArgument = this.GetQuotedValue(rightSegment);
                        return visitor.Merge(targetSegment, rightSegment, $"CAST(DATEADD(MILLISECOND,DATEDIFF(MILLISECOND,'00:00:00',{targetArgument})+DATEDIFF(MILLISECOND,'00:00:00',{rightArgument}),'00:00:00') AS TIME)", false, true);
                    });
                    result = true;
                    break;
                case "Subtract":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(targetSegment.Clone(args[0]));
                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return visitor.Merge(targetSegment, rightSegment, ((TimeSpan)targetSegment.Value).Subtract((TimeSpan)rightSegment.Value));

                        var targetArgument = this.GetQuotedValue(targetSegment);
                        var rightArgument = this.GetQuotedValue(rightSegment);
                        return visitor.Merge(targetSegment, rightSegment, $"CAST(DATEADD(MILLISECOND,DATEDIFF(MILLISECOND,'00:00:00',{targetArgument})-DATEDIFF(MILLISECOND,'00:00:00',{rightArgument}),'00:00:00') AS TIME)", false, true);
                    });
                    result = true;
                    break;
                case "Multiply":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return visitor.Merge(targetSegment, rightSegment, ((TimeSpan)targetSegment.Value).Multiply((double)rightSegment.Value));

                        var targetArgument = this.GetQuotedValue(targetSegment);
                        var rightArgument = this.GetQuotedValue(rightSegment);
                        return visitor.Merge(targetSegment, rightSegment, $"CAST(DATEADD(MILLISECOND,DATEDIFF(MILLISECOND,'00:00:00',{targetArgument})*{rightArgument},'00:00:00') AS TIME)", false, true);
                    });
                    result = true;
                    break;
                case "Divide":
                    if (parameterInfos[0].ParameterType == typeof(double))
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                            var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (rightSegment.IsConstant || rightSegment.IsVariable))
                                return visitor.Merge(targetSegment, rightSegment, ((TimeSpan)targetSegment.Value).Divide((double)rightSegment.Value));

                            var targetArgument = this.GetQuotedValue(targetSegment);
                            var rightArgument = this.GetQuotedValue(rightSegment);
                            return visitor.Merge(targetSegment, rightSegment, $"CAST(DATEADD(MILLISECOND,DATEDIFF(MILLISECOND,'00:00:00',{targetArgument})/{rightArgument},'00:00:00') AS TIME)", false, true);
                        });
                        result = true;
                    }
                    if (parameterInfos[0].ParameterType == typeof(TimeSpan))
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                            var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (rightSegment.IsConstant || rightSegment.IsVariable))
                                return visitor.Merge(targetSegment, rightSegment, ((TimeSpan)targetSegment.Value).Divide((TimeSpan)rightSegment.Value));

                            var targetArgument = this.GetQuotedValue(targetSegment);
                            var rightArgument = this.GetQuotedValue(rightSegment);
                            return visitor.Merge(targetSegment, rightSegment, $"CAST(DATEADD(MILLISECOND,DATEDIFF(MILLISECOND,'00:00:00',{targetArgument})/DATEDIFF(MILLISECOND,'00:00:00',{rightArgument}),'00:00:00') AS TIME)", false, true);
                        });
                        result = true;
                    }
                    break;
            }
        }
        return result;
    }
}
