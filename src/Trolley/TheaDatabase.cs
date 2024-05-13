using System;
using System.Collections.Generic;

namespace Trolley;

public class TheaDatabase
{
    public string DbKey { get; set; }
    public string ConnectionString { get; set; }
    public bool IsDefault { get; set; }
    public Type OrmProviderType { get; set; }
    public IOrmProvider OrmProvider { get; internal set; }
    public IEntityMapProvider MapProvider { get; internal set; }
}
