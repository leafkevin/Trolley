using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.PostgreSql;

partial class PostgreSqlProvider
{
    public override bool TryGetTimeSpanMemberAccessSqlFormatter(MemberExpression memberExpr, out Func<ISqlVisitor, SqlSegment, SqlSegment> formatter)
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
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) => target.ChangeValue(TimeSpan.MinValue, true));
                    result = true;
                    break;
                case "MaxValue":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) => target.ChangeValue(TimeSpan.MaxValue, true));
                    result = true;
                    break;
                case "Zero":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) => target.ChangeValue(TimeSpan.Zero, true));
                    result = true;
                    break;
            }
        }
        else
        {
            switch (memberInfo.Name)
            {
                case "Ticks":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        var targetSegment = visitor.Visit(target);
                        if (targetSegment.IsValue)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).Ticks);

                        return targetSegment.Change($"(EXTRACT(EPOCH FROM {targetSegment.Value})*10000000)::INT8");
                    });
                    result = true;
                    break;
                case "Days":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        var targetSegment = visitor.Visit(target);
                        if (targetSegment.IsValue)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).Days);

                        return targetSegment.Change($"EXTRACT(DAY FROM {targetSegment.Value})::INT4");
                    });
                    result = true;
                    break;
                case "Hours":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        var targetSegment = visitor.Visit(target);
                        if (targetSegment.IsValue)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).Hours);

                        return targetSegment.Change($"EXTRACT(HOUR FROM {targetSegment.Value})::INT4");
                    });
                    result = true;
                    break;
                case "Milliseconds":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        var targetSegment = visitor.Visit(target);
                        if (targetSegment.IsValue)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).Milliseconds);

                        return targetSegment.Change($"(EXTRACT(SECOND FROM {targetSegment.Value})*1000)::INT4");
                    });
                    result = true;
                    break;
                case "Minutes":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        var targetSegment = visitor.Visit(target);
                        if (targetSegment.IsValue)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).Minutes);

                        return targetSegment.Change($"EXTRACT(MINUTE FROM {targetSegment.Value})::INT4");
                    });
                    result = true;
                    break;
                case "Seconds":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        var targetSegment = visitor.Visit(target);
                        if (targetSegment.IsValue)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).Seconds);

                        return targetSegment.Change($"EXTRACT(SECOND FROM {targetSegment.Value})::INT4");
                    });
                    result = true;
                    break;
                case "TotalDays":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        var targetSegment = visitor.Visit(target);
                        if (targetSegment.IsValue)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).TotalDays);

                        return targetSegment.Change($"(EXTRACT(EPOCH FROM {targetSegment.Value})/{3600 * 24})::FLOAT8");
                    });
                    result = true;
                    break;
                case "TotalHours":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        var targetSegment = visitor.Visit(target);
                        if (targetSegment.IsValue)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).TotalHours);

                        return targetSegment.Change($"(EXTRACT(EPOCH FROM {targetSegment.Value})/3600)::FLOAT8");
                    });
                    result = true;
                    break;
                case "TotalMilliseconds":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        var targetSegment = visitor.Visit(target);
                        if (targetSegment.IsValue)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).TotalMilliseconds);

                        return targetSegment.Change($"(EXTRACT(EPOCH FROM {targetSegment.Value})*1000)::FLOAT8");
                    });
                    result = true;
                    break;
                case "TotalMinutes":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        var targetSegment = visitor.Visit(target);
                        if (targetSegment.IsValue)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).TotalMinutes);

                        return targetSegment.Change($"(EXTRACT(EPOCH FROM {targetSegment.Value})/60)::FLOAT8");
                    });
                    result = true;
                    break;
                case "TotalSeconds":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        var targetSegment = visitor.Visit(target);
                        if (targetSegment.IsValue)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).TotalSeconds);

                        return targetSegment.Change($"EXTRACT(EPOCH FROM {targetSegment.Value})::FLOAT8", SqlType.MethodCall);
                    });
                    result = true;
                    break;
            }
        }
        return result;
    }
    public override bool TryGetTimeSpanMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out Func<ISqlVisitor, MethodCallExpression, Stack<DeferredOperation>, SqlSegment> formatter)
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
                case "Equals":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var leftSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[1] });

                        var leftArgument = visitor.WrapSql(leftSegment);
                        var rightArgument = visitor.WrapSql(rightSegment);
                        return leftSegment.Change($"{leftArgument}={rightArgument}");
                    });
                    result = true;
                    break;
                case "FromDays":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var valueSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if (valueSegment.IsValue)
                            return valueSegment.Change(TimeSpan.FromDays(Convert.ToDouble(valueSegment.Value)));

                        return valueSegment.Change($"INTERVAL '1D'*{valueSegment.ToExprWrap()}");
                    });
                    result = true;
                    break;
                case "FromHours":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var valueSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if (valueSegment.IsValue)
                            return valueSegment.Change(TimeSpan.FromHours(Convert.ToDouble(valueSegment.Value)));

                        return valueSegment.Change($"INTERVAL '1H'*{valueSegment.ToExprWrap()}");
                    });
                    result = true;
                    break;
                case "FromMilliseconds":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var valueSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if (valueSegment.IsValue)
                            return valueSegment.Change(TimeSpan.FromMilliseconds(Convert.ToDouble(valueSegment.Value)));

                        return valueSegment.Change($"INTERVAL '1S'*{valueSegment.ToExprWrap()}/1000");
                    });
                    result = true;
                    break;
                case "FromMinutes":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var valueSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if (valueSegment.IsValue)
                            return valueSegment.Change(TimeSpan.FromMinutes(Convert.ToDouble(valueSegment.Value)));

                        return valueSegment.Change($"INTERVAL '1M'*{valueSegment.ToExprWrap()}");
                    });
                    result = true;
                    break;
                case "FromSeconds":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var valueSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if (valueSegment.IsValue)
                            return valueSegment.Change(TimeSpan.FromSeconds(Convert.ToDouble(valueSegment.Value)));

                        return valueSegment.Change($"INTERVAL '1S'*{valueSegment.ToExprWrap()}");
                    });
                    result = true;
                    break;
                case "FromTicks":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var valueSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if (valueSegment.IsValue)
                            return valueSegment.Change(TimeSpan.FromTicks(Convert.ToInt64(valueSegment.Value)));

                        return valueSegment.Change($"INTERVAL '1S'*{valueSegment.ToExprWrap()}/{TimeSpan.TicksPerSecond}");
                    });
                    result = true;
                    break;
                case "Parse":
                case "TryParse":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var valueSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if (valueSegment.IsValue)
                            return valueSegment.Change(TimeSpan.Parse(valueSegment.Value.ToString()));

                        return valueSegment.Change($"'{valueSegment.Value}'::INTERVAL");
                    });
                    result = true;
                    break;
                case "ParseExact":
                case "TryParseExact":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var valueSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        var formatSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[1] });
                        if (valueSegment.IsValue
                            && formatSegment.IsValue)
                            return valueSegment.Change(TimeSpan.ParseExact(valueSegment.Value.ToString(), formatSegment.Value.ToString(), CultureInfo.InvariantCulture));

                        string formatArgument = null;
                        if formatSegment.IsValue
                        {
                            formatArgument = $"'{formatSegment.Value}'";

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
                        else formatArgument = visitor.WrapSql(formatSegment);
                        return valueSegment.Change($"'{formatArgument}'::INTERVAL");
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
                case "Add":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                        var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });

                        if (targetSegment.IsValue
                            && (rightSegment.IsValue))
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).Add((TimeSpan)rightSegment.Value));

                        var targetArgument = visitor.WrapSql(targetSegment);
                        var rightArgument = visitor.WrapSql(rightSegment);
                        return targetSegment.Change($"{targetArgument}+{rightArgument}");
                    });
                    result = true;
                    break;
                case "Subtract":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                        var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if (targetSegment.IsValue
                            && (rightSegment.IsValue))
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).Subtract((TimeSpan)rightSegment.Value));

                        var targetArgument = visitor.WrapSql(targetSegment);
                        var rightArgument = visitor.WrapSql(rightSegment);
                        return targetSegment.Change($"{targetArgument}-{rightArgument}");
                    });
                    result = true;
                    break;
