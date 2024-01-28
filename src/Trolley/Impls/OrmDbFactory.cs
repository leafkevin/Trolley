using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Trolley;

public sealed class OrmDbFactory : IOrmDbFactory
{
    private OrmDbFactoryOptions options;
    private TenantDatabase defaultDatabase;
    private ConcurrentDictionary<string, string> connStrings = new();
    private Dictionary<OrmProviderType, IOrmProvider> typedOrmProviders = new();
    private ConcurrentDictionary<Type, IOrmProvider> ormProviders = new();
    private ConcurrentDictionary<string, TenantDatabase> databases = new();
    private ConcurrentDictionary<Type, IEntityMapProvider> mapProviders = new();

    public ICollection<TenantDatabase> Databases => this.databases.Values;
    public ICollection<IOrmProvider> OrmProviders => this.ormProviders.Values;
    public ICollection<IEntityMapProvider> MapProviders => this.mapProviders.Values;

    public void Register(string dbKey, string connectionString, Type ormProviderType, bool isDefault)
    {
        if (string.IsNullOrEmpty(dbKey)) throw new ArgumentNullException(nameof(dbKey));
        if (string.IsNullOrEmpty(connectionString)) throw new ArgumentNullException(nameof(connectionString));
        if (ormProviderType == null) throw new ArgumentNullException(nameof(ormProviderType));

        TenantDatabase database = null;
        if (!this.databases.TryAdd(dbKey, database = new TenantDatabase
        {
            DbKey = dbKey,
            ConnectionString = connectionString,
            IsDefault = isDefault,
            OrmProviderType = ormProviderType
        })) throw new Exception($"dbKey:{database.DbKey}数据库已经添加");

        if (isDefault) this.defaultDatabase = database;
        if (!this.ormProviders.ContainsKey(ormProviderType))
        {
            var ormProvider = Activator.CreateInstance(ormProviderType) as IOrmProvider;
            if (this.ormProviders.TryAdd(ormProviderType, ormProvider))
                this.typedOrmProviders.TryAdd(ormProvider.OrmProviderType, ormProvider);
        }
    }
    public void AddOrmProvider(IOrmProvider ormProvider)
    {
        if (ormProvider == null)
            throw new ArgumentNullException(nameof(ormProvider));

        var ormProviderType = ormProvider.GetType();
        if (this.ormProviders.TryAdd(ormProviderType, ormProvider))
            this.typedOrmProviders.TryAdd(ormProvider.OrmProviderType, ormProvider);
    }
    public bool TryGetOrmProvider(Type ormProviderType, out IOrmProvider ormProvider)
    {
        if (ormProviderType == null)
            throw new ArgumentNullException(nameof(ormProviderType));

        return this.ormProviders.TryGetValue(ormProviderType, out ormProvider);
    }
    public bool TryGetOrmProvider(OrmProviderType ormProviderType, out IOrmProvider ormProvider)
        => this.typedOrmProviders.TryGetValue(ormProviderType, out ormProvider);
    public void AddMapProvider(Type ormProviderType, IEntityMapProvider entityMapProvider)
    {
        if (ormProviderType == null)
            throw new ArgumentNullException(nameof(ormProviderType));
        if (entityMapProvider == null)
            throw new ArgumentNullException(nameof(entityMapProvider));

        this.mapProviders.TryAdd(ormProviderType, entityMapProvider);
    }
    public bool TryGetMapProvider(Type ormProviderType, out IEntityMapProvider entityMapProvider)
    {
        if (ormProviderType == null)
            throw new ArgumentNullException(nameof(ormProviderType));

        return this.mapProviders.TryGetValue(ormProviderType, out entityMapProvider);
    }
    public TenantDatabase GetDatabase(string dbKey = null)
    {
        if (string.IsNullOrEmpty(dbKey))
        {
            if (this.defaultDatabase == null)
                throw new Exception($"未配置默认数据库");
            return this.defaultDatabase;
        }
        if (!this.databases.TryGetValue(dbKey, out var database))
            throw new Exception($"未配置dbKey:{dbKey}的数据库");
        return database;
    }
    //public  IRepository Create(string dbKey = null, string tenantId = null)
    //{
    //    var database = this.GetDatabase(dbKey);
    //    if (!this.TryGetOrmProvider(database.OrmProviderType, out var ormProvider))
    //        throw new Exception($"未注册类型为{database.OrmProviderType.FullName}的OrmProvider");
    //    var tenantDatabase = database.GetTenantDatabase(tenantId);
    //    if (!this.TryGetMapProvider(database.OrmProviderType, out var mapProvider))
    //        throw new Exception($"未注册Key为{database.OrmProviderType.FullName}的EntityMapProvider");
    //    var connection = new TheaConnection
    //    {
    //        DbKey = dbKey,
    //        BaseConnection = ormProvider.CreateConnection(tenantDatabase.ConnectionString),
    //        OrmProvider = ormProvider
    //    };
    //    return new Repository(dbKey, connection, ormProvider, mapProvider).With(this.options);
    //}


    public IRepository CreateRepository(string dbKey = null)
    {
        var database = this.GetDatabase(dbKey);
        var ormProviderType = database.OrmProviderType;
        if (!this.TryGetOrmProvider(ormProviderType, out var ormProvider))
            throw new Exception($"未注册类型为{ormProviderType.FullName}的OrmProvider");
        if (!this.TryGetMapProvider(ormProviderType, out var mapProvider))
            throw new Exception($"未注册Key为{ormProviderType.FullName}的EntityMapProvider");
        var dbContext = new DbContext
        {
            DbKey = dbKey ?? database.DbKey,
            ConnectionString = database.ConnectionString,
            OrmProvider = ormProvider,
            MapProvider = mapProvider,
            CommandTimeout = this.options?.Timeout ?? 30,
            IsParameterized = this.options?.IsParameterized ?? false
        };
        return ormProvider.CreateRepository(dbContext);
    }
    internal void With(Action<OrmDbFactoryOptions> optionsInitializer)
    {
        if (optionsInitializer == null)
            throw new ArgumentNullException(nameof(optionsInitializer));
        this.options = new OrmDbFactoryOptions();
        optionsInitializer.Invoke(this.options);
    }
    internal IOrmDbFactory Build()
    {
        foreach (var ormProviderType in this.ormProviders.Keys)
        {
            if (!this.TryGetMapProvider(ormProviderType, out var mapProvider))
                this.AddMapProvider(ormProviderType, new EntityMapProvider { OrmProviderType = ormProviderType });
            mapProvider.Build(this.ormProviders[ormProviderType]);
        }
        return this;
    }
}
