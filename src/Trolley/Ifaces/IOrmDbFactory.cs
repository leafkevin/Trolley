using System;
using System.Collections.Generic;

namespace Trolley;

public interface IOrmDbFactory
{
    /// <summary>
    /// 所有注册的数据库实例
    /// </summary>
    ICollection<TheaDatabase> Databases { get; }
    /// <summary>
    /// 所有注册的ORM提供器实例
    /// </summary>
    ICollection<IOrmProvider> OrmProviders { get; }

    /// <summary>
    /// 获取或设置命令超时时间，单位是秒，默认是30秒
    /// </summary>
    int CommandTimeout { get; set; }
    /// <summary>
    /// 表达式中使用变量默认的参数名前缀，默认值是p，如：@p1,@p2等
    /// </summary>
    string UserParameterPrefix { get; set; }
    /// <summary>
    /// 表达式解析中，常量是否参数化。如果设置为true，所有常量也将都会参数化，所有变量都会做参数化处理。
    /// </summary>
    bool IsConstantParameterized { get; set; }
    /// <summary>
    /// 枚举类型常量或变量，在未指定dbType类型时映射到数据库的默认类型，默认值是int类型
    /// </summary>
    Type DefaultEnumMapDbType { get; set; }
    /// <summary>
    /// DateTime、DateTimeOffset类型的DateTimeKind，默认是DateTimeKind.Local，如果返回的日期类型不是默认是DefaultDateTimeKind，将转换为DefaultDateTimeKind类型，如果值为DateTimeKind.Unspecified，将不做处理
    /// </summary>
    DateTimeKind DefaultDateTimeKind { get; set; }
    /// <summary>
    /// 拦截器，默认为null
    /// </summary>
    DbInterceptors DbInterceptors { get; set; }
    /// <summary>
    /// 字段映射处理器，默认为DefaultFieldMapHandler实例
    /// </summary>
    IFieldMapHandler FieldMapHandler { get; set; }


    void UseDefaultDatabase(string dbKey);
    bool TryGetConnectionStringSelector(string dbKey, out Delegate connectionStringSelector);
    void AddConnectionStringSelector(string dbKey, Delegate connectionStringSelector);
    bool TryGetConnectionStringSelector(OrmProviderType ormProviderType, out Delegate connectionStringSelector);
    void AddConnectionStringSelector(OrmProviderType ormProviderType, Delegate connectionStringSelector);

    bool TryGetTableShardingProvider(string dbKey, out ITableShardingProvider tableShardingProvider);
    void AddTableShardingProvider(string dbKey, ITableShardingProvider tableShardingProvider);
    bool TryGetTableShardingProvider(OrmProviderType ormProviderType, out ITableShardingProvider tableShardingProvider);
    void AddTableShardingProvider(OrmProviderType ormProviderType, ITableShardingProvider tableShardingProvider);
    bool TryGetTableSharding(string dbKey, Type entityType, out TableShardingInfo tableShardingInfo);
    bool TryGetTableSharding(OrmProviderType ormProviderType, Type entityType, out TableShardingInfo tableShardingInfo);

    TheaDatabase Register(OrmProviderType ormProviderType, string dbKey, bool isDefaultDatabase);
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
    /// <summary>
    /// 根据已有的dbContext对象，创建仓储对象，可以使用已有的事务
    /// </summary>
    /// <param name="dbContext"></param>
    /// <returns></returns>
    IRepository CreateRepository(DbContext dbContext);
    void Build();
}
