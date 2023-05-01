using System;
using System.Linq.Expressions;

namespace Trolley.SqlServer;

partial class SqlServerProvider
{
    public override bool TryGetDateTimeMemberAccessSqlFormatter(MemberExpression memberExpr, out MemberAccessSqlFormatter formatter)
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
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) => target.Change(DateTime.MinValue));
                    result = true;
                    break;
                case "MaxValue":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) => target.Change(DateTime.MaxValue));
                    result = true;
                    break;
                case "UnixEpoch":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) => target.Change(DateTime.UnixEpoch));
                    result = true;
                    break;
                case "Today":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) => target.Change("DAY(GETDATE())", false, false, true));
                    result = true;
                    break;
                case "Now":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) => target.Change("GETDATE()", false, false, true));
                    result = true;
                    break;
                case "UtcNow":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) => target.Change("GETUTCDATE()", false, false, true));
                    result = true;
                    break;
            }
            return result;
        }
        else
        {
            switch (memberInfo.Name)
            {
                case "Date":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstantValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Date);

                        return targetSegment.Change($"CONVERT(CHAR(10),{this.GetQuotedValue(targetSegment)},120)", false, false, true);
                    });
                    result = true;
                    break;
                case "Day":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstantValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Day);

                        return targetSegment.Change($"DATEPART(DAY,{this.GetQuotedValue(targetSegment)})", false, false, true);
                    });
                    result = true;
                    break;
                case "DayOfWeek":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstantValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).DayOfWeek);

                        return targetSegment.Change($"(DATEPART(WEEKDAY,{this.GetQuotedValue(targetSegment)})-1)", false, false, true);
                    });
                    result = true;
                    break;
                case "DayOfYear":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstantValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).DayOfYear);

                        return targetSegment.Change($"DATEPART(DAYOFYEAR,{this.GetQuotedValue(targetSegment)})", false, false, true);
                    });
                    result = true;
                    break;
                case "Hour":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstantValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Hour);

                        return targetSegment.Change($"DATEPART(HOUR,{this.GetQuotedValue(targetSegment)})", false, false, true);
                    });
                    result = true;
                    break;
                case "Kind":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstantValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Kind);
                        throw new NotSupportedException("不支持的成员访问，DateTime只支持常量的Kind成员访问");
                    });
                    result = true;
                    break;
                case "Millisecond":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstantValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Millisecond);

                        return targetSegment.Change($"(DATEPART(MILLISECOND,{this.GetQuotedValue(targetSegment)})/1000)", false, false, true);
                    });
                    result = true;
                    break;
                case "Minute":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstantValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Minute);

                        return targetSegment.Change($"DATEPART(MINUTE,{this.GetQuotedValue(targetSegment)})", false, false, true);
                    });
                    result = true;
                    break;
                case "Month":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstantValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Month);

                        return targetSegment.Change($"DATEPART(MONTH,{this.GetQuotedValue(targetSegment)})", false, false, true);
                    });
                    result = true;
                    break;
                case "Second":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstantValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Second);

                        return targetSegment.Change($"DATEPART(SECOND,{this.GetQuotedValue(targetSegment)})", false, false, true);
                    });
                    result = true;
                    break;
                case "Ticks":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstantValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Ticks);

                        return targetSegment.Change($"(CAST(DATEDIFF(SECOND,'1970-01-01',{this.GetQuotedValue(targetSegment)}) AS BIGINT)*10000000+621355968000000000)", false, false, true);
                    });
                    result = true;
                    break;
                case "TimeOfDay":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstantValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).TimeOfDay);

                        return targetSegment.Change($"DATEDIFF(SECOND,CONVERT(CHAR(10),{this.GetQuotedValue(targetSegment)},120),{this.GetQuotedValue(targetSegment)})", false, false, true);
                    });
                    result = true;
                    break;
                case "Year":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstantValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Year);

                        return targetSegment.Change($"DATEPART(YEAR,{this.GetQuotedValue(targetSegment)})", false, false, true);
                    });
                    result = true;
                    break;
            }
        }
        return result;
    }
    public override bool TryGetDateTimeMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter)
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
                case "DaysInMonth":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var leftSegment = visitor.VisitAndDeferred(args[0]);
                        var rightSegment = visitor.VisitAndDeferred(args[1]);

                        if (leftSegment.IsConstantValue && rightSegment.IsConstantValue)
                            return leftSegment.Change(DateTime.DaysInMonth(Convert.ToInt32(leftSegment.Value), Convert.ToInt32(rightSegment.Value)));

                        leftSegment.Merge(rightSegment);
                        return leftSegment.Change($"DATEPART(DAY,DATEADD(DAY,-1,DATEADD(MONTH,1,CAST({this.GetQuotedValue(leftSegment)} AS VARCHAR(100))+'-'+CAST({this.GetQuotedValue(rightSegment)} AS VARCHAR(100))+'-1')))", false, false, true);
                    });
                    result = true;
                    break;
                case "IsLeapYear":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(args[0]);
                        if (valueSegment.IsConstantValue)
                            return valueSegment.Change(DateTime.IsLeapYear(Convert.ToInt32(valueSegment.Value)));

                        return args[0].Change($"(({valueSegment})%4=0 AND ({valueSegment})%100<>0 OR ({valueSegment})%400=0)", false, false, true);
                    });
                    result = true;
                    break;
                case "Parse":
                case "TryParse":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(args[0]);
                        if (valueSegment.IsConstantValue)
                            return valueSegment.Change(DateTime.Parse(valueSegment.ToString()));

                        return valueSegment.Change($"CAST({this.GetQuotedValue(valueSegment)} AS DATETIME)", false, false, true);
                    });
                    result = true;
                    if (methodInfo.IsStatic && parameterInfos.Length >= 3 && parameterInfos[0].ParameterType == typeof(ReadOnlySpan<char>))
                        throw new NotSupportedException("DateTime.Parse方法暂时不支持ReadOnlySpan<char>类型参数的解析，请转换成String类型");
                    break;
                case "ParseExact":
                case "TryParseExact":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(args[0]);
                        var formatSegment = visitor.VisitAndDeferred(args[1]);
                        var providerSegment = visitor.VisitAndDeferred(args[2]);

                        if (valueSegment.IsConstantValue && formatSegment.IsConstantValue && providerSegment.IsConstantValue)
                            return valueSegment.Change(DateTime.ParseExact(valueSegment.ToString(), formatSegment.ToString(), (IFormatProvider)providerSegment.Value));

                        valueSegment.Merge(formatSegment);
                        return valueSegment.Change($"CAST({this.GetQuotedValue(valueSegment)} AS DATETIME)", false, false, true);
                    });
                    result = true;
                    if (methodInfo.IsStatic && parameterInfos.Length >= 1 && parameterInfos[0].ParameterType == typeof(ReadOnlySpan<char>))
                        throw new NotSupportedException($"DateTime.{methodInfo.Name}方法暂时不支持ReadOnlySpan<char>类型参数的解析，请转换成String类型");
                    break;
                case "Compare":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var leftSegment = visitor.VisitAndDeferred(args[0]);
                        var rightSegment = visitor.VisitAndDeferred(args[1]);

                        leftSegment.Merge(rightSegment);
                        return leftSegment.Change($"(CASE WHEN DATEDIFF_BIG(MS,{this.GetQuotedValue(leftSegment)},{this.GetQuotedValue(rightSegment)})=0 THEN 0 WHEN DATEDIFF_BIG(MS,{this.GetQuotedValue(leftSegment)},{this.GetQuotedValue(rightSegment)})<0 THEN 1 ELSE -1 END)", false, false, true);
                    });
                    result = true;
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

                        if (targetSegment.IsConstantValue && rightSegment.IsConstantValue)
                            return targetSegment.Change(Convert.ToDateTime(targetSegment.Value).Add((TimeSpan)rightSegment.Value));

                        targetSegment.Merge(rightSegment);
                        return targetSegment.Change($"DATEADD(MS,{this.GetQuotedValue(rightSegment)},{this.GetQuotedValue(targetSegment)})", false, false, true);
                    });
                    result = true;
                    break;
                case "AddDays":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);

                        if (targetSegment.IsConstantValue && rightSegment.IsConstantValue)
                            return targetSegment.Change(Convert.ToDateTime(targetSegment.Value).AddDays(Convert.ToDouble(rightSegment.Value)));

                        targetSegment.Merge(rightSegment);
                        return targetSegment.Change($"DATEADD(DAY,{this.GetQuotedValue(rightSegment)},{this.GetQuotedValue(targetSegment)})", false, false, true);
                    });
                    result = true;
                    break;
                case "AddHours":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);

                        if (targetSegment.IsConstantValue && rightSegment.IsConstantValue)
                            return targetSegment.Change(Convert.ToDateTime(targetSegment.Value).AddHours(Convert.ToDouble(rightSegment.Value)));

                        targetSegment.Merge(rightSegment);
                        return targetSegment.Change($"DATEADD(HOUR,{this.GetQuotedValue(rightSegment)},{this.GetQuotedValue(targetSegment)})", false, false, true);
                    });
                    result = true;
                    break;
                case "AddMilliseconds":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);

                        if (targetSegment.IsConstantValue && rightSegment.IsConstantValue)
                            return targetSegment.Change(Convert.ToDateTime(targetSegment.Value).AddMilliseconds(Convert.ToDouble(rightSegment.Value)));

                        targetSegment.Merge(rightSegment);
                        return targetSegment.Change($"DATEADD(SECOND,{this.GetQuotedValue(rightSegment)}/1000,{this.GetQuotedValue(targetSegment)})", false, false, true);
                    });
                    result = true;
                    break;
                case "AddMinutes":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);

                        if (targetSegment.IsConstantValue && rightSegment.IsConstantValue)
                            return targetSegment.Change(Convert.ToDateTime(targetSegment.Value).AddMinutes(Convert.ToDouble(rightSegment.Value)));

                        targetSegment.Merge(rightSegment);
                        return targetSegment.Change($"DATEADD(MINUTE,{this.GetQuotedValue(rightSegment)},{this.GetQuotedValue(targetSegment)})", false, false, true);
                    });
                    result = true;
                    break;
                case "AddMonths":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);

                        if (targetSegment.IsConstantValue && rightSegment.IsConstantValue)
                            return targetSegment.Change(Convert.ToDateTime(targetSegment.Value).AddMonths(Convert.ToInt32(rightSegment.Value)));

                        targetSegment.Merge(rightSegment);
                        return targetSegment.Change($"DATEADD(MONTH,{this.GetQuotedValue(rightSegment)},{this.GetQuotedValue(targetSegment)})", false, false, true);
                    });
                    result = true;
                    break;
                case "AddSeconds":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);

                        if (targetSegment.IsConstantValue && rightSegment.IsConstantValue)
                            return targetSegment.Change(Convert.ToDateTime(targetSegment.Value).AddSeconds(Convert.ToDouble(rightSegment.Value)));

                        targetSegment.Merge(rightSegment);
                        return targetSegment.Change($"DATEADD(SECOND,{this.GetQuotedValue(rightSegment)},{this.GetQuotedValue(targetSegment)})", false, false, true);
                    });
                    result = true;
                    break;
                case "AddTicks":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);

                        if (targetSegment.IsConstantValue && rightSegment.IsConstantValue)
                            return targetSegment.Change(Convert.ToDateTime(targetSegment.Value).AddTicks(Convert.ToInt64(rightSegment.Value)));
                        targetSegment.Merge(rightSegment);

                        double milliSeconds = 0;
                        if (rightSegment.IsConstantValue)
                        {
                            milliSeconds = TimeSpan.FromTicks(Convert.ToInt64(rightSegment.Value)).TotalMilliseconds;
                            return targetSegment.Change($"DATEADD(MS,{milliSeconds},{this.GetQuotedValue(targetSegment)})", false, true);
                        }
                        return targetSegment.Change($"DATEADD(MS,{this.GetQuotedValue(targetSegment)}/{TimeSpan.TicksPerMillisecond},{this.GetQuotedValue(targetSegment)})", false, false, true);
                    });
                    result = true;
                    break;
                case "AddYears":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);

                        if (targetSegment.IsConstantValue && rightSegment.IsConstantValue)
                            return targetSegment.Change(Convert.ToDateTime(targetSegment.Value).AddYears(Convert.ToInt32(rightSegment.Value)));

                        targetSegment.Merge(rightSegment);
                        return targetSegment.Change($"DATEADD(YEAR,{rightSegment},{this.GetQuotedValue(targetSegment)})", false, false, true);
                    });
                    result = true;
                    break;
                case "Subtract":
                    if (parameterInfos[0].ParameterType == typeof(DateTime))
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            var rightSegment = visitor.VisitAndDeferred(args[0]);

                            if (targetSegment.IsConstantValue && rightSegment.IsConstantValue)
                                return targetSegment.Change(this.GetQuotedValue(Convert.ToDateTime(targetSegment.Value).Subtract(Convert.ToDateTime(rightSegment.Value))));

                            targetSegment.Merge(rightSegment);
                            return targetSegment.Change($"DATEDIFF(MS,{this.GetQuotedValue(rightSegment)},{this.GetQuotedValue(targetSegment)})", false, false, true);
                        });
                        result = true;
                    }
                    if (parameterInfos[0].ParameterType == typeof(TimeSpan))
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            var rightSegment = visitor.VisitAndDeferred(args[0]);

                            if (targetSegment.IsConstantValue && rightSegment.IsConstantValue)
                                return targetSegment.Change(Convert.ToDateTime(targetSegment.Value).Subtract((TimeSpan)rightSegment.Value));
                            targetSegment.Merge(rightSegment);

                            return targetSegment.Change($"DATEADD(MS,{this.GetQuotedValue(rightSegment)}*-1,{this.GetQuotedValue(targetSegment)})", false, false, true);
                        });
                        result = true;
                    }
                    break;
                case "Equals":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);

                        targetSegment.Merge(rightSegment);
                        return targetSegment.Change($"(CASE WHEN DATEDIFF_BIG(MS,{this.GetQuotedValue(targetSegment)},{this.GetQuotedValue(rightSegment)})=0 THEN 1 ELSE 0 END)", false, false, true);
                    });
                    result = true;
                    break;
                case "CompareTo":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);

                        targetSegment.Merge(rightSegment);
                        return targetSegment.Change($"(CASE WHEN DATEDIFF_BIG(MS,{this.GetQuotedValue(targetSegment)},{this.GetQuotedValue(rightSegment)})=0 THEN 0 WHEN DATEDIFF_BIG(MS,{this.GetQuotedValue(targetSegment)},{this.GetQuotedValue(rightSegment)})<0 THEN 1 ELSE -1 END)", false, false, true);
                    });
                    result = true;
                    break;
                case "ToString":
                    if (parameterInfos.Length == 0)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            if (targetSegment.IsConstantValue)
                                return targetSegment.Change(this.GetQuotedValue(targetSegment));

                            return targetSegment.Change($"CONVERT(VARCHAR,{this.GetQuotedValue(targetSegment)},121)", false, false, true);
                        });
                        result = true;
                    }
                    if (parameterInfos.Length == 1)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            var rightSegment = visitor.VisitAndDeferred(args[0]);

                            if (targetSegment.IsConstantValue && rightSegment.IsConstantValue)
                                return targetSegment.Change(((DateTime)targetSegment.Value).ToString(rightSegment.ToString()));

                            if (rightSegment.IsConstantValue)
                                return targetSegment.Change($"FORMAT({this.GetQuotedValue(targetSegment)},{rightSegment})", false, false, true);

                            throw new NotSupportedException("DateTime类型暂时不支持非常量的格式化字符串");
                        });
                        result = true;
                    }
                    break;
            }
        }
        return result;
    }
}
