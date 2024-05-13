using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Trolley;

public class ShardingBuilder
{
    private IOrmDbFactory dbFactory;
    public ShardingBuilder(IOrmDbFactory dbFactory) => this.dbFactory = dbFactory;

    /// <summary>
    /// 配置获取dbKey委托，可以使用租户，也可以映射表，或是指定的规则，如：
    /// <code>
    /// .UseDatabase(() =&gt;
    /// {
    ///     var passport = f.GetService&lt;IPassport&gt;();
    ///     return passport.TenantId switch
    ///     {
    ///         200 =&gt; "dbKey1",
    ///         300 =&gt; "dbKey2",
    ///         _ =&gt; "defaultDbKey"
    ///     }
    /// });
    /// </code>
    /// </summary>
    /// <param name="dbKeySelector">dbKey获取委托</param>
    /// <returns></returns>
    public ShardingBuilder UseDatabase(Func<string> dbKeySelector)
    {
        this.dbFactory.UseDatabase(dbKeySelector);
        return this;
    }
    public ShardingBuilder UseTable<TEntity>(Action<TableShardingBuilder<TEntity>> shardingInitializer)
    {
        if (shardingInitializer == null)
            throw new ArgumentNullException(nameof(shardingInitializer));

        var builder = new TableShardingBuilder<TEntity>(this.dbFactory);
        shardingInitializer.Invoke(builder);
        return this;
    }
}

public class TableShardingBuilder<TEntity>
{
    private IOrmDbFactory dbFactory;
    public TableShardingBuilder(IOrmDbFactory dbFactory) => this.dbFactory = dbFactory;
    public FieldShardingBuilder<TEntity, TField> DependOn<TField>(Expression<Func<TEntity, TField>> fieldSelector)
    {
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式，多个字段可以多次使用DependOn方法，最多支持两个字段");

        var memberExpr = fieldSelector.Body as MemberExpression;
        var memberName = memberExpr.Member.Name;
        var entityType = typeof(TEntity);
        if (!this.dbFactory.TryGetShardingTable(entityType, out var shardingTable))
            this.dbFactory.AddShardingTable(entityType, shardingTable = new ShardingTable());
        shardingTable.DependOnMembers.Add(memberName);

        return new FieldShardingBuilder<TEntity, TField>(this.dbFactory);
    }
}
public class FieldShardingBuilder<TEntity, TField>
{
    private IOrmDbFactory dbFactory;
    public FieldShardingBuilder(IOrmDbFactory dbFactory) => this.dbFactory = dbFactory;
    public FieldShardingBuilder<TEntity, TField, TField2> DependOn<TField2>(Expression<Func<TEntity, TField2>> fieldSelector)
    {
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式，多个字段可以多次使用DependOn方法，最多支持两个字段");

        var memberExpr = fieldSelector.Body as MemberExpression;
        var memberName = memberExpr.Member.Name;
        var entityType = typeof(TEntity);
        if (!this.dbFactory.TryGetShardingTable(entityType, out var shardingTable))
            this.dbFactory.AddShardingTable(entityType, shardingTable = new ShardingTable());
        shardingTable.DependOnMembers.Add(memberName);

        return new FieldShardingBuilder<TEntity, TField, TField2>(this.dbFactory);
    }
    public FieldShardingBuilder<TEntity, TField> UseRule(Func<string, string, TField, string> tableNameGetter)
    {
        var entityType = typeof(TEntity);
        if (!this.dbFactory.TryGetShardingTable(entityType, out var shardingTable))
            this.dbFactory.AddShardingTable(entityType, shardingTable = new ShardingTable());
        shardingTable.Rule = (string dbKey, string origName, object fieldValue) => tableNameGetter(origName, dbKey, (TField)fieldValue);
        return this;
    }
    public FieldShardingBuilder<TEntity, TField> UseRangeRule(Func<string, string, TField, TField, List<string>> tableNamesGetter)
    {
        var entityType = typeof(TEntity);
        if (!this.dbFactory.TryGetShardingTable(entityType, out var shardingTable))
            this.dbFactory.AddShardingTable(entityType, shardingTable = new ShardingTable());
        shardingTable.Rule = (string dbKey, string origName, object beginFieldValue, object endFieldValue) => tableNamesGetter(origName, dbKey, (TField)beginFieldValue, (TField)endFieldValue);
        return this;
    }
}
public class FieldShardingBuilder<TEntity, TField1, TField2>
{
    private IOrmDbFactory dbFactory;
    public FieldShardingBuilder(IOrmDbFactory dbFactory) => this.dbFactory = dbFactory;
    public FieldShardingBuilder<TEntity, TField1, TField2> UseRule(Func<string, string, TField1, TField2, string> tableNameGetter)
    {
        var entityType = typeof(TEntity);
        if (!this.dbFactory.TryGetShardingTable(entityType, out var shardingTable))
            this.dbFactory.AddShardingTable(entityType, shardingTable = new ShardingTable());
        shardingTable.Rule = (string dbKey, string origName, object field1Value, object field2Value) => tableNameGetter(origName, dbKey, (TField1)field1Value, (TField2)field2Value);
        return this;
    }
    public FieldShardingBuilder<TEntity, TField1, TField2> UseRangeRule(Func<string, string, TField1, TField2, TField2, List<string>> tableNamesGetter)
    {
        var entityType = typeof(TEntity);
        if (!this.dbFactory.TryGetShardingTable(entityType, out var shardingTable))
            this.dbFactory.AddShardingTable(entityType, shardingTable = new ShardingTable());
        shardingTable.RangleRule = (string dbKey, string origName, object beginField1Value, object beginField2Value, object endField2Value) => tableNamesGetter(origName, dbKey, (TField1)beginField1Value, (TField2)beginField2Value, (TField2)endField2Value);
        return this;
    }
    public FieldShardingBuilder<TEntity, TField1, TField2> UseRangeRule(Func<string, string, TField1, TField1, TField2, List<string>> tableNamesGetter)
    {
        var entityType = typeof(TEntity);
        if (!this.dbFactory.TryGetShardingTable(entityType, out var shardingTable))
            this.dbFactory.AddShardingTable(entityType, shardingTable = new ShardingTable());
        shardingTable.RangleRule = (string dbKey, string origName, object beginField1Value, object endField1Value, object beginField2Value) => tableNamesGetter(origName, dbKey, (TField1)beginField1Value, (TField1)endField1Value, (TField2)beginField2Value);
        return this;
    }
}