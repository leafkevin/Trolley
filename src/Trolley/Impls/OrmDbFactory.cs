using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Trolley;

public sealed class OrmDbFactory : IOrmDbFactory
{
    private readonly ConcurrentDictionary<string, TheaDatabase> databases = new();
    private readonly ConcurrentDictionary<OrmProviderType, IOrmProvider> ormProviders = new();
    private readonly ConcurrentDictionary<string, IEntityMapProvider> entityMapProviders = new();
    private readonly ConcurrentDictionary<OrmProviderType, IEntityMapProvider> globalEntityMapProviders = new();
    private readonly ConcurrentDictionary<string, ITableShardingProvider> tableShardingProviders = new();
    private readonly ConcurrentDictionary<OrmProviderType, ITableShardingProvider> globalTableShardingProviders = new();
    private readonly ConcurrentDictionary<Type, ITypeHandler> typeHandlers = new();

    private TheaDatabase defaultDatabase;
    private Delegate dbKeySelector;
    private List<TheaDatabase> allDatabases = null;
    private List<IOrmProvider> allOrmProviders = null;
    private List<IEntityMapProvider> allEntityMapProviders = null;
    private List<ITableShardingProvider> allTableShardingProviders = null;
    private DbInterceptors dbInterceptors = new DbInterceptors();

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


    public Delegate DbKeySelector => this.dbKeySelector;
    public List<TheaDatabase> Databases => this.allDatabases;
    public List<IOrmProvider> OrmProviders => this.allOrmProviders;
    public List<IEntityMapProvider> EntityMapProviders => this.allEntityMapProviders;
    public List<ITableShardingProvider> TableShardingProviders => this.allTableShardingProviders;
    public DbInterceptors DbInterceptors => this.dbInterceptors;

    public void Register(TheaDatabase database)
    {
        if (string.IsNullOrEmpty(database.DbKey))
            throw new ArgumentNullException(nameof(database.DbKey));
        this.databases.TryAdd(database.DbKey, database);
        if (database.OrmProvider != null)
            this.ormProviders.TryAdd(database.OrmProviderType, database.OrmProvider);
        if (database.IsDefault)
            this.defaultDatabase = database;
    }
    public TheaDatabase GetDatabase(string dbKey)
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
    public void UseDbKeySelector(Delegate dbKeySelector)
        => this.dbKeySelector = dbKeySelector;

    public void UseOrmProvider(IOrmProvider ormProvider)
    {
        if (ormProvider == null)
            throw new ArgumentNullException(nameof(ormProvider));
        this.ormProviders.AddOrUpdate(ormProvider.OrmProviderType, ormProvider, (o, k) => ormProvider);
    }
    public bool TryGetOrmProvider(OrmProviderType ormProviderType, out IOrmProvider ormProvider)
        => this.ormProviders.TryGetValue(ormProviderType, out ormProvider);

    public void UseEntityMapProvider(string dbKey, IEntityMapProvider entityMapProvider)
    {
        if (string.IsNullOrEmpty(dbKey))
            throw new ArgumentNullException(nameof(dbKey));
        if (entityMapProvider == null)
            throw new ArgumentNullException(nameof(entityMapProvider));
        this.entityMapProviders.AddOrUpdate(dbKey, entityMapProvider, (o, k) => entityMapProvider);
    }
    public bool TryGetEntityMapProvider(string dbKey, out IEntityMapProvider entityMapProvider)
        => this.entityMapProviders.TryGetValue(dbKey, out entityMapProvider);
    public void UseEntityMapProvider(OrmProviderType ormProviderType, IEntityMapProvider entityMapProvider)
    {
        if (entityMapProvider == null)
            throw new ArgumentNullException(nameof(entityMapProvider));
        this.globalEntityMapProviders.AddOrUpdate(ormProviderType, entityMapProvider, (o, k) => entityMapProvider);
    }
    public bool TryGetEntityMapProvider(OrmProviderType ormProviderType, out IEntityMapProvider entityMapProvider)
        => this.globalEntityMapProviders.TryGetValue(ormProviderType, out entityMapProvider);

