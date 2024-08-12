using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Trolley;

public sealed class OrmDbFactory : IOrmDbFactory
{
    private OrmDbFactoryOptions options;
    private TheaDatabase defaultDatabase;
    private Dictionary<OrmProviderType, IOrmProvider> typedOrmProviders = new();
    private ConcurrentDictionary<Type, IOrmProvider> ormProviders = new();
    private ConcurrentDictionary<string, TheaDatabase> databases = new();
    private ConcurrentDictionary<Type, IEntityMapProvider> mapProviders = new();
    private IShardingProvider shardingProvider = new ShardingProvider();
    private DbFilters dbFilters = new();

    public ICollection<TheaDatabase> Databases => this.databases.Values;
    public ICollection<IOrmProvider> OrmProviders => this.ormProviders.Values;
    public ICollection<IEntityMapProvider> MapProviders => this.mapProviders.Values;
    public DbFilters DbFilters => this.dbFilters;

    public void Register(string dbKey, string connectionString, Type ormProviderType, bool isDefault, string defaultTableSchema = null)
    {
        if (string.IsNullOrEmpty(dbKey)) throw new ArgumentNullException(nameof(dbKey));
        if (string.IsNullOrEmpty(connectionString)) throw new ArgumentNullException(nameof(connectionString));
        if (ormProviderType == null) throw new ArgumentNullException(nameof(ormProviderType));

        TheaDatabase database = null;
        if (!this.databases.TryAdd(dbKey, database = new TheaDatabase
        {
            DbKey = dbKey,
            ConnectionString = connectionString,
            IsDefault = isDefault,
            DefaultTableSchema = defaultTableSchema,
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
    public TheaDatabase GetDatabase(string dbKey = null)
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
    public void UseDatabase(Func<string> dbKeySelector) => this.shardingProvider.UseDatabase(dbKeySelector);
    public bool TryGetShardingTable(Type entityType, out ShardingTable shardingTable)
        => this.shardingProvider.TryGetShardingTable(entityType, out shardingTable);
    public void AddShardingTable(Type entityType, ShardingTable shardingTable)
        => this.shardingProvider.AddShardingTable(entityType, shardingTable);

    public IRepository CreateRepository(string dbKey = null)
    {
        //如果有指定dbKey，就是使用指定的dbKey创建IRepository对象
        //如果没有指定dbKey，再判断是否有指定分库规则，有指定就调用分库规则获取dbKey
        //如果也没有指定分库规则，就使用配置的默认dbKey
        var localDbKey = dbKey ?? this.shardingProvider.UseDefaultDatabase(this.defaultDatabase?.DbKey);
        if (string.IsNullOrEmpty(localDbKey))
            throw new ArgumentNullException(nameof(dbKey), "请配置dbKey，既没有设置分库规则来获取dbKey，也没有设置默认的dbKey");

        var database = this.GetDatabase(localDbKey);
        var ormProviderType = database.OrmProviderType;
        if (!this.TryGetOrmProvider(ormProviderType, out var ormProvider))
            throw new Exception($"未注册类型为{ormProviderType.FullName}的OrmProvider");
        if (!this.TryGetMapProvider(ormProviderType, out var mapProvider))
            throw new Exception($"未注册Key为{ormProviderType.FullName}的EntityMapProvider");
        var connection = ormProvider.CreateConnection(database.ConnectionString);
        this.dbFilters.OnConnectionCreated?.Invoke(new ConectionEventArgs
        {
            ConnectionId = Guid.NewGuid().ToString("N"),
            DbKey = localDbKey,
            ConnectionString = database.ConnectionString,
            OrmProvider = ormProvider,
            CreatedAt = DateTime.UtcNow
        });
        var dbContext = new DbContext
        {
            DbKey = localDbKey,
            ConnectionString = database.ConnectionString,
            Connection = new TheaConnection(connection),
            TableSchema = database.DefaultTableSchema ?? connection.Database,
            OrmProvider = ormProvider,
            MapProvider = mapProvider,
            ShardingProvider = this.shardingProvider,
            CommandTimeout = this.options?.Timeout ?? 30,
            IsParameterized = this.options?.IsParameterized ?? false,
            DbFilters = this.dbFilters
        };
        return ormProvider.CreateRepository(dbContext);
    }
    public void With(Action<OrmDbFactoryOptions> optionsInitializer)
    {
        if (optionsInitializer == null)
            throw new ArgumentNullException(nameof(optionsInitializer));
        this.options = new OrmDbFactoryOptions();
        optionsInitializer.Invoke(this.options);
    }
    public void Build()
    {
        foreach (var ormProviderType in this.ormProviders.Keys)
        {
            if (!this.TryGetMapProvider(ormProviderType, out var mapProvider))
                this.AddMapProvider(ormProviderType, new EntityMapProvider { OrmProviderType = ormProviderType });
            mapProvider.Build(this.ormProviders[ormProviderType]);
        }
    }
}
