using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Trolley;

public sealed class OrmDbFactory : IOrmDbFactory
{
    private readonly ConcurrentDictionary<OrmProviderType, IOrmProvider> ormProviders = new();
    private readonly ConcurrentDictionary<string, TheaDatabase> databases = new();

    private readonly ConcurrentDictionary<string, Delegate> masterConnectionStringSelectors = new();
    private readonly ConcurrentDictionary<OrmProviderType, Delegate> masterGlobalConnectionStringSelectors = new();
    private readonly ConcurrentDictionary<string, Delegate> slaveConnectionStringSelectors = new();
    private readonly ConcurrentDictionary<OrmProviderType, Delegate> slaveGlobalConnectionStringSelectors = new();

    private readonly ConcurrentDictionary<string, IEntityMapProvider> mapProviders = new();
    private readonly ConcurrentDictionary<OrmProviderType, IEntityMapProvider> globalMapProviders = new();
    private readonly ConcurrentDictionary<string, ITableShardingProvider> tableShardingProviders = new();
    private readonly ConcurrentDictionary<OrmProviderType, ITableShardingProvider> globalTableShardingProviders = new();

    private readonly ConcurrentDictionary<string, Delegate> complexMasterConnectionStringSelectors = new();
    private readonly ConcurrentDictionary<string, Delegate> complexSlaveConnectionStringSelectors = new();
    private readonly ConcurrentDictionary<string, IEntityMapProvider> complexMapProviders = new();
    private readonly ConcurrentDictionary<string, ITableShardingProvider> complexTableShardingProviders = new();
    private TheaDatabase defaultDatabase;

    /// <summary>
    /// 获取或设置命令超时时间，单位是秒，默认是30秒
    /// </summary>
    public int CommandTimeout { get; set; } = 30;
    /// <summary>
    /// 表达式中使用变量默认的参数名前缀，默认值是p，如：@p1,@p2等
    /// </summary>
    public string UserParameterPrefix { get; set; } = "p";
    /// <summary>
    /// 表达式解析中，常量是否参数化。如果设置为true，所有常量也将都会参数化，所有变量都会做参数化处理。
    /// </summary>
    public bool IsConstantParameterized { get; set; } = false;
    /// <summary>
    /// 枚举类型常量或变量，在未指定dbType类型时映射到数据库的默认类型，默认值是int类型
    /// </summary>
    public Type DefaultEnumMapDbType { get; set; } = typeof(int);
    /// <summary>
    /// DateTime、DateTimeOffset类型的DateTimeKind，默认是DateTimeKind.Local，如果返回的日期类型不是默认是DefaultDateTimeKind，将转换为DefaultDateTimeKind类型，如果值为DateTimeKind.Unspecified，将不做处理
    /// </summary>
    public DateTimeKind DefaultDateTimeKind { get; set; } = DateTimeKind.Local;
    /// <summary>
    /// 拦截器，默认为null
    /// </summary>
    public DbInterceptors DbInterceptors { get; set; } = new DbInterceptors();
    /// <summary>
    /// 字段映射处理器，默认为DefaultFieldMapHandler实例
    /// </summary>
    public IFieldMapHandler FieldMapHandler { get; set; } = new DefaultFieldMapHandler();


    public ICollection<TheaDatabase> Databases => this.databases.Values;
    public ICollection<IOrmProvider> OrmProviders => this.ormProviders.Values;

    public TheaDatabase Register(OrmProviderType ormProviderType, string dbKey, bool isDefault)
    {
        if (string.IsNullOrEmpty(dbKey)) throw new ArgumentNullException(nameof(dbKey));

        if (!this.ormProviders.TryGetValue(ormProviderType, out var ormProvider))
        {
            var type = this.GetOrmProviderType(ormProviderType);
            ormProvider = RepositoryHelper.CreateInstance(type) as IOrmProvider;
            this.ormProviders.TryAdd(ormProviderType, ormProvider);
        }

        TheaDatabase database;
        if (!this.databases.TryAdd(dbKey, database = new TheaDatabase
        {
            DbKey = dbKey,
            OrmProviderType = ormProviderType,
            OrmProvider = ormProvider,
            IsDefault = isDefault
        })) throw new Exception($"dbKey:{database.DbKey}数据库已经存在！");
        if (isDefault) this.defaultDatabase = database;
        return database;
    }

