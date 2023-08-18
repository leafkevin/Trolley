using System;
using System.Reflection;

namespace Trolley;

public class MemberMap
{
    public EntityMap Parent { get; set; }
    public MemberInfo Member { get; set; }
    public string MemberName { get; set; }
    public Type MemberType { get; set; }
    public bool IsKey { get; set; }
    public bool IsAutoIncrement { get; set; }
    public string FieldName { get; set; }
    public object NativeDbType { get; set; }
    public Type DbDefaultType { get; set; }
    public int Length { get; set; }
    public bool IsIgnore { get; set; }

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

    public MemberMap(EntityMap parent, string fieldPrefix, MemberInfo memberInfo)
    {
        this.Parent = parent;
        this.Member = memberInfo;
        this.FieldName = $"{fieldPrefix}{memberInfo.Name}";
        this.MemberName = memberInfo.Name;
        switch (memberInfo.MemberType)
        {
            case MemberTypes.Field:
                var fieldInfo = memberInfo as FieldInfo;
                this.MemberType = fieldInfo.FieldType;
                break;
            case MemberTypes.Property:
                var propertyInfo = memberInfo as PropertyInfo;
                this.MemberType = propertyInfo.PropertyType;
                break;
        }
    }
    public MemberMap Clone(EntityMap parent, string fieldPrefix, MemberInfo memberInfo)
    {
        var result = new MemberMap(parent, fieldPrefix, memberInfo);
        result.Parent = parent;
        result.Member = memberInfo;
        result.FieldName = this.FieldName;
        result.MemberName = memberInfo.Name;
        switch (memberInfo.MemberType)
        {
            case MemberTypes.Field:
                var fieldInfo = memberInfo as FieldInfo;
                result.MemberType = fieldInfo.FieldType;
                break;
            case MemberTypes.Property:
                var propertyInfo = memberInfo as PropertyInfo;
                result.MemberType = propertyInfo.PropertyType;
                break;
        }
        result.NativeDbType = this.NativeDbType;
        result.TypeHandler = this.TypeHandler;
        return result;
    }
}
