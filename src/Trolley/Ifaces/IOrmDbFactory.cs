using System;
using System.Collections.Generic;

namespace Trolley;

public interface IOrmDbFactory
{
    #region 配置属性Options
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
    #endregion

    #region DbFactory运行时属性
    /// <summary>
    /// dbKey数据库实例选择器委托
    /// </summary>
    public Delegate DbKeySelector { get; }
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

    void UseTypeHandler(ITypeHandler typeHandler);

    IRepository CreateRepository(string dbKey);
    void Build();
}
