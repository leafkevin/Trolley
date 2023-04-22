using System;
using System.Linq.Expressions;

namespace Trolley.SqlServer;

partial class SqlServerProvider
{
    public virtual bool TryGetDateTimeMemberAccessSqlFormatter(MemberExpression memberExpr, out MemberAccessSqlFormatter formatter)
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
                    memberAccessSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target) => target.Change(DateTime.MinValue));
                    result = true;
                    break;
                case "MaxValue":
                    memberAccessSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target) => target.Change(DateTime.MaxValue));
                    result = true;
                    break;
                case "UnixEpoch":
                    memberAccessSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target) => target.Change(DateTime.UnixEpoch));
                    result = true;
                    break;
                case "Today":
                    memberAccessSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target) => target.Change(DateTime.Today));
                    result = true;
                    break;
                case "Now":
                    memberAccessSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target) => target.Change(DateTime.Now));
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
                    memberAccessSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstantValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Date);

                        return targetSegment.Change($"CONVERT(CHAR(10),{this.GetQuotedValue(targetSegment)},120)", false, true);
                    });
                    result = true;
                    break;
                case "Day":
                    memberAccessSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstantValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Day);

                        return targetSegment.Change($"DATEPART(DAY,{this.GetQuotedValue(targetSegment)})", false, true);
                    });
                    result = true;
                    break;
                case "DayOfWeek":
                    memberAccessSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstantValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).DayOfWeek);

                        return targetSegment.Change($"(DATEPART(WEEKDAY,{this.GetQuotedValue(targetSegment)})-1)", false, true);
                    });
                    result = true;
                    break;
                case "DayOfYear":
                    memberAccessSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstantValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).DayOfYear);

                        return targetSegment.Change($"DATEPART(DAYOFYEAR,{this.GetQuotedValue(targetSegment)})", false, true);
                    });
                    result = true;
                    break;
                case "Hour":
                    memberAccessSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstantValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Hour);

                        return targetSegment.Change($"DATEPART(HOUR,{this.GetQuotedValue(targetSegment)})", false, true);
                    });
                    result = true;
                    break;
                case "Kind":
                    memberAccessSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstantValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Kind);
                        throw new NotSupportedException("不支持的成员访问，DateTime只支持常量的Kind成员访问");
                    });
                    result = true;
                    break;
                case "Millisecond":
                    memberAccessSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstantValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Millisecond);

                        return targetSegment.Change($"(DATEPART(MILLISECOND,{this.GetQuotedValue(targetSegment)})/1000)", false, true);
                    });
                    result = true;
                    break;
                case "Minute":
                    memberAccessSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstantValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Minute);

                        return targetSegment.Change($"DATEPART(MINUTE,{this.GetQuotedValue(targetSegment)})", false, true);
                    });
                    result = true;
                    break;
                case "Month":
                    memberAccessSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstantValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Month);

                        return targetSegment.Change($"DATEPART(MONTH,{this.GetQuotedValue(targetSegment)})", false, true);
                    });
                    result = true;
                    break;
                case "Second":
                    memberAccessSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstantValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Second);

                        return targetSegment.Change($"DATEPART(SECOND,{this.GetQuotedValue(targetSegment)})", false, true);
                    });
                    result = true;
                    break;
                case "Ticks":
                    memberAccessSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstantValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Ticks);

                        return targetSegment.Change($"(CAST(DATEDIFF(SECOND,'1970-01-01',{this.GetQuotedValue(targetSegment)}) AS BIGINT)*10000000+621355968000000000)", false, true);
                    });
                    result = true;
                    break;
                case "TimeOfDay":
                    memberAccessSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstantValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).TimeOfDay);

                        return targetSegment.Change($"DATEDIFF(SECOND,CONVERT(CHAR(10),{this.GetQuotedValue(targetSegment)},120),{this.GetQuotedValue(targetSegment)})", false, true);
                    });
                    result = true;
                    break;
                case "Year":
                    memberAccessSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstantValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Year);

                        return targetSegment.Change($"DATEPART(YEAR,{this.GetQuotedValue(targetSegment)})", false, true);
                    });
                    result = true;
                    break;
            }
        }
        return result;
    }
    public virtual bool TryGetDateTimeMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter)
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
                    methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var leftSegment = visitor.VisitAndDeferred(args[0]);
                        var rightSegment = visitor.VisitAndDeferred(args[1]);

                        if (leftSegment.IsConstantValue && rightSegment.IsConstantValue)
                            return leftSegment.Change(DateTime.DaysInMonth(Convert.ToInt32(leftSegment.Value), Convert.ToInt32(rightSegment.Value)));

                        leftSegment.Merge(rightSegment);
                        return leftSegment.Change($"DATEPART(DAY,DATEADD(DAY,-1,DATEADD(MONTH,1,CAST({this.GetQuotedValue(leftSegment)} AS VARCHAR(100))+'-'+CAST({this.GetQuotedValue(rightSegment)} AS VARCHAR(100))+'-1')))", false, true);
                    });
                    result = true;
                    break;
                case "IsLeapYear":
                    methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(args[0]);
                        if (valueSegment.IsConstantValue)
                            return valueSegment.Change(DateTime.IsLeapYear(Convert.ToInt32(valueSegment.Value)));

                        return args[0].Change($"(({valueSegment})%4=0 AND ({valueSegment})%100<>0 OR ({valueSegment})%400=0)", false, true);
                    });
                    result = true;
                    break;
                case "Parse":
                case "TryParse":
                    methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(args[0]);
                        if (valueSegment.IsConstantValue)
                            return valueSegment.Change(DateTime.Parse(valueSegment.ToString()));

                        return valueSegment.Change($"CAST({this.GetQuotedValue(valueSegment)} AS DATETIME)", false, true);
                    });
                    result = true;
                    if (methodInfo.IsStatic && parameterInfos.Length >= 3 && parameterInfos[0].ParameterType == typeof(ReadOnlySpan<char>))
                        throw new NotSupportedException("DateTime.Parse方法暂时不支持ReadOnlySpan<char>类型参数的解析，请转换成String类型");
                    break;
                case "ParseExact":
                case "TryParseExact":
                    methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(args[0]);
                        var formatSegment = visitor.VisitAndDeferred(args[1]);
                        var providerSegment = visitor.VisitAndDeferred(args[2]);

                        if (valueSegment.IsConstantValue && formatSegment.IsConstantValue && providerSegment.IsConstantValue)
                            return valueSegment.Change(DateTime.ParseExact(valueSegment.ToString(), formatSegment.ToString(), (IFormatProvider)providerSegment.Value));

                        valueSegment.Merge(formatSegment);
                        return valueSegment.Change($"CAST({this.GetQuotedValue(valueSegment)} AS DATETIME)", false, true);
                    });
                    result = true;
                    if (methodInfo.IsStatic && parameterInfos.Length >= 1 && parameterInfos[0].ParameterType == typeof(ReadOnlySpan<char>))
                        throw new NotSupportedException($"DateTime.{methodInfo.Name}方法暂时不支持ReadOnlySpan<char>类型参数的解析，请转换成String类型");
                    break;
                case "Compare":
                    methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var leftSegment = visitor.VisitAndDeferred(args[0]);
                        var rightSegment = visitor.VisitAndDeferred(args[1]);

                        leftSegment.Merge(rightSegment);
                        return leftSegment.Change($"(CASE WHEN DATEDIFF_BIG(MS,{this.GetQuotedValue(leftSegment)},{this.GetQuotedValue(rightSegment)})=0 THEN 0 WHEN DATEDIFF_BIG(MS,{this.GetQuotedValue(leftSegment)},{this.GetQuotedValue(rightSegment)})<0 THEN 1 ELSE -1 END)", false, true);
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
                    methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);

                        if (targetSegment.IsConstantValue && rightSegment.IsConstantValue)
                            return targetSegment.Change(Convert.ToDateTime(targetSegment.Value).Add((TimeSpan)rightSegment.Value));

                        targetSegment.Merge(rightSegment);
                        return targetSegment.Change($"DATEADD(MS,{this.GetQuotedValue(rightSegment)},{this.GetQuotedValue(targetSegment)})", false, true);
                    });
                    result = true;
                    break;
                case "AddDays":
                    methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);

                        if (targetSegment.IsConstantValue && rightSegment.IsConstantValue)
                            return targetSegment.Change(Convert.ToDateTime(targetSegment.Value).AddDays(Convert.ToDouble(rightSegment.Value)));

                        targetSegment.Merge(rightSegment);
                        return targetSegment.Change($"DATEADD(DAY,{this.GetQuotedValue(rightSegment)},{this.GetQuotedValue(targetSegment)})", false, true);
                    });
                    result = true;
                    break;
                case "AddHours":
                    methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);

                        if (targetSegment.IsConstantValue && rightSegment.IsConstantValue)
                            return targetSegment.Change(Convert.ToDateTime(targetSegment.Value).AddHours(Convert.ToDouble(rightSegment.Value)));

                        targetSegment.Merge(rightSegment);
                        return targetSegment.Change($"DATEADD(HOUR,{this.GetQuotedValue(rightSegment)},{this.GetQuotedValue(targetSegment)})", false, true);
                    });
                    result = true;
                    break;
                case "AddMilliseconds":
                    methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);

                        if (targetSegment.IsConstantValue && rightSegment.IsConstantValue)
                            return targetSegment.Change(Convert.ToDateTime(targetSegment.Value).AddMilliseconds(Convert.ToDouble(rightSegment.Value)));

                        targetSegment.Merge(rightSegment);
                        return targetSegment.Change($"DATEADD(SECOND,{this.GetQuotedValue(rightSegment)}/1000,{this.GetQuotedValue(targetSegment)})", false, true);
                    });
                    result = true;
                    break;
                case "AddMinutes":
                    methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);

                        if (targetSegment.IsConstantValue && rightSegment.IsConstantValue)
                            return targetSegment.Change(Convert.ToDateTime(targetSegment.Value).AddMinutes(Convert.ToDouble(rightSegment.Value)));

                        targetSegment.Merge(rightSegment);
                        return targetSegment.Change($"DATEADD(MINUTE,{this.GetQuotedValue(rightSegment)},{this.GetQuotedValue(targetSegment)})", false, true);
                    });
                    result = true;
                    break;
                case "AddMonths":
                    methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);

                        if (targetSegment.IsConstantValue && rightSegment.IsConstantValue)
                            return targetSegment.Change(Convert.ToDateTime(targetSegment.Value).AddMonths(Convert.ToInt32(rightSegment.Value)));

                        targetSegment.Merge(rightSegment);
                        return targetSegment.Change($"DATEADD(MONTH,{this.GetQuotedValue(rightSegment)},{this.GetQuotedValue(targetSegment)})", false, true);
                    });
                    result = true;
                    break;
                case "AddSeconds":
                    methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);

                        if (targetSegment.IsConstantValue && rightSegment.IsConstantValue)
                            return targetSegment.Change(Convert.ToDateTime(targetSegment.Value).AddSeconds(Convert.ToDouble(rightSegment.Value)));

                        targetSegment.Merge(rightSegment);
                        return targetSegment.Change($"DATEADD(SECOND,{this.GetQuotedValue(rightSegment)},{this.GetQuotedValue(targetSegment)})", false, true);
                    });
                    result = true;
                    break;
                case "AddTicks":
                    methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
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
                        return targetSegment.Change($"DATEADD(MS,{this.GetQuotedValue(targetSegment)}/{TimeSpan.TicksPerMillisecond},{this.GetQuotedValue(targetSegment)})", false, true);
                    });
                    result = true;
                    break;
                case "AddYears":
                    methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);

                        if (targetSegment.IsConstantValue && rightSegment.IsConstantValue)
                            return targetSegment.Change(Convert.ToDateTime(targetSegment.Value).AddYears(Convert.ToInt32(rightSegment.Value)));

                        targetSegment.Merge(rightSegment);
                        return targetSegment.Change($"DATEADD(YEAR,{rightSegment},{this.GetQuotedValue(targetSegment)})", false, true);
                    });
                    result = true;
                    break;
                case "Subtract":
                    if (parameterInfos[0].ParameterType == typeof(DateTime))
                    {
                        methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            var rightSegment = visitor.VisitAndDeferred(args[0]);

                            if (targetSegment.IsConstantValue && rightSegment.IsConstantValue)
                                return targetSegment.Change(this.GetQuotedValue(Convert.ToDateTime(targetSegment.Value).Subtract(Convert.ToDateTime(rightSegment.Value))));

                            targetSegment.Merge(rightSegment);
                            return targetSegment.Change($"DATEDIFF(MS,{this.GetQuotedValue(rightSegment)},{this.GetQuotedValue(targetSegment)})", false, true);
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
                                return targetSegment.Change(Convert.ToDateTime(targetSegment.Value).Subtract((TimeSpan)rightSegment.Value));
                            targetSegment.Merge(rightSegment);

                            return targetSegment.Change($"DATEADD(MS,{this.GetQuotedValue(rightSegment)}*-1,{this.GetQuotedValue(targetSegment)})", false, true);
                        });
                        result = true;
                    }
                    break;
                case "Equals":
                    methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);

                        targetSegment.Merge(rightSegment);
                        return targetSegment.Change($"(CASE WHEN DATEDIFF_BIG(MS,{this.GetQuotedValue(targetSegment)},{this.GetQuotedValue(rightSegment)})=0 THEN 1 ELSE 0 END)", false, true);
                    });
                    result = true;
                    break;
                case "CompareTo":
                    methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);

                        targetSegment.Merge(rightSegment);
                        return targetSegment.Change($"(CASE WHEN DATEDIFF_BIG(MS,{this.GetQuotedValue(targetSegment)},{this.GetQuotedValue(rightSegment)})=0 THEN 0 WHEN DATEDIFF_BIG(MS,{this.GetQuotedValue(targetSegment)},{this.GetQuotedValue(rightSegment)})<0 THEN 1 ELSE -1 END)", false, true);
                    });
                    result = true;
                    break;
                case "ToString":
                    if (parameterInfos.Length == 0)
                    {
                        methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            if (targetSegment.IsConstantValue)
                                return targetSegment.Change(this.GetQuotedValue(targetSegment));

                            return targetSegment.Change($"CONVERT(VARCHAR,{this.GetQuotedValue(targetSegment)},121)", false, true);
                        });
                        result = true;
                    }
                    if (parameterInfos.Length == 1)
                    {
                        methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            var rightSegment = visitor.VisitAndDeferred(args[0]);

                            if (targetSegment.IsConstantValue && rightSegment.IsConstantValue)
                                return targetSegment.Change(((DateTime)targetSegment.Value).ToString(rightSegment.ToString()));

                            if (rightSegment.IsConstantValue)
                                return targetSegment.Change($"FORMAT({this.GetQuotedValue(targetSegment)},{rightSegment})", false, true);

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
