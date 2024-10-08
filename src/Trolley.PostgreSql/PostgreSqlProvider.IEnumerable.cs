﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Trolley.PostgreSql;

partial class PostgreSqlProvider
{
    public override bool TryGetIEnumerableMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter)
    {
        var result = false;
        formatter = null;
        var methodInfo = methodCallExpr.Method;
        var parameterInfos = methodInfo.GetParameters();
        var cacheKey = RepositoryHelper.GetCacheKey(methodInfo.DeclaringType, methodInfo);
        switch (methodInfo.Name)
        {
            case "Contains":
                //public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value);
                //public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value, IEqualityComparer<TSource>? comparer);
                if (methodInfo.IsStatic && parameterInfos.Length >= 2)
                {
                    //数组调用
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var elementSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[1] });
                        var arraySegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });

                        if (arraySegment.IsConstant || arraySegment.IsVariable)
                        {
                            var builder = new StringBuilder();
                            var enumerable = arraySegment.Value as IEnumerable;
                            foreach (var item in enumerable)
                            {
                                if (builder.Length > 0)
                                    builder.Append(',');
                                var sqlArgument = visitor.GetQuotedValue(item, arraySegment, elementSegment);
                                builder.Append(sqlArgument);
                            }
                            if (builder.Length > 0)
                            {
                                var notString = deferExprs.IsDeferredNot() ? "NOT " : "";
                                var elementArgument = visitor.GetQuotedValue(elementSegment);
                                return elementSegment.Merge(arraySegment, $"{elementArgument} {notString}IN ({builder})");
                            }
                            else return elementSegment.Change("1<>0");
                        }
                        else if (arraySegment.HasField)
                        {
                            if (deferExprs.IsDeferredNot())
                                throw new NotSupportedException("数组查询不支持非！操作");
                            var elementArgument = visitor.GetQuotedValue(elementSegment);
                            return arraySegment.Merge(elementSegment, $"{arraySegment.Body} @> ARRAY[{elementArgument}]");
                        }
                        else throw new NotSupportedException("不支持的查询操作");
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
                        var elementSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        var targetSegment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = target });

                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                        {
                            var builder = new StringBuilder();
                            var enumerable = targetSegment.Value as IEnumerable;
                            foreach (var item in enumerable)
                            {
                                if (builder.Length > 0)
                                    builder.Append(',');
                                var sqlArgument = visitor.GetQuotedValue(item, targetSegment, elementSegment);
                                builder.Append(sqlArgument);
                            }
                            if (builder.Length > 0)
                            {
                                string elementArgument = visitor.GetQuotedValue(elementSegment);
                                var notString = deferExprs.IsDeferredNot() ? "NOT " : "";
                                return elementSegment.Merge(targetSegment, $"{elementArgument} {notString}IN ({builder})");
                            }
                            else return elementSegment.Change("1<>0");
                        }
                        else if (targetSegment.HasField)
                        {
                            if (deferExprs.IsDeferredNot())
                                throw new NotSupportedException("数组查询不支持非！操作");
                            var elementArgument = visitor.GetQuotedValue(elementSegment);
                            return targetSegment.Merge(elementSegment, $"{targetSegment.Body} @> ARRAY[{elementArgument}]");
                        }
                        else throw new NotSupportedException("不支持的查询操作");
                    });
                    return true;
                }
                break;
            case "Reverse":
                if (!methodInfo.IsStatic && parameterInfos.Length == 1 && methodInfo.DeclaringType.GenericTypeArguments.Length > 0
                    && methodInfo.DeclaringType.GenericTypeArguments[0] == typeof(char))
                {
                    formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var args0Segment = visitor.VisitAndDeferred(new SqlFieldSegment { Expression = args[0] });
                        if (args0Segment.IsConstant || args0Segment.IsVariable)
                            return args0Segment.ChangeValue(methodInfo.Invoke(args0Segment.Value, null));
                        return args0Segment.Change($"REVERSE({args0Segment.Body})", false, true);
                    });
                    result = true;
                }
                break;
        }
        return result;
    }
}