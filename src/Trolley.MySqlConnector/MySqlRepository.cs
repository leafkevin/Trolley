using System.Data;

namespace Trolley.MySqlConnector;

public class MySqlRepository : Repository, IMySqlRepository
{
    #region Constructor
    public MySqlRepository() { }
    public MySqlRepository(DbContext dbContext) : base(dbContext) { }
    #endregion 

    public override IMySqlCreate<TEntity> Create<TEntity>() => new MySqlCreate<TEntity>(this.DbContext);
    public override IMySqlUpdate<TEntity> Update<TEntity>() => new MySqlUpdate<TEntity>(this.DbContext);
}
