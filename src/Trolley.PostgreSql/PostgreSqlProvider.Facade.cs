using System.Data;

namespace Trolley.PostgreSql;

partial class PostgreSqlProvider
{
    public override IRepository CreateRepository(DbContext dbContext) => new PostgreSqlRepository(dbContext);
    public override IQueryVisitor NewQueryVisitor(DbContext dbContext, char tableAsStart = 'a', IDataParameterCollection dbParameters = null)
        => new PostgreSqlQueryVisitor(dbContext, tableAsStart, dbParameters);

    public override IQuery<T> NewQuery<T>(DbContext dbContext, IQueryVisitor visitor) => new PostgreSqlQuery<T>(dbContext, visitor);
    public override IQuery<T1, T2> NewQuery<T1, T2>(DbContext dbContext, IQueryVisitor visitor) => new PostgreSqlQuery<T1, T2>(dbContext, visitor);
    public override IQuery<T1, T2, T3> NewQuery<T1, T2, T3>(DbContext dbContext, IQueryVisitor visitor) => new PostgreSqlQuery<T1, T2, T3>(dbContext, visitor);
    public override IQuery<T1, T2, T3, T4> NewQuery<T1, T2, T3, T4>(DbContext dbContext, IQueryVisitor visitor) => new PostgreSqlQuery<T1, T2, T3, T4>(dbContext, visitor);
    public override IQuery<T1, T2, T3, T4, T5> NewQuery<T1, T2, T3, T4, T5>(DbContext dbContext, IQueryVisitor visitor) => new PostgreSqlQuery<T1, T2, T3, T4, T5>(dbContext, visitor);
    public override IQuery<T1, T2, T3, T4, T5, T6> NewQuery<T1, T2, T3, T4, T5, T6>(DbContext dbContext, IQueryVisitor visitor) => new PostgreSqlQuery<T1, T2, T3, T4, T5, T6>(dbContext, visitor);
    public override IQuery<T1, T2, T3, T4, T5, T6, T7> NewQuery<T1, T2, T3, T4, T5, T6, T7>(DbContext dbContext, IQueryVisitor visitor) => new PostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7>(dbContext, visitor);
    public override IQuery<T1, T2, T3, T4, T5, T6, T7, T8> NewQuery<T1, T2, T3, T4, T5, T6, T7, T8>(DbContext dbContext, IQueryVisitor visitor) => new PostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8>(dbContext, visitor);
    public override IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>(DbContext dbContext, IQueryVisitor visitor) => new PostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>(dbContext, visitor);
    public override IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(DbContext dbContext, IQueryVisitor visitor) => new PostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(dbContext, visitor);
    public override IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(DbContext dbContext, IQueryVisitor visitor) => new PostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(dbContext, visitor);
    public override IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(DbContext dbContext, IQueryVisitor visitor) => new PostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(dbContext, visitor);
    public override IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(DbContext dbContext, IQueryVisitor visitor) => new PostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(dbContext, visitor);
    public override IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(DbContext dbContext, IQueryVisitor visitor) => new PostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(dbContext, visitor);
    public override IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(DbContext dbContext, IQueryVisitor visitor) => new PostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(dbContext, visitor);
    public override IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> NewQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(DbContext dbContext, IQueryVisitor visitor) => new PostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(dbContext, visitor);

