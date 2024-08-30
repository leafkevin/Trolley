using System;

namespace Trolley;

public sealed class OrmDbFactoryBuilder
{
    private readonly IOrmDbFactory dbFactory = new OrmDbFactory();

    public OrmDbFactoryBuilder Register(OrmProviderType ormProviderType, string dbKey, string connectionString, bool isDefault = false)
    {
        this.dbFactory.Register(ormProviderType, dbKey, connectionString, isDefault);
        return this;
    }
    public OrmDbFactoryBuilder Register(OrmProviderType ormProviderType, string dbKey, string connectionString, Action<OrmDatabaseBuilder> databaseInitializer)
    {
        if (databaseInitializer == null)
            throw new ArgumentNullException(nameof(databaseInitializer));
        var database = this.dbFactory.Register(ormProviderType, dbKey, connectionString);
        var builder = new OrmDatabaseBuilder(this.dbFactory, database);
        databaseInitializer.Invoke(builder);
        return this;
    }
    public OrmDbFactoryBuilder Configure(string dbKey, IModelConfiguration configuration)
    {
        this.dbFactory.Configure(dbKey, configuration);
        return this;
    }
    public OrmDbFactoryBuilder Configure(OrmProviderType ormProviderType, IModelConfiguration configuration)
    {
        this.dbFactory.Configure(ormProviderType, configuration);
        return this;
    }
    public OrmDbFactoryBuilder UseDatabaseSharding(Func<string> dbKeySelector)
    {
        this.dbFactory.UseDatabaseSharding(dbKeySelector);
        return this;
    }
    public OrmDbFactoryBuilder UseTableSharding(string dbKey, Action<TableShardingBuilder> shardingInitializer)
    {
        if (shardingInitializer == null)
            throw new ArgumentNullException(nameof(shardingInitializer));

        if (!this.dbFactory.TryGetTableShardingProvider(dbKey, out var tableShardingProvider))
            this.dbFactory.AddTableShardingProvider(dbKey, tableShardingProvider = new TableShardingProvider());

        var builder = new TableShardingBuilder(tableShardingProvider);
        shardingInitializer.Invoke(builder);
        return this;
    }
    public OrmDbFactoryBuilder UseTableSharding(string dbKey, ITableShardingConfiguration configuration)
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        if (!this.dbFactory.TryGetTableShardingProvider(dbKey, out var tableShardingProvider))
            this.dbFactory.AddTableShardingProvider(dbKey, tableShardingProvider = new TableShardingProvider());
        var builder = new TableShardingBuilder(tableShardingProvider);
        configuration.OnModelCreating(builder);
        return this;
    }
    public OrmDbFactoryBuilder UseTableSharding(OrmProviderType ormProviderType, Action<TableShardingBuilder> shardingInitializer)
    {
        if (shardingInitializer == null)
            throw new ArgumentNullException(nameof(shardingInitializer));

        if (!this.dbFactory.TryGetTableShardingProvider(ormProviderType, out var tableShardingProvider))
            this.dbFactory.AddTableShardingProvider(ormProviderType, tableShardingProvider = new TableShardingProvider());

        var builder = new TableShardingBuilder(tableShardingProvider);
        shardingInitializer.Invoke(builder);
        return this;
    }
    public OrmDbFactoryBuilder UseTableSharding(OrmProviderType ormProviderType, ITableShardingConfiguration configuration)
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        if (!this.dbFactory.TryGetTableShardingProvider(ormProviderType, out var tableShardingProvider))
            this.dbFactory.AddTableShardingProvider(ormProviderType, tableShardingProvider = new TableShardingProvider());

        var builder = new TableShardingBuilder(tableShardingProvider);
        configuration.OnModelCreating(builder);
        return this;
    }
    public OrmDbFactoryBuilder UseFieldMapHandler(IFieldMapHandler fieldMapHandler)
    {
        this.dbFactory.UseFieldMapHandler(fieldMapHandler);
        return this;
    }
    public OrmDbFactoryBuilder With(Action<OrmDbFactoryOptions> optionsInitializer)
    {
        this.dbFactory.With(optionsInitializer);
        return this;
    }
    public OrmDbFactoryBuilder UseInterceptors(Action<DbInterceptors> filterInitializer)
    {
        if (filterInitializer == null)
            throw new ArgumentNullException(nameof(filterInitializer));

        filterInitializer.Invoke(this.dbFactory.Interceptors);
        return this;
    }
    public IOrmDbFactory Build()
    {
        this.dbFactory.Build();
        return this.dbFactory;
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
    public OrmDatabaseBuilder UseSlave(params string[] connectionStrings)
    {
        database.SlaveConnectionStrings ??= new();
        database.SlaveConnectionStrings.AddRange(connectionStrings);
        return this;
    }
    public OrmDatabaseBuilder Configure<TModelConfiguration>() where TModelConfiguration : class, IModelConfiguration, new()
    {
        this.dbFactory.Configure(this.dbKey, new TModelConfiguration());
        return this;
    }
    public OrmDatabaseBuilder UseTableSharding(Action<TableShardingBuilder> shardingInitializer)
    {
        if (shardingInitializer == null)
            throw new ArgumentNullException(nameof(shardingInitializer));

        if (!this.dbFactory.TryGetTableShardingProvider(this.dbKey, out var tableShardingProvider))
            this.dbFactory.AddTableShardingProvider(this.dbKey, tableShardingProvider = new TableShardingProvider());

        var builder = new TableShardingBuilder(tableShardingProvider);
        shardingInitializer.Invoke(builder);
        return this;
    }
    public OrmDatabaseBuilder AsDefaultDatabase()
    {
        this.dbFactory.UseDefaultDatabase(this.dbKey);
        return this;
    }
}