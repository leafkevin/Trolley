using System;
using System.Collections.Generic;

namespace Trolley;

public interface IOrmDbFactory
{
    ITypeHandlerProvider TypeHandlerProvider { get; }
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
