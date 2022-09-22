using System;
using System.Data;
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
    public DbType DbType { get; set; }
    public int? NativeDbType { get; set; }
    public bool IsIgnore { get; set; }

    public bool IsNavigation { get; set; }
    public string NavigationTable { get; set; }
    public Type NavigationTargetType { get; set; }
    public bool IsToOne { get; set; }
    public string NavigationMemberName { get; set; }
    public MemberMap(MemberInfo memberInfo) : this(null, string.Empty, memberInfo) { }
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
                //this.GetMethodInfo = propertyInfo.GetGetMethod();
                //this.SetMethodInfo = propertyInfo.GetSetMethod();
                break;
        }
        this.DbType = DbTypeMap.FindDbType(this.MemberType);
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
}
