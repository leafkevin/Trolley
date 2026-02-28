using System;
using System.Collections.Generic;

namespace Trolley;

public interface IOrmDbFactory
{
    #region DbFactory运行时属性
    /// <summary>
    /// dbKey数据库实例选择器委托
    /// </summary>
    Delegate DbKeySelector { get; }
    /// <summary>
    /// 所有注册的数据库实例
    /// </summary>
    List<TheaDatabase> Databases { get; }
    /// <summary>
    /// 所有注册的ORM提供器实例
    /// </summary>
    List<IOrmProvider> OrmProviders { get; }
    /// <summary>
    /// 所有注册的实体映射提供器实例
    /// </summary>
    List<IEntityMapProvider> EntityMapProviders { get; }
    /// <summary>
    /// 所有注册的分表提供器实例
    /// </summary>
    List<ITableShardingProvider> TableShardingProviders { get; }
    /// <summary>
    /// 拦截器，默认为null
    /// </summary>
    DbInterceptors DbInterceptors { get; }
    /// <summary>
    /// 默认全局配置
    /// </summary>
    OrmDbFactoryOptions Options { get; }
    #endregion

    void Register(TheaDatabase database);
    TheaDatabase GetDatabase(string dbKey);
    void UseDbKeySelector(Delegate dbKeySelector);

    void UseOrmProvider(IOrmProvider ormProvider);
    bool TryGetOrmProvider(OrmProviderType ormProviderType, out IOrmProvider ormProvider);

    void UseEntityMapProvider(string dbKey, IEntityMapProvider entityMapProvider);
    void UseEntityMapProvider(OrmProviderType ormProviderType, IEntityMapProvider entityMapProvider);
    bool TryGetEntityMapProvider(string dbKey, out IEntityMapProvider entityMapProvider);
    bool TryGetEntityMapProvider(OrmProviderType ormProviderType, out IEntityMapProvider entityMapProvider);

    void UseTableShardingProvider(string dbKey, ITableShardingProvider tableShardingProvider);
    void UseTableShardingProvider(OrmProviderType ormProviderType, ITableShardingProvider tableShardingProvider);
    bool TryGetTableShardingProvider(string dbKey, out ITableShardingProvider tableShardingProvider);
    bool TryGetTableShardingProvider(OrmProviderType ormProviderType, out ITableShardingProvider tableShardingProvider);

    void AddTypeHandler(ITypeHandler typeHandler);

    IRepository Create(string dbKey = null);
    void Build();
}
