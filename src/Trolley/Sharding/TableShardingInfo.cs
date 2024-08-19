using System.Collections.Generic;

namespace Trolley;

public class TableShardingInfo
{
    /// <summary>
    /// 依赖的实体成员名称
    /// </summary>
    public List<string> DependOnMembers { get; set; }
    public object Rule { get; set; }
    /// <summary>
    /// 分表名称验证正则表达式，用于筛选分表名称
    /// </summary>
    public string ValidateRegex { get; set; }
    public object RangleRule { get; set; }
}
