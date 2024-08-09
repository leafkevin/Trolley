using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.MySqlConnector;

partial class MySqlProvider
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
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) => target.Change("CURDATE()", false, true));
                    result = true;
                    break;
                case "Now":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) => target.Change("NOW()", false, true));
                    result = true;
                    break;
                case "UtcNow":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) => target.Change("UTC_TIMESTAMP()", false, true));
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

                        return targetSegment.Change($"CONVERT({targetSegment.Body},DATE)", false, true);
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

                        return targetSegment.Change($"DAYOFMONTH({targetSegment.Body})", false, true);
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

                        return targetSegment.Change($"DAYOFWEEK({targetSegment.Body})-1");
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

                        return targetSegment.Change($"DAYOFYEAR({targetSegment.Body})", false, true);
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

                        return targetSegment.Change($"HOUR({targetSegment.Body})", false, true);
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

                        return targetSegment.Change($"FLOOR(MICROSECOND({targetSegment.Body})/1000)", false, true);
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

                        return targetSegment.Change($"MINUTE({targetSegment.Body})", false, true);
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

                        return targetSegment.Change($"MONTH({targetSegment.Body})", false, true);
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

                        return targetSegment.Change($"SECOND({targetSegment.Body})", false, true);
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

                        return targetSegment.Change($"TIMESTAMPDIFF(MICROSECOND,'0001-01-01',{targetSegment.Body})*10");
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

                        return targetSegment.Change($"CONVERT({targetSegment.Body},TIME)", false, true);
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

                        return targetSegment.Change($"YEAR({targetSegment.Body})", false, true);
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
                        return leftSegment.Merge(rightSegment, $"DAYOFMONTH(LAST_DAY(CONCAT({leftArgument},'-',{rightArgument},'-01')))", false, true);
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
                        return valueSegment.Change($"({valueArgument}%4=0 AND {valueArgument}%100<>0 OR {valueArgument}%400=0)", false, true);
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

                        string formatArgument = null;
                        if (formatSegment.IsConstant)
                        {
                            formatArgument = $"'{formatSegment}'";

                            if (formatArgument.Contains("mm"))
                                formatArgument = formatArgument.NextReplace("mm", "%i");
                            else formatArgument = formatArgument.NextReplace("m", "%i");

                            if (formatArgument.Contains("yyyy"))
                                formatArgument = formatArgument.NextReplace("yyyy", "%Y");
                            else if (formatArgument.Contains("yyy"))
                                formatArgument = formatArgument.NextReplace("yyy", "%Y");
                            else if (formatArgument.Contains("yy"))
                                formatArgument = formatArgument.NextReplace("yy", "%y");

                            if (formatArgument.Contains("MMMM"))
                                formatArgument = formatArgument.NextReplace("MMMM", "%M");
                            else if (formatArgument.Contains("MMM"))
                                formatArgument = formatArgument.NextReplace("MMM", "%b");
                            else if (formatArgument.Contains("MM"))
                                formatArgument = formatArgument.NextReplace("MM", "%m");
                            else if (formatArgument.Contains("M"))
                                formatArgument = formatArgument.NextReplace("M", "%c");

                            if (formatArgument.Contains("dddd"))
                                formatArgument = formatArgument.NextReplace("dddd", "%W");
                            else if (formatArgument.Contains("ddd"))
                                formatArgument = formatArgument.NextReplace("ddd", "%a");
                            else if (formatArgument.Contains("dd"))
                                formatArgument = formatArgument.NextReplace("dd", "%d");
                            else if (formatArgument.Contains("d"))
                                formatArgument = formatArgument.NextReplace("d", "%e");

                            if (formatArgument.Contains("HH"))
                                formatArgument = formatArgument.NextReplace("HH", "%H");
                            else if (formatArgument.Contains("H"))
                                formatArgument = formatArgument.NextReplace("H", "%k");
                            else if (formatArgument.Contains("hh"))
                                formatArgument = formatArgument.NextReplace("hh", "%h");
                            else if (formatArgument.Contains("h"))
                                formatArgument = formatArgument.NextReplace("h", "%l");

                            if (formatArgument.Contains("ss"))
                                formatArgument = formatArgument.NextReplace("ss", "%s");
                            else if (formatArgument.Contains("s"))
                                formatArgument = formatArgument.NextReplace("s", "%s");

                            if (formatArgument.Contains("tt"))
                                formatArgument = formatArgument.NextReplace("tt", "%p");
                            else if (formatArgument.Contains('t'))
                                formatArgument = formatArgument.NextReplace("t", "%p");
                        }
                        else formatArgument = visitor.GetQuotedValue(formatSegment);
                        var valueArgument = visitor.GetQuotedValue(valueSegment);
                        return valueSegment.Merge(formatSegment, $"STR_TO_DATE({valueArgument},{formatArgument})", false, true);
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
                                builder.Append($"DATE_ADD({targetArgument},INTERVAL {timeSpan.Days} DAY)");
                                timeSpan = timeSpan.Subtract(TimeSpan.FromDays(timeSpan.Days));
                            }
                            if (timeSpan.Ticks > 0)
                            {
                                if (builder.Length > 0) builder.Insert(0, $"ADDTIME(");
                                else builder.Append($"ADDTIME({targetArgument}");
                                builder.Append($",{this.GetQuotedValue(timeSpan)})");
                            }
                            return targetSegment.Change(builder.ToString(), false, true);
                        }
                        //非常量、变量的，只能小于一天,数据库的Time类型映射成TimeSpan
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(rightSegment, $"ADDTIME({targetArgument},{rightArgument})", false, true);
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
                        return targetSegment.Merge(rightSegment, $"DATE_ADD({targetArgument},INTERVAL {rightArgument} DAY)", false, true);
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
                        return targetSegment.Merge(rightSegment, $"DATE_ADD({targetArgument},INTERVAL {rightArgument} HOUR)", false, true);
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
                        var rightArgument = visitor.GetQuotedValue(rightSegment, true);
                        return targetSegment.Merge(rightSegment, $"DATE_ADD({targetArgument},INTERVAL {rightArgument}*1000 MICROSECOND)", false, true);
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
                        return targetSegment.Merge(rightSegment, $"DATE_ADD({targetArgument},INTERVAL {rightArgument} MINUTE)", false, true);
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
                        return targetSegment.Merge(rightSegment, $"DATE_ADD({targetArgument},INTERVAL {rightSegment} MONTH)", false, true);
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
                        return targetSegment.Merge(rightSegment, $"DATE_ADD({targetArgument},INTERVAL {rightArgument} SECOND)", false, true);
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
                        return targetSegment.Merge(rightSegment, $"DATE_ADD({targetArgument},INTERVAL {rightArgument}/10 MICROSECOND)", false, true);
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
                        return targetSegment.Merge(rightSegment, $"DATE_ADD({targetArgument},INTERVAL {rightArgument} YEAR)", false, true);
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
                            return targetSegment.Merge(rightSegment, $"TIMEDIFF({targetArgument},{rightArgument})", false, true);
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
                                    builder.Append($"DATE_SUB({targetArgument},INTERVAL {timeSpan.Days} DAY)");
                                    timeSpan = timeSpan.Subtract(TimeSpan.FromDays(timeSpan.Days));
                                }
                                if (timeSpan.Ticks > 0)
                                {
                                    if (builder.Length > 0) builder.Insert(0, $"SUBTIME(");
                                    else builder.Append($"SUBTIME({targetArgument}");
                                    builder.Append($",{this.GetQuotedValue(timeSpan)})");
                                }
                                return targetSegment.Change(builder.ToString(), false, true);
                            }
                            //非常量、变量的，只能小于一天,数据库的Time类型映射成TimeSpan
                            var rightArgument = visitor.GetQuotedValue(rightSegment);
                            return targetSegment.Merge(rightSegment, $"SUBTIME({targetArgument},{rightArgument})", false, true);
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
                        return targetSegment.Merge(rightSegment, $"{targetArgument}={rightArgument}");
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

                            return targetSegment.Change($"DATE_FORMAT({targetSegment.Body},'%Y-%m-%d %H:%i:%s')", false, true);
                        });
                        result = true;
                    }
                    if (parameterInfos.Length == 1)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });
                            var formatSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });

                            string formatArgument = null;
                            if (formatSegment.IsConstant || formatSegment.IsVariable)
                            {
                                formatArgument = $"'{formatSegment}'";

                                //分钟
                                if (formatArgument.Contains("mm"))
                                    formatArgument = formatArgument.NextReplace("mm", "%i");
                                else formatArgument = formatArgument.NextReplace("m", "%i");

                                if (formatArgument.Contains("yyyy"))
                                    formatArgument = formatArgument.NextReplace("yyyy", "%Y");
                                else if (formatArgument.Contains("yyy"))
                                    formatArgument = formatArgument.NextReplace("yyy", "%Y");
                                else if (formatArgument.Contains("yy"))
                                    formatArgument = formatArgument.NextReplace("yy", "%y");

                                if (formatArgument.Contains("MMMM"))
                                    formatArgument = formatArgument.NextReplace("MMMM", "%M");
                                else if (formatArgument.Contains("MMM"))
                                    formatArgument = formatArgument.NextReplace("MMM", "%b");
                                else if (formatArgument.Contains("MM"))
                                    formatArgument = formatArgument.NextReplace("MM", "%m");
                                else if (formatArgument.Contains("M"))
                                    formatArgument = formatArgument.NextReplace("M", "%c");

                                if (formatArgument.Contains("dddd"))
                                    formatArgument = formatArgument.NextReplace("dddd", "%W");
                                else if (formatArgument.Contains("ddd"))
                                    formatArgument = formatArgument.NextReplace("ddd", "%a");
                                else if (formatArgument.Contains("dd"))
                                    formatArgument = formatArgument.NextReplace("dd", "%d");
                                else if (formatArgument.Contains("d"))
                                    formatArgument = formatArgument.NextReplace("d", "%e");

                                if (formatArgument.Contains("HH"))
                                    formatArgument = formatArgument.NextReplace("HH", "%H");
                                else if (formatArgument.Contains("H"))
                                    formatArgument = formatArgument.NextReplace("H", "%k");
                                else if (formatArgument.Contains("hh"))
                                    formatArgument = formatArgument.NextReplace("hh", "%h");
                                else if (formatArgument.Contains("h"))
                                    formatArgument = formatArgument.NextReplace("h", "%l");

                                if (formatArgument.Contains("ss"))
                                    formatArgument = formatArgument.NextReplace("ss", "%s");
                                else if (formatArgument.Contains("s"))
                                    formatArgument = formatArgument.NextReplace("s", "%s");

                                if (formatArgument.Contains("tt"))
                                    formatArgument = formatArgument.NextReplace("tt", "%p");
                                else if (formatArgument.Contains("t"))
                                    formatArgument = formatArgument.NextReplace("t", "SUBSTRING(%p,1,1)");
                            }
                            else formatArgument = visitor.GetQuotedValue(formatSegment);

                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (formatSegment.IsConstant || formatSegment.IsVariable))
                                return targetSegment.MergeValue(formatSegment, ((DateTime)targetSegment.Value).ToString(formatSegment.Value.ToString()));

                            var targetArgument = visitor.GetQuotedValue(targetSegment);
                            return targetSegment.Merge(formatSegment, $"DATE_FORMAT({targetArgument},{formatArgument})", false, true);
                        });
                        result = true;
                    }
                    break;
            }
        }
        return result;
    }
}
