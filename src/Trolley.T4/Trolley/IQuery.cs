using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public interface IQuery<T>
{
    #region Union
    /// <summary>
    /// Union操作，用法：
    /// <code>
    /// await repository.From&lt;Order&gt;()
    ///     .Where(x => x.Id == 1)
    ///     .Select(x => new
    ///     {
    ///         x.Id,
    ///         x.OrderNo,
    ///         x.SellerId,
    ///         x.BuyerId
    ///      })
    ///     .Union(f => f.From&lt;Order&gt;()
    ///         .Where(x => x.Id > 1)
    ///         .Select(x => new
    ///         {
    ///             x.Id,
    ///             x.OrderNo,
    ///             x.SellerId,
    ///             x.BuyerId
    ///         }))
    ///     .ToListAsync();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT `Id`,`OrderNo`,`SellerId`,`BuyerId` FROM `sys_order` WHERE `Id`=1 UNION
    /// SELECT `Id`,`OrderNo`,`SellerId`,`BuyerId` FROM `sys_order` WHERE `Id`&gt;1
    /// </code>
    /// </summary>
    /// <param name="subQuery">子查询，需要有Select语句，如：
    /// <code>
    /// f.From&lt;Order&gt;()
    ///     .Where(x => x.Id > 1)
    ///     .Select(x => new
    ///     {
    ///         x.Id,
    ///         x.OrderNo,
    ///         x.SellerId,
    ///         x.BuyerId
    ///     }
    /// </code>
    /// </param>
    /// <returns>返回查询对象</returns>
    IQuery<T> Union(Func<IFromQuery, IFromQuery<T>> subQuery);
    /// <summary>
    /// Union操作，用法：
    /// <code>
    /// repository.From&lt;Order&gt;()
    ///     .Where(x =&gt; x.Id == 1)
    ///     .Select(x =&gt; new
    ///     {
    ///         x.Id,
    ///         x.OrderNo,
    ///         x.SellerId,
    ///         x.BuyerId
    ///     })
    ///     .UnionAll(f =&gt; f
    ///         .From&lt;Order&gt;()
    ///         .Where(x =&gt; x.Id &gt; 1)
    ///         .Select(x =&gt; new
    ///         {
    ///             x.Id,
    ///             x.OrderNo,
    ///             x.SellerId,
    ///             x.BuyerId
    ///         }))
    ///     .ToListAsync();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT `Id`,`OrderNo`,`SellerId`,`BuyerId` FROM `sys_order` WHERE `Id`=1 UNION ALL
    /// SELECT `Id`,`OrderNo`,`SellerId`,`BuyerId` FROM `sys_order` WHERE `Id`&gt;1
    /// </code>
    /// </summary>
    /// <param name="subQuery">子查询，需要有Select语句，如：
    ///  f.From&lt;Order&gt;()
    ///     .Where(x =&gt; x.Id &gt; 1)
    ///     .Select(x =&gt; new
    ///     {
    ///         x.Id,
    ///         x.OrderNo,
    ///         x.SellerId,
    ///         x.BuyerId
    ///     }
    /// </param>
    /// <returns>返回查询对象</returns>
    IQuery<T> UnionAll(Func<IFromQuery, IFromQuery<T>> subQuery);
    #endregion

    #region WithCte
    /// <summary>
    /// CTE With子句，在Select之前，With子句要连续定义，用法：
    /// <code>
    /// repository
    ///     .FromWith((f =&gt; ...), "MenuList")
    ///     .NextWith(f => f.From&lt;Page&gt;()
    ///         .InnerJoin&lt;Menu&gt;((a, b) =&gt; a.Id == b.PageId)
    ///         .Where((a, b) =&gt; a.Id == 1)
    ///         .Select((x, y) =&gt; new { y.Id, x.Url })), "MenuPageList")
    ///     .InnerJoin((a, b) =&gt; a.Id == b.Id)
    ///     .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH MenuList(Id,Name,ParentId) AS 
    /// (
    ///     SELECT `Id`,`Name`,`ParentId` FROM `sys_menu` WHERE `Id`=1
    /// ),
    /// MenuPageList(Id,Url) AS
    /// (
    ///     SELECT b.`Id`, a.`Url` FROM `sys_page` a INNER JOIN `sys_menu` b ON a.`Id`=b.`PageId` WHERE a.`Id`=1
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,b.`Url` FROM MenuList a INNER JOIN MenuPageList b ON a.`Id`=b.`Id`
    /// </code>
    /// </summary>
    /// <typeparam name="TTarget">当前CTE临时返回的实体类型</typeparam>
    /// <param name="cteSubQuery">CTE子查询，如：
    /// <code>
    /// f.From&lt;Page&gt;()
    ///     .InnerJoin&lt;Menu&gt;((a, b) =&gt; a.Id == b.PageId)
    ///     .Where((a, b) =&gt; a.Id == 1)
    ///     .Select((x, y) =&gt; new { y.Id, x.Url })
    /// </code>
    /// </param>
    /// <param name="cteTableName">CTE表名</param>
    /// <param name="tableAsStart">CTE子句中使用的表的别名开始字母，默认从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IQuery<T, TTarget> NextWith<TTarget>(Func<IFromQuery, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    /// <summary>
    /// 递归CTE With子句，在Select之前，With子句要连续定义，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId })), "MenuList")
    ///     .NextWithRecursive((f, cte) =&gt; f.From&lt;Page, Menu&gt;()
    ///             .Where((a, b) =&gt; a.Id == b.PageId)
    ///             .Select((x, y) =&gt; new { y.Id, y.ParentId, x.Url })
    ///         .UnionAll(x =&gt; x.From&lt;Menu&gt;()
    ///             .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///             .Where((a, b) =&gt; a.Id &gt; 1)
    ///             .Select((x, y) =&gt; new { x.Id, x.ParentId, y.Url })), "MenuPageList")
    ///     .InnerJoin((a, b) =&gt; a.Id == b.Id)
    ///     .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, b.Url })
    ///     .ToList();
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId) AS 
    /// (
    ///     SELECT `Id`,`Name`,`ParentId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    ///     SELECT a.`Id`, a.`Name`, a.`ParentId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// ),
    /// MenuPageList(Id,ParentId,Url) AS
    /// (
    ///     SELECT a.`Id`,a.`ParentId`,b.`Url` FROM `sys_menu` a,`sys_page` b WHERE a.`PageId`=b.Id UNION ALL
    ///     SELECT a.`Id`,a.`ParentId`,b.`Url` FROM `sys_menu` a INNER JOIN MenuList b ON a.`Id`=b.`ParentId`
    /// ),
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,b.`Url` FROM MenuList a INNER JOIN MenuPageList b ON a.`Id`=b.`Id`
    /// </code>
    /// </summary>
    /// <typeparam name="TTarget">当前CTE With子句临时返回的实体类型</typeparam>
    /// <param name="cteSubQuery">CTE子查询，如：
    /// <code>
    /// f.From&lt;Page&gt;()
    ///     .InnerJoin&lt;Menu&gt;((a, b) =&gt; a.Id == b.PageId)
    ///     .Where((a, b) =&gt; a.Id == 1)
    ///     .Select((x, y) =&gt; new { y.Id, x.Url })
    /// </code>
    /// </param>
    /// <param name="cteTableName">CTE表名</param>
    /// <param name="tableAsStart">CTE子句中使用的表的别名开始字母，默认从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IQuery<T, TTarget> NextWithRecursive<TTarget>(Func<IFromQuery, string, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    #endregion

    #region WithTable
    /// <summary>
    /// SQL子查询临时表，用法：
    /// <code>
    /// repository
    ///     .From&lt;Menu&gt;()
    ///     .WithTable(f => f.From&lt;Page, Menu&gt;('c')
    ///         .Where((a, b) => a.Id == b.PageId)
    ///         .Select((x, y) => new { y.Id, y.ParentId, x.Url }))
    ///     .Where((a, b) => a.Id == b.Id)
    ///     .Select((a, b) => new { a.Id, a.Name, a.ParentId, b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,b.`Url` FROM `sys_menu` a,(SELECT d.`Id`,d.`ParentId`,c.`Url` FROM `sys_page` c,`sys_menu` d WHERE c.`Id`=d.`PageId`) b WHERE a.`Id`=b.`Id`
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">当前临时表子句返回的实体类型</typeparam>
    /// <param name="subQuery"></param>
    /// <returns></returns>
    IQuery<T, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    #endregion

    #region Join
    /// <summary>
    /// 添加TOther表INNER JOIN关联，用法:
    /// <code>
    /// repository.From&lt;User&gt;()
    ///     .InnerJoin&lt;Orderr&gt;((x, y) =&gt; x.Id == y.BuyerId)
    ///     .Where((a, b) =r&gt; b.ProductCount &gt; 1)
    ///     .Select((x, y) =r&gt; new
    ///     {
    ///         User = x,
    ///         Order = y
    ///     })
    ///     .ToList();
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// SELECT a.`Id`,a.`Name`,a.`Gender`,a.`Age`,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt`,b.`Id`,b.`OrderNo`,b.`ProductCount`,b.`TotalAmount`,b.`BuyerId`,b.`SellerId`,b.`Products`,b.`IsEnabled`,b.`CreatedBy`,b.`CreatedAt`,b.`UpdatedBy`,b.`UpdatedAt` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` WHERE b.`ProductCount`&gt;1
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">实体类型</typeparam>
    /// <param name="joinOn">INNER JOIN关联条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T, TOther> InnerJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn);
    /// <summary>
    /// 添加TOther表LEFT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;User&gt;()
    ///     .LeftJoin&lt;Orderr&gt;((x, y) =&gt; x.Id == y.BuyerId)
    ///     .Where((a, b) =r&gt; b.ProductCount &gt; 1)
    ///     .Select((x, y) =r&gt; new
    ///     {
    ///         User = x,
    ///         Order = y
    ///     })
    ///     .ToList();
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// SELECT a.`Id`,a.`Name`,a.`Gender`,a.`Age`,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt`,b.`Id`,b.`OrderNo`,b.`ProductCount`,b.`TotalAmount`,b.`BuyerId`,b.`SellerId`,b.`Products`,b.`IsEnabled`,b.`CreatedBy`,b.`CreatedAt`,b.`UpdatedBy`,b.`UpdatedAt` FROM `sys_user` a LEFT JOIN `sys_order` b ON a.`Id`=b.`BuyerId` WHERE b.`ProductCount`&gt;1
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">实体类型</typeparam>
    /// <param name="joinOn">LEFT JOIN关联条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T, TOther> LeftJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn);
    /// <summary>
    /// 添加TOther表RIGHT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;User&gt;()
    ///     .RightJoin&lt;Orderr&gt;((x, y) =&gt; x.Id == y.BuyerId)
    ///     .Where((a, b) =r&gt; b.ProductCount &gt; 1)
    ///     .Select((x, y) =r&gt; new
    ///     {
    ///         User = x,
    ///         Order = y
    ///     })
    ///     .ToList();
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// SELECT a.`Id`,a.`Name`,a.`Gender`,a.`Age`,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt`,b.`Id`,b.`OrderNo`,b.`ProductCount`,b.`TotalAmount`,b.`BuyerId`,b.`SellerId`,b.`Products`,b.`IsEnabled`,b.`CreatedBy`,b.`CreatedAt`,b.`UpdatedBy`,b.`UpdatedAt` FROM `sys_user` a RIGHT JOIN `sys_order` b ON a.`Id`=b.`BuyerId` WHERE b.`ProductCount`&gt;1
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">实体类型</typeparam>
    /// <param name="joinOn">RIGHT JOIN关联条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T, TOther> RightJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn);
    /// <summary>
    /// 添加子查询作为临时表INNER JOIN关联，用法:
    /// <code>
    /// await repository.From&lt;User&gt;()
    ///     .InnerJoin&lt;Order&gt;((x, y) =&gt; x.Id == y.BuyerId)
    ///     .InnerJoin(f =&gt; f.From&lt;OrderDetail&gt;()
    ///         .GroupBy(x =&gt; x.OrderId)
    ///         .Select((x, y) =&gt; new
    ///         {
    ///             y.OrderId,
    ///             ProductCount = x.CountDistinct(y.ProductId)
    ///         }), (a, b, c) =&gt; b.Id == c.OrderId)
    ///     .Where((a, b, c) =&gt; c.ProductCount &gt; 2)
    ///     .Select((a, b, c) =&gt; new
    ///     {
    ///         User = a,
    ///         Order = b,
    ///         c.ProductCount
    ///     })
    ///     .ToListAsync();
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// SELECT a.`Id`,a.`Name`,a.`Gender`,a.`Age`,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt`,b.`Id`,b.`OrderNo`,b.`ProductCount`,b.`TotalAmount`,b.`BuyerId`,b.`SellerId`,b.`Products`,b.`IsEnabled`,b.`CreatedBy`,b.`CreatedAt`,b.`UpdatedBy`,b.`UpdatedAt`,c.`ProductCount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` INNER JOIN (SELECT a.`OrderId`,COUNT(DISTINCT a.`ProductId`) AS ProductCount FROM `sys_order_detail` a GROUP BY a.`OrderId`) c ON b.`Id`=c.`OrderId` WHERE c.`ProductCount`&gt;2
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">实体类型，子查询中通常会有SELECT操作，返回的<paramref name="TOther"/>类型是一个匿名的</typeparam>
    /// <param name="joinOn">INNER JOIN关联条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T, TOther> InnerJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T, TOther, bool>> joinOn);
    /// <summary>
    /// 添加子查询作为临时表LEFT JOIN关联，用法:
    /// <code>
    /// await repository.From&lt;User&gt;()
    ///     .InnerJoin&lt;Order&gt;((x, y) =&gt; x.Id == y.BuyerId)
    ///     .LeftJoin(f =&gt; f.From&lt;OrderDetail&gt;()
    ///         .GroupBy(x =&gt; x.OrderId)
    ///         .Select((x, y) =&gt; new
    ///         {
    ///             y.OrderId,
    ///             ProductCount = x.CountDistinct(y.ProductId)
    ///         }), (a, b, c) =&gt; b.Id == c.OrderId)
    ///     .Where((a, b, c) =&gt; c.ProductCount &gt; 2)
    ///     .Select((a, b, c) =&gt; new
    ///     {
    ///         User = a,
    ///         Order = b,
    ///         c.ProductCount
    ///     })
    ///     .ToListAsync();
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// SELECT a.`Id`,a.`Name`,a.`Gender`,a.`Age`,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt`,b.`Id`,b.`OrderNo`,b.`ProductCount`,b.`TotalAmount`,b.`BuyerId`,b.`SellerId`,b.`Products`,b.`IsEnabled`,b.`CreatedBy`,b.`CreatedAt`,b.`UpdatedBy`,b.`UpdatedAt`,c.`ProductCount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` LEFT JOIN (SELECT a.`OrderId`,COUNT(DISTINCT a.`ProductId`) AS ProductCount FROM `sys_order_detail` a GROUP BY a.`OrderId`) c ON b.`Id`=c.`OrderId` WHERE c.`ProductCount`&gt;2
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">实体类型，子查询中通常会有SELECT操作，返回的<paramref name="TOther"/>类型是一个匿名的</typeparam>
    /// <param name="joinOn">LEFT JOIN关联条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T, TOther> LeftJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T, TOther, bool>> joinOn);
    /// <summary>
    /// 添加子查询作为临时表RIGHT JOIN关联，用法:
    /// <code>
    /// await repository.From&lt;User&gt;()
    ///     .InnerJoin&lt;Order&gt;((x, y) =&gt; x.Id == y.BuyerId)
    ///     .RightJoin(f =&gt; f.From&lt;OrderDetail&gt;()
    ///         .GroupBy(x =&gt; x.OrderId)
    ///         .Select((x, y) =&gt; new
    ///         {
    ///             y.OrderId,
    ///             ProductCount = x.CountDistinct(y.ProductId)
    ///         }), (a, b, c) =&gt; b.Id == c.OrderId)
    ///     .Where((a, b, c) =&gt; c.ProductCount &gt; 2)
    ///     .Select((a, b, c) =&gt; new
    ///     {
    ///         User = a,
    ///         Order = b,
    ///         c.ProductCount
    ///     })
    ///     .ToListAsync();
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// SELECT a.`Id`,a.`Name`,a.`Gender`,a.`Age`,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt`,b.`Id`,b.`OrderNo`,b.`ProductCount`,b.`TotalAmount`,b.`BuyerId`,b.`SellerId`,b.`Products`,b.`IsEnabled`,b.`CreatedBy`,b.`CreatedAt`,b.`UpdatedBy`,b.`UpdatedAt`,c.`ProductCount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` RIGHT JOIN (SELECT a.`OrderId`,COUNT(DISTINCT a.`ProductId`) AS ProductCount FROM `sys_order_detail` a GROUP BY a.`OrderId`) c ON b.`Id`=c.`OrderId` WHERE c.`ProductCount`&gt;2
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">实体类型，子查询中通常会有SELECT操作，返回的<paramref name="TOther"/>类型是一个匿名的</typeparam>
    /// <param name="joinOn">RIGHT JOIN关联条件</param>
    /// <returns>返回查询对象</returns>
    IQuery<T, TOther> RightJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T, TOther, bool>> joinOn);
    #endregion

    #region Include
    /// <summary>
    /// 单表查询，包含1:1关联方式的导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系。
    /// 可以通过使用InnerJoin(Expression&lt;Func&lt;T, bool&gt;&gt; joinOn)、RightJoin(Expression&lt;Func&lt;T, bool&gt;&gt; joinOn)方法改变默认关联方式
    /// </summary>
    /// <typeparam name="TMember">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IIncludableQuery<T, TMember> Include<TMember>(Expression<Func<T, TMember>> memberSelector);
    /// <summary>
    /// 单表查询，包含1:N关联方式的导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系。
    /// 可以通过使用InnerJoin(Expression&lt;Func&lt;T, bool&gt;&gt; joinOn)、RightJoin(Expression&lt;Func&lt;T, bool&gt;&gt; joinOn)方法改变默认关联方式
    /// </summary>
    /// <typeparam name="TElment">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IIncludableQuery<T, TElment> IncludeMany<TElment>(Expression<Func<T, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null);
    #endregion

    #region Where
    IQuery<T> Where(Expression<Func<T, bool>> predicate = null);
    IQuery<T> Where(bool condition, Expression<Func<T, bool>> predicate = null);
    IQuery<T> And(bool condition, Expression<Func<T, bool>> predicate = null);
    #endregion

    #region GroupBy/OrderBy
    IGroupingQuery<T, TGrouping> GroupBy<TGrouping>(Expression<Func<T, TGrouping>> groupingExpr);
    IQuery<T> OrderBy(string rawSql);
    IQuery<T> OrderBy<TFields>(Expression<Func<T, TFields>> fieldsExpr);
    IQuery<T> OrderByDescending<TFields>(Expression<Func<T, TFields>> fieldsExpr);
    #endregion

    IQuery<T> Distinct();
    IQuery<T> Skip(int offset);
    IQuery<T> Take(int limit);
    IQuery<T> Page(int pageIndex, int pageSize);
    IQuery<T> ToChunk(int size);

    IQuery<T> Select();
    IQuery<TTarget> Select<TTarget>(Expression<Func<T, TTarget>> fieldsExpr);
    IQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T, TTarget>> fieldsExpr);

    int Count();
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    long LongCount();
    Task<long> LongCountAsync(CancellationToken cancellationToken = default);
    int Count<TField>(Expression<Func<T, TField>> fieldExpr);
    Task<int> CountAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default);
    int CountDistinct<TField>(Expression<Func<T, TField>> fieldExpr);
    Task<int> CountDistinctAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default);
    long LongCount<TField>(Expression<Func<T, TField>> fieldExpr);
    Task<long> LongCountAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default);
    long LongCountDistinct<TField>(Expression<Func<T, TField>> fieldExpr);
    Task<long> LongCountDistinctAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default);

    TField Sum<TField>(Expression<Func<T, TField>> fieldExpr);
    Task<TField> SumAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default);
    TField Avg<TField>(Expression<Func<T, TField>> fieldExpr);
    Task<TField> AvgAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default);
    TField Max<TField>(Expression<Func<T, TField>> fieldExpr);
    Task<TField> MaxAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default);
    TField Min<TField>(Expression<Func<T, TField>> fieldExpr);
    Task<TField> MinAsync<TField>(Expression<Func<T, TField>> fieldExpr, CancellationToken cancellationToken = default);
    
    T First();
    Task<T> FirstAsync(CancellationToken cancellationToken = default);
    TTarget First<TTarget>(Expression<Func<T, TTarget>> toTargetExpr);
    Task<TTarget> FirstAsync<TTarget>(Expression<Func<T, TTarget>> toTargetExpr, CancellationToken cancellationToken = default);
    List<T> ToList();
    Task<List<T>> ToListAsync(CancellationToken cancellationToken = default);
    IPagedList<T> ToPageList(int pageIndex, int pageSize);
    Task<IPagedList<T>> ToPageListAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default);
    Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(Func<T, TKey> keySelector, Func<T, TValue> valueSelector) where TKey : notnull;
    Task<Dictionary<TKey, TValue>> ToDictionaryAsync<TKey, TValue>(Func<T, TKey> keySelector, Func<T, TValue> valueSelector, CancellationToken cancellationToken = default) where TKey : notnull;
    
    string ToSql(out List<IDbDataParameter> dbParameters);
}
public interface IQuery<T1, T2>
{
    #region WithCte
    /// <summary>
    /// CTE With子句，在Select之前，With子句要连续定义，用法：
    /// <code>
    /// repository
    ///     .FromWith(f =&gt; ..., "Cte1")
    ///     ... ...
    ///     .NextWith(f =&gt; ..., "CteN")
    ///     .InnerJoin((T1, T2) =&gt; a.Id == b.Id)
    ///     .InnerJoin((T1, T2) =&gt; a.Id == c.Id)
    ///     ... ...
    ///     .Select((T1, T2) =&gt; new { ... ... })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH Cte1(...) AS 
    /// (
    ///     SELECT ... FROM ... WHERE ...
    /// ),
    /// ... ...
    /// CteN(...) AS
    /// (
    ///     SELECT ... FROM ... WHERE ... UNION ALL ...
    /// )
    /// SELECT ... FROM Cte1 a INNER JOIN Cte2 b ON xxx ... LEFT JOIN CteN ON ... ... WHERE ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TTarget">当前CTE临时返回的实体类型</typeparam>
    /// <param name="cteSubQuery">CTE子查询，如：
    /// <code>
    /// f.From&lt;Page&gt;()
    ///     .InnerJoin&lt;Menu&gt;((a, b) =&gt; a.Id == b.PageId)
    ///     .Where((a, b) =&gt; a.Id == 1)
    ///     .Select((x, y) =&gt; new { y.Id, x.Url })
    /// </code>
    /// </param>
    /// <param name="cteTableName">CTE表名</param>
    /// <param name="tableAsStart">CTE子句中使用的表的别名开始字母，默认从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, TTarget> NextWith<TTarget>(Func<IFromQuery, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    IQuery<T1, T2, TTarget> NextWithRecursive<TTarget>(Func<IFromQuery, string, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    #endregion

