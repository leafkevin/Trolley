using System;
using System.Collections.Generic;

namespace Trolley;

/// <summary>
/// 表类型
/// </summary>
public enum TableType : byte
{
    /// <summary>
    /// 实体表，真实表，主表或是Include表
    /// </summary>
    Entity = 1,
    /// <summary>
    /// 子查询表，临时表
    /// </summary>
    FromQuery = 2,
    /// <summary>
    /// CTE表自身引用，此时IsMaster=false
    /// </summary>
    CteSelfRef
}
public class TableSegment
{
    /// <summary>
    /// 关联类型，第一个表的JoinType为空字符串，后面的每个表都有关联类型
    /// LEFT JOIN、INNER JOIN、RIGHT JOIN中的值之一
    /// </summary>
    public string JoinType { get; set; }
    /// <summary>
    /// 实体类型，可以是数据库表的实体类型，也可以是匿名对象类型
    /// </summary>
    public Type EntityType { get; set; }
    /// <summary>
    /// 表别名
    /// </summary>
    public string AliasName { get; set; }
    /// <summary>
    /// 父亲表，当前实体表是FromTable实体表中的Include导航属性，如：order.Buyer，当前实体表是buyer表，FromTable值是order实体表
    /// </summary>
    public TableSegment FromTable { get; set; }
    /// <summary>
    /// 父亲实体中的成员访问，如：order.Buyer中Buyer属性
    /// </summary>
    public MemberMap FromMember { get; set; }
    /// <summary>
    /// 当前实体表的模型映射Mapper
    /// </summary>
    public EntityMap Mapper { get; set; }
    /// <summary>
    /// 实体表名或是子查询SQL，如：sys_user或是(select * from ...)
    /// </summary>
    public string Body { get; set; }
    /// <summary>
    /// 自身引用时的表名，主要用在cte自身引用时的表名，如：WITH CteTableName (...) AS ...中的CteTableName
    /// </summary>
    public string RefTableName { get; set; }
    /// <summary>
    /// 表后缀字符串，SqlServer在表名后，会接一个后缀字符串，如：select * from A WITH (UPDLOCK)
    /// </summary>
    public string SuffixRawSql { get; set; }
    /// <summary>
    /// 表类型，实体表(真实表)从Mapper中取字段，子查询表时，从ReaderFields中取字段
    /// </summary>
    public TableType TableType { get; set; }
    /// <summary>
    /// 是否是主表，可以是实体表(真实表)或是子查询表
    /// </summary>
    public bool IsMaster { get; set; }
    /// <summary>
    /// 表的访问路径，主表时，是别名，Include表时，是主表的成员访问，如:a.Buyer, 只有一个层级
    /// </summary>
    public string Path { get; set; }
    /// <summary>
    /// IncludeMany表的过滤调整，1:N的Include表，在第二次查询中会使用
    /// </summary>
    public string Filter { get; set; }
    /// <summary>
    /// 与Join表的关联条件，通常使用LeftJoin，放在右表后面
    /// </summary>
    public string OnExpr { get; set; }
    /// <summary>
    /// 子查询表时，所有字段定义，包括CTE表
    /// </summary>
    public List<ReaderField> ReaderFields { get; set; }
    /// <summary>
    /// 是否需要别名，子查询表需要别名
    /// </summary>
    public bool IsNeedAlais { get; set; }
    /// <summary>
    /// 表是否被使用
    /// </summary>
    public bool IsUsed { get; set; }

    /// <summary>
    /// 生成一个自身引用的副本，主要用在cte表的自身引用
    /// </summary>
    /// <param name="aliasName"></param>
    /// <param name="joinType"></param>
    /// <param name="joinOnExpr"></param>
    /// <returns></returns>
    public TableSegment Clone(string aliasName = "a", string joinType = null, string joinOnExpr = null)
    {
        return new TableSegment
        {
            JoinType = joinType,
            EntityType = this.EntityType,
            AliasName = aliasName,
            FromTable = this.FromTable,
            FromMember = this.FromMember,
            Mapper = this.Mapper,
            Body = this.Body,
            RefTableName = this.RefTableName,
            SuffixRawSql = this.SuffixRawSql,
            TableType = this.TableType,
            IsMaster = this.IsMaster,
            Path = this.Path,
            Filter = this.Filter,
            OnExpr = joinOnExpr,
            ReaderFields = this.ReaderFields,
            IsNeedAlais = this.IsNeedAlais,
            IsUsed = false
        };
    }

    public override bool Equals(object obj)
    {
        if (obj == null) return false;
        var right = (TableSegment)obj;
        return this.Path == right.Path;
    }
    public override int GetHashCode()
        => HashCode.Combine(this.EntityType, this.Path);
}