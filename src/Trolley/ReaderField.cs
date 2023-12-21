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
    //public int Index { get; set; }
    /// <summary>
    /// 字段类型
    /// </summary>
    public ReaderFieldType FieldType { get; set; }
    /// <summary>
    /// 当前查询中的Table，如：User表
    /// </summary>
    public TableSegment TableSegment { get; set; }
    /// <summary>
    /// 原TableSegment表中的成员，Include子表的场景时，父亲对象中的成员，如：Order.Buyer成员，根据此成员信息设置主表属性值
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
    /// 当前ReaderField是否是已有Include表中的ReaderField的引用
    /// ReaderField引用，查询数据库时，不会重复查询字段，在创建返回实体的时候，把对应数据再设置进去
    /// Include表引用时，主表数据返回，Include表引用才生效
    /// </summary>
    public bool IsRef { get; set; }
    /// <summary>
    /// Include子表的主表ReaderField引用
    /// </summary>
    public ReaderField Parent { get; set; }
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
    public Expression DeferredTarget { get; set; }
    /// <summary>
    /// 函数委托
    /// 先从数据库中获取字段，再调用函数，赋值指定字段中
    /// </summary>
    public MethodInfo DeferredMethod { get; set; }
    /// <summary>
    /// 字段查询后，执行函数调用的参数
    /// </summary>
    public List<Expression> DeferredArguments { get; set; }
    /// <summary>
    /// 是否是最外层目标类型，通常用判断第一个字段是否是参数访问，并且只有一个字段，可以有include操作
    /// </summary>
    public bool IsTargetType { get; set; }
    /// <summary>
    /// 最外层Select时，原参数访问的路径，如：.Select(x => new { Order = x, x.Seller.Company })中的x, x.Seller.Company
    /// 当有Include导航属性成员访问时，查找其主表在Select返回的实体中的属性值，构造延迟属性设置方法
    /// 此处获取Company表字段信息，在Order属性中已经存在了，直接取里面的值，不再查询数据库，只做延迟属性值设置
    /// </summary>
    public string Path { get; set; }
}
public enum ReaderFieldType : byte
{
    /// <summary>
    /// 字段
    /// </summary>
    Field = 1,
    /// <summary>
    /// 实体类型，三种场景：参数访问，直接主表的Include导航属性成员访问，Grouping分组对象成员，返回的类型是ReaderField列表
    /// </summary>
    Entity,
    /// <summary>
    /// Include子表引用，场景: .Select(x => new { Order = x, CompanyInfo = x.Buyer.Company })
    /// </summary>
    IncludeRef,
    /// <summary>
    /// 先从数据库中查询连续的一个或多个字段，再执行函数调用返回一个字段
    /// </summary>
    DeferredFields
}
