using System;
using System.Collections.Generic;

namespace Trolley;

public interface IShardingProvider
{
    void UseDatabase(Func<string> dbKeySelector);
    //List<string> GetShardingTableNames(string dbKey, params string[] origTableNames);
    bool TryGetShardingTable(Type entityType, out ShardingTable shardingTable);
    void AddShardingTable(Type entityType, ShardingTable shardingTable);
}
