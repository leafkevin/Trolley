using System;
using System.Collections.Generic;

namespace Trolley;

public interface IOrmDbFactory
{
    ICollection<TheaDatabase> Databases { get; }
    ICollection<IOrmProvider> OrmProviders { get; }
    ICollection<IEntityMapProvider> MapProviders { get; }
    DbInterceptors Interceptors { get; }

    void UseDatabase(Func<string> dbKeySelector);
    bool TryGetShardingTable(Type entityType, out ShardingTable shardingTable);
    void AddShardingTable(Type entityType, ShardingTable shardingTable);

    void Register(OrmProviderType ormProviderType, string dbKey, string connectionString, bool isDefault, string defaultTableSchema = null);
    void AddOrmProvider(IOrmProvider ormProvider);
    bool TryGetOrmProvider(OrmProviderType ormProviderType, out IOrmProvider ormProvider);
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
