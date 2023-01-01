using System;

namespace Trolley;

public class TableSegment
{
    public string JoinType { get; set; }
    public Type EntityType { get; set; }
    public string AliasName { get; set; }
    /// <summary>
    /// 父亲表，如：Order表
    /// </summary>
    public TableSegment FromTable { get; set; }
    /// <summary>
    /// 父亲实体中的成员访问，如：Order中的Details
    /// </summary>
    public MemberMap FromMember { get; set; }    
    /// <summary>
    /// 当前表Mapper，如：OrderDetail
    /// </summary>
    public EntityMap Mapper { get; set; }
    public string Body { get; set; }
    public bool IsInclude { get; set; }
    public bool IsMaster { get; set; }
    public string Path { get; set; }
    public string Filter { get; set; }
    public string OnExpr { get; set; }
    public bool IsUsed { get; set; }
}