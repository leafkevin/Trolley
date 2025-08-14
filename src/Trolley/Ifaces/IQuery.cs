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
public interface IQuery
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
/// <summary>
/// CTE表查询对象
/// </summary>
public interface ICteQuery : IQuery
{
    #region Properties
    /// <summary>
    /// CTE表名称
    /// </summary>
    string TableName { get; set; }
    /// <summary>
    /// CTE表字段定义
    /// </summary>
    List<SqlFieldSegment> ReaderFields { get; set; }
    /// <summary>
    /// CTE表主体SQL
    /// </summary>
    string Body { get; set; }
    /// <summary>
    /// 是否是递归CTE表
    /// </summary>
    bool IsRecursive { get; set; }
    #endregion
}
/// <summary>
/// 表T查询对象
/// </summary>
public interface IQueryBase : IQuery
{
    #region Count
    /// <summary>
    /// 返回数据条数
    /// </summary>
    /// <returns>返回int类型数据条数</returns>
    int Count();
    /// <summary>
    /// 返回数据条数
    /// </summary>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回int类型数据条数</returns>
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回数据条数
    /// </summary>
    /// <returns>返回long类型数据条数</returns>
    long LongCount();
    /// <summary>
    /// 返回数据条数
    /// </summary>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回long类型数据条数</returns>
    Task<long> LongCountAsync(CancellationToken cancellationToken = default);
    #endregion

    #region Exists
    /// <summary>
    /// 判断数据是否存在，存在为true，否则为false
    /// </summary>
    /// <returns>返回布尔值，存在为true，否则为false</returns>
    bool Exists();
    /// <summary>
    /// 判断数据是否存在，存在为true，否则为false
    /// </summary>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回布尔值，存在为true，否则为false</returns>
    Task<bool> ExistsAsync(CancellationToken cancellationToken = default);
    #endregion
}
/// <summary>
/// 查询对象
/// </summary>
/// <typeparam name="T">实体类型</typeparam>
public interface IQuery<T> : IQueryBase
{
    #region Sharding
    /// <summary>
    /// 直接指定T表分表名，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回查询对象</returns>
    IQuery<T> UseTable(params string[] tableNames);
    /// <summary>
    /// 手动指定分表规则参数值，执行分表规则确定T表分表名，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，最多支持3个字段值，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回查询对象</returns>
    IQuery<T> UseTableBy(params object[] fieldValues);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginFieldValue &lt;= endFieldValue，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="beginFieldValue">字段范围起始值</param>
    /// <param name="endFieldValue">字段范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T> UseTableByRange(object beginFieldValue, object endFieldValue);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginField2Value &lt;= endField2Value，常用于是日期规则分表，如：.UseTableByRange(1, DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="beginField2Value">字段2范围起始值</param>
    /// <param name="endField2Value">字段2范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T> UseTableByRange(object field1Value, object beginField2Value, object endField2Value);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginField3Value &lt;= endField3Value，常用于是日期规则分表，如：.UseTableByRange(1, "Game1", DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="field2Value">字段2值</param>
    /// <param name="beginField3Value">字段3范围起始值</param>
    /// <param name="endField3Value">字段3范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value);
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回查询对象</returns>
    IQuery<T> UseTableSchema(string tableSchema);
    #endregion

    #region GetShardingTableNames
    /// <summary>
    /// 获取实体TEntity满足断言tableNameSelector条件的所有分表名
    /// </summary>
    /// <param name="tableNameSelector">分表名选择委托</param>
    /// <returns>返回满足条件的所有分表</returns>
    List<string> GetShardingTableNames(Func<string, bool> tableNameSelector);
    /// <summary>
    /// 获取实体TEntity满足断言tableNameSelector条件的所有分表名
    /// </summary>
    /// <param name="tableNameSelector">分表名选择表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回满足条件的所有分表</returns>
    Task<List<string>> GetShardingTableNamesAsync(Func<string, bool> tableNameSelector, CancellationToken cancellationToken = default);
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
    IQuery<T> Union(IQuery<T> subQuery);
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
    IQuery<T> Union(Expression<Func<IFromQuery, IQuery<T>>> subQueryExpr);
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
    IQuery<T> UnionAll(IQuery<T> subQuery);
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
    IQuery<T> UnionAll(Expression<Func<IFromQuery, IQuery<T>>> subQueryExpr);
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
    IQuery<T> UnionRecursive(Expression<Func<IFromQuery, IQuery<T>, IQuery<T>>> subQueryExpr);
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
    IQuery<T> UnionAllRecursive(Expression<Func<IFromQuery, IQuery<T>, IQuery<T>>> subQueryExpr);
    #endregion

