using System;
using System.Collections.Generic;

namespace Trolley;

public interface IEntityMapProvider
{
    ICollection<EntityMap> EntityMaps { get; }
    Func<string, string, bool> FieldMapHandler { get; set; }
    void UseEntityMap(Type entityType, EntityMap entityMapper);
    bool TryGetEntityMap(Type entityType, out EntityMap entityMapper);
    void Build(TheaDatabase database);
}