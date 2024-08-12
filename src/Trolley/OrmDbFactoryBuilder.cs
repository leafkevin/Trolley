using System;

namespace Trolley;

public sealed class OrmDbFactoryBuilder
{
    private IOrmDbFactory dbFactory = new OrmDbFactory();

    public OrmDbFactoryBuilder Register(OrmProviderType ormProviderType, string dbKey, string connectionString, bool isDefault, string defaultTableSchema = null)
    {
        this.dbFactory.Register(ormProviderType, dbKey, connectionString, isDefault, defaultTableSchema);
        return this;
    }
    public OrmDbFactoryBuilder Configure(OrmProviderType ormProviderType, IModelConfiguration configuration)
    {
        this.dbFactory.Configure(ormProviderType, configuration);
        return this;
    }
    public OrmDbFactoryBuilder Configure<TModelConfiguration>(OrmProviderType ormProviderType) where TModelConfiguration : class, IModelConfiguration, new()
        => this.Configure(ormProviderType, new TModelConfiguration());
    public OrmDbFactoryBuilder UseSharding(Action<ShardingBuilder> shardingInitializer)
    {
        if (shardingInitializer == null)
            throw new ArgumentNullException(nameof(shardingInitializer));

        var builder = new ShardingBuilder(this.dbFactory);
        shardingInitializer.Invoke(builder);
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