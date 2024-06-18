using Trolley.MySqlConnector;
using Trolley.SqlServer;

namespace Trolley.Dialects;

public static class DialectExtensions
{
    public static IMySqlRepository CreateMySqlRepository(this IOrmDbFactory dbFactory, string dbKey = null)
        => dbFactory.CreateRepository(dbKey) as IMySqlRepository;
    public static ISqlServerRepository CreateSqlServerRepository(this IOrmDbFactory dbFactory, string dbKey = null)
        => dbFactory.CreateRepository(dbKey) as ISqlServerRepository;
}
