using System.Linq.Expressions;
using System.Reflection;

namespace Trolley;

public class MemberSegment
{
    public int ReaderIndex { get; set; }
    public int TableIndex { get; set; }
    /// <summary>
    /// 当前实体的父亲成员
    /// </summary>
    public MemberInfo FromMember { get; set; }
    /// <summary>
    /// 字段来源表的成员映射
    /// </summary>
    public MemberMap MemberMapper { get; set; }
    /// <summary>
    /// 字段来源表
    /// </summary>
    public TableSegment TableSegment { get; set; }
    public string Body { get; set; }
    public bool IsNeedAlias { get; set; }
}