    /// <summary>
    /// dbKey将作为默认数据库的dbKey
    /// </summary>
    /// <param name="dbKey">默认数据库的dbKey</param>
    /// <exception cref="Exception"></exception>
    public void UseDefaultDatabase(string dbKey)
    {
        foreach (var database in this.databases.Values)
            database.IsDefault = database.DbKey == dbKey;
        if (!this.databases.TryGetValue(dbKey, out var defaultDatabase))
            throw new Exception($"未配置dbKey:{dbKey}的数据库");
        this.defaultDatabase = defaultDatabase;
    }

    public bool TryGetConnectionStringSelector(string dbKey, out Delegate connectionStringSelector)
        => this.TryGetMasterConnectionStringSelector(dbKey, out connectionStringSelector);
    public void AddConnectionStringSelector(string dbKey, Delegate connectionStringSelector)
        => this.AddMasterConnectionStringSelector(dbKey, connectionStringSelector);
    public bool TryGetConnectionStringSelector(OrmProviderType ormProviderType, out Delegate connectionStringSelector)
        => this.TryGetMasterConnectionStringSelector(ormProviderType, out connectionStringSelector);
    public void AddConnectionStringSelector(OrmProviderType ormProviderType, Delegate connectionStringSelector)
        => this.AddMasterConnectionStringSelector(ormProviderType, connectionStringSelector);

    public bool TryGetMasterConnectionStringSelector(string dbKey, out Delegate connectionStringSelector)
       => this.masterConnectionStringSelectors.TryGetValue(dbKey, out connectionStringSelector);
    public void AddMasterConnectionStringSelector(string dbKey, Delegate connectionStringSelector)
        => this.masterConnectionStringSelectors.TryAdd(dbKey, connectionStringSelector);
    public bool TryGetMasterConnectionStringSelector(OrmProviderType ormProviderType, out Delegate connectionStringSelector)
        => this.masterGlobalConnectionStringSelectors.TryGetValue(ormProviderType, out connectionStringSelector);
    public void AddMasterConnectionStringSelector(OrmProviderType ormProviderType, Delegate connectionStringSelector)
        => this.masterGlobalConnectionStringSelectors.TryAdd(ormProviderType, connectionStringSelector);

    public bool TryGetSlaveConnectionStringSelector(string dbKey, out Delegate connectionStringSelector)
        => this.slaveConnectionStringSelectors.TryGetValue(dbKey, out connectionStringSelector);
    public void AddSlaveConnectionStringSelector(string dbKey, Delegate connectionStringSelector)
        => this.slaveConnectionStringSelectors.TryAdd(dbKey, connectionStringSelector);
    public bool TryGetSlaveConnectionStringSelector(OrmProviderType ormProviderType, out Delegate connectionStringSelector)
        => this.slaveGlobalConnectionStringSelectors.TryGetValue(ormProviderType, out connectionStringSelector);
    public void AddSlaveConnectionStringSelector(OrmProviderType ormProviderType, Delegate connectionStringSelector)
        => this.slaveGlobalConnectionStringSelectors.TryAdd(ormProviderType, connectionStringSelector);

    public bool TryGetTableShardingProvider(string dbKey, out ITableShardingProvider tableShardingProvider)
        => this.tableShardingProviders.TryGetValue(dbKey, out tableShardingProvider);
    public void AddTableShardingProvider(string dbKey, ITableShardingProvider tableShardingProvider)
        => this.tableShardingProviders.TryAdd(dbKey, tableShardingProvider);
    public bool TryGetTableShardingProvider(OrmProviderType ormProviderType, out ITableShardingProvider tableShardingProvider)
        => this.globalTableShardingProviders.TryGetValue(ormProviderType, out tableShardingProvider);
    public void AddTableShardingProvider(OrmProviderType ormProviderType, ITableShardingProvider tableShardingProvider)
        => this.globalTableShardingProviders.TryAdd(ormProviderType, tableShardingProvider);
    public bool TryGetTableSharding(string dbKey, Type entityType, out TableShardingInfo tableShardingInfo)
    {
        if (this.tableShardingProviders.TryGetValue(dbKey, out var tableShardingProvider)
            && tableShardingProvider.TryGetTableSharding(entityType, out tableShardingInfo))
            return true;
        var database = this.GetDatabase(dbKey);
        if (this.globalTableShardingProviders.TryGetValue(database.OrmProviderType, out tableShardingProvider)
            && tableShardingProvider.TryGetTableSharding(entityType, out tableShardingInfo))
            return true;
        tableShardingInfo = null;
        return false;
    }
    public bool TryGetTableSharding(OrmProviderType ormProviderType, Type entityType, out TableShardingInfo tableShardingInfo)
    {
        if (this.globalTableShardingProviders.TryGetValue(ormProviderType, out var tableShardingProvider)
            && tableShardingProvider.TryGetTableSharding(entityType, out tableShardingInfo))
            return true;
        tableShardingInfo = null;
        return false;
    }

