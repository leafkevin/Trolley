namespace Trolley.SqlServer;

public static class SqlServerExtensions
{
    public static ISqlServerRepository Create(this IOrmDbFactory dbFactory, string dbKey = null)
        => dbFactory.CreateRepository(dbKey) as ISqlServerRepository;
}
