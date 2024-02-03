﻿using System;
using System.Data;

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

    public virtual IQueryVisitor NewQueryVisitor(IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p", IDataParameterCollection dbParameters = null) => new QueryVisitor(this, mapProvider, isParameterized, tableAsStart, parameterPrefix, dbParameters);
    public virtual ICreateVisitor NewCreateVisitor(IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p") => new CreateVisitor(this, mapProvider, isParameterized, tableAsStart, parameterPrefix);
    public virtual IUpdateVisitor NewUpdateVisitor(IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p") => new UpdateVisitor(this, mapProvider, isParameterized, tableAsStart, parameterPrefix);
    public virtual IDeleteVisitor NewDeleteVisitor(IEntityMapProvider mapProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p") => new DeleteVisitor(this, mapProvider, isParameterized, tableAsStart, parameterPrefix);
}