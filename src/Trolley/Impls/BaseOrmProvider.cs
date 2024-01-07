using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;

namespace Trolley;

public abstract class BaseOrmProvider : IOrmProvider
{
    protected static readonly ConcurrentDictionary<int, MemberAccessSqlFormatter> memberAccessSqlFormatterCache = new();
    protected static readonly ConcurrentDictionary<int, MethodCallSqlFormatter> methodCallSqlFormatterCache = new();
    protected static readonly ConcurrentDictionary<int, Delegate> methodCallCache = new();

    public virtual OrmProviderType OrmProviderType => OrmProviderType.Normal;
    public virtual string ParameterPrefix => "@";
    public abstract Type NativeDbTypeType { get; }
    public abstract IDbConnection CreateConnection(string connectionString);
    public abstract IDbDataParameter CreateParameter(string parameterName, object value);
    public abstract IDbDataParameter CreateParameter(string parameterName, object nativeDbType, object value);
    public abstract IRepository CreateRepository(DbContext dbContext);

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

    public virtual IIncludableQuery<T, TMember> NewIncludableQuery<T, TMember>(DbContext dbContext, IQueryVisitor visitor) => new IncludableQuery<T, TMember>(dbContext, visitor);
    public virtual IIncludableQuery<T1, T2, TMember> NewIncludableQuery<T1, T2, TMember>(DbContext dbContext, IQueryVisitor visitor) => new IncludableQuery<T1, T2, TMember>(dbContext, visitor);
    public virtual IIncludableQuery<T1, T2, T3, TMember> NewIncludableQuery<T1, T2, T3, TMember>(DbContext dbContext, IQueryVisitor visitor) => new IncludableQuery<T1, T2, T3, TMember>(dbContext, visitor);
    public virtual IIncludableQuery<T1, T2, T3, T4, TMember> NewIncludableQuery<T1, T2, T3, T4, TMember>(DbContext dbContext, IQueryVisitor visitor) => new IncludableQuery<T1, T2, T3, T4, TMember>(dbContext, visitor);
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, TMember> NewIncludableQuery<T1, T2, T3, T4, T5, TMember>(DbContext dbContext, IQueryVisitor visitor) => new IncludableQuery<T1, T2, T3, T4, T5, TMember>(dbContext, visitor);
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> NewIncludableQuery<T1, T2, T3, T4, T5, T6, TMember>(DbContext dbContext, IQueryVisitor visitor) => new IncludableQuery<T1, T2, T3, T4, T5, T6, TMember>(dbContext, visitor);
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember>(DbContext dbContext, IQueryVisitor visitor) => new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember>(dbContext, visitor);
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember>(DbContext dbContext, IQueryVisitor visitor) => new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember>(dbContext, visitor);
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>(DbContext dbContext, IQueryVisitor visitor) => new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>(dbContext, visitor);
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>(DbContext dbContext, IQueryVisitor visitor) => new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>(dbContext, visitor);
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>(DbContext dbContext, IQueryVisitor visitor) => new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>(dbContext, visitor);
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>(DbContext dbContext, IQueryVisitor visitor) => new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>(dbContext, visitor);
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>(DbContext dbContext, IQueryVisitor visitor) => new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>(dbContext, visitor);
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>(DbContext dbContext, IQueryVisitor visitor) => new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>(dbContext, visitor);
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>(DbContext dbContext, IQueryVisitor visitor) => new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>(dbContext, visitor);
    public virtual IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember> NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>(DbContext dbContext, IQueryVisitor visitor) => new IncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>(dbContext, visitor);

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

