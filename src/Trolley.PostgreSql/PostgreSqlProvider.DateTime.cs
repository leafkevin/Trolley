using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.PostgreSql;

partial class PostgreSqlProvider
{
#if !NETSTANDARD2_1_OR_GREATER
    private static DateTime UnixEpoch = new DateTime(1970, 1, 1);
#endif
    public override bool TryGetDateTimeMemberAccessSqlFormatter(MemberExpression memberExpr, out Func<ISqlVisitor, SqlSegment, SqlSegment> formatter)
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
#if NETSTANDARD2_1_OR_GREATER
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) => target.ChangeValue(DateTime.UnixEpoch, true));       
#else
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) => target.ChangeValue(UnixEpoch, true));
#endif
                    result = true;
                    break;
                case "Today":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) => target.Change("CURRENT_DATE", false, true));
                    result = true;
                    break;
                case "Now":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) => target.Change("CURRENT_TIMESTAMP", false, true));
                    result = true;
                    break;
                case "UtcNow":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) => target.Change("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'"));
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
                        SqlSegment targetSegment = default;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.Visit(target);

                        if (targetSegment.IsValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Date);
                        return targetSegment.Change($"{targetSegment.ToExprWrap()}::DATE", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "Day":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlSegment targetSegment = default;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.Visit(target);

                        if (targetSegment.IsValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Day);

                        return targetSegment.Change($"EXTRACT(DAY FROM {targetSegment.Value})::INT4");
                    });
                    result = true;
                    break;
                case "DayOfWeek":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlSegment targetSegment = default;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.Visit(target);

                        if (targetSegment.IsValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).DayOfWeek);

                        return targetSegment.Change($"EXTRACT(DOW FROM {targetSegment.Value})::INT4");
                    });
                    result = true;
                    break;
                case "DayOfYear":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlSegment targetSegment = default;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.Visit(target);

                        if (targetSegment.IsValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).DayOfYear);

                        return targetSegment.Change($"EXTRACT(DOY FROM {targetSegment.Value})::INT4");
                    });
                    result = true;
                    break;
                case "Hour":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlSegment targetSegment = default;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.Visit(target);

                        if (targetSegment.IsValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Hour);

                        return targetSegment.Change($"EXTRACT(HOUR FROM {targetSegment.Value})::INT4");
                    });
                    result = true;
                    break;
                case "Kind":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlSegment targetSegment = default;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.Visit(target);

                        if (targetSegment.IsValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Kind);

                        throw new NotSupportedException("不支持的成员访问，DateTime只支持常量的Kind成员访问");
                    });
                    result = true;
                    break;
                case "Millisecond":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlSegment targetSegment = default;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.Visit(target);

                        if (targetSegment.IsValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Millisecond);

                        return targetSegment.Change($"(EXTRACT(MILLISECONDS FROM {targetSegment.Value})-FLOOR(EXTRACT(SECOND FROM {targetSegment.Value}))*1000)::INT8");
                    });
                    result = true;
                    break;
                case "Minute":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlSegment targetSegment = default;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.Visit(target);

                        if (targetSegment.IsValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Minute);

                        return targetSegment.Change($"EXTRACT(MINUTE FROM {targetSegment.Value})::INT4");
                    });
                    result = true;
                    break;
                case "Month":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlSegment targetSegment = default;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.Visit(target);

                        if (targetSegment.IsValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Month);

                        return targetSegment.Change($"EXTRACT(MONTH FROM {targetSegment.Value})::INT4");
                    });
                    result = true;
                    break;
                case "Second":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlSegment targetSegment = default;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.Visit(target);

                        if (targetSegment.IsValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Second);

                        return targetSegment.Change($"FLOOR(EXTRACT(SECOND FROM {targetSegment.Value}))::INT4");
                    });
                    result = true;
                    break;
                case "Ticks":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlSegment targetSegment = default;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.Visit(target);

                        if (targetSegment.IsValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Ticks);

                        return targetSegment.Change($"(EXTRACT(EPOCH FROM {targetSegment.Value})*10000000+621355968000000000:::INT8");
                    });
                    result = true;
                    break;
                case "TimeOfDay":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlSegment targetSegment = default;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.Visit(target);

                        if (targetSegment.IsValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).TimeOfDay);

                        return targetSegment.Change($"{targetSegment.ToExprWrap()}::TIME");
                    });
                    result = true;
                    break;
                case "Year":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, (visitor, target) =>
                    {
                        SqlSegment targetSegment = default;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.Visit(target);

                        if (targetSegment.IsValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Year);

                        return targetSegment.Change($"EXTRACT(YEAR FROM {targetSegment.Value})::INT4");
                    });
                    result = true;
                    break;
            }
        }
        return result;
    }
    public override bool TryGetDateTimeMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out Func<ISqlVisitor, MethodCallExpression, Stack<DeferredOperation>, SqlSegment> formatter)
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
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var leftSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[1] });
                        if ((leftSegment.IsValue)
                            && (rightSegment.IsValue))
                            return leftSegment.MergeValue(rightSegment, DateTime.DaysInMonth(Convert.ToInt32(leftSegment.Value), Convert.ToInt32(rightSegment.Value)));

                        var leftArgument = visitor.GetQuotedValue(leftSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return leftSegment.Merge(rightSegment, $"EXTRACT(DAYS FROM (MAKE_DATE({leftArgument},{rightArgument},1)+INTERVAL '1 MONTH'-INTERVAL '1 DAY'))", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "IsLeapYear":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var valueSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if (valueSegment.IsValue)
                            return valueSegment.Change(DateTime.IsLeapYear(Convert.ToInt32(valueSegment.Value)));

                        var valueArgument = valueSegment.ToExprWrap();
                        return valueSegment.Change($"({valueArgument}%4=0 AND {valueArgument}%100<>0 OR {valueArgument}%400=0)", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "Parse":
                case "TryParse":
                    if (parameterInfos.Length == 3)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var valueSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            var providerSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[1] });
                            var styleSegment = visitor.Visit(new SqlSegment { Expression = args[2] });

                            if ((valueSegment.IsConstant || valueSegment.IsVariable)
                                && providerSegment.IsValue
                                && (styleSegment.IsConstant || styleSegment.IsVariable))
                                return valueSegment.Change(DateTime.Parse(valueSegment.Value.ToString(), (IFormatProvider)providerSegment.Value, (DateTimeStyles)styleSegment.Value));

                            return valueSegment.Change($"{valueSegment.Value}::TIMESTAMP");
                        });
                    }
                    else if (parameterInfos.Length == 2)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var valueSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            var providerSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[1] });

                            if ((valueSegment.IsConstant || valueSegment.IsVariable)
                                && providerSegment.IsValue)
                                return valueSegment.Change(DateTime.Parse(valueSegment.Value.ToString(), (IFormatProvider)providerSegment.Value));

                            return valueSegment.Change($"{valueSegment.Value}::TIMESTAMP");
                        });
                    }
                    else
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var valueSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            if (valueSegment.IsValue)
                                return valueSegment.Change(DateTime.Parse(valueSegment.Value.ToString()));

                            return valueSegment.Change($"{valueSegment.Value}::TIMESTAMP");
                        });
                    }
                    result = true;
                    if (methodInfo.IsStatic && parameterInfos.Length >= 3 && parameterInfos[0].ParameterType == typeof(ReadOnlySpan<char>))
                        throw new NotSupportedException("DateTime.Parse方法暂时不支持ReadOnlySpan<char>类型参数的解析，请转换成String类型");
                    break;
                case "ParseExact":
                case "TryParseExact":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var valueSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        var formatSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[1] });
                        var providerSegment = visitor.Visit(new SqlSegment { Expression = args[2] });

                        if ((valueSegment.IsConstant || valueSegment.IsVariable)
                            && formatSegment.IsValue
                            && providerSegment.IsValue)
                            return valueSegment.MergeValue(formatSegment, DateTime.ParseExact(valueSegment.Value.ToString(), formatSegment.Value.ToString(), (IFormatProvider)providerSegment.Value));

                        string formatArgument = null;
                        if (formatSegment.IsConstant)
                        {
                            formatArgument = $"'{formatSegment.Value}'";

                            if (formatArgument.Contains("yyyy"))
                                formatArgument = formatArgument.Replace("yyyy", "YYYY");
                            else if (formatArgument.Contains("yyy"))
                                formatArgument = formatArgument.Replace("yyy", "YYY");
                            else if (formatArgument.Contains("yy"))
                                formatArgument = formatArgument.Replace("yy", "YY");

                            if (formatArgument.Contains("MMMM"))
                                formatArgument = formatArgument.Replace("MMMM", "Month");
                            else if (formatArgument.Contains("MMM"))
                                formatArgument = formatArgument.Replace("MMM", "Mon");
                            else if (formatArgument.Contains("M") && !formatArgument.Contains("MM"))
                                formatArgument = formatArgument.Replace("M", "FMMM");

                            if (formatArgument.Contains("dddd"))
                                formatArgument = formatArgument.Replace("dddd", "Day");
                            else if (formatArgument.Contains("ddd"))
                                formatArgument = formatArgument.Replace("ddd", "DY");
                            else if (formatArgument.Contains("dd"))
                                formatArgument = formatArgument.Replace("dd", "DD");
                            else if (formatArgument.Contains("d"))
                                formatArgument = formatArgument.Replace("d", "FMDD");

                            if (formatArgument.Contains("HH"))
                                formatArgument = formatArgument.Replace("HH", "HH24");
                            else if (formatArgument.Contains("H"))
                                formatArgument = formatArgument.Replace("H", "FMHH24");
                            else if (formatArgument.Contains("hh"))
                                formatArgument = formatArgument.Replace("hh", "HH12");
                            else if (formatArgument.Contains("h"))
                                formatArgument = formatArgument.Replace("h", "FMHH12");

                            if (formatArgument.Contains("mm"))
                                formatArgument = formatArgument.Replace("mm", "MI");
                            else formatArgument = formatArgument.Replace("m", "FMMI");

                            if (formatArgument.Contains("ss"))
                                formatArgument = formatArgument.Replace("ss", "SS");
                            else if (formatArgument.Contains("s"))
                                formatArgument = formatArgument.Replace("s", "FMSS");

                            if (formatArgument.Contains("tt"))
                                formatArgument = formatArgument.Replace("tt", "AM");
                            else if (formatArgument.Contains("t"))
                                formatArgument = formatArgument.Replace("t", "AM");

                            if (formatArgument.Contains("FFFFFF"))
                                formatArgument = formatArgument.Replace("FFFFFF", "US");
                            else if (formatArgument.Contains("FFFFF"))
                                formatArgument = formatArgument.Replace("FFFFF", "FMUS");
                            else if (formatArgument.Contains("FFFF"))
                                formatArgument = formatArgument.Replace("FFFF", "FMUS");
                            else if (formatArgument.Contains("ffffff"))
                                formatArgument = formatArgument.Replace("ffffff", "US");
                            else if (formatArgument.Contains("fffff"))
                                formatArgument = formatArgument.Replace("fffff", "FMUS");
                            else if (formatArgument.Contains("ffff"))
                                formatArgument = formatArgument.Replace("ffff", "FMUS");

                            if (formatArgument.Contains("FFF"))
                                formatArgument = formatArgument.Replace("FFF", "MS");
                            else if (formatArgument.Contains("FF"))
                                formatArgument = formatArgument.Replace("FF", "FMMS");
                            else if (formatArgument.Contains("F"))
                                formatArgument = formatArgument.Replace("F", "FMMS");
                            else if (formatArgument.Contains("fff"))
                                formatArgument = formatArgument.Replace("fff", "MS");
                            else if (formatArgument.Contains("ff"))
                                formatArgument = formatArgument.Replace("ff", "FMMS");
                            else if (formatArgument.Contains("f"))
                                formatArgument = formatArgument.Replace("f", "FMMS");
                        }
                        else formatArgument = visitor.GetQuotedValue(formatSegment);
                        var valueArgument = visitor.GetQuotedValue(valueSegment);
                        return valueSegment.Merge(formatSegment, $"STR_TO_DATE({valueArgument},{formatArgument})", SqlType.MethodCall);
                    });
                    result = true;
                    if (methodInfo.IsStatic && parameterInfos.Length >= 1 && parameterInfos[0].ParameterType == typeof(ReadOnlySpan<char>))
                        throw new NotSupportedException($"DateTime.{methodInfo.Name}方法暂时不支持ReadOnlySpan<char>类型参数的解析，请转换成String类型");
                    break;
                case "Compare":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var leftSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[1] });

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
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                        var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if ((targetSegment.IsValue)
                            && (rightSegment.IsValue))
                            return targetSegment.MergeValue(rightSegment, Convert.ToDateTime(targetSegment.Value).Add((TimeSpan)rightSegment.Value));

                        var targetArgument = visitor.GetQuotedValue(targetSegment, true);
                        if (rightSegment.IsValue)
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
                            return targetSegment.Change(builder.ToString());
                        }
                        //非常量、变量的，只能小于一天,数据库的Time类型映射成TimeSpan
                        var rightArgument = visitor.GetQuotedValue(rightSegment, true);
                        return targetSegment.Merge(rightSegment, $"{targetArgument}-{rightArgument}");
                    });
                    result = true;
                    break;
                case "AddDays":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                        var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if ((targetSegment.IsValue)
                           && (rightSegment.IsValue))
                            return targetSegment.MergeValue(rightSegment, Convert.ToDateTime(targetSegment.Value).AddDays(Convert.ToDouble(rightSegment.Value)));

                        var targetArgument = visitor.GetQuotedValue(targetSegment, true);
                        var rightArgument = visitor.GetQuotedValue(rightSegment, true);
                        return targetSegment.Merge(rightSegment, $"{targetArgument}+INTERVAL '1D'*{rightArgument}");
                    });
                    result = true;
                    break;
                case "AddHours":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                        var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if ((targetSegment.IsValue)
                           && (rightSegment.IsValue))
                            return targetSegment.MergeValue(rightSegment, Convert.ToDateTime(targetSegment.Value).AddHours(Convert.ToDouble(rightSegment.Value)));

                        var targetArgument = visitor.GetQuotedValue(targetSegment, true);
                        var rightArgument = visitor.GetQuotedValue(rightSegment, true);
                        return targetSegment.Merge(rightSegment, $"{targetArgument}+INTERVAL '1H'*{rightArgument}");
                    });
                    result = true;
                    break;
                case "AddMilliseconds":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                        var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if ((targetSegment.IsValue)
                            && (rightSegment.IsValue))
                            return targetSegment.MergeValue(rightSegment, Convert.ToDateTime(targetSegment.Value).AddMilliseconds(Convert.ToDouble(rightSegment.Value)));

                        var targetArgument = visitor.GetQuotedValue(targetSegment, true);
                        var rightArgument = visitor.GetQuotedValue(rightSegment, true);
                        return targetSegment.Merge(rightSegment, $"{targetArgument}+INTERVAL '1S'*{rightArgument}/1000");
                    });
                    result = true;
                    break;
                case "AddMinutes":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                        var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if ((targetSegment.IsValue)
                            && (rightSegment.IsValue))
                            return targetSegment.MergeValue(rightSegment, Convert.ToDateTime(targetSegment.Value).AddMinutes(Convert.ToDouble(rightSegment.Value)));

                        var targetArgument = visitor.GetQuotedValue(targetSegment, true);
                        var rightArgument = visitor.GetQuotedValue(rightSegment, true);
                        return targetSegment.Merge(rightSegment, $"{targetArgument}+INTERVAL '1M'*{rightArgument}");
                    });
                    result = true;
                    break;
                case "AddMonths":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                        var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if ((targetSegment.IsValue)
                            && (rightSegment.IsValue))
                            return targetSegment.MergeValue(rightSegment, Convert.ToDateTime(targetSegment.Value).AddMonths(Convert.ToInt32(rightSegment.Value)));

                        var targetArgument = visitor.GetQuotedValue(targetSegment, true);
                        var rightArgument = visitor.GetQuotedValue(rightSegment, true);
                        return targetSegment.Merge(rightSegment, $"{targetArgument}+INTERVAL '1MON'*{rightArgument}");
                    });
                    result = true;
                    break;
                case "AddSeconds":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                        var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if ((targetSegment.IsValue)
                            && (rightSegment.IsValue))
                            return targetSegment.MergeValue(rightSegment, Convert.ToDateTime(targetSegment.Value).AddSeconds(Convert.ToDouble(rightSegment.Value)));

                        var targetArgument = visitor.GetQuotedValue(targetSegment, true);
                        var rightArgument = visitor.GetQuotedValue(rightSegment, true);
                        return targetSegment.Merge(rightSegment, $"{targetArgument}+INTERVAL '1S'*{rightArgument}");
                    });
                    result = true;
                    break;
                case "AddTicks":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                        var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if ((targetSegment.IsValue)
                            && (rightSegment.IsValue))
                            return targetSegment.MergeValue(rightSegment, Convert.ToDateTime(targetSegment.Value).AddTicks(Convert.ToInt64(rightSegment.Value)));

                        var targetArgument = visitor.GetQuotedValue(targetSegment, true);
                        var rightArgument = visitor.GetQuotedValue(rightSegment, true);
                        return targetSegment.Merge(rightSegment, $"{targetArgument}+INTERVAL '1S'*{rightArgument}/10000000", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "AddYears":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                        var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if ((targetSegment.IsValue)
                            && (rightSegment.IsValue))
                            return targetSegment.MergeValue(rightSegment, Convert.ToDateTime(targetSegment.Value).AddYears(Convert.ToInt32(rightSegment.Value)));

                        var targetArgument = visitor.GetQuotedValue(targetSegment, true);
                        var rightArgument = visitor.GetQuotedValue(rightSegment, true);
                        return targetSegment.Merge(rightSegment, $"{targetArgument}+INTERVAL '1Y'*{rightArgument}");
                    });
                    result = true;
                    break;
                case "Subtract":
                    if (parameterInfos[0].ParameterType == typeof(DateTime))
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                            var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            if ((targetSegment.IsValue)
                                && (rightSegment.IsValue))
                                return targetSegment.MergeValue(rightSegment, Convert.ToDateTime(targetSegment.Value).Subtract(Convert.ToDateTime(rightSegment.Value)));

                            var targetArgument = visitor.GetQuotedValue(targetSegment);
                            var rightArgument = visitor.GetQuotedValue(rightSegment);
                            return targetSegment.Merge(rightSegment, $"{targetArgument}-{rightArgument}");
                        });
                        result = true;
                    }
                    if (parameterInfos[0].ParameterType == typeof(TimeSpan))
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                            var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            if ((targetSegment.IsValue)
                                && (rightSegment.IsValue))
                                return targetSegment.MergeValue(rightSegment, Convert.ToDateTime(targetSegment.Value).Subtract((TimeSpan)rightSegment.Value));

                            var targetArgument = visitor.GetQuotedValue(targetSegment);
                            var rightArgument = visitor.GetQuotedValue(rightSegment);
                            return targetSegment.Merge(rightSegment, $"{targetArgument}-{rightArgument}");
                        });
                        result = true;
                    }
                    break;
                case "Equals":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                        var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(rightSegment, $"{targetArgument}={rightArgument}");
                    });
                    result = true;
                    break;
                case "CompareTo":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                        var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });

                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(rightSegment, $"CASE WHEN {targetArgument}={rightArgument} THEN 0 WHEN {targetArgument}>{rightArgument} THEN 1 ELSE -1 END");
                    });
                    result = true;
                    break;
                case "ToString":
                    if (parameterInfos.Length == 0)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                            if (targetSegment.IsValue)
                                return targetSegment.Change(targetSegment.Value.ToString());

                            return targetSegment.Change($"TO_CHAR({targetSegment.Value},'YYYY-MM-DD HH24:MI:SS.MS')", SqlType.MethodCall);
                        });
                        result = true;
                    }
                    if (parameterInfos.Length == 1)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                            var formatSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });

                            string formatArgument = null;
                            if formatSegment.IsValue
                            {
                                formatArgument = $"'{formatSegment.Value}'";

                                if (formatArgument.Contains("yyyy"))
                                    formatArgument = formatArgument.Replace("yyyy", "YYYY");
                                else if (formatArgument.Contains("yyy"))
                                    formatArgument = formatArgument.Replace("yyy", "YYY");
                                else if (formatArgument.Contains("yy"))
                                    formatArgument = formatArgument.Replace("yy", "YY");

                                if (formatArgument.Contains("MMMM"))
                                    formatArgument = formatArgument.Replace("MMMM", "Month");
                                else if (formatArgument.Contains("MMM"))
                                    formatArgument = formatArgument.Replace("MMM", "Mon");
                                else if (formatArgument.Contains("M") && !formatArgument.Contains("MM"))
                                    formatArgument = formatArgument.Replace("M", "FMMM");

                                if (formatArgument.Contains("dddd"))
                                    formatArgument = formatArgument.Replace("dddd", "Day");
                                else if (formatArgument.Contains("ddd"))
                                    formatArgument = formatArgument.Replace("ddd", "DY");
                                else if (formatArgument.Contains("dd"))
                                    formatArgument = formatArgument.Replace("dd", "DD");
                                else if (formatArgument.Contains("d"))
                                    formatArgument = formatArgument.Replace("d", "FMDD");

                                if (formatArgument.Contains("HH"))
                                    formatArgument = formatArgument.Replace("HH", "HH24");
                                else if (formatArgument.Contains("H"))
                                    formatArgument = formatArgument.Replace("H", "FMHH24");
                                else if (formatArgument.Contains("hh"))
                                    formatArgument = formatArgument.Replace("hh", "HH12");
                                else if (formatArgument.Contains("h"))
                                    formatArgument = formatArgument.Replace("h", "FMHH12");

                                if (formatArgument.Contains("mm"))
                                    formatArgument = formatArgument.Replace("mm", "MI");
                                else formatArgument = formatArgument.Replace("m", "FMMI");

                                if (formatArgument.Contains("ss"))
                                    formatArgument = formatArgument.Replace("ss", "SS");
                                else if (formatArgument.Contains("s"))
                                    formatArgument = formatArgument.Replace("s", "FMSS");

                                if (formatArgument.Contains("tt"))
                                    formatArgument = formatArgument.Replace("tt", "AM");
                                else if (formatArgument.Contains("t"))
                                    formatArgument = formatArgument.Replace("t", "AM");

                                if (formatArgument.Contains("FFFFFF"))
                                    formatArgument = formatArgument.Replace("FFFFFF", "US");
                                else if (formatArgument.Contains("FFFFF"))
                                    formatArgument = formatArgument.Replace("FFFFF", "FMUS");
                                else if (formatArgument.Contains("FFFF"))
                                    formatArgument = formatArgument.Replace("FFFF", "FMUS");
                                else if (formatArgument.Contains("ffffff"))
                                    formatArgument = formatArgument.Replace("ffffff", "US");
                                else if (formatArgument.Contains("fffff"))
                                    formatArgument = formatArgument.Replace("fffff", "FMUS");
                                else if (formatArgument.Contains("ffff"))
                                    formatArgument = formatArgument.Replace("ffff", "FMUS");

                                if (formatArgument.Contains("FFF"))
                                    formatArgument = formatArgument.Replace("FFF", "MS");
                                else if (formatArgument.Contains("FF"))
                                    formatArgument = formatArgument.Replace("FF", "FMMS");
                                else if (formatArgument.Contains("F"))
                                    formatArgument = formatArgument.Replace("F", "FMMS");
                                else if (formatArgument.Contains("fff"))
                                    formatArgument = formatArgument.Replace("fff", "MS");
                                else if (formatArgument.Contains("ff"))
                                    formatArgument = formatArgument.Replace("ff", "FMMS");
                                else if (formatArgument.Contains("f"))
                                    formatArgument = formatArgument.Replace("f", "FMMS");
                            }
                            else formatArgument = visitor.GetQuotedValue(formatSegment);

                            if ((targetSegment.IsValue)
                                && formatSegment.IsValue)
                                return targetSegment.MergeValue(formatSegment, ((DateTime)targetSegment.Value).ToString(formatSegment.Value.ToString()));

                            var targetArgument = visitor.GetQuotedValue(targetSegment);
                            return targetSegment.Merge(formatSegment, $"TO_CHAR({targetArgument},{formatArgument})", SqlType.MethodCall);
                        });
                        result = true;
                    }
                    break;
            }
        }
        return result;
    }
}