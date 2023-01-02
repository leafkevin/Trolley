using System.Reflection;

namespace Trolley;

public enum ReaderFieldType : byte
{
    Field = 1,
    Entity = 2,
    AnonymousField = 3
}
public class ReaderField
{
    public int Index { get; set; }
    public ReaderFieldType Type { get; set; }
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
    /// 单个字段
    /// </summary>
    public string Body { get; set; }
    /// <summary>
    /// 如果是实体，字段个数，主要用于匿名对象
    /// </summary>
    public int FieldCount { get; set; }
    /// <summary>
    /// 是否有后续的子对象
    /// </summary>
    public bool HasNextInclude { get; set; }
}
