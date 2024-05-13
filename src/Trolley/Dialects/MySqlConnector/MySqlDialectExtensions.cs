namespace Trolley.MySqlConnector;

public static class MySqlDialectExtensions
{
    public static IMySqlRepository Create(this IOrmDbFactory dbFactory, string dbKey = null)
        => dbFactory.CreateRepository(dbKey) as IMySqlRepository;
}
