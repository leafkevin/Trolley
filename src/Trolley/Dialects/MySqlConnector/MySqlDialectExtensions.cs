namespace Trolley.MySqlConnector;

public static class MySqlDialectExtensions
{
    public static IMySqlRepository Create(this IOrmDbFactory dbFactory, string tenantId = null)
        => dbFactory.CreateRepository(tenantId) as IMySqlRepository;
}
