﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Trolley;

public interface IMultipleQuery
{
    #region Query
    /// <summary>
    /// 查询数据，用法：
    /// <code>
    /// repository
    ///     .From&lt;Menu&gt;()
    ///     .Select(f => new { f.Id, f.Name, f.ParentId })
    ///     .First();
    /// </code>
    /// 生成的SQL:<code>SELECT `Id`,`Name`,`ParentId` FROM `sys_menu`</code>
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <param name="tableAsStart">表别名起始字母，默认值从字母a开始</param>
    /// <param name="suffixRawSql">额外的原始SQL, SqlServer会有With用法，如：
    /// <cdoe>SELECT * FROM sys_user WITH(NOLOCK)</cdoe>
    /// </param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T> From<T>(char tableAsStart = 'a', string suffixRawSql = null);
    /// <summary>
    /// SQL子查询，用法：
    /// <code>
    /// repository
    ///     .From(f => f.From&lt;Page, Menu&gt;('o')
    ///         .Where((a, b) => a.Id == b.PageId)
    ///         .Select((x, y) => new { y.Id, y.ParentId, x.Url }))
    ///     .InnerJoin&lt;Menu&gt;>((a, b) => a.Id == b.Id)
    ///     .Where((a, b) => a.Id == b.Id)
    ///     .Select((a, b) => new { a.Id, a.Name, a.ParentId, b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// SELECT a.`Id`,b.`Name`,a.`ParentId`,a.`Url` FROM (SELECT p.`Id`,p.`ParentId`,o.`Url` FROM `sys_page` o,`sys_menu` p WHERE o.`Id`=p.`PageId`) a INNER JOIN `sys_menu` b ON a.`Id`=b.`Id` WHERE a.`Id`=b.`Id`
    /// </code>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="subQuery"></param>
    /// <param name="tableAsStart"></param>
    /// <returns></returns>
    IMultiQuery<T> From<T>(Func<IFromQuery, IFromQuery<T>> subQuery, char tableAsStart = 'a');
    /// <summary>
    /// 使用CTE子句创建查询对象，不能自我引用不能递归查询，用法：
    /// <code>
    /// repository
    ///     .FromWith(f => f.From<Menu>()
    ///         .Select(x => new { x.Id, x.Name, x.ParentId, x.PageId }), "MenuList")
    ///     .InnerJoin<Page>((a, b) => a.Id == b.Id)
    ///     .Select((a, b) => new { a.Id, a.Name, a.ParentId, b.Url })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH MenuList(Id,Name,ParentId,PageId) AS 
    /// (
    ///     SELECT `Id`,`Name`,`ParentId`,`PageId` FROM `sys_menu`
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,b.`Url` FROM MenuList a INNER JOIN `sys_page` b ON a.`Id`=b.`Id`
    /// </code>
    /// </summary>
    /// <typeparam name="T">CTE With子句的临时实体类型，通常是一个匿名的</typeparam>
    /// <param name="cteSubQuery">CTE 查询子句</param>
    /// <param name="cteTableName">CTE子句的临时表名，默认值：cte</param>
    /// <param name="tableAsStart">表别名起始字母，默认值从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T> FromWith<T>(Func<IFromQuery, IFromQuery<T>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    /// <summary>
    /// 使用可递归CTE子句创建查询对象，可以自我引用递归查询，要有包含自我引用的Union或Union All查询子句，
    /// <para>通常用来查询树型数据，查找叶子或是查找根，用法：</para>
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
    /// </code>
    /// 生成的SQL
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId) AS
    /// (
    ///     SELECT `Id`,`Name`,`ParentId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    ///     SELECT a.`Id`,a.`Name`,a.`ParentId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// ),
    /// MenuPageList(Id,Url) AS 
    /// (
    ///     SELECT b.`Id`,a.`Url` FROM `sys_page` a INNER JOIN `sys_menu` b ON a.`Id`=b.`PageId` WHERE a.`Id`=1 UNION ALL
    ///     SELECT b.`Id`,a.`Url` FROM `sys_page` a INNER JOIN `sys_menu` b ON a.`Id`=b.`PageId` WHERE a.`Id`>1
    /// )
    /// SELECT a.`Id`,a.`Name`,a.`ParentId`,b.`Url` FROM MenuList a INNER JOIN MenuPageList b ON a.`Id`=b.`Id`
    /// </code>
    /// </summary>
    /// <typeparam name="T">CTE With子句的临时实体类型，通常是一个匿名的</typeparam>
    /// <param name="cteSubQuery">CTE 查询子句，带有Union或Union All查询子句</param>
    /// <param name="cteTableName">CTE子句的临时表名，默认值：cte</param>
    /// <param name="tableAsStart">表别名起始字母，默认值从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T> FromWithRecursive<T>(Func<IFromQuery, string, IFromQuery<T>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    /// <summary>
    /// 使用2个表创建查询对象
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <param name="tableAsStart">表别名起始字母，默认值从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2> From<T1, T2>(char tableAsStart = 'a');
    /// <summary>
    /// 使用3个表创建查询对象
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <typeparam name="T3">表T3实体类型</typeparam>
    /// <param name="tableAsStart">表别名起始字母，默认值从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3> From<T1, T2, T3>(char tableAsStart = 'a');
    /// <summary>
    /// 使用4个表创建查询对象
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <typeparam name="T3">表T3实体类型</typeparam>
    /// <typeparam name="T4">表T4实体类型</typeparam>
    /// <param name="tableAsStart">表别名起始字母，默认值从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4> From<T1, T2, T3, T4>(char tableAsStart = 'a');
    /// <summary>
    /// 使用5个表创建查询对象
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <typeparam name="T3">表T3实体类型</typeparam>
    /// <typeparam name="T4">表T4实体类型</typeparam>
    /// <typeparam name="T5">表T5实体类型</typeparam>
    /// <param name="tableAsStart">表别名起始字母，默认值从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>(char tableAsStart = 'a');
    /// <summary>
    /// 使用6个表创建查询对象
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <typeparam name="T3">表T3实体类型</typeparam>
    /// <typeparam name="T4">表T4实体类型</typeparam>
    /// <typeparam name="T5">表T5实体类型</typeparam>
    /// <typeparam name="T6">表T6实体类型</typeparam>
    /// <param name="tableAsStart">表别名起始字母，默认值从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>(char tableAsStart = 'a');
    /// <summary>
    /// 使用7个表创建查询对象
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <typeparam name="T3">表T3实体类型</typeparam>
    /// <typeparam name="T4">表T4实体类型</typeparam>
    /// <typeparam name="T5">表T5实体类型</typeparam>
    /// <typeparam name="T6">表T6实体类型</typeparam>
    /// <typeparam name="T7">表T7实体类型</typeparam>
    /// <param name="tableAsStart">表别名起始字母，默认值从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7> From<T1, T2, T3, T4, T5, T6, T7>(char tableAsStart = 'a');
    /// <summary>
    /// 使用8个表创建查询对象
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <typeparam name="T3">表T3实体类型</typeparam>
    /// <typeparam name="T4">表T4实体类型</typeparam>
    /// <typeparam name="T5">表T5实体类型</typeparam>
    /// <typeparam name="T6">表T6实体类型</typeparam>
    /// <typeparam name="T7">表T7实体类型</typeparam>
    /// <typeparam name="T8">表T8实体类型</typeparam>
    /// <param name="tableAsStart">表别名起始字母，默认值从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> From<T1, T2, T3, T4, T5, T6, T7, T8>(char tableAsStart = 'a');
    /// <summary>
    /// 使用9个表创建查询对象
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
    /// <param name="tableAsStart">表别名起始字母，默认值从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T1, T2, T3, T4, T5, T6, T7, T8, T9>(char tableAsStart = 'a');
    /// <summary>
    /// 使用10个表创建查询对象
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
    /// <param name="tableAsStart">表别名起始字母，默认值从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(char tableAsStart = 'a');
    /// <summary>
    /// 执行SQL查询，返回满足条件的TEntity实体第一条记录，记录不存在时返回TEntity类型的默认值
    /// </summary>
    /// <typeparam name="TEntity">实体TEntity类型</typeparam>
    /// <param name="rawSql">原始SQL</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery QueryFirst<TEntity>(string rawSql);
    /// <summary>
    /// 执行SQL查询，返回满足条件的TEntity实体第一条记录，记录不存在时返回TEntity类型的默认值
    /// </summary>
    /// <typeparam name="TEntity">实体TEntity类型</typeparam>
    /// <param name="rawSql">原始SQL</param>
    /// <param name="parameters">参数，可以是命名对象、匿名对象或是Dictionary类型对象</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery QueryFirst<TEntity>(string rawSql, object parameters);
    /// <summary>
    /// 执行SQL查询，返回TEntity实体所有字段的第一条记录，记录不存在时返回TEntity类型的默认值
    /// </summary>
    /// <typeparam name="TEntity">实体TEntity类型</typeparam>
    /// <param name="whereObj">参数，可以是命名对象、匿名对象或是Dictionary类型对象</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery QueryFirst<TEntity>(object whereObj);
    /// <summary>
    /// 执行SQL查询，返回满足条件的所有TEntity实体记录，记录不存在时返回空的List&lt;TEntity&gt;类型对象
    /// </summary>
    /// <typeparam name="TEntity">实体TEntity类型</typeparam>
    /// <param name="rawSql">原始SQL</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Query<TEntity>(string rawSql);
    /// <summary>
    /// 执行SQL查询，返回满足条件的所有TEntity实体记录，记录不存在时返回空的List&lt;TEntity&gt;类型对象
    /// </summary>
    /// <typeparam name="TEntity">实体TEntity类型</typeparam>
    /// <param name="rawSql">原始SQL</param>
    /// <param name="parameters">参数，可以是命名对象、匿名对象或是Dictionary类型对象</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Query<TEntity>(string rawSql, object parameters);
    /// <summary>
    /// 执行SQL查询，返回满足条件的所有TEntity实体记录，记录不存在时返回空的List&lt;TEntity&gt;类型对象
    /// </summary>
    /// <typeparam name="TEntity">实体TEntity类型</typeparam>
    /// <param name="whereObj">参数，可以是命名对象、匿名对象或是Dictionary类型对象</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Query<TEntity>(object whereObj);
    #endregion

    #region CRUD
    /// <summary>
    /// 根据主键信息，获取TEntity实体，记录不存在时返回TEntity类型的默认值
    /// </summary>
    /// <typeparam name="TEntity">获取实体类型</typeparam>
    /// <returns>返回实体对象或是TEntity类型默认值</returns>
    IMultipleQuery Get<TEntity>(object whereObj);
    /// <summary>
    /// 创建TEntity类型插入对象
    /// </summary>
    /// <typeparam name="TEntity">插入对象类型</typeparam>
    /// <returns>返回插入对象</returns>
    IMultiCreate<TEntity> Create<TEntity>();
    /// <summary>
    /// 创建T类型更新对象
    /// </summary>
    /// <typeparam name="TEntity">更新对象类型</typeparam>
    /// <returns>返回更新对象</returns>
    //IMultiUpdate<TEntity> Update<TEntity>();
    /// <summary>
    /// 创建TEntity类型删除对象
    /// </summary>
    /// <typeparam name="TEntity">删除对象类型</typeparam>
    /// <returns>返回删除对象</returns>
    IMultiDelete<TEntity> Delete<TEntity>();
    /// <summary>
    /// 判断TEntity表是否存在满足whereObj条件的记录，存在返回true,否则返回false，查询条件：TEntity表中对应字段值等于whreObj对象中的各属性值。
    /// </summary>
    /// <typeparam name="TEntity">实体对象类型</typeparam>
    /// <param name="whereObj">where条件对象，根据TEntity表中对应字段值等于whreObj对象中的各属性值</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Exists<TEntity>(object whereObj);
    /// <summary>
    /// 判断TEntity表是否存在满足wherePredicate条件的记录，存在返回true,否则返回false
    /// </summary>
    /// <typeparam name="TEntity">实体对象类型</typeparam>
    /// <param name="wherePredicate">where条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Exists<TEntity>(Expression<Func<TEntity, bool>> wherePredicate);
    /// <summary>
    /// 执行原始SQL，并返回影响行数
    /// </summary>
    /// <param name="sql">要执行的SQL</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Execute(string sql);
    /// <summary>
    /// 执行原始SQL，并返回影响行数
    /// </summary>
    /// <param name="sql">要执行的SQL</param>
    /// <param name="parameters">SQL中使用的参数</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Execute(string sql, object parameters);
    #endregion
}
/// <summary>
/// 查询数据
/// </summary>
/// <typeparam name="T">实体类型</typeparam>
public interface IMultiQuery<T>
{
    #region Union/UnionAll
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
    IMultiQuery<T> Union(Func<IFromQuery, IFromQuery<T>> subQuery);
    /// <summary>
    /// Union All操作，用法：
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
    IMultiQuery<T> UnionAll(Func<IFromQuery, IFromQuery<T>> subQuery);
    #endregion

    #region CTE NextWith/NextWithRecursive
    /// <summary>
    /// 继续定义CTE With子句，在Select查询之前，可以定义一个或多个CTE子句，多个CTE With子句要连续定义，用法：
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
    /// <param name="tableAsStart">CTE子句中使用的表别名开始字母，默认从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T, TTarget> NextWith<TTarget>(Func<IFromQuery, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    /// <summary>
    /// 继续定义可递归的CTE With子句，可以引用CTE自身。在Select查询之前，可以定义一个或多个CTE子句，多个CTE With子句要连续定义，用法：
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
    /// <param name="tableAsStart">CTE子句中使用的表别名开始字母，默认从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T, TTarget> NextWithRecursive<TTarget>(Func<IFromQuery, string, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    #endregion

