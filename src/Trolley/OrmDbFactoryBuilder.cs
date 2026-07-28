using System;

namespace Trolley;

public sealed class OrmDbFactoryBuilder
{
    private readonly IOrmDbFactory dbFactory = OrmDbFactory.Instance;

    public OrmDbFactoryBuilder Register(OrmProviderType ormProviderType, string dbKey, Action<OrmDatabaseBuilder> databaseInitializer, bool isDefaultDatabase = false)
    {
        if (databaseInitializer == null)
            throw new ArgumentNullException(nameof(databaseInitializer));
        if (!this.dbFactory.TryGetOrmProvider(ormProviderType, out var ormProvider))
        {
            var type = this.GetOrmProviderType(ormProviderType);
            ormProvider = RepositoryHelper.CreateInstance(type) as IOrmProvider;
            this.dbFactory.UseOrmProvider(ormProvider);
        }
        var database = new TheaDatabase
        {
            DbKey = dbKey,
            OrmProviderType = ormProviderType,
            OrmProvider = ormProvider,
            IsDefault = isDefaultDatabase
        };
        var builder = new OrmDatabaseBuilder(this.dbFactory, database);
        databaseInitializer.Invoke(builder);
        this.dbFactory.Register(database);
        return this;
    }
    public OrmDbFactoryBuilder UseDbKeySelector(Delegate dbKeySelector)
    {
        if (dbKeySelector == null)
            throw new ArgumentNullException(nameof(dbKeySelector));

        this.dbFactory.UseDbKeySelector(dbKeySelector);
        return this;
    }

