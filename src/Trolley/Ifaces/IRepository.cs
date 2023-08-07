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
    /// <summary>
    /// 驱动提供者
    /// </summary>
    IOrmProvider OrmProvider { get; }
    /// <summary>
    /// 数据库连接
    /// </summary>
    IDbConnection Connection { get; }
    /// <summary>
    /// 事务对象
    /// </summary>
    IDbTransaction Transaction { get; }
    #endregion

    #region Query
    /// <summary>
    /// 从表T中查询数据，用法：
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
    /// 从SQL子查询中查询数据，用法：
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
    /// 使用2个表创建查询对象
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <param name="tableAsStart">表别名起始字母，默认值从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2> From<T1, T2>(char tableAsStart = 'a');
    /// <summary>
    /// 使用3个表创建查询对象
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <typeparam name="T3">表T3实体类型</typeparam>
    /// <param name="tableAsStart">表别名起始字母，默认值从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3> From<T1, T2, T3>(char tableAsStart = 'a');
    /// <summary>
    /// 使用4个表创建查询对象
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <typeparam name="T3">表T3实体类型</typeparam>
    /// <typeparam name="T4">表T4实体类型</typeparam>
    /// <param name="tableAsStart">表别名起始字母，默认值从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IQuery<T1, T2, T3, T4> From<T1, T2, T3, T4>(char tableAsStart = 'a');
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
    IQuery<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>(char tableAsStart = 'a');
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
    IQuery<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>(char tableAsStart = 'a');
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
    IQuery<T1, T2, T3, T4, T5, T6, T7> From<T1, T2, T3, T4, T5, T6, T7>(char tableAsStart = 'a');
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8> From<T1, T2, T3, T4, T5, T6, T7, T8>(char tableAsStart = 'a');
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T1, T2, T3, T4, T5, T6, T7, T8, T9>(char tableAsStart = 'a');
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
    IQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(char tableAsStart = 'a');
    /// <summary>
    /// 使用原始SQL和parameters参数，查询满足条件的首条记录，记录不存在时返回TEntity类型的默认值
    /// </summary>
    /// <typeparam name="TEntity">返回的实体类型</typeparam>
    /// <param name="rawSql">查询SQL</param>
    /// <param name="parameters">参数，可以是命名对象、匿名对象或是Dictionary类型对象，可以为null</param>
    /// <returns>返回查询结果，记录不存在时返回TEntity类型的默认值</returns>
    TEntity QueryFirst<TEntity>(string rawSql, object parameters = null);
    /// <summary>
    /// 使用原始SQL和parameters参数，查询满足条件的首条记录，记录不存在时返回TEntity类型的默认值
    /// </summary>
    /// <typeparam name="TEntity">返回的实体类型</typeparam>
    /// <param name="rawSql">查询SQL</param>
    /// <param name="parameters">参数，可以是命名对象、匿名对象或是Dictionary类型对象，可以为null</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回查询结果，记录不存在时返回TEntity类型的默认值</returns>
    Task<TEntity> QueryFirstAsync<TEntity>(string rawSql, object parameters = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// 查询与whereObj对象各属性值都相等的首条记录，记录不存在时返回TEntity类型的默认值,用法：
    /// <code>
    /// repository.Exists&lt;User&gt;(new { Id = 1 })
    /// SQL: SELECT COUNT(1) FROM `sys_user` WHERE `Id`=@Id
    /// </code>
    /// </summary>
    /// <typeparam name="TEntity">返回的实体类型</typeparam>
    /// <param name="whereObj">条件对象，对象的每个属性都参与比较，推荐使用匿名对象</param>
    /// <returns>返回查询结果，记录不存在时返回TEntity类型的默认值</returns>
    TEntity QueryFirst<TEntity>(object whereObj);
    /// <summary>
    /// 查询与whereObj对象各属性值都相等的首条记录，记录不存在时返回TEntity类型的默认值,用法：
    /// <code>
    /// repository.QueryFirstAsync&lt;User&gt;(new { Id = 1 })
    /// SQL: SELECT COUNT(1) FROM `sys_user` WHERE `Id`=@Id
    /// </code>
    /// </summary>
    /// <typeparam name="TEntity">返回的实体类型</typeparam>
    /// <param name="whereObj">条件对象，对象的每个属性都参与比较，推荐使用匿名对象</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回查询结果，记录不存在时返回TEntity类型的默认值</returns>
    Task<TEntity> QueryFirstAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default);
    /// <summary>
    /// 使用原始SQL语句rawSql和参数parameters查询多条数据，记录不存在时返回TEntity类型的默认值,用法：
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="rawSql"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    List<TEntity> Query<TEntity>(string rawSql, object parameters = null);
    Task<List<TEntity>> QueryAsync<TEntity>(string rawSql, object parameters = null, CancellationToken cancellationToken = default);
    List<TEntity> Query<TEntity>(object whereObj);
    /// <summary>
    /// 查询与whereObj对象各属性值都相等的首条记录，记录不存在时返回TEntity类型的默认值,用法：
    /// <code>
    /// repository.Exists&lt;User&gt;(new { Id = 1 })
    /// SQL: SELECT COUNT(1) FROM `sys_user` WHERE `Id`=@Id
    /// </code>
    /// </summary>
    /// <typeparam name="TEntity">返回的实体类型</typeparam>
    /// <param name="whereObj">条件对象，对象的每个属性都参与比较，推荐使用匿名对象</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns></returns>
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
    /// <summary>
    /// 判断是否存在表TEntity中满足与whereObj对象各属性值都相等的记录，存在返回true，否则返回false。
    /// <code>
    /// repository.Exists&lt;User&gt;(new { Id = 1 })
    /// SQL: SELECT COUNT(1) FROM `sys_user` WHERE `Id`=@Id
    /// </code>
    /// </summary>
    /// <typeparam name="TEntity">实体对象类型</typeparam>
    /// <param name="whereObj">where条件对象，whereObj对象各属性值都参与相等比较,推荐使用匿名对象</param>
    /// <returns>返回是否存在的布尔值</returns>
    bool Exists<TEntity>(object whereObj);
    /// <summary>
    /// 判断是否存在表TEntity中满足与whereObj对象各属性值都相等的记录，存在返回true，否则返回false。
    /// <code>
    /// await repository.ExistsAsync&lt;User&gt;(new { Id = 1 })
    /// SQL: SELECT COUNT(1) FROM `sys_user` WHERE `Id`=@Id
    /// </code>
    /// </summary>
    /// <typeparam name="TEntity">实体对象类型</typeparam>
    /// <param name="whereObj">where条件对象，whereObj对象各属性值都参与相等比较,推荐使用匿名对象</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回是否存在的布尔值</returns>
    Task<bool> ExistsAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default);
    /// <summary>
    /// 判断TEntity表是否存在满足wherePredicate条件的记录，存在返回true，否则返回false。
    /// </summary>
    /// <typeparam name="TEntity">实体对象类型</typeparam>
    /// <param name="wherePredicate">where条件表达式</param>
    /// <param name="result">返回是否存在的布尔值</param>
    /// <returns>返回是否存在的布尔值</returns>
    bool Exists<TEntity>(Expression<Func<TEntity, bool>> wherePredicate);
    /// <summary>
    /// 判断TEntity表是否存在满足wherePredicate条件的记录，存在返回true，否则返回false。
    /// </summary>
    /// <typeparam name="TEntity">实体对象类型</typeparam>
    /// <param name="wherePredicate">where条件表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回是否存在的布尔值</returns>
    Task<bool> ExistsAsync<TEntity>(Expression<Func<TEntity, bool>> wherePredicate, CancellationToken cancellationToken = default);
    #endregion

    #region Execute
    int Execute(string sql, object parameters = null);
    Task<int> ExecuteAsync(string sql, object parameters = null, CancellationToken cancellationToken = default);
    #endregion

    #region QueryMultiple
    //IMultiQueryReader QueryMultiple(string rawSql, object parameters = null);
    //Task<IMultiQueryReader> QueryMultipleAsync(string rawSql, object parameters = null, CancellationToken cancellationToken = default);
    //IMultiQueryReader QueryMultiple(Action<IMultipleQuery> subQueries);
    //Task<IMultiQueryReader> QueryMultipleAsync(Action<IMultipleQuery> subQueries, CancellationToken cancellationToken = default);
    #endregion

    #region Others
    /// <summary>
    /// 关闭连接
    /// </summary>
    void Close();
    /// <summary>
    /// 异步关闭连接
    /// </summary>
    /// <returns>返回异步Task对象</returns>
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
