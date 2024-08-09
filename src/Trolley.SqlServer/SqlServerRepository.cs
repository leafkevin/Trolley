namespace Trolley.SqlServer;

public class SqlServerRepository : Repository, ISqlServerRepository
{
    #region Constructor
    public SqlServerRepository(DbContext dbContext) : base(dbContext) { }
    #endregion

    #region Create
    public override ISqlServerCreate<TEntity> Create<TEntity>() => this.ormProvider.NewCreate<TEntity>(this.DbContext) as ISqlServerCreate<TEntity>;
    #endregion

    #region Update
    public override ISqlServerUpdate<TEntity> Update<TEntity>() => this.ormProvider.NewUpdate<TEntity>(this.DbContext) as ISqlServerUpdate<TEntity>;
    #endregion
}
