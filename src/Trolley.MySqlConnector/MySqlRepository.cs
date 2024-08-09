namespace Trolley.MySqlConnector;

public class MySqlRepository : Repository, IMySqlRepository
{
    #region Constructor
    public MySqlRepository(DbContext dbContext) : base(dbContext) { }
    #endregion

    #region Create
    public override IMySqlCreate<TEntity> Create<TEntity>() => this.ormProvider.NewCreate<TEntity>(this.DbContext) as IMySqlCreate<TEntity>;
    #endregion

    #region Update
    public override IMySqlUpdate<TEntity> Update<TEntity>() => this.ormProvider.NewUpdate<TEntity>(this.DbContext) as IMySqlUpdate<TEntity>;
    #endregion
}
