using System;
using System.Data;

namespace Trolley.MySqlConnector;

partial class MySqlProvider
{
    public override IRepository CreateRepository(DbContext dbContext) => new MySqlRepository(dbContext);

    public override IContinuedCreate NewContinuedCreate(DbContext dbContext, ICreateVisitor visitor)
        => new MySqlContinuedCreate(dbContext, visitor);
    public override IContinuedCreate<TEntity> NewContinuedCreate<TEntity>(DbContext dbContext, ICreateVisitor visitor)
        => new MySqlContinuedCreate<TEntity>(dbContext, visitor);
    public override IBulkContinuedCreate NewBulkContinuedCreate(DbContext dbContext, ICreateVisitor visitor)
        => new MySqlBulkContinuedCreate(dbContext, visitor);
    public override IBulkContinuedCreate<TEntity> NewBulkContinuedCreate<TEntity>(DbContext dbContext, ICreateVisitor visitor)
        => new MySqlBulkContinuedCreate<TEntity>(dbContext, visitor);

    public override IIdentitiedCreated NewIdentitiedCreated(DbContext dbContext, ICreateVisitor visitor)
        => new MySqlIdentitiedCreated(dbContext, visitor);
    public override ICreated NewCreated(DbContext dbContext, ICreateVisitor visitor)
        => new MySqlCreated(dbContext, visitor);

    public override IUpdated NewUpdated(DbContext dbContext, IUpdateVisitor visitor)
        => new MySqlUpdated(dbContext, visitor);

    public override IQueryVisitor NewQueryVisitor(DbContext dbContext, char tableAsStart = 'a', ITheaCommand command = null)
        => new MySqlQueryVisitor(dbContext, tableAsStart, command);

    public override ICreateVisitor NewCreateVisitor(Type entityType, DbContext dbContext, char tableAsStart = 'a', ITheaCommand command = null)
        => new MySqlCreateVisitor(entityType, dbContext, tableAsStart, command);

    public override IUpdateVisitor NewUpdateVisitor(Type entityType, DbContext dbContext, char tableAsStart = 'a', ITheaCommand command = null)
        => new MySqlUpdateVisitor(entityType, dbContext, tableAsStart, command);

    public override IDeleteVisitor NewDeleteVisitor(Type entityType, DbContext dbContext, char tableAsStart = 'a', ITheaCommand command = null)
        => new MySqlDeleteVisitor(entityType, dbContext, tableAsStart, command);
}