using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

public abstract class BaseOrmProvider : IOrmProvider
{
    protected static readonly ConcurrentDictionary<int, MemberAccessSqlFormatter> memberAccessSqlFormatterCache = new();
    protected static readonly ConcurrentDictionary<int, MethodCallSqlFormatter> methodCallSqlFormatterCache = new();
    protected static readonly ConcurrentDictionary<int, Delegate> methodCallCache = new();

    public virtual string ParameterPrefix => "@";
    public abstract Type NativeDbTypeType { get; }
    public abstract IDbConnection CreateConnection(string connectionString);
    public abstract IDbDataParameter CreateParameter(string parameterName, object value);
    public abstract IDbDataParameter CreateParameter(string parameterName, object nativeDbType, object value);

    public virtual IQuery<T> NewQuery<T>(DbContext dbContext, IQueryVisitor visitor) => new Query<T>(dbContext, visitor);
    public virtual IQuery<T1, T2> NewQuery<T1, T2>(DbContext dbContext, IQueryVisitor visitor) => new Query<T1, T2>(dbContext, visitor);
    public virtual IQuery<T1, T2, T3> NewQuery<T1, T2, T3>(DbContext dbContext, IQueryVisitor visitor) => new Query<T1, T2, T3>(dbContext, visitor);
    public virtual IQuery<T1, T2, T3, T4> NewQuery<T1, T2, T3, T4>(DbContext dbContext, IQueryVisitor visitor) => new Query<T1, T2, T3, T4>(dbContext, visitor);
    public virtual IQuery<T1, T2, T3, T4, T5> NewQuery<T1, T2, T3, T4, T5>(DbContext dbContext, IQueryVisitor visitor) => new Query<T1, T2, T3, T4, T5>(dbContext, visitor);
    public virtual IQuery<T1, T2, T3, T4, T5, T6> NewQuery<T1, T2, T3, T4, T5, T6>(DbContext dbContext, IQueryVisitor visitor) => new Query<T1, T2, T3, T4, T5, T6>(dbContext, visitor);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7> NewQuery<T1, T2, T3, T4, T5, T6, T7>(DbContext dbContext, IQueryVisitor visitor) => new Query<T1, T2, T3, T4, T5, T6, T7>(dbContext, visitor);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8> NewQuery<T1, T2, T3, T4, T5, T6, T7, T8>(DbContext dbContext, IQueryVisitor visitor) => new Query<T1, T2, T3, T4, T5, T6, T7, T8>(dbContext, visitor);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>(DbContext dbContext, IQueryVisitor visitor) => new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9>(dbContext, visitor);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(DbContext dbContext, IQueryVisitor visitor) => new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(dbContext, visitor);

    public virtual ICreate<TEntity> NewCreate<TEntity>(DbContext dbContext) => new Create<TEntity>(dbContext);
    public virtual ICreated<TEntity> NewCreated<TEntity>(DbContext dbContext, ICreateVisitor visitor) => new Created<TEntity>(dbContext, visitor);
    public virtual IContinuedCreate<TEntity> NewContinuedCreate<TEntity>(DbContext dbContext, ICreateVisitor visitor) => new ContinuedCreate<TEntity>(dbContext, visitor);

    public virtual IUpdate<TEntity> NewUpdate<TEntity>(DbContext dbContext) => new Update<TEntity>(dbContext);
    public virtual IUpdated<TEntity> NewUpdated<TEntity>(DbContext dbContext, IUpdateVisitor visitor) => new Updated<TEntity>(dbContext, visitor);
    public virtual IContinuedUpdate<TEntity> NewContinuedUpdate<TEntity>(DbContext dbContext, IUpdateVisitor visitor) => new ContinuedUpdate<TEntity>(dbContext, visitor);

