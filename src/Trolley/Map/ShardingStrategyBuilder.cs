using System;
using System.Linq.Expressions;

namespace Trolley;

public delegate string NameShardingStrategy(string orgTableName);
public delegate string FieldShardingStrategy<TEntity>(string orgTableName, TEntity table);
public delegate string TimeShardingStrategy(string orgTableName, DateTime dateTime);
public delegate string TimeRangeShardingStrategy(string orgTableName, DateTime beginTime, DateTime endTime);
public delegate string ParameterShardingStrategy<TParameter>(string orgTableName, TParameter parameter = default);
public delegate string CustomShardingStrategy(string orgTableName, params object[] parameters);
public class ShardingStrategyBuilder
{
    private Delegate strategyRule;
    public string OrgTableName { get; private set; }

    public ShardingStrategyBuilder(string orgTableName)
        => this.OrgTableName = orgTableName;

    public ShardingStrategyBuilder WithNameRule(Expression<NameShardingStrategy> shardingStrategy)
    {
        //this.strategyRule = shardingStrategy;
        return this;
    }
    public ShardingStrategyBuilder WithTimeRule(Expression<TimeShardingStrategy> shardingStrategy)
    {
        return this;
    }
    public ShardingStrategyBuilder WithTimeRangeRule(Expression<TimeRangeShardingStrategy> shardingStrategy)
    {
        return this;
    }
    public string Build()
    {
        return null;
    }
}
