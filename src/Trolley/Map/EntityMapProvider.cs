using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Trolley;

public class EntityMapProvider : IEntityMapProvider
{
    private readonly IFieldMapHandler defaultFieldMapHandler;
    private readonly ConcurrentDictionary<Type, EntityMap> entityMappers = new();
    private IFieldMapHandler fieldMapHandler;

    public ICollection<EntityMap> EntityMaps => this.entityMappers.Values;
    public IFieldMapHandler FieldMapHandler => this.fieldMapHandler;

    public EntityMapProvider(IFieldMapHandler defaultFieldMapHandler)
    {
        this.defaultFieldMapHandler = defaultFieldMapHandler;
        this.fieldMapHandler = defaultFieldMapHandler;
    }
    public void AddEntityMap(Type entityType, EntityMap entityMapper) =>
        this.entityMappers.TryAdd(entityType, entityMapper);
    public bool TryGetEntityMap(Type entityType, out EntityMap entityMapper)
        => this.entityMappers.TryGetValue(entityType, out entityMapper);
    public void UseDefaultFieldMapHandler() => this.fieldMapHandler = this.defaultFieldMapHandler;
    public void UseFieldMapHandler(IFieldMapHandler fieldMapHandler) => this.fieldMapHandler = fieldMapHandler;
    public void Build(TheaDatabase database)
    {
        database.OrmProvider.MapTables(database.UseMaster(), this);
        foreach (var entityMapper in this.EntityMaps)
            entityMapper.Build(database.OrmProvider);
    }
}