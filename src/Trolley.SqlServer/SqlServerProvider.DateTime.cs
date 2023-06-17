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
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) => target.Change("CONVERT(DATE,GETDATE())", false, false, true));
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
        }
        else
        {
            switch (memberInfo.Name)
            {
                case "Date":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((DateTime)targetSegment.Value).Date);

                        return visitor.Change(targetSegment, $"CONVERT(DATE,{this.GetQuotedValue(targetSegment)})", false, true);
                    });
                    result = true;
                    break;
                case "Day":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((DateTime)targetSegment.Value).Day);

                        return visitor.Change(targetSegment, $"DATEPART(DAY,{this.GetQuotedValue(targetSegment)})", false, true);
                    });
                    result = true;
                    break;
                case "DayOfWeek":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((DateTime)targetSegment.Value).DayOfWeek);

                        return visitor.Change(targetSegment, $"DATEPART(WEEKDAY,{this.GetQuotedValue(targetSegment)})-1", true, false);
                    });
                    result = true;
                    break;
                case "DayOfYear":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((DateTime)targetSegment.Value).DayOfYear);

                        return visitor.Change(targetSegment, $"DATEPART(DAYOFYEAR,{this.GetQuotedValue(targetSegment)})", false, true);
                    });
                    result = true;
                    break;
                case "Hour":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((DateTime)targetSegment.Value).Hour);

                        return visitor.Change(targetSegment, $"DATEPART(HOUR,{this.GetQuotedValue(targetSegment)})", false, true);
                    });
                    result = true;
                    break;
                case "Kind":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((DateTime)targetSegment.Value).Kind);
                        throw new NotSupportedException("不支持的成员访问，DateTime只支持常量的Kind成员访问");
                    });
                    result = true;
                    break;
                case "Millisecond":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((DateTime)targetSegment.Value).Millisecond);

                        return visitor.Change(targetSegment, $"DATEPART(MILLISECOND,{this.GetQuotedValue(targetSegment)})", true, false);
                    });
                    result = true;
                    break;
                case "Minute":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((DateTime)targetSegment.Value).Minute);

                        return visitor.Change(targetSegment, $"DATEPART(MINUTE,{this.GetQuotedValue(targetSegment)})", false, true);
                    });
                    result = true;
                    break;
                case "Month":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((DateTime)targetSegment.Value).Month);

                        return visitor.Change(targetSegment, $"DATEPART(MONTH,{this.GetQuotedValue(targetSegment)})", false, true);
                    });
                    result = true;
                    break;
                case "Second":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((DateTime)targetSegment.Value).Second);

                        return visitor.Change(targetSegment, $"DATEPART(SECOND,{this.GetQuotedValue(targetSegment)})", false, true);
                    });
                    result = true;
                    break;
                case "Ticks":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((DateTime)targetSegment.Value).Ticks);

                        return visitor.Change(targetSegment, $"DATEDIFF_BIG(MICROSECOND,'0001-01-01',{this.GetQuotedValue(targetSegment)})*10", true, false);
                    });
                    result = true;
                    break;
                case "TimeOfDay":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((DateTime)targetSegment.Value).TimeOfDay);

                        return visitor.Change(targetSegment, $"CONVERT(TIME,{this.GetQuotedValue(targetSegment)})", false, true);
                    });
                    result = true;
                    break;
                case "Year":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((DateTime)targetSegment.Value).Year);

                        return visitor.Change(targetSegment, $"DATEPART(YEAR,{this.GetQuotedValue(targetSegment)})", false, true);
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
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var leftSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[1] });
                        if ((leftSegment.IsConstant || leftSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return visitor.Merge(leftSegment, rightSegment, DateTime.DaysInMonth(Convert.ToInt32(leftSegment.Value), Convert.ToInt32(rightSegment.Value)));

                        var leftArgument = this.GetQuotedValue(visitor.Change(leftSegment));
                        var rightArgument = this.GetQuotedValue(visitor.Change(rightSegment));
                        return visitor.Merge(leftSegment, rightSegment, $"DAY(EOMONTH('{this.GetQuotedValue(leftSegment)}-{this.GetQuotedValue(rightSegment).PadLeft(2, '0')}-01')", false, true);
                    });
                    result = true;
                    break;
                case "IsLeapYear":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if (valueSegment.IsConstant || valueSegment.IsVariable)
                            return visitor.Change(valueSegment, DateTime.IsLeapYear(Convert.ToInt32(valueSegment.Value)));

                        return visitor.Change(valueSegment, $"(({valueSegment})%4=0 AND ({valueSegment})%100<>0 OR ({valueSegment})%400=0)", false, true);
                    });
                    result = true;
                    break;
                case "Parse":
                case "TryParse":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if (valueSegment.IsConstant || valueSegment.IsVariable)
                            return visitor.Change(valueSegment, DateTime.Parse(valueSegment.ToString()));

                        return visitor.Change(valueSegment, $"CAST({this.GetQuotedValue(valueSegment)} AS DATETIME)", false, true);
                    });
                    result = true;
                    if (methodInfo.IsStatic && parameterInfos.Length >= 3 && parameterInfos[0].ParameterType == typeof(ReadOnlySpan<char>))
                        throw new NotSupportedException("DateTime.Parse方法暂时不支持ReadOnlySpan<char>类型参数的解析，请转换成String类型");
                    break;
                case "ParseExact":
                case "TryParseExact":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        var formatSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[1] });
                        var providerSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[2] });

                        if ((valueSegment.IsConstant || valueSegment.IsVariable)
                            && (formatSegment.IsConstant || formatSegment.IsVariable)
                            && (providerSegment.IsConstant || providerSegment.IsVariable))
                            return visitor.Change(valueSegment, DateTime.ParseExact(valueSegment.ToString(), formatSegment.ToString(), (IFormatProvider)providerSegment.Value));

                        valueSegment.Merge(formatSegment);
                        return visitor.Change(valueSegment, $"CAST({this.GetQuotedValue(valueSegment)} AS DATETIME)", false, true);
                    });
                    result = true;
                    if (methodInfo.IsStatic && parameterInfos.Length >= 1 && parameterInfos[0].ParameterType == typeof(ReadOnlySpan<char>))
                        throw new NotSupportedException($"DateTime.{methodInfo.Name}方法暂时不支持ReadOnlySpan<char>类型参数的解析，请转换成String类型");
                    break;
                case "Compare":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var leftSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[1] });

                        leftSegment.Merge(rightSegment);
                        return visitor.Merge(leftSegment, rightSegment, $"(CASE WHEN DATEDIFF_BIG(MS,{this.GetQuotedValue(leftSegment)},{this.GetQuotedValue(rightSegment)})=0 THEN 0 WHEN DATEDIFF_BIG(MS,{this.GetQuotedValue(leftSegment)},{this.GetQuotedValue(rightSegment)})<0 THEN 1 ELSE -1 END)", false, true);
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
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });

                        if (targetSegment.IsConstant && rightSegment.IsConstant)
                            return visitor.Change(targetSegment, Convert.ToDateTime(targetSegment.Value).Add((TimeSpan)rightSegment.Value));

                        targetSegment.Merge(rightSegment);
                        return visitor.Change(targetSegment, $"DATEADD(MS,{this.GetQuotedValue(rightSegment)},{this.GetQuotedValue(targetSegment)})", false, true);
                    });
                    result = true;
                    break;
                case "AddDays":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });

                        if (targetSegment.IsConstant && rightSegment.IsConstant)
                            return visitor.Change(targetSegment, Convert.ToDateTime(targetSegment.Value).AddDays(Convert.ToDouble(rightSegment.Value)));

                        targetSegment.Merge(rightSegment);
                        return visitor.Change(targetSegment, $"DATEADD(DAY,{this.GetQuotedValue(rightSegment)},{this.GetQuotedValue(targetSegment)})", false, true);
                    });
                    result = true;
                    break;
                case "AddHours":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });

                        if (targetSegment.IsConstant && rightSegment.IsConstant)
                            return visitor.Change(targetSegment, Convert.ToDateTime(targetSegment.Value).AddHours(Convert.ToDouble(rightSegment.Value)));

                        targetSegment.Merge(rightSegment);
                        return visitor.Change(targetSegment, $"DATEADD(HOUR,{this.GetQuotedValue(rightSegment)},{this.GetQuotedValue(targetSegment)})", false, true);
                    });
                    result = true;
                    break;
                case "AddMilliseconds":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });

                        if (targetSegment.IsConstant && rightSegment.IsConstant)
                            return visitor.Change(targetSegment, Convert.ToDateTime(targetSegment.Value).AddMilliseconds(Convert.ToDouble(rightSegment.Value)));

                        targetSegment.Merge(rightSegment);
                        return visitor.Change(targetSegment, $"DATEADD(SECOND,{this.GetQuotedValue(rightSegment)}/1000,{this.GetQuotedValue(targetSegment)})", false, true);
                    });
                    result = true;
                    break;
                case "AddMinutes":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });

                        if (targetSegment.IsConstant && rightSegment.IsConstant)
                            return visitor.Change(targetSegment, Convert.ToDateTime(targetSegment.Value).AddMinutes(Convert.ToDouble(rightSegment.Value)));

                        targetSegment.Merge(rightSegment);
                        return visitor.Change(targetSegment, $"DATEADD(MINUTE,{this.GetQuotedValue(rightSegment)},{this.GetQuotedValue(targetSegment)})", false, true);
                    });
                    result = true;
                    break;
                case "AddMonths":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });

                        if (targetSegment.IsConstant && rightSegment.IsConstant)
                            return visitor.Change(targetSegment, Convert.ToDateTime(targetSegment.Value).AddMonths(Convert.ToInt32(rightSegment.Value)));

                        targetSegment.Merge(rightSegment);
                        return visitor.Change(targetSegment, $"DATEADD(MONTH,{this.GetQuotedValue(rightSegment)},{this.GetQuotedValue(targetSegment)})", false, true);
                    });
                    result = true;
                    break;
                case "AddSeconds":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });

                        if (targetSegment.IsConstant && rightSegment.IsConstant)
                            return visitor.Change(targetSegment, Convert.ToDateTime(targetSegment.Value).AddSeconds(Convert.ToDouble(rightSegment.Value)));

                        targetSegment.Merge(rightSegment);
                        return visitor.Change(targetSegment, $"DATEADD(SECOND,{this.GetQuotedValue(rightSegment)},{this.GetQuotedValue(targetSegment)})", false, true);
                    });
                    result = true;
                    break;
                case "AddTicks":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });

                        if (targetSegment.IsConstant && rightSegment.IsConstant)
                            return visitor.Change(targetSegment, Convert.ToDateTime(targetSegment.Value).AddTicks(Convert.ToInt64(rightSegment.Value)));
                        targetSegment.Merge(rightSegment);

                        double milliSeconds = 0;
                        if (rightSegment.IsConstant)
                        {
                            milliSeconds = TimeSpan.FromTicks(Convert.ToInt64(rightSegment.Value)).TotalMilliseconds;
                            return visitor.Change(targetSegment, $"DATEADD(MS,{milliSeconds},{this.GetQuotedValue(targetSegment)})", false, true);
                        }
                        return visitor.Change(targetSegment, $"DATEADD(MS,{this.GetQuotedValue(targetSegment)}/{TimeSpan.TicksPerMillisecond},{this.GetQuotedValue(targetSegment)})", false, true);
                    });
                    result = true;
                    break;
                case "AddYears":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });

                        if (targetSegment.IsConstant && rightSegment.IsConstant)
                            return visitor.Change(targetSegment, Convert.ToDateTime(targetSegment.Value).AddYears(Convert.ToInt32(rightSegment.Value)));

                        targetSegment.Merge(rightSegment);
                        return visitor.Change(targetSegment, $"DATEADD(YEAR,{rightSegment},{this.GetQuotedValue(targetSegment)})", false, true);
                    });
                    result = true;
                    break;
                case "Subtract":
                    if (parameterInfos[0].ParameterType == typeof(DateTime))
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                            var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });

                            if (targetSegment.IsConstant && rightSegment.IsConstant)
                                return visitor.Change(targetSegment, this.GetQuotedValue(Convert.ToDateTime(targetSegment.Value).Subtract(Convert.ToDateTime(rightSegment.Value))));

                            targetSegment.Merge(rightSegment);
                            return visitor.Change(targetSegment, $"DATEDIFF(MS,{this.GetQuotedValue(rightSegment)},{this.GetQuotedValue(targetSegment)})", false, true);
                        });
                        result = true;
                    }
                    if (parameterInfos[0].ParameterType == typeof(TimeSpan))
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                            var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });

                            if (targetSegment.IsConstant && rightSegment.IsConstant)
                                return visitor.Change(targetSegment, Convert.ToDateTime(targetSegment.Value).Subtract((TimeSpan)rightSegment.Value));
                            targetSegment.Merge(rightSegment);

                            return visitor.Change(targetSegment, $"DATEADD(MS,{this.GetQuotedValue(rightSegment)}*-1,{this.GetQuotedValue(targetSegment)})", false, true);
                        });
                        result = true;
                    }
                    break;
                case "Equals":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });

                        targetSegment.Merge(rightSegment);
                        return visitor.Change(targetSegment, $"(CASE WHEN DATEDIFF_BIG(MS,{this.GetQuotedValue(targetSegment)},{this.GetQuotedValue(rightSegment)})=0 THEN 1 ELSE 0 END)", false, true);
                    });
                    result = true;
                    break;
                case "CompareTo":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });

                        targetSegment.Merge(rightSegment);
                        return visitor.Change(targetSegment, $"(CASE WHEN DATEDIFF_BIG(MS,{this.GetQuotedValue(targetSegment)},{this.GetQuotedValue(rightSegment)})=0 THEN 0 WHEN DATEDIFF_BIG(MS,{this.GetQuotedValue(targetSegment)},{this.GetQuotedValue(rightSegment)})<0 THEN 1 ELSE -1 END)", false, true);
                    });
                    result = true;
                    break;
                case "ToString":
                    if (parameterInfos.Length == 0)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                            if (targetSegment.IsConstant || targetSegment.IsVariable)
                                return visitor.Change(targetSegment, this.GetQuotedValue(targetSegment));

                            return visitor.Change(targetSegment, $"CONVERT(VARCHAR,{this.GetQuotedValue(targetSegment)},121)", false, true);
                        });
                        result = true;
                    }
                    if (parameterInfos.Length == 1)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                            var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });

                            if (targetSegment.IsConstant && rightSegment.IsConstant)
                                return visitor.Change(targetSegment, ((DateTime)targetSegment.Value).ToString(rightSegment.ToString()));

                            if (rightSegment.IsConstant)
                                return visitor.Change(targetSegment, $"FORMAT({this.GetQuotedValue(targetSegment)},{this.GetQuotedValue(rightSegment)})", false, true);

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
