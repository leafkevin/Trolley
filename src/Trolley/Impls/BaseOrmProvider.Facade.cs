using System;

namespace Trolley;

partial class BaseOrmProvider
{
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
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(DbContext dbContext, IQueryVisitor visitor) => new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(dbContext, visitor);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(DbContext dbContext, IQueryVisitor visitor) => new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(dbContext, visitor);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(DbContext dbContext, IQueryVisitor visitor) => new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(dbContext, visitor);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(DbContext dbContext, IQueryVisitor visitor) => new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(dbContext, visitor);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(DbContext dbContext, IQueryVisitor visitor) => new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(dbContext, visitor);
    public virtual IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(DbContext dbContext, IQueryVisitor visitor) => new Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(dbContext, visitor);

    public virtual IGroupingQuery<T, TGrouping> NewGroupQuery<T, TGrouping>(DbContext dbContext, IQueryVisitor visitor) => new GroupingQuery<T, TGrouping>(dbContext, visitor);
    public virtual IGroupingQuery<T1, T2, TGrouping> NewGroupQuery<T1, T2, TGrouping>(DbContext dbContext, IQueryVisitor visitor) => new GroupingQuery<T1, T2, TGrouping>(dbContext, visitor);
    public virtual IGroupingQuery<T1, T2, T3, TGrouping> NewGroupQuery<T1, T2, T3, TGrouping>(DbContext dbContext, IQueryVisitor visitor) => new GroupingQuery<T1, T2, T3, TGrouping>(dbContext, visitor);
    public virtual IGroupingQuery<T1, T2, T3, T4, TGrouping> NewGroupQuery<T1, T2, T3, T4, TGrouping>(DbContext dbContext, IQueryVisitor visitor) => new GroupingQuery<T1, T2, T3, T4, TGrouping>(dbContext, visitor);
    public virtual IGroupingQuery<T1, T2, T3, T4, T5, TGrouping> NewGroupQuery<T1, T2, T3, T4, T5, TGrouping>(DbContext dbContext, IQueryVisitor visitor) => new GroupingQuery<T1, T2, T3, T4, T5, TGrouping>(dbContext, visitor);
    public virtual IGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> NewGroupQuery<T1, T2, T3, T4, T5, T6, TGrouping>(DbContext dbContext, IQueryVisitor visitor) => new GroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping>(dbContext, visitor);
    public virtual IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> NewGroupQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping>(DbContext dbContext, IQueryVisitor visitor) => new GroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping>(dbContext, visitor);
    public virtual IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> NewGroupQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping>(DbContext dbContext, IQueryVisitor visitor) => new GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping>(dbContext, visitor);
    public virtual IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> NewGroupQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping>(DbContext dbContext, IQueryVisitor visitor) => new GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping>(dbContext, visitor);
    public virtual IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> NewGroupQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping>(DbContext dbContext, IQueryVisitor visitor) => new GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping>(dbContext, visitor);
    public virtual IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> NewGroupQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping>(DbContext dbContext, IQueryVisitor visitor) => new GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping>(dbContext, visitor);
    public virtual IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> NewGroupQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping>(DbContext dbContext, IQueryVisitor visitor) => new GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping>(dbContext, visitor);
    public virtual IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> NewGroupQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping>(DbContext dbContext, IQueryVisitor visitor) => new GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping>(dbContext, visitor);
    public virtual IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> NewGroupQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping>(DbContext dbContext, IQueryVisitor visitor) => new GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping>(dbContext, visitor);
    public virtual IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping> NewGroupQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping>(DbContext dbContext, IQueryVisitor visitor) => new GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping>(dbContext, visitor);
    public virtual IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TGrouping> NewGroupQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TGrouping>(DbContext dbContext, IQueryVisitor visitor) => new GroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TGrouping>(dbContext, visitor);

    public virtual IMultipleQuery NewMultipleQuery(DbContext dbContext, ITheaCommand command) => new MultipleQuery(dbContext, command);

