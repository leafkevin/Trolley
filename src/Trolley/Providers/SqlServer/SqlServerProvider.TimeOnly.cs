using System;
using System.Linq.Expressions;

namespace Trolley;

partial class SqlServerProvider
{
    public virtual bool TryGetTimeOnlyMemberAccessSqlFormatter(MemberExpression memberExpr, out MemberAccessSqlFormatter formatter)
    {
        bool result = false;
        formatter = null;
        var memberInfo = memberExpr.Member;
        var cacheKey = HashCode.Combine(memberInfo.DeclaringType, memberInfo);
        if (memberExpr.Expression == null)
        {
            switch (memberInfo.Name)
            {
                //静态成员访问，理论上没有target对象，为了不再创建sqlSegment对象，外层直接把对象传了进来
                case "MinValue":
                    memberAccessSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target) => target.Change(TimeOnly.MinValue));
                    result = true;
                    break;
                case "MaxValue":
                    memberAccessSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target) => target.Change(TimeOnly.MaxValue));
                    result = true;
                    break;
            }
            return result;
        }
        switch (memberInfo.Name)
        {
            case "Ticks":
                memberAccessSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target) =>
                {
                    var targetSegment = visitor.VisitAndDeferred(target);
                    if (targetSegment.IsConstantValue)
                        return targetSegment.Change(((TimeOnly)targetSegment.Value).Ticks);
                    //mysql只返回6位，丢失一位精度
                    return targetSegment.Change($"(DATEDIFF_BIG(NANOSECOND,'00:00:00',{this.GetQuotedValue(targetSegment)})/100)", false, true);
                });
                result = true;
                break;
            case "Hour":
                memberAccessSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target) =>
                {
                    var targetSegment = visitor.VisitAndDeferred(target);
                    if (targetSegment.IsConstantValue)
                        return targetSegment.Change(((TimeOnly)targetSegment.Value).Hour);

                    return targetSegment.Change($"DATEPART(HOUR,{this.GetQuotedValue(targetSegment)})", false, true);
                });
                result = true;
                break;
            case "Millisecond":
                memberAccessSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target) =>
                {
                    var targetSegment = visitor.VisitAndDeferred(target);
                    if (targetSegment.IsConstantValue)
                        return targetSegment.Change(((TimeOnly)targetSegment.Value).Millisecond);

                    return targetSegment.Change($"DATEPART(MILLISECOND,{this.GetQuotedValue(targetSegment)})", false, true);
                });
                result = true;
                break;
            case "Minute":
                memberAccessSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target) =>
                {
                    var targetSegment = visitor.VisitAndDeferred(target);
                    if (targetSegment.IsConstantValue)
                        return targetSegment.Change(((TimeOnly)targetSegment.Value).Minute);

                    return targetSegment.Change($"DATEPART(MINUTE,{this.GetQuotedValue(targetSegment)})", false, true);
                });
                result = true;
                break;
            case "Second":
                memberAccessSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target) =>
                {
                    var targetSegment = visitor.VisitAndDeferred(target);
                    if (targetSegment.IsConstantValue)
                        return targetSegment.Change(((TimeOnly)targetSegment.Value).Second);

                    return targetSegment.Change($"DATEPART(SECOND,{this.GetQuotedValue(targetSegment)})", false, true);
                });
                result = true;
                break;
        }
        return result;
    }
    public virtual bool TryGetTimeOnlyMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter)
    {
        var result = false;
        formatter = null;
        var methodInfo = methodCallExpr.Method;
        var parameterInfos = methodInfo.GetParameters();
        var cacheKey = HashCode.Combine(methodInfo.DeclaringType, methodInfo);
        if (methodInfo.IsStatic)
        {
            switch (methodInfo.Name)
            {
                case "FromTimeSpan":
                    methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(args[0]);
                        if (valueSegment.IsConstantValue)
                            valueSegment.Change(TimeOnly.FromTimeSpan((TimeSpan)valueSegment.Value));

                        return valueSegment.Change($"CAST({this.GetQuotedValue(valueSegment)} AS TIME(7))", false, true);
                    });
                    result = true;
                    break;
                case "FromDateTime":
                    methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(args[0]);
                        if (valueSegment.IsConstantValue)
                            valueSegment.Value = TimeOnly.FromDateTime((DateTime)valueSegment.Value);

                        return valueSegment.Change($"CAST({this.GetQuotedValue(valueSegment)} AS TIME(7))", false, true);
                    });
                    result = true;
                    break;
                case "Parse":
                case "TryParse":
                    methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(args[0]);
                        if (valueSegment.IsConstantValue)
                            return valueSegment.Change(TimeOnly.Parse(valueSegment.ToString()));

                        return valueSegment.Change($"CAST({this.GetQuotedValue(valueSegment)} AS TIME(7))", false, true);
                    });
                    result = true;
                    break;
                case "ParseExact":
                case "TryParseExact":
                    methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var valueSegment = visitor.VisitAndDeferred(args[0]);
                        var formatSegment = visitor.VisitAndDeferred(args[1]);

                        if (valueSegment.IsConstantValue && formatSegment.IsConstantValue)
                            valueSegment.Change(TimeOnly.ParseExact(valueSegment.ToString(), formatSegment.ToString()));

                        return valueSegment.Change($"CAST({this.GetQuotedValue(valueSegment)} AS TIME(7))", false, true);
                    });
                    result = true;
                    break;
            }
        }
        else
        {
            switch (methodInfo.Name)
            {
                case "CompareTo":
                    methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);

                        targetSegment.Merge(rightSegment);
                        return targetSegment.Change($"CASE WHEN ({this.GetQuotedValue(targetSegment)}={this.GetQuotedValue(rightSegment)} THEN 0 WHEN ({this.GetQuotedValue(targetSegment)}>{this.GetQuotedValue(rightSegment)})=1 THEN 1 ELSE -1 END)", false, true);
                    });
                    result = true;
                    break;
                case "Equals":
                    methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);

                        targetSegment.Merge(rightSegment);
                        return targetSegment.Change($"{this.GetQuotedValue(targetSegment)}={this.GetQuotedValue(rightSegment)}", false, true);
                    });
                    result = true;
                    break;
                case "Add":
                    methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var rightSegment = visitor.VisitAndDeferred(args[0]);

                        if (targetSegment.IsConstantValue && rightSegment.IsConstantValue)
                            return targetSegment.Change(((TimeOnly)targetSegment.Value).Add((TimeSpan)rightSegment.Value));

                        targetSegment.Merge(rightSegment);
                        return targetSegment.Change($"TIME('{targetSegment}+{rightSegment}')", false, true);
                    });
                    result = true;
                    break;
                    //case "Subtract":
                    //    methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    //    {
                    //        var targetSegment = visitor.VisitAndDeferred(target);
                    //        var rightSegment = visitor.VisitAndDeferred(args[0]);
                    //        if (targetSegment.IsConstantValue && rightSegment.IsConstantValue)
                    //            return targetSegment.Change(((TimeOnly)targetSegment.Value).Subtract((TimeOnly)rightSegment.Value));

                    //        targetSegment.Merge(rightSegment);
                    //        return targetSegment.Change($"{targetSegment}-{rightSegment}", false, true);
                    //    });
                    //    result = true;
                    //    break;
                    //case "Multiply":
                    //    methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    //    {
                    //        var targetSegment = visitor.VisitAndDeferred(target);
                    //        var rightSegment = visitor.VisitAndDeferred(args[0]);
                    //        if (targetSegment.IsConstantValue && rightSegment.IsConstantValue)
                    //            return targetSegment.Change(((TimeOnly)targetSegment.Value).Multiply((double)rightSegment.Value));

                    //        targetSegment.Merge(rightSegment);
                    //        return targetSegment.Change($"{targetSegment}*{rightSegment}", false, true);
                    //    });
                    //    result = true;
                    //    break;
                    //case "Divide":
                    //    if (parameterInfos[0].ParameterType == typeof(double))
                    //    {
                    //        methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    //        {
                    //            var targetSegment = visitor.VisitAndDeferred(target);
                    //            var rightSegment = visitor.VisitAndDeferred(args[0]);
                    //            if (targetSegment.IsConstantValue && rightSegment.IsConstantValue)
                    //                return targetSegment.Change(((TimeOnly)targetSegment.Value).Divide((double)rightSegment.Value));

                    //            targetSegment.Merge(rightSegment);
                    //            return targetSegment.Change($"{targetSegment}/{rightSegment}", false, true);
                    //        });
                    //        result = true;
                    //    }
                    //    if (parameterInfos[0].ParameterType == typeof(TimeOnly))
                    //    {
                    //        methodCallSqlFormatterCahe.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    //        {
                    //            var targetSegment = visitor.VisitAndDeferred(target);
                    //            var rightSegment = visitor.VisitAndDeferred(args[0]);
                    //            if (targetSegment.IsConstantValue && rightSegment.IsConstantValue)
                    //                return targetSegment.Change(((TimeOnly)targetSegment.Value).Divide((TimeOnly)rightSegment.Value));

                    //            targetSegment.Merge(rightSegment);
                    //            return targetSegment.Change($"{targetSegment}/{rightSegment}", false, true);
                    //        });
                    //        result = true;
                    //    }
                    //    break;
            }
        }
        return result;
    }
}
