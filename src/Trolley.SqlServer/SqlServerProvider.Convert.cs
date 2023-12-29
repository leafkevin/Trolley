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
                             args0Segment.Value = methodInfo.Invoke(null, new object[] { args0Segment.Value });
                             //类型改变了
                             args0Segment.ExpectType = methodInfo.ReturnType;
                             return args0Segment;
                         }
                         var args0Argument = args0Segment.Value;
                         if ((args0Segment.ExpectType ?? args0Segment.Expression.Type) != methodInfo.ReturnType)
                             args0Argument = this.CastTo(methodCallExpr.Type, args0Segment.Value);
                         //类型改变了
                         args0Segment.ExpectType = methodInfo.ReturnType;
                         args0Segment.IsExpression = true;
                         return args0Segment;
                     });
                    result = true;
                }
                break;
        }
        return result;
    }
}