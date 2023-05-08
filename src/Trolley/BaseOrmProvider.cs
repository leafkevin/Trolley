using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

public abstract class BaseOrmProvider : IOrmProvider
{
    protected static ConcurrentDictionary<int, MemberAccessSqlFormatter> memberAccessSqlFormatterCache = new();
    protected static ConcurrentDictionary<int, MethodCallSqlFormatter> methodCallSqlFormatterCache = new();
    protected static ConcurrentDictionary<int, Func<object, object[], object>> indexMethodCallCache = new();

    public virtual string ParameterPrefix => "@";
    public virtual string SelectIdentitySql => ";SELECT @@IDENTITY";
    public abstract Type NativeDbTypeType { get; }
    public abstract IDbConnection CreateConnection(string connectionString);
    public abstract IDbDataParameter CreateParameter(string parameterName, object value);
    public abstract IDbDataParameter CreateParameter(string parameterName, object nativeDbType, object value);
    public virtual IQueryVisitor NewQueryVisitor(string dbKey, IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
        => new QueryVisitor(dbKey, this, mapProvider, isParameterized, tableAsStart, parameterPrefix);
    public virtual ICreateVisitor NewCreateVisitor(string dbKey, IEntityMapProvider mapProvider, Type entityType, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
        => new CreateVisitor(dbKey, this, mapProvider, entityType, isParameterized, tableAsStart, parameterPrefix);
    public virtual IUpdateVisitor NewUpdateVisitor(string dbKey, IEntityMapProvider mapProvider, Type entityType, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
        => new UpdateVisitor(dbKey, this, mapProvider, entityType, isParameterized, tableAsStart, parameterPrefix);
    public virtual IDeleteVisitor NewDeleteVisitor(string dbKey, IEntityMapProvider mapProvider, Type entityType, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
        => new DeleteVisitor(dbKey, this, mapProvider, entityType, isParameterized, tableAsStart, parameterPrefix);
    public virtual string GetTableName(string entityName) => entityName;
    public virtual string GetFieldName(string propertyName) => propertyName;
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
    public virtual string GetQuotedValue(Type expectType, object value)
    {
        if (value == null) return "NULL";
        if (expectType == typeof(bool))
            return Convert.ToBoolean(value) ? "1" : "0";
        if (expectType == typeof(string))
            return "'" + value.ToString().Replace("\\", "\\\\").Replace("'", @"\'") + "'";
        if (expectType == typeof(DateTime))
            return "'" + Convert.ToDateTime(value).ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";
        if (value is SqlSegment sqlSegment)
        {
            if (sqlSegment == SqlSegment.Null || !sqlSegment.IsConstantValue)
                return sqlSegment.ToString();
            return this.GetQuotedValue(sqlSegment.Value);
        }
        return value.ToString();
    }
    public virtual object ToFieldValue(object fieldValue, object nativeDbType)
    {
        if (fieldValue == null)
            return DBNull.Value;

        var result = fieldValue;
        var fieldType = fieldValue.GetType();
        if (fieldType.IsNullableType(out var underlyingType))
            result = Convert.ChangeType(result, underlyingType);

        if (nativeDbType != null)
        {
            var defaultType = this.MapDefaultType(nativeDbType);
            if (defaultType == underlyingType)
                return result;

            //Gender? gender = Gender.Male;
            //(int)gender.Value;
            if (underlyingType.IsEnum)
            {
                if (defaultType == typeof(string))
                    result = result.ToString();
                else result = Convert.ChangeType(result, defaultType);
            }
            else if (underlyingType == typeof(Guid))
            {
                if (defaultType == typeof(string))
                    result = result.ToString();
                if (defaultType == typeof(byte[]))
                    result = ((Guid)result).ToByteArray();
            }
            else if (underlyingType == typeof(TimeSpan) || underlyingType == typeof(TimeOnly))
            {
                if (defaultType == typeof(long))
                {
                    if (result is TimeSpan timeSpan)
                        result = timeSpan.Ticks;
                    if (result is TimeOnly timeOnly)
                        result = timeOnly.Ticks;
                }
            }
            else result = Convert.ChangeType(result, defaultType);
        }
        return result;
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
    public virtual bool TryGetMemberAccessSqlFormatter(MemberExpression memberExpr, out MemberAccessSqlFormatter formatter)
    {
        var memberInfo = memberExpr.Member;
        var cacheKey = HashCode.Combine(memberInfo.DeclaringType, memberInfo);
        if (!memberAccessSqlFormatterCache.TryGetValue(cacheKey, out formatter))
        {
            bool result = false;
            if (memberInfo.DeclaringType == typeof(DateTime) && this.TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out formatter))
                return true;
            if (memberInfo.DeclaringType == typeof(TimeSpan) && this.TryGetTimeSpanMemberAccessSqlFormatter(memberExpr, out formatter))
                return true;
            return result;
        }
        return true;
    }
    public virtual bool TryGetMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter)
    {
        var methodInfo = methodCallExpr.Method;
        var parameterInfos = methodInfo.GetParameters();
        var cacheKey = HashCode.Combine(methodInfo.DeclaringType, methodInfo);
        if (!methodCallSqlFormatterCache.TryGetValue(cacheKey, out formatter))
        {
            bool result = false;
            if (methodInfo.DeclaringType == typeof(string) && this.TryGetStringMethodCallSqlFormatter(methodCallExpr, out formatter))
                return true;
            if (methodInfo.DeclaringType == typeof(DateTime) && this.TryGetDateTimeMethodCallSqlFormatter(methodCallExpr, out formatter))
                return true;
            if (methodInfo.DeclaringType == typeof(TimeSpan) && this.TryGetTimeSpanMethodCallSqlFormatter(methodCallExpr, out formatter))
                return true;
            if (methodInfo.DeclaringType == typeof(Convert) && this.TryGetConvertMethodCallSqlFormatter(methodCallExpr, out formatter))
                return true;
            if (this.TryGetIEnumerableMethodCallSqlFormatter(methodCallExpr, out formatter))
                return true;
            if (methodInfo.DeclaringType == typeof(Math) && this.TryGetMathMethodCallSqlFormatter(methodCallExpr, out formatter))
                return true;
            switch (methodInfo.Name)
            {
                case "Equals":
                    if (!methodInfo.IsStatic && parameterInfos.Length == 1)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            var rightSegment = visitor.VisitAndDeferred(target);
                            targetSegment.Merge(rightSegment);
                            return targetSegment.Change($"{this.GetQuotedValue(targetSegment)}={this.GetQuotedValue(rightSegment)}", false, true, false);
                        });
                        result = true;
                    }
                    break;
                case "Compare":
                    if (methodInfo.IsStatic && parameterInfos.Length == 2)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var leftSegment = visitor.VisitAndDeferred(args[0]);
                            var rightSegment = visitor.VisitAndDeferred(args[1]);

                            leftSegment.Merge(rightSegment);
                            return leftSegment.Change($"CASE WHEN {this.GetQuotedValue(leftSegment)}={this.GetQuotedValue(rightSegment)} THEN 0 WHEN {this.GetQuotedValue(leftSegment)}>{this.GetQuotedValue(rightSegment)} THEN 1 ELSE -1 END", false, true, false);
                        });
                        result = true;
                    }
                    break;
                case "CompareTo":
                    if (!methodInfo.IsStatic && parameterInfos.Length == 1)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            var rightSegment = visitor.VisitAndDeferred(args[0]);

