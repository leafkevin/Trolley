using System;
using System.Collections.Generic;
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
    /// 实体TEntity表，使用字段进行分表，如：.UseTable&lt;Order&gt;(f =&gt; f.TenantId, (origName, tenantId) =&gt; $"{origName}_{tenantId}")
    /// </summary>
    /// <typeparam name="TEntity">表实体类型</typeparam>
    /// <param name="fieldsSelector">依赖字段获取委托</param>
    /// <param name="tableNameGetter">分表名获取委托</param>
    /// <returns></returns>
    public ShardingStrategyBuilder UseTable<TEntity>(Expression<Func<TEntity, object>> fieldsSelector, Func<string, object, string> tableNameGetter)
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
    public ShardingStrategyBuilder UseTable<TEntity>(Func<string, bool> condition, Expression<Func<TEntity, object>> fieldsSelector, Func<string, object, string> tableNameGetter)
    {
        return this;
    }
    public void Build()
    {

    }
}