    #region WithTable
    IQuery<T1, T2, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    #endregion

    #region Include
    IIncludableQuery<T1, T2, TMember> Include<TMember>(Expression<Func<T1, T2, TMember>> memberSelector);
    IIncludableQuery<T1, T2, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null);
    #endregion

    #region Join
    IQuery<T1, T2> InnerJoin(Expression<Func<T1, T2, bool>> joinOn);
    IQuery<T1, T2> LeftJoin(Expression<Func<T1, T2, bool>> joinOn);
    IQuery<T1, T2> RightJoin(Expression<Func<T1, T2, bool>> joinOn);
    IQuery<T1, T2, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn);
    IQuery<T1, T2, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn);
    IQuery<T1, T2, TOther> RightJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn);
    IQuery<T1, T2, TOther> InnerJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn);
    IQuery<T1, T2, TOther> LeftJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn);
    IQuery<T1, T2, TOther> RightJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn);
    #endregion

    IQuery<T1, T2> Where(Expression<Func<T1, T2, bool>> predicate);
    IQuery<T1, T2> Where(bool condition, Expression<Func<T1, T2, bool>> predicate = null);
    IQuery<T1, T2> And(bool condition, Expression<Func<T1, T2, bool>> predicate);
    
    IGroupingQuery<T1, T2, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, TGrouping>> groupingExpr);
    IQuery<T1, T2> OrderBy<TFields>(Expression<Func<T1, T2, TFields>> fieldsExpr);
    IQuery<T1, T2> OrderByDescending<TFields>(Expression<Func<T1, T2, TFields>> fieldsExpr);

    IQuery<T1, T2> Skip(int offset);
    IQuery<T1, T2> Take(int limit);
    IQuery<T1, T2> ToChunk(int size);

    IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, TTarget>> fieldsExpr);
    IQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, TTarget>> fieldsExpr);

    int Count();
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    long LongCount();
    Task<long> LongCountAsync(CancellationToken cancellationToken = default);
    int Count<TField>(Expression<Func<T1, T2, TField>> fieldExpr);
    Task<int> CountAsync<TField>(Expression<Func<T1, T2, TField>> fieldExpr, CancellationToken cancellationToken = default);
    int CountDistinct<TField>(Expression<Func<T1, T2, TField>> fieldExpr);
    Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, TField>> fieldExpr, CancellationToken cancellationToken = default);
    long LongCount<TField>(Expression<Func<T1, T2, TField>> fieldExpr);
    Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, TField>> fieldExpr, CancellationToken cancellationToken = default);
    long LongCountDistinct<TField>(Expression<Func<T1, T2, TField>> fieldExpr);
    Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, TField>> fieldExpr, CancellationToken cancellationToken = default);

    TField Sum<TField>(Expression<Func<T1, T2, TField>> fieldExpr);
    Task<TField> SumAsync<TField>(Expression<Func<T1, T2, TField>> fieldExpr, CancellationToken cancellationToken = default);
    TField Avg<TField>(Expression<Func<T1, T2, TField>> fieldExpr);
    Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, TField>> fieldExpr, CancellationToken cancellationToken = default);
    TField Max<TField>(Expression<Func<T1, T2, TField>> fieldExpr);
    Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, TField>> fieldExpr, CancellationToken cancellationToken = default);
    TField Min<TField>(Expression<Func<T1, T2, TField>> fieldExpr);
    Task<TField> MinAsync<TField>(Expression<Func<T1, T2, TField>> fieldExpr, CancellationToken cancellationToken = default);
    
    string ToSql(out List<IDbDataParameter> dbParameters);
}
public interface IQuery<T1, T2, T3>
{
    #region WithCte
    /// <summary>
    /// CTE With子句，在Select之前，With子句要连续定义，用法：
    /// <code>
    /// repository
    ///     .FromWith(f =&gt; ..., "Cte1")
    ///     ... ...
    ///     .NextWith(f =&gt; ..., "CteN")
    ///     .InnerJoin((T1, T2, T3) =&gt; a.Id == b.Id)
    ///     .InnerJoin((T1, T2, T3) =&gt; a.Id == c.Id)
    ///     ... ...
    ///     .Select((T1, T2, T3) =&gt; new { ... ... })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH Cte1(...) AS 
    /// (
    ///     SELECT ... FROM ... WHERE ...
    /// ),
    /// ... ...
    /// CteN(...) AS
    /// (
    ///     SELECT ... FROM ... WHERE ... UNION ALL ...
    /// )
    /// SELECT ... FROM Cte1 a INNER JOIN Cte2 b ON xxx ... LEFT JOIN CteN ON ... ... WHERE ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TTarget">当前CTE临时返回的实体类型</typeparam>
    /// <param name="cteSubQuery">CTE子查询，如：
    /// <code>
    /// f.From&lt;Page&gt;()
    ///     .InnerJoin&lt;Menu&gt;((a, b) =&gt; a.Id == b.PageId)
    ///     .Where((a, b) =&gt; a.Id == 1)
    ///     .Select((x, y) =&gt; new { y.Id, x.Url })
    /// </code>
    /// </param>
    /// <param name="cteTableName">CTE表名</param>
    /// <param name="tableAsStart">CTE子句中使用的表的别名开始字母，默认从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, TTarget> NextWith<TTarget>(Func<IFromQuery, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    IQuery<T1, T2, T3, TTarget> NextWithRecursive<TTarget>(Func<IFromQuery, string, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    #endregion

