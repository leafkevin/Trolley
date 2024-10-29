using System.Data;

namespace Trolley.Sqlite;

partial class SqliteProvider
{
    public override IRepository CreateRepository(DbContext dbContext) => new SqliteRepository(dbContext);
    public override IQueryVisitor NewQueryVisitor(DbContext dbContext, char tableAsStart = 'a', IDataParameterCollection dbParameters = null)
        => new SqliteQueryVisitor(dbContext, tableAsStart, dbParameters);
    public override ICreate<TEntity> NewCreate<TEntity>(DbContext dbContext) => new SqliteCreate<TEntity>(dbContext);
    public override IContinuedCreate<TEntity> NewContinuedCreate<TEntity>(DbContext dbContext, ICreateVisitor visitor)
    {
        if (visitor.ActionMode == ActionMode.Bulk)
            return new SqliteBulkContinuedCreate<TEntity>(dbContext, visitor);
        else return new SqliteContinuedCreate<TEntity>(dbContext, visitor);
    }
    public override ICreated<TEntity> NewCreated<TEntity>(DbContext dbContext, ICreateVisitor visitor)
        => new SqliteCreated<TEntity>(dbContext, visitor);
    public override ICreateVisitor NewCreateVisitor(DbContext dbContext, char tableAsStart = 'a')
        => new SqliteCreateVisitor(dbContext, tableAsStart);
    public override IUpdate<TEntity> NewUpdate<TEntity>(DbContext dbContext) => new SqliteUpdate<TEntity>(dbContext);
    public override IContinuedUpdate<TEntity> NewContinuedUpdate<TEntity>(DbContext dbContext, IUpdateVisitor visitor)
        => new SqliteContinuedUpdate<TEntity>(dbContext, visitor);
    public override IUpdated<TEntity> NewUpdated<TEntity>(DbContext dbContext, IUpdateVisitor visitor)
        => new SqliteUpdated<TEntity>(dbContext, visitor);
    public override IUpdateVisitor NewUpdateVisitor(DbContext dbContext, char tableAsStart = 'a')
        => new SqliteUpdateVisitor(dbContext, tableAsStart);
    public override IDeleteVisitor NewDeleteVisitor(DbContext dbContext, char tableAsStart = 'a')
        => new SqliteDeleteVisitor(dbContext, tableAsStart);
}
