using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

/// <summary>
/// 查询对象
/// </summary>
public interface IFromCommand
{
    #region Properties
    /// <summary>
    /// DbContext对象
    /// </summary>
    DbContext DbContext { get; }
    /// <summary>
    /// Visitor对象
    /// </summary>
    IQueryVisitor Visitor { get; }
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
    /// 直接指定<typeparamref name="T"/>表分表名，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T> UseTable(params string[] tableNames);
    /// <summary>
    /// 手动指定分表规则参数值，执行分表规则确定<typeparamref name="T"/>表分表名，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T> UseTableBy(params object[] fieldValues);
    /// <summary>
    /// 手动指定分表范围规则参数值数组，手动指定<typeparamref name="T"/>表分表名执行查询，参数值的顺序与配置的分表范围规则参数值数组元素顺序保持一致，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素，元素个数&gt;=2，最后两个字段值是范围值，并确保fieldValues[n-1] &lt;= fieldValues[n]，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T> UseTableByRange(params object[] fieldValues);
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
    /// <summary>
    /// Union操作，去掉重复记录，如：
    /// <code>
    /// var subQuery = repository.From&lt;Order&gt;()
    ///     .Where(x =&gt; x.Id &gt; 1)
    ///     .Select(x =&gt; new { ... });
    /// await repository.From&lt;Order&gt;() ...
    ///     .Union(subQuery).ToListAsync();
    /// SQL:
    /// SELECT ... FROM `sys_order` ... UNION
    /// SELECT ... FROM `sys_order` WHERE `Id`&gt;1
    /// </code>
    /// </summary>
    /// <param name="subQuery">子查询，需要有Select语句，如：<code>repository.From&lt;Order&gt;() ... .Select(x =&gt; new { ... })</code>
    /// </param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T> Union(IQuery<T> subQuery);
    /// <summary>
    /// Union操作，去掉重复记录，如：
    /// <code>
    /// await repository.From&lt;Order&gt;() ...
    ///     .Union(f =&gt; f.From&lt;Order&gt;()
    ///         .Where(x =&gt; x.Id &gt; 1)
    ///         .Select(x =&gt; new { ... }))
    ///     .ToListAsync();
    /// SQL:
    /// SELECT ... FROM `sys_order` ... UNION
    /// SELECT ... FROM `sys_order` WHERE `Id`&gt;1
    /// </code>
    /// </summary>
    /// <param name="subQueryExpr">子查询表达式，需要有Select语句，如：<code>f.From&lt;Order&gt;() ... .Select(x =&gt; new { ... })</code>
    /// <returns>返回查询对象</returns>
    IFromCommand<T> Union(Expression<Func<IFromQuery, IQuery<T>>> subQueryExpr);
    /// <summary>
    /// Union All操作，所有记录不去掉重复，如：
    /// <code>
    /// var subQuery = repository.From&lt;Order&gt;() ...
    ///     .Select(x =&gt; new { ... })
    /// await repository.From&lt;Order&gt;() ...
    ///     .UnionAll(subQuery).ToListAsync();
    /// SQL:
    /// SELECT ... FROM `sys_order` ... UNION ALL
    /// SELECT ... FROM `sys_order` ...
    /// </code>
    /// </summary>
    /// <param name="subQuery">子查询，需要有Select语句，如：<code>repository.From&lt;Order&gt;() ... .Select(x =&gt; new { ... })</code>
    /// </param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T> UnionAll(IQuery<T> subQuery);
    /// <summary>
    /// Union All操作，所有记录不去掉重复，如：
    /// <code>
    /// await repository.From&lt;Order&gt;() ...
    ///     .UnionAll(f =&gt; f.From&lt;Order&gt;() ...
    ///         .Select(x =&gt; new { ... }))
    ///     .ToListAsync();
    /// SQL:
    /// SELECT ... FROM `sys_order` ... UNION ALL
    /// SELECT ... FROM `sys_order` WHERE `Id`&gt;1
    /// </code>
    /// </summary>
    /// <param name="subQueryExpr">子查询表达式，需要有Select语句，如：<code>f.From&lt;Order&gt;() ... .Select(x =&gt; new { ... })</code>
    /// <returns>返回查询对象</returns>
    IFromCommand<T> UnionAll(Expression<Func<IFromQuery, IQuery<T>>> subQueryExpr);
    /// <summary>
    /// 递归CTE子查询中的Union操作，表达式subQueryExpr中的第二参数是CTE自身引用，如：
    /// <code>
    /// ... f.From&lt;Menu&gt;() ...
    ///         .Select(x =&gt; new { ... })
    ///     .UnionRecursive((x, self) =&gt; x.From&lt;Menu&gt;()
    ///         .InnerJoin(self, (a, b) =&gt; a.ParentId == b.Id)
    ///         .Select((a, b) =&gt; new { ... }))) ...
    /// SQL:
    /// WITH RECURSIVE `myCteTable`(`Id`,`Name`,`ParentId`,`PageId`) AS 
    /// (
    /// SELECT ... FROM `sys_menu` a WHERE a.`Id`=1 UNION
    /// SELECT ... FROM `sys_menu` a INNER JOIN `myCteTable` b ON a.`ParentId`=b.`Id` ...
    /// ) ...
    /// </code>
    /// </summary>
    /// <param name="subQueryExpr">子查询表达式，需要有Select语句，如：<code>f.From&lt;Menu&gt;().Where(x =&gt; ... ).Select(x =&gt; new { ... })</code>
    /// </param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T> UnionRecursive(Expression<Func<IFromQuery, IQuery<T>, IQuery<T>>> subQueryExpr);
    /// <summary>
    /// 递归CTE子查询中的UnionAll操作，表达式subQueryExpr中的第二参数是CTE自身引用，如：
    /// <code>
    /// ... f.From&lt;Menu&gt;() ...
    ///         .Select(x =&gt; new { ... })
    ///     .UnionAllRecursive((x, self) =&gt; x.From&lt;Menu&gt;()
    ///         .InnerJoin(self, (a, b) =&gt; a.ParentId == b.Id)
    ///         .Select((a, b) =&gt; new { ... }))) ...
    /// SQL:
    /// WITH RECURSIVE `myCteTable`(`Id`,`Name`,`ParentId`,`PageId`) AS 
    /// (
    /// SELECT ... FROM `sys_menu` a WHERE a.`Id`=1 UNION ALL
    /// SELECT ... FROM `sys_menu` a INNER JOIN `myCteTable` b ON a.`ParentId`=b.`Id` ...
    /// ) ...
    /// </summary>
    /// <param name="subQueryExpr">子查询表达式，需要有Select语句，如：<code>f.From&lt;Menu&gt;() .Where(x =&gt; ... ) .Select(x =&gt; new { ... })</code>
    /// </param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T> UnionAllRecursive(Expression<Func<IFromQuery, IQuery<T>, IQuery<T>>> subQueryExpr);
    #endregion

    #region WithTable
    /// <summary>
    /// 添加实体表，方便后面做JOIN关联，如：<code>.WithTable&lt;Page&gt;()</code>
    /// </summary>
    /// <typeparam name="TOther">实体表类型</typeparam>
    /// <returns>返回查询对象</returns>
    IFromCommand<T, TOther> WithTable<TOther>();
    #endregion