    #region WithTable
    IQuery<T1, T2, T3, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    #endregion

    #region Include
    IIncludableQuery<T1, T2, T3, TMember> Include<TMember>(Expression<Func<T1, T2, T3, TMember>> memberSelector);
    IIncludableQuery<T1, T2, T3, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null);
    #endregion

    #region Join
    IQuery<T1, T2, T3> InnerJoin(Expression<Func<T1, T2, T3, bool>> joinOn);
    IQuery<T1, T2, T3> LeftJoin(Expression<Func<T1, T2, T3, bool>> joinOn);
    IQuery<T1, T2, T3> RightJoin(Expression<Func<T1, T2, T3, bool>> joinOn);
    IQuery<T1, T2, T3, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, TOther> InnerJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, TOther> LeftJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, TOther> RightJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn);
    #endregion

    IQuery<T1, T2, T3> Where(Expression<Func<T1, T2, T3, bool>> predicate);
    IQuery<T1, T2, T3> Where(bool condition, Expression<Func<T1, T2, T3, bool>> predicate = null);
    IQuery<T1, T2, T3> And(bool condition, Expression<Func<T1, T2, T3, bool>> predicate);
    
    IGroupingQuery<T1, T2, T3, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, TGrouping>> groupingExpr);
    IQuery<T1, T2, T3> OrderBy<TFields>(Expression<Func<T1, T2, T3, TFields>> fieldsExpr);
    IQuery<T1, T2, T3> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, TFields>> fieldsExpr);

    IQuery<T1, T2, T3> Skip(int offset);
    IQuery<T1, T2, T3> Take(int limit);
    IQuery<T1, T2, T3> ToChunk(int size);

    IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, TTarget>> fieldsExpr);
    IQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, TTarget>> fieldsExpr);

    int Count();
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    long LongCount();
    Task<long> LongCountAsync(CancellationToken cancellationToken = default);
    int Count<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr);
    Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr, CancellationToken cancellationToken = default);
    int CountDistinct<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr);
    Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr, CancellationToken cancellationToken = default);
    long LongCount<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr);
    Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr, CancellationToken cancellationToken = default);
    long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr);
    Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr, CancellationToken cancellationToken = default);

    TField Sum<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr);
    Task<TField> SumAsync<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr, CancellationToken cancellationToken = default);
    TField Avg<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr);
    Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr, CancellationToken cancellationToken = default);
    TField Max<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr);
    Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr, CancellationToken cancellationToken = default);
    TField Min<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr);
    Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr, CancellationToken cancellationToken = default);
    
    string ToSql(out List<IDbDataParameter> dbParameters);
}
public interface IQuery<T1, T2, T3, T4>
{
    #region WithCte
    /// <summary>
    /// CTE With子句，在Select之前，With子句要连续定义，用法：
    /// <code>
    /// repository
    ///     .FromWith(f =&gt; ..., "Cte1")
    ///     ... ...
    ///     .NextWith(f =&gt; ..., "CteN")
    ///     .InnerJoin((T1, T2, T3, T4) =&gt; a.Id == b.Id)
    ///     .InnerJoin((T1, T2, T3, T4) =&gt; a.Id == c.Id)
    ///     ... ...
    ///     .Select((T1, T2, T3, T4) =&gt; new { ... ... })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH Cte1(...) AS 
    /// (
    ///     SELECT ... FROM ... WHERE ...
    /// ),
    /// ... ...
    /// CteN(...) AS
    /// (
    ///     SELECT ... FROM ... WHERE ... UNION ALL ...
    /// )
    /// SELECT ... FROM Cte1 a INNER JOIN Cte2 b ON xxx ... LEFT JOIN CteN ON ... ... WHERE ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TTarget">当前CTE临时返回的实体类型</typeparam>
    /// <param name="cteSubQuery">CTE子查询，如：
    /// <code>
    /// f.From&lt;Page&gt;()
    ///     .InnerJoin&lt;Menu&gt;((a, b) =&gt; a.Id == b.PageId)
    ///     .Where((a, b) =&gt; a.Id == 1)
    ///     .Select((x, y) =&gt; new { y.Id, x.Url })
    /// </code>
    /// </param>
    /// <param name="cteTableName">CTE表名</param>
    /// <param name="tableAsStart">CTE子句中使用的表的别名开始字母，默认从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, TTarget> NextWith<TTarget>(Func<IFromQuery, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    IQuery<T1, T2, T3, T4, TTarget> NextWithRecursive<TTarget>(Func<IFromQuery, string, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    #endregion

