using System.Data;

namespace Trolley.MySqlConnector;

public class MySqlRepository : Repository, IMySqlRepository
{
    #region Constructor
    public MySqlRepository(string dbKey, IDbConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider)
        : base(dbKey, connection, ormProvider, mapProvider) { }
    public MySqlRepository(TheaConnection connection, IOrmProvider ormProvider, IEntityMapProvider mapProvider)
        : base(connection, ormProvider, mapProvider) { }
    #endregion

    public new IMySqlCreate<TEntity> Create<TEntity>() => new MySqlCreate<TEntity>(this.dbContext);
    public new IMySqlUpdate<TEntity> Update<TEntity>() => new MySqlUpdate<TEntity>(this.dbContext);
}
