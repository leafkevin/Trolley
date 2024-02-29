using System;

namespace Trolley;

public class OrmDbFactoryBuilder
{
    private OrmDbFactory dbFactory = new();

    public OrmDbFactoryBuilder Register<TOrmProvider>(string dbKey, string connectionString, bool isDefault) where TOrmProvider : class, IOrmProvider, new()
    {
        this.dbFactory.Register<TOrmProvider>(dbKey, connectionString, isDefault);
        return this;
    }
    public OrmDbFactoryBuilder Configure(Type ormProviderType, IModelConfiguration configuration)
    {
        if (ormProviderType == null)
            throw new ArgumentNullException(nameof(ormProviderType));

        if (!this.dbFactory.TryGetMapProvider(ormProviderType, out _))
        {
            var mapProvider = new EntityMapProvider() { OrmProviderType = ormProviderType };
            configuration.OnModelCreating(new ModelBuilder(mapProvider));
            this.dbFactory.AddMapProvider(ormProviderType, mapProvider);
        }
        return this;
    }
    public OrmDbFactoryBuilder Configure<TOrmProvider>(IModelConfiguration configuration)
    {
        this.Configure(typeof(TOrmProvider), configuration);
        return this;
    }
    public OrmDbFactoryBuilder Configure<TOrmProvider, TModelConfiguration>() where TModelConfiguration : class, IModelConfiguration, new()
    {
        this.Configure(typeof(TOrmProvider), new TModelConfiguration());
        return this;
    }
    public OrmDbFactoryBuilder UseSharding(Action<ShardingBuilder> shardingInitializer)
    {
        if (shardingInitializer == null)
            throw new ArgumentNullException(nameof(shardingInitializer));

        //TODO:
        return this;
    }
    public OrmDbFactoryBuilder With(Action<OrmDbFactoryOptions> optionsInitializer)
    {
        this.dbFactory.With(optionsInitializer);
        return this;
    }
    public IOrmDbFactory Build() => this.dbFactory.Build();
}