    #region WithTable
    IQuery<T1, T2, T3, T4, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    #endregion

    #region Include
    IIncludableQuery<T1, T2, T3, T4, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, TMember>> memberSelector);
    IIncludableQuery<T1, T2, T3, T4, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null);
    #endregion

    #region Join
    IQuery<T1, T2, T3, T4> InnerJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn);
    IQuery<T1, T2, T3, T4> LeftJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn);
    IQuery<T1, T2, T3, T4> RightJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn);
    IQuery<T1, T2, T3, T4, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, TOther> InnerJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, TOther> LeftJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, TOther> RightJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn);
    #endregion

    IQuery<T1, T2, T3, T4> Where(Expression<Func<T1, T2, T3, T4, bool>> predicate);
    IQuery<T1, T2, T3, T4> Where(bool condition, Expression<Func<T1, T2, T3, T4, bool>> predicate = null);
    IQuery<T1, T2, T3, T4> And(bool condition, Expression<Func<T1, T2, T3, T4, bool>> predicate);
    
    IGroupingQuery<T1, T2, T3, T4, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, TGrouping>> groupingExpr);
    IQuery<T1, T2, T3, T4> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr);
    IQuery<T1, T2, T3, T4> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr);

    IQuery<T1, T2, T3, T4> Skip(int offset);
    IQuery<T1, T2, T3, T4> Take(int limit);
    IQuery<T1, T2, T3, T4> ToChunk(int size);

    IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, TTarget>> fieldsExpr);
    IQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, TTarget>> fieldsExpr);

    int Count();
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    long LongCount();
    Task<long> LongCountAsync(CancellationToken cancellationToken = default);
    int Count<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr);
    Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr, CancellationToken cancellationToken = default);
    int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr);
    Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr, CancellationToken cancellationToken = default);
    long LongCount<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr);
    Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr, CancellationToken cancellationToken = default);
    long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr);
    Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr, CancellationToken cancellationToken = default);

    TField Sum<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr);
    Task<TField> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr, CancellationToken cancellationToken = default);
    TField Avg<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr);
    Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr, CancellationToken cancellationToken = default);
    TField Max<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr);
    Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr, CancellationToken cancellationToken = default);
    TField Min<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr);
    Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr, CancellationToken cancellationToken = default);
    
    string ToSql(out List<IDbDataParameter> dbParameters);
}
public interface IQuery<T1, T2, T3, T4, T5>
{
    #region WithCte
    /// <summary>
    /// CTE With子句，在Select之前，With子句要连续定义，用法：
    /// <code>
    /// repository
    ///     .FromWith(f =&gt; ..., "Cte1")
    ///     ... ...
    ///     .NextWith(f =&gt; ..., "CteN")
    ///     .InnerJoin((T1, T2, T3, T4, T5) =&gt; a.Id == b.Id)
    ///     .InnerJoin((T1, T2, T3, T4, T5) =&gt; a.Id == c.Id)
    ///     ... ...
    ///     .Select((T1, T2, T3, T4, T5) =&gt; new { ... ... })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH Cte1(...) AS 
    /// (
    ///     SELECT ... FROM ... WHERE ...
    /// ),
    /// ... ...
    /// CteN(...) AS
    /// (
    ///     SELECT ... FROM ... WHERE ... UNION ALL ...
    /// )
    /// SELECT ... FROM Cte1 a INNER JOIN Cte2 b ON xxx ... LEFT JOIN CteN ON ... ... WHERE ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TTarget">当前CTE临时返回的实体类型</typeparam>
    /// <param name="cteSubQuery">CTE子查询，如：
    /// <code>
    /// f.From&lt;Page&gt;()
    ///     .InnerJoin&lt;Menu&gt;((a, b) =&gt; a.Id == b.PageId)
    ///     .Where((a, b) =&gt; a.Id == 1)
    ///     .Select((x, y) =&gt; new { y.Id, x.Url })
    /// </code>
    /// </param>
    /// <param name="cteTableName">CTE表名</param>
    /// <param name="tableAsStart">CTE子句中使用的表的别名开始字母，默认从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, TTarget> NextWith<TTarget>(Func<IFromQuery, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    IQuery<T1, T2, T3, T4, T5, TTarget> NextWithRecursive<TTarget>(Func<IFromQuery, string, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    #endregion

