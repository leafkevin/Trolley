using System;
using System.Collections.Generic;

namespace Trolley;

public class TableShardingInfo
{
    /// <summary>
    /// 映射实体
    /// </summary>
    public Type EntityType { get; set; }
    /// <summary>
    /// 依赖的实体成员名称
    /// </summary>
    public List<string> DependOnMembers { get; set; }
    /// <summary>
    /// 分表规则，可用于查询、单条插入、单条更新、删除等操作，可设置依赖字段，也可以设置不依赖字段
    /// </summary>
    public Delegate Rule { get; set; }
    /// <summary>
    /// 分表名称验证正则表达式，用于筛选分表名称
    /// </summary>
    public string ValidateRegex { get; set; }
    /// <summary>
    /// 分表范围规则，用于范围查询、范围更新等操作，常用于时间分表策略查询
    /// </summary>
    public object RangleRule { get; set; }
}
