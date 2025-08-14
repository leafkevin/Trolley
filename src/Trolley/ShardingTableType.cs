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
    /// 多分表，首个多分表为主分表
    /// </summary>
    MultiTable,
    /// <summary>
    /// 映射表，与首个多分表的进行表名映射的表，可能是一个或是多个分表
    /// </summary>
    TableMap
}