    public virtual IMultiQuery<T> NewMultiQuery<T>(IMultipleQuery multiQuery, IQueryVisitor visitor) => new MultiQuery<T>(multiQuery, visitor);
    public virtual IMultiQuery<T1, T2> NewMultiQuery<T1, T2>(IMultipleQuery multiQuery, IQueryVisitor visitor) => new MultiQuery<T1, T2>(multiQuery, visitor);
    public virtual IMultiQuery<T1, T2, T3> NewMultiQuery<T1, T2, T3>(IMultipleQuery multiQuery, IQueryVisitor visitor) => new MultiQuery<T1, T2, T3>(multiQuery, visitor);
    public virtual IMultiQuery<T1, T2, T3, T4> NewMultiQuery<T1, T2, T3, T4>(IMultipleQuery multiQuery, IQueryVisitor visitor) => new MultiQuery<T1, T2, T3, T4>(multiQuery, visitor);
    public virtual IMultiQuery<T1, T2, T3, T4, T5> NewMultiQuery<T1, T2, T3, T4, T5>(IMultipleQuery multiQuery, IQueryVisitor visitor) => new MultiQuery<T1, T2, T3, T4, T5>(multiQuery, visitor);
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6> NewMultiQuery<T1, T2, T3, T4, T5, T6>(IMultipleQuery multiQuery, IQueryVisitor visitor) => new MultiQuery<T1, T2, T3, T4, T5, T6>(multiQuery, visitor);
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7> NewMultiQuery<T1, T2, T3, T4, T5, T6, T7>(IMultipleQuery multiQuery, IQueryVisitor visitor) => new MultiQuery<T1, T2, T3, T4, T5, T6, T7>(multiQuery, visitor);
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8>(IMultipleQuery multiQuery, IQueryVisitor visitor) => new MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8>(multiQuery, visitor);
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>(IMultipleQuery multiQuery, IQueryVisitor visitor) => new MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>(multiQuery, visitor);
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(IMultipleQuery multiQuery, IQueryVisitor visitor) => new MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(multiQuery, visitor);
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(IMultipleQuery multiQuery, IQueryVisitor visitor) => new MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(multiQuery, visitor);
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(IMultipleQuery multiQuery, IQueryVisitor visitor) => new MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(multiQuery, visitor);
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(IMultipleQuery multiQuery, IQueryVisitor visitor) => new MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(multiQuery, visitor);
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(IMultipleQuery multiQuery, IQueryVisitor visitor) => new MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(multiQuery, visitor);
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(IMultipleQuery multiQuery, IQueryVisitor visitor) => new MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(multiQuery, visitor);
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(IMultipleQuery multiQuery, IQueryVisitor visitor) => new MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(multiQuery, visitor);