    #region WithTable
    IQuery<T1, T2, T3, T4, T5, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    #endregion

    #region Include
    IIncludableQuery<T1, T2, T3, T4, T5, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, TMember>> memberSelector);
    IIncludableQuery<T1, T2, T3, T4, T5, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, T5, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null);
    #endregion

    #region Join
    IQuery<T1, T2, T3, T4, T5> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5> RightJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn);
    #endregion

    IQuery<T1, T2, T3, T4, T5> Where(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate);
    IQuery<T1, T2, T3, T4, T5> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> predicate = null);
    IQuery<T1, T2, T3, T4, T5> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> predicate);
    
    IGroupingQuery<T1, T2, T3, T4, T5, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, TGrouping>> groupingExpr);
    IQuery<T1, T2, T3, T4, T5> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    IQuery<T1, T2, T3, T4, T5> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr);

    IQuery<T1, T2, T3, T4, T5> Skip(int offset);
    IQuery<T1, T2, T3, T4, T5> Take(int limit);
    IQuery<T1, T2, T3, T4, T5> ToChunk(int size);

    IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, TTarget>> fieldsExpr);
    IQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, TTarget>> fieldsExpr);

    int Count();
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    long LongCount();
    Task<long> LongCountAsync(CancellationToken cancellationToken = default);
    int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr);
    Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr, CancellationToken cancellationToken = default);
    int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr);
    Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr, CancellationToken cancellationToken = default);
    long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr);
    Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr, CancellationToken cancellationToken = default);
    long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr);
    Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr, CancellationToken cancellationToken = default);

    TField Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr);
    Task<TField> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr, CancellationToken cancellationToken = default);
    TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr);
    Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr, CancellationToken cancellationToken = default);
    TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr);
    Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr, CancellationToken cancellationToken = default);
    TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr);
    Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr, CancellationToken cancellationToken = default);
    
    string ToSql(out List<IDbDataParameter> dbParameters);
}
public interface IQuery<T1, T2, T3, T4, T5, T6>
{
    #region WithCte
    /// <summary>
    /// CTE With子句，在Select之前，With子句要连续定义，用法：
    /// <code>
    /// repository
    ///     .FromWith(f =&gt; ..., "Cte1")
    ///     ... ...
    ///     .NextWith(f =&gt; ..., "CteN")
    ///     .InnerJoin((T1, T2, T3, T4, T5, T6) =&gt; a.Id == b.Id)
    ///     .InnerJoin((T1, T2, T3, T4, T5, T6) =&gt; a.Id == c.Id)
    ///     ... ...
    ///     .Select((T1, T2, T3, T4, T5, T6) =&gt; new { ... ... })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH Cte1(...) AS 
    /// (
    ///     SELECT ... FROM ... WHERE ...
    /// ),
    /// ... ...
    /// CteN(...) AS
    /// (
    ///     SELECT ... FROM ... WHERE ... UNION ALL ...
    /// )
    /// SELECT ... FROM Cte1 a INNER JOIN Cte2 b ON xxx ... LEFT JOIN CteN ON ... ... WHERE ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TTarget">当前CTE临时返回的实体类型</typeparam>
    /// <param name="cteSubQuery">CTE子查询，如：
    /// <code>
    /// f.From&lt;Page&gt;()
    ///     .InnerJoin&lt;Menu&gt;((a, b) =&gt; a.Id == b.PageId)
    ///     .Where((a, b) =&gt; a.Id == 1)
    ///     .Select((x, y) =&gt; new { y.Id, x.Url })
    /// </code>
    /// </param>
    /// <param name="cteTableName">CTE表名</param>
    /// <param name="tableAsStart">CTE子句中使用的表的别名开始字母，默认从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, TTarget> NextWith<TTarget>(Func<IFromQuery, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    IQuery<T1, T2, T3, T4, T5, T6, TTarget> NextWithRecursive<TTarget>(Func<IFromQuery, string, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    #endregion

    #region WithTable
    IQuery<T1, T2, T3, T4, T5, T6, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    #endregion