#if NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
                case "Multiply":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                        var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if (targetSegment.IsValue
                            && (rightSegment.IsValue))
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).Multiply((double)rightSegment.Value));

                        var targetArgument = visitor.WrapSql(targetSegment, true);
                        var rightArgument = visitor.WrapSql(rightSegment, true);
                        return targetSegment.Change($"{targetArgument}*{rightArgument}");
                    });
                    result = true;
                    break;
                case "Divide":
                    if (parameterInfos[0].ParameterType == typeof(double))
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                            var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            if (targetSegment.IsValue
                                && (rightSegment.IsValue))
                                return targetSegment.Change(((TimeSpan)targetSegment.Value).Divide((double)rightSegment.Value));

                            var targetArgument = visitor.WrapSql(targetSegment, true);
                            var rightArgument = visitor.WrapSql(rightSegment, true);
                            return targetSegment.Change($"{targetArgument}/{rightArgument}");
                        });
                        result = true;
                    }
                    if (parameterInfos[0].ParameterType == typeof(TimeSpan))
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                            var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            if (targetSegment.IsValue
                                && (rightSegment.IsValue))
                                return targetSegment.Change(((TimeSpan)targetSegment.Value).Divide((TimeSpan)rightSegment.Value));

                            var targetArgument = visitor.WrapSql(targetSegment, true);
                            var rightArgument = visitor.WrapSql(rightSegment, true);
                            return targetSegment.Change($"{targetArgument}/{rightArgument}");
                        });
                        result = true;
                    }
                    break;
#endif
            }
        }
        return result;
    }
}
