namespace Trolley.MySqlConnector;

public static class MySqlDialectExtensions
{
    public static IMySqlRepository Create(this IOrmDbFactory dbFactory, string dbKey = null, string tenantId = null)
        => dbFactory.Create<IMySqlRepository>(dbKey, tenantId);
}