    #region WithQuery
    /// <summary>
    /// 添加子查询，方便后面做JOIN关联，如：
    /// <code>
    /// var subQuery = repository.From&lt;Menu&gt;() ... .Select( ...);
    /// .From&lt;Menu&gt;().WithQuery(subQuery)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T, TOther> WithQuery<TOther>(IQuery<TOther> subQuery);
    /// <summary>
    /// 添加子查询，方便后面做JOIN关联，如：
    /// <code>
    /// .From&lt;Menu&gt;().WithQuery(f =&gt; f.From&lt;Page, Menu&gt;('c') ... )  
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr);
    #endregion

    #region InnerJoin
    /// <summary>
    /// 添加<typeparamref name="TOther"/>表，与现有表<typeparamref name="T"/>做INNER JOIN关联，与.WithTable&lt;TOther&gt;().InnerJoin(...)等价，如：
    /// <code>
    /// .From&lt;User&gt;().InnerJoin&lt;Order&gt;((x, y) =&gt; x.Id == y.BuyerId)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T, TOther> InnerJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn);
    /// <summary>
    /// 添加子查询subQuery，并与现有表<typeparamref name="T"/>做INNER JOIN关联，也可以用在CTE子句中自我引用，与.WithQuery(subQuery).InnerJoin(...)等价，如：
    /// <code>
    /// var subQuery = repository.From&lt;Menu&gt;() ... .Select( ...);
    /// .InnerJoin(subQuery, (a, b) =&gt; a.ParentId == b.Id)
    /// .UnionAllRecursive((x, self) =&gt; x.From&lt;Menu&gt;()
    ///     .InnerJoin(self, (a, b) =&gt; a.ParentId == b.Id) )) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型，子查询中通常会有SELECT操作，返回的类型是一个匿名类</typeparam>
    /// <param name="subQuery">子查询对象，也可以CTE表的自我引用</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr子查询，并与现有表<typeparamref name="T"/>做INNER JOIN关联，与.WithQuery(subQueryExpr).InnerJoin(...)等价，如：
    /// <code>
    /// ... .InnerJoin(f =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select((x, y) =&gt; new { ... }), (a, b) =&gt; a.Id == b.OrderId) ...
    /// SQL：
    /// ... a INNER JOIN (SELECT ... FROM `sys_order_detail` ...) b ON a.`Id`=b.`OrderId` ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型，子查询中通常会有SELECT操作，返回的类型是一个匿名类</typeparam>
    /// <param name="subQueryExpr">子查询表达式</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T, TOther, bool>> joinOn);
    #endregion

    #region LeftJoin
    /// <summary>
    /// 添加<typeparamref name="TOther"/>表，与现有表<typeparamref name="T"/>做LEFT JOIN关联，与.WithTable&lt;TOther&gt;().LeftJoin(...)等价，如：
    /// <code>
    /// repository.From&lt;User&gt;()
    ///     .LeftJoin&lt;Order&gt;((x, y) =&gt; x.Id == y.BuyerId)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T, TOther> LeftJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn);
    /// <summary>
    /// 添加子查询subQuery，并与现有表<typeparamref name="T"/>做LEFT JOIN关联，可以用在CTE子句中自我引用，与.WithQuery(subQueryExpr).LeftJoin(...)等价，如：
    /// <code>
    /// var subQuery = repository.From&lt;Menu&gt;() ... .Select( ...);
    /// .LeftJoin(subQuery, (a, b) =&gt; a.ParentId == b.Id)
    /// .UnionAllRecursive((x, self) =&gt; x.From&lt;Menu&gt;()
    ///     .LeftJoin(self, (a, b) =&gt; a.ParentId == b.Id) )) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型，子查询中通常会有SELECT操作，返回的类型是一个匿名类</typeparam>
    /// <param name="subQuery">子查询对象，也可以CTE表的自我引用</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr子查询，并与现有表<typeparamref name="T"/>做LEFT JOIN关联，如：
    /// <code>
    /// .LeftJoin(f =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select((x, y) =&gt; ...), (a, b, c) =&gt; b.Id == c.OrderId)
    /// SQL：... LEFT JOIN (SELECT ... FROM `sys_order_detail` a ...) c ON b.`Id`=c.`OrderId` ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型，子查询中通常会有SELECT操作，返回的类型是一个匿名类</typeparam>
    /// <param name="subQueryExpr">子查询表达式</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T, TOther, bool>> joinOn);
    #endregion

    #region RightJoin
    /// <summary>
    /// 添加<typeparamref name="TOther"/>表，与现有表<typeparamref name="T"/>做RIGHT JOIN关联，与.WithTable&lt;TOther&gt;().RightJoin(...)等价，如：
    /// <code>
    /// .From&lt;User&gt;().RightJoin&lt;Order&gt;((x, y) =&gt; x.Id == y.BuyerId)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T, TOther> RightJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn);
    /// <summary>
    /// 添加子查询subQuery，并与现有表<typeparamref name="T"/>做LEFT JOIN关联，可以用在CTE子句中自我引用，与.WithQuery(subQuery).RightJoin(...)等价，如：
    /// <code>
    /// var subQuery = repository.From&lt;Menu&gt;() ... .Select( ...);
    /// .RightJoin(subQuery, (a, b) =&gt; a.ParentId == b.Id)
    /// .UnionAllRecursive((x, self) =&gt; x.From&lt;Menu&gt;()
    ///     .RightJoin(self, (a, b) =&gt; a.ParentId == b.Id) )) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型，子查询中通常会有SELECT操作，返回的类型是一个匿名类</typeparam>
    /// <param name="subQuery">子查询对象，也可以CTE表的自我引用</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr子查询，并与现有表<typeparamref name="T"/>做RIGHT JOIN关联，，与.WithQuery(subQueryExpr).RightJoin(...)等价，如：
    /// <code>
    /// .RightJoin(f =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select( ... ), (a, b, c) =&gt; b.Id == c.OrderId)
    /// SQL：... RIGHT JOIN (SELECT ... FROM ...) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型，子查询中通常会有SELECT操作，返回的<paramref name="TOther"/>类型是一个匿名类</typeparam>
    /// <param name="subQueryExpr">子查询表达式</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T, TOther, bool>> joinOn);
    #endregion

    #region Where
    /// <summary>
    /// 条件查询，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T> Where(Expression<Func<T, bool>> predicate);
    /// <summary>
    /// 条件查询，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T> Where(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T> WherePredicate(Func<PredicateBuilder<T>, Expression<Func<T, bool>>> predicateInitializer);
    #endregion

    #region And
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T> And(Expression<Func<T, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T> And(bool condition, Expression<Func<T, bool>> ifPredicate = null, Expression<Func<T, bool>> elsePredicate = null);
    /// <summary>
    /// 条件查询，构造表达式断言predicateInitializer生成Where条件，并与已有的条件AND操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T> AndPredicate(Func<PredicateBuilder<T>, Expression<Func<T, bool>>> predicateInitializer);
    #endregion

    #region Or
    /// <summary>
    /// 条件查询，并与已有的条件OR操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T> Or(Expression<Func<T, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件OR操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T> Or(bool condition, Expression<Func<T, bool>> ifPredicate = null, Expression<Func<T, bool>> elsePredicate = null);
    /// <summary>
    /// 条件查询，构造表达式断言predicateInitializer生成Where条件，并与已有的条件OR操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T> OrPredicate(Func<PredicateBuilder<T>, Expression<Func<T, bool>>> predicateInitializer);
    #endregion

    #region GroupBy
    /// <summary>
    /// 分组查询，分组表达式groupingExpr可以是单个或多个字段的匿名对象，如：
    /// <code>
    /// .GroupBy(f =&gt; new { f.Id, f.Name, f.CreatedAt.Date })
    /// SQL: ... GROUP BY a.`Id`,a.`Name`,CONVERT(a.`CreatedAt`,DATE) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TGrouping">分组后的实体对象类型，New类型表达式，可以一个或是多个字段</typeparam>
    /// <param name="groupingExpr">分组表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IGroupingCommand<T, TGrouping> GroupBy<TGrouping>(Expression<Func<T, TGrouping>> groupingExpr);
    #endregion

    #region OrderBy/OrderByDescending
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderBy(f =&gt; new { f.Id, f.OtherId }) 或是 .OrderBy(x =&gt; x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T> OrderBy<TFields>(Expression<Func<T, TFields>> fieldsExpr);
    /// <summary>
    /// ASC排序，condition为true，ASC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderBy(true, f =&gt; new { f.Id, f.OtherId }) 或是 .OrderBy(true, x =&gt; x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T> OrderBy<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr);
    /// <summary>
    /// DSC排序，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderByDescending(f =&gt; new { f.Id, f.OtherId }) 或是 .OrderByDescending(x =&gt; x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T> OrderByDescending<TFields>(Expression<Func<T, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，condition为true，DESC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderByDescending(true, f =&gt; new { f.Id, f.OtherId }) 或是 .OrderByDescending(true, x =&gt; x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T> OrderByDescending<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr);
    /// <summary>
    /// 动态排序，使用OrderByBuilder构建排序字段选择器，fieldsGetter可以是单个或多个字段的匿名对象，如：
    /// <code>string orderFields = "Name";bool isAsc = true;
    /// .OrderByDynamic(t =&gt; t.Switch(orderFields, isAsc).When("Name", f =&lt; f.Name).When("Gender", f =&lt; f.Gender).Build()</code>)
    /// </summary>
    /// <param name="fieldsGetter">排序字段选择器，需调用Build方法，返回排序字段表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T> OrderByDynamic(Func<OrderByBuilder<T>, Expression> fieldsGetter);
    #endregion

    #region Skip/Take/Page
    /// <summary>
    /// 跳过offset条数据
    /// </summary>
    /// <param name="offset">要跳过的数据条数</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T> Skip(int offset);
    /// <summary>
    /// 只返回limit条数据
    /// </summary>
    /// <param name="limit">返回的数据条数</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T> Take(int limit);
    /// <summary>
    /// 分页查询，pageNumber从1开始
    /// </summary>
    /// <param name="pageNumber">第几页，从1开始，小于1时当作1处理</param>
    /// <param name="pageSize">每页显示条数</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T> Page(int pageNumber, int pageSize);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段，如：
    /// .Select() 或是 .Select("*") 或是 .Select("Id, Name ...")
    /// </summary>
    /// <param name="fields">原始字段字符串，默认值*</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T> Select(string fields = "*");
    /// <summary>
    /// 选择指定字段返回，可以是单个或多个字段的匿名对象，如：
    /// <code> .Select(f =&gt; new { f.Id, f.Name }) 或是 .Select(x =&gt; x.CreatedAt.Date)</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<TTarget> Select<TTarget>(Expression<Func<T, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定字段返回，只需要指定需要特殊处理的成员赋值即可，其他成员将从现有表的字段中按名称匹配赋值，多个同名字段时如果未特殊指定赋值，默认选取第一个表中的字段赋值。如：
    /// <code> .SelectTo&lt;TDto&gt;() 或是 .SelectTo((a, b) =&gt; new TDto{ b.Id }) //使用第二个表的Id字段作为Id成员</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="specialMemberSelector">特殊成员赋值表达式，通常是重名字段或是不存在的字段赋值</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<TTarget> SelectTo<TTarget>(Expression<Func<T, TTarget>> specialMemberSelector = null);
    /// <summary>
    /// 选择指定聚合字段返回，可以是单个或多个聚合字段的匿名对象，如：
    /// <code>
    /// .SelectAggregate((x, a) =&gt; new
    /// {
    ///     OrderCount = x.Count(a.Id),
    ///     TotalAmount = x.Sum(a.TotalAmount)
    /// })
    /// SQL: COUNT(a.`Id`) AS `OrderCount`,SUM(a.`TotalAmount`) AS `TotalAmount`
    /// </code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型，通常是一个匿名类</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个或多个聚合字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T, TTarget>> fieldsExpr);
    #endregion

    #region Distinct
    /// <summary>
    /// DISTINCT去掉重复数据
    /// </summary>
    /// <returns>返回查询对象</returns>
    IFromCommand<T> Distinct();
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
}
/// <summary>
/// 多表T1, T2查询
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
public interface IFromCommand<T1, T2> : IFromCommand
{
    #region Sharding
    /// <summary>
    /// 直接指定T2表分表名，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2> UseTable(params string[] tableNames);
    /// <summary>
    /// 根据首个分表TMasterSharding表与当前T2表名的映射关系，指定当前T2表分表名获取委托，执行委托获取当前T2表分表名。委托第一个参数是<typeparamref name="TMasterSharding"/>主表原始表名，第二个参数是当前T2表原始表名，第三个参数是TMasterSharding表当前分表名，返回值是当前T2表分表名称，如：
    /// <code>
    /// .From&lt;Order&gt;().UseTable("sys_order_104_202405", "sys_order_105_202405")
    /// .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    /// .UseTableMap&lt;Order&gt;((orderOrigName, userOrigName, orderTableName) =&gt;
    /// {
    ///     //sys_order_104_202405 -&gt; sys_user_104, sys_order_105_202405 -&gt; sys_user_105
    ///     var tableName = orderTableName.Replace(orderOrigName, userOrigName);
    ///     return tableName.Substring(0, tableName.Length - 7);
    /// }) ...
    /// SQL:
    /// SELECT ... FROM `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` ...
    /// UNION ALL
    /// SELECT ... FROM `sys_order_105_202405` a INNER JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` ...
    /// </code>
    /// </summary>
    /// <typeparam name="TMasterSharding">TMasterSharding主表分表实体类型</typeparam>
    /// <param name="tableNameGetter"><typeparamref name="T2"/>表分表名获取委托</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter);
    /// <summary>
    /// 手动指定分表规则参数值，执行分表规则确定<typeparamref name="T2"/>表分表名，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2> UseTableBy(params object[] fieldValues);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定<typeparamref name="T2"/>表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginFieldValue &lt;= endFieldValue，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2> UseTableByRange(params object[] fieldValues);
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2> UseTableSchema(string tableSchema);
    #endregion

    #region WithTable
    /// <summary>
    /// 添加实体表，方便后面做JOIN关联，如：<code>.From&lt;Menu&gt;().WithTable&lt;Page&gt;()</code>
    /// </summary>
    /// <typeparam name="TOther">实体表类型</typeparam>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, TOther> WithTable<TOther>();
    #endregion

    #region WithQuery
    /// <summary>
    /// 添加子查询，方便后面做JOIN关联，如：
    /// <code>
    /// var subQuery = repository.From&lt;Menu&gt;() ... .Select( ...);
    /// .From&lt;Menu&gt;().WithQuery(subQuery)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, TOther> WithQuery<TOther>(IQuery<TOther> subQuery);
    /// <summary>
    /// 添加子查询，方便后面做JOIN关联，如：
    /// <code>
    /// .From&lt;Menu&gt;()
    ///     .WithQuery(f =&gt; f.From&lt;Page, Menu&gt;('c') ... )  
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr);
    #endregion

    #region InnerJoin
    /// <summary>
    /// 在现有表中，选择两个表进行INNER JOIN关联，可多次关联，如：<code>.InnerJoin((a, b, ...) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2> InnerJoin(Expression<Func<T1, T2, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做INNER JOIN关联，与.WithTable&lt;TOther&gt;().InnerJoin(...)等价，如：
    /// <code>
    /// .From&lt;User&gt;().InnerJoin&lt;Order&gt;((x, y) =&gt; x.Id == y.BuyerId)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn);
    /// <summary>
    /// 添加子查询subQuery，并选择一个现有表做INNER JOIN关联，也可以用在CTE子句中自我引用，与.WithQuery(subQuery).InnerJoin(...)等价，如：
    /// <code>
    /// var subQuery = repository.From&lt;Menu&gt;() ... .Select( ...);
    /// .InnerJoin(subQuery, (a, b) =&gt; a.ParentId == b.Id)
    /// .UnionAllRecursive((x, self) =&gt; x.From&lt;Menu&gt;()
    ///     .InnerJoin(self, (a, b) =&gt; a.ParentId == b.Id) )) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型，子查询中通常会有SELECT操作，返回的类型是一个匿名类</typeparam>
    /// <param name="subQuery">子查询对象，也可以CTE表的自我引用</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr表达式构建子查询，并选择一个现有表做INNER JOIN关联，与.WithQuery(subQueryExpr).InnerJoin(...)等价，如：
    /// <code>
    /// .InnerJoin(f =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select(x =&gt; new { ... }), (a, b, ...) =&gt; x.xxx = y.yyy) ...
    /// SQL: ... INNER JOIN (SELECT ... FROM `sys_order_detail` ...) c ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, TOther, bool>> joinOn);
    #endregion

    #region LeftJoin
    /// <summary>
    /// 在现有表中，选择两个表进行LEFT JOIN关联，可多次关联，如：<code>.LeftJoin((a, b, ...) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2> LeftJoin(Expression<Func<T1, T2, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做LEFT JOIN关联，与.WithTable&lt;TOther&gt;().LeftJoin(...)等价，如：
    /// <code>
    /// .LeftJoin&lt;TOther&gt;((a, b, ...) =&gt; a.xxx = b.yyy)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn);
    /// <summary>
    /// 添加子查询subQuery，并选择一个现有表做LEFT JOIN关联，也可以用在CTE子句中自我引用，与.WithQuery(subQuery).LeftJoin(...)等价，如：
    /// <code>
    /// var subQuery = repository.From&lt;Menu&gt;() ... .Select( ...);
    /// .LeftJoin(subQuery, (a, b) =&gt; a.ParentId == b.Id)
    /// .UnionAllRecursive((x, self) =&gt; x.From&lt;Menu&gt;()
    ///     .LeftJoin(self, (a, b) =&gt; a.ParentId == b.Id) )) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型，子查询中通常会有SELECT操作，返回的类型是一个匿名类</typeparam>
    /// <param name="subQuery">子查询对象，也可以CTE表的自我引用</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr表达式构建子查询，并选择一个现有表做LEFT JOIN关联，与.WithQuery(subQueryExpr).LeftJoin(...)等价，如：
    /// <code>
    /// .LeftJoin(f =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select(t =&gt; new { ... }), (a, b, ...) =&gt; a.Id = b.OrderId) ...
    /// SQL: ... LEFT JOIN (SELECT ... FROM `sys_order_detail` ...) c ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, TOther, bool>> joinOn);
    #endregion

    #region RightJoin
    /// <summary>
    /// 在现有表中，选择两个表进行RIGHT JOIN关联，可多次关联，如：<code>.RightJoin((a, b) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2> RightJoin(Expression<Func<T1, T2, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做RIGHT JOIN关联，与.WithTable&lt;TOther&gt;().RightJoin(...)等价，如：
    /// <code>
    /// .RightJoin&lt;TOther&gt;((a, b) =&gt; ...)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, TOther> RightJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn);
    /// <summary>
    /// 添加子查询subQuery，并选择一个现有表做RIGHT JOIN关联，也可以用在CTE子句中自我引用，与.WithQuery(subQuery).RightJoin(...)等价，如：
    /// <code>
    /// var subQuery = repository.From&lt;Menu&gt;() ... .Select( ...);
    /// .LeftJoin(subQuery, (a, b) =&gt; a.ParentId == b.Id)
    /// .UnionAllRecursive((x, self) =&gt; x.From&lt;Menu&gt;()
    ///     .LeftJoin(self, (a, b) =&gt; a.ParentId == b.Id) )) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型，子查询中通常会有SELECT操作，返回的类型是一个匿名类</typeparam>
    /// <param name="subQuery">子查询对象，也可以CTE表的自我引用</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr表达式构建子查询，并选择一个现有表做RIGHT JOIN关联，与.WithQuery(subQueryExpr).RightJoin(...)等价，如：
    /// <code>
    /// .RightJoin(f =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select(t =&gt; new { ... }), (a, b, ...) =&gt; a.Id = b.OrderId) ...
    /// SQL: ... RIGHT JOIN (SELECT ... FROM `sys_order_detail` ...) c ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, TOther, bool>> joinOn);
    #endregion

    #region Where
    /// <summary>
    /// 条件查询，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2> Where(Expression<Func<T1, T2, bool>> predicate);
    /// <summary>
    /// 条件查询，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2> Where(bool condition, Expression<Func<T1, T2, bool>> ifPredicate, Expression<Func<T1, T2, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2> WherePredicate(Func<PredicateBuilder<T1, T2>, Expression<Func<T1, T2, bool>>> predicateInitializer);
    #endregion

    #region And
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2> And(Expression<Func<T1, T2, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2> And(bool condition, Expression<Func<T1, T2, bool>> ifPredicate = null, Expression<Func<T1, T2, bool>> elsePredicate = null);
    /// <summary>
    /// 条件查询，构造表达式断言predicateInitializer生成Where条件，并与已有的条件AND操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2> AndPredicate(Func<PredicateBuilder<T1, T2>, Expression<Func<T1, T2, bool>>> predicateInitializer);
    #endregion

    #region Or
    /// <summary>
    /// 条件查询，并与已有的条件OR操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2> Or(Expression<Func<T1, T2, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件OR操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2> Or(bool condition, Expression<Func<T1, T2, bool>> ifPredicate = null, Expression<Func<T1, T2, bool>> elsePredicate = null);
    /// <summary>
    /// 条件查询，构造表达式断言predicateInitializer生成Where条件，并与已有的条件OR操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2> OrPredicate(Func<PredicateBuilder<T1, T2>, Expression<Func<T1, T2, bool>>> predicateInitializer);
    #endregion

    #region GroupBy
    /// <summary>
    /// 分组查询，分组表达式groupingExpr可以是单个或多个字段的匿名对象，如：
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
    /// <param name="groupingExpr">分组表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IGroupingCommand<T1, T2, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, TGrouping>> groupingExpr);
    #endregion

    #region OrderBy
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// OrderBy((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderBy((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2> OrderBy<TFields>(Expression<Func<T1, T2, TFields>> fieldsExpr);
    /// <summary>
    /// ASC排序，condition为true，ASC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderBy(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, TFields>> fieldsExpr);
    /// <summary>
    /// DSC排序，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// OrderByDescending((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2> OrderByDescending<TFields>(Expression<Func<T1, T2, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，condition为true，DESC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, TFields>> fieldsExpr);
    /// <summary>
    /// 动态排序，使用OrderByBuilder构建排序字段选择器，fieldsGetter可以是单个或多个字段的匿名对象，如：
    /// <code>string orderFields = "Name";bool isAsc = true;
    /// .OrderByDynamic(t =&gt; t.Switch(orderFields, isAsc).When("Name", (a, b, ...) =&lt; a.Name).When("Gender", (a, b, ...) =&lt; a.Gender).Build()</code>)
    /// </summary>
    /// <param name="fieldsGetter">排序字段选择器，需调用Build方法，返回排序字段表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2> OrderByDynamic(Func<OrderByBuilder<T1, T2>, Expression> fieldsGetter);
    #endregion

    #region Skip/Take/Page
    /// <summary>
    /// 跳过offset条数据
    /// </summary>
    /// <param name="offset">要跳过的数据条数</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2> Skip(int offset);
    /// <summary>
    /// 只返回limit条数据
    /// </summary>
    /// <param name="limit">返回的数据条数</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2> Take(int limit);
    /// <summary>
    /// 分页查询，pageNumber从1开始
    /// </summary>
    /// <param name="pageNumber">第几页，从1开始，小于1时当作1处理</param>
    /// <param name="pageSize">每页显示条数</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2> Page(int pageNumber, int pageSize);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，如：
    /// Select((a, b, ...) =&gt; new { a.Id, a.Name, ... }) 或是 Select((a, b, ...) =&gt; x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<TTarget> Select<TTarget>(Expression<Func<T1, T2, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定字段返回，只需要指定需要特殊处理的成员赋值即可，其他成员将从现有表的字段中按名称匹配赋值，多个同名字段时如果未特殊指定赋值，默认选取第一个表中的字段赋值。如：
    /// <code> .SelectTo&lt;TDto&gt;() 或是 .SelectTo((a, b) =&gt; new TDto{ b.Id }) //使用第二个表的Id字段作为Id成员</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型，通常是一个匿名类</typeparam>
    /// <param name="specialMemberSelector">特殊成员赋值表达式，通常是重名字段或是不存在的字段赋值</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, TTarget>> specialMemberSelector = null);
    #endregion    
}
/// <summary>
/// 多表T1, T2, T3查询
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
public interface IFromCommand<T1, T2, T3> : IFromCommand
{
    #region Sharding
    /// <summary>
    /// 直接指定T3表分表名，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3> UseTable(params string[] tableNames);
    /// <summary>
    /// 根据首个分表TMasterSharding表与当前T3表名的映射关系，指定当前T3表分表名获取委托，执行委托获取当前T3表分表名。委托第一个参数是<typeparamref name="TMasterSharding"/>主表原始表名，第二个参数是当前T3表原始表名，第三个参数是TMasterSharding表当前分表名，返回值是当前T3表分表名称，如：
    /// <code>
    /// .From&lt;Order&gt;().UseTable("sys_order_104_202405", "sys_order_105_202405")
    /// .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    /// .UseTableMap&lt;Order&gt;((orderOrigName, userOrigName, orderTableName) =&gt;
    /// {
    ///     //sys_order_104_202405 -&gt; sys_user_104, sys_order_105_202405 -&gt; sys_user_105
    ///     var tableName = orderTableName.Replace(orderOrigName, userOrigName);
    ///     return tableName.Substring(0, tableName.Length - 7);
    /// }) ...
    /// SQL:
    /// SELECT ... FROM `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` ...
    /// UNION ALL
    /// SELECT ... FROM `sys_order_105_202405` a INNER JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` ...
    /// </code>
    /// </summary>
    /// <typeparam name="TMasterSharding">TMasterSharding主表分表实体类型</typeparam>
    /// <param name="tableNameGetter"><typeparamref name="T3"/>表分表名获取委托</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter);
    /// <summary>
    /// 手动指定分表规则参数值，执行分表规则确定<typeparamref name="T3"/>表分表名，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3> UseTableBy(params object[] fieldValues);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定<typeparamref name="T3"/>表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginFieldValue &lt;= endFieldValue，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3> UseTableByRange(params object[] fieldValues);
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3> UseTableSchema(string tableSchema);
    #endregion

    #region WithTable
    /// <summary>
    /// 添加实体表，方便后面做JOIN关联，如：<code>.From&lt;Menu&gt;().WithTable&lt;Page&gt;()</code>
    /// </summary>
    /// <typeparam name="TOther">实体表类型</typeparam>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, TOther> WithTable<TOther>();
    #endregion

    #region WithQuery
    /// <summary>
    /// 添加子查询，方便后面做JOIN关联，如：
    /// <code>
    /// var subQuery = repository.From&lt;Menu&gt;() ... .Select( ...);
    /// .From&lt;Menu&gt;().WithQuery(subQuery)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, TOther> WithQuery<TOther>(IQuery<TOther> subQuery);
    /// <summary>
    /// 添加子查询，方便后面做JOIN关联，如：
    /// <code>
    /// .From&lt;Menu&gt;()
    ///     .WithQuery(f =&gt; f.From&lt;Page, Menu&gt;('c') ... )  
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr);
    #endregion

    #region InnerJoin
    /// <summary>
    /// 在现有表中，选择两个表进行INNER JOIN关联，可多次关联，如：<code>.InnerJoin((a, b, ...) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3> InnerJoin(Expression<Func<T1, T2, T3, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做INNER JOIN关联，与.WithTable&lt;TOther&gt;().InnerJoin(...)等价，如：
    /// <code>
    /// .From&lt;User&gt;().InnerJoin&lt;Order&gt;((x, y) =&gt; x.Id == y.BuyerId)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn);
    /// <summary>
    /// 添加子查询subQuery，并选择一个现有表做INNER JOIN关联，也可以用在CTE子句中自我引用，与.WithQuery(subQuery).InnerJoin(...)等价，如：
    /// <code>
    /// var subQuery = repository.From&lt;Menu&gt;() ... .Select( ...);
    /// .InnerJoin(subQuery, (a, b) =&gt; a.ParentId == b.Id)
    /// .UnionAllRecursive((x, self) =&gt; x.From&lt;Menu&gt;()
    ///     .InnerJoin(self, (a, b) =&gt; a.ParentId == b.Id) )) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型，子查询中通常会有SELECT操作，返回的类型是一个匿名类</typeparam>
    /// <param name="subQuery">子查询对象，也可以CTE表的自我引用</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr表达式构建子查询，并选择一个现有表做INNER JOIN关联，与.WithQuery(subQueryExpr).InnerJoin(...)等价，如：
    /// <code>
    /// .InnerJoin(f =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select(x =&gt; new { ... }), (a, b, ...) =&gt; x.xxx = y.yyy) ...
    /// SQL: ... INNER JOIN (SELECT ... FROM `sys_order_detail` ...) d ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, TOther, bool>> joinOn);
    #endregion

    #region LeftJoin
    /// <summary>
    /// 在现有表中，选择两个表进行LEFT JOIN关联，可多次关联，如：<code>.LeftJoin((a, b, ...) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3> LeftJoin(Expression<Func<T1, T2, T3, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做LEFT JOIN关联，与.WithTable&lt;TOther&gt;().LeftJoin(...)等价，如：
    /// <code>
    /// .LeftJoin&lt;TOther&gt;((a, b, ...) =&gt; a.xxx = b.yyy)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn);
    /// <summary>
    /// 添加子查询subQuery，并选择一个现有表做LEFT JOIN关联，也可以用在CTE子句中自我引用，与.WithQuery(subQuery).LeftJoin(...)等价，如：
    /// <code>
    /// var subQuery = repository.From&lt;Menu&gt;() ... .Select( ...);
    /// .LeftJoin(subQuery, (a, b) =&gt; a.ParentId == b.Id)
    /// .UnionAllRecursive((x, self) =&gt; x.From&lt;Menu&gt;()
    ///     .LeftJoin(self, (a, b) =&gt; a.ParentId == b.Id) )) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型，子查询中通常会有SELECT操作，返回的类型是一个匿名类</typeparam>
    /// <param name="subQuery">子查询对象，也可以CTE表的自我引用</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr表达式构建子查询，并选择一个现有表做LEFT JOIN关联，与.WithQuery(subQueryExpr).LeftJoin(...)等价，如：
    /// <code>
    /// .LeftJoin(f =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select(t =&gt; new { ... }), (a, b, ...) =&gt; a.Id = b.OrderId) ...
    /// SQL: ... LEFT JOIN (SELECT ... FROM `sys_order_detail` ...) d ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, TOther, bool>> joinOn);
    #endregion

    #region RightJoin
    /// <summary>
    /// 在现有表中，选择两个表进行RIGHT JOIN关联，可多次关联，如：<code>.RightJoin((a, b) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3> RightJoin(Expression<Func<T1, T2, T3, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做RIGHT JOIN关联，与.WithTable&lt;TOther&gt;().RightJoin(...)等价，如：
    /// <code>
    /// .RightJoin&lt;TOther&gt;((a, b, c) =&gt; ...)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn);
    /// <summary>
    /// 添加子查询subQuery，并选择一个现有表做RIGHT JOIN关联，也可以用在CTE子句中自我引用，与.WithQuery(subQuery).RightJoin(...)等价，如：
    /// <code>
    /// var subQuery = repository.From&lt;Menu&gt;() ... .Select( ...);
    /// .LeftJoin(subQuery, (a, b) =&gt; a.ParentId == b.Id)
    /// .UnionAllRecursive((x, self) =&gt; x.From&lt;Menu&gt;()
    ///     .LeftJoin(self, (a, b) =&gt; a.ParentId == b.Id) )) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型，子查询中通常会有SELECT操作，返回的类型是一个匿名类</typeparam>
    /// <param name="subQuery">子查询对象，也可以CTE表的自我引用</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr表达式构建子查询，并选择一个现有表做RIGHT JOIN关联，与.WithQuery(subQueryExpr).RightJoin(...)等价，如：
    /// <code>
    /// .RightJoin(f =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select(t =&gt; new { ... }), (a, b, ...) =&gt; a.Id = b.OrderId) ...
    /// SQL: ... RIGHT JOIN (SELECT ... FROM `sys_order_detail` ...) d ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, TOther, bool>> joinOn);
    #endregion

    #region Where
    /// <summary>
    /// 条件查询，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3> Where(Expression<Func<T1, T2, T3, bool>> predicate);
    /// <summary>
    /// 条件查询，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3> Where(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate, Expression<Func<T1, T2, T3, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3> WherePredicate(Func<PredicateBuilder<T1, T2, T3>, Expression<Func<T1, T2, T3, bool>>> predicateInitializer);
    #endregion

    #region And
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3> And(Expression<Func<T1, T2, T3, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3> And(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, bool>> elsePredicate = null);
    /// <summary>
    /// 条件查询，构造表达式断言predicateInitializer生成Where条件，并与已有的条件AND操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3> AndPredicate(Func<PredicateBuilder<T1, T2, T3>, Expression<Func<T1, T2, T3, bool>>> predicateInitializer);
    #endregion

    #region Or
    /// <summary>
    /// 条件查询，并与已有的条件OR操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3> Or(Expression<Func<T1, T2, T3, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件OR操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3> Or(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, bool>> elsePredicate = null);
    /// <summary>
    /// 条件查询，构造表达式断言predicateInitializer生成Where条件，并与已有的条件OR操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3> OrPredicate(Func<PredicateBuilder<T1, T2, T3>, Expression<Func<T1, T2, T3, bool>>> predicateInitializer);
    #endregion

    #region GroupBy
    /// <summary>
    /// 分组查询，分组表达式groupingExpr可以是单个或多个字段的匿名对象，如：
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
    /// <param name="groupingExpr">分组表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IGroupingCommand<T1, T2, T3, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, TGrouping>> groupingExpr);
    #endregion

    #region OrderBy
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// OrderBy((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderBy((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3> OrderBy<TFields>(Expression<Func<T1, T2, T3, TFields>> fieldsExpr);
    /// <summary>
    /// ASC排序，condition为true，ASC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderBy(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, TFields>> fieldsExpr);
    /// <summary>
    /// DSC排序，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// OrderByDescending((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，condition为true，DESC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, TFields>> fieldsExpr);
    /// <summary>
    /// 动态排序，使用OrderByBuilder构建排序字段选择器，fieldsGetter可以是单个或多个字段的匿名对象，如：
    /// <code>string orderFields = "Name";bool isAsc = true;
    /// .OrderByDynamic(t =&gt; t.Switch(orderFields, isAsc).When("Name", (a, b, ...) =&lt; a.Name).When("Gender", (a, b, ...) =&lt; a.Gender).Build()</code>)
    /// </summary>
    /// <param name="fieldsGetter">排序字段选择器，需调用Build方法，返回排序字段表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3>, Expression> fieldsGetter);
    #endregion

    #region Skip/Take/Page
    /// <summary>
    /// 跳过offset条数据
    /// </summary>
    /// <param name="offset">要跳过的数据条数</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3> Skip(int offset);
    /// <summary>
    /// 只返回limit条数据
    /// </summary>
    /// <param name="limit">返回的数据条数</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3> Take(int limit);
    /// <summary>
    /// 分页查询，pageNumber从1开始
    /// </summary>
    /// <param name="pageNumber">第几页，从1开始，小于1时当作1处理</param>
    /// <param name="pageSize">每页显示条数</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3> Page(int pageNumber, int pageSize);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，如：
    /// Select((a, b, ...) =&gt; new { a.Id, a.Name, ... }) 或是 Select((a, b, ...) =&gt; x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定字段返回，只需要指定需要特殊处理的成员赋值即可，其他成员将从现有表的字段中按名称匹配赋值，多个同名字段时如果未特殊指定赋值，默认选取第一个表中的字段赋值。如：
    /// <code> .SelectTo&lt;TDto&gt;() 或是 .SelectTo((a, b) =&gt; new TDto{ b.Id }) //使用第二个表的Id字段作为Id成员</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型，通常是一个匿名类</typeparam>
    /// <param name="specialMemberSelector">特殊成员赋值表达式，通常是重名字段或是不存在的字段赋值</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, TTarget>> specialMemberSelector = null);
    #endregion    
}
/// <summary>
/// 多表T1, T2, T3, T4查询
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
public interface IFromCommand<T1, T2, T3, T4> : IFromCommand
{
    #region Sharding
    /// <summary>
    /// 直接指定T4表分表名，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4> UseTable(params string[] tableNames);
    /// <summary>
    /// 根据首个分表TMasterSharding表与当前T4表名的映射关系，指定当前T4表分表名获取委托，执行委托获取当前T4表分表名。委托第一个参数是<typeparamref name="TMasterSharding"/>主表原始表名，第二个参数是当前T4表原始表名，第三个参数是TMasterSharding表当前分表名，返回值是当前T4表分表名称，如：
    /// <code>
    /// .From&lt;Order&gt;().UseTable("sys_order_104_202405", "sys_order_105_202405")
    /// .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    /// .UseTableMap&lt;Order&gt;((orderOrigName, userOrigName, orderTableName) =&gt;
    /// {
    ///     //sys_order_104_202405 -&gt; sys_user_104, sys_order_105_202405 -&gt; sys_user_105
    ///     var tableName = orderTableName.Replace(orderOrigName, userOrigName);
    ///     return tableName.Substring(0, tableName.Length - 7);
    /// }) ...
    /// SQL:
    /// SELECT ... FROM `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` ...
    /// UNION ALL
    /// SELECT ... FROM `sys_order_105_202405` a INNER JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` ...
    /// </code>
    /// </summary>
    /// <typeparam name="TMasterSharding">TMasterSharding主表分表实体类型</typeparam>
    /// <param name="tableNameGetter"><typeparamref name="T4"/>表分表名获取委托</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter);
    /// <summary>
    /// 手动指定分表规则参数值，执行分表规则确定<typeparamref name="T4"/>表分表名，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4> UseTableBy(params object[] fieldValues);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定<typeparamref name="T4"/>表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginFieldValue &lt;= endFieldValue，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4> UseTableByRange(params object[] fieldValues);
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4> UseTableSchema(string tableSchema);
    #endregion

    #region WithTable
    /// <summary>
    /// 添加实体表，方便后面做JOIN关联，如：<code>.From&lt;Menu&gt;().WithTable&lt;Page&gt;()</code>
    /// </summary>
    /// <typeparam name="TOther">实体表类型</typeparam>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, TOther> WithTable<TOther>();
    #endregion

    #region WithQuery
    /// <summary>
    /// 添加子查询，方便后面做JOIN关联，如：
    /// <code>
    /// var subQuery = repository.From&lt;Menu&gt;() ... .Select( ...);
    /// .From&lt;Menu&gt;().WithQuery(subQuery)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, TOther> WithQuery<TOther>(IQuery<TOther> subQuery);
    /// <summary>
    /// 添加子查询，方便后面做JOIN关联，如：
    /// <code>
    /// .From&lt;Menu&gt;()
    ///     .WithQuery(f =&gt; f.From&lt;Page, Menu&gt;('c') ... )  
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr);
    #endregion

    #region InnerJoin
    /// <summary>
    /// 在现有表中，选择两个表进行INNER JOIN关联，可多次关联，如：<code>.InnerJoin((a, b, ...) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4> InnerJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做INNER JOIN关联，与.WithTable&lt;TOther&gt;().InnerJoin(...)等价，如：
    /// <code>
    /// .From&lt;User&gt;().InnerJoin&lt;Order&gt;((x, y) =&gt; x.Id == y.BuyerId)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn);
    /// <summary>
    /// 添加子查询subQuery，并选择一个现有表做INNER JOIN关联，也可以用在CTE子句中自我引用，与.WithQuery(subQuery).InnerJoin(...)等价，如：
    /// <code>
    /// var subQuery = repository.From&lt;Menu&gt;() ... .Select( ...);
    /// .InnerJoin(subQuery, (a, b) =&gt; a.ParentId == b.Id)
    /// .UnionAllRecursive((x, self) =&gt; x.From&lt;Menu&gt;()
    ///     .InnerJoin(self, (a, b) =&gt; a.ParentId == b.Id) )) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型，子查询中通常会有SELECT操作，返回的类型是一个匿名类</typeparam>
    /// <param name="subQuery">子查询对象，也可以CTE表的自我引用</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr表达式构建子查询，并选择一个现有表做INNER JOIN关联，与.WithQuery(subQueryExpr).InnerJoin(...)等价，如：
    /// <code>
    /// .InnerJoin(f =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select(x =&gt; new { ... }), (a, b, ...) =&gt; x.xxx = y.yyy) ...
    /// SQL: ... INNER JOIN (SELECT ... FROM `sys_order_detail` ...) e ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn);
    #endregion

    #region LeftJoin
    /// <summary>
    /// 在现有表中，选择两个表进行LEFT JOIN关联，可多次关联，如：<code>.LeftJoin((a, b, ...) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4> LeftJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做LEFT JOIN关联，与.WithTable&lt;TOther&gt;().LeftJoin(...)等价，如：
    /// <code>
    /// .LeftJoin&lt;TOther&gt;((a, b, ...) =&gt; a.xxx = b.yyy)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn);
    /// <summary>
    /// 添加子查询subQuery，并选择一个现有表做LEFT JOIN关联，也可以用在CTE子句中自我引用，与.WithQuery(subQuery).LeftJoin(...)等价，如：
    /// <code>
    /// var subQuery = repository.From&lt;Menu&gt;() ... .Select( ...);
    /// .LeftJoin(subQuery, (a, b) =&gt; a.ParentId == b.Id)
    /// .UnionAllRecursive((x, self) =&gt; x.From&lt;Menu&gt;()
    ///     .LeftJoin(self, (a, b) =&gt; a.ParentId == b.Id) )) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型，子查询中通常会有SELECT操作，返回的类型是一个匿名类</typeparam>
    /// <param name="subQuery">子查询对象，也可以CTE表的自我引用</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr表达式构建子查询，并选择一个现有表做LEFT JOIN关联，与.WithQuery(subQueryExpr).LeftJoin(...)等价，如：
    /// <code>
    /// .LeftJoin(f =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select(t =&gt; new { ... }), (a, b, ...) =&gt; a.Id = b.OrderId) ...
    /// SQL: ... LEFT JOIN (SELECT ... FROM `sys_order_detail` ...) e ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn);
    #endregion

    #region RightJoin
    /// <summary>
    /// 在现有表中，选择两个表进行RIGHT JOIN关联，可多次关联，如：<code>.RightJoin((a, b) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4> RightJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做RIGHT JOIN关联，与.WithTable&lt;TOther&gt;().RightJoin(...)等价，如：
    /// <code>
    /// .RightJoin&lt;TOther&gt;((a, b, c, d) =&gt; ...)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn);
    /// <summary>
    /// 添加子查询subQuery，并选择一个现有表做RIGHT JOIN关联，也可以用在CTE子句中自我引用，与.WithQuery(subQuery).RightJoin(...)等价，如：
    /// <code>
    /// var subQuery = repository.From&lt;Menu&gt;() ... .Select( ...);
    /// .LeftJoin(subQuery, (a, b) =&gt; a.ParentId == b.Id)
    /// .UnionAllRecursive((x, self) =&gt; x.From&lt;Menu&gt;()
    ///     .LeftJoin(self, (a, b) =&gt; a.ParentId == b.Id) )) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型，子查询中通常会有SELECT操作，返回的类型是一个匿名类</typeparam>
    /// <param name="subQuery">子查询对象，也可以CTE表的自我引用</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr表达式构建子查询，并选择一个现有表做RIGHT JOIN关联，与.WithQuery(subQueryExpr).RightJoin(...)等价，如：
    /// <code>
    /// .RightJoin(f =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select(t =&gt; new { ... }), (a, b, ...) =&gt; a.Id = b.OrderId) ...
    /// SQL: ... RIGHT JOIN (SELECT ... FROM `sys_order_detail` ...) e ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn);
    #endregion

    #region Where
    /// <summary>
    /// 条件查询，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4> Where(Expression<Func<T1, T2, T3, T4, bool>> predicate);
    /// <summary>
    /// 条件查询，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4> Where(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4>, Expression<Func<T1, T2, T3, T4, bool>>> predicateInitializer);
    #endregion

    #region And
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4> And(Expression<Func<T1, T2, T3, T4, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4> And(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null);
    /// <summary>
    /// 条件查询，构造表达式断言predicateInitializer生成Where条件，并与已有的条件AND操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4>, Expression<Func<T1, T2, T3, T4, bool>>> predicateInitializer);
    #endregion

    #region Or
    /// <summary>
    /// 条件查询，并与已有的条件OR操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4> Or(Expression<Func<T1, T2, T3, T4, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件OR操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4> Or(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null);
    /// <summary>
    /// 条件查询，构造表达式断言predicateInitializer生成Where条件，并与已有的条件OR操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4>, Expression<Func<T1, T2, T3, T4, bool>>> predicateInitializer);
    #endregion

    #region GroupBy
    /// <summary>
    /// 分组查询，分组表达式groupingExpr可以是单个或多个字段的匿名对象，如：
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
    /// <param name="groupingExpr">分组表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IGroupingCommand<T1, T2, T3, T4, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, TGrouping>> groupingExpr);
    #endregion

    #region OrderBy
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// OrderBy((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderBy((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr);
    /// <summary>
    /// ASC排序，condition为true，ASC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderBy(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr);
    /// <summary>
    /// DSC排序，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// OrderByDescending((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，condition为true，DESC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr);
    /// <summary>
    /// 动态排序，使用OrderByBuilder构建排序字段选择器，fieldsGetter可以是单个或多个字段的匿名对象，如：
    /// <code>string orderFields = "Name";bool isAsc = true;
    /// .OrderByDynamic(t =&gt; t.Switch(orderFields, isAsc).When("Name", (a, b, ...) =&lt; a.Name).When("Gender", (a, b, ...) =&lt; a.Gender).Build()</code>)
    /// </summary>
    /// <param name="fieldsGetter">排序字段选择器，需调用Build方法，返回排序字段表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4>, Expression> fieldsGetter);
    #endregion

    #region Skip/Take/Page
    /// <summary>
    /// 跳过offset条数据
    /// </summary>
    /// <param name="offset">要跳过的数据条数</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4> Skip(int offset);
    /// <summary>
    /// 只返回limit条数据
    /// </summary>
    /// <param name="limit">返回的数据条数</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4> Take(int limit);
    /// <summary>
    /// 分页查询，pageNumber从1开始
    /// </summary>
    /// <param name="pageNumber">第几页，从1开始，小于1时当作1处理</param>
    /// <param name="pageSize">每页显示条数</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4> Page(int pageNumber, int pageSize);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，如：
    /// Select((a, b, ...) =&gt; new { a.Id, a.Name, ... }) 或是 Select((a, b, ...) =&gt; x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定字段返回，只需要指定需要特殊处理的成员赋值即可，其他成员将从现有表的字段中按名称匹配赋值，多个同名字段时如果未特殊指定赋值，默认选取第一个表中的字段赋值。如：
    /// <code> .SelectTo&lt;TDto&gt;() 或是 .SelectTo((a, b) =&gt; new TDto{ b.Id }) //使用第二个表的Id字段作为Id成员</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型，通常是一个匿名类</typeparam>
    /// <param name="specialMemberSelector">特殊成员赋值表达式，通常是重名字段或是不存在的字段赋值</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, TTarget>> specialMemberSelector = null);
    #endregion    
}
/// <summary>
/// 多表T1, T2, T3, T4, T5查询
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
/// <typeparam name="T5">表T5实体类型</typeparam>
public interface IFromCommand<T1, T2, T3, T4, T5> : IFromCommand
{
    #region Sharding
    /// <summary>
    /// 直接指定T5表分表名，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5> UseTable(params string[] tableNames);
    /// <summary>
    /// 根据首个分表TMasterSharding表与当前T5表名的映射关系，指定当前T5表分表名获取委托，执行委托获取当前T5表分表名。委托第一个参数是<typeparamref name="TMasterSharding"/>主表原始表名，第二个参数是当前T5表原始表名，第三个参数是TMasterSharding表当前分表名，返回值是当前T5表分表名称，如：
    /// <code>
    /// .From&lt;Order&gt;().UseTable("sys_order_104_202405", "sys_order_105_202405")
    /// .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    /// .UseTableMap&lt;Order&gt;((orderOrigName, userOrigName, orderTableName) =&gt;
    /// {
    ///     //sys_order_104_202405 -&gt; sys_user_104, sys_order_105_202405 -&gt; sys_user_105
    ///     var tableName = orderTableName.Replace(orderOrigName, userOrigName);
    ///     return tableName.Substring(0, tableName.Length - 7);
    /// }) ...
    /// SQL:
    /// SELECT ... FROM `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` ...
    /// UNION ALL
    /// SELECT ... FROM `sys_order_105_202405` a INNER JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` ...
    /// </code>
    /// </summary>
    /// <typeparam name="TMasterSharding">TMasterSharding主表分表实体类型</typeparam>
    /// <param name="tableNameGetter"><typeparamref name="T5"/>表分表名获取委托</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter);
    /// <summary>
    /// 手动指定分表规则参数值，执行分表规则确定<typeparamref name="T5"/>表分表名，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5> UseTableBy(params object[] fieldValues);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定<typeparamref name="T5"/>表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginFieldValue &lt;= endFieldValue，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5> UseTableByRange(params object[] fieldValues);
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5> UseTableSchema(string tableSchema);
    #endregion

    #region WithTable
    /// <summary>
    /// 添加实体表，方便后面做JOIN关联，如：<code>.From&lt;Menu&gt;().WithTable&lt;Page&gt;()</code>
    /// </summary>
    /// <typeparam name="TOther">实体表类型</typeparam>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, TOther> WithTable<TOther>();
    #endregion

    #region WithQuery
    /// <summary>
    /// 添加子查询，方便后面做JOIN关联，如：
    /// <code>
    /// var subQuery = repository.From&lt;Menu&gt;() ... .Select( ...);
    /// .From&lt;Menu&gt;().WithQuery(subQuery)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, TOther> WithQuery<TOther>(IQuery<TOther> subQuery);
    /// <summary>
    /// 添加子查询，方便后面做JOIN关联，如：
    /// <code>
    /// .From&lt;Menu&gt;()
    ///     .WithQuery(f =&gt; f.From&lt;Page, Menu&gt;('c') ... )  
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr);
    #endregion

    #region InnerJoin
    /// <summary>
    /// 在现有表中，选择两个表进行INNER JOIN关联，可多次关联，如：<code>.InnerJoin((a, b, ...) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做INNER JOIN关联，与.WithTable&lt;TOther&gt;().InnerJoin(...)等价，如：
    /// <code>
    /// .From&lt;User&gt;().InnerJoin&lt;Order&gt;((x, y) =&gt; x.Id == y.BuyerId)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn);
    /// <summary>
    /// 添加子查询subQuery，并选择一个现有表做INNER JOIN关联，也可以用在CTE子句中自我引用，与.WithQuery(subQuery).InnerJoin(...)等价，如：
    /// <code>
    /// var subQuery = repository.From&lt;Menu&gt;() ... .Select( ...);
    /// .InnerJoin(subQuery, (a, b) =&gt; a.ParentId == b.Id)
    /// .UnionAllRecursive((x, self) =&gt; x.From&lt;Menu&gt;()
    ///     .InnerJoin(self, (a, b) =&gt; a.ParentId == b.Id) )) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型，子查询中通常会有SELECT操作，返回的类型是一个匿名类</typeparam>
    /// <param name="subQuery">子查询对象，也可以CTE表的自我引用</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr表达式构建子查询，并选择一个现有表做INNER JOIN关联，与.WithQuery(subQueryExpr).InnerJoin(...)等价，如：
    /// <code>
    /// .InnerJoin(f =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select(x =&gt; new { ... }), (a, b, ...) =&gt; x.xxx = y.yyy) ...
    /// SQL: ... INNER JOIN (SELECT ... FROM `sys_order_detail` ...) f ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn);
    #endregion

    #region LeftJoin
    /// <summary>
    /// 在现有表中，选择两个表进行LEFT JOIN关联，可多次关联，如：<code>.LeftJoin((a, b, ...) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做LEFT JOIN关联，与.WithTable&lt;TOther&gt;().LeftJoin(...)等价，如：
    /// <code>
    /// .LeftJoin&lt;TOther&gt;((a, b, ...) =&gt; a.xxx = b.yyy)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn);
    /// <summary>
    /// 添加子查询subQuery，并选择一个现有表做LEFT JOIN关联，也可以用在CTE子句中自我引用，与.WithQuery(subQuery).LeftJoin(...)等价，如：
    /// <code>
    /// var subQuery = repository.From&lt;Menu&gt;() ... .Select( ...);
    /// .LeftJoin(subQuery, (a, b) =&gt; a.ParentId == b.Id)
    /// .UnionAllRecursive((x, self) =&gt; x.From&lt;Menu&gt;()
    ///     .LeftJoin(self, (a, b) =&gt; a.ParentId == b.Id) )) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型，子查询中通常会有SELECT操作，返回的类型是一个匿名类</typeparam>
    /// <param name="subQuery">子查询对象，也可以CTE表的自我引用</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr表达式构建子查询，并选择一个现有表做LEFT JOIN关联，与.WithQuery(subQueryExpr).LeftJoin(...)等价，如：
    /// <code>
    /// .LeftJoin(f =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select(t =&gt; new { ... }), (a, b, ...) =&gt; a.Id = b.OrderId) ...
    /// SQL: ... LEFT JOIN (SELECT ... FROM `sys_order_detail` ...) f ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn);
    #endregion

    #region RightJoin
    /// <summary>
    /// 在现有表中，选择两个表进行RIGHT JOIN关联，可多次关联，如：<code>.RightJoin((a, b) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5> RightJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做RIGHT JOIN关联，与.WithTable&lt;TOther&gt;().RightJoin(...)等价，如：
    /// <code>
    /// .RightJoin&lt;TOther&gt;((a, b, c, d, e) =&gt; ...)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn);
    /// <summary>
    /// 添加子查询subQuery，并选择一个现有表做RIGHT JOIN关联，也可以用在CTE子句中自我引用，与.WithQuery(subQuery).RightJoin(...)等价，如：
    /// <code>
    /// var subQuery = repository.From&lt;Menu&gt;() ... .Select( ...);
    /// .LeftJoin(subQuery, (a, b) =&gt; a.ParentId == b.Id)
    /// .UnionAllRecursive((x, self) =&gt; x.From&lt;Menu&gt;()
    ///     .LeftJoin(self, (a, b) =&gt; a.ParentId == b.Id) )) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型，子查询中通常会有SELECT操作，返回的类型是一个匿名类</typeparam>
    /// <param name="subQuery">子查询对象，也可以CTE表的自我引用</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr表达式构建子查询，并选择一个现有表做RIGHT JOIN关联，与.WithQuery(subQueryExpr).RightJoin(...)等价，如：
    /// <code>
    /// .RightJoin(f =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select(t =&gt; new { ... }), (a, b, ...) =&gt; a.Id = b.OrderId) ...
    /// SQL: ... RIGHT JOIN (SELECT ... FROM `sys_order_detail` ...) f ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn);
    #endregion

    #region Where
    /// <summary>
    /// 条件查询，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5> Where(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate);
    /// <summary>
    /// 条件查询，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5>, Expression<Func<T1, T2, T3, T4, T5, bool>>> predicateInitializer);
    #endregion

    #region And
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5> And(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null);
    /// <summary>
    /// 条件查询，构造表达式断言predicateInitializer生成Where条件，并与已有的条件AND操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5>, Expression<Func<T1, T2, T3, T4, T5, bool>>> predicateInitializer);
    #endregion

    #region Or
    /// <summary>
    /// 条件查询，并与已有的条件OR操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5> Or(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件OR操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null);
    /// <summary>
    /// 条件查询，构造表达式断言predicateInitializer生成Where条件，并与已有的条件OR操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5>, Expression<Func<T1, T2, T3, T4, T5, bool>>> predicateInitializer);
    #endregion

    #region GroupBy
    /// <summary>
    /// 分组查询，分组表达式groupingExpr可以是单个或多个字段的匿名对象，如：
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
    /// <param name="groupingExpr">分组表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IGroupingCommand<T1, T2, T3, T4, T5, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, TGrouping>> groupingExpr);
    #endregion

    #region OrderBy
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// OrderBy((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderBy((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    /// <summary>
    /// ASC排序，condition为true，ASC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderBy(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    /// <summary>
    /// DSC排序，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// OrderByDescending((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，condition为true，DESC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    /// <summary>
    /// 动态排序，使用OrderByBuilder构建排序字段选择器，fieldsGetter可以是单个或多个字段的匿名对象，如：
    /// <code>string orderFields = "Name";bool isAsc = true;
    /// .OrderByDynamic(t =&gt; t.Switch(orderFields, isAsc).When("Name", (a, b, ...) =&lt; a.Name).When("Gender", (a, b, ...) =&lt; a.Gender).Build()</code>)
    /// </summary>
    /// <param name="fieldsGetter">排序字段选择器，需调用Build方法，返回排序字段表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5>, Expression> fieldsGetter);
    #endregion

    #region Skip/Take/Page
    /// <summary>
    /// 跳过offset条数据
    /// </summary>
    /// <param name="offset">要跳过的数据条数</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5> Skip(int offset);
    /// <summary>
    /// 只返回limit条数据
    /// </summary>
    /// <param name="limit">返回的数据条数</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5> Take(int limit);
    /// <summary>
    /// 分页查询，pageNumber从1开始
    /// </summary>
    /// <param name="pageNumber">第几页，从1开始，小于1时当作1处理</param>
    /// <param name="pageSize">每页显示条数</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5> Page(int pageNumber, int pageSize);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，如：
    /// Select((a, b, ...) =&gt; new { a.Id, a.Name, ... }) 或是 Select((a, b, ...) =&gt; x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定字段返回，只需要指定需要特殊处理的成员赋值即可，其他成员将从现有表的字段中按名称匹配赋值，多个同名字段时如果未特殊指定赋值，默认选取第一个表中的字段赋值。如：
    /// <code> .SelectTo&lt;TDto&gt;() 或是 .SelectTo((a, b) =&gt; new TDto{ b.Id }) //使用第二个表的Id字段作为Id成员</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型，通常是一个匿名类</typeparam>
    /// <param name="specialMemberSelector">特殊成员赋值表达式，通常是重名字段或是不存在的字段赋值</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, TTarget>> specialMemberSelector = null);
    #endregion    
}
/// <summary>
/// 多表T1, T2, T3, T4, T5, T6查询
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
/// <typeparam name="T5">表T5实体类型</typeparam>
/// <typeparam name="T6">表T6实体类型</typeparam>
public interface IFromCommand<T1, T2, T3, T4, T5, T6> : IFromCommand
{
    #region Sharding
    /// <summary>
    /// 直接指定T6表分表名，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, T6> UseTable(params string[] tableNames);
    /// <summary>
    /// 根据首个分表TMasterSharding表与当前T6表名的映射关系，指定当前T6表分表名获取委托，执行委托获取当前T6表分表名。委托第一个参数是<typeparamref name="TMasterSharding"/>主表原始表名，第二个参数是当前T6表原始表名，第三个参数是TMasterSharding表当前分表名，返回值是当前T6表分表名称，如：
    /// <code>
    /// .From&lt;Order&gt;().UseTable("sys_order_104_202405", "sys_order_105_202405")
    /// .InnerJoin&lt;User&gt;((x, y) =&gt; x.BuyerId == y.Id)
    /// .UseTableMap&lt;Order&gt;((orderOrigName, userOrigName, orderTableName) =&gt;
    /// {
    ///     //sys_order_104_202405 -&gt; sys_user_104, sys_order_105_202405 -&gt; sys_user_105
    ///     var tableName = orderTableName.Replace(orderOrigName, userOrigName);
    ///     return tableName.Substring(0, tableName.Length - 7);
    /// }) ...
    /// SQL:
    /// SELECT ... FROM `sys_order_104_202405` a INNER JOIN `sys_user_104` b ON a.`BuyerId`=b.`Id` ...
    /// UNION ALL
    /// SELECT ... FROM `sys_order_105_202405` a INNER JOIN `sys_user_105` b ON a.`BuyerId`=b.`Id` ...
    /// </code>
    /// </summary>
    /// <typeparam name="TMasterSharding">TMasterSharding主表分表实体类型</typeparam>
    /// <param name="tableNameGetter"><typeparamref name="T6"/>表分表名获取委托</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, T6> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter);
    /// <summary>
    /// 手动指定分表规则参数值，执行分表规则确定<typeparamref name="T6"/>表分表名，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, T6> UseTableBy(params object[] fieldValues);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定<typeparamref name="T6"/>表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginFieldValue &lt;= endFieldValue，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, T6> UseTableByRange(params object[] fieldValues);
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, T6> UseTableSchema(string tableSchema);
    #endregion

