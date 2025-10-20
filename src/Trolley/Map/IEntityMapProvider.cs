using System;
using System.Collections.Generic;

namespace Trolley;

public interface IEntityMapProvider
{
    ICollection<EntityMap> EntityMaps { get; }
    void AddEntityMap(Type entityType, EntityMap entityMapper);
    bool TryGetEntityMap(Type entityType, out EntityMap entityMapper);
    void Build(TheaDatabase database, IFieldMapHandler fieldMapHandler);
}