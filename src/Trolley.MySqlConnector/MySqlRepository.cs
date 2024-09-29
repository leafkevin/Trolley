namespace Trolley.MySqlConnector;

public class MySqlRepository : Repository, IMySqlRepository
{
    #region Constructor
    public MySqlRepository(DbContext dbContext) : base(dbContext) { }
    #endregion

    #region Create
    public new IMySqlCreate<TEntity> Create<TEntity>() => this.OrmProvider.NewCreate<TEntity>(this.DbContext) as IMySqlCreate<TEntity>;
    #endregion

    #region Update
    public new IMySqlUpdate<TEntity> Update<TEntity>() => this.OrmProvider.NewUpdate<TEntity>(this.DbContext) as IMySqlUpdate<TEntity>;
    #endregion
}
