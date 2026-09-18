using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Trolley;

public class TableShardingBuilder
{
    private readonly ITableShardingProvider shardingProvider;
    public TableShardingBuilder(ITableShardingProvider tableShardingProvider)
        => this.shardingProvider = tableShardingProvider;

    /// <summary>
    /// 为指定实体表设置分表规则，通常用于配置分表的依赖字段和分表名称规则。
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="shardingInitializer">分表规则初始化器</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public TableShardingBuilder Table<TEntity>(Action<TableShardingBuilder<TEntity>> shardingInitializer)
    {
        if (shardingInitializer == null)
            throw new ArgumentNullException(nameof(shardingInitializer));

        var builder = new TableShardingBuilder<TEntity>(this.shardingProvider);
        shardingInitializer.Invoke(builder);
        return this;
    }
}
public class TableShardingBuilder<TEntity>
{
    private readonly Type entityType = typeof(TEntity);
    private readonly ITableShardingProvider shardingProvider = null;
    private readonly TableShardingInfo shardingTableInfo = null;
    public TableShardingBuilder(ITableShardingProvider tableShardingProvider)
    {
        this.shardingProvider = tableShardingProvider;
        if (!shardingProvider.TryGetTableSharding(entityType, out this.shardingTableInfo))
        {
            this.shardingProvider.AddTableSharding(entityType, this.shardingTableInfo = new TableShardingInfo
            {
                EntityType = entityType
            });
        }
    }

    /// <summary>
    /// 设置分表依赖的字段，在未明确指定分表时，将使用依赖字段来确定分表名，如：.DependOn(x => new { x.TenantId, x.CreatedAt})
    /// </summary>
    /// <typeparam name="TFields">字段类型</typeparam>
    /// <param name="fieldSelector">字段选择器</param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public TableShardingBuilder<TEntity> DependOn<TFields>(Expression<Func<TEntity, TFields>> fieldSelector)
    {
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式，多个字段可以多次使用DependOn方法，最多支持两个字段");

        var memberExpr = fieldSelector.Body as MemberExpression;
        var memberName = memberExpr.Member.Name;
        this.shardingTableInfo.DependOnMembers ??= new();
        this.shardingTableInfo.DependOnMembers.Add(memberName);
        return this;
    }
    /// <summary>
    /// 设置分表规则，不依赖任何参数值和字段值，只用于读写分表。
    /// </summary>
    /// <param name="usageMode">读写模式</param>
    /// <param name="tableNameGetter">分表名获取委托</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public TableShardingBuilder<TEntity> UseRule(TableUsageMode usageMode, Func<string, string> tableNameGetter)
    {
        if (tableNameGetter == null)
            throw new ArgumentNullException(nameof(tableNameGetter));

        var tableRuleInfo = this.GetTableRule(usageMode);
        tableRuleInfo.Rule = (tableName, values) => tableNameGetter.Invoke(tableName);
        return this;
    }
    /// <summary>
    /// 设置分表规则和分表名称验证正则表达式，需要提供额外依赖参数值才能获取分表名称，需要手动传入参数值，可用于时间、商户...+读写复杂分表，必须提供分表名验证规则
    /// </summary>
    /// <param name="usageMode">读写模式</param>
    /// <param name="tableNameGetter">分表名获取委托</param>
    /// <param name="validateRegex">分表名验证规则</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public TableShardingBuilder<TEntity> UseRule(TableUsageMode usageMode, Func<string, object[], string> tableNameGetter, string validateRegex)
    {
        if (tableNameGetter == null)
            throw new ArgumentNullException(nameof(tableNameGetter));
        if (string.IsNullOrEmpty(validateRegex))
            throw new ArgumentNullException(nameof(validateRegex));

        var tableRuleInfo = this.GetTableRule(usageMode);
        tableRuleInfo.Rule = tableNameGetter;
        tableRuleInfo.ValidateRegex = validateRegex;
        return this;
    }
    /// <summary>
    /// 设置分表范围规则，通常用于时间范围查询，需要手动传入字段值数组来获取分表名称，数组的最后两个值是范围的起始和结束栏位，要先设置分表规则后再设置范围规则。如：
    /// .UseRangeRule((origName, parameters) =>
    /// {
    ///     var tenantId = parameters[0] as string;
    ///     var beginTime = parameters[1] as DateTime;
    ///     var endTime = parameters[2] as DateTime;
    ///     var tableNames = new List&lt;string&gt;();
    ///     var current = beginTime.AddDays(1 - beginTime.Day);
    ///     while (current <= endTime)
    ///     {
    ///         var tableName = $"{origName}_{tenantId}_{current:yyyyMM}";
    ///         if (tableNames.Contains(tableName))
    ///         {
    ///             current = current.AddMonths(1);
    ///             continue;
    ///         }
    ///         tableNames.Add(tableName);
    ///         current = current.AddMonths(1);
    ///     }
    ///     return tableNames;
    /// }))
    /// </summary>
    /// <param name="tableNamesGetter">分表名获取委托</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public TableShardingBuilder<TEntity> UseRangeRule(TableUsageMode usageMode, Func<string, object[], List<string>> tableNamesGetter)
    {
        if (tableNamesGetter == null)
            throw new ArgumentNullException(nameof(tableNamesGetter));
        var tableRuleInfo = this.GetTableRule(usageMode);
        tableRuleInfo.RangleRule = tableNamesGetter;
        return this;
    }
    private TableRuleInfo GetTableRule(TableUsageMode usageMode)
    {
        TableRuleInfo tableRuleInfo = null;
        if (usageMode == TableUsageMode.Default)
        {
            if (!this.shardingProvider.TryGetTableRule(this.entityType, TableUsageMode.WriteOnly, out tableRuleInfo))
                this.shardingProvider.AddTableRule(this.entityType, TableUsageMode.WriteOnly, tableRuleInfo = new TableRuleInfo());
            this.shardingProvider.AddTableRule(this.entityType, TableUsageMode.ReadOnly, tableRuleInfo);
        }
        else
        {
            if (!this.shardingProvider.TryGetTableRule(this.entityType, usageMode, out tableRuleInfo))
                this.shardingProvider.AddTableRule(this.entityType, usageMode, tableRuleInfo = new TableRuleInfo());
        }
        return tableRuleInfo;
    }
}