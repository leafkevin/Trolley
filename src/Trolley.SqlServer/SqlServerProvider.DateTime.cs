using System;
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
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) => target.Change($"{DateTime.MinValue:yyyy-MM-dd HH:mm:ss.fffffff}"));
                    result = true;
                    break;
                case "MaxValue":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) => target.Change($"{DateTime.MaxValue:yyyy-MM-dd HH:mm:ss.fffffff}"));
                    result = true;
                    break;
                case "UnixEpoch":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) => target.Change($"{DateTime.UnixEpoch:yyyy-MM-dd HH:mm:ss.fffffff}"));
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
                        SqlSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((DateTime)targetSegment.Value).Date);

                        var targetArgument = this.GetQuotedValue(targetSegment);
                        return visitor.Change(targetSegment, $"CONVERT(DATE,{targetArgument})", false, true);
                    });
                    result = true;
                    break;
                case "Day":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        SqlSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((DateTime)targetSegment.Value).Day);

                        var targetArgument = this.GetQuotedValue(targetSegment);
                        return visitor.Change(targetSegment, $"DATEPART(DAY,{targetArgument})", false, true);
                    });
                    result = true;
                    break;
                case "DayOfWeek":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        SqlSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((DateTime)targetSegment.Value).DayOfWeek);

                        var targetArgument = this.GetQuotedValue(targetSegment);
                        return visitor.Change(targetSegment, $"DATEPART(WEEKDAY,{targetArgument})-1", true, false);
                    });
                    result = true;
                    break;
                case "DayOfYear":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        SqlSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((DateTime)targetSegment.Value).DayOfYear);

                        var targetArgument = this.GetQuotedValue(targetSegment);
                        return visitor.Change(targetSegment, $"DATEPART(DAYOFYEAR,{targetArgument})", false, true);
                    });
                    result = true;
                    break;
                case "Hour":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        SqlSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((DateTime)targetSegment.Value).Hour);

                        var targetArgument = this.GetQuotedValue(targetSegment);
                        return visitor.Change(targetSegment, $"DATEPART(HOUR,{targetArgument})", false, true);
                    });
                    result = true;
                    break;
                case "Kind":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        SqlSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((DateTime)targetSegment.Value).Kind);

                        throw new NotSupportedException("不支持的成员访问，DateTime只支持常量的Kind成员访问");
                    });
                    result = true;
                    break;
                case "Millisecond":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        SqlSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((DateTime)targetSegment.Value).Millisecond);

                        var targetArgument = this.GetQuotedValue(targetSegment);
                        return visitor.Change(targetSegment, $"DATEPART(MILLISECOND,{targetArgument})", true, false);
                    });
                    result = true;
                    break;
                case "Minute":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        SqlSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((DateTime)targetSegment.Value).Minute);

                        var targetArgument = this.GetQuotedValue(targetSegment);
                        return visitor.Change(targetSegment, $"DATEPART(MINUTE,{targetArgument})", false, true);
                    });
                    result = true;
                    break;
                case "Month":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        SqlSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((DateTime)targetSegment.Value).Month);

                        var targetArgument = this.GetQuotedValue(targetSegment);
                        return visitor.Change(targetSegment, $"DATEPART(MONTH,{targetArgument})", false, true);
                    });
                    result = true;
                    break;
                case "Second":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        SqlSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((DateTime)targetSegment.Value).Second);

                        var targetArgument = this.GetQuotedValue(targetSegment);
                        return visitor.Change(targetSegment, $"DATEPART(SECOND,{targetArgument})", false, true);
                    });
                    result = true;
                    break;
                case "Ticks":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        SqlSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((DateTime)targetSegment.Value).Ticks);

                        var targetArgument = this.GetQuotedValue(targetSegment);
                        return visitor.Change(targetSegment, $"DATEDIFF_BIG(MICROSECOND,'0001-01-01',{targetArgument})*10", true, false);
                    });
                    result = true;
                    break;
                case "TimeOfDay":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        SqlSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((DateTime)targetSegment.Value).TimeOfDay);

                        var targetArgument = this.GetQuotedValue(targetSegment);
                        return visitor.Change(targetSegment, $"CONVERT(TIME,{targetArgument})", false, true);
                    });
                    result = true;
                    break;
                case "Year":
                    memberAccessSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target) =>
                    {
                        SqlSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return visitor.Change(targetSegment, ((DateTime)targetSegment.Value).Year);

                        var targetArgument = this.GetQuotedValue(targetSegment);
                        return visitor.Change(targetSegment, $"DATEPART(YEAR,{targetArgument})", false, true);
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

                        var leftArgument = this.GetQuotedValue(leftSegment);
                        var rightArgument = this.GetQuotedValue(rightSegment);
                        return visitor.Merge(leftSegment, rightSegment, $"DAY(EOMONTH(CAST({leftArgument} AS NVARCHAR(4))+'-'+CAST({rightArgument} AS NVARCHAR(2))+'-01'))", false, true);
                    });
                    result = true;
                    break;
                case "IsLeapYear":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if (valueSegment.IsConstant || valueSegment.IsVariable)
                            return visitor.Change(valueSegment, DateTime.IsLeapYear(Convert.ToInt32(valueSegment.Value)));

                        var valueArgument = this.GetQuotedValue(valueSegment);
                        return visitor.Change(valueSegment, $"CASE WHEN ({valueArgument})%4=0 AND ({valueArgument})%100<>0 OR ({valueArgument})%400=0 THEN 1 ELSE 0 END", true, false);
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

                        var valueArgument = this.GetQuotedValue(valueSegment);
                        return visitor.Change(valueSegment, $"CAST({valueArgument} AS DATETIME)", false, true);
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
                            return visitor.Merge(valueSegment, formatSegment, DateTime.ParseExact(valueSegment.ToString(), formatSegment.ToString(), (IFormatProvider)providerSegment.Value));

                        if (!(formatSegment.IsConstant || formatSegment.IsVariable))
                            throw new NotSupportedException($"方法DateTime.{methodInfo.Name}格式化字符串，暂时不支持非常量、变量的解析");

                        var valueArgument = this.GetQuotedValue(valueSegment);
                        var format = formatSegment.ToString();
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
                        return visitor.Merge(valueSegment, formatSegment, formatValue, false, true);
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
                        visitor.ChangeSameType(leftSegment, rightSegment);
                        var leftArgument = this.GetQuotedValue(leftSegment);
                        var rightArgument = this.GetQuotedValue(rightSegment);
                        return visitor.Merge(leftSegment, rightSegment, $"CASE WHEN {leftArgument}={rightArgument} THEN 0 WHEN {leftArgument}>{rightArgument} THEN 1 ELSE -1 END", true, false);
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
                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return visitor.Merge(targetSegment, rightSegment, Convert.ToDateTime(targetSegment.Value).Add((TimeSpan)rightSegment.Value));

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
                                else builder.Append($"DATEADD(MILLISECOND,{timeSpan.TotalMilliseconds},{targetArgument}");
                                builder.Append(')');
                            }
                            //变量当作常量处理
                            targetSegment.IsVariable = false;
                            return targetSegment.Change(builder.ToString(), false, false, true);
                        }
                        //非常量、变量的，只能小于一天
                        visitor.Change(rightSegment, $"DATEDIFF_BIG(MILLISECOND,'00:00:00',{rightSegment}");
                        var rightArgument = this.GetQuotedValue(rightSegment);
                        return visitor.Merge(targetSegment, rightSegment, $"DATEADD(MILLISECOND,{rightArgument},{targetArgument})", false, true);
                    });
                    result = true;
                    break;
                case "AddDays":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });

                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return visitor.Merge(targetSegment, rightSegment, Convert.ToDateTime(targetSegment.Value).AddDays(Convert.ToDouble(rightSegment.Value)));

                        var targetArgument = this.GetQuotedValue(targetSegment);
                        var rightArgument = this.GetQuotedValue(rightSegment);
                        return visitor.Merge(targetSegment, rightSegment, $"DATEADD(DAY,{rightArgument},{targetArgument})", false, true);
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
                            return visitor.Merge(targetSegment, rightSegment, Convert.ToDateTime(targetSegment.Value).AddHours(Convert.ToDouble(rightSegment.Value)));

                        var targetArgument = this.GetQuotedValue(targetSegment);
                        var rightArgument = this.GetQuotedValue(rightSegment);
                        return visitor.Merge(targetSegment, rightSegment, $"DATEADD(HOUR,{rightArgument},{targetArgument})", false, true);
                    });
                    result = true;
                    break;
                case "AddMilliseconds":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });

                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return visitor.Merge(targetSegment, rightSegment, Convert.ToDateTime(targetSegment.Value).AddMilliseconds(Convert.ToDouble(rightSegment.Value)));

                        var targetArgument = this.GetQuotedValue(targetSegment);
                        var rightArgument = this.GetQuotedValue(rightSegment);
                        return visitor.Merge(targetSegment, rightSegment, $"DATEADD(MILLISECOND,{rightArgument},{targetArgument})", false, true);
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
                            return visitor.Merge(targetSegment, rightSegment, Convert.ToDateTime(targetSegment.Value).AddMinutes(Convert.ToDouble(rightSegment.Value)));

                        var targetArgument = this.GetQuotedValue(targetSegment);
                        var rightArgument = this.GetQuotedValue(rightSegment);
                        return visitor.Merge(targetSegment, rightSegment, $"DATEADD(MINUTE,{rightArgument},{targetArgument})", false, true);
                    });
                    result = true;
                    break;
                case "AddMonths":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });

                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return visitor.Merge(targetSegment, rightSegment, Convert.ToDateTime(targetSegment.Value).AddMonths(Convert.ToInt32(rightSegment.Value)));

                        var targetArgument = this.GetQuotedValue(targetSegment);
                        var rightArgument = this.GetQuotedValue(rightSegment);
                        return visitor.Merge(targetSegment, rightSegment, $"DATEADD(MONTH,{rightArgument},{targetArgument})", false, true);
                    });
                    result = true;
                    break;
                case "AddSeconds":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });

                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return visitor.Merge(targetSegment, rightSegment, Convert.ToDateTime(targetSegment.Value).AddSeconds(Convert.ToDouble(rightSegment.Value)));

                        var targetArgument = this.GetQuotedValue(targetSegment);
                        var rightArgument = this.GetQuotedValue(rightSegment);
                        return visitor.Merge(targetSegment, rightSegment, $"DATEADD(SECOND,{rightArgument},{targetArgument})", false, true);
                    });
                    result = true;
                    break;
                case "AddTicks":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });

                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return visitor.Merge(targetSegment, rightSegment, Convert.ToDateTime(targetSegment.Value).AddTicks(Convert.ToInt64(rightSegment.Value)));

                        var targetArgument = this.GetQuotedValue(targetSegment);
                        var rightArgument = this.GetQuotedValue(rightSegment);
                        return visitor.Merge(targetSegment, rightSegment, $"DATEADD(MILLISECOND,{rightArgument}/{TimeSpan.TicksPerMillisecond},{targetArgument})", false, true);
                    });
                    result = true;
                    break;
                case "AddYears":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });

                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return visitor.Merge(targetSegment, rightSegment, Convert.ToDateTime(targetSegment.Value).AddYears(Convert.ToInt32(rightSegment.Value)));

                        var targetArgument = this.GetQuotedValue(targetSegment);
                        var rightArgument = this.GetQuotedValue(rightSegment);
                        return visitor.Merge(targetSegment, rightSegment, $"DATEADD(YEAR,{rightSegment},{targetArgument})", false, true);
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

                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (rightSegment.IsConstant || rightSegment.IsVariable))
                                return visitor.Merge(targetSegment, rightSegment, Convert.ToDateTime(targetSegment.Value).Subtract(Convert.ToDateTime(rightSegment.Value)));

                            var targetArgument = this.GetQuotedValue(targetSegment);
                            var rightArgument = this.GetQuotedValue(rightSegment);
                            targetSegment.IsVariable = false;
                            return targetSegment.Change($"CAST(DATEDIFF(DAY,'1900-01-01 00:00:00',{targetArgument}-{rightArgument}) AS VARCHAR)+'.'+CONVERT(VARCHAR,{targetArgument}-{rightArgument},108)", false, true, false);
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
                                return visitor.Merge(targetSegment, rightSegment, Convert.ToDateTime(targetSegment.Value).Subtract((TimeSpan)rightSegment.Value));

                            var targetArgument = this.GetQuotedValue(targetSegment);
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
                    }
                    break;
                case "Equals":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        var targetArgument = this.GetQuotedValue(targetSegment);
                        var rightArgument = this.GetQuotedValue(rightSegment);
                        return visitor.Merge(targetSegment, rightSegment, $"CASE WHEN {targetArgument}={rightArgument} THEN 1 ELSE 0 END", true, false);
                    });
                    result = true;
                    break;
                case "CompareTo":
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        var targetArgument = this.GetQuotedValue(targetSegment);
                        var rightArgument = this.GetQuotedValue(rightSegment);
                        return visitor.Merge(targetSegment, rightSegment, $"CASE WHEN {targetArgument}={rightArgument} THEN 0 WHEN {targetArgument}>{rightArgument} THEN 1 ELSE -1 END", true, false);
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
                                return visitor.Change(targetSegment, visitor.Change(targetSegment).ToString());

                            var targetArgument = this.GetQuotedValue(targetSegment);
                            targetSegment.ExpectType = methodInfo.ReturnType;
                            return visitor.Change(targetSegment, $"CONVERT(VARCHAR,{targetArgument},121)", false, true);
                        });
                        result = true;
                    }
                    if (parameterInfos.Length == 1)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                            var formatSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });

                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (formatSegment.IsConstant || formatSegment.IsVariable))
                                return visitor.Merge(targetSegment, formatSegment, ((DateTime)targetSegment.Value).ToString(formatSegment.ToString()));

                            var targetArgument = this.GetQuotedValue(targetSegment);
                            var formatArgument = this.GetQuotedValue(formatSegment);
                            if (formatSegment.IsConstant || formatSegment.IsVariable)
                                return visitor.Merge(targetSegment, formatSegment, $"FORMAT({targetArgument},{formatArgument})", false, true);

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
