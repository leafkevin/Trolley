namespace Trolley.SqlServer;

public static class SqlServerDialectExtensions
{
    public static ISqlServerRepository Create(this IOrmDbFactory dbFactory, string dbKey = null)
        => dbFactory.CreateRepository(dbKey) as ISqlServerRepository;
}