    public virtual IUpdateFrom<TEntity, T1> NewUpdateFrom<TEntity, T1>(DbContext dbContext, IUpdateVisitor visitor) => new UpdateFrom<TEntity, T1>(dbContext, visitor);
    public virtual IUpdateFrom<TEntity, T1, T2> NewUpdateFrom<TEntity, T1, T2>(DbContext dbContext, IUpdateVisitor visitor) => new UpdateFrom<TEntity, T1, T2>(dbContext, visitor);
    public virtual IUpdateFrom<TEntity, T1, T2, T3> NewUpdateFrom<TEntity, T1, T2, T3>(DbContext dbContext, IUpdateVisitor visitor) => new UpdateFrom<TEntity, T1, T2, T3>(dbContext, visitor);
    public virtual IUpdateFrom<TEntity, T1, T2, T3, T4> NewUpdateFrom<TEntity, T1, T2, T3, T4>(DbContext dbContext, IUpdateVisitor visitor) => new UpdateFrom<TEntity, T1, T2, T3, T4>(dbContext, visitor);
    public virtual IUpdateFrom<TEntity, T1, T2, T3, T4, T5> NewUpdateFrom<TEntity, T1, T2, T3, T4, T5>(DbContext dbContext, IUpdateVisitor visitor) => new UpdateFrom<TEntity, T1, T2, T3, T4, T5>(dbContext, visitor);

    public virtual IUpdateJoin<TEntity, T1> NewUpdateJoin<TEntity, T1>(DbContext dbContext, IUpdateVisitor visitor) => new UpdateJoin<TEntity, T1>(dbContext, visitor);
    public virtual IUpdateJoin<TEntity, T1, T2> NewUpdateJoin<TEntity, T1, T2>(DbContext dbContext, IUpdateVisitor visitor) => new UpdateJoin<TEntity, T1, T2>(dbContext, visitor);
    public virtual IUpdateJoin<TEntity, T1, T2, T3> NewUpdateJoin<TEntity, T1, T2, T3>(DbContext dbContext, IUpdateVisitor visitor) => new UpdateJoin<TEntity, T1, T2, T3>(dbContext, visitor);
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4> NewUpdateJoin<TEntity, T1, T2, T3, T4>(DbContext dbContext, IUpdateVisitor visitor) => new UpdateJoin<TEntity, T1, T2, T3, T4>(dbContext, visitor);
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4, T5> NewUpdateJoin<TEntity, T1, T2, T3, T4, T5>(DbContext dbContext, IUpdateVisitor visitor) => new UpdateJoin<TEntity, T1, T2, T3, T4, T5>(dbContext, visitor);

    public virtual IDelete<TEntity> NewDelete<TEntity>(DbContext dbContext) => new Delete<TEntity>(dbContext);
    public virtual IDeleted<TEntity> NewDeleted<TEntity>(DbContext dbContext, IDeleteVisitor visitor) => new Deleted<TEntity>(dbContext, visitor);
    public virtual IContinuedDelete<TEntity> NewContinuedDelete<TEntity>(DbContext dbContext, IDeleteVisitor visitor) => new ContinuedDelete<TEntity>(dbContext, visitor);

