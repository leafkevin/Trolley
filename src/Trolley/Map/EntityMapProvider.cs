using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Trolley;

public class EntityMapProvider : IEntityMapProvider
{
    private readonly IFieldMapHandler defaultFieldMapHandler;
    private readonly ConcurrentDictionary<Type, EntityMap> entityMappers = new();

    public ICollection<EntityMap> EntityMaps => this.entityMappers.Values;

    public void AddEntityMap(Type entityType, EntityMap entityMapper) =>
        this.entityMappers.TryAdd(entityType, entityMapper);
    public bool TryGetEntityMap(Type entityType, out EntityMap entityMapper)
        => this.entityMappers.TryGetValue(entityType, out entityMapper);
    /// <summary>
    /// 构建本实体映射对象
    /// </summary>
    /// <param name="database"></param>
    /// <param name="fieldMapHandler"></param>
    public void Build(TheaDatabase database, IFieldMapHandler fieldMapHandler)
    {
        //获取数据库元数据，如果全部实体映射都已经存在，则不需要重新映射
        foreach (var connectionString in database.MasterConnectionStrings)
        {
            if (database.OrmProvider.MapTables(connectionString, this, fieldMapHandler))
                break;
        }
        //映射实体每个字段
        foreach (var entityMapper in this.EntityMaps)
            entityMapper.Build(database.OrmProvider);
    }
}