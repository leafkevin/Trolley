using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.PostgreSql;

partial class PostgreSqlProvider
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
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) => target.Change(DateTime.MinValue, true));
                    result = true;
                    break;
                case "MaxValue":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) => target.Change(DateTime.MaxValue, true));
                    result = true;
                    break;
                case "UnixEpoch":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) => target.Change(DateTime.UnixEpoch, true));
                    result = true;
                    break;
                case "Today":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) => target.Change("CURRENT_DATE", false, false, false, true));
                    result = true;
                    break;
                case "Now":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) => target.Change("CURRENT_TIMESTAMP", false, false, false, true));
                    result = true;
                    break;
                case "UtcNow":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) => target.Change("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'", false, false, true));
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
                        SqlSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Date);

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        return targetSegment.Change($"{this.CastTo(typeof(DateOnly), targetArgument)}", false, false, false, true);
                    });
                    result = true;
                    break;
                case "Day":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Day);

                        return targetSegment.Change($"EXTRACT(DAY FROM {targetSegment})::INT4", false, false, true);
                    });
                    result = true;
                    break;
                case "DayOfWeek":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.Change(((DateTime)targetSegment.Value).DayOfWeek);

                        return targetSegment.Change($"EXTRACT(DOW FROM {targetSegment})::INT4", false, false, true);
                    });
                    result = true;
                    break;
                case "DayOfYear":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.Change(((DateTime)targetSegment.Value).DayOfYear);

                        return targetSegment.Change($"EXTRACT(DOY FROM {targetSegment})::INT4", false, false, true);
                    });
                    result = true;
                    break;
                case "Hour":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Hour);

                        return targetSegment.Change($"EXTRACT(HOUR FROM {targetSegment})::INT4", false, false, true);
                    });
                    result = true;
                    break;
                case "Kind":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Kind);

                        throw new NotSupportedException("不支持的成员访问，DateTime只支持常量的Kind成员访问");
                    });
                    result = true;
                    break;
                case "Millisecond":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Millisecond);

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        return targetSegment.Change($"(EXTRACT(MILLISECONDS FROM {targetArgument})-FLOOR(EXTRACT(SECOND FROM {targetArgument}))*1000)::INT8", false, false, true);
                    });
                    result = true;
                    break;
                case "Minute":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Minute);

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        return targetSegment.Change($"EXTRACT(MINUTE FROM {targetArgument})::INT4", false, false, true);
                    });
                    result = true;
                    break;
                case "Month":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Month);

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        return targetSegment.Change($"EXTRACT(MONTH FROM {targetArgument})::INT4", false, false, true);
                    });
                    result = true;
                    break;
                case "Second":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Second);

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        return targetSegment.Change($"FLOOR(EXTRACT(SECOND FROM {targetArgument}))::INT4", false, false, true);
                    });
                    result = true;
                    break;
                case "Ticks":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Ticks);

                        return targetSegment.Change($"(EXTRACT(EPOCH FROM {targetSegment})*10000000+621355968000000000:::INT8", false, false, true);
                    });
                    result = true;
                    break;
                case "TimeOfDay":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.Change(((DateTime)targetSegment.Value).TimeOfDay);

                        return targetSegment.Change($"{targetSegment.Value}-{this.CastTo(typeof(DateOnly), targetSegment.Value)}", false, false, true);
                    });
                    result = true;
                    break;
                case "Year":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlSegment targetSegment = null;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Year);

                        return targetSegment.Change($"EXTRACT(YEAR FROM {targetSegment})::INT4", false, false, true);
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
                        var leftSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[1] });
                        if ((leftSegment.IsConstant || leftSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return leftSegment.Merge(rightSegment, DateTime.DaysInMonth(Convert.ToInt32(leftSegment.Value), Convert.ToInt32(rightSegment.Value)));

                        var leftArgument = visitor.GetQuotedValue(leftSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return leftSegment.Merge(rightSegment, $"EXTRACT(DAYS FROM (MAKE_DATE({leftArgument},{rightArgument},1)+INTERVAL '1 MONTH'-INTERVAL '1 DAY'))", false, false, false, true);
                    });
                    result = true;
                    break;
                case "IsLeapYear":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if (valueSegment.IsConstant || valueSegment.IsVariable)
                            return valueSegment.Change(DateTime.IsLeapYear(Convert.ToInt32(valueSegment.Value)));

                        var valueArgument = visitor.GetQuotedValue(valueSegment);
                        if (valueSegment.IsExpression) valueArgument = $"({valueArgument})";
                        return valueSegment.Change($"{valueArgument}%4=0 AND {valueArgument}%100<>0 OR {valueArgument}%400=0", false, false, true);
                    });
                    result = true;
                    break;
                case "Parse":
                case "TryParse":
                    if (parameterInfos.Length == 3)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var valueSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                            var providerSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[1] });
                            var styleSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[2] });

                            if ((valueSegment.IsConstant || valueSegment.IsVariable)
                                && (providerSegment.IsConstant || providerSegment.IsVariable)
                                && (styleSegment.IsConstant || styleSegment.IsVariable))
                                return valueSegment.Change(DateTime.Parse(valueSegment.ToString(), (IFormatProvider)providerSegment.Value, (DateTimeStyles)styleSegment.Value));

                            return valueSegment.Change($"{valueSegment}::TIMESTAMP", false, false, true);
                        });
                    }
                    else if (parameterInfos.Length == 2)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var valueSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                            var providerSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[1] });

                            if ((valueSegment.IsConstant || valueSegment.IsVariable)
                                && (providerSegment.IsConstant || providerSegment.IsVariable))
                                return valueSegment.Change(DateTime.Parse(valueSegment.ToString(), (IFormatProvider)providerSegment.Value));

                            return valueSegment.Change($"{valueSegment}::TIMESTAMP", false, false, true);
                        });
                    }
                    else
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var valueSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                            if (valueSegment.IsConstant || valueSegment.IsVariable)
                                return valueSegment.Change(DateTime.Parse(valueSegment.ToString()));

                            return valueSegment.Change($"{valueSegment}::TIMESTAMP", false, false, true);
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
                        var valueSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        var formatSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[1] });
                        var providerSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[2] });

                        if ((valueSegment.IsConstant || valueSegment.IsVariable)
                            && (formatSegment.IsConstant || formatSegment.IsVariable)
                            && (providerSegment.IsConstant || providerSegment.IsVariable))
                            return valueSegment.Merge(formatSegment, DateTime.ParseExact(valueSegment.ToString(), formatSegment.ToString(), (IFormatProvider)providerSegment.Value));

                        string formatArgument = null;
                        if (formatSegment.IsConstant)
                        {
                            formatArgument = $"'{formatSegment}'";

                            if (formatArgument.Contains("yyyy"))
                                formatArgument = formatArgument.NextReplace("yyyy", "YYYY");
                            else if (formatArgument.Contains("yyy"))
                                formatArgument = formatArgument.NextReplace("yyy", "YYY");
                            else if (formatArgument.Contains("yy"))
                                formatArgument = formatArgument.NextReplace("yy", "YY");

                            if (formatArgument.Contains("MMMM"))
                                formatArgument = formatArgument.NextReplace("MMMM", "Month");
                            else if (formatArgument.Contains("MMM"))
                                formatArgument = formatArgument.NextReplace("MMM", "Mon");
                            else if (formatArgument.Contains("M") && !formatArgument.Contains("MM"))
                                formatArgument = formatArgument.NextReplace("M", "FMMM");

                            if (formatArgument.Contains("dddd"))
                                formatArgument = formatArgument.NextReplace("dddd", "Day");
                            else if (formatArgument.Contains("ddd"))
                                formatArgument = formatArgument.NextReplace("ddd", "DY");
                            else if (formatArgument.Contains("dd"))
                                formatArgument = formatArgument.NextReplace("dd", "DD");
                            else if (formatArgument.Contains("d"))
                                formatArgument = formatArgument.NextReplace("d", "FMDD");

                            if (formatArgument.Contains("HH"))
                                formatArgument = formatArgument.NextReplace("HH", "HH24");
                            else if (formatArgument.Contains("H"))
                                formatArgument = formatArgument.NextReplace("H", "FMHH24");
                            else if (formatArgument.Contains("hh"))
                                formatArgument = formatArgument.NextReplace("hh", "HH12");
                            else if (formatArgument.Contains("h"))
                                formatArgument = formatArgument.NextReplace("h", "FMHH12");

                            if (formatArgument.Contains("mm"))
                                formatArgument = formatArgument.NextReplace("mm", "MI");
                            else formatArgument = formatArgument.NextReplace("m", "FMMI");

                            if (formatArgument.Contains("ss"))
                                formatArgument = formatArgument.NextReplace("ss", "SS");
                            else if (formatArgument.Contains("s"))
                                formatArgument = formatArgument.NextReplace("s", "FMSS");

                            if (formatArgument.Contains("tt"))
                                formatArgument = formatArgument.NextReplace("tt", "AM");
                            else if (formatArgument.Contains("t"))
                                formatArgument = formatArgument.NextReplace("t", "AM");

                            if (formatArgument.Contains("FFFFFF"))
                                formatArgument = formatArgument.NextReplace("FFFFFF", "US");
                            else if (formatArgument.Contains("FFFFF"))
                                formatArgument = formatArgument.NextReplace("FFFFF", "FMUS");
                            else if (formatArgument.Contains("FFFF"))
                                formatArgument = formatArgument.NextReplace("FFFF", "FMUS");
                            else if (formatArgument.Contains("ffffff"))
                                formatArgument = formatArgument.NextReplace("ffffff", "US");
                            else if (formatArgument.Contains("fffff"))
                                formatArgument = formatArgument.NextReplace("fffff", "FMUS");
                            else if (formatArgument.Contains("ffff"))
                                formatArgument = formatArgument.NextReplace("ffff", "FMUS");

                            if (formatArgument.Contains("FFF"))
                                formatArgument = formatArgument.NextReplace("FFF", "MS");
                            else if (formatArgument.Contains("FF"))
                                formatArgument = formatArgument.NextReplace("FF", "FMMS");
                            else if (formatArgument.Contains("F"))
                                formatArgument = formatArgument.NextReplace("F", "FMMS");
                            else if (formatArgument.Contains("fff"))
                                formatArgument = formatArgument.NextReplace("fff", "MS");
                            else if (formatArgument.Contains("ff"))
                                formatArgument = formatArgument.NextReplace("ff", "FMMS");
                            else if (formatArgument.Contains("f"))
                                formatArgument = formatArgument.NextReplace("f", "FMMS");
                        }
                        else formatArgument = visitor.GetQuotedValue(formatSegment);
                        var valueArgument = visitor.GetQuotedValue(valueSegment);
                        return valueSegment.Merge(formatSegment, $"STR_TO_DATE({valueArgument},{formatArgument})", false, false, false, true);
                    });
                    result = true;
                    if (methodInfo.IsStatic && parameterInfos.Length >= 1 && parameterInfos[0].ParameterType == typeof(ReadOnlySpan<char>))
                        throw new NotSupportedException($"DateTime.{methodInfo.Name}方法暂时不支持ReadOnlySpan<char>类型参数的解析，请转换成String类型");
                    break;
                case "Compare":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var leftSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[1] });

                        var leftArgument = visitor.GetQuotedValue(leftSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return leftSegment.Merge(rightSegment, $"CASE WHEN {leftArgument}={rightArgument} THEN 0 WHEN {leftArgument}>{rightArgument} THEN 1 ELSE -1 END", false, false, true);
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
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return targetSegment.Merge(rightSegment, Convert.ToDateTime(targetSegment.Value).Add((TimeSpan)rightSegment.Value));

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        if (rightSegment.IsConstant || rightSegment.IsVariable)
                        {
                            var builder = new StringBuilder();
                            builder.Append(targetArgument);
                            var timeSpan = (TimeSpan)rightSegment.Value;
                            builder.Append(timeSpan.Ticks > 0 ? "+" : "-");
                            builder.Append(" INTERVAL '");
                            if (timeSpan.Ticks < 0)
                                timeSpan = -timeSpan;
                            if (timeSpan.TotalDays > 1)
                            {
                                var days = Math.Floor(timeSpan.TotalDays);
                                builder.Append($"{days}D");
                                timeSpan = timeSpan.Subtract(TimeSpan.FromDays(days));
                            }
                            if (timeSpan.Ticks > 0)
                                builder.Append(timeSpan.ToString("hh\\:mm\\:ss\\.ffffff"));
                            builder.Append("'");
                            return targetSegment.Change(builder.ToString(), false, false, true);
                        }
                        //非常量、变量的，只能小于一天,数据库的Time类型映射成TimeSpan
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(rightSegment, $"{targetArgument}-{rightArgument}", false, false, true);
                    });
                    result = true;
                    break;
                case "AddDays":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                           && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return targetSegment.Merge(rightSegment, Convert.ToDateTime(targetSegment.Value).AddDays(Convert.ToDouble(rightSegment.Value)));

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(rightSegment, $"{targetArgument}+INTERVAL '1D'*{rightArgument}", false, false, true);
                    });
                    result = true;
                    break;
                case "AddHours":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                           && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return targetSegment.Merge(rightSegment, Convert.ToDateTime(targetSegment.Value).AddHours(Convert.ToDouble(rightSegment.Value)));

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(rightSegment, $"{targetArgument}+INTERVAL '1H'*{rightArgument}", false, false, true);
                    });
                    result = true;
                    break;
                case "AddMilliseconds":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return targetSegment.Merge(rightSegment, Convert.ToDateTime(targetSegment.Value).AddMilliseconds(Convert.ToDouble(rightSegment.Value)));

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(rightSegment, $"{targetArgument}+INTERVAL '1S'*{rightArgument}/1000", false, false, true);
                    });
                    result = true;
                    break;
                case "AddMinutes":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return targetSegment.Merge(rightSegment, Convert.ToDateTime(targetSegment.Value).AddMinutes(Convert.ToDouble(rightSegment.Value)));

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(rightSegment, $"{targetArgument}+INTERVAL '1M'*{rightArgument}", false, false, true);
                    });
                    result = true;
                    break;
                case "AddMonths":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return targetSegment.Merge(rightSegment, Convert.ToDateTime(targetSegment.Value).AddMonths(Convert.ToInt32(rightSegment.Value)));

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(rightSegment, $"{targetArgument}+INTERVAL '1MON'*{rightArgument}", false, false, true);
                    });
                    result = true;
                    break;
                case "AddSeconds":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return targetSegment.Merge(rightSegment, Convert.ToDateTime(targetSegment.Value).AddSeconds(Convert.ToDouble(rightSegment.Value)));

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(rightSegment, $"{targetArgument}+INTERVAL '1S'*{rightArgument}", false, false, true);
                    });
                    result = true;
                    break;
                case "AddTicks":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return targetSegment.Merge(rightSegment, Convert.ToDateTime(targetSegment.Value).AddTicks(Convert.ToInt64(rightSegment.Value)));

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(rightSegment, $"{targetArgument}+INTERVAL '1S'*{rightArgument}/10000000", false, false, false, true);
                    });
                    result = true;
                    break;
                case "AddYears":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if ((targetSegment.IsConstant || targetSegment.IsVariable)
                            && (rightSegment.IsConstant || rightSegment.IsVariable))
                            return targetSegment.Merge(rightSegment, Convert.ToDateTime(targetSegment.Value).AddYears(Convert.ToInt32(rightSegment.Value)));

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(rightSegment, $"{targetArgument}+INTERVAL '1Y'*{rightArgument}", false, false, true);
                    });
                    result = true;
                    break;
                case "Subtract":
                    if (parameterInfos[0].ParameterType == typeof(DateTime))
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                            var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (rightSegment.IsConstant || rightSegment.IsVariable))
                                return targetSegment.Merge(rightSegment, Convert.ToDateTime(targetSegment.Value).Subtract(Convert.ToDateTime(rightSegment.Value)));

                            var targetArgument = visitor.GetQuotedValue(targetSegment);
                            var rightArgument = visitor.GetQuotedValue(rightSegment);
                            return targetSegment.Merge(rightSegment, $"{targetArgument}-{rightArgument}", false, false, true);
                        });
                        result = true;
                    }
                    if (parameterInfos[0].ParameterType == typeof(TimeSpan))
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                            var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (rightSegment.IsConstant || rightSegment.IsVariable))
                                return targetSegment.Merge(rightSegment, Convert.ToDateTime(targetSegment.Value).Subtract((TimeSpan)rightSegment.Value));

                            var targetArgument = visitor.GetQuotedValue(targetSegment);
                            var rightArgument = visitor.GetQuotedValue(rightSegment);
                            return targetSegment.Merge(rightSegment, $"{targetArgument}-{rightArgument}", false, false, true);
                        });
                        result = true;
                    }
                    break;
                case "Equals":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(rightSegment, $"{targetArgument}={rightArgument}", false, false, true);
                    });
                    result = true;
                    break;
                case "CompareTo":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(rightSegment, $"CASE WHEN {targetArgument}={rightArgument} THEN 0 WHEN {targetArgument}>{rightArgument} THEN 1 ELSE -1 END", false, false, true);
                    });
                    result = true;
                    break;
                case "ToString":
                    if (parameterInfos.Length == 0)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                            if (targetSegment.IsConstant || targetSegment.IsVariable)
                                return targetSegment.Change(targetSegment.ToString());

                            var targetArgument = visitor.GetQuotedValue(targetSegment);
                            return targetSegment.Change($"TO_CHAR({targetArgument},'YYYY-MM-DD HH24:MI:SS.MS')", false, false, false, true);
                        });
                        result = true;
                    }
                    if (parameterInfos.Length == 1)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                            var formatSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });

                            string formatArgument = null;
                            if (formatSegment.IsConstant || formatSegment.IsVariable)
                            {
                                formatArgument = $"'{formatSegment}'";

                                if (formatArgument.Contains("yyyy"))
                                    formatArgument = formatArgument.NextReplace("yyyy", "YYYY");
                                else if (formatArgument.Contains("yyy"))
                                    formatArgument = formatArgument.NextReplace("yyy", "YYY");
                                else if (formatArgument.Contains("yy"))
                                    formatArgument = formatArgument.NextReplace("yy", "YY");

                                if (formatArgument.Contains("MMMM"))
                                    formatArgument = formatArgument.NextReplace("MMMM", "Month");
                                else if (formatArgument.Contains("MMM"))
                                    formatArgument = formatArgument.NextReplace("MMM", "Mon");
                                else if (formatArgument.Contains("M") && !formatArgument.Contains("MM"))
                                    formatArgument = formatArgument.NextReplace("M", "FMMM");

                                if (formatArgument.Contains("dddd"))
                                    formatArgument = formatArgument.NextReplace("dddd", "Day");
                                else if (formatArgument.Contains("ddd"))
                                    formatArgument = formatArgument.NextReplace("ddd", "DY");
                                else if (formatArgument.Contains("dd"))
                                    formatArgument = formatArgument.NextReplace("dd", "DD");
                                else if (formatArgument.Contains("d"))
                                    formatArgument = formatArgument.NextReplace("d", "FMDD");

                                if (formatArgument.Contains("HH"))
                                    formatArgument = formatArgument.NextReplace("HH", "HH24");
                                else if (formatArgument.Contains("H"))
                                    formatArgument = formatArgument.NextReplace("H", "FMHH24");
                                else if (formatArgument.Contains("hh"))
                                    formatArgument = formatArgument.NextReplace("hh", "HH12");
                                else if (formatArgument.Contains("h"))
                                    formatArgument = formatArgument.NextReplace("h", "FMHH12");

                                if (formatArgument.Contains("mm"))
                                    formatArgument = formatArgument.NextReplace("mm", "MI");
                                else formatArgument = formatArgument.NextReplace("m", "FMMI");

                                if (formatArgument.Contains("ss"))
                                    formatArgument = formatArgument.NextReplace("ss", "SS");
                                else if (formatArgument.Contains("s"))
                                    formatArgument = formatArgument.NextReplace("s", "FMSS");

                                if (formatArgument.Contains("tt"))
                                    formatArgument = formatArgument.NextReplace("tt", "AM");
                                else if (formatArgument.Contains("t"))
                                    formatArgument = formatArgument.NextReplace("t", "AM");

                                if (formatArgument.Contains("FFFFFF"))
                                    formatArgument = formatArgument.NextReplace("FFFFFF", "US");
                                else if (formatArgument.Contains("FFFFF"))
                                    formatArgument = formatArgument.NextReplace("FFFFF", "FMUS");
                                else if (formatArgument.Contains("FFFF"))
                                    formatArgument = formatArgument.NextReplace("FFFF", "FMUS");
                                else if (formatArgument.Contains("ffffff"))
                                    formatArgument = formatArgument.NextReplace("ffffff", "US");
                                else if (formatArgument.Contains("fffff"))
                                    formatArgument = formatArgument.NextReplace("fffff", "FMUS");
                                else if (formatArgument.Contains("ffff"))
                                    formatArgument = formatArgument.NextReplace("ffff", "FMUS");

                                if (formatArgument.Contains("FFF"))
                                    formatArgument = formatArgument.NextReplace("FFF", "MS");
                                else if (formatArgument.Contains("FF"))
                                    formatArgument = formatArgument.NextReplace("FF", "FMMS");
                                else if (formatArgument.Contains("F"))
                                    formatArgument = formatArgument.NextReplace("F", "FMMS");
                                else if (formatArgument.Contains("fff"))
                                    formatArgument = formatArgument.NextReplace("fff", "MS");
                                else if (formatArgument.Contains("ff"))
                                    formatArgument = formatArgument.NextReplace("ff", "FMMS");
                                else if (formatArgument.Contains("f"))
                                    formatArgument = formatArgument.NextReplace("f", "FMMS");
                            }
                            else formatArgument = visitor.GetQuotedValue(formatSegment);

                            if ((targetSegment.IsConstant || targetSegment.IsVariable)
                                && (formatSegment.IsConstant || formatSegment.IsVariable))
                                return targetSegment.Merge(formatSegment, ((DateTime)targetSegment.Value).ToString(formatSegment.ToString()));

                            var targetArgument = visitor.GetQuotedValue(targetSegment);
                            return targetSegment.Merge(formatSegment, $"TO_CHAR({targetArgument},{formatArgument})", false, false, false, true);
                        });
                        result = true;
                    }
                    break;
            }
        }
        return result;
    }
}