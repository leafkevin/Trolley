using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;

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
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) => target.ChangeValue(DateTime.MinValue, true));
                    result = true;
                    break;
                case "MaxValue":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) => target.ChangeValue(DateTime.MaxValue, true));
                    result = true;
                    break;
                case "UnixEpoch":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) => target.ChangeValue(DateTime.UnixEpoch, true));
                    result = true;
                    break;
                case "Today":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) => target.Change("CONVERT(DATE,GETDATE())", false, true));
                    result = true;
                    break;
                case "Now":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) => target.Change("GETDATE()", false, true));
                    result = true;
                    break;
                case "UtcNow":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) => target.Change("GETUTCDATE()", false, true));
                    result = true;
                    break;
            }
        }
        else
        {
            switch (memberInfo.Name)
            {
                case "Date":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlFieldSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((DateTime)targetSegment.Value).Date);

                        return targetSegment.Change($"CONVERT(DATE,{targetSegment.Body})", false, true);
                    });
                    result = true;
                    break;
                case "Day":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlFieldSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((DateTime)targetSegment.Value).Day);

                        return targetSegment.Change($"DATEPART(DAY,{targetSegment.Body})", false, true);
                    });
                    result = true;
                    break;
                case "DayOfWeek":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlFieldSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((DateTime)targetSegment.Value).DayOfWeek);

                        return targetSegment.Change($"DATEPART(WEEKDAY,{targetSegment.Body})-1");
                    });
                    result = true;
                    break;
                case "DayOfYear":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlFieldSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((DateTime)targetSegment.Value).DayOfYear);

                        return targetSegment.Change($"DATEPART(DAYOFYEAR,{targetSegment.Body})", false, true);
                    });
                    result = true;
                    break;
                case "Hour":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlFieldSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((DateTime)targetSegment.Value).Hour);

                        return targetSegment.Change($"DATEPART(HOUR,{targetSegment.Body})", false, true);
                    });
                    result = true;
                    break;
                case "Kind":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlFieldSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((DateTime)targetSegment.Value).Kind);

                        throw new NotSupportedException("不支持的成员访问，DateTime只支持常量的Kind成员访问");
                    });
                    result = true;
                    break;
                case "Millisecond":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlFieldSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((DateTime)targetSegment.Value).Millisecond);

                        return targetSegment.Change($"DATEPART(MILLISECOND,{targetSegment.Body})", false, true);
                    });
                    result = true;
                    break;
                case "Minute":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlFieldSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((DateTime)targetSegment.Value).Minute);

                        return targetSegment.Change($"DATEPART(MINUTE,{targetSegment.Body})", false, true);
                    });
                    result = true;
                    break;
                case "Month":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlFieldSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((DateTime)targetSegment.Value).Month);

                        return targetSegment.Change($"DATEPART(MONTH,{targetSegment.Body})", false, true);
                    });
                    result = true;
                    break;
                case "Second":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlFieldSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((DateTime)targetSegment.Value).Second);

                        return targetSegment.Change($"DATEPART(SECOND,{targetSegment.Body})", false, true);
                    });
                    result = true;
                    break;
                case "Ticks":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlFieldSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((DateTime)targetSegment.Value).Ticks);

                        return targetSegment.Change($"DATEDIFF_BIG(MICROSECOND,'0001-01-01',{targetSegment.Body})*10");
                    });
                    result = true;
                    break;
                case "TimeOfDay":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlFieldSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((DateTime)targetSegment.Value).TimeOfDay);

                        return targetSegment.Change($"CONVERT(TIME,{targetSegment.Body})", false, true);
                    });
                    result = true;
                    break;
                case "Year":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlFieldSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.ChangeValue(((DateTime)targetSegment.Value).Year);

                        return targetSegment.Change($"DATEPART(YEAR,{targetSegment.Body})", false, true);
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
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var leftSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        var rightSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[1] });
                        if ((leftSegment.IsConstant || leftSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return leftSegment.MergeValue(rightSegment, DateTime.DaysInMonth(Convert.ToInt32(leftSegment.Value), Convert.ToInt32(rightSegment.Value)));

                        var leftArgument = visitor.GetQuotedValue(leftSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return leftSegment.Merge(rightSegment, $"DAY(EOMONTH(CAST({leftArgument} AS NVARCHAR(4))+'-'+CAST({rightArgument} AS NVARCHAR(2))+'-01'))", false, true);
                    });
                    result = true;
                    break;
                case "IsLeapYear":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        if (valueSegment.IsConstant || valueSegment.IsVariable)
                            return valueSegment.ChangeValue(DateTime.IsLeapYear(Convert.ToInt32(valueSegment.Value)));

                        var valueArgument = valueSegment.ToExprWrap();
                        if (visitor.IsSelect)
                        {
                            valueArgument = $"CASE WHEN {valueArgument}%4=0 AND {valueArgument}%100<>0 OR {valueArgument}%400=0 THEN 1 ELSE 0 END";
                            return valueSegment.Change(valueArgument);
                        }
                        valueArgument = $"({valueArgument}%4=0 AND {valueArgument}%100<>0 OR {valueArgument}%400=0)";
                        return valueSegment.Change(valueArgument, false, true);
                    });
                    result = true;
                    break;
                case "Parse":
                case "TryParse":
                    if (parameterInfos.Length == 3)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var valueSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                            var providerSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[1] });
                            var styleSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[2] });

                            if ((valueSegment.IsConstant || valueSegment.IsVariable)
                                && (providerSegment.IsConstant || providerSegment.IsVariable)
                                && (styleSegment.IsConstant || styleSegment.IsVariable))
                                return valueSegment.ChangeValue(DateTime.Parse(valueSegment.Value.ToString(), (IFormatProvider)providerSegment.Value, (DateTimeStyles)styleSegment.Value));

                            return valueSegment.Change($"CAST({valueSegment.Body} AS DATETIME)", false, true);
                        });
                    }
                    else if (parameterInfos.Length == 2)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var valueSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                            var providerSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[1] });

                            if ((valueSegment.IsConstant || valueSegment.IsVariable)
                                && (providerSegment.IsConstant || providerSegment.IsVariable))
                                return valueSegment.ChangeValue(DateTime.Parse(valueSegment.Value.ToString(), (IFormatProvider)providerSegment.Value));

                            return valueSegment.Change($"CAST({valueSegment.Body} AS DATETIME)", false, true);
                        });
                    }
                    else
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var valueSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                            if (valueSegment.IsConstant || valueSegment.IsVariable)
                                return valueSegment.ChangeValue(DateTime.Parse(valueSegment.Value.ToString()));

                            return valueSegment.Change($"CAST({valueSegment.Body} AS DATETIME)", false, true);
                        });
                    }
                    result = true;
                    if (methodInfo.IsStatic && parameterInfos.Length >= 3 && parameterInfos[0].ParameterType == typeof(ReadOnlySpan<char>))
                        throw new NotSupportedException("DateTime.Parse方法暂时不支持ReadOnlySpan<char>类型参数的解析，请转换成String类型");
                    break;
                case "ParseExact":
                case "TryParseExact":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        var formatSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[1] });
                        var providerSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[2] });

                        if ((valueSegment.IsConstant || valueSegment.IsVariable)
                            && (formatSegment.IsConstant || formatSegment.IsVariable)
                            && (providerSegment.IsConstant || providerSegment.IsVariable))
                            return valueSegment.MergeValue(formatSegment, DateTime.ParseExact(valueSegment.Value.ToString(), formatSegment.Value.ToString(), (IFormatProvider)providerSegment.Value));

                        if (!(formatSegment.IsConstant || formatSegment.IsVariable))
                            throw new NotSupportedException($"方法DateTime.{methodInfo.Name}格式化字符串，暂时不支持非常量、变量的解析");

                        var valueArgument = visitor.GetQuotedValue(valueSegment);
                        var format = formatSegment.Value.ToString();
                        string formatValue = null;
                        switch (format)
                        {
                            case "mm/dd/yyyy": formatValue = $"CONVERT(DATETIME,{valueArgument},101)"; break;
                            case "yyyy.mm.dd": formatValue = $"CONVERT(DATETIME,{valueArgument},102)"; break;
                            case "dd/mm/yyyy": formatValue = $"CONVERT(DATETIME,{valueArgument},103)"; break;
                            case "dd.mm.yyyy": formatValue = $"CONVERT(DATETIME,{valueArgument},104)"; break;
                            case "dd-mm-yyyy": formatValue = $"CONVERT(DATETIME,{valueArgument},105)"; break;
                            case "dd mon yyyy": formatValue = $"CONVERT(DATETIME,{valueArgument},106)"; break;
                            case "mon dd, yyyy": formatValue = $"CONVERT(DATETIME,{valueArgument},107)"; break;
                            case "hh:mi:ss": formatValue = $"CONVERT(DATETIME,{valueArgument},108)"; break;
                            case "mon dd yyyy hh:mi:ss:mmmAM":
                            case "mon dd yyyy hh:mi:ss:mmmPM": formatValue = $"CONVERT(DATETIME,{valueArgument},109)"; break;
                            case "mm-dd-yyyy": formatValue = $"CONVERT(DATETIME,{valueArgument},110)"; break;
                            case "yyyy/mm/dd": formatValue = $"CONVERT(DATETIME,{valueArgument},111)"; break;
                            case "yyyymmdd": formatValue = $"CONVERT(DATETIME,{valueArgument},112)"; break;
                            case "yyyy-mm-dd hh:mi:ss": formatValue = $"CONVERT(DATETIME,{valueArgument},120)"; break;
                            case "yyyy-mm-dd hh:mi:ss.mmm": formatValue = $"CONVERT(DATETIME,{valueArgument},121)"; break;
                            case "yyyy-mm-ddThh:mi:ss.mmm": formatValue = $"CONVERT(DATETIME,{valueArgument},126)"; break;
                            case "dd mon yyyy hh:mi:ss:mmmAM":
                            case "dd mon yyyy hh:mi:ss:mmmPM": formatValue = $"CONVERT(DATETIME,{valueArgument},130)"; break;
                            case "dd/mm/yyyy hh:mi:ss:mmmAM":
                            case "dd/mm/yyyy hh:mi:ss:mmmPM": formatValue = $"CONVERT(DATETIME,{valueArgument},131)"; break;
                            default: formatValue = $"CAST({valueArgument} AS DATETIME)"; break;
                        }
                        return valueSegment.Merge(formatSegment, formatValue, false, true);
                    });
                    result = true;
                    if (methodInfo.IsStatic && parameterInfos.Length >= 1 && parameterInfos[0].ParameterType == typeof(ReadOnlySpan<char>))
                        throw new NotSupportedException($"DateTime.{methodInfo.Name}方法暂时不支持ReadOnlySpan<char>类型参数的解析，请转换成String类型");
                    break;
                case "Compare":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var leftSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        var rightSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[1] });

                        var leftArgument = visitor.GetQuotedValue(leftSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return leftSegment.Merge(rightSegment, $"CASE WHEN {leftArgument}={rightArgument} THEN 0 WHEN {leftArgument}>{rightArgument} THEN 1 ELSE -1 END");
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
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return targetSegment.MergeValue(rightSegment, Convert.ToDateTime(targetSegment.Value).Add((TimeSpan)rightSegment.Value));

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
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
                                else builder.Append($"DATEADD(MILLISECOND,{timeSpan.TotalMilliseconds},{targetArgument}");
                                builder.Append(')');
                            }
                            return targetSegment.Change(builder.ToString(), false, true);
                        }
                        //非常量、变量的，只能小于一天,数据库的Time类型映射成TimeSpan
                        var rightArgument = $"DATEDIFF_BIG(MILLISECOND,'00:00:00',{rightSegment.Body})";
                        return targetSegment.Merge(rightSegment, $"DATEADD(MILLISECOND,{rightArgument},{targetArgument})", false, true);
                    });
                    result = true;
                    break;
                case "AddDays":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });

                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return targetSegment.MergeValue(rightSegment, Convert.ToDateTime(targetSegment.Value).AddDays(Convert.ToDouble(rightSegment.Value)));

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(rightSegment, $"DATEADD(DAY,{rightArgument},{targetArgument})", false, true);
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
                            return targetSegment.MergeValue(rightSegment, Convert.ToDateTime(targetSegment.Value).AddHours(Convert.ToDouble(rightSegment.Value)));

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(rightSegment, $"DATEADD(HOUR,{rightArgument},{targetArgument})", false, true);
                    });
                    result = true;
                    break;
                case "AddMilliseconds":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return targetSegment.MergeValue(rightSegment, Convert.ToDateTime(targetSegment.Value).AddMilliseconds(Convert.ToDouble(rightSegment.Value)));

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(rightSegment, $"DATEADD(MILLISECOND,{rightArgument},{targetArgument})", false, true);
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
                            return targetSegment.MergeValue(rightSegment, Convert.ToDateTime(targetSegment.Value).AddMinutes(Convert.ToDouble(rightSegment.Value)));

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(rightSegment, $"DATEADD(MINUTE,{rightArgument},{targetArgument})", false, true);
                    });
                    result = true;
                    break;
                case "AddMonths":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return targetSegment.MergeValue(rightSegment, Convert.ToDateTime(targetSegment.Value).AddMonths(Convert.ToInt32(rightSegment.Value)));

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(rightSegment, $"DATEADD(MONTH,{rightArgument},{targetArgument})", false, true);
                    });
                    result = true;
                    break;
                case "AddSeconds":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return targetSegment.MergeValue(rightSegment, Convert.ToDateTime(targetSegment.Value).AddSeconds(Convert.ToDouble(rightSegment.Value)));

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(rightSegment, $"DATEADD(SECOND,{rightArgument},{targetArgument})", false, true);
                    });
                    result = true;
                    break;
                case "AddTicks":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return targetSegment.MergeValue(rightSegment, Convert.ToDateTime(targetSegment.Value).AddTicks(Convert.ToInt64(rightSegment.Value)));

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment, true);
                        return targetSegment.Merge(rightSegment, $"DATEADD(MILLISECOND,{rightArgument}/{TimeSpan.TicksPerMillisecond},{targetArgument})", false, true);
                    });
                    result = true;
                    break;
                case "AddYears":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return targetSegment.MergeValue(rightSegment, Convert.ToDateTime(targetSegment.Value).AddYears(Convert.ToInt32(rightSegment.Value)));

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(rightSegment, $"DATEADD(YEAR,{rightSegment},{targetArgument})", false, true);
                    });
                    result = true;
                    break;
                case "Subtract":
                    if (parameterInfos[0].ParameterType == typeof(DateTime))
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                            var rightSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (rightSegment.IsConstant || rightSegment.IsVariable))
                                return targetSegment.MergeValue(rightSegment, Convert.ToDateTime(targetSegment.Value).Subtract(Convert.ToDateTime(rightSegment.Value)));

                            var targetArgument = visitor.GetQuotedValue(targetSegment);
                            var rightArgument = visitor.GetQuotedValue(rightSegment);
                            return targetSegment.Change($"CASE WHEN DATEDIFF(SECOND,{rightArgument},{targetArgument})>0 THEN CAST(DATEDIFF(DAY,{rightArgument},{targetArgument}) AS VARCHAR)+'.'+CAST(CONVERT(TIME,DATEADD(SECOND,DATEDIFF(SECOND,{rightArgument},{targetArgument})%86400,'00:00:00')) AS VARCHAR) ELSE CAST(DATEDIFF(DAY,{rightArgument},{targetArgument})+1 AS VARCHAR)+'.'+CAST(CONVERT(TIME,DATEADD(SECOND,86400-DATEDIFF(SECOND,{rightArgument},{targetArgument})%86400,'00:00:00')) AS VARCHAR) END");
                        });
                        result = true;
                    }
                    if (parameterInfos[0].ParameterType == typeof(TimeSpan))
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                            var rightSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (rightSegment.IsConstant || rightSegment.IsVariable))
                                return targetSegment.MergeValue(rightSegment, Convert.ToDateTime(targetSegment.Value).Subtract((TimeSpan)rightSegment.Value));

                            var targetArgument = visitor.GetQuotedValue(targetSegment);
                            if (rightSegment.IsConstant || rightSegment.IsVariable)
                            {
                                var builder = new StringBuilder();
                                var timeSpan = (TimeSpan)rightSegment.Value;
                                if (timeSpan.Days > 0)
                                {
                                    builder.Append($"DATEADD(DAY,-{timeSpan.Days},{targetArgument})");
                                    timeSpan = timeSpan.Subtract(TimeSpan.FromDays(timeSpan.Days));
                                }
                                if (timeSpan.Ticks > 0)
                                {
                                    if (builder.Length > 0) builder.Insert(0, $"DATEADD(MILLISECOND,-{timeSpan.TotalMilliseconds},");
                                    else builder.Append($"DATEADD(MILLISECOND,-{timeSpan.TotalMilliseconds},{targetArgument}");
                                    builder.Append(')');
                                }
                                return targetSegment.Change(builder.ToString());
                            }
                            //非常量、变量的，只能小于一天
                            var rightArgument = $"DATEDIFF_BIG(MILLISECOND,'00:00:00',{rightSegment.Body})";
                            return targetSegment.Merge(rightSegment, $"DATEADD(MILLISECOND,-{rightArgument},{targetArgument})", false, true);
                        });
                        result = true;
                    }
                    break;
                case "Equals":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(rightSegment, $"CASE WHEN {targetArgument}={rightArgument} THEN 1 ELSE 0 END");
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
                        return targetSegment.Merge(rightSegment, $"CASE WHEN {targetArgument}={rightArgument} THEN 0 WHEN {targetArgument}>{rightArgument} THEN 1 ELSE -1 END");
                    });
                    result = true;
                    break;
                case "ToString":
                    if (parameterInfos.Length == 0)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                            if (targetSegment.IsConstant || targetSegment.IsVariable)
                                return targetSegment.ChangeValue(targetSegment.Value.ToString());

                            return targetSegment.Change($"CONVERT(VARCHAR,{targetSegment.Body},121)", false, true);
                        });
                        result = true;
                    }
                    if (parameterInfos.Length == 1)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                            var formatSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });

                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (formatSegment.IsConstant || formatSegment.IsVariable))
                                return targetSegment.MergeValue(formatSegment, ((DateTime)targetSegment.Value).ToString(formatSegment.Value.ToString()));

                            var targetArgument = visitor.GetQuotedValue(targetSegment);
                            var formatArgument = visitor.GetQuotedValue(formatSegment);
                            if (formatSegment.IsConstant || formatSegment.IsVariable)
                                return targetSegment.Merge(formatSegment, $"FORMAT({targetArgument},{formatArgument})", false, true);

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
