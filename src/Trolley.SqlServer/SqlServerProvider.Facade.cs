namespace Trolley.SqlServer;

partial class SqlServerProvider
{
    public override IRepository CreateRepository(DbContext dbContext) => new SqlServerRepository(dbContext);
}
