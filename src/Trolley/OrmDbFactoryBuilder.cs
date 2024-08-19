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

    public OrmDbFactoryBuilder Configure(string dbKey, IModelConfiguration configuration)
    {
        this.dbFactory.Configure(dbKey, configuration);
        return this;
    }
    public OrmDbFactoryBuilder Configure<TModelConfiguration>(string dbKey) where TModelConfiguration : class, IModelConfiguration, new()
        => this.Configure(dbKey, new TModelConfiguration());
    public OrmDbFactoryBuilder Configure(OrmProviderType ormProviderType, IModelConfiguration configuration)
    {
        this.dbFactory.Configure(ormProviderType, configuration);
        return this;
    }
    public OrmDbFactoryBuilder Configure<TModelConfiguration>(OrmProviderType ormProviderType) where TModelConfiguration : class, IModelConfiguration, new()
        => this.Configure(ormProviderType, new TModelConfiguration());

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