    public virtual IFromCommand<T> NewFromCommand<T>(Type entityType, DbContext dbContext, IQueryVisitor visitor) => new FromCommand<T>(entityType, dbContext, visitor);
    public virtual IFromCommand<T1, T2> NewFromCommand<T1, T2>(Type entityType, DbContext dbContext, IQueryVisitor visitor) => new FromCommand<T1, T2>(entityType, dbContext, visitor);
    public virtual IFromCommand<T1, T2, T3> NewFromCommand<T1, T2, T3>(Type entityType, DbContext dbContext, IQueryVisitor visitor) => new FromCommand<T1, T2, T3>(entityType, dbContext, visitor);
    public virtual IFromCommand<T1, T2, T3, T4> NewFromCommand<T1, T2, T3, T4>(Type entityType, DbContext dbContext, IQueryVisitor visitor) => new FromCommand<T1, T2, T3, T4>(entityType, dbContext, visitor);
    public virtual IFromCommand<T1, T2, T3, T4, T5> NewFromCommand<T1, T2, T3, T4, T5>(Type entityType, DbContext dbContext, IQueryVisitor visitor) => new FromCommand<T1, T2, T3, T4, T5>(entityType, dbContext, visitor);
    public virtual IFromCommand<T1, T2, T3, T4, T5, T6> NewFromCommand<T1, T2, T3, T4, T5, T6>(Type entityType, DbContext dbContext, IQueryVisitor visitor) => new FromCommand<T1, T2, T3, T4, T5, T6>(entityType, dbContext, visitor);

    public virtual IGroupingCommand<T, TGrouping> NewGroupCommand<T, TGrouping>(Type entityType, DbContext dbContext, IQueryVisitor visitor) => new GroupingCommand<T, TGrouping>(entityType, dbContext, visitor);
    public virtual IGroupingCommand<T1, T2, TGrouping> NewGroupCommand<T1, T2, TGrouping>(Type entityType, DbContext dbContext, IQueryVisitor visitor) => new GroupingCommand<T1, T2, TGrouping>(entityType, dbContext, visitor);
    public virtual IGroupingCommand<T1, T2, T3, TGrouping> NewGroupCommand<T1, T2, T3, TGrouping>(Type entityType, DbContext dbContext, IQueryVisitor visitor) => new GroupingCommand<T1, T2, T3, TGrouping>(entityType, dbContext, visitor);
    public virtual IGroupingCommand<T1, T2, T3, T4, TGrouping> NewGroupCommand<T1, T2, T3, T4, TGrouping>(Type entityType, DbContext dbContext, IQueryVisitor visitor) => new GroupingCommand<T1, T2, T3, T4, TGrouping>(entityType, dbContext, visitor);
    public virtual IGroupingCommand<T1, T2, T3, T4, T5, TGrouping> NewGroupCommand<T1, T2, T3, T4, T5, TGrouping>(Type entityType, DbContext dbContext, IQueryVisitor visitor) => new GroupingCommand<T1, T2, T3, T4, T5, TGrouping>(entityType, dbContext, visitor);
    public virtual IGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping> NewGroupCommand<T1, T2, T3, T4, T5, T6, TGrouping>(Type entityType, DbContext dbContext, IQueryVisitor visitor) => new GroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping>(entityType, dbContext, visitor);

    public virtual IMultiQuery<T> NewMultiQuery<T>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiQuery<T>(multiQuery, visitor);
    public virtual IMultiQuery<T1, T2> NewMultiQuery<T1, T2>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiQuery<T1, T2>(multiQuery, visitor);
    public virtual IMultiQuery<T1, T2, T3> NewMultiQuery<T1, T2, T3>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiQuery<T1, T2, T3>(multiQuery, visitor);
    public virtual IMultiQuery<T1, T2, T3, T4> NewMultiQuery<T1, T2, T3, T4>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiQuery<T1, T2, T3, T4>(multiQuery, visitor);
    public virtual IMultiQuery<T1, T2, T3, T4, T5> NewMultiQuery<T1, T2, T3, T4, T5>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiQuery<T1, T2, T3, T4, T5>(multiQuery, visitor);
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6> NewMultiQuery<T1, T2, T3, T4, T5, T6>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiQuery<T1, T2, T3, T4, T5, T6>(multiQuery, visitor);
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7> NewMultiQuery<T1, T2, T3, T4, T5, T6, T7>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiQuery<T1, T2, T3, T4, T5, T6, T7>(multiQuery, visitor);
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8>(multiQuery, visitor);
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>(multiQuery, visitor);
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(multiQuery, visitor);
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(multiQuery, visitor);
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(multiQuery, visitor);
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(multiQuery, visitor);
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(multiQuery, visitor);
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(multiQuery, visitor);
    public virtual IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> NewMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(multiQuery, visitor);

