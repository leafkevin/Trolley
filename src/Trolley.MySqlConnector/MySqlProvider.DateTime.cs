using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.MySqlConnector;

partial class MySqlProvider
{
#if NETSTANDARD2_0
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
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) => target.Change(DateTime.MinValue, SqlType.Constant));
                    result = true;
                    break;
                case "MaxValue":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) => target.Change(DateTime.MaxValue, SqlType.Constant));
                    result = true;
                    break;
                case "UnixEpoch":
#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) => target.Change(DateTime.UnixEpoch, SqlType.Constant));
#else
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) => target.Change(UnixEpoch, SqlType.Constant));
#endif
                    result = true;
                    break;
                case "Today":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) => target.Change("CURDATE()", SqlType.MethodCall));
                    result = true;
                    break;
                case "Now":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) => target.Change("NOW()", SqlType.MethodCall));
                    result = true;
                    break;
                case "UtcNow":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) => target.Change("UTC_TIMESTAMP()", SqlType.MethodCall));
                    result = true;
                    break;
            }
        }
        else
        {
            switch (memberInfo.Name)
            {
                case "Date":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        SqlSegment targetSegment = default;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.Visit(target);

                        if (targetSegment.IsValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Date);

                        return targetSegment.Change($"CONVERT({targetSegment.Value},DATE)", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "Day":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        SqlSegment targetSegment = default;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.Visit(target);

                        if (targetSegment.IsValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Day);

                        return targetSegment.Change($"DAYOFMONTH({targetSegment.Value})", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "DayOfWeek":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        SqlSegment targetSegment = default;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.Visit(target);

                        if (targetSegment.IsValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).DayOfWeek);

                        return targetSegment.Change($"DAYOFWEEK({targetSegment.Value})-1");
                    });
                    result = true;
                    break;
                case "DayOfYear":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        SqlSegment targetSegment = default;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.Visit(target);

                        if (targetSegment.IsValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).DayOfYear);

                        return targetSegment.Change($"DAYOFYEAR({targetSegment.Value})", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "Hour":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        SqlSegment targetSegment = default;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.Visit(target);

                        if (targetSegment.IsValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Hour);

                        return targetSegment.Change($"HOUR({targetSegment.Value})", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "Kind":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
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
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        SqlSegment targetSegment = default;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.Visit(target);

                        if (targetSegment.IsValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Millisecond);

                        return targetSegment.Change($"FLOOR(MICROSECOND({targetSegment.Value})/1000)", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "Minute":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        SqlSegment targetSegment = default;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.Visit(target);

                        if (targetSegment.IsValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Minute);

                        return targetSegment.Change($"MINUTE({targetSegment.Value})", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "Month":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        SqlSegment targetSegment = default;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.Visit(target);

                        if (targetSegment.IsValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Month);

                        return targetSegment.Change($"MONTH({targetSegment.Value})", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "Second":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        SqlSegment targetSegment = default;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.Visit(target);

                        if (targetSegment.IsValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Second);

                        return targetSegment.Change($"SECOND({targetSegment.Value})", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "Ticks":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        SqlSegment targetSegment = default;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.Visit(target);

                        if (targetSegment.IsValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Ticks);

                        return targetSegment.Change($"TIMESTAMPDIFF(MICROSECOND,'0001-01-01',{targetSegment.Value})*10");
                    });
                    result = true;
                    break;
                case "TimeOfDay":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        SqlSegment targetSegment = default;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.Visit(target);

                        if (targetSegment.IsValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).TimeOfDay);

                        return targetSegment.Change($"CONVERT({targetSegment.Value},TIME)", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "Year":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        SqlSegment targetSegment = default;
                        if (target.Expression is MemberExpression memberExpr && memberExpr.Expression == null
                            && TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, target);
                        else targetSegment = visitor.Visit(target);

                        if (targetSegment.IsValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Year);

                        return targetSegment.Change($"YEAR({targetSegment.Value})", SqlType.MethodCall);
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
                        if (leftSegment.IsValue && rightSegment.IsValue)
                            return leftSegment.Change(DateTime.DaysInMonth(Convert.ToInt32(leftSegment.Value), Convert.ToInt32(rightSegment.Value)));

                        var leftArgument = visitor.WrapSql(leftSegment);
                        var rightArgument = visitor.WrapSql(rightSegment);
                        return leftSegment.Change($"DAYOFMONTH(LAST_DAY(CONCAT({leftArgument},'-',{rightArgument},'-01')))", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "IsLeapYear":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var valueSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if (valueSegment.IsValue)
                            return valueSegment.Change(DateTime.IsLeapYear(Convert.ToInt32(valueSegment.Value)));

                        var valueArgument = visitor.WrapSql(valueSegment);
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
                            var styleSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[2] });

                            if (valueSegment.IsValue && providerSegment.IsValue && styleSegment.IsValue)
                                return valueSegment.Change(DateTime.Parse(valueSegment.Value.ToString(), (IFormatProvider)providerSegment.Value, (DateTimeStyles)styleSegment.Value));

                            return valueSegment.Change($"CAST({valueSegment.Value} AS DATETIME)", SqlType.MethodCall);
                        });
                    }
                    else if (parameterInfos.Length == 2)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var valueSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            var providerSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[1] });

                            if (valueSegment.IsValue && providerSegment.IsValue)
                                return valueSegment.Change(DateTime.Parse(valueSegment.Value.ToString(), (IFormatProvider)providerSegment.Value));

                            return valueSegment.Change($"CAST({valueSegment.Value} AS DATETIME)", SqlType.MethodCall);
                        });
                    }
                    else
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var valueSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            if (valueSegment.IsValue)
                                return valueSegment.Change(DateTime.Parse(valueSegment.Value.ToString()));

                            return valueSegment.Change($"CAST({valueSegment.Value} AS DATETIME)", SqlType.MethodCall);
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
                        var providerSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[2] });

                        if (valueSegment.IsValue && formatSegment.IsValue && providerSegment.IsValue)
                            return valueSegment.Change(DateTime.ParseExact(valueSegment.Value.ToString(), formatSegment.Value.ToString(), (IFormatProvider)providerSegment.Value));

                        string formatArgument = null;
                        if (formatSegment.IsValue)
                        {
                            formatArgument = $"'{formatSegment.Value}'";

                            if (formatArgument.Contains("mm"))
                                formatArgument = formatArgument.Replace("mm", "%i");
                            else formatArgument = formatArgument.Replace("m", "%i");

                            if (formatArgument.Contains("yyyy"))
                                formatArgument = formatArgument.Replace("yyyy", "%Y");
                            else if (formatArgument.Contains("yyy"))
                                formatArgument = formatArgument.Replace("yyy", "%Y");
                            else if (formatArgument.Contains("yy"))
                                formatArgument = formatArgument.Replace("yy", "%y");

                            if (formatArgument.Contains("MMMM"))
                                formatArgument = formatArgument.Replace("MMMM", "%M");
                            else if (formatArgument.Contains("MMM"))
                                formatArgument = formatArgument.Replace("MMM", "%b");
                            else if (formatArgument.Contains("MM"))
                                formatArgument = formatArgument.Replace("MM", "%m");
                            else if (formatArgument.Contains("M"))
                                formatArgument = formatArgument.Replace("M", "%c");

                            if (formatArgument.Contains("dddd"))
                                formatArgument = formatArgument.Replace("dddd", "%W");
                            else if (formatArgument.Contains("ddd"))
                                formatArgument = formatArgument.Replace("ddd", "%a");
                            else if (formatArgument.Contains("dd"))
                                formatArgument = formatArgument.Replace("dd", "%d");
                            else if (formatArgument.Contains("d"))
                                formatArgument = formatArgument.Replace("d", "%e");

                            if (formatArgument.Contains("HH"))
                                formatArgument = formatArgument.Replace("HH", "%H");
                            else if (formatArgument.Contains("H"))
                                formatArgument = formatArgument.Replace("H", "%k");
                            else if (formatArgument.Contains("hh"))
                                formatArgument = formatArgument.Replace("hh", "%h");
                            else if (formatArgument.Contains("h"))
                                formatArgument = formatArgument.Replace("h", "%l");

                            if (formatArgument.Contains("ss"))
                                formatArgument = formatArgument.Replace("ss", "%s");
                            else if (formatArgument.Contains("s"))
                                formatArgument = formatArgument.Replace("s", "%s");

                            if (formatArgument.Contains("tt"))
                                formatArgument = formatArgument.Replace("tt", "%p");
                            else if (formatArgument.Contains("t"))
                                formatArgument = formatArgument.Replace("t", "%p");
                        }
                        else formatArgument = visitor.WrapSql(formatSegment);
                        var valueArgument = visitor.WrapSql(valueSegment);
                        return valueSegment.Change($"STR_TO_DATE({valueArgument},{formatArgument})", SqlType.MethodCall);
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

                        var leftArgument = visitor.WrapSql(leftSegment);
                        var rightArgument = visitor.WrapSql(rightSegment);
                        return leftSegment.Change($"CASE WHEN {leftArgument}={rightArgument} THEN 0 WHEN {leftArgument}>{rightArgument} THEN 1 ELSE -1 END");
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
                        if (targetSegment.IsValue && rightSegment.IsValue)
                            return targetSegment.Change(Convert.ToDateTime(targetSegment.Value).Add((TimeSpan)rightSegment.Value));

                        var targetArgument = visitor.WrapSql(targetSegment);
                        if (rightSegment.IsValue)
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
                            return targetSegment.Change(builder.ToString(), SqlType.MethodCall);
                        }
                        //非常量、变量的，只能小于一天,数据库的Time类型映射成TimeSpan
                        var rightArgument = visitor.WrapSql(rightSegment);
                        return targetSegment.Change($"ADDTIME({targetArgument},{rightArgument})", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "AddDays":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                        var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if (targetSegment.IsValue && rightSegment.IsValue)
                            return targetSegment.Change(Convert.ToDateTime(targetSegment.Value).AddDays(Convert.ToDouble(rightSegment.Value)));

                        var targetArgument = visitor.WrapSql(targetSegment);
                        var rightArgument = visitor.WrapSql(rightSegment);
                        return targetSegment.Change($"DATE_ADD({targetArgument},INTERVAL {rightArgument} DAY)", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "AddHours":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                        var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if (targetSegment.IsValue
                           && (rightSegment.IsValue))
                            return targetSegment.Change(Convert.ToDateTime(targetSegment.Value).AddHours(Convert.ToDouble(rightSegment.Value)));

                        var targetArgument = visitor.WrapSql(targetSegment);
                        var rightArgument = visitor.WrapSql(rightSegment);
                        return targetSegment.Change($"DATE_ADD({targetArgument},INTERVAL {rightArgument} HOUR)", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "AddMilliseconds":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                        var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if (targetSegment.IsValue
                            && (rightSegment.IsValue))
                            return targetSegment.Change(Convert.ToDateTime(targetSegment.Value).AddMilliseconds(Convert.ToDouble(rightSegment.Value)));

                        var targetArgument = visitor.WrapSql(targetSegment);
                        var rightArgument = visitor.WrapSql(rightSegment, true);
                        return targetSegment.Change($"DATE_ADD({targetArgument},INTERVAL {rightArgument}*1000 MICROSECOND)", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "AddMinutes":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                        var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if (targetSegment.IsValue
                            && (rightSegment.IsValue))
                            return targetSegment.Change(Convert.ToDateTime(targetSegment.Value).AddMinutes(Convert.ToDouble(rightSegment.Value)));

                        var targetArgument = visitor.WrapSql(targetSegment);
                        var rightArgument = visitor.WrapSql(rightSegment);
                        return targetSegment.Change($"DATE_ADD({targetArgument},INTERVAL {rightArgument} MINUTE)", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "AddMonths":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                        var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if (targetSegment.IsValue
                            && (rightSegment.IsValue))
                            return targetSegment.Change(Convert.ToDateTime(targetSegment.Value).AddMonths(Convert.ToInt32(rightSegment.Value)));

                        var targetArgument = visitor.WrapSql(targetSegment);
                        var rightArgument = visitor.WrapSql(rightSegment);
                        return targetSegment.Change($"DATE_ADD({targetArgument},INTERVAL {rightArgument} MONTH)", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "AddSeconds":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                        var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if (targetSegment.IsValue && rightSegment.IsValue)
                            return targetSegment.Change(Convert.ToDateTime(targetSegment.Value).AddSeconds(Convert.ToDouble(rightSegment.Value)));

                        var targetArgument = visitor.WrapSql(targetSegment);
                        var rightArgument = visitor.WrapSql(rightSegment);
                        return targetSegment.Change($"DATE_ADD({targetArgument},INTERVAL {rightArgument} SECOND)", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "AddTicks":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                        var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if (targetSegment.IsValue && rightSegment.IsValue)
                            return targetSegment.Change(Convert.ToDateTime(targetSegment.Value).AddTicks(Convert.ToInt64(rightSegment.Value)));

                        var targetArgument = visitor.WrapSql(targetSegment);
                        var rightArgument = visitor.WrapSql(rightSegment, true);
                        return targetSegment.Change($"DATE_ADD({targetArgument},INTERVAL {rightArgument}/10 MICROSECOND)", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "AddYears":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                        var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if (targetSegment.IsValue && rightSegment.IsValue)
                            return targetSegment.Change(Convert.ToDateTime(targetSegment.Value).AddYears(Convert.ToInt32(rightSegment.Value)));

                        var targetArgument = visitor.WrapSql(targetSegment);
                        var rightArgument = visitor.WrapSql(rightSegment);
                        return targetSegment.Change($"DATE_ADD({targetArgument},INTERVAL {rightArgument} YEAR)", SqlType.MethodCall);
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
                            if (targetSegment.IsValue && rightSegment.IsValue)
                                return targetSegment.Change(Convert.ToDateTime(targetSegment.Value).Subtract(Convert.ToDateTime(rightSegment.Value)));

                            var targetArgument = visitor.WrapSql(targetSegment);
                            var rightArgument = visitor.WrapSql(rightSegment);
                            return targetSegment.Change($"TIMEDIFF({targetArgument},{rightArgument})", SqlType.MethodCall);
                        });
                        result = true;
                    }
                    if (parameterInfos[0].ParameterType == typeof(TimeSpan))
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                            var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            if (targetSegment.IsValue && rightSegment.IsValue)
                                return targetSegment.Change(Convert.ToDateTime(targetSegment.Value).Subtract((TimeSpan)rightSegment.Value));

                            var targetArgument = visitor.WrapSql(targetSegment);
                            if (rightSegment.IsValue)
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
                                return targetSegment.Change(builder.ToString(), SqlType.MethodCall);
                            }
                            //非常量、变量的，只能小于一天,数据库的Time类型映射成TimeSpan
                            var rightArgument = visitor.WrapSql(rightSegment);
                            return targetSegment.Change($"SUBTIME({targetArgument},{rightArgument})", SqlType.MethodCall);
                        });
                        result = true;
                    }
                    break;
                case "Equals":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                        var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        var targetArgument = visitor.WrapSql(targetSegment);
                        var rightArgument = visitor.WrapSql(rightSegment);
                        return targetSegment.Change($"{targetArgument}={rightArgument}");
                    });
                    result = true;
                    break;
                case "CompareTo":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                        var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        var targetArgument = visitor.WrapSql(targetSegment);
                        var rightArgument = visitor.WrapSql(rightSegment);
                        return targetSegment.Change($"CASE WHEN {targetArgument}={rightArgument} THEN 0 WHEN {targetArgument}>{rightArgument} THEN 1 ELSE -1 END");
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

                            return targetSegment.Change($"DATE_FORMAT({targetSegment.Value},'%Y-%m-%d %H:%i:%s')", SqlType.MethodCall);
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
                            if (formatSegment.IsValue)
                            {
                                formatArgument = $"'{formatSegment.Value}'";

                                //分钟
                                if (formatArgument.Contains("mm"))
                                    formatArgument = formatArgument.Replace("mm", "%i");
                                else formatArgument = formatArgument.Replace("m", "%i");

                                if (formatArgument.Contains("yyyy"))
                                    formatArgument = formatArgument.Replace("yyyy", "%Y");
                                else if (formatArgument.Contains("yyy"))
                                    formatArgument = formatArgument.Replace("yyy", "%Y");
                                else if (formatArgument.Contains("yy"))
                                    formatArgument = formatArgument.Replace("yy", "%y");

                                if (formatArgument.Contains("MMMM"))
                                    formatArgument = formatArgument.Replace("MMMM", "%M");
                                else if (formatArgument.Contains("MMM"))
                                    formatArgument = formatArgument.Replace("MMM", "%b");
                                else if (formatArgument.Contains("MM"))
                                    formatArgument = formatArgument.Replace("MM", "%m");
                                else if (formatArgument.Contains("M"))
                                    formatArgument = formatArgument.Replace("M", "%c");

                                if (formatArgument.Contains("dddd"))
                                    formatArgument = formatArgument.Replace("dddd", "%W");
                                else if (formatArgument.Contains("ddd"))
                                    formatArgument = formatArgument.Replace("ddd", "%a");
                                else if (formatArgument.Contains("dd"))
                                    formatArgument = formatArgument.Replace("dd", "%d");
                                else if (formatArgument.Contains("d"))
                                    formatArgument = formatArgument.Replace("d", "%e");

                                if (formatArgument.Contains("HH"))
                                    formatArgument = formatArgument.Replace("HH", "%H");
                                else if (formatArgument.Contains("H"))
                                    formatArgument = formatArgument.Replace("H", "%k");
                                else if (formatArgument.Contains("hh"))
                                    formatArgument = formatArgument.Replace("hh", "%h");
                                else if (formatArgument.Contains("h"))
                                    formatArgument = formatArgument.Replace("h", "%l");

                                if (formatArgument.Contains("ss"))
                                    formatArgument = formatArgument.Replace("ss", "%s");
                                else if (formatArgument.Contains("s"))
                                    formatArgument = formatArgument.Replace("s", "%s");

                                if (formatArgument.Contains("tt"))
                                    formatArgument = formatArgument.Replace("tt", "%p");
                                else if (formatArgument.Contains("t"))
                                    formatArgument = formatArgument.Replace("t", "%p");
                            }
                            else formatArgument = visitor.WrapSql(formatSegment);

                            if (targetSegment.IsValue && formatSegment.IsValue)
                                return targetSegment.Change(((DateTime)targetSegment.Value).ToString(formatSegment.Value.ToString()));

                            var targetArgument = visitor.WrapSql(targetSegment);
                            return targetSegment.Change($"DATE_FORMAT({targetArgument},{formatArgument})", SqlType.MethodCall);
                        });
                        result = true;
                    }
                    break;
            }
        }
        return result;
    }
}