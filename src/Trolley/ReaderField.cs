using System.Collections.Generic;
using System.Reflection;

namespace Trolley;

public class ReaderField
{
    public int Index { get; set; }
    public ReaderFieldType FieldType { get; set; }
    public int? ParentIndex { get; set; }
    /// <summary>
    /// 当前查询中的Table，如：User表
    /// </summary>
    public TableSegment TableSegment { get; set; }
    /// <summary>
    /// 父亲对象中的成员，如：Order.Buyer成员
    /// </summary>
    public MemberInfo FromMember { get; set; }
    /// <summary>
    /// 最外层返回实体要设置的成员
    /// </summary>
    public MemberInfo TargetMember { get; set; }
    /// <summary>
    /// 单个字段或是*
    /// </summary>
    public string Body { get; set; }
    /// <summary>
    /// 是否是最外层返回实体的字段，只对当前字段有效，
    /// 如果当前字段类型还是一个实体，就是一个嵌套的匿名对象了
    /// </summary>
    public bool IsTarget { get; set; }
    /// <summary>
    /// 是否有后续的子对象
    /// </summary>
    public bool HasNextInclude { get; set; }
    /// <summary>
    /// 临时表的字段集合，通常是从一个子查询返回的
    /// </summary>
    public List<ReaderField> ReaderFields { get; set; }
}
