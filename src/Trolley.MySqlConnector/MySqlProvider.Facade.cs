using System;
using System.Data;

namespace Trolley.MySqlConnector;

partial class MySqlProvider
{
    public override IRepository CreateRepository(DbContext dbContext) => new MySqlRepository(dbContext);

    public override IQuery<T> NewQuery<T>(DbContext dbContext, IQueryVisitor visitor) => new MySqlQuery<T>(dbContext, visitor);

    public override IQueryVisitor NewQueryVisitor(DbContext dbContext, char tableAsStart = 'a', IDataParameterCollection dbParameters = null)
        => new MySqlQueryVisitor(dbContext, tableAsStart, dbParameters);

    public override IFromCommand<TEntity, T> NewFromCommand<TEntity, T>(DbContext dbContext, IQueryVisitor visitor)
        => new MySqlFromCommand<TEntity, T>(dbContext, visitor);
    public override IFromCommand<TEntity, T1, T2> NewFromCommand<TEntity, T1, T2>(DbContext dbContext, IQueryVisitor visitor)
        => new MySqlFromCommand<TEntity, T1, T2>(dbContext, visitor);
    public override IFromCommand<TEntity, T1, T2, T3> NewFromCommand<TEntity, T1, T2, T3>(DbContext dbContext, IQueryVisitor visitor)
        => new MySqlFromCommand<TEntity, T1, T2, T3>(dbContext, visitor);
    public override IFromCommand<TEntity, T1, T2, T3, T4> NewFromCommand<TEntity, T1, T2, T3, T4>(DbContext dbContext, IQueryVisitor visitor)
        => new MySqlFromCommand<TEntity, T1, T2, T3, T4>(dbContext, visitor);
    public override IFromCommand<TEntity, T1, T2, T3, T4, T5> NewFromCommand<TEntity, T1, T2, T3, T4, T5>(DbContext dbContext, IQueryVisitor visitor)
        => new MySqlFromCommand<TEntity, T1, T2, T3, T4, T5>(dbContext, visitor);
    public override IFromCommand<TEntity, T1, T2, T3, T4, T5, T6> NewFromCommand<TEntity, T1, T2, T3, T4, T5, T6>(DbContext dbContext, IQueryVisitor visitor)
        => new MySqlFromCommand<TEntity, T1, T2, T3, T4, T5, T6>(dbContext, visitor);

    public override IGroupingCommand<TEntity, T, TGrouping> NewGroupCommand<TEntity, T, TGrouping>(DbContext dbContext, IQueryVisitor visitor)
        => new MySqlGroupingCommand<TEntity, T, TGrouping>(dbContext, visitor);
    public override IGroupingCommand<TEntity, T1, T2, TGrouping> NewGroupCommand<TEntity, T1, T2, TGrouping>(DbContext dbContext, IQueryVisitor visitor)
        => new MySqlGroupingCommand<TEntity, T1, T2, TGrouping>(dbContext, visitor);
    public override IGroupingCommand<TEntity, T1, T2, T3, TGrouping> NewGroupCommand<TEntity, T1, T2, T3, TGrouping>(DbContext dbContext, IQueryVisitor visitor)
        => new MySqlGroupingCommand<TEntity, T1, T2, T3, TGrouping>(dbContext, visitor);
    public override IGroupingCommand<TEntity, T1, T2, T3, T4, TGrouping> NewGroupCommand<TEntity, T1, T2, T3, T4, TGrouping>(DbContext dbContext, IQueryVisitor visitor)
        => new MySqlGroupingCommand<TEntity, T1, T2, T3, T4, TGrouping>(dbContext, visitor);
    public override IGroupingCommand<TEntity, T1, T2, T3, T4, T5, TGrouping> NewGroupCommand<TEntity, T1, T2, T3, T4, T5, TGrouping>(DbContext dbContext, IQueryVisitor visitor)
        => new MySqlGroupingCommand<TEntity, T1, T2, T3, T4, T5, TGrouping>(dbContext, visitor);
    public override IGroupingCommand<TEntity, T1, T2, T3, T4, T5, T6, TGrouping> NewGroupCommand<TEntity, T1, T2, T3, T4, T5, T6, TGrouping>(DbContext dbContext, IQueryVisitor visitor)
        => new MySqlGroupingCommand<TEntity, T1, T2, T3, T4, T5, T6, TGrouping>(dbContext, visitor);

    public override ICreate<TEntity> NewCreate<TEntity>(DbContext dbContext) => new MySqlCreate<TEntity>(dbContext);
    public override IContinuedCreate<TEntity> NewContinuedCreate<TEntity>(DbContext dbContext, ICreateVisitor visitor)
        => new MySqlContinuedCreate<TEntity>(dbContext, visitor);
    public override IBulkContinuedCreate<TEntity> NewBulkContinuedCreate<TEntity>(DbContext dbContext, ICreateVisitor visitor)
        => new MySqlBulkContinuedCreate<TEntity>(dbContext, visitor);
    public override ICreated<TEntity> NewCreated<TEntity>(DbContext dbContext, ICreateVisitor visitor)
        => new MySqlCreated<TEntity>(dbContext, visitor);
    public override ICreateVisitor NewCreateVisitor(Type entityType, DbContext dbContext, char tableAsStart = 'a')
        => new MySqlCreateVisitor(entityType, dbContext, tableAsStart);
    public override IUpdate<TEntity> NewUpdate<TEntity>(DbContext dbContext) => new MySqlUpdate<TEntity>(dbContext);
    public override IContinuedUpdate<TEntity> NewContinuedUpdate<TEntity>(DbContext dbContext, IUpdateVisitor visitor)
        => new MySqlContinuedUpdate<TEntity>(dbContext, visitor);
    public override IBulkContinuedUpdate<TEntity> NewBulkContinuedUpdate<TEntity>(DbContext dbContext, IUpdateVisitor visitor)
        => new MySqlBulkContinuedUpdate<TEntity>(dbContext, visitor);
    public override IBulkCopyContinuedUpdate<TEntity> NewBulkCopyContinuedUpdate<TEntity>(DbContext dbContext, IUpdateVisitor visitor)
        => new MySqlBulkCopyContinuedUpdate<TEntity>(dbContext, visitor);
    public override IUpdated<TEntity> NewUpdated<TEntity>(DbContext dbContext, IUpdateVisitor visitor)
        => new MySqlUpdated<TEntity>(dbContext, visitor);
    public override IDelete<TEntity> NewDelete<TEntity>(DbContext dbContext)
        => new MySqlDelete<TEntity>(dbContext);

    public override IUpdateVisitor NewUpdateVisitor(Type entityType, DbContext dbContext, char tableAsStart = 'a')
        => new MySqlUpdateVisitor(entityType, dbContext, tableAsStart);
    public override IDeleteVisitor NewDeleteVisitor(Type entityType, DbContext dbContext, char tableAsStart = 'a')
        => new MySqlDeleteVisitor(entityType, dbContext, tableAsStart);
}