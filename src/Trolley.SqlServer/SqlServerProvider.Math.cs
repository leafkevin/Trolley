using System;
using System.Linq.Expressions;

namespace Trolley.SqlServer;

partial class SqlServerProvider
{
    public override bool TryGetMathMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter)
    {
        var result = false;
        formatter = null;
        var methodInfo = methodCallExpr.Method;
        var parameterInfos = methodInfo.GetParameters();
        var cacheKey = HashCode.Combine(methodInfo.DeclaringType, methodInfo);
        switch (methodInfo.Name)
        {
            case "Abs":
                methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                {
                    var args0Segment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                    return visitor.Change(args0Segment, $"ABS({this.GetQuotedValue(args0Segment)})", false, true);
                });
                result = true;
                break;
            case "Sign":
                methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                {
                    var args0Segment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                    return visitor.Change(args0Segment, $"SIGN({this.GetQuotedValue(args0Segment)})", false, true);
                });
                result = true;
                break;
            case "Floor":
                methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                {
                    var args0Segment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                    return visitor.Change(args0Segment, $"FLOOR({this.GetQuotedValue(args0Segment)})", false, true);
                });
                result = true;
                break;
            case "Ceiling":
                methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                {
                    var args0Segment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                    return visitor.Change(args0Segment, $"CEILING({this.GetQuotedValue(args0Segment)})", false, true);
                });
                result = true;
                break;
            case "Round":
                if (parameterInfos.Length > 1 && parameterInfos[1].ParameterType == typeof(int))
                {
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var args0Segment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        var args1Segment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[1] });
                        var args0Argument = this.GetQuotedValue(args0Segment);
                        var args1Argument = this.GetQuotedValue(args1Segment);
                        return visitor.Merge(args0Segment, args1Segment, $"ROUND({args0Argument},{args1Argument})", false, true);
                    });
                    result = true;
                }
                if (parameterInfos.Length == 1)
                {
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var args0Segment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        return visitor.Change(args0Segment, $"ROUND({this.GetQuotedValue(args0Segment)})", false, true);
                    });
                    result = true;
                }
                break;
            case "Exp":
                methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                {
                    var args0Segment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                    return visitor.Change(args0Segment, $"EXP({this.GetQuotedValue(args0Segment)})", false, true);
                });
                result = true;
                break;
            case "Log":
                methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                {
                    var args0Segment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                    return visitor.Change(args0Segment, $"LOG({this.GetQuotedValue(args0Segment)})", false, true);
                });
                result = true;
                break;
            case "Log10":
                methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                {
                    var args0Segment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                    return visitor.Change(args0Segment, $"LOG10({this.GetQuotedValue(args0Segment)})", false, true);
                });
                result = true;
                break;
            case "Pow":
                methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                {
                    var args0Segment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                    var args1Segment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[1] });
                    var args0Argument = this.GetQuotedValue(args0Segment);
                    var args1Argument = this.GetQuotedValue(args1Segment);
                    return visitor.Merge(args0Segment, args1Segment, $"POW({args0Argument},{args1Argument})", false, true);
                });
                result = true;
                break;
            case "Sqrt":
                methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                {
                    var args0Segment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                    return visitor.Change(args0Segment, $"SQRT({this.GetQuotedValue(args0Segment)})", false, true);
                });
                result = true;
                break;
            case "Cos":
                methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                {
                    var args0Segment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                    return visitor.Change(args0Segment, $"COS({this.GetQuotedValue(args0Segment)})", false, true);
                });
                result = true;
                break;
            case "Sin":
                methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                {
                    var args0Segment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                    return visitor.Change(args0Segment, $"SIN({this.GetQuotedValue(args0Segment)})", false, true);
                });
                result = true;
                break;
            case "Tan":
                methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                {
                    var args0Segment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                    return visitor.Change(args0Segment, $"TAN({this.GetQuotedValue(args0Segment)})", false, true);
                });
                result = true;
                break;
            case "Acos":
                methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                {
                    var args0Segment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                    return visitor.Change(args0Segment, $"ACOS({this.GetQuotedValue(args0Segment)})", false, true);
                });
                result = true;
                break;
            case "Asin":
                methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                {
                    var args0Segment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                    return visitor.Change(args0Segment, $"ASIN({this.GetQuotedValue(args0Segment)})", false, true);
                });
                result = true;
                break;
            case "Atan":
                methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                {
                    var args0Segment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                    return visitor.Change(args0Segment, $"ATAN({this.GetQuotedValue(args0Segment)})", false, true);
                });
                result = true;
                break;
            case "Atan2":
                methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                {
                    var args0Segment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                    var args1Segment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[1] });
                    var args0Argument = this.GetQuotedValue(args0Segment);
                    var args1Argument = this.GetQuotedValue(args1Segment);
                    return visitor.Merge(args0Segment, args1Segment, $"ATAN2({args0Argument},{args1Argument})", false, true);
                });
                result = true;
                break;
            case "Truncate":
                methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                {
                    var args0Segment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                    return visitor.Change(args0Segment, $"TRUNCATE({this.GetQuotedValue(args0Segment)},0)", false, true);
                });
                result = true;
                break;
        }
        return result;
    }
}