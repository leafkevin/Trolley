using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Trolley;

public sealed class OrmDbFactory : IOrmDbFactory
{
    private readonly ConcurrentDictionary<OrmProviderType, IOrmProvider> ormProviders = new();
    private readonly ConcurrentDictionary<string, TheaDatabase> databases = new();
    private readonly ConcurrentDictionary<string, IEntityMapProvider> mapProviders = new();
    private readonly ConcurrentDictionary<OrmProviderType, IEntityMapProvider> globalMapProviders = new();

    private readonly ConcurrentDictionary<string, ITableShardingProvider> tableShardingProviders = new();
    private readonly ConcurrentDictionary<OrmProviderType, ITableShardingProvider> globalTableShardingProviders = new();
    private ConcurrentDictionary<string, IEntityMapProvider> complexMapProviders;
    private ConcurrentDictionary<string, ITableShardingProvider> complexTableShardingProviders;

    private OrmDbFactoryOptions options = new();
    private Func<string> dbKeySelector;
    private TheaDatabase defaultDatabase;

    public ICollection<TheaDatabase> Databases => this.databases.Values;
    public ICollection<IOrmProvider> OrmProviders => this.ormProviders.Values;
    public OrmDbFactoryOptions Options => this.options;

    public TheaDatabase Register(OrmProviderType ormProviderType, string dbKey, string connectionString)
    {
        if (string.IsNullOrEmpty(dbKey)) throw new ArgumentNullException(nameof(dbKey));
        if (string.IsNullOrEmpty(connectionString)) throw new ArgumentNullException(nameof(connectionString));

        if (!this.ormProviders.TryGetValue(ormProviderType, out var ormProvider))
        {
            var type = this.GetOrmProviderType(ormProviderType);
            ormProvider = Activator.CreateInstance(type) as IOrmProvider;
            this.ormProviders.TryAdd(ormProviderType, ormProvider);
        }

        TheaDatabase database;
        if (!this.databases.TryAdd(dbKey, database = new TheaDatabase
        {
            DbKey = dbKey,
            ConnectionString = connectionString,
            OrmProviderType = ormProviderType,
            OrmProvider = ormProvider
        })) throw new Exception($"dbKey:{database.DbKey}数据库已经添加");
        return database;
    }
    public void Register(OrmProviderType ormProviderType, string dbKey, string connectionString, bool isDefault)
    {
        if (string.IsNullOrEmpty(dbKey)) throw new ArgumentNullException(nameof(dbKey));
        if (string.IsNullOrEmpty(connectionString)) throw new ArgumentNullException(nameof(connectionString));

        if (!this.ormProviders.TryGetValue(ormProviderType, out var ormProvider))
        {
            var type = this.GetOrmProviderType(ormProviderType);
            ormProvider = Activator.CreateInstance(type) as IOrmProvider;
            this.ormProviders.TryAdd(ormProviderType, ormProvider);
        }

        TheaDatabase database;
        if (!this.databases.TryAdd(dbKey, database = new TheaDatabase
        {
            DbKey = dbKey,
            ConnectionString = connectionString,
            IsDefault = isDefault,
            OrmProviderType = ormProviderType,
            OrmProvider = ormProvider
        })) throw new Exception($"dbKey:{database.DbKey}数据库已经添加");
        if (isDefault) this.defaultDatabase = database;
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
    /// <summary>
    /// 配置分库dbKey获取委托，配置此委托后，使用未指定dbKey的IOrmDbFactory.CreateRepository方法创建每个Repository对象，都将调用此委托。如：
    /// <code>
    /// .UseDatabaseSharding(() =&gt;
    /// {
    ///     var passport = f.GetService&lt;IPassport&gt;();
    ///     return passport.TenantId switch
    ///     {
    ///         200 =&gt; "dbKey1",
    ///         300 =&gt; "dbKey2",
    ///         _ =&gt; "defaultDbKey"
    ///     }
    /// });
    /// </code>
    /// </summary>
    /// <param name="dbKeySelector">dbKey获取委托</param>
    public void UseDatabaseSharding(Func<string> dbKeySelector)
        => this.dbKeySelector = dbKeySelector ?? throw new ArgumentNullException(nameof(dbKeySelector));

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
        //如果有指定dbKey，就是使用指定的dbKey创建IRepository对象
        //如果没有指定dbKey，再判断是否有指定分库规则，有指定就调用分库规则获取dbKey
        //如果也没有指定分库规则，就使用配置的默认dbKey
        var localDbKey = dbKey ?? this.dbKeySelector?.Invoke() ?? this.defaultDatabase?.DbKey;
        if (string.IsNullOrEmpty(localDbKey))
            throw new ArgumentNullException(nameof(dbKey), "请配置dbKey，既没有设置分库规则来获取dbKey，也没有设置默认的dbKey");

        var database = this.GetDatabase(localDbKey);
        if (!this.complexMapProviders.TryGetValue(localDbKey, out var mapProvider))
            throw new Exception($"没有注册dbKey：{localDbKey}的IEntityMapProvider对象，也没有注册OrmProviderType：{database.OrmProviderType}的IEntityMapProvider对象");
        this.complexTableShardingProviders.TryGetValue(localDbKey, out var tableShardingProvider);

        //只是为了获取默认TableSchema      
        var connection = database.OrmProvider.CreateConnection(localDbKey, database.ConnectionString);
        var defaultSchema = database.OrmProvider.DefaultTableSchema ?? connection.Database;
        connection.Dispose();
        var dbContext = new DbContext
        {
            DbKey = localDbKey,
            Database = database,
            DefaultTableSchema = defaultSchema,
            OrmProvider = database.OrmProvider,
            MapProvider = mapProvider,
            ShardingProvider = tableShardingProvider,
            Options = this.options
        };
        return database.OrmProvider.CreateRepository(dbContext);
    }
    public void With(Action<OrmDbFactoryOptions> optionsInitializer)
    {
        if (optionsInitializer == null)
            throw new ArgumentNullException(nameof(optionsInitializer));
        optionsInitializer.Invoke(this.options);
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
        if (!this.mapProviders.IsEmpty)
        {
            foreach (var mapProvider in this.mapProviders)
            {
                var database = this.GetDatabase(mapProvider.Key);
                mapProvider.Value.Build(database);
            }
        }
        if (!this.globalMapProviders.IsEmpty)
        {
            foreach (var mapProvider in this.globalMapProviders)
            {
                foreach (var database in this.Databases)
                {
                    if (database.OrmProviderType == mapProvider.Key)
                    {
                        mapProvider.Value.Build(database);
                        break;
                    }
                }
            }
        }
        if (this.globalMapProviders.IsEmpty)
            this.complexMapProviders = this.mapProviders;
        else
        {
            this.complexMapProviders = new();
            foreach (var database in this.databases.Values)
            {
                var ormProviderType = database.OrmProviderType;
                this.TryGetMapProvider(database.DbKey, out var mapProvider);
                this.TryGetMapProvider(ormProviderType, out var globalMapProvider);
                var entityMapProvider = new ComplexEntityMapProvider(mapProvider, globalMapProvider, this.options.FieldMapHandler);
                entityMapProvider.Build(database);
                this.complexMapProviders.TryAdd(database.DbKey, entityMapProvider);
            }
        }
        if (this.globalTableShardingProviders.IsEmpty)
            this.complexTableShardingProviders = this.tableShardingProviders;
        else
        {
            this.complexTableShardingProviders = new();
            foreach (var database in this.databases.Values)
            {
                var ormProviderType = database.OrmProviderType;
                this.TryGetTableShardingProvider(database.DbKey, out var tbleShardingProvider);
                this.TryGetTableShardingProvider(ormProviderType, out var globalTbleShardingProvider);
                var complexTableShardingProvider = new ComplexTableShardingProvider(tbleShardingProvider, globalTbleShardingProvider);
                this.complexTableShardingProviders.TryAdd(database.DbKey, complexTableShardingProvider);
            }
        }
    }
}
