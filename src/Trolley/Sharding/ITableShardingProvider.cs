using System;
using System.Collections.Generic;

namespace Trolley;

public interface ITableShardingProvider
{
    ICollection<TableShardingInfo> TableShardings { get; }
    bool TryGetTableSharding(Type entityType, out TableShardingInfo tableShardingInfo);
    void AddTableSharding(Type entityType, TableShardingInfo shardingTableInfo);
}
public enum TableShardingUsageMode
{
    /// <summary>
    /// 所有操作类型，包括增、删、改、查所有操作
    /// </summary>
    Default = 0,
    /// <summary>
    /// 只读，查询操作
    /// </summary>
    ReadOnly = 1,
    /// <summary>
    /// 只写，包含插入、更新、删除操作
    /// </summary>
    WriteOnly = 2
}