    #region InnerJoin
    /// <summary>
    /// 在现有表中，选择两个表进行INNER JOIN关联，可多次关联，如：<code>.InnerJoin((a, b, ...) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, T6> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn);
    #endregion

    #region LeftJoin
    /// <summary>
    /// 在现有表中，选择两个表进行LEFT JOIN关联，可多次关联，如：<code>.LeftJoin((a, b, ...) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, T6> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn);
    #endregion

    #region RightJoin
    /// <summary>
    /// 在现有表中，选择两个表进行RIGHT JOIN关联，可多次关联，如：<code>.RightJoin((a, b) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, T6> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn);
    #endregion

    #region Where
    /// <summary>
    /// 条件查询，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, T6> Where(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate);
    /// <summary>
    /// 条件查询，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, T6> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, T6> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6>, Expression<Func<T1, T2, T3, T4, T5, T6, bool>>> predicateInitializer);
    #endregion

    #region And
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, T6> And(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, T6> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null);
    /// <summary>
    /// 条件查询，构造表达式断言predicateInitializer生成Where条件，并与已有的条件AND操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, T6> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6>, Expression<Func<T1, T2, T3, T4, T5, T6, bool>>> predicateInitializer);
    #endregion

    #region Or
    /// <summary>
    /// 条件查询，并与已有的条件OR操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, T6> Or(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件OR操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, T6> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null);
    /// <summary>
    /// 条件查询，构造表达式断言predicateInitializer生成Where条件，并与已有的条件OR操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, T6> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6>, Expression<Func<T1, T2, T3, T4, T5, T6, bool>>> predicateInitializer);
    #endregion

