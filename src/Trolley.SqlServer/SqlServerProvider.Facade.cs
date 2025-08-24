using System.Data;

namespace Trolley.SqlServer;

partial class SqlServerProvider
{
    public override IRepository CreateRepository(DbContext dbContext) => new SqlServerRepository(dbContext);
    public override IQueryVisitor NewQueryVisitor(DbContext dbContext, char tableAsStart = 'a', IDataParameterCollection dbParameters = null)
        => new SqlServerQueryVisitor(dbContext, tableAsStart, dbParameters);
    public override IMultipleQuery NewMultipleQuery(DbContext dbContext) => new SqlServerMultipleQuery(dbContext);
    public override IFromCommand<TEntity, T> NewFromCommand<TEntity, T>(DbContext dbContext, IQueryVisitor visitor)
        => new SqlServerFromCommand<TEntity, T>(dbContext, visitor);
    public override IFromCommand<TEntity, T1, T2> NewFromCommand<TEntity, T1, T2>(DbContext dbContext, IQueryVisitor visitor)
        => new SqlServerFromCommand<TEntity, T1, T2>(dbContext, visitor);
    public override IFromCommand<TEntity, T1, T2, T3> NewFromCommand<TEntity, T1, T2, T3>(DbContext dbContext, IQueryVisitor visitor)
        => new SqlServerFromCommand<TEntity, T1, T2, T3>(dbContext, visitor);
    public override IFromCommand<TEntity, T1, T2, T3, T4> NewFromCommand<TEntity, T1, T2, T3, T4>(DbContext dbContext, IQueryVisitor visitor)
        => new SqlServerFromCommand<TEntity, T1, T2, T3, T4>(dbContext, visitor);
    public override IFromCommand<TEntity, T1, T2, T3, T4, T5> NewFromCommand<TEntity, T1, T2, T3, T4, T5>(DbContext dbContext, IQueryVisitor visitor)
        => new SqlServerFromCommand<TEntity, T1, T2, T3, T4, T5>(dbContext, visitor);
    public override IFromCommand<TEntity, T1, T2, T3, T4, T5, T6> NewFromCommand<TEntity, T1, T2, T3, T4, T5, T6>(DbContext dbContext, IQueryVisitor visitor)
        => new SqlServerFromCommand<TEntity, T1, T2, T3, T4, T5, T6>(dbContext, visitor);

    public override IGroupingCommand<TEntity, T, TGrouping> NewGroupCommand<T, TGrouping>(DbContext dbContext, IQueryVisitor visitor)
        => new SqlServerGroupingCommand<TEntity, T, TGrouping>(dbContext, visitor);
    public override IGroupingCommand<TEntity, T1, T2, TGrouping> NewGroupCommand<TEntity, T1, T2, TGrouping>(DbContext dbContext, IQueryVisitor visitor)
        => new SqlServerGroupingCommand<TEntity, T1, T2, TGrouping>(dbContext, visitor);
    public override IGroupingCommand<TEntity, T1, T2, T3, TGrouping> NewGroupCommand<TEntity, T1, T2, T3, TGrouping>(DbContext dbContext, IQueryVisitor visitor)
        => new SqlServerGroupingCommand<TEntity, T1, T2, T3, TGrouping>(dbContext, visitor);
    public override IGroupingCommand<TEntity, T1, T2, T3, T4, TGrouping> NewGroupCommand<TEntity, T1, T2, T3, T4, TGrouping>(DbContext dbContext, IQueryVisitor visitor)
        => new SqlServerGroupingCommand<TEntity, T1, T2, T3, T4, TGrouping>(dbContext, visitor);
    public override IGroupingCommand<TEntity, T1, T2, T3, T4, T5, TGrouping> NewGroupCommand<TEntity, T1, T2, T3, T4, T5, TGrouping>(DbContext dbContext, IQueryVisitor visitor)
        => new SqlServerGroupingCommand<TEntity, T1, T2, T3, T4, T5, TGrouping>(dbContext, visitor);
    public override IGroupingCommand<TEntity, T1, T2, T3, T4, T5, T6, TGrouping> NewGroupCommand<TEntity, T1, T2, T3, T4, T5, T6, TGrouping>(DbContext dbContext, IQueryVisitor visitor)
        => new SqlServerGroupingCommand<TEntity, T1, T2, T3, T4, T5, T6, TGrouping>(dbContext, visitor);
    public override ICreate<TEntity> NewCreate<TEntity>(DbContext dbContext) => new SqlServerCreate<TEntity>(dbContext);
    public override IContinuedCreate<TEntity> NewContinuedCreate<TEntity>(DbContext dbContext, ICreateVisitor visitor)
    {
        if (visitor.ActionMode == ActionMode.Bulk)
            return new SqlServerBulkContinuedCreate<TEntity>(dbContext, visitor);
        else return new SqlServerContinuedCreate<TEntity>(dbContext, visitor);
    }
    public override ICreated<TEntity> NewCreated<TEntity>(DbContext dbContext, ICreateVisitor visitor)
        => new SqlServerCreated<TEntity>(dbContext, visitor);
    public override ICreateVisitor NewCreateVisitor(DbContext dbContext, char tableAsStart = 'a')
        => new SqlServerCreateVisitor(dbContext, tableAsStart);

    public override IUpdate<TEntity> NewUpdate<TEntity>(DbContext dbContext) => new SqlServerUpdate<TEntity>(dbContext);
    public override IContinuedUpdate<TEntity> NewContinuedUpdate<TEntity>(DbContext dbContext, IUpdateVisitor visitor)
        => new SqlServerContinuedUpdate<TEntity>(dbContext, visitor);
    public override IBulkContinuedUpdate<TEntity> NewBulkContinuedUpdate<TEntity>(DbContext dbContext, IUpdateVisitor visitor)
        => new SqlServerBulkContinuedUpdate<TEntity>(dbContext, visitor);
    public override IUpdated<TEntity> NewUpdated<TEntity>(DbContext dbContext, IUpdateVisitor visitor)
        => new SqlServerUpdated<TEntity>(dbContext, visitor);
    public override IUpdateVisitor NewUpdateVisitor(DbContext dbContext, char tableAsStart = 'a')
        => new SqlServerUpdateVisitor(dbContext, tableAsStart);

    public override IDelete<TEntity> NewDelete<TEntity>(DbContext dbContext)
        => new SqlServerDelete<TEntity>(dbContext);
    public override IContinuedDelete<TEntity> NewContinuedDelete<TEntity>(DbContext dbContext, IDeleteVisitor visitor)
        => new SqlServerContinuedDelete<TEntity>(dbContext, visitor);
    public override IDeleteVisitor NewDeleteVisitor(DbContext dbContext, char tableAsStart = 'a')
        => new SqlServerDeleteVisitor(dbContext, tableAsStart);
}