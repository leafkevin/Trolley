using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

/// <summary>
/// 仓储对象
/// </summary>
public interface IRepository : IUnitOfWork, IDisposable, IAsyncDisposable
{
    #region 属性
    IOrmProvider OrmProvider { get; }
    IDbConnection Connection { get; }
    IDbTransaction Transaction { get; }
    #endregion

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
    IQuery<T> From<T>(char tableAsStart = 'a', string suffixRawSql = null);
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
    IQuery<T> From<T>(Func<IFromQuery, IFromQuery<T>> subQuery, char tableAsStart = 'a');
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
    IQuery<T> FromWith<T>(Func<IFromQuery, IFromQuery<T>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');
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
    IQuery<T> FromWithRecursive<T>(Func<IFromQuery, string, IFromQuery<T>> cteSubQuery, string cteTableName = "cte", char tableAsStart = 'a');

    /// <summary>
    /// 使用两个表创建查询对象
    /// </summary>
    /// <typeparam name="T1">实体类型1</typeparam>
    /// <typeparam name="T2">实体类型2</typeparam>
    /// <param name="tableAsStart">表别名起始字母，默认值从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2> From<T1, T2>(char tableAsStart = 'a');
    /// <summary>
    /// 使用三个表创建查询对象
    /// </summary>
    /// <typeparam name="T1">实体类型1</typeparam>
    /// <typeparam name="T2">实体类型2</typeparam>
    /// <typeparam name="T3">实体类型3</typeparam>
    /// <param name="tableAsStart">表别名起始字母，默认值从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3> From<T1, T2, T3>(char tableAsStart = 'a');
    /// <summary>
    /// 使用四个表创建查询对象
    /// </summary>
    /// <typeparam name="T1">实体类型1</typeparam>
    /// <typeparam name="T2">实体类型2</typeparam>
    /// <typeparam name="T3">实体类型3</typeparam>
    /// <typeparam name="T4">实体类型4</typeparam>
    /// <param name="tableAsStart">表别名起始字母，默认值从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4> From<T1, T2, T3, T4>(char tableAsStart = 'a');
    IQuery<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>(char tableAsStart = 'a');
    IQuery<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>(char tableAsStart = 'a');
    IQuery<T1, T2, T3, T4, T5, T6, T7> From<T1, T2, T3, T4, T5, T6, T7>(char tableAsStart = 'a');
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8> From<T1, T2, T3, T4, T5, T6, T7, T8>(char tableAsStart = 'a');
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T1, T2, T3, T4, T5, T6, T7, T8, T9>(char tableAsStart = 'a');
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(char tableAsStart = 'a');
    TEntity QueryFirst<TEntity>(string rawSql, object parameters = null);
    Task<TEntity> QueryFirstAsync<TEntity>(string rawSql, object parameters = null, CancellationToken cancellationToken = default);
    TEntity QueryFirst<TEntity>(object whereObj);
    Task<TEntity> QueryFirstAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default);
    List<TEntity> Query<TEntity>(string rawSql, object parameters = null);
    Task<List<TEntity>> QueryAsync<TEntity>(string rawSql, object parameters = null, CancellationToken cancellationToken = default);
    List<TEntity> Query<TEntity>(object whereObj);
    Task<List<TEntity>> QueryAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default);
    #endregion

    #region Get
    TEntity Get<TEntity>(object whereObj);
    Task<TEntity> GetAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default);
    #endregion

    #region Create
    /// <summary>
    /// 创建T类型插入对象
    /// </summary>
    /// <typeparam name="T">插入对象类型</typeparam>
    /// <returns>返回插入对象</returns>
    ICreate<T> Create<T>();
    #endregion

    #region Update  
    IUpdate<T> Update<T>();
    #endregion

    #region Delete
    IDelete<T> Delete<T>();
    #endregion

    #region Exists
    bool Exists<TEntity>(object whereObj);
    Task<bool> ExistsAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default);
    bool Exists<TEntity>(Expression<Func<TEntity, bool>> wherePredicate);
    Task<bool> ExistsAsync<TEntity>(Expression<Func<TEntity, bool>> wherePredicate, CancellationToken cancellationToken = default);
    #endregion

    #region Execute
    int Execute(string sql, object parameters = null);
    Task<int> ExecuteAsync(string sql, object parameters = null, CancellationToken cancellationToken = default);
    #endregion

    #region Others
    void Close();
    Task CloseAsync();
    /// <summary>
    /// 设置命令超时时间，本次命令执行有效，单位是秒
    /// </summary>
    /// <param name="timeout">超时时间，单位是秒</param>
    /// <returns>返回仓储对象</returns>
    IRepository Timeout(int timeout);
    /// <summary>
    /// 是否使用参数化，如果设置为true，本IRepository对象的所有查询语句中用到的变量都将变成参数
    /// Create、Update、Delete操作本身就是参数化的，主要是Lambda表达式中用到的变量，如：
    /// string productNo="xxx";//变量，会使用参数化
    /// using var repository = dbFactory.Create().WithParameterized();
    /// var result1 = await repository.QueryAsync&lt;Product&gt;(f =&gt; f.ProductNo.Contains(productNo));
    /// var result2 = await repository.QueryAsync&lt;Product&gt;(f =&gt; f.ProductNo.Contains("PN-001"));//常量，不会使用参数化
    /// </summary>
    /// <param name="isParameterized">是否参数化</param>
    /// <returns>返回仓储对象</returns>
    IRepository WithParameterized(bool isParameterized = true);
    #endregion
}
