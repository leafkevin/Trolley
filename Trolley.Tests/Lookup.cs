using System;

namespace Trolley.Tests;

/// <summary>
/// 参数表
/// </summary>
public class Lookup
{
    /// <summary>
    /// 参数ID
    /// </summary>
    public string LookupId { get; set; }
    /// <summary>
    /// 参数名称
    /// </summary>
    public string LookupName { get; set; }
    /// <summary>
    /// 描述
    /// </summary>
    public string Description { get; set; }
    /// <summary>
    /// 上级ID
    /// </summary>
    public string ParentId { get; set; }
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }
    /// <summary>
    /// 创建人
    /// </summary>
    public int CreatedBy { get; set; }
    /// <summary>
    /// 创建日期
    /// </summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>
    /// 最后更新人
    /// </summary>
    public int UpdatedBy { get; set; }
    /// <summary>
    /// 最后更新日期
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
/// <summary>
/// 参数值表
/// </summary>
public class LookupValue
{
    /// <summary>
    /// 参数ID
    /// </summary>
    public string LookupId { get; set; }
    /// <summary>
    /// 参数值
    /// </summary>
    public string Value { get; set; }
    /// <summary>
    /// 参数文本
    /// </summary>
    public string LookupText { get; set; }
    /// <summary>
    /// 描述
    /// </summary>
    public string Description { get; set; }
    /// <summary>
    /// 序号
    /// </summary>
    public int Sequence { get; set; }
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }
    /// <summary>
    /// 创建人
    /// </summary>
    public int CreatedBy { get; set; }
    /// <summary>
    /// 创建日期
    /// </summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>
    /// 最后更新人
    /// </summary>
    public int UpdatedBy { get; set; }
    /// <summary>
    /// 最后更新日期
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
