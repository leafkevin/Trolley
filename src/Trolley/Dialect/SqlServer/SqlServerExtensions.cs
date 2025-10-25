namespace Trolley.SqlServer;

public static class SqlServerExtensions
{
    public static ISqlServerRepository Create(this IOrmDbFactory dbFactory, params object[] dbKeySelectorValues)
        => dbFactory.Create<ISqlServerRepository>(dbKeySelectorValues);
    public static ISqlServerRepository CreateRepository(this IOrmDbFactory dbFactory, string dbKey)
        => dbFactory.CreateRepository<ISqlServerRepository>(dbKey);
}