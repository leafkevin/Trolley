using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace Trolley;

public class MemberBuilder<TMember>
{
    private MemberMap mapper;
    public MemberBuilder(MemberMap mapper) => this.mapper = mapper;
    public virtual MemberBuilder<TMember> Name(string memberName)
    {
        this.mapper.MemberName = memberName;
        return this;
    }
    public virtual MemberBuilder<TMember> Field(string fieldName)
    {
        this.mapper.FieldName = fieldName;
        return this;
    }
    public virtual MemberBuilder<TMember> DbType(DbType dbType)
    {
        this.mapper.DbType = dbType;
        return this;
    }
    public virtual MemberBuilder<TMember> NativeDbType(int nativeDbType)
    {
        this.mapper.NativeDbType = nativeDbType;
        return this;
    }
    public virtual MemberBuilder<TMember> Ignore()
    {
        this.mapper.IsIgnore = true;
        return this;
    }
    public virtual MemberBuilder<TMember> Navigate(string memberName)
    {
        this.mapper.IsNavigation = true;
        this.mapper.NavigationMemberName = memberName;
        if (this.mapper.MemberType.IsGenericType)
        {
            var targetType = this.mapper.MemberType.GetGenericArguments()[0];
            if (this.mapper.MemberType.IsAssignableFrom(typeof(IEnumerable<>).MakeGenericType(targetType)))
                throw new Exception("当前导航属性是List<T>,ICollection<T>,IEnumerable<T>类型才能使用本方法");

            var members = targetType.GetMember(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (members == null || members.Length <= 0)
                throw new Exception($"{targetType.FullName}类型不存在{memberName}的成员,memberName应为{targetType.FullName}类型的成员");

            this.mapper.NavigationTargetType = targetType;
            this.mapper.IsToOne = false;
        }
        else
        {
            var entityType = this.mapper.Parent.EntityType;
            var members = entityType.GetMember(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (members == null || members.Length <= 0)
                throw new Exception($"{entityType.FullName}类型不存在{memberName}的成员,memberName应为{entityType.FullName}类型的成员");
            this.mapper.NavigationTargetType = this.mapper.UnderlyingType;
            this.mapper.IsToOne = true;
        }
        return this;
    }
    //public virtual MemberBuilder<TMember> ValueObject<TTarget>(string memberName)
    //{
    //}
    public MemberMap Build() => this.mapper;
}
