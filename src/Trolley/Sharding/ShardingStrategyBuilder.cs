using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Trolley;

public class ShardingStrategyBuilder
{
    private Dictionary<Type, object> tableShardingRules = new Dictionary<Type, object>();
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
    /// SQL查询中包含了fieldsSelector指定的字段，表TEntity将使用tableNameGetter获取分表名称，这个规则可用于查询、插入、更新、删除语句中
    /// </summary>
    /// <typeparam name="TFields">用于判断分表的依赖字段类型</typeparam>
    /// <param name="fieldsSelector">用于判断分表的依赖字段</param>
    /// <param name="tableNameGetter"></param>
    /// <returns></returns>
    public ShardingTableStrategyBuilder<TEntity> DependOn<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector, Func<string, TFields, string> tableNameGetter)
    {
        return this;
    }
    /// <summary>
    /// 可以用来CRUD操作，建议此栏位在表中直接存在，而不是经过计算得到的栏位值
    /// </summary>
    /// <param name="isRequired">在进行插入、更新、删除时，是否是必须栏位</param>
    /// <returns></returns>
    public ShardingTableStrategyBuilder<TEntity> UseCrud(bool isRequired)
    {
        return this;
    }
    /// <summary>
    /// 仅用于查询操作
    /// </summary>
    /// <returns></returns>
    public ShardingTableStrategyBuilder<TEntity> OnlyQuery()
    {
        return this;
    }
}