    public virtual IMultiIncludableQuery<T, TMember> NewMultiIncludableQuery<T, TMember>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiIncludableQuery<T, TMember>(multiQuery, visitor);
    public virtual IMultiIncludableQuery<T1, T2, TMember> NewMultiIncludableQuery<T1, T2, TMember>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiIncludableQuery<T1, T2, TMember>(multiQuery, visitor);
    public virtual IMultiIncludableQuery<T1, T2, T3, TMember> NewMultiIncludableQuery<T1, T2, T3, TMember>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiIncludableQuery<T1, T2, T3, TMember>(multiQuery, visitor);
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, TMember> NewMultiIncludableQuery<T1, T2, T3, T4, TMember>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiIncludableQuery<T1, T2, T3, T4, TMember>(multiQuery, visitor);
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, TMember> NewMultiIncludableQuery<T1, T2, T3, T4, T5, TMember>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiIncludableQuery<T1, T2, T3, T4, T5, TMember>(multiQuery, visitor);
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, TMember>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiIncludableQuery<T1, T2, T3, T4, T5, T6, TMember>(multiQuery, visitor);
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember>(multiQuery, visitor);
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember>(multiQuery, visitor);
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>(multiQuery, visitor);
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>(multiQuery, visitor);
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>(multiQuery, visitor);
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>(multiQuery, visitor);
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>(multiQuery, visitor);
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>(multiQuery, visitor);
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>(multiQuery, visitor);
    public virtual IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember> NewMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>(multiQuery, visitor);

    public virtual IMultiGroupingQuery<T, TGrouping> NewMultiGroupQuery<T, TGrouping>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiGroupingQuery<T, TGrouping>(multiQuery, visitor);
    public virtual IMultiGroupingQuery<T1, T2, TGrouping> NewMultiGroupQuery<T1, T2, TGrouping>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiGroupingQuery<T1, T2, TGrouping>(multiQuery, visitor);
    public virtual IMultiGroupingQuery<T1, T2, T3, TGrouping> NewMultiGroupQuery<T1, T2, T3, TGrouping>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiGroupingQuery<T1, T2, T3, TGrouping>(multiQuery, visitor);
    public virtual IMultiGroupingQuery<T1, T2, T3, T4, TGrouping> NewMultiGroupQuery<T1, T2, T3, T4, TGrouping>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiGroupingQuery<T1, T2, T3, T4, TGrouping>(multiQuery, visitor);
    public virtual IMultiGroupingQuery<T1, T2, T3, T4, T5, TGrouping> NewMultiGroupQuery<T1, T2, T3, T4, T5, TGrouping>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiGroupingQuery<T1, T2, T3, T4, T5, TGrouping>(multiQuery, visitor);
    public virtual IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> NewMultiGroupQuery<T1, T2, T3, T4, T5, T6, TGrouping>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping>(multiQuery, visitor);
    public virtual IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> NewMultiGroupQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping>(multiQuery, visitor);
    public virtual IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> NewMultiGroupQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping>(multiQuery, visitor);
    public virtual IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> NewMultiGroupQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping>(multiQuery, visitor);
    public virtual IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> NewMultiGroupQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping>(multiQuery, visitor);
    public virtual IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> NewMultiGroupQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping>(multiQuery, visitor);
    public virtual IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> NewMultiGroupQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping>(multiQuery, visitor);
    public virtual IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> NewMultiGroupQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping>(multiQuery, visitor);
    public virtual IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> NewMultiGroupQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping>(multiQuery, visitor);
    public virtual IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping> NewMultiGroupQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping>(multiQuery, visitor);
    public virtual IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TGrouping> NewMultiGroupQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TGrouping>(MultipleQuery multiQuery, IQueryVisitor visitor) => new MultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TGrouping>(multiQuery, visitor);


    public virtual ICreate<TEntity> NewCreate<TEntity>(DbContext dbContext) => new Create<TEntity>(dbContext);
    public virtual ICreated<TEntity> NewCreated<TEntity>(DbContext dbContext, ICreateVisitor visitor) => new Created<TEntity>(dbContext, visitor);
    public virtual IContinuedCreate<TEntity> NewContinuedCreate<TEntity>(DbContext dbContext, ICreateVisitor visitor) => new ContinuedCreate<TEntity>(dbContext, visitor);