    #region GroupBy
    /// <summary>
    /// 分组查询，分组表达式groupingExpr可以是单个或多个字段的匿名对象，如：
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
    /// <param name="groupingExpr">分组表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IGroupingCommand<T1, T2, T3, T4, T5, T6, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, TGrouping>> groupingExpr);
    #endregion

    #region OrderBy
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// OrderBy((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderBy((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, T6> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr);
    /// <summary>
    /// ASC排序，condition为true，ASC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderBy(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, T6> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr);
    /// <summary>
    /// DSC排序，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// OrderByDescending((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, T6> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，condition为true，DESC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, T6> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr);
    /// <summary>
    /// 动态排序，使用OrderByBuilder构建排序字段选择器，fieldsGetter可以是单个或多个字段的匿名对象，如：
    /// <code>string orderFields = "Name";bool isAsc = true;
    /// .OrderByDynamic(t =&gt; t.Switch(orderFields, isAsc).When("Name", (a, b, ...) =&lt; a.Name).When("Gender", (a, b, ...) =&lt; a.Gender).Build()</code>)
    /// </summary>
    /// <param name="fieldsGetter">排序字段选择器，需调用Build方法，返回排序字段表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, T6> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6>, Expression> fieldsGetter);
    #endregion

    #region Skip/Take/Page
    /// <summary>
    /// 跳过offset条数据
    /// </summary>
    /// <param name="offset">要跳过的数据条数</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, T6> Skip(int offset);
    /// <summary>
    /// 只返回limit条数据
    /// </summary>
    /// <param name="limit">返回的数据条数</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, T6> Take(int limit);
    /// <summary>
    /// 分页查询，pageNumber从1开始
    /// </summary>
    /// <param name="pageNumber">第几页，从1开始，小于1时当作1处理</param>
    /// <param name="pageSize">每页显示条数</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, T6> Page(int pageNumber, int pageSize);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，如：
    /// Select((a, b, ...) =&gt; new { a.Id, a.Name, ... }) 或是 Select((a, b, ...) =&gt; x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定字段返回，只需要指定需要特殊处理的成员赋值即可，其他成员将从现有表的字段中按名称匹配赋值，多个同名字段时如果未特殊指定赋值，默认选取第一个表中的字段赋值。如：
    /// <code> .SelectTo&lt;TDto&gt;() 或是 .SelectTo((a, b) =&gt; new TDto{ b.Id }) //使用第二个表的Id字段作为Id成员</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型，通常是一个匿名类</typeparam>
    /// <param name="specialMemberSelector">特殊成员赋值表达式，通常是重名字段或是不存在的字段赋值</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, TTarget>> specialMemberSelector = null);
    #endregion    
}