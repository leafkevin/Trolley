﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace Trolley;

/// <summary>
/// 表类型
/// </summary>
public enum TableType : byte
{
    /// <summary>
    /// 实体表，真实表，主表或是Include表
    /// </summary>
    Entity,
    /// <summary>
    /// 子查询表，临时表
    /// </summary>
    FromQuery,
    /// <summary>
    /// Include子表，此时IsMaster=false
    /// </summary>
    Include,
    /// <summary>
    /// CTE表自身引用，此时IsMaster=false
    /// </summary>
    CteSelfRef,
    /// <summary>
    /// 临时的ReaderFields包装表，只能取ReaderFields栏位使用
    /// </summary>
    TempReaderFields
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
    /// 指定的分表名称，多个分表时才有值，单个分表直接当作不分表处理
    /// </summary>
    public List<string> TableNames { get; set; }
    /// <summary>
    /// 实体表名或是特定TableSchema表或是子查询SQL，如：sys_user或是myschema.sys_user或是(select * from ...)
    /// </summary>
    public string Body { get; set; }
    public string TableSchema { get; set; }
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
    /// 表的访问路径，主表时，是别名，Include表时，是从根主表到本成员完整访问路径，为了便于查找，如:a.Seller.Company.Products
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
    public List<SqlFieldSegment> Fields { get; set; }
    /// <summary>
    /// Include 1:N关系表时，从最外层Select参数访问到Include成员的父亲路径所有成员访问列表，方便最后赋值
    /// </summary>
    public List<MemberInfo> ParentMemberVisits { get; set; }
    /// <summary>
    /// 是否已经指定分表，指定了分表此值为true
    /// </summary>
    public bool IsSharding { get; set; }
    /// <summary>
    /// 区分不同表的临时唯一ID，用作分表替换
    /// </summary>
    public string ShardingId { get; set; }
    /// <summary>
    /// 当前表分表类型
    /// </summary>
    public ShardingTableType ShardingType { get; set; }
    /// <summary>
    /// 当前分表所依赖的主表
    /// </summary>
    public TableSegment ShardingDependent { get; set; }
    /// <summary>
    /// 当前表的分表名筛选委托
    /// </summary>
    public Func<string, bool> ShardingFilter { get; set; }
    /// <summary>
    /// 当前表分表与主表的表名映射委托
    /// </summary>
    public Func<string, string, string, string> ShardingMapGetter { get; set; }

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
            SuffixRawSql = this.SuffixRawSql,
            TableType = this.TableType,
            IsMaster = this.IsMaster,
            Path = this.Path,
            Filter = this.Filter,
            OnExpr = joinOnExpr,
            Fields = this.Fields
        };
    }
}
