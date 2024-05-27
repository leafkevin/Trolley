using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Trolley;

public class ShardingBuilder
{
    private IOrmDbFactory dbFactory;
    public ShardingBuilder(IOrmDbFactory dbFactory) => this.dbFactory = dbFactory;

    /// <summary>
    /// 配置分库dbKey获取委托，配置此委托后，使用未指定dbKey的IOrmDbFactory.CreateRepository方法创建每个Repository对象，都将调用此委托。如：
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
        shardingTable.DependOnMembers ??= new();
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
    public FieldShardingBuilder<TEntity, TField> UseRule(Func<string, string, TField, string> tableNameGetter, string validateRegex)
    {
        if (tableNameGetter == null)
            throw new ArgumentNullException(nameof(tableNameGetter));
        if (string.IsNullOrEmpty(validateRegex))
            throw new ArgumentNullException(nameof(validateRegex));

        var entityType = typeof(TEntity);
        if (!this.dbFactory.TryGetShardingTable(entityType, out var shardingTable))
            this.dbFactory.AddShardingTable(entityType, shardingTable = new ShardingTable());
        shardingTable.Rule = (string dbKey, string origName, object fieldValue) => tableNameGetter(dbKey, origName, (TField)fieldValue);
        shardingTable.ValidateRegex = validateRegex;
        return this;
    }
    public FieldShardingBuilder<TEntity, TField> UseRangeRule(Func<string, string, TField, TField, List<string>> tableNamesGetter)
    {
        if (tableNamesGetter == null)
            throw new ArgumentNullException(nameof(tableNamesGetter));

        var entityType = typeof(TEntity);
        if (!this.dbFactory.TryGetShardingTable(entityType, out var shardingTable))
            this.dbFactory.AddShardingTable(entityType, shardingTable = new ShardingTable());
        shardingTable.Rule = (string dbKey, string origName, object beginFieldValue, object endFieldValue) => tableNamesGetter(dbKey, origName, (TField)beginFieldValue, (TField)endFieldValue);
        return this;
    }
}
public class FieldShardingBuilder<TEntity, TField1, TField2>
{
    private IOrmDbFactory dbFactory;
    public FieldShardingBuilder(IOrmDbFactory dbFactory) => this.dbFactory = dbFactory;
    /// <summary>
    /// 设置分表名称命名规则和分表名称验证正则表达式
    /// </summary>
    /// <param name="tableNameGetter">分表名称获取委托</param>
    /// <param name="validateRegex"> 分表名称验证正则表达式，用于筛选分表名称</param>
    /// <returns></returns>
    public FieldShardingBuilder<TEntity, TField1, TField2> UseRule(Func<string, string, TField1, TField2, string> tableNameGetter, string validateRegex)
    {
        if (tableNameGetter == null)
            throw new ArgumentNullException(nameof(tableNameGetter));
        if (string.IsNullOrEmpty(validateRegex))
            throw new ArgumentNullException(nameof(validateRegex));

        var entityType = typeof(TEntity);
        if (!this.dbFactory.TryGetShardingTable(entityType, out var shardingTable))
            this.dbFactory.AddShardingTable(entityType, shardingTable = new ShardingTable());
        shardingTable.Rule = (string dbKey, string origName, object field1Value, object field2Value) => tableNameGetter(dbKey, origName, (TField1)field1Value, (TField2)field2Value);
        shardingTable.ValidateRegex = validateRegex;
        return this;
    }
    public FieldShardingBuilder<TEntity, TField1, TField2> UseRangeRule(Func<string, string, TField1, TField2, TField2, List<string>> tableNamesGetter)
    {
        if (tableNamesGetter == null)
            throw new ArgumentNullException(nameof(tableNamesGetter));

        var entityType = typeof(TEntity);
        if (!this.dbFactory.TryGetShardingTable(entityType, out var shardingTable))
            this.dbFactory.AddShardingTable(entityType, shardingTable = new ShardingTable());
        shardingTable.RangleRule = (string dbKey, string origName, object beginField1Value, object beginField2Value, object endField2Value) => tableNamesGetter(dbKey, origName, (TField1)beginField1Value, (TField2)beginField2Value, (TField2)endField2Value);
        return this;
    }
    public FieldShardingBuilder<TEntity, TField1, TField2> UseRangeRule(Func<string, string, TField1, TField1, TField2, List<string>> tableNamesGetter)
    {
        if (tableNamesGetter == null)
            throw new ArgumentNullException(nameof(tableNamesGetter));

        var entityType = typeof(TEntity);
        if (!this.dbFactory.TryGetShardingTable(entityType, out var shardingTable))
            this.dbFactory.AddShardingTable(entityType, shardingTable = new ShardingTable());
        shardingTable.RangleRule = (string dbKey, string origName, object beginField1Value, object endField1Value, object beginField2Value) => tableNamesGetter(dbKey, origName, (TField1)beginField1Value, (TField1)endField1Value, (TField2)beginField2Value);
        return this;
    }
}