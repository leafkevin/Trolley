using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Trolley;

/// <summary>
/// 匿名查询对象
/// </summary>
public interface IQueryAnonymousObject
{
    // <summary>
    /// 返回当前查询的SQL和参数列表
    /// </summary>
    /// <param name="dbParameters">参数列表</param>
    /// <returns>当前查询的SQL</returns>
    string ToSql(out List<IDbDataParameter> dbParameters);
}
/// <summary>
/// 子查询，所有的子查询都是从From开始的
/// </summary>
public interface IFromQuery
{
    /// <summary>
    /// 创建子查询，生成SQL: FROM T
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <param name="tableAsStart">表别名起始字母，默认从'a'开始</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T> From<T>(char tableAsStart = 'a');
    /// <summary>
    /// 联合表T1, T2创建子查询，生成SQL: FROM T1, T2
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <param name="tableAsStart">表别名起始字母，默认是'a'</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2> From<T1, T2>(char tableAsStart = 'a');
    /// <summary>
    /// 联合表T1, T2, T3创建子查询，生成SQL: FROM T1, T2, T3
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <typeparam name="T3">表T3实体类型</typeparam>
    /// <param name="tableAsStart">表别名起始字母，默认是'a'</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3> From<T1, T2, T3>(char tableAsStart = 'a');
    /// <summary>
    /// 联合表T1, T2, ..., T4创建子查询，生成SQL: FROM T1, T2, ..., T4
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <typeparam name="T3">表T3实体类型</typeparam>
    /// <typeparam name="T4">表T4实体类型</typeparam>
    /// <param name="tableAsStart">表别名起始字母，默认是'a'</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4> From<T1, T2, T3, T4>(char tableAsStart = 'a');
    /// <summary>
    /// 联合表T1, T2, ..., T5创建子查询，生成SQL: FROM T1, T2, ..., T5
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <typeparam name="T3">表T3实体类型</typeparam>
    /// <typeparam name="T4">表T4实体类型</typeparam>
    /// <typeparam name="T5">表T5实体类型</typeparam>
    /// <param name="tableAsStart">表别名起始字母，默认是'a'</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>(char tableAsStart = 'a');
    /// <summary>
    /// 联合表T1, T2, ..., T6创建子查询，生成SQL: FROM T1, T2, ..., T6
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <typeparam name="T3">表T3实体类型</typeparam>
    /// <typeparam name="T4">表T4实体类型</typeparam>
    /// <typeparam name="T5">表T5实体类型</typeparam>
    /// <typeparam name="T6">表T6实体类型</typeparam>
    /// <param name="tableAsStart">表别名起始字母，默认是'a'</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>(char tableAsStart = 'a');
    /// <summary>
    /// 联合表T1, T2, ..., T7创建子查询，生成SQL: FROM T1, T2, ..., T7
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <typeparam name="T3">表T3实体类型</typeparam>
    /// <typeparam name="T4">表T4实体类型</typeparam>
    /// <typeparam name="T5">表T5实体类型</typeparam>
    /// <typeparam name="T6">表T6实体类型</typeparam>
    /// <typeparam name="T7">表T7实体类型</typeparam>
    /// <param name="tableAsStart">表别名起始字母，默认是'a'</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7> From<T1, T2, T3, T4, T5, T6, T7>(char tableAsStart = 'a');
    /// <summary>
    /// 联合表T1, T2, ..., T8创建子查询，生成SQL: FROM T1, T2, ..., T8
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <typeparam name="T3">表T3实体类型</typeparam>
    /// <typeparam name="T4">表T4实体类型</typeparam>
    /// <typeparam name="T5">表T5实体类型</typeparam>
    /// <typeparam name="T6">表T6实体类型</typeparam>
    /// <typeparam name="T7">表T7实体类型</typeparam>
    /// <typeparam name="T8">表T8实体类型</typeparam>
    /// <param name="tableAsStart">表别名起始字母，默认是'a'</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> From<T1, T2, T3, T4, T5, T6, T7, T8>(char tableAsStart = 'a');
    /// <summary>
    /// 联合表T1, T2, ..., T9创建子查询，生成SQL: FROM T1, T2, ..., T9
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
    /// <param name="tableAsStart">表别名起始字母，默认是'a'</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T1, T2, T3, T4, T5, T6, T7, T8, T9>(char tableAsStart = 'a');
    /// <summary>
    /// 联合表T1, T2, ..., T10创建子查询，生成SQL: FROM T1, T2, ..., T10
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
    /// <param name="tableAsStart">表别名起始字母，默认是'a'</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(char tableAsStart = 'a');
    /// <summary>
    /// 联合表T1, T2, ..., T11创建子查询，生成SQL: FROM T1, T2, ..., T11
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
    /// <param name="tableAsStart">表别名起始字母，默认是'a'</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(char tableAsStart = 'a');
    /// <summary>
    /// 联合表T1, T2, ..., T12创建子查询，生成SQL: FROM T1, T2, ..., T12
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
    /// <param name="tableAsStart">表别名起始字母，默认是'a'</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(char tableAsStart = 'a');
    /// <summary>
    /// 联合表T1, T2, ..., T13创建子查询，生成SQL: FROM T1, T2, ..., T13
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
    /// <param name="tableAsStart">表别名起始字母，默认是'a'</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(char tableAsStart = 'a');
    /// <summary>
    /// 联合表T1, T2, ..., T14创建子查询，生成SQL: FROM T1, T2, ..., T14
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
    /// <param name="tableAsStart">表别名起始字母，默认是'a'</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(char tableAsStart = 'a');
    /// <summary>
    /// 联合表T1, T2, ..., T15创建子查询，生成SQL: FROM T1, T2, ..., T15
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
    /// <param name="tableAsStart">表别名起始字母，默认是'a'</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(char tableAsStart = 'a');
    /// <summary>
    /// 返回当前查询的SQL和参数列表
    /// </summary>
    /// <param name="dbParameters">参数列表</param>
    /// <returns>当前查询的SQL</returns>
    string ToSql(out List<IDbDataParameter> dbParameters);
}
/// <summary>
/// 单表T子查询
/// </summary>
public interface IFromQuery<T>
{
    #region Union/UnionAll
    /// <summary>
    /// 子查询中的Union操作，用法：
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
    IFromQuery<T> Union(Func<IFromQuery, IFromQuery<T>> subQuery);
    /// <summary>
    /// 子查询中的Union All操作，用法：
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
    IFromQuery<T> UnionAll(Func<IFromQuery, IFromQuery<T>> subQuery);
    /// <summary>
    /// 递归CTE子查询中的Union操作，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId })
    ///         .UnionRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId })), "MenuList")
    ///     .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///     .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId, PageUrl = b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId,PageId) AS 
    /// (
    /// SELECT `Id`,`Name`,`ParentId`,`PageId` FROM `sys_menu` WHERE `Id`=1 UNION
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId`,b.`Url` AS `PageUrl` FROM MenuList a LEFT JOIN `sys_page` b ON a.`PageId`=b.`Id`
    /// </summary>
    /// <param name="subQuery">子查询，需要有Select语句，如：
    ///  f.From&lt;Menu&gt;()
    ///     .Where(x =&gt; x.Id == 1)
    ///     .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId })
    /// </param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T> UnionRecursive(Func<IFromQuery, IFromQuery<T>, IFromQuery<T>> subQuery);
    /// <summary>
    /// 递归CTE子查询中的UnionAll操作，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId })
    ///         .UnionRecursiveAll((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId })), "MenuList")
    ///     .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///     .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId, PageUrl = b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId,PageId) AS 
    /// (
    /// SELECT `Id`,`Name`,`ParentId`,`PageId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId`,b.`Url` AS `PageUrl` FROM MenuList a LEFT JOIN `sys_page` b ON a.`PageId`=b.`Id`
    /// </summary>
    /// <param name="subQuery">子查询，需要有Select语句，如：
    ///  f.From&lt;Menu&gt;()
    ///     .Where(x =&gt; x.Id == 1)
    ///     .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId })
    /// </param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T> UnionAllRecursive(Func<IFromQuery, IFromQuery<T>, IFromQuery<T>> subQuery);
    #endregion

    #region Join
    /// <summary>
    /// 添加TOther表，与现有表T做INNER JOIN关联，用法:
    /// <code>
    /// repository.From&lt;User&gt;()
    ///     .InnerJoin&lt;Order&gt;((x, y) =&gt; x.Id == y.BuyerId)
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
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T, TOther> InnerJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，与现有表T做LEFT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;User&gt;()
    ///     .LeftJoin&lt;Order&gt;((x, y) =&gt; x.Id == y.BuyerId)
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
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T, TOther> LeftJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn);
    /// <summary>
    /// 添加TOther表，与现有表T做RIGHT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;User&gt;()
    ///     .RightJoin&lt;Order&gt;((x, y) =&gt; x.Id == y.BuyerId)
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
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T, TOther> RightJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn);
    /// <summary>
    /// 在递归CTE的Union/UnionAll子句中，添加当前CTE自身引用，与上个子句中的表做INNER JOIN关联，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId })), "MenuList")
    ///     .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///     .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId, PageUrl = b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId,PageId) AS 
    /// (
    /// SELECT `Id`,`Name`,`ParentId`,`PageId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId`,b.`Url` AS `PageUrl` FROM MenuList a LEFT JOIN `sys_page` b ON a.`PageId`=b.`Id`
    /// </summary>
    /// <typeparam name="TTarget">CTE子句返回的实体类型，CTE子句中通常会有SELECT操作，返回的类型是一个匿名类</typeparam>
    /// <param name="target">CTE子句返回的对象</param>
    /// <param name="cteTableName">CTE子句表名</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T, TTarget, bool>> joinOn);
    /// <summary>
    /// 在递归CTE的Union/UnionAll子句中，添加当前CTE自身引用，与上个子句中的表做LEFT JOIN关联，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId })), "MenuList")
    ///     .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///     .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId, PageUrl = b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId,PageId) AS 
    /// (
    /// SELECT `Id`,`Name`,`ParentId`,`PageId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId`,b.`Url` AS `PageUrl` FROM MenuList a LEFT JOIN `sys_page` b ON a.`PageId`=b.`Id`
    /// </summary>
    /// <typeparam name="TTarget">CTE子句返回的实体类型，CTE子句中通常会有SELECT操作，返回的类型是一个匿名类</typeparam>
    /// <param name="target">CTE子句返回的对象</param>
    /// <param name="cteTableName">CTE子句表名</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T, TTarget, bool>> joinOn);
    /// <summary>
    /// 在递归CTE的Union/UnionAll子句中，添加当前CTE自身引用，与上个子句中的表做RIGHT JOIN关联，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId })), "MenuList")
    ///     .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///     .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId, PageUrl = b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId,PageId) AS 
    /// (
    /// SELECT `Id`,`Name`,`ParentId`,`PageId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId`,b.`Url` AS `PageUrl` FROM MenuList a LEFT JOIN `sys_page` b ON a.`PageId`=b.`Id`
    /// </summary>
    /// <typeparam name="TTarget">CTE子句返回的实体类型</typeparam>
    /// <param name="target">CTE子句返回的对象</param>
    /// <param name="cteTableName">CTE子句表名</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T, TTarget, bool>> joinOn);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T> Where(Expression<Func<T, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可以为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T> Where(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T> And(Expression<Func<T, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T> And(bool condition, Expression<Func<T, bool>> ifPredicate = null, Expression<Func<T, bool>> elsePredicate = null);
    #endregion

    #region GroupBy/OrderBy
    /// <summary>
    /// 分组查询，分组表达式groupingExpr可以是单个字段或多个字段的匿名对象，用法:
    /// <example>
    /// <code>
    /// repository.From&lt;User&gt;()
    ///    .InnerJoin&lt;Order&gt;((x, y) =&gt; x.Id == y.BuyerId)
    ///    .GroupBy((a, b) =&gt; new { a.Id, a.Name, b.CreatedAt.Date })
    ///    .OrderBy((x, a, b) =&gt; x.Grouping)
    ///    .Select((x, a, b) =&gt; new
    ///    {
    ///        x.Grouping,
    ///        OrderCount = x.Count(b.Id),
    ///        TotalAmount = x.Sum(b.TotalAmount)
    ///    })
    ///    .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `Date`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) ORDER BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE)
    /// </code>
    /// </example>
    /// </summary>
    /// <typeparam name="TGrouping">分组后的实体对象类型，可以是单个字段类型或是匿名类型</typeparam>
    /// <param name="groupingExpr">分组表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromGroupingQuery<T, TGrouping> GroupBy<TGrouping>(Expression<Func<T, TGrouping>> groupingExpr);
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy((a, b) => new { a.Id, b.Id }) 或是 OrderBy(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T> OrderBy<TFields>(Expression<Func<T, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition的布尔值，如果为true，生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b) => new { a.Id, b.Id }) 或是 OrderBy(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T> OrderBy<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr);
    /// <summary>
    /// 使用表达式fieldsExpr，生成DSC排序语句，fieldsExpr可以是一或多个字段，用法：
    /// OrderByDescending((a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T> OrderByDescending<TFields>(Expression<Func<T, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition的布尔值，如果为true，生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T> OrderByDescending<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr);
    #endregion

    /// <summary>
    /// 生成DISTINCT语句，去掉重复数据
    /// </summary>
    /// <returns>返回查询对象</returns>
    IFromQuery<T> Distinct();
    /// <summary>
    /// 只返回limit条数据
    /// </summary>
    /// <param name="limit">返回的数据条数</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T> Take(int limit);

    #region Select
    /// <summary>
    /// 直接返回所有字段
    /// </summary>
    /// <returns>返回查询对象</returns>
    IFromQuery<T> Select();
    /// <summary>
    /// 在Sql.Exists的子查询中，返回fields内容，此方法用在Sql.Exists的子查询中
    /// </summary>
    /// <param name="fields">返回的字段内容，通常是 * 或是指定常量 1 ...等</param>
    /// <returns></returns>
    IQueryAnonymousObject Select(string fields = "*");
    /// <summary>
    /// 选择指定字段返回，可以是单个字段或多个字段的匿名对象，用法：
    /// Select(f => new { f.Id, f.Name }) 或是 Select(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<TTarget> Select<TTarget>(Expression<Func<T, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定聚合字段返回，可以是单个聚合字段或多个聚合字段的匿名对象，用法：
    /// <code>
    /// repository.From&lt;Order&gt;()
    ///    .SelectAggregate((x, a) =&gt; new
    ///    {
    ///        OrderCount = x.Count(a.Id),
    ///        TotalAmount = x.Sum(a.TotalAmount)
    ///    })
    ///    .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>SELECT COUNT(`Id`) AS `OrderCount`,SUM(`TotalAmount`) AS `TotalAmount` FROM `sys_order`</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型，单个字段类型，或是多个字段的匿名类</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个聚合字段或多个聚合字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T, TTarget>> fieldsExpr);
    #endregion

    /// <summary>
    /// 返回当前查询的SQL和参数列表
    /// </summary>
    /// <param name="dbParameters">参数列表</param>
    /// <returns>当前查询的SQL</returns>
    string ToSql(out List<IDbDataParameter> dbParameters);
}
/// <summary>
/// 多表T1, T2子查询
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
public interface IFromQuery<T1, T2>
{
    #region Join
    /// <summary>
    /// 在现有表中，指定2个表进行INNER JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2&gt;()
    ///     ... ...
    ///     .InnerJoin((a, b) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2> InnerJoin(Expression<Func<T1, T2, bool>> joinOn);
    /// <summary>
    /// 在现有表中，指定2个表进行LEFT JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2&gt;()
    ///     ... ...
    ///     .LeftJoin((a, b) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2> LeftJoin(Expression<Func<T1, T2, bool>> joinOn);
    /// <summary>
    /// 在现有表中，指定2个表进行RIGHT JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2&gt;()
    ///     ... ...
    ///     .RightJoin((a, b) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2> RightJoin(Expression<Func<T1, T2, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其进行INNER JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2&gt;()
    ///     ... ...
    ///     .InnerJoin&lt;TOther&gt;((a, b) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其做LEFT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2&gt;()
    ///     ... ...
    ///     .LeftJoin&lt;TOther&gt;((a, b) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其做RIGHT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2&gt;()
    ///     ... ...
    ///     .RightJoin&lt;T2&gt;((a, b) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, TOther> RightJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn);
    /// <summary>
    /// 在递归CTE的Union/UnionAll子句中,添加当前CTE自身引用，与上个子句中的表做INNER JOIN关联，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId })), "MenuList")
    ///     .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///     .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId, PageUrl = b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId,PageId) AS 
    /// (
    /// SELECT `Id`,`Name`,`ParentId`,`PageId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId`,b.`Url` AS `PageUrl` FROM MenuList a LEFT JOIN `sys_page` b ON a.`PageId`=b.`Id`
    /// </summary>
    /// <typeparam name="TTarget">CTE子句返回的实体类型</typeparam>
    /// <param name="target">CTE子句返回对象</param>
    /// <param name="cteTableName">CTE子句表名</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, TTarget, bool>> joinOn);
    /// <summary>
    /// 在递归CTE的Union/UnionAll子句中，添加当前CTE自身引用，与上个子句中的表做LEFT JOIN关联，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId })), "MenuList")
    ///     .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///     .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId, PageUrl = b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId,PageId) AS 
    /// (
    /// SELECT `Id`,`Name`,`ParentId`,`PageId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId`,b.`Url` AS `PageUrl` FROM MenuList a LEFT JOIN `sys_page` b ON a.`PageId`=b.`Id`
    /// </summary>
    /// <typeparam name="TTarget">CTE子句返回的实体类型</typeparam>
    /// <param name="target">CTE子句返回对象</param>
    /// <param name="cteTableName">CTE子句表名</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, TTarget, bool>> joinOn);
    /// <summary>
    /// 在递归CTE的Union/UnionAll子句中，添加当前CTE自身引用，与上个子句中的表做RIGHT JOIN关联，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId })), "MenuList")
    ///     .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///     .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId, PageUrl = b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId,PageId) AS 
    /// (
    /// SELECT `Id`,`Name`,`ParentId`,`PageId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId`,b.`Url` AS `PageUrl` FROM MenuList a LEFT JOIN `sys_page` b ON a.`PageId`=b.`Id`
    /// </summary>
    /// <typeparam name="TTarget">CTE子句返回的实体类型</typeparam>
    /// <param name="target">CTE子句返回对象</param>
    /// <param name="cteTableName">CTE子句表名</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, TTarget, bool>> joinOn);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2> Where(Expression<Func<T1, T2, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2> Where(bool condition, Expression<Func<T1, T2, bool>> ifPredicate, Expression<Func<T1, T2, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，当表达式predicate为null时，将不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2> And(Expression<Func<T1, T2, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2> And(bool condition, Expression<Func<T1, T2, bool>> ifPredicate = null, Expression<Func<T1, T2, bool>> elsePredicate = null);
    #endregion

    #region GroupBy/OrderBy
    /// <summary>
    /// 分组查询，分组表达式groupingExpr可以是单个字段或多个字段的匿名对象，用法:
    /// <example>
    /// <code>
    /// repository.From&lt;User&gt;()
    ///    .InnerJoin&lt;Order&gt;((x, y) =&gt; x.Id == y.BuyerId)
    ///    .GroupBy((a, b) =&gt; new { a.Id, a.Name, b.CreatedAt.Date })
    ///    .OrderBy((x, a, b) =&gt; x.Grouping)
    ///    .Select((x, a, b) =&gt; new
    ///    {
    ///        x.Grouping,
    ///        OrderCount = x.Count(b.Id),
    ///        TotalAmount = x.Sum(b.TotalAmount)
    ///    })
    ///    .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `Date`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) ORDER BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE)
    /// </code>
    /// </example>
    /// </summary>
    /// <typeparam name="TGrouping">分组后的实体对象类型，可以是单个字段类型或是匿名类型</typeparam>
    /// <param name="groupingExpr">分组表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromGroupingQuery<T1, T2, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, TGrouping>> groupingExpr);
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy((a, b) => new { a.Id, b.Id }) 或是 OrderBy(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2> OrderBy<TFields>(Expression<Func<T1, T2, TFields>> fieldsExpr);
    /// <summary>
    /// 判断条件condition的值，当condition值为true时生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b) => new { a.Id, b.Id }) 或是 OrderBy(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, TFields>> fieldsExpr);
    /// <summary>
    /// 使用表达式fieldsExpr，生成DSC排序语句，fieldsExpr可以是一或多个字段，用法：
    /// OrderByDescending((a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2> OrderByDescending<TFields>(Expression<Func<T1, T2, TFields>> fieldsExpr);
    /// <summary>
    /// 判断条件condition的值，当condition值为true时生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 在Sql.Exists的子查询中，返回fields内容，此方法用在Sql.Exists的子查询中
    /// </summary>
    /// <param name="fields">返回的字段内容，通常是 * 或是指定常量 1 ...等</param>
    /// <returns></returns>
    IQueryAnonymousObject Select(string fields = "*");
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，用法：
    /// Select(f => new { f.Id, f.Name }) 或是 Select(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定聚合字段返回实体，单个聚合字段或多个聚合字段的匿名对象，用法：
    /// <code>
    /// repository.From&lt;Order&gt;()
    ///    .SelectAggregate((x, a) =&gt; new
    ///    {
    ///        OrderCount = x.Count(a.Id),
    ///        TotalAmount = x.Sum(a.TotalAmount)
    ///    })
    ///    .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>SELECT COUNT(`Id`) AS `OrderCount`,SUM(`TotalAmount`) AS `TotalAmount` FROM `sys_order`</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型，通常是一个匿名类</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个或多个聚合字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, TTarget>> fieldsExpr);
    #endregion

    /// <summary>
    /// 返回当前查询的SQL和参数列表
    /// </summary>
    /// <param name="dbParameters">参数列表</param>
    /// <returns>当前查询的SQL</returns>
    string ToSql(out List<IDbDataParameter> dbParameters);
}
/// <summary>
/// 多表T1, T2, T3子查询
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
public interface IFromQuery<T1, T2, T3>
{
    #region Join
    /// <summary>
    /// 在现有表中，指定2个表进行INNER JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3&gt;()
    ///     ... ...
    ///     .InnerJoin((a, b, c) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3> InnerJoin(Expression<Func<T1, T2, T3, bool>> joinOn);
    /// <summary>
    /// 在现有表中，指定2个表进行LEFT JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3&gt;()
    ///     ... ...
    ///     .LeftJoin((a, b, c) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3> LeftJoin(Expression<Func<T1, T2, T3, bool>> joinOn);
    /// <summary>
    /// 在现有表中，指定2个表进行RIGHT JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3&gt;()
    ///     ... ...
    ///     .RightJoin((a, b, c) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3> RightJoin(Expression<Func<T1, T2, T3, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其进行INNER JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3&gt;()
    ///     ... ...
    ///     .InnerJoin&lt;TOther&gt;((a, b, c) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其做LEFT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3&gt;()
    ///     ... ...
    ///     .LeftJoin&lt;TOther&gt;((a, b, c) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其做RIGHT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3&gt;()
    ///     ... ...
    ///     .RightJoin&lt;T3&gt;((a, b, c) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn);
    /// <summary>
    /// 在递归CTE的Union/UnionAll子句中,添加当前CTE自身引用，与上个子句中的表做INNER JOIN关联，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId })), "MenuList")
    ///     .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///     .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId, PageUrl = b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId,PageId) AS 
    /// (
    /// SELECT `Id`,`Name`,`ParentId`,`PageId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId`,b.`Url` AS `PageUrl` FROM MenuList a LEFT JOIN `sys_page` b ON a.`PageId`=b.`Id`
    /// </summary>
    /// <typeparam name="TTarget">CTE子句返回的实体类型</typeparam>
    /// <param name="target">CTE子句返回对象</param>
    /// <param name="cteTableName">CTE子句表名</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, TTarget, bool>> joinOn);
    /// <summary>
    /// 在递归CTE的Union/UnionAll子句中，添加当前CTE自身引用，与上个子句中的表做LEFT JOIN关联，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId })), "MenuList")
    ///     .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///     .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId, PageUrl = b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId,PageId) AS 
    /// (
    /// SELECT `Id`,`Name`,`ParentId`,`PageId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId`,b.`Url` AS `PageUrl` FROM MenuList a LEFT JOIN `sys_page` b ON a.`PageId`=b.`Id`
    /// </summary>
    /// <typeparam name="TTarget">CTE子句返回的实体类型</typeparam>
    /// <param name="target">CTE子句返回对象</param>
    /// <param name="cteTableName">CTE子句表名</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, TTarget, bool>> joinOn);
    /// <summary>
    /// 在递归CTE的Union/UnionAll子句中，添加当前CTE自身引用，与上个子句中的表做RIGHT JOIN关联，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId })), "MenuList")
    ///     .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///     .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId, PageUrl = b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId,PageId) AS 
    /// (
    /// SELECT `Id`,`Name`,`ParentId`,`PageId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId`,b.`Url` AS `PageUrl` FROM MenuList a LEFT JOIN `sys_page` b ON a.`PageId`=b.`Id`
    /// </summary>
    /// <typeparam name="TTarget">CTE子句返回的实体类型</typeparam>
    /// <param name="target">CTE子句返回对象</param>
    /// <param name="cteTableName">CTE子句表名</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, TTarget, bool>> joinOn);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3> Where(Expression<Func<T1, T2, T3, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3> Where(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate, Expression<Func<T1, T2, T3, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，当表达式predicate为null时，将不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3> And(Expression<Func<T1, T2, T3, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3> And(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, bool>> elsePredicate = null);
    #endregion

    #region GroupBy/OrderBy
    /// <summary>
    /// 分组查询，分组表达式groupingExpr可以是单个字段或多个字段的匿名对象，用法:
    /// <example>
    /// <code>
    /// repository.From&lt;User&gt;()
    ///    .InnerJoin&lt;Order&gt;((x, y) =&gt; x.Id == y.BuyerId)
    ///    .GroupBy((a, b) =&gt; new { a.Id, a.Name, b.CreatedAt.Date })
    ///    .OrderBy((x, a, b) =&gt; x.Grouping)
    ///    .Select((x, a, b) =&gt; new
    ///    {
    ///        x.Grouping,
    ///        OrderCount = x.Count(b.Id),
    ///        TotalAmount = x.Sum(b.TotalAmount)
    ///    })
    ///    .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `Date`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) ORDER BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE)
    /// </code>
    /// </example>
    /// </summary>
    /// <typeparam name="TGrouping">分组后的实体对象类型，可以是单个字段类型或是匿名类型</typeparam>
    /// <param name="groupingExpr">分组表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromGroupingQuery<T1, T2, T3, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, TGrouping>> groupingExpr);
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy((a, b) => new { a.Id, b.Id }) 或是 OrderBy(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3> OrderBy<TFields>(Expression<Func<T1, T2, T3, TFields>> fieldsExpr);
    /// <summary>
    /// 判断条件condition的值，当condition值为true时生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b) => new { a.Id, b.Id }) 或是 OrderBy(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, TFields>> fieldsExpr);
    /// <summary>
    /// 使用表达式fieldsExpr，生成DSC排序语句，fieldsExpr可以是一或多个字段，用法：
    /// OrderByDescending((a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, TFields>> fieldsExpr);
    /// <summary>
    /// 判断条件condition的值，当condition值为true时生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 在Sql.Exists的子查询中，返回fields内容，此方法用在Sql.Exists的子查询中
    /// </summary>
    /// <param name="fields">返回的字段内容，通常是 * 或是指定常量 1 ...等</param>
    /// <returns></returns>
    IQueryAnonymousObject Select(string fields = "*");
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，用法：
    /// Select(f => new { f.Id, f.Name }) 或是 Select(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定聚合字段返回实体，单个聚合字段或多个聚合字段的匿名对象，用法：
    /// <code>
    /// repository.From&lt;Order&gt;()
    ///    .SelectAggregate((x, a) =&gt; new
    ///    {
    ///        OrderCount = x.Count(a.Id),
    ///        TotalAmount = x.Sum(a.TotalAmount)
    ///    })
    ///    .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>SELECT COUNT(`Id`) AS `OrderCount`,SUM(`TotalAmount`) AS `TotalAmount` FROM `sys_order`</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型，通常是一个匿名类</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个或多个聚合字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, TTarget>> fieldsExpr);
    #endregion

    /// <summary>
    /// 返回当前查询的SQL和参数列表
    /// </summary>
    /// <param name="dbParameters">参数列表</param>
    /// <returns>当前查询的SQL</returns>
    string ToSql(out List<IDbDataParameter> dbParameters);
}
/// <summary>
/// 多表T1, T2, T3, T4子查询
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
public interface IFromQuery<T1, T2, T3, T4>
{
    #region Join
    /// <summary>
    /// 在现有表中，指定2个表进行INNER JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4&gt;()
    ///     ... ...
    ///     .InnerJoin((a, b, c, d) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4> InnerJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn);
    /// <summary>
    /// 在现有表中，指定2个表进行LEFT JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4&gt;()
    ///     ... ...
    ///     .LeftJoin((a, b, c, d) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4> LeftJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn);
    /// <summary>
    /// 在现有表中，指定2个表进行RIGHT JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4&gt;()
    ///     ... ...
    ///     .RightJoin((a, b, c, d) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4> RightJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其进行INNER JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4&gt;()
    ///     ... ...
    ///     .InnerJoin&lt;TOther&gt;((a, b, c, d) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其做LEFT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4&gt;()
    ///     ... ...
    ///     .LeftJoin&lt;TOther&gt;((a, b, c, d) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其做RIGHT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4&gt;()
    ///     ... ...
    ///     .RightJoin&lt;T4&gt;((a, b, c, d) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn);
    /// <summary>
    /// 在递归CTE的Union/UnionAll子句中,添加当前CTE自身引用，与上个子句中的表做INNER JOIN关联，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId })), "MenuList")
    ///     .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///     .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId, PageUrl = b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId,PageId) AS 
    /// (
    /// SELECT `Id`,`Name`,`ParentId`,`PageId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId`,b.`Url` AS `PageUrl` FROM MenuList a LEFT JOIN `sys_page` b ON a.`PageId`=b.`Id`
    /// </summary>
    /// <typeparam name="TTarget">CTE子句返回的实体类型</typeparam>
    /// <param name="target">CTE子句返回对象</param>
    /// <param name="cteTableName">CTE子句表名</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, TTarget, bool>> joinOn);
    /// <summary>
    /// 在递归CTE的Union/UnionAll子句中，添加当前CTE自身引用，与上个子句中的表做LEFT JOIN关联，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId })), "MenuList")
    ///     .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///     .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId, PageUrl = b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId,PageId) AS 
    /// (
    /// SELECT `Id`,`Name`,`ParentId`,`PageId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId`,b.`Url` AS `PageUrl` FROM MenuList a LEFT JOIN `sys_page` b ON a.`PageId`=b.`Id`
    /// </summary>
    /// <typeparam name="TTarget">CTE子句返回的实体类型</typeparam>
    /// <param name="target">CTE子句返回对象</param>
    /// <param name="cteTableName">CTE子句表名</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, TTarget, bool>> joinOn);
    /// <summary>
    /// 在递归CTE的Union/UnionAll子句中，添加当前CTE自身引用，与上个子句中的表做RIGHT JOIN关联，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId })), "MenuList")
    ///     .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///     .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId, PageUrl = b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId,PageId) AS 
    /// (
    /// SELECT `Id`,`Name`,`ParentId`,`PageId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId`,b.`Url` AS `PageUrl` FROM MenuList a LEFT JOIN `sys_page` b ON a.`PageId`=b.`Id`
    /// </summary>
    /// <typeparam name="TTarget">CTE子句返回的实体类型</typeparam>
    /// <param name="target">CTE子句返回对象</param>
    /// <param name="cteTableName">CTE子句表名</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, TTarget, bool>> joinOn);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4> Where(Expression<Func<T1, T2, T3, T4, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4> Where(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，当表达式predicate为null时，将不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4> And(Expression<Func<T1, T2, T3, T4, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4> And(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null);
    #endregion

    #region GroupBy/OrderBy
    /// <summary>
    /// 分组查询，分组表达式groupingExpr可以是单个字段或多个字段的匿名对象，用法:
    /// <example>
    /// <code>
    /// repository.From&lt;User&gt;()
    ///    .InnerJoin&lt;Order&gt;((x, y) =&gt; x.Id == y.BuyerId)
    ///    .GroupBy((a, b) =&gt; new { a.Id, a.Name, b.CreatedAt.Date })
    ///    .OrderBy((x, a, b) =&gt; x.Grouping)
    ///    .Select((x, a, b) =&gt; new
    ///    {
    ///        x.Grouping,
    ///        OrderCount = x.Count(b.Id),
    ///        TotalAmount = x.Sum(b.TotalAmount)
    ///    })
    ///    .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `Date`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) ORDER BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE)
    /// </code>
    /// </example>
    /// </summary>
    /// <typeparam name="TGrouping">分组后的实体对象类型，可以是单个字段类型或是匿名类型</typeparam>
    /// <param name="groupingExpr">分组表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromGroupingQuery<T1, T2, T3, T4, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, TGrouping>> groupingExpr);
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy((a, b) => new { a.Id, b.Id }) 或是 OrderBy(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr);
    /// <summary>
    /// 判断条件condition的值，当condition值为true时生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b) => new { a.Id, b.Id }) 或是 OrderBy(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr);
    /// <summary>
    /// 使用表达式fieldsExpr，生成DSC排序语句，fieldsExpr可以是一或多个字段，用法：
    /// OrderByDescending((a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr);
    /// <summary>
    /// 判断条件condition的值，当condition值为true时生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 在Sql.Exists的子查询中，返回fields内容，此方法用在Sql.Exists的子查询中
    /// </summary>
    /// <param name="fields">返回的字段内容，通常是 * 或是指定常量 1 ...等</param>
    /// <returns></returns>
    IQueryAnonymousObject Select(string fields = "*");
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，用法：
    /// Select(f => new { f.Id, f.Name }) 或是 Select(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定聚合字段返回实体，单个聚合字段或多个聚合字段的匿名对象，用法：
    /// <code>
    /// repository.From&lt;Order&gt;()
    ///    .SelectAggregate((x, a) =&gt; new
    ///    {
    ///        OrderCount = x.Count(a.Id),
    ///        TotalAmount = x.Sum(a.TotalAmount)
    ///    })
    ///    .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>SELECT COUNT(`Id`) AS `OrderCount`,SUM(`TotalAmount`) AS `TotalAmount` FROM `sys_order`</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型，通常是一个匿名类</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个或多个聚合字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, TTarget>> fieldsExpr);
    #endregion

    /// <summary>
    /// 返回当前查询的SQL和参数列表
    /// </summary>
    /// <param name="dbParameters">参数列表</param>
    /// <returns>当前查询的SQL</returns>
    string ToSql(out List<IDbDataParameter> dbParameters);
}
/// <summary>
/// 多表T1, T2, T3, T4, T5子查询
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
/// <typeparam name="T5">表T5实体类型</typeparam>
public interface IFromQuery<T1, T2, T3, T4, T5>
{
    #region Join
    /// <summary>
    /// 在现有表中，指定2个表进行INNER JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5&gt;()
    ///     ... ...
    ///     .InnerJoin((a, b, c, d, e) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn);
    /// <summary>
    /// 在现有表中，指定2个表进行LEFT JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5&gt;()
    ///     ... ...
    ///     .LeftJoin((a, b, c, d, e) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn);
    /// <summary>
    /// 在现有表中，指定2个表进行RIGHT JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5&gt;()
    ///     ... ...
    ///     .RightJoin((a, b, c, d, e) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5> RightJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其进行INNER JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5&gt;()
    ///     ... ...
    ///     .InnerJoin&lt;TOther&gt;((a, b, c, d, e) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其做LEFT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5&gt;()
    ///     ... ...
    ///     .LeftJoin&lt;TOther&gt;((a, b, c, d, e) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其做RIGHT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5&gt;()
    ///     ... ...
    ///     .RightJoin&lt;T5&gt;((a, b, c, d, e) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn);
    /// <summary>
    /// 在递归CTE的Union/UnionAll子句中,添加当前CTE自身引用，与上个子句中的表做INNER JOIN关联，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId })), "MenuList")
    ///     .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///     .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId, PageUrl = b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId,PageId) AS 
    /// (
    /// SELECT `Id`,`Name`,`ParentId`,`PageId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId`,b.`Url` AS `PageUrl` FROM MenuList a LEFT JOIN `sys_page` b ON a.`PageId`=b.`Id`
    /// </summary>
    /// <typeparam name="TTarget">CTE子句返回的实体类型</typeparam>
    /// <param name="target">CTE子句返回对象</param>
    /// <param name="cteTableName">CTE子句表名</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, TTarget, bool>> joinOn);
    /// <summary>
    /// 在递归CTE的Union/UnionAll子句中，添加当前CTE自身引用，与上个子句中的表做LEFT JOIN关联，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId })), "MenuList")
    ///     .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///     .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId, PageUrl = b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId,PageId) AS 
    /// (
    /// SELECT `Id`,`Name`,`ParentId`,`PageId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId`,b.`Url` AS `PageUrl` FROM MenuList a LEFT JOIN `sys_page` b ON a.`PageId`=b.`Id`
    /// </summary>
    /// <typeparam name="TTarget">CTE子句返回的实体类型</typeparam>
    /// <param name="target">CTE子句返回对象</param>
    /// <param name="cteTableName">CTE子句表名</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, TTarget, bool>> joinOn);
    /// <summary>
    /// 在递归CTE的Union/UnionAll子句中，添加当前CTE自身引用，与上个子句中的表做RIGHT JOIN关联，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId })), "MenuList")
    ///     .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///     .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId, PageUrl = b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId,PageId) AS 
    /// (
    /// SELECT `Id`,`Name`,`ParentId`,`PageId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId`,b.`Url` AS `PageUrl` FROM MenuList a LEFT JOIN `sys_page` b ON a.`PageId`=b.`Id`
    /// </summary>
    /// <typeparam name="TTarget">CTE子句返回的实体类型</typeparam>
    /// <param name="target">CTE子句返回对象</param>
    /// <param name="cteTableName">CTE子句表名</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, TTarget, bool>> joinOn);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5> Where(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，当表达式predicate为null时，将不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5> And(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null);
    #endregion

    #region GroupBy/OrderBy
    /// <summary>
    /// 分组查询，分组表达式groupingExpr可以是单个字段或多个字段的匿名对象，用法:
    /// <example>
    /// <code>
    /// repository.From&lt;User&gt;()
    ///    .InnerJoin&lt;Order&gt;((x, y) =&gt; x.Id == y.BuyerId)
    ///    .GroupBy((a, b) =&gt; new { a.Id, a.Name, b.CreatedAt.Date })
    ///    .OrderBy((x, a, b) =&gt; x.Grouping)
    ///    .Select((x, a, b) =&gt; new
    ///    {
    ///        x.Grouping,
    ///        OrderCount = x.Count(b.Id),
    ///        TotalAmount = x.Sum(b.TotalAmount)
    ///    })
    ///    .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `Date`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) ORDER BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE)
    /// </code>
    /// </example>
    /// </summary>
    /// <typeparam name="TGrouping">分组后的实体对象类型，可以是单个字段类型或是匿名类型</typeparam>
    /// <param name="groupingExpr">分组表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromGroupingQuery<T1, T2, T3, T4, T5, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, TGrouping>> groupingExpr);
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy((a, b) => new { a.Id, b.Id }) 或是 OrderBy(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    /// <summary>
    /// 判断条件condition的值，当condition值为true时生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b) => new { a.Id, b.Id }) 或是 OrderBy(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    /// <summary>
    /// 使用表达式fieldsExpr，生成DSC排序语句，fieldsExpr可以是一或多个字段，用法：
    /// OrderByDescending((a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    /// <summary>
    /// 判断条件condition的值，当condition值为true时生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 在Sql.Exists的子查询中，返回fields内容，此方法用在Sql.Exists的子查询中
    /// </summary>
    /// <param name="fields">返回的字段内容，通常是 * 或是指定常量 1 ...等</param>
    /// <returns></returns>
    IQueryAnonymousObject Select(string fields = "*");
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，用法：
    /// Select(f => new { f.Id, f.Name }) 或是 Select(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定聚合字段返回实体，单个聚合字段或多个聚合字段的匿名对象，用法：
    /// <code>
    /// repository.From&lt;Order&gt;()
    ///    .SelectAggregate((x, a) =&gt; new
    ///    {
    ///        OrderCount = x.Count(a.Id),
    ///        TotalAmount = x.Sum(a.TotalAmount)
    ///    })
    ///    .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>SELECT COUNT(`Id`) AS `OrderCount`,SUM(`TotalAmount`) AS `TotalAmount` FROM `sys_order`</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型，通常是一个匿名类</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个或多个聚合字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, TTarget>> fieldsExpr);
    #endregion

    /// <summary>
    /// 返回当前查询的SQL和参数列表
    /// </summary>
    /// <param name="dbParameters">参数列表</param>
    /// <returns>当前查询的SQL</returns>
    string ToSql(out List<IDbDataParameter> dbParameters);
}
/// <summary>
/// 多表T1, T2, T3, T4, T5, T6子查询
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
/// <typeparam name="T5">表T5实体类型</typeparam>
/// <typeparam name="T6">表T6实体类型</typeparam>
public interface IFromQuery<T1, T2, T3, T4, T5, T6>
{
    #region Join
    /// <summary>
    /// 在现有表中，指定2个表进行INNER JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6&gt;()
    ///     ... ...
    ///     .InnerJoin((a, b, c, d, e, f) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn);
    /// <summary>
    /// 在现有表中，指定2个表进行LEFT JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6&gt;()
    ///     ... ...
    ///     .LeftJoin((a, b, c, d, e, f) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn);
    /// <summary>
    /// 在现有表中，指定2个表进行RIGHT JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6&gt;()
    ///     ... ...
    ///     .RightJoin((a, b, c, d, e, f) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其进行INNER JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6&gt;()
    ///     ... ...
    ///     .InnerJoin&lt;TOther&gt;((a, b, c, d, e, f) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其做LEFT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6&gt;()
    ///     ... ...
    ///     .LeftJoin&lt;TOther&gt;((a, b, c, d, e, f) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其做RIGHT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6&gt;()
    ///     ... ...
    ///     .RightJoin&lt;T6&gt;((a, b, c, d, e, f) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn);
    /// <summary>
    /// 在递归CTE的Union/UnionAll子句中,添加当前CTE自身引用，与上个子句中的表做INNER JOIN关联，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId })), "MenuList")
    ///     .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///     .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId, PageUrl = b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId,PageId) AS 
    /// (
    /// SELECT `Id`,`Name`,`ParentId`,`PageId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId`,b.`Url` AS `PageUrl` FROM MenuList a LEFT JOIN `sys_page` b ON a.`PageId`=b.`Id`
    /// </summary>
    /// <typeparam name="TTarget">CTE子句返回的实体类型</typeparam>
    /// <param name="target">CTE子句返回对象</param>
    /// <param name="cteTableName">CTE子句表名</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, TTarget, bool>> joinOn);
    /// <summary>
    /// 在递归CTE的Union/UnionAll子句中，添加当前CTE自身引用，与上个子句中的表做LEFT JOIN关联，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId })), "MenuList")
    ///     .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///     .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId, PageUrl = b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId,PageId) AS 
    /// (
    /// SELECT `Id`,`Name`,`ParentId`,`PageId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId`,b.`Url` AS `PageUrl` FROM MenuList a LEFT JOIN `sys_page` b ON a.`PageId`=b.`Id`
    /// </summary>
    /// <typeparam name="TTarget">CTE子句返回的实体类型</typeparam>
    /// <param name="target">CTE子句返回对象</param>
    /// <param name="cteTableName">CTE子句表名</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, TTarget, bool>> joinOn);
    /// <summary>
    /// 在递归CTE的Union/UnionAll子句中，添加当前CTE自身引用，与上个子句中的表做RIGHT JOIN关联，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId })), "MenuList")
    ///     .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///     .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId, PageUrl = b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId,PageId) AS 
    /// (
    /// SELECT `Id`,`Name`,`ParentId`,`PageId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId`,b.`Url` AS `PageUrl` FROM MenuList a LEFT JOIN `sys_page` b ON a.`PageId`=b.`Id`
    /// </summary>
    /// <typeparam name="TTarget">CTE子句返回的实体类型</typeparam>
    /// <param name="target">CTE子句返回对象</param>
    /// <param name="cteTableName">CTE子句表名</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, TTarget, bool>> joinOn);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6> Where(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，当表达式predicate为null时，将不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6> And(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null);
    #endregion

    #region GroupBy/OrderBy
    /// <summary>
    /// 分组查询，分组表达式groupingExpr可以是单个字段或多个字段的匿名对象，用法:
    /// <example>
    /// <code>
    /// repository.From&lt;User&gt;()
    ///    .InnerJoin&lt;Order&gt;((x, y) =&gt; x.Id == y.BuyerId)
    ///    .GroupBy((a, b) =&gt; new { a.Id, a.Name, b.CreatedAt.Date })
    ///    .OrderBy((x, a, b) =&gt; x.Grouping)
    ///    .Select((x, a, b) =&gt; new
    ///    {
    ///        x.Grouping,
    ///        OrderCount = x.Count(b.Id),
    ///        TotalAmount = x.Sum(b.TotalAmount)
    ///    })
    ///    .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `Date`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) ORDER BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE)
    /// </code>
    /// </example>
    /// </summary>
    /// <typeparam name="TGrouping">分组后的实体对象类型，可以是单个字段类型或是匿名类型</typeparam>
    /// <param name="groupingExpr">分组表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, TGrouping>> groupingExpr);
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy((a, b) => new { a.Id, b.Id }) 或是 OrderBy(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr);
    /// <summary>
    /// 判断条件condition的值，当condition值为true时生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b) => new { a.Id, b.Id }) 或是 OrderBy(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr);
    /// <summary>
    /// 使用表达式fieldsExpr，生成DSC排序语句，fieldsExpr可以是一或多个字段，用法：
    /// OrderByDescending((a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr);
    /// <summary>
    /// 判断条件condition的值，当condition值为true时生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 在Sql.Exists的子查询中，返回fields内容，此方法用在Sql.Exists的子查询中
    /// </summary>
    /// <param name="fields">返回的字段内容，通常是 * 或是指定常量 1 ...等</param>
    /// <returns></returns>
    IQueryAnonymousObject Select(string fields = "*");
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，用法：
    /// Select(f => new { f.Id, f.Name }) 或是 Select(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定聚合字段返回实体，单个聚合字段或多个聚合字段的匿名对象，用法：
    /// <code>
    /// repository.From&lt;Order&gt;()
    ///    .SelectAggregate((x, a) =&gt; new
    ///    {
    ///        OrderCount = x.Count(a.Id),
    ///        TotalAmount = x.Sum(a.TotalAmount)
    ///    })
    ///    .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>SELECT COUNT(`Id`) AS `OrderCount`,SUM(`TotalAmount`) AS `TotalAmount` FROM `sys_order`</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型，通常是一个匿名类</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个或多个聚合字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr);
    #endregion

    /// <summary>
    /// 返回当前查询的SQL和参数列表
    /// </summary>
    /// <param name="dbParameters">参数列表</param>
    /// <returns>当前查询的SQL</returns>
    string ToSql(out List<IDbDataParameter> dbParameters);
}
/// <summary>
/// 多表T1, T2, T3, T4, T5, T6, T7子查询
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
/// <typeparam name="T5">表T5实体类型</typeparam>
/// <typeparam name="T6">表T6实体类型</typeparam>
/// <typeparam name="T7">表T7实体类型</typeparam>
public interface IFromQuery<T1, T2, T3, T4, T5, T6, T7>
{
    #region Join
    /// <summary>
    /// 在现有表中，指定2个表进行INNER JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7&gt;()
    ///     ... ...
    ///     .InnerJoin((a, b, c, d, e, f, g) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> joinOn);
    /// <summary>
    /// 在现有表中，指定2个表进行LEFT JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7&gt;()
    ///     ... ...
    ///     .LeftJoin((a, b, c, d, e, f, g) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> joinOn);
    /// <summary>
    /// 在现有表中，指定2个表进行RIGHT JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7&gt;()
    ///     ... ...
    ///     .RightJoin((a, b, c, d, e, f, g) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其进行INNER JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7&gt;()
    ///     ... ...
    ///     .InnerJoin&lt;TOther&gt;((a, b, c, d, e, f, g) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其做LEFT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7&gt;()
    ///     ... ...
    ///     .LeftJoin&lt;TOther&gt;((a, b, c, d, e, f, g) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其做RIGHT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7&gt;()
    ///     ... ...
    ///     .RightJoin&lt;T7&gt;((a, b, c, d, e, f, g) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn);
    /// <summary>
    /// 在递归CTE的Union/UnionAll子句中,添加当前CTE自身引用，与上个子句中的表做INNER JOIN关联，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId })), "MenuList")
    ///     .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///     .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId, PageUrl = b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId,PageId) AS 
    /// (
    /// SELECT `Id`,`Name`,`ParentId`,`PageId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId`,b.`Url` AS `PageUrl` FROM MenuList a LEFT JOIN `sys_page` b ON a.`PageId`=b.`Id`
    /// </summary>
    /// <typeparam name="TTarget">CTE子句返回的实体类型</typeparam>
    /// <param name="target">CTE子句返回对象</param>
    /// <param name="cteTableName">CTE子句表名</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TTarget, bool>> joinOn);
    /// <summary>
    /// 在递归CTE的Union/UnionAll子句中，添加当前CTE自身引用，与上个子句中的表做LEFT JOIN关联，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId })), "MenuList")
    ///     .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///     .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId, PageUrl = b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId,PageId) AS 
    /// (
    /// SELECT `Id`,`Name`,`ParentId`,`PageId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId`,b.`Url` AS `PageUrl` FROM MenuList a LEFT JOIN `sys_page` b ON a.`PageId`=b.`Id`
    /// </summary>
    /// <typeparam name="TTarget">CTE子句返回的实体类型</typeparam>
    /// <param name="target">CTE子句返回对象</param>
    /// <param name="cteTableName">CTE子句表名</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TTarget, bool>> joinOn);
    /// <summary>
    /// 在递归CTE的Union/UnionAll子句中，添加当前CTE自身引用，与上个子句中的表做RIGHT JOIN关联，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId })), "MenuList")
    ///     .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///     .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId, PageUrl = b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId,PageId) AS 
    /// (
    /// SELECT `Id`,`Name`,`ParentId`,`PageId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId`,b.`Url` AS `PageUrl` FROM MenuList a LEFT JOIN `sys_page` b ON a.`PageId`=b.`Id`
    /// </summary>
    /// <typeparam name="TTarget">CTE子句返回的实体类型</typeparam>
    /// <param name="target">CTE子句返回对象</param>
    /// <param name="cteTableName">CTE子句表名</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TTarget, bool>> joinOn);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，当表达式predicate为null时，将不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> elsePredicate = null);
    #endregion

    #region GroupBy/OrderBy
    /// <summary>
    /// 分组查询，分组表达式groupingExpr可以是单个字段或多个字段的匿名对象，用法:
    /// <example>
    /// <code>
    /// repository.From&lt;User&gt;()
    ///    .InnerJoin&lt;Order&gt;((x, y) =&gt; x.Id == y.BuyerId)
    ///    .GroupBy((a, b) =&gt; new { a.Id, a.Name, b.CreatedAt.Date })
    ///    .OrderBy((x, a, b) =&gt; x.Grouping)
    ///    .Select((x, a, b) =&gt; new
    ///    {
    ///        x.Grouping,
    ///        OrderCount = x.Count(b.Id),
    ///        TotalAmount = x.Sum(b.TotalAmount)
    ///    })
    ///    .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `Date`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) ORDER BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE)
    /// </code>
    /// </example>
    /// </summary>
    /// <typeparam name="TGrouping">分组后的实体对象类型，可以是单个字段类型或是匿名类型</typeparam>
    /// <param name="groupingExpr">分组表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TGrouping>> groupingExpr);
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy((a, b) => new { a.Id, b.Id }) 或是 OrderBy(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr);
    /// <summary>
    /// 判断条件condition的值，当condition值为true时生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b) => new { a.Id, b.Id }) 或是 OrderBy(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr);
    /// <summary>
    /// 使用表达式fieldsExpr，生成DSC排序语句，fieldsExpr可以是一或多个字段，用法：
    /// OrderByDescending((a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr);
    /// <summary>
    /// 判断条件condition的值，当condition值为true时生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 在Sql.Exists的子查询中，返回fields内容，此方法用在Sql.Exists的子查询中
    /// </summary>
    /// <param name="fields">返回的字段内容，通常是 * 或是指定常量 1 ...等</param>
    /// <returns></returns>
    IQueryAnonymousObject Select(string fields = "*");
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，用法：
    /// Select(f => new { f.Id, f.Name }) 或是 Select(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定聚合字段返回实体，单个聚合字段或多个聚合字段的匿名对象，用法：
    /// <code>
    /// repository.From&lt;Order&gt;()
    ///    .SelectAggregate((x, a) =&gt; new
    ///    {
    ///        OrderCount = x.Count(a.Id),
    ///        TotalAmount = x.Sum(a.TotalAmount)
    ///    })
    ///    .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>SELECT COUNT(`Id`) AS `OrderCount`,SUM(`TotalAmount`) AS `TotalAmount` FROM `sys_order`</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型，通常是一个匿名类</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个或多个聚合字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, TTarget>> fieldsExpr);
    #endregion

    /// <summary>
    /// 返回当前查询的SQL和参数列表
    /// </summary>
    /// <param name="dbParameters">参数列表</param>
    /// <returns>当前查询的SQL</returns>
    string ToSql(out List<IDbDataParameter> dbParameters);
}
/// <summary>
/// 多表T1, T2, T3, T4, T5, T6, T7, T8子查询
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
/// <typeparam name="T5">表T5实体类型</typeparam>
/// <typeparam name="T6">表T6实体类型</typeparam>
/// <typeparam name="T7">表T7实体类型</typeparam>
/// <typeparam name="T8">表T8实体类型</typeparam>
public interface IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8>
{
    #region Join
    /// <summary>
    /// 在现有表中，指定2个表进行INNER JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8&gt;()
    ///     ... ...
    ///     .InnerJoin((a, b, c, d, e, f, g, h) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> joinOn);
    /// <summary>
    /// 在现有表中，指定2个表进行LEFT JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8&gt;()
    ///     ... ...
    ///     .LeftJoin((a, b, c, d, e, f, g, h) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> joinOn);
    /// <summary>
    /// 在现有表中，指定2个表进行RIGHT JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8&gt;()
    ///     ... ...
    ///     .RightJoin((a, b, c, d, e, f, g, h) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其进行INNER JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8&gt;()
    ///     ... ...
    ///     .InnerJoin&lt;TOther&gt;((a, b, c, d, e, f, g, h) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其做LEFT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8&gt;()
    ///     ... ...
    ///     .LeftJoin&lt;TOther&gt;((a, b, c, d, e, f, g, h) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其做RIGHT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8&gt;()
    ///     ... ...
    ///     .RightJoin&lt;T8&gt;((a, b, c, d, e, f, g, h) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn);
    /// <summary>
    /// 在递归CTE的Union/UnionAll子句中,添加当前CTE自身引用，与上个子句中的表做INNER JOIN关联，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId })), "MenuList")
    ///     .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///     .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId, PageUrl = b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId,PageId) AS 
    /// (
    /// SELECT `Id`,`Name`,`ParentId`,`PageId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId`,b.`Url` AS `PageUrl` FROM MenuList a LEFT JOIN `sys_page` b ON a.`PageId`=b.`Id`
    /// </summary>
    /// <typeparam name="TTarget">CTE子句返回的实体类型</typeparam>
    /// <param name="target">CTE子句返回对象</param>
    /// <param name="cteTableName">CTE子句表名</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TTarget, bool>> joinOn);
    /// <summary>
    /// 在递归CTE的Union/UnionAll子句中，添加当前CTE自身引用，与上个子句中的表做LEFT JOIN关联，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId })), "MenuList")
    ///     .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///     .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId, PageUrl = b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId,PageId) AS 
    /// (
    /// SELECT `Id`,`Name`,`ParentId`,`PageId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId`,b.`Url` AS `PageUrl` FROM MenuList a LEFT JOIN `sys_page` b ON a.`PageId`=b.`Id`
    /// </summary>
    /// <typeparam name="TTarget">CTE子句返回的实体类型</typeparam>
    /// <param name="target">CTE子句返回对象</param>
    /// <param name="cteTableName">CTE子句表名</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TTarget, bool>> joinOn);
    /// <summary>
    /// 在递归CTE的Union/UnionAll子句中，添加当前CTE自身引用，与上个子句中的表做RIGHT JOIN关联，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId })), "MenuList")
    ///     .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///     .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId, PageUrl = b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId,PageId) AS 
    /// (
    /// SELECT `Id`,`Name`,`ParentId`,`PageId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId`,b.`Url` AS `PageUrl` FROM MenuList a LEFT JOIN `sys_page` b ON a.`PageId`=b.`Id`
    /// </summary>
    /// <typeparam name="TTarget">CTE子句返回的实体类型</typeparam>
    /// <param name="target">CTE子句返回对象</param>
    /// <param name="cteTableName">CTE子句表名</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TTarget, bool>> joinOn);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，当表达式predicate为null时，将不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> elsePredicate = null);
    #endregion

    #region GroupBy/OrderBy
    /// <summary>
    /// 分组查询，分组表达式groupingExpr可以是单个字段或多个字段的匿名对象，用法:
    /// <example>
    /// <code>
    /// repository.From&lt;User&gt;()
    ///    .InnerJoin&lt;Order&gt;((x, y) =&gt; x.Id == y.BuyerId)
    ///    .GroupBy((a, b) =&gt; new { a.Id, a.Name, b.CreatedAt.Date })
    ///    .OrderBy((x, a, b) =&gt; x.Grouping)
    ///    .Select((x, a, b) =&gt; new
    ///    {
    ///        x.Grouping,
    ///        OrderCount = x.Count(b.Id),
    ///        TotalAmount = x.Sum(b.TotalAmount)
    ///    })
    ///    .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `Date`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) ORDER BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE)
    /// </code>
    /// </example>
    /// </summary>
    /// <typeparam name="TGrouping">分组后的实体对象类型，可以是单个字段类型或是匿名类型</typeparam>
    /// <param name="groupingExpr">分组表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping>> groupingExpr);
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy((a, b) => new { a.Id, b.Id }) 或是 OrderBy(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr);
    /// <summary>
    /// 判断条件condition的值，当condition值为true时生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b) => new { a.Id, b.Id }) 或是 OrderBy(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr);
    /// <summary>
    /// 使用表达式fieldsExpr，生成DSC排序语句，fieldsExpr可以是一或多个字段，用法：
    /// OrderByDescending((a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr);
    /// <summary>
    /// 判断条件condition的值，当condition值为true时生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 在Sql.Exists的子查询中，返回fields内容，此方法用在Sql.Exists的子查询中
    /// </summary>
    /// <param name="fields">返回的字段内容，通常是 * 或是指定常量 1 ...等</param>
    /// <returns></returns>
    IQueryAnonymousObject Select(string fields = "*");
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，用法：
    /// Select(f => new { f.Id, f.Name }) 或是 Select(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定聚合字段返回实体，单个聚合字段或多个聚合字段的匿名对象，用法：
    /// <code>
    /// repository.From&lt;Order&gt;()
    ///    .SelectAggregate((x, a) =&gt; new
    ///    {
    ///        OrderCount = x.Count(a.Id),
    ///        TotalAmount = x.Sum(a.TotalAmount)
    ///    })
    ///    .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>SELECT COUNT(`Id`) AS `OrderCount`,SUM(`TotalAmount`) AS `TotalAmount` FROM `sys_order`</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型，通常是一个匿名类</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个或多个聚合字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, TTarget>> fieldsExpr);
    #endregion

    /// <summary>
    /// 返回当前查询的SQL和参数列表
    /// </summary>
    /// <param name="dbParameters">参数列表</param>
    /// <returns>当前查询的SQL</returns>
    string ToSql(out List<IDbDataParameter> dbParameters);
}
/// <summary>
/// 多表T1, T2, T3, T4, T5, T6, T7, T8, T9子查询
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
public interface IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9>
{
    #region Join
    /// <summary>
    /// 在现有表中，指定2个表进行INNER JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9&gt;()
    ///     ... ...
    ///     .InnerJoin((a, b, c, d, e, f, g, h, i) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinOn);
    /// <summary>
    /// 在现有表中，指定2个表进行LEFT JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9&gt;()
    ///     ... ...
    ///     .LeftJoin((a, b, c, d, e, f, g, h, i) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinOn);
    /// <summary>
    /// 在现有表中，指定2个表进行RIGHT JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9&gt;()
    ///     ... ...
    ///     .RightJoin((a, b, c, d, e, f, g, h, i) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其进行INNER JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9&gt;()
    ///     ... ...
    ///     .InnerJoin&lt;TOther&gt;((a, b, c, d, e, f, g, h, i) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其做LEFT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9&gt;()
    ///     ... ...
    ///     .LeftJoin&lt;TOther&gt;((a, b, c, d, e, f, g, h, i) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其做RIGHT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9&gt;()
    ///     ... ...
    ///     .RightJoin&lt;T9&gt;((a, b, c, d, e, f, g, h, i) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn);
    /// <summary>
    /// 在递归CTE的Union/UnionAll子句中,添加当前CTE自身引用，与上个子句中的表做INNER JOIN关联，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId })), "MenuList")
    ///     .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///     .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId, PageUrl = b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId,PageId) AS 
    /// (
    /// SELECT `Id`,`Name`,`ParentId`,`PageId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId`,b.`Url` AS `PageUrl` FROM MenuList a LEFT JOIN `sys_page` b ON a.`PageId`=b.`Id`
    /// </summary>
    /// <typeparam name="TTarget">CTE子句返回的实体类型</typeparam>
    /// <param name="target">CTE子句返回对象</param>
    /// <param name="cteTableName">CTE子句表名</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget, bool>> joinOn);
    /// <summary>
    /// 在递归CTE的Union/UnionAll子句中，添加当前CTE自身引用，与上个子句中的表做LEFT JOIN关联，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId })), "MenuList")
    ///     .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///     .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId, PageUrl = b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId,PageId) AS 
    /// (
    /// SELECT `Id`,`Name`,`ParentId`,`PageId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId`,b.`Url` AS `PageUrl` FROM MenuList a LEFT JOIN `sys_page` b ON a.`PageId`=b.`Id`
    /// </summary>
    /// <typeparam name="TTarget">CTE子句返回的实体类型</typeparam>
    /// <param name="target">CTE子句返回对象</param>
    /// <param name="cteTableName">CTE子句表名</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget, bool>> joinOn);
    /// <summary>
    /// 在递归CTE的Union/UnionAll子句中，添加当前CTE自身引用，与上个子句中的表做RIGHT JOIN关联，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId })), "MenuList")
    ///     .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///     .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId, PageUrl = b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId,PageId) AS 
    /// (
    /// SELECT `Id`,`Name`,`ParentId`,`PageId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId`,b.`Url` AS `PageUrl` FROM MenuList a LEFT JOIN `sys_page` b ON a.`PageId`=b.`Id`
    /// </summary>
    /// <typeparam name="TTarget">CTE子句返回的实体类型</typeparam>
    /// <param name="target">CTE子句返回对象</param>
    /// <param name="cteTableName">CTE子句表名</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget, bool>> joinOn);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，当表达式predicate为null时，将不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> elsePredicate = null);
    #endregion

    #region GroupBy/OrderBy
    /// <summary>
    /// 分组查询，分组表达式groupingExpr可以是单个字段或多个字段的匿名对象，用法:
    /// <example>
    /// <code>
    /// repository.From&lt;User&gt;()
    ///    .InnerJoin&lt;Order&gt;((x, y) =&gt; x.Id == y.BuyerId)
    ///    .GroupBy((a, b) =&gt; new { a.Id, a.Name, b.CreatedAt.Date })
    ///    .OrderBy((x, a, b) =&gt; x.Grouping)
    ///    .Select((x, a, b) =&gt; new
    ///    {
    ///        x.Grouping,
    ///        OrderCount = x.Count(b.Id),
    ///        TotalAmount = x.Sum(b.TotalAmount)
    ///    })
    ///    .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `Date`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) ORDER BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE)
    /// </code>
    /// </example>
    /// </summary>
    /// <typeparam name="TGrouping">分组后的实体对象类型，可以是单个字段类型或是匿名类型</typeparam>
    /// <param name="groupingExpr">分组表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping>> groupingExpr);
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy((a, b) => new { a.Id, b.Id }) 或是 OrderBy(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr);
    /// <summary>
    /// 判断条件condition的值，当condition值为true时生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b) => new { a.Id, b.Id }) 或是 OrderBy(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr);
    /// <summary>
    /// 使用表达式fieldsExpr，生成DSC排序语句，fieldsExpr可以是一或多个字段，用法：
    /// OrderByDescending((a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr);
    /// <summary>
    /// 判断条件condition的值，当condition值为true时生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 在Sql.Exists的子查询中，返回fields内容，此方法用在Sql.Exists的子查询中
    /// </summary>
    /// <param name="fields">返回的字段内容，通常是 * 或是指定常量 1 ...等</param>
    /// <returns></returns>
    IQueryAnonymousObject Select(string fields = "*");
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，用法：
    /// Select(f => new { f.Id, f.Name }) 或是 Select(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定聚合字段返回实体，单个聚合字段或多个聚合字段的匿名对象，用法：
    /// <code>
    /// repository.From&lt;Order&gt;()
    ///    .SelectAggregate((x, a) =&gt; new
    ///    {
    ///        OrderCount = x.Count(a.Id),
    ///        TotalAmount = x.Sum(a.TotalAmount)
    ///    })
    ///    .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>SELECT COUNT(`Id`) AS `OrderCount`,SUM(`TotalAmount`) AS `TotalAmount` FROM `sys_order`</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型，通常是一个匿名类</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个或多个聚合字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget>> fieldsExpr);
    #endregion

    /// <summary>
    /// 返回当前查询的SQL和参数列表
    /// </summary>
    /// <param name="dbParameters">参数列表</param>
    /// <returns>当前查询的SQL</returns>
    string ToSql(out List<IDbDataParameter> dbParameters);
}
/// <summary>
/// 多表T1, T2, T3, T4, T5, T6, T7, T8, T9, T10子查询
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
public interface IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>
{
    #region Join
    /// <summary>
    /// 在现有表中，指定2个表进行INNER JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10&gt;()
    ///     ... ...
    ///     .InnerJoin((a, b, c, d, e, f, g, h, i, j) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinOn);
    /// <summary>
    /// 在现有表中，指定2个表进行LEFT JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10&gt;()
    ///     ... ...
    ///     .LeftJoin((a, b, c, d, e, f, g, h, i, j) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinOn);
    /// <summary>
    /// 在现有表中，指定2个表进行RIGHT JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10&gt;()
    ///     ... ...
    ///     .RightJoin((a, b, c, d, e, f, g, h, i, j) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其进行INNER JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10&gt;()
    ///     ... ...
    ///     .InnerJoin&lt;TOther&gt;((a, b, c, d, e, f, g, h, i, j) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其做LEFT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10&gt;()
    ///     ... ...
    ///     .LeftJoin&lt;TOther&gt;((a, b, c, d, e, f, g, h, i, j) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其做RIGHT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10&gt;()
    ///     ... ...
    ///     .RightJoin&lt;T10&gt;((a, b, c, d, e, f, g, h, i, j) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn);
    /// <summary>
    /// 在递归CTE的Union/UnionAll子句中,添加当前CTE自身引用，与上个子句中的表做INNER JOIN关联，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId })), "MenuList")
    ///     .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///     .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId, PageUrl = b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId,PageId) AS 
    /// (
    /// SELECT `Id`,`Name`,`ParentId`,`PageId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId`,b.`Url` AS `PageUrl` FROM MenuList a LEFT JOIN `sys_page` b ON a.`PageId`=b.`Id`
    /// </summary>
    /// <typeparam name="TTarget">CTE子句返回的实体类型</typeparam>
    /// <param name="target">CTE子句返回对象</param>
    /// <param name="cteTableName">CTE子句表名</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget, bool>> joinOn);
    /// <summary>
    /// 在递归CTE的Union/UnionAll子句中，添加当前CTE自身引用，与上个子句中的表做LEFT JOIN关联，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId })), "MenuList")
    ///     .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///     .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId, PageUrl = b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId,PageId) AS 
    /// (
    /// SELECT `Id`,`Name`,`ParentId`,`PageId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId`,b.`Url` AS `PageUrl` FROM MenuList a LEFT JOIN `sys_page` b ON a.`PageId`=b.`Id`
    /// </summary>
    /// <typeparam name="TTarget">CTE子句返回的实体类型</typeparam>
    /// <param name="target">CTE子句返回对象</param>
    /// <param name="cteTableName">CTE子句表名</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget, bool>> joinOn);
    /// <summary>
    /// 在递归CTE的Union/UnionAll子句中，添加当前CTE自身引用，与上个子句中的表做RIGHT JOIN关联，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId })), "MenuList")
    ///     .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///     .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId, PageUrl = b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId,PageId) AS 
    /// (
    /// SELECT `Id`,`Name`,`ParentId`,`PageId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId`,b.`Url` AS `PageUrl` FROM MenuList a LEFT JOIN `sys_page` b ON a.`PageId`=b.`Id`
    /// </summary>
    /// <typeparam name="TTarget">CTE子句返回的实体类型</typeparam>
    /// <param name="target">CTE子句返回对象</param>
    /// <param name="cteTableName">CTE子句表名</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget, bool>> joinOn);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，当表达式predicate为null时，将不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> elsePredicate = null);
    #endregion

    #region GroupBy/OrderBy
    /// <summary>
    /// 分组查询，分组表达式groupingExpr可以是单个字段或多个字段的匿名对象，用法:
    /// <example>
    /// <code>
    /// repository.From&lt;User&gt;()
    ///    .InnerJoin&lt;Order&gt;((x, y) =&gt; x.Id == y.BuyerId)
    ///    .GroupBy((a, b) =&gt; new { a.Id, a.Name, b.CreatedAt.Date })
    ///    .OrderBy((x, a, b) =&gt; x.Grouping)
    ///    .Select((x, a, b) =&gt; new
    ///    {
    ///        x.Grouping,
    ///        OrderCount = x.Count(b.Id),
    ///        TotalAmount = x.Sum(b.TotalAmount)
    ///    })
    ///    .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `Date`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) ORDER BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE)
    /// </code>
    /// </example>
    /// </summary>
    /// <typeparam name="TGrouping">分组后的实体对象类型，可以是单个字段类型或是匿名类型</typeparam>
    /// <param name="groupingExpr">分组表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping>> groupingExpr);
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy((a, b) => new { a.Id, b.Id }) 或是 OrderBy(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr);
    /// <summary>
    /// 判断条件condition的值，当condition值为true时生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b) => new { a.Id, b.Id }) 或是 OrderBy(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr);
    /// <summary>
    /// 使用表达式fieldsExpr，生成DSC排序语句，fieldsExpr可以是一或多个字段，用法：
    /// OrderByDescending((a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr);
    /// <summary>
    /// 判断条件condition的值，当condition值为true时生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 在Sql.Exists的子查询中，返回fields内容，此方法用在Sql.Exists的子查询中
    /// </summary>
    /// <param name="fields">返回的字段内容，通常是 * 或是指定常量 1 ...等</param>
    /// <returns></returns>
    IQueryAnonymousObject Select(string fields = "*");
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，用法：
    /// Select(f => new { f.Id, f.Name }) 或是 Select(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定聚合字段返回实体，单个聚合字段或多个聚合字段的匿名对象，用法：
    /// <code>
    /// repository.From&lt;Order&gt;()
    ///    .SelectAggregate((x, a) =&gt; new
    ///    {
    ///        OrderCount = x.Count(a.Id),
    ///        TotalAmount = x.Sum(a.TotalAmount)
    ///    })
    ///    .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>SELECT COUNT(`Id`) AS `OrderCount`,SUM(`TotalAmount`) AS `TotalAmount` FROM `sys_order`</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型，通常是一个匿名类</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个或多个聚合字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget>> fieldsExpr);
    #endregion

    /// <summary>
    /// 返回当前查询的SQL和参数列表
    /// </summary>
    /// <param name="dbParameters">参数列表</param>
    /// <returns>当前查询的SQL</returns>
    string ToSql(out List<IDbDataParameter> dbParameters);
}
/// <summary>
/// 多表T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11子查询
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
public interface IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>
{
    #region Join
    /// <summary>
    /// 在现有表中，指定2个表进行INNER JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11&gt;()
    ///     ... ...
    ///     .InnerJoin((a, b, c, d, e, f, g, h, i, j, k) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinOn);
    /// <summary>
    /// 在现有表中，指定2个表进行LEFT JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11&gt;()
    ///     ... ...
    ///     .LeftJoin((a, b, c, d, e, f, g, h, i, j, k) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinOn);
    /// <summary>
    /// 在现有表中，指定2个表进行RIGHT JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11&gt;()
    ///     ... ...
    ///     .RightJoin((a, b, c, d, e, f, g, h, i, j, k) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其进行INNER JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11&gt;()
    ///     ... ...
    ///     .InnerJoin&lt;TOther&gt;((a, b, c, d, e, f, g, h, i, j, k) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其做LEFT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11&gt;()
    ///     ... ...
    ///     .LeftJoin&lt;TOther&gt;((a, b, c, d, e, f, g, h, i, j, k) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其做RIGHT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11&gt;()
    ///     ... ...
    ///     .RightJoin&lt;T11&gt;((a, b, c, d, e, f, g, h, i, j, k) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn);
    /// <summary>
    /// 在递归CTE的Union/UnionAll子句中,添加当前CTE自身引用，与上个子句中的表做INNER JOIN关联，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId })), "MenuList")
    ///     .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///     .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId, PageUrl = b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId,PageId) AS 
    /// (
    /// SELECT `Id`,`Name`,`ParentId`,`PageId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId`,b.`Url` AS `PageUrl` FROM MenuList a LEFT JOIN `sys_page` b ON a.`PageId`=b.`Id`
    /// </summary>
    /// <typeparam name="TTarget">CTE子句返回的实体类型</typeparam>
    /// <param name="target">CTE子句返回对象</param>
    /// <param name="cteTableName">CTE子句表名</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget, bool>> joinOn);
    /// <summary>
    /// 在递归CTE的Union/UnionAll子句中，添加当前CTE自身引用，与上个子句中的表做LEFT JOIN关联，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId })), "MenuList")
    ///     .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///     .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId, PageUrl = b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId,PageId) AS 
    /// (
    /// SELECT `Id`,`Name`,`ParentId`,`PageId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId`,b.`Url` AS `PageUrl` FROM MenuList a LEFT JOIN `sys_page` b ON a.`PageId`=b.`Id`
    /// </summary>
    /// <typeparam name="TTarget">CTE子句返回的实体类型</typeparam>
    /// <param name="target">CTE子句返回对象</param>
    /// <param name="cteTableName">CTE子句表名</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget, bool>> joinOn);
    /// <summary>
    /// 在递归CTE的Union/UnionAll子句中，添加当前CTE自身引用，与上个子句中的表做RIGHT JOIN关联，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId })), "MenuList")
    ///     .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///     .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId, PageUrl = b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId,PageId) AS 
    /// (
    /// SELECT `Id`,`Name`,`ParentId`,`PageId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId`,b.`Url` AS `PageUrl` FROM MenuList a LEFT JOIN `sys_page` b ON a.`PageId`=b.`Id`
    /// </summary>
    /// <typeparam name="TTarget">CTE子句返回的实体类型</typeparam>
    /// <param name="target">CTE子句返回对象</param>
    /// <param name="cteTableName">CTE子句表名</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget, bool>> joinOn);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，当表达式predicate为null时，将不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> elsePredicate = null);
    #endregion

    #region GroupBy/OrderBy
    /// <summary>
    /// 分组查询，分组表达式groupingExpr可以是单个字段或多个字段的匿名对象，用法:
    /// <example>
    /// <code>
    /// repository.From&lt;User&gt;()
    ///    .InnerJoin&lt;Order&gt;((x, y) =&gt; x.Id == y.BuyerId)
    ///    .GroupBy((a, b) =&gt; new { a.Id, a.Name, b.CreatedAt.Date })
    ///    .OrderBy((x, a, b) =&gt; x.Grouping)
    ///    .Select((x, a, b) =&gt; new
    ///    {
    ///        x.Grouping,
    ///        OrderCount = x.Count(b.Id),
    ///        TotalAmount = x.Sum(b.TotalAmount)
    ///    })
    ///    .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `Date`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) ORDER BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE)
    /// </code>
    /// </example>
    /// </summary>
    /// <typeparam name="TGrouping">分组后的实体对象类型，可以是单个字段类型或是匿名类型</typeparam>
    /// <param name="groupingExpr">分组表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping>> groupingExpr);
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy((a, b) => new { a.Id, b.Id }) 或是 OrderBy(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr);
    /// <summary>
    /// 判断条件condition的值，当condition值为true时生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b) => new { a.Id, b.Id }) 或是 OrderBy(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr);
    /// <summary>
    /// 使用表达式fieldsExpr，生成DSC排序语句，fieldsExpr可以是一或多个字段，用法：
    /// OrderByDescending((a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr);
    /// <summary>
    /// 判断条件condition的值，当condition值为true时生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 在Sql.Exists的子查询中，返回fields内容，此方法用在Sql.Exists的子查询中
    /// </summary>
    /// <param name="fields">返回的字段内容，通常是 * 或是指定常量 1 ...等</param>
    /// <returns></returns>
    IQueryAnonymousObject Select(string fields = "*");
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，用法：
    /// Select(f => new { f.Id, f.Name }) 或是 Select(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定聚合字段返回实体，单个聚合字段或多个聚合字段的匿名对象，用法：
    /// <code>
    /// repository.From&lt;Order&gt;()
    ///    .SelectAggregate((x, a) =&gt; new
    ///    {
    ///        OrderCount = x.Count(a.Id),
    ///        TotalAmount = x.Sum(a.TotalAmount)
    ///    })
    ///    .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>SELECT COUNT(`Id`) AS `OrderCount`,SUM(`TotalAmount`) AS `TotalAmount` FROM `sys_order`</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型，通常是一个匿名类</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个或多个聚合字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget>> fieldsExpr);
    #endregion

    /// <summary>
    /// 返回当前查询的SQL和参数列表
    /// </summary>
    /// <param name="dbParameters">参数列表</param>
    /// <returns>当前查询的SQL</returns>
    string ToSql(out List<IDbDataParameter> dbParameters);
}
/// <summary>
/// 多表T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12子查询
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
public interface IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>
{
    #region Join
    /// <summary>
    /// 在现有表中，指定2个表进行INNER JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12&gt;()
    ///     ... ...
    ///     .InnerJoin((a, b, c, d, e, f, g, h, i, j, k, l) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinOn);
    /// <summary>
    /// 在现有表中，指定2个表进行LEFT JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12&gt;()
    ///     ... ...
    ///     .LeftJoin((a, b, c, d, e, f, g, h, i, j, k, l) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinOn);
    /// <summary>
    /// 在现有表中，指定2个表进行RIGHT JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12&gt;()
    ///     ... ...
    ///     .RightJoin((a, b, c, d, e, f, g, h, i, j, k, l) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其进行INNER JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12&gt;()
    ///     ... ...
    ///     .InnerJoin&lt;TOther&gt;((a, b, c, d, e, f, g, h, i, j, k, l) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其做LEFT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12&gt;()
    ///     ... ...
    ///     .LeftJoin&lt;TOther&gt;((a, b, c, d, e, f, g, h, i, j, k, l) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其做RIGHT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12&gt;()
    ///     ... ...
    ///     .RightJoin&lt;T12&gt;((a, b, c, d, e, f, g, h, i, j, k, l) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn);
    /// <summary>
    /// 在递归CTE的Union/UnionAll子句中,添加当前CTE自身引用，与上个子句中的表做INNER JOIN关联，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId })), "MenuList")
    ///     .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///     .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId, PageUrl = b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId,PageId) AS 
    /// (
    /// SELECT `Id`,`Name`,`ParentId`,`PageId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId`,b.`Url` AS `PageUrl` FROM MenuList a LEFT JOIN `sys_page` b ON a.`PageId`=b.`Id`
    /// </summary>
    /// <typeparam name="TTarget">CTE子句返回的实体类型</typeparam>
    /// <param name="target">CTE子句返回对象</param>
    /// <param name="cteTableName">CTE子句表名</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget, bool>> joinOn);
    /// <summary>
    /// 在递归CTE的Union/UnionAll子句中，添加当前CTE自身引用，与上个子句中的表做LEFT JOIN关联，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId })), "MenuList")
    ///     .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///     .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId, PageUrl = b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId,PageId) AS 
    /// (
    /// SELECT `Id`,`Name`,`ParentId`,`PageId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId`,b.`Url` AS `PageUrl` FROM MenuList a LEFT JOIN `sys_page` b ON a.`PageId`=b.`Id`
    /// </summary>
    /// <typeparam name="TTarget">CTE子句返回的实体类型</typeparam>
    /// <param name="target">CTE子句返回对象</param>
    /// <param name="cteTableName">CTE子句表名</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget, bool>> joinOn);
    /// <summary>
    /// 在递归CTE的Union/UnionAll子句中，添加当前CTE自身引用，与上个子句中的表做RIGHT JOIN关联，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId })), "MenuList")
    ///     .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///     .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId, PageUrl = b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId,PageId) AS 
    /// (
    /// SELECT `Id`,`Name`,`ParentId`,`PageId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId`,b.`Url` AS `PageUrl` FROM MenuList a LEFT JOIN `sys_page` b ON a.`PageId`=b.`Id`
    /// </summary>
    /// <typeparam name="TTarget">CTE子句返回的实体类型</typeparam>
    /// <param name="target">CTE子句返回对象</param>
    /// <param name="cteTableName">CTE子句表名</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget, bool>> joinOn);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，当表达式predicate为null时，将不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> elsePredicate = null);
    #endregion

    #region GroupBy/OrderBy
    /// <summary>
    /// 分组查询，分组表达式groupingExpr可以是单个字段或多个字段的匿名对象，用法:
    /// <example>
    /// <code>
    /// repository.From&lt;User&gt;()
    ///    .InnerJoin&lt;Order&gt;((x, y) =&gt; x.Id == y.BuyerId)
    ///    .GroupBy((a, b) =&gt; new { a.Id, a.Name, b.CreatedAt.Date })
    ///    .OrderBy((x, a, b) =&gt; x.Grouping)
    ///    .Select((x, a, b) =&gt; new
    ///    {
    ///        x.Grouping,
    ///        OrderCount = x.Count(b.Id),
    ///        TotalAmount = x.Sum(b.TotalAmount)
    ///    })
    ///    .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `Date`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) ORDER BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE)
    /// </code>
    /// </example>
    /// </summary>
    /// <typeparam name="TGrouping">分组后的实体对象类型，可以是单个字段类型或是匿名类型</typeparam>
    /// <param name="groupingExpr">分组表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping>> groupingExpr);
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy((a, b) => new { a.Id, b.Id }) 或是 OrderBy(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr);
    /// <summary>
    /// 判断条件condition的值，当condition值为true时生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b) => new { a.Id, b.Id }) 或是 OrderBy(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr);
    /// <summary>
    /// 使用表达式fieldsExpr，生成DSC排序语句，fieldsExpr可以是一或多个字段，用法：
    /// OrderByDescending((a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr);
    /// <summary>
    /// 判断条件condition的值，当condition值为true时生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 在Sql.Exists的子查询中，返回fields内容，此方法用在Sql.Exists的子查询中
    /// </summary>
    /// <param name="fields">返回的字段内容，通常是 * 或是指定常量 1 ...等</param>
    /// <returns></returns>
    IQueryAnonymousObject Select(string fields = "*");
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，用法：
    /// Select(f => new { f.Id, f.Name }) 或是 Select(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定聚合字段返回实体，单个聚合字段或多个聚合字段的匿名对象，用法：
    /// <code>
    /// repository.From&lt;Order&gt;()
    ///    .SelectAggregate((x, a) =&gt; new
    ///    {
    ///        OrderCount = x.Count(a.Id),
    ///        TotalAmount = x.Sum(a.TotalAmount)
    ///    })
    ///    .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>SELECT COUNT(`Id`) AS `OrderCount`,SUM(`TotalAmount`) AS `TotalAmount` FROM `sys_order`</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型，通常是一个匿名类</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个或多个聚合字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget>> fieldsExpr);
    #endregion

    /// <summary>
    /// 返回当前查询的SQL和参数列表
    /// </summary>
    /// <param name="dbParameters">参数列表</param>
    /// <returns>当前查询的SQL</returns>
    string ToSql(out List<IDbDataParameter> dbParameters);
}
/// <summary>
/// 多表T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13子查询
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
public interface IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>
{
    #region Join
    /// <summary>
    /// 在现有表中，指定2个表进行INNER JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13&gt;()
    ///     ... ...
    ///     .InnerJoin((a, b, c, d, e, f, g, h, i, j, k, l, m) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinOn);
    /// <summary>
    /// 在现有表中，指定2个表进行LEFT JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13&gt;()
    ///     ... ...
    ///     .LeftJoin((a, b, c, d, e, f, g, h, i, j, k, l, m) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinOn);
    /// <summary>
    /// 在现有表中，指定2个表进行RIGHT JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13&gt;()
    ///     ... ...
    ///     .RightJoin((a, b, c, d, e, f, g, h, i, j, k, l, m) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其进行INNER JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13&gt;()
    ///     ... ...
    ///     .InnerJoin&lt;TOther&gt;((a, b, c, d, e, f, g, h, i, j, k, l, m) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其做LEFT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13&gt;()
    ///     ... ...
    ///     .LeftJoin&lt;TOther&gt;((a, b, c, d, e, f, g, h, i, j, k, l, m) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其做RIGHT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13&gt;()
    ///     ... ...
    ///     .RightJoin&lt;T13&gt;((a, b, c, d, e, f, g, h, i, j, k, l, m) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn);
    /// <summary>
    /// 在递归CTE的Union/UnionAll子句中,添加当前CTE自身引用，与上个子句中的表做INNER JOIN关联，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId })), "MenuList")
    ///     .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///     .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId, PageUrl = b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId,PageId) AS 
    /// (
    /// SELECT `Id`,`Name`,`ParentId`,`PageId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId`,b.`Url` AS `PageUrl` FROM MenuList a LEFT JOIN `sys_page` b ON a.`PageId`=b.`Id`
    /// </summary>
    /// <typeparam name="TTarget">CTE子句返回的实体类型</typeparam>
    /// <param name="target">CTE子句返回对象</param>
    /// <param name="cteTableName">CTE子句表名</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget, bool>> joinOn);
    /// <summary>
    /// 在递归CTE的Union/UnionAll子句中，添加当前CTE自身引用，与上个子句中的表做LEFT JOIN关联，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId })), "MenuList")
    ///     .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///     .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId, PageUrl = b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId,PageId) AS 
    /// (
    /// SELECT `Id`,`Name`,`ParentId`,`PageId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId`,b.`Url` AS `PageUrl` FROM MenuList a LEFT JOIN `sys_page` b ON a.`PageId`=b.`Id`
    /// </summary>
    /// <typeparam name="TTarget">CTE子句返回的实体类型</typeparam>
    /// <param name="target">CTE子句返回对象</param>
    /// <param name="cteTableName">CTE子句表名</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget, bool>> joinOn);
    /// <summary>
    /// 在递归CTE的Union/UnionAll子句中，添加当前CTE自身引用，与上个子句中的表做RIGHT JOIN关联，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId })), "MenuList")
    ///     .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///     .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId, PageUrl = b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId,PageId) AS 
    /// (
    /// SELECT `Id`,`Name`,`ParentId`,`PageId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId`,b.`Url` AS `PageUrl` FROM MenuList a LEFT JOIN `sys_page` b ON a.`PageId`=b.`Id`
    /// </summary>
    /// <typeparam name="TTarget">CTE子句返回的实体类型</typeparam>
    /// <param name="target">CTE子句返回对象</param>
    /// <param name="cteTableName">CTE子句表名</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget, bool>> joinOn);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，当表达式predicate为null时，将不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> elsePredicate = null);
    #endregion

    #region GroupBy/OrderBy
    /// <summary>
    /// 分组查询，分组表达式groupingExpr可以是单个字段或多个字段的匿名对象，用法:
    /// <example>
    /// <code>
    /// repository.From&lt;User&gt;()
    ///    .InnerJoin&lt;Order&gt;((x, y) =&gt; x.Id == y.BuyerId)
    ///    .GroupBy((a, b) =&gt; new { a.Id, a.Name, b.CreatedAt.Date })
    ///    .OrderBy((x, a, b) =&gt; x.Grouping)
    ///    .Select((x, a, b) =&gt; new
    ///    {
    ///        x.Grouping,
    ///        OrderCount = x.Count(b.Id),
    ///        TotalAmount = x.Sum(b.TotalAmount)
    ///    })
    ///    .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `Date`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) ORDER BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE)
    /// </code>
    /// </example>
    /// </summary>
    /// <typeparam name="TGrouping">分组后的实体对象类型，可以是单个字段类型或是匿名类型</typeparam>
    /// <param name="groupingExpr">分组表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping>> groupingExpr);
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy((a, b) => new { a.Id, b.Id }) 或是 OrderBy(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr);
    /// <summary>
    /// 判断条件condition的值，当condition值为true时生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b) => new { a.Id, b.Id }) 或是 OrderBy(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr);
    /// <summary>
    /// 使用表达式fieldsExpr，生成DSC排序语句，fieldsExpr可以是一或多个字段，用法：
    /// OrderByDescending((a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr);
    /// <summary>
    /// 判断条件condition的值，当condition值为true时生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 在Sql.Exists的子查询中，返回fields内容，此方法用在Sql.Exists的子查询中
    /// </summary>
    /// <param name="fields">返回的字段内容，通常是 * 或是指定常量 1 ...等</param>
    /// <returns></returns>
    IQueryAnonymousObject Select(string fields = "*");
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，用法：
    /// Select(f => new { f.Id, f.Name }) 或是 Select(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定聚合字段返回实体，单个聚合字段或多个聚合字段的匿名对象，用法：
    /// <code>
    /// repository.From&lt;Order&gt;()
    ///    .SelectAggregate((x, a) =&gt; new
    ///    {
    ///        OrderCount = x.Count(a.Id),
    ///        TotalAmount = x.Sum(a.TotalAmount)
    ///    })
    ///    .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>SELECT COUNT(`Id`) AS `OrderCount`,SUM(`TotalAmount`) AS `TotalAmount` FROM `sys_order`</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型，通常是一个匿名类</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个或多个聚合字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget>> fieldsExpr);
    #endregion

    /// <summary>
    /// 返回当前查询的SQL和参数列表
    /// </summary>
    /// <param name="dbParameters">参数列表</param>
    /// <returns>当前查询的SQL</returns>
    string ToSql(out List<IDbDataParameter> dbParameters);
}
/// <summary>
/// 多表T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14子查询
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
public interface IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
{
    #region Join
    /// <summary>
    /// 在现有表中，指定2个表进行INNER JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14&gt;()
    ///     ... ...
    ///     .InnerJoin((a, b, c, d, e, f, g, h, i, j, k, l, m, n) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinOn);
    /// <summary>
    /// 在现有表中，指定2个表进行LEFT JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14&gt;()
    ///     ... ...
    ///     .LeftJoin((a, b, c, d, e, f, g, h, i, j, k, l, m, n) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinOn);
    /// <summary>
    /// 在现有表中，指定2个表进行RIGHT JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14&gt;()
    ///     ... ...
    ///     .RightJoin((a, b, c, d, e, f, g, h, i, j, k, l, m, n) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其进行INNER JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14&gt;()
    ///     ... ...
    ///     .InnerJoin&lt;TOther&gt;((a, b, c, d, e, f, g, h, i, j, k, l, m, n) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其做LEFT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14&gt;()
    ///     ... ...
    ///     .LeftJoin&lt;TOther&gt;((a, b, c, d, e, f, g, h, i, j, k, l, m, n) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn);
    /// <summary>
    /// 在现有表中，添加TOther表，并指定1个表与其做RIGHT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14&gt;()
    ///     ... ...
    ///     .RightJoin&lt;T14&gt;((a, b, c, d, e, f, g, h, i, j, k, l, m, n) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">表TOther实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn);
    /// <summary>
    /// 在递归CTE的Union/UnionAll子句中,添加当前CTE自身引用，与上个子句中的表做INNER JOIN关联，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId })), "MenuList")
    ///     .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///     .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId, PageUrl = b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId,PageId) AS 
    /// (
    /// SELECT `Id`,`Name`,`ParentId`,`PageId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId`,b.`Url` AS `PageUrl` FROM MenuList a LEFT JOIN `sys_page` b ON a.`PageId`=b.`Id`
    /// </summary>
    /// <typeparam name="TTarget">CTE子句返回的实体类型</typeparam>
    /// <param name="target">CTE子句返回对象</param>
    /// <param name="cteTableName">CTE子句表名</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget> InnerJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget, bool>> joinOn);
    /// <summary>
    /// 在递归CTE的Union/UnionAll子句中，添加当前CTE自身引用，与上个子句中的表做LEFT JOIN关联，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId })), "MenuList")
    ///     .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///     .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId, PageUrl = b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId,PageId) AS 
    /// (
    /// SELECT `Id`,`Name`,`ParentId`,`PageId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId`,b.`Url` AS `PageUrl` FROM MenuList a LEFT JOIN `sys_page` b ON a.`PageId`=b.`Id`
    /// </summary>
    /// <typeparam name="TTarget">CTE子句返回的实体类型</typeparam>
    /// <param name="target">CTE子句返回对象</param>
    /// <param name="cteTableName">CTE子句表名</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget> LeftJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget, bool>> joinOn);
    /// <summary>
    /// 在递归CTE的Union/UnionAll子句中，添加当前CTE自身引用，与上个子句中的表做RIGHT JOIN关联，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId })), "MenuList")
    ///     .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///     .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId, a.PageId, PageUrl = b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId,PageId) AS 
    /// (
    /// SELECT `Id`,`Name`,`ParentId`,`PageId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,a.`PageId`,b.`Url` AS `PageUrl` FROM MenuList a LEFT JOIN `sys_page` b ON a.`PageId`=b.`Id`
    /// </summary>
    /// <typeparam name="TTarget">CTE子句返回的实体类型</typeparam>
    /// <param name="target">CTE子句返回对象</param>
    /// <param name="cteTableName">CTE子句表名</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget> RightJoinRecursive<TTarget>(IFromQuery<TTarget> target, string cteTableName, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget, bool>> joinOn);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，当表达式predicate为null时，将不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> elsePredicate = null);
    #endregion

