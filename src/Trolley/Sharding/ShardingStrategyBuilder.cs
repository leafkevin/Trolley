using System;
using System.Linq.Expressions;

namespace Trolley;

public class ShardingStrategyBuilder
{
    private OrmDbFactory dbFactory;
    public ShardingStrategyBuilder(OrmDbFactory dbFactory) => this.dbFactory = dbFactory;

    /// <summary>
    /// 配置获取dbKey委托，可以使用租户，也可以映射表，或是指定的规则，如：.UseDbKey(() =&gt;
    /// {
    ///     var passport = f.GetService&lt;IPassport&gt;();
    ///     return passport.TenantId switch
    ///     {
    ///         200 =&gt; "dbKey1",
    ///         300 =&gt; "dbKey2",
    ///         _ =&gt; "defaultDbKey"
    ///     }
    /// });
    /// </summary>
    /// <param name="dbKeySelector">dbKey获取委托</param>
    /// <returns></returns>
    public ShardingStrategyBuilder UseDbKey(Func<string> dbKeySelector)
    {
        this.dbFactory.SetDbKeySelector(dbKeySelector);
        return this;
    }
    /// <summary>
    /// 使用依赖字段分表，支持各种场景，只支持依赖单个字段分表，但可以有多个字段用于CRUD查询，如：.UseTable&lt;Order&gt;(f =&gt; f.CreatedAt, (origName, createdAt) =&gt; $"{origName}_{createdAt:yyyyMM}")
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="fieldsSelector">依赖字段获取委托</param>
    /// <param name="tableNameGetter">分表名获取委托</param>
    /// <param name="isRequired">是否是必须栏位，如果为true，在Insert、Update、Delete场景会使用</param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public ShardingStrategyBuilder UseTable<TEntity>(Expression<Func<TEntity, object>> fieldsSelector, Func<string, object, string> tableNameGetter, bool isRequired = false)
    {
        if (fieldsSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldsSelector)},只支持MemberAccess类型表达式，单个字段分表");
        var memberExpr = fieldsSelector.Body as MemberExpression;
        var memberName = memberExpr.Member.Name;
        return this;
    }
    public ShardingStrategyBuilder UseTableIf<TEntity>(Func<string, bool> condition, Expression<Func<TEntity, object>> fieldsSelector, Func<string, object, string> tableNameGetter, bool isRequired = false)
    {
        return this;
    }
    public void Build()
    {

    }
}