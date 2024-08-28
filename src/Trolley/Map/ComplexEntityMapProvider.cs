using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Trolley;

public class ComplexEntityMapProvider : IEntityMapProvider
{
    private bool isUseAutoMap = false;
    private readonly IFieldMapHandler defaultFieldMapHandler;
    private readonly IEntityMapProvider mapProvider;
    private readonly IEntityMapProvider globalMapProvider;
    private readonly ConcurrentDictionary<Type, EntityMap> entityMappers = new();
    private IFieldMapHandler fieldMapHandler;

    public ICollection<EntityMap> EntityMaps => this.entityMappers.Values;
    public IFieldMapHandler FieldMapHandler => this.fieldMapHandler;

    public ComplexEntityMapProvider(IEntityMapProvider mapProvider, IEntityMapProvider globalMapProvider, IFieldMapHandler defaultFieldMapHandler)
    {
        this.mapProvider = mapProvider;
        this.globalMapProvider = globalMapProvider;
        this.defaultFieldMapHandler = defaultFieldMapHandler;
        this.fieldMapHandler = defaultFieldMapHandler;
    }

    public void AddEntityMap(Type entityType, EntityMap entityMapper)
    {
        if (this.mapProvider != null && this.mapProvider.TryGetEntityMap(entityType, out _))
            return;
        if (this.globalMapProvider.TryGetEntityMap(entityType, out _))
            return;
        this.mapProvider.AddEntityMap(entityType, entityMapper);
    }
    public bool TryGetEntityMap(Type entityType, out EntityMap entityMapper)
    {
        if (this.mapProvider != null && this.mapProvider.TryGetEntityMap(entityType, out entityMapper))
            return true;
        if (this.globalMapProvider.TryGetEntityMap(entityType, out entityMapper))
            return true;
        return false;
    }
    public void UseDefaultFieldMapHandler() => this.fieldMapHandler = this.defaultFieldMapHandler;
    public void UseFieldMapHandler(IFieldMapHandler fieldMapHandler) => this.fieldMapHandler = fieldMapHandler;
    public void UseAutoMap() { }
    public void Build(TheaDatabase database)
    {
        this.entityMappers.Clear();
        if (this.mapProvider != null)
        {
            foreach (var entityMap in this.mapProvider.EntityMaps)
            {
                this.entityMappers.TryAdd(entityMap.EntityType, entityMap);
            }
        }
        foreach (var entityMap in this.globalMapProvider.EntityMaps)
        {
            this.entityMappers.TryAdd(entityMap.EntityType, entityMap);
        }
    }
}
