using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Trolley;

public class EntityMapProvider : IEntityMapProvider
{
    private readonly ConcurrentDictionary<Type, EntityMap> entityMappers = new();
    public ICollection<EntityMap> EntityMaps => this.entityMappers.Values;
    public Func<string, string, bool> IsCanMapTo { get; set; }
    public void UseEntityMap(Type entityType, EntityMap entityMapper) =>
        this.entityMappers.AddOrUpdate(entityType, entityMapper, (k, o) => entityMapper);
    public bool TryGetEntityMap(Type entityType, out EntityMap entityMapper)
        => this.entityMappers.TryGetValue(entityType, out entityMapper);
    public void Build(TheaDatabase database)
    {
        //获取数据库元数据，如果全部实体映射都已经存在，则不需要重新映射
        foreach (var connectionString in database.ConnectionStrings)
        {
            if (database.OrmProvider.MapTables(connectionString, this))
                break;
        }
        //映射实体每个字段
        foreach (var entityMapper in this.EntityMaps)
            entityMapper.Build(database.OrmProvider);
    }
}