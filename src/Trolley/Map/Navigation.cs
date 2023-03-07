using System;
using System.Linq.Expressions;

namespace Trolley;

public class Navigation<TEntity> where TEntity : class
{
    private readonly MemberMap memberMapper;
    public Navigation(MemberMap memberMapper) => this.memberMapper = memberMapper;
    public Navigation<TEntity> HasForeignKey<TNavigation>(Expression<Func<TEntity, TNavigation>> MemberSelector)
    {
        var memberExpr = MemberSelector.Body as MemberExpression;
        memberMapper.ForeignKey = memberExpr.Member.Name;
        return this;
    }
    public void MapTo<TModel>() => this.memberMapper.MapType = typeof(TModel);
    public void MapTo(Type mapType) => this.memberMapper.MapType = mapType;
}
