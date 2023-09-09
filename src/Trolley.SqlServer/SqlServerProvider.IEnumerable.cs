using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.SqlServer;

partial class SqlServerProvider
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
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var builder = new StringBuilder();
                        var elementSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[1] });
                        var arraySegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });

                        var enumerable = arraySegment.Value as IEnumerable;
                        foreach (var item in enumerable)
                        {
                            if (builder.Length > 0)
                                builder.Append(',');
                            string sqlArgument = null;
                            if (item is SqlSegment sqlSegment)
                                sqlArgument = visitor.GetQuotedValue(sqlSegment);
                            else sqlArgument = visitor.GetQuotedValue(item, arraySegment);
                            builder.Append(sqlArgument);
                        }

                        if (builder.Length > 0)
                        {
                            var elementArgument = visitor.GetQuotedValue(elementSegment);
                            var notString = deferExprs.IsDeferredNot() ? "NOT " : "";
                            return visitor.Merge(elementSegment, arraySegment.ToParameter(visitor), $"{elementArgument} {notString}IN ({builder})", true, false);
                        }
                        else return visitor.Change(elementSegment, "1<>0", true, false);
                    });
                    result = true;
                }

                //IEnumerable<T>,List<T>
                //public bool Contains(T item);
                if (!methodInfo.IsStatic && parameterInfos.Length == 1 && methodInfo.DeclaringType.GenericTypeArguments.Length > 0
                     && typeof(IEnumerable<>).MakeGenericType(methodInfo.DeclaringType.GenericTypeArguments[0]).IsAssignableFrom(methodInfo.DeclaringType))
                {
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var builder = new StringBuilder();
                        var elementSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });

                        var enumerable = targetSegment.Value as IEnumerable;
                        foreach (var item in enumerable)
                        {
                            if (builder.Length > 0)
                                builder.Append(',');
                            string sqlArgument = null;
                            if (item is SqlSegment sqlSegment)
                                sqlArgument = visitor.GetQuotedValue(sqlSegment);
                            else sqlArgument = visitor.GetQuotedValue(item, targetSegment);
                            builder.Append(sqlArgument);
                        }

                        if (builder.Length > 0)
                        {
                            string elementArgument = visitor.GetQuotedValue(elementSegment);
                            var notString = deferExprs.IsDeferredNot() ? "NOT " : "";
                            return visitor.Merge(elementSegment, targetSegment.ToParameter(visitor), $"{elementArgument} {notString}IN ({builder})", true, false);
                        }
                        else return visitor.Change(elementSegment, "1<>0", true, false);
                    });
                    return true;
                }
                break;
            case "Reverse":
                if (!methodInfo.IsStatic && parameterInfos.Length == 1 && methodInfo.DeclaringType == typeof(Enumerable)
                    && methodInfo.DeclaringType.GenericTypeArguments.Length > 0
                    && methodInfo.DeclaringType.GenericTypeArguments[0] == typeof(char))
                {
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var args0Segment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if (args0Segment.IsConstant || args0Segment.IsVariable)
                        {
                            if (!methodCallCache.TryGetValue(cacheKey, out var reverseDelegate))
                            {
                                var sourceExpr = Expression.Parameter(target.Type, "source");
                                var callExpr = Expression.Call(methodInfo, sourceExpr);
                                var resultExpr = Expression.Convert(callExpr, typeof(object));
                                reverseDelegate = Expression.Lambda<Func<object, object>>(resultExpr, sourceExpr).Compile();
                                methodCallCache.TryAdd(cacheKey, reverseDelegate);
                            }
                            var toValue = reverseDelegate as Func<object, object>;
                            args0Segment.Value = toValue.Invoke(args0Segment.Value);
                            return visitor.Change(args0Segment);
                        }
                        return visitor.Change(args0Segment, $"REVERSE({visitor.GetQuotedValue(args0Segment)})", false, true);
                    });
                    result = true;
                }
                break;
        }
        return result;
    }
}
