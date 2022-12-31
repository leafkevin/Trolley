using System;

namespace Trolley;

public class IncludeSegment
{
    /// <summary>
    /// 当前查询中的Order表的TableSegment
    /// </summary>
    public TableSegment FromTable { get; set; }
    /// <summary>
    /// Order实体内，Details成员的MemberMapper
    /// </summary>
    public MemberMap IncludeMember { get; set; }
    /// <summary>
    /// OrderDetail Mapper
    /// </summary>
    public EntityMap TargetMapper { get; set; }
}