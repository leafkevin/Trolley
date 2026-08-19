using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.MySqlConnector;

partial class MySqlProvider
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
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) => target.Change(TimeSpan.MinValue, SqlType.Constant));
                    result = true;
                    break;
                case "MaxValue":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) => target.Change(TimeSpan.MaxValue, SqlType.Constant));
                    result = true;
                    break;
                case "Zero":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) => target.Change(TimeSpan.Zero, SqlType.Constant));
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

                        return targetSegment.Change($"TIME_TO_SEC({targetSegment.Value})*10000000", SqlType.Expression);
                    });
                    result = true;
                    break;
                case "Days":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        var targetSegment = visitor.Visit(target);
                        if (targetSegment.IsValue)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).Days);

                        throw new NotSupportedException("暂时不支持非常量、变量TimeSpan类型返回天数");
                    });
                    result = true;
                    break;
                case "Hours":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        var targetSegment = visitor.Visit(target);
                        if (targetSegment.IsValue)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).Hours);

                        return targetSegment.Change($"HOUR({targetSegment.Value})", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "Milliseconds":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        var targetSegment = visitor.Visit(target);
                        if (targetSegment.IsValue)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).Milliseconds);

                        return targetSegment.Change($"MICROSECOND({targetSegment.Value}) DIV 1000 MOD 1000", SqlType.Expression);
                    });
                    result = true;
                    break;
                case "Minutes":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        var targetSegment = visitor.Visit(target);
                        if (targetSegment.IsValue)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).Minutes);

                        return targetSegment.Change($"MINUTE({targetSegment.Value})", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "Seconds":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        var targetSegment = visitor.Visit(target);
                        if (targetSegment.IsValue)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).Seconds);

                        return targetSegment.Change($"SECOND({targetSegment.Value})", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "TotalDays":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        var targetSegment = visitor.Visit(target);
                        if (targetSegment.IsValue)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).TotalDays);

                        return targetSegment.Change($"TIME_TO_SEC({targetSegment.Value})/{3600 * 24}", SqlType.Expression);
                    });
                    result = true;
                    break;
                case "TotalHours":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        var targetSegment = visitor.Visit(target);
                        if (targetSegment.IsValue)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).TotalHours);

                        return targetSegment.Change($"TIME_TO_SEC({targetSegment.Value})/3600", SqlType.Expression);
                    });
                    result = true;
                    break;
                case "TotalMilliseconds":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        var targetSegment = visitor.Visit(target);
                        if (targetSegment.IsValue)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).TotalMilliseconds);

                        return targetSegment.Change($"TIME_TO_SEC({targetSegment.Value})*1000", SqlType.Expression);
                    });
                    result = true;
                    break;
                case "TotalMinutes":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        var targetSegment = visitor.Visit(target);
                        if (targetSegment.IsValue)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).TotalMinutes);

                        return targetSegment.Change($"TIME_TO_SEC({targetSegment.Value})/60", SqlType.Expression);
                    });
                    result = true;
                    break;
                case "TotalSeconds":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        var targetSegment = visitor.Visit(target);
                        if (targetSegment.IsValue)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).TotalSeconds);

                        return targetSegment.Change($"TIME_TO_SEC({targetSegment.Value})", SqlType.MethodCall);
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
                        return leftSegment.Change($"CASE WHEN {leftArgument}={rightArgument} THEN 0 WHEN {leftArgument}>{rightArgument} THEN 1 ELSE -1 END", SqlType.Expression);
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
                        return leftSegment.Change($"{leftArgument}={rightArgument}", SqlType.Expression);
                    });
                    result = true;
                    break;
                case "FromDays":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var valueSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if (valueSegment.IsValue)
                            return valueSegment.Change(TimeSpan.FromDays(Convert.ToDouble(valueSegment.Value)));

                        return valueSegment.Change($"ADDTIME('00:00:00',SEC_TO_TIME({visitor.WrapSql(valueSegment)}*{24 * 3600}))", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "FromHours":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var valueSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if (valueSegment.IsValue)
                            return valueSegment.Change(TimeSpan.FromHours(Convert.ToDouble(valueSegment.Value)));
                        return valueSegment.Change($"ADDTIME('00:00:00',SEC_TO_TIME({visitor.WrapSql(valueSegment)}*3600))", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "FromMilliseconds":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var valueSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if (valueSegment.IsValue)
                            return valueSegment.Change(TimeSpan.FromMilliseconds(Convert.ToDouble(valueSegment.Value)));
                        return valueSegment.Change($"ADDTIME('00:00:00',SEC_TO_TIME({visitor.WrapSql(valueSegment)}/1000))", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "FromMinutes":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var valueSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if (valueSegment.IsValue)
                            return valueSegment.Change(TimeSpan.FromMinutes(Convert.ToDouble(valueSegment.Value)));
                        return valueSegment.Change($"ADDTIME('00:00:00',SEC_TO_TIME({visitor.WrapSql(valueSegment)}*60))", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "FromSeconds":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var valueSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if (valueSegment.IsValue)
                            return valueSegment.Change(TimeSpan.FromSeconds(Convert.ToDouble(valueSegment.Value)));
                        return valueSegment.Change($"ADDTIME('00:00:00',SEC_TO_TIME({visitor.WrapSql(valueSegment)}))", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "FromTicks":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var valueSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if (valueSegment.IsValue)
                            return valueSegment.Change(TimeSpan.FromTicks(Convert.ToInt64(valueSegment.Value)));
                        return valueSegment.Change($"ADDTIME('00:00:00',SEC_TO_TIME({visitor.WrapSql(valueSegment)}/{TimeSpan.TicksPerSecond}))", SqlType.MethodCall);
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
                        return valueSegment.Change($"CAST({valueSegment.Value} AS TIME)", SqlType.MethodCall);
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

                        return valueSegment.Change($"CAST({valueSegment.Value} AS TIME)", SqlType.MethodCall);
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
                        return targetSegment.Change($"CASE WHEN {targetArgument}={rightArgument} THEN 0 WHEN {targetArgument}>{rightArgument} THEN 1 ELSE -1 END", SqlType.Expression);
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
                        return targetSegment.Change($"{targetArgument}={rightArgument}", SqlType.Expression);
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

                        var rightArgument = visitor.WrapSql(rightSegment);
                        return targetSegment.Change($"ADDTIME({targetArgument},{rightArgument})", SqlType.MethodCall);
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
                        return targetSegment.Change($"TIMEDIFF({targetArgument},{rightArgument})", SqlType.MethodCall);
                    });
                    result = true;
                    break;
#if NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
                case "Multiply":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                        var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if (targetSegment.IsValue && rightSegment.IsValue)
                            return targetSegment.Change(((TimeSpan)targetSegment.Value).Multiply((double)rightSegment.Value));

                        var targetArgument = visitor.WrapSql(targetSegment, true);
                        var rightArgument = visitor.WrapSql(rightSegment, true);
                        return targetSegment.Change($"{targetArgument}*{rightArgument}", SqlType.Expression);
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
                            return targetSegment.Change($"{targetArgument}/{rightArgument}", SqlType.Expression);
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
                                return targetSegment.Change(((TimeSpan)targetSegment.Value).Divide((TimeSpan)rightSegment.Value));

                            var targetArgument = visitor.WrapSql(targetSegment, true);
                            var rightArgument = visitor.WrapSql(rightSegment, true);
                            return targetSegment.Change($"{targetArgument}/{rightArgument}", SqlType.Expression);
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