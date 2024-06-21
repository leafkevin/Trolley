using System.Data;

namespace Trolley.SqlServer;

partial class SqlServerProvider
{
    public override IRepository CreateRepository(DbContext dbContext) => new SqlServerRepository(dbContext);
    public override IQueryVisitor NewQueryVisitor(string dbKey, IEntityMapProvider mapProvider, IShardingProvider shardingProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p", IDataParameterCollection dbParameters = null)
        => new SqlServerQueryVisitor(dbKey, this, mapProvider, shardingProvider, isParameterized, tableAsStart, parameterPrefix, dbParameters);
    public override ICreate<TEntity> NewCreate<TEntity>(DbContext dbContext) => new SqlServerCreate<TEntity>(dbContext);
    public override IContinuedCreate<TEntity> NewContinuedCreate<TEntity>(DbContext dbContext, ICreateVisitor visitor)
    {
        if (visitor.ActionMode == ActionMode.Bulk)
            return new SqlServerBulkContinuedCreate<TEntity>(dbContext, visitor);
        else return new SqlServerContinuedCreate<TEntity>(dbContext, visitor);
    }
    public override ICreated<TEntity> NewCreated<TEntity>(DbContext dbContext, ICreateVisitor visitor)
        => new SqlServerCreated<TEntity>(dbContext, visitor);
    public override ICreateVisitor NewCreateVisitor(string dbKey, IEntityMapProvider mapProvider, IShardingProvider shardingProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
        => new SqlServerCreateVisitor(dbKey, this, mapProvider, shardingProvider, isParameterized, tableAsStart, parameterPrefix);
    public override IUpdate<TEntity> NewUpdate<TEntity>(DbContext dbContext) => new SqlServerUpdate<TEntity>(dbContext);
    public override IContinuedUpdate<TEntity> NewContinuedUpdate<TEntity>(DbContext dbContext, IUpdateVisitor visitor)
        => new SqlServerContinuedUpdate<TEntity>(dbContext, visitor);
    public override IUpdated<TEntity> NewUpdated<TEntity>(DbContext dbContext, IUpdateVisitor visitor)
        => new SqlServerUpdated<TEntity>(dbContext, visitor);
    public override IUpdateVisitor NewUpdateVisitor(string dbKey, IEntityMapProvider mapProvider, IShardingProvider shardingProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
        => new SqlServerUpdateVisitor(dbKey, this, mapProvider, shardingProvider, isParameterized, tableAsStart, parameterPrefix);
    public override IDeleteVisitor NewDeleteVisitor(string dbKey, IEntityMapProvider mapProvider, IShardingProvider shardingProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
        => new SqlServerDeleteVisitor(dbKey, this, mapProvider, shardingProvider, isParameterized, tableAsStart, parameterPrefix);
}
