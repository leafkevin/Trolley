using System;
using System.Collections.Generic;

namespace Trolley;

public interface ITableShardingProvider
{
    ICollection<TableShardingInfo> TableShardings { get; }
    bool TryGetTableSharding(Type entityType, out TableShardingInfo tableShardingInfo);
    void AddTableSharding(Type entityType, TableShardingInfo shardingTableInfo);
}