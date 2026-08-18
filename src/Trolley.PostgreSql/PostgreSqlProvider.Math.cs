using System;
using System.Linq.Expressions;

namespace Trolley.PostgreSql;

partial class PostgreSqlProvider
{
    public override bool TryGetMathMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out Func<ISqlVisitor, MethodCallExpression, Stack<DeferredOperation>, SqlSegment> formatter)
    {
        var result = false;
        formatter = null;
        var methodInfo = methodCallExpr.Method;
        var parameterInfos = methodInfo.GetParameters();
        var cacheKey = HashCode.Combine(methodInfo.DeclaringType, methodInfo);
        switch (methodInfo.Name)
        {
            case "Abs":
                formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                {
                    var args0Segment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                    return args0Segment.Change($"ABS({visitor.WrapSql(args0Segment)})", SqlType.MethodCall);
                });
                result = true;
                break;
            case "Sign":
                formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                {
                    var args0Segment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                    return args0Segment.Change($"SIGN({visitor.WrapSql(args0Segment)})", SqlType.MethodCall);
                });
                result = true;
                break;
            case "Floor":
                formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                {
                    var args0Segment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                    return args0Segment.Change($"FLOOR({visitor.WrapSql(args0Segment)})", SqlType.MethodCall);
                });
                result = true;
                break;
            case "Ceiling":
                formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                {
                    var args0Segment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                    return args0Segment.Change($"CEILING({visitor.WrapSql(args0Segment)})", SqlType.MethodCall);
                });
                result = true;
                break;
            case "Round":
                if (parameterInfos.Length > 1 && parameterInfos[1].ParameterType == typeof(int))
                {
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var args0Segment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        var args1Segment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[1] });
                        var args0Argument = visitor.WrapSql(args0Segment);
                        var args1Argument = visitor.WrapSql(args1Segment);
                        return args0Segment.Change($"ROUND({args0Argument},{args1Argument})", SqlType.MethodCall);
                    });
                    result = true;
                }
                if (parameterInfos.Length == 1)
                {
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                    {
                        var args0Segment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                        return args0Segment.Change($"ROUND({visitor.WrapSql(args0Segment)})", SqlType.MethodCall);
                    });
                    result = true;
                }
                break;
            case "Exp":
                formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                {
                    var args0Segment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                    return args0Segment.Change($"EXP({visitor.WrapSql(args0Segment)})", SqlType.MethodCall);
                });
                result = true;
                break;
            case "Log":
                formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                {
                    var args0Segment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                    return args0Segment.Change($"LOG({visitor.WrapSql(args0Segment)})", SqlType.MethodCall);
                });
                result = true;
                break;
            case "Log10":
                formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                {
                    var args0Segment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                    return args0Segment.Change($"LOG10({visitor.WrapSql(args0Segment)})", SqlType.MethodCall);
                });
                result = true;
                break;
            case "Pow":
                formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                {
                    var args0Segment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                    var args1Segment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[1] });
                    var args0Argument = visitor.WrapSql(args0Segment);
                    var args1Argument = visitor.WrapSql(args1Segment);
                    return args0Segment.Change($"POW({args0Argument},{args1Argument})", SqlType.MethodCall);
                });
                result = true;
                break;
            case "Sqrt":
                formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                {
                    var args0Segment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                    return args0Segment.Change($"SQRT({visitor.WrapSql(args0Segment)})", SqlType.MethodCall);
                });
                result = true;
                break;
            case "Cos":
                formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                {
                    var args0Segment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                    return args0Segment.Change($"COS({visitor.WrapSql(args0Segment)})", SqlType.MethodCall);
                });
                result = true;
                break;
            case "Sin":
                formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                {
                    var args0Segment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                    return args0Segment.Change($"SIN({visitor.WrapSql(args0Segment)})", SqlType.MethodCall);
                });
                result = true;
                break;
            case "Tan":
                formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                {
                    var args0Segment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                    return args0Segment.Change($"TAN({visitor.WrapSql(args0Segment)})", SqlType.MethodCall);
                });
                result = true;
                break;
            case "Acos":
                formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                {
                    var args0Segment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                    return args0Segment.Change($"ACOS({visitor.WrapSql(args0Segment)})", SqlType.MethodCall);
                });
                result = true;
                break;
            case "Asin":
                formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                {
                    var args0Segment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                    return args0Segment.Change($"ASIN({visitor.WrapSql(args0Segment)})", SqlType.MethodCall);
                });
                result = true;
                break;
            case "Atan":
                formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                {
                    var args0Segment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                    return args0Segment.Change($"ATAN({visitor.WrapSql(args0Segment)})", SqlType.MethodCall);
                });
                result = true;
                break;
            case "Atan2":
                formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                {
                    var args0Segment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                    var args1Segment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[1] });
                    var args0Argument = visitor.WrapSql(args0Segment);
                    var args1Argument = visitor.WrapSql(args1Segment);
                    return args0Segment.Change($"ATAN2({args0Argument},{args1Argument})", SqlType.MethodCall);
                });
                result = true;
                break;
            case "Truncate":
                formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, key => (visitor, methodCallExpr, deferredOperations) =>
                {
                    var args0Segment = visitor.Visit(new SqlSegment { Expression = methodCallExpr.Arguments[0] });
                    return args0Segment.Change($"TRUNC({visitor.WrapSql(args0Segment)},0)", SqlType.MethodCall);
                });
                result = true;
                break;
        }
        return result;
    }
}
