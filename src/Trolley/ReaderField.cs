using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Trolley;

/// <summary>
/// 实体字段，经过Select操作后，就会生成ReaderField
/// </summary>
public class ReaderField
{
    /// <summary>
    /// 序号索引，当前字段在返回映射实体中的索引位置
    /// </summary>
    public int Index { get; set; }
    /// <summary>
    /// 字段类型
    /// </summary>
    public ReaderFieldType FieldType { get; set; }
    /// <summary>
    /// Include表的主表ReaderField的序号索引
    /// </summary>
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
    /// 当前是成员访问时，才有值，和FromMember是同一个栏位，是Mapper
    /// </summary>
    public MemberMap MemberMapper { get; set; }
    /// <summary>
    /// 最外层返回实体要设置的成员
    /// </summary>
    public MemberInfo TargetMember { get; set; }
    /// <summary>
    /// 单个字段或是*，只有FromQuery类型表会赋值
    /// </summary>
    public string Body { get; set; }
    /// <summary>
    /// 当前ReaderField是否是已有ReaderField的引用，可以是实体表(真实表)也可以Include表引用 
    /// ReaderField引用，查询数据库时，不会重复查询字段，在创建返回实体的时候，把对应数据再设置进去
    /// Include表引用时，主表数据返回，Include表引用才生效
    /// </summary>
    //public bool IsRef { get; set; }
    /// <summary>
    /// ReaderField引用的索引位置，IsRef为true，才会有值
    /// </summary>
    //public int? RefIndex { get; set; }
    /// <summary>
    /// 是否有后续的Include表，当前是主表ReaderField时且有Include表，此值为true
    /// </summary>
    public bool HasNextInclude { get; set; }
    /// <summary>
    /// 是否是字段，有二元操作或是函数调用时，此值为false
    /// </summary>
    public bool IsOnlyField { get; set; }
    /// <summary>
    /// 实体表(真实表)或是子查询表的所有字段，FieldType为Entity或是AnonymousObject时有值
    /// </summary>
    public List<ReaderField> ReaderFields { get; set; }
    /// <summary>
    /// 字段查询后，执行函数调用的目标对象
    /// </summary>
    public Expression DeferCallTarget { get; set; }
    /// <summary>
    /// 函数委托
    /// 先从数据库中获取字段，再调用函数，赋值指定字段中
    /// </summary>
    public MethodInfo DeferCallMethod { get; set; }
    /// <summary>
    /// 字段查询后，执行函数调用的参数
    /// </summary>
    public List<Expression> DeferCallArgs { get; set; }
    /// <summary>
    /// 是否是最外层目标类型，通常用判断第一个字段是否是参数访问
    /// </summary>
    public bool IsTargetType { get; set; }
}
public enum ReaderFieldType : byte
{
    /// <summary>
    /// 字段
    /// </summary>
    Field = 1,
    /// <summary>
    /// 实体类型，实体表(真实表)、子查询表，都会返回此类型的ReaderField
    /// 通过参数访问、实体类型的成员访问返回，返回的类型是ReaderField列表
    /// </summary>
    Entity = 2,
    /// <summary>
    /// 临时的匿名对象，像Grouping，FromQuery返回的实体对象中的实体类成员
    /// 基于现有实体表(真实表)、子查询表的字段引用，组合生成的临时对象，方便后续SQL查询字段访问,减少代码编写
    /// 通过ReaderFields获取字段，FromQuery返回的实体对象中的实体类成员只支持一层
    /// 也就是说，FromQuery返回的实体中，只允许有一层实体类型的成员
    /// </summary>
    AnonymousObject = 3,
    /// <summary>
    /// 先从数据库中查询，连续的一个或多个字段，再执行函数调用返回一个字段
    /// </summary>
    DeferredFields = 4
}