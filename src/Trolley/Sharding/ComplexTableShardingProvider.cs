using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Trolley;

public class ComplexTableShardingProvider : ITableShardingProvider
{
    private ConcurrentDictionary<Type, TableShardingInfo> tableShardingProviders = new();
    public ICollection<TableShardingInfo> TableShardings => this.tableShardingProviders.Values;

    public ComplexTableShardingProvider(ITableShardingProvider tableShardingProvider, ITableShardingProvider globalTableShardingProvider)
    {
        if (tableShardingProvider != null)
        {
            foreach (var tableShardingInfo in tableShardingProvider.TableShardings)
            {
                this.tableShardingProviders.TryAdd(tableShardingInfo.EntityType, tableShardingInfo);
            }
        }
        foreach (var tableShardingInfo in globalTableShardingProvider.TableShardings)
        {
            this.tableShardingProviders.TryAdd(tableShardingInfo.EntityType, tableShardingInfo);
        }
    }

    public bool TryGetTableSharding(Type entityType, out TableShardingInfo tableShardingInfo)
        => this.tableShardingProviders.TryGetValue(entityType, out tableShardingInfo);
    public void AddTableSharding(Type entityType, TableShardingInfo tableShardingInfo)
        => this.tableShardingProviders.TryAdd(entityType, tableShardingInfo);
}