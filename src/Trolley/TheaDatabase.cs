using System;
using System.Collections.Generic;

namespace Trolley;

public class TheaDatabase
{
    private readonly Dictionary<string, TenantDatabase> tenantDatabases = new();

    public string DbKey { get; set; }
    public bool IsDefault { get; set; }
    public Type OrmProviderType { get; set; }
    public TenantDatabase DefaultDatabase { get; private set; }
    public List<TenantDatabase> TenantDatabases { get; private set; } = new();

    public TenantDatabase GetTenantDatabase(string tenantId = null)
    {
        var tId = tenantId ?? string.Empty;
        if (this.tenantDatabases.TryGetValue(tId, out var result))
            return result;
        if (string.IsNullOrEmpty(tenantId))
            throw new Exception($"dbKey:{this.DbKey}未配置默认数据库连接串");
        else throw new Exception($"dbKey:{this.DbKey}未配置租户Id:{tenantId}的数据库连接串，也未配置默认数据库连接串");
    }
    public void AddTenantDatabase(TenantDatabase database)
    {
        if (database.TenantIds != null)
        {
            foreach (var tenantId in database.TenantIds)
            {
                if (!this.tenantDatabases.TryAdd(tenantId, database))
                    throw new Exception($"租户ID:{tenantId}租户数据库连接串已存在");
            }
        }
        this.TenantDatabases.Add(database);
        if (database.IsDefault)
        {
            this.DefaultDatabase = database;
            this.tenantDatabases[string.Empty] = database;
        }
    }
}
public class TenantDatabase
{
    public string ConnectionString { get; set; }
    public bool IsDefault { get; set; }
    public string[] TenantIds { get; set; }
}
