using System;
using System.Linq.Expressions;

namespace Trolley;

partial class MySqlProvider
{
    public bool TryGetDateTimeMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter)
    {
        var result = false;
        formatter = null;
        var methodInfo = methodCallExpr.Method;
        var parameterInfos = methodInfo.GetParameters();
        switch (methodInfo.Name)
        {
            //DateTime方法
            case "DaysInMonth":
                if (methodInfo.IsStatic && parameterInfos.Length == 2)
                {
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var leftSegment = visitor.VisitAndDeferred(args[0]);
                        var rightSegment = visitor.VisitAndDeferred(args[1]);

                        if (leftSegment.IsConstantValue && rightSegment.IsConstantValue)
                            return leftSegment.Change(DateTime.DaysInMonth(Convert.ToInt32(leftSegment.Value), Convert.ToInt32(rightSegment.Value)));

                        return leftSegment.Change($"DAYOFMONTH(LAST_DAY(CONCAT({leftSegment},'-',{rightSegment},'-01')))", false, true);
                    });
                    result = true;
                }
                break;
            case "IsLeapYear":
                if (methodInfo.IsStatic && parameterInfos.Length == 1)
                {
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var argumentSegment = visitor.VisitAndDeferred(args[0]);
                        return args[0].Change($"(({argumentSegment})%4=0 AND ({argumentSegment})%100<>0 OR ({argumentSegment})%400=0)", false, true);
                    });
                    result = true;
                }
                break;
            case "Parse":
            case "TryParse":
                if (methodInfo.IsStatic && parameterInfos.Length >= 1 && typeof(string).IsAssignableFrom(parameterInfos[0].ParameterType))
                {
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(args[0]);
                        if (valueSegment.IsConstantValue)
                            return valueSegment.Change(this.GetQuotedValue(DateTime.Parse(valueSegment.ToString())));

                        return valueSegment.Change($"CAST({valueSegment} AS DATETIME)", false, true);
                    });
                    result = true;
                }
                if (methodInfo.IsStatic && parameterInfos.Length >= 3 && parameterInfos[0].ParameterType == typeof(ReadOnlySpan<char>))
                    throw new NotSupportedException("DateTime.Parse方法暂时不支持ReadOnlySpan<char>类型参数的解析，请转换成String类型");
                break;
            case "ParseExact":
            case "TryParseExact":
                if (methodInfo.IsStatic && parameterInfos.Length >= 1 && typeof(string).IsAssignableFrom(parameterInfos[0].ParameterType))
                {
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(args[0]);
                        var formatSegment = visitor.VisitAndDeferred(args[1]);
                        var providerSegment = visitor.VisitAndDeferred(args[2]);

                        if (valueSegment.IsConstantValue && formatSegment.IsConstantValue && providerSegment.IsConstantValue)
                            return valueSegment.Change(this.GetQuotedValue(DateTime.ParseExact(valueSegment.ToString(), formatSegment.ToString(), (IFormatProvider)providerSegment.Value)));

                        return formatSegment.Change($"CAST({formatSegment} AS DATETIME)", false, true);
                    });
                    result = true;
                }
                if (methodInfo.IsStatic && parameterInfos.Length >= 1 && parameterInfos[0].ParameterType == typeof(ReadOnlySpan<char>))
                    throw new NotSupportedException($"DateTime.{methodInfo.Name}方法暂时不支持ReadOnlySpan<char>类型参数的解析，请转换成String类型");
                break;
            case "Add":
                if (!methodInfo.IsStatic && parameterInfos.Length == 1)
                {
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstantValue && rightSegment.IsConstantValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Add((TimeSpan)rightSegment.Value));

                        string targetArgument = null;
                        if (targetSegment.IsConstantValue)
                            targetArgument = this.GetQuotedValue(targetSegment.ToString());
                        else targetArgument = targetSegment.ToString();

                        string rightArgument = null;
                        if (rightSegment.IsConstantValue)
                            rightArgument = this.GetQuotedValue(rightSegment.ToString());
                        else rightArgument = rightSegment.ToString();

                        return targetSegment.Change($"DATE_ADD({targetArgument},INTERVAL({rightArgument}) MICROSECOND)", false, true);
                    });
                    result = true;
                }
                break;
            case "AddDays":
                if (!methodInfo.IsStatic && parameterInfos.Length == 1)
                {
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstantValue && rightSegment.IsConstantValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).AddDays(Convert.ToDouble(rightSegment.Value)));

                        string targetArgument = null;
                        if (targetSegment.IsConstantValue)
                            targetArgument = this.GetQuotedValue(targetSegment.ToString());
                        else targetArgument = targetSegment.ToString();

                        string rightArgument = null;
                        if (rightSegment.IsConstantValue)
                            rightArgument = this.GetQuotedValue(rightSegment.ToString());
                        else rightArgument = rightSegment.ToString();

                        return targetSegment.Change($"DATE_ADD({targetArgument},INTERVAL({rightArgument}) DAY)", false, true);
                    });
                    result = true;
                }
                break;
            case "AddHours":
                if (!methodInfo.IsStatic && parameterInfos.Length == 1)
                {
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstantValue && rightSegment.IsConstantValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).AddHours(Convert.ToDouble(rightSegment.Value)));

                        string targetArgument = null;
                        if (targetSegment.IsConstantValue)
                            targetArgument = this.GetQuotedValue(targetSegment.ToString());
                        else targetArgument = targetSegment.ToString();

                        string rightArgument = null;
                        if (rightSegment.IsConstantValue)
                            rightArgument = this.GetQuotedValue(rightSegment.ToString());
                        else rightArgument = rightSegment.ToString();

                        return targetSegment.Change($"DATE_ADD({targetArgument},INTERVAL({rightArgument}) HOUR)", false, true);
                    });
                    result = true;
                }
                break;
            case "AddMilliseconds":
                if (!methodInfo.IsStatic && parameterInfos.Length == 1)
                {
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstantValue && rightSegment.IsConstantValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).AddMilliseconds(Convert.ToDouble(rightSegment.Value)));

                        string targetArgument = null;
                        if (targetSegment.IsConstantValue)
                            targetArgument = this.GetQuotedValue(targetSegment.ToString());
                        else targetArgument = targetSegment.ToString();

                        string rightArgument = null;
                        if (rightSegment.IsConstantValue)
                            rightArgument = this.GetQuotedValue(rightSegment.ToString());
                        else rightArgument = rightSegment.ToString();

                        return targetSegment.Change($"DATE_ADD({targetArgument},INTERVAL({rightArgument})*1000 MICROSECOND)", false, true);
                    });
                    result = true;
                }
                break;
            case "AddMinutes":
                if (!methodInfo.IsStatic && parameterInfos.Length == 1)
                {
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstantValue && rightSegment.IsConstantValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).AddMinutes(Convert.ToDouble(rightSegment.Value)));

                        string targetArgument = null;
                        if (targetSegment.IsConstantValue)
                            targetArgument = this.GetQuotedValue(targetSegment.ToString());
                        else targetArgument = targetSegment.ToString();

                        string rightArgument = null;
                        if (rightSegment.IsConstantValue)
                            rightArgument = this.GetQuotedValue(rightSegment.ToString());
                        else rightArgument = rightSegment.ToString();

                        return targetSegment.Change($"DATE_ADD({targetArgument},INTERVAL({rightArgument}) MINUTE)", false, true);
                    });
                    result = true;
                }
                break;
            case "AddMonths":
                if (!methodInfo.IsStatic && parameterInfos.Length == 1)
                {
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstantValue && rightSegment.IsConstantValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).AddMonths(Convert.ToInt32(rightSegment.Value)));

                        string targetArgument = null;
                        if (targetSegment.IsConstantValue)
                            targetArgument = this.GetQuotedValue(targetSegment.ToString());
                        else targetArgument = targetSegment.ToString();

                        string rightArgument = null;
                        if (rightSegment.IsConstantValue)
                            rightArgument = this.GetQuotedValue(rightSegment.ToString());
                        else rightArgument = rightSegment.ToString();

                        return targetSegment.Change($"DATE_ADD({targetArgument},INTERVAL({rightArgument}) MONTH)", false, true);
                    });
                    result = true;
                }
                break;
            case "AddSeconds":
                if (!methodInfo.IsStatic && parameterInfos.Length == 1)
                {
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstantValue && rightSegment.IsConstantValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).AddSeconds(Convert.ToDouble(rightSegment.Value)));

                        string targetArgument = null;
                        if (targetSegment.IsConstantValue)
                            targetArgument = this.GetQuotedValue(targetSegment.ToString());
                        else targetArgument = targetSegment.ToString();

                        string rightArgument = null;
                        if (rightSegment.IsConstantValue)
                            rightArgument = this.GetQuotedValue(rightSegment.ToString());
                        else rightArgument = rightSegment.ToString();

                        return targetSegment.Change($"DATE_ADD({targetArgument},INTERVAL({rightArgument}) SECOND)", false, true);
                    });
                    result = true;
                }
                break;
            case "AddTicks":
                if (!methodInfo.IsStatic && parameterInfos.Length == 1)
                {
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstantValue && rightSegment.IsConstantValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).AddTicks(Convert.ToInt64(rightSegment.Value)));

                        string targetArgument = null;
                        if (targetSegment.IsConstantValue)
                            targetArgument = this.GetQuotedValue(targetSegment.ToString());
                        else targetArgument = targetSegment.ToString();

                        string rightArgument = null;
                        if (rightSegment.IsConstantValue)
                            rightArgument = this.GetQuotedValue(rightSegment.ToString());
                        else rightArgument = rightSegment.ToString();

                        return targetSegment.Change($"DATE_ADD({targetArgument},INTERVAL({rightArgument})/10 MICROSECOND)", false, true);
                    });
                    result = true;
                }
                break;
            case "AddYears":
                if (!methodInfo.IsStatic && parameterInfos.Length == 1)
                {
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstantValue && rightSegment.IsConstantValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).AddYears(Convert.ToInt32(rightSegment.Value)));

                        string targetArgument = null;
                        if (targetSegment.IsConstantValue)
                            targetArgument = this.GetQuotedValue(targetSegment.ToString());
                        else targetArgument = targetSegment.ToString();

                        string rightArgument = null;
                        if (rightSegment.IsConstantValue)
                            rightArgument = this.GetQuotedValue(rightSegment.ToString());
                        else rightArgument = rightSegment.ToString();

                        return targetSegment.Change($"DATE_ADD({targetArgument},INTERVAL({rightArgument}) YEAR)", false, true);
                    });
                    result = true;
                }
                break;
            case "Subtract":
                if (!methodInfo.IsStatic && parameterInfos.Length == 1 && parameterInfos[0].ParameterType == typeof(DateTime))
                {
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstantValue && rightSegment.IsConstantValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Subtract(Convert.ToDateTime(rightSegment.Value)));

                        string targetArgument = null;
                        if (targetSegment.IsConstantValue)
                            targetArgument = this.GetQuotedValue(targetSegment.ToString());
                        else targetArgument = targetSegment.ToString();

                        string rightArgument = null;
                        if (rightSegment.IsConstantValue)
                            rightArgument = this.GetQuotedValue(rightSegment.ToString());
                        else rightArgument = rightSegment.ToString();

                        return targetSegment.Change($"TIME_SUB({targetArgument},{rightArgument})", false, true);
                    });
                    result = true;
                }
                if (!methodInfo.IsStatic && parameterInfos.Length == 1 && parameterInfos[0].ParameterType == typeof(TimeSpan))
                {
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstantValue && rightSegment.IsConstantValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).Subtract((TimeSpan)rightSegment.Value));

                        string targetArgument = null;
                        if (targetSegment.IsConstantValue)
                            targetArgument = this.GetQuotedValue(targetSegment.ToString());
                        else targetArgument = targetSegment.ToString();

                        string rightArgument = null;
                        if (rightSegment.IsConstantValue)
                            rightArgument = this.GetQuotedValue(rightSegment.ToString());
                        else rightArgument = rightSegment.ToString();

                        return targetSegment.Change($"DATE_SUB({targetArgument},INTERVAL({rightArgument}) MICROSECOND)", false, true);
                    });
                    result = true;
                }
                break;
            case "Equals":
                if (!methodInfo.IsStatic && parameterInfos.Length == 1)
                {
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        string targetArgument = null;
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstantValue)
                            targetArgument = this.GetQuotedValue(targetSegment.ToString());
                        else targetArgument = targetSegment.ToString();

                        string rightArgument = null;
                        var rightSegment = visitor.VisitAndDeferred(target);
                        if (rightSegment.IsConstantValue)
                            rightArgument = this.GetQuotedValue(rightSegment.ToString());
                        else rightArgument = rightSegment.ToString();

                        return targetSegment.Change($"({targetArgument}={rightArgument})", false, true);
                    });
                    result = true;
                }
                break;
            case "CompareTo":
                if (!methodInfo.IsStatic && parameterInfos.Length == 1)
                {
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        string targetArgument = null;
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstantValue)
                            targetArgument = this.GetQuotedValue(targetSegment.ToString());
                        else targetArgument = targetSegment.ToString();

                        string rightArgument = null;
                        var rightSegment = visitor.VisitAndDeferred(target);
                        if (rightSegment.IsConstantValue)
                            rightArgument = this.GetQuotedValue(rightSegment.ToString());
                        else rightArgument = rightSegment.ToString();

                        return targetSegment.Change($"TIMESTAMPDIFF(MICROSECOND,{rightArgument},{targetArgument})", false, true);
                    });
                    result = true;
                }
                break;
            case "ToString":
                if (!methodInfo.IsStatic && parameterInfos.Length == 0)
                {
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        if (targetSegment.IsConstantValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).ToString("yyyy-MM-dd HH:mm:ss"));

                        return targetSegment.Change($"DATE_FORMAT({targetSegment},'%Y-%m-%d %H:%i:%s')", false, true);
                    });
                    result = true;
                }
                if (!methodInfo.IsStatic && parameterInfos.Length == 1)
                {
                    methodCallSqlFormatterCahe.TryAdd(methodInfo, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(target);

                        if (targetSegment.IsConstantValue && rightSegment.IsConstantValue)
                            return targetSegment.Change(((DateTime)targetSegment.Value).ToString(rightSegment.ToString()));

                        string targetArgument = null;
                        if (targetSegment.IsConstantValue)
                            targetArgument = this.GetQuotedValue(targetSegment.ToString());
                        else targetArgument = targetSegment.ToString();

                        string rightArgument = null;
                        if (rightSegment.IsConstantValue)
                        {
                            rightArgument = rightSegment.ToString();
                            rightArgument = rightArgument.Replace("yyyy", "%Y");
                            rightArgument = rightArgument.Replace("yyy", "%Y");
                            rightArgument = rightArgument.Replace("yy", "%y");

                            rightArgument = rightArgument.Replace("MMMM", "%M");
                            rightArgument = rightArgument.Replace("MMM", "%b");
                            rightArgument = rightArgument.Replace("MM", "%m");
                            rightArgument = rightArgument.Replace("M", "%c");

                            rightArgument = rightArgument.Replace("dddd", "%W");
                            rightArgument = rightArgument.Replace("ddd", "%a");

                            rightArgument = rightArgument.Replace("dd", "%d");
                            rightArgument = rightArgument.Replace("d", "%e");

                            rightArgument = rightArgument.Replace("HH", "%H");
                            rightArgument = rightArgument.Replace("H", "%k");
                            rightArgument = rightArgument.Replace("hh", "%h");
                            rightArgument = rightArgument.Replace("h", "%l");

                            rightArgument = rightArgument.Replace("mm", "%i");
                            rightArgument = rightArgument.Replace("m", "%i");

                            rightArgument = rightArgument.Replace("ss", "%s");
                            rightArgument = rightArgument.Replace("s", "%s");

                            rightArgument = rightArgument.Replace("tt", "%p");
                            rightArgument = rightArgument.Replace("t", "SUBSTR(%s,1,1)");
                        }

                        return targetSegment.Change($"DATE_FORMAT({targetArgument},{rightArgument})", false, true);
                    });
                    result = true;
                }
                break;
        }
        return result;
    }
}
