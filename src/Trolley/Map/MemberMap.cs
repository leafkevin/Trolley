using System;
using System.Reflection;

namespace Trolley;

public class MemberMap
{
    public EntityMap Parent { get; set; }
    public MemberInfo Member { get; set; }
    public string MemberName { get; set; }
    public Type MemberType { get; set; }
    public Type UnderlyingType { get; set; }
    public bool IsKey { get; set; }
    public bool IsAutoIncrement { get; set; }
    public string FieldName { get; set; }
    /// <summary>
    /// 数据库原始类型，如：varchar(50),time(6)等
    /// </summary>
    public string DbColumnType { get; set; }
    public object NativeDbType { get; set; }
    public int Position { get; set; }
    public int Length { get; set; }
    public bool IsIgnore { get; set; }
    public bool IsRequired { get; set; }
    public bool IsRowVersion { get; set; }

    public bool IsNavigation { get; set; }
    /// <summary>
    /// 如果是1:1关系，导航属性类型和成员类型MemberType一致，如果是1:N关系，导航属性类型和成员类型的元素类型ElementType一致
    /// </summary>
    public Type NavigationType { get; set; }
    /// <summary>
    /// 当前属性是导航属性，映射的实体类不是真实的模型，瘦身版的模型Type，
    /// 与真正的模型属性名都一样，只是属性较少
    /// </summary>
    public Type MapType { get; set; }
    /// <summary>
    /// 是否是1:1关系
    /// </summary>
    public bool IsToOne { get; set; }
    /// <summary>
    /// 外键成员，是类属性
    /// </summary>
    public string ForeignKey { get; set; }
    /// <summary>
    /// 类型处理器
    /// </summary>
    public ITypeHandler TypeHandler { get; set; }
    public Type TypeHandlerType { get; set; }

    public MemberMap(EntityMap parent, MemberInfo memberInfo)
    {
        this.Parent = parent;
        this.Member = memberInfo;
        this.FieldName = memberInfo.Name;
        this.MemberName = memberInfo.Name;
        this.MemberType = memberInfo.GetMemberType();
        this.UnderlyingType = Nullable.GetUnderlyingType(this.MemberType) ?? this.MemberType;
    }
    public MemberMap Clone(EntityMap parent, MemberInfo memberInfo)
    {
        var result = new MemberMap(parent, memberInfo);
        result.Parent = parent;
        result.Member = memberInfo;
        result.FieldName = this.FieldName;
        result.MemberName = memberInfo.Name;
        result.MemberType = memberInfo.GetMemberType();
        result.NativeDbType = this.NativeDbType;
        result.UnderlyingType = this.UnderlyingType;
        result.TypeHandler = this.TypeHandler;
        return result;
    }
}
