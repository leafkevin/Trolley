namespace Trolley;

public enum ShardingTableType : byte
{
    /// <summary>
    /// 不分表
    /// </summary>
    None,
    /// <summary>
    /// 指定一个分表
    /// </summary>
    SingleTable,
    /// <summary>
    /// 指定多个分表
    /// </summary>
    MultiTable,
    /// <summary>
    /// 范围区间表，通常是时间分表场景
    /// </summary>
    TableRange,
    /// <summary>
    /// 主表表达式条件过滤
    /// </summary>
    MasterFilter,
    /// <summary>
    /// 从表表名映射
    /// </summary>
    SubordinateMap
}