    public OrmDbFactoryBuilder UseMapping(string dbKey, IModelMappingConfiguration configuration)
    {
        if (string.IsNullOrEmpty(dbKey))
            throw new ArgumentNullException(nameof(dbKey));
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        if (!this.dbFactory.TryGetEntityMapProvider(dbKey, out var entityMapProvider))
            this.dbFactory.UseEntityMapProvider(dbKey, entityMapProvider = new EntityMapProvider());
        entityMapProvider.IsCanMapTo = configuration.IsCanMapTo;
        configuration.Configure(new ModelBuilder(entityMapProvider));
        return this;
    }
    public OrmDbFactoryBuilder UseMapping(OrmProviderType ormProviderType, IModelMappingConfiguration configuration)
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        if (!this.dbFactory.TryGetEntityMapProvider(ormProviderType, out var entityMapProvider))
            this.dbFactory.UseEntityMapProvider(ormProviderType, entityMapProvider = new EntityMapProvider());
        entityMapProvider.IsCanMapTo = configuration.IsCanMapTo;
        configuration.Configure(new ModelBuilder(entityMapProvider));
        return this;
    }

    public OrmDbFactoryBuilder UseTableSharding(string dbKey, Action<TableShardingBuilder> shardingInitializer)
    {
        if (string.IsNullOrEmpty(dbKey))
            throw new ArgumentNullException(nameof(dbKey));
        if (shardingInitializer == null)
            throw new ArgumentNullException(nameof(shardingInitializer));

        if (!this.dbFactory.TryGetTableShardingProvider(dbKey, out var tableShardingProvider))
            this.dbFactory.UseTableShardingProvider(dbKey, tableShardingProvider = new TableShardingProvider());
        shardingInitializer.Invoke(new TableShardingBuilder(tableShardingProvider));
        return this;
    }
    public OrmDbFactoryBuilder UseTableSharding(string dbKey, ITableShardingConfiguration configuration)
    {
        if (string.IsNullOrEmpty(dbKey))
            throw new ArgumentNullException(nameof(dbKey));
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        return this.UseTableSharding(dbKey, configuration.Configure);
    }
    public OrmDbFactoryBuilder UseTableSharding(OrmProviderType ormProviderType, Action<TableShardingBuilder> shardingInitializer)
    {
        if (shardingInitializer == null)
            throw new ArgumentNullException(nameof(shardingInitializer));

        if (!this.dbFactory.TryGetTableShardingProvider(ormProviderType, out var tableShardingProvider))
            this.dbFactory.UseTableShardingProvider(ormProviderType, tableShardingProvider = new TableShardingProvider());
        shardingInitializer.Invoke(new TableShardingBuilder(tableShardingProvider));
        return this;
    }
    public OrmDbFactoryBuilder UseTableSharding(OrmProviderType ormProviderType, ITableShardingConfiguration configuration)
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        return this.UseTableSharding(ormProviderType, configuration.Configure);
    }

    public OrmDbFactoryBuilder UseInterceptor(IDbInterceptor interceptor)
    {
        this.dbFactory.UseInterceptor(interceptor);
        return this;
    }
    public OrmDbFactoryBuilder UseTypeHandler(ITypeHandler typeHandler)
    {
        if (typeHandler == null)
            throw new ArgumentNullException(nameof(typeHandler));

        this.dbFactory.AddTypeHandler(typeHandler);
        return this;
    }
    public OrmDbFactoryBuilder WithOptions(Action<OrmDbFactoryOptions> optionsInitializer)
    {
        optionsInitializer.Invoke(this.dbFactory.Options);
        return this;
    }

    public IOrmDbFactory Build()
    {
        this.dbFactory.Build();
        return this.dbFactory;
    }
    private Type GetOrmProviderType(OrmProviderType ormProviderType)
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
}
public sealed class OrmDatabaseBuilder
{
    private readonly IOrmDbFactory dbFactory;
    private readonly TheaDatabase database;
    private readonly string dbKey;
    public OrmDatabaseBuilder(IOrmDbFactory dbFactory, TheaDatabase database)
    {
        this.dbFactory = dbFactory;
        this.database = database;
        this.dbKey = database.DbKey;
    }
    /// <summary>
    /// 设置1个或多个主库连接串，多个主库时，使用轮询方式选择连接串，适用于多分库场景
    /// </summary>
    /// <param name="connectionStrings"></param>
    /// <returns></returns>
    public OrmDatabaseBuilder Use(params string[] connectionStrings)
    {
        this.database.Use(connectionStrings);
        return this;
    }
    /// <summary>
    /// 设置1个或多个主库连接串，同时设置连接串选择器，适用于多分库、多租户、多租户分库场景
    /// </summary>
    /// <param name="connectionStrings"></param>
    /// <param name="connectionStringSelector"></param>
    /// <returns></returns>
    public OrmDatabaseBuilder Use(string[] connectionStrings, Func<object[], string> connectionStringSelector)
    {
        this.database.Use(connectionStrings, connectionStringSelector);
        return this;
    }
    /// <summary>
    /// 设置1个或多个从库连接串，多个从库时，使用轮询方式选择连接串，适用于多从库场景
    /// </summary>
    /// <param name="connectionStrings"></param>
    /// <returns></returns>
    public OrmDatabaseBuilder UseSlave(params string[] connectionStrings)
    {
        this.database.UseSlave(connectionStrings);
        return this;
    }
    /// <summary>
    /// 设置1个或多个从库连接串，同时设置连接串选择器，适用于多从库、多租户多从库分库场景
    /// </summary>
    /// <param name="connectionStrings"></param>
    /// <param name="connectionStringSelector"></param>
    /// <returns></returns>
    public OrmDatabaseBuilder UseSlave(string[] connectionStrings, Func<object[], string> connectionStringSelector)
    {
        this.database.Use(connectionStrings, connectionStringSelector);
        return this;
    }

    public OrmDatabaseBuilder UseMapping(IModelMappingConfiguration configuration)
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));
        if (!this.dbFactory.TryGetEntityMapProvider(this.dbKey, out var entityMapProvider))
            this.dbFactory.UseEntityMapProvider(this.dbKey, entityMapProvider = new EntityMapProvider());
        configuration.Configure(new ModelBuilder(entityMapProvider));
        this.database.UseEntityMapProvider(entityMapProvider);
        return this;
    }

    public OrmDatabaseBuilder UseTableSharding(ITableShardingConfiguration configuration)
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));
        return this.UseTableSharding(configuration.Configure);
    }
    public OrmDatabaseBuilder UseTableSharding(Action<TableShardingBuilder> shardingInitializer)
    {
        if (shardingInitializer == null)
            throw new ArgumentNullException(nameof(shardingInitializer));

        if (!this.dbFactory.TryGetTableShardingProvider(this.dbKey, out var tableShardingProvider))
            this.dbFactory.UseTableShardingProvider(this.dbKey, tableShardingProvider = new TableShardingProvider());
        shardingInitializer.Invoke(new TableShardingBuilder(tableShardingProvider));
        this.database.UseTableShardingProvider(tableShardingProvider);
        return this;
    }
}