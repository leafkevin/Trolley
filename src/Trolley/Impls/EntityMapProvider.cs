using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Trolley;

public class EntityMapProvider : IEntityMapProvider
{
    private readonly ConcurrentDictionary<Type, EntityMap> entityMappers = new();
    public Type OrmProviderType { get; set; }
    public ICollection<EntityMap> EntityMaps => this.entityMappers.Values;

    public void AddEntityMap(Type entityType, EntityMap entityMapper) =>
        this.entityMappers.TryAdd(entityType, entityMapper);
    public bool TryGetEntityMap(Type entityType, out EntityMap entityMapper)
        => this.entityMappers.TryGetValue(entityType, out entityMapper);
    public void Build(IOrmProvider ormProvider)
    {
        foreach (var entityMapper in this.EntityMaps)
            entityMapper.Build(ormProvider);
    }
}
