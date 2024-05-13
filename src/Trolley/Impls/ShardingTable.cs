using System.Collections.Generic;

namespace Trolley;

public class ShardingTable
{
    /// <summary>
    /// 依赖的实体成员名称
    /// </summary>
    public List<string> DependOnMembers { get; set; }
    public object Rule { get; set; }
    public object RangleRule { get; set; }
}