    public void AddOrmProvider(IOrmProvider ormProvider)
    {
        if (ormProvider == null)
            throw new ArgumentNullException(nameof(ormProvider));
        this.ormProviders.TryAdd(ormProvider.OrmProviderType, ormProvider);
    }
    public bool TryGetOrmProvider(OrmProviderType ormProviderType, out IOrmProvider ormProvider)
        => this.ormProviders.TryGetValue(ormProviderType, out ormProvider);

    public void AddMapProvider(OrmProviderType ormProviderType, IEntityMapProvider entityMapProvider)
    {
        if (entityMapProvider == null)
            throw new ArgumentNullException(nameof(entityMapProvider));
        this.globalMapProviders.TryAdd(ormProviderType, entityMapProvider);
    }
    public bool TryGetMapProvider(OrmProviderType ormProviderType, out IEntityMapProvider entityMapProvider)
        => this.globalMapProviders.TryGetValue(ormProviderType, out entityMapProvider);
    public void AddMapProvider(string dbKey, IEntityMapProvider entityMapProvider)
    {
        if (string.IsNullOrEmpty(dbKey))
            throw new ArgumentNullException(nameof(dbKey));
        if (entityMapProvider == null)
            throw new ArgumentNullException(nameof(entityMapProvider));

        this.mapProviders.TryAdd(dbKey, entityMapProvider);
    }
    public bool TryGetMapProvider(string dbKey, out IEntityMapProvider entityMapProvider)
        => this.mapProviders.TryGetValue(dbKey, out entityMapProvider);

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
    public IRepository CreateRepository(string dbKey = null)
    {
        //如果有指定dbKey，就是使用指定的dbKey创建IRepository对象,如果也没有指定，就使用配置的默认dbKey
        var localDbKey = dbKey ?? this.defaultDatabase?.DbKey;
        if (string.IsNullOrEmpty(localDbKey))
            throw new ArgumentNullException(nameof(dbKey), "dbKey不可为null，未配置dbKey，也没有配置默认数据库");

        var database = this.GetDatabase(localDbKey);
        //会不需要实体映射和分表规则的场景
        this.complexMapProviders.TryGetValue(localDbKey, out var mapProvider);
        this.complexTableShardingProviders.TryGetValue(localDbKey, out var tableShardingProvider);

        return database.OrmProvider.CreateRepository(new DbContext
        {
            DbKey = localDbKey,
            Database = database,
            //mysql默认Schema是数据库名，暂时此处为null,pgsql的默认Schema是public，sqlserver的默认Schema是dbo
            DefaultTableSchema = database.OrmProvider.DefaultTableSchema,
            OrmProvider = database.OrmProvider,
            MapProvider = mapProvider,
            ShardingProvider = tableShardingProvider,
            DefaultDateTimeKind = this.DefaultDateTimeKind,
            UserParameterPrefix = this.UserParameterPrefix,
            CommandTimeout = this.CommandTimeout,
            FieldMapHandler = this.FieldMapHandler,
            DbInterceptors = this.DbInterceptors,
            DefaultEnumMapDbType = this.DefaultEnumMapDbType,
            IsConstantParameterized = this.IsConstantParameterized
        });
    }
    public IRepository CreateRepository(DbContext dbContext)
    {
        var ormProvider = dbContext.OrmProvider;
        return ormProvider.CreateRepository(dbContext);
    }
    public Type GetOrmProviderType(OrmProviderType ormProviderType)
    {
        string fileName = null;
        string strOrmProviderType = null;
        switch (ormProviderType)
        {
            case OrmProviderType.MySql:
                fileName = "Trolley.MySqlConnector.dll";
                strOrmProviderType = "Trolley.MySqlConnector.MySqlProvider, Trolley.MySqlConnector";
                break;
            case OrmProviderType.PostgreSql:
                fileName = "Trolley.PostgreSql.dll";
                strOrmProviderType = "Trolley.PostgreSql.PostgreSqlProvider, Trolley.PostgreSql";
                break;
            case OrmProviderType.SqlServer:
                fileName = "Trolley.SqlServer.dll";
                strOrmProviderType = "Trolley.SqlServer.SqlServerProvider, Trolley.SqlServer";
                break;
        }
        var type = Type.GetType(strOrmProviderType);
        var packageName = fileName.Replace(".dll", string.Empty);
        if (type == null)
            throw new DllNotFoundException($"没有找到[{fileName}]文件，或是没有引入[{packageName}]nuget包");
        return type;
    }

