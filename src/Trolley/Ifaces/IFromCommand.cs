using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public interface IFromCommand : IDisposable
{
    #region Visitor
    DbContext DbContext { get; }
    IQueryVisitor Visitor { get; }
    #endregion

    #region Select    
    /// <summary>
    /// 使用原始字段返回查询结果，用法：Select&lt;Order&gt;("*") 或是 Select&lt;int&gt;("1")
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fields">原始字段字符串，默认值*</param>
    /// <returns>返回插入对象</returns>
    IFromCommand<TTarget> Select<TTarget>(string fields = "*");
    #endregion

    #region Execute
    /// <summary>
    /// 执行插入操作，并返回插入行数
    /// </summary>
    /// <returns>返回插入行数</returns>
    int Execute();
    /// <summary>
    /// 执行插入操作，并返回插入行数
    /// </summary>
    /// <param name="cancellationToken">取消token</param>
    /// <returns>返回插入行数</returns>
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
    #endregion

    #region ToSql
    /// <summary>
    /// 返回当前查询的SQL和参数列表
    /// </summary>
    /// <param name="dbParameters">参数列表</param>
    /// <returns>当前查询的SQL</returns>
    string ToSql(out List<IDbDataParameter> dbParameters);
    #endregion
}
public interface IFromCommand<T> : IFromCommand
{
    #region Sharding
    /// <summary>
    /// 使用固定表名确定T表一个或多个分表名执行查询，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")，按月分表
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T> UseTable(params string[] tableNames);
    /// <summary>
    /// 使用表名断言确定T表一个或多个分表执行查询，完整的表名，如：.UseTable(f =&gt; f.Contains("202001"))，按月分表
    /// </summary>
    /// <param name="tableNamePredicate">表名断言，如：f =&gt; f.Contains("202001")</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T> UseTable(Func<string, bool> tableNamePredicate);
    /// <summary>
    /// 根据字段值确定T表分表名，最多支持2个字段，字段值的顺序与配置的字段顺序保持一致，可多次调用
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="field2Value">字段2值</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T> UseTableBy(object field1Value, object field2Value = null);
    /// <summary>
    /// 根据单个字段值范围确定T表分表名执行查询，通常是日期规则分表使用，如：repository.From&lt;Order&gt;().UseTableByRange(DateTime.Parse("2020-01-01"), DateTime.Now)
    /// </summary>
    /// <param name="beginFieldValue">字段起始值</param>
    /// <param name="endFieldValue">字段结束值</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T> UseTableByRange(object beginFieldValue, object endFieldValue);
    /// <summary>
    /// 根据1个固定字段值和1个字段值范围确定T表分表名执行查询，字段值的顺序与配置的字段顺序保持一致，通常是日期规则分表使用，
    /// 如：配置时 .UseSharding(s =&gt;s.UseTable&lt;Order&gt;(t =&gt; t.DependOn(d =&gt; d.TenantId).DependOn(d =&gt; d.CreatedAt).UseRule((dbKey, origName, tenantId, dateTime) =&gt; $"{origName}_{tenantId}_{dateTime:yyyMM}")
    /// .UseRangeRule((dbKey, origName, tenantId, beginTime, endTime) =&gt;{ ...}))，此处使用 repository.From&lt;Order&gt;().UseTableByRange("tenant001", DateTime.Parse("2020-01-01"), DateTime.Now)
    /// </summary>
    /// <param name="fieldValue1">第一个值</param>
    /// <param name="fieldValue2">第二个值</param>
    /// <param name="fieldValue3">第三个值</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3);
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T> UseTableSchema(string tableSchema);
    #endregion

    #region Union/UnionAll
    IFromCommand<T> Union(IQuery<T> subQuery);
    IFromCommand<T> Union(Func<IFromQuery, IQuery<T>> subQuery);
    IFromCommand<T> UnionAll(IQuery<T> subQuery);
    IFromCommand<T> UnionAll(Func<IFromQuery, IQuery<T>> subQuery);
    #endregion

    #region Join
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其进行INNER JOIN关联，用法:
    /// <code>
    /// .InnerJoin&lt;TOther&gt;((a, b) =&gt; ...)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T, TOther> InnerJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其做LEFT JOIN关联，用法:
    /// <code>
    /// .LeftJoin&lt;TOther&gt;((a, b) =&gt; ...)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T, TOther> LeftJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其做RIGHT JOIN关联，用法:
    /// <code>
    /// .RightJoin&lt;TOther&gt;((a, b) =&gt; ...)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T, TOther> RightJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn);
    /// <summary>
    /// 添加子查询临时表subQuery，并与现有表T做INNER JOIN关联，可以用在CTE子句中自我引用，用法:
    /// <code>
    /// repository.FromWith(...).NextWith(...)
    ///     .UnionAllRecursive((x, self) =&gt; x.From&lt;Menu&gt;()
    ///         .InnerJoin(self, (a, b) =&gt; a.ParentId == b.Id)
    ///         .Select(...), "MenuList") ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型，子查询中通常会有SELECT操作，返回的类型是一个匿名类</typeparam>
    /// <param name="subQuery">子查询对象，也可以CTE表的自我引用</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn);
    /// <summary>
    /// 添加子查询临时表subQuery，并与现有表T做LEFT JOIN关联，可以用在CTE子句中自我引用，用法:
    /// <code>
    /// repository.FromWith(...).NextWith(...)
    ///     .UnionAllRecursive((x, self) =&gt; x.From&lt;Menu&gt;()
    ///         .LeftJoin(self, (a, b) =&gt; a.ParentId == b.Id)
    ///         .Select(...), "MenuList") ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型，子查询中通常会有SELECT操作，返回的类型是一个匿名类</typeparam>
    /// <param name="subQuery">子查询对象，也可以CTE表的自我引用</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn);
    /// <summary>
    /// 添加子查询临时表subQuery，并与现有表T做LEFT JOIN关联，可以用在CTE子句中自我引用，用法:
    /// <code>
    /// repository.FromWith(...).NextWith(...)
    ///     .UnionAllRecursive((x, self) =&gt; x.From&lt;Menu&gt;()
    ///         .RightJoin(self, (a, b) =&gt; a.ParentId == b.Id)
    ///         .Select(...), "MenuList") ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型，子查询中通常会有SELECT操作，返回的类型是一个匿名类</typeparam>
    /// <param name="subQuery">子查询对象，也可以CTE表的自我引用</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做INNER JOIN关联，与.WithTable(...).InnerJoin(...)等效，用法:
    /// <code>
    /// .InnerJoin((a, b) =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select((x, y) =&gt; new { ... }), (a, b) =&gt; ...) ...
    /// SQL:
    /// ... INNER JOIN (SELECT ... FROM `sys_order_detail` ...) c ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T, TOther> InnerJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做LEFT JOIN关联，与.WithTable(...).LeftJoin(...)等效，用法:
    /// <code>
    /// .LeftJoin((a, b) =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select((x, y) =&gt; new { ... }), (a, b) =&gt; ...) ...
    /// SQL:
    /// ... LEFT JOIN (SELECT ... FROM `sys_order_detail` ...) c ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T, TOther> LeftJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做RIGHT JOIN关联，与.WithTable(...).RightJoin(...)等效，用法:
    /// <code>
    /// .RightJoin((a, b) =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select((x, y) =&gt; new { ... }), (a, b) =&gt; ...) ...
    /// SQL:
    /// ... RIGHT JOIN (SELECT ... FROM `sys_order_detail` ...) c ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T, TOther> RightJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T, TOther, bool>> joinOn);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T> Where(Expression<Func<T, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T> Where(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T> And(Expression<Func<T, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T> And(bool condition, Expression<Func<T, bool>> ifPredicate = null, Expression<Func<T, bool>> elsePredicate = null);
    #endregion

