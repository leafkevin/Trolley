namespace Trolley.MySqlConnector;

public static class MySqlExtensions
{
    public static IMySqlRepository Create(this IOrmDbFactory dbFactory, string dbKey = null)
        => dbFactory.CreateRepository(dbKey) as IMySqlRepository;
    public static IMySqlRepository Create(this IOrmDbFactory dbFactory, DbContext dbContext)
        => dbFactory.CreateRepository(dbContext) as IMySqlRepository;
}