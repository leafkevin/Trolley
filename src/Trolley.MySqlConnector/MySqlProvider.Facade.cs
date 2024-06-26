﻿using System.Data;

namespace Trolley.MySqlConnector;

partial class MySqlProvider
{
    public override IRepository CreateRepository(DbContext dbContext) => new MySqlRepository(dbContext);
    public override IQueryVisitor NewQueryVisitor(string dbKey, IEntityMapProvider mapProvider, IShardingProvider shardingProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p", IDataParameterCollection dbParameters = null)
        => new MySqlQueryVisitor(dbKey, this, mapProvider, shardingProvider, isParameterized, tableAsStart, parameterPrefix, dbParameters);
    public override ICreate<TEntity> NewCreate<TEntity>(DbContext dbContext) => new MySqlCreate<TEntity>(dbContext);
    public override IContinuedCreate<TEntity> NewContinuedCreate<TEntity>(DbContext dbContext, ICreateVisitor visitor)
    {
        if (visitor.ActionMode == ActionMode.Bulk)
            return new MySqlBulkContinuedCreate<TEntity>(dbContext, visitor);
        else return new MySqlContinuedCreate<TEntity>(dbContext, visitor);
    }
    public override ICreated<TEntity> NewCreated<TEntity>(DbContext dbContext, ICreateVisitor visitor)
        => new MySqlCreated<TEntity>(dbContext, visitor);
    public override ICreateVisitor NewCreateVisitor(string dbKey, IEntityMapProvider mapProvider, IShardingProvider shardingProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
        => new MySqlCreateVisitor(dbKey, this, mapProvider, shardingProvider, isParameterized, tableAsStart, parameterPrefix);
    public override IUpdate<TEntity> NewUpdate<TEntity>(DbContext dbContext) => new MySqlUpdate<TEntity>(dbContext);
    public override IContinuedUpdate<TEntity> NewContinuedUpdate<TEntity>(DbContext dbContext, IUpdateVisitor visitor)
        => new MySqlContinuedUpdate<TEntity>(dbContext, visitor);
    public override IUpdated<TEntity> NewUpdated<TEntity>(DbContext dbContext, IUpdateVisitor visitor)
        => new MySqlUpdated<TEntity>(dbContext, visitor);
    public override IUpdateVisitor NewUpdateVisitor(string dbKey, IEntityMapProvider mapProvider, IShardingProvider shardingProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
        => new MySqlUpdateVisitor(dbKey, this, mapProvider, shardingProvider, isParameterized, tableAsStart, parameterPrefix);
    public override IDeleteVisitor NewDeleteVisitor(string dbKey, IEntityMapProvider mapProvider, IShardingProvider shardingProvider, bool isParameterized = false, char tableAsStart = 'a', string parameterPrefix = "p")
        => new MySqlDeleteVisitor(dbKey, this, mapProvider, shardingProvider, isParameterized, tableAsStart, parameterPrefix);
}