    #region GroupBy/OrderBy
    /// <summary>
    /// 分组查询，分组表达式groupingExpr可以是单个字段或多个字段的匿名对象，用法:
    /// <example>
    /// <code>
    /// repository.From&lt;User&gt;()
    ///    .InnerJoin&lt;Order&gt;((x, y) =&gt; x.Id == y.BuyerId)
    ///    .GroupBy((a, b) =&gt; new { a.Id, a.Name, b.CreatedAt.Date })
    ///    .OrderBy((x, a, b) =&gt; x.Grouping)
    ///    .Select((x, a, b) =&gt; new
    ///    {
    ///        x.Grouping,
    ///        OrderCount = x.Count(b.Id),
    ///        TotalAmount = x.Sum(b.TotalAmount)
    ///    })
    ///    .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) AS `Date`,COUNT(b.`Id`) AS `OrderCount`,SUM(b.`TotalAmount`) AS `TotalAmount` FROM `sys_user` a INNER JOIN `sys_order` b ON a.`Id`=b.`BuyerId` GROUP BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE) ORDER BY a.`Id`,a.`Name`,CONVERT(b.`CreatedAt`,DATE)
    /// </code>
    /// </example>
    /// </summary>
    /// <typeparam name="TGrouping">分组后的实体对象类型，可以是单个字段类型或是匿名类型</typeparam>
    /// <param name="groupingExpr">分组表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping>> groupingExpr);
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy((a, b) => new { a.Id, b.Id }) 或是 OrderBy(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr);
    /// <summary>
    /// 判断条件condition的值，当condition值为true时生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b) => new { a.Id, b.Id }) 或是 OrderBy(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr);
    /// <summary>
    /// 使用表达式fieldsExpr，生成DSC排序语句，fieldsExpr可以是一或多个字段，用法：
    /// OrderByDescending((a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr);
    /// <summary>
    /// 判断条件condition的值，当condition值为true时生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 在Sql.Exists的子查询中，返回fields内容，此方法用在Sql.Exists的子查询中
    /// </summary>
    /// <param name="fields">返回的字段内容，通常是 * 或是指定常量 1 ...等</param>
    /// <returns></returns>
    IQueryAnonymousObject Select(string fields = "*");
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，用法：
    /// Select(f => new { f.Id, f.Name }) 或是 Select(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定聚合字段返回实体，单个聚合字段或多个聚合字段的匿名对象，用法：
    /// <code>
    /// repository.From&lt;Order&gt;()
    ///    .SelectAggregate((x, a) =&gt; new
    ///    {
    ///        OrderCount = x.Count(a.Id),
    ///        TotalAmount = x.Sum(a.TotalAmount)
    ///    })
    ///    .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>SELECT COUNT(`Id`) AS `OrderCount`,SUM(`TotalAmount`) AS `TotalAmount` FROM `sys_order`</code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型，通常是一个匿名类</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个或多个聚合字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget>> fieldsExpr);
    #endregion

