using System;
using System.Collections.Generic;

namespace Trolley;

public interface IOrmDbFactory
{
    /// <summary>
    /// 类型处理器提供者，可以添加和获取类型处理器
    /// </summary>
    ITypeHandlerProvider TypeHandlerProvider { get; }
    /// <summary>
    /// 数据库提供者集合，包含所有已注册的数据库提供者。每个数据库提供者，又包含一组数据库
    /// </summary>
    ICollection<TheaDatabaseProvider> DatabaseProviders { get; }

    void Register(string dbKey, bool isDefault, Action<TheaDatabaseBuilder> databaseInitializer);
    void AddOrmProvider(IOrmProvider ormProvider);
    bool TryGetOrmProvider(Type ormProviderType, out IOrmProvider ormProvider);
    void AddEntityMapProvider(Type ormProviderType, IEntityMapProvider entityMapProvider);
    bool TryGetEntityMapProvider(Type ormProviderType, out IEntityMapProvider entityMapProvider);
    void AddTypeHandler(ITypeHandler typeHandler);
    bool TryGetTypeHandler(Type handlerType, out ITypeHandler typeHandler);
    void Configure(Type ormProviderType, IModelConfiguration configuration);
    TheaDatabaseProvider GetDatabaseProvider(string dbKey = null);
    IRepository Create(string dbKey = null, int? tenantId = null);
}
