namespace Trolley.PostgreSql;

public static class PostgreSqlExtensions
{
    public static IPostgreSqlRepository Create(this IOrmDbFactory dbFactory, params object[] dbKeySelectorValues)
        => dbFactory.Create<IPostgreSqlRepository>(dbKeySelectorValues);
    public static IPostgreSqlRepository CreateRepository(this IOrmDbFactory dbFactory, string dbKey)
        => dbFactory.CreateRepository<IPostgreSqlRepository>(dbKey);
}