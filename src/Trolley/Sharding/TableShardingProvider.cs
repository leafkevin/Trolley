using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Trolley;

public class TableShardingProvider : ITableShardingProvider
{
    private ConcurrentDictionary<Type, ConcurrentDictionary<TableShardingType, TableShardingInfo>> tableShardingProviders = new();
    public ICollection<TableShardingInfo> TableShardings { get; private set; }
    public bool TryGetTableSharding(Type entityType, TableShardingType shardingType, out TableShardingInfo tableShardingInfo)
    {
        var myTableShardingProviders = this.tableShardingProviders.GetOrAdd(entityType, f => new());
        return myTableShardingProviders.TryGetValue(shardingType, out tableShardingInfo);
    }
    public void AddTableSharding(Type entityType, TableShardingType shardingType, TableShardingInfo tableShardingInfo)
    {
        var myTableShardingProviders = this.tableShardingProviders.GetOrAdd(entityType, f => new());
        myTableShardingProviders.TryAdd(shardingType, tableShardingInfo);
    }
    internal void Build() => this.TableShardings = this.tableShardingProviders.Values.SelectMany(f => f.Values).ToList();
}