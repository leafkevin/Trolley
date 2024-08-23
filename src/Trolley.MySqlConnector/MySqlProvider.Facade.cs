using System.Data;

namespace Trolley.MySqlConnector;

partial class MySqlProvider
{
    public override IRepository CreateRepository(DbContext dbContext) => new MySqlRepository(dbContext);
    public override IQueryVisitor NewQueryVisitor(DbContext dbContext, char tableAsStart = 'a', IDataParameterCollection dbParameters = null)
        => new MySqlQueryVisitor(dbContext, tableAsStart, dbParameters);
    public override ICreate<TEntity> NewCreate<TEntity>(DbContext dbContext) => new MySqlCreate<TEntity>(dbContext);
    public override IContinuedCreate<TEntity> NewContinuedCreate<TEntity>(DbContext dbContext, ICreateVisitor visitor)
    {
        if (visitor.ActionMode == ActionMode.Bulk)
            return new MySqlBulkContinuedCreate<TEntity>(dbContext, visitor);
        else return new MySqlContinuedCreate<TEntity>(dbContext, visitor);
    }
    public override ICreated<TEntity> NewCreated<TEntity>(DbContext dbContext, ICreateVisitor visitor)
        => new MySqlCreated<TEntity>(dbContext, visitor);
    public override ICreateVisitor NewCreateVisitor(DbContext dbContext, char tableAsStart = 'a')
        => new MySqlCreateVisitor(dbContext, tableAsStart);
    public override IUpdate<TEntity> NewUpdate<TEntity>(DbContext dbContext) => new MySqlUpdate<TEntity>(dbContext);
    public override IContinuedUpdate<TEntity> NewContinuedUpdate<TEntity>(DbContext dbContext, IUpdateVisitor visitor)
        => new MySqlContinuedUpdate<TEntity>(dbContext, visitor);
    public override IUpdated<TEntity> NewUpdated<TEntity>(DbContext dbContext, IUpdateVisitor visitor)
        => new MySqlUpdated<TEntity>(dbContext, visitor);
    public override IUpdateVisitor NewUpdateVisitor(DbContext dbContext, char tableAsStart = 'a')
        => new MySqlUpdateVisitor(dbContext, tableAsStart);
    public override IDeleteVisitor NewDeleteVisitor(DbContext dbContext, char tableAsStart = 'a')
        => new MySqlDeleteVisitor(dbContext, tableAsStart);
}