    public override IIncludableQuery<T, TMember> NewIncludableQuery<T, TMember>(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany) => new PostgreSqlIncludableQuery<T, TMember>(dbContext, visitor, isIncludeMany);
    public override IIncludableQuery<T1, T2, TMember> NewIncludableQuery<T1, T2, TMember>(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany) => new PostgreSqlIncludableQuery<T1, T2, TMember>(dbContext, visitor, isIncludeMany);
    public override IIncludableQuery<T1, T2, T3, TMember> NewIncludableQuery<T1, T2, T3, TMember>(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany) => new PostgreSqlIncludableQuery<T1, T2, T3, TMember>(dbContext, visitor, isIncludeMany);
    public override IIncludableQuery<T1, T2, T3, T4, TMember> NewIncludableQuery<T1, T2, T3, T4, TMember>(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany) => new PostgreSqlIncludableQuery<T1, T2, T3, T4, TMember>(dbContext, visitor, isIncludeMany);
    public override IIncludableQuery<T1, T2, T3, T4, T5, TMember> NewIncludableQuery<T1, T2, T3, T4, T5, TMember>(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany) => new PostgreSqlIncludableQuery<T1, T2, T3, T4, T5, TMember>(dbContext, visitor, isIncludeMany);
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> NewIncludableQuery<T1, T2, T3, T4, T5, T6, TMember>(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany) => new PostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, TMember>(dbContext, visitor, isIncludeMany);
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember>(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany) => new PostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember>(dbContext, visitor, isIncludeMany);
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember>(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany) => new PostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember>(dbContext, visitor, isIncludeMany);
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany) => new PostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>(dbContext, visitor, isIncludeMany);
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany) => new PostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>(dbContext, visitor, isIncludeMany);
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany) => new PostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>(dbContext, visitor, isIncludeMany);
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany) => new PostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>(dbContext, visitor, isIncludeMany);
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany) => new PostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>(dbContext, visitor, isIncludeMany);
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany) => new PostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>(dbContext, visitor, isIncludeMany);
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany) => new PostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>(dbContext, visitor, isIncludeMany);
    public override IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember> NewIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>(DbContext dbContext, IQueryVisitor visitor, bool isIncludeMany) => new PostgreSqlIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>(dbContext, visitor, isIncludeMany);

    public override IGroupingQuery<T, TGrouping> NewGroupQuery<T, TGrouping>(DbContext dbContext, IQueryVisitor visitor) => new PostgreSqlGroupingQuery<T, TGrouping>(dbContext, visitor);
    public override IGroupingQuery<T1, T2, TGrouping> NewGroupQuery<T1, T2, TGrouping>(DbContext dbContext, IQueryVisitor visitor) => new PostgreSqlGroupingQuery<T1, T2, TGrouping>(dbContext, visitor);
    public override IGroupingQuery<T1, T2, T3, TGrouping> NewGroupQuery<T1, T2, T3, TGrouping>(DbContext dbContext, IQueryVisitor visitor) => new PostgreSqlGroupingQuery<T1, T2, T3, TGrouping>(dbContext, visitor);
    public override IGroupingQuery<T1, T2, T3, T4, TGrouping> NewGroupQuery<T1, T2, T3, T4, TGrouping>(DbContext dbContext, IQueryVisitor visitor) => new PostgreSqlGroupingQuery<T1, T2, T3, T4, TGrouping>(dbContext, visitor);
    public override IGroupingQuery<T1, T2, T3, T4, T5, TGrouping> NewGroupQuery<T1, T2, T3, T4, T5, TGrouping>(DbContext dbContext, IQueryVisitor visitor) => new PostgreSqlGroupingQuery<T1, T2, T3, T4, T5, TGrouping>(dbContext, visitor);
    public override IGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> NewGroupQuery<T1, T2, T3, T4, T5, T6, TGrouping>(DbContext dbContext, IQueryVisitor visitor) => new PostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping>(dbContext, visitor);
    public override IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> NewGroupQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping>(DbContext dbContext, IQueryVisitor visitor) => new PostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping>(dbContext, visitor);
    public override IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> NewGroupQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping>(DbContext dbContext, IQueryVisitor visitor) => new PostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping>(dbContext, visitor);
    public override IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> NewGroupQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping>(DbContext dbContext, IQueryVisitor visitor) => new PostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping>(dbContext, visitor);
    public override IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> NewGroupQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping>(DbContext dbContext, IQueryVisitor visitor) => new PostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping>(dbContext, visitor);
    public override IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> NewGroupQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping>(DbContext dbContext, IQueryVisitor visitor) => new PostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping>(dbContext, visitor);
    public override IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> NewGroupQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping>(DbContext dbContext, IQueryVisitor visitor) => new PostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping>(dbContext, visitor);
    public override IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> NewGroupQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping>(DbContext dbContext, IQueryVisitor visitor) => new PostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping>(dbContext, visitor);
    public override IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> NewGroupQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping>(DbContext dbContext, IQueryVisitor visitor) => new PostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping>(dbContext, visitor);
    public override IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping> NewGroupQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping>(DbContext dbContext, IQueryVisitor visitor) => new PostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping>(dbContext, visitor);
    public override IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TGrouping> NewGroupQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TGrouping>(DbContext dbContext, IQueryVisitor visitor) => new PostgreSqlGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TGrouping>(dbContext, visitor);

    public override ICreate<TEntity> NewCreate<TEntity>(DbContext dbContext) => new PostgreSqlCreate<TEntity>(dbContext);
    public override IContinuedCreate<TEntity> NewContinuedCreate<TEntity>(DbContext dbContext, ICreateVisitor visitor)
    {
        if (visitor.ActionMode == ActionMode.Bulk)
            return new PostgreSqlBulkContinuedCreate<TEntity>(dbContext, visitor);
        else return new PostgreSqlContinuedCreate<TEntity>(dbContext, visitor);
    }
    public override ICreated<TEntity> NewCreated<TEntity>(DbContext dbContext, ICreateVisitor visitor)
        => new PostgreSqlCreated<TEntity>(dbContext, visitor);
    public override ICreateVisitor NewCreateVisitor(DbContext dbContext, char tableAsStart = 'a')
        => new PostgreSqlCreateVisitor(dbContext, tableAsStart);
    public override IUpdate<TEntity> NewUpdate<TEntity>(DbContext dbContext) => new PostgreSqlUpdate<TEntity>(dbContext);
    public override IContinuedUpdate<TEntity> NewContinuedUpdate<TEntity>(DbContext dbContext, IUpdateVisitor visitor)
        => new PostgreSqlContinuedUpdate<TEntity>(dbContext, visitor);
    public override IUpdated<TEntity> NewUpdated<TEntity>(DbContext dbContext, IUpdateVisitor visitor)
        => new PostgreSqlUpdated<TEntity>(dbContext, visitor);
    public override IUpdateVisitor NewUpdateVisitor(DbContext dbContext, char tableAsStart = 'a')
        => new PostgreSqlUpdateVisitor(dbContext, tableAsStart);
    public override IDeleteVisitor NewDeleteVisitor(DbContext dbContext, char tableAsStart = 'a')
        => new PostgreSqlDeleteVisitor(dbContext, tableAsStart);
}
