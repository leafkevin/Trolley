using System;
using System.Linq.Expressions;

namespace Trolley.MySqlConnector;

partial class MySqlProvider
{
    public override bool TryGetMathMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter)
    {
        var result = false;
        formatter = null;
        var methodInfo = methodCallExpr.Method;
        var parameterInfos = methodInfo.GetParameters();
        var cacheKey = RepositoryHelper.GetCacheKey(methodInfo.DeclaringType, methodInfo);
        switch (methodInfo.Name)
        {
            case "Abs":
                formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                {
                    var args0Segment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                    return args0Segment.Change($"ABS({visitor.GetQuotedValue(args0Segment)})", false, true);
                });
                result = true;
                break;
            case "Sign":
                formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                {
                    var args0Segment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                    return args0Segment.Change($"SIGN({visitor.GetQuotedValue(args0Segment)})", false, true);
                });
                result = true;
                break;
            case "Floor":
                formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                {
                    var args0Segment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                    return args0Segment.Change($"FLOOR({visitor.GetQuotedValue(args0Segment)})", false, true);
                });
                result = true;
                break;
            case "Ceiling":
                formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                {
                    var args0Segment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                    return args0Segment.Change($"CEILING({visitor.GetQuotedValue(args0Segment)})", false, true);
                });
                result = true;
                break;
            case "Round":
                if (parameterInfos.Length > 1 && parameterInfos[1].ParameterType == typeof(int))
                {
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var args0Segment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        var args1Segment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[1] });
                        var args0Argument = visitor.GetQuotedValue(args0Segment);
                        var args1Argument = visitor.GetQuotedValue(args1Segment);
                        return args0Segment.Merge(args1Segment, $"ROUND({args0Argument},{args1Argument})", false, true);
                    });
                    result = true;
                }
                if (parameterInfos.Length == 1)
                {
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var args0Segment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        return args0Segment.Change($"ROUND({visitor.GetQuotedValue(args0Segment)})", false, true);
                    });
                    result = true;
                }
                break;
            case "Exp":
                formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                {
                    var args0Segment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                    return args0Segment.Change($"EXP({visitor.GetQuotedValue(args0Segment)})", false, true);
                });
                result = true;
                break;
            case "Log":
                formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                {
                    var args0Segment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                    return args0Segment.Change($"LOG({visitor.GetQuotedValue(args0Segment)})", false, true);
                });
                result = true;
                break;
            case "Log10":
                formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                {
                    var args0Segment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                    return args0Segment.Change($"LOG10({visitor.GetQuotedValue(args0Segment)})", false, true);
                });
                result = true;
                break;
            case "Pow":
                formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                {
                    var args0Segment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                    var args1Segment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[1] });
                    var args0Argument = visitor.GetQuotedValue(args0Segment);
                    var args1Argument = visitor.GetQuotedValue(args1Segment);
                    return args0Segment.Merge(args1Segment, $"POW({args0Argument},{args1Argument})", false, true);
                });
                result = true;
                break;
            case "Sqrt":
                formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                {
                    var args0Segment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                    return args0Segment.Change($"SQRT({visitor.GetQuotedValue(args0Segment)})", false, true);
                });
                result = true;
                break;
            case "Cos":
                formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                {
                    var args0Segment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                    return args0Segment.Change($"COS({visitor.GetQuotedValue(args0Segment)})", false, true);
                });
                result = true;
                break;
            case "Sin":
                formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                {
                    var args0Segment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                    return args0Segment.Change($"SIN({visitor.GetQuotedValue(args0Segment)})", false, true);
                });
                result = true;
                break;
            case "Tan":
                formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                {
                    var args0Segment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                    return args0Segment.Change($"TAN({visitor.GetQuotedValue(args0Segment)})", false, true);
                });
                result = true;
                break;
            case "Acos":
                formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                {
                    var args0Segment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                    return args0Segment.Change($"ACOS({visitor.GetQuotedValue(args0Segment)})", false, true);
                });
                result = true;
                break;
            case "Asin":
                formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                {
                    var args0Segment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                    return args0Segment.Change($"ASIN({visitor.GetQuotedValue(args0Segment)})", false, true);
                });
                result = true;
                break;
            case "Atan":
                formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                {
                    var args0Segment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                    return args0Segment.Change($"ATAN({visitor.GetQuotedValue(args0Segment)})", false, true);
                });
                result = true;
                break;
            case "Atan2":
                formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                {
                    var args0Segment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                    var args1Segment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[1] });
                    var args0Argument = visitor.GetQuotedValue(args0Segment);
                    var args1Argument = visitor.GetQuotedValue(args1Segment);
                    return args0Segment.Merge(args1Segment, $"ATAN2({args0Argument},{args1Argument})", false, true);
                });
                result = true;
                break;
            case "Truncate":
                formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                {
                    var args0Segment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                    return args0Segment.Change($"TRUNCATE({visitor.GetQuotedValue(args0Segment)},0)", false, true);
                });
                result = true;
                break;
        }
        return result;
    }
}
