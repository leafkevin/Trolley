namespace Trolley.MySqlConnector;

public static class MySqlExtensions
{
    public static IMySqlRepository Create(this IOrmDbFactory dbFactory, params object[] dbKeySelectorValues)
        => dbFactory.Create<IMySqlRepository>(dbKeySelectorValues);
    public static IMySqlRepository CreateRepository(this IOrmDbFactory dbFactory, string dbKey)
        => dbFactory.CreateRepository<IMySqlRepository>(dbKey);
}