    public virtual IQueryVisitor NewQueryVisitor(string dbKey, IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p", IDataParameterCollection dbParameters = null)
        => new QueryVisitor(dbKey, this, mapProvider, isParameterized, tableAsStart, parameterPrefix, dbParameters);
    public virtual ICreateVisitor NewCreateVisitor(string dbKey, IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
        => new CreateVisitor(dbKey, this, mapProvider, isParameterized, tableAsStart, parameterPrefix);
    public virtual IUpdateVisitor NewUpdateVisitor(string dbKey, IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
        => new UpdateVisitor(dbKey, this, mapProvider, isParameterized, tableAsStart, parameterPrefix);
    public virtual IDeleteVisitor NewDeleteVisitor(string dbKey, IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
        => new DeleteVisitor(dbKey, this, mapProvider, isParameterized, tableAsStart, parameterPrefix);

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
    public virtual string GetIdentitySql(Type entityType) => ";SELECT @@IDENTITY";
    public virtual string GetQuotedValue(Type expectType, object value)
    {
        if (value == null) return "NULL";
        if (expectType == typeof(bool) && value is bool bValue)
            return bValue ? "1" : "0";
        if (expectType == typeof(string) && value is string strValue)
            return $"'{strValue.Replace("\\", "\\\\").Replace("'", @"\'")}'";
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
    //public virtual object ToFieldValue(MemberMap memberMapper, object fieldValue)
    //{
    //    if (fieldValue == null || fieldValue is DBNull) return fieldValue;
    //    if (memberMapper.TypeHandler != null)
    //        return memberMapper.TypeHandler.ToFieldValue(this, fieldValue);

    //    var result = fieldValue;
    //    memberMapper.MemberType.IsNullableType(out var underlyingType);

    //    //模型类型与数据库默认映射类型一致，如：bool,数字，浮点数，String，DateTime，TimeSpan，DateOnly，TimeOnly，Guid等
    //    //通常fieldValue和memberMapper的类型是一致的，不一致表达式无法书写出来
    //    if (memberMapper.DbDefaultType == underlyingType)
    //        return result;

    //    //模型类型与数据库默认映射类型不一致的情况，如：数字，浮点数，TimeSpan，DateOnly，TimeOnly，枚举，Guid
    //    //Gender? gender = Gender.Male;
    //    //(int)gender.Value;
    //    if (underlyingType.IsEnum)
    //    {
    //        if (memberMapper.DbDefaultType == typeof(string))
    //        {
    //            if (result.GetType() != underlyingType)
    //                result = Enum.Parse(underlyingType, result.ToString());
    //            result = result.ToString();
    //        }
    //        else result = Convert.ChangeType(result, memberMapper.DbDefaultType);
    //    }
    //    else if (underlyingType == typeof(Guid))
    //    {
    //        if (memberMapper.DbDefaultType == typeof(string))
    //            result = result.ToString();
    //        if (memberMapper.DbDefaultType == typeof(byte[]))
    //            result = ((Guid)result).ToByteArray();
    //    }
    //    else if (underlyingType == typeof(DateTime))
    //    {
    //        if (memberMapper.DbDefaultType == typeof(long))
    //            result = ((DateTime)result).Ticks;
    //        if (memberMapper.DbDefaultType == typeof(string))
    //            result = ((DateTime)result).ToString("yyyy-MM-dd HH:mm:ss.fffffff");
    //    }
    //    else if (underlyingType == typeof(DateOnly))
    //    {
    //        if (memberMapper.DbDefaultType == typeof(string))
    //            result = ((DateOnly)result).ToString("yyyy-MM-dd");
    //    }
    //    else if (underlyingType == typeof(TimeSpan))
    //    {
    //        var timeSpan = (TimeSpan)result;
    //        if (memberMapper.DbDefaultType == typeof(long))
    //            result = timeSpan.Ticks;
    //        if (memberMapper.DbDefaultType == typeof(string))
    //        {
    //            if (timeSpan.TotalDays > 1)
    //                result = timeSpan.ToString("d\\.hh\\:mm\\:ss\\.fffffff");
    //            else result = ((DateOnly)result).ToString("hh\\:mm\\:ss\\.fffffff");
    //        }
    //    }
    //    else if (underlyingType == typeof(TimeOnly))
    //    {
    //        if (memberMapper.DbDefaultType == typeof(long))
    //            result = ((TimeSpan)result).Ticks;
    //        if (memberMapper.DbDefaultType == typeof(string))
    //            result = ((DateOnly)result).ToString("hh\\:mm\\:ss\\.fffffff");
    //    }
    //    else result = Convert.ChangeType(result, memberMapper.DbDefaultType);
    //    return result;
    //}
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
            if (memberInfo.DeclaringType == typeof(DBNull))
            {
                formatter = (visitor, target) => SqlSegment.Null;
                return true;
            }
            if (memberInfo.DeclaringType == typeof(string) && this.TryGetStringMemberAccessSqlFormatter(memberExpr, out formatter))
                return true;
            if (memberInfo.DeclaringType == typeof(DateTime) && this.TryGetDateTimeMemberAccessSqlFormatter(memberExpr, out formatter))
                return true;
            if (memberInfo.DeclaringType == typeof(TimeSpan) && this.TryGetTimeSpanMemberAccessSqlFormatter(memberExpr, out formatter))
                return true;
            if (memberInfo.DeclaringType == typeof(TimeOnly) && this.TryGetTimeOnlyMemberAccessSqlFormatter(memberExpr, out formatter))
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
            if (methodInfo.DeclaringType == typeof(TimeOnly) && this.TryGetTimeOnlyMethodCallSqlFormatter(methodCallExpr, out formatter))
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
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                            var rightSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                            visitor.ChangeSameType(targetSegment, rightSegment);
                            var targetArgument = this.GetQuotedValue(targetSegment);
                            var rightArgument = this.GetQuotedValue(rightSegment);

                            return visitor.Merge(targetSegment, rightSegment, $"{targetArgument}={rightArgument}", true, false);
                        });
                        result = true;
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
                            var leftArgument = this.GetQuotedValue(leftSegment);
                            var rightArgument = this.GetQuotedValue(rightSegment);
                            return visitor.Merge(leftSegment, rightSegment, $"CASE WHEN {leftArgument}={rightArgument} THEN 0 WHEN {leftArgument}>{rightArgument} THEN 1 ELSE -1 END", true, false);
                        });
                        result = true;
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
                            var targetArgument = this.GetQuotedValue(targetSegment);
                            var rightArgument = this.GetQuotedValue(rightSegment);
                            return visitor.Merge(targetSegment, rightSegment, $"CASE WHEN {targetArgument}={rightArgument} THEN 0 WHEN {targetArgument}>{rightArgument} THEN 1 ELSE -1 END", true, false);
                        });
                        result = true;
                    }
                    break;
                case "ToString":
                    if (!methodInfo.IsStatic && parameterInfos.Length == 0)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            var targetSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = target });
                            if (targetSegment.IsConstant || targetSegment.IsVariable)
                                return visitor.Change(targetSegment, targetSegment.Value.ToString());

                            var targetArgument = this.GetQuotedValue(targetSegment);
                            targetSegment.ExpectType = methodInfo.ReturnType;
                            return visitor.Change(targetSegment, this.CastTo(typeof(string), targetArgument), false, true);
                        });
                        result = true;
                    }
                    break;
                case "Parse":
                    if (methodInfo.IsStatic && methodInfo.DeclaringType == typeof(Enum))
                    {
                        if (parameterInfos.Length == 1 || parameterInfos[0].ParameterType != typeof(Type))
                        {
                            var enumType = methodInfo.GetGenericArguments()[0];
                            var enumUnderlyingType = enumType.GetEnumUnderlyingType();
                            methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                            {
                                var args0Segment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                                if (args0Segment.IsConstant || args0Segment.IsVariable)
                                    return visitor.Change(args0Segment, Enum.Parse(enumType, args0Segment.Value.ToString()));

                                var args0Argument = this.GetQuotedValue(args0Segment);
                                return visitor.Change(args0Segment, this.CastTo(enumUnderlyingType, args0Argument), false, true);
                            });
                            result = true;
                            break;
                        }
                        if (parameterInfos.Length > 1 && parameterInfos[0].ParameterType == typeof(Type))
                        {
                            var enumType = parameterInfos[0].ParameterType;
                            var enumUnderlyingType = enumType.GetEnumUnderlyingType();
                            methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                            {
                                SqlSegment resultSegment = null;
                                var argumentSegments = new List<SqlSegment>();
                                Array.ForEach(args, f =>
                                {
                                    var sqlSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = f });
                                    argumentSegments.Add(sqlSegment);
                                    if (resultSegment == null) resultSegment = sqlSegment;
                                    else resultSegment.Merge(sqlSegment);
                                    resultSegment.IsConstant = resultSegment.IsConstant && sqlSegment.IsConstant;
                                    resultSegment.IsVariable = resultSegment.IsVariable || sqlSegment.IsVariable;
                                });
                                if (resultSegment.IsConstant || resultSegment.IsVariable)
                                {
                                    if (!methodCallCache.TryGetValue(cacheKey, out var parseDelegate))
                                    {
                                        var argumentsExpr = new List<ParameterExpression>();
                                        for (int i = 0; i < args.Length; i++)
                                        {
                                            argumentsExpr.Add(Expression.Parameter(args[i].Type, $"args{i}"));
                                        }
                                        var callExpr = Expression.Call(methodInfo, argumentsExpr.ToArray());
                                        parseDelegate = Expression.Lambda(callExpr, argumentsExpr).Compile();
                                        methodCallCache.TryAdd(cacheKey, parseDelegate);
                                    }
                                    var arguments = argumentSegments.Select(f => f.Value).ToArray();
                                    return visitor.Change(resultSegment, parseDelegate.DynamicInvoke(arguments));
                                }
                                var valueArgument = this.GetQuotedValue(argumentSegments[1]);
                                return visitor.Change(resultSegment, this.CastTo(methodInfo.DeclaringType, valueArgument), false, true);
                            });
                            result = true;
                            break;
                        }
                    }
                    if (methodInfo.IsStatic && parameterInfos.Length >= 1)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            SqlSegment resultSegment = null;
                            var argumentSegments = new List<SqlSegment>();
                            Array.ForEach(args, f =>
                            {
                                var sqlSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = f });
                                argumentSegments.Add(sqlSegment);
                                if (resultSegment == null) resultSegment = sqlSegment;
                                else resultSegment.Merge(sqlSegment);
                                resultSegment.IsConstant = resultSegment.IsConstant && sqlSegment.IsConstant;
                                resultSegment.IsVariable = resultSegment.IsVariable || sqlSegment.IsVariable;
                            });
                            if (resultSegment.IsConstant || resultSegment.IsVariable)
                            {
                                if (!methodCallCache.TryGetValue(cacheKey, out var parseDelegate))
                                {
                                    var argumentsExpr = new List<ParameterExpression>();
                                    for (int i = 0; i < args.Length; i++)
                                    {
                                        argumentsExpr.Add(Expression.Parameter(args[i].Type, $"args{i}"));
                                    }
                                    var callExpr = Expression.Call(methodInfo, argumentsExpr.ToArray());
                                    parseDelegate = Expression.Lambda(callExpr, argumentsExpr).Compile();
                                    methodCallCache.TryAdd(cacheKey, parseDelegate);
                                }
                                var arguments = argumentSegments.Select(f => f.Value).ToArray();
                                return visitor.Change(resultSegment, parseDelegate.DynamicInvoke(arguments));
                            }
                            var valueArgument = this.GetQuotedValue(argumentSegments[1]);
                            return visitor.Change(resultSegment, this.CastTo(methodInfo.DeclaringType, valueArgument), false, true);
                        });
                        result = true;
                    }
                    break;
                case "TryParse":
                    if (methodInfo.IsStatic && methodInfo.DeclaringType == typeof(Enum))
                    {
                        if (parameterInfos.Length == 1 || parameterInfos[0].ParameterType != typeof(Type))
                        {
                            var enumType = methodInfo.GetGenericArguments()[0];
                            var enumUnderlyingType = enumType.GetEnumUnderlyingType();
                            methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                            {
                                var args0Segment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                                if (args0Segment.IsConstant || args0Segment.IsVariable)
                                    return visitor.Change(args0Segment, Enum.Parse(enumType, args0Segment.Value.ToString()));

                                var args0Argument = this.GetQuotedValue(args0Segment);
                                return visitor.Change(args0Segment, this.CastTo(enumUnderlyingType, args0Argument), false, true);
                            });
                            result = true;
                            break;
                        }
                        if (parameterInfos.Length > 1 && parameterInfos[0].ParameterType == typeof(Type))
                        {
                            var enumType = parameterInfos[0].ParameterType;
                            var enumUnderlyingType = enumType.GetEnumUnderlyingType();
                            methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                            {
                                SqlSegment resultSegment = null;
                                var argumentSegments = new List<SqlSegment>();
                                for (int i = 0; i < args.Length - 1; i++)
                                {
                                    var sqlSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[i] });
                                    argumentSegments.Add(sqlSegment);
                                    if (resultSegment == null) resultSegment = sqlSegment;
                                    else resultSegment.Merge(sqlSegment);
                                    resultSegment.IsConstant = resultSegment.IsConstant && sqlSegment.IsConstant;
                                    resultSegment.IsVariable = resultSegment.IsVariable || sqlSegment.IsVariable;
                                }
                                if (resultSegment.IsConstant || resultSegment.IsVariable)
                                {
                                    if (!methodCallCache.TryGetValue(cacheKey, out var parseDelegate))
                                    {
                                        var argTypes = new List<Type>();
                                        var argumentExprs = new List<ParameterExpression>();
                                        for (int i = 0; i < args.Length - 1; i++)
                                        {
                                            argTypes.Add(args[i].Type);
                                            argumentExprs.Add(Expression.Parameter(args[i].Type, $"args{i}"));
                                        }
                                        var parseMethodInfo = typeof(Enum).GetMethod(nameof(Enum.Parse), argTypes.ToArray());
                                        var callExpr = Expression.Call(parseMethodInfo, argumentExprs.ToArray());
                                        parseDelegate = Expression.Lambda(callExpr, argumentExprs).Compile();
                                        methodCallCache.TryAdd(cacheKey, parseDelegate);
                                    }
                                    var arguments = argumentSegments.Select(f => f.Value).ToArray();
                                    return visitor.Change(resultSegment, parseDelegate.DynamicInvoke(arguments));
                                }
                                var valueArgument = this.GetQuotedValue(argumentSegments[1]);
                                return visitor.Change(resultSegment, this.CastTo(enumUnderlyingType, valueArgument), false, true);
                            });
                            result = true;
                            break;
                        }
                    }
                    if (methodInfo.IsStatic && parameterInfos.Length >= 1)
                    {
                        methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                        {
                            SqlSegment resultSegment = null;
                            var argumentSegments = new List<SqlSegment>();
                            for (int i = 0; i < args.Length - 1; i++)
                            {
                                var sqlSegment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[i] });
                                argumentSegments.Add(sqlSegment);
                                if (resultSegment == null) resultSegment = sqlSegment;
                                else resultSegment.Merge(sqlSegment);
                                resultSegment.IsConstant = resultSegment.IsConstant && sqlSegment.IsConstant;
                                resultSegment.IsVariable = resultSegment.IsVariable || sqlSegment.IsVariable;
                            }
                            if (resultSegment.IsConstant || resultSegment.IsVariable)
                            {
                                if (!methodCallCache.TryGetValue(cacheKey, out var parseDelegate))
                                {
                                    var argTypes = new List<Type>();
                                    var argumentExprs = new List<ParameterExpression>();
                                    for (int i = 0; i < args.Length - 1; i++)
                                    {
                                        argTypes.Add(args[i].Type);
                                        argumentExprs.Add(Expression.Parameter(args[i].Type, $"args{i}"));
                                    }
                                    var parseMethodInfo = methodInfo.DeclaringType.GetMethod("Parse", argTypes.ToArray());
                                    var callExpr = Expression.Call(parseMethodInfo, argumentExprs.ToArray());
                                    parseDelegate = Expression.Lambda(callExpr, argumentExprs).Compile();
                                    methodCallCache.TryAdd(cacheKey, parseDelegate);
                                }
                                var arguments = argumentSegments.Select(f => f.Value).ToArray();
                                return visitor.Change(resultSegment, parseDelegate.DynamicInvoke(arguments));
                            }
                            var valueArgument = this.GetQuotedValue(argumentSegments[0]);
                            return visitor.Change(resultSegment, this.CastTo(methodInfo.DeclaringType, valueArgument), false, true);
                        });
                        result = true;
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
                            {
                                if (!methodCallCache.TryGetValue(cacheKey, out var indexDelegate))
                                {
                                    var targetType = methodInfo.DeclaringType;
                                    var listObjExpr = Expression.Parameter(typeof(object), "listObj");
                                    var indicesExpr = Expression.Parameter(typeof(object[]), "indices");

                                    var argumentsExpr = new List<Expression>();
                                    for (int i = 0; i < parameterInfos.Length; i++)
                                    {
                                        var indexExpr = Expression.ArrayIndex(indicesExpr, Expression.Constant(i));
                                        var typedIndexExpr = Expression.Convert(indexExpr, parameterInfos[i].ParameterType);
                                        argumentsExpr.Add(typedIndexExpr);
                                    }
                                    var targetExpr = Expression.Convert(listObjExpr, targetType);
                                    var callExpr = Expression.Call(targetExpr, methodInfo, argumentsExpr.ToArray());
                                    var resultExpr = Expression.Convert(callExpr, typeof(object));

                                    indexDelegate = Expression.Lambda<Func<object, object[], object>>(resultExpr, listObjExpr, indicesExpr).Compile();
                                    methodCallCache.TryAdd(cacheKey, indexDelegate);
                                }
                                var indexToValue = indexDelegate as Func<object, object[], object>;
                                return visitor.Change(targetSegment, indexToValue.Invoke(targetSegment.Value, arguments.ToArray()));
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