    public virtual IMultiGroupingQuery<T, TGrouping> NewMultiGroupingQuery<T, TGrouping>(IMultipleQuery multiQuery, IQueryVisitor visitor) => new MultiGroupingQuery<T, TGrouping>(multiQuery, visitor);
    public virtual IMultiGroupingQuery<T1, T2, TGrouping> NewMultiGroupingQuery<T1, T2, TGrouping>(IMultipleQuery multiQuery, IQueryVisitor visitor) => new MultiGroupingQuery<T1, T2, TGrouping>(multiQuery, visitor);
    public virtual IMultiGroupingQuery<T1, T2, T3, TGrouping> NewMultiGroupingQuery<T1, T2, T3, TGrouping>(IMultipleQuery multiQuery, IQueryVisitor visitor) => new MultiGroupingQuery<T1, T2, T3, TGrouping>(multiQuery, visitor);
    public virtual IMultiGroupingQuery<T1, T2, T3, T4, TGrouping> NewMultiGroupingQuery<T1, T2, T3, T4, TGrouping>(IMultipleQuery multiQuery, IQueryVisitor visitor) => new MultiGroupingQuery<T1, T2, T3, T4, TGrouping>(multiQuery, visitor);
    public virtual IMultiGroupingQuery<T1, T2, T3, T4, T5, TGrouping> NewMultiGroupingQuery<T1, T2, T3, T4, T5, TGrouping>(IMultipleQuery multiQuery, IQueryVisitor visitor) => new MultiGroupingQuery<T1, T2, T3, T4, T5, TGrouping>(multiQuery, visitor);
    public virtual IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> NewMultiGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping>(IMultipleQuery multiQuery, IQueryVisitor visitor) => new MultiGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping>(multiQuery, visitor);
    public virtual IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> NewMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping>(IMultipleQuery multiQuery, IQueryVisitor visitor) => new MultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping>(multiQuery, visitor);
    public virtual IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> NewMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping>(IMultipleQuery multiQuery, IQueryVisitor visitor) => new MultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping>(multiQuery, visitor);
    public virtual IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> NewMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping>(IMultipleQuery multiQuery, IQueryVisitor visitor) => new MultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping>(multiQuery, visitor);
    public virtual IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> NewMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping>(IMultipleQuery multiQuery, IQueryVisitor visitor) => new MultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping>(multiQuery, visitor);
    public virtual IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> NewMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping>(IMultipleQuery multiQuery, IQueryVisitor visitor) => new MultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping>(multiQuery, visitor);
    public virtual IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> NewMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping>(IMultipleQuery multiQuery, IQueryVisitor visitor) => new MultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping>(multiQuery, visitor);
    public virtual IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> NewMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping>(IMultipleQuery multiQuery, IQueryVisitor visitor) => new MultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping>(multiQuery, visitor);
    public virtual IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> NewMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping>(IMultipleQuery multiQuery, IQueryVisitor visitor) => new MultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping>(multiQuery, visitor);
    public virtual IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping> NewMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping>(IMultipleQuery multiQuery, IQueryVisitor visitor) => new MultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping>(multiQuery, visitor);
    public virtual IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TGrouping> NewMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TGrouping>(IMultipleQuery multiQuery, IQueryVisitor visitor) => new MultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TGrouping>(multiQuery, visitor);


    public virtual ICreate NewCreate(Type entityType, DbContext dbContext) => new Create(entityType, dbContext);
    public virtual ICreate<TEntity> NewCreate<TEntity>(DbContext dbContext) => new Create<TEntity>(dbContext);
    public virtual IContinuedCreate NewContinuedCreate(DbContext dbContext, ICreateVisitor visitor) => new ContinuedCreate(dbContext, visitor);
    public virtual IContinuedCreate<TEntity> NewContinuedCreate<TEntity>(DbContext dbContext, ICreateVisitor visitor) => new ContinuedCreate<TEntity>(dbContext, visitor);
    public virtual IBulkContinuedCreate NewBulkContinuedCreate(DbContext dbContext, ICreateVisitor visitor) => new BulkContinuedCreate(dbContext, visitor);
    public virtual IBulkContinuedCreate<TEntity> NewBulkContinuedCreate<TEntity>(DbContext dbContext, ICreateVisitor visitor) => new BulkContinuedCreate<TEntity>(dbContext, visitor);

    public virtual IIdentitiedCreated NewIdentitiedCreated(DbContext dbContext, ICreateVisitor visitor) => new IdentitiedCreated(dbContext, visitor);
    public virtual ICreated NewCreated(DbContext dbContext, ICreateVisitor visitor) => new Created(dbContext, visitor);
    public virtual IFromCreate NewFromCreate(DbContext dbContext, ICreateVisitor visitor) => new FromCreated(dbContext, visitor);
    public virtual IFromCreate<TEntity> NewFromCreate<TEntity>(DbContext dbContext, ICreateVisitor visitor) => new FromCreated<TEntity>(dbContext, visitor);

    public virtual IResultCommand<TResult> NewResultCreated<TResult>(DbContext dbContext, ICreateVisitor visitor) => new ResultCreated<TResult>(dbContext, visitor);
    public virtual IBulkResultCommand<TResult> NewBulkResultCreated<TResult>(DbContext dbContext, ICreateVisitor visitor) => new BulkResultCreated<TResult>(dbContext, visitor);