    #region Include
    IIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, TMember>> memberSelector);
    IIncludableQuery<T1, T2, T3, T4, T5, T6, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, T5, T6, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null);
    #endregion

    #region Join
    IQuery<T1, T2, T3, T4, T5, T6> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, TOther> InnerJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, TOther> LeftJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, TOther> RightJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn);
    #endregion

    IQuery<T1, T2, T3, T4, T5, T6> Where(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate);
    IQuery<T1, T2, T3, T4, T5, T6> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate = null);
    IQuery<T1, T2, T3, T4, T5, T6> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate);
    
    IGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, TGrouping>> groupingExpr);
    IQuery<T1, T2, T3, T4, T5, T6> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr);
    IQuery<T1, T2, T3, T4, T5, T6> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr);

    IQuery<T1, T2, T3, T4, T5, T6> Skip(int offset);
    IQuery<T1, T2, T3, T4, T5, T6> Take(int limit);
    IQuery<T1, T2, T3, T4, T5, T6> ToChunk(int size);

    IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr);
    IQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr);

    int Count();
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    long LongCount();
    Task<long> LongCountAsync(CancellationToken cancellationToken = default);
    int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr);
    Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr, CancellationToken cancellationToken = default);
    int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr);
    Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr, CancellationToken cancellationToken = default);
    long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr);
    Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr, CancellationToken cancellationToken = default);
    long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr);
    Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr, CancellationToken cancellationToken = default);

    TField Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr);
    Task<TField> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr, CancellationToken cancellationToken = default);
    TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr);
    Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr, CancellationToken cancellationToken = default);
    TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr);
    Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr, CancellationToken cancellationToken = default);
    TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr);
    Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr, CancellationToken cancellationToken = default);
    
    string ToSql(out List<IDbDataParameter> dbParameters);
}
public interface IQuery<T1, T2, T3, T4, T5, T6, T7>
{
    #region WithCte
    /// <summary>
    /// CTE With子句，在Select之前，With子句要连续定义，用法：
    /// <code>
    /// repository
    ///     .FromWith(f =&gt; ..., "Cte1")
    ///     ... ...
    ///     .NextWith(f =&gt; ..., "CteN")
    ///     .InnerJoin((T1, T2, T3, T4, T5, T6, T7) =&gt; a.Id == b.Id)
    ///     .InnerJoin((T1, T2, T3, T4, T5, T6, T7) =&gt; a.Id == c.Id)
    ///     ... ...
    ///     .Select((T1, T2, T3, T4, T5, T6, T7) =&gt; new { ... ... })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH Cte1(...) AS 
    /// (
    ///     SELECT ... FROM ... WHERE ...
    /// ),
    /// ... ...
    /// CteN(...) AS
    /// (
    ///     SELECT ... FROM ... WHERE ... UNION ALL ...
    /// )
    /// SELECT ... FROM Cte1 a INNER JOIN Cte2 b ON xxx ... LEFT JOIN CteN ON ... ... WHERE ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TTarget">当前CTE临时返回的实体类型</typeparam>
    /// <param name="cteSubQuery">CTE子查询，如：
    /// <code>
    /// f.From&lt;Page&gt;()
    ///     .InnerJoin&lt;Menu&gt;((a, b) =&gt; a.Id == b.PageId)
    ///     .Where((a, b) =&gt; a.Id == 1)
    ///     .Select((x, y) =&gt; new { y.Id, x.Url })
    /// </code>
    /// </param>
    /// <param name="cteTableName">CTE表名</param>
    /// <param name="tableAsStart">CTE子句中使用的表的别名开始字母，默认从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, TTarget> NextWith<TTarget>(Func<IFromQuery, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    IQuery<T1, T2, T3, T4, T5, T6, T7, TTarget> NextWithRecursive<TTarget>(Func<IFromQuery, string, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    #endregion

    #region WithTable
    IQuery<T1, T2, T3, T4, T5, T6, T7, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    #endregion

    #region Include
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>> memberSelector);
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null);
    #endregion

    #region Join
    IQuery<T1, T2, T3, T4, T5, T6, T7> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, TOther> InnerJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, TOther> LeftJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, TOther> RightJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn);
    #endregion

    IQuery<T1, T2, T3, T4, T5, T6, T7> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> predicate);
    IQuery<T1, T2, T3, T4, T5, T6, T7> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> predicate = null);
    IQuery<T1, T2, T3, T4, T5, T6, T7> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> predicate);
    
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TGrouping>> groupingExpr);
    IQuery<T1, T2, T3, T4, T5, T6, T7> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr);
    IQuery<T1, T2, T3, T4, T5, T6, T7> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr);

    IQuery<T1, T2, T3, T4, T5, T6, T7> Skip(int offset);
    IQuery<T1, T2, T3, T4, T5, T6, T7> Take(int limit);
    IQuery<T1, T2, T3, T4, T5, T6, T7> ToChunk(int size);

    IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TTarget>> fieldsExpr);
    IQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, TTarget>> fieldsExpr);

    int Count();
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    long LongCount();
    Task<long> LongCountAsync(CancellationToken cancellationToken = default);
    int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr);
    Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr, CancellationToken cancellationToken = default);
    int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr);
    Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr, CancellationToken cancellationToken = default);
    long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr);
    Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr, CancellationToken cancellationToken = default);
    long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr);
    Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr, CancellationToken cancellationToken = default);

    TField Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr);
    Task<TField> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr, CancellationToken cancellationToken = default);
    TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr);
    Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr, CancellationToken cancellationToken = default);
    TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr);
    Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr, CancellationToken cancellationToken = default);
    TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr);
    Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr, CancellationToken cancellationToken = default);
    
    string ToSql(out List<IDbDataParameter> dbParameters);
}
public interface IQuery<T1, T2, T3, T4, T5, T6, T7, T8>
{
    #region WithCte
    /// <summary>
    /// CTE With子句，在Select之前，With子句要连续定义，用法：
    /// <code>
    /// repository
    ///     .FromWith(f =&gt; ..., "Cte1")
    ///     ... ...
    ///     .NextWith(f =&gt; ..., "CteN")
    ///     .InnerJoin((T1, T2, T3, T4, T5, T6, T7, T8) =&gt; a.Id == b.Id)
    ///     .InnerJoin((T1, T2, T3, T4, T5, T6, T7, T8) =&gt; a.Id == c.Id)
    ///     ... ...
    ///     .Select((T1, T2, T3, T4, T5, T6, T7, T8) =&gt; new { ... ... })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH Cte1(...) AS 
    /// (
    ///     SELECT ... FROM ... WHERE ...
    /// ),
    /// ... ...
    /// CteN(...) AS
    /// (
    ///     SELECT ... FROM ... WHERE ... UNION ALL ...
    /// )
    /// SELECT ... FROM Cte1 a INNER JOIN Cte2 b ON xxx ... LEFT JOIN CteN ON ... ... WHERE ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TTarget">当前CTE临时返回的实体类型</typeparam>
    /// <param name="cteSubQuery">CTE子查询，如：
    /// <code>
    /// f.From&lt;Page&gt;()
    ///     .InnerJoin&lt;Menu&gt;((a, b) =&gt; a.Id == b.PageId)
    ///     .Where((a, b) =&gt; a.Id == 1)
    ///     .Select((x, y) =&gt; new { y.Id, x.Url })
    /// </code>
    /// </param>
    /// <param name="cteTableName">CTE表名</param>
    /// <param name="tableAsStart">CTE子句中使用的表的别名开始字母，默认从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, TTarget> NextWith<TTarget>(Func<IFromQuery, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, TTarget> NextWithRecursive<TTarget>(Func<IFromQuery, string, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    #endregion

    #region WithTable
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    #endregion

    #region Include
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TMember>> memberSelector);
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null);
    #endregion

    #region Join
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> InnerJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> LeftJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> RightJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn);
    #endregion

    IQuery<T1, T2, T3, T4, T5, T6, T7, T8> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate = null);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate);
    
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping>> groupingExpr);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr);

    IQuery<T1, T2, T3, T4, T5, T6, T7, T8> Skip(int offset);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8> Take(int limit);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8> ToChunk(int size);

    IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TTarget>> fieldsExpr);
    IQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, TTarget>> fieldsExpr);

    int Count();
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    long LongCount();
    Task<long> LongCountAsync(CancellationToken cancellationToken = default);
    int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr);
    Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr, CancellationToken cancellationToken = default);
    int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr);
    Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr, CancellationToken cancellationToken = default);
    long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr);
    Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr, CancellationToken cancellationToken = default);
    long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr);
    Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr, CancellationToken cancellationToken = default);

    TField Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr);
    Task<TField> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr, CancellationToken cancellationToken = default);
    TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr);
    Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr, CancellationToken cancellationToken = default);
    TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr);
    Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr, CancellationToken cancellationToken = default);
    TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr);
    Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr, CancellationToken cancellationToken = default);
    
    string ToSql(out List<IDbDataParameter> dbParameters);
}
public interface IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>
{
    #region WithCte
    /// <summary>
    /// CTE With子句，在Select之前，With子句要连续定义，用法：
    /// <code>
    /// repository
    ///     .FromWith(f =&gt; ..., "Cte1")
    ///     ... ...
    ///     .NextWith(f =&gt; ..., "CteN")
    ///     .InnerJoin((T1, T2, T3, T4, T5, T6, T7, T8, T9) =&gt; a.Id == b.Id)
    ///     .InnerJoin((T1, T2, T3, T4, T5, T6, T7, T8, T9) =&gt; a.Id == c.Id)
    ///     ... ...
    ///     .Select((T1, T2, T3, T4, T5, T6, T7, T8, T9) =&gt; new { ... ... })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH Cte1(...) AS 
    /// (
    ///     SELECT ... FROM ... WHERE ...
    /// ),
    /// ... ...
    /// CteN(...) AS
    /// (
    ///     SELECT ... FROM ... WHERE ... UNION ALL ...
    /// )
    /// SELECT ... FROM Cte1 a INNER JOIN Cte2 b ON xxx ... LEFT JOIN CteN ON ... ... WHERE ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TTarget">当前CTE临时返回的实体类型</typeparam>
    /// <param name="cteSubQuery">CTE子查询，如：
    /// <code>
    /// f.From&lt;Page&gt;()
    ///     .InnerJoin&lt;Menu&gt;((a, b) =&gt; a.Id == b.PageId)
    ///     .Where((a, b) =&gt; a.Id == 1)
    ///     .Select((x, y) =&gt; new { y.Id, x.Url })
    /// </code>
    /// </param>
    /// <param name="cteTableName">CTE表名</param>
    /// <param name="tableAsStart">CTE子句中使用的表的别名开始字母，默认从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget> NextWith<TTarget>(Func<IFromQuery, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget> NextWithRecursive<TTarget>(Func<IFromQuery, string, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    #endregion

