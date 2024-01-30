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
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var builder = new StringBuilder();
                        var elementSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[1] });
                        var arraySegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });

                        //TODO:数组优化
                        var enumerable = arraySegment.Value as IEnumerable;
                        foreach (var item in enumerable)
                        {
                            if (builder.Length > 0)
                                builder.Append(',');
                            string sqlArgument = null;
                            if (item is SqlSegment sqlSegment)
                                sqlArgument = visitor.GetQuotedValue(sqlSegment);
                            else sqlArgument = visitor.GetQuotedValue(item, arraySegment,
                                elementSegment.TypeHandler, elementSegment.ExpectType ?? elementSegment.UnderlyingType);
                            builder.Append(sqlArgument);
                        }
                        if (builder.Length > 0)
                        {
                            var notString = deferExprs.IsDeferredNot() ? "NOT " : "";
                            var elementArgument = visitor.GetQuotedValue(elementSegment);
                            return elementSegment.Merge(arraySegment, $"{elementArgument} {notString}IN ({builder})", false, false, true);
                        }
                        else return elementSegment.Change("1<>0", false, false, true);
                    });
                    result = true;
                }

                //IEnumerable<T>,List<T>
                //public bool Contains(T item);
                if (!methodInfo.IsStatic && parameterInfos.Length == 1 && methodInfo.DeclaringType.GenericTypeArguments.Length > 0
                     && typeof(IEnumerable<>).MakeGenericType(methodInfo.DeclaringType.GenericTypeArguments[0]).IsAssignableFrom(methodInfo.DeclaringType))
                {
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
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
                            else sqlArgument = visitor.GetQuotedValue(item, targetSegment,
                                elementSegment.TypeHandler, elementSegment.ExpectType ?? elementSegment.UnderlyingType);
                            builder.Append(sqlArgument);
                        }
                        if (builder.Length > 0)
                        {
                            string elementArgument = visitor.GetQuotedValue(elementSegment);
                            var notString = deferExprs.IsDeferredNot() ? "NOT " : "";
                            return elementSegment.Merge(targetSegment, $"{elementArgument} {notString}IN ({builder})", false, false, true);
                        }
                        else return elementSegment.Change("1<>0", false, false, true);
                    });
                    return true;
                }
                break;
            case "Reverse":
                if (!methodInfo.IsStatic && parameterInfos.Length == 1 && methodInfo.DeclaringType == typeof(Enumerable)
                    && methodInfo.DeclaringType.GenericTypeArguments.Length > 0
                    && methodInfo.DeclaringType.GenericTypeArguments[0] == typeof(char))
                {
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var args0Segment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        if (args0Segment.IsConstant || args0Segment.IsVariable)
                            return args0Segment.Change(methodInfo.Invoke(args0Segment.Value, null));
                        return args0Segment.Change($"REVERSE({args0Segment})", false, false, false, true);
                    });
                    result = true;
                }
                break;
        }
        return result;
    }
}