    public void UseTableShardingProvider(string dbKey, ITableShardingProvider tableShardingProvider)
    {
        if (tableShardingProvider == null)
            throw new ArgumentNullException(nameof(tableShardingProvider));
        this.tableShardingProviders.AddOrUpdate(dbKey, tableShardingProvider, (o, k) => tableShardingProvider);
    }
    public bool TryGetTableShardingProvider(string dbKey, out ITableShardingProvider tableShardingProvider)
        => this.tableShardingProviders.TryGetValue(dbKey, out tableShardingProvider);
    public void UseTableShardingProvider(OrmProviderType ormProviderType, ITableShardingProvider tableShardingProvider)
    {
        if (tableShardingProvider == null)
            throw new ArgumentNullException(nameof(tableShardingProvider));
        this.globalTableShardingProviders.AddOrUpdate(ormProviderType, tableShardingProvider, (o, k) => tableShardingProvider);
    }
    public bool TryGetTableShardingProvider(OrmProviderType ormProviderType, out ITableShardingProvider tableShardingProvider)
        => this.globalTableShardingProviders.TryGetValue(ormProviderType, out tableShardingProvider);

    public void UseTypeHandler(ITypeHandler typeHandler)
    {
        if (typeHandler == null)
            throw new ArgumentNullException(nameof(typeHandler));
        var typeHandlerType = typeHandler.GetType();
        this.typeHandlers.AddOrUpdate(typeHandlerType, typeHandler, (o, k) => typeHandler);
    }
    public IRepository Create(string dbKey = null)
    {
        if (string.IsNullOrEmpty(dbKey))
        {
            if (this.defaultDatabase == null)
                throw new ArgumentNullException(nameof(dbKey), "dbKey不可为null，也没有配置默认数据库");
            dbKey = this.defaultDatabase.DbKey;
        }
        var database = this.GetDatabase(dbKey);
        return database.OrmProvider.CreateRepository(new DbContext
        {
            DbKey = dbKey,
            Database = database,
            //mysql默认Schema是数据库名，暂时此处为null,pgsql的默认Schema是public，sqlserver的默认Schema是dbo
            DefaultTableSchema = database.OrmProvider.DefaultTableSchema,
            DbInterceptors = this.DbInterceptors,

            DefaultDateTimeKind = this.DefaultDateTimeKind,
            UserParameterPrefix = this.UserParameterPrefix,
            CommandTimeout = this.CommandTimeout,
            DefaultEnumMapDbType = this.DefaultEnumMapDbType,
            IsConstantParameterized = this.IsConstantParameterized
        });
    }
    public void Build()
    {
        this.allEntityMapProviders = new();
        this.allTableShardingProviders = new();
        if (!this.entityMapProviders.IsEmpty)
        {
            //遍历所有数据库，映射实体对象
            foreach (var dbKey in this.entityMapProviders.Keys)
            {
                var database = this.GetDatabase(dbKey);
                var entityMapProvider = this.entityMapProviders[dbKey];
                entityMapProvider.Build(database);
                database.UseEntityMapProvider(entityMapProvider);
                this.allEntityMapProviders.Add(entityMapProvider);
            }
        }
        if (!this.globalEntityMapProviders.IsEmpty)
        {
            foreach (var ormProviderType in this.globalEntityMapProviders.Keys)
            {
                var entityMapProvider = this.globalEntityMapProviders[ormProviderType];
                foreach (var database in this.Databases)
                {
                    if (this.entityMapProviders.ContainsKey(database.DbKey)
                        || database.OrmProviderType != ormProviderType)
                        continue;
                    //确保每个实体都映射到，如果映射过了，不再映射
                    //有时候一个数据库并不能映射完所有实体，有的实体在其他数据库中使用，所以需要遍历所有数据库映射
                    entityMapProvider.Build(database);
                    database.UseEntityMapProvider(entityMapProvider);
                }
                this.allEntityMapProviders.Add(entityMapProvider);
            }
        }
        if (!this.tableShardingProviders.IsEmpty)
        {
            foreach (var dbKey in this.tableShardingProviders.Keys)
            {
                var database = this.GetDatabase(dbKey);
                var tableShardingProvider = this.tableShardingProviders[dbKey];
                database.UseTableShardingProvider(tableShardingProvider);
                this.allTableShardingProviders.Add(tableShardingProvider);
            }
        }
        if (!this.globalTableShardingProviders.IsEmpty)
        {
            foreach (var ormProviderType in this.globalTableShardingProviders.Keys)
            {
                var tableShardingProvider = this.globalTableShardingProviders[ormProviderType];
                foreach (var database in this.databases.Values)
                {
                    if (this.tableShardingProviders.ContainsKey(database.DbKey)
                        || database.OrmProviderType != ormProviderType)
                        continue;
                    database.UseTableShardingProvider(tableShardingProvider);
                }
                this.allTableShardingProviders.Add(tableShardingProvider);
            }
        }
        //遍历所有数据库，未设置连接串选择器的，默认设置轮询方式选择连接串
        foreach (var database in this.Databases)
            database.Build();
        this.allOrmProviders = this.ormProviders.Values.ToList();
        this.allDatabases = this.databases.Values.ToList();
    }
}