    public void Build()
    {
        //连接串选择器、实体映射、分表规则，都需要按照dbKey来进行分类存储，这样确保从dbKey开始数据库操作后，可以更快的使用他们
        this.BuildConnectionStringSelectors(this.masterConnectionStringSelectors,
            this.masterGlobalConnectionStringSelectors, this.complexMasterConnectionStringSelectors,
            (database, connectionStringSelector) => database.UseMasterSelector(connectionStringSelector));

        this.BuildConnectionStringSelectors(this.slaveConnectionStringSelectors,
            this.slaveGlobalConnectionStringSelectors, this.complexSlaveConnectionStringSelectors,
            (database, connectionStringSelector) => database.UseSlaveSelector(connectionStringSelector));

        //遍历所有数据库，未设置连接串选择器的，默认设置轮询方式选择连接串
        foreach (var database in this.Databases)
            database.Build();

        if (!this.mapProviders.IsEmpty)
        {
            //遍历所有数据库，映射实体对象
            foreach (var mapProvider in this.mapProviders)
            {
                var database = this.GetDatabase(mapProvider.Key);
                mapProvider.Value.Build(database, this.FieldMapHandler);
                this.complexMapProviders.TryAdd(database.DbKey, mapProvider.Value);
            }
        }
        if (!this.globalMapProviders.IsEmpty)
        {
            foreach (var mapProvider in this.globalMapProviders)
            {
                foreach (var database in this.Databases)
                {
                    //前面已经映射过了，就不再映射
                    if (this.complexMapProviders.ContainsKey(database.DbKey)
                        || database.OrmProviderType != mapProvider.Key)
                        continue;
                    //确保每个实体都映射到，如果映射过了，不再映射
                    //有时候一个数据库并不能映射完所有实体，有的实体在其他数据库中使用，所以需要遍历所有数据库映射
                    mapProvider.Value.Build(database, this.FieldMapHandler);
                    this.complexMapProviders.TryAdd(database.DbKey, mapProvider.Value);
                }
            }
        }
        if (!this.tableShardingProviders.IsEmpty)
        {
            foreach (var tableShardingProvider in this.tableShardingProviders)
                this.complexTableShardingProviders.TryAdd(tableShardingProvider.Key, tableShardingProvider.Value);
        }
        if (!this.globalTableShardingProviders.IsEmpty)
        {
            foreach (var globalTableShardingProvider in this.globalTableShardingProviders)
            {
                foreach (var database in this.databases.Values)
                {
                    if (this.complexTableShardingProviders.ContainsKey(database.DbKey)
                        || database.OrmProviderType != globalTableShardingProvider.Key)
                        continue;
                    this.complexTableShardingProviders.TryAdd(database.DbKey, globalTableShardingProvider.Value);
                }
            }
        }
    }
    private void BuildConnectionStringSelectors(ConcurrentDictionary<string, Delegate> connectionStringSelectors,
        ConcurrentDictionary<OrmProviderType, Delegate> globalConnectionStringSelectors, ConcurrentDictionary<string, Delegate> complexConnectionStringSelectors,
        Action<TheaDatabase, Delegate> useConnectionStringSelector)
    {
        if (connectionStringSelectors.IsEmpty && globalConnectionStringSelectors.IsEmpty)
            return;

        if (!connectionStringSelectors.IsEmpty)
        {
            foreach (var connectionStringSelector in connectionStringSelectors)
            {
                complexConnectionStringSelectors.TryAdd(connectionStringSelector.Key, connectionStringSelector.Value);
                var database = this.databases[connectionStringSelector.Key];
                useConnectionStringSelector.Invoke(database, connectionStringSelector.Value);
            }
        }
        if (!globalConnectionStringSelectors.IsEmpty)
        {
            foreach (var globalConnectionStringSelector in globalConnectionStringSelectors)
            {
                foreach (var database in this.Databases)
                {
                    //前面已经映射过了，就不再映射
                    if (complexConnectionStringSelectors.ContainsKey(database.DbKey)
                        || database.OrmProviderType != globalConnectionStringSelector.Key)
                        continue;
                    complexConnectionStringSelectors.TryAdd(database.DbKey, globalConnectionStringSelector.Value);
                    useConnectionStringSelector.Invoke(database, globalConnectionStringSelector.Value);
                }
            }
        }
    }
}