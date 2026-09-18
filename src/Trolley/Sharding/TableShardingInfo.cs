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
    /// 依赖的实体成员名称，分表规则参数对应的成员名称列表
    /// </summary>
    public List<string> DependOnMembers { get; set; }
    /// <summary>
    /// 分表名确定规则
    /// </summary>
    public Dictionary<TableUsageMode, TableRuleInfo> Rules { get; set; } = new();
}