    #region WithTable
    /// <summary>
    /// 使用子查询作为临时表，方便后面做关联查询，用法：
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
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
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
    IMultiQuery<T, TOther> InnerJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn);
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
    IMultiQuery<T, TOther> LeftJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn);
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
    IMultiQuery<T, TOther> RightJoin<TOther>(Expression<Func<T, TOther, bool>> joinOn);
    /// <summary>
    /// 添加子查询作为临时表，并与现有表T做INNER JOIN关联，用法:
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
    /// <typeparam name="TOther">子查询返回的实体类型，子查询中通常会有SELECT操作，返回的类型是一个匿名类</typeparam>
    /// <param name="subQuery">子查询对象</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T, TOther> InnerJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T, TOther, bool>> joinOn);
    /// <summary>
    /// 添加子查询作为临时表，并与现有表T做LEFT JOIN关联，用法:
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
    /// <typeparam name="TOther">子查询返回的实体类型，子查询中通常会有SELECT操作，返回的类型是一个匿名类</typeparam>
    /// <param name="subQuery">子查询对象</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T, TOther> LeftJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T, TOther, bool>> joinOn);
    /// <summary>
    /// 添加子查询作为临时表，并与现有表T做RIGHT JOIN关联，用法:
    /// <code>
    /// ... ...
    /// .RightJoin(f =&gt; f.From&lt;OrderDetail&gt;()
    ///     .GroupBy(x =&gt; x.OrderId)
    ///     .Select((x, y) =&gt; new
    ///     {
    ///         y.OrderId,
    ///         ProductCount = x.CountDistinct(y.ProductId)
    ///     }), (a, b, c) =&gt; b.Id == c.OrderId)
    /// ... ...
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... ... RIGHT JOIN (SELECT a.`OrderId`,COUNT(DISTINCT a.`ProductId`) AS ProductCount FROM `sys_order_detail` a GROUP BY a.`OrderId`) ... ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型，子查询中通常会有SELECT操作，返回的<paramref name="TOther"/>类型是一个匿名类</typeparam>
    /// <param name="subQuery">子查询对象</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T, TOther> RightJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T, TOther, bool>> joinOn);
    #endregion

    #region Include
    /// <summary>
    /// 贪婪加载导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系生成JOIN ON子句。
    /// 1:1关联关系，随主表一起查询，1:N关联关系，分两次查询，第二次查询返回结果，再赋值到主实体的属性上。
    /// </summary>
    /// <example>
    /// 1:1关联关系导航属性
    /// <code>
    /// repository.From&lt;Product&gt;()
    ///   .Include(f =&gt; f.Brand)
    ///   .Where(f =&gt; f.ProductNo.Contains("PN-00"))
    ///   .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT a.`Id`,a.`ProductNo`,a.`Name`,a.`BrandId`,a.`CategoryId`,a.`Price`,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt`,b.`Id`,b.`BrandNo`,b.`Name` FROM `sys_product` a LEFT JOIN `sys_brand` b ON a.`BrandId`=b.`Id` WHERE a.`ProductNo` LIKE '%PN-00%'
    /// </code>
    /// </example>
    /// <example>
    /// 1:1关联关系导航属性
    /// <code>
    /// repository.From&lt;Product&gt;()
    ///   .Include(f =&gt; f.Brand)
    ///   .Where(f =&gt; f.ProductNo.Contains("PN-00"))
    ///   .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT a.`Id`,a.`ProductNo`,a.`Name`,a.`BrandId`,a.`CategoryId`,a.`Price`,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt`,b.`Id`,b.`BrandNo`,b.`Name` FROM `sys_product` a LEFT JOIN `sys_brand` b ON a.`BrandId`=b.`Id` WHERE a.`ProductNo` LIKE '%PN-00%'
    /// </code>
    /// </example>
    /// <typeparam name="TMember">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IMultiIncludableQuery<T, TMember> Include<TMember>(Expression<Func<T, TMember>> memberSelector);
    /// <summary>
    /// 单表查询，包含1:N关联方式的导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系。
    /// </summary>
    /// <typeparam name="TElment">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IMultiIncludableQuery<T, TElment> IncludeMany<TElment>(Expression<Func<T, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T> Where(Expression<Func<T, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T> Where(bool condition, Expression<Func<T, bool>> ifPredicate, Expression<Func<T, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T> And(Expression<Func<T, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T> And(bool condition, Expression<Func<T, bool>> ifPredicate = null, Expression<Func<T, bool>> elsePredicate = null);
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
    IMultiGroupingQuery<T, TGrouping> GroupBy<TGrouping>(Expression<Func<T, TGrouping>> groupingExpr);
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy((a, b) => new { a.Id, b.Id }) 或是 OrderBy(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T> OrderBy<TFields>(Expression<Func<T, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b) => new { a.Id, b.Id }) 或是 OrderBy(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T> OrderBy<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr);
    /// <summary>
    /// 使用表达式fieldsExpr，生成DSC排序语句，fieldsExpr可以是一或多个字段，用法：
    /// OrderByDescending((a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T> OrderByDescending<TFields>(Expression<Func<T, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T> OrderByDescending<TFields>(bool condition, Expression<Func<T, TFields>> fieldsExpr);
    #endregion

    #region Distinct
    /// <summary>
    /// 生成DISTINCT语句，去掉重复数据
    /// </summary>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T> Distinct();
    #endregion

    #region Skip/Take/Page
    /// <summary>
    /// 跳过offset条数据
    /// </summary>
    /// <param name="offset">要跳过查询的数据条数</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T> Skip(int offset);
    /// <summary>
    /// 只返回limit条数据
    /// </summary>
    /// <param name="limit">返回的数据条数</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T> Take(int limit);
    /// <summary>
    /// 分页查询
    /// </summary>
    /// <param name="pageIndex">第几页索引，从1开始</param>
    /// <param name="pageSize">每页显示条数</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T> Page(int pageIndex, int pageSize);
    #endregion

    #region Select
    /// <summary>
    /// 直接返回所有字段
    /// </summary>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T> Select();
    /// <summary>
    /// 选择指定字段返回，可以是单个字段或多个字段的匿名对象，用法：
    /// Select(f => new { f.Id, f.Name }) 或是 Select(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式，单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T, TTarget>> fieldsExpr);
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
    IMultiQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T, TTarget>> fieldsExpr);
    #endregion

    #region Aggregate
    #region Count
    /// <summary>
    /// 返回int类型数据条数
    /// </summary>
    /// <param name="result">返回数据条数</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Count();
    /// <summary>
    /// 返回long类型数据条数
    /// </summary>
    /// <param name="result">返回数据条数</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery LongCount();
    /// <summary>
    /// 返回某个字段的int类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().Count(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Count<TField>(Expression<Func<T, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，这个更有实际意义，用法：
    /// <code>repository.From&lt;Order&gt;().CountDistinct(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery CountDistinct<TField>(Expression<Func<T, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的long类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().LongCount(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery LongCount<TField>(Expression<Func<T, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，这个更有实际意义，用法：
    /// <code>repository.From&lt;Order&gt;().LongCountDistinct(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery LongCountDistinct<TField>(Expression<Func<T, TField>> fieldExpr);
    #endregion

    /// <summary>
    /// 计算指定字段的求和值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Sum<TField>(Expression<Func<T, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的平均值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Avg<TField>(Expression<Func<T, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最大值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <param name="result">返回该字段的最大值</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Max<TField>(Expression<Func<T, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最小值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Min<TField>(Expression<Func<T, TField>> fieldExpr);
    #endregion

    #region First/ToList/ToPageList/ToDictionary
    /// <summary>
    /// 执行SQL查询，返回T实体所有字段的第一条记录，记录不存在时返回T类型的默认值
    /// </summary>
    /// <returns>返回查询对象</returns>
    IMultipleQuery First();
    /// <summary>
    /// 执行SQL查询，返回T实体所有字段的记录，记录不存在时返回没有任何元素的空列表
    /// </summary>
    /// <param name="result">返回T实体列表或没有任何元素的空列表</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery ToList();
    /// <summary>
    /// 按照指定的分页设置执行SQL查询，返回T实体所有字段的指定条数IPagedList&lt;T&gt;列表，记录不存在时返回没有任何元素的IPagedList&lt;T&gt;空列表
    /// </summary>
    /// <returns>返回查询对象</returns>
    IMultipleQuery ToPageList();
    /// <summary>
    /// 执行SQL查询，返回T实体所有字段的记录并转化为Dictionary&lt;TKey, TValue&gt;字典，记录不存在时返回没有任何元素的Dictionary&lt;TKey, TValue&gt;空字典
    /// </summary>
    /// <typeparam name="TKey">字典Key类型</typeparam>
    /// <typeparam name="TValue">字典Value类型</typeparam>
    /// <param name="keySelector">字典Key选择委托</param>
    /// <param name="valueSelector">字典Value选择委托</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery ToDictionary<TKey, TValue>(Func<T, TKey> keySelector, Func<T, TValue> valueSelector) where TKey : notnull;
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
public interface IMultiQueryBase
{
    #region Count
    /// <summary>
    /// 返回数据条数
    /// </summary>
    /// <param name="result">返回数据条数</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Count();
    /// <summary>
    /// 返回long类型数据条数
    /// </summary>
    /// <returns>返回查询对象</returns>
    IMultipleQuery LongCount();
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
/// 多表T1, T2查询
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
public interface IMultiQuery<T1, T2> : IMultiQueryBase
{
    #region CTE NextWith/NextWithRecursive
    /// <summary>
    /// 继续定义CTE With子句，在Select查询之前，可以定义一个或多个CTE子句，多个CTE With子句要连续定义，用法：
    /// <code>
    /// repository
    ///     .FromWith(f =&gt; ..., "Cte1")
    ///     ... ...
    ///     .NextWith(f =&gt; ..., "CteN")
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
    IMultiQuery<T1, T2, TTarget> NextWith<TTarget>(Func<IFromQuery, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    /// <summary>
    /// 继续定义可递归的CTE With子句，可以引用CTE自身。在Select查询之前，可以定义一个或多个CTE子句，多个CTE With子句要连续定义，首个CTE With子句必须也是可递归的，才能使用本方法，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId })), "MenuList")
    ///     ... ...
    ///     .NextWithRecursive((f, cte) =&gt; f.From&lt;Page, Menu&gt;()
    ///             .Where((a, b) =&gt; a.Id == b.PageId)
    ///             .Select((x, y) =&gt; new { y.Id, y.ParentId, x.Url })
    ///         .UnionAll(x =&gt; x.From&lt;Menu&gt;()
    ///             .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///             .Where((a, b) =&gt; a.Id &gt; 1)
    ///             .Select((x, y) =&gt; new { x.Id, x.ParentId, y.Url })), "MenuPageList")
    ///     ... ...
    ///     .Select((a, b) =&gt; new { ... ... })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId) AS 
    /// (
    ///     SELECT `Id`,`Name`,`ParentId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    ///     SELECT a.`Id`, a.`Name`, a.`ParentId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// ),
    /// ... ...
    /// MenuPageList(Id,ParentId,Url) AS
    /// (
    ///     SELECT a.`Id`,a.`ParentId`,b.`Url` FROM `sys_menu` a,`sys_page` b WHERE a.`PageId`=b.Id UNION ALL
    ///     SELECT a.`Id`,a.`ParentId`,b.`Url` FROM `sys_menu` a INNER JOIN MenuList b ON a.`Id`=b.`ParentId`
    /// ),
    /// ... ...
    /// SELECT ... FROM MenuList a INNER JOIN ... INNER JOIN MenuPageList f ON ... .... WHERE ... ...
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
    /// <param name="tableAsStart">CTE子句中使用的表别名开始字母，默认从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, TTarget> NextWithRecursive<TTarget>(Func<IFromQuery, string, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    #endregion

    #region WithTable
    /// <summary>
    /// 使用子查询作为临时表，方便后面做关联查询，用法：
    /// <code>
    /// repository
    ///     ... ...
    ///     .WithTable(f =&gt; f.From&lt;Page, Menu&gt;()
    ///         .Where((a, b) =&gt; a.Id == b.PageId)
    ///         .Select((x, y) =&gt; new { y.Id, y.ParentId, x.Url }))
    ///     .InnerJoin((a, b) =&gt; ...)
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... (SELECT b.`Id`,b.`ParentId`,a.`Url` FROM `sys_page` a,`sys_menu` b WHERE c.`Id`=d.`PageId`) c INNER JOIN ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    #endregion

    #region Include
    /// <summary>
    /// 贪婪加载导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系生成JOIN ON子句。
    /// 可以通过使用InnerJoin(Expression&lt;Func&lt;T1, T2, bool&gt;&gt; joinOn)、RightJoin(Expression&lt;Func&lt;T1, T2, bool&gt;&gt; joinOn)方法改变默认关联方式。
    /// 1:1关联关系，随主表一起查询，1:N关联关系，分两次查询，第二次查询返回结果，再赋值到主实体的属性上。
    /// </summary>
    /// <example>
    /// 1:1关联关系导航属性
    /// <code>
    /// repository.From&lt;Product&gt;()
    ///   .Include(f =&gt; f.Brand)
    ///   .Where(f =&gt; f.ProductNo.Contains("PN-00"))
    ///   .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT a.`Id`,a.`ProductNo`,a.`Name`,a.`BrandId`,a.`CategoryId`,a.`Price`,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt`,b.`Id`,b.`BrandNo`,b.`Name` FROM `sys_product` a LEFT JOIN `sys_brand` b ON a.`BrandId`=b.`Id` WHERE a.`ProductNo` LIKE '%PN-00%'
    /// </code>
    /// </example>
    /// <example>
    /// 1:1关联关系导航属性
    /// <code>
    /// repository.From&lt;Product&gt;()
    ///   .Include(f =&gt; f.Brand)
    ///   .Where(f =&gt; f.ProductNo.Contains("PN-00"))
    ///   .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT a.`Id`,a.`ProductNo`,a.`Name`,a.`BrandId`,a.`CategoryId`,a.`Price`,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt`,b.`Id`,b.`BrandNo`,b.`Name` FROM `sys_product` a LEFT JOIN `sys_brand` b ON a.`BrandId`=b.`Id` WHERE a.`ProductNo` LIKE '%PN-00%'
    /// </code>
    /// </example>
    /// <typeparam name="TMember">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IMultiIncludableQuery<T1, T2, TMember> Include<TMember>(Expression<Func<T1, T2, TMember>> memberSelector);
    /// <summary>
    /// 单表查询，包含1:N关联方式的导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系。
    /// 可以通过使用InnerJoin(Expression&lt;Func&lt;T, bool&gt;&gt; joinOn)、RightJoin(Expression&lt;Func&lt;T, bool&gt;&gt; joinOn)方法改变默认关联方式
    /// </summary>
    /// <typeparam name="TElment">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IMultiIncludableQuery<T1, T2, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null);
    #endregion

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
    IMultiQuery<T1, T2> InnerJoin(Expression<Func<T1, T2, bool>> joinOn);
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
    IMultiQuery<T1, T2> LeftJoin(Expression<Func<T1, T2, bool>> joinOn);
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
    IMultiQuery<T1, T2> RightJoin(Expression<Func<T1, T2, bool>> joinOn);
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
    IMultiQuery<T1, T2, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn);
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
    IMultiQuery<T1, T2, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn);
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
    IMultiQuery<T1, T2, TOther> RightJoin<TOther>(Expression<Func<T1, T2, TOther, bool>> joinOn);

    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做INNER JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2&gt;()
    ///     ... ...
    ///     .InnerJoin((a, b) =&gt; f.From&lt;OrderDetail&gt;()
    ///         .GroupBy(x =&gt; x.OrderId)
    ///         .Select((x, y) =&gt; new
    ///         {
    ///             y.OrderId,
    ///             ProductCount = x.CountDistinct(y.ProductId)
    ///         }), (a, b) =&gt; ...)
    ///     ... ...
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... INNER JOIN (SELECT a.`OrderId`,COUNT(DISTINCT a.`ProductId`) AS ProductCount FROM `sys_order_detail` a GROUP BY a.`OrderId`) c ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, TOther> InnerJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做LEFT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2&gt;()
    ///     ... ...
    ///     .LeftJoin((a, b) =&gt; f.From&lt;OrderDetail&gt;()
    ///         .GroupBy(x =&gt; x.OrderId)
    ///         .Select((x, y) =&gt; new
    ///         {
    ///             y.OrderId,
    ///             ProductCount = x.CountDistinct(y.ProductId)
    ///         }), (a, b) =&gt; ...)
    ///     ... ...
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... INNER JOIN (SELECT a.`OrderId`,COUNT(DISTINCT a.`ProductId`) AS ProductCount FROM `sys_order_detail` a GROUP BY a.`OrderId`) c ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, TOther> LeftJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做RIGHT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2&gt;()
    ///     ... ...
    ///     .RightJoin((a, b) =&gt; f.From&lt;OrderDetail&gt;()
    ///         .GroupBy(x =&gt; x.OrderId)
    ///         .Select((x, y) =&gt; new
    ///         {
    ///             y.OrderId,
    ///             ProductCount = x.CountDistinct(y.ProductId)
    ///         }), (a, b) =&gt; ...)
    ///     ... ...
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... INNER JOIN (SELECT a.`OrderId`,COUNT(DISTINCT a.`ProductId`) AS ProductCount FROM `sys_order_detail` a GROUP BY a.`OrderId`) c ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, TOther> RightJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, TOther, bool>> joinOn);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2> Where(Expression<Func<T1, T2, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2> Where(bool condition, Expression<Func<T1, T2, bool>> ifPredicate, Expression<Func<T1, T2, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2> And(Expression<Func<T1, T2, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2> And(bool condition, Expression<Func<T1, T2, bool>> ifPredicate = null, Expression<Func<T1, T2, bool>> elsePredicate = null);
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
    IMultiGroupingQuery<T1, T2, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, TGrouping>> groupingExpr);
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy((a, b) => new { a.Id, b.Id }) 或是 OrderBy(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2> OrderBy<TFields>(Expression<Func<T1, T2, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b) => new { a.Id, b.Id }) 或是 OrderBy(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, TFields>> fieldsExpr);
    /// <summary>
    /// 使用表达式fieldsExpr，生成DSC排序语句，fieldsExpr可以是一或多个字段，用法：
    /// OrderByDescending((a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2> OrderByDescending<TFields>(Expression<Func<T1, T2, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，用法：
    /// Select((a, b) => new { f.Id, f.Name }) 或是 Select((a, b) => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定聚合字段返回实体，单个或多个聚合字段的匿名对象，用法：
    /// <code>
    /// repository.From&lt;Order&gt;()
    ///    ... ...
    ///    .SelectAggregate((x, a, b) =&gt; new
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
    IMultiQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, TTarget>> fieldsExpr);
    #endregion

    #region Aggregate
    #region Count
    /// <summary>
    /// 返回某个字段的int类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().Count(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Count<TField>(Expression<Func<T1, T2, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().CountDistinct(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery CountDistinct<TField>(Expression<Func<T1, T2, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的long类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().LongCount(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery LongCount<TField>(Expression<Func<T1, T2, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().LongCountDistinct(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery LongCountDistinct<TField>(Expression<Func<T1, T2, TField>> fieldExpr);
    #endregion

    /// <summary>
    /// 计算指定字段的求和值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Sum<TField>(Expression<Func<T1, T2, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的平均值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Avg<TField>(Expression<Func<T1, T2, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最大值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Max<TField>(Expression<Func<T1, T2, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最小值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Min<TField>(Expression<Func<T1, T2, TField>> fieldExpr);
    #endregion
}
/// <summary>
/// 多表T1, T2, T3查询
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
public interface IMultiQuery<T1, T2, T3> : IMultiQueryBase
{
    #region CTE NextWith/NextWithRecursive
    /// <summary>
    /// 继续定义CTE With子句，在Select查询之前，可以定义一个或多个CTE子句，多个CTE With子句要连续定义，用法：
    /// <code>
    /// repository
    ///     .FromWith(f =&gt; ..., "Cte1")
    ///     ... ...
    ///     .NextWith(f =&gt; ..., "CteN")
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
    IMultiQuery<T1, T2, T3, TTarget> NextWith<TTarget>(Func<IFromQuery, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    /// <summary>
    /// 继续定义可递归的CTE With子句，可以引用CTE自身。在Select查询之前，可以定义一个或多个CTE子句，多个CTE With子句要连续定义，首个CTE With子句必须也是可递归的，才能使用本方法，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId })), "MenuList")
    ///     ... ...
    ///     .NextWithRecursive((f, cte) =&gt; f.From&lt;Page, Menu&gt;()
    ///             .Where((a, b) =&gt; a.Id == b.PageId)
    ///             .Select((x, y) =&gt; new { y.Id, y.ParentId, x.Url })
    ///         .UnionAll(x =&gt; x.From&lt;Menu&gt;()
    ///             .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///             .Where((a, b) =&gt; a.Id &gt; 1)
    ///             .Select((x, y) =&gt; new { x.Id, x.ParentId, y.Url })), "MenuPageList")
    ///     ... ...
    ///     .Select((a, b, c) =&gt; new { ... ... })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId) AS 
    /// (
    ///     SELECT `Id`,`Name`,`ParentId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    ///     SELECT a.`Id`, a.`Name`, a.`ParentId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// ),
    /// ... ...
    /// MenuPageList(Id,ParentId,Url) AS
    /// (
    ///     SELECT a.`Id`,a.`ParentId`,b.`Url` FROM `sys_menu` a,`sys_page` b WHERE a.`PageId`=b.Id UNION ALL
    ///     SELECT a.`Id`,a.`ParentId`,b.`Url` FROM `sys_menu` a INNER JOIN MenuList b ON a.`Id`=b.`ParentId`
    /// ),
    /// ... ...
    /// SELECT ... FROM MenuList a INNER JOIN ... INNER JOIN MenuPageList f ON ... .... WHERE ... ...
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
    /// <param name="tableAsStart">CTE子句中使用的表别名开始字母，默认从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, TTarget> NextWithRecursive<TTarget>(Func<IFromQuery, string, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    #endregion

    #region WithTable
    /// <summary>
    /// 使用子查询作为临时表，方便后面做关联查询，用法：
    /// <code>
    /// repository
    ///     ... ...
    ///     .WithTable(f =&gt; f.From&lt;Page, Menu&gt;()
    ///         .Where((a, b) =&gt; a.Id == b.PageId)
    ///         .Select((x, y) =&gt; new { y.Id, y.ParentId, x.Url }))
    ///     .InnerJoin((a, b, c) =&gt; ...)
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... (SELECT b.`Id`,b.`ParentId`,a.`Url` FROM `sys_page` a,`sys_menu` b WHERE c.`Id`=d.`PageId`) d INNER JOIN ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    #endregion

    #region Include
    /// <summary>
    /// 贪婪加载导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系生成JOIN ON子句。
    /// 可以通过使用InnerJoin(Expression&lt;Func&lt;T1, T2, T3, bool&gt;&gt; joinOn)、RightJoin(Expression&lt;Func&lt;T1, T2, T3, bool&gt;&gt; joinOn)方法改变默认关联方式。
    /// 1:1关联关系，随主表一起查询，1:N关联关系，分两次查询，第二次查询返回结果，再赋值到主实体的属性上。
    /// </summary>
    /// <example>
    /// 1:1关联关系导航属性
    /// <code>
    /// repository.From&lt;Product&gt;()
    ///   .Include(f =&gt; f.Brand)
    ///   .Where(f =&gt; f.ProductNo.Contains("PN-00"))
    ///   .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT a.`Id`,a.`ProductNo`,a.`Name`,a.`BrandId`,a.`CategoryId`,a.`Price`,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt`,b.`Id`,b.`BrandNo`,b.`Name` FROM `sys_product` a LEFT JOIN `sys_brand` b ON a.`BrandId`=b.`Id` WHERE a.`ProductNo` LIKE '%PN-00%'
    /// </code>
    /// </example>
    /// <example>
    /// 1:1关联关系导航属性
    /// <code>
    /// repository.From&lt;Product&gt;()
    ///   .Include(f =&gt; f.Brand)
    ///   .Where(f =&gt; f.ProductNo.Contains("PN-00"))
    ///   .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT a.`Id`,a.`ProductNo`,a.`Name`,a.`BrandId`,a.`CategoryId`,a.`Price`,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt`,b.`Id`,b.`BrandNo`,b.`Name` FROM `sys_product` a LEFT JOIN `sys_brand` b ON a.`BrandId`=b.`Id` WHERE a.`ProductNo` LIKE '%PN-00%'
    /// </code>
    /// </example>
    /// <typeparam name="TMember">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IMultiIncludableQuery<T1, T2, T3, TMember> Include<TMember>(Expression<Func<T1, T2, T3, TMember>> memberSelector);
    /// <summary>
    /// 单表查询，包含1:N关联方式的导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系。
    /// 可以通过使用InnerJoin(Expression&lt;Func&lt;T, bool&gt;&gt; joinOn)、RightJoin(Expression&lt;Func&lt;T, bool&gt;&gt; joinOn)方法改变默认关联方式
    /// </summary>
    /// <typeparam name="TElment">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IMultiIncludableQuery<T1, T2, T3, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null);
    #endregion

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
    IMultiQuery<T1, T2, T3> InnerJoin(Expression<Func<T1, T2, T3, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3> LeftJoin(Expression<Func<T1, T2, T3, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3> RightJoin(Expression<Func<T1, T2, T3, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, TOther, bool>> joinOn);

    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做INNER JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3&gt;()
    ///     ... ...
    ///     .InnerJoin((a, b, c) =&gt; f.From&lt;OrderDetail&gt;()
    ///         .GroupBy(x =&gt; x.OrderId)
    ///         .Select((x, y) =&gt; new
    ///         {
    ///             y.OrderId,
    ///             ProductCount = x.CountDistinct(y.ProductId)
    ///         }), (a, b, c) =&gt; ...)
    ///     ... ...
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... INNER JOIN (SELECT a.`OrderId`,COUNT(DISTINCT a.`ProductId`) AS ProductCount FROM `sys_order_detail` a GROUP BY a.`OrderId`) d ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, TOther> InnerJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做LEFT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3&gt;()
    ///     ... ...
    ///     .LeftJoin((a, b, c) =&gt; f.From&lt;OrderDetail&gt;()
    ///         .GroupBy(x =&gt; x.OrderId)
    ///         .Select((x, y) =&gt; new
    ///         {
    ///             y.OrderId,
    ///             ProductCount = x.CountDistinct(y.ProductId)
    ///         }), (a, b, c) =&gt; ...)
    ///     ... ...
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... INNER JOIN (SELECT a.`OrderId`,COUNT(DISTINCT a.`ProductId`) AS ProductCount FROM `sys_order_detail` a GROUP BY a.`OrderId`) d ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, TOther> LeftJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做RIGHT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3&gt;()
    ///     ... ...
    ///     .RightJoin((a, b, c) =&gt; f.From&lt;OrderDetail&gt;()
    ///         .GroupBy(x =&gt; x.OrderId)
    ///         .Select((x, y) =&gt; new
    ///         {
    ///             y.OrderId,
    ///             ProductCount = x.CountDistinct(y.ProductId)
    ///         }), (a, b, c) =&gt; ...)
    ///     ... ...
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... INNER JOIN (SELECT a.`OrderId`,COUNT(DISTINCT a.`ProductId`) AS ProductCount FROM `sys_order_detail` a GROUP BY a.`OrderId`) d ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, TOther> RightJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, TOther, bool>> joinOn);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3> Where(Expression<Func<T1, T2, T3, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3> Where(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate, Expression<Func<T1, T2, T3, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3> And(Expression<Func<T1, T2, T3, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3> And(bool condition, Expression<Func<T1, T2, T3, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, bool>> elsePredicate = null);
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
    IMultiGroupingQuery<T1, T2, T3, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, TGrouping>> groupingExpr);
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy((a, b) => new { a.Id, b.Id }) 或是 OrderBy(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3> OrderBy<TFields>(Expression<Func<T1, T2, T3, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b) => new { a.Id, b.Id }) 或是 OrderBy(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, TFields>> fieldsExpr);
    /// <summary>
    /// 使用表达式fieldsExpr，生成DSC排序语句，fieldsExpr可以是一或多个字段，用法：
    /// OrderByDescending((a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，用法：
    /// Select((a, b, c) => new { f.Id, f.Name }) 或是 Select((a, b, c) => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定聚合字段返回实体，单个或多个聚合字段的匿名对象，用法：
    /// <code>
    /// repository.From&lt;Order&gt;()
    ///    ... ...
    ///    .SelectAggregate((x, a, b, c) =&gt; new
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
    IMultiQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, TTarget>> fieldsExpr);
    #endregion

    #region Aggregate
    #region Count
    /// <summary>
    /// 返回某个字段的int类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().Count(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Count<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().CountDistinct(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery CountDistinct<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的long类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().LongCount(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery LongCount<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().LongCountDistinct(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery LongCountDistinct<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr);
    #endregion

    /// <summary>
    /// 计算指定字段的求和值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Sum<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的平均值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Avg<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最大值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Max<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最小值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Min<TField>(Expression<Func<T1, T2, T3, TField>> fieldExpr);
    #endregion
}
/// <summary>
/// 多表T1, T2, T3, T4查询
/// </summary>
/// <typeparam name="T1">表T1实体类型</typeparam>
/// <typeparam name="T2">表T2实体类型</typeparam>
/// <typeparam name="T3">表T3实体类型</typeparam>
/// <typeparam name="T4">表T4实体类型</typeparam>
public interface IMultiQuery<T1, T2, T3, T4> : IMultiQueryBase
{
    #region CTE NextWith/NextWithRecursive
    /// <summary>
    /// 继续定义CTE With子句，在Select查询之前，可以定义一个或多个CTE子句，多个CTE With子句要连续定义，用法：
    /// <code>
    /// repository
    ///     .FromWith(f =&gt; ..., "Cte1")
    ///     ... ...
    ///     .NextWith(f =&gt; ..., "CteN")
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
    IMultiQuery<T1, T2, T3, T4, TTarget> NextWith<TTarget>(Func<IFromQuery, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    /// <summary>
    /// 继续定义可递归的CTE With子句，可以引用CTE自身。在Select查询之前，可以定义一个或多个CTE子句，多个CTE With子句要连续定义，首个CTE With子句必须也是可递归的，才能使用本方法，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId })), "MenuList")
    ///     ... ...
    ///     .NextWithRecursive((f, cte) =&gt; f.From&lt;Page, Menu&gt;()
    ///             .Where((a, b) =&gt; a.Id == b.PageId)
    ///             .Select((x, y) =&gt; new { y.Id, y.ParentId, x.Url })
    ///         .UnionAll(x =&gt; x.From&lt;Menu&gt;()
    ///             .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///             .Where((a, b) =&gt; a.Id &gt; 1)
    ///             .Select((x, y) =&gt; new { x.Id, x.ParentId, y.Url })), "MenuPageList")
    ///     ... ...
    ///     .Select((a, b, c, d) =&gt; new { ... ... })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId) AS 
    /// (
    ///     SELECT `Id`,`Name`,`ParentId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    ///     SELECT a.`Id`, a.`Name`, a.`ParentId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// ),
    /// ... ...
    /// MenuPageList(Id,ParentId,Url) AS
    /// (
    ///     SELECT a.`Id`,a.`ParentId`,b.`Url` FROM `sys_menu` a,`sys_page` b WHERE a.`PageId`=b.Id UNION ALL
    ///     SELECT a.`Id`,a.`ParentId`,b.`Url` FROM `sys_menu` a INNER JOIN MenuList b ON a.`Id`=b.`ParentId`
    /// ),
    /// ... ...
    /// SELECT ... FROM MenuList a INNER JOIN ... INNER JOIN MenuPageList f ON ... .... WHERE ... ...
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
    /// <param name="tableAsStart">CTE子句中使用的表别名开始字母，默认从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, TTarget> NextWithRecursive<TTarget>(Func<IFromQuery, string, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    #endregion

    #region WithTable
    /// <summary>
    /// 使用子查询作为临时表，方便后面做关联查询，用法：
    /// <code>
    /// repository
    ///     ... ...
    ///     .WithTable(f =&gt; f.From&lt;Page, Menu&gt;()
    ///         .Where((a, b) =&gt; a.Id == b.PageId)
    ///         .Select((x, y) =&gt; new { y.Id, y.ParentId, x.Url }))
    ///     .InnerJoin((a, b, c, d) =&gt; ...)
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... (SELECT b.`Id`,b.`ParentId`,a.`Url` FROM `sys_page` a,`sys_menu` b WHERE c.`Id`=d.`PageId`) e INNER JOIN ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    #endregion

    #region Include
    /// <summary>
    /// 贪婪加载导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系生成JOIN ON子句。
    /// 可以通过使用InnerJoin(Expression&lt;Func&lt;T1, T2, T3, T4, bool&gt;&gt; joinOn)、RightJoin(Expression&lt;Func&lt;T1, T2, T3, T4, bool&gt;&gt; joinOn)方法改变默认关联方式。
    /// 1:1关联关系，随主表一起查询，1:N关联关系，分两次查询，第二次查询返回结果，再赋值到主实体的属性上。
    /// </summary>
    /// <example>
    /// 1:1关联关系导航属性
    /// <code>
    /// repository.From&lt;Product&gt;()
    ///   .Include(f =&gt; f.Brand)
    ///   .Where(f =&gt; f.ProductNo.Contains("PN-00"))
    ///   .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT a.`Id`,a.`ProductNo`,a.`Name`,a.`BrandId`,a.`CategoryId`,a.`Price`,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt`,b.`Id`,b.`BrandNo`,b.`Name` FROM `sys_product` a LEFT JOIN `sys_brand` b ON a.`BrandId`=b.`Id` WHERE a.`ProductNo` LIKE '%PN-00%'
    /// </code>
    /// </example>
    /// <example>
    /// 1:1关联关系导航属性
    /// <code>
    /// repository.From&lt;Product&gt;()
    ///   .Include(f =&gt; f.Brand)
    ///   .Where(f =&gt; f.ProductNo.Contains("PN-00"))
    ///   .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT a.`Id`,a.`ProductNo`,a.`Name`,a.`BrandId`,a.`CategoryId`,a.`Price`,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt`,b.`Id`,b.`BrandNo`,b.`Name` FROM `sys_product` a LEFT JOIN `sys_brand` b ON a.`BrandId`=b.`Id` WHERE a.`ProductNo` LIKE '%PN-00%'
    /// </code>
    /// </example>
    /// <typeparam name="TMember">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IMultiIncludableQuery<T1, T2, T3, T4, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, TMember>> memberSelector);
    /// <summary>
    /// 单表查询，包含1:N关联方式的导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系。
    /// 可以通过使用InnerJoin(Expression&lt;Func&lt;T, bool&gt;&gt; joinOn)、RightJoin(Expression&lt;Func&lt;T, bool&gt;&gt; joinOn)方法改变默认关联方式
    /// </summary>
    /// <typeparam name="TElment">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IMultiIncludableQuery<T1, T2, T3, T4, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null);
    #endregion

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
    IMultiQuery<T1, T2, T3, T4> InnerJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4> LeftJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4> RightJoin(Expression<Func<T1, T2, T3, T4, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn);

    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做INNER JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4&gt;()
    ///     ... ...
    ///     .InnerJoin((a, b, c, d) =&gt; f.From&lt;OrderDetail&gt;()
    ///         .GroupBy(x =&gt; x.OrderId)
    ///         .Select((x, y) =&gt; new
    ///         {
    ///             y.OrderId,
    ///             ProductCount = x.CountDistinct(y.ProductId)
    ///         }), (a, b, c, d) =&gt; ...)
    ///     ... ...
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... INNER JOIN (SELECT a.`OrderId`,COUNT(DISTINCT a.`ProductId`) AS ProductCount FROM `sys_order_detail` a GROUP BY a.`OrderId`) e ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, TOther> InnerJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做LEFT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4&gt;()
    ///     ... ...
    ///     .LeftJoin((a, b, c, d) =&gt; f.From&lt;OrderDetail&gt;()
    ///         .GroupBy(x =&gt; x.OrderId)
    ///         .Select((x, y) =&gt; new
    ///         {
    ///             y.OrderId,
    ///             ProductCount = x.CountDistinct(y.ProductId)
    ///         }), (a, b, c, d) =&gt; ...)
    ///     ... ...
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... INNER JOIN (SELECT a.`OrderId`,COUNT(DISTINCT a.`ProductId`) AS ProductCount FROM `sys_order_detail` a GROUP BY a.`OrderId`) e ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, TOther> LeftJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做RIGHT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4&gt;()
    ///     ... ...
    ///     .RightJoin((a, b, c, d) =&gt; f.From&lt;OrderDetail&gt;()
    ///         .GroupBy(x =&gt; x.OrderId)
    ///         .Select((x, y) =&gt; new
    ///         {
    ///             y.OrderId,
    ///             ProductCount = x.CountDistinct(y.ProductId)
    ///         }), (a, b, c, d) =&gt; ...)
    ///     ... ...
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... INNER JOIN (SELECT a.`OrderId`,COUNT(DISTINCT a.`ProductId`) AS ProductCount FROM `sys_order_detail` a GROUP BY a.`OrderId`) e ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, TOther> RightJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, TOther, bool>> joinOn);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4> Where(Expression<Func<T1, T2, T3, T4, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4> Where(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4> And(Expression<Func<T1, T2, T3, T4, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4> And(bool condition, Expression<Func<T1, T2, T3, T4, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, bool>> elsePredicate = null);
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
    IMultiGroupingQuery<T1, T2, T3, T4, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, TGrouping>> groupingExpr);
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy((a, b) => new { a.Id, b.Id }) 或是 OrderBy(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b) => new { a.Id, b.Id }) 或是 OrderBy(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr);
    /// <summary>
    /// 使用表达式fieldsExpr，生成DSC排序语句，fieldsExpr可以是一或多个字段，用法：
    /// OrderByDescending((a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，用法：
    /// Select((a, b, c, d) => new { f.Id, f.Name }) 或是 Select((a, b, c, d) => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定聚合字段返回实体，单个或多个聚合字段的匿名对象，用法：
    /// <code>
    /// repository.From&lt;Order&gt;()
    ///    ... ...
    ///    .SelectAggregate((x, a, b, c, d) =&gt; new
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
    IMultiQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, TTarget>> fieldsExpr);
    #endregion

    #region Aggregate
    #region Count
    /// <summary>
    /// 返回某个字段的int类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().Count(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Count<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().CountDistinct(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的long类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().LongCount(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery LongCount<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().LongCountDistinct(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr);
    #endregion

    /// <summary>
    /// 计算指定字段的求和值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Sum<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的平均值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Avg<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最大值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Max<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最小值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Min<TField>(Expression<Func<T1, T2, T3, T4, TField>> fieldExpr);
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
public interface IMultiQuery<T1, T2, T3, T4, T5> : IMultiQueryBase
{
    #region CTE NextWith/NextWithRecursive
    /// <summary>
    /// 继续定义CTE With子句，在Select查询之前，可以定义一个或多个CTE子句，多个CTE With子句要连续定义，用法：
    /// <code>
    /// repository
    ///     .FromWith(f =&gt; ..., "Cte1")
    ///     ... ...
    ///     .NextWith(f =&gt; ..., "CteN")
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
    IMultiQuery<T1, T2, T3, T4, T5, TTarget> NextWith<TTarget>(Func<IFromQuery, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    /// <summary>
    /// 继续定义可递归的CTE With子句，可以引用CTE自身。在Select查询之前，可以定义一个或多个CTE子句，多个CTE With子句要连续定义，首个CTE With子句必须也是可递归的，才能使用本方法，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId })), "MenuList")
    ///     ... ...
    ///     .NextWithRecursive((f, cte) =&gt; f.From&lt;Page, Menu&gt;()
    ///             .Where((a, b) =&gt; a.Id == b.PageId)
    ///             .Select((x, y) =&gt; new { y.Id, y.ParentId, x.Url })
    ///         .UnionAll(x =&gt; x.From&lt;Menu&gt;()
    ///             .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///             .Where((a, b) =&gt; a.Id &gt; 1)
    ///             .Select((x, y) =&gt; new { x.Id, x.ParentId, y.Url })), "MenuPageList")
    ///     ... ...
    ///     .Select((a, b, c, d, e) =&gt; new { ... ... })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId) AS 
    /// (
    ///     SELECT `Id`,`Name`,`ParentId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    ///     SELECT a.`Id`, a.`Name`, a.`ParentId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// ),
    /// ... ...
    /// MenuPageList(Id,ParentId,Url) AS
    /// (
    ///     SELECT a.`Id`,a.`ParentId`,b.`Url` FROM `sys_menu` a,`sys_page` b WHERE a.`PageId`=b.Id UNION ALL
    ///     SELECT a.`Id`,a.`ParentId`,b.`Url` FROM `sys_menu` a INNER JOIN MenuList b ON a.`Id`=b.`ParentId`
    /// ),
    /// ... ...
    /// SELECT ... FROM MenuList a INNER JOIN ... INNER JOIN MenuPageList f ON ... .... WHERE ... ...
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
    /// <param name="tableAsStart">CTE子句中使用的表别名开始字母，默认从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, TTarget> NextWithRecursive<TTarget>(Func<IFromQuery, string, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    #endregion

    #region WithTable
    /// <summary>
    /// 使用子查询作为临时表，方便后面做关联查询，用法：
    /// <code>
    /// repository
    ///     ... ...
    ///     .WithTable(f =&gt; f.From&lt;Page, Menu&gt;()
    ///         .Where((a, b) =&gt; a.Id == b.PageId)
    ///         .Select((x, y) =&gt; new { y.Id, y.ParentId, x.Url }))
    ///     .InnerJoin((a, b, c, d, e) =&gt; ...)
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... (SELECT b.`Id`,b.`ParentId`,a.`Url` FROM `sys_page` a,`sys_menu` b WHERE c.`Id`=d.`PageId`) f INNER JOIN ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    #endregion

    #region Include
    /// <summary>
    /// 贪婪加载导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系生成JOIN ON子句。
    /// 可以通过使用InnerJoin(Expression&lt;Func&lt;T1, T2, T3, T4, T5, bool&gt;&gt; joinOn)、RightJoin(Expression&lt;Func&lt;T1, T2, T3, T4, T5, bool&gt;&gt; joinOn)方法改变默认关联方式。
    /// 1:1关联关系，随主表一起查询，1:N关联关系，分两次查询，第二次查询返回结果，再赋值到主实体的属性上。
    /// </summary>
    /// <example>
    /// 1:1关联关系导航属性
    /// <code>
    /// repository.From&lt;Product&gt;()
    ///   .Include(f =&gt; f.Brand)
    ///   .Where(f =&gt; f.ProductNo.Contains("PN-00"))
    ///   .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT a.`Id`,a.`ProductNo`,a.`Name`,a.`BrandId`,a.`CategoryId`,a.`Price`,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt`,b.`Id`,b.`BrandNo`,b.`Name` FROM `sys_product` a LEFT JOIN `sys_brand` b ON a.`BrandId`=b.`Id` WHERE a.`ProductNo` LIKE '%PN-00%'
    /// </code>
    /// </example>
    /// <example>
    /// 1:1关联关系导航属性
    /// <code>
    /// repository.From&lt;Product&gt;()
    ///   .Include(f =&gt; f.Brand)
    ///   .Where(f =&gt; f.ProductNo.Contains("PN-00"))
    ///   .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT a.`Id`,a.`ProductNo`,a.`Name`,a.`BrandId`,a.`CategoryId`,a.`Price`,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt`,b.`Id`,b.`BrandNo`,b.`Name` FROM `sys_product` a LEFT JOIN `sys_brand` b ON a.`BrandId`=b.`Id` WHERE a.`ProductNo` LIKE '%PN-00%'
    /// </code>
    /// </example>
    /// <typeparam name="TMember">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IMultiIncludableQuery<T1, T2, T3, T4, T5, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, TMember>> memberSelector);
    /// <summary>
    /// 单表查询，包含1:N关联方式的导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系。
    /// 可以通过使用InnerJoin(Expression&lt;Func&lt;T, bool&gt;&gt; joinOn)、RightJoin(Expression&lt;Func&lt;T, bool&gt;&gt; joinOn)方法改变默认关联方式
    /// </summary>
    /// <typeparam name="TElment">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IMultiIncludableQuery<T1, T2, T3, T4, T5, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, T5, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null);
    #endregion

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
    IMultiQuery<T1, T2, T3, T4, T5> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5> RightJoin(Expression<Func<T1, T2, T3, T4, T5, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn);

    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做INNER JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5&gt;()
    ///     ... ...
    ///     .InnerJoin((a, b, c, d, e) =&gt; f.From&lt;OrderDetail&gt;()
    ///         .GroupBy(x =&gt; x.OrderId)
    ///         .Select((x, y) =&gt; new
    ///         {
    ///             y.OrderId,
    ///             ProductCount = x.CountDistinct(y.ProductId)
    ///         }), (a, b, c, d, e) =&gt; ...)
    ///     ... ...
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... INNER JOIN (SELECT a.`OrderId`,COUNT(DISTINCT a.`ProductId`) AS ProductCount FROM `sys_order_detail` a GROUP BY a.`OrderId`) f ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, TOther> InnerJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做LEFT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5&gt;()
    ///     ... ...
    ///     .LeftJoin((a, b, c, d, e) =&gt; f.From&lt;OrderDetail&gt;()
    ///         .GroupBy(x =&gt; x.OrderId)
    ///         .Select((x, y) =&gt; new
    ///         {
    ///             y.OrderId,
    ///             ProductCount = x.CountDistinct(y.ProductId)
    ///         }), (a, b, c, d, e) =&gt; ...)
    ///     ... ...
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... INNER JOIN (SELECT a.`OrderId`,COUNT(DISTINCT a.`ProductId`) AS ProductCount FROM `sys_order_detail` a GROUP BY a.`OrderId`) f ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, TOther> LeftJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做RIGHT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5&gt;()
    ///     ... ...
    ///     .RightJoin((a, b, c, d, e) =&gt; f.From&lt;OrderDetail&gt;()
    ///         .GroupBy(x =&gt; x.OrderId)
    ///         .Select((x, y) =&gt; new
    ///         {
    ///             y.OrderId,
    ///             ProductCount = x.CountDistinct(y.ProductId)
    ///         }), (a, b, c, d, e) =&gt; ...)
    ///     ... ...
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... INNER JOIN (SELECT a.`OrderId`,COUNT(DISTINCT a.`ProductId`) AS ProductCount FROM `sys_order_detail` a GROUP BY a.`OrderId`) f ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, TOther> RightJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, TOther, bool>> joinOn);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5> Where(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5> And(Expression<Func<T1, T2, T3, T4, T5, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, bool>> elsePredicate = null);
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
    IMultiGroupingQuery<T1, T2, T3, T4, T5, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, TGrouping>> groupingExpr);
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy((a, b) => new { a.Id, b.Id }) 或是 OrderBy(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b) => new { a.Id, b.Id }) 或是 OrderBy(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    /// <summary>
    /// 使用表达式fieldsExpr，生成DSC排序语句，fieldsExpr可以是一或多个字段，用法：
    /// OrderByDescending((a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，用法：
    /// Select((a, b, c, d, e) => new { f.Id, f.Name }) 或是 Select((a, b, c, d, e) => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定聚合字段返回实体，单个或多个聚合字段的匿名对象，用法：
    /// <code>
    /// repository.From&lt;Order&gt;()
    ///    ... ...
    ///    .SelectAggregate((x, a, b, c, d, e) =&gt; new
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
    IMultiQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, TTarget>> fieldsExpr);
    #endregion

    #region Aggregate
    #region Count
    /// <summary>
    /// 返回某个字段的int类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().Count(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Count<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().CountDistinct(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的long类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().LongCount(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().LongCountDistinct(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr);
    #endregion

    /// <summary>
    /// 计算指定字段的求和值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的平均值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最大值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Max<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最小值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Min<TField>(Expression<Func<T1, T2, T3, T4, T5, TField>> fieldExpr);
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
public interface IMultiQuery<T1, T2, T3, T4, T5, T6> : IMultiQueryBase
{
    #region CTE NextWith/NextWithRecursive
    /// <summary>
    /// 继续定义CTE With子句，在Select查询之前，可以定义一个或多个CTE子句，多个CTE With子句要连续定义，用法：
    /// <code>
    /// repository
    ///     .FromWith(f =&gt; ..., "Cte1")
    ///     ... ...
    ///     .NextWith(f =&gt; ..., "CteN")
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, TTarget> NextWith<TTarget>(Func<IFromQuery, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    /// <summary>
    /// 继续定义可递归的CTE With子句，可以引用CTE自身。在Select查询之前，可以定义一个或多个CTE子句，多个CTE With子句要连续定义，首个CTE With子句必须也是可递归的，才能使用本方法，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId })), "MenuList")
    ///     ... ...
    ///     .NextWithRecursive((f, cte) =&gt; f.From&lt;Page, Menu&gt;()
    ///             .Where((a, b) =&gt; a.Id == b.PageId)
    ///             .Select((x, y) =&gt; new { y.Id, y.ParentId, x.Url })
    ///         .UnionAll(x =&gt; x.From&lt;Menu&gt;()
    ///             .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///             .Where((a, b) =&gt; a.Id &gt; 1)
    ///             .Select((x, y) =&gt; new { x.Id, x.ParentId, y.Url })), "MenuPageList")
    ///     ... ...
    ///     .Select((a, b, c, d, e, f) =&gt; new { ... ... })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId) AS 
    /// (
    ///     SELECT `Id`,`Name`,`ParentId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    ///     SELECT a.`Id`, a.`Name`, a.`ParentId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// ),
    /// ... ...
    /// MenuPageList(Id,ParentId,Url) AS
    /// (
    ///     SELECT a.`Id`,a.`ParentId`,b.`Url` FROM `sys_menu` a,`sys_page` b WHERE a.`PageId`=b.Id UNION ALL
    ///     SELECT a.`Id`,a.`ParentId`,b.`Url` FROM `sys_menu` a INNER JOIN MenuList b ON a.`Id`=b.`ParentId`
    /// ),
    /// ... ...
    /// SELECT ... FROM MenuList a INNER JOIN ... INNER JOIN MenuPageList f ON ... .... WHERE ... ...
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
    /// <param name="tableAsStart">CTE子句中使用的表别名开始字母，默认从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, TTarget> NextWithRecursive<TTarget>(Func<IFromQuery, string, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    #endregion

    #region WithTable
    /// <summary>
    /// 使用子查询作为临时表，方便后面做关联查询，用法：
    /// <code>
    /// repository
    ///     ... ...
    ///     .WithTable(f =&gt; f.From&lt;Page, Menu&gt;()
    ///         .Where((a, b) =&gt; a.Id == b.PageId)
    ///         .Select((x, y) =&gt; new { y.Id, y.ParentId, x.Url }))
    ///     .InnerJoin((a, b, c, d, e, f) =&gt; ...)
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... (SELECT b.`Id`,b.`ParentId`,a.`Url` FROM `sys_page` a,`sys_menu` b WHERE c.`Id`=d.`PageId`) g INNER JOIN ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    #endregion

    #region Include
    /// <summary>
    /// 贪婪加载导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系生成JOIN ON子句。
    /// 可以通过使用InnerJoin(Expression&lt;Func&lt;T1, T2, T3, T4, T5, T6, bool&gt;&gt; joinOn)、RightJoin(Expression&lt;Func&lt;T1, T2, T3, T4, T5, T6, bool&gt;&gt; joinOn)方法改变默认关联方式。
    /// 1:1关联关系，随主表一起查询，1:N关联关系，分两次查询，第二次查询返回结果，再赋值到主实体的属性上。
    /// </summary>
    /// <example>
    /// 1:1关联关系导航属性
    /// <code>
    /// repository.From&lt;Product&gt;()
    ///   .Include(f =&gt; f.Brand)
    ///   .Where(f =&gt; f.ProductNo.Contains("PN-00"))
    ///   .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT a.`Id`,a.`ProductNo`,a.`Name`,a.`BrandId`,a.`CategoryId`,a.`Price`,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt`,b.`Id`,b.`BrandNo`,b.`Name` FROM `sys_product` a LEFT JOIN `sys_brand` b ON a.`BrandId`=b.`Id` WHERE a.`ProductNo` LIKE '%PN-00%'
    /// </code>
    /// </example>
    /// <example>
    /// 1:1关联关系导航属性
    /// <code>
    /// repository.From&lt;Product&gt;()
    ///   .Include(f =&gt; f.Brand)
    ///   .Where(f =&gt; f.ProductNo.Contains("PN-00"))
    ///   .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT a.`Id`,a.`ProductNo`,a.`Name`,a.`BrandId`,a.`CategoryId`,a.`Price`,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt`,b.`Id`,b.`BrandNo`,b.`Name` FROM `sys_product` a LEFT JOIN `sys_brand` b ON a.`BrandId`=b.`Id` WHERE a.`ProductNo` LIKE '%PN-00%'
    /// </code>
    /// </example>
    /// <typeparam name="TMember">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, TMember>> memberSelector);
    /// <summary>
    /// 单表查询，包含1:N关联方式的导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系。
    /// 可以通过使用InnerJoin(Expression&lt;Func&lt;T, bool&gt;&gt; joinOn)、RightJoin(Expression&lt;Func&lt;T, bool&gt;&gt; joinOn)方法改变默认关联方式
    /// </summary>
    /// <typeparam name="TElment">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, T5, T6, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null);
    #endregion

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
    IMultiQuery<T1, T2, T3, T4, T5, T6> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, T6> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, T6> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn);

    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做INNER JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6&gt;()
    ///     ... ...
    ///     .InnerJoin((a, b, c, d, e, f) =&gt; f.From&lt;OrderDetail&gt;()
    ///         .GroupBy(x =&gt; x.OrderId)
    ///         .Select((x, y) =&gt; new
    ///         {
    ///             y.OrderId,
    ///             ProductCount = x.CountDistinct(y.ProductId)
    ///         }), (a, b, c, d, e, f) =&gt; ...)
    ///     ... ...
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... INNER JOIN (SELECT a.`OrderId`,COUNT(DISTINCT a.`ProductId`) AS ProductCount FROM `sys_order_detail` a GROUP BY a.`OrderId`) g ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, TOther> InnerJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做LEFT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6&gt;()
    ///     ... ...
    ///     .LeftJoin((a, b, c, d, e, f) =&gt; f.From&lt;OrderDetail&gt;()
    ///         .GroupBy(x =&gt; x.OrderId)
    ///         .Select((x, y) =&gt; new
    ///         {
    ///             y.OrderId,
    ///             ProductCount = x.CountDistinct(y.ProductId)
    ///         }), (a, b, c, d, e, f) =&gt; ...)
    ///     ... ...
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... INNER JOIN (SELECT a.`OrderId`,COUNT(DISTINCT a.`ProductId`) AS ProductCount FROM `sys_order_detail` a GROUP BY a.`OrderId`) g ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, TOther> LeftJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做RIGHT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6&gt;()
    ///     ... ...
    ///     .RightJoin((a, b, c, d, e, f) =&gt; f.From&lt;OrderDetail&gt;()
    ///         .GroupBy(x =&gt; x.OrderId)
    ///         .Select((x, y) =&gt; new
    ///         {
    ///             y.OrderId,
    ///             ProductCount = x.CountDistinct(y.ProductId)
    ///         }), (a, b, c, d, e, f) =&gt; ...)
    ///     ... ...
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... INNER JOIN (SELECT a.`OrderId`,COUNT(DISTINCT a.`ProductId`) AS ProductCount FROM `sys_order_detail` a GROUP BY a.`OrderId`) g ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, TOther> RightJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, TOther, bool>> joinOn);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6> Where(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6> And(Expression<Func<T1, T2, T3, T4, T5, T6, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, bool>> elsePredicate = null);
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
    IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, TGrouping>> groupingExpr);
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy((a, b) => new { a.Id, b.Id }) 或是 OrderBy(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b) => new { a.Id, b.Id }) 或是 OrderBy(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr);
    /// <summary>
    /// 使用表达式fieldsExpr，生成DSC排序语句，fieldsExpr可以是一或多个字段，用法：
    /// OrderByDescending((a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，用法：
    /// Select((a, b, c, d, e, f) => new { f.Id, f.Name }) 或是 Select((a, b, c, d, e, f) => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定聚合字段返回实体，单个或多个聚合字段的匿名对象，用法：
    /// <code>
    /// repository.From&lt;Order&gt;()
    ///    ... ...
    ///    .SelectAggregate((x, a, b, c, d, e, f) =&gt; new
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
    IMultiQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, TTarget>> fieldsExpr);
    #endregion

    #region Aggregate
    #region Count
    /// <summary>
    /// 返回某个字段的int类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().Count(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().CountDistinct(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的long类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().LongCount(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().LongCountDistinct(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr);
    #endregion

    /// <summary>
    /// 计算指定字段的求和值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的平均值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最大值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最小值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, TField>> fieldExpr);
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
public interface IMultiQuery<T1, T2, T3, T4, T5, T6, T7> : IMultiQueryBase
{
    #region CTE NextWith/NextWithRecursive
    /// <summary>
    /// 继续定义CTE With子句，在Select查询之前，可以定义一个或多个CTE子句，多个CTE With子句要连续定义，用法：
    /// <code>
    /// repository
    ///     .FromWith(f =&gt; ..., "Cte1")
    ///     ... ...
    ///     .NextWith(f =&gt; ..., "CteN")
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, TTarget> NextWith<TTarget>(Func<IFromQuery, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    /// <summary>
    /// 继续定义可递归的CTE With子句，可以引用CTE自身。在Select查询之前，可以定义一个或多个CTE子句，多个CTE With子句要连续定义，首个CTE With子句必须也是可递归的，才能使用本方法，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId })), "MenuList")
    ///     ... ...
    ///     .NextWithRecursive((f, cte) =&gt; f.From&lt;Page, Menu&gt;()
    ///             .Where((a, b) =&gt; a.Id == b.PageId)
    ///             .Select((x, y) =&gt; new { y.Id, y.ParentId, x.Url })
    ///         .UnionAll(x =&gt; x.From&lt;Menu&gt;()
    ///             .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///             .Where((a, b) =&gt; a.Id &gt; 1)
    ///             .Select((x, y) =&gt; new { x.Id, x.ParentId, y.Url })), "MenuPageList")
    ///     ... ...
    ///     .Select((a, b, c, d, e, f, g) =&gt; new { ... ... })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId) AS 
    /// (
    ///     SELECT `Id`,`Name`,`ParentId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    ///     SELECT a.`Id`, a.`Name`, a.`ParentId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// ),
    /// ... ...
    /// MenuPageList(Id,ParentId,Url) AS
    /// (
    ///     SELECT a.`Id`,a.`ParentId`,b.`Url` FROM `sys_menu` a,`sys_page` b WHERE a.`PageId`=b.Id UNION ALL
    ///     SELECT a.`Id`,a.`ParentId`,b.`Url` FROM `sys_menu` a INNER JOIN MenuList b ON a.`Id`=b.`ParentId`
    /// ),
    /// ... ...
    /// SELECT ... FROM MenuList a INNER JOIN ... INNER JOIN MenuPageList f ON ... .... WHERE ... ...
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
    /// <param name="tableAsStart">CTE子句中使用的表别名开始字母，默认从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, TTarget> NextWithRecursive<TTarget>(Func<IFromQuery, string, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    #endregion

    #region WithTable
    /// <summary>
    /// 使用子查询作为临时表，方便后面做关联查询，用法：
    /// <code>
    /// repository
    ///     ... ...
    ///     .WithTable(f =&gt; f.From&lt;Page, Menu&gt;()
    ///         .Where((a, b) =&gt; a.Id == b.PageId)
    ///         .Select((x, y) =&gt; new { y.Id, y.ParentId, x.Url }))
    ///     .InnerJoin((a, b, c, d, e, f, g) =&gt; ...)
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... (SELECT b.`Id`,b.`ParentId`,a.`Url` FROM `sys_page` a,`sys_menu` b WHERE c.`Id`=d.`PageId`) h INNER JOIN ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    #endregion

    #region Include
    /// <summary>
    /// 贪婪加载导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系生成JOIN ON子句。
    /// 可以通过使用InnerJoin(Expression&lt;Func&lt;T1, T2, T3, T4, T5, T6, T7, bool&gt;&gt; joinOn)、RightJoin(Expression&lt;Func&lt;T1, T2, T3, T4, T5, T6, T7, bool&gt;&gt; joinOn)方法改变默认关联方式。
    /// 1:1关联关系，随主表一起查询，1:N关联关系，分两次查询，第二次查询返回结果，再赋值到主实体的属性上。
    /// </summary>
    /// <example>
    /// 1:1关联关系导航属性
    /// <code>
    /// repository.From&lt;Product&gt;()
    ///   .Include(f =&gt; f.Brand)
    ///   .Where(f =&gt; f.ProductNo.Contains("PN-00"))
    ///   .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT a.`Id`,a.`ProductNo`,a.`Name`,a.`BrandId`,a.`CategoryId`,a.`Price`,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt`,b.`Id`,b.`BrandNo`,b.`Name` FROM `sys_product` a LEFT JOIN `sys_brand` b ON a.`BrandId`=b.`Id` WHERE a.`ProductNo` LIKE '%PN-00%'
    /// </code>
    /// </example>
    /// <example>
    /// 1:1关联关系导航属性
    /// <code>
    /// repository.From&lt;Product&gt;()
    ///   .Include(f =&gt; f.Brand)
    ///   .Where(f =&gt; f.ProductNo.Contains("PN-00"))
    ///   .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT a.`Id`,a.`ProductNo`,a.`Name`,a.`BrandId`,a.`CategoryId`,a.`Price`,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt`,b.`Id`,b.`BrandNo`,b.`Name` FROM `sys_product` a LEFT JOIN `sys_brand` b ON a.`BrandId`=b.`Id` WHERE a.`ProductNo` LIKE '%PN-00%'
    /// </code>
    /// </example>
    /// <typeparam name="TMember">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TMember>> memberSelector);
    /// <summary>
    /// 单表查询，包含1:N关联方式的导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系。
    /// 可以通过使用InnerJoin(Expression&lt;Func&lt;T, bool&gt;&gt; joinOn)、RightJoin(Expression&lt;Func&lt;T, bool&gt;&gt; joinOn)方法改变默认关联方式
    /// </summary>
    /// <typeparam name="TElment">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null);
    #endregion

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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn);

    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做INNER JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7&gt;()
    ///     ... ...
    ///     .InnerJoin((a, b, c, d, e, f, g) =&gt; f.From&lt;OrderDetail&gt;()
    ///         .GroupBy(x =&gt; x.OrderId)
    ///         .Select((x, y) =&gt; new
    ///         {
    ///             y.OrderId,
    ///             ProductCount = x.CountDistinct(y.ProductId)
    ///         }), (a, b, c, d, e, f, g) =&gt; ...)
    ///     ... ...
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... INNER JOIN (SELECT a.`OrderId`,COUNT(DISTINCT a.`ProductId`) AS ProductCount FROM `sys_order_detail` a GROUP BY a.`OrderId`) h ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther> InnerJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做LEFT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7&gt;()
    ///     ... ...
    ///     .LeftJoin((a, b, c, d, e, f, g) =&gt; f.From&lt;OrderDetail&gt;()
    ///         .GroupBy(x =&gt; x.OrderId)
    ///         .Select((x, y) =&gt; new
    ///         {
    ///             y.OrderId,
    ///             ProductCount = x.CountDistinct(y.ProductId)
    ///         }), (a, b, c, d, e, f, g) =&gt; ...)
    ///     ... ...
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... INNER JOIN (SELECT a.`OrderId`,COUNT(DISTINCT a.`ProductId`) AS ProductCount FROM `sys_order_detail` a GROUP BY a.`OrderId`) h ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther> LeftJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做RIGHT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7&gt;()
    ///     ... ...
    ///     .RightJoin((a, b, c, d, e, f, g) =&gt; f.From&lt;OrderDetail&gt;()
    ///         .GroupBy(x =&gt; x.OrderId)
    ///         .Select((x, y) =&gt; new
    ///         {
    ///             y.OrderId,
    ///             ProductCount = x.CountDistinct(y.ProductId)
    ///         }), (a, b, c, d, e, f, g) =&gt; ...)
    ///     ... ...
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... INNER JOIN (SELECT a.`OrderId`,COUNT(DISTINCT a.`ProductId`) AS ProductCount FROM `sys_order_detail` a GROUP BY a.`OrderId`) h ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, TOther> RightJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TOther, bool>> joinOn);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, bool>> elsePredicate = null);
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
    IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TGrouping>> groupingExpr);
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy((a, b) => new { a.Id, b.Id }) 或是 OrderBy(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b) => new { a.Id, b.Id }) 或是 OrderBy(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr);
    /// <summary>
    /// 使用表达式fieldsExpr，生成DSC排序语句，fieldsExpr可以是一或多个字段，用法：
    /// OrderByDescending((a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，用法：
    /// Select((a, b, c, d, e, f, g) => new { f.Id, f.Name }) 或是 Select((a, b, c, d, e, f, g) => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定聚合字段返回实体，单个或多个聚合字段的匿名对象，用法：
    /// <code>
    /// repository.From&lt;Order&gt;()
    ///    ... ...
    ///    .SelectAggregate((x, a, b, c, d, e, f, g) =&gt; new
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
    IMultiQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, TTarget>> fieldsExpr);
    #endregion

    #region Aggregate
    #region Count
    /// <summary>
    /// 返回某个字段的int类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().Count(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().CountDistinct(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的long类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().LongCount(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().LongCountDistinct(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr);
    #endregion

    /// <summary>
    /// 计算指定字段的求和值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的平均值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最大值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最小值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, TField>> fieldExpr);
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
public interface IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> : IMultiQueryBase
{
    #region CTE NextWith/NextWithRecursive
    /// <summary>
    /// 继续定义CTE With子句，在Select查询之前，可以定义一个或多个CTE子句，多个CTE With子句要连续定义，用法：
    /// <code>
    /// repository
    ///     .FromWith(f =&gt; ..., "Cte1")
    ///     ... ...
    ///     .NextWith(f =&gt; ..., "CteN")
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TTarget> NextWith<TTarget>(Func<IFromQuery, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    /// <summary>
    /// 继续定义可递归的CTE With子句，可以引用CTE自身。在Select查询之前，可以定义一个或多个CTE子句，多个CTE With子句要连续定义，首个CTE With子句必须也是可递归的，才能使用本方法，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId })), "MenuList")
    ///     ... ...
    ///     .NextWithRecursive((f, cte) =&gt; f.From&lt;Page, Menu&gt;()
    ///             .Where((a, b) =&gt; a.Id == b.PageId)
    ///             .Select((x, y) =&gt; new { y.Id, y.ParentId, x.Url })
    ///         .UnionAll(x =&gt; x.From&lt;Menu&gt;()
    ///             .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///             .Where((a, b) =&gt; a.Id &gt; 1)
    ///             .Select((x, y) =&gt; new { x.Id, x.ParentId, y.Url })), "MenuPageList")
    ///     ... ...
    ///     .Select((a, b, c, d, e, f, g, h) =&gt; new { ... ... })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId) AS 
    /// (
    ///     SELECT `Id`,`Name`,`ParentId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    ///     SELECT a.`Id`, a.`Name`, a.`ParentId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// ),
    /// ... ...
    /// MenuPageList(Id,ParentId,Url) AS
    /// (
    ///     SELECT a.`Id`,a.`ParentId`,b.`Url` FROM `sys_menu` a,`sys_page` b WHERE a.`PageId`=b.Id UNION ALL
    ///     SELECT a.`Id`,a.`ParentId`,b.`Url` FROM `sys_menu` a INNER JOIN MenuList b ON a.`Id`=b.`ParentId`
    /// ),
    /// ... ...
    /// SELECT ... FROM MenuList a INNER JOIN ... INNER JOIN MenuPageList f ON ... .... WHERE ... ...
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
    /// <param name="tableAsStart">CTE子句中使用的表别名开始字母，默认从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TTarget> NextWithRecursive<TTarget>(Func<IFromQuery, string, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    #endregion

    #region WithTable
    /// <summary>
    /// 使用子查询作为临时表，方便后面做关联查询，用法：
    /// <code>
    /// repository
    ///     ... ...
    ///     .WithTable(f =&gt; f.From&lt;Page, Menu&gt;()
    ///         .Where((a, b) =&gt; a.Id == b.PageId)
    ///         .Select((x, y) =&gt; new { y.Id, y.ParentId, x.Url }))
    ///     .InnerJoin((a, b, c, d, e, f, g, h) =&gt; ...)
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... (SELECT b.`Id`,b.`ParentId`,a.`Url` FROM `sys_page` a,`sys_menu` b WHERE c.`Id`=d.`PageId`) i INNER JOIN ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    #endregion

    #region Include
    /// <summary>
    /// 贪婪加载导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系生成JOIN ON子句。
    /// 可以通过使用InnerJoin(Expression&lt;Func&lt;T1, T2, T3, T4, T5, T6, T7, T8, bool&gt;&gt; joinOn)、RightJoin(Expression&lt;Func&lt;T1, T2, T3, T4, T5, T6, T7, T8, bool&gt;&gt; joinOn)方法改变默认关联方式。
    /// 1:1关联关系，随主表一起查询，1:N关联关系，分两次查询，第二次查询返回结果，再赋值到主实体的属性上。
    /// </summary>
    /// <example>
    /// 1:1关联关系导航属性
    /// <code>
    /// repository.From&lt;Product&gt;()
    ///   .Include(f =&gt; f.Brand)
    ///   .Where(f =&gt; f.ProductNo.Contains("PN-00"))
    ///   .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT a.`Id`,a.`ProductNo`,a.`Name`,a.`BrandId`,a.`CategoryId`,a.`Price`,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt`,b.`Id`,b.`BrandNo`,b.`Name` FROM `sys_product` a LEFT JOIN `sys_brand` b ON a.`BrandId`=b.`Id` WHERE a.`ProductNo` LIKE '%PN-00%'
    /// </code>
    /// </example>
    /// <example>
    /// 1:1关联关系导航属性
    /// <code>
    /// repository.From&lt;Product&gt;()
    ///   .Include(f =&gt; f.Brand)
    ///   .Where(f =&gt; f.ProductNo.Contains("PN-00"))
    ///   .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT a.`Id`,a.`ProductNo`,a.`Name`,a.`BrandId`,a.`CategoryId`,a.`Price`,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt`,b.`Id`,b.`BrandNo`,b.`Name` FROM `sys_product` a LEFT JOIN `sys_brand` b ON a.`BrandId`=b.`Id` WHERE a.`ProductNo` LIKE '%PN-00%'
    /// </code>
    /// </example>
    /// <typeparam name="TMember">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TMember>> memberSelector);
    /// <summary>
    /// 单表查询，包含1:N关联方式的导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系。
    /// 可以通过使用InnerJoin(Expression&lt;Func&lt;T, bool&gt;&gt; joinOn)、RightJoin(Expression&lt;Func&lt;T, bool&gt;&gt; joinOn)方法改变默认关联方式
    /// </summary>
    /// <typeparam name="TElment">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null);
    #endregion

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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn);

    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做INNER JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8&gt;()
    ///     ... ...
    ///     .InnerJoin((a, b, c, d, e, f, g, h) =&gt; f.From&lt;OrderDetail&gt;()
    ///         .GroupBy(x =&gt; x.OrderId)
    ///         .Select((x, y) =&gt; new
    ///         {
    ///             y.OrderId,
    ///             ProductCount = x.CountDistinct(y.ProductId)
    ///         }), (a, b, c, d, e, f, g, h) =&gt; ...)
    ///     ... ...
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... INNER JOIN (SELECT a.`OrderId`,COUNT(DISTINCT a.`ProductId`) AS ProductCount FROM `sys_order_detail` a GROUP BY a.`OrderId`) i ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> InnerJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做LEFT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8&gt;()
    ///     ... ...
    ///     .LeftJoin((a, b, c, d, e, f, g, h) =&gt; f.From&lt;OrderDetail&gt;()
    ///         .GroupBy(x =&gt; x.OrderId)
    ///         .Select((x, y) =&gt; new
    ///         {
    ///             y.OrderId,
    ///             ProductCount = x.CountDistinct(y.ProductId)
    ///         }), (a, b, c, d, e, f, g, h) =&gt; ...)
    ///     ... ...
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... INNER JOIN (SELECT a.`OrderId`,COUNT(DISTINCT a.`ProductId`) AS ProductCount FROM `sys_order_detail` a GROUP BY a.`OrderId`) i ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> LeftJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做RIGHT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8&gt;()
    ///     ... ...
    ///     .RightJoin((a, b, c, d, e, f, g, h) =&gt; f.From&lt;OrderDetail&gt;()
    ///         .GroupBy(x =&gt; x.OrderId)
    ///         .Select((x, y) =&gt; new
    ///         {
    ///             y.OrderId,
    ///             ProductCount = x.CountDistinct(y.ProductId)
    ///         }), (a, b, c, d, e, f, g, h) =&gt; ...)
    ///     ... ...
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... INNER JOIN (SELECT a.`OrderId`,COUNT(DISTINCT a.`ProductId`) AS ProductCount FROM `sys_order_detail` a GROUP BY a.`OrderId`) i ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, TOther> RightJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TOther, bool>> joinOn);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, bool>> elsePredicate = null);
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
    IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TGrouping>> groupingExpr);
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy((a, b) => new { a.Id, b.Id }) 或是 OrderBy(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b) => new { a.Id, b.Id }) 或是 OrderBy(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr);
    /// <summary>
    /// 使用表达式fieldsExpr，生成DSC排序语句，fieldsExpr可以是一或多个字段，用法：
    /// OrderByDescending((a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，用法：
    /// Select((a, b, c, d, e, f, g, h) => new { f.Id, f.Name }) 或是 Select((a, b, c, d, e, f, g, h) => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定聚合字段返回实体，单个或多个聚合字段的匿名对象，用法：
    /// <code>
    /// repository.From&lt;Order&gt;()
    ///    ... ...
    ///    .SelectAggregate((x, a, b, c, d, e, f, g, h) =&gt; new
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
    IMultiQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, TTarget>> fieldsExpr);
    #endregion

    #region Aggregate
    #region Count
    /// <summary>
    /// 返回某个字段的int类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().Count(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().CountDistinct(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的long类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().LongCount(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().LongCountDistinct(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr);
    #endregion

    /// <summary>
    /// 计算指定字段的求和值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的平均值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最大值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最小值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, TField>> fieldExpr);
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
public interface IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> : IMultiQueryBase
{
    #region CTE NextWith/NextWithRecursive
    /// <summary>
    /// 继续定义CTE With子句，在Select查询之前，可以定义一个或多个CTE子句，多个CTE With子句要连续定义，用法：
    /// <code>
    /// repository
    ///     .FromWith(f =&gt; ..., "Cte1")
    ///     ... ...
    ///     .NextWith(f =&gt; ..., "CteN")
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget> NextWith<TTarget>(Func<IFromQuery, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    /// <summary>
    /// 继续定义可递归的CTE With子句，可以引用CTE自身。在Select查询之前，可以定义一个或多个CTE子句，多个CTE With子句要连续定义，首个CTE With子句必须也是可递归的，才能使用本方法，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId })), "MenuList")
    ///     ... ...
    ///     .NextWithRecursive((f, cte) =&gt; f.From&lt;Page, Menu&gt;()
    ///             .Where((a, b) =&gt; a.Id == b.PageId)
    ///             .Select((x, y) =&gt; new { y.Id, y.ParentId, x.Url })
    ///         .UnionAll(x =&gt; x.From&lt;Menu&gt;()
    ///             .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///             .Where((a, b) =&gt; a.Id &gt; 1)
    ///             .Select((x, y) =&gt; new { x.Id, x.ParentId, y.Url })), "MenuPageList")
    ///     ... ...
    ///     .Select((a, b, c, d, e, f, g, h, i) =&gt; new { ... ... })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId) AS 
    /// (
    ///     SELECT `Id`,`Name`,`ParentId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    ///     SELECT a.`Id`, a.`Name`, a.`ParentId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// ),
    /// ... ...
    /// MenuPageList(Id,ParentId,Url) AS
    /// (
    ///     SELECT a.`Id`,a.`ParentId`,b.`Url` FROM `sys_menu` a,`sys_page` b WHERE a.`PageId`=b.Id UNION ALL
    ///     SELECT a.`Id`,a.`ParentId`,b.`Url` FROM `sys_menu` a INNER JOIN MenuList b ON a.`Id`=b.`ParentId`
    /// ),
    /// ... ...
    /// SELECT ... FROM MenuList a INNER JOIN ... INNER JOIN MenuPageList f ON ... .... WHERE ... ...
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
    /// <param name="tableAsStart">CTE子句中使用的表别名开始字母，默认从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget> NextWithRecursive<TTarget>(Func<IFromQuery, string, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    #endregion

    #region WithTable
    /// <summary>
    /// 使用子查询作为临时表，方便后面做关联查询，用法：
    /// <code>
    /// repository
    ///     ... ...
    ///     .WithTable(f =&gt; f.From&lt;Page, Menu&gt;()
    ///         .Where((a, b) =&gt; a.Id == b.PageId)
    ///         .Select((x, y) =&gt; new { y.Id, y.ParentId, x.Url }))
    ///     .InnerJoin((a, b, c, d, e, f, g, h, i) =&gt; ...)
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... (SELECT b.`Id`,b.`ParentId`,a.`Url` FROM `sys_page` a,`sys_menu` b WHERE c.`Id`=d.`PageId`) j INNER JOIN ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    #endregion

    #region Include
    /// <summary>
    /// 贪婪加载导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系生成JOIN ON子句。
    /// 可以通过使用InnerJoin(Expression&lt;Func&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, bool&gt;&gt; joinOn)、RightJoin(Expression&lt;Func&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, bool&gt;&gt; joinOn)方法改变默认关联方式。
    /// 1:1关联关系，随主表一起查询，1:N关联关系，分两次查询，第二次查询返回结果，再赋值到主实体的属性上。
    /// </summary>
    /// <example>
    /// 1:1关联关系导航属性
    /// <code>
    /// repository.From&lt;Product&gt;()
    ///   .Include(f =&gt; f.Brand)
    ///   .Where(f =&gt; f.ProductNo.Contains("PN-00"))
    ///   .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT a.`Id`,a.`ProductNo`,a.`Name`,a.`BrandId`,a.`CategoryId`,a.`Price`,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt`,b.`Id`,b.`BrandNo`,b.`Name` FROM `sys_product` a LEFT JOIN `sys_brand` b ON a.`BrandId`=b.`Id` WHERE a.`ProductNo` LIKE '%PN-00%'
    /// </code>
    /// </example>
    /// <example>
    /// 1:1关联关系导航属性
    /// <code>
    /// repository.From&lt;Product&gt;()
    ///   .Include(f =&gt; f.Brand)
    ///   .Where(f =&gt; f.ProductNo.Contains("PN-00"))
    ///   .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT a.`Id`,a.`ProductNo`,a.`Name`,a.`BrandId`,a.`CategoryId`,a.`Price`,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt`,b.`Id`,b.`BrandNo`,b.`Name` FROM `sys_product` a LEFT JOIN `sys_brand` b ON a.`BrandId`=b.`Id` WHERE a.`ProductNo` LIKE '%PN-00%'
    /// </code>
    /// </example>
    /// <typeparam name="TMember">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TMember>> memberSelector);
    /// <summary>
    /// 单表查询，包含1:N关联方式的导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系。
    /// 可以通过使用InnerJoin(Expression&lt;Func&lt;T, bool&gt;&gt; joinOn)、RightJoin(Expression&lt;Func&lt;T, bool&gt;&gt; joinOn)方法改变默认关联方式
    /// </summary>
    /// <typeparam name="TElment">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null);
    #endregion

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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn);

    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做INNER JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9&gt;()
    ///     ... ...
    ///     .InnerJoin((a, b, c, d, e, f, g, h, i) =&gt; f.From&lt;OrderDetail&gt;()
    ///         .GroupBy(x =&gt; x.OrderId)
    ///         .Select((x, y) =&gt; new
    ///         {
    ///             y.OrderId,
    ///             ProductCount = x.CountDistinct(y.ProductId)
    ///         }), (a, b, c, d, e, f, g, h, i) =&gt; ...)
    ///     ... ...
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... INNER JOIN (SELECT a.`OrderId`,COUNT(DISTINCT a.`ProductId`) AS ProductCount FROM `sys_order_detail` a GROUP BY a.`OrderId`) j ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> InnerJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做LEFT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9&gt;()
    ///     ... ...
    ///     .LeftJoin((a, b, c, d, e, f, g, h, i) =&gt; f.From&lt;OrderDetail&gt;()
    ///         .GroupBy(x =&gt; x.OrderId)
    ///         .Select((x, y) =&gt; new
    ///         {
    ///             y.OrderId,
    ///             ProductCount = x.CountDistinct(y.ProductId)
    ///         }), (a, b, c, d, e, f, g, h, i) =&gt; ...)
    ///     ... ...
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... INNER JOIN (SELECT a.`OrderId`,COUNT(DISTINCT a.`ProductId`) AS ProductCount FROM `sys_order_detail` a GROUP BY a.`OrderId`) j ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> LeftJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做RIGHT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9&gt;()
    ///     ... ...
    ///     .RightJoin((a, b, c, d, e, f, g, h, i) =&gt; f.From&lt;OrderDetail&gt;()
    ///         .GroupBy(x =&gt; x.OrderId)
    ///         .Select((x, y) =&gt; new
    ///         {
    ///             y.OrderId,
    ///             ProductCount = x.CountDistinct(y.ProductId)
    ///         }), (a, b, c, d, e, f, g, h, i) =&gt; ...)
    ///     ... ...
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... INNER JOIN (SELECT a.`OrderId`,COUNT(DISTINCT a.`ProductId`) AS ProductCount FROM `sys_order_detail` a GROUP BY a.`OrderId`) j ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther> RightJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TOther, bool>> joinOn);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, bool>> elsePredicate = null);
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
    IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TGrouping>> groupingExpr);
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy((a, b) => new { a.Id, b.Id }) 或是 OrderBy(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b) => new { a.Id, b.Id }) 或是 OrderBy(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr);
    /// <summary>
    /// 使用表达式fieldsExpr，生成DSC排序语句，fieldsExpr可以是一或多个字段，用法：
    /// OrderByDescending((a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，用法：
    /// Select((a, b, c, d, e, f, g, h, i) => new { f.Id, f.Name }) 或是 Select((a, b, c, d, e, f, g, h, i) => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定聚合字段返回实体，单个或多个聚合字段的匿名对象，用法：
    /// <code>
    /// repository.From&lt;Order&gt;()
    ///    ... ...
    ///    .SelectAggregate((x, a, b, c, d, e, f, g, h, i) =&gt; new
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
    IMultiQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, TTarget>> fieldsExpr);
    #endregion

    #region Aggregate
    #region Count
    /// <summary>
    /// 返回某个字段的int类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().Count(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().CountDistinct(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的long类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().LongCount(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().LongCountDistinct(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr);
    #endregion

    /// <summary>
    /// 计算指定字段的求和值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的平均值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最大值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最小值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TField>> fieldExpr);
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
public interface IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : IMultiQueryBase
{
    #region CTE NextWith/NextWithRecursive
    /// <summary>
    /// 继续定义CTE With子句，在Select查询之前，可以定义一个或多个CTE子句，多个CTE With子句要连续定义，用法：
    /// <code>
    /// repository
    ///     .FromWith(f =&gt; ..., "Cte1")
    ///     ... ...
    ///     .NextWith(f =&gt; ..., "CteN")
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget> NextWith<TTarget>(Func<IFromQuery, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    /// <summary>
    /// 继续定义可递归的CTE With子句，可以引用CTE自身。在Select查询之前，可以定义一个或多个CTE子句，多个CTE With子句要连续定义，首个CTE With子句必须也是可递归的，才能使用本方法，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId })), "MenuList")
    ///     ... ...
    ///     .NextWithRecursive((f, cte) =&gt; f.From&lt;Page, Menu&gt;()
    ///             .Where((a, b) =&gt; a.Id == b.PageId)
    ///             .Select((x, y) =&gt; new { y.Id, y.ParentId, x.Url })
    ///         .UnionAll(x =&gt; x.From&lt;Menu&gt;()
    ///             .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///             .Where((a, b) =&gt; a.Id &gt; 1)
    ///             .Select((x, y) =&gt; new { x.Id, x.ParentId, y.Url })), "MenuPageList")
    ///     ... ...
    ///     .Select((a, b, c, d, e, f, g, h, i, j) =&gt; new { ... ... })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId) AS 
    /// (
    ///     SELECT `Id`,`Name`,`ParentId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    ///     SELECT a.`Id`, a.`Name`, a.`ParentId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// ),
    /// ... ...
    /// MenuPageList(Id,ParentId,Url) AS
    /// (
    ///     SELECT a.`Id`,a.`ParentId`,b.`Url` FROM `sys_menu` a,`sys_page` b WHERE a.`PageId`=b.Id UNION ALL
    ///     SELECT a.`Id`,a.`ParentId`,b.`Url` FROM `sys_menu` a INNER JOIN MenuList b ON a.`Id`=b.`ParentId`
    /// ),
    /// ... ...
    /// SELECT ... FROM MenuList a INNER JOIN ... INNER JOIN MenuPageList f ON ... .... WHERE ... ...
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
    /// <param name="tableAsStart">CTE子句中使用的表别名开始字母，默认从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget> NextWithRecursive<TTarget>(Func<IFromQuery, string, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    #endregion

    #region WithTable
    /// <summary>
    /// 使用子查询作为临时表，方便后面做关联查询，用法：
    /// <code>
    /// repository
    ///     ... ...
    ///     .WithTable(f =&gt; f.From&lt;Page, Menu&gt;()
    ///         .Where((a, b) =&gt; a.Id == b.PageId)
    ///         .Select((x, y) =&gt; new { y.Id, y.ParentId, x.Url }))
    ///     .InnerJoin((a, b, c, d, e, f, g, h, i, j) =&gt; ...)
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... (SELECT b.`Id`,b.`ParentId`,a.`Url` FROM `sys_page` a,`sys_menu` b WHERE c.`Id`=d.`PageId`) k INNER JOIN ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    #endregion

    #region Include
    /// <summary>
    /// 贪婪加载导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系生成JOIN ON子句。
    /// 可以通过使用InnerJoin(Expression&lt;Func&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool&gt;&gt; joinOn)、RightJoin(Expression&lt;Func&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool&gt;&gt; joinOn)方法改变默认关联方式。
    /// 1:1关联关系，随主表一起查询，1:N关联关系，分两次查询，第二次查询返回结果，再赋值到主实体的属性上。
    /// </summary>
    /// <example>
    /// 1:1关联关系导航属性
    /// <code>
    /// repository.From&lt;Product&gt;()
    ///   .Include(f =&gt; f.Brand)
    ///   .Where(f =&gt; f.ProductNo.Contains("PN-00"))
    ///   .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT a.`Id`,a.`ProductNo`,a.`Name`,a.`BrandId`,a.`CategoryId`,a.`Price`,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt`,b.`Id`,b.`BrandNo`,b.`Name` FROM `sys_product` a LEFT JOIN `sys_brand` b ON a.`BrandId`=b.`Id` WHERE a.`ProductNo` LIKE '%PN-00%'
    /// </code>
    /// </example>
    /// <example>
    /// 1:1关联关系导航属性
    /// <code>
    /// repository.From&lt;Product&gt;()
    ///   .Include(f =&gt; f.Brand)
    ///   .Where(f =&gt; f.ProductNo.Contains("PN-00"))
    ///   .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT a.`Id`,a.`ProductNo`,a.`Name`,a.`BrandId`,a.`CategoryId`,a.`Price`,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt`,b.`Id`,b.`BrandNo`,b.`Name` FROM `sys_product` a LEFT JOIN `sys_brand` b ON a.`BrandId`=b.`Id` WHERE a.`ProductNo` LIKE '%PN-00%'
    /// </code>
    /// </example>
    /// <typeparam name="TMember">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TMember>> memberSelector);
    /// <summary>
    /// 单表查询，包含1:N关联方式的导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系。
    /// 可以通过使用InnerJoin(Expression&lt;Func&lt;T, bool&gt;&gt; joinOn)、RightJoin(Expression&lt;Func&lt;T, bool&gt;&gt; joinOn)方法改变默认关联方式
    /// </summary>
    /// <typeparam name="TElment">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null);
    #endregion

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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn);

    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做INNER JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10&gt;()
    ///     ... ...
    ///     .InnerJoin((a, b, c, d, e, f, g, h, i, j) =&gt; f.From&lt;OrderDetail&gt;()
    ///         .GroupBy(x =&gt; x.OrderId)
    ///         .Select((x, y) =&gt; new
    ///         {
    ///             y.OrderId,
    ///             ProductCount = x.CountDistinct(y.ProductId)
    ///         }), (a, b, c, d, e, f, g, h, i, j) =&gt; ...)
    ///     ... ...
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... INNER JOIN (SELECT a.`OrderId`,COUNT(DISTINCT a.`ProductId`) AS ProductCount FROM `sys_order_detail` a GROUP BY a.`OrderId`) k ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> InnerJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做LEFT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10&gt;()
    ///     ... ...
    ///     .LeftJoin((a, b, c, d, e, f, g, h, i, j) =&gt; f.From&lt;OrderDetail&gt;()
    ///         .GroupBy(x =&gt; x.OrderId)
    ///         .Select((x, y) =&gt; new
    ///         {
    ///             y.OrderId,
    ///             ProductCount = x.CountDistinct(y.ProductId)
    ///         }), (a, b, c, d, e, f, g, h, i, j) =&gt; ...)
    ///     ... ...
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... INNER JOIN (SELECT a.`OrderId`,COUNT(DISTINCT a.`ProductId`) AS ProductCount FROM `sys_order_detail` a GROUP BY a.`OrderId`) k ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> LeftJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做RIGHT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10&gt;()
    ///     ... ...
    ///     .RightJoin((a, b, c, d, e, f, g, h, i, j) =&gt; f.From&lt;OrderDetail&gt;()
    ///         .GroupBy(x =&gt; x.OrderId)
    ///         .Select((x, y) =&gt; new
    ///         {
    ///             y.OrderId,
    ///             ProductCount = x.CountDistinct(y.ProductId)
    ///         }), (a, b, c, d, e, f, g, h, i, j) =&gt; ...)
    ///     ... ...
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... INNER JOIN (SELECT a.`OrderId`,COUNT(DISTINCT a.`ProductId`) AS ProductCount FROM `sys_order_detail` a GROUP BY a.`OrderId`) k ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther> RightJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TOther, bool>> joinOn);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> elsePredicate = null);
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
    IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TGrouping>> groupingExpr);
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy((a, b) => new { a.Id, b.Id }) 或是 OrderBy(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b) => new { a.Id, b.Id }) 或是 OrderBy(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr);
    /// <summary>
    /// 使用表达式fieldsExpr，生成DSC排序语句，fieldsExpr可以是一或多个字段，用法：
    /// OrderByDescending((a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，用法：
    /// Select((a, b, c, d, e, f, g, h, i, j) => new { f.Id, f.Name }) 或是 Select((a, b, c, d, e, f, g, h, i, j) => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定聚合字段返回实体，单个或多个聚合字段的匿名对象，用法：
    /// <code>
    /// repository.From&lt;Order&gt;()
    ///    ... ...
    ///    .SelectAggregate((x, a, b, c, d, e, f, g, h, i, j) =&gt; new
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
    IMultiQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TTarget>> fieldsExpr);
    #endregion

    #region Aggregate
    #region Count
    /// <summary>
    /// 返回某个字段的int类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().Count(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().CountDistinct(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的long类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().LongCount(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().LongCountDistinct(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr);
    #endregion

    /// <summary>
    /// 计算指定字段的求和值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的平均值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最大值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最小值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TField>> fieldExpr);
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
public interface IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : IMultiQueryBase
{
    #region CTE NextWith/NextWithRecursive
    /// <summary>
    /// 继续定义CTE With子句，在Select查询之前，可以定义一个或多个CTE子句，多个CTE With子句要连续定义，用法：
    /// <code>
    /// repository
    ///     .FromWith(f =&gt; ..., "Cte1")
    ///     ... ...
    ///     .NextWith(f =&gt; ..., "CteN")
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget> NextWith<TTarget>(Func<IFromQuery, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    /// <summary>
    /// 继续定义可递归的CTE With子句，可以引用CTE自身。在Select查询之前，可以定义一个或多个CTE子句，多个CTE With子句要连续定义，首个CTE With子句必须也是可递归的，才能使用本方法，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId })), "MenuList")
    ///     ... ...
    ///     .NextWithRecursive((f, cte) =&gt; f.From&lt;Page, Menu&gt;()
    ///             .Where((a, b) =&gt; a.Id == b.PageId)
    ///             .Select((x, y) =&gt; new { y.Id, y.ParentId, x.Url })
    ///         .UnionAll(x =&gt; x.From&lt;Menu&gt;()
    ///             .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///             .Where((a, b) =&gt; a.Id &gt; 1)
    ///             .Select((x, y) =&gt; new { x.Id, x.ParentId, y.Url })), "MenuPageList")
    ///     ... ...
    ///     .Select((a, b, c, d, e, f, g, h, i, j, k) =&gt; new { ... ... })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId) AS 
    /// (
    ///     SELECT `Id`,`Name`,`ParentId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    ///     SELECT a.`Id`, a.`Name`, a.`ParentId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// ),
    /// ... ...
    /// MenuPageList(Id,ParentId,Url) AS
    /// (
    ///     SELECT a.`Id`,a.`ParentId`,b.`Url` FROM `sys_menu` a,`sys_page` b WHERE a.`PageId`=b.Id UNION ALL
    ///     SELECT a.`Id`,a.`ParentId`,b.`Url` FROM `sys_menu` a INNER JOIN MenuList b ON a.`Id`=b.`ParentId`
    /// ),
    /// ... ...
    /// SELECT ... FROM MenuList a INNER JOIN ... INNER JOIN MenuPageList f ON ... .... WHERE ... ...
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
    /// <param name="tableAsStart">CTE子句中使用的表别名开始字母，默认从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget> NextWithRecursive<TTarget>(Func<IFromQuery, string, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    #endregion

    #region WithTable
    /// <summary>
    /// 使用子查询作为临时表，方便后面做关联查询，用法：
    /// <code>
    /// repository
    ///     ... ...
    ///     .WithTable(f =&gt; f.From&lt;Page, Menu&gt;()
    ///         .Where((a, b) =&gt; a.Id == b.PageId)
    ///         .Select((x, y) =&gt; new { y.Id, y.ParentId, x.Url }))
    ///     .InnerJoin((a, b, c, d, e, f, g, h, i, j, k) =&gt; ...)
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... (SELECT b.`Id`,b.`ParentId`,a.`Url` FROM `sys_page` a,`sys_menu` b WHERE c.`Id`=d.`PageId`) l INNER JOIN ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    #endregion

    #region Include
    /// <summary>
    /// 贪婪加载导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系生成JOIN ON子句。
    /// 可以通过使用InnerJoin(Expression&lt;Func&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool&gt;&gt; joinOn)、RightJoin(Expression&lt;Func&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool&gt;&gt; joinOn)方法改变默认关联方式。
    /// 1:1关联关系，随主表一起查询，1:N关联关系，分两次查询，第二次查询返回结果，再赋值到主实体的属性上。
    /// </summary>
    /// <example>
    /// 1:1关联关系导航属性
    /// <code>
    /// repository.From&lt;Product&gt;()
    ///   .Include(f =&gt; f.Brand)
    ///   .Where(f =&gt; f.ProductNo.Contains("PN-00"))
    ///   .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT a.`Id`,a.`ProductNo`,a.`Name`,a.`BrandId`,a.`CategoryId`,a.`Price`,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt`,b.`Id`,b.`BrandNo`,b.`Name` FROM `sys_product` a LEFT JOIN `sys_brand` b ON a.`BrandId`=b.`Id` WHERE a.`ProductNo` LIKE '%PN-00%'
    /// </code>
    /// </example>
    /// <example>
    /// 1:1关联关系导航属性
    /// <code>
    /// repository.From&lt;Product&gt;()
    ///   .Include(f =&gt; f.Brand)
    ///   .Where(f =&gt; f.ProductNo.Contains("PN-00"))
    ///   .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT a.`Id`,a.`ProductNo`,a.`Name`,a.`BrandId`,a.`CategoryId`,a.`Price`,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt`,b.`Id`,b.`BrandNo`,b.`Name` FROM `sys_product` a LEFT JOIN `sys_brand` b ON a.`BrandId`=b.`Id` WHERE a.`ProductNo` LIKE '%PN-00%'
    /// </code>
    /// </example>
    /// <typeparam name="TMember">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TMember>> memberSelector);
    /// <summary>
    /// 单表查询，包含1:N关联方式的导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系。
    /// 可以通过使用InnerJoin(Expression&lt;Func&lt;T, bool&gt;&gt; joinOn)、RightJoin(Expression&lt;Func&lt;T, bool&gt;&gt; joinOn)方法改变默认关联方式
    /// </summary>
    /// <typeparam name="TElment">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null);
    #endregion

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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn);

    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做INNER JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11&gt;()
    ///     ... ...
    ///     .InnerJoin((a, b, c, d, e, f, g, h, i, j, k) =&gt; f.From&lt;OrderDetail&gt;()
    ///         .GroupBy(x =&gt; x.OrderId)
    ///         .Select((x, y) =&gt; new
    ///         {
    ///             y.OrderId,
    ///             ProductCount = x.CountDistinct(y.ProductId)
    ///         }), (a, b, c, d, e, f, g, h, i, j, k) =&gt; ...)
    ///     ... ...
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... INNER JOIN (SELECT a.`OrderId`,COUNT(DISTINCT a.`ProductId`) AS ProductCount FROM `sys_order_detail` a GROUP BY a.`OrderId`) l ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> InnerJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做LEFT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11&gt;()
    ///     ... ...
    ///     .LeftJoin((a, b, c, d, e, f, g, h, i, j, k) =&gt; f.From&lt;OrderDetail&gt;()
    ///         .GroupBy(x =&gt; x.OrderId)
    ///         .Select((x, y) =&gt; new
    ///         {
    ///             y.OrderId,
    ///             ProductCount = x.CountDistinct(y.ProductId)
    ///         }), (a, b, c, d, e, f, g, h, i, j, k) =&gt; ...)
    ///     ... ...
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... INNER JOIN (SELECT a.`OrderId`,COUNT(DISTINCT a.`ProductId`) AS ProductCount FROM `sys_order_detail` a GROUP BY a.`OrderId`) l ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> LeftJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做RIGHT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11&gt;()
    ///     ... ...
    ///     .RightJoin((a, b, c, d, e, f, g, h, i, j, k) =&gt; f.From&lt;OrderDetail&gt;()
    ///         .GroupBy(x =&gt; x.OrderId)
    ///         .Select((x, y) =&gt; new
    ///         {
    ///             y.OrderId,
    ///             ProductCount = x.CountDistinct(y.ProductId)
    ///         }), (a, b, c, d, e, f, g, h, i, j, k) =&gt; ...)
    ///     ... ...
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... INNER JOIN (SELECT a.`OrderId`,COUNT(DISTINCT a.`ProductId`) AS ProductCount FROM `sys_order_detail` a GROUP BY a.`OrderId`) l ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther> RightJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TOther, bool>> joinOn);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, bool>> elsePredicate = null);
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
    IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TGrouping>> groupingExpr);
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy((a, b) => new { a.Id, b.Id }) 或是 OrderBy(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b) => new { a.Id, b.Id }) 或是 OrderBy(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr);
    /// <summary>
    /// 使用表达式fieldsExpr，生成DSC排序语句，fieldsExpr可以是一或多个字段，用法：
    /// OrderByDescending((a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，用法：
    /// Select((a, b, c, d, e, f, g, h, i, j, k) => new { f.Id, f.Name }) 或是 Select((a, b, c, d, e, f, g, h, i, j, k) => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定聚合字段返回实体，单个或多个聚合字段的匿名对象，用法：
    /// <code>
    /// repository.From&lt;Order&gt;()
    ///    ... ...
    ///    .SelectAggregate((x, a, b, c, d, e, f, g, h, i, j, k) =&gt; new
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
    IMultiQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TTarget>> fieldsExpr);
    #endregion

    #region Aggregate
    #region Count
    /// <summary>
    /// 返回某个字段的int类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().Count(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().CountDistinct(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的long类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().LongCount(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().LongCountDistinct(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr);
    #endregion

    /// <summary>
    /// 计算指定字段的求和值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的平均值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最大值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最小值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TField>> fieldExpr);
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
public interface IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : IMultiQueryBase
{
    #region CTE NextWith/NextWithRecursive
    /// <summary>
    /// 继续定义CTE With子句，在Select查询之前，可以定义一个或多个CTE子句，多个CTE With子句要连续定义，用法：
    /// <code>
    /// repository
    ///     .FromWith(f =&gt; ..., "Cte1")
    ///     ... ...
    ///     .NextWith(f =&gt; ..., "CteN")
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget> NextWith<TTarget>(Func<IFromQuery, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    /// <summary>
    /// 继续定义可递归的CTE With子句，可以引用CTE自身。在Select查询之前，可以定义一个或多个CTE子句，多个CTE With子句要连续定义，首个CTE With子句必须也是可递归的，才能使用本方法，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId })), "MenuList")
    ///     ... ...
    ///     .NextWithRecursive((f, cte) =&gt; f.From&lt;Page, Menu&gt;()
    ///             .Where((a, b) =&gt; a.Id == b.PageId)
    ///             .Select((x, y) =&gt; new { y.Id, y.ParentId, x.Url })
    ///         .UnionAll(x =&gt; x.From&lt;Menu&gt;()
    ///             .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///             .Where((a, b) =&gt; a.Id &gt; 1)
    ///             .Select((x, y) =&gt; new { x.Id, x.ParentId, y.Url })), "MenuPageList")
    ///     ... ...
    ///     .Select((a, b, c, d, e, f, g, h, i, j, k, l) =&gt; new { ... ... })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId) AS 
    /// (
    ///     SELECT `Id`,`Name`,`ParentId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    ///     SELECT a.`Id`, a.`Name`, a.`ParentId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// ),
    /// ... ...
    /// MenuPageList(Id,ParentId,Url) AS
    /// (
    ///     SELECT a.`Id`,a.`ParentId`,b.`Url` FROM `sys_menu` a,`sys_page` b WHERE a.`PageId`=b.Id UNION ALL
    ///     SELECT a.`Id`,a.`ParentId`,b.`Url` FROM `sys_menu` a INNER JOIN MenuList b ON a.`Id`=b.`ParentId`
    /// ),
    /// ... ...
    /// SELECT ... FROM MenuList a INNER JOIN ... INNER JOIN MenuPageList f ON ... .... WHERE ... ...
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
    /// <param name="tableAsStart">CTE子句中使用的表别名开始字母，默认从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget> NextWithRecursive<TTarget>(Func<IFromQuery, string, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    #endregion

    #region WithTable
    /// <summary>
    /// 使用子查询作为临时表，方便后面做关联查询，用法：
    /// <code>
    /// repository
    ///     ... ...
    ///     .WithTable(f =&gt; f.From&lt;Page, Menu&gt;()
    ///         .Where((a, b) =&gt; a.Id == b.PageId)
    ///         .Select((x, y) =&gt; new { y.Id, y.ParentId, x.Url }))
    ///     .InnerJoin((a, b, c, d, e, f, g, h, i, j, k, l) =&gt; ...)
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... (SELECT b.`Id`,b.`ParentId`,a.`Url` FROM `sys_page` a,`sys_menu` b WHERE c.`Id`=d.`PageId`) m INNER JOIN ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    #endregion

    #region Include
    /// <summary>
    /// 贪婪加载导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系生成JOIN ON子句。
    /// 可以通过使用InnerJoin(Expression&lt;Func&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool&gt;&gt; joinOn)、RightJoin(Expression&lt;Func&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool&gt;&gt; joinOn)方法改变默认关联方式。
    /// 1:1关联关系，随主表一起查询，1:N关联关系，分两次查询，第二次查询返回结果，再赋值到主实体的属性上。
    /// </summary>
    /// <example>
    /// 1:1关联关系导航属性
    /// <code>
    /// repository.From&lt;Product&gt;()
    ///   .Include(f =&gt; f.Brand)
    ///   .Where(f =&gt; f.ProductNo.Contains("PN-00"))
    ///   .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT a.`Id`,a.`ProductNo`,a.`Name`,a.`BrandId`,a.`CategoryId`,a.`Price`,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt`,b.`Id`,b.`BrandNo`,b.`Name` FROM `sys_product` a LEFT JOIN `sys_brand` b ON a.`BrandId`=b.`Id` WHERE a.`ProductNo` LIKE '%PN-00%'
    /// </code>
    /// </example>
    /// <example>
    /// 1:1关联关系导航属性
    /// <code>
    /// repository.From&lt;Product&gt;()
    ///   .Include(f =&gt; f.Brand)
    ///   .Where(f =&gt; f.ProductNo.Contains("PN-00"))
    ///   .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT a.`Id`,a.`ProductNo`,a.`Name`,a.`BrandId`,a.`CategoryId`,a.`Price`,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt`,b.`Id`,b.`BrandNo`,b.`Name` FROM `sys_product` a LEFT JOIN `sys_brand` b ON a.`BrandId`=b.`Id` WHERE a.`ProductNo` LIKE '%PN-00%'
    /// </code>
    /// </example>
    /// <typeparam name="TMember">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TMember>> memberSelector);
    /// <summary>
    /// 单表查询，包含1:N关联方式的导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系。
    /// 可以通过使用InnerJoin(Expression&lt;Func&lt;T, bool&gt;&gt; joinOn)、RightJoin(Expression&lt;Func&lt;T, bool&gt;&gt; joinOn)方法改变默认关联方式
    /// </summary>
    /// <typeparam name="TElment">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null);
    #endregion

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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn);

    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做INNER JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12&gt;()
    ///     ... ...
    ///     .InnerJoin((a, b, c, d, e, f, g, h, i, j, k, l) =&gt; f.From&lt;OrderDetail&gt;()
    ///         .GroupBy(x =&gt; x.OrderId)
    ///         .Select((x, y) =&gt; new
    ///         {
    ///             y.OrderId,
    ///             ProductCount = x.CountDistinct(y.ProductId)
    ///         }), (a, b, c, d, e, f, g, h, i, j, k, l) =&gt; ...)
    ///     ... ...
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... INNER JOIN (SELECT a.`OrderId`,COUNT(DISTINCT a.`ProductId`) AS ProductCount FROM `sys_order_detail` a GROUP BY a.`OrderId`) m ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> InnerJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做LEFT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12&gt;()
    ///     ... ...
    ///     .LeftJoin((a, b, c, d, e, f, g, h, i, j, k, l) =&gt; f.From&lt;OrderDetail&gt;()
    ///         .GroupBy(x =&gt; x.OrderId)
    ///         .Select((x, y) =&gt; new
    ///         {
    ///             y.OrderId,
    ///             ProductCount = x.CountDistinct(y.ProductId)
    ///         }), (a, b, c, d, e, f, g, h, i, j, k, l) =&gt; ...)
    ///     ... ...
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... INNER JOIN (SELECT a.`OrderId`,COUNT(DISTINCT a.`ProductId`) AS ProductCount FROM `sys_order_detail` a GROUP BY a.`OrderId`) m ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> LeftJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做RIGHT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12&gt;()
    ///     ... ...
    ///     .RightJoin((a, b, c, d, e, f, g, h, i, j, k, l) =&gt; f.From&lt;OrderDetail&gt;()
    ///         .GroupBy(x =&gt; x.OrderId)
    ///         .Select((x, y) =&gt; new
    ///         {
    ///             y.OrderId,
    ///             ProductCount = x.CountDistinct(y.ProductId)
    ///         }), (a, b, c, d, e, f, g, h, i, j, k, l) =&gt; ...)
    ///     ... ...
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... INNER JOIN (SELECT a.`OrderId`,COUNT(DISTINCT a.`ProductId`) AS ProductCount FROM `sys_order_detail` a GROUP BY a.`OrderId`) m ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther> RightJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TOther, bool>> joinOn);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, bool>> elsePredicate = null);
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
    IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TGrouping>> groupingExpr);
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy((a, b) => new { a.Id, b.Id }) 或是 OrderBy(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b) => new { a.Id, b.Id }) 或是 OrderBy(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr);
    /// <summary>
    /// 使用表达式fieldsExpr，生成DSC排序语句，fieldsExpr可以是一或多个字段，用法：
    /// OrderByDescending((a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，用法：
    /// Select((a, b, c, d, e, f, g, h, i, j, k, l) => new { f.Id, f.Name }) 或是 Select((a, b, c, d, e, f, g, h, i, j, k, l) => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定聚合字段返回实体，单个或多个聚合字段的匿名对象，用法：
    /// <code>
    /// repository.From&lt;Order&gt;()
    ///    ... ...
    ///    .SelectAggregate((x, a, b, c, d, e, f, g, h, i, j, k, l) =&gt; new
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
    IMultiQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TTarget>> fieldsExpr);
    #endregion

    #region Aggregate
    #region Count
    /// <summary>
    /// 返回某个字段的int类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().Count(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().CountDistinct(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的long类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().LongCount(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().LongCountDistinct(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr);
    #endregion

    /// <summary>
    /// 计算指定字段的求和值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的平均值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最大值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最小值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TField>> fieldExpr);
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
public interface IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> : IMultiQueryBase
{
    #region CTE NextWith/NextWithRecursive
    /// <summary>
    /// 继续定义CTE With子句，在Select查询之前，可以定义一个或多个CTE子句，多个CTE With子句要连续定义，用法：
    /// <code>
    /// repository
    ///     .FromWith(f =&gt; ..., "Cte1")
    ///     ... ...
    ///     .NextWith(f =&gt; ..., "CteN")
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget> NextWith<TTarget>(Func<IFromQuery, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    /// <summary>
    /// 继续定义可递归的CTE With子句，可以引用CTE自身。在Select查询之前，可以定义一个或多个CTE子句，多个CTE With子句要连续定义，首个CTE With子句必须也是可递归的，才能使用本方法，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId })), "MenuList")
    ///     ... ...
    ///     .NextWithRecursive((f, cte) =&gt; f.From&lt;Page, Menu&gt;()
    ///             .Where((a, b) =&gt; a.Id == b.PageId)
    ///             .Select((x, y) =&gt; new { y.Id, y.ParentId, x.Url })
    ///         .UnionAll(x =&gt; x.From&lt;Menu&gt;()
    ///             .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///             .Where((a, b) =&gt; a.Id &gt; 1)
    ///             .Select((x, y) =&gt; new { x.Id, x.ParentId, y.Url })), "MenuPageList")
    ///     ... ...
    ///     .Select((a, b, c, d, e, f, g, h, i, j, k, l, m) =&gt; new { ... ... })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId) AS 
    /// (
    ///     SELECT `Id`,`Name`,`ParentId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    ///     SELECT a.`Id`, a.`Name`, a.`ParentId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// ),
    /// ... ...
    /// MenuPageList(Id,ParentId,Url) AS
    /// (
    ///     SELECT a.`Id`,a.`ParentId`,b.`Url` FROM `sys_menu` a,`sys_page` b WHERE a.`PageId`=b.Id UNION ALL
    ///     SELECT a.`Id`,a.`ParentId`,b.`Url` FROM `sys_menu` a INNER JOIN MenuList b ON a.`Id`=b.`ParentId`
    /// ),
    /// ... ...
    /// SELECT ... FROM MenuList a INNER JOIN ... INNER JOIN MenuPageList f ON ... .... WHERE ... ...
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
    /// <param name="tableAsStart">CTE子句中使用的表别名开始字母，默认从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget> NextWithRecursive<TTarget>(Func<IFromQuery, string, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    #endregion

    #region WithTable
    /// <summary>
    /// 使用子查询作为临时表，方便后面做关联查询，用法：
    /// <code>
    /// repository
    ///     ... ...
    ///     .WithTable(f =&gt; f.From&lt;Page, Menu&gt;()
    ///         .Where((a, b) =&gt; a.Id == b.PageId)
    ///         .Select((x, y) =&gt; new { y.Id, y.ParentId, x.Url }))
    ///     .InnerJoin((a, b, c, d, e, f, g, h, i, j, k, l, m) =&gt; ...)
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... (SELECT b.`Id`,b.`ParentId`,a.`Url` FROM `sys_page` a,`sys_menu` b WHERE c.`Id`=d.`PageId`) n INNER JOIN ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    #endregion

    #region Include
    /// <summary>
    /// 贪婪加载导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系生成JOIN ON子句。
    /// 可以通过使用InnerJoin(Expression&lt;Func&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool&gt;&gt; joinOn)、RightJoin(Expression&lt;Func&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool&gt;&gt; joinOn)方法改变默认关联方式。
    /// 1:1关联关系，随主表一起查询，1:N关联关系，分两次查询，第二次查询返回结果，再赋值到主实体的属性上。
    /// </summary>
    /// <example>
    /// 1:1关联关系导航属性
    /// <code>
    /// repository.From&lt;Product&gt;()
    ///   .Include(f =&gt; f.Brand)
    ///   .Where(f =&gt; f.ProductNo.Contains("PN-00"))
    ///   .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT a.`Id`,a.`ProductNo`,a.`Name`,a.`BrandId`,a.`CategoryId`,a.`Price`,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt`,b.`Id`,b.`BrandNo`,b.`Name` FROM `sys_product` a LEFT JOIN `sys_brand` b ON a.`BrandId`=b.`Id` WHERE a.`ProductNo` LIKE '%PN-00%'
    /// </code>
    /// </example>
    /// <example>
    /// 1:1关联关系导航属性
    /// <code>
    /// repository.From&lt;Product&gt;()
    ///   .Include(f =&gt; f.Brand)
    ///   .Where(f =&gt; f.ProductNo.Contains("PN-00"))
    ///   .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT a.`Id`,a.`ProductNo`,a.`Name`,a.`BrandId`,a.`CategoryId`,a.`Price`,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt`,b.`Id`,b.`BrandNo`,b.`Name` FROM `sys_product` a LEFT JOIN `sys_brand` b ON a.`BrandId`=b.`Id` WHERE a.`ProductNo` LIKE '%PN-00%'
    /// </code>
    /// </example>
    /// <typeparam name="TMember">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TMember>> memberSelector);
    /// <summary>
    /// 单表查询，包含1:N关联方式的导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系。
    /// 可以通过使用InnerJoin(Expression&lt;Func&lt;T, bool&gt;&gt; joinOn)、RightJoin(Expression&lt;Func&lt;T, bool&gt;&gt; joinOn)方法改变默认关联方式
    /// </summary>
    /// <typeparam name="TElment">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null);
    #endregion

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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn);

    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做INNER JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13&gt;()
    ///     ... ...
    ///     .InnerJoin((a, b, c, d, e, f, g, h, i, j, k, l, m) =&gt; f.From&lt;OrderDetail&gt;()
    ///         .GroupBy(x =&gt; x.OrderId)
    ///         .Select((x, y) =&gt; new
    ///         {
    ///             y.OrderId,
    ///             ProductCount = x.CountDistinct(y.ProductId)
    ///         }), (a, b, c, d, e, f, g, h, i, j, k, l, m) =&gt; ...)
    ///     ... ...
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... INNER JOIN (SELECT a.`OrderId`,COUNT(DISTINCT a.`ProductId`) AS ProductCount FROM `sys_order_detail` a GROUP BY a.`OrderId`) n ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> InnerJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做LEFT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13&gt;()
    ///     ... ...
    ///     .LeftJoin((a, b, c, d, e, f, g, h, i, j, k, l, m) =&gt; f.From&lt;OrderDetail&gt;()
    ///         .GroupBy(x =&gt; x.OrderId)
    ///         .Select((x, y) =&gt; new
    ///         {
    ///             y.OrderId,
    ///             ProductCount = x.CountDistinct(y.ProductId)
    ///         }), (a, b, c, d, e, f, g, h, i, j, k, l, m) =&gt; ...)
    ///     ... ...
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... INNER JOIN (SELECT a.`OrderId`,COUNT(DISTINCT a.`ProductId`) AS ProductCount FROM `sys_order_detail` a GROUP BY a.`OrderId`) n ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> LeftJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做RIGHT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13&gt;()
    ///     ... ...
    ///     .RightJoin((a, b, c, d, e, f, g, h, i, j, k, l, m) =&gt; f.From&lt;OrderDetail&gt;()
    ///         .GroupBy(x =&gt; x.OrderId)
    ///         .Select((x, y) =&gt; new
    ///         {
    ///             y.OrderId,
    ///             ProductCount = x.CountDistinct(y.ProductId)
    ///         }), (a, b, c, d, e, f, g, h, i, j, k, l, m) =&gt; ...)
    ///     ... ...
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... INNER JOIN (SELECT a.`OrderId`,COUNT(DISTINCT a.`ProductId`) AS ProductCount FROM `sys_order_detail` a GROUP BY a.`OrderId`) n ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther> RightJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TOther, bool>> joinOn);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, bool>> elsePredicate = null);
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
    IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TGrouping>> groupingExpr);
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy((a, b) => new { a.Id, b.Id }) 或是 OrderBy(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b) => new { a.Id, b.Id }) 或是 OrderBy(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr);
    /// <summary>
    /// 使用表达式fieldsExpr，生成DSC排序语句，fieldsExpr可以是一或多个字段，用法：
    /// OrderByDescending((a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，用法：
    /// Select((a, b, c, d, e, f, g, h, i, j, k, l, m) => new { f.Id, f.Name }) 或是 Select((a, b, c, d, e, f, g, h, i, j, k, l, m) => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定聚合字段返回实体，单个或多个聚合字段的匿名对象，用法：
    /// <code>
    /// repository.From&lt;Order&gt;()
    ///    ... ...
    ///    .SelectAggregate((x, a, b, c, d, e, f, g, h, i, j, k, l, m) =&gt; new
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
    IMultiQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TTarget>> fieldsExpr);
    #endregion

    #region Aggregate
    #region Count
    /// <summary>
    /// 返回某个字段的int类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().Count(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().CountDistinct(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的long类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().LongCount(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().LongCountDistinct(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr);
    #endregion

    /// <summary>
    /// 计算指定字段的求和值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的平均值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最大值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最小值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TField>> fieldExpr);
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
public interface IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> : IMultiQueryBase
{
    #region CTE NextWith/NextWithRecursive
    /// <summary>
    /// 继续定义CTE With子句，在Select查询之前，可以定义一个或多个CTE子句，多个CTE With子句要连续定义，用法：
    /// <code>
    /// repository
    ///     .FromWith(f =&gt; ..., "Cte1")
    ///     ... ...
    ///     .NextWith(f =&gt; ..., "CteN")
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget> NextWith<TTarget>(Func<IFromQuery, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    /// <summary>
    /// 继续定义可递归的CTE With子句，可以引用CTE自身。在Select查询之前，可以定义一个或多个CTE子句，多个CTE With子句要连续定义，首个CTE With子句必须也是可递归的，才能使用本方法，用法：
    /// <code>
    /// repository
    ///     .FromWithRecursive((f, cte) =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId })
    ///         .UnionAllRecursive((x, y) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoinRecursive(y, cte, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId })), "MenuList")
    ///     ... ...
    ///     .NextWithRecursive((f, cte) =&gt; f.From&lt;Page, Menu&gt;()
    ///             .Where((a, b) =&gt; a.Id == b.PageId)
    ///             .Select((x, y) =&gt; new { y.Id, y.ParentId, x.Url })
    ///         .UnionAll(x =&gt; x.From&lt;Menu&gt;()
    ///             .LeftJoin&lt;Page&gt;((a, b) =&gt; a.PageId == b.Id)
    ///             .Where((a, b) =&gt; a.Id &gt; 1)
    ///             .Select((x, y) =&gt; new { x.Id, x.ParentId, y.Url })), "MenuPageList")
    ///     ... ...
    ///     .Select((a, b, c, d, e, f, g, h, i, j, k, l, m, n) =&gt; new { ... ... })
    ///     .ToList();
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// WITH RECURSIVE MenuList(Id,Name,ParentId) AS 
    /// (
    ///     SELECT `Id`,`Name`,`ParentId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    ///     SELECT a.`Id`, a.`Name`, a.`ParentId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// ),
    /// ... ...
    /// MenuPageList(Id,ParentId,Url) AS
    /// (
    ///     SELECT a.`Id`,a.`ParentId`,b.`Url` FROM `sys_menu` a,`sys_page` b WHERE a.`PageId`=b.Id UNION ALL
    ///     SELECT a.`Id`,a.`ParentId`,b.`Url` FROM `sys_menu` a INNER JOIN MenuList b ON a.`Id`=b.`ParentId`
    /// ),
    /// ... ...
    /// SELECT ... FROM MenuList a INNER JOIN ... INNER JOIN MenuPageList f ON ... .... WHERE ... ...
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
    /// <param name="tableAsStart">CTE子句中使用的表别名开始字母，默认从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget> NextWithRecursive<TTarget>(Func<IFromQuery, string, IFromQuery<TTarget>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
    #endregion

    #region WithTable
    /// <summary>
    /// 使用子查询作为临时表，方便后面做关联查询，用法：
    /// <code>
    /// repository
    ///     ... ...
    ///     .WithTable(f =&gt; f.From&lt;Page, Menu&gt;()
    ///         .Where((a, b) =&gt; a.Id == b.PageId)
    ///         .Select((x, y) =&gt; new { y.Id, y.ParentId, x.Url }))
    ///     .InnerJoin((a, b, c, d, e, f, g, h, i, j, k, l, m, n) =&gt; ...)
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... (SELECT b.`Id`,b.`ParentId`,a.`Url` FROM `sys_page` a,`sys_menu` b WHERE c.`Id`=d.`PageId`) o INNER JOIN ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> WithTable<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery);
    #endregion

