using System;
using System.Collections.Generic;

namespace Trolley;

public class TableShardingInfo
{
    /// <summary>
    /// 映射实体
    /// </summary>
    public Type EntityType { get; set; }
    public TableShardingUsageMode UsageMode { get; set; }
    /// <summary>
    /// 依赖的实体成员名称，分表规则参数对应的成员名称列表
    /// </summary>
    public List<string> DependOnMembers { get; set; }
    /// <summary>
    /// 分表规则，可用于查询、插入、更新、删除等操作，可设置依赖字段，也可以设置不依赖字段。
    /// 设置依赖字段后，未手动指定分表规则时会根据依赖字段进行规则获取分表名，如果不设置依赖字段，执行增删改查操作都需要手动指定分表名或是分表名获取委托。
    /// 委托第一个参数是原始表名，第二个参数是依赖字段的值数组，返回值是分表名称。
    /// </summary>
    public Func<string, object[], string> Rule { get; set; }
    /// <summary>
    /// 分表名称验证正则表达式，用于筛选分表名称
    /// </summary>
    public string ValidateRegex { get; set; }
    /// <summary>
    /// 分表范围规则，用于查询、更新操作，执行查询时，需要手动指定范围参数，常用于时间、数字等分表策略查询。
    /// 委托第一个参数是原始表名，第二个参数是依赖字段的值数组，数组的最后两个字段值是范围起始值和结束值，返回值是分表名称。
    /// </summary>
    public Func<string, object[], List<string>> RangleRule { get; set; }
}
