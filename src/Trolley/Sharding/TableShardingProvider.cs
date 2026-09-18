using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Trolley;

public class TableShardingProvider : ITableShardingProvider
{
    private ConcurrentDictionary<Type, TableShardingInfo> tableShardingProviders = new();
    private ConcurrentDictionary<Type, ConcurrentDictionary<TableUsageMode, TableRuleInfo>> tableRuleProviders = new();
    public ICollection<TableShardingInfo> TableShardings { get; private set; }
    public bool TryGetTableSharding(Type entityType, out TableShardingInfo tableShardingInfo)
        => this.tableShardingProviders.TryGetValue(entityType, out tableShardingInfo);
    public void AddTableSharding(Type entityType, TableShardingInfo tableShardingInfo)
        => this.tableShardingProviders.TryAdd(entityType, tableShardingInfo);
    public bool TryGetTableRule(Type entityType, TableUsageMode usageMode, out TableRuleInfo tableRuleInfo)
    {
        var myTableRuleProviders = this.tableRuleProviders.GetOrAdd(entityType, f => new());
        return myTableRuleProviders.TryGetValue(usageMode, out tableRuleInfo);
    }
    public void AddTableRule(Type entityType, TableUsageMode usageMode, TableRuleInfo tableRuleInfo)
    {
        var myTableRuleProviders = this.tableRuleProviders.GetOrAdd(entityType, f => new());
        if (myTableRuleProviders.TryAdd(usageMode, tableRuleInfo))
        {
            if (this.TryGetTableSharding(entityType, out var tableShardingInfo))
                tableShardingInfo.Rules[usageMode] = tableRuleInfo;
        }
    }
    public void Build() => this.TableShardings = this.tableShardingProviders.Values.ToList();
}