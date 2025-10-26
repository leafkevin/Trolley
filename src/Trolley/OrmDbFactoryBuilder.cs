using System;
using System.Net.Http.Headers;

namespace Trolley;

public sealed class OrmDbFactoryBuilder
{
    private readonly IOrmDbFactory dbFactory = new OrmDbFactory();

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
        var builder = new OrmDatabaseBuilder(this.dbFactory, new TheaDatabase
        {
            DbKey = dbKey,
            OrmProviderType = ormProviderType,
            OrmProvider = ormProvider,
            IsDefault = isDefaultDatabase
        });
        databaseInitializer.Invoke(builder);
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

    public OrmDbFactoryBuilder UseInterceptors(Action<DbInterceptors> filterInitializer)
    {
        if (filterInitializer == null)
            throw new ArgumentNullException(nameof(filterInitializer));

        filterInitializer.Invoke(this.dbFactory.DbInterceptors);
        return this;
    }
    public OrmDbFactoryBuilder UseTypeHandler(ITypeHandler typeHandler)
    {
        if (typeHandler == null)
            throw new ArgumentNullException(nameof(typeHandler));

        this.dbFactory.UseTypeHandler(typeHandler);
        return this;
    }
    public OrmDbFactoryBuilder WithOptions(Action<OrmDbFactoryOptions> optionsInitializer)
    {
        var options = new OrmDbFactoryOptions
        {
            CommandTimeout = this.dbFactory.CommandTimeout,
            UserParameterPrefix = this.dbFactory.UserParameterPrefix,
            IsConstantParameterized = this.dbFactory.IsConstantParameterized,
            DefaultEnumMapDbType = this.dbFactory.DefaultEnumMapDbType,
            DefaultDateTimeKind = this.dbFactory.DefaultDateTimeKind
        };
        optionsInitializer.Invoke(options);
        this.dbFactory.CommandTimeout = options.CommandTimeout;
        this.dbFactory.UserParameterPrefix = options.UserParameterPrefix;
        this.dbFactory.IsConstantParameterized = options.IsConstantParameterized;
        this.dbFactory.DefaultEnumMapDbType = options.DefaultEnumMapDbType;
        this.dbFactory.DefaultDateTimeKind = options.DefaultDateTimeKind;
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
    /// 设置主库连接串，可1个或多个主库连接串，多个主库时，未设置连接串选择器，则默认使用轮询方式选择连接串
    /// </summary>
    /// <param name="connectionStrings"></param>
    /// <returns></returns>
    public OrmDatabaseBuilder Use(params string[] connectionStrings)
    {
        this.database.Use(connectionStrings);
        return this;
    }
    public OrmDatabaseBuilder Use(string[] connectionStrings, Func<object[], string> connectionStringSelector)
    {
        this.database.Use(connectionStrings, connectionStringSelector);
        return this;
    }
    public OrmDatabaseBuilder UseSlave(params string[] connectionStrings)
    {
        this.database.UseSlave(connectionStrings);
        return this;
    }
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
    public OrmDatabaseBuilder UseMapping(Action<ModelBuilder> mappingInitializer)
    {
        if (mappingInitializer == null)
            throw new ArgumentNullException(nameof(mappingInitializer));

        if (!this.dbFactory.TryGetEntityMapProvider(this.dbKey, out var entityMapProvider))
            this.dbFactory.UseEntityMapProvider(this.dbKey, entityMapProvider = new EntityMapProvider());
        mappingInitializer.Invoke(new ModelBuilder(entityMapProvider));
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