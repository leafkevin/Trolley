namespace Trolley.PostgreSql;

public static class PostgreSqlExtensions
{
    public static IPostgreSqlRepository Create(this IOrmDbFactory dbFactory, string dbKey = null)
        => dbFactory.CreateRepository(dbKey) as IPostgreSqlRepository;
}
