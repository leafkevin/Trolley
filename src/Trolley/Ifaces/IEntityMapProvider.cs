using System;
using System.Collections.Generic;

namespace Trolley;

public interface IEntityMapProvider
{
    public Type OrmProviderType { get; }
    public ICollection<EntityMap> EntityMaps { get; }
    void AddEntityMap(Type entityType, EntityMap entityMapper);
    bool TryGetEntityMap(Type entityType, out EntityMap entityMapper);
    void Build(IOrmProvider ormProvider);
}
