using System;
using System.Collections.Generic;

namespace Trolley;

public interface IEntityMapProvider
{
    ICollection<EntityMap> EntityMaps { get; }
    IFieldMapHandler FieldMapHandler { get; }
    void AddEntityMap(Type entityType, EntityMap entityMapper);
    bool TryGetEntityMap(Type entityType, out EntityMap entityMapper);
    void UseDefaultFieldMapHandler();
    void UseFieldMapHandler(IFieldMapHandler fieldMapHandler);
    void UseAutoMap();
    void Build(TheaDatabase database);
}