    public virtual IUpdate<TEntity> NewUpdate<TEntity>(DbContext dbContext) => new Update<TEntity>(dbContext);
    public virtual IUpdated<TEntity> NewUpdated<TEntity>(DbContext dbContext, IUpdateVisitor visitor) => new Updated<TEntity>(dbContext, visitor);
    public virtual IContinuedUpdate<TEntity> NewContinuedUpdate<TEntity>(DbContext dbContext, IUpdateVisitor visitor) => new ContinuedUpdate<TEntity>(dbContext, visitor);

    public virtual IDelete<TEntity> NewDelete<TEntity>(DbContext dbContext) => new Delete<TEntity>(dbContext);
    public virtual IDeleted<TEntity> NewDeleted<TEntity>(DbContext dbContext, IDeleteVisitor visitor) => new Deleted<TEntity>(dbContext, visitor);
    public virtual IContinuedDelete<TEntity> NewContinuedDelete<TEntity>(DbContext dbContext, IDeleteVisitor visitor) => new ContinuedDelete<TEntity>(dbContext, visitor);

    public virtual IQueryVisitor NewQueryVisitor(string dbKey, IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p", IDataParameterCollection dbParameters = null) => new QueryVisitor(dbKey, this, mapProvider, isParameterized, tableAsStart, parameterPrefix, dbParameters);
    public virtual ICreateVisitor NewCreateVisitor(string dbKey, IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p") => new CreateVisitor(dbKey, this, mapProvider, isParameterized, tableAsStart, parameterPrefix);
    public virtual IUpdateVisitor NewUpdateVisitor(string dbKey, IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p") => new UpdateVisitor(dbKey, this, mapProvider, isParameterized, tableAsStart, parameterPrefix);
    public virtual IDeleteVisitor NewDeleteVisitor(string dbKey, IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p") => new DeleteVisitor(dbKey, this, mapProvider, isParameterized, tableAsStart, parameterPrefix);

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
                            var targetArgument = visitor.GetQuotedValue(targetSegment);
                            var rightArgument = visitor.GetQuotedValue(rightSegment);
                            return targetSegment.Merge(targetSegment, rightSegment, $"{targetArgument}={rightArgument}", false, false, true);
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
                            var leftArgument = visitor.GetQuotedValue(leftSegment);
                            var rightArgument = visitor.GetQuotedValue(rightSegment);
                            return leftSegment.Merge(rightSegment, $"CASE WHEN {leftArgument}={rightArgument} THEN 0 WHEN {leftArgument}>{rightArgument} THEN 1 ELSE -1 END", false, false, true);
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
                            var targetArgument = visitor.GetQuotedValue(targetSegment);
                            var rightArgument = visitor.GetQuotedValue(rightSegment);
                            return targetSegment.Merge(rightSegment, $"CASE WHEN {targetArgument}={rightArgument} THEN 0 WHEN {targetArgument}>{rightArgument} THEN 1 ELSE -1 END", false, false, true);
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
                            {
                                targetSegment.ExpectType = methodInfo.ReturnType;
                                return targetSegment.Change(targetSegment.ToString());
                            }
                            targetSegment.ExpectType = methodInfo.ReturnType;
                            return targetSegment.Change(this.CastTo(typeof(string), targetSegment.Value), false, false, false, true);
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
                            methodCallSqlFormatterCache.TryAdd(cacheKey, formatter = (visitor, orgExpr, target, deferExprs, args) =>
                            {
                                var args0Segment = visitor.VisitAndDeferred(new SqlSegment { Expression = args[0] });
                                if (args0Segment.IsConstant || args0Segment.IsVariable)
                                    return args0Segment.Change(Enum.Parse(enumType, args0Segment.Value.ToString()));

                                throw new NotSupportedException("不支持的表达式访问，Enum.Parse方法只支持常量、变量参数");
                            });
                            result = true;
                            break;
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
                            result = true;
                            break;
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
                        result = true;
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
                            result = true;
                            break;
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
                            result = true;
                            break;
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
                                return targetSegment.Change(methodInfo.Invoke(targetSegment.Value, arguments.ToArray()));

                            throw new NotSupportedException("不支持的表达式访问，get_Item索引方法只支持常量、变量参数");
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