    #region Include
    /// <summary>
    /// 贪婪加载导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系生成JOIN ON子句。
    /// 可以通过使用InnerJoin(Expression&lt;Func&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool&gt;&gt; joinOn)、RightJoin(Expression&lt;Func&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool&gt;&gt; joinOn)方法改变默认关联方式。
    /// 1:1关联关系，随主表一起查询，1:N关联关系，分两次查询，第二次查询返回结果，再赋值到主实体的属性上。
    /// </summary>
    /// <example>
    /// 1:1关联关系导航属性
    /// <code>
    /// repository.From&lt;Product&gt;()
    ///   .Include(f =&gt; f.Brand)
    ///   .Where(f =&gt; f.ProductNo.Contains("PN-00"))
    ///   .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT a.`Id`,a.`ProductNo`,a.`Name`,a.`BrandId`,a.`CategoryId`,a.`Price`,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt`,b.`Id`,b.`BrandNo`,b.`Name` FROM `sys_product` a LEFT JOIN `sys_brand` b ON a.`BrandId`=b.`Id` WHERE a.`ProductNo` LIKE '%PN-00%'
    /// </code>
    /// </example>
    /// <example>
    /// 1:1关联关系导航属性
    /// <code>
    /// repository.From&lt;Product&gt;()
    ///   .Include(f =&gt; f.Brand)
    ///   .Where(f =&gt; f.ProductNo.Contains("PN-00"))
    ///   .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT a.`Id`,a.`ProductNo`,a.`Name`,a.`BrandId`,a.`CategoryId`,a.`Price`,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt`,b.`Id`,b.`BrandNo`,b.`Name` FROM `sys_product` a LEFT JOIN `sys_brand` b ON a.`BrandId`=b.`Id` WHERE a.`ProductNo` LIKE '%PN-00%'
    /// </code>
    /// </example>
    /// <typeparam name="TMember">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TMember>> memberSelector);
    /// <summary>
    /// 单表查询，包含1:N关联方式的导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系。
    /// 可以通过使用InnerJoin(Expression&lt;Func&lt;T, bool&gt;&gt; joinOn)、RightJoin(Expression&lt;Func&lt;T, bool&gt;&gt; joinOn)方法改变默认关联方式
    /// </summary>
    /// <typeparam name="TElment">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null);
    #endregion

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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> InnerJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> LeftJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn);
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
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> RightJoin<TOther>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn);

    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做INNER JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14&gt;()
    ///     ... ...
    ///     .InnerJoin((a, b, c, d, e, f, g, h, i, j, k, l, m, n) =&gt; f.From&lt;OrderDetail&gt;()
    ///         .GroupBy(x =&gt; x.OrderId)
    ///         .Select((x, y) =&gt; new
    ///         {
    ///             y.OrderId,
    ///             ProductCount = x.CountDistinct(y.ProductId)
    ///         }), (a, b, c, d, e, f, g, h, i, j, k, l, m, n) =&gt; ...)
    ///     ... ...
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... INNER JOIN (SELECT a.`OrderId`,COUNT(DISTINCT a.`ProductId`) AS ProductCount FROM `sys_order_detail` a GROUP BY a.`OrderId`) o ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> InnerJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做LEFT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14&gt;()
    ///     ... ...
    ///     .LeftJoin((a, b, c, d, e, f, g, h, i, j, k, l, m, n) =&gt; f.From&lt;OrderDetail&gt;()
    ///         .GroupBy(x =&gt; x.OrderId)
    ///         .Select((x, y) =&gt; new
    ///         {
    ///             y.OrderId,
    ///             ProductCount = x.CountDistinct(y.ProductId)
    ///         }), (a, b, c, d, e, f, g, h, i, j, k, l, m, n) =&gt; ...)
    ///     ... ...
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... INNER JOIN (SELECT a.`OrderId`,COUNT(DISTINCT a.`ProductId`) AS ProductCount FROM `sys_order_detail` a GROUP BY a.`OrderId`) o ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> LeftJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn);
    /// <summary>
    /// 添加subQuery子查询作为临时表，并与现有表做RIGHT JOIN关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14&gt;()
    ///     ... ...
    ///     .RightJoin((a, b, c, d, e, f, g, h, i, j, k, l, m, n) =&gt; f.From&lt;OrderDetail&gt;()
    ///         .GroupBy(x =&gt; x.OrderId)
    ///         .Select((x, y) =&gt; new
    ///         {
    ///             y.OrderId,
    ///             ProductCount = x.CountDistinct(y.ProductId)
    ///         }), (a, b, c, d, e, f, g, h, i, j, k, l, m, n) =&gt; ...)
    ///     ... ...
    /// </code>
    /// 生成的SQL：
    /// <code>
    /// ... INNER JOIN (SELECT a.`OrderId`,COUNT(DISTINCT a.`ProductId`) AS ProductCount FROM `sys_order_detail` a GROUP BY a.`OrderId`) o ON ...
    /// </code>
    /// </summary>
    /// <typeparam name="TOther">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询语句</param>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther> RightJoin<TOther>(Func<IFromQuery, IFromQuery<TOther>> subQuery, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TOther, bool>> joinOn);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, bool>> elsePredicate = null);
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
    IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TGrouping>> groupingExpr);
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy((a, b) => new { a.Id, b.Id }) 或是 OrderBy(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b) => new { a.Id, b.Id }) 或是 OrderBy(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr);
    /// <summary>
    /// 使用表达式fieldsExpr，生成DSC排序语句，fieldsExpr可以是一或多个字段，用法：
    /// OrderByDescending((a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，用法：
    /// Select((a, b, c, d, e, f, g, h, i, j, k, l, m, n) => new { f.Id, f.Name }) 或是 Select((a, b, c, d, e, f, g, h, i, j, k, l, m, n) => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget>> fieldsExpr);
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
    IMultiQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TTarget>> fieldsExpr);
    #endregion

    #region Aggregate
    #region Count
    /// <summary>
    /// 返回某个字段的int类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().Count(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().CountDistinct(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的long类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().LongCount(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().LongCountDistinct(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr);
    #endregion

    /// <summary>
    /// 计算指定字段的求和值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的平均值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最大值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最小值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TField>> fieldExpr);
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
public interface IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> : IMultiQueryBase
{
    #region Include
    /// <summary>
    /// 贪婪加载导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系生成JOIN ON子句。
    /// 可以通过使用InnerJoin(Expression&lt;Func&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool&gt;&gt; joinOn)、RightJoin(Expression&lt;Func&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool&gt;&gt; joinOn)方法改变默认关联方式。
    /// 1:1关联关系，随主表一起查询，1:N关联关系，分两次查询，第二次查询返回结果，再赋值到主实体的属性上。
    /// </summary>
    /// <example>
    /// 1:1关联关系导航属性
    /// <code>
    /// repository.From&lt;Product&gt;()
    ///   .Include(f =&gt; f.Brand)
    ///   .Where(f =&gt; f.ProductNo.Contains("PN-00"))
    ///   .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT a.`Id`,a.`ProductNo`,a.`Name`,a.`BrandId`,a.`CategoryId`,a.`Price`,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt`,b.`Id`,b.`BrandNo`,b.`Name` FROM `sys_product` a LEFT JOIN `sys_brand` b ON a.`BrandId`=b.`Id` WHERE a.`ProductNo` LIKE '%PN-00%'
    /// </code>
    /// </example>
    /// <example>
    /// 1:1关联关系导航属性
    /// <code>
    /// repository.From&lt;Product&gt;()
    ///   .Include(f =&gt; f.Brand)
    ///   .Where(f =&gt; f.ProductNo.Contains("PN-00"))
    ///   .ToSql(out _);
    /// </code>
    /// 生成的SQL:
    /// <code>
    /// SELECT a.`Id`,a.`ProductNo`,a.`Name`,a.`BrandId`,a.`CategoryId`,a.`Price`,a.`CompanyId`,a.`IsEnabled`,a.`CreatedBy`,a.`CreatedAt`,a.`UpdatedBy`,a.`UpdatedAt`,b.`Id`,b.`BrandNo`,b.`Name` FROM `sys_product` a LEFT JOIN `sys_brand` b ON a.`BrandId`=b.`Id` WHERE a.`ProductNo` LIKE '%PN-00%'
    /// </code>
    /// </example>
    /// <typeparam name="TMember">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember> Include<TMember>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TMember>> memberSelector);
    /// <summary>
    /// 单表查询，包含1:N关联方式的导航属性，默认使用LeftJoin方式，使用导航属性配置的关联关系。
    /// 可以通过使用InnerJoin(Expression&lt;Func&lt;T, bool&gt;&gt; joinOn)、RightJoin(Expression&lt;Func&lt;T, bool&gt;&gt; joinOn)方法改变默认关联方式
    /// </summary>
    /// <typeparam name="TElment">导航属性泛型类型</typeparam>
    /// <param name="memberSelector">导航属性选择表达式</param>
    /// <param name="filter">导航属性过滤条件，对1:N关联方式的集合属性有效</param>
    /// <returns>返回实体对象，带有导航属性</returns>
    IMultiIncludableQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TElment> IncludeMany<TElment>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, IEnumerable<TElment>>> memberSelector, Expression<Func<TElment, bool>> filter = null);
    #endregion

    #region Join
    /// <summary>
    /// 在现有表中，指定2个表进行INNER JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15&gt;()
    ///     ... ...
    ///     .InnerJoin((a, b, c, d, e, f, g, h, i, j, k, l, m, n, o) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> InnerJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinOn);
    /// <summary>
    /// 在现有表中，指定2个表进行LEFT JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15&gt;()
    ///     ... ...
    ///     .LeftJoin((a, b, c, d, e, f, g, h, i, j, k, l, m, n, o) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> LeftJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinOn);
    /// <summary>
    /// 在现有表中，指定2个表进行RIGHT JOIN关联，一次只能指定2个表，但可以多次使用本方法关联，用法:
    /// <code>
    /// repository.From&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15&gt;()
    ///     ... ...
    ///     .RightJoin((a, b, c, d, e, f, g, h, i, j, k, l, m, n, o) =&gt; ...)
    ///     ... ...
    /// </code>
    /// </summary>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> RightJoin(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> joinOn);
    #endregion

    #region Where/And
    /// <summary>
    /// 使用predicate表达式生成Where条件，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Where(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，否则使用表达式elsePredicate生成Where条件
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，不生成Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，则不生成Where条件</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> Where(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> ifPredicate, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> elsePredicate = null);
    /// <summary>
    /// 使用predicate表达式生成Where条件，并添加到已有的Where条件末尾，表达式predicate不能为null
    /// </summary>
    /// <param name="predicate">条件表达式，表达式predicate不能为null</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> And(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> predicate);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用表达式ifPredicate生成Where条件，并添加到已有的Where条件末尾，否则使用表达式elsePredicate生成Where条件，并添加到已有的Where条件末尾
    /// 表达式elsePredicate值可为nul，condition布尔值为false且表达式elsePredicate为null时，将不生成追加的Where条件
    /// </summary>
    /// <param name="condition">根据condition的值进行判断使用表达式</param>
    /// <param name="ifPredicate">condition为true时，使用的表达式，不可为null</param>
    /// <param name="elsePredicate">condition为false时，使用的表达式，值可为null，condition为false且elsePredicate为null时，将不生成追加的Where条件</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> And(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> ifPredicate = null, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, bool>> elsePredicate = null);
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
    IMultiGroupingQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping> GroupBy<TGrouping>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TGrouping>> groupingExpr);
    /// <summary>
    /// ASC排序，fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy((a, b) => new { a.Id, b.Id }) 或是 OrderBy(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderBy<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成ASC排序，否则不生成ASC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderBy(true, (a, b) => new { a.Id, b.Id }) 或是 OrderBy(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderBy<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr);
    /// <summary>
    /// 使用表达式fieldsExpr，生成DSC排序语句，fieldsExpr可以是一或多个字段，用法：
    /// OrderByDescending((a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderByDescending<TFields>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr);
    /// <summary>
    /// 判断condition布尔值，如果为true，生成DESC排序，否则不生成DESC排序。fieldsExpr可以是单个字段或多个字段的匿名对象，用法：
    /// OrderByDescending(true, (a, b) => new { a.Id, b.Id }) 或是 OrderByDescending(true, x => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TFields">表达式fieldsExpr的类型</typeparam>
    /// <param name="condition">排序表达式生效条件，为true生效</param>
    /// <param name="fieldsExpr">字段表达式，可以是单个字段或多个字段的匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> OrderByDescending<TFields>(bool condition, Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TFields>> fieldsExpr);
    #endregion

    #region Select
    /// <summary>
    /// 选择指定字段返回实体，一个字段或多个字段的匿名对象，用法：
    /// Select((a, b, c, d, e, f, g, h, i, j, k, l, m, n, o) => new { f.Id, f.Name }) 或是 Select((a, b, c, d, e, f, g, h, i, j, k, l, m, n, o) => x.CreatedAt.Date)
    /// </summary>
    /// <typeparam name="TTarget">返回实体的类型</typeparam>
    /// <param name="fieldsExpr">字段选择表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<TTarget> Select<TTarget>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TTarget>> fieldsExpr);
    /// <summary>
    /// 选择指定聚合字段返回实体，单个或多个聚合字段的匿名对象，用法：
    /// <code>
    /// repository.From&lt;Order&gt;()
    ///    ... ...
    ///    .SelectAggregate((x, a, b, c, d, e, f, g, h, i, j, k, l, m, n, o) =&gt; new
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
    IMultiQuery<TTarget> SelectAggregate<TTarget>(Expression<Func<IAggregateSelect, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TTarget>> fieldsExpr);
    #endregion

    #region Aggregate
    #region Count
    /// <summary>
    /// 返回某个字段的int类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().Count(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Count<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的int类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().CountDistinct(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery CountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段的long类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().LongCount(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery LongCount<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr);
    /// <summary>
    /// 返回某个字段去重后的long类型数据条数，用法：
    /// <code>repository.From&lt;Order&gt;().LongCountDistinct(f =&gt; f.BuyerId);</code>
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery LongCountDistinct<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr);
    #endregion

    /// <summary>
    /// 计算指定字段的求和值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Sum<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的平均值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Avg<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最大值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Max<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr);
    /// <summary>
    /// 计算指定字段的最小值
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldExpr">字段表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Min<TField>(Expression<Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TField>> fieldExpr);
    #endregion
}