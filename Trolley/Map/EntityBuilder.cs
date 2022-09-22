using System;
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
    public virtual EntityBuilder<TEntity> FieldPrefix(string fieldPrefix)
    {
        this.mapper.FieldPrefix = fieldPrefix;
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
    public virtual MemberBuilder<TMember> Member<TMember>(Expression<Func<TEntity, TMember>> memberExpr)
    {
        var memberVisitExpr = memberExpr.Body as MemberExpression;
        if (memberVisitExpr == null)
            throw new Exception("不支持的表达式");

        var memberName = memberVisitExpr.Member.Name;
        if (!this.mapper.TryGetMemberMap(memberName, out var memberMapper))
            this.mapper.AddMemberMap(memberName, memberMapper = new MemberMap(this.mapper, this.mapper.FieldPrefix, memberVisitExpr.Member));
        return new MemberBuilder<TMember>(memberMapper);
    }
    public EntityMap Build() => this.mapper.Build();
}