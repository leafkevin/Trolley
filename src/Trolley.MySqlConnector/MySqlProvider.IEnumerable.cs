using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.MySqlConnector;

partial class MySqlProvider
{
    public override bool TryGetIEnumerableMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter)
    {
        var result = false;
        formatter = null;
        var methodInfo = methodCallExpr.Method;
        var parameterInfos = methodInfo.GetParameters();
        var cacheKey = HashCode.Combine(methodInfo.DeclaringType, methodInfo);
        switch (methodInfo.Name)
        {
            case "Contains":
                //public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value);
                //public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value, IEqualityComparer<TSource>? comparer);
                if (methodInfo.IsStatic && parameterInfos.Length >= 2 && methodInfo.DeclaringType == typeof(Enumerable))
                {
                    //数组调用
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {                       
                        var arraySegment = visitor.VisitAndDeferred(args[0]);
                        var enumerable = arraySegment.Value as IEnumerable;

                        var builder = new StringBuilder();
                        foreach (var item in enumerable)
                        {
                            if (builder.Length > 0)
                                builder.Append(',');
                            builder.Append(this.GetQuotedValue(item));
                        }

                        var elementSegment = visitor.VisitAndDeferred(args[1]);
                        var element = this.GetQuotedValue(elementSegment);

                        int notIndex = 0;
                        if (deferExprs != null && deferExprs.Count > 0)
                        {
                            while (deferExprs.TryPop(out var deferredExpr))
                            {
                                switch (deferredExpr.OperationType)
                                {
                                    case OperationType.Equal:
                                        continue;
                                    case OperationType.Not:
                                        notIndex++;
                                        break;
                                }
                            }
                        }

                        string notString = notIndex % 2 > 0 ? "NOT " : "";
                        if (builder.Length > 0)
                            return elementSegment.Change($"{element} {notString}IN ({builder})", false, true, false);
                        else return elementSegment.Change("1<>0", false, true, false);
                    });
                    result = true;
                }

                //IEnumerable<T>,List<T>
                //public bool Contains(T item);
                if (!methodInfo.IsStatic && parameterInfos.Length == 1 && methodInfo.DeclaringType.GenericTypeArguments.Length > 0
                     && typeof(IEnumerable<>).MakeGenericType(methodInfo.DeclaringType.GenericTypeArguments[0]).IsAssignableFrom(methodInfo.DeclaringType))
                {
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var builder = new StringBuilder();
                        var targetSegment = visitor.VisitAndDeferred(target);
                        var elementSegment = visitor.VisitAndDeferred(args[0]);
                        var enumerable = targetSegment.Value as IEnumerable;

                        foreach (var item in enumerable)
                        {
                            if (builder.Length > 0)
                                builder.Append(',');
                            builder.Append(this.GetQuotedValue(item));
                        }
                        string element = this.GetQuotedValue(elementSegment);

                        int notIndex = 0;
                        if (deferExprs != null && deferExprs.Count > 0)
                        {
                            while (deferExprs.TryPop(out var deferredExpr))
                            {
                                switch (deferredExpr.OperationType)
                                {
                                    case OperationType.Equal:
                                        continue;
                                    case OperationType.Not:
                                        notIndex++;
                                        break;
                                }
                            }
                        }

                        string notString = notIndex % 2 > 0 ? "NOT " : "";
                        if (builder.Length > 0)
                            return elementSegment.Change($"{element} {notString}IN ({builder})", false, true, false);
                        else return elementSegment.Change("1<>0", false, true, false);
                    });
                    return true;
                }
                break;
            case "Reverse":
                if (!methodInfo.IsStatic && parameterInfos.Length == 1 && methodInfo.DeclaringType == typeof(Enumerable) && methodInfo.DeclaringType.GenericTypeArguments.Length > 0
                     && methodInfo.DeclaringType.GenericTypeArguments[0] == typeof(char))
                {
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(target);
                        return target.Change($"REVERSE({this.GetQuotedValue(targetSegment)})", false, false, true);
                    });
                    result = true;
                }
                break;
        }
        return result;
    }
}
