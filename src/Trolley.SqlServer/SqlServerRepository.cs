namespace Trolley.SqlServer;

public class SqlServerRepository : Repository, ISqlServerRepository
{
    #region Constructor
    public SqlServerRepository() { }
    public SqlServerRepository(DbContext dbContext) : base(dbContext) { }
    #endregion
}
