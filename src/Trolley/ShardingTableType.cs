namespace Trolley;

public enum ShardingTableType : byte
{
    /// <summary>
    /// 不分表
    /// </summary>
    None,
    /// <summary>
    /// 指定分表
    /// </summary>
    TableName,
    /// <summary>
    /// 范围区间表，通常是时间分表场景
    /// </summary>
    TableRange,
    /// <summary>
    /// 主表分表筛选表达式
    /// </summary>
    MasterFilter,
    /// <summary>
    /// 从表表名映射
    /// </summary>
    SubordinateMap
}