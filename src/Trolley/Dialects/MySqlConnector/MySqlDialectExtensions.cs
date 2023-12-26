namespace Trolley.MySqlConnector;

public static class MySqlDialectExtensions
{
    public static IMySqlRepository Create(this IOrmDbFactory dbFactory, string dbKey = null, string tenantId = null)
        => dbFactory.CreateRepository(dbKey, tenantId) as IMySqlRepository;
}
