using System;
using System.Collections.Generic;

namespace Trolley;

public interface IOrmDbFactory
{
    ICollection<TheaDatabase> Databases { get; }
    ICollection<IOrmProvider> OrmProviders { get; }
    ICollection<IEntityMapProvider> MapProviders { get; }

    void UseDatabase(Func<string> dbKeySelector);
    bool TryGetShardingTableNames(string dbKey, Type entityType, out List<string> tableNames);
    void AddShardingTableNames(string dbKey, Type entityType, List<string> tableNames);
    bool TryGetShardingTable(Type entityType, out ShardingTable shardingTable);
    void AddShardingTable(Type entityType, ShardingTable shardingTable);

    void Register(string dbKey, string connectionString, Type ormProviderType, bool isDefault);
    void AddOrmProvider(IOrmProvider ormProvider);
    bool TryGetOrmProvider(Type ormProviderType, out IOrmProvider ormProvider);
    bool TryGetOrmProvider(OrmProviderType ormProviderType, out IOrmProvider ormProvider);
    void AddMapProvider(Type ormProviderType, IEntityMapProvider mapProvider);
    bool TryGetMapProvider(Type ormProviderType, out IEntityMapProvider mapProvider);
    TheaDatabase GetDatabase(string dbKey = null);
    IRepository CreateRepository(string dbKey = null);
}