    #region GroupBy
    /// <summary>
    /// 分组查询，分组表达式groupingExpr可以是单个字段或多个字段的匿名对象，用法:
    /// <code>
    /// repository.From&lt;User&gt;() ...
    ///    .GroupBy((a, b, ...) =&gt; new { a.Id, a.Name, b.CreatedAt.Date }) //或是 .GroupBy((a, b, ...) =&gt; a.CreatedAt.Date)
    ///    .Select((x, a, b) =&gt; new
    ///    {
    ///        x.Grouping, //可以直接返回分组对象，也可以返回分组对象的某个字段,如：a.Id, a.Name, b.CreatedAt.Date，也可以 x.Grouping.Id, x.Grouping.Name, x.Grouping.Date ...
    ///        OrderCount = x.Count(b.Id), //也可以返回分组后的聚合操作
    ///        TotalAmount = x.Sum(b.TotalAmount) //也可以返回分组后的聚合操作
    ///    })
    ///    .ToSql(out _);
    /// SQL:
    /// SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `Date`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a ... GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TGrouping">分组后的实体对象类型，可以是单个字段类型或是匿名类型</typeparam>
    /// <param name="groupingExpr">分组表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IGroupingCommand<T, TGrouping> GroupBy<TGrouping>(Expression<Func<T, TGrouping>> groupingExpr);
    #endregion

