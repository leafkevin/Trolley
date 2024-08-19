using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Trolley;

public class TableShardingProvider : ITableShardingProvider
{
    private ConcurrentDictionary<Type, TableShardingInfo> tableShardingProviders = new();
    public ICollection<TableShardingInfo> TableShardings => this.tableShardingProviders.Values;
    public bool TryGetTableSharding(Type entityType, out TableShardingInfo tableShardingInfo)
        => this.tableShardingProviders.TryGetValue(entityType, out tableShardingInfo);
    public void AddTableSharding(Type entityType, TableShardingInfo tableShardingInfo)
        => this.tableShardingProviders.TryAdd(entityType, tableShardingInfo);
}
