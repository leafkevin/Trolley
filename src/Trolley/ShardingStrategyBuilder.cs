using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Trolley;

public class ShardingStrategyBuilder
{
    private Dictionary<Type, object> tableShardingRules = new Dictionary<Type, object>();
    /// <summary>
    /// 配置获取dbKey委托，可以使用租户，也可以映射表，或是指定的规则，如：.UseDbKey(f =&gt;
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
    /// <param name="dbKeyGetter">dbKey获取委托</param>
    /// <returns></returns>
    public ShardingStrategyBuilder UseDbKey(Func<string> dbKeyGetter)
    {
        return this;
    }
    /// <summary>
    /// 实体TEntity表，使用字段进行分表，如：.UseTable&lt;Order&gt;((f, orgName) =&gt; $"{orgName}_{f.TenantId}")
    /// </summary>
    /// <typeparam name="TEntity">表实体类型</typeparam>
    /// <param name="shardingInitializer">分表名获取委托</param>
    /// <returns></returns>
    public ShardingStrategyBuilder UseTable<TEntity>(Action<ShardingTableStrategyBuilder<TEntity>> shardingInitializer)
    {
        return this;
    }
    public ShardingStrategyBuilder UseTableIf<TEntity>(Func<string, bool> dbKeyCondition, Action<ShardingTableStrategyBuilder<TEntity>> shardingInitializer)
    {
        return this;
    }
    public void Build()
    {

    }
}
public class ShardingTableStrategyBuilder<TEntity>
{
    /// <summary>
    /// SQL查询中包含了dependentFields指定的字段，表TEntity将使用tableNameGetter获取分表名称，这个规则可用于查询、插入、更新、删除语句中
    /// </summary>
    /// <typeparam name="TFields">用于判断分表的依赖字段类型</typeparam>
    /// <param name="dependentFields">用于判断分表的依赖字段</param>
    /// <param name="tableNameGetter"></param>
    /// <returns></returns>
    public ShardingTableStrategyBuilder<TEntity> DependOn<TFields>(Expression<Func<TEntity, TFields>> dependentFields, Func<string, TFields, string> tableNameGetter)
    {
        return this;
    }
    /// <summary>
    /// 可以用来CRUD操作
    /// </summary>
    /// <param name="isUnique">是否可以确定唯一</param>
    /// <returns></returns>
    public ShardingTableStrategyBuilder<TEntity> UseCrud(bool isUnique)
    {
        return this;
    }
    /// <summary>
    /// 仅用于查询操作
    /// </summary>
    /// <param name="isFuzzyMatching">是否模糊匹配</param>
    /// <returns></returns>
    public ShardingTableStrategyBuilder<TEntity> OnlyQuery(bool isFuzzyMatching)
    {
        return this;
    }
}