using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Trolley;

class OrmDbFactory : IOrmDbFactory
{
    private readonly ConcurrentDictionary<Type, IOrmProvider> ormProviders = new();
    private readonly ConcurrentDictionary<string, TheaDatabase> databases = new();
    private readonly ConcurrentDictionary<Type, EntityMap> entityMappers = new();
    private TheaDatabase defaultDatabase;

    public TheaDatabase Register(string dbKey, bool isDefault, Action<TheaDatabaseBuilder> databaseInitializer)
    {
        if (!this.databases.TryGetValue(dbKey, out var database))
        {
            this.databases.TryAdd(dbKey, database = new TheaDatabase
            {
                DbKey = dbKey,
                IsDefault = isDefault,
                ConnectionInfos = new List<TheaConnectionInfo>()
            });
        }
        if (isDefault) this.defaultDatabase = database;
        var builder = new TheaDatabaseBuilder(this, database);
        databaseInitializer?.Invoke(builder);
        return database;
    }
    public void AddOrmProvider(IOrmProvider ormProvider)
    {
        if (ormProvider == null)
            throw new ArgumentNullException(nameof(ormProvider));

        this.ormProviders.TryAdd(ormProvider.GetType(), ormProvider);
    }
    public void AddEntityMap(Type entityType, EntityMap mapper)
    {
        if (entityType == null)
            throw new ArgumentNullException(nameof(entityType));
        if (mapper == null)
            throw new ArgumentNullException(nameof(mapper));

        this.entityMappers.TryAdd(entityType, mapper);
    }
    public bool TryGetEntityMap(Type entityType, out EntityMap mapper)
    {
        if (entityType == null)
            throw new ArgumentNullException(nameof(entityType));

        return this.entityMappers.TryGetValue(entityType, out mapper);
    }

    public IRepository Create(TheaConnection connection) => new Repository(this, connection);
    public IRepository Create(string dbKey = null, int? tenantId = null)
    {
        var connectionInfo = this.GetConnectionInfo(dbKey, tenantId);
        var connection = new TheaConnection(connectionInfo);
        return new Repository(this, connection);
    }
    public TheaDatabase GetDatabase(string dbKey = null)
    {
        TheaDatabase database = null;
        if (string.IsNullOrEmpty(dbKey))
            database = this.defaultDatabase;
        else if (!this.databases.TryGetValue(dbKey, out database))
            throw new Exception($"未配置dbKey:{dbKey}数据库连接串");
        return database;
    }
    public TheaConnectionInfo GetConnectionInfo(string dbKey = null, int? tenantId = null)
    {
        var database = this.GetDatabase(dbKey);
        return database.GetConnectionInfo(tenantId);
    }
}
