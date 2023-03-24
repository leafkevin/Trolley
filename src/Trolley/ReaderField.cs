using System.Collections.Generic;
using System.Reflection;

namespace Trolley;

public enum ReaderFieldType : byte
{
    Field = 1,
    Entity = 2,
    /// <summary>
    /// 临时的匿名对象，像Grouping，FromQuery中的参数访问的实体类成员
    /// </summary>
    AnonymousObject = 3,
    /// <summary>
    /// 访问了实体的IncludeMany的成员，当前字段是主表主键字段
    /// </summary>
    MasterField = 4
}
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
    /// 是否有后续的子对象
    /// </summary>
    public bool HasNextInclude { get; set; }
    public List<ReaderField> ReaderFields { get; set; }
}
