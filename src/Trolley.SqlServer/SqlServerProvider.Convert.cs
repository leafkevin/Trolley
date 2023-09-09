using System;
using System.Linq.Expressions;

namespace Trolley.SqlServer;

partial class SqlServerProvider
{
    public override bool TryGetConvertMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter)
    {
        var result = false;
        formatter = null;
        var methodInfo = methodCallExpr.Method;
        var parameterInfos = methodInfo.GetParameters();
        var cacheKey = HashCode.Combine(methodInfo.DeclaringType, methodInfo);
        switch (methodInfo.Name)
        {
            case "ToBoolean":
            case "ToByte":
            case "ToChar":
            case "ToDateTime":
            case "ToDouble":
            case "ToInt16":
            case "ToInt32":
            case "ToInt64":
            case "ToSByte":
            case "ToSingle":
            case "ToUInt16":
            case "ToUInt32":
            case "ToUInt64":
            case "ToDecimal":
            case "ToString":
                if (parameterInfos.Length == 1)
                {
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var args0Segment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if (args0Segment.IsConstant || args0Segment.IsVariable)
                        {
                            var cacheKey = HashCode.Combine(methodInfo.DeclaringType, methodInfo);
                            if (!methodCallCache.TryGetValue(cacheKey, out var toValueDelegate))
                            {
                                var valueExpr = Expression.Parameter(typeof(object), "value");
                                var callExpr = Expression.Call(methodInfo, valueExpr);
                                var toValueExpr = Expression.Convert(callExpr, typeof(object));
                                toValueDelegate = Expression.Lambda(toValueExpr, valueExpr).Compile();
                                methodCallCache.TryAdd(cacheKey, toValueDelegate);
                            }
                            var toValue = toValueDelegate as Func<object, object>;
                            args0Segment.ExpectType = methodInfo.ReturnType;
                            args0Segment.Value = toValue.Invoke(args0Segment.Value);
                            return visitor.Change(args0Segment);
                        }
                        var args0Argument = visitor.GetQuotedValue(args0Segment);
                        args0Segment.ExpectType = methodInfo.ReturnType;
                        return visitor.Change(args0Segment, this.CastTo(methodCallExpr.Type, args0Argument), false, true);
                    });
                    result = true;
                }
                break;
        }
        return result;
    }
}