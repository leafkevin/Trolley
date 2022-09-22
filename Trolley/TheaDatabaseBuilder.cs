using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Trolley;

public class TheaDatabaseBuilder
{
    private readonly ConcurrentDictionary<Type, IOrmProvider> ormProviders = new();
    private readonly TheaDatabase database;

    public TheaDatabaseBuilder(TheaDatabase database)
    {
        this.database = database;
    }
    public TheaDatabaseBuilder Add<TOrmProvider>(string connectionString, bool isDefault, List<int> tenantIds = null) where TOrmProvider : class, IOrmProvider, new()
    {
        var type = typeof(TOrmProvider);
        if (!ormProviders.TryGetValue(type, out var ormProvider))
            ormProviders.TryAdd(type, ormProvider = new TOrmProvider());
        this.database.ConnectionStrings.Add(new TheaConnectionInfo
        {
            DbKey = this.database.DbKey,
            ConnectionString = connectionString,
            IsDefault = isDefault,
            OrmProvider = ormProvider,
            TenantIds = tenantIds
        });
        return this;
    }
}
