using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Trolley;

class OrmDbFactory : IOrmDbFactory
{
    private TheaDatabaseProvider defaultDatabaseProvider;
    private readonly ConcurrentDictionary<Type, IOrmProvider> ormProviders = new();
    private readonly ConcurrentDictionary<string, TheaDatabaseProvider> databaseProviders = new();
    private readonly ConcurrentDictionary<Type, IEntityMapProvider> entityMapProviders = new();
    private readonly TypeHandlerProvider typeHandlerProvider = new TypeHandlerProvider();
    private OrmDbFactoryOptions options;

    public TypeHandlerProvider TypeHandlerProvider => this.typeHandlerProvider;
    public ICollection<TheaDatabaseProvider> DatabaseProviders => this.databaseProviders.Values;
    public void Register(string dbKey, bool isDefault, Action<TheaDatabaseBuilder> databaseInitializer)
    {         
        if (!this.databaseProviders.TryGetValue(dbKey, out var database))
        {
            this.databaseProviders.TryAdd(dbKey, database = new TheaDatabaseProvider
            {
                DbKey = dbKey,
                IsDefault = isDefault
            });
        }
        if (isDefault) this.defaultDatabaseProvider = database;
        var builder = new TheaDatabaseBuilder(database);
        databaseInitializer?.Invoke(builder);
    }
    public void AddTypeHandler(ITypeHandler typeHandler)
        => this.typeHandlerProvider.AddTypeHandler(typeHandler);

    public bool TryGetTypeHandler(Type handlerType, out ITypeHandler typeHandler)
        => this.typeHandlerProvider.TryGetTypeHandler(handlerType, out typeHandler);

    public void AddOrmProvider(IOrmProvider ormProvider)
    {
        if (ormProvider == null)
            throw new ArgumentNullException(nameof(ormProvider));

        var ormProviderType = ormProvider.GetType();
        this.ormProviders.TryAdd(ormProviderType, ormProvider);
    }
    public bool TryGetOrmProvider(Type ormProviderType, out IOrmProvider ormProvider)
    {
        if (ormProviderType == null)
            throw new ArgumentNullException(nameof(ormProviderType));

        return this.ormProviders.TryGetValue(ormProviderType, out ormProvider);
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
    public void Configure(Type ormProviderType, IModelConfiguration configuration)
    {
        if (ormProviderType == null)
            throw new ArgumentNullException(nameof(ormProviderType));

        if (!this.TryGetEntityMapProvider(ormProviderType, out var mapProvider))
            this.AddEntityMapProvider(ormProviderType, mapProvider = new EntityMapProvider());

        configuration.OnModelCreating(new ModelBuilder(mapProvider));
    }
    public List<TheaDatabaseProvider> GetDatabaseProviders()
        => this.databaseProviders.Values.ToList();
    public TheaDatabaseProvider GetDatabaseProvider(string dbKey = null)
    {
        if (string.IsNullOrEmpty(dbKey))
        {
            if (this.defaultDatabaseProvider == null)
                throw new Exception($"未配置默认数据库连接串");
            return this.defaultDatabaseProvider;
        }
        if (!this.databaseProviders.TryGetValue(dbKey, out var database))
            throw new Exception($"dbKey:{dbKey}未配置任何数据库连接串");
        return database;
    }
    public IRepository Create(string dbKey = null, int? tenantId = null)
    {
        var databaseProvider = this.GetDatabaseProvider(dbKey);
        var database = databaseProvider.GetDatabase(tenantId);
        if (!this.TryGetOrmProvider(database.OrmProviderType, out var ormProvider))
            throw new Exception($"未注册类型为{database.OrmProviderType.FullName}的OrmProvider");

        var baseConnection = ormProvider.CreateConnection(database.ConnectionString);
        if (!this.TryGetEntityMapProvider(database.OrmProviderType, out var entityMapProvider))
            throw new Exception($"未注册Key为{database.OrmProviderType.FullName}的EntityMapProvider");

        var connection = new TheaConnection()
        {
            DbKey = dbKey,
            ConnectionString = database.ConnectionString,
            BaseConnection = baseConnection
        };
        return new Repository(connection, ormProvider, entityMapProvider);
    }
    internal void With(OrmDbFactoryOptions options) => this.options = options;
}