                            targetSegment.Merge(rightSegment);
                            return targetSegment.Change($"CASE WHEN {this.GetQuotedValue(targetSegment)}={this.GetQuotedValue(rightSegment)} THEN 0 WHEN {this.GetQuotedValue(targetSegment)}>{this.GetQuotedValue(rightSegment)} THEN 1 ELSE -1 END", false, true, false);
                        });
                        result = true;
                    }
                    break;
                case "ToString":
                    if (!methodInfo.IsStatic && parameterInfos.Length == 0)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            if (targetSegment.IsConstantValue)
                                return targetSegment.Change(targetSegment.ToString());
                            return targetSegment.Change(this.CastTo(typeof(string), this.GetQuotedValue(targetSegment)), false, false, true);
                        });
                        result = true;
                    }
                    break;
                case "Parse":
                case "TryParse":
                    if (!methodInfo.IsStatic && parameterInfos.Length == 1)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            args[0] = visitor.VisitAndDeferred(args[0]);
                            if (args[0].IsConstantValue)
                                return args[0].Change(this.GetQuotedValue(methodInfo.DeclaringType, args[0]));
                            return args[0].Change(this.CastTo(methodInfo.DeclaringType, this.GetQuotedValue(args[0])), false, false, true);
                        });
                        result = true;
                    }
                    break;
                case "get_Item":
                    if (!methodInfo.IsStatic && parameterInfos.Length > 0)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(target);
                            var isConstantValue = targetSegment.IsConstantValue;
                            var targetType = targetSegment.Value.GetType();
                            var arguments = new List<object>();
                            for (int i = 0; i < args.Length; i++)
                            {
                                var argumentSegment = visitor.VisitAndDeferred(args[i]);
                                isConstantValue = isConstantValue && argumentSegment.IsConstantValue;
                                targetSegment.Merge(argumentSegment);
                                arguments.Add(argumentSegment.Value);
                            }
                            if (isConstantValue)
                            {
                                if (!indexMethodCallCache.TryGetValue(cacheKey, out var indexDelegate))
                                {
                                    var listExpr = Expression.Parameter(typeof(object), "list");
                                    var indicesExpr = Expression.Parameter(typeof(object[]), "indices");

                                    var argumentsExpr = new List<Expression>();
                                    for (int i = 0; i < parameterInfos.Length; i++)
                                    {
                                        var indexExpr = Expression.ArrayIndex(indicesExpr, Expression.Constant(i));
                                        var typedIndexExpr = Expression.Convert(indexExpr, parameterInfos[i].ParameterType);
                                        argumentsExpr.Add(typedIndexExpr);
                                    }
                                    var targetExpr = Expression.Convert(listExpr, targetType);
                                    var callExpr = Expression.Call(targetExpr, methodInfo, argumentsExpr.ToArray());
                                    var resultExpr = Expression.Convert(callExpr, typeof(object));

                                    indexDelegate = Expression.Lambda<Func<object, object[], object>>(resultExpr, listExpr, indicesExpr).Compile();
                                    indexMethodCallCache.TryAdd(cacheKey, indexDelegate);
                                }
                                return targetSegment.Change(indexDelegate.Invoke(targetSegment.Value, arguments.ToArray()));
                            }
                            throw new NotSupportedException($"不支持的方法调用,{methodInfo.DeclaringType.FullName}.{methodInfo.Name}");
                        });
                        result = true;
                    }
                    break;
            }
            return result;
        }
        return true;
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