    public virtual IUpdate NewUpdate(Type entityType, DbContext dbContext) => new Update(entityType, dbContext);
    public virtual IUpdate<TEntity> NewUpdate<TEntity>(DbContext dbContext) => new Update<TEntity>(dbContext);
    public virtual IUpdated NewUpdated(DbContext dbContext, IUpdateVisitor visitor) => new Updated(dbContext, visitor);
    public virtual IBulkResultCommand<TResult> NewResultUpdated<TResult>(DbContext dbContext, IUpdateVisitor visitor) => null;

    public virtual IContinuedUpdate NewContinuedUpdate(DbContext dbContext, IUpdateVisitor visitor) => new ContinuedUpdate(dbContext, visitor);
    public virtual IContinuedUpdate<TEntity> NewContinuedUpdate<TEntity>(DbContext dbContext, IUpdateVisitor visitor) => new ContinuedUpdate<TEntity>(dbContext, visitor);
    public virtual IBulkContinuedUpdate NewBulkContinuedUpdate(DbContext dbContext, IUpdateVisitor visitor) => new BulkContinuedUpdate(dbContext, visitor);
    public virtual IBulkContinuedUpdate<TEntity> NewBulkContinuedUpdate<TEntity>(DbContext dbContext, IUpdateVisitor visitor) => new BulkContinuedUpdate<TEntity>(dbContext, visitor);
    public virtual IBulkCopyContinuedUpdate<TEntity> NewBulkCopyContinuedUpdate<TEntity>(DbContext dbContext, IUpdateVisitor visitor) => new BulkCopyContinuedUpdate<TEntity>(dbContext, visitor);

    public virtual IUpdateJoin<TEntity, T1> NewUpdateJoin<TEntity, T1>(DbContext dbContext, IUpdateVisitor visitor) => new UpdateJoin<TEntity, T1>(dbContext, visitor);
    public virtual IUpdateJoin<TEntity, T1, T2> NewUpdateJoin<TEntity, T1, T2>(DbContext dbContext, IUpdateVisitor visitor) => new UpdateJoin<TEntity, T1, T2>(dbContext, visitor);
    public virtual IUpdateJoin<TEntity, T1, T2, T3> NewUpdateJoin<TEntity, T1, T2, T3>(DbContext dbContext, IUpdateVisitor visitor) => new UpdateJoin<TEntity, T1, T2, T3>(dbContext, visitor);
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4> NewUpdateJoin<TEntity, T1, T2, T3, T4>(DbContext dbContext, IUpdateVisitor visitor) => new UpdateJoin<TEntity, T1, T2, T3, T4>(dbContext, visitor);
    public virtual IUpdateJoin<TEntity, T1, T2, T3, T4, T5> NewUpdateJoin<TEntity, T1, T2, T3, T4, T5>(DbContext dbContext, IUpdateVisitor visitor) => new UpdateJoin<TEntity, T1, T2, T3, T4, T5>(dbContext, visitor);

    public virtual IDelete NewDelete(Type entityType, DbContext dbContext) => new Delete(entityType, dbContext);
    public virtual IDelete<TEntity> NewDelete<TEntity>(DbContext dbContext) => new Delete<TEntity>(dbContext);
    public virtual IDeleted NewDeleted(Type entityType, DbContext dbContext) => new Deleted(entityType, dbContext);
    public virtual IBulkResultCommand<TResult> NewResultDeleted<TResult>(DbContext dbContext, IDeleteVisitor visitor) => new ResultDeleted<TResult>(dbContext, visitor);

    public virtual IQueryVisitor NewQueryVisitor(DbContext dbContext, char tableAsStart = 'a', ITheaCommand command = null) => new QueryVisitor(dbContext, tableAsStart, command);
    public virtual ICreateVisitor NewCreateVisitor(Type entityType, DbContext dbContext, char tableAsStart = 'a', ITheaCommand command = null) => new CreateVisitor(entityType, dbContext, tableAsStart, command);
    public virtual IUpdateVisitor NewUpdateVisitor(Type entityType, DbContext dbContext, char tableAsStart = 'a', ITheaCommand command = null) => new UpdateVisitor(entityType, dbContext, tableAsStart, command);
    public virtual IDeleteVisitor NewDeleteVisitor(Type entityType, DbContext dbContext, char tableAsStart = 'a', ITheaCommand command = null) => new DeleteVisitor(entityType, dbContext, tableAsStart, command);
}