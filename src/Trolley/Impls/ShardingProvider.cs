using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Trolley;

public class ShardingProvider : IShardingProvider
{
    private Func<string> dbKeySelector = null;
    private ConcurrentDictionary<Type, ShardingTable> shardingTables = new();

    public string UseDefaultDatabase(string defaultDbKey) => this.dbKeySelector?.Invoke() ?? defaultDbKey;
    public void UseDatabase(Func<string> dbKeySelector) => this.dbKeySelector = dbKeySelector;
    public List<string> GetShardingTableNames(string dbKey, params string[] origTableNames)
    {
        return null;
    }
    public bool TryGetShardingTable(Type entityType, out ShardingTable shardingTable)
        => this.shardingTables.TryGetValue(entityType, out shardingTable);
    public void AddShardingTable(Type entityType, ShardingTable shardingTable)
       => this.shardingTables.TryAdd(entityType, shardingTable);
}
