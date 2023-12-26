namespace Trolley.SqlServer;

public static class SqlServerDialectExtensions
{
    public static ISqlServerRepository Create(this IOrmDbFactory dbFactory, string dbKey = null, string tenantId = null)
        => dbFactory.CreateRepository(dbKey, tenantId) as ISqlServerRepository;
} 