    #region OrderBy
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderBy((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T> OrderBy<TFields>(Expression<Func<T, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T> OrderBy<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr);
    /// <summary>
    /// DSC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T> OrderByDescending<TFields>(Expression<Func<T, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T> OrderByDescending<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，用法：
    /// Select((a, b) =&gt; new { a.Id, a.Name, ... }) 或是 Select((a, b) =&gt; x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<TTarget> Select<TTarget>(Expression<Func<T, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定聚合字段返回实体，单个或多个聚合字段的匿名对象，用法：
    /// <code>
    /// .SelectAggregate((x, a, b) =&gt; new
    /// {
    ///     OrderCount = x.Count(a.Id),
    ///     TotalAmount = x.Sum(a.TotalAmount)
    /// })
    /// </code>
    /// 生成的SQL:
    /// <code>SELECT COUNT(`Id`) AS `OrderCount`,SUM(`TotalAmount`) AS `TotalAmount` ... </code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型，通常是一个匿名类</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个或多个聚合字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T, TTarget>> fieldsExpr);
    #endregion

    #region Take
    IFromCommand<T> Take(int limit);
    #endregion
}
public interface IFromCommand<T1, T2> : IFromCommand
{
    #region Sharding
    /// <summary>
    /// 使用固定表名确定T2表一个或多个分表名执行查询，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")，按月分表
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2> UseTable(params string[] tableNames);
    /// <summary>
    /// 使用表名断言确定T2表一个或多个分表执行查询，完整的表名，如：.UseTable(f =&gt; f.Contains("202001"))，按月分表
    /// </summary>
    /// <param name="tableNamePredicate">表名断言，如：f =&gt; f.Contains("202001")</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2> UseTable(Func<string, bool> tableNamePredicate);
    /// <summary>
    /// 根据字段值确定T2表分表名，最多支持2个字段，字段值的顺序与配置的字段顺序保持一致，可多次调用
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="field2Value">字段2值</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2> UseTableBy(object field1Value, object field2Value = null);
    /// <summary>
    /// 根据单个字段值范围确定T表分表名执行查询，通常是日期规则分表使用，如：repository.From&lt;Order&gt;().UseTableByRange(DateTime.Parse("2020-01-01"), DateTime.Now)
    /// </summary>
    /// <param name="beginFieldValue">字段起始值</param>
    /// <param name="endFieldValue">字段结束值</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2> UseTableByRange(object beginFieldValue, object endFieldValue);
    /// <summary>
    /// 根据1个固定字段值和1个字段值范围确定T2表分表名执行查询，字段值的顺序与配置的字段顺序保持一致，通常是日期规则分表使用，
    /// 如：配置时 .UseSharding(s =&gt;s.UseTable&lt;Order&gt;(t =&gt; t.DependOn(d =&gt; d.TenantId).DependOn(d =&gt; d.CreatedAt).UseRule((dbKey, origName, tenantId, dateTime) =&gt; $"{origName}_{tenantId}_{dateTime:yyyMM}")
    /// .UseRangeRule((dbKey, origName, tenantId, beginTime, endTime) =&gt;{ ...}))，此处使用 repository.From&lt;Order&gt;().UseTableByRange("tenant001", DateTime.Parse("2020-01-01"), DateTime.Now)
    /// </summary>
    /// <param name="fieldValue1">第一个值</param>
    /// <param name="fieldValue2">第二个值</param>
    /// <param name="fieldValue3">第三个值</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3);
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2> UseTableSchema(string tableSchema);
    #endregion

    #region Join
    /// <summary>
    /// 在现有表中，指定2个表进行INNER JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// .InnerJoin((a, b) =&gt; ...)
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2> InnerJoin(Expression<Func<T1, T2, bool>> joinOn);
    /// <summary>
    /// 在现有表中，指定2个表进行LEFT JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>.LeftJoin((a, b) =&gt; ...)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2> LeftJoin(Expression<Func<T1, T2, bool>> joinOn);
    /// <summary>
    /// 在现有表中，指定2个表进行RIGHT JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// .RightJoin((a, b) =&gt; ...)
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2> RightJoin(Expression<Func<T1, T2, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其进行INNER JOIN关联，用法:
    /// <code>
    /// .InnerJoin&lt;TOther&gt;((a, b) =&gt; ...)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其做LEFT JOIN关联，用法:
    /// <code>
    /// .LeftJoin&lt;TOther&gt;((a, b) =&gt; ...)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其做RIGHT JOIN关联，用法:
    /// <code>
    /// .RightJoin&lt;TOther&gt;((a, b) =&gt; ...)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, TOther> RightJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn);
    /// <summary>
    /// 添加子查询临时表subQuery，并与现有表T做INNER JOIN关联，可以用在CTE子句中自我引用，用法:
    /// <code>
    /// repository.FromWith(...).NextWith(...)
    ///     .UnionAllRecursive((x, self) =&gt; x.From&lt;Menu&gt;()
    ///         .InnerJoin(self, (a, b) =&gt; a.ParentId == b.Id)
    ///         .Select(...), "MenuList") ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型，子查询中通常会有SELECT操作，返回的类型是一个匿名类</typeparam>
    /// <param name="subQuery">子查询对象，也可以CTE表的自我引用</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn);
    /// <summary>
    /// 添加子查询临时表subQuery，并与现有表T做LEFT JOIN关联，可以用在CTE子句中自我引用，用法:
    /// <code>
    /// repository.FromWith(...).NextWith(...)
    ///     .UnionAllRecursive((x, self) =&gt; x.From&lt;Menu&gt;()
    ///         .LeftJoin(self, (a, b) =&gt; a.ParentId == b.Id)
    ///         .Select(...), "MenuList") ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型，子查询中通常会有SELECT操作，返回的类型是一个匿名类</typeparam>
    /// <param name="subQuery">子查询对象，也可以CTE表的自我引用</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn);
    /// <summary>
    /// 添加子查询临时表subQuery，并与现有表T做LEFT JOIN关联，可以用在CTE子句中自我引用，用法:
    /// <code>
    /// repository.FromWith(...).NextWith(...)
    ///     .UnionAllRecursive((x, self) =&gt; x.From&lt;Menu&gt;()
    ///         .RightJoin(self, (a, b) =&gt; a.ParentId == b.Id)
    ///         .Select(...), "MenuList") ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型，子查询中通常会有SELECT操作，返回的类型是一个匿名类</typeparam>
    /// <param name="subQuery">子查询对象，也可以CTE表的自我引用</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做INNER JOIN关联，与.WithTable(...).InnerJoin(...)等效，用法:
    /// <code>
    /// .InnerJoin((a, b) =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select((x, y) =&gt; new { ... }), (a, b) =&gt; ...) ...
    /// SQL:
    /// ... INNER JOIN (SELECT ... FROM `sys_order_detail` ...) c ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, TOther> InnerJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做LEFT JOIN关联，与.WithTable(...).LeftJoin(...)等效，用法:
    /// <code>
    /// .LeftJoin((a, b) =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select((x, y) =&gt; new { ... }), (a, b) =&gt; ...) ...
    /// SQL:
    /// ... LEFT JOIN (SELECT ... FROM `sys_order_detail` ...) c ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, TOther> LeftJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做RIGHT JOIN关联，与.WithTable(...).RightJoin(...)等效，用法:
    /// <code>
    /// .RightJoin((a, b) =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select((x, y) =&gt; new { ... }), (a, b) =&gt; ...) ...
    /// SQL:
    /// ... RIGHT JOIN (SELECT ... FROM `sys_order_detail` ...) c ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, TOther> RightJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2> Where(Expression<Func<T1, T2, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2> Where(bool condition, Expression<Func<T1, T2, bool>> ifPredicate, Expression<Func<T1, T2, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2> And(Expression<Func<T1, T2, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2> And(bool condition, Expression<Func<T1, T2, bool>> ifPredicate = null, Expression<Func<T1, T2, bool>> elsePredicate = null);
    #endregion

    #region GroupBy
    /// <summary>
    /// 分组查询，分组表达式groupingExpr可以是单个字段或多个字段的匿名对象，用法:
    /// <code>
    /// repository.From&lt;User&gt;() ...
    ///    .GroupBy((a, b, ...) =&gt; new { a.Id, a.Name, b.CreatedAt.Date }) //或是 .GroupBy((a, b, ...) =&gt; a.CreatedAt.Date)
    ///    .Select((x, a, b, ...) =&gt; new
    ///    {
    ///        x.Grouping, //可以直接返回分组对象，也可以返回分组对象的某个字段,如：a.Id, a.Name, b.CreatedAt.Date，也可以 x.Grouping.Id, x.Grouping.Name, x.Grouping.Date ...
    ///        OrderCount = x.Count(b.Id), //也可以返回分组后的聚合操作
    ///        TotalAmount = x.Sum(b.TotalAmount) //也可以返回分组后的聚合操作
    ///    })
    ///    .ToSql(out _);
    /// SQL:
    /// SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `Date`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a ... GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TGrouping">分组后的实体对象类型，可以是单个字段类型或是匿名类型</typeparam>
    /// <param name="groupingExpr">分组表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IGroupingCommand<T1, T2, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, TGrouping>> groupingExpr);
    #endregion

    #region OrderBy
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderBy((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2> OrderBy<TFields>(Expression<Func<T1, T2, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, TFields>> fieldsExpr);
    /// <summary>
    /// DSC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2> OrderByDescending<TFields>(Expression<Func<T1, T2, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，用法：
    /// Select((a, b, ...) =&gt; new { a.Id, a.Name, ... }) 或是 Select((a, b, ...) =&gt; x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<TTarget> Select<TTarget>(Expression<Func<T1, T2, TTarget>> fieldsExpr);
    #endregion
}
public interface IFromCommand<T1, T2, T3> : IFromCommand
{
    #region Sharding
    /// <summary>
    /// 使用固定表名确定T3表一个或多个分表名执行查询，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")，按月分表
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3> UseTable(params string[] tableNames);
    /// <summary>
    /// 使用表名断言确定T3表一个或多个分表执行查询，完整的表名，如：.UseTable(f =&gt; f.Contains("202001"))，按月分表
    /// </summary>
    /// <param name="tableNamePredicate">表名断言，如：f =&gt; f.Contains("202001")</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3> UseTable(Func<string, bool> tableNamePredicate);
    /// <summary>
    /// 根据字段值确定T3表分表名，最多支持2个字段，字段值的顺序与配置的字段顺序保持一致，可多次调用
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="field2Value">字段2值</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3> UseTableBy(object field1Value, object field2Value = null);
    /// <summary>
    /// 根据单个字段值范围确定T表分表名执行查询，通常是日期规则分表使用，如：repository.From&lt;Order&gt;().UseTableByRange(DateTime.Parse("2020-01-01"), DateTime.Now)
    /// </summary>
    /// <param name="beginFieldValue">字段起始值</param>
    /// <param name="endFieldValue">字段结束值</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3> UseTableByRange(object beginFieldValue, object endFieldValue);
    /// <summary>
    /// 根据1个固定字段值和1个字段值范围确定T3表分表名执行查询，字段值的顺序与配置的字段顺序保持一致，通常是日期规则分表使用，
    /// 如：配置时 .UseSharding(s =&gt;s.UseTable&lt;Order&gt;(t =&gt; t.DependOn(d =&gt; d.TenantId).DependOn(d =&gt; d.CreatedAt).UseRule((dbKey, origName, tenantId, dateTime) =&gt; $"{origName}_{tenantId}_{dateTime:yyyMM}")
    /// .UseRangeRule((dbKey, origName, tenantId, beginTime, endTime) =&gt;{ ...}))，此处使用 repository.From&lt;Order&gt;().UseTableByRange("tenant001", DateTime.Parse("2020-01-01"), DateTime.Now)
    /// </summary>
    /// <param name="fieldValue1">第一个值</param>
    /// <param name="fieldValue2">第二个值</param>
    /// <param name="fieldValue3">第三个值</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3);
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3> UseTableSchema(string tableSchema);
    #endregion

    #region Join
    /// <summary>
    /// 在现有表中，指定2个表进行INNER JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// .InnerJoin((a, b, c) =&gt; ...)
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3> InnerJoin(Expression<Func<T1, T2, T3, bool>> joinOn);
    /// <summary>
    /// 在现有表中，指定2个表进行LEFT JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>.LeftJoin((a, b, c) =&gt; ...)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3> LeftJoin(Expression<Func<T1, T2, T3, bool>> joinOn);
    /// <summary>
    /// 在现有表中，指定2个表进行RIGHT JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// .RightJoin((a, b, c) =&gt; ...)
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3> RightJoin(Expression<Func<T1, T2, T3, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其进行INNER JOIN关联，用法:
    /// <code>
    /// .InnerJoin&lt;TOther&gt;((a, b, c) =&gt; ...)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其做LEFT JOIN关联，用法:
    /// <code>
    /// .LeftJoin&lt;TOther&gt;((a, b, c) =&gt; ...)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其做RIGHT JOIN关联，用法:
    /// <code>
    /// .RightJoin&lt;TOther&gt;((a, b, c) =&gt; ...)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn);
    /// <summary>
    /// 添加子查询临时表subQuery，并与现有表T做INNER JOIN关联，可以用在CTE子句中自我引用，用法:
    /// <code>
    /// repository.FromWith(...).NextWith(...)
    ///     .UnionAllRecursive((x, self) =&gt; x.From&lt;Menu&gt;()
    ///         .InnerJoin(self, (a, b, c) =&gt; a.ParentId == b.Id)
    ///         .Select(...), "MenuList") ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型，子查询中通常会有SELECT操作，返回的类型是一个匿名类</typeparam>
    /// <param name="subQuery">子查询对象，也可以CTE表的自我引用</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn);
    /// <summary>
    /// 添加子查询临时表subQuery，并与现有表T做LEFT JOIN关联，可以用在CTE子句中自我引用，用法:
    /// <code>
    /// repository.FromWith(...).NextWith(...)
    ///     .UnionAllRecursive((x, self) =&gt; x.From&lt;Menu&gt;()
    ///         .LeftJoin(self, (a, b, c) =&gt; a.ParentId == b.Id)
    ///         .Select(...), "MenuList") ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型，子查询中通常会有SELECT操作，返回的类型是一个匿名类</typeparam>
    /// <param name="subQuery">子查询对象，也可以CTE表的自我引用</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn);
    /// <summary>
    /// 添加子查询临时表subQuery，并与现有表T做LEFT JOIN关联，可以用在CTE子句中自我引用，用法:
    /// <code>
    /// repository.FromWith(...).NextWith(...)
    ///     .UnionAllRecursive((x, self) =&gt; x.From&lt;Menu&gt;()
    ///         .RightJoin(self, (a, b, c) =&gt; a.ParentId == b.Id)
    ///         .Select(...), "MenuList") ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型，子查询中通常会有SELECT操作，返回的类型是一个匿名类</typeparam>
    /// <param name="subQuery">子查询对象，也可以CTE表的自我引用</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做INNER JOIN关联，与.WithTable(...).InnerJoin(...)等效，用法:
    /// <code>
    /// .InnerJoin((a, b, c) =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select((x, y) =&gt; new { ... }), (a, b, c) =&gt; ...) ...
    /// SQL:
    /// ... INNER JOIN (SELECT ... FROM `sys_order_detail` ...) c ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, TOther> InnerJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做LEFT JOIN关联，与.WithTable(...).LeftJoin(...)等效，用法:
    /// <code>
    /// .LeftJoin((a, b, c) =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select((x, y) =&gt; new { ... }), (a, b, c) =&gt; ...) ...
    /// SQL:
    /// ... LEFT JOIN (SELECT ... FROM `sys_order_detail` ...) c ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, TOther> LeftJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做RIGHT JOIN关联，与.WithTable(...).RightJoin(...)等效，用法:
    /// <code>
    /// .RightJoin((a, b, c) =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select((x, y) =&gt; new { ... }), (a, b, c) =&gt; ...) ...
    /// SQL:
    /// ... RIGHT JOIN (SELECT ... FROM `sys_order_detail` ...) c ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, TOther> RightJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3> Where(Expression<Func<T1, T2, T3, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3> Where(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate, Expression<Func<T1, T2, T3, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3> And(Expression<Func<T1, T2, T3, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3> And(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, bool>> elsePredicate = null);
    #endregion

    #region GroupBy
    /// <summary>
    /// 分组查询，分组表达式groupingExpr可以是单个字段或多个字段的匿名对象，用法:
    /// <code>
    /// repository.From&lt;User&gt;() ...
    ///    .GroupBy((a, b, ...) =&gt; new { a.Id, a.Name, b.CreatedAt.Date }) //或是 .GroupBy((a, b, ...) =&gt; a.CreatedAt.Date)
    ///    .Select((x, a, b, ...) =&gt; new
    ///    {
    ///        x.Grouping, //可以直接返回分组对象，也可以返回分组对象的某个字段,如：a.Id, a.Name, b.CreatedAt.Date，也可以 x.Grouping.Id, x.Grouping.Name, x.Grouping.Date ...
    ///        OrderCount = x.Count(b.Id), //也可以返回分组后的聚合操作
    ///        TotalAmount = x.Sum(b.TotalAmount) //也可以返回分组后的聚合操作
    ///    })
    ///    .ToSql(out _);
    /// SQL:
    /// SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `Date`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a ... GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TGrouping">分组后的实体对象类型，可以是单个字段类型或是匿名类型</typeparam>
    /// <param name="groupingExpr">分组表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IGroupingCommand<T1, T2, T3, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, TGrouping>> groupingExpr);
    #endregion

    #region OrderBy
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderBy((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3> OrderBy<TFields>(Expression<Func<T1, T2, T3, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, TFields>> fieldsExpr);
    /// <summary>
    /// DSC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，用法：
    /// Select((a, b, ...) =&gt; new { a.Id, a.Name, ... }) 或是 Select((a, b, ...) =&gt; x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, TTarget>> fieldsExpr);
    #endregion
}
public interface IFromCommand<T1, T2, T3, T4> : IFromCommand
{
    #region Sharding
    /// <summary>
    /// 使用固定表名确定T4表一个或多个分表名执行查询，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")，按月分表
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4> UseTable(params string[] tableNames);
    /// <summary>
    /// 使用表名断言确定T4表一个或多个分表执行查询，完整的表名，如：.UseTable(f =&gt; f.Contains("202001"))，按月分表
    /// </summary>
    /// <param name="tableNamePredicate">表名断言，如：f =&gt; f.Contains("202001")</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4> UseTable(Func<string, bool> tableNamePredicate);
    /// <summary>
    /// 根据字段值确定T4表分表名，最多支持2个字段，字段值的顺序与配置的字段顺序保持一致，可多次调用
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="field2Value">字段2值</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4> UseTableBy(object field1Value, object field2Value = null);
    /// <summary>
    /// 根据单个字段值范围确定T表分表名执行查询，通常是日期规则分表使用，如：repository.From&lt;Order&gt;().UseTableByRange(DateTime.Parse("2020-01-01"), DateTime.Now)
    /// </summary>
    /// <param name="beginFieldValue">字段起始值</param>
    /// <param name="endFieldValue">字段结束值</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4> UseTableByRange(object beginFieldValue, object endFieldValue);
    /// <summary>
    /// 根据1个固定字段值和1个字段值范围确定T4表分表名执行查询，字段值的顺序与配置的字段顺序保持一致，通常是日期规则分表使用，
    /// 如：配置时 .UseSharding(s =&gt;s.UseTable&lt;Order&gt;(t =&gt; t.DependOn(d =&gt; d.TenantId).DependOn(d =&gt; d.CreatedAt).UseRule((dbKey, origName, tenantId, dateTime) =&gt; $"{origName}_{tenantId}_{dateTime:yyyMM}")
    /// .UseRangeRule((dbKey, origName, tenantId, beginTime, endTime) =&gt;{ ...}))，此处使用 repository.From&lt;Order&gt;().UseTableByRange("tenant001", DateTime.Parse("2020-01-01"), DateTime.Now)
    /// </summary>
    /// <param name="fieldValue1">第一个值</param>
    /// <param name="fieldValue2">第二个值</param>
    /// <param name="fieldValue3">第三个值</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3);
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4> UseTableSchema(string tableSchema);
    #endregion

    #region Join
    /// <summary>
    /// 在现有表中，指定2个表进行INNER JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// .InnerJoin((a, b, c, d) =&gt; ...)
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4> InnerJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn);
    /// <summary>
    /// 在现有表中，指定2个表进行LEFT JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>.LeftJoin((a, b, c, d) =&gt; ...)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4> LeftJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn);
    /// <summary>
    /// 在现有表中，指定2个表进行RIGHT JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// .RightJoin((a, b, c, d) =&gt; ...)
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4> RightJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其进行INNER JOIN关联，用法:
    /// <code>
    /// .InnerJoin&lt;TOther&gt;((a, b, c, d) =&gt; ...)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其做LEFT JOIN关联，用法:
    /// <code>
    /// .LeftJoin&lt;TOther&gt;((a, b, c, d) =&gt; ...)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其做RIGHT JOIN关联，用法:
    /// <code>
    /// .RightJoin&lt;TOther&gt;((a, b, c, d) =&gt; ...)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn);
    /// <summary>
    /// 添加子查询临时表subQuery，并与现有表T做INNER JOIN关联，可以用在CTE子句中自我引用，用法:
    /// <code>
    /// repository.FromWith(...).NextWith(...)
    ///     .UnionAllRecursive((x, self) =&gt; x.From&lt;Menu&gt;()
    ///         .InnerJoin(self, (a, b, c, d) =&gt; a.ParentId == b.Id)
    ///         .Select(...), "MenuList") ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型，子查询中通常会有SELECT操作，返回的类型是一个匿名类</typeparam>
    /// <param name="subQuery">子查询对象，也可以CTE表的自我引用</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn);
    /// <summary>
    /// 添加子查询临时表subQuery，并与现有表T做LEFT JOIN关联，可以用在CTE子句中自我引用，用法:
    /// <code>
    /// repository.FromWith(...).NextWith(...)
    ///     .UnionAllRecursive((x, self) =&gt; x.From&lt;Menu&gt;()
    ///         .LeftJoin(self, (a, b, c, d) =&gt; a.ParentId == b.Id)
    ///         .Select(...), "MenuList") ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型，子查询中通常会有SELECT操作，返回的类型是一个匿名类</typeparam>
    /// <param name="subQuery">子查询对象，也可以CTE表的自我引用</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn);
    /// <summary>
    /// 添加子查询临时表subQuery，并与现有表T做LEFT JOIN关联，可以用在CTE子句中自我引用，用法:
    /// <code>
    /// repository.FromWith(...).NextWith(...)
    ///     .UnionAllRecursive((x, self) =&gt; x.From&lt;Menu&gt;()
    ///         .RightJoin(self, (a, b, c, d) =&gt; a.ParentId == b.Id)
    ///         .Select(...), "MenuList") ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型，子查询中通常会有SELECT操作，返回的类型是一个匿名类</typeparam>
    /// <param name="subQuery">子查询对象，也可以CTE表的自我引用</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做INNER JOIN关联，与.WithTable(...).InnerJoin(...)等效，用法:
    /// <code>
    /// .InnerJoin((a, b, c, d) =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select((x, y) =&gt; new { ... }), (a, b, c, d) =&gt; ...) ...
    /// SQL:
    /// ... INNER JOIN (SELECT ... FROM `sys_order_detail` ...) c ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, TOther> InnerJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做LEFT JOIN关联，与.WithTable(...).LeftJoin(...)等效，用法:
    /// <code>
    /// .LeftJoin((a, b, c, d) =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select((x, y) =&gt; new { ... }), (a, b, c, d) =&gt; ...) ...
    /// SQL:
    /// ... LEFT JOIN (SELECT ... FROM `sys_order_detail` ...) c ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, TOther> LeftJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做RIGHT JOIN关联，与.WithTable(...).RightJoin(...)等效，用法:
    /// <code>
    /// .RightJoin((a, b, c, d) =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select((x, y) =&gt; new { ... }), (a, b, c, d) =&gt; ...) ...
    /// SQL:
    /// ... RIGHT JOIN (SELECT ... FROM `sys_order_detail` ...) c ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, TOther> RightJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4> Where(Expression<Func<T1, T2, T3, T4, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4> Where(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4> And(Expression<Func<T1, T2, T3, T4, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4> And(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null);
    #endregion

    #region GroupBy
    /// <summary>
    /// 分组查询，分组表达式groupingExpr可以是单个字段或多个字段的匿名对象，用法:
    /// <code>
    /// repository.From&lt;User&gt;() ...
    ///    .GroupBy((a, b, ...) =&gt; new { a.Id, a.Name, b.CreatedAt.Date }) //或是 .GroupBy((a, b, ...) =&gt; a.CreatedAt.Date)
    ///    .Select((x, a, b, ...) =&gt; new
    ///    {
    ///        x.Grouping, //可以直接返回分组对象，也可以返回分组对象的某个字段,如：a.Id, a.Name, b.CreatedAt.Date，也可以 x.Grouping.Id, x.Grouping.Name, x.Grouping.Date ...
    ///        OrderCount = x.Count(b.Id), //也可以返回分组后的聚合操作
    ///        TotalAmount = x.Sum(b.TotalAmount) //也可以返回分组后的聚合操作
    ///    })
    ///    .ToSql(out _);
    /// SQL:
    /// SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `Date`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a ... GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TGrouping">分组后的实体对象类型，可以是单个字段类型或是匿名类型</typeparam>
    /// <param name="groupingExpr">分组表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IGroupingCommand<T1, T2, T3, T4, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, TGrouping>> groupingExpr);
    #endregion

    #region OrderBy
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderBy((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr);
    /// <summary>
    /// DSC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，用法：
    /// Select((a, b, ...) =&gt; new { a.Id, a.Name, ... }) 或是 Select((a, b, ...) =&gt; x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, TTarget>> fieldsExpr);
    #endregion
}
public interface IFromCommand<T1, T2, T3, T4, T5> : IFromCommand
{
    #region Sharding
    /// <summary>
    /// 使用固定表名确定T5表一个或多个分表名执行查询，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")，按月分表
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5> UseTable(params string[] tableNames);
    /// <summary>
    /// 使用表名断言确定T5表一个或多个分表执行查询，完整的表名，如：.UseTable(f =&gt; f.Contains("202001"))，按月分表
    /// </summary>
    /// <param name="tableNamePredicate">表名断言，如：f =&gt; f.Contains("202001")</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5> UseTable(Func<string, bool> tableNamePredicate);
    /// <summary>
    /// 根据字段值确定T5表分表名，最多支持2个字段，字段值的顺序与配置的字段顺序保持一致，可多次调用
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="field2Value">字段2值</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5> UseTableBy(object field1Value, object field2Value = null);
    /// <summary>
    /// 根据单个字段值范围确定T表分表名执行查询，通常是日期规则分表使用，如：repository.From&lt;Order&gt;().UseTableByRange(DateTime.Parse("2020-01-01"), DateTime.Now)
    /// </summary>
    /// <param name="beginFieldValue">字段起始值</param>
    /// <param name="endFieldValue">字段结束值</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5> UseTableByRange(object beginFieldValue, object endFieldValue);
    /// <summary>
    /// 根据1个固定字段值和1个字段值范围确定T5表分表名执行查询，字段值的顺序与配置的字段顺序保持一致，通常是日期规则分表使用，
    /// 如：配置时 .UseSharding(s =&gt;s.UseTable&lt;Order&gt;(t =&gt; t.DependOn(d =&gt; d.TenantId).DependOn(d =&gt; d.CreatedAt).UseRule((dbKey, origName, tenantId, dateTime) =&gt; $"{origName}_{tenantId}_{dateTime:yyyMM}")
    /// .UseRangeRule((dbKey, origName, tenantId, beginTime, endTime) =&gt;{ ...}))，此处使用 repository.From&lt;Order&gt;().UseTableByRange("tenant001", DateTime.Parse("2020-01-01"), DateTime.Now)
    /// </summary>
    /// <param name="fieldValue1">第一个值</param>
    /// <param name="fieldValue2">第二个值</param>
    /// <param name="fieldValue3">第三个值</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3);
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5> UseTableSchema(string tableSchema);
    #endregion

    #region Join
    /// <summary>
    /// 在现有表中，指定2个表进行INNER JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// .InnerJoin((a, b, c, d, e) =&gt; ...)
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn);
    /// <summary>
    /// 在现有表中，指定2个表进行LEFT JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>.LeftJoin((a, b, c, d, e) =&gt; ...)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn);
    /// <summary>
    /// 在现有表中，指定2个表进行RIGHT JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// .RightJoin((a, b, c, d, e) =&gt; ...)
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5> RightJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其进行INNER JOIN关联，用法:
    /// <code>
    /// .InnerJoin&lt;TOther&gt;((a, b, c, d, e) =&gt; ...)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其做LEFT JOIN关联，用法:
    /// <code>
    /// .LeftJoin&lt;TOther&gt;((a, b, c, d, e) =&gt; ...)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其做RIGHT JOIN关联，用法:
    /// <code>
    /// .RightJoin&lt;TOther&gt;((a, b, c, d, e) =&gt; ...)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn);
    /// <summary>
    /// 添加子查询临时表subQuery，并与现有表T做INNER JOIN关联，可以用在CTE子句中自我引用，用法:
    /// <code>
    /// repository.FromWith(...).NextWith(...)
    ///     .UnionAllRecursive((x, self) =&gt; x.From&lt;Menu&gt;()
    ///         .InnerJoin(self, (a, b, c, d, e) =&gt; a.ParentId == b.Id)
    ///         .Select(...), "MenuList") ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型，子查询中通常会有SELECT操作，返回的类型是一个匿名类</typeparam>
    /// <param name="subQuery">子查询对象，也可以CTE表的自我引用</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn);
    /// <summary>
    /// 添加子查询临时表subQuery，并与现有表T做LEFT JOIN关联，可以用在CTE子句中自我引用，用法:
    /// <code>
    /// repository.FromWith(...).NextWith(...)
    ///     .UnionAllRecursive((x, self) =&gt; x.From&lt;Menu&gt;()
    ///         .LeftJoin(self, (a, b, c, d, e) =&gt; a.ParentId == b.Id)
    ///         .Select(...), "MenuList") ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型，子查询中通常会有SELECT操作，返回的类型是一个匿名类</typeparam>
    /// <param name="subQuery">子查询对象，也可以CTE表的自我引用</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn);
    /// <summary>
    /// 添加子查询临时表subQuery，并与现有表T做LEFT JOIN关联，可以用在CTE子句中自我引用，用法:
    /// <code>
    /// repository.FromWith(...).NextWith(...)
    ///     .UnionAllRecursive((x, self) =&gt; x.From&lt;Menu&gt;()
    ///         .RightJoin(self, (a, b, c, d, e) =&gt; a.ParentId == b.Id)
    ///         .Select(...), "MenuList") ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型，子查询中通常会有SELECT操作，返回的类型是一个匿名类</typeparam>
    /// <param name="subQuery">子查询对象，也可以CTE表的自我引用</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做INNER JOIN关联，与.WithTable(...).InnerJoin(...)等效，用法:
    /// <code>
    /// .InnerJoin((a, b, c, d, e) =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select((x, y) =&gt; new { ... }), (a, b, c, d, e) =&gt; ...) ...
    /// SQL:
    /// ... INNER JOIN (SELECT ... FROM `sys_order_detail` ...) c ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做LEFT JOIN关联，与.WithTable(...).LeftJoin(...)等效，用法:
    /// <code>
    /// .LeftJoin((a, b, c, d, e) =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select((x, y) =&gt; new { ... }), (a, b, c, d, e) =&gt; ...) ...
    /// SQL:
    /// ... LEFT JOIN (SELECT ... FROM `sys_order_detail` ...) c ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做RIGHT JOIN关联，与.WithTable(...).RightJoin(...)等效，用法:
    /// <code>
    /// .RightJoin((a, b, c, d, e) =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select((x, y) =&gt; new { ... }), (a, b, c, d, e) =&gt; ...) ...
    /// SQL:
    /// ... RIGHT JOIN (SELECT ... FROM `sys_order_detail` ...) c ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(Func<IFromQuery, IQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5> Where(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5> And(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null);
    #endregion

    #region GroupBy
    /// <summary>
    /// 分组查询，分组表达式groupingExpr可以是单个字段或多个字段的匿名对象，用法:
    /// <code>
    /// repository.From&lt;User&gt;() ...
    ///    .GroupBy((a, b, ...) =&gt; new { a.Id, a.Name, b.CreatedAt.Date }) //或是 .GroupBy((a, b, ...) =&gt; a.CreatedAt.Date)
    ///    .Select((x, a, b, ...) =&gt; new
    ///    {
    ///        x.Grouping, //可以直接返回分组对象，也可以返回分组对象的某个字段,如：a.Id, a.Name, b.CreatedAt.Date，也可以 x.Grouping.Id, x.Grouping.Name, x.Grouping.Date ...
    ///        OrderCount = x.Count(b.Id), //也可以返回分组后的聚合操作
    ///        TotalAmount = x.Sum(b.TotalAmount) //也可以返回分组后的聚合操作
    ///    })
    ///    .ToSql(out _);
    /// SQL:
    /// SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `Date`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a ... GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TGrouping">分组后的实体对象类型，可以是单个字段类型或是匿名类型</typeparam>
    /// <param name="groupingExpr">分组表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IGroupingCommand<T1, T2, T3, T4, T5, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, TGrouping>> groupingExpr);
    #endregion

    #region OrderBy
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderBy((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    /// <summary>
    /// DSC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，用法：
    /// Select((a, b, ...) =&gt; new { a.Id, a.Name, ... }) 或是 Select((a, b, ...) =&gt; x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, TTarget>> fieldsExpr);
    #endregion
}
public interface IFromCommand<T1, T2, T3, T4, T5, T6> : IFromCommand
{
    #region Sharding
    /// <summary>
    /// 使用固定表名确定T6表一个或多个分表名执行查询，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")，按月分表
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, T6> UseTable(params string[] tableNames);
    /// <summary>
    /// 使用表名断言确定T6表一个或多个分表执行查询，完整的表名，如：.UseTable(f =&gt; f.Contains("202001"))，按月分表
    /// </summary>
    /// <param name="tableNamePredicate">表名断言，如：f =&gt; f.Contains("202001")</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, T6> UseTable(Func<string, bool> tableNamePredicate);
    /// <summary>
    /// 根据字段值确定T6表分表名，最多支持2个字段，字段值的顺序与配置的字段顺序保持一致，可多次调用
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="field2Value">字段2值</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, T6> UseTableBy(object field1Value, object field2Value = null);
    /// <summary>
    /// 根据单个字段值范围确定T表分表名执行查询，通常是日期规则分表使用，如：repository.From&lt;Order&gt;().UseTableByRange(DateTime.Parse("2020-01-01"), DateTime.Now)
    /// </summary>
    /// <param name="beginFieldValue">字段起始值</param>
    /// <param name="endFieldValue">字段结束值</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, T6> UseTableByRange(object beginFieldValue, object endFieldValue);
    /// <summary>
    /// 根据1个固定字段值和1个字段值范围确定T6表分表名执行查询，字段值的顺序与配置的字段顺序保持一致，通常是日期规则分表使用，
    /// 如：配置时 .UseSharding(s =&gt;s.UseTable&lt;Order&gt;(t =&gt; t.DependOn(d =&gt; d.TenantId).DependOn(d =&gt; d.CreatedAt).UseRule((dbKey, origName, tenantId, dateTime) =&gt; $"{origName}_{tenantId}_{dateTime:yyyMM}")
    /// .UseRangeRule((dbKey, origName, tenantId, beginTime, endTime) =&gt;{ ...}))，此处使用 repository.From&lt;Order&gt;().UseTableByRange("tenant001", DateTime.Parse("2020-01-01"), DateTime.Now)
    /// </summary>
    /// <param name="fieldValue1">第一个值</param>
    /// <param name="fieldValue2">第二个值</param>
    /// <param name="fieldValue3">第三个值</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, T6> UseTableByRange(object fieldValue1, object fieldValue2, object fieldValue3);
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, T6> UseTableSchema(string tableSchema);
    #endregion

    #region Join
    /// <summary>
    /// 在现有表中，指定2个表进行INNER JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// .InnerJoin((a, b, c, d, e, f) =&gt; ...)
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, T6> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn);
    /// <summary>
    /// 在现有表中，指定2个表进行LEFT JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>.LeftJoin((a, b, c, d, e, f) =&gt; ...)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, T6> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn);
    /// <summary>
    /// 在现有表中，指定2个表进行RIGHT JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// .RightJoin((a, b, c, d, e, f) =&gt; ...)
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, T6> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, T6> Where(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, T6> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, T6> And(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, T6> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null);
    #endregion

    #region GroupBy
    /// <summary>
    /// 分组查询，分组表达式groupingExpr可以是单个字段或多个字段的匿名对象，用法:
    /// <code>
    /// repository.From&lt;User&gt;() ...
    ///    .GroupBy((a, b, ...) =&gt; new { a.Id, a.Name, b.CreatedAt.Date }) //或是 .GroupBy((a, b, ...) =&gt; a.CreatedAt.Date)
    ///    .Select((x, a, b, ...) =&gt; new
    ///    {
    ///        x.Grouping, //可以直接返回分组对象，也可以返回分组对象的某个字段,如：a.Id, a.Name, b.CreatedAt.Date，也可以 x.Grouping.Id, x.Grouping.Name, x.Grouping.Date ...
    ///        OrderCount = x.Count(b.Id), //也可以返回分组后的聚合操作
    ///        TotalAmount = x.Sum(b.TotalAmount) //也可以返回分组后的聚合操作
    ///    })
    ///    .ToSql(out _);
    /// SQL:
    /// SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `Date`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a ... GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TGrouping">分组后的实体对象类型，可以是单个字段类型或是匿名类型</typeparam>
    /// <param name="groupingExpr">分组表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, TGrouping>> groupingExpr);
    #endregion

    #region OrderBy
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderBy((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, T6> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, T6> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr);
    /// <summary>
    /// DSC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, T6> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, T6> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，用法：
    /// Select((a, b, ...) =&gt; new { a.Id, a.Name, ... }) 或是 Select((a, b, ...) =&gt; x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr);
    #endregion
}