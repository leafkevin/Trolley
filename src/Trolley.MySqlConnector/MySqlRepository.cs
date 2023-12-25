using System.Data;

namespace Trolley.MySqlConnector;

public class MySqlRepository : Repository, IMySqlRepository
{
    #region Constructor
    public MySqlRepository() { }
    public MySqlRepository(DbContext dbContext) : base(dbContext) { }
    #endregion 

    public new IMySqlCreate<TEntity> Create<TEntity>() => new MySqlCreate<TEntity>(this.DbContext);
    public new IMySqlUpdate<TEntity> Update<TEntity>() => new MySqlUpdate<TEntity>(this.DbContext);
}
