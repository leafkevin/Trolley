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
    private readonly ITableShardingProvider shardingProvider;
    public TableShardingBuilder(ITableShardingProvider tableShardingProvider)
        => this.shardingProvider = tableShardingProvider;

    /// <summary>
    /// 设置第一个分表依赖的字段，后续的分表规则会依赖这个字段的值来进行分表，不需要手动设置分表，最多支持两个依赖字段
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldSelector">字段选择器</param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public FieldShardingBuilder<TEntity, TField> DependOn<TField>(Expression<Func<TEntity, TField>> fieldSelector)
    {
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式，多个字段可以多次使用DependOn方法，最多支持两个字段");

        var memberExpr = fieldSelector.Body as MemberExpression;
        var memberName = memberExpr.Member.Name;
        var entityType = typeof(TEntity);
        if (!this.shardingProvider.TryGetTableSharding(entityType, out var shardingTable))
            this.shardingProvider.AddTableSharding(entityType, shardingTable = new TableShardingInfo { EntityType = entityType });
        shardingTable.DependOnMembers ??= new();
        shardingTable.DependOnMembers.Add(memberName);

        return new FieldShardingBuilder<TEntity, TField>(this.shardingProvider);
    }
    /// <summary>
    /// 设置依赖1个参数值的分表规则和分表名称验证正则表达式，此分表规则依赖于1个参数值，需要手动传入参数值来获取分表名称，不依赖于任何字段。
    /// </summary>
    /// <typeparam name="TParameter">参数值类型</typeparam>
    /// <param name="tableNameGetter">分表名获取委托</param>
    /// <param name="validateRegex">分表名验证规则</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public TableShardingBuilder<TEntity> UseRule<TParameter>(Func<string, TParameter, string> tableNameGetter, string validateRegex)
    {
        if (tableNameGetter == null)
            throw new ArgumentNullException(nameof(tableNameGetter));
        if (string.IsNullOrEmpty(validateRegex))
            throw new ArgumentNullException(nameof(validateRegex));

        var entityType = typeof(TEntity);
        if (!this.shardingProvider.TryGetTableSharding(entityType, out var shardingTable))
            this.shardingProvider.AddTableSharding(entityType, shardingTable = new TableShardingInfo { EntityType = entityType });
        shardingTable.Rule = (string origName, object fieldValue) => tableNameGetter(origName, (TParameter)fieldValue);
        shardingTable.ValidateRegex = validateRegex;
        return this;
    }
    /// <summary>
    /// 设置依赖2个参数值的分表规则和分表名称验证正则表达式，此分表规则依赖于2个参数值，需要手动传入参数值来获取分表名称，不依赖于任何字段。
    /// </summary>
    /// <typeparam name="TParameter1">参数值1类型</typeparam>
    /// <typeparam name="TParameter2">参数值2类型</typeparam>
    /// <param name="tableNameGetter">分表名获取委托</param>
    /// <param name="validateRegex">分表名验证规则</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public TableShardingBuilder<TEntity> UseRule<TParameter1, TParameter2>(Func<string, TParameter1, TParameter2, string> tableNameGetter, string validateRegex)
    {
        if (tableNameGetter == null)
            throw new ArgumentNullException(nameof(tableNameGetter));
        if (string.IsNullOrEmpty(validateRegex))
            throw new ArgumentNullException(nameof(validateRegex));

        var entityType = typeof(TEntity);
        if (!this.shardingProvider.TryGetTableSharding(entityType, out var shardingTable))
            this.shardingProvider.AddTableSharding(entityType, shardingTable = new TableShardingInfo { EntityType = entityType });
        shardingTable.Rule = (string origName, object field1Value, object field2Value) => tableNameGetter(origName, (TParameter1)field1Value, (TParameter2)field2Value);
        shardingTable.ValidateRegex = validateRegex;
        return this;
    }
    /// <summary>
    /// 设置依赖3个参数值的分表规则和分表名称验证正则表达式，此分表规则依赖于3个参数值，需要手动传入参数值来获取分表名称，不依赖于任何字段。
    /// </summary>
    /// <typeparam name="TParameter1">参数值1类型</typeparam>
    /// <typeparam name="TParameter2">参数值2类型</typeparam>
    /// <typeparam name="TParameter3">参数值3类型</typeparam>
    /// <param name="tableNameGetter">分表名获取委托</param>
    /// <param name="validateRegex">分表名验证规则</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public TableShardingBuilder<TEntity> UseRule<TParameter1, TParameter2, TParameter3>(Func<string, TParameter1, TParameter2, TParameter3, string> tableNameGetter, string validateRegex)
    {
        if (tableNameGetter == null)
            throw new ArgumentNullException(nameof(tableNameGetter));
        if (string.IsNullOrEmpty(validateRegex))
            throw new ArgumentNullException(nameof(validateRegex));

        var entityType = typeof(TEntity);
        if (!this.shardingProvider.TryGetTableSharding(entityType, out var shardingTable))
            this.shardingProvider.AddTableSharding(entityType, shardingTable = new TableShardingInfo { EntityType = entityType });
        shardingTable.Rule = (string origName, object field1Value, object field2Value, object field3Value) => tableNameGetter(origName, (TParameter1)field1Value, (TParameter2)field2Value, (TParameter3)field3Value);
        shardingTable.ValidateRegex = validateRegex;
        return this;
    }
    /// <summary>
    /// 设置分表范围规则，此分表规则依赖于2个参数值，通常用于时间范围查询，需要手动传入字段值来获取分表名称，要先设置分表规则后再设置范围规则。
    /// </summary>
    /// <typeparam name="TParameter">参数值类型</typeparam>
    /// <param name="tableNamesGetter">分表名获取委托</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public TableShardingBuilder<TEntity> UseRangeRule<TParameter>(Func<string, TParameter, TParameter, List<string>> tableNamesGetter)
    {
        if (tableNamesGetter == null)
            throw new ArgumentNullException(nameof(tableNamesGetter));

        var entityType = typeof(TEntity);
        if (!this.shardingProvider.TryGetTableSharding(entityType, out var shardingTable))
            this.shardingProvider.AddTableSharding(entityType, shardingTable = new TableShardingInfo { EntityType = entityType });
        shardingTable.RangleRule = (string origName, object beginFieldValue, object endFieldValue)
            => tableNamesGetter(origName, (TParameter)beginFieldValue, (TParameter)endFieldValue);
        return this;
    }
    /// <summary>
    /// 设置分表范围规则，此分表规则依赖于3个参数值，通常用于时间范围查询，需要手动传入字段值来获取分表名称，要先设置分表规则后再设置范围规则。
    /// </summary>
    /// <typeparam name="TParameter1">参数值1类型</typeparam>
    /// <typeparam name="TParameter2">参数值2类型</typeparam>
    /// <param name="tableNamesGetter">分表名获取委托</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public TableShardingBuilder<TEntity> UseRangeRule<TParameter1, TParameter2>(Func<string, TParameter1, TParameter2, TParameter2, List<string>> tableNamesGetter)
    {
        if (tableNamesGetter == null)
            throw new ArgumentNullException(nameof(tableNamesGetter));

        var entityType = typeof(TEntity);
        if (!this.shardingProvider.TryGetTableSharding(entityType, out var shardingTable))
            this.shardingProvider.AddTableSharding(entityType, shardingTable = new TableShardingInfo { EntityType = entityType });
        shardingTable.RangleRule = (string origName, object field1Value, object beginField2Value, object endField2Value)
            => tableNamesGetter(origName, (TParameter1)field1Value, (TParameter2)beginField2Value, (TParameter2)endField2Value);
        return this;
    }
    /// <summary>
    /// 设置分表范围规则，此分表规则依赖于4个参数值，通常用于时间范围查询，需要手动传入字段值来获取分表名称，要先设置分表规则后再设置范围规则。
    /// </summary>
    /// <typeparam name="TParameter1">参数值1类型</typeparam>
    /// <typeparam name="TParameter2">参数值2类型</typeparam>
    /// <typeparam name="TParameter3">参数值3类型</typeparam>
    /// <param name="tableNamesGetter">分表名获取委托</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public TableShardingBuilder<TEntity> UseRangeRule<TParameter1, TParameter2, TParameter3>(Func<string, TParameter1, TParameter2, TParameter3, TParameter3, List<string>> tableNamesGetter)
    {
        if (tableNamesGetter == null)
            throw new ArgumentNullException(nameof(tableNamesGetter));

        var entityType = typeof(TEntity);
        if (!this.shardingProvider.TryGetTableSharding(entityType, out var shardingTable))
            this.shardingProvider.AddTableSharding(entityType, shardingTable = new TableShardingInfo { EntityType = entityType });
        shardingTable.RangleRule = (string origName, object field1Value, object field2Value, object beginField3Value, object endField3Value)
            => tableNamesGetter(origName, (TParameter1)field1Value, (TParameter2)field2Value, (TParameter3)beginField3Value, (TParameter3)endField3Value);
        return this;
    }
}
public class FieldShardingBuilder<TEntity, TField>
{
    private readonly ITableShardingProvider shardingProvider;
    public FieldShardingBuilder(ITableShardingProvider tableShardingProvider)
        => this.shardingProvider = tableShardingProvider;
    /// <summary>
    /// 设置第二个分表依赖的字段，后续的分表规则会依赖这个字段的值来进行分表，最多支持两个依赖字段
    /// </summary>
    /// <typeparam name="TField2">字段类型</typeparam>
    /// <param name="fieldSelector">字段选择器</param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public FieldShardingBuilder<TEntity, TField, TField2> DependOn<TField2>(Expression<Func<TEntity, TField2>> fieldSelector)
    {
        if (fieldSelector.Body.NodeType != ExpressionType.MemberAccess)
            throw new NotSupportedException($"不支持的表达式{nameof(fieldSelector)},只支持MemberAccess类型表达式，多个字段可以多次使用DependOn方法，最多支持两个字段");

        var memberExpr = fieldSelector.Body as MemberExpression;
        var memberName = memberExpr.Member.Name;
        var entityType = typeof(TEntity);
        if (!this.shardingProvider.TryGetTableSharding(entityType, out var shardingTable))
            this.shardingProvider.AddTableSharding(entityType, shardingTable = new TableShardingInfo { EntityType = entityType });
        shardingTable.DependOnMembers.Add(memberName);

        return new FieldShardingBuilder<TEntity, TField, TField2>(this.shardingProvider);
    }
    /// <summary>
    /// 设置分表名称命名规则和分表名称验证正则表达式，此分表规则依赖于已设置的依赖字段值，也可以手动传入字段值来获取分表名称，不依赖于任何字段值。
    /// </summary>
    /// <param name="tableNameGetter">分表名获取委托</param>
    /// <param name="validateRegex">分表名验证规则</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public FieldShardingBuilder<TEntity, TField> UseRule(Func<string, TField, string> tableNameGetter, string validateRegex)
    {
        if (tableNameGetter == null)
            throw new ArgumentNullException(nameof(tableNameGetter));
        if (string.IsNullOrEmpty(validateRegex))
            throw new ArgumentNullException(nameof(validateRegex));

        var entityType = typeof(TEntity);
        if (!this.shardingProvider.TryGetTableSharding(entityType, out var shardingTable))
            this.shardingProvider.AddTableSharding(entityType, shardingTable = new TableShardingInfo { EntityType = entityType });
        shardingTable.Rule = (string origName, object fieldValue) => tableNameGetter(origName, (TField)fieldValue);
        shardingTable.ValidateRegex = validateRegex;
        return this;
    }
    /// <summary>
    /// 设置分表范围规则，此分表规则依赖于已设置的依赖字段值，通常用于时间范围查询，需要手动传入字段值来获取分表名称，要先设置分表规则后再设置范围规则。
    /// </summary>
    /// <param name="tableNamesGetter">分表名获取委托</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public FieldShardingBuilder<TEntity, TField> UseRangeRule(Func<string, TField, TField, List<string>> tableNamesGetter)
    {
        if (tableNamesGetter == null)
            throw new ArgumentNullException(nameof(tableNamesGetter));

        var entityType = typeof(TEntity);
        if (!this.shardingProvider.TryGetTableSharding(entityType, out var shardingTable))
            this.shardingProvider.AddTableSharding(entityType, shardingTable = new TableShardingInfo { EntityType = entityType });
        shardingTable.RangleRule = (string origName, object beginFieldValue, object endFieldValue)
            => tableNamesGetter(origName, (TField)beginFieldValue, (TField)endFieldValue);
        return this;
    }
}
public class FieldShardingBuilder<TEntity, TField1, TField2>
{
    private readonly ITableShardingProvider shardingProvider;
    public FieldShardingBuilder(ITableShardingProvider tableShardingProvider)
        => this.shardingProvider = tableShardingProvider;
    /// <summary>
    /// 设置分表名称命名规则和分表名称验证正则表达式
    /// </summary>
    /// <param name="tableNameGetter">分表名称获取委托</param>
    /// <param name="validateRegex"> 分表名称验证正则表达式，用于筛选分表名称</param>
    /// <returns></returns>
    public FieldShardingBuilder<TEntity, TField1, TField2> UseRule(Func<string, TField1, TField2, string> tableNameGetter, string validateRegex)
    {
        if (tableNameGetter == null)
            throw new ArgumentNullException(nameof(tableNameGetter));
        if (string.IsNullOrEmpty(validateRegex))
            throw new ArgumentNullException(nameof(validateRegex));

        var entityType = typeof(TEntity);
        if (!this.shardingProvider.TryGetTableSharding(entityType, out var shardingTable))
            this.shardingProvider.AddTableSharding(entityType, shardingTable = new TableShardingInfo { EntityType = entityType });
        shardingTable.Rule = (string origName, object field1Value, object field2Value) => tableNameGetter(origName, (TField1)field1Value, (TField2)field2Value);
        shardingTable.ValidateRegex = validateRegex;
        return this;
    }
    public FieldShardingBuilder<TEntity, TField1, TField2> UseRangeRule(Func<string, TField1, TField2, TField2, List<string>> tableNamesGetter)
    {
        if (tableNamesGetter == null)
            throw new ArgumentNullException(nameof(tableNamesGetter));

        var entityType = typeof(TEntity);
        if (!this.shardingProvider.TryGetTableSharding(entityType, out var shardingTable))
            this.shardingProvider.AddTableSharding(entityType, shardingTable = new TableShardingInfo { EntityType = entityType });
        shardingTable.RangleRule = (string origName, object field1Value, object beginField2Value, object endField2Value)
            => tableNamesGetter(origName, (TField1)field1Value, (TField2)beginField2Value, (TField2)endField2Value);
        return this;
    }
}
public class FieldShardingBuilder<TEntity, TField1, TField2, TField3>
{
    private readonly ITableShardingProvider shardingProvider;
    public FieldShardingBuilder(ITableShardingProvider tableShardingProvider)
        => this.shardingProvider = tableShardingProvider;
    /// <summary>
    /// 设置分表名称命名规则和分表名称验证正则表达式
    /// </summary>
    /// <param name="tableNameGetter">分表名称获取委托</param>
    /// <param name="validateRegex"> 分表名称验证正则表达式，用于筛选分表名称</param>
    /// <returns></returns>
    public FieldShardingBuilder<TEntity, TField1, TField2, TField3> UseRule(Func<string, TField1, TField2, TField3, string> tableNameGetter, string validateRegex)
    {
        if (tableNameGetter == null)
            throw new ArgumentNullException(nameof(tableNameGetter));
        if (string.IsNullOrEmpty(validateRegex))
            throw new ArgumentNullException(nameof(validateRegex));

        var entityType = typeof(TEntity);
        if (!this.shardingProvider.TryGetTableSharding(entityType, out var shardingTable))
            this.shardingProvider.AddTableSharding(entityType, shardingTable = new TableShardingInfo { EntityType = entityType });
        shardingTable.Rule = (string origName, object field1Value, object field2Value, object field3Value) => tableNameGetter(origName, (TField1)field1Value, (TField2)field2Value, (TField3)field3Value);
        shardingTable.ValidateRegex = validateRegex;
        return this;
    }
    public FieldShardingBuilder<TEntity, TField1, TField2, TField3> UseRangeRule(Func<string, TField1, TField2, TField3, TField3, List<string>> tableNamesGetter)
    {
        if (tableNamesGetter == null)
            throw new ArgumentNullException(nameof(tableNamesGetter));

        var entityType = typeof(TEntity);
        if (!this.shardingProvider.TryGetTableSharding(entityType, out var shardingTable))
            this.shardingProvider.AddTableSharding(entityType, shardingTable = new TableShardingInfo { EntityType = entityType });
        shardingTable.RangleRule = (string origName, object field1Value, object field2Value, object beginField3Value, object endField3Value)
            => tableNamesGetter(origName, (TField1)field1Value, (TField2)field2Value, (TField3)beginField3Value, (TField3)endField3Value);
        return this;
    }
}