    #region WithTable
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    #endregion

    #region Include
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>> memberSelector);
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null);
    #endregion

    #region Join
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> InnerJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> LeftJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> RightJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn);
    #endregion

    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate = null);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate);
    
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping>> groupingExpr);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr);

    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Skip(int offset);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Take(int limit);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> ToChunk(int size);

    IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget>> fieldsExpr);
    IQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget>> fieldsExpr);

    int Count();
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    long LongCount();
    Task<long> LongCountAsync(CancellationToken cancellationToken = default);
    int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr);
    Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr, CancellationToken cancellationToken = default);
    int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr);
    Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr, CancellationToken cancellationToken = default);
    long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr);
    Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr, CancellationToken cancellationToken = default);
    long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr);
    Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr, CancellationToken cancellationToken = default);

    TField Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr);
    Task<TField> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr, CancellationToken cancellationToken = default);
    TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr);
    Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr, CancellationToken cancellationToken = default);
    TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr);
    Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr, CancellationToken cancellationToken = default);
    TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr);
    Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr, CancellationToken cancellationToken = default);
    
    string ToSql(out List<IDbDataParameter> dbParameters);
}
public interface IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
{
    #region WithCte
    /// <summary>
    /// CTE With子句，在Select之前，With子句要连续定义，用法：
    /// <code>
    /// repository
    ///     .FromWith(f =&gt; ..., "Cte1")
    ///     ... ...
    ///     .NextWith(f =&gt; ..., "CteN")
    ///     .InnerJoin((T1, T2, T3, T4, T5, T6, T7, T8, T9, T10) =&gt; a.Id == b.Id)
    ///     .InnerJoin((T1, T2, T3, T4, T5, T6, T7, T8, T9, T10) =&gt; a.Id == c.Id)
    ///     ... ...
    ///     .Select((T1, T2, T3, T4, T5, T6, T7, T8, T9, T10) =&gt; new { ... ... })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH Cte1(...) AS 
    /// (
    ///     SELECT ... FROM ... WHERE ...
    /// ),
    /// ... ...
    /// CteN(...) AS
    /// (
    ///     SELECT ... FROM ... WHERE ... UNION ALL ...
    /// )
    /// SELECT ... FROM Cte1 a INNER JOIN Cte2 b ON xxx ... LEFT JOIN CteN ON ... ... WHERE ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TTarget">当前CTE临时返回的实体类型</typeparam>
    /// <param name="cteSubQuery">CTE子查询，如：
    /// <code>
    /// f.From&lt;Page&gt;()
    ///     .InnerJoin&lt;Menu&gt;((a, b) =&gt; a.Id == b.PageId)
    ///     .Where((a, b) =&gt; a.Id == 1)
    ///     .Select((x, y) =&gt; new { y.Id, x.Url })
    /// </code>
    /// </param>
    /// <param name="cteTableName">CTE表名</param>
    /// <param name="tableAsStart">CTE子句中使用的表的别名开始字母，默认从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget> NextWith<TTarget>(Func<IFromQuery, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget> NextWithRecursive<TTarget>(Func<IFromQuery, string, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    #endregion

    #region WithTable
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    #endregion

    #region Include
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>> memberSelector);
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null);
    #endregion

    #region Join
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> InnerJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> LeftJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> RightJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn);
    #endregion

    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate = null);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate);
    
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping>> groupingExpr);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr);

    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Skip(int offset);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Take(int limit);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> ToChunk(int size);

    IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget>> fieldsExpr);
    IQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget>> fieldsExpr);

    int Count();
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    long LongCount();
    Task<long> LongCountAsync(CancellationToken cancellationToken = default);
    int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr);
    Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr, CancellationToken cancellationToken = default);
    int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr);
    Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr, CancellationToken cancellationToken = default);
    long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr);
    Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr, CancellationToken cancellationToken = default);
    long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr);
    Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr, CancellationToken cancellationToken = default);

    TField Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr);
    Task<TField> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr, CancellationToken cancellationToken = default);
    TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr);
    Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr, CancellationToken cancellationToken = default);
    TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr);
    Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr, CancellationToken cancellationToken = default);
    TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr);
    Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr, CancellationToken cancellationToken = default);
    
    string ToSql(out List<IDbDataParameter> dbParameters);
}
public interface IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
{
    #region WithCte
    /// <summary>
    /// CTE With子句，在Select之前，With子句要连续定义，用法：
    /// <code>
    /// repository
    ///     .FromWith(f =&gt; ..., "Cte1")
    ///     ... ...
    ///     .NextWith(f =&gt; ..., "CteN")
    ///     .InnerJoin((T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11) =&gt; a.Id == b.Id)
    ///     .InnerJoin((T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11) =&gt; a.Id == c.Id)
    ///     ... ...
    ///     .Select((T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11) =&gt; new { ... ... })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH Cte1(...) AS 
    /// (
    ///     SELECT ... FROM ... WHERE ...
    /// ),
    /// ... ...
    /// CteN(...) AS
    /// (
    ///     SELECT ... FROM ... WHERE ... UNION ALL ...
    /// )
    /// SELECT ... FROM Cte1 a INNER JOIN Cte2 b ON xxx ... LEFT JOIN CteN ON ... ... WHERE ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TTarget">当前CTE临时返回的实体类型</typeparam>
    /// <param name="cteSubQuery">CTE子查询，如：
    /// <code>
    /// f.From&lt;Page&gt;()
    ///     .InnerJoin&lt;Menu&gt;((a, b) =&gt; a.Id == b.PageId)
    ///     .Where((a, b) =&gt; a.Id == 1)
    ///     .Select((x, y) =&gt; new { y.Id, x.Url })
    /// </code>
    /// </param>
    /// <param name="cteTableName">CTE表名</param>
    /// <param name="tableAsStart">CTE子句中使用的表的别名开始字母，默认从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget> NextWith<TTarget>(Func<IFromQuery, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget> NextWithRecursive<TTarget>(Func<IFromQuery, string, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    #endregion

    #region WithTable
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    #endregion

    #region Include
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>> memberSelector);
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null);
    #endregion

    #region Join
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> InnerJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> LeftJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> RightJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn);
    #endregion

    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate = null);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate);
    
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping>> groupingExpr);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr);

    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Skip(int offset);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Take(int limit);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> ToChunk(int size);

    IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget>> fieldsExpr);
    IQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget>> fieldsExpr);

    int Count();
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    long LongCount();
    Task<long> LongCountAsync(CancellationToken cancellationToken = default);
    int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr);
    Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr, CancellationToken cancellationToken = default);
    int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr);
    Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr, CancellationToken cancellationToken = default);
    long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr);
    Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr, CancellationToken cancellationToken = default);
    long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr);
    Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr, CancellationToken cancellationToken = default);

    TField Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr);
    Task<TField> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr, CancellationToken cancellationToken = default);
    TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr);
    Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr, CancellationToken cancellationToken = default);
    TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr);
    Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr, CancellationToken cancellationToken = default);
    TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr);
    Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr, CancellationToken cancellationToken = default);
    
    string ToSql(out List<IDbDataParameter> dbParameters);
}
public interface IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
{
    #region WithCte
    /// <summary>
    /// CTE With子句，在Select之前，With子句要连续定义，用法：
    /// <code>
    /// repository
    ///     .FromWith(f =&gt; ..., "Cte1")
    ///     ... ...
    ///     .NextWith(f =&gt; ..., "CteN")
    ///     .InnerJoin((T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12) =&gt; a.Id == b.Id)
    ///     .InnerJoin((T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12) =&gt; a.Id == c.Id)
    ///     ... ...
    ///     .Select((T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12) =&gt; new { ... ... })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH Cte1(...) AS 
    /// (
    ///     SELECT ... FROM ... WHERE ...
    /// ),
    /// ... ...
    /// CteN(...) AS
    /// (
    ///     SELECT ... FROM ... WHERE ... UNION ALL ...
    /// )
    /// SELECT ... FROM Cte1 a INNER JOIN Cte2 b ON xxx ... LEFT JOIN CteN ON ... ... WHERE ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TTarget">当前CTE临时返回的实体类型</typeparam>
    /// <param name="cteSubQuery">CTE子查询，如：
    /// <code>
    /// f.From&lt;Page&gt;()
    ///     .InnerJoin&lt;Menu&gt;((a, b) =&gt; a.Id == b.PageId)
    ///     .Where((a, b) =&gt; a.Id == 1)
    ///     .Select((x, y) =&gt; new { y.Id, x.Url })
    /// </code>
    /// </param>
    /// <param name="cteTableName">CTE表名</param>
    /// <param name="tableAsStart">CTE子句中使用的表的别名开始字母，默认从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget> NextWith<TTarget>(Func<IFromQuery, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget> NextWithRecursive<TTarget>(Func<IFromQuery, string, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    #endregion

