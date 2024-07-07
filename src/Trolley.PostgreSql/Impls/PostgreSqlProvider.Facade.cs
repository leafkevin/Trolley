using System.Data;

namespace Trolley.PostgreSql;

partial class PostgreSqlProvider
{
    public override IRepository CreateRepository(DbContext dbContext) => new PostgreSqlRepository(dbContext);
    public override IQueryVisitor NewQueryVisitor(string dbKey, IEntityMapProvider mapProvider, IShardingProvider shardingProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p", IDataParameterCollection dbParameters = null)
        => new PostgreSqlQueryVisitor(dbKey, this, mapProvider, shardingProvider, isParameterized, tableAsStart, parameterPrefix, dbParameters);
    public override ICreate<TEntity> NewCreate<TEntity>(DbContext dbContext) => new PostgreSqlCreate<TEntity>(dbContext);
    public override IContinuedCreate<TEntity> NewContinuedCreate<TEntity>(DbContext dbContext, ICreateVisitor visitor)
    {
        if (visitor.ActionMode == ActionMode.Bulk)
            return new PostgreSqlBulkContinuedCreate<TEntity>(dbContext, visitor);
        else return new PostgreSqlContinuedCreate<TEntity>(dbContext, visitor);
    }
    public override ICreated<TEntity> NewCreated<TEntity>(DbContext dbContext, ICreateVisitor visitor)
        => new PostgreSqlCreated<TEntity>(dbContext, visitor);
    public override ICreateVisitor NewCreateVisitor(string dbKey, IEntityMapProvider mapProvider, IShardingProvider shardingProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
        => new PostgreSqlCreateVisitor(dbKey, this, mapProvider, shardingProvider, isParameterized, tableAsStart, parameterPrefix);
    public override IUpdate<TEntity> NewUpdate<TEntity>(DbContext dbContext) => new PostgreSqlUpdate<TEntity>(dbContext);
    public override IContinuedUpdate<TEntity> NewContinuedUpdate<TEntity>(DbContext dbContext, IUpdateVisitor visitor)
        => new PostgreSqlContinuedUpdate<TEntity>(dbContext, visitor);
    public override IUpdated<TEntity> NewUpdated<TEntity>(DbContext dbContext, IUpdateVisitor visitor)
        => new PostgreSqlUpdated<TEntity>(dbContext, visitor);
    public override IUpdateVisitor NewUpdateVisitor(string dbKey, IEntityMapProvider mapProvider, IShardingProvider shardingProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
        => new PostgreSqlUpdateVisitor(dbKey, this, mapProvider, shardingProvider, isParameterized, tableAsStart, parameterPrefix);
    public override IDeleteVisitor NewDeleteVisitor(string dbKey, IEntityMapProvider mapProvider, IShardingProvider shardingProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
        => new PostgreSqlDeleteVisitor(dbKey, this, mapProvider, shardingProvider, isParameterized, tableAsStart, parameterPrefix);
}
