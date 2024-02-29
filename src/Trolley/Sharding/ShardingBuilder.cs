using System;
using System.Collections.Generic;

namespace Trolley;

public class ShardingBuilder
{
    private OrmDbFactory dbFactory;
    public ShardingBuilder(OrmDbFactory dbFactory) => this.dbFactory = dbFactory;

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
    public ShardingBuilder UseDbKey(Func<string> dbKeySelector)
    {
        this.dbFactory.SetDbKeySelector(dbKeySelector);
        return this;
    }
    public ShardingBuilder UseTable<TEntity>(Action<TableShardingBuilder<TEntity>> shardingInitializer)
    {
        //if (fieldsSelector.Body.NodeType != ExpressionType.MemberAccess)
        //    throw new NotSupportedException($"不支持的表达式{nameof(fieldsSelector)},只支持MemberAccess类型表达式，单个字段分表");
        //var memberExpr = fieldsSelector.Body as MemberExpression;
        //var memberName = memberExpr.Member.Name;
        return this;
    }
    public ShardingBuilder UseTable<TEntity>(Func<string, bool> condition, Action<TableShardingBuilder<TEntity>> shardingInitializer)
    {
        //if (fieldsSelector.Body.NodeType != ExpressionType.MemberAccess)
        //    throw new NotSupportedException($"不支持的表达式{nameof(fieldsSelector)},只支持MemberAccess类型表达式，单个字段分表");
        //var memberExpr = fieldsSelector.Body as MemberExpression;
        //var memberName = memberExpr.Member.Name;
        return this;
    }
    public void Build()
    {

    }
}

public class TableShardingBuilder<TEntity>
{
    public FieldShardingBuilder<TEntity, TField> DependOn<TField>(Func<TEntity, TField> fieldSelector)
    {
        return null;
    }
    public RangeShardingBuilder<TEntity, DateTime> DependOnTime()
    {
        return null;
    }
}
public class RangeShardingBuilder<TEntity, TRange>
{
    public FieldRangeShardingBuilder<TEntity, TField, TRange> DependOn<TField>(Func<TEntity, TField> fieldSelector)
    {
        return null;
    }
    public RangeShardingBuilder<TEntity, TRange> Use(Func<string, TRange, string> tableNameGetter)
    {
        return this;
    }
    public RangeShardingBuilder<TEntity, TRange> UseRange(Func<string, TRange, TRange, List<string>> tableNamesGetter)
    {
        return this;
    }
}
public class FieldShardingBuilder<TEntity, TField>
{
    public FieldRangeShardingBuilder<TEntity, TField, DateTime> DependOnTime()
    {
        return null;
    }
    public FieldShardingBuilder<TEntity, TField> Use(Func<string, TField, string> tableNameGetter)
    {
        return this;
    }
    public FieldShardingBuilder<TEntity, TField> UseRange<TRange>(Func<string, TRange, TRange, List<string>> tableNameGetter)
    {
        return this;
    }
}
public class FieldRangeShardingBuilder<TEntity, TField, TRange>
{
    public FieldRangeShardingBuilder<TEntity, TField, TRange> Use(Func<string, TField, TRange, string> tableNameGetter)
    {
        return this;
    }
    public FieldRangeShardingBuilder<TEntity, TField, TRange> UseRange(Func<string, TField, TRange, TRange, List<string>> tableNamesGetter)
    {
        return this;
    }
}