using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Trolley;

public class EntityBuilder<TEntity> where TEntity : class
{
    private readonly EntityMap mapper;

    public EntityBuilder(EntityMap mapper) => this.mapper = mapper;

    public virtual EntityBuilder<TEntity> ToTable(string tableName)
    {
        this.mapper.TableName = tableName;
        return this;
    }
    public virtual EntityBuilder<TEntity> Key(params string[] propertyNames)
    {
        var properties = this.mapper.EntityType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        this.mapper.SetKeys(properties);
        return this;
    }
    public virtual EntityBuilder<TEntity> Key<TMember>(Expression<Func<TEntity, TMember>> keysExpr)
    {
        if (keysExpr.Body is NewExpression newExpr)
        {
            var memberInfos = newExpr.Arguments.Select(f => ((MemberExpression)f).Member).ToArray();
            this.mapper.SetKeys(memberInfos);
        }
        else if (keysExpr.Body is MemberExpression memberExpr)
            this.mapper.SetKeys(memberExpr.Member);
        else throw new Exception("不支持的Linq表达式");
        return this;
    }
    public virtual EntityBuilder<TEntity> AutoIncrement(string memberName)
    {
        var memberInfos = this.mapper.EntityType.GetMember(memberName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        this.mapper.SetAutoIncrement(memberInfos[0]);
        return this;
    }
    public virtual EntityBuilder<TEntity> AutoIncrement<TMember>(Expression<Func<TEntity, TMember>> memberExpr)
    {
        if (memberExpr.Body is MemberExpression memberVisitExpr)
            this.mapper.SetAutoIncrement(memberVisitExpr.Member);
        else throw new Exception("不支持的表达式");
        return this;
    }
    public virtual MemberBuilder<TMember> Member<TMember>(Expression<Func<TEntity, TMember>> memberSelector)
    {
        var memberExpr = memberSelector.Body as MemberExpression;
        if (memberExpr == null) throw new Exception("不支持的表达式");

        var memberName = memberExpr.Member.Name;
        if (!this.mapper.TryGetMemberMap(memberName, out var memberMapper))
            this.mapper.AddMemberMap(memberName, memberMapper = new MemberMap(this.mapper, memberExpr.Member));
        return new MemberBuilder<TMember>(memberMapper);
    }
    public virtual Navigation<TEntity> Navigation<TMember>(Expression<Func<TEntity, TMember>> memberSelector) where TMember : class
        => this.HasOne(memberSelector);
    public virtual Navigation<TElement> Navigation<TElement>(Expression<Func<TEntity, IEnumerable<TElement>>> memberSelector) where TElement : class
        => this.HasMany(memberSelector);
    public virtual Navigation<TEntity> HasOne<TMember>(Expression<Func<TEntity, TMember>> memberSelector) where TMember : class
    {
        var memberExpr = memberSelector.Body as MemberExpression;
        if (memberExpr == null) throw new Exception("不支持的表达式");

        var memberName = memberExpr.Member.Name;
        if (!this.mapper.TryGetMemberMap(memberName, out var memberMapper))
            this.mapper.AddMemberMap(memberName, memberMapper = new MemberMap(this.mapper, memberExpr.Member));

        memberMapper.IsNavigation = true;
        memberMapper.IsToOne = true;
        memberMapper.NavigationType = typeof(TMember);
        memberMapper.MapType = typeof(TMember);
        return new Navigation<TEntity>(memberMapper);
    }
    public virtual Navigation<TElement> HasMany<TElement>(Expression<Func<TEntity, IEnumerable<TElement>>> memberSelector) where TElement : class
    {
        var memberExpr = memberSelector.Body as MemberExpression;
        if (memberExpr == null) throw new Exception("不支持的表达式");

        var memberName = memberExpr.Member.Name;
        if (!this.mapper.TryGetMemberMap(memberName, out var memberMapper))
            this.mapper.AddMemberMap(memberName, memberMapper = new MemberMap(this.mapper, memberExpr.Member));

        memberMapper.IsNavigation = true;
        memberMapper.IsToOne = false;
        memberMapper.NavigationType = typeof(TElement);
        memberMapper.MapType = typeof(TElement);
        return new Navigation<TElement>(memberMapper);
    }
    /// <summary>
    /// 实体TEntity表，使用字段进行分表，如：.UseTable&lt;Order&gt;(f =&gt; f.TenantId, (origName, tenantId) =&gt; $"{origName}_{tenantId}")
    /// </summary>
    /// <param name="fieldsSelector">依赖字段获取委托</param>
    /// <param name="tableNameGetter">分表名获取委托</param>
    /// <returns></returns>
    public virtual EntityBuilder<TEntity> UseSharding(Expression<Func<TEntity, object>> fieldsSelector, Func<string, object, string> tableNameGetter)
    {
        return this;
    }
    /// <summary>
    /// 实体TEntity表，当满足条件condition时，使用字段进行分表，如：.UseTable&lt;Order&gt;(f =&gt; f.TenantId, (origName, tenantId) =&gt; $"{origName}_{tenantId}")
    /// </summary>
    /// <typeparam name="TEntity">表实体类型</typeparam>
    /// <param name="condition">分表条件委托，参数是dbKey</param>
    /// <param name="fieldsSelector">依赖字段获取委托</param>
    /// <param name="tableNameGetter">分表名获取委托</param>
    /// <returns></returns>
    public virtual EntityBuilder<TEntity> UseSharding(Func<string, bool> condition, Expression<Func<TEntity, object>> fieldsSelector, Func<string, object, string> tableNameGetter)
    {
        return this;
    }
}