    #region WithTable
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    #endregion

    #region Include
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>> memberSelector);
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null);
    #endregion

    #region Join
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> InnerJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> LeftJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> RightJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn);
    #endregion

    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate = null);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate);
    
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping>> groupingExpr);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr);

    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Skip(int offset);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Take(int limit);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> ToChunk(int size);

    IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget>> fieldsExpr);
    IQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget>> fieldsExpr);

    int Count();
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    long LongCount();
    Task<long> LongCountAsync(CancellationToken cancellationToken = default);
    int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr);
    Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr, CancellationToken cancellationToken = default);
    int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr);
    Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr, CancellationToken cancellationToken = default);
    long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr);
    Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr, CancellationToken cancellationToken = default);
    long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr);
    Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr, CancellationToken cancellationToken = default);

    TField Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr);
    Task<TField> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr, CancellationToken cancellationToken = default);
    TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr);
    Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr, CancellationToken cancellationToken = default);
    TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr);
    Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr, CancellationToken cancellationToken = default);
    TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr);
    Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr, CancellationToken cancellationToken = default);
    
    string ToSql(out List<IDbDataParameter> dbParameters);
}
public interface IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
{
    #region WithCte
    /// <summary>
    /// CTE With子句，在Select之前，With子句要连续定义，用法：
    /// <code>
    /// repository
    ///     .FromWith(f =&gt; ..., "Cte1")
    ///     ... ...
    ///     .NextWith(f =&gt; ..., "CteN")
    ///     .InnerJoin((T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13) =&gt; a.Id == b.Id)
    ///     .InnerJoin((T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13) =&gt; a.Id == c.Id)
    ///     ... ...
    ///     .Select((T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13) =&gt; new { ... ... })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH Cte1(...) AS 
    /// (
    ///     SELECT ... FROM ... WHERE ...
    /// ),
    /// ... ...
    /// CteN(...) AS
    /// (
    ///     SELECT ... FROM ... WHERE ... UNION ALL ...
    /// )
    /// SELECT ... FROM Cte1 a INNER JOIN Cte2 b ON xxx ... LEFT JOIN CteN ON ... ... WHERE ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TTarget">当前CTE临时返回的实体类型</typeparam>
    /// <param name="cteSubQuery">CTE子查询，如：
    /// <code>
    /// f.From&lt;Page&gt;()
    ///     .InnerJoin&lt;Menu&gt;((a, b) =&gt; a.Id == b.PageId)
    ///     .Where((a, b) =&gt; a.Id == 1)
    ///     .Select((x, y) =&gt; new { y.Id, x.Url })
    /// </code>
    /// </param>
    /// <param name="cteTableName">CTE表名</param>
    /// <param name="tableAsStart">CTE子句中使用的表的别名开始字母，默认从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget> NextWith<TTarget>(Func<IFromQuery, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget> NextWithRecursive<TTarget>(Func<IFromQuery, string, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    #endregion

    #region WithTable
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    #endregion

    #region Include
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>> memberSelector);
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null);
    #endregion

    #region Join
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> InnerJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> LeftJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> RightJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn);
    #endregion

    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate = null);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate);
    
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping>> groupingExpr);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr);

    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Skip(int offset);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Take(int limit);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> ToChunk(int size);

    IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget>> fieldsExpr);
    IQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget>> fieldsExpr);

    int Count();
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    long LongCount();
    Task<long> LongCountAsync(CancellationToken cancellationToken = default);
    int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr);
    Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr, CancellationToken cancellationToken = default);
    int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr);
    Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr, CancellationToken cancellationToken = default);
    long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr);
    Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr, CancellationToken cancellationToken = default);
    long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr);
    Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr, CancellationToken cancellationToken = default);

    TField Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr);
    Task<TField> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr, CancellationToken cancellationToken = default);
    TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr);
    Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr, CancellationToken cancellationToken = default);
    TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr);
    Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr, CancellationToken cancellationToken = default);
    TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr);
    Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr, CancellationToken cancellationToken = default);
    
    string ToSql(out List<IDbDataParameter> dbParameters);
}
public interface IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
{
    #region WithCte
    /// <summary>
    /// CTE With子句，在Select之前，With子句要连续定义，用法：
    /// <code>
    /// repository
    ///     .FromWith(f =&gt; ..., "Cte1")
    ///     ... ...
    ///     .NextWith(f =&gt; ..., "CteN")
    ///     .InnerJoin((T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14) =&gt; a.Id == b.Id)
    ///     .InnerJoin((T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14) =&gt; a.Id == c.Id)
    ///     ... ...
    ///     .Select((T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14) =&gt; new { ... ... })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH Cte1(...) AS 
    /// (
    ///     SELECT ... FROM ... WHERE ...
    /// ),
    /// ... ...
    /// CteN(...) AS
    /// (
    ///     SELECT ... FROM ... WHERE ... UNION ALL ...
    /// )
    /// SELECT ... FROM Cte1 a INNER JOIN Cte2 b ON xxx ... LEFT JOIN CteN ON ... ... WHERE ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TTarget">当前CTE临时返回的实体类型</typeparam>
    /// <param name="cteSubQuery">CTE子查询，如：
    /// <code>
    /// f.From&lt;Page&gt;()
    ///     .InnerJoin&lt;Menu&gt;((a, b) =&gt; a.Id == b.PageId)
    ///     .Where((a, b) =&gt; a.Id == 1)
    ///     .Select((x, y) =&gt; new { y.Id, x.Url })
    /// </code>
    /// </param>
    /// <param name="cteTableName">CTE表名</param>
    /// <param name="tableAsStart">CTE子句中使用的表的别名开始字母，默认从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget> NextWith<TTarget>(Func<IFromQuery, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget> NextWithRecursive<TTarget>(Func<IFromQuery, string, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    #endregion

    #region WithTable
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    #endregion

    #region Include
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>> memberSelector);
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null);
    #endregion

    #region Join
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> InnerJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> LeftJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> RightJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn);
    #endregion

    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate = null);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate);
    
    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping>> groupingExpr);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr);

    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Skip(int offset);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Take(int limit);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> ToChunk(int size);

    IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget>> fieldsExpr);
    IQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget>> fieldsExpr);

    int Count();
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    long LongCount();
    Task<long> LongCountAsync(CancellationToken cancellationToken = default);
    int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr);
    Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr, CancellationToken cancellationToken = default);
    int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr);
    Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr, CancellationToken cancellationToken = default);
    long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr);
    Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr, CancellationToken cancellationToken = default);
    long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr);
    Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr, CancellationToken cancellationToken = default);

    TField Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr);
    Task<TField> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr, CancellationToken cancellationToken = default);
    TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr);
    Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr, CancellationToken cancellationToken = default);
    TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr);
    Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr, CancellationToken cancellationToken = default);
    TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr);
    Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr, CancellationToken cancellationToken = default);
    
    string ToSql(out List<IDbDataParameter> dbParameters);
}
public interface IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
{
    #region Include
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>> memberSelector);
    IIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null);
    #endregion

    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> predicate);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> predicate = null);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> predicate);

    IGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping>> groupingExpr);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr);

    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Skip(int offset);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Take(int limit);
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> ToChunk(int size);

    IQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TTarget>> fieldsExpr);

    int Count();
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    long LongCount();
    Task<long> LongCountAsync(CancellationToken cancellationToken = default);
    int Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr);
    Task<int> CountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr, CancellationToken cancellationToken = default);
    int CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr);
    Task<int> CountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr, CancellationToken cancellationToken = default);
    long LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr);
    Task<long> LongCountAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr, CancellationToken cancellationToken = default);
    long LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr);
    Task<long> LongCountDistinctAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr, CancellationToken cancellationToken = default);

    TField Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr);
    Task<TField> SumAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr, CancellationToken cancellationToken = default);
    TField Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr);
    Task<TField> AvgAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr, CancellationToken cancellationToken = default);
    TField Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr);
    Task<TField> MaxAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr, CancellationToken cancellationToken = default);
    TField Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr);
    Task<TField> MinAsync<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr, CancellationToken cancellationToken = default);

    string ToSql(out List<IDbDataParameter> dbParameters);
}