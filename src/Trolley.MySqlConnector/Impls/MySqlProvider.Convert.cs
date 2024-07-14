using System;
using System.Linq.Expressions;

namespace Trolley.MySqlConnector;

partial class MySqlProvider
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
                if (parameterInfos.Length == 1)
                {
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var args0Segment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if (args0Segment.IsConstant || args0Segment.IsVariable)
                        {
                            args0Segment.Value = methodInfo.Invoke(null, new object[] { args0Segment.Value });
                            return args0Segment;
                        }
                        if (args0Segment.SegmentType != methodInfo.ReturnType)
                        {
                            args0Segment.Value = this.CastTo(methodCallExpr.Type, args0Segment.Value);
                            args0Segment.IsMethodCall = true;
                        }
                        return args0Segment;
                    });
                    result = true;
                }
                break;
            case "ToString":
                if (parameterInfos.Length == 1)
                {
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var args0Segment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if (args0Segment.IsConstant || args0Segment.IsVariable)
                        {
                            args0Segment.Value = methodInfo.Invoke(null, new object[] { args0Segment.Value });
                            return args0Segment;
                        }
                        if (args0Segment.SegmentType != methodInfo.ReturnType)
                        {
                            if (args0Segment.SegmentType.IsEnum && !args0Segment.IsExpression && !args0Segment.IsMethodCall)
                                visitor.ToEnumString(args0Segment);
                            else
                            {
                                args0Segment.Value = this.CastTo(methodCallExpr.Type, args0Segment.Value);
                                args0Segment.IsMethodCall = true;
                            }
                        }
                        return args0Segment;
                    });
                    result = true;
                }
                break;
        }
        return result;
    }
}