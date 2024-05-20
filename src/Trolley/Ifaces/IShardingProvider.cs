using System;
using System.Collections.Generic;

namespace Trolley;

public interface IShardingProvider
{
    string UseDefaultDatabase(string defaultDbKey);
    void UseDatabase(Func<string> dbKeySelector);
    bool TryGetShardingTable(Type entityType, out ShardingTable shardingTable);
    void AddShardingTable(Type entityType, ShardingTable shardingTable);
}
