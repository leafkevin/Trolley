using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace Trolley;

public delegate SqlSegment MemberAccessSqlFormatter(ISqlVisitor visitor, SqlSegment target);
public delegate SqlSegment MethodCallSqlFormatter(ISqlVisitor visitor, Expression orgExpr, Expression target, Stack<DeferredExpr> DeferredExprs, params Expression[] arguments);

public enum OrmProviderType
{
    Normal,
    MySql,
    SqlServer,
    PostgreSql,
    Oracle,
    Sqlite
}
public interface IOrmProvider
{
    OrmProviderType OrmProviderType { get; }
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(DbContext dbContext, IQueryVisitor visitor);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(DbContext dbContext, IQueryVisitor visitor);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(DbContext dbContext, IQueryVisitor visitor);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(DbContext dbContext, IQueryVisitor visitor);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(DbContext dbContext, IQueryVisitor visitor);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(DbContext dbContext, IQueryVisitor visitor);

    IIncludableQuery<T, TMember> NewIncludableQuery<T, TMember>(DbContext dbContext, IQueryVisitor visitor);
    IIncludableQuery<T1, T2, TMember> NewIncludableQuery<T1, T2, TMember>(DbContext dbContext, IQueryVisitor visitor);
    IIncludableQuery<T1, T2, T3, TMember> NewIncludableQuery<T1, T2, T3, TMember>(DbContext dbContext, IQueryVisitor visitor);
    IIncludableQuery<T1, T2, T3, T4, TMember> NewIncludableQuery<T1, T2, T3, T4, TMember>(DbContext dbContext, IQueryVisitor visitor);
    IIncludableQuery<T1, T2, T3, T4, T5, TMember> NewIncludableQuery<T1, T2, T3, T4, T5, TMember>(DbContext dbContext, IQueryVisitor visitor);
    IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> NewIncludableQuery<T1, T2, T3, T4, T5, T6, TMember>(DbContext dbContext, IQueryVisitor visitor);
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember>(DbContext dbContext, IQueryVisitor visitor);
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember>(DbContext dbContext, IQueryVisitor visitor);
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>(DbContext dbContext, IQueryVisitor visitor);
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>(DbContext dbContext, IQueryVisitor visitor);
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>(DbContext dbContext, IQueryVisitor visitor);
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>(DbContext dbContext, IQueryVisitor visitor);
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>(DbContext dbContext, IQueryVisitor visitor);
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>(DbContext dbContext, IQueryVisitor visitor);
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>(DbContext dbContext, IQueryVisitor visitor);
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember> NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>(DbContext dbContext, IQueryVisitor visitor);

    IGroupingQuery<T, TGrouping> NewGroupQuery<T, TGrouping>(DbContext dbContext, IQueryVisitor visitor);
    IGroupingQuery<T1, T2, TGrouping> NewGroupQuery<T1, T2, TGrouping>(DbContext dbContext, IQueryVisitor visitor);
    IGroupingQuery<T1, T2, T3, TGrouping> NewGroupQuery<T1, T2, T3, TGrouping>(DbContext dbContext, IQueryVisitor visitor);
    IGroupingQuery<T1, T2, T3, T4, TGrouping> NewGroupQuery<T1, T2, T3, T4, TGrouping>(DbContext dbContext, IQueryVisitor visitor);
    IGroupingQuery<T1, T2, T3, T4, T5, TGrouping> NewGroupQuery<T1, T2, T3, T4, T5, TGrouping>(DbContext dbContext, IQueryVisitor visitor);
    IGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> NewGroupQuery<T1, T2, T3, T4, T5, T6, TGrouping>(DbContext dbContext, IQueryVisitor visitor);
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> NewGroupQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping>(DbContext dbContext, IQueryVisitor visitor);
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> NewGroupQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping>(DbContext dbContext, IQueryVisitor visitor);
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> NewGroupQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping>(DbContext dbContext, IQueryVisitor visitor);
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> NewGroupQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping>(DbContext dbContext, IQueryVisitor visitor);
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> NewGroupQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping>(DbContext dbContext, IQueryVisitor visitor);
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> NewGroupQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping>(DbContext dbContext, IQueryVisitor visitor);
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> NewGroupQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping>(DbContext dbContext, IQueryVisitor visitor);
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> NewGroupQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping>(DbContext dbContext, IQueryVisitor visitor);
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping> NewGroupQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping>(DbContext dbContext, IQueryVisitor visitor);
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TGrouping> NewGroupQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TGrouping>(DbContext dbContext, IQueryVisitor visitor);

    ICreate<TEntity> NewCreate<TEntity>(DbContext dbContext);
    ICreated<TEntity> NewCreated<TEntity>(DbContext dbContext, ICreateVisitor visitor);
    IContinuedCreate<TEntity> NewContinuedCreate<TEntity>(DbContext dbContext, ICreateVisitor visitor);

    IUpdate<TEntity> NewUpdate<TEntity>(DbContext dbContext) => new Update<TEntity>(dbContext);
    IUpdated<TEntity> NewUpdated<TEntity>(DbContext dbContext, IUpdateVisitor visitor);
    IContinuedUpdate<TEntity> NewContinuedUpdate<TEntity>(DbContext dbContext, IUpdateVisitor visitor);

    IDelete<TEntity> NewDelete<TEntity>(DbContext dbContext);
    IDeleted<TEntity> NewDeleted<TEntity>(DbContext dbContext, IDeleteVisitor visitor);
    IContinuedDelete<TEntity> NewContinuedDelete<TEntity>(DbContext dbContext, IDeleteVisitor visitor);

    IFromCommand<T> NewFromCommand<T>(DbContext dbContext, IQueryVisitor visitor);
    IFromCommand<T1, T2> NewFromCommand<T1, T2>(DbContext dbContext, IQueryVisitor visitor);
    IFromCommand<T1, T2, T3> NewFromCommand<T1, T2, T3>(DbContext dbContext, IQueryVisitor visitor);
    IFromCommand<T1, T2, T3, T4> NewFromCommand<T1, T2, T3, T4>(DbContext dbContext, IQueryVisitor visitor);
    IFromCommand<T1, T2, T3, T4, T5> NewFromCommand<T1, T2, T3, T4, T5>(DbContext dbContext, IQueryVisitor visitor);
    IFromCommand<T1, T2, T3, T4, T5, T6> NewFromCommand<T1, T2, T3, T4, T5, T6>(DbContext dbContext, IQueryVisitor visitor);

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
    string GetQuotedValue(Type expectType, object value);
    //object ToFieldValue(MemberMap memberMapper, object fieldValue);
    string GetBinaryOperator(ExpressionType nodeType);
    bool TryGetMemberAccessSqlFormatter(MemberExpression memberExpr, out MemberAccessSqlFormatter formatter);
    bool TryGetMethodCallSqlFormatter(MethodCallExpression methodCallExpr, out MethodCallSqlFormatter formatter);
}