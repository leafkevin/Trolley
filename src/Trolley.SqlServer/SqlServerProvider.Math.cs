using System;
using System.Linq.Expressions;

namespace Trolley.SqlServer;

partial class SqlServerProvider
{
    public virtual bool TryGetMathMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter)
    {
        var result = false;
        formatter = null;
        var methodInfo = methodCallExpr.Method;
        var parameterInfos = methodInfo.GetParameters();
        var cacheKey = HashCode.Combine(methodInfo.DeclaringType, methodInfo);
        switch (methodInfo.Name)
        {
            case "Abs":
                methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                {
                    args[0] = visitor.VisitAndDeferred(args[0]);
                    return args[0].Change($"ABS({args[0]})", false, true);
                });
                result = true;
                break;
            case "Sign":
                methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                {
                    args[0] = visitor.VisitAndDeferred(args[0]);
                    return args[0].Change($"SIGN({args[0]})", false, true);
                });
                result = true;
                break;
            case "Floor":
                methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                {
                    args[0] = visitor.VisitAndDeferred(args[0]);
                    return args[0].Change($"FLOOR({args[0]})", false, true);
                });
                result = true;
                break;
            case "Ceiling":
                methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                {
                    args[0] = visitor.VisitAndDeferred(args[0]);
                    return args[0].Change($"CEILING({args[0]})", false, true);
                });
                result = true;
                break;
            case "Round":
                if (parameterInfos.Length > 1 && parameterInfos[1].ParameterType == typeof(int))
                {
                    methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        args[0] = visitor.VisitAndDeferred(args[0]);
                        args[1] = visitor.VisitAndDeferred(args[1]);
                        return args[0].Change($"ROUND({args[0]},{args[1]})", false, true);
                    });
                    result = true;
                }
                if (parameterInfos.Length == 1)
                {
                    methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        args[0] = visitor.VisitAndDeferred(args[0]);
                        return args[0].Change($"ROUND({args[0]})", false, true);
                    });
                    result = true;
                }
                break;
            case "Exp":
                methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                {
                    args[0] = visitor.VisitAndDeferred(args[0]);
                    return args[0].Change($"EXP({args[0]})", false, true);
                });
                result = true;
                break;
            case "Log":
                methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                {
                    args[0] = visitor.VisitAndDeferred(args[0]);
                    return args[0].Change($"LOG({args[0]})", false, true);
                });
                result = true;
                break;
            case "Log10":
                methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                {
                    args[0] = visitor.VisitAndDeferred(args[0]);
                    return args[0].Change($"LOG10({args[0]})", false, true);
                });
                result = true;
                break;
            case "Pow":
                methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                {
                    args[0] = visitor.VisitAndDeferred(args[0]);
                    args[1] = visitor.VisitAndDeferred(args[1]);
                    return args[0].Change($"POW({args[0]},{args[1]})", false, true);
                });
                result = true;
                break;
            case "Sqrt":
                methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                {
                    args[0] = visitor.VisitAndDeferred(args[0]);
                    return args[0].Change($"SQRT({args[0]})", false, true);
                });
                result = true;
                break;
            case "Cos":
                methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                {
                    args[0] = visitor.VisitAndDeferred(args[0]);
                    return args[0].Change($"COS({args[0]})", false, true);
                });
                result = true;
                break;
            case "Sin":
                methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                {
                    args[0] = visitor.VisitAndDeferred(args[0]);
                    return args[0].Change($"SIN({args[0]})", false, true);
                });
                result = true;
                break;
            case "Tan":
                methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                {
                    args[0] = visitor.VisitAndDeferred(args[0]);
                    return args[0].Change($"TAN({args[0]})", false, true);
                });
                result = true;
                break;
            case "Acos":
                methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                {
                    args[0] = visitor.VisitAndDeferred(args[0]);
                    return args[0].Change($"ACOS({args[0]})", false, true);
                });
                result = true;
                break;
            case "Asin":
                methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                {
                    args[0] = visitor.VisitAndDeferred(args[0]);
                    return args[0].Change($"ASIN({args[0]})", false, true);
                });
                result = true;
                break;
            case "Atan":
                methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                {
                    args[0] = visitor.VisitAndDeferred(args[0]);
                    return args[0].Change($"ATAN({args[0]})", false, true);
                });
                result = true;
                break;
            case "Atan2":
                methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                {
                    args[0] = visitor.VisitAndDeferred(args[0]);
                    args[1] = visitor.VisitAndDeferred(args[1]);
                    return args[0].Change($"ATAN2({args[0]},{args[1]})", false, true);
                });
                result = true;
                break;
            case "Truncate":
                methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                {
                    args[0] = visitor.VisitAndDeferred(args[0]);
                    return args[0].Change($"TRUNCATE({args[0]}, 0)", false, true);
                });
                result = true;
                break;
        }
        return result;
    }
}