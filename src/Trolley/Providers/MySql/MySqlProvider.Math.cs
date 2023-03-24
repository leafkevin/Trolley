using System.Linq.Expressions;

namespace Trolley;

partial class MySqlProvider
{
    public bool TryGetMathMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter)
    {
        var result = false;
        formatter = null;
        var methodInfo = methodCallExpr.Method;
        var parameterInfos = methodInfo.GetParameters();
        if (!methodCallSqlFormatterCahe.TryGetValue(methodInfo, out formatter))
        {
            switch (methodInfo.Name)
            {
                case "Abs":
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        args[0] = visitor.VisitAndDeferred(args[0]);
                        return args[0].Change($"ABS({args[0]})", false, true);
                    });
                    result = true;
                    break;
                case "Sign":
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        args[0] = visitor.VisitAndDeferred(args[0]);
                        return args[0].Change($"SIGN({args[0]})", false, true);
                    });
                    result = true;
                    break;
                case "Floor":
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        args[0] = visitor.VisitAndDeferred(args[0]);
                        return args[0].Change($"FLOOR({args[0]})", false, true);
                    });
                    result = true;
                    break;
                case "Ceiling":
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        args[0] = visitor.VisitAndDeferred(args[0]);
                        return args[0].Change($"CEILING({args[0]})", false, true);
                    });
                    result = true;
                    break;
                case "Round":
                    if (parameterInfos.Length > 1 && parameterInfos[1].ParameterType == typeof(int))
                    {
                        methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                        {
                            args[0] = visitor.VisitAndDeferred(args[0]);
                            args[1] = visitor.VisitAndDeferred(args[1]);
                            return args[0].Change($"ROUND({args[0]},{args[1]})", false, true);
                        });
                        result = true;
                    }
                    if (parameterInfos.Length == 1)
                    {
                        methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                        {
                            args[0] = visitor.VisitAndDeferred(args[0]);
                            return args[0].Change($"ROUND({args[0]})", false, true);
                        });
                        result = true;
                    }
                    break;
                case "Exp":
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        args[0] = visitor.VisitAndDeferred(args[0]);
                        return args[0].Change($"EXP({args[0]})", false, true);
                    });
                    result = true;
                    break;
                case "Log":
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        args[0] = visitor.VisitAndDeferred(args[0]);
                        return args[0].Change($"LOG({args[0]})", false, true);
                    });
                    result = true;
                    break;
                case "Log10":
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        args[0] = visitor.VisitAndDeferred(args[0]);
                        return args[0].Change($"LOG10({args[0]})", false, true);
                    });
                    result = true;
                    break;
                case "Pow":
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        args[0] = visitor.VisitAndDeferred(args[0]);
                        args[1] = visitor.VisitAndDeferred(args[1]);
                        return args[0].Change($"POW({args[0]},{args[1]})", false, true);
                    });
                    result = true;
                    break;
                case "Sqrt":
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        args[0] = visitor.VisitAndDeferred(args[0]);
                        return args[0].Change($"SQRT({args[0]})", false, true);
                    });
                    result = true;
                    break;
                case "Cos":
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        args[0] = visitor.VisitAndDeferred(args[0]);
                        return args[0].Change($"Cos({args[0]})", false, true);
                    });
                    result = true;
                    break;
                case "Sin":
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        args[0] = visitor.VisitAndDeferred(args[0]);
                        return args[0].Change($"SIN({args[0]})", false, true);
                    });
                    result = true;
                    break;
                case "Tan":
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        args[0] = visitor.VisitAndDeferred(args[0]);
                        return args[0].Change($"TAN({args[0]})", false, true);
                    });
                    result = true;
                    break;
                case "Acos":
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        args[0] = visitor.VisitAndDeferred(args[0]);
                        return args[0].Change($"ACOS({args[0]})", false, true);
                    });
                    result = true;
                    break;
                case "Asin":
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        args[0] = visitor.VisitAndDeferred(args[0]);
                        return args[0].Change($"ASIN({args[0]})", false, true);
                    });
                    result = true;
                    break;
                case "Atan":
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        args[0] = visitor.VisitAndDeferred(args[0]);
                        return args[0].Change($"ATAN({args[0]})", false, true);
                    });
                    result = true;
                    break;
                case "Atan2":
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        args[0] = visitor.VisitAndDeferred(args[0]);
                        args[1] = visitor.VisitAndDeferred(args[1]);
                        return args[0].Change($"ATAN2({args[0]},{args[1]})", false, true);
                    });
                    result = true;
                    break;
                case "Truncate":
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        args[0] = visitor.VisitAndDeferred(args[0]);
                        return args[0].Change($"TRUNCATE({args[0]}, 0)", false, true);
                    });
                    result = true;
                    break;
            }
            return result;
        }
        return false;
    }
}