using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Trolley.MySqlConnector;

partial class MySqlProvider
{
    public override bool TryGetTimeOnlyMemberAccessSqlFormatter(MemberExpression memberExpr, out Func<ISqlVisitor, SqlSegment, SqlSegment> formatter)
    {
        bool result = false;
        formatter = null;
#if NET6_0_OR_GREATER
        var memberInfo = memberExpr.Member;
        var cacheKey = HashCode.Combine(memberInfo.DeclaringType, memberInfo);
        if (memberExpr.Expression == null)
        {
            switch (memberInfo.Name)
            {
                case "MinValue":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) => target.Change(TimeOnly.MinValue, SqlType.Constant));
                    result = true;
                    break;
                case "MaxValue":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) => target.Change(TimeOnly.MaxValue, SqlType.Constant));
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
                            return targetSegment.Change(((TimeOnly)targetSegment.Value).Ticks);

                        return targetSegment.Change($"TIME_TO_SEC({targetSegment.Value})*10000000");
                    });
                    result = true;
                    break;
                case "Hour":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        var targetSegment = visitor.Visit(target);
                        if (targetSegment.IsValue)
                            return targetSegment.Change(((TimeOnly)targetSegment.Value).Hour);

                        return targetSegment.Change($"HOUR({targetSegment.Value})", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "Millisecond":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        var targetSegment = visitor.Visit(target);
                        if (targetSegment.IsValue)
                            return targetSegment.Change(((TimeOnly)targetSegment.Value).Millisecond);

                        return targetSegment.Change($"MICROSECOND({targetSegment.Value}) DIV 1000 MOD 1000");
                    });
                    result = true;
                    break;
                case "Minute":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        var targetSegment = visitor.Visit(target);
                        if (targetSegment.IsValue)
                            return targetSegment.Change(((TimeOnly)targetSegment.Value).Minute);

                        return targetSegment.Change($"MINUTE({targetSegment.Value})", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "Second":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        var targetSegment = visitor.Visit(target);
                        if (targetSegment.IsValue)
                            return targetSegment.Change(((TimeOnly)targetSegment.Value).Second);

                        return targetSegment.Change($"SECOND({targetSegment.Value})", SqlType.MethodCall);
                    });
                    result = true;
                    break;
            }
        }
#endif
        return result;
    }
    public override bool TryGetTimeOnlyMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out Func<ISqlVisitor, MethodCallExpression, Stack<DeferredOperation>, SqlSegment> formatter)
    {
        var result = false;
        formatter = null;
#if NET6_0_OR_GREATER
        var methodInfo = methodCallExpr.Method;
        var parameterInfos = methodInfo.GetParameters();
        var cacheKey = HashCode.Combine(methodInfo.DeclaringType, methodInfo);
        if (methodInfo.IsStatic)
        {
            switch (methodInfo.Name)
            {
                case "FromTimeSpan":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var valueSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if (valueSegment.IsValue)
                            return valueSegment.Change(TimeOnly.FromTimeSpan((TimeSpan)valueSegment.Value));

                        return valueSegment.Change($"TIME({valueSegment.Value})", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "FromDateTime":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var valueSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if (valueSegment.IsValue)
                            return valueSegment.Change(TimeOnly.FromDateTime((DateTime)valueSegment.Value));

                        return valueSegment.Change($"TIME({valueSegment.Value})", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "Parse":
                case "TryParse":
                    if (parameterInfos.Length >= 1 && parameterInfos[0].ParameterType == typeof(string))
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var valueSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            if (valueSegment.IsValue)
                                return valueSegment.Change(TimeOnly.Parse(valueSegment.Value.ToString()));

                            return valueSegment.Change($"TIME({valueSegment.Value})", SqlType.MethodCall);
                        });
                        result = true;
                    }
                    break;
                case "ParseExact":
                case "TryParseExact":
                    if (parameterInfos.Length >= 2 && parameterInfos[0].ParameterType == typeof(string) && parameterInfos[1].ParameterType == typeof(string))
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var valueSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            var formatSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[1] });
                            if (valueSegment.IsValue
                                && formatSegment.IsValue)
                                return valueSegment.Change(TimeOnly.ParseExact(valueSegment.Value.ToString(), formatSegment.Value.ToString()));

                            var valueArgument = visitor.WrapSql(valueSegment);
                            return valueSegment.Change($"TIME({valueArgument})", SqlType.MethodCall);
                        });
                        result = true;
                    }
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
                        if (targetSegment.IsValue
                            && (rightSegment.IsValue))
                            return targetSegment.Change(((TimeOnly)targetSegment.Value).Add((TimeSpan)rightSegment.Value));

                        var targetArgument = visitor.WrapSql(targetSegment);
                        var rightArgument = visitor.WrapSql(rightSegment);
                        return targetSegment.Change($"ADDTIME({targetArgument},{rightArgument})", SqlType.MethodCall);
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
                            return targetSegment.Change(((TimeOnly)targetSegment.Value).AddHours((double)rightSegment.Value));

                        var targetArgument = visitor.WrapSql(targetSegment);
                        var rightArgument = visitor.WrapSql(rightSegment);
                        return targetSegment.Change($"TIME_ADD({targetArgument},INTERVAL {rightArgument} HOUR)", SqlType.MethodCall);
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
                            return targetSegment.Change(((TimeOnly)targetSegment.Value).AddMinutes((double)rightSegment.Value));

                        var targetArgument = visitor.WrapSql(targetSegment);
                        var rightArgument = visitor.WrapSql(rightSegment);
                        return targetSegment.Change($"TIME_ADD({targetArgument},INTERVAL {rightArgument} MINUTE)", SqlType.MethodCall);
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
                        return targetSegment.Change($"CASE WHEN ({targetArgument}={rightArgument} THEN 0 WHEN {targetArgument}>{rightArgument} THEN 1 ELSE -1 END");
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
                case "ToTimeSpan":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                        if (targetSegment.IsConstant && targetSegment.IsVariable)
                            return targetSegment.Change(((TimeOnly)targetSegment.Value).ToTimeSpan());

                        return targetSegment.Change(this.CastTo(typeof(TimeSpan), targetSegment.Value), SqlType.MethodCall);
                    });
                    result = true;
                    break;
            }
        }
#endif
        return result;
    }
}
