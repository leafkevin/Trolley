using System;
using System.Collections.Generic;

namespace Trolley;

public interface IOrmDbFactory
{
    ICollection<TheaDatabase> Databases { get; }
    ICollection<IOrmProvider> OrmProviders { get; }
    OrmDbFactoryOptions Options { get; }

    void UseDefaultDatabase(string dbKey);
    void UseDatabaseSharding(Func<string> dbKeySelector);
    bool TryGetTableShardingProvider(string dbKey, out ITableShardingProvider tableShardingProvider);
    void AddTableShardingProvider(string dbKey, ITableShardingProvider tableShardingProvider);
    bool TryGetTableShardingProvider(OrmProviderType ormProviderType, out ITableShardingProvider tableShardingProvider);
    void AddTableShardingProvider(OrmProviderType ormProviderType, ITableShardingProvider tableShardingProvider);
    bool TryGetTableSharding(string dbKey, Type entityType, out TableShardingInfo tableShardingInfo);
    bool TryGetTableSharding(OrmProviderType ormProviderType, Type entityType, out TableShardingInfo tableShardingInfo);

    TheaDatabase Register(OrmProviderType ormProviderType, string dbKey, string connectionString);
    void Register(OrmProviderType ormProviderType, string dbKey, string connectionString, bool isDefaultDatabase);
    void AddOrmProvider(IOrmProvider ormProvider);
    bool TryGetOrmProvider(OrmProviderType ormProviderType, out IOrmProvider ormProvider);
    void AddMapProvider(string dbKey, IEntityMapProvider mapProvider);
    bool TryGetMapProvider(string dbKey, out IEntityMapProvider mapProvider);
    void AddMapProvider(OrmProviderType ormProviderType, IEntityMapProvider mapProvider);
    bool TryGetMapProvider(OrmProviderType ormProviderType, out IEntityMapProvider mapProvider);
    TheaDatabase GetDatabase(string dbKey = null);
    /// <summary>
    /// 使用指定的dbKey，创建仓储对象。
    /// 如果没有指定dbKey，有指定分库规则，会调用分库规则获取dbKey
    /// 如果也没有指定分库规则，就使用默认的dbKey
    /// 如果默认dbKey也没有指定，就会抛出异常，需要配置dbKey
    /// </summary>
    /// <param name="dbKey">指定的dbKey</param>
    /// <returns></returns>
    IRepository CreateRepository(string dbKey = null);
    void With(Action<OrmDbFactoryOptions> optionsInitializer);
    void Build();
}
