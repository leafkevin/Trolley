using System.Linq.Expressions;

namespace Trolley;

partial class MySqlProvider
{
    public bool TryGetConvertMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter)
    {
        var result = false;
        formatter = null;
        var methodInfo = methodCallExpr.Method;
        var parameterInfos = methodInfo.GetParameters();
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
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        args[0] = visitor.VisitAndDeferred(args[0]);
                        if (args[0].IsConstantValue)
                            return args[0].Change(this.GetQuotedValue(methodCallExpr.Type, args[0]));
                        return args[0].Change($"CAST({this.GetQuotedValue(args[0])} AS {this.CastTo(methodCallExpr.Type)})", false, true);
                    });
                    result = true;
                }
                break;
        }
        return result;
    }
}