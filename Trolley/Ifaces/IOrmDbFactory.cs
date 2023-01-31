using System;

namespace Trolley;

public interface IOrmDbFactory
{
    TheaDatabase Register(string dbKey, bool isDefault, Action<TheaDatabaseBuilder> databaseInitializer);
    void AddOrmProvider(IOrmProvider ormProvider);
    void AddTypeHandler(ITypeHandler typeHandler);
    void AddEntityMap(Type entityType, EntityMap mapper);
    bool TryGetTypeHandler(Type handlerType, out ITypeHandler typeHandler);
    bool TryGetEntityMap(Type entityType, out EntityMap mapper);
    IRepository Create(TheaConnection connection);
    IRepository Create(string dbKey = null, int? tenantId = null);
    TheaConnectionInfo GetConnectionInfo(string dbKey = null, int? tenantId = null);
    TheaDatabase GetDatabase(string dbKey = null);
}
