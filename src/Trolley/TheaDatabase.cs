using System;
using System.Collections.Generic;

namespace Trolley;

public class TheaDatabase
{
    public string DbKey { get; set; }
    public string ConnectionString { get; set; }
    public bool IsDefault { get; set; }
    /// <summary>
    /// 默认是数据库名字，只有明确设置才不是默认值
    /// </summary>
    public string DefaultTableSchema { get; set; }
    public OrmProviderType OrmProviderType { get; set; }
    public IOrmProvider OrmProvider { get; internal set; }
    public IEntityMapProvider MapProvider { get; internal set; }
}
