using System.Collections.Generic;

namespace Trolley;

public class TheaDatabaseBuilder
{
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaDatabase database;

    public TheaDatabaseBuilder(IOrmDbFactory dbFactory, TheaDatabase database)
    {
        this.dbFactory = dbFactory;
        this.database = database;
    }
    public TheaDatabaseBuilder Add(TheaConnectionInfo connectionInfo)
    {
        this.dbFactory.AddOrmProvider(connectionInfo.OrmProvider);
        this.database.ConnectionInfos.Add(connectionInfo);
        return this;
    }
    public TheaDatabaseBuilder Add(string connectionString, IOrmProvider ormProvider, bool isDefault, List<int> tenantIds = null)
    {
        this.dbFactory.AddOrmProvider(ormProvider);
        this.database.ConnectionInfos.Add(new TheaConnectionInfo
        {
            DbKey = this.database.DbKey,
            ConnectionString = connectionString,
            IsDefault = isDefault,
            OrmProvider = ormProvider,
            TenantIds = tenantIds
        });
        return this;
    }
    public TheaDatabaseBuilder Add<TOrmProvider>(string connectionString, bool isDefault, List<int> tenantIds = null) where TOrmProvider : IOrmProvider, new()
    {
        var ormProvider = new TOrmProvider();
        this.dbFactory.AddOrmProvider(ormProvider);
        this.database.ConnectionInfos.Add(new TheaConnectionInfo
        {
            DbKey = this.database.DbKey,
            ConnectionString = connectionString,
            IsDefault = isDefault,
            OrmProvider = ormProvider,
            TenantIds = tenantIds
        });
        return this;
    }
}
