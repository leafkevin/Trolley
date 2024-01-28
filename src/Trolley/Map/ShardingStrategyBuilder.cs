using System;
using System.Linq.Expressions;

namespace Trolley;

public delegate string NameShardingStrategy(string orgTableName);
public delegate string TenantShardingStrategy<TTenantId>(string orgTableName, TTenantId tenantId);
public delegate string FieldShardingStrategy<TEntity>(string orgTableName, TEntity table);
public delegate string TimeShardingStrategy(string orgTableName, DateTime dateTime);
public delegate string TimeRangeShardingStrategy(string orgTableName, DateTime beginTime, DateTime endTime);
public delegate string ParameterShardingStrategy<TParameter>(string orgTableName, TParameter parameter = default);
public delegate string CustomShardingStrategy(string orgTableName, params object[] parameters);
public class ShardingStrategyBuilder<TEntity>
{
    private Delegate strategyRule;
    public string OrgTableName { get; private set; }

    public ShardingStrategyBuilder(string orgTableName)
        => this.OrgTableName = orgTableName;

    public ShardingStrategyBuilder<TEntity> UseTable(NameShardingStrategy tableNameGetter)
    {
        //this.strategyRule = shardingStrategy;
        return this;
    }
    public ShardingStrategyBuilder<TEntity> UseTenant<TTenantId>(TenantShardingStrategy<TTenantId> tableNameGetter)
    {
        //this.strategyRule = shardingStrategy;
        return this;
    }
    public ShardingStrategyBuilder<TEntity> UseTenant (TenantShardingStrategy<TEntity> tableNameGetter)
    {
        //this.strategyRule = shardingStrategy;
        return this;
    }
    public ShardingStrategyBuilder<TEntity> WithParameter<TParameter>(ParameterShardingStrategy<TParameter> tableNameGetter)
    {
        return this;
    }
    public ShardingStrategyBuilder<TEntity> WithTimeRangeRule(Expression<TimeRangeShardingStrategy> shardingStrategy)
    {
        return this;
    }
    public string Build()
    {
        return null;
    }
}
