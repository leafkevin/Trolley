using System;

namespace Trolley;

public class TheaDatabase
{
    public string DbKey { get; set; }
    public string ConnectionString { get; set; }
    public bool IsDefault { get; set; }
    public int[] TenantIds { get; set; }
    public Type OrmProviderType { get; set; }
}