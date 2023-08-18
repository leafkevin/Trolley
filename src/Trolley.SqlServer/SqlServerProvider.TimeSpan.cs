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
                            return targetSegment.ChangeValue(((TimeSpan)targetSegment.Value).Ticks);

                        var targetArgument = this.GetQuotedValue(targetSegment);
                        return visitor.Change(targetSegment, $"DATEDIFF_BIG(MICROSECOND,CAST('00:00:00' AS TIME),{targetArgument})*10", true, false);
                    });
                    result = true;
                    break;
                case "Days":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((TimeSpan)targetSegment.Value).Days);

                        var targetArgument = this.GetQuotedValue(targetSegment);
                        return visitor.Change(targetSegment, $"DATEPART(DAY,{targetArgument})", false, true);
                    });
                    result = true;
                    break;
                case "Hours":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((TimeSpan)targetSegment.Value).Hours);

                        var targetArgument = this.GetQuotedValue(targetSegment);
                        return visitor.Change(targetSegment, $"DATEPART(HOUR,{targetArgument})", false, true);
                    });
                    result = true;
                    break;
                case "Milliseconds":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((TimeSpan)targetSegment.Value).Milliseconds);

                        var targetArgument = this.GetQuotedValue(targetSegment);
                        return visitor.Change(targetSegment, $"DATEPART(MILLISECOND,{targetArgument})", false, true);
                    });
                    result = true;
                    break;
                case "Minutes":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((TimeSpan)targetSegment.Value).Minutes);

                        var targetArgument = this.GetQuotedValue(targetSegment);
                        return visitor.Change(targetSegment, $"DATEPART(MINUTE,{targetArgument})", false, true);
                    });
                    result = true;
                    break;
                case "Seconds":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((TimeSpan)targetSegment.Value).Seconds);

                        var targetArgument = this.GetQuotedValue(targetSegment);
                        return visitor.Change(targetSegment, $"DATEPART(SECOND,{targetArgument})", false, true);
                    });
                    result = true;
                    break;
                case "TotalDays":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((TimeSpan)targetSegment.Value).TotalDays);

                        var targetArgument = this.GetQuotedValue(targetSegment);
                        return visitor.Change(targetSegment, $"DATEDIFF_BIG(SECOND,'00:00:00',{targetArgument})/CAST({3600 * 24} AS FLOAT)", true, false);
                    });
                    result = true;
                    break;
                case "TotalHours":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((TimeSpan)targetSegment.Value).TotalHours);

                        var targetArgument = this.GetQuotedValue(targetSegment);
                        return visitor.Change(targetSegment, $"DATEDIFF_BIG(SECOND,'00:00:00',{targetArgument})/CAST(3600 AS FLOAT)", true, false);
                    });
                    result = true;
                    break;
                case "TotalMilliseconds":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((TimeSpan)targetSegment.Value).TotalMilliseconds);

                        var targetArgument = this.GetQuotedValue(targetSegment);
                        return visitor.Change(targetSegment, $"CAST(DATEDIFF_BIG(MILLISECOND,'00:00:00',{targetArgument}) AS FLOAT)", false, true);
                    });
                    result = true;
                    break;
                case "TotalMinutes":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((TimeSpan)targetSegment.Value).TotalMinutes);

                        var targetArgument = this.GetQuotedValue(targetSegment);
                        return visitor.Change(targetSegment, $"DATEDIFF_BIG(SECOND,'00:00:00',{targetArgument})/CAST(60 AS FLOAT)", true, false);
                    });
                    result = true;
                    break;
                case "TotalSeconds":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((TimeSpan)targetSegment.Value).TotalSeconds);

                        var targetArgument = this.GetQuotedValue(targetSegment);
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
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[1] });
                        visitor.ChangeSameType(leftSegment, rightSegment);
                        var leftArgument = this.GetQuotedValue(leftSegment);
                        var rightArgument = this.GetQuotedValue(rightSegment);
                        return visitor.Merge(leftSegment, rightSegment, $"CASE WHEN ({leftArgument}={rightArgument} THEN 0 WHEN ({leftArgument}>{rightArgument})=1 THEN 1 ELSE -1 END", true, false);
                    });
                    result = true;
                    break;
                case "Equals":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var leftSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[1] });
                        visitor.ChangeSameType(leftSegment, rightSegment);
                        var leftArgument = this.GetQuotedValue(leftSegment);
                        var rightArgument = this.GetQuotedValue(rightSegment);
                        return visitor.Merge(leftSegment, rightSegment, $"{leftArgument}={rightArgument}", true, false);
                    });
                    result = true;
                    break;
                case "FromDays":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if (valueSegment.IsConstant || valueSegment.IsVariable)
                            return valueSegment.ChangeValue(TimeSpan.FromDays(Convert.ToDouble(valueSegment.Value)));

                        var valueArgument = this.GetQuotedValue(valueSegment);
                        return visitor.Change(valueSegment, $"CAST(DATEADD(MILLISECOND,CAST({valueArgument}*{(long)1000 * 60 * 60 * 24} AS BIGINT),'00:00:00') AS TIME)", false, true);
                    });
                    result = true;
                    break;
                case "FromHours":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if (valueSegment.IsConstant || valueSegment.IsVariable)
                            return valueSegment.ChangeValue(TimeSpan.FromHours(Convert.ToDouble(valueSegment.Value)));
                        var valueArgument = this.GetQuotedValue(valueSegment);
                        return visitor.Change(valueSegment, $"CAST(DATEADD(MILLISECOND,CAST({valueArgument}*{(long)1000 * 60 * 60} AS BIGINT),'00:00:00') AS TIME)", false, true);
                    });
                    result = true;
                    break;
                case "FromMilliseconds":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if (valueSegment.IsConstant || valueSegment.IsVariable)
                            return valueSegment.ChangeValue(TimeSpan.FromMilliseconds(Convert.ToDouble(valueSegment.Value)));
                        var valueArgument = this.GetQuotedValue(valueSegment);
                        return visitor.Change(valueSegment, $"CAST(DATEADD(MILLISECOND,{valueArgument},'00:00:00') AS TIME)", false, true);
                    });
                    result = true;
                    break;
                case "FromMinutes":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if (valueSegment.IsConstant || valueSegment.IsVariable)
                            return valueSegment.ChangeValue(TimeSpan.FromMinutes(Convert.ToDouble(valueSegment.Value)));
                        var valueArgument = this.GetQuotedValue(valueSegment);
                        return visitor.Change(valueSegment, $"CAST(DATEADD(MILLISECOND,CAST({valueArgument}*{(long)1000 * 60} AS BIGINT),'00:00:00') AS TIME)", false, true);
                    });
                    result = true;
                    break;
                case "FromSeconds":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if (valueSegment.IsConstant || valueSegment.IsVariable)
                            return valueSegment.ChangeValue(TimeSpan.FromSeconds(Convert.ToDouble(valueSegment.Value)));
                        var valueArgument = this.GetQuotedValue(valueSegment);
                        return visitor.Change(valueSegment, $"CAST(DATEADD(MILLISECOND,CAST({valueArgument}*1000 AS BIGINT),'00:00:00') AS TIME)", false, true);
                    });
                    result = true;
                    break;
                case "FromTicks":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if (valueSegment.IsConstant || valueSegment.IsVariable)
                            return valueSegment.ChangeValue(TimeSpan.FromTicks(Convert.ToInt64(valueSegment.Value)));
                        var valueArgument = this.GetQuotedValue(valueSegment);
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
                            return valueSegment.ChangeValue(TimeSpan.Parse(valueSegment.ToString()));
                        var valueArgument = this.GetQuotedValue(valueSegment);
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
                            return valueSegment.Merge(formatSegment).ChangeValue(TimeSpan.ParseExact(valueSegment.ToString(), formatSegment.ToString(), CultureInfo.InvariantCulture));

                        var valueArgument = this.GetQuotedValue(valueSegment);
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
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        visitor.ChangeSameType(targetSegment, rightSegment);
                        var targetArgument = this.GetQuotedValue(targetSegment);
                        var rightArgument = this.GetQuotedValue(rightSegment);
                        return visitor.Merge(targetSegment, rightSegment, $"CASE WHEN {targetArgument}={rightArgument} THEN 0 WHEN {targetArgument}>{rightArgument} THEN 1 ELSE -1 END", true, false);
                    });
                    result = true;
                    break;
                case "Equals":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });

                        var targetArgument = this.GetQuotedValue(targetSegment);
                        var rightArgument = this.GetQuotedValue(rightSegment);
                        return visitor.Merge(targetSegment, rightSegment, $"{targetArgument}={rightArgument}", true, false);
                    });
                    result = true;
                    break;
                case "Add":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });

                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return targetSegment.Merge(rightSegment).ChangeValue(((TimeSpan)targetSegment.Value).Add((TimeSpan)rightSegment.Value));

                        var targetArgument = this.GetQuotedValue(targetSegment);
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
                            //变量当作常量处理
                            targetSegment.IsVariable = false;
                            return targetSegment.Change(builder.ToString(), false, false, true);
                        }
                        //非常量、变量的，只能小于一天
                        visitor.Change(rightSegment, $"DATEDIFF_BIG(MILLISECOND,'00:00:00',{rightSegment}");
                        var rightArgument = this.GetQuotedValue(rightSegment);
                        return visitor.Merge(targetSegment, rightSegment, $"DATEADD(MILLISECOND,-{rightArgument},{targetArgument})", false, true);
                    });
                    result = true;
                    break;
                case "Subtract":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return targetSegment.Merge(rightSegment).ChangeValue(((TimeSpan)targetSegment.Value).Subtract((TimeSpan)rightSegment.Value));

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
                            return targetSegment.Merge(rightSegment).ChangeValue(((TimeSpan)targetSegment.Value).Multiply((double)rightSegment.Value));

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
                                return targetSegment.Merge(rightSegment).ChangeValue(((TimeSpan)targetSegment.Value).Divide((double)rightSegment.Value));

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
                                return targetSegment.Merge(rightSegment).ChangeValue(((TimeSpan)targetSegment.Value).Divide((TimeSpan)rightSegment.Value));

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
