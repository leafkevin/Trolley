#if NET6_0_OR_GREATER
using System;
using System.Globalization;
using System.Linq;
#endif
using System.Linq.Expressions;

namespace Trolley.SqlServer;

partial class SqlServerProvider
{
    public override bool TryGetDateOnlyMemberAccessSqlFormatter(MemberExpression memberExpr, out Func<ISqlVisitor, SqlSegment, SqlSegment> formatter)
    {
        bool result = false;
        formatter = null;
#if NET6_0_OR_GREATER
        var memberInfo = memberExpr.Member;
        var cacheKey = RepositoryHelper.GetCacheKey(memberInfo.DeclaringType, memberInfo);
        if (memberExpr.Expression == null)
        {
            switch (memberInfo.Name)
            {
                //静态成员访问，理论上没有target对象，为了不再创建sqlSegment对象，外层直接把对象传了进来
                case "MinValue":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) => target.ChangeValue(DateOnly.MinValue, true));
                    result = true;
                    break;
                case "MaxValue":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) => target.ChangeValue(DateOnly.MaxValue, true));
                    result = true;
                    break;
            }
        }
        else
        {
            switch (memberInfo.Name)
            {
                case "Day":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        SqlSegment targetSegment = default;
                        if (target.Expression is MethodCallExpression callExpr && callExpr.Object == null
                            && TryGetDateOnlyMethodCallSqlFormatter(callExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, callExpr,  null);
                        else targetSegment = visitor.Visit(target);

                        if (targetSegment.IsValue)
                            return targetSegment.Change(((DateOnly)targetSegment.Value).Day);

                        return targetSegment.Change($"DATEPART(DAY,{targetSegment.Value})", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "DayOfWeek":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        SqlSegment targetSegment = default;
                        if (target.Expression is MethodCallExpression callExpr && callExpr.Object == null
                            && TryGetDateOnlyMethodCallSqlFormatter(callExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, callExpr,  null);
                        else targetSegment = visitor.Visit(target);

                        if (targetSegment.IsValue)
                            return targetSegment.Change(((DateOnly)targetSegment.Value).DayOfWeek);

                        return targetSegment.Change($"DATEPART(WEEKDAY,{targetSegment.Value})-1");
                    });
                    result = true;
                    break;
                case "DayOfYear":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        SqlSegment targetSegment = default;
                        if (target.Expression is MethodCallExpression callExpr && callExpr.Object == null
                            && TryGetDateOnlyMethodCallSqlFormatter(callExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, callExpr,  null);
                        else targetSegment = visitor.Visit(target);

                        if (targetSegment.IsValue)
                            return targetSegment.Change(((DateOnly)targetSegment.Value).DayOfYear);

                        return targetSegment.Change($"DATEPART(DAYOFYEAR,{targetSegment.Value})", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "Month":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        SqlSegment targetSegment = default;
                        if (target.Expression is MethodCallExpression callExpr && callExpr.Object == null
                            && TryGetDateOnlyMethodCallSqlFormatter(callExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, callExpr,  null);
                        else targetSegment = visitor.Visit(target);

                        if (targetSegment.IsValue)
                            return targetSegment.Change(((DateOnly)targetSegment.Value).Month);

                        return targetSegment.Change($"DATEPART(MONTH,{targetSegment.Value})", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "Year":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        SqlSegment targetSegment = default;
                        if (target.Expression is MethodCallExpression callExpr && callExpr.Object == null
                            && TryGetDateOnlyMethodCallSqlFormatter(callExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, callExpr,  null);
                        else targetSegment = visitor.Visit(target);

                        if (targetSegment.IsValue)
                            return targetSegment.Change(((DateOnly)targetSegment.Value).Year);

                        return targetSegment.Change($"DATEPART(YEAR,{targetSegment.Value})", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "DayNumber":
                    formatter = memberAccessSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, target) =>
                    {
                        SqlSegment targetSegment = default;
                        if (target.Expression is MethodCallExpression callExpr && callExpr.Object == null
                            && TryGetDateOnlyMethodCallSqlFormatter(callExpr, out var exprFormatter))
                            targetSegment = exprFormatter.Invoke(visitor, callExpr,  null);
                        else targetSegment = visitor.Visit(target);

                        if (targetSegment.IsValue)
                            return targetSegment.Change(((DateOnly)targetSegment.Value).DayNumber);

                        return targetSegment.Change($"DATEDIFF(DAY,'0001-01-01',{targetSegment.Value})", SqlType.MethodCall);
                    });
                    result = true;
                    break;
            }
        }
#endif
        return result;
    }
    public override bool TryGetDateOnlyMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out Func<ISqlVisitor, MethodCallExpression, Stack<DeferredOperation>, SqlSegment> formatter)
    {
        var result = false;
        formatter = null;
#if NET6_0_OR_GREATER
        var methodInfo = methodCallExpr.Method;
        var parameterInfos = methodInfo.GetParameters();
        var cacheKey = RepositoryHelper.GetCacheKey(methodInfo.DeclaringType, methodInfo);
        if (methodInfo.IsStatic)
        {
            switch (methodInfo.Name)
            {
                case "FromDateTime":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var valueSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if (valueSegment.IsValue)
                            return valueSegment.Change(DateOnly.FromDateTime((DateTime)valueSegment.Value));

                        return valueSegment.Change($"CAST({valueSegment.Value} AS DATE)", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "FromDayNumber":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var valueSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if (valueSegment.IsValue)
                            return valueSegment.Change(DateOnly.FromDayNumber(Convert.ToInt32(valueSegment.Value)));

                        return valueSegment.Change($"CAST(DATEADD(DAY,{valueSegment.Value},'0001-01-01') AS DATE)", SqlType.MethodCall);
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

                            if (valueSegment.IsValue
                                && providerSegment.IsValue
                                &&styleSegment.IsValue)
                                return valueSegment.Change(DateOnly.Parse(valueSegment.Value.ToString(), (IFormatProvider)providerSegment.Value, (DateTimeStyles)styleSegment.Value));

                            return valueSegment.Change($"CAST({valueSegment.Value} AS DATE)", SqlType.MethodCall);
                        });
                    }
                    else if (parameterInfos.Length == 2)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var valueSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            var providerSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[1] });

                            if (valueSegment.IsValue
                                && providerSegment.IsValue)
                                return valueSegment.Change(DateOnly.Parse(valueSegment.Value.ToString(), (IFormatProvider)providerSegment.Value));

                            return valueSegment.Change($"CAST({valueSegment.Value} AS DATE)", SqlType.MethodCall);
                        });
                    }
                    else
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var valueSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            if (valueSegment.IsValue)
                                return valueSegment.Change(DateOnly.Parse(valueSegment.Value.ToString()));

                            return valueSegment.Change($"CAST({valueSegment.Value} AS DATE)", SqlType.MethodCall);
                        });
                    }
                    result = true;
                    if (methodInfo.IsStatic && parameterInfos.Length >= 3 && parameterInfos[0].ParameterType == typeof(ReadOnlySpan<char>))
                        throw new NotSupportedException("DateOnly.Parse方法暂时不支持ReadOnlySpan<char>类型参数的解析，请转换成String类型");
                    break;
                case "ParseExact":
                case "TryParseExact":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var valueSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        var formatSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[1] });
                        var providerSegment = visitor.Visit(new SqlSegment { Expression = args[2] });

                        if (valueSegment.IsValue
                            && formatSegment.IsValue
                            && providerSegment.IsValue)
                            return valueSegment.Change(DateOnly.ParseExact(valueSegment.Value.ToString(), formatSegment.Value.ToString(), (IFormatProvider)providerSegment.Value));

                        if (!formatSegment.IsValue)
                            throw new NotSupportedException($"方法DateOnly.{methodInfo.Name}格式化字符串，暂时不支持非常量、变量的解析");

                        var valueArgument = visitor.WrapSql(valueSegment);
                        var format = formatSegment.Value.ToString();
                        string formatValue = null;
                        switch (format)
                        {
                            case "mm/dd/yyyy": formatValue = $"CONVERT(DATE,{valueArgument},101)"; break;
                            case "yyyy.mm.dd": formatValue = $"CONVERT(DATE,{valueArgument},102)"; break;
                            case "dd/mm/yyyy": formatValue = $"CONVERT(DATE,{valueArgument},103)"; break;
                            case "dd.mm.yyyy": formatValue = $"CONVERT(DATE,{valueArgument},104)"; break;
                            case "dd-mm-yyyy": formatValue = $"CONVERT(DATE,{valueArgument},105)"; break;
                            case "dd mon yyyy": formatValue = $"CONVERT(DATE,{valueArgument},106)"; break;
                            case "mon dd, yyyy": formatValue = $"CONVERT(DATE,{valueArgument},107)"; break;
                            case "mm-dd-yyyy": formatValue = $"CONVERT(DATE,{valueArgument},110)"; break;
                            case "yyyy/mm/dd": formatValue = $"CONVERT(DATE,{valueArgument},111)"; break;
                            case "yyyymmdd": formatValue = $"CONVERT(DATE,{valueArgument},112)"; break;
                            default: formatValue = $"CAST({valueArgument} AS DATE)"; break;
                        }
                        return valueSegment.Change(formatValue, false, true);
                    });
                    result = true;
                    if (methodInfo.IsStatic && parameterInfos.Length >= 1 && parameterInfos[0].ParameterType == typeof(ReadOnlySpan<char>))
                        throw new NotSupportedException($"DateOnly.{methodInfo.Name}方法暂时不支持ReadOnlySpan<char>类型参数的解析，请转换成String类型");
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
                case "AddDays":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                        var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if (targetSegment.IsValue
                            && (rightSegment.IsValue))
                            return targetSegment.Change(((DateOnly)targetSegment.Value).AddDays(Convert.ToInt32(rightSegment.Value)));

                        var targetArgument = visitor.WrapSql(targetSegment);
                        var rightArgument = visitor.WrapSql(rightSegment);
                        return targetSegment.Change($"DATEADD(DAY,{rightArgument},{targetArgument})", SqlType.MethodCall);
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
                            return targetSegment.Change(((DateOnly)targetSegment.Value).AddMonths(Convert.ToInt32(rightSegment.Value)));

                        var targetArgument = visitor.WrapSql(targetSegment);
                        var rightArgument = visitor.WrapSql(rightSegment);
                        return targetSegment.Change($"DATEADD(MONTH,{rightArgument},{targetArgument})", SqlType.MethodCall);
                    });
                    result = true;
                    break;
                case "AddYears":
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                        var rightSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        if (targetSegment.IsValue
                            && (rightSegment.IsValue))
                            return targetSegment.Change(((DateOnly)targetSegment.Value).AddDays(Convert.ToInt32(rightSegment.Value)));

                        var targetArgument = visitor.WrapSql(targetSegment);
                        var rightArgument = visitor.WrapSql(rightSegment);
                        return targetSegment.Change($"DATEADD(YEAR,{rightArgument},{targetArgument})", SqlType.MethodCall);
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

                            return targetSegment.Change($"CONVERT({targetSegment.Value},'YYYY-MM-DD')", SqlType.MethodCall);
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
                            }
                            else formatArgument = visitor.WrapSql(formatSegment);

                            if (targetSegment.IsValue
                                && formatSegment.IsValue)
                                return targetSegment.Change(((DateOnly)targetSegment.Value).ToString(formatSegment.Value.ToString()));

                            var targetArgument = visitor.WrapSql(targetSegment);
                            if formatSegment.IsValue
                                return targetSegment.Change($"FORMAT({targetArgument},{formatArgument})", SqlType.MethodCall);

                            throw new NotSupportedException("DateOnly类型暂时不支持非常量的格式化字符串");
                        });
                        result = true;
                    }
                    break;
                case "ToDateTime":
                    if (parameterInfos.Length > 1)
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                            var valueSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            var kindSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[1] });
                            if (!kindSegment.IsConstant && !kindSegment.IsVariable)
                                throw new NotSupportedException($"DateOnly.{methodInfo.Name}方法暂时仅支持第二个参数是常量或是变量的解析");

                            if (targetSegment.IsValue
                                && valueSegment.IsValue)
                                return targetSegment.Change(((DateOnly)targetSegment.Value).ToDateTime((TimeOnly)valueSegment.Value, (DateTimeKind)kindSegment.Value));

                            throw new NotSupportedException("DateOnly类型ToDateTime方法暂时不支持非常量的参数解析");
                        });
                    }
                    else
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                        {
                            var targetSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Object });
                            var valueSegment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                            if (targetSegment.IsValue
                                && valueSegment.IsValue)
                                return targetSegment.Change(((DateOnly)targetSegment.Value).ToDateTime((TimeOnly)valueSegment.Value));

                            throw new NotSupportedException("DateOnly类型ToDateTime方法暂时不支持非常量的参数解析");
                        });
                    }
                    result = true;
                    break;
            }
        }
#endif
        return result;
    }
}