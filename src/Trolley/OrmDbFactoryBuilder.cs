using System;

namespace Trolley;

public sealed class OrmDbFactoryBuilder
{
    private readonly IOrmDbFactory dbFactory = new OrmDbFactory();

    public OrmDbFactoryBuilder Register(OrmProviderType ormProviderType, string dbKey, Action<OrmDatabaseBuilder> databaseInitializer, bool isDefaultDatabase = false)
    {
        if (databaseInitializer == null)
            throw new ArgumentNullException(nameof(databaseInitializer));
        var database = this.dbFactory.Register(ormProviderType, dbKey, isDefaultDatabase);
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

    /// <summary>
    /// 设置dbKey的数据库连接串选择器，通常是数据一致的多个数据库，非主从库模式，也非类似租户等水平分库模式，通常设置为轮询方式或者是随机方式选择连接串
    /// </summary>
    /// <param name="dbKey"></param>
    /// <param name="connectionStringSelector"></param>
    /// <returns></returns>
    public OrmDbFactoryBuilder UseConnectionStringSelector(string dbKey, Func<string> connectionStringSelector)
    {
        this.dbFactory.AddConnectionStringSelector(dbKey, connectionStringSelector);
        return this;
    }
    /// <summary>
    /// 设置所有类型为ormProviderType的数据库的连接串选择器，此方法适用于数据一致的多个数据库，非主从库模式，也非类似租户等水平分库模式，通常设置为轮询方式或者是随机方式选择连接串
    /// </summary>
    /// <param name="ormProviderType"></param>
    /// <param name="connectionStringSelector"></param>
    /// <returns></returns>
    public OrmDbFactoryBuilder UseConnectionStringSelector(OrmProviderType ormProviderType, Func<string> connectionStringSelector)
    {
        this.dbFactory.AddConnectionStringSelector(ormProviderType, connectionStringSelector);
        return this;
    }
    /// <summary>
    /// 设置dbKey的数据库主库连接串选择器，此方法适用于主从库模式，并且主库有多个，也可以类似多租户等水平分库模式，也可以是多主库且又类似多租户等水平分库模式则默认使用轮询方式选择连接串
    /// </summary>
    /// <param name="dbKey"></param>
    /// <param name="connectionStringSelector"></param>
    /// <returns></returns>
    public OrmDbFactoryBuilder UseMasterConnectionStringSelector(string dbKey, Func<object, string> connectionStringSelector)
    {
        this.dbFactory.AddConnectionStringSelector(dbKey, connectionStringSelector);
        return this;
    }
    /// <summary>
    /// 设置所有类型为ormProviderType的数据库主库连接串选择器，此方法适用于主从库模式，并且主库有多个时，未设置连接串选择器，则默认使用轮询方式选择连接串
    /// </summary>
    /// <param name="ormProviderType"></param>
    /// <param name="connectionStringSelector"></param>
    /// <returns></returns>
    public OrmDbFactoryBuilder UseMasterConnectionStringSelector(OrmProviderType ormProviderType, Func<object, string> connectionStringSelector)
    {
        this.dbFactory.AddConnectionStringSelector(ormProviderType, connectionStringSelector);
        return this;
    }
    public OrmDbFactoryBuilder UseSlaveConnectionStringSelector(string dbKey, Func<object, string> connectionStringSelector)
    {
        this.dbFactory.AddConnectionStringSelector(dbKey, connectionStringSelector);
        return this;
    }
    public OrmDbFactoryBuilder UseSlaveConnectionStringSelector(OrmProviderType ormProviderType, Func<object, string> connectionStringSelector)
    {
        this.dbFactory.AddConnectionStringSelector(ormProviderType, connectionStringSelector);
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
        this.dbFactory.Options.FieldMapHandler = fieldMapHandler;
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

        filterInitializer.Invoke(this.dbFactory.Options.DbInterceptors);
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
    /// <summary>
    /// 设置单库连接串，不使用主从库模式，直接使用此数据库进行所有操作
    /// </summary>
    /// <param name="connectionString"></param>
    /// <returns></returns>
    public OrmDatabaseBuilder UseConnectionString(string connectionString)
    {
        database.MasterConnectionStrings ??= new();
        database.MasterConnectionStrings.Add(connectionString);
        return this;
    }
    /// <summary>
    /// 设置主库连接串，可多个主库连接串，多个主库时，未设置连接串选择器，则默认使用轮询方式选择连接串
    /// </summary>
    /// <param name="connectionStrings"></param>
    /// <returns></returns>
    public OrmDatabaseBuilder UseMaster(params string[] connectionStrings)
    {
        database.MasterConnectionStrings ??= new();
        database.MasterConnectionStrings.AddRange(connectionStrings);
        return this;
    }
    /// <summary>
    /// 设置主库连接串同时设置连接串选择器，多个主库时使用connectionStringSelector委托选择连接串，委托connectionStringSelector也可作为主库的分库规则，如：主库有2个数据库，分别为tenant1_master和tenant2_master，保存不同租户的数据，可使用此委托可获取不同租户的数据，进行写操作
    /// </summary>
    /// <param name="connectionStrings"></param>
    /// <param name="connectionStringSelector"></param>
    /// <returns></returns>
    public OrmDatabaseBuilder UseMaster(string[] connectionStrings, Func<string> connectionStringSelector)
    {
        database.MasterConnectionStrings ??= new();
        database.MasterConnectionStrings.AddRange(connectionStrings);
        if (connectionStringSelector == null)
            throw new ArgumentNullException(nameof(connectionStringSelector));
        database.UseMasterSelector(connectionStringSelector);
        return this;
    }
    /// <summary>
    /// 设置主库连接串同时设置连接串选择器，多个主库时通过指定参数的connectionStringSelector委托选择连接串，这个参数在调用CreateRepository方法时指定，使用此场景通常是多主库并且多租户，如：主库有4个数据库，分别为tenant1_master1、tenant1_master2、other_tenant_master1、other_tenant_master2，调用CreateRepository("default", tenant1)时，将选择tenant1中的某一个主库，调用CreateRepository("default", tenant2)时，将选择other_tenant中的某一个主库，进行写操作
    /// </summary>
    /// <param name="connectionStrings"></param>
    /// <param name="connectionStringSelector"></param>
    /// <returns></returns>
    public OrmDatabaseBuilder UseMaster(string[] connectionStrings, Func<object, string> connectionStringSelector)
    {
        database.MasterConnectionStrings ??= new();
        database.MasterConnectionStrings.AddRange(connectionStrings);
        if (connectionStringSelector == null)
            throw new ArgumentNullException(nameof(connectionStringSelector));
        database.UseMasterSelector(connectionStringSelector);
        return this;
    }
    public OrmDatabaseBuilder UseSlave(params string[] connectionStrings)
    {
        database.SlaveConnectionStrings ??= new();
        database.SlaveConnectionStrings.AddRange(connectionStrings);
        return this;
    }
    public OrmDatabaseBuilder UseSlave(string[] connectionStrings, Func<string> connectionStringSelector)
    {
        database.SlaveConnectionStrings ??= new();
        database.SlaveConnectionStrings.AddRange(connectionStrings);
        if (connectionStringSelector == null)
            throw new ArgumentNullException(nameof(connectionStringSelector));
        database.UseSlaveSelector(connectionStringSelector);
        return this;
    }
    public OrmDatabaseBuilder UseSlave(string[] connectionStrings, Func<object, string> connectionStringSelector)
    {
        database.SlaveConnectionStrings ??= new();
        database.SlaveConnectionStrings.AddRange(connectionStrings);
        if (connectionStringSelector == null)
            throw new ArgumentNullException(nameof(connectionStringSelector));
        database.UseSlaveSelector(connectionStringSelector);
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