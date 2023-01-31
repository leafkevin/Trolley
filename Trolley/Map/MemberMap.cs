using System;
using System.Reflection;

namespace Trolley;

public class MemberMap
{
    public EntityMap Parent { get; set; }
    public MemberInfo Member { get; set; }
    public string MemberName { get; set; }
    public Type MemberType { get; set; }
    public bool IsNullable { get; set; }
    public Type UnderlyingType { get; set; }
    public bool IsEnum { get; set; }
    public Type EnumUnderlyingType { get; set; }
    public bool IsKey { get; set; }
    public bool IsAutoIncrement { get; set; }
    public string FieldName { get; set; }
    public int? NativeDbType { get; set; }
    public bool IsIgnore { get; set; }

    public bool IsNavigation { get; set; }
    /// <summary>
    /// 如果是1:1关系，导航属性类型和成员类型MemberType一致，如果是1:N关系，导航属性类型和成员类型的元素类型ElementType一致
    /// </summary>
    public Type NavigationType { get; set; }
    /// <summary>
    /// 当前值对象映射的实体模型Type
    /// </summary>
    public Type MapType { get; set; }
    public bool IsToOne { get; set; }
    /// <summary>
    /// 外键成员，是类属性
    /// </summary>
    public string ForeignKey { get; set; }

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
        this.UnderlyingType = this.MemberType;
        if (this.MemberType.IsValueType)
        {
            var underlyingType = Nullable.GetUnderlyingType(this.MemberType);
            this.IsNullable = underlyingType != null;
            if (this.IsNullable)
                this.UnderlyingType = underlyingType;
        }
        this.IsEnum = this.UnderlyingType.IsEnum;
        if (this.IsEnum)
            this.EnumUnderlyingType = this.UnderlyingType.GetEnumUnderlyingType();
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
        result.UnderlyingType = result.MemberType;
        if (result.MemberType.IsValueType)
        {
            var underlyingType = Nullable.GetUnderlyingType(result.MemberType);
            result.IsNullable = underlyingType != null;
            if (result.IsNullable)
                result.UnderlyingType = underlyingType;
        }
        result.IsEnum = result.UnderlyingType.IsEnum;
        if (result.IsEnum)
            result.EnumUnderlyingType = result.UnderlyingType.GetEnumUnderlyingType();
        result.TypeHandler = this.TypeHandler;
        return result;
    }
}
