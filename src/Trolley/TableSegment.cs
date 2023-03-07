using System;
using System.Collections.Generic;

namespace Trolley;

public enum TableType : byte
{
    Master = 1,
    Include = 2,
    IncludeMany = 3,
    /// <summary>
    /// 临时映射表
    /// </summary>
    MapTable = 4,
    CteTable = 5
}
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
    /// <summary>
    /// 嵌套表的SQL内容，如：select * from (...) a
    /// </summary>
    public string Body { get; set; }
    /// <summary>
    /// SqlServer在表名后，会接一个后缀字符串，如：select * from A WITH (UPDLOCK)
    /// </summary>
    public string SuffixRawSql { get; set; }
    public TableType TableType { get; set; }
    public bool IsMaster { get; set; }
    public string Path { get; set; }
    public string Filter { get; set; }
    public string OnExpr { get; set; }
    public List<ReaderField> ReaderFields { get; set; }
    public bool IsUsed { get; set; }

    public override bool Equals(object obj)
    {
        if (obj == null) return false;
        var right = (TableSegment)obj;
        return this.Path == right.Path;
    }
    public override int GetHashCode() => HashCode.Combine(this.Path);
}