using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Trolley;

public delegate SqlSegment MemberAccessSqlFormatter(ISqlVisitor visitor, SqlSegment target);
public delegate SqlSegment MethodCallSqlFormatter(ISqlVisitor visitor, Expression orgExpr, Expression target, Stack<DeferredExpr> DeferredExprs, params Expression[] arguments);

public interface IOrmProvider
{
    string ParameterPrefix { get; }
    Type NativeDbTypeType { get; }
    IDbConnection CreateConnection(string connectionString);
    IDbDataParameter CreateParameter(string parameterName, object value);
    IDbDataParameter CreateParameter(string parameterName, object nativeDbType, object value);

    IQuery<T> NewQuery<T>(DbContext dbContext, IQueryVisitor visitor);
    IQuery<T1, T2> NewQuery<T1, T2>(DbContext dbContext, IQueryVisitor visitor);
    IQuery<T1, T2, T3> NewQuery<T1, T2, T3>(DbContext dbContext, IQueryVisitor visitor);
    IQuery<T1, T2, T3, T4> NewQuery<T1, T2, T3, T4>(DbContext dbContext, IQueryVisitor visitor);
    IQuery<T1, T2, T3, T4, T5> NewQuery<T1, T2, T3, T4, T5>(DbContext dbContext, IQueryVisitor visitor);
    IQuery<T1, T2, T3, T4, T5, T6> NewQuery<T1, T2, T3, T4, T5, T6>(DbContext dbContext, IQueryVisitor visitor);
    IQuery<T1, T2, T3, T4, T5, T6, T7> NewQuery<T1, T2, T3, T4, T5, T6, T7>(DbContext dbContext, IQueryVisitor visitor);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8> NewQuery<T1, T2, T3, T4, T5, T6, T7, T8>(DbContext dbContext, IQueryVisitor visitor);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>(DbContext dbContext, IQueryVisitor visitor);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(DbContext dbContext, IQueryVisitor visitor);

    ICreate<TEntity> NewCreate<TEntity>(DbContext dbContext);
    ICreated<TEntity> NewCreated<TEntity>(DbContext dbContext, ICreateVisitor visitor);
    IContinuedCreate<TEntity> NewContinuedCreate<TEntity>(DbContext dbContext, ICreateVisitor visitor);

    IUpdate<TEntity> NewUpdate<TEntity>(DbContext dbContext) => new Update<TEntity>(dbContext);
    IUpdated<TEntity> NewUpdated<TEntity>(DbContext dbContext, IUpdateVisitor visitor);
    IContinuedUpdate<TEntity> NewContinuedUpdate<TEntity>(DbContext dbContext, IUpdateVisitor visitor);

    IUpdateFrom<TEntity, T1> NewUpdateFrom<TEntity, T1>(DbContext dbContext, IUpdateVisitor visitor);
    IUpdateFrom<TEntity, T1, T2> NewUpdateFrom<TEntity, T1, T2>(DbContext dbContext, IUpdateVisitor visitor);
    IUpdateFrom<TEntity, T1, T2, T3> NewUpdateFrom<TEntity, T1, T2, T3>(DbContext dbContext, IUpdateVisitor visitor);
    IUpdateFrom<TEntity, T1, T2, T3, T4> NewUpdateFrom<TEntity, T1, T2, T3, T4>(DbContext dbContext, IUpdateVisitor visitor);
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> NewUpdateFrom<TEntity, T1, T2, T3, T4, T5>(DbContext dbContext, IUpdateVisitor visitor);

    IUpdateJoin<TEntity, T1> NewUpdateJoin<TEntity, T1>(DbContext dbContext, IUpdateVisitor visitor);
    IUpdateJoin<TEntity, T1, T2> NewUpdateJoin<TEntity, T1, T2>(DbContext dbContext, IUpdateVisitor visitor);
    IUpdateJoin<TEntity, T1, T2, T3> NewUpdateJoin<TEntity, T1, T2, T3>(DbContext dbContext, IUpdateVisitor visitor);
    IUpdateJoin<TEntity, T1, T2, T3, T4> NewUpdateJoin<TEntity, T1, T2, T3, T4>(DbContext dbContext, IUpdateVisitor visitor);
    IUpdateJoin<TEntity, T1, T2, T3, T4, T5> NewUpdateJoin<TEntity, T1, T2, T3, T4, T5>(DbContext dbContext, IUpdateVisitor visitor);

    IDelete<TEntity> NewDelete<TEntity>(DbContext dbContext);
    IDeleted<TEntity> NewDeleted<TEntity>(DbContext dbContext, IDeleteVisitor visitor);
    IContinuedDelete<TEntity> NewContinuedDelete<TEntity>(DbContext dbContext, IDeleteVisitor visitor);

    IQueryVisitor NewQueryVisitor(string dbKey, IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p", IDataParameterCollection dbParameters = null);
    ICreateVisitor NewCreateVisitor(string dbKey, IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p");
    IUpdateVisitor NewUpdateVisitor(string dbKey, IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p");
    IDeleteVisitor NewDeleteVisitor(string dbKey, IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p");

    string GetTableName(string entityName);
    string GetFieldName(string propertyName);
    string GetPagingTemplate(int? skip, int? limit, string orderBy = null);
    object GetNativeDbType(Type type);
    Type MapDefaultType(object nativeDbType);
    string GetIdentitySql(Type entityType);
    string CastTo(Type type, object value);
    string GetQuotedValue(Type fieldType, object value);
    //object ToFieldValue(MemberMap memberMapper, object fieldValue);
    string GetBinaryOperator(ExpressionType nodeType);
    bool TryGetMemberAccessSqlFormatter(MemberExpression memberExpr, out MemberAccessSqlFormatter formatter);
    bool TryGetMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter);
}