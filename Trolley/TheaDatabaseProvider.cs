using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Trolley;

public class TheaDatabaseProvider
{
    public TheaDatabase defaultDatabase;
    private readonly ConcurrentDictionary<int, TheaDatabase> tenantDatabases = new();
    private readonly ConcurrentDictionary<Type, IEntityMapProvider> entityMapProviders = new();

    public string DbKey { get; set; }
    public bool IsDefault { get; set; }
    public List<TheaDatabase> Databases { get; set; } = new();

    public TheaDatabase GetDatabase(int? tenantId = null)
    {
        if (tenantId.HasValue && this.tenantDatabases.TryGetValue(tenantId.Value, out var result))
            return result;
        if (this.tenantDatabases.TryGetValue(0, out result))
            return result;
        if (tenantId.HasValue)
            throw new Exception($"dbKey:{this.DbKey}未配置租户Id:{tenantId}的数据库连接串，也未配置默认数据库连接串");
        throw new Exception($"dbKey:{this.DbKey}未配置默认数据库连接串");
    }
    public void AddDatabase(TheaDatabase database)
    {
        if (database.TenantIds != null)
        {
            foreach (var tenantId in database.TenantIds)
            {
                if (this.tenantDatabases.ContainsKey(tenantId))
                    throw new Exception($"租户ID:{tenantId}租户数据库连接串已存在");
                this.tenantDatabases.TryAdd(tenantId, database);
            }
        }
        this.Databases.Add(database);
        if (database.IsDefault)
        {
            this.defaultDatabase = database;
            this.tenantDatabases[0] = database;
        }
    }
    public void AddEntityMapProvider(Type ormProviderType, IEntityMapProvider entityMapProvider)
    {
        if (ormProviderType == null)
            throw new ArgumentNullException(nameof(ormProviderType));
        if (entityMapProvider == null)
            throw new ArgumentNullException(nameof(entityMapProvider));

        this.entityMapProviders.TryAdd(ormProviderType, entityMapProvider);
    }
    public bool TryGetEntityMapProvider(Type ormProviderType, out IEntityMapProvider entityMapProvider)
    {
        if (ormProviderType == null)
            throw new ArgumentNullException(nameof(ormProviderType));

        return this.entityMapProviders.TryGetValue(ormProviderType, out entityMapProvider);
    }
    public void Build(IOrmDbFactory dbFactory, ITypeHandlerProvider typeHandlerProvider)
    {
        foreach (var ormProviderType in this.entityMapProviders.Keys)
        {
            var mapProvider = this.entityMapProviders[ormProviderType];
            if (!dbFactory.TryGetOrmProvider(ormProviderType, out var ormProvider))
            {
                ormProvider = Activator.CreateInstance(ormProviderType) as IOrmProvider;
                dbFactory.AddOrmProvider(ormProvider);
            }
            foreach (var entityMapper in mapProvider.EntityMaps)
            {
                entityMapper.Build(ormProvider, typeHandlerProvider);
            }
        }
    }
}
