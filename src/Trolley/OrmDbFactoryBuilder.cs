using System;

namespace Trolley;

public sealed class OrmDbFactoryBuilder
{
    private IOrmDbFactory dbFactory = new OrmDbFactory();

    public OrmDbFactoryBuilder Register(OrmProviderType ormProviderType, string dbKey, string connectionString, bool isDefault, string defaultTableSchema = null)
    {
        var type = this.GetOrmProviderType(ormProviderType);
        this.dbFactory.Register(dbKey, connectionString, type, isDefault, defaultTableSchema);
        return this;
    }
    public OrmDbFactoryBuilder Register(Type ormProviderType, string dbKey, string connectionString, bool isDefault, string defaultTableSchema = null)
    {
        this.dbFactory.Register(dbKey, connectionString, ormProviderType, isDefault, defaultTableSchema);
        return this;
    }
    public OrmDbFactoryBuilder Register<TOrmProvider>(string dbKey, string connectionString, bool isDefault, string defaultTableSchema = null) where TOrmProvider : class, IOrmProvider, new()
    {
        var ormProviderType = typeof(TOrmProvider);
        this.dbFactory.Register(dbKey, connectionString, ormProviderType, isDefault, defaultTableSchema);
        return this;
    }
    public OrmDbFactoryBuilder Configure(OrmProviderType ormProviderType, IModelConfiguration configuration)
    {
        var type = this.GetOrmProviderType(ormProviderType);
        return this.Configure(type, configuration);
    }
    public OrmDbFactoryBuilder Configure<TModelConfiguration>(OrmProviderType ormProviderType) where TModelConfiguration : class, IModelConfiguration, new()
    {
        var type = this.GetOrmProviderType(ormProviderType);
        return this.Configure(type, new TModelConfiguration());
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

        var builder = new ShardingBuilder(this.dbFactory);
        shardingInitializer.Invoke(builder);
        return this;
    }
    public OrmDbFactoryBuilder With(Action<OrmDbFactoryOptions> optionsInitializer)
    {
        this.dbFactory.With(optionsInitializer);
        return this;
    }
    public OrmDbFactoryBuilder UseDbFilter(Action<DbFilters> filterInitializer)
    {
        if (filterInitializer == null)
            throw new ArgumentNullException(nameof(filterInitializer));

        filterInitializer.Invoke(this.dbFactory.DbFilters);
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
        var packageName = fileName.Replace(".dll", "");
        if (type == null)
            throw new DllNotFoundException($"没有找到[{fileName}]文件，或是没有引入[{packageName}]nuget包");
        return type;
    }
}