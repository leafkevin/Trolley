using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

public abstract partial class BaseOrmProvider : IOrmProvider
{
    protected static readonly ConcurrentDictionary<int, MemberAccessSqlFormatter> memberAccessSqlFormatterCache = new();
    protected static readonly ConcurrentDictionary<int, MethodCallSqlFormatter> methodCallSqlFormatterCache = new();
    protected static readonly ConcurrentDictionary<int, Delegate> methodCallCache = new();

    public virtual OrmProviderType OrmProviderType => OrmProviderType.Basic;
    public virtual string ParameterPrefix => "@";
    public abstract Type NativeDbTypeType { get; }
    public abstract ICollection<ITypeHandler> TypeHandlers { get; }
    public abstract IDbConnection CreateConnection(string connectionString);
    public abstract IDbDataParameter CreateParameter(string parameterName, object value);
    public abstract IDbDataParameter CreateParameter(string parameterName, object nativeDbType, object value);
    public abstract IRepository CreateRepository(DbContext dbContext);

    public virtual string GetTableName(string entityName) => entityName;
    public virtual string GetFieldName(string fieldName) => fieldName;
    public virtual string GetPagingTemplate(int? skip, int? limit, string orderBy = null)
    {
        var builder = new StringBuilder("SELECT /**fields**/ FROM /**tables**/ /**others**/");
        if (!String.IsNullOrEmpty(orderBy)) builder.Append($" {orderBy}");
        if (limit.HasValue) builder.Append($" LIMIT {limit}");
        if (skip.HasValue) builder.Append($" OFFSET {skip}");
        return builder.ToString();
    }
    public abstract object GetNativeDbType(Type type);
    public abstract Type MapDefaultType(object nativeDbType);
    public abstract string CastTo(Type type, object value);
    public virtual string GetIdentitySql(Type entityType) => ";SELECT @@IDENTITY";
    public virtual string GetQuotedValue(Type expectType, object value)
    {
        if (value == null) return "NULL";
        if (expectType == typeof(bool) && value is bool bValue)
            return bValue ? "1" : "0";
        if (expectType == typeof(string) && value is string strValue)
            return $"'{strValue.Replace("'", @"\'")}'";
        if (expectType == typeof(DateTime) && value is DateTime dateTime)
            return $"'{dateTime:yyyy-MM-dd HH:mm:ss.fffffff}'";
        if (expectType == typeof(TimeSpan) && value is TimeSpan timeSpan)
            return $"'{timeSpan.ToString("hh\\:mm\\:ss\\.fffffff")}'";
        if (expectType == typeof(TimeOnly) && value is TimeOnly timeOnly)
            return $"'{timeOnly.ToString("hh\\:mm\\:ss\\.fffffff")}'";
        if (value is SqlSegment sqlSegment)
        {
            if (sqlSegment == SqlSegment.Null || !sqlSegment.IsConstant)
                return sqlSegment.ToString();
            //此处不应出现变量的情况，应该在此之前把变量都已经变成了参数
            if (sqlSegment.IsVariable) throw new Exception("此处不应出现变量的情况，先调用ISqlVisitor.Change方法把变量都变成参数后，再调用本方法");
            return this.GetQuotedValue(sqlSegment.Value);
        }
        return value.ToString();
    }
    public virtual string GetBinaryOperator(ExpressionType nodeType) =>
        nodeType switch
        {
            ExpressionType.Equal => "=",
            ExpressionType.NotEqual => "<>",
            ExpressionType.GreaterThan => ">",
            ExpressionType.GreaterThanOrEqual => ">=",
            ExpressionType.LessThan => "<",
            ExpressionType.LessThanOrEqual => "<=",
            ExpressionType.AndAlso => "AND",
            ExpressionType.OrElse => "OR",
            ExpressionType.Add => "+",
            ExpressionType.Subtract => "-",
            ExpressionType.Multiply => "*",
            ExpressionType.Divide => "/",
            ExpressionType.Modulo => "%",
            ExpressionType.Coalesce => "COALESCE",
            ExpressionType.And => "&",
            ExpressionType.Or => "|",
            ExpressionType.ExclusiveOr => "^",
            ExpressionType.LeftShift => "<<",
            ExpressionType.RightShift => ">>",
            _ => nodeType.ToString()
        };
    public abstract void AddTypeHandler(ITypeHandler typeHandler);
    public abstract ITypeHandler GetTypeHandler(Type targetType, Type fieldType, bool isRequired);
    public virtual bool TryGetMemberAccessSqlFormatter(MemberExpression memberExpr, out MemberAccessSqlFormatter formatter)
    {
        var memberInfo = memberExpr.Member;
        if (memberInfo.DeclaringType == typeof(string) && this.TryGetStringMemberAccessSqlFormatter(memberExpr, out formatter))
            return true;
        if (memberInfo.DeclaringType == typeof(DateTime) && this.TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out formatter))
            return true;
        if (memberInfo.DeclaringType == typeof(TimeSpan) && this.TryGetTimeSpanMemberAccessSqlFormatter(memberExpr, out formatter))
            return true;
        if (memberInfo.DeclaringType == typeof(TimeOnly) && this.TryGetTimeOnlyMemberAccessSqlFormatter(memberExpr, out formatter))
            return true;
        //自定义成员访问解析
        if (this.TryGetMyMemberAccessSqlFormatter(memberExpr, out formatter))
            return true;
        return false;
    }
    public virtual bool TryGetMyMemberAccessSqlFormatter(MemberExpression memberExpr, out MemberAccessSqlFormatter formatter)
    {
        formatter = null;
        return false;
    }
    public virtual bool TryGetMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter)
    {
        var methodInfo = methodCallExpr.Method;
        if (methodInfo.DeclaringType == typeof(string) && this.TryGetStringMethodCallSqlFormatter(methodCallExpr, out formatter))
            return true;
        if (methodInfo.DeclaringType == typeof(DateTime) && this.TryGetDateTimeMethodCallSqlFormatter(methodCallExpr, out formatter))
            return true;
        if (methodInfo.DeclaringType == typeof(TimeSpan) && this.TryGetTimeSpanMethodCallSqlFormatter(methodCallExpr, out formatter))
            return true;
        if (methodInfo.DeclaringType == typeof(TimeOnly) && this.TryGetTimeOnlyMethodCallSqlFormatter(methodCallExpr, out formatter))
            return true;
        if (methodInfo.DeclaringType == typeof(Convert) && this.TryGetConvertMethodCallSqlFormatter(methodCallExpr, out formatter))
            return true;
        if (this.TryGetIEnumerableMethodCallSqlFormatter(methodCallExpr, out formatter))
            return true;
        if (methodInfo.DeclaringType == typeof(Math) && this.TryGetMathMethodCallSqlFormatter(methodCallExpr, out formatter))
            return true;
        //自定义函数解析
        if (this.TryGetMyMethodCallSqlFormatter(methodCallExpr, out formatter))
            return true;

        //兜底函数解析
        var parameterInfos = methodInfo.GetParameters();
        var cacheKey = HashCode.Combine(methodInfo.DeclaringType, methodInfo);
        switch (methodInfo.Name)
        {
            case "Equals":
                if (!methodInfo.IsStatic && parameterInfos.Length == 1)
                {
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        visitor.ChangeSameType(targetSegment, rightSegment);
                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(targetSegment, rightSegment, $"{targetArgument}={rightArgument}", false, false, true);
                    });
                    return true;
                }
                break;
            case "Compare":
                if (methodInfo.IsStatic && parameterInfos.Length == 2)
                {
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var leftSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[1] });
                        visitor.ChangeSameType(leftSegment, rightSegment);
                        var leftArgument = visitor.GetQuotedValue(leftSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return leftSegment.Merge(rightSegment, $"CASE WHEN {leftArgument}={rightArgument} THEN 0 WHEN {leftArgument}>{rightArgument} THEN 1 ELSE -1 END", false, false, true);
                    });
                    return true;
                }
                break;
            case "CompareTo":
                if (!methodInfo.IsStatic && parameterInfos.Length == 1)
                {
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                        visitor.ChangeSameType(targetSegment, rightSegment);
                        var targetArgument = visitor.GetQuotedValue(targetSegment);
                        var rightArgument = visitor.GetQuotedValue(rightSegment);
                        return targetSegment.Merge(rightSegment, $"CASE WHEN {targetArgument}={rightArgument} THEN 0 WHEN {targetArgument}>{rightArgument} THEN 1 ELSE -1 END", false, false, true);
                    });
                    return true;
                }
                break;
            case "ToString":
                if (!methodInfo.IsStatic && parameterInfos.Length == 0)
                {
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                        {
                            targetSegment.ExpectType = methodInfo.ReturnType;
                            return targetSegment.Change(targetSegment.ToString());
                        }
                        targetSegment.ExpectType = methodInfo.ReturnType;
                        return targetSegment.Change(this.CastTo(typeof(string), targetSegment.Value), false, false, false, true);
                    });
                    return true;
                }
                break;
            case "Parse":
                if (methodInfo.IsStatic && methodInfo.DeclaringType == typeof(Enum))
                {
                    if (parameterInfos.Length == 1 || parameterInfos[0].ParameterType != typeof(Type))
                    {
                        var enumType = methodInfo.GetGenericArguments()[0];
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var args0Segment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                            if (args0Segment.IsConstant || args0Segment.IsVariable)
                                return args0Segment.Change(Enum.Parse(enumType, args0Segment.Value.ToString()));

                            throw new NotSupportedException("不支持的表达式访问，Enum.Parse方法只支持常量、变量参数");
                        });
                        return true;
                    }
                    if (parameterInfos.Length > 1 && parameterInfos[0].ParameterType == typeof(Type))
                    {
                        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            SqlSegment resultSegment = null;
                            var arguments = new List<object>();
                            Array.ForEach(args, f =>
                            {
                                var sqlSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = f });
                                arguments.Add(sqlSegment.Value);
                                if (resultSegment == null) resultSegment = sqlSegment;
                                else resultSegment.Merge(sqlSegment);
                            });
                            if (resultSegment.IsConstant || resultSegment.IsVariable)
                                return resultSegment.Change(methodInfo.Invoke(null, arguments.ToArray()));

                            throw new NotSupportedException("不支持的表达式访问，Enum.Parse方法只支持常量、变量参数");
                        });
                        return true;
                    }
                }
                if (methodInfo.IsStatic && parameterInfos.Length >= 1)
                {
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        SqlSegment resultSegment = null;
                        var arguments = new List<object>();
                        Array.ForEach(args, f =>
                        {
                            var sqlSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = f });
                            arguments.Add(sqlSegment);
                            if (resultSegment == null) resultSegment = sqlSegment;
                            else resultSegment.Merge(sqlSegment);
                        });
                        if (resultSegment.IsConstant || resultSegment.IsVariable)
                            return resultSegment.Change(methodInfo.Invoke(null, arguments.ToArray()));

                        throw new NotSupportedException("不支持的表达式访问，Enum.Parse方法只支持常量、变量参数");
                    });
                    return true;
                }
                break;
            case "TryParse":
                if (methodInfo.IsStatic && methodInfo.DeclaringType == typeof(Enum))
                {
                    if (parameterInfos.Length == 1 || parameterInfos[0].ParameterType != typeof(Type))
                    {
                        var enumType = methodInfo.GetGenericArguments()[0];
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var args0Segment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                            if (args0Segment.IsConstant || args0Segment.IsVariable)
                                return args0Segment.Change(Enum.Parse(enumType, args0Segment.Value.ToString()));

                            throw new NotSupportedException("不支持的表达式访问，Enum.Parse方法只支持常量、变量参数");
                        });
                        return true;
                    }
                    if (parameterInfos.Length > 1 && parameterInfos[0].ParameterType == typeof(Type))
                    {
                        var enumType = parameterInfos[0].ParameterType;
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            SqlSegment resultSegment = null;
                            var arguments = new List<object>();
                            for (int i = 0; i < args.Length - 1; i++)
                            {
                                var sqlSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[i] });
                                arguments.Add(sqlSegment.Value);
                                if (resultSegment == null) resultSegment = sqlSegment;
                                else resultSegment.Merge(sqlSegment);
                            }
                            if (resultSegment.IsConstant || resultSegment.IsVariable)
                                return resultSegment.Change(methodInfo.Invoke(null, arguments.ToArray()));

                            throw new NotSupportedException("不支持的表达式访问，Enum.Parse方法只支持常量、变量参数");
                        });
                        return true;
                    }
                }
                if (methodInfo.IsStatic && parameterInfos.Length >= 1)
                {
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        SqlSegment resultSegment = null;
                        var arguments = new List<object>();
                        for (int i = 0; i < args.Length - 1; i++)
                        {
                            var sqlSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[i] });
                            arguments.Add(sqlSegment);
                            if (resultSegment == null) resultSegment = sqlSegment;
                            else resultSegment.Merge(sqlSegment);
                        }
                        if (resultSegment.IsConstant || resultSegment.IsVariable)
                            return resultSegment.Change(methodInfo.Invoke(null, arguments.ToArray()));

                        throw new NotSupportedException("不支持的表达式访问，Enum.Parse方法只支持常量、变量参数");
                    });
                    return true;
                }
                break;
            case "get_Item":
                if (!methodInfo.IsStatic && parameterInfos.Length > 0)
                {
                    methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                    {
                        var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                        var arguments = new List<object>();
                        for (int i = 0; i < args.Length; i++)
                        {
                            var argumentSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[i] });
                            targetSegment.Merge(argumentSegment);
                            arguments.Add(argumentSegment.Value);
                        }
                        if (targetSegment.IsConstant || targetSegment.IsVariable)
                            return targetSegment.Change(methodInfo.Invoke(targetSegment.Value, arguments.ToArray()));

                        throw new NotSupportedException("不支持的表达式访问，get_Item索引方法只支持常量、变量参数");
                    });
                    return true;
                }
                break;
                //case "Exists":
                //    if (parameterInfos.Length == 1 && methodInfo.DeclaringType.GenericTypeArguments.Length > 0
                //        && parameterInfos[0].ParameterType == typeof(Predicate<>).MakeGenericType(methodInfo.DeclaringType.GenericTypeArguments[0]))
                //    {
                //        formatter = methodCallSqlFormatterCache.GetOrAdd(cacheKey, (visitor, orgExpr, target, deferExprs, args) =>
                //        {
                //            args[0].GetParameters(out var parameters);
                //            var lambdaExpr = args[0] as LambdaExpression;
                //            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                //            if (!targetSegment.IsConstant && !targetSegment.IsVariable)
                //                throw new NotSupportedException($"不支持的表达式访问，Exists方法只支持常量和变量的解析，Path:{orgExpr}");
                //            if (parameters.Count > 1)
                //            {
                //                lambdaExpr = Expression.Lambda(lambdaExpr.Body, parameters);
                //                var targetArray = targetSegment.Value as IEnumerable;                            
                //            }

                //            var args0Segment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                //            if (args0Segment.IsConstant || args0Segment.IsVariable)
                //                return args0Segment.Change(methodInfo.Invoke(args0Segment.Value, null));
                //            return args0Segment.Change($"REVERSE({args0Segment})", false, false, false, true);
                //        });
                //        result = true;
                //    }
                //    break;
        }
        return false;
    }
    public virtual bool TryGetMyMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter)
    {
        formatter = null;
        return false;
    }
    public abstract bool TryGetStringMemberAccessSqlFormatter(MemberExpression memberExpr, out MemberAccessSqlFormatter formatter);
    public abstract bool TryGetStringMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter);
    public abstract bool TryGetDateTimeMemberAccessSqlFormatter(MemberExpression memberExpr, out MemberAccessSqlFormatter formatter);
    public abstract bool TryGetDateTimeMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter);
    public abstract bool TryGetTimeSpanMemberAccessSqlFormatter(MemberExpression memberExpr, out MemberAccessSqlFormatter formatter);
    public abstract bool TryGetTimeSpanMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter);
    public abstract bool TryGetTimeOnlyMemberAccessSqlFormatter(MemberExpression memberExpr, out MemberAccessSqlFormatter formatter);
    public abstract bool TryGetTimeOnlyMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter);
    public abstract bool TryGetConvertMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter);
    public abstract bool TryGetIEnumerableMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter);
    public abstract bool TryGetMathMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter);
}
