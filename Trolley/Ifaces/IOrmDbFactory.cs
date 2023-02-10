using System;

namespace Trolley;

public interface IOrmDbFactory
{
    void Register(string dbKey, bool isDefault, Action<TheaDatabaseBuilder> databaseInitializer);
    void AddOrmProvider(IOrmProvider ormProvider);
    bool TryGetOrmProvider(Type ormProviderType, out IOrmProvider ormProvider);
    void AddTypeHandler(ITypeHandler typeHandler);
    bool TryGetTypeHandler(Type handlerType, out ITypeHandler typeHandler);
    IRepository Create(string dbKey = null, int? tenantId = null);
    TheaDatabaseProvider GetDatabaseProvider(string dbKey = null);
}