    #region WithTable
    /// <summary>
    /// 添加实体表，方便后面做JOIN关联，如：<code>.WithTable&lt;Page&gt;()</code>
    /// </summary>
    /// <typeparam name="TOther">实体表类型</typeparam>
    /// <returns>返回查询对象</returns>
    IQuery<T, TOther> WithTable<TOther>();
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
    IQuery<T, TOther> WithQuery<TOther>(IQuery<TOther> subQuery);
    /// <summary>
    /// 添加子查询，方便后面做JOIN关联，如：
    /// <code>
    /// .From&lt;Menu&gt;().WithQuery(f =&gt; f.From&lt;Page, Menu&gt;('c') ... )  
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr);
    #endregion

    #region Include
    /// <summary>
    /// 加载导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句。
    /// 1:1关联关系，随主表一起查询,支持无限级，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .From&lt;Product&gt;().Include(f =&gt; f.Brand) ...
    /// .From&lt;Brand&gt;().Include(f =&gt; f.Products) ...
    /// .From&lt;Order&gt;().Include(f =&gt; f.Seller.Company.Products) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TMember">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    IIncludableQuery<T, TMember> Include<TMember>(Expression<Func<T, TMember>> memberSelector);
    /// <summary>
    /// 加载集合类导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，可使用filter筛选满足条件的导航属性，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .From&lt;User&gt;().IncludeMany(f =&gt; f.Orders)  //与 .From&lt;User&gt;().Include(f =&gt; f.Orders) 等价
    /// .From&lt;User&gt;().IncludeMany(f =&gt; f.Orders, order =&gt; order.TotalAmout &gt; 500)
    /// </code>
    /// </summary>
    /// <typeparam name="TElement">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IIncludableQuery<T, TElement> IncludeMany<TElement>(Expression<Func<T, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null);
    #endregion

    #region InnerJoin
    /// <summary>
    /// 添加TOther表，与现有表T做INNER JOIN关联，与.WithTable&lt;TOther&gt;().InnerJoin(...)等价，如：
    /// <code>
    /// .From&lt;User&gt;().InnerJoin&lt;Order&gt;((x, y) =&gt; x.Id == y.BuyerId)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T, TOther> InnerJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn);
    /// <summary>
    /// 添加子查询subQuery，并与现有表T做INNER JOIN关联，也可以用在CTE子句中自我引用，与.WithQuery(subQuery).InnerJoin(...)等价，如：
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
    IQuery<T, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr子查询，并与现有表T做INNER JOIN关联，与.WithQuery(subQueryExpr).InnerJoin(...)等价，如：
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
    IQuery<T, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T, TOther, bool>> joinOn);
    #endregion

    #region LeftJoin
    /// <summary>
    /// 添加TOther表，与现有表T做LEFT JOIN关联，与.WithTable&lt;TOther&gt;().LeftJoin(...)等价，如：
    /// <code>
    /// repository.From&lt;User&gt;()
    ///     .LeftJoin&lt;Order&gt;((x, y) =&gt; x.Id == y.BuyerId)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T, TOther> LeftJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn);
    /// <summary>
    /// 添加子查询subQuery，并与现有表T做LEFT JOIN关联，可以用在CTE子句中自我引用，与.WithQuery(subQueryExpr).LeftJoin(...)等价，如：
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
    IQuery<T, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr子查询，并与现有表T做LEFT JOIN关联，如：
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
    IQuery<T, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T, TOther, bool>> joinOn);
    #endregion

    #region RightJoin
    /// <summary>
    /// 添加TOther表，与现有表T做RIGHT JOIN关联，与.WithTable&lt;TOther&gt;().RightJoin(...)等价，如：
    /// <code>
    /// .From&lt;User&gt;().RightJoin&lt;Order&gt;((x, y) =&gt; x.Id == y.BuyerId)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T, TOther> RightJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn);
    /// <summary>
    /// 添加子查询subQuery，并与现有表T做LEFT JOIN关联，可以用在CTE子句中自我引用，与.WithQuery(subQuery).RightJoin(...)等价，如：
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
    IQuery<T, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr子查询，并与现有表T做RIGHT JOIN关联，，与.WithQuery(subQueryExpr).RightJoin(...)等价，如：
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
    IQuery<T, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T, TOther, bool>> joinOn);
    #endregion

    #region Where
    /// <summary>
    /// 条件查询，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T> Where(Expression<Func<T, bool>> predicate);
    /// <summary>
    /// 条件查询，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T> Where(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T> WherePredicate(Func<PredicateBuilder<T>, Expression<Func<T, bool>>> predicateInitializer);
    #endregion

    #region And
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T> And(Expression<Func<T, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T> And(bool condition, Expression<Func<T, bool>> ifPredicate = null, Expression<Func<T, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，并与已有的条件AND操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T> AndPredicate(Func<PredicateBuilder<T>, Expression<Func<T, bool>>> predicateInitializer);
    #endregion

    #region Or
    /// <summary>
    /// 使用predicate表达式生成Or条件，并添加到已有的Where条件末尾，表达式predicate不可为null
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T> Or(Expression<Func<T, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件OR操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T> Or(bool condition, Expression<Func<T, bool>> ifPredicate = null, Expression<Func<T, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，并与已有的条件OR操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T> OrPredicate(Func<PredicateBuilder<T>, Expression<Func<T, bool>>> predicateInitializer);
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
    IGroupingQuery<T, TGrouping> GroupBy<TGrouping>(Expression<Func<T, TGrouping>> groupingExpr);
    #endregion

    #region OrderBy/OrderByDescending
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderBy(f =&gt; new { f.Id, f.OtherId }) 或是 .OrderBy(x =&gt; x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T> OrderBy<TFields>(Expression<Func<T, TFields>> fieldsExpr);
    /// <summary>
    /// ASC排序，condition为true，ASC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderBy(true, f =&gt; new { f.Id, f.OtherId }) 或是 .OrderBy(true, x =&gt; x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T> OrderBy<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr);
    /// <summary>
    /// DSC排序，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderByDescending(f =&gt; new { f.Id, f.OtherId }) 或是 .OrderByDescending(x =&gt; x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T> OrderByDescending<TFields>(Expression<Func<T, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，condition为true，DESC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderByDescending(true, f =&gt; new { f.Id, f.OtherId }) 或是 .OrderByDescending(true, x =&gt; x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T> OrderByDescending<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr);
    /// <summary>
    /// 动态排序，使用OrderByBuilder构建排序字段选择器，fieldsGetter可以是单个或多个字段的匿名对象，如：
    /// <code>string orderFields = "Name";bool isAsc = true;
    /// .OrderByDynamic(t =&gt; t.Switch(orderFields, isAsc).When("Name", f =&lt; f.Name).When("Gender", f =&lt; f.Gender).Build()</code>)
    /// </summary>
    /// <param name="fieldsGetter">排序字段选择器，需调用Build方法，返回排序字段表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T> OrderByDynamic(Func<OrderByBuilder<T>, Expression> fieldsGetter);
    #endregion

    #region Skip/Take/Page
    /// <summary>
    /// 跳过offset条数据
    /// </summary>
    /// <param name="offset">要跳过的数据条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T> Skip(int offset);
    /// <summary>
    /// 只返回limit条数据
    /// </summary>
    /// <param name="limit">返回的数据条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T> Take(int limit);
    /// <summary>
    /// 分页查询，pageNumber从1开始
    /// </summary>
    /// <param name="pageNumber">第几页，从1开始，小于1时当作1处理</param>
    /// <param name="pageSize">每页显示条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T> Page(int pageNumber, int pageSize);
    #endregion

    #region Select
    /// <summary>
    /// 直接返回所有字段
    /// </summary>
    /// <returns>返回查询对象</returns>
    IQuery<T> Select();
    /// <summary>
    /// 选择指定字段返回，可以是单个或多个字段的匿名对象，如：
    /// <code> .Select(f =&gt; new { f.Id, f.Name }) 或是 .Select(x =&gt; x.CreatedAt.Date)</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<TTarget> Select<TTarget>(Expression<Func<T, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定字段返回，只需要指定需要特殊处理的成员赋值即可，其他成员将从现有表的字段中按名称匹配赋值，多个同名字段时如果未特殊指定赋值，默认选取第一个表中的字段赋值。如：
    /// <code> .SelectTo&lt;TDto&gt;() 或是 .SelectTo((a, b) =&gt; new TDto{ b.Id }) //使用第二个表的Id字段作为Id成员</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="specialMemberSelector">特殊成员赋值表达式，通常是重名字段或是不存在的字段赋值</param>
    /// <returns>返回查询对象</returns>
    IQuery<TTarget> SelectTo<TTarget>(Expression<Func<T, TTarget>> specialMemberSelector = null);
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
    IQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T, TTarget>> fieldsExpr);
    #endregion

    #region Distinct
    /// <summary>
    /// DISTINCT去掉重复数据
    /// </summary>
    /// <returns>返回查询对象</returns>
    IQuery<T> Distinct();
    #endregion

    #region Count
    /// <summary>
    /// 返回某个字段的int类型数据条数，如：
    /// <code>.Count(f =&gt; f.BuyerId)</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的int类型数据条数</returns>
    int Count<TField>(Expression<Func<T, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的int类型数据条数，如：
    /// <code>.CountAsync(f =&gt; f.BuyerId)</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的int类型数据条数</returns>
    Task<int> CountAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，如：
    /// <code>.CountDistinct(f =&gt; f.BuyerId)</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段去重后的int类型数据条数</returns>
    int CountDistinct<TField>(Expression<Func<T, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，如：
    /// <code>.CountDistinctAsync(f =&gt; f.BuyerId)</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段去重后的int类型数据条数</returns>
    Task<int> CountDistinctAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段的long类型数据条数，如：
    /// <code>.LongCount(f =&gt; f.BuyerId)</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的long类型数据条数</returns>
    long LongCount<TField>(Expression<Func<T, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的long类型数据条数，如：
    /// <code>.LongCountAsync(f =&gt; f.BuyerId)</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的long类型数据条数</returns>
    Task<long> LongCountAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，如：
    /// <code>.LongCountDistinct(f =&gt; f.BuyerId)</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段去重后的long类型数据条数</returns>
    long LongCountDistinct<TField>(Expression<Func<T, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，如：
    /// <code>.LongCountDistinctAsync(f =&gt; f.BuyerId)</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段去重后的long类型数据条数</returns>
    Task<long> LongCountDistinctAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default);
    #endregion

    #region Aggregate
    /// <summary>
    /// 计算指定字段的求和值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的求和值，decimal类型</returns>
    decimal Sum<TField>(Expression<Func<T, TField>> fieldExpr);
    /// <summary>
    /// <summary>
    /// 计算指定字段的求和值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的求和值，decimal类型</returns>
    Task<decimal> SumAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的平均值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的平均值</returns>
    TField Avg<TField>(Expression<Func<T, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的平均值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的平均值</returns>
    Task<TField> AvgAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的最大值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的最大值</returns>
    TField Max<TField>(Expression<Func<T, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最大值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的最大值</returns>
    Task<TField> MaxAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的最小值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的最小值</returns>
    TField Min<TField>(Expression<Func<T, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最小值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的最小值</returns>
    Task<TField> MinAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default);
    #endregion

    #region First/ToList/ToPageList
    /// <summary>
    /// 执行查询，返回第一条记录，没有记录时返回默认值
    /// </summary>
    /// <returns>返回T实体对象或默认值</returns>
    T First();
    /// <summary>
    /// 执行查询，返回第一条记录，没有记录时返回默认值
    /// </summary>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回T实体对象或默认值</returns>
    Task<T> FirstAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// 执行查询，返回多条记录，没有记录时返回没有任何元素的空列表
    /// </summary>
    /// <returns>返回T实体列表或没有任何元素的空列表</returns>
    List<T> ToList();
    /// <summary>
    /// 执行查询，返回多条记录，没有记录时返回没有任何元素的空列表
    /// </summary>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回T实体列表或没有任何元素的空列表</returns>
    Task<List<T>> ToListAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// 执行查询，返回分页的多条记录，没有记录时返回没有任何元素的空分页列表
    /// </summary>
    /// <returns>返回IPagedList&lt;T&gt;列表或没有任何元素的空IPagedList&lt;T&gt;空列表</returns>
    IPagedList<T> ToPageList();
    /// <summary>
    /// 执行查询，返回分页的多条记录，没有记录时返回没有任何元素的空分页列表
    /// </summary>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回IPagedList&lt;T&gt;列表或没有任何元素的空IPagedList&lt;T&gt;空列表</returns>
    Task<IPagedList<T>> ToPageListAsync(CancellationToken cancellationToken = default);
    #endregion

    #region AsCteTable
    /// <summary>
    /// 把当前子查询转为CTE表
    /// </summary>
    /// <param name="tableName">CTE表名</param>
    /// <returns></returns>
    ICteQuery<T> AsCteTable(string tableName);
    #endregion
}
/// <summary>
/// CTE表查询对象
/// </summary>
/// <typeparam name="T">实体类型</typeparam>
public interface ICteQuery<T> : ICteQuery, IQuery<T> { }
/// <summary>
/// 多表T1, T2查询
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
public interface IQuery<T1, T2> : IQueryBase
{
    #region Sharding
    /// <summary>
    /// 直接指定T2表分表名，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2> UseTable(params string[] tableNames);
    /// <summary>
    /// 根据首个分表TMasterSharding表与当前T2表名的映射关系，指定当前T2表分表名获取委托，执行委托获取当前T2表分表名。委托第一个参数是TMasterSharding主表原始表名，第二个参数是当前T2表原始表名，第三个参数是TMasterSharding表当前分表名，返回值是当前T2表分表名称，如：
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
    /// <param name="tableNameGetter">T2表分表名获取委托</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter);
    /// <summary>
    /// 手动指定分表规则参数值，执行分表规则确定T2表分表名，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，最多支持3个字段值，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2> UseTableBy(params object[] fieldValues);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T2表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginFieldValue &lt;= endFieldValue，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="beginFieldValue">字段范围起始值</param>
    /// <param name="endFieldValue">字段范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2> UseTableByRange(object beginFieldValue, object endFieldValue);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T2表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginField2Value &lt;= endField2Value，常用于是日期规则分表，如：.UseTableByRange(1, DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="beginField2Value">字段2范围起始值</param>
    /// <param name="endField2Value">字段2范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2> UseTableByRange(object field1Value, object beginField2Value, object endField2Value);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T2表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginField3Value &lt;= endField3Value，常用于是日期规则分表，如：.UseTableByRange(1, "Game1", DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="field2Value">字段2值</param>
    /// <param name="beginField3Value">字段3范围起始值</param>
    /// <param name="endField3Value">字段3范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value);
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2> UseTableSchema(string tableSchema);
    #endregion

    #region WithTable
    /// <summary>
    /// 添加实体表，方便后面做JOIN关联，如：<code>.From&lt;Menu&gt;().WithTable&lt;Page&gt;()</code>
    /// </summary>
    /// <typeparam name="TOther">实体表类型</typeparam>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, TOther> WithTable<TOther>();
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
    IQuery<T1, T2, TOther> WithQuery<TOther>(IQuery<TOther> subQuery);
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
    IQuery<T1, T2, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr);
    #endregion

    #region Include
    /// <summary>
    /// 加载导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，1:1关联关系，随主表一起查询,支持无限级，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .From&lt;Product&gt;().Include(f =&gt; f.Brand) ...
    /// .From&lt;Brand&gt;().Include(f =&gt; f.Products) ...
    /// .From&lt;Order&gt;().Include(f =&gt; f.Seller.Company.Products) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TMember">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, TMember> Include<TMember>(Expression<Func<T1, T2, TMember>> memberSelector);
    /// <summary>
    /// 加载集合类导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，可使用filter筛选满足条件的导航属性，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .From&lt;User&gt;().IncludeMany(f =&gt; f.Orders)  //与 .From&lt;User&gt;().Include(f =&gt; f.Orders) 等价
    /// .From&lt;User&gt;().IncludeMany(f =&gt; f.Orders, order =&gt; order.TotalAmout &gt; 500)
    /// </code>
    /// </summary>
    /// <typeparam name="TElement">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null);
    #endregion

    #region InnerJoin
    /// <summary>
    /// 在现有表中，选择两个表进行INNER JOIN关联，可多次关联，如：<code>.InnerJoin((a, b, ...) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2> InnerJoin(Expression<Func<T1, T2, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做INNER JOIN关联，与.WithTable&lt;TOther&gt;().InnerJoin(...)等价，如：
    /// <code>
    /// .From&lt;User&gt;().InnerJoin&lt;Order&gt;((x, y) =&gt; x.Id == y.BuyerId)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn);
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
    IQuery<T1, T2, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn);
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
    IQuery<T1, T2, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, TOther, bool>> joinOn);
    #endregion

    #region LeftJoin
    /// <summary>
    /// 在现有表中，选择两个表进行LEFT JOIN关联，可多次关联，如：<code>.LeftJoin((a, b, ...) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2> LeftJoin(Expression<Func<T1, T2, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做LEFT JOIN关联，与.WithTable&lt;TOther&gt;().LeftJoin(...)等价，如：
    /// <code>
    /// .LeftJoin&lt;TOther&gt;((a, b, ...) =&gt; a.xxx = b.yyy)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn);
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
    IQuery<T1, T2, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn);
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
    IQuery<T1, T2, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, TOther, bool>> joinOn);
    #endregion

    #region RightJoin
    /// <summary>
    /// 在现有表中，选择两个表进行RIGHT JOIN关联，可多次关联，如：<code>.RightJoin((a, b) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2> RightJoin(Expression<Func<T1, T2, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做RIGHT JOIN关联，与.WithTable&lt;TOther&gt;().RightJoin(...)等价，如：
    /// <code>
    /// .RightJoin&lt;TOther&gt;((a, b) =&gt; ...)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, TOther> RightJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn);
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
    IQuery<T1, T2, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn);
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
    IQuery<T1, T2, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, TOther, bool>> joinOn);
    #endregion

    #region Where
    /// <summary>
    /// 条件查询，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2> Where(Expression<Func<T1, T2, bool>> predicate);
    /// <summary>
    /// 条件查询，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2> Where(bool condition, Expression<Func<T1, T2, bool>> ifPredicate, Expression<Func<T1, T2, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2> WherePredicate(Func<PredicateBuilder<T1, T2>, Expression<Func<T1, T2, bool>>> predicateInitializer);
    #endregion

    #region And
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2> And(Expression<Func<T1, T2, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2> And(bool condition, Expression<Func<T1, T2, bool>> ifPredicate = null, Expression<Func<T1, T2, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，并与已有的条件AND操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2> AndPredicate(Func<PredicateBuilder<T1, T2>, Expression<Func<T1, T2, bool>>> predicateInitializer);
    #endregion

    #region Or
    /// <summary>
    /// 使用predicate表达式生成Or条件，并添加到已有的Where条件末尾，表达式predicate不可为null
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2> Or(Expression<Func<T1, T2, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件OR操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2> Or(bool condition, Expression<Func<T1, T2, bool>> ifPredicate = null, Expression<Func<T1, T2, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，并与已有的条件OR操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2> OrPredicate(Func<PredicateBuilder<T1, T2>, Expression<Func<T1, T2, bool>>> predicateInitializer);
    #endregion

    #region GroupBy
    /// <summary>
    /// 分组查询，分组表达式groupingExpr可以是单个或多个字段的匿名对象，如：
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
    /// <param name="groupingExpr">分组表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IGroupingQuery<T1, T2, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, TGrouping>> groupingExpr);
    #endregion

    #region OrderBy
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// OrderBy((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderBy((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2> OrderBy<TFields>(Expression<Func<T1, T2, TFields>> fieldsExpr);
    /// <summary>
    /// ASC排序，condition为true，ASC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderBy(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, TFields>> fieldsExpr);
    /// <summary>
    /// DSC排序，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// OrderByDescending((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2> OrderByDescending<TFields>(Expression<Func<T1, T2, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，condition为true，DESC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, TFields>> fieldsExpr);
    /// <summary>
    /// 动态排序，使用OrderByBuilder构建排序字段选择器，fieldsGetter可以是单个或多个字段的匿名对象，如：
    /// <code>string orderFields = "Name";bool isAsc = true;
    /// .OrderByDynamic(t =&gt; t.Switch(orderFields, isAsc).When("Name", (a, b, ...) =&lt; a.Name).When("Gender", (a, b, ...) =&lt; a.Gender).Build()</code>)
    /// </summary>
    /// <param name="fieldsGetter">排序字段选择器，需调用Build方法，返回排序字段表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2> OrderByDynamic(Func<OrderByBuilder<T1, T2>, Expression> fieldsGetter);
    #endregion

    #region Skip/Take/Page
    /// <summary>
    /// 跳过offset条数据
    /// </summary>
    /// <param name="offset">要跳过的数据条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2> Skip(int offset);
    /// <summary>
    /// 只返回limit条数据
    /// </summary>
    /// <param name="limit">返回的数据条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2> Take(int limit);
    /// <summary>
    /// 分页查询，pageNumber从1开始
    /// </summary>
    /// <param name="pageNumber">第几页，从1开始，小于1时当作1处理</param>
    /// <param name="pageSize">每页显示条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2> Page(int pageNumber, int pageSize);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，如：
    /// Select((a, b, ...) =&gt; new { a.Id, a.Name, ... }) 或是 Select((a, b, ...) =&gt; x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定字段返回，只需要指定需要特殊处理的成员赋值即可，其他成员将从现有表的字段中按名称匹配赋值，多个同名字段时如果未特殊指定赋值，默认选取第一个表中的字段赋值。如：
    /// <code> .SelectTo&lt;TDto&gt;() 或是 .SelectTo((a, b) =&gt; new TDto{ b.Id }) //使用第二个表的Id字段作为Id成员</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型，通常是一个匿名类</typeparam>
    /// <param name="specialMemberSelector">特殊成员赋值表达式，通常是重名字段或是不存在的字段赋值</param>
    /// <returns>返回查询对象</returns>
    IQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, TTarget>> specialMemberSelector = null);
    #endregion

    #region Count
    /// <summary>
    /// 返回某个字段的int类型数据条数，如：
    /// <code>.From&lt;T1, T2&gt;().Count((a, b) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的int类型数据条数</returns>
    int Count<TField>(Expression<Func<T1, T2, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的int类型数据条数，如：
    /// <code>.From&lt;T1, T2&gt;().CountAsync((a, b) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的int类型数据条数</returns>
    Task<int> CountAsync<TField>(Expression<Func<T1, T2, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，如：
    /// <code>.From&lt;T1, T2&gt;().CountDistinct((a, b) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段去重后的int类型数据条数</returns>
    int CountDistinct<TField>(Expression<Func<T1, T2, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，如：
    /// <code>.From&lt;T1, T2&gt;().CountDistinctAsync((a, b) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段去重后的int类型数据条数</returns>
    Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段的long类型数据条数，如：
    /// <code>.From&lt;T1, T2&gt;().LongCount((a, b) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的long类型数据条数</returns>
    long LongCount<TField>(Expression<Func<T1, T2, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的long类型数据条数，如：
    /// <code>.From&lt;T1, T2&gt;().LongCountAsync((a, b) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的long类型数据条数</returns>
    Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，如：
    /// <code>.From&lt;T1, T2&gt;().LongCountDistinct((a, b) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段去重后的long类型数据条数</returns>
    long LongCountDistinct<TField>(Expression<Func<T1, T2, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，如：
    /// <code>.From&lt;T1, T2&gt;().LongCountDistinctAsync((a, b) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段去重后的long类型数据条数</returns>
    Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, TField>> fieldExpr, CancellationToken cancellationToken = default);
    #endregion

    #region Aggregate
    /// <summary>
    /// 计算指定字段的求和值
    /// <code>.From&lt;T1, T2&gt;().Sum((a, b) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的求和值</returns>
    decimal Sum<TField>(Expression<Func<T1, T2, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的求和值
    /// <code>.From&lt;T1, T2&gt;().SumAsync((a, b) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的求和值</returns>
    Task<decimal> SumAsync<TField>(Expression<Func<T1, T2, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的平均值
    /// <code>.From&lt;T1, T2&gt;().Avg((a, b) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的平均值</returns>
    TField Avg<TField>(Expression<Func<T1, T2, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的平均值
    /// <code>.From&lt;T1, T2&gt;().AvgAsync((a, b) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的平均值</returns>
    Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的最大值
    /// <code>.From&lt;T1, T2&gt;().Max((a, b) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的最大值</returns>
    TField Max<TField>(Expression<Func<T1, T2, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最大值
    /// <code>.From&lt;T1, T2&gt;().MaxAsync((a, b) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的最大值</returns>
    Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的最小值
    /// <code>.From&lt;T1, T2&gt;().Min((a, b) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的最小值</returns>
    TField Min<TField>(Expression<Func<T1, T2, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最小值
    /// <code>.From&lt;T1, T2&gt;().MinAsync((a, b) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的最小值</returns>
    Task<TField> MinAsync<TField>(Expression<Func<T1, T2, TField>> fieldExpr, CancellationToken cancellationToken = default);
    #endregion
}
/// <summary>
/// 多表T1, T2, T3查询
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
public interface IQuery<T1, T2, T3> : IQueryBase
{
    #region Sharding
    /// <summary>
    /// 直接指定T3表分表名，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3> UseTable(params string[] tableNames);
    /// <summary>
    /// 根据首个分表TMasterSharding表与当前T3表名的映射关系，指定当前T3表分表名获取委托，执行委托获取当前T3表分表名。委托第一个参数是TMasterSharding主表原始表名，第二个参数是当前T3表原始表名，第三个参数是TMasterSharding表当前分表名，返回值是当前T3表分表名称，如：
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
    /// <param name="tableNameGetter">T3表分表名获取委托</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter);
    /// <summary>
    /// 手动指定分表规则参数值，执行分表规则确定T3表分表名，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，最多支持3个字段值，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3> UseTableBy(params object[] fieldValues);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T3表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginFieldValue &lt;= endFieldValue，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="beginFieldValue">字段范围起始值</param>
    /// <param name="endFieldValue">字段范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3> UseTableByRange(object beginFieldValue, object endFieldValue);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T3表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginField2Value &lt;= endField2Value，常用于是日期规则分表，如：.UseTableByRange(1, DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="beginField2Value">字段2范围起始值</param>
    /// <param name="endField2Value">字段2范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3> UseTableByRange(object field1Value, object beginField2Value, object endField2Value);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T3表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginField3Value &lt;= endField3Value，常用于是日期规则分表，如：.UseTableByRange(1, "Game1", DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="field2Value">字段2值</param>
    /// <param name="beginField3Value">字段3范围起始值</param>
    /// <param name="endField3Value">字段3范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value);
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3> UseTableSchema(string tableSchema);
    #endregion

    #region WithTable
    /// <summary>
    /// 添加实体表，方便后面做JOIN关联，如：<code>.From&lt;Menu&gt;().WithTable&lt;Page&gt;()</code>
    /// </summary>
    /// <typeparam name="TOther">实体表类型</typeparam>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, TOther> WithTable<TOther>();
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
    IQuery<T1, T2, T3, TOther> WithQuery<TOther>(IQuery<TOther> subQuery);
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
    IQuery<T1, T2, T3, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr);
    #endregion

    #region Include
    /// <summary>
    /// 加载导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，1:1关联关系，随主表一起查询,支持无限级，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .From&lt;Product&gt;().Include(f =&gt; f.Brand) ...
    /// .From&lt;Brand&gt;().Include(f =&gt; f.Products) ...
    /// .From&lt;Order&gt;().Include(f =&gt; f.Seller.Company.Products) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TMember">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, TMember> Include<TMember>(Expression<Func<T1, T2, T3, TMember>> memberSelector);
    /// <summary>
    /// 加载集合类导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，可使用filter筛选满足条件的导航属性，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .From&lt;User&gt;().IncludeMany(f =&gt; f.Orders)  //与 .From&lt;User&gt;().Include(f =&gt; f.Orders) 等价
    /// .From&lt;User&gt;().IncludeMany(f =&gt; f.Orders, order =&gt; order.TotalAmout &gt; 500)
    /// </code>
    /// </summary>
    /// <typeparam name="TElement">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null);
    #endregion

    #region InnerJoin
    /// <summary>
    /// 在现有表中，选择两个表进行INNER JOIN关联，可多次关联，如：<code>.InnerJoin((a, b, ...) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3> InnerJoin(Expression<Func<T1, T2, T3, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做INNER JOIN关联，与.WithTable&lt;TOther&gt;().InnerJoin(...)等价，如：
    /// <code>
    /// .From&lt;User&gt;().InnerJoin&lt;Order&gt;((x, y) =&gt; x.Id == y.BuyerId)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, TOther, bool>> joinOn);
    #endregion

    #region LeftJoin
    /// <summary>
    /// 在现有表中，选择两个表进行LEFT JOIN关联，可多次关联，如：<code>.LeftJoin((a, b, ...) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3> LeftJoin(Expression<Func<T1, T2, T3, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做LEFT JOIN关联，与.WithTable&lt;TOther&gt;().LeftJoin(...)等价，如：
    /// <code>
    /// .LeftJoin&lt;TOther&gt;((a, b, ...) =&gt; a.xxx = b.yyy)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, TOther, bool>> joinOn);
    #endregion

    #region RightJoin
    /// <summary>
    /// 在现有表中，选择两个表进行RIGHT JOIN关联，可多次关联，如：<code>.RightJoin((a, b) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3> RightJoin(Expression<Func<T1, T2, T3, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做RIGHT JOIN关联，与.WithTable&lt;TOther&gt;().RightJoin(...)等价，如：
    /// <code>
    /// .RightJoin&lt;TOther&gt;((a, b, c) =&gt; ...)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, TOther, bool>> joinOn);
    #endregion

    #region Where
    /// <summary>
    /// 条件查询，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3> Where(Expression<Func<T1, T2, T3, bool>> predicate);
    /// <summary>
    /// 条件查询，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3> Where(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate, Expression<Func<T1, T2, T3, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3> WherePredicate(Func<PredicateBuilder<T1, T2, T3>, Expression<Func<T1, T2, T3, bool>>> predicateInitializer);
    #endregion

    #region And
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3> And(Expression<Func<T1, T2, T3, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3> And(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，并与已有的条件AND操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3> AndPredicate(Func<PredicateBuilder<T1, T2, T3>, Expression<Func<T1, T2, T3, bool>>> predicateInitializer);
    #endregion

    #region Or
    /// <summary>
    /// 使用predicate表达式生成Or条件，并添加到已有的Where条件末尾，表达式predicate不可为null
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3> Or(Expression<Func<T1, T2, T3, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件OR操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3> Or(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，并与已有的条件OR操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3> OrPredicate(Func<PredicateBuilder<T1, T2, T3>, Expression<Func<T1, T2, T3, bool>>> predicateInitializer);
    #endregion

    #region GroupBy
    /// <summary>
    /// 分组查询，分组表达式groupingExpr可以是单个或多个字段的匿名对象，如：
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
    /// <param name="groupingExpr">分组表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IGroupingQuery<T1, T2, T3, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, TGrouping>> groupingExpr);
    #endregion

    #region OrderBy
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// OrderBy((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderBy((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3> OrderBy<TFields>(Expression<Func<T1, T2, T3, TFields>> fieldsExpr);
    /// <summary>
    /// ASC排序，condition为true，ASC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderBy(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, TFields>> fieldsExpr);
    /// <summary>
    /// DSC排序，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// OrderByDescending((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，condition为true，DESC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, TFields>> fieldsExpr);
    /// <summary>
    /// 动态排序，使用OrderByBuilder构建排序字段选择器，fieldsGetter可以是单个或多个字段的匿名对象，如：
    /// <code>string orderFields = "Name";bool isAsc = true;
    /// .OrderByDynamic(t =&gt; t.Switch(orderFields, isAsc).When("Name", (a, b, ...) =&lt; a.Name).When("Gender", (a, b, ...) =&lt; a.Gender).Build()</code>)
    /// </summary>
    /// <param name="fieldsGetter">排序字段选择器，需调用Build方法，返回排序字段表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3>, Expression> fieldsGetter);
    #endregion

    #region Skip/Take/Page
    /// <summary>
    /// 跳过offset条数据
    /// </summary>
    /// <param name="offset">要跳过的数据条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3> Skip(int offset);
    /// <summary>
    /// 只返回limit条数据
    /// </summary>
    /// <param name="limit">返回的数据条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3> Take(int limit);
    /// <summary>
    /// 分页查询，pageNumber从1开始
    /// </summary>
    /// <param name="pageNumber">第几页，从1开始，小于1时当作1处理</param>
    /// <param name="pageSize">每页显示条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3> Page(int pageNumber, int pageSize);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，如：
    /// Select((a, b, ...) =&gt; new { a.Id, a.Name, ... }) 或是 Select((a, b, ...) =&gt; x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定字段返回，只需要指定需要特殊处理的成员赋值即可，其他成员将从现有表的字段中按名称匹配赋值，多个同名字段时如果未特殊指定赋值，默认选取第一个表中的字段赋值。如：
    /// <code> .SelectTo&lt;TDto&gt;() 或是 .SelectTo((a, b) =&gt; new TDto{ b.Id }) //使用第二个表的Id字段作为Id成员</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型，通常是一个匿名类</typeparam>
    /// <param name="specialMemberSelector">特殊成员赋值表达式，通常是重名字段或是不存在的字段赋值</param>
    /// <returns>返回查询对象</returns>
    IQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, TTarget>> specialMemberSelector = null);
    #endregion

    #region Count
    /// <summary>
    /// 返回某个字段的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3&gt;().Count((a, b, c) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的int类型数据条数</returns>
    int Count<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3&gt;().CountAsync((a, b, c) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的int类型数据条数</returns>
    Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3&gt;().CountDistinct((a, b, c) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段去重后的int类型数据条数</returns>
    int CountDistinct<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3&gt;().CountDistinctAsync((a, b, c) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段去重后的int类型数据条数</returns>
    Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3&gt;().LongCount((a, b, c) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的long类型数据条数</returns>
    long LongCount<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3&gt;().LongCountAsync((a, b, c) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的long类型数据条数</returns>
    Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3&gt;().LongCountDistinct((a, b, c) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段去重后的long类型数据条数</returns>
    long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3&gt;().LongCountDistinctAsync((a, b, c) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段去重后的long类型数据条数</returns>
    Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr, CancellationToken cancellationToken = default);
    #endregion

    #region Aggregate
    /// <summary>
    /// 计算指定字段的求和值
    /// <code>.From&lt;T1, T2, T3&gt;().Sum((a, b, c) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的求和值</returns>
    decimal Sum<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的求和值
    /// <code>.From&lt;T1, T2, T3&gt;().SumAsync((a, b, c) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的求和值</returns>
    Task<decimal> SumAsync<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的平均值
    /// <code>.From&lt;T1, T2, T3&gt;().Avg((a, b, c) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的平均值</returns>
    TField Avg<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的平均值
    /// <code>.From&lt;T1, T2, T3&gt;().AvgAsync((a, b, c) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的平均值</returns>
    Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的最大值
    /// <code>.From&lt;T1, T2, T3&gt;().Max((a, b, c) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的最大值</returns>
    TField Max<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最大值
    /// <code>.From&lt;T1, T2, T3&gt;().MaxAsync((a, b, c) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的最大值</returns>
    Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的最小值
    /// <code>.From&lt;T1, T2, T3&gt;().Min((a, b, c) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的最小值</returns>
    TField Min<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最小值
    /// <code>.From&lt;T1, T2, T3&gt;().MinAsync((a, b, c) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的最小值</returns>
    Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr, CancellationToken cancellationToken = default);
    #endregion
}
/// <summary>
/// 多表T1, T2, T3, T4查询
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
public interface IQuery<T1, T2, T3, T4> : IQueryBase
{
    #region Sharding
    /// <summary>
    /// 直接指定T4表分表名，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4> UseTable(params string[] tableNames);
    /// <summary>
    /// 根据首个分表TMasterSharding表与当前T4表名的映射关系，指定当前T4表分表名获取委托，执行委托获取当前T4表分表名。委托第一个参数是TMasterSharding主表原始表名，第二个参数是当前T4表原始表名，第三个参数是TMasterSharding表当前分表名，返回值是当前T4表分表名称，如：
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
    /// <param name="tableNameGetter">T4表分表名获取委托</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter);
    /// <summary>
    /// 手动指定分表规则参数值，执行分表规则确定T4表分表名，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，最多支持3个字段值，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4> UseTableBy(params object[] fieldValues);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T4表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginFieldValue &lt;= endFieldValue，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="beginFieldValue">字段范围起始值</param>
    /// <param name="endFieldValue">字段范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4> UseTableByRange(object beginFieldValue, object endFieldValue);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T4表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginField2Value &lt;= endField2Value，常用于是日期规则分表，如：.UseTableByRange(1, DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="beginField2Value">字段2范围起始值</param>
    /// <param name="endField2Value">字段2范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4> UseTableByRange(object field1Value, object beginField2Value, object endField2Value);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T4表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginField3Value &lt;= endField3Value，常用于是日期规则分表，如：.UseTableByRange(1, "Game1", DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="field2Value">字段2值</param>
    /// <param name="beginField3Value">字段3范围起始值</param>
    /// <param name="endField3Value">字段3范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value);
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4> UseTableSchema(string tableSchema);
    #endregion

    #region WithTable
    /// <summary>
    /// 添加实体表，方便后面做JOIN关联，如：<code>.From&lt;Menu&gt;().WithTable&lt;Page&gt;()</code>
    /// </summary>
    /// <typeparam name="TOther">实体表类型</typeparam>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, TOther> WithTable<TOther>();
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
    IQuery<T1, T2, T3, T4, TOther> WithQuery<TOther>(IQuery<TOther> subQuery);
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
    IQuery<T1, T2, T3, T4, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr);
    #endregion

    #region Include
    /// <summary>
    /// 加载导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，1:1关联关系，随主表一起查询,支持无限级，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .From&lt;Product&gt;().Include(f =&gt; f.Brand) ...
    /// .From&lt;Brand&gt;().Include(f =&gt; f.Products) ...
    /// .From&lt;Order&gt;().Include(f =&gt; f.Seller.Company.Products) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TMember">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, TMember>> memberSelector);
    /// <summary>
    /// 加载集合类导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，可使用filter筛选满足条件的导航属性，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .From&lt;User&gt;().IncludeMany(f =&gt; f.Orders)  //与 .From&lt;User&gt;().Include(f =&gt; f.Orders) 等价
    /// .From&lt;User&gt;().IncludeMany(f =&gt; f.Orders, order =&gt; order.TotalAmout &gt; 500)
    /// </code>
    /// </summary>
    /// <typeparam name="TElement">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null);
    #endregion

    #region InnerJoin
    /// <summary>
    /// 在现有表中，选择两个表进行INNER JOIN关联，可多次关联，如：<code>.InnerJoin((a, b, ...) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4> InnerJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做INNER JOIN关联，与.WithTable&lt;TOther&gt;().InnerJoin(...)等价，如：
    /// <code>
    /// .From&lt;User&gt;().InnerJoin&lt;Order&gt;((x, y) =&gt; x.Id == y.BuyerId)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, T4, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, T4, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn);
    #endregion

    #region LeftJoin
    /// <summary>
    /// 在现有表中，选择两个表进行LEFT JOIN关联，可多次关联，如：<code>.LeftJoin((a, b, ...) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4> LeftJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做LEFT JOIN关联，与.WithTable&lt;TOther&gt;().LeftJoin(...)等价，如：
    /// <code>
    /// .LeftJoin&lt;TOther&gt;((a, b, ...) =&gt; a.xxx = b.yyy)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, T4, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, T4, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn);
    #endregion

    #region RightJoin
    /// <summary>
    /// 在现有表中，选择两个表进行RIGHT JOIN关联，可多次关联，如：<code>.RightJoin((a, b) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4> RightJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做RIGHT JOIN关联，与.WithTable&lt;TOther&gt;().RightJoin(...)等价，如：
    /// <code>
    /// .RightJoin&lt;TOther&gt;((a, b, c, d) =&gt; ...)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, T4, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, T4, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn);
    #endregion

    #region Where
    /// <summary>
    /// 条件查询，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4> Where(Expression<Func<T1, T2, T3, T4, bool>> predicate);
    /// <summary>
    /// 条件查询，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4> Where(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4>, Expression<Func<T1, T2, T3, T4, bool>>> predicateInitializer);
    #endregion

    #region And
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4> And(Expression<Func<T1, T2, T3, T4, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4> And(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，并与已有的条件AND操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4>, Expression<Func<T1, T2, T3, T4, bool>>> predicateInitializer);
    #endregion

    #region Or
    /// <summary>
    /// 使用predicate表达式生成Or条件，并添加到已有的Where条件末尾，表达式predicate不可为null
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4> Or(Expression<Func<T1, T2, T3, T4, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件OR操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4> Or(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，并与已有的条件OR操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4>, Expression<Func<T1, T2, T3, T4, bool>>> predicateInitializer);
    #endregion

    #region GroupBy
    /// <summary>
    /// 分组查询，分组表达式groupingExpr可以是单个或多个字段的匿名对象，如：
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
    /// <param name="groupingExpr">分组表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IGroupingQuery<T1, T2, T3, T4, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, TGrouping>> groupingExpr);
    #endregion

    #region OrderBy
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// OrderBy((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderBy((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr);
    /// <summary>
    /// ASC排序，condition为true，ASC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderBy(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr);
    /// <summary>
    /// DSC排序，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// OrderByDescending((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，condition为true，DESC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr);
    /// <summary>
    /// 动态排序，使用OrderByBuilder构建排序字段选择器，fieldsGetter可以是单个或多个字段的匿名对象，如：
    /// <code>string orderFields = "Name";bool isAsc = true;
    /// .OrderByDynamic(t =&gt; t.Switch(orderFields, isAsc).When("Name", (a, b, ...) =&lt; a.Name).When("Gender", (a, b, ...) =&lt; a.Gender).Build()</code>)
    /// </summary>
    /// <param name="fieldsGetter">排序字段选择器，需调用Build方法，返回排序字段表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4>, Expression> fieldsGetter);
    #endregion

    #region Skip/Take/Page
    /// <summary>
    /// 跳过offset条数据
    /// </summary>
    /// <param name="offset">要跳过的数据条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4> Skip(int offset);
    /// <summary>
    /// 只返回limit条数据
    /// </summary>
    /// <param name="limit">返回的数据条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4> Take(int limit);
    /// <summary>
    /// 分页查询，pageNumber从1开始
    /// </summary>
    /// <param name="pageNumber">第几页，从1开始，小于1时当作1处理</param>
    /// <param name="pageSize">每页显示条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4> Page(int pageNumber, int pageSize);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，如：
    /// Select((a, b, ...) =&gt; new { a.Id, a.Name, ... }) 或是 Select((a, b, ...) =&gt; x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定字段返回，只需要指定需要特殊处理的成员赋值即可，其他成员将从现有表的字段中按名称匹配赋值，多个同名字段时如果未特殊指定赋值，默认选取第一个表中的字段赋值。如：
    /// <code> .SelectTo&lt;TDto&gt;() 或是 .SelectTo((a, b) =&gt; new TDto{ b.Id }) //使用第二个表的Id字段作为Id成员</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型，通常是一个匿名类</typeparam>
    /// <param name="specialMemberSelector">特殊成员赋值表达式，通常是重名字段或是不存在的字段赋值</param>
    /// <returns>返回查询对象</returns>
    IQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, TTarget>> specialMemberSelector = null);
    #endregion

    #region Count
    /// <summary>
    /// 返回某个字段的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4&gt;().Count((a, b, c, d) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的int类型数据条数</returns>
    int Count<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4&gt;().CountAsync((a, b, c, d) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的int类型数据条数</returns>
    Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4&gt;().CountDistinct((a, b, c, d) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段去重后的int类型数据条数</returns>
    int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4&gt;().CountDistinctAsync((a, b, c, d) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段去重后的int类型数据条数</returns>
    Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4&gt;().LongCount((a, b, c, d) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的long类型数据条数</returns>
    long LongCount<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4&gt;().LongCountAsync((a, b, c, d) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的long类型数据条数</returns>
    Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4&gt;().LongCountDistinct((a, b, c, d) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段去重后的long类型数据条数</returns>
    long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4&gt;().LongCountDistinctAsync((a, b, c, d) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段去重后的long类型数据条数</returns>
    Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr, CancellationToken cancellationToken = default);
    #endregion

    #region Aggregate
    /// <summary>
    /// 计算指定字段的求和值
    /// <code>.From&lt;T1, T2, T3, T4&gt;().Sum((a, b, c, d) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的求和值</returns>
    decimal Sum<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的求和值
    /// <code>.From&lt;T1, T2, T3, T4&gt;().SumAsync((a, b, c, d) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的求和值</returns>
    Task<decimal> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的平均值
    /// <code>.From&lt;T1, T2, T3, T4&gt;().Avg((a, b, c, d) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的平均值</returns>
    TField Avg<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的平均值
    /// <code>.From&lt;T1, T2, T3, T4&gt;().AvgAsync((a, b, c, d) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的平均值</returns>
    Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的最大值
    /// <code>.From&lt;T1, T2, T3, T4&gt;().Max((a, b, c, d) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的最大值</returns>
    TField Max<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最大值
    /// <code>.From&lt;T1, T2, T3, T4&gt;().MaxAsync((a, b, c, d) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的最大值</returns>
    Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的最小值
    /// <code>.From&lt;T1, T2, T3, T4&gt;().Min((a, b, c, d) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的最小值</returns>
    TField Min<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最小值
    /// <code>.From&lt;T1, T2, T3, T4&gt;().MinAsync((a, b, c, d) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的最小值</returns>
    Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr, CancellationToken cancellationToken = default);
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
public interface IQuery<T1, T2, T3, T4, T5> : IQueryBase
{
    #region Sharding
    /// <summary>
    /// 直接指定T5表分表名，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5> UseTable(params string[] tableNames);
    /// <summary>
    /// 根据首个分表TMasterSharding表与当前T5表名的映射关系，指定当前T5表分表名获取委托，执行委托获取当前T5表分表名。委托第一个参数是TMasterSharding主表原始表名，第二个参数是当前T5表原始表名，第三个参数是TMasterSharding表当前分表名，返回值是当前T5表分表名称，如：
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
    /// <param name="tableNameGetter">T5表分表名获取委托</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter);
    /// <summary>
    /// 手动指定分表规则参数值，执行分表规则确定T5表分表名，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，最多支持3个字段值，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5> UseTableBy(params object[] fieldValues);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T5表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginFieldValue &lt;= endFieldValue，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="beginFieldValue">字段范围起始值</param>
    /// <param name="endFieldValue">字段范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5> UseTableByRange(object beginFieldValue, object endFieldValue);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T5表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginField2Value &lt;= endField2Value，常用于是日期规则分表，如：.UseTableByRange(1, DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="beginField2Value">字段2范围起始值</param>
    /// <param name="endField2Value">字段2范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5> UseTableByRange(object field1Value, object beginField2Value, object endField2Value);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T5表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginField3Value &lt;= endField3Value，常用于是日期规则分表，如：.UseTableByRange(1, "Game1", DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="field2Value">字段2值</param>
    /// <param name="beginField3Value">字段3范围起始值</param>
    /// <param name="endField3Value">字段3范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value);
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5> UseTableSchema(string tableSchema);
    #endregion

    #region WithTable
    /// <summary>
    /// 添加实体表，方便后面做JOIN关联，如：<code>.From&lt;Menu&gt;().WithTable&lt;Page&gt;()</code>
    /// </summary>
    /// <typeparam name="TOther">实体表类型</typeparam>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, TOther> WithTable<TOther>();
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
    IQuery<T1, T2, T3, T4, T5, TOther> WithQuery<TOther>(IQuery<TOther> subQuery);
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
    IQuery<T1, T2, T3, T4, T5, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr);
    #endregion

    #region Include
    /// <summary>
    /// 加载导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，1:1关联关系，随主表一起查询,支持无限级，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .From&lt;Product&gt;().Include(f =&gt; f.Brand) ...
    /// .From&lt;Brand&gt;().Include(f =&gt; f.Products) ...
    /// .From&lt;Order&gt;().Include(f =&gt; f.Seller.Company.Products) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TMember">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, TMember>> memberSelector);
    /// <summary>
    /// 加载集合类导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，可使用filter筛选满足条件的导航属性，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .From&lt;User&gt;().IncludeMany(f =&gt; f.Orders)  //与 .From&lt;User&gt;().Include(f =&gt; f.Orders) 等价
    /// .From&lt;User&gt;().IncludeMany(f =&gt; f.Orders, order =&gt; order.TotalAmout &gt; 500)
    /// </code>
    /// </summary>
    /// <typeparam name="TElement">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null);
    #endregion

    #region InnerJoin
    /// <summary>
    /// 在现有表中，选择两个表进行INNER JOIN关联，可多次关联，如：<code>.InnerJoin((a, b, ...) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做INNER JOIN关联，与.WithTable&lt;TOther&gt;().InnerJoin(...)等价，如：
    /// <code>
    /// .From&lt;User&gt;().InnerJoin&lt;Order&gt;((x, y) =&gt; x.Id == y.BuyerId)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn);
    #endregion

    #region LeftJoin
    /// <summary>
    /// 在现有表中，选择两个表进行LEFT JOIN关联，可多次关联，如：<code>.LeftJoin((a, b, ...) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做LEFT JOIN关联，与.WithTable&lt;TOther&gt;().LeftJoin(...)等价，如：
    /// <code>
    /// .LeftJoin&lt;TOther&gt;((a, b, ...) =&gt; a.xxx = b.yyy)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn);
    #endregion

    #region RightJoin
    /// <summary>
    /// 在现有表中，选择两个表进行RIGHT JOIN关联，可多次关联，如：<code>.RightJoin((a, b) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5> RightJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做RIGHT JOIN关联，与.WithTable&lt;TOther&gt;().RightJoin(...)等价，如：
    /// <code>
    /// .RightJoin&lt;TOther&gt;((a, b, c, d, e) =&gt; ...)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn);
    #endregion

    #region Where
    /// <summary>
    /// 条件查询，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5> Where(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate);
    /// <summary>
    /// 条件查询，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5>, Expression<Func<T1, T2, T3, T4, T5, bool>>> predicateInitializer);
    #endregion

    #region And
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5> And(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，并与已有的条件AND操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5>, Expression<Func<T1, T2, T3, T4, T5, bool>>> predicateInitializer);
    #endregion

    #region Or
    /// <summary>
    /// 使用predicate表达式生成Or条件，并添加到已有的Where条件末尾，表达式predicate不可为null
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5> Or(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件OR操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，并与已有的条件OR操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5>, Expression<Func<T1, T2, T3, T4, T5, bool>>> predicateInitializer);
    #endregion

    #region GroupBy
    /// <summary>
    /// 分组查询，分组表达式groupingExpr可以是单个或多个字段的匿名对象，如：
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
    /// <param name="groupingExpr">分组表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IGroupingQuery<T1, T2, T3, T4, T5, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, TGrouping>> groupingExpr);
    #endregion

    #region OrderBy
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// OrderBy((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderBy((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    /// <summary>
    /// ASC排序，condition为true，ASC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderBy(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    /// <summary>
    /// DSC排序，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// OrderByDescending((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，condition为true，DESC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    /// <summary>
    /// 动态排序，使用OrderByBuilder构建排序字段选择器，fieldsGetter可以是单个或多个字段的匿名对象，如：
    /// <code>string orderFields = "Name";bool isAsc = true;
    /// .OrderByDynamic(t =&gt; t.Switch(orderFields, isAsc).When("Name", (a, b, ...) =&lt; a.Name).When("Gender", (a, b, ...) =&lt; a.Gender).Build()</code>)
    /// </summary>
    /// <param name="fieldsGetter">排序字段选择器，需调用Build方法，返回排序字段表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5>, Expression> fieldsGetter);
    #endregion

    #region Skip/Take/Page
    /// <summary>
    /// 跳过offset条数据
    /// </summary>
    /// <param name="offset">要跳过的数据条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5> Skip(int offset);
    /// <summary>
    /// 只返回limit条数据
    /// </summary>
    /// <param name="limit">返回的数据条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5> Take(int limit);
    /// <summary>
    /// 分页查询，pageNumber从1开始
    /// </summary>
    /// <param name="pageNumber">第几页，从1开始，小于1时当作1处理</param>
    /// <param name="pageSize">每页显示条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5> Page(int pageNumber, int pageSize);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，如：
    /// Select((a, b, ...) =&gt; new { a.Id, a.Name, ... }) 或是 Select((a, b, ...) =&gt; x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定字段返回，只需要指定需要特殊处理的成员赋值即可，其他成员将从现有表的字段中按名称匹配赋值，多个同名字段时如果未特殊指定赋值，默认选取第一个表中的字段赋值。如：
    /// <code> .SelectTo&lt;TDto&gt;() 或是 .SelectTo((a, b) =&gt; new TDto{ b.Id }) //使用第二个表的Id字段作为Id成员</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型，通常是一个匿名类</typeparam>
    /// <param name="specialMemberSelector">特殊成员赋值表达式，通常是重名字段或是不存在的字段赋值</param>
    /// <returns>返回查询对象</returns>
    IQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, TTarget>> specialMemberSelector = null);
    #endregion

    #region Count
    /// <summary>
    /// 返回某个字段的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5&gt;().Count((a, b, c, d, e) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的int类型数据条数</returns>
    int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5&gt;().CountAsync((a, b, c, d, e) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的int类型数据条数</returns>
    Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5&gt;().CountDistinct((a, b, c, d, e) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段去重后的int类型数据条数</returns>
    int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5&gt;().CountDistinctAsync((a, b, c, d, e) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段去重后的int类型数据条数</returns>
    Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5&gt;().LongCount((a, b, c, d, e) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的long类型数据条数</returns>
    long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5&gt;().LongCountAsync((a, b, c, d, e) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的long类型数据条数</returns>
    Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5&gt;().LongCountDistinct((a, b, c, d, e) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段去重后的long类型数据条数</returns>
    long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5&gt;().LongCountDistinctAsync((a, b, c, d, e) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段去重后的long类型数据条数</returns>
    Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr, CancellationToken cancellationToken = default);
    #endregion

    #region Aggregate
    /// <summary>
    /// 计算指定字段的求和值
    /// <code>.From&lt;T1, T2, T3, T4, T5&gt;().Sum((a, b, c, d, e) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的求和值</returns>
    decimal Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的求和值
    /// <code>.From&lt;T1, T2, T3, T4, T5&gt;().SumAsync((a, b, c, d, e) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的求和值</returns>
    Task<decimal> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的平均值
    /// <code>.From&lt;T1, T2, T3, T4, T5&gt;().Avg((a, b, c, d, e) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的平均值</returns>
    TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的平均值
    /// <code>.From&lt;T1, T2, T3, T4, T5&gt;().AvgAsync((a, b, c, d, e) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的平均值</returns>
    Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的最大值
    /// <code>.From&lt;T1, T2, T3, T4, T5&gt;().Max((a, b, c, d, e) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的最大值</returns>
    TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最大值
    /// <code>.From&lt;T1, T2, T3, T4, T5&gt;().MaxAsync((a, b, c, d, e) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的最大值</returns>
    Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的最小值
    /// <code>.From&lt;T1, T2, T3, T4, T5&gt;().Min((a, b, c, d, e) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的最小值</returns>
    TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最小值
    /// <code>.From&lt;T1, T2, T3, T4, T5&gt;().MinAsync((a, b, c, d, e) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的最小值</returns>
    Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr, CancellationToken cancellationToken = default);
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
public interface IQuery<T1, T2, T3, T4, T5, T6> : IQueryBase
{
    #region Sharding
    /// <summary>
    /// 直接指定T6表分表名，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6> UseTable(params string[] tableNames);
    /// <summary>
    /// 根据首个分表TMasterSharding表与当前T6表名的映射关系，指定当前T6表分表名获取委托，执行委托获取当前T6表分表名。委托第一个参数是TMasterSharding主表原始表名，第二个参数是当前T6表原始表名，第三个参数是TMasterSharding表当前分表名，返回值是当前T6表分表名称，如：
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
    /// <param name="tableNameGetter">T6表分表名获取委托</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter);
    /// <summary>
    /// 手动指定分表规则参数值，执行分表规则确定T6表分表名，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，最多支持3个字段值，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6> UseTableBy(params object[] fieldValues);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T6表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginFieldValue &lt;= endFieldValue，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="beginFieldValue">字段范围起始值</param>
    /// <param name="endFieldValue">字段范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6> UseTableByRange(object beginFieldValue, object endFieldValue);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T6表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginField2Value &lt;= endField2Value，常用于是日期规则分表，如：.UseTableByRange(1, DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="beginField2Value">字段2范围起始值</param>
    /// <param name="endField2Value">字段2范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6> UseTableByRange(object field1Value, object beginField2Value, object endField2Value);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T6表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginField3Value &lt;= endField3Value，常用于是日期规则分表，如：.UseTableByRange(1, "Game1", DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="field2Value">字段2值</param>
    /// <param name="beginField3Value">字段3范围起始值</param>
    /// <param name="endField3Value">字段3范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value);
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6> UseTableSchema(string tableSchema);
    #endregion

    #region WithTable
    /// <summary>
    /// 添加实体表，方便后面做JOIN关联，如：<code>.From&lt;Menu&gt;().WithTable&lt;Page&gt;()</code>
    /// </summary>
    /// <typeparam name="TOther">实体表类型</typeparam>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, TOther> WithTable<TOther>();
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
    IQuery<T1, T2, T3, T4, T5, T6, TOther> WithQuery<TOther>(IQuery<TOther> subQuery);
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
    IQuery<T1, T2, T3, T4, T5, T6, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr);
    #endregion

    #region Include
    /// <summary>
    /// 加载导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，1:1关联关系，随主表一起查询,支持无限级，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .From&lt;Product&gt;().Include(f =&gt; f.Brand) ...
    /// .From&lt;Brand&gt;().Include(f =&gt; f.Products) ...
    /// .From&lt;Order&gt;().Include(f =&gt; f.Seller.Company.Products) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TMember">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, TMember>> memberSelector);
    /// <summary>
    /// 加载集合类导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，可使用filter筛选满足条件的导航属性，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .From&lt;User&gt;().IncludeMany(f =&gt; f.Orders)  //与 .From&lt;User&gt;().Include(f =&gt; f.Orders) 等价
    /// .From&lt;User&gt;().IncludeMany(f =&gt; f.Orders, order =&gt; order.TotalAmout &gt; 500)
    /// </code>
    /// </summary>
    /// <typeparam name="TElement">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, T6, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null);
    #endregion

    #region InnerJoin
    /// <summary>
    /// 在现有表中，选择两个表进行INNER JOIN关联，可多次关联，如：<code>.InnerJoin((a, b, ...) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做INNER JOIN关联，与.WithTable&lt;TOther&gt;().InnerJoin(...)等价，如：
    /// <code>
    /// .From&lt;User&gt;().InnerJoin&lt;Order&gt;((x, y) =&gt; x.Id == y.BuyerId)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, T4, T5, T6, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr表达式构建子查询，并选择一个现有表做INNER JOIN关联，与.WithQuery(subQueryExpr).InnerJoin(...)等价，如：
    /// <code>
    /// .InnerJoin(f =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select(x =&gt; new { ... }), (a, b, ...) =&gt; x.xxx = y.yyy) ...
    /// SQL: ... INNER JOIN (SELECT ... FROM `sys_order_detail` ...) g ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn);
    #endregion

    #region LeftJoin
    /// <summary>
    /// 在现有表中，选择两个表进行LEFT JOIN关联，可多次关联，如：<code>.LeftJoin((a, b, ...) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做LEFT JOIN关联，与.WithTable&lt;TOther&gt;().LeftJoin(...)等价，如：
    /// <code>
    /// .LeftJoin&lt;TOther&gt;((a, b, ...) =&gt; a.xxx = b.yyy)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, T4, T5, T6, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr表达式构建子查询，并选择一个现有表做LEFT JOIN关联，与.WithQuery(subQueryExpr).LeftJoin(...)等价，如：
    /// <code>
    /// .LeftJoin(f =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select(t =&gt; new { ... }), (a, b, ...) =&gt; a.Id = b.OrderId) ...
    /// SQL: ... LEFT JOIN (SELECT ... FROM `sys_order_detail` ...) g ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn);
    #endregion

    #region RightJoin
    /// <summary>
    /// 在现有表中，选择两个表进行RIGHT JOIN关联，可多次关联，如：<code>.RightJoin((a, b) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做RIGHT JOIN关联，与.WithTable&lt;TOther&gt;().RightJoin(...)等价，如：
    /// <code>
    /// .RightJoin&lt;TOther&gt;((a, b, c, d, e, f) =&gt; ...)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, T4, T5, T6, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr表达式构建子查询，并选择一个现有表做RIGHT JOIN关联，与.WithQuery(subQueryExpr).RightJoin(...)等价，如：
    /// <code>
    /// .RightJoin(f =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select(t =&gt; new { ... }), (a, b, ...) =&gt; a.Id = b.OrderId) ...
    /// SQL: ... RIGHT JOIN (SELECT ... FROM `sys_order_detail` ...) g ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn);
    #endregion

    #region Where
    /// <summary>
    /// 条件查询，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6> Where(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate);
    /// <summary>
    /// 条件查询，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6>, Expression<Func<T1, T2, T3, T4, T5, T6, bool>>> predicateInitializer);
    #endregion

    #region And
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6> And(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，并与已有的条件AND操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6>, Expression<Func<T1, T2, T3, T4, T5, T6, bool>>> predicateInitializer);
    #endregion

    #region Or
    /// <summary>
    /// 使用predicate表达式生成Or条件，并添加到已有的Where条件末尾，表达式predicate不可为null
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6> Or(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件OR操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，并与已有的条件OR操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6>, Expression<Func<T1, T2, T3, T4, T5, T6, bool>>> predicateInitializer);
    #endregion

    #region GroupBy
    /// <summary>
    /// 分组查询，分组表达式groupingExpr可以是单个或多个字段的匿名对象，如：
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
    /// <param name="groupingExpr">分组表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, TGrouping>> groupingExpr);
    #endregion

    #region OrderBy
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// OrderBy((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderBy((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr);
    /// <summary>
    /// ASC排序，condition为true，ASC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderBy(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr);
    /// <summary>
    /// DSC排序，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// OrderByDescending((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，condition为true，DESC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr);
    /// <summary>
    /// 动态排序，使用OrderByBuilder构建排序字段选择器，fieldsGetter可以是单个或多个字段的匿名对象，如：
    /// <code>string orderFields = "Name";bool isAsc = true;
    /// .OrderByDynamic(t =&gt; t.Switch(orderFields, isAsc).When("Name", (a, b, ...) =&lt; a.Name).When("Gender", (a, b, ...) =&lt; a.Gender).Build()</code>)
    /// </summary>
    /// <param name="fieldsGetter">排序字段选择器，需调用Build方法，返回排序字段表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6>, Expression> fieldsGetter);
    #endregion

    #region Skip/Take/Page
    /// <summary>
    /// 跳过offset条数据
    /// </summary>
    /// <param name="offset">要跳过的数据条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6> Skip(int offset);
    /// <summary>
    /// 只返回limit条数据
    /// </summary>
    /// <param name="limit">返回的数据条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6> Take(int limit);
    /// <summary>
    /// 分页查询，pageNumber从1开始
    /// </summary>
    /// <param name="pageNumber">第几页，从1开始，小于1时当作1处理</param>
    /// <param name="pageSize">每页显示条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6> Page(int pageNumber, int pageSize);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，如：
    /// Select((a, b, ...) =&gt; new { a.Id, a.Name, ... }) 或是 Select((a, b, ...) =&gt; x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定字段返回，只需要指定需要特殊处理的成员赋值即可，其他成员将从现有表的字段中按名称匹配赋值，多个同名字段时如果未特殊指定赋值，默认选取第一个表中的字段赋值。如：
    /// <code> .SelectTo&lt;TDto&gt;() 或是 .SelectTo((a, b) =&gt; new TDto{ b.Id }) //使用第二个表的Id字段作为Id成员</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型，通常是一个匿名类</typeparam>
    /// <param name="specialMemberSelector">特殊成员赋值表达式，通常是重名字段或是不存在的字段赋值</param>
    /// <returns>返回查询对象</returns>
    IQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, TTarget>> specialMemberSelector = null);
    #endregion

    #region Count
    /// <summary>
    /// 返回某个字段的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6&gt;().Count((a, b, c, d, e, f) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的int类型数据条数</returns>
    int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6&gt;().CountAsync((a, b, c, d, e, f) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的int类型数据条数</returns>
    Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6&gt;().CountDistinct((a, b, c, d, e, f) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段去重后的int类型数据条数</returns>
    int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6&gt;().CountDistinctAsync((a, b, c, d, e, f) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段去重后的int类型数据条数</returns>
    Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6&gt;().LongCount((a, b, c, d, e, f) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的long类型数据条数</returns>
    long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6&gt;().LongCountAsync((a, b, c, d, e, f) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的long类型数据条数</returns>
    Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6&gt;().LongCountDistinct((a, b, c, d, e, f) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段去重后的long类型数据条数</returns>
    long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6&gt;().LongCountDistinctAsync((a, b, c, d, e, f) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段去重后的long类型数据条数</returns>
    Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr, CancellationToken cancellationToken = default);
    #endregion

    #region Aggregate
    /// <summary>
    /// 计算指定字段的求和值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6&gt;().Sum((a, b, c, d, e, f) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的求和值</returns>
    decimal Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的求和值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6&gt;().SumAsync((a, b, c, d, e, f) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的求和值</returns>
    Task<decimal> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的平均值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6&gt;().Avg((a, b, c, d, e, f) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的平均值</returns>
    TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的平均值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6&gt;().AvgAsync((a, b, c, d, e, f) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的平均值</returns>
    Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的最大值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6&gt;().Max((a, b, c, d, e, f) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的最大值</returns>
    TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最大值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6&gt;().MaxAsync((a, b, c, d, e, f) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的最大值</returns>
    Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的最小值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6&gt;().Min((a, b, c, d, e, f) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的最小值</returns>
    TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最小值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6&gt;().MinAsync((a, b, c, d, e, f) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的最小值</returns>
    Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr, CancellationToken cancellationToken = default);
    #endregion
}
/// <summary>
/// 多表T1, T2, T3, T4, T5, T6, T7查询
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
/// <typeparam name="T5">表T5实体类型</typeparam>
/// <typeparam name="T6">表T6实体类型</typeparam>
/// <typeparam name="T7">表T7实体类型</typeparam>
public interface IQuery<T1, T2, T3, T4, T5, T6, T7> : IQueryBase
{
    #region Sharding
    /// <summary>
    /// 直接指定T7表分表名，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7> UseTable(params string[] tableNames);
    /// <summary>
    /// 根据首个分表TMasterSharding表与当前T7表名的映射关系，指定当前T7表分表名获取委托，执行委托获取当前T7表分表名。委托第一个参数是TMasterSharding主表原始表名，第二个参数是当前T7表原始表名，第三个参数是TMasterSharding表当前分表名，返回值是当前T7表分表名称，如：
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
    /// <param name="tableNameGetter">T7表分表名获取委托</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter);
    /// <summary>
    /// 手动指定分表规则参数值，执行分表规则确定T7表分表名，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，最多支持3个字段值，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7> UseTableBy(params object[] fieldValues);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T7表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginFieldValue &lt;= endFieldValue，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="beginFieldValue">字段范围起始值</param>
    /// <param name="endFieldValue">字段范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7> UseTableByRange(object beginFieldValue, object endFieldValue);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T7表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginField2Value &lt;= endField2Value，常用于是日期规则分表，如：.UseTableByRange(1, DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="beginField2Value">字段2范围起始值</param>
    /// <param name="endField2Value">字段2范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7> UseTableByRange(object field1Value, object beginField2Value, object endField2Value);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T7表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginField3Value &lt;= endField3Value，常用于是日期规则分表，如：.UseTableByRange(1, "Game1", DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="field2Value">字段2值</param>
    /// <param name="beginField3Value">字段3范围起始值</param>
    /// <param name="endField3Value">字段3范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value);
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7> UseTableSchema(string tableSchema);
    #endregion

    #region WithTable
    /// <summary>
    /// 添加实体表，方便后面做JOIN关联，如：<code>.From&lt;Menu&gt;().WithTable&lt;Page&gt;()</code>
    /// </summary>
    /// <typeparam name="TOther">实体表类型</typeparam>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, TOther> WithTable<TOther>();
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, TOther> WithQuery<TOther>(IQuery<TOther> subQuery);
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr);
    #endregion

    #region Include
    /// <summary>
    /// 加载导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，1:1关联关系，随主表一起查询,支持无限级，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .From&lt;Product&gt;().Include(f =&gt; f.Brand) ...
    /// .From&lt;Brand&gt;().Include(f =&gt; f.Products) ...
    /// .From&lt;Order&gt;().Include(f =&gt; f.Seller.Company.Products) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TMember">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>> memberSelector);
    /// <summary>
    /// 加载集合类导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，可使用filter筛选满足条件的导航属性，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .From&lt;User&gt;().IncludeMany(f =&gt; f.Orders)  //与 .From&lt;User&gt;().Include(f =&gt; f.Orders) 等价
    /// .From&lt;User&gt;().IncludeMany(f =&gt; f.Orders, order =&gt; order.TotalAmout &gt; 500)
    /// </code>
    /// </summary>
    /// <typeparam name="TElement">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null);
    #endregion

    #region InnerJoin
    /// <summary>
    /// 在现有表中，选择两个表进行INNER JOIN关联，可多次关联，如：<code>.InnerJoin((a, b, ...) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做INNER JOIN关联，与.WithTable&lt;TOther&gt;().InnerJoin(...)等价，如：
    /// <code>
    /// .From&lt;User&gt;().InnerJoin&lt;Order&gt;((x, y) =&gt; x.Id == y.BuyerId)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr表达式构建子查询，并选择一个现有表做INNER JOIN关联，与.WithQuery(subQueryExpr).InnerJoin(...)等价，如：
    /// <code>
    /// .InnerJoin(f =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select(x =&gt; new { ... }), (a, b, ...) =&gt; x.xxx = y.yyy) ...
    /// SQL: ... INNER JOIN (SELECT ... FROM `sys_order_detail` ...) h ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn);
    #endregion

    #region LeftJoin
    /// <summary>
    /// 在现有表中，选择两个表进行LEFT JOIN关联，可多次关联，如：<code>.LeftJoin((a, b, ...) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做LEFT JOIN关联，与.WithTable&lt;TOther&gt;().LeftJoin(...)等价，如：
    /// <code>
    /// .LeftJoin&lt;TOther&gt;((a, b, ...) =&gt; a.xxx = b.yyy)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr表达式构建子查询，并选择一个现有表做LEFT JOIN关联，与.WithQuery(subQueryExpr).LeftJoin(...)等价，如：
    /// <code>
    /// .LeftJoin(f =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select(t =&gt; new { ... }), (a, b, ...) =&gt; a.Id = b.OrderId) ...
    /// SQL: ... LEFT JOIN (SELECT ... FROM `sys_order_detail` ...) h ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn);
    #endregion

    #region RightJoin
    /// <summary>
    /// 在现有表中，选择两个表进行RIGHT JOIN关联，可多次关联，如：<code>.RightJoin((a, b) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做RIGHT JOIN关联，与.WithTable&lt;TOther&gt;().RightJoin(...)等价，如：
    /// <code>
    /// .RightJoin&lt;TOther&gt;((a, b, c, d, e, f, g) =&gt; ...)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr表达式构建子查询，并选择一个现有表做RIGHT JOIN关联，与.WithQuery(subQueryExpr).RightJoin(...)等价，如：
    /// <code>
    /// .RightJoin(f =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select(t =&gt; new { ... }), (a, b, ...) =&gt; a.Id = b.OrderId) ...
    /// SQL: ... RIGHT JOIN (SELECT ... FROM `sys_order_detail` ...) h ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn);
    #endregion

    #region Where
    /// <summary>
    /// 条件查询，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> predicate);
    /// <summary>
    /// 条件查询，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>>> predicateInitializer);
    #endregion

    #region And
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，并与已有的条件AND操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>>> predicateInitializer);
    #endregion

    #region Or
    /// <summary>
    /// 使用predicate表达式生成Or条件，并添加到已有的Where条件末尾，表达式predicate不可为null
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件OR操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，并与已有的条件OR操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>>> predicateInitializer);
    #endregion

    #region GroupBy
    /// <summary>
    /// 分组查询，分组表达式groupingExpr可以是单个或多个字段的匿名对象，如：
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
    /// <param name="groupingExpr">分组表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TGrouping>> groupingExpr);
    #endregion

    #region OrderBy
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// OrderBy((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderBy((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr);
    /// <summary>
    /// ASC排序，condition为true，ASC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderBy(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr);
    /// <summary>
    /// DSC排序，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// OrderByDescending((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，condition为true，DESC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr);
    /// <summary>
    /// 动态排序，使用OrderByBuilder构建排序字段选择器，fieldsGetter可以是单个或多个字段的匿名对象，如：
    /// <code>string orderFields = "Name";bool isAsc = true;
    /// .OrderByDynamic(t =&gt; t.Switch(orderFields, isAsc).When("Name", (a, b, ...) =&lt; a.Name).When("Gender", (a, b, ...) =&lt; a.Gender).Build()</code>)
    /// </summary>
    /// <param name="fieldsGetter">排序字段选择器，需调用Build方法，返回排序字段表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6, T7>, Expression> fieldsGetter);
    #endregion

    #region Skip/Take/Page
    /// <summary>
    /// 跳过offset条数据
    /// </summary>
    /// <param name="offset">要跳过的数据条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7> Skip(int offset);
    /// <summary>
    /// 只返回limit条数据
    /// </summary>
    /// <param name="limit">返回的数据条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7> Take(int limit);
    /// <summary>
    /// 分页查询，pageNumber从1开始
    /// </summary>
    /// <param name="pageNumber">第几页，从1开始，小于1时当作1处理</param>
    /// <param name="pageSize">每页显示条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7> Page(int pageNumber, int pageSize);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，如：
    /// Select((a, b, ...) =&gt; new { a.Id, a.Name, ... }) 或是 Select((a, b, ...) =&gt; x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定字段返回，只需要指定需要特殊处理的成员赋值即可，其他成员将从现有表的字段中按名称匹配赋值，多个同名字段时如果未特殊指定赋值，默认选取第一个表中的字段赋值。如：
    /// <code> .SelectTo&lt;TDto&gt;() 或是 .SelectTo((a, b) =&gt; new TDto{ b.Id }) //使用第二个表的Id字段作为Id成员</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型，通常是一个匿名类</typeparam>
    /// <param name="specialMemberSelector">特殊成员赋值表达式，通常是重名字段或是不存在的字段赋值</param>
    /// <returns>返回查询对象</returns>
    IQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TTarget>> specialMemberSelector = null);
    #endregion

    #region Count
    /// <summary>
    /// 返回某个字段的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7&gt;().Count((a, b, c, d, e, f, g) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的int类型数据条数</returns>
    int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7&gt;().CountAsync((a, b, c, d, e, f, g) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的int类型数据条数</returns>
    Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7&gt;().CountDistinct((a, b, c, d, e, f, g) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段去重后的int类型数据条数</returns>
    int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7&gt;().CountDistinctAsync((a, b, c, d, e, f, g) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段去重后的int类型数据条数</returns>
    Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7&gt;().LongCount((a, b, c, d, e, f, g) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的long类型数据条数</returns>
    long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7&gt;().LongCountAsync((a, b, c, d, e, f, g) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的long类型数据条数</returns>
    Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7&gt;().LongCountDistinct((a, b, c, d, e, f, g) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段去重后的long类型数据条数</returns>
    long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7&gt;().LongCountDistinctAsync((a, b, c, d, e, f, g) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段去重后的long类型数据条数</returns>
    Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr, CancellationToken cancellationToken = default);
    #endregion

    #region Aggregate
    /// <summary>
    /// 计算指定字段的求和值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7&gt;().Sum((a, b, c, d, e, f, g) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的求和值</returns>
    decimal Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的求和值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7&gt;().SumAsync((a, b, c, d, e, f, g) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的求和值</returns>
    Task<decimal> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的平均值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7&gt;().Avg((a, b, c, d, e, f, g) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的平均值</returns>
    TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的平均值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7&gt;().AvgAsync((a, b, c, d, e, f, g) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的平均值</returns>
    Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的最大值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7&gt;().Max((a, b, c, d, e, f, g) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的最大值</returns>
    TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最大值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7&gt;().MaxAsync((a, b, c, d, e, f, g) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的最大值</returns>
    Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的最小值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7&gt;().Min((a, b, c, d, e, f, g) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的最小值</returns>
    TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最小值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7&gt;().MinAsync((a, b, c, d, e, f, g) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的最小值</returns>
    Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr, CancellationToken cancellationToken = default);
    #endregion
}
/// <summary>
/// 多表T1, T2, T3, T4, T5, T6, T7, T8查询
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
/// <typeparam name="T5">表T5实体类型</typeparam>
/// <typeparam name="T6">表T6实体类型</typeparam>
/// <typeparam name="T7">表T7实体类型</typeparam>
/// <typeparam name="T8">表T8实体类型</typeparam>
public interface IQuery<T1, T2, T3, T4, T5, T6, T7, T8> : IQueryBase
{
    #region Sharding
    /// <summary>
    /// 直接指定T8表分表名，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8> UseTable(params string[] tableNames);
    /// <summary>
    /// 根据首个分表TMasterSharding表与当前T8表名的映射关系，指定当前T8表分表名获取委托，执行委托获取当前T8表分表名。委托第一个参数是TMasterSharding主表原始表名，第二个参数是当前T8表原始表名，第三个参数是TMasterSharding表当前分表名，返回值是当前T8表分表名称，如：
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
    /// <param name="tableNameGetter">T8表分表名获取委托</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter);
    /// <summary>
    /// 手动指定分表规则参数值，执行分表规则确定T8表分表名，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，最多支持3个字段值，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8> UseTableBy(params object[] fieldValues);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T8表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginFieldValue &lt;= endFieldValue，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="beginFieldValue">字段范围起始值</param>
    /// <param name="endFieldValue">字段范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8> UseTableByRange(object beginFieldValue, object endFieldValue);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T8表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginField2Value &lt;= endField2Value，常用于是日期规则分表，如：.UseTableByRange(1, DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="beginField2Value">字段2范围起始值</param>
    /// <param name="endField2Value">字段2范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8> UseTableByRange(object field1Value, object beginField2Value, object endField2Value);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T8表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginField3Value &lt;= endField3Value，常用于是日期规则分表，如：.UseTableByRange(1, "Game1", DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="field2Value">字段2值</param>
    /// <param name="beginField3Value">字段3范围起始值</param>
    /// <param name="endField3Value">字段3范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value);
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8> UseTableSchema(string tableSchema);
    #endregion

    #region WithTable
    /// <summary>
    /// 添加实体表，方便后面做JOIN关联，如：<code>.From&lt;Menu&gt;().WithTable&lt;Page&gt;()</code>
    /// </summary>
    /// <typeparam name="TOther">实体表类型</typeparam>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> WithTable<TOther>();
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> WithQuery<TOther>(IQuery<TOther> subQuery);
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr);
    #endregion

    #region Include
    /// <summary>
    /// 加载导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，1:1关联关系，随主表一起查询,支持无限级，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .From&lt;Product&gt;().Include(f =&gt; f.Brand) ...
    /// .From&lt;Brand&gt;().Include(f =&gt; f.Products) ...
    /// .From&lt;Order&gt;().Include(f =&gt; f.Seller.Company.Products) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TMember">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TMember>> memberSelector);
    /// <summary>
    /// 加载集合类导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，可使用filter筛选满足条件的导航属性，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .From&lt;User&gt;().IncludeMany(f =&gt; f.Orders)  //与 .From&lt;User&gt;().Include(f =&gt; f.Orders) 等价
    /// .From&lt;User&gt;().IncludeMany(f =&gt; f.Orders, order =&gt; order.TotalAmout &gt; 500)
    /// </code>
    /// </summary>
    /// <typeparam name="TElement">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null);
    #endregion

    #region InnerJoin
    /// <summary>
    /// 在现有表中，选择两个表进行INNER JOIN关联，可多次关联，如：<code>.InnerJoin((a, b, ...) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做INNER JOIN关联，与.WithTable&lt;TOther&gt;().InnerJoin(...)等价，如：
    /// <code>
    /// .From&lt;User&gt;().InnerJoin&lt;Order&gt;((x, y) =&gt; x.Id == y.BuyerId)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr表达式构建子查询，并选择一个现有表做INNER JOIN关联，与.WithQuery(subQueryExpr).InnerJoin(...)等价，如：
    /// <code>
    /// .InnerJoin(f =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select(x =&gt; new { ... }), (a, b, ...) =&gt; x.xxx = y.yyy) ...
    /// SQL: ... INNER JOIN (SELECT ... FROM `sys_order_detail` ...) i ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn);
    #endregion

    #region LeftJoin
    /// <summary>
    /// 在现有表中，选择两个表进行LEFT JOIN关联，可多次关联，如：<code>.LeftJoin((a, b, ...) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做LEFT JOIN关联，与.WithTable&lt;TOther&gt;().LeftJoin(...)等价，如：
    /// <code>
    /// .LeftJoin&lt;TOther&gt;((a, b, ...) =&gt; a.xxx = b.yyy)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr表达式构建子查询，并选择一个现有表做LEFT JOIN关联，与.WithQuery(subQueryExpr).LeftJoin(...)等价，如：
    /// <code>
    /// .LeftJoin(f =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select(t =&gt; new { ... }), (a, b, ...) =&gt; a.Id = b.OrderId) ...
    /// SQL: ... LEFT JOIN (SELECT ... FROM `sys_order_detail` ...) i ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn);
    #endregion

    #region RightJoin
    /// <summary>
    /// 在现有表中，选择两个表进行RIGHT JOIN关联，可多次关联，如：<code>.RightJoin((a, b) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做RIGHT JOIN关联，与.WithTable&lt;TOther&gt;().RightJoin(...)等价，如：
    /// <code>
    /// .RightJoin&lt;TOther&gt;((a, b, c, d, e, f, g, h) =&gt; ...)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr表达式构建子查询，并选择一个现有表做RIGHT JOIN关联，与.WithQuery(subQueryExpr).RightJoin(...)等价，如：
    /// <code>
    /// .RightJoin(f =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select(t =&gt; new { ... }), (a, b, ...) =&gt; a.Id = b.OrderId) ...
    /// SQL: ... RIGHT JOIN (SELECT ... FROM `sys_order_detail` ...) i ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn);
    #endregion

    #region Where
    /// <summary>
    /// 条件查询，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate);
    /// <summary>
    /// 条件查询，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>>> predicateInitializer);
    #endregion

    #region And
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，并与已有的条件AND操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>>> predicateInitializer);
    #endregion

    #region Or
    /// <summary>
    /// 使用predicate表达式生成Or条件，并添加到已有的Where条件末尾，表达式predicate不可为null
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件OR操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，并与已有的条件OR操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>>> predicateInitializer);
    #endregion

    #region GroupBy
    /// <summary>
    /// 分组查询，分组表达式groupingExpr可以是单个或多个字段的匿名对象，如：
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
    /// <param name="groupingExpr">分组表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping>> groupingExpr);
    #endregion

    #region OrderBy
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// OrderBy((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderBy((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr);
    /// <summary>
    /// ASC排序，condition为true，ASC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderBy(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr);
    /// <summary>
    /// DSC排序，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// OrderByDescending((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，condition为true，DESC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr);
    /// <summary>
    /// 动态排序，使用OrderByBuilder构建排序字段选择器，fieldsGetter可以是单个或多个字段的匿名对象，如：
    /// <code>string orderFields = "Name";bool isAsc = true;
    /// .OrderByDynamic(t =&gt; t.Switch(orderFields, isAsc).When("Name", (a, b, ...) =&lt; a.Name).When("Gender", (a, b, ...) =&lt; a.Gender).Build()</code>)
    /// </summary>
    /// <param name="fieldsGetter">排序字段选择器，需调用Build方法，返回排序字段表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8>, Expression> fieldsGetter);
    #endregion

    #region Skip/Take/Page
    /// <summary>
    /// 跳过offset条数据
    /// </summary>
    /// <param name="offset">要跳过的数据条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8> Skip(int offset);
    /// <summary>
    /// 只返回limit条数据
    /// </summary>
    /// <param name="limit">返回的数据条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8> Take(int limit);
    /// <summary>
    /// 分页查询，pageNumber从1开始
    /// </summary>
    /// <param name="pageNumber">第几页，从1开始，小于1时当作1处理</param>
    /// <param name="pageSize">每页显示条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8> Page(int pageNumber, int pageSize);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，如：
    /// Select((a, b, ...) =&gt; new { a.Id, a.Name, ... }) 或是 Select((a, b, ...) =&gt; x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定字段返回，只需要指定需要特殊处理的成员赋值即可，其他成员将从现有表的字段中按名称匹配赋值，多个同名字段时如果未特殊指定赋值，默认选取第一个表中的字段赋值。如：
    /// <code> .SelectTo&lt;TDto&gt;() 或是 .SelectTo((a, b) =&gt; new TDto{ b.Id }) //使用第二个表的Id字段作为Id成员</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型，通常是一个匿名类</typeparam>
    /// <param name="specialMemberSelector">特殊成员赋值表达式，通常是重名字段或是不存在的字段赋值</param>
    /// <returns>返回查询对象</returns>
    IQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TTarget>> specialMemberSelector = null);
    #endregion

    #region Count
    /// <summary>
    /// 返回某个字段的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8&gt;().Count((a, b, c, d, e, f, g, h) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的int类型数据条数</returns>
    int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8&gt;().CountAsync((a, b, c, d, e, f, g, h) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的int类型数据条数</returns>
    Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8&gt;().CountDistinct((a, b, c, d, e, f, g, h) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段去重后的int类型数据条数</returns>
    int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8&gt;().CountDistinctAsync((a, b, c, d, e, f, g, h) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段去重后的int类型数据条数</returns>
    Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8&gt;().LongCount((a, b, c, d, e, f, g, h) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的long类型数据条数</returns>
    long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8&gt;().LongCountAsync((a, b, c, d, e, f, g, h) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的long类型数据条数</returns>
    Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8&gt;().LongCountDistinct((a, b, c, d, e, f, g, h) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段去重后的long类型数据条数</returns>
    long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8&gt;().LongCountDistinctAsync((a, b, c, d, e, f, g, h) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段去重后的long类型数据条数</returns>
    Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr, CancellationToken cancellationToken = default);
    #endregion

    #region Aggregate
    /// <summary>
    /// 计算指定字段的求和值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8&gt;().Sum((a, b, c, d, e, f, g, h) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的求和值</returns>
    decimal Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的求和值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8&gt;().SumAsync((a, b, c, d, e, f, g, h) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的求和值</returns>
    Task<decimal> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的平均值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8&gt;().Avg((a, b, c, d, e, f, g, h) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的平均值</returns>
    TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的平均值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8&gt;().AvgAsync((a, b, c, d, e, f, g, h) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的平均值</returns>
    Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的最大值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8&gt;().Max((a, b, c, d, e, f, g, h) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的最大值</returns>
    TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最大值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8&gt;().MaxAsync((a, b, c, d, e, f, g, h) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的最大值</returns>
    Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的最小值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8&gt;().Min((a, b, c, d, e, f, g, h) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的最小值</returns>
    TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最小值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8&gt;().MinAsync((a, b, c, d, e, f, g, h) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的最小值</returns>
    Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr, CancellationToken cancellationToken = default);
    #endregion
}
/// <summary>
/// 多表T1, T2, T3, T4, T5, T6, T7, T8, T9查询
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
/// <typeparam name="T5">表T5实体类型</typeparam>
/// <typeparam name="T6">表T6实体类型</typeparam>
/// <typeparam name="T7">表T7实体类型</typeparam>
/// <typeparam name="T8">表T8实体类型</typeparam>
/// <typeparam name="T9">表T9实体类型</typeparam>
public interface IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> : IQueryBase
{
    #region Sharding
    /// <summary>
    /// 直接指定T9表分表名，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> UseTable(params string[] tableNames);
    /// <summary>
    /// 根据首个分表TMasterSharding表与当前T9表名的映射关系，指定当前T9表分表名获取委托，执行委托获取当前T9表分表名。委托第一个参数是TMasterSharding主表原始表名，第二个参数是当前T9表原始表名，第三个参数是TMasterSharding表当前分表名，返回值是当前T9表分表名称，如：
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
    /// <param name="tableNameGetter">T9表分表名获取委托</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter);
    /// <summary>
    /// 手动指定分表规则参数值，执行分表规则确定T9表分表名，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，最多支持3个字段值，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> UseTableBy(params object[] fieldValues);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T9表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginFieldValue &lt;= endFieldValue，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="beginFieldValue">字段范围起始值</param>
    /// <param name="endFieldValue">字段范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> UseTableByRange(object beginFieldValue, object endFieldValue);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T9表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginField2Value &lt;= endField2Value，常用于是日期规则分表，如：.UseTableByRange(1, DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="beginField2Value">字段2范围起始值</param>
    /// <param name="endField2Value">字段2范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> UseTableByRange(object field1Value, object beginField2Value, object endField2Value);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T9表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginField3Value &lt;= endField3Value，常用于是日期规则分表，如：.UseTableByRange(1, "Game1", DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="field2Value">字段2值</param>
    /// <param name="beginField3Value">字段3范围起始值</param>
    /// <param name="endField3Value">字段3范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value);
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> UseTableSchema(string tableSchema);
    #endregion

    #region WithTable
    /// <summary>
    /// 添加实体表，方便后面做JOIN关联，如：<code>.From&lt;Menu&gt;().WithTable&lt;Page&gt;()</code>
    /// </summary>
    /// <typeparam name="TOther">实体表类型</typeparam>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> WithTable<TOther>();
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> WithQuery<TOther>(IQuery<TOther> subQuery);
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr);
    #endregion

    #region Include
    /// <summary>
    /// 加载导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，1:1关联关系，随主表一起查询,支持无限级，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .From&lt;Product&gt;().Include(f =&gt; f.Brand) ...
    /// .From&lt;Brand&gt;().Include(f =&gt; f.Products) ...
    /// .From&lt;Order&gt;().Include(f =&gt; f.Seller.Company.Products) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TMember">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>> memberSelector);
    /// <summary>
    /// 加载集合类导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，可使用filter筛选满足条件的导航属性，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .From&lt;User&gt;().IncludeMany(f =&gt; f.Orders)  //与 .From&lt;User&gt;().Include(f =&gt; f.Orders) 等价
    /// .From&lt;User&gt;().IncludeMany(f =&gt; f.Orders, order =&gt; order.TotalAmout &gt; 500)
    /// </code>
    /// </summary>
    /// <typeparam name="TElement">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null);
    #endregion

    #region InnerJoin
    /// <summary>
    /// 在现有表中，选择两个表进行INNER JOIN关联，可多次关联，如：<code>.InnerJoin((a, b, ...) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做INNER JOIN关联，与.WithTable&lt;TOther&gt;().InnerJoin(...)等价，如：
    /// <code>
    /// .From&lt;User&gt;().InnerJoin&lt;Order&gt;((x, y) =&gt; x.Id == y.BuyerId)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr表达式构建子查询，并选择一个现有表做INNER JOIN关联，与.WithQuery(subQueryExpr).InnerJoin(...)等价，如：
    /// <code>
    /// .InnerJoin(f =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select(x =&gt; new { ... }), (a, b, ...) =&gt; x.xxx = y.yyy) ...
    /// SQL: ... INNER JOIN (SELECT ... FROM `sys_order_detail` ...) j ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn);
    #endregion

    #region LeftJoin
    /// <summary>
    /// 在现有表中，选择两个表进行LEFT JOIN关联，可多次关联，如：<code>.LeftJoin((a, b, ...) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做LEFT JOIN关联，与.WithTable&lt;TOther&gt;().LeftJoin(...)等价，如：
    /// <code>
    /// .LeftJoin&lt;TOther&gt;((a, b, ...) =&gt; a.xxx = b.yyy)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr表达式构建子查询，并选择一个现有表做LEFT JOIN关联，与.WithQuery(subQueryExpr).LeftJoin(...)等价，如：
    /// <code>
    /// .LeftJoin(f =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select(t =&gt; new { ... }), (a, b, ...) =&gt; a.Id = b.OrderId) ...
    /// SQL: ... LEFT JOIN (SELECT ... FROM `sys_order_detail` ...) j ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn);
    #endregion

    #region RightJoin
    /// <summary>
    /// 在现有表中，选择两个表进行RIGHT JOIN关联，可多次关联，如：<code>.RightJoin((a, b) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做RIGHT JOIN关联，与.WithTable&lt;TOther&gt;().RightJoin(...)等价，如：
    /// <code>
    /// .RightJoin&lt;TOther&gt;((a, b, c, d, e, f, g, h, i) =&gt; ...)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr表达式构建子查询，并选择一个现有表做RIGHT JOIN关联，与.WithQuery(subQueryExpr).RightJoin(...)等价，如：
    /// <code>
    /// .RightJoin(f =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select(t =&gt; new { ... }), (a, b, ...) =&gt; a.Id = b.OrderId) ...
    /// SQL: ... RIGHT JOIN (SELECT ... FROM `sys_order_detail` ...) j ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn);
    #endregion

    #region Where
    /// <summary>
    /// 条件查询，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate);
    /// <summary>
    /// 条件查询，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>>> predicateInitializer);
    #endregion

    #region And
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，并与已有的条件AND操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>>> predicateInitializer);
    #endregion

    #region Or
    /// <summary>
    /// 使用predicate表达式生成Or条件，并添加到已有的Where条件末尾，表达式predicate不可为null
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件OR操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，并与已有的条件OR操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>>> predicateInitializer);
    #endregion

    #region GroupBy
    /// <summary>
    /// 分组查询，分组表达式groupingExpr可以是单个或多个字段的匿名对象，如：
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
    /// <param name="groupingExpr">分组表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping>> groupingExpr);
    #endregion

    #region OrderBy
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// OrderBy((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderBy((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr);
    /// <summary>
    /// ASC排序，condition为true，ASC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderBy(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr);
    /// <summary>
    /// DSC排序，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// OrderByDescending((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，condition为true，DESC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr);
    /// <summary>
    /// 动态排序，使用OrderByBuilder构建排序字段选择器，fieldsGetter可以是单个或多个字段的匿名对象，如：
    /// <code>string orderFields = "Name";bool isAsc = true;
    /// .OrderByDynamic(t =&gt; t.Switch(orderFields, isAsc).When("Name", (a, b, ...) =&lt; a.Name).When("Gender", (a, b, ...) =&lt; a.Gender).Build()</code>)
    /// </summary>
    /// <param name="fieldsGetter">排序字段选择器，需调用Build方法，返回排序字段表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9>, Expression> fieldsGetter);
    #endregion

    #region Skip/Take/Page
    /// <summary>
    /// 跳过offset条数据
    /// </summary>
    /// <param name="offset">要跳过的数据条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Skip(int offset);
    /// <summary>
    /// 只返回limit条数据
    /// </summary>
    /// <param name="limit">返回的数据条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Take(int limit);
    /// <summary>
    /// 分页查询，pageNumber从1开始
    /// </summary>
    /// <param name="pageNumber">第几页，从1开始，小于1时当作1处理</param>
    /// <param name="pageSize">每页显示条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Page(int pageNumber, int pageSize);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，如：
    /// Select((a, b, ...) =&gt; new { a.Id, a.Name, ... }) 或是 Select((a, b, ...) =&gt; x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定字段返回，只需要指定需要特殊处理的成员赋值即可，其他成员将从现有表的字段中按名称匹配赋值，多个同名字段时如果未特殊指定赋值，默认选取第一个表中的字段赋值。如：
    /// <code> .SelectTo&lt;TDto&gt;() 或是 .SelectTo((a, b) =&gt; new TDto{ b.Id }) //使用第二个表的Id字段作为Id成员</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型，通常是一个匿名类</typeparam>
    /// <param name="specialMemberSelector">特殊成员赋值表达式，通常是重名字段或是不存在的字段赋值</param>
    /// <returns>返回查询对象</returns>
    IQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget>> specialMemberSelector = null);
    #endregion

    #region Count
    /// <summary>
    /// 返回某个字段的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9&gt;().Count((a, b, c, d, e, f, g, h, i) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的int类型数据条数</returns>
    int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9&gt;().CountAsync((a, b, c, d, e, f, g, h, i) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的int类型数据条数</returns>
    Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9&gt;().CountDistinct((a, b, c, d, e, f, g, h, i) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段去重后的int类型数据条数</returns>
    int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9&gt;().CountDistinctAsync((a, b, c, d, e, f, g, h, i) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段去重后的int类型数据条数</returns>
    Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9&gt;().LongCount((a, b, c, d, e, f, g, h, i) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的long类型数据条数</returns>
    long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9&gt;().LongCountAsync((a, b, c, d, e, f, g, h, i) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的long类型数据条数</returns>
    Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9&gt;().LongCountDistinct((a, b, c, d, e, f, g, h, i) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段去重后的long类型数据条数</returns>
    long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9&gt;().LongCountDistinctAsync((a, b, c, d, e, f, g, h, i) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段去重后的long类型数据条数</returns>
    Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr, CancellationToken cancellationToken = default);
    #endregion

    #region Aggregate
    /// <summary>
    /// 计算指定字段的求和值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9&gt;().Sum((a, b, c, d, e, f, g, h, i) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的求和值</returns>
    decimal Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的求和值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9&gt;().SumAsync((a, b, c, d, e, f, g, h, i) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的求和值</returns>
    Task<decimal> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的平均值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9&gt;().Avg((a, b, c, d, e, f, g, h, i) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的平均值</returns>
    TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的平均值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9&gt;().AvgAsync((a, b, c, d, e, f, g, h, i) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的平均值</returns>
    Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的最大值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9&gt;().Max((a, b, c, d, e, f, g, h, i) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的最大值</returns>
    TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最大值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9&gt;().MaxAsync((a, b, c, d, e, f, g, h, i) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的最大值</returns>
    Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的最小值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9&gt;().Min((a, b, c, d, e, f, g, h, i) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的最小值</returns>
    TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最小值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9&gt;().MinAsync((a, b, c, d, e, f, g, h, i) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的最小值</returns>
    Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr, CancellationToken cancellationToken = default);
    #endregion
}
/// <summary>
/// 多表T1, T2, T3, T4, T5, T6, T7, T8, T9, T10查询
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
/// <typeparam name="T5">表T5实体类型</typeparam>
/// <typeparam name="T6">表T6实体类型</typeparam>
/// <typeparam name="T7">表T7实体类型</typeparam>
/// <typeparam name="T8">表T8实体类型</typeparam>
/// <typeparam name="T9">表T9实体类型</typeparam>
/// <typeparam name="T10">表T10实体类型</typeparam>
public interface IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : IQueryBase
{
    #region Sharding
    /// <summary>
    /// 直接指定T10表分表名，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> UseTable(params string[] tableNames);
    /// <summary>
    /// 根据首个分表TMasterSharding表与当前T10表名的映射关系，指定当前T10表分表名获取委托，执行委托获取当前T10表分表名。委托第一个参数是TMasterSharding主表原始表名，第二个参数是当前T10表原始表名，第三个参数是TMasterSharding表当前分表名，返回值是当前T10表分表名称，如：
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
    /// <param name="tableNameGetter">T10表分表名获取委托</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter);
    /// <summary>
    /// 手动指定分表规则参数值，执行分表规则确定T10表分表名，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，最多支持3个字段值，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> UseTableBy(params object[] fieldValues);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T10表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginFieldValue &lt;= endFieldValue，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="beginFieldValue">字段范围起始值</param>
    /// <param name="endFieldValue">字段范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> UseTableByRange(object beginFieldValue, object endFieldValue);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T10表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginField2Value &lt;= endField2Value，常用于是日期规则分表，如：.UseTableByRange(1, DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="beginField2Value">字段2范围起始值</param>
    /// <param name="endField2Value">字段2范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> UseTableByRange(object field1Value, object beginField2Value, object endField2Value);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T10表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginField3Value &lt;= endField3Value，常用于是日期规则分表，如：.UseTableByRange(1, "Game1", DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="field2Value">字段2值</param>
    /// <param name="beginField3Value">字段3范围起始值</param>
    /// <param name="endField3Value">字段3范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value);
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> UseTableSchema(string tableSchema);
    #endregion

    #region WithTable
    /// <summary>
    /// 添加实体表，方便后面做JOIN关联，如：<code>.From&lt;Menu&gt;().WithTable&lt;Page&gt;()</code>
    /// </summary>
    /// <typeparam name="TOther">实体表类型</typeparam>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> WithTable<TOther>();
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> WithQuery<TOther>(IQuery<TOther> subQuery);
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr);
    #endregion

    #region Include
    /// <summary>
    /// 加载导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，1:1关联关系，随主表一起查询,支持无限级，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .From&lt;Product&gt;().Include(f =&gt; f.Brand) ...
    /// .From&lt;Brand&gt;().Include(f =&gt; f.Products) ...
    /// .From&lt;Order&gt;().Include(f =&gt; f.Seller.Company.Products) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TMember">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>> memberSelector);
    /// <summary>
    /// 加载集合类导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，可使用filter筛选满足条件的导航属性，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .From&lt;User&gt;().IncludeMany(f =&gt; f.Orders)  //与 .From&lt;User&gt;().Include(f =&gt; f.Orders) 等价
    /// .From&lt;User&gt;().IncludeMany(f =&gt; f.Orders, order =&gt; order.TotalAmout &gt; 500)
    /// </code>
    /// </summary>
    /// <typeparam name="TElement">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null);
    #endregion

    #region InnerJoin
    /// <summary>
    /// 在现有表中，选择两个表进行INNER JOIN关联，可多次关联，如：<code>.InnerJoin((a, b, ...) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做INNER JOIN关联，与.WithTable&lt;TOther&gt;().InnerJoin(...)等价，如：
    /// <code>
    /// .From&lt;User&gt;().InnerJoin&lt;Order&gt;((x, y) =&gt; x.Id == y.BuyerId)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr表达式构建子查询，并选择一个现有表做INNER JOIN关联，与.WithQuery(subQueryExpr).InnerJoin(...)等价，如：
    /// <code>
    /// .InnerJoin(f =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select(x =&gt; new { ... }), (a, b, ...) =&gt; x.xxx = y.yyy) ...
    /// SQL: ... INNER JOIN (SELECT ... FROM `sys_order_detail` ...) k ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn);
    #endregion

    #region LeftJoin
    /// <summary>
    /// 在现有表中，选择两个表进行LEFT JOIN关联，可多次关联，如：<code>.LeftJoin((a, b, ...) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做LEFT JOIN关联，与.WithTable&lt;TOther&gt;().LeftJoin(...)等价，如：
    /// <code>
    /// .LeftJoin&lt;TOther&gt;((a, b, ...) =&gt; a.xxx = b.yyy)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr表达式构建子查询，并选择一个现有表做LEFT JOIN关联，与.WithQuery(subQueryExpr).LeftJoin(...)等价，如：
    /// <code>
    /// .LeftJoin(f =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select(t =&gt; new { ... }), (a, b, ...) =&gt; a.Id = b.OrderId) ...
    /// SQL: ... LEFT JOIN (SELECT ... FROM `sys_order_detail` ...) k ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn);
    #endregion

    #region RightJoin
    /// <summary>
    /// 在现有表中，选择两个表进行RIGHT JOIN关联，可多次关联，如：<code>.RightJoin((a, b) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做RIGHT JOIN关联，与.WithTable&lt;TOther&gt;().RightJoin(...)等价，如：
    /// <code>
    /// .RightJoin&lt;TOther&gt;((a, b, c, d, e, f, g, h, i, j) =&gt; ...)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr表达式构建子查询，并选择一个现有表做RIGHT JOIN关联，与.WithQuery(subQueryExpr).RightJoin(...)等价，如：
    /// <code>
    /// .RightJoin(f =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select(t =&gt; new { ... }), (a, b, ...) =&gt; a.Id = b.OrderId) ...
    /// SQL: ... RIGHT JOIN (SELECT ... FROM `sys_order_detail` ...) k ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn);
    #endregion

    #region Where
    /// <summary>
    /// 条件查询，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate);
    /// <summary>
    /// 条件查询，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>>> predicateInitializer);
    #endregion

    #region And
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，并与已有的条件AND操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>>> predicateInitializer);
    #endregion

    #region Or
    /// <summary>
    /// 使用predicate表达式生成Or条件，并添加到已有的Where条件末尾，表达式predicate不可为null
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件OR操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，并与已有的条件OR操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>>> predicateInitializer);
    #endregion

    #region GroupBy
    /// <summary>
    /// 分组查询，分组表达式groupingExpr可以是单个或多个字段的匿名对象，如：
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
    /// <param name="groupingExpr">分组表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping>> groupingExpr);
    #endregion

    #region OrderBy
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// OrderBy((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderBy((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr);
    /// <summary>
    /// ASC排序，condition为true，ASC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderBy(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr);
    /// <summary>
    /// DSC排序，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// OrderByDescending((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，condition为true，DESC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr);
    /// <summary>
    /// 动态排序，使用OrderByBuilder构建排序字段选择器，fieldsGetter可以是单个或多个字段的匿名对象，如：
    /// <code>string orderFields = "Name";bool isAsc = true;
    /// .OrderByDynamic(t =&gt; t.Switch(orderFields, isAsc).When("Name", (a, b, ...) =&lt; a.Name).When("Gender", (a, b, ...) =&lt; a.Gender).Build()</code>)
    /// </summary>
    /// <param name="fieldsGetter">排序字段选择器，需调用Build方法，返回排序字段表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>, Expression> fieldsGetter);
    #endregion

    #region Skip/Take/Page
    /// <summary>
    /// 跳过offset条数据
    /// </summary>
    /// <param name="offset">要跳过的数据条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Skip(int offset);
    /// <summary>
    /// 只返回limit条数据
    /// </summary>
    /// <param name="limit">返回的数据条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Take(int limit);
    /// <summary>
    /// 分页查询，pageNumber从1开始
    /// </summary>
    /// <param name="pageNumber">第几页，从1开始，小于1时当作1处理</param>
    /// <param name="pageSize">每页显示条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Page(int pageNumber, int pageSize);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，如：
    /// Select((a, b, ...) =&gt; new { a.Id, a.Name, ... }) 或是 Select((a, b, ...) =&gt; x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定字段返回，只需要指定需要特殊处理的成员赋值即可，其他成员将从现有表的字段中按名称匹配赋值，多个同名字段时如果未特殊指定赋值，默认选取第一个表中的字段赋值。如：
    /// <code> .SelectTo&lt;TDto&gt;() 或是 .SelectTo((a, b) =&gt; new TDto{ b.Id }) //使用第二个表的Id字段作为Id成员</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型，通常是一个匿名类</typeparam>
    /// <param name="specialMemberSelector">特殊成员赋值表达式，通常是重名字段或是不存在的字段赋值</param>
    /// <returns>返回查询对象</returns>
    IQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget>> specialMemberSelector = null);
    #endregion

    #region Count
    /// <summary>
    /// 返回某个字段的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10&gt;().Count((a, b, c, d, e, f, g, h, i, j) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的int类型数据条数</returns>
    int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10&gt;().CountAsync((a, b, c, d, e, f, g, h, i, j) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的int类型数据条数</returns>
    Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10&gt;().CountDistinct((a, b, c, d, e, f, g, h, i, j) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段去重后的int类型数据条数</returns>
    int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10&gt;().CountDistinctAsync((a, b, c, d, e, f, g, h, i, j) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段去重后的int类型数据条数</returns>
    Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10&gt;().LongCount((a, b, c, d, e, f, g, h, i, j) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的long类型数据条数</returns>
    long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10&gt;().LongCountAsync((a, b, c, d, e, f, g, h, i, j) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的long类型数据条数</returns>
    Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10&gt;().LongCountDistinct((a, b, c, d, e, f, g, h, i, j) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段去重后的long类型数据条数</returns>
    long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10&gt;().LongCountDistinctAsync((a, b, c, d, e, f, g, h, i, j) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段去重后的long类型数据条数</returns>
    Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr, CancellationToken cancellationToken = default);
    #endregion

    #region Aggregate
    /// <summary>
    /// 计算指定字段的求和值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10&gt;().Sum((a, b, c, d, e, f, g, h, i, j) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的求和值</returns>
    decimal Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的求和值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10&gt;().SumAsync((a, b, c, d, e, f, g, h, i, j) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的求和值</returns>
    Task<decimal> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的平均值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10&gt;().Avg((a, b, c, d, e, f, g, h, i, j) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的平均值</returns>
    TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的平均值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10&gt;().AvgAsync((a, b, c, d, e, f, g, h, i, j) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的平均值</returns>
    Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的最大值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10&gt;().Max((a, b, c, d, e, f, g, h, i, j) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的最大值</returns>
    TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最大值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10&gt;().MaxAsync((a, b, c, d, e, f, g, h, i, j) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的最大值</returns>
    Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的最小值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10&gt;().Min((a, b, c, d, e, f, g, h, i, j) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的最小值</returns>
    TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最小值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10&gt;().MinAsync((a, b, c, d, e, f, g, h, i, j) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的最小值</returns>
    Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr, CancellationToken cancellationToken = default);
    #endregion
}
/// <summary>
/// 多表T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11查询
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
/// <typeparam name="T5">表T5实体类型</typeparam>
/// <typeparam name="T6">表T6实体类型</typeparam>
/// <typeparam name="T7">表T7实体类型</typeparam>
/// <typeparam name="T8">表T8实体类型</typeparam>
/// <typeparam name="T9">表T9实体类型</typeparam>
/// <typeparam name="T10">表T10实体类型</typeparam>
/// <typeparam name="T11">表T11实体类型</typeparam>
public interface IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : IQueryBase
{
    #region Sharding
    /// <summary>
    /// 直接指定T11表分表名，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> UseTable(params string[] tableNames);
    /// <summary>
    /// 根据首个分表TMasterSharding表与当前T11表名的映射关系，指定当前T11表分表名获取委托，执行委托获取当前T11表分表名。委托第一个参数是TMasterSharding主表原始表名，第二个参数是当前T11表原始表名，第三个参数是TMasterSharding表当前分表名，返回值是当前T11表分表名称，如：
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
    /// <param name="tableNameGetter">T11表分表名获取委托</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter);
    /// <summary>
    /// 手动指定分表规则参数值，执行分表规则确定T11表分表名，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，最多支持3个字段值，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> UseTableBy(params object[] fieldValues);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T11表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginFieldValue &lt;= endFieldValue，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="beginFieldValue">字段范围起始值</param>
    /// <param name="endFieldValue">字段范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> UseTableByRange(object beginFieldValue, object endFieldValue);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T11表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginField2Value &lt;= endField2Value，常用于是日期规则分表，如：.UseTableByRange(1, DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="beginField2Value">字段2范围起始值</param>
    /// <param name="endField2Value">字段2范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> UseTableByRange(object field1Value, object beginField2Value, object endField2Value);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T11表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginField3Value &lt;= endField3Value，常用于是日期规则分表，如：.UseTableByRange(1, "Game1", DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="field2Value">字段2值</param>
    /// <param name="beginField3Value">字段3范围起始值</param>
    /// <param name="endField3Value">字段3范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value);
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> UseTableSchema(string tableSchema);
    #endregion

    #region WithTable
    /// <summary>
    /// 添加实体表，方便后面做JOIN关联，如：<code>.From&lt;Menu&gt;().WithTable&lt;Page&gt;()</code>
    /// </summary>
    /// <typeparam name="TOther">实体表类型</typeparam>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> WithTable<TOther>();
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> WithQuery<TOther>(IQuery<TOther> subQuery);
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr);
    #endregion

    #region Include
    /// <summary>
    /// 加载导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，1:1关联关系，随主表一起查询,支持无限级，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .From&lt;Product&gt;().Include(f =&gt; f.Brand) ...
    /// .From&lt;Brand&gt;().Include(f =&gt; f.Products) ...
    /// .From&lt;Order&gt;().Include(f =&gt; f.Seller.Company.Products) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TMember">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>> memberSelector);
    /// <summary>
    /// 加载集合类导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，可使用filter筛选满足条件的导航属性，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .From&lt;User&gt;().IncludeMany(f =&gt; f.Orders)  //与 .From&lt;User&gt;().Include(f =&gt; f.Orders) 等价
    /// .From&lt;User&gt;().IncludeMany(f =&gt; f.Orders, order =&gt; order.TotalAmout &gt; 500)
    /// </code>
    /// </summary>
    /// <typeparam name="TElement">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null);
    #endregion

    #region InnerJoin
    /// <summary>
    /// 在现有表中，选择两个表进行INNER JOIN关联，可多次关联，如：<code>.InnerJoin((a, b, ...) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做INNER JOIN关联，与.WithTable&lt;TOther&gt;().InnerJoin(...)等价，如：
    /// <code>
    /// .From&lt;User&gt;().InnerJoin&lt;Order&gt;((x, y) =&gt; x.Id == y.BuyerId)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr表达式构建子查询，并选择一个现有表做INNER JOIN关联，与.WithQuery(subQueryExpr).InnerJoin(...)等价，如：
    /// <code>
    /// .InnerJoin(f =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select(x =&gt; new { ... }), (a, b, ...) =&gt; x.xxx = y.yyy) ...
    /// SQL: ... INNER JOIN (SELECT ... FROM `sys_order_detail` ...) l ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn);
    #endregion

    #region LeftJoin
    /// <summary>
    /// 在现有表中，选择两个表进行LEFT JOIN关联，可多次关联，如：<code>.LeftJoin((a, b, ...) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做LEFT JOIN关联，与.WithTable&lt;TOther&gt;().LeftJoin(...)等价，如：
    /// <code>
    /// .LeftJoin&lt;TOther&gt;((a, b, ...) =&gt; a.xxx = b.yyy)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr表达式构建子查询，并选择一个现有表做LEFT JOIN关联，与.WithQuery(subQueryExpr).LeftJoin(...)等价，如：
    /// <code>
    /// .LeftJoin(f =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select(t =&gt; new { ... }), (a, b, ...) =&gt; a.Id = b.OrderId) ...
    /// SQL: ... LEFT JOIN (SELECT ... FROM `sys_order_detail` ...) l ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn);
    #endregion

    #region RightJoin
    /// <summary>
    /// 在现有表中，选择两个表进行RIGHT JOIN关联，可多次关联，如：<code>.RightJoin((a, b) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做RIGHT JOIN关联，与.WithTable&lt;TOther&gt;().RightJoin(...)等价，如：
    /// <code>
    /// .RightJoin&lt;TOther&gt;((a, b, c, d, e, f, g, h, i, j, k) =&gt; ...)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr表达式构建子查询，并选择一个现有表做RIGHT JOIN关联，与.WithQuery(subQueryExpr).RightJoin(...)等价，如：
    /// <code>
    /// .RightJoin(f =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select(t =&gt; new { ... }), (a, b, ...) =&gt; a.Id = b.OrderId) ...
    /// SQL: ... RIGHT JOIN (SELECT ... FROM `sys_order_detail` ...) l ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn);
    #endregion

    #region Where
    /// <summary>
    /// 条件查询，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate);
    /// <summary>
    /// 条件查询，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>>> predicateInitializer);
    #endregion

    #region And
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，并与已有的条件AND操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>>> predicateInitializer);
    #endregion

    #region Or
    /// <summary>
    /// 使用predicate表达式生成Or条件，并添加到已有的Where条件末尾，表达式predicate不可为null
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件OR操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，并与已有的条件OR操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>>> predicateInitializer);
    #endregion

    #region GroupBy
    /// <summary>
    /// 分组查询，分组表达式groupingExpr可以是单个或多个字段的匿名对象，如：
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
    /// <param name="groupingExpr">分组表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping>> groupingExpr);
    #endregion

    #region OrderBy
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// OrderBy((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderBy((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr);
    /// <summary>
    /// ASC排序，condition为true，ASC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderBy(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr);
    /// <summary>
    /// DSC排序，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// OrderByDescending((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，condition为true，DESC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr);
    /// <summary>
    /// 动态排序，使用OrderByBuilder构建排序字段选择器，fieldsGetter可以是单个或多个字段的匿名对象，如：
    /// <code>string orderFields = "Name";bool isAsc = true;
    /// .OrderByDynamic(t =&gt; t.Switch(orderFields, isAsc).When("Name", (a, b, ...) =&lt; a.Name).When("Gender", (a, b, ...) =&lt; a.Gender).Build()</code>)
    /// </summary>
    /// <param name="fieldsGetter">排序字段选择器，需调用Build方法，返回排序字段表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>, Expression> fieldsGetter);
    #endregion

    #region Skip/Take/Page
    /// <summary>
    /// 跳过offset条数据
    /// </summary>
    /// <param name="offset">要跳过的数据条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Skip(int offset);
    /// <summary>
    /// 只返回limit条数据
    /// </summary>
    /// <param name="limit">返回的数据条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Take(int limit);
    /// <summary>
    /// 分页查询，pageNumber从1开始
    /// </summary>
    /// <param name="pageNumber">第几页，从1开始，小于1时当作1处理</param>
    /// <param name="pageSize">每页显示条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Page(int pageNumber, int pageSize);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，如：
    /// Select((a, b, ...) =&gt; new { a.Id, a.Name, ... }) 或是 Select((a, b, ...) =&gt; x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定字段返回，只需要指定需要特殊处理的成员赋值即可，其他成员将从现有表的字段中按名称匹配赋值，多个同名字段时如果未特殊指定赋值，默认选取第一个表中的字段赋值。如：
    /// <code> .SelectTo&lt;TDto&gt;() 或是 .SelectTo((a, b) =&gt; new TDto{ b.Id }) //使用第二个表的Id字段作为Id成员</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型，通常是一个匿名类</typeparam>
    /// <param name="specialMemberSelector">特殊成员赋值表达式，通常是重名字段或是不存在的字段赋值</param>
    /// <returns>返回查询对象</returns>
    IQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget>> specialMemberSelector = null);
    #endregion

    #region Count
    /// <summary>
    /// 返回某个字段的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11&gt;().Count((a, b, c, d, e, f, g, h, i, j, k) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的int类型数据条数</returns>
    int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11&gt;().CountAsync((a, b, c, d, e, f, g, h, i, j, k) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的int类型数据条数</returns>
    Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11&gt;().CountDistinct((a, b, c, d, e, f, g, h, i, j, k) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段去重后的int类型数据条数</returns>
    int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11&gt;().CountDistinctAsync((a, b, c, d, e, f, g, h, i, j, k) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段去重后的int类型数据条数</returns>
    Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11&gt;().LongCount((a, b, c, d, e, f, g, h, i, j, k) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的long类型数据条数</returns>
    long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11&gt;().LongCountAsync((a, b, c, d, e, f, g, h, i, j, k) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的long类型数据条数</returns>
    Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11&gt;().LongCountDistinct((a, b, c, d, e, f, g, h, i, j, k) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段去重后的long类型数据条数</returns>
    long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11&gt;().LongCountDistinctAsync((a, b, c, d, e, f, g, h, i, j, k) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段去重后的long类型数据条数</returns>
    Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr, CancellationToken cancellationToken = default);
    #endregion

    #region Aggregate
    /// <summary>
    /// 计算指定字段的求和值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11&gt;().Sum((a, b, c, d, e, f, g, h, i, j, k) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的求和值</returns>
    decimal Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的求和值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11&gt;().SumAsync((a, b, c, d, e, f, g, h, i, j, k) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的求和值</returns>
    Task<decimal> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的平均值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11&gt;().Avg((a, b, c, d, e, f, g, h, i, j, k) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的平均值</returns>
    TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的平均值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11&gt;().AvgAsync((a, b, c, d, e, f, g, h, i, j, k) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的平均值</returns>
    Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的最大值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11&gt;().Max((a, b, c, d, e, f, g, h, i, j, k) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的最大值</returns>
    TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最大值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11&gt;().MaxAsync((a, b, c, d, e, f, g, h, i, j, k) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的最大值</returns>
    Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的最小值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11&gt;().Min((a, b, c, d, e, f, g, h, i, j, k) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的最小值</returns>
    TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最小值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11&gt;().MinAsync((a, b, c, d, e, f, g, h, i, j, k) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的最小值</returns>
    Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr, CancellationToken cancellationToken = default);
    #endregion
}
/// <summary>
/// 多表T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12查询
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
/// <typeparam name="T5">表T5实体类型</typeparam>
/// <typeparam name="T6">表T6实体类型</typeparam>
/// <typeparam name="T7">表T7实体类型</typeparam>
/// <typeparam name="T8">表T8实体类型</typeparam>
/// <typeparam name="T9">表T9实体类型</typeparam>
/// <typeparam name="T10">表T10实体类型</typeparam>
/// <typeparam name="T11">表T11实体类型</typeparam>
/// <typeparam name="T12">表T12实体类型</typeparam>
public interface IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : IQueryBase
{
    #region Sharding
    /// <summary>
    /// 直接指定T12表分表名，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> UseTable(params string[] tableNames);
    /// <summary>
    /// 根据首个分表TMasterSharding表与当前T12表名的映射关系，指定当前T12表分表名获取委托，执行委托获取当前T12表分表名。委托第一个参数是TMasterSharding主表原始表名，第二个参数是当前T12表原始表名，第三个参数是TMasterSharding表当前分表名，返回值是当前T12表分表名称，如：
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
    /// <param name="tableNameGetter">T12表分表名获取委托</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter);
    /// <summary>
    /// 手动指定分表规则参数值，执行分表规则确定T12表分表名，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，最多支持3个字段值，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> UseTableBy(params object[] fieldValues);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T12表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginFieldValue &lt;= endFieldValue，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="beginFieldValue">字段范围起始值</param>
    /// <param name="endFieldValue">字段范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> UseTableByRange(object beginFieldValue, object endFieldValue);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T12表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginField2Value &lt;= endField2Value，常用于是日期规则分表，如：.UseTableByRange(1, DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="beginField2Value">字段2范围起始值</param>
    /// <param name="endField2Value">字段2范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> UseTableByRange(object field1Value, object beginField2Value, object endField2Value);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T12表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginField3Value &lt;= endField3Value，常用于是日期规则分表，如：.UseTableByRange(1, "Game1", DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="field2Value">字段2值</param>
    /// <param name="beginField3Value">字段3范围起始值</param>
    /// <param name="endField3Value">字段3范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value);
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> UseTableSchema(string tableSchema);
    #endregion

    #region WithTable
    /// <summary>
    /// 添加实体表，方便后面做JOIN关联，如：<code>.From&lt;Menu&gt;().WithTable&lt;Page&gt;()</code>
    /// </summary>
    /// <typeparam name="TOther">实体表类型</typeparam>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> WithTable<TOther>();
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> WithQuery<TOther>(IQuery<TOther> subQuery);
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr);
    #endregion

    #region Include
    /// <summary>
    /// 加载导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，1:1关联关系，随主表一起查询,支持无限级，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .From&lt;Product&gt;().Include(f =&gt; f.Brand) ...
    /// .From&lt;Brand&gt;().Include(f =&gt; f.Products) ...
    /// .From&lt;Order&gt;().Include(f =&gt; f.Seller.Company.Products) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TMember">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>> memberSelector);
    /// <summary>
    /// 加载集合类导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，可使用filter筛选满足条件的导航属性，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .From&lt;User&gt;().IncludeMany(f =&gt; f.Orders)  //与 .From&lt;User&gt;().Include(f =&gt; f.Orders) 等价
    /// .From&lt;User&gt;().IncludeMany(f =&gt; f.Orders, order =&gt; order.TotalAmout &gt; 500)
    /// </code>
    /// </summary>
    /// <typeparam name="TElement">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null);
    #endregion

    #region InnerJoin
    /// <summary>
    /// 在现有表中，选择两个表进行INNER JOIN关联，可多次关联，如：<code>.InnerJoin((a, b, ...) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做INNER JOIN关联，与.WithTable&lt;TOther&gt;().InnerJoin(...)等价，如：
    /// <code>
    /// .From&lt;User&gt;().InnerJoin&lt;Order&gt;((x, y) =&gt; x.Id == y.BuyerId)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr表达式构建子查询，并选择一个现有表做INNER JOIN关联，与.WithQuery(subQueryExpr).InnerJoin(...)等价，如：
    /// <code>
    /// .InnerJoin(f =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select(x =&gt; new { ... }), (a, b, ...) =&gt; x.xxx = y.yyy) ...
    /// SQL: ... INNER JOIN (SELECT ... FROM `sys_order_detail` ...) m ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn);
    #endregion

    #region LeftJoin
    /// <summary>
    /// 在现有表中，选择两个表进行LEFT JOIN关联，可多次关联，如：<code>.LeftJoin((a, b, ...) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做LEFT JOIN关联，与.WithTable&lt;TOther&gt;().LeftJoin(...)等价，如：
    /// <code>
    /// .LeftJoin&lt;TOther&gt;((a, b, ...) =&gt; a.xxx = b.yyy)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr表达式构建子查询，并选择一个现有表做LEFT JOIN关联，与.WithQuery(subQueryExpr).LeftJoin(...)等价，如：
    /// <code>
    /// .LeftJoin(f =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select(t =&gt; new { ... }), (a, b, ...) =&gt; a.Id = b.OrderId) ...
    /// SQL: ... LEFT JOIN (SELECT ... FROM `sys_order_detail` ...) m ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn);
    #endregion

    #region RightJoin
    /// <summary>
    /// 在现有表中，选择两个表进行RIGHT JOIN关联，可多次关联，如：<code>.RightJoin((a, b) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做RIGHT JOIN关联，与.WithTable&lt;TOther&gt;().RightJoin(...)等价，如：
    /// <code>
    /// .RightJoin&lt;TOther&gt;((a, b, c, d, e, f, g, h, i, j, k, l) =&gt; ...)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr表达式构建子查询，并选择一个现有表做RIGHT JOIN关联，与.WithQuery(subQueryExpr).RightJoin(...)等价，如：
    /// <code>
    /// .RightJoin(f =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select(t =&gt; new { ... }), (a, b, ...) =&gt; a.Id = b.OrderId) ...
    /// SQL: ... RIGHT JOIN (SELECT ... FROM `sys_order_detail` ...) m ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn);
    #endregion

    #region Where
    /// <summary>
    /// 条件查询，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate);
    /// <summary>
    /// 条件查询，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>>> predicateInitializer);
    #endregion

    #region And
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，并与已有的条件AND操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>>> predicateInitializer);
    #endregion

    #region Or
    /// <summary>
    /// 使用predicate表达式生成Or条件，并添加到已有的Where条件末尾，表达式predicate不可为null
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件OR操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，并与已有的条件OR操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>>> predicateInitializer);
    #endregion

    #region GroupBy
    /// <summary>
    /// 分组查询，分组表达式groupingExpr可以是单个或多个字段的匿名对象，如：
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
    /// <param name="groupingExpr">分组表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping>> groupingExpr);
    #endregion

    #region OrderBy
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// OrderBy((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderBy((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr);
    /// <summary>
    /// ASC排序，condition为true，ASC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderBy(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr);
    /// <summary>
    /// DSC排序，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// OrderByDescending((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，condition为true，DESC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr);
    /// <summary>
    /// 动态排序，使用OrderByBuilder构建排序字段选择器，fieldsGetter可以是单个或多个字段的匿名对象，如：
    /// <code>string orderFields = "Name";bool isAsc = true;
    /// .OrderByDynamic(t =&gt; t.Switch(orderFields, isAsc).When("Name", (a, b, ...) =&lt; a.Name).When("Gender", (a, b, ...) =&lt; a.Gender).Build()</code>)
    /// </summary>
    /// <param name="fieldsGetter">排序字段选择器，需调用Build方法，返回排序字段表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>, Expression> fieldsGetter);
    #endregion

    #region Skip/Take/Page
    /// <summary>
    /// 跳过offset条数据
    /// </summary>
    /// <param name="offset">要跳过的数据条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Skip(int offset);
    /// <summary>
    /// 只返回limit条数据
    /// </summary>
    /// <param name="limit">返回的数据条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Take(int limit);
    /// <summary>
    /// 分页查询，pageNumber从1开始
    /// </summary>
    /// <param name="pageNumber">第几页，从1开始，小于1时当作1处理</param>
    /// <param name="pageSize">每页显示条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Page(int pageNumber, int pageSize);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，如：
    /// Select((a, b, ...) =&gt; new { a.Id, a.Name, ... }) 或是 Select((a, b, ...) =&gt; x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定字段返回，只需要指定需要特殊处理的成员赋值即可，其他成员将从现有表的字段中按名称匹配赋值，多个同名字段时如果未特殊指定赋值，默认选取第一个表中的字段赋值。如：
    /// <code> .SelectTo&lt;TDto&gt;() 或是 .SelectTo((a, b) =&gt; new TDto{ b.Id }) //使用第二个表的Id字段作为Id成员</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型，通常是一个匿名类</typeparam>
    /// <param name="specialMemberSelector">特殊成员赋值表达式，通常是重名字段或是不存在的字段赋值</param>
    /// <returns>返回查询对象</returns>
    IQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget>> specialMemberSelector = null);
    #endregion

    #region Count
    /// <summary>
    /// 返回某个字段的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12&gt;().Count((a, b, c, d, e, f, g, h, i, j, k, l) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的int类型数据条数</returns>
    int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12&gt;().CountAsync((a, b, c, d, e, f, g, h, i, j, k, l) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的int类型数据条数</returns>
    Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12&gt;().CountDistinct((a, b, c, d, e, f, g, h, i, j, k, l) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段去重后的int类型数据条数</returns>
    int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12&gt;().CountDistinctAsync((a, b, c, d, e, f, g, h, i, j, k, l) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段去重后的int类型数据条数</returns>
    Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12&gt;().LongCount((a, b, c, d, e, f, g, h, i, j, k, l) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的long类型数据条数</returns>
    long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12&gt;().LongCountAsync((a, b, c, d, e, f, g, h, i, j, k, l) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的long类型数据条数</returns>
    Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12&gt;().LongCountDistinct((a, b, c, d, e, f, g, h, i, j, k, l) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段去重后的long类型数据条数</returns>
    long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12&gt;().LongCountDistinctAsync((a, b, c, d, e, f, g, h, i, j, k, l) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段去重后的long类型数据条数</returns>
    Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr, CancellationToken cancellationToken = default);
    #endregion

    #region Aggregate
    /// <summary>
    /// 计算指定字段的求和值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12&gt;().Sum((a, b, c, d, e, f, g, h, i, j, k, l) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的求和值</returns>
    decimal Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的求和值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12&gt;().SumAsync((a, b, c, d, e, f, g, h, i, j, k, l) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的求和值</returns>
    Task<decimal> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的平均值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12&gt;().Avg((a, b, c, d, e, f, g, h, i, j, k, l) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的平均值</returns>
    TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的平均值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12&gt;().AvgAsync((a, b, c, d, e, f, g, h, i, j, k, l) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的平均值</returns>
    Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的最大值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12&gt;().Max((a, b, c, d, e, f, g, h, i, j, k, l) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的最大值</returns>
    TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最大值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12&gt;().MaxAsync((a, b, c, d, e, f, g, h, i, j, k, l) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的最大值</returns>
    Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的最小值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12&gt;().Min((a, b, c, d, e, f, g, h, i, j, k, l) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的最小值</returns>
    TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最小值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12&gt;().MinAsync((a, b, c, d, e, f, g, h, i, j, k, l) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的最小值</returns>
    Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr, CancellationToken cancellationToken = default);
    #endregion
}
/// <summary>
/// 多表T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13查询
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
/// <typeparam name="T5">表T5实体类型</typeparam>
/// <typeparam name="T6">表T6实体类型</typeparam>
/// <typeparam name="T7">表T7实体类型</typeparam>
/// <typeparam name="T8">表T8实体类型</typeparam>
/// <typeparam name="T9">表T9实体类型</typeparam>
/// <typeparam name="T10">表T10实体类型</typeparam>
/// <typeparam name="T11">表T11实体类型</typeparam>
/// <typeparam name="T12">表T12实体类型</typeparam>
/// <typeparam name="T13">表T13实体类型</typeparam>
public interface IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> : IQueryBase
{
    #region Sharding
    /// <summary>
    /// 直接指定T13表分表名，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> UseTable(params string[] tableNames);
    /// <summary>
    /// 根据首个分表TMasterSharding表与当前T13表名的映射关系，指定当前T13表分表名获取委托，执行委托获取当前T13表分表名。委托第一个参数是TMasterSharding主表原始表名，第二个参数是当前T13表原始表名，第三个参数是TMasterSharding表当前分表名，返回值是当前T13表分表名称，如：
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
    /// <param name="tableNameGetter">T13表分表名获取委托</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter);
    /// <summary>
    /// 手动指定分表规则参数值，执行分表规则确定T13表分表名，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，最多支持3个字段值，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> UseTableBy(params object[] fieldValues);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T13表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginFieldValue &lt;= endFieldValue，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="beginFieldValue">字段范围起始值</param>
    /// <param name="endFieldValue">字段范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> UseTableByRange(object beginFieldValue, object endFieldValue);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T13表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginField2Value &lt;= endField2Value，常用于是日期规则分表，如：.UseTableByRange(1, DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="beginField2Value">字段2范围起始值</param>
    /// <param name="endField2Value">字段2范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> UseTableByRange(object field1Value, object beginField2Value, object endField2Value);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T13表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginField3Value &lt;= endField3Value，常用于是日期规则分表，如：.UseTableByRange(1, "Game1", DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="field2Value">字段2值</param>
    /// <param name="beginField3Value">字段3范围起始值</param>
    /// <param name="endField3Value">字段3范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value);
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> UseTableSchema(string tableSchema);
    #endregion

    #region WithTable
    /// <summary>
    /// 添加实体表，方便后面做JOIN关联，如：<code>.From&lt;Menu&gt;().WithTable&lt;Page&gt;()</code>
    /// </summary>
    /// <typeparam name="TOther">实体表类型</typeparam>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> WithTable<TOther>();
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> WithQuery<TOther>(IQuery<TOther> subQuery);
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr);
    #endregion

    #region Include
    /// <summary>
    /// 加载导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，1:1关联关系，随主表一起查询,支持无限级，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .From&lt;Product&gt;().Include(f =&gt; f.Brand) ...
    /// .From&lt;Brand&gt;().Include(f =&gt; f.Products) ...
    /// .From&lt;Order&gt;().Include(f =&gt; f.Seller.Company.Products) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TMember">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>> memberSelector);
    /// <summary>
    /// 加载集合类导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，可使用filter筛选满足条件的导航属性，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .From&lt;User&gt;().IncludeMany(f =&gt; f.Orders)  //与 .From&lt;User&gt;().Include(f =&gt; f.Orders) 等价
    /// .From&lt;User&gt;().IncludeMany(f =&gt; f.Orders, order =&gt; order.TotalAmout &gt; 500)
    /// </code>
    /// </summary>
    /// <typeparam name="TElement">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null);
    #endregion

    #region InnerJoin
    /// <summary>
    /// 在现有表中，选择两个表进行INNER JOIN关联，可多次关联，如：<code>.InnerJoin((a, b, ...) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做INNER JOIN关联，与.WithTable&lt;TOther&gt;().InnerJoin(...)等价，如：
    /// <code>
    /// .From&lt;User&gt;().InnerJoin&lt;Order&gt;((x, y) =&gt; x.Id == y.BuyerId)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr表达式构建子查询，并选择一个现有表做INNER JOIN关联，与.WithQuery(subQueryExpr).InnerJoin(...)等价，如：
    /// <code>
    /// .InnerJoin(f =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select(x =&gt; new { ... }), (a, b, ...) =&gt; x.xxx = y.yyy) ...
    /// SQL: ... INNER JOIN (SELECT ... FROM `sys_order_detail` ...) n ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn);
    #endregion

    #region LeftJoin
    /// <summary>
    /// 在现有表中，选择两个表进行LEFT JOIN关联，可多次关联，如：<code>.LeftJoin((a, b, ...) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做LEFT JOIN关联，与.WithTable&lt;TOther&gt;().LeftJoin(...)等价，如：
    /// <code>
    /// .LeftJoin&lt;TOther&gt;((a, b, ...) =&gt; a.xxx = b.yyy)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr表达式构建子查询，并选择一个现有表做LEFT JOIN关联，与.WithQuery(subQueryExpr).LeftJoin(...)等价，如：
    /// <code>
    /// .LeftJoin(f =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select(t =&gt; new { ... }), (a, b, ...) =&gt; a.Id = b.OrderId) ...
    /// SQL: ... LEFT JOIN (SELECT ... FROM `sys_order_detail` ...) n ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn);
    #endregion

    #region RightJoin
    /// <summary>
    /// 在现有表中，选择两个表进行RIGHT JOIN关联，可多次关联，如：<code>.RightJoin((a, b) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做RIGHT JOIN关联，与.WithTable&lt;TOther&gt;().RightJoin(...)等价，如：
    /// <code>
    /// .RightJoin&lt;TOther&gt;((a, b, c, d, e, f, g, h, i, j, k, l, m) =&gt; ...)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr表达式构建子查询，并选择一个现有表做RIGHT JOIN关联，与.WithQuery(subQueryExpr).RightJoin(...)等价，如：
    /// <code>
    /// .RightJoin(f =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select(t =&gt; new { ... }), (a, b, ...) =&gt; a.Id = b.OrderId) ...
    /// SQL: ... RIGHT JOIN (SELECT ... FROM `sys_order_detail` ...) n ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn);
    #endregion

    #region Where
    /// <summary>
    /// 条件查询，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate);
    /// <summary>
    /// 条件查询，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>>> predicateInitializer);
    #endregion

    #region And
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，并与已有的条件AND操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>>> predicateInitializer);
    #endregion

    #region Or
    /// <summary>
    /// 使用predicate表达式生成Or条件，并添加到已有的Where条件末尾，表达式predicate不可为null
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件OR操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，并与已有的条件OR操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>>> predicateInitializer);
    #endregion

    #region GroupBy
    /// <summary>
    /// 分组查询，分组表达式groupingExpr可以是单个或多个字段的匿名对象，如：
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
    /// <param name="groupingExpr">分组表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping>> groupingExpr);
    #endregion

    #region OrderBy
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// OrderBy((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderBy((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr);
    /// <summary>
    /// ASC排序，condition为true，ASC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderBy(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr);
    /// <summary>
    /// DSC排序，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// OrderByDescending((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，condition为true，DESC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr);
    /// <summary>
    /// 动态排序，使用OrderByBuilder构建排序字段选择器，fieldsGetter可以是单个或多个字段的匿名对象，如：
    /// <code>string orderFields = "Name";bool isAsc = true;
    /// .OrderByDynamic(t =&gt; t.Switch(orderFields, isAsc).When("Name", (a, b, ...) =&lt; a.Name).When("Gender", (a, b, ...) =&lt; a.Gender).Build()</code>)
    /// </summary>
    /// <param name="fieldsGetter">排序字段选择器，需调用Build方法，返回排序字段表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>, Expression> fieldsGetter);
    #endregion

    #region Skip/Take/Page
    /// <summary>
    /// 跳过offset条数据
    /// </summary>
    /// <param name="offset">要跳过的数据条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Skip(int offset);
    /// <summary>
    /// 只返回limit条数据
    /// </summary>
    /// <param name="limit">返回的数据条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Take(int limit);
    /// <summary>
    /// 分页查询，pageNumber从1开始
    /// </summary>
    /// <param name="pageNumber">第几页，从1开始，小于1时当作1处理</param>
    /// <param name="pageSize">每页显示条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Page(int pageNumber, int pageSize);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，如：
    /// Select((a, b, ...) =&gt; new { a.Id, a.Name, ... }) 或是 Select((a, b, ...) =&gt; x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定字段返回，只需要指定需要特殊处理的成员赋值即可，其他成员将从现有表的字段中按名称匹配赋值，多个同名字段时如果未特殊指定赋值，默认选取第一个表中的字段赋值。如：
    /// <code> .SelectTo&lt;TDto&gt;() 或是 .SelectTo((a, b) =&gt; new TDto{ b.Id }) //使用第二个表的Id字段作为Id成员</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型，通常是一个匿名类</typeparam>
    /// <param name="specialMemberSelector">特殊成员赋值表达式，通常是重名字段或是不存在的字段赋值</param>
    /// <returns>返回查询对象</returns>
    IQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget>> specialMemberSelector = null);
    #endregion

    #region Count
    /// <summary>
    /// 返回某个字段的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13&gt;().Count((a, b, c, d, e, f, g, h, i, j, k, l, m) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的int类型数据条数</returns>
    int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13&gt;().CountAsync((a, b, c, d, e, f, g, h, i, j, k, l, m) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的int类型数据条数</returns>
    Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13&gt;().CountDistinct((a, b, c, d, e, f, g, h, i, j, k, l, m) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段去重后的int类型数据条数</returns>
    int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13&gt;().CountDistinctAsync((a, b, c, d, e, f, g, h, i, j, k, l, m) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段去重后的int类型数据条数</returns>
    Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13&gt;().LongCount((a, b, c, d, e, f, g, h, i, j, k, l, m) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的long类型数据条数</returns>
    long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13&gt;().LongCountAsync((a, b, c, d, e, f, g, h, i, j, k, l, m) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的long类型数据条数</returns>
    Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13&gt;().LongCountDistinct((a, b, c, d, e, f, g, h, i, j, k, l, m) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段去重后的long类型数据条数</returns>
    long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13&gt;().LongCountDistinctAsync((a, b, c, d, e, f, g, h, i, j, k, l, m) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段去重后的long类型数据条数</returns>
    Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr, CancellationToken cancellationToken = default);
    #endregion

    #region Aggregate
    /// <summary>
    /// 计算指定字段的求和值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13&gt;().Sum((a, b, c, d, e, f, g, h, i, j, k, l, m) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的求和值</returns>
    decimal Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的求和值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13&gt;().SumAsync((a, b, c, d, e, f, g, h, i, j, k, l, m) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的求和值</returns>
    Task<decimal> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的平均值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13&gt;().Avg((a, b, c, d, e, f, g, h, i, j, k, l, m) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的平均值</returns>
    TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的平均值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13&gt;().AvgAsync((a, b, c, d, e, f, g, h, i, j, k, l, m) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的平均值</returns>
    Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的最大值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13&gt;().Max((a, b, c, d, e, f, g, h, i, j, k, l, m) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的最大值</returns>
    TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最大值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13&gt;().MaxAsync((a, b, c, d, e, f, g, h, i, j, k, l, m) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的最大值</returns>
    Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的最小值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13&gt;().Min((a, b, c, d, e, f, g, h, i, j, k, l, m) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的最小值</returns>
    TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最小值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13&gt;().MinAsync((a, b, c, d, e, f, g, h, i, j, k, l, m) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的最小值</returns>
    Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr, CancellationToken cancellationToken = default);
    #endregion
}
/// <summary>
/// 多表T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14查询
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
/// <typeparam name="T5">表T5实体类型</typeparam>
/// <typeparam name="T6">表T6实体类型</typeparam>
/// <typeparam name="T7">表T7实体类型</typeparam>
/// <typeparam name="T8">表T8实体类型</typeparam>
/// <typeparam name="T9">表T9实体类型</typeparam>
/// <typeparam name="T10">表T10实体类型</typeparam>
/// <typeparam name="T11">表T11实体类型</typeparam>
/// <typeparam name="T12">表T12实体类型</typeparam>
/// <typeparam name="T13">表T13实体类型</typeparam>
/// <typeparam name="T14">表T14实体类型</typeparam>
public interface IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> : IQueryBase
{
    #region Sharding
    /// <summary>
    /// 直接指定T14表分表名，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> UseTable(params string[] tableNames);
    /// <summary>
    /// 根据首个分表TMasterSharding表与当前T14表名的映射关系，指定当前T14表分表名获取委托，执行委托获取当前T14表分表名。委托第一个参数是TMasterSharding主表原始表名，第二个参数是当前T14表原始表名，第三个参数是TMasterSharding表当前分表名，返回值是当前T14表分表名称，如：
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
    /// <param name="tableNameGetter">T14表分表名获取委托</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter);
    /// <summary>
    /// 手动指定分表规则参数值，执行分表规则确定T14表分表名，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，最多支持3个字段值，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> UseTableBy(params object[] fieldValues);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T14表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginFieldValue &lt;= endFieldValue，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="beginFieldValue">字段范围起始值</param>
    /// <param name="endFieldValue">字段范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> UseTableByRange(object beginFieldValue, object endFieldValue);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T14表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginField2Value &lt;= endField2Value，常用于是日期规则分表，如：.UseTableByRange(1, DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="beginField2Value">字段2范围起始值</param>
    /// <param name="endField2Value">字段2范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> UseTableByRange(object field1Value, object beginField2Value, object endField2Value);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T14表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginField3Value &lt;= endField3Value，常用于是日期规则分表，如：.UseTableByRange(1, "Game1", DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="field2Value">字段2值</param>
    /// <param name="beginField3Value">字段3范围起始值</param>
    /// <param name="endField3Value">字段3范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value);
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> UseTableSchema(string tableSchema);
    #endregion

    #region WithTable
    /// <summary>
    /// 添加实体表，方便后面做JOIN关联，如：<code>.From&lt;Menu&gt;().WithTable&lt;Page&gt;()</code>
    /// </summary>
    /// <typeparam name="TOther">实体表类型</typeparam>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> WithTable<TOther>();
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> WithQuery<TOther>(IQuery<TOther> subQuery);
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr);
    #endregion

    #region Include
    /// <summary>
    /// 加载导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，1:1关联关系，随主表一起查询,支持无限级，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .From&lt;Product&gt;().Include(f =&gt; f.Brand) ...
    /// .From&lt;Brand&gt;().Include(f =&gt; f.Products) ...
    /// .From&lt;Order&gt;().Include(f =&gt; f.Seller.Company.Products) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TMember">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>> memberSelector);
    /// <summary>
    /// 加载集合类导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，可使用filter筛选满足条件的导航属性，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .From&lt;User&gt;().IncludeMany(f =&gt; f.Orders)  //与 .From&lt;User&gt;().Include(f =&gt; f.Orders) 等价
    /// .From&lt;User&gt;().IncludeMany(f =&gt; f.Orders, order =&gt; order.TotalAmout &gt; 500)
    /// </code>
    /// </summary>
    /// <typeparam name="TElement">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null);
    #endregion

    #region InnerJoin
    /// <summary>
    /// 在现有表中，选择两个表进行INNER JOIN关联，可多次关联，如：<code>.InnerJoin((a, b, ...) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做INNER JOIN关联，与.WithTable&lt;TOther&gt;().InnerJoin(...)等价，如：
    /// <code>
    /// .From&lt;User&gt;().InnerJoin&lt;Order&gt;((x, y) =&gt; x.Id == y.BuyerId)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr表达式构建子查询，并选择一个现有表做INNER JOIN关联，与.WithQuery(subQueryExpr).InnerJoin(...)等价，如：
    /// <code>
    /// .InnerJoin(f =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select(x =&gt; new { ... }), (a, b, ...) =&gt; x.xxx = y.yyy) ...
    /// SQL: ... INNER JOIN (SELECT ... FROM `sys_order_detail` ...) o ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn);
    #endregion

    #region LeftJoin
    /// <summary>
    /// 在现有表中，选择两个表进行LEFT JOIN关联，可多次关联，如：<code>.LeftJoin((a, b, ...) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做LEFT JOIN关联，与.WithTable&lt;TOther&gt;().LeftJoin(...)等价，如：
    /// <code>
    /// .LeftJoin&lt;TOther&gt;((a, b, ...) =&gt; a.xxx = b.yyy)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr表达式构建子查询，并选择一个现有表做LEFT JOIN关联，与.WithQuery(subQueryExpr).LeftJoin(...)等价，如：
    /// <code>
    /// .LeftJoin(f =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select(t =&gt; new { ... }), (a, b, ...) =&gt; a.Id = b.OrderId) ...
    /// SQL: ... LEFT JOIN (SELECT ... FROM `sys_order_detail` ...) o ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn);
    #endregion

    #region RightJoin
    /// <summary>
    /// 在现有表中，选择两个表进行RIGHT JOIN关联，可多次关联，如：<code>.RightJoin((a, b) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做RIGHT JOIN关联，与.WithTable&lt;TOther&gt;().RightJoin(...)等价，如：
    /// <code>
    /// .RightJoin&lt;TOther&gt;((a, b, c, d, e, f, g, h, i, j, k, l, m, n) =&gt; ...)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr表达式构建子查询，并选择一个现有表做RIGHT JOIN关联，与.WithQuery(subQueryExpr).RightJoin(...)等价，如：
    /// <code>
    /// .RightJoin(f =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select(t =&gt; new { ... }), (a, b, ...) =&gt; a.Id = b.OrderId) ...
    /// SQL: ... RIGHT JOIN (SELECT ... FROM `sys_order_detail` ...) o ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn);
    #endregion

    #region Where
    /// <summary>
    /// 条件查询，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate);
    /// <summary>
    /// 条件查询，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>>> predicateInitializer);
    #endregion

    #region And
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，并与已有的条件AND操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>>> predicateInitializer);
    #endregion

    #region Or
    /// <summary>
    /// 使用predicate表达式生成Or条件，并添加到已有的Where条件末尾，表达式predicate不可为null
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件OR操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，并与已有的条件OR操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>>> predicateInitializer);
    #endregion

    #region GroupBy
    /// <summary>
    /// 分组查询，分组表达式groupingExpr可以是单个或多个字段的匿名对象，如：
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
    /// <param name="groupingExpr">分组表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping>> groupingExpr);
    #endregion

    #region OrderBy
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// OrderBy((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderBy((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr);
    /// <summary>
    /// ASC排序，condition为true，ASC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderBy(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr);
    /// <summary>
    /// DSC排序，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// OrderByDescending((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，condition为true，DESC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr);
    /// <summary>
    /// 动态排序，使用OrderByBuilder构建排序字段选择器，fieldsGetter可以是单个或多个字段的匿名对象，如：
    /// <code>string orderFields = "Name";bool isAsc = true;
    /// .OrderByDynamic(t =&gt; t.Switch(orderFields, isAsc).When("Name", (a, b, ...) =&lt; a.Name).When("Gender", (a, b, ...) =&lt; a.Gender).Build()</code>)
    /// </summary>
    /// <param name="fieldsGetter">排序字段选择器，需调用Build方法，返回排序字段表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>, Expression> fieldsGetter);
    #endregion

    #region Skip/Take/Page
    /// <summary>
    /// 跳过offset条数据
    /// </summary>
    /// <param name="offset">要跳过的数据条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Skip(int offset);
    /// <summary>
    /// 只返回limit条数据
    /// </summary>
    /// <param name="limit">返回的数据条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Take(int limit);
    /// <summary>
    /// 分页查询，pageNumber从1开始
    /// </summary>
    /// <param name="pageNumber">第几页，从1开始，小于1时当作1处理</param>
    /// <param name="pageSize">每页显示条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Page(int pageNumber, int pageSize);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，如：
    /// Select((a, b, ...) =&gt; new { a.Id, a.Name, ... }) 或是 Select((a, b, ...) =&gt; x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定字段返回，只需要指定需要特殊处理的成员赋值即可，其他成员将从现有表的字段中按名称匹配赋值，多个同名字段时如果未特殊指定赋值，默认选取第一个表中的字段赋值。如：
    /// <code> .SelectTo&lt;TDto&gt;() 或是 .SelectTo((a, b) =&gt; new TDto{ b.Id }) //使用第二个表的Id字段作为Id成员</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型，通常是一个匿名类</typeparam>
    /// <param name="specialMemberSelector">特殊成员赋值表达式，通常是重名字段或是不存在的字段赋值</param>
    /// <returns>返回查询对象</returns>
    IQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget>> specialMemberSelector = null);
    #endregion

    #region Count
    /// <summary>
    /// 返回某个字段的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14&gt;().Count((a, b, c, d, e, f, g, h, i, j, k, l, m, n) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的int类型数据条数</returns>
    int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14&gt;().CountAsync((a, b, c, d, e, f, g, h, i, j, k, l, m, n) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的int类型数据条数</returns>
    Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14&gt;().CountDistinct((a, b, c, d, e, f, g, h, i, j, k, l, m, n) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段去重后的int类型数据条数</returns>
    int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14&gt;().CountDistinctAsync((a, b, c, d, e, f, g, h, i, j, k, l, m, n) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段去重后的int类型数据条数</returns>
    Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14&gt;().LongCount((a, b, c, d, e, f, g, h, i, j, k, l, m, n) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的long类型数据条数</returns>
    long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14&gt;().LongCountAsync((a, b, c, d, e, f, g, h, i, j, k, l, m, n) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的long类型数据条数</returns>
    Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14&gt;().LongCountDistinct((a, b, c, d, e, f, g, h, i, j, k, l, m, n) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段去重后的long类型数据条数</returns>
    long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14&gt;().LongCountDistinctAsync((a, b, c, d, e, f, g, h, i, j, k, l, m, n) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段去重后的long类型数据条数</returns>
    Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr, CancellationToken cancellationToken = default);
    #endregion

    #region Aggregate
    /// <summary>
    /// 计算指定字段的求和值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14&gt;().Sum((a, b, c, d, e, f, g, h, i, j, k, l, m, n) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的求和值</returns>
    decimal Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的求和值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14&gt;().SumAsync((a, b, c, d, e, f, g, h, i, j, k, l, m, n) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的求和值</returns>
    Task<decimal> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的平均值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14&gt;().Avg((a, b, c, d, e, f, g, h, i, j, k, l, m, n) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的平均值</returns>
    TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的平均值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14&gt;().AvgAsync((a, b, c, d, e, f, g, h, i, j, k, l, m, n) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的平均值</returns>
    Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的最大值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14&gt;().Max((a, b, c, d, e, f, g, h, i, j, k, l, m, n) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的最大值</returns>
    TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最大值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14&gt;().MaxAsync((a, b, c, d, e, f, g, h, i, j, k, l, m, n) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的最大值</returns>
    Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的最小值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14&gt;().Min((a, b, c, d, e, f, g, h, i, j, k, l, m, n) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的最小值</returns>
    TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最小值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14&gt;().MinAsync((a, b, c, d, e, f, g, h, i, j, k, l, m, n) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的最小值</returns>
    Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr, CancellationToken cancellationToken = default);
    #endregion
}
/// <summary>
/// 多表T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15查询
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
/// <typeparam name="T5">表T5实体类型</typeparam>
/// <typeparam name="T6">表T6实体类型</typeparam>
/// <typeparam name="T7">表T7实体类型</typeparam>
/// <typeparam name="T8">表T8实体类型</typeparam>
/// <typeparam name="T9">表T9实体类型</typeparam>
/// <typeparam name="T10">表T10实体类型</typeparam>
/// <typeparam name="T11">表T11实体类型</typeparam>
/// <typeparam name="T12">表T12实体类型</typeparam>
/// <typeparam name="T13">表T13实体类型</typeparam>
/// <typeparam name="T14">表T14实体类型</typeparam>
/// <typeparam name="T15">表T15实体类型</typeparam>
public interface IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> : IQueryBase
{
    #region Sharding
    /// <summary>
    /// 直接指定T15表分表名，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> UseTable(params string[] tableNames);
    /// <summary>
    /// 根据首个分表TMasterSharding表与当前T15表名的映射关系，指定当前T15表分表名获取委托，执行委托获取当前T15表分表名。委托第一个参数是TMasterSharding主表原始表名，第二个参数是当前T15表原始表名，第三个参数是TMasterSharding表当前分表名，返回值是当前T15表分表名称，如：
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
    /// <param name="tableNameGetter">T15表分表名获取委托</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter);
    /// <summary>
    /// 手动指定分表规则参数值，执行分表规则确定T15表分表名，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，最多支持3个字段值，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> UseTableBy(params object[] fieldValues);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T15表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginFieldValue &lt;= endFieldValue，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="beginFieldValue">字段范围起始值</param>
    /// <param name="endFieldValue">字段范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> UseTableByRange(object beginFieldValue, object endFieldValue);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T15表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginField2Value &lt;= endField2Value，常用于是日期规则分表，如：.UseTableByRange(1, DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="beginField2Value">字段2范围起始值</param>
    /// <param name="endField2Value">字段2范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> UseTableByRange(object field1Value, object beginField2Value, object endField2Value);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T15表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginField3Value &lt;= endField3Value，常用于是日期规则分表，如：.UseTableByRange(1, "Game1", DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="field2Value">字段2值</param>
    /// <param name="beginField3Value">字段3范围起始值</param>
    /// <param name="endField3Value">字段3范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value);
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> UseTableSchema(string tableSchema);
    #endregion

    #region WithTable
    /// <summary>
    /// 添加实体表，方便后面做JOIN关联，如：<code>.From&lt;Menu&gt;().WithTable&lt;Page&gt;()</code>
    /// </summary>
    /// <typeparam name="TOther">实体表类型</typeparam>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> WithTable<TOther>();
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> WithQuery<TOther>(IQuery<TOther> subQuery);
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> WithQuery<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr);
    #endregion

    #region Include
    /// <summary>
    /// 加载导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，1:1关联关系，随主表一起查询,支持无限级，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .From&lt;Product&gt;().Include(f =&gt; f.Brand) ...
    /// .From&lt;Brand&gt;().Include(f =&gt; f.Products) ...
    /// .From&lt;Order&gt;().Include(f =&gt; f.Seller.Company.Products) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TMember">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>> memberSelector);
    /// <summary>
    /// 加载集合类导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，可使用filter筛选满足条件的导航属性，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .From&lt;User&gt;().IncludeMany(f =&gt; f.Orders)  //与 .From&lt;User&gt;().Include(f =&gt; f.Orders) 等价
    /// .From&lt;User&gt;().IncludeMany(f =&gt; f.Orders, order =&gt; order.TotalAmout &gt; 500)
    /// </code>
    /// </summary>
    /// <typeparam name="TElement">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null);
    #endregion

    #region InnerJoin
    /// <summary>
    /// 在现有表中，选择两个表进行INNER JOIN关联，可多次关联，如：<code>.InnerJoin((a, b, ...) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做INNER JOIN关联，与.WithTable&lt;TOther&gt;().InnerJoin(...)等价，如：
    /// <code>
    /// .From&lt;User&gt;().InnerJoin&lt;Order&gt;((x, y) =&gt; x.Id == y.BuyerId)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> InnerJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr表达式构建子查询，并选择一个现有表做INNER JOIN关联，与.WithQuery(subQueryExpr).InnerJoin(...)等价，如：
    /// <code>
    /// .InnerJoin(f =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select(x =&gt; new { ... }), (a, b, ...) =&gt; x.xxx = y.yyy) ...
    /// SQL: ... INNER JOIN (SELECT ... FROM `sys_order_detail` ...) p ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> InnerJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn);
    #endregion

    #region LeftJoin
    /// <summary>
    /// 在现有表中，选择两个表进行LEFT JOIN关联，可多次关联，如：<code>.LeftJoin((a, b, ...) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做LEFT JOIN关联，与.WithTable&lt;TOther&gt;().LeftJoin(...)等价，如：
    /// <code>
    /// .LeftJoin&lt;TOther&gt;((a, b, ...) =&gt; a.xxx = b.yyy)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> LeftJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr表达式构建子查询，并选择一个现有表做LEFT JOIN关联，与.WithQuery(subQueryExpr).LeftJoin(...)等价，如：
    /// <code>
    /// .LeftJoin(f =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select(t =&gt; new { ... }), (a, b, ...) =&gt; a.Id = b.OrderId) ...
    /// SQL: ... LEFT JOIN (SELECT ... FROM `sys_order_detail` ...) p ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> LeftJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn);
    #endregion

    #region RightJoin
    /// <summary>
    /// 在现有表中，选择两个表进行RIGHT JOIN关联，可多次关联，如：<code>.RightJoin((a, b) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，并选择一个现有表做RIGHT JOIN关联，与.WithTable&lt;TOther&gt;().RightJoin(...)等价，如：
    /// <code>
    /// .RightJoin&lt;TOther&gt;((a, b, c, d, e, f, g, h, i, j, k, l, m, n, o) =&gt; ...)
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn);
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> RightJoin<TOther>(IQuery<TOther> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQueryExpr表达式构建子查询，并选择一个现有表做RIGHT JOIN关联，与.WithQuery(subQueryExpr).RightJoin(...)等价，如：
    /// <code>
    /// .RightJoin(f =&gt; f.From&lt;OrderDetail&gt;() ...
    ///     .Select(t =&gt; new { ... }), (a, b, ...) =&gt; a.Id = b.OrderId) ...
    /// SQL: ... RIGHT JOIN (SELECT ... FROM `sys_order_detail` ...) p ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQueryExpr">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther> RightJoin<TOther>(Expression<Func<IFromQuery, IQuery<TOther>>> subQueryExpr, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TOther, bool>> joinOn);
    #endregion

    #region Where
    /// <summary>
    /// 条件查询，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> predicate);
    /// <summary>
    /// 条件查询，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>>> predicateInitializer);
    #endregion

    #region And
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，并与已有的条件AND操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>>> predicateInitializer);
    #endregion

    #region Or
    /// <summary>
    /// 使用predicate表达式生成Or条件，并添加到已有的Where条件末尾，表达式predicate不可为null
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件OR操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，并与已有的条件OR操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>>> predicateInitializer);
    #endregion

    #region GroupBy
    /// <summary>
    /// 分组查询，分组表达式groupingExpr可以是单个或多个字段的匿名对象，如：
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
    /// <param name="groupingExpr">分组表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping>> groupingExpr);
    #endregion

    #region OrderBy
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// OrderBy((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderBy((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr);
    /// <summary>
    /// ASC排序，condition为true，ASC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderBy(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr);
    /// <summary>
    /// DSC排序，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// OrderByDescending((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，condition为true，DESC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr);
    /// <summary>
    /// 动态排序，使用OrderByBuilder构建排序字段选择器，fieldsGetter可以是单个或多个字段的匿名对象，如：
    /// <code>string orderFields = "Name";bool isAsc = true;
    /// .OrderByDynamic(t =&gt; t.Switch(orderFields, isAsc).When("Name", (a, b, ...) =&lt; a.Name).When("Gender", (a, b, ...) =&lt; a.Gender).Build()</code>)
    /// </summary>
    /// <param name="fieldsGetter">排序字段选择器，需调用Build方法，返回排序字段表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>, Expression> fieldsGetter);
    #endregion

    #region Skip/Take/Page
    /// <summary>
    /// 跳过offset条数据
    /// </summary>
    /// <param name="offset">要跳过的数据条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Skip(int offset);
    /// <summary>
    /// 只返回limit条数据
    /// </summary>
    /// <param name="limit">返回的数据条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Take(int limit);
    /// <summary>
    /// 分页查询，pageNumber从1开始
    /// </summary>
    /// <param name="pageNumber">第几页，从1开始，小于1时当作1处理</param>
    /// <param name="pageSize">每页显示条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Page(int pageNumber, int pageSize);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，如：
    /// Select((a, b, ...) =&gt; new { a.Id, a.Name, ... }) 或是 Select((a, b, ...) =&gt; x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定字段返回，只需要指定需要特殊处理的成员赋值即可，其他成员将从现有表的字段中按名称匹配赋值，多个同名字段时如果未特殊指定赋值，默认选取第一个表中的字段赋值。如：
    /// <code> .SelectTo&lt;TDto&gt;() 或是 .SelectTo((a, b) =&gt; new TDto{ b.Id }) //使用第二个表的Id字段作为Id成员</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型，通常是一个匿名类</typeparam>
    /// <param name="specialMemberSelector">特殊成员赋值表达式，通常是重名字段或是不存在的字段赋值</param>
    /// <returns>返回查询对象</returns>
    IQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TTarget>> specialMemberSelector = null);
    #endregion

    #region Count
    /// <summary>
    /// 返回某个字段的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15&gt;().Count((a, b, c, d, e, f, g, h, i, j, k, l, m, n, o) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的int类型数据条数</returns>
    int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15&gt;().CountAsync((a, b, c, d, e, f, g, h, i, j, k, l, m, n, o) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的int类型数据条数</returns>
    Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15&gt;().CountDistinct((a, b, c, d, e, f, g, h, i, j, k, l, m, n, o) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段去重后的int类型数据条数</returns>
    int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15&gt;().CountDistinctAsync((a, b, c, d, e, f, g, h, i, j, k, l, m, n, o) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段去重后的int类型数据条数</returns>
    Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15&gt;().LongCount((a, b, c, d, e, f, g, h, i, j, k, l, m, n, o) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的long类型数据条数</returns>
    long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15&gt;().LongCountAsync((a, b, c, d, e, f, g, h, i, j, k, l, m, n, o) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的long类型数据条数</returns>
    Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15&gt;().LongCountDistinct((a, b, c, d, e, f, g, h, i, j, k, l, m, n, o) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段去重后的long类型数据条数</returns>
    long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15&gt;().LongCountDistinctAsync((a, b, c, d, e, f, g, h, i, j, k, l, m, n, o) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段去重后的long类型数据条数</returns>
    Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr, CancellationToken cancellationToken = default);
    #endregion

    #region Aggregate
    /// <summary>
    /// 计算指定字段的求和值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15&gt;().Sum((a, b, c, d, e, f, g, h, i, j, k, l, m, n, o) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的求和值</returns>
    decimal Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的求和值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15&gt;().SumAsync((a, b, c, d, e, f, g, h, i, j, k, l, m, n, o) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的求和值</returns>
    Task<decimal> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的平均值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15&gt;().Avg((a, b, c, d, e, f, g, h, i, j, k, l, m, n, o) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的平均值</returns>
    TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的平均值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15&gt;().AvgAsync((a, b, c, d, e, f, g, h, i, j, k, l, m, n, o) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的平均值</returns>
    Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的最大值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15&gt;().Max((a, b, c, d, e, f, g, h, i, j, k, l, m, n, o) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的最大值</returns>
    TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最大值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15&gt;().MaxAsync((a, b, c, d, e, f, g, h, i, j, k, l, m, n, o) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的最大值</returns>
    Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的最小值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15&gt;().Min((a, b, c, d, e, f, g, h, i, j, k, l, m, n, o) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的最小值</returns>
    TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最小值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15&gt;().MinAsync((a, b, c, d, e, f, g, h, i, j, k, l, m, n, o) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的最小值</returns>
    Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr, CancellationToken cancellationToken = default);
    #endregion
}
/// <summary>
/// 多表T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16查询
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
/// <typeparam name="T5">表T5实体类型</typeparam>
/// <typeparam name="T6">表T6实体类型</typeparam>
/// <typeparam name="T7">表T7实体类型</typeparam>
/// <typeparam name="T8">表T8实体类型</typeparam>
/// <typeparam name="T9">表T9实体类型</typeparam>
/// <typeparam name="T10">表T10实体类型</typeparam>
/// <typeparam name="T11">表T11实体类型</typeparam>
/// <typeparam name="T12">表T12实体类型</typeparam>
/// <typeparam name="T13">表T13实体类型</typeparam>
/// <typeparam name="T14">表T14实体类型</typeparam>
/// <typeparam name="T15">表T15实体类型</typeparam>
/// <typeparam name="T16">表T16实体类型</typeparam>
public interface IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> : IQueryBase
{
    #region Sharding
    /// <summary>
    /// 直接指定T16表分表名，完整的表名，如：.UseTable("sys_order_202001")，.UseTable("sys_order_202001", "sys_order_202002")
    /// </summary>
    /// <param name="tableNames">多个表名，完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> UseTable(params string[] tableNames);
    /// <summary>
    /// 根据首个分表TMasterSharding表与当前T16表名的映射关系，指定当前T16表分表名获取委托，执行委托获取当前T16表分表名。委托第一个参数是TMasterSharding主表原始表名，第二个参数是当前T16表原始表名，第三个参数是TMasterSharding表当前分表名，返回值是当前T16表分表名称，如：
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
    /// <param name="tableNameGetter">T16表分表名获取委托</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> UseTableMap<TMasterSharding>(Func<string, string, string, string> tableNameGetter);
    /// <summary>
    /// 手动指定分表规则参数值，执行分表规则确定T16表分表名，可多次调用实现多个分表，参数值的顺序与配置的分表规则参数值顺序保持一致，最多支持3个字段值，不能为null，如：.UseTableBy(DateTime.Now)，.UseTableBy(1, 6, DateTime.Now)等
    /// </summary>
    /// <param name="fieldValues">字段值数组，不可为nul或空元素</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> UseTableBy(params object[] fieldValues);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T16表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginFieldValue &lt;= endFieldValue，如：.UseTableByRange(DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="beginFieldValue">字段范围起始值</param>
    /// <param name="endFieldValue">字段范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> UseTableByRange(object beginFieldValue, object endFieldValue);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T16表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginField2Value &lt;= endField2Value，常用于是日期规则分表，如：.UseTableByRange(1, DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="beginField2Value">字段2范围起始值</param>
    /// <param name="endField2Value">字段2范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> UseTableByRange(object field1Value, object beginField2Value, object endField2Value);
    /// <summary>
    /// 手动指定分表范围规则参数值，手动指定T16表分表名执行查询，参数值的顺序与配置的分表范围规则参数值顺序保持一致，确保beginField3Value &lt;= endField3Value，常用于是日期规则分表，如：.UseTableByRange(1, "Game1", DateTime.Now.AddDays(-7), DateTime.Now)
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="field2Value">字段2值</param>
    /// <param name="beginField3Value">字段3范围起始值</param>
    /// <param name="endField3Value">字段3范围结束值</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> UseTableByRange(object field1Value, object field2Value, object beginField3Value, object endField3Value);
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> UseTableSchema(string tableSchema);
    #endregion

    #region Include
    /// <summary>
    /// 加载导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，1:1关联关系，随主表一起查询,支持无限级，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .From&lt;Product&gt;().Include(f =&gt; f.Brand) ...
    /// .From&lt;Brand&gt;().Include(f =&gt; f.Products) ...
    /// .From&lt;Order&gt;().Include(f =&gt; f.Seller.Company.Products) ...
    /// </code>
    /// </summary>
    /// <typeparam name="TMember">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <returns>返回查询对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TMember>> memberSelector);
    /// <summary>
    /// 加载集合类导航属性，使用LEFT JOIN关联导航属性表，使用实体映射中的导航属性配置生成LEFT JOIN ... ON子句，可使用filter筛选满足条件的导航属性，1:N关联关系，分两次查询，第二次查询返回结果，只支持1级。
    /// <code>
    /// .From&lt;User&gt;().IncludeMany(f =&gt; f.Orders)  //与 .From&lt;User&gt;().Include(f =&gt; f.Orders) 等价
    /// .From&lt;User&gt;().IncludeMany(f =&gt; f.Orders, order =&gt; order.TotalAmout &gt; 500)
    /// </code>
    /// </summary>
    /// <typeparam name="TElement">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TElement> IncludeMany<TElement>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, IEnumerable<TElement>>> memberSelector, Expression<Func<TElement, bool>> filter = null);
    #endregion

    #region InnerJoin
    /// <summary>
    /// 在现有表中，选择两个表进行INNER JOIN关联，可多次关联，如：<code>.InnerJoin((a, b, ...) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> joinOn);
    #endregion

    #region LeftJoin
    /// <summary>
    /// 在现有表中，选择两个表进行LEFT JOIN关联，可多次关联，如：<code>.LeftJoin((a, b, ...) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> joinOn);
    #endregion

    #region RightJoin
    /// <summary>
    /// 在现有表中，选择两个表进行RIGHT JOIN关联，可多次关联，如：<code>.RightJoin((a, b) =&gt; a.xxx = b.yyy)</code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> joinOn);
    #endregion

    #region Where
    /// <summary>
    /// 条件查询，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> predicate);
    /// <summary>
    /// 条件查询，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> WherePredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>>> predicateInitializer);
    #endregion

    #region And
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，predicate为null时不生成任何条件
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件AND操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，并与已有的条件AND操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> AndPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>>> predicateInitializer);
    #endregion

    #region Or
    /// <summary>
    /// 使用predicate表达式生成Or条件，并添加到已有的Where条件末尾，表达式predicate不可为null
    /// </summary>
    /// <param name="predicate">条件表达式，predicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Or(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> predicate);
    /// <summary>
    /// 条件查询，并与已有的条件OR操作，condition为true，ifPredicate条件生效，否则elsePredicate条件生效，elsePredicate可为null    
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，elsePredicate为null时不生成任何条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Or(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>> elsePredicate = null);
    /// <summary>
    /// 构造表达式断言predicateInitializer生成Where条件，并与已有的条件OR操作，predicateInitializer不可为null
    /// </summary>
    /// <param name="predicateInitializer">表达式断言predicateInitializer构造器，predicateInitializer不可为null</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> OrPredicate(Func<PredicateBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, bool>>> predicateInitializer);
    #endregion

    #region GroupBy
    /// <summary>
    /// 分组查询，分组表达式groupingExpr可以是单个或多个字段的匿名对象，如：
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
    /// <param name="groupingExpr">分组表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TGrouping>> groupingExpr);
    #endregion

    #region OrderBy
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// OrderBy((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderBy((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TFields>> fieldsExpr);
    /// <summary>
    /// ASC排序，condition为true，ASC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderBy(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderBy(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TFields>> fieldsExpr);
    /// <summary>
    /// DSC排序，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// OrderByDescending((a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 OrderByDescending((a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TFields>> fieldsExpr);
    /// <summary>
    /// DESC排序，condition为true，DESC排序生效，fieldsExpr可以是单个或多个字段的匿名对象，不可为null，如：
    /// .OrderByDescending(true, (a, b, ...) =&gt; new { a.Id, b.Id, ... }) 或是 .OrderByDescending(true, (a, b, ...) =&gt; a.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TFields>> fieldsExpr);
    /// <summary>
    /// 动态排序，使用OrderByBuilder构建排序字段选择器，fieldsGetter可以是单个或多个字段的匿名对象，如：
    /// <code>string orderFields = "Name";bool isAsc = true;
    /// .OrderByDynamic(t =&gt; t.Switch(orderFields, isAsc).When("Name", (a, b, ...) =&lt; a.Name).When("Gender", (a, b, ...) =&lt; a.Gender).Build()</code>)
    /// </summary>
    /// <param name="fieldsGetter">排序字段选择器，需调用Build方法，返回排序字段表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> OrderByDynamic(Func<OrderByBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>, Expression> fieldsGetter);
    #endregion

    #region Skip/Take/Page
    /// <summary>
    /// 跳过offset条数据
    /// </summary>
    /// <param name="offset">要跳过的数据条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Skip(int offset);
    /// <summary>
    /// 只返回limit条数据
    /// </summary>
    /// <param name="limit">返回的数据条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Take(int limit);
    /// <summary>
    /// 分页查询，pageNumber从1开始
    /// </summary>
    /// <param name="pageNumber">第几页，从1开始，小于1时当作1处理</param>
    /// <param name="pageSize">每页显示条数</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> Page(int pageNumber, int pageSize);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，如：
    /// Select((a, b, ...) =&gt; new { a.Id, a.Name, ... }) 或是 Select((a, b, ...) =&gt; x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定字段返回，只需要指定需要特殊处理的成员赋值即可，其他成员将从现有表的字段中按名称匹配赋值，多个同名字段时如果未特殊指定赋值，默认选取第一个表中的字段赋值。如：
    /// <code> .SelectTo&lt;TDto&gt;() 或是 .SelectTo((a, b) =&gt; new TDto{ b.Id }) //使用第二个表的Id字段作为Id成员</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型，通常是一个匿名类</typeparam>
    /// <param name="specialMemberSelector">特殊成员赋值表达式，通常是重名字段或是不存在的字段赋值</param>
    /// <returns>返回查询对象</returns>
    IQuery<TTarget> SelectTo<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TTarget>> specialMemberSelector = null);
    #endregion

    #region Count
    /// <summary>
    /// 返回某个字段的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16&gt;().Count((a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的int类型数据条数</returns>
    int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16&gt;().CountAsync((a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的int类型数据条数</returns>
    Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16&gt;().CountDistinct((a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段去重后的int类型数据条数</returns>
    int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16&gt;().CountDistinctAsync((a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段去重后的int类型数据条数</returns>
    Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16&gt;().LongCount((a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的long类型数据条数</returns>
    long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16&gt;().LongCountAsync((a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的long类型数据条数</returns>
    Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16&gt;().LongCountDistinct((a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段去重后的long类型数据条数</returns>
    long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，如：
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16&gt;().LongCountDistinctAsync((a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p) =&gt; a.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段去重后的long类型数据条数</returns>
    Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr, CancellationToken cancellationToken = default);
    #endregion

    #region Aggregate
    /// <summary>
    /// 计算指定字段的求和值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16&gt;().Sum((a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的求和值</returns>
    decimal Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的求和值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16&gt;().SumAsync((a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的求和值</returns>
    Task<decimal> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的平均值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16&gt;().Avg((a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的平均值</returns>
    TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的平均值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16&gt;().AvgAsync((a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的平均值</returns>
    Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的最大值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16&gt;().Max((a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的最大值</returns>
    TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最大值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16&gt;().MaxAsync((a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的最大值</returns>
    Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr, CancellationToken cancellationToken = default);
    /// <summary>
    /// 计算指定字段的最小值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16&gt;().Min((a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回该字段的最小值</returns>
    TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最小值
    /// <code>.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16&gt;().MinAsync((a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p) =&gt; a.TotalAmount);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回该字段的最小值</returns>
    Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TField>> fieldExpr, CancellationToken cancellationToken = default);
    #endregion
}