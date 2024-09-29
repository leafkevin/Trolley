namespace Trolley.SqlServer;

public class SqlServerRepository : Repository, ISqlServerRepository
{
    #region Constructor
    public SqlServerRepository(DbContext dbContext) : base(dbContext) { }
    #endregion

    #region Create
    public new ISqlServerCreate<TEntity> Create<TEntity>() => this.OrmProvider.NewCreate<TEntity>(this.DbContext) as ISqlServerCreate<TEntity>;
    #endregion

    #region Update
    public new ISqlServerUpdate<TEntity> Update<TEntity>() => this.OrmProvider.NewUpdate<TEntity>(this.DbContext) as ISqlServerUpdate<TEntity>;
    #endregion
}