    /// <summary>
    /// 返回当前查询的SQL和参数列表
    /// </summary>
    /// <param name="dbParameters">参数列表</param>
    /// <returns>当前查询的SQL</returns>
    string ToSql(out List<IDbDataParameter> dbParameters);
}
public interface IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>
{
    #region Join
    /// <summary>
    /// 在现有的表中，指定2个表进行INNER JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15&gt;()
    ///     ... ...
    ///     .InnerJoin((a, b, c, d, e, f, g, h, i, j, k, l, m, n) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinOn);
    /// <summary>
    /// 在现有的表中，指定2个表进行LEFT JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15&gt;()
    ///     ... ...
    ///     .LeftJoin((a, b, c, d, e, f, g, h, i, j, k, l, m, n) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinOn);
    /// <summary>
    /// 在现有的表中，指定2个表进行RIGHT JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15&gt;()
    ///     ... ...
    ///     .RightJoin((a, b, c, d, e, f, g, h, i, j, k, l, m, n) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinOn);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate为null时，将不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> elsePredicate = null);
    #endregion

    #region OrderBy
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy((a, b) => new { a.Id, b.Id }) 或是 OrderBy(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b) => new { a.Id, b.Id }) 或是 OrderBy(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr);
    /// <summary>
    /// 使用表达式fieldsExpr，生成DSC排序语句，fieldsExpr可以是一或多个字段，用法：
    /// OrderByDescending((a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 在Sql.Exists的子查询中，返回fields内容，此方法用在Sql.Exists的子查询中
    /// </summary>
    /// <param name="fields">返回的字段内容，通常是 * 或是指定常量 1...等</param>
    /// <returns></returns>
    IQueryAnonymousObject Select(string fields = "*");
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，用法：
    /// Select((a, b, c, d, e, f, g, h, i, j, k, l, m, n) => new { f.Id, f.Name }) 或是 Select((a, b, c, d, e, f, g, h, i, j, k, l, m, n) => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定聚合字段返回实体，单个或多个聚合字段的匿名对象，用法：
    /// <code>
    /// repository.From&lt;Order&gt;()
    ///    ... ...
    ///    .SelectAggregate((x, a, b, c, d, e, f, g, h, i, j, k, l, m, n) =&gt; new
    ///    {
    ///        OrderCount = x.Count(a.Id),
    ///        TotalAmount = x.Sum(a.TotalAmount)
    ///    })
    ///    .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>SELECT COUNT(`Id`) AS `OrderCount`,SUM(`TotalAmount`) AS `TotalAmount` FROM ... `sys_order` ... </code>
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型，通常是一个匿名类</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个或多个聚合字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IFromQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TTarget>> fieldsExpr);
    #endregion

    /// <summary>
    /// 返回当前查询的SQL和参数列表
    /// </summary>
    /// <param name="dbParameters">参数列表</param>
    /// <returns>当前查询的SQL</returns>
    string ToSql(out List<IDbDataParameter> dbParameters);
}
