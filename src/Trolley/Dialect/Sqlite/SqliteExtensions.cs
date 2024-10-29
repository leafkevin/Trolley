namespace Trolley.Sqlite;

public static class SqliteExtensions
{
    public static ISqliteRepository Create(this IOrmDbFactory dbFactory, string dbKey = null)
        => dbFactory.CreateRepository(dbKey) as ISqliteRepository;
}