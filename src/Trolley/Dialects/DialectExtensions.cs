using Trolley.MySqlConnector;

namespace Trolley.Dialects;

public static class DialectExtensions
{
    public static IMySqlRepository CreateMySqlRepository(this IOrmDbFactory dbFactory, string dbKey = null, string tenantId = null)
        => dbFactory.CreateRepository(dbKey, tenantId) as IMySqlRepository;
    //public static ISqlServerRepository CreateSqlServerRepository(this IOrmDbFactory dbFactory, string dbKey = null, string tenantId = null)
    //   => dbFactory.CreateRepository(dbKey, tenantId) as ISqlServerRepository; 
}
