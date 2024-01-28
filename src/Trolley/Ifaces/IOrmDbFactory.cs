using System;
using System.Collections.Generic;

namespace Trolley;

public interface IOrmDbFactory
{
    ICollection<TenantDatabase> Databases { get; }
    ICollection<IOrmProvider> OrmProviders { get; }
    ICollection<IEntityMapProvider> MapProviders { get; }

    void Register(string dbKey, string connectionString, Type ormProviderType, bool isDefault);
    void AddOrmProvider(IOrmProvider ormProvider);
    bool TryGetOrmProvider(Type ormProviderType, out IOrmProvider ormProvider);
    bool TryGetOrmProvider(OrmProviderType ormProviderType, out IOrmProvider ormProvider);
    void AddMapProvider(Type ormProviderType, IEntityMapProvider mapProvider);
    bool TryGetMapProvider(Type ormProviderType, out IEntityMapProvider mapProvider);
    TenantDatabase GetDatabase(string dbKey = null);
    IRepository CreateRepository(string dbKey = null);
}
