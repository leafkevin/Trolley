using System;
using System.Collections.Generic;

namespace Trolley;

public interface IOrmDbFactory
{
    ICollection<TheaDatabase> Databases { get; }
    ICollection<ITypeHandler> TypeHandlers { get; }

    void AddOrmProvider(IOrmProvider ormProvider);
    bool TryGetOrmProvider(Type ormProviderType, out IOrmProvider ormProvider);
    bool TryGetOrmProvider(OrmProviderType ormProviderType, out IOrmProvider ormProvider);
    void AddMapProvider(Type ormProviderType, IEntityMapProvider mapProvider);
    bool TryGetMapProvider(Type ormProviderType, out IEntityMapProvider mapProvider);
    void AddTypeHandler(ITypeHandler typeHandler);
    bool TryGetTypeHandler(Type handlerType, out ITypeHandler typeHandler);
    TheaDatabase GetDatabase(string dbKey = null);
    //IRepository Create(string dbKey = null, string tenantId = null);
    TRepository Create<TRepository>(string dbKey = null, string tenantId = null) where TRepository : IRepository, new();
}
