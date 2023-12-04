﻿using System;
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
    #region Properties
    /// <summary>
    /// 数据库链接串Key
    /// </summary>
    string DbKey { get; }
    /// <summary>
    /// 驱动提供者
    /// </summary>
    IOrmProvider OrmProvider { get; }
    /// <summary>
    /// 实体映射提供者
    /// </summary>
    IEntityMapProvider MapProvider { get; }
    /// <summary>
    /// 数据库连接
    /// </summary>
    IDbConnection Connection { get; }
    /// <summary>
    /// 事务对象
    /// </summary>
    IDbTransaction Transaction { get; }
    #endregion

    #region From
    /// <summary>
    /// 从表T中查询数据，用法：
    /// <code>
    /// repository.From&lt;Menu&gt;()
    /// SQL:FROM `sys_menu`
    /// </code>
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <param name="tableAsStart">表别名起始字母，默认值从字母a开始</param>
    /// <param name="suffixRawSql">额外的原始SQL, SqlServer会有With用法，如：<cdoe>SELECT * FROM sys_user WITH(NOLOCK)</cdoe>
    /// </param>
    /// <returns>返回查询对象</returns>
    IQuery<T> From<T>(char tableAsStart = 'a', string suffixRawSql = null);
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
    #endregion

    #region From SubQuery
    /// <summary>
    /// 从SQL子查询中查询数据，用法：
    /// <code>
    /// var subQuery = repository.From&lt;Page, Menu&gt;('o')
    ///     .Where((a, b) =&gt; a.Id == b.PageId)
    ///     .Select((x, y) =&gt; new { y.Id, y.ParentId, x.Url });
    /// repository.From(subQuery) ...
    /// SQL:
    /// ... FROM (SELECT p.`Id`,p.`ParentId`,o.`Url` FROM `sys_page` o,`sys_menu` p WHERE o.`Id`=p.`PageId`) ...
    /// </code>
    /// </summary>
    /// <typeparam name="T">表T实体类型</typeparam>
    /// <param name="subQuery">子查询</param>
    /// <param name="tableAsStart">表别名起始字母，默认值从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IQuery<T> From<T>(IQuery<T> subQuery, char tableAsStart = 'a');
    #endregion

    #region FromWith CTE
    /// <summary>
    /// 使用CTE子句创建查询对象，不能自我引用不能递归查询，用法：
    /// <code>
    /// var subQuery = repository.From&lt;Menu&gt;()
    ///     .Select(x =&gt; new { x.Id, x.Name, x.ParentId, x.PageId });
    /// repository.FromWith(subQuery, "MenuList") ...
    /// SQL:
    /// WITH MenuList(Id,Name,ParentId,PageId) AS 
    /// (
    ///     SELECT `Id`,`Name`,`ParentId`,`PageId` FROM `sys_menu`
    /// )
    /// ...
    /// </code>
    /// </summary>
    /// <typeparam name="T">CTE With子句的临时实体类型，通常是一个匿名的</typeparam>
    /// <param name="cteSubQuery">CTE 查询子句</param>
    /// <param name="cteTableName">CTE表自身引用的表名称，如果在UnionRecursive/UnionAllRecursive方法中设置过了，此处无需设置</param>
    /// <param name="tableAsStart">表别名起始字母，默认值从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IQuery<T> FromWith<T>(IQuery<T> cteSubQuery, string cteTableName = null, char tableAsStart = 'a');
    /// <summary>
    /// 使用CTE子句创建查询对象，包含UnionRecursive或UnionAllRecursive子句可以自我引用递归查询，用法：
    /// <code>
    /// repository
    ///     .FromWith(f =&gt; f.From&lt;Menu&gt;()
    ///             .Where(x =&gt; x.Id == 1)
    ///             .Select(x =&gt; new { x.Id, x.Name, x.ParentId })
    ///         .UnionAllRecursive((x, self) =&gt; x.From&lt;Menu&gt;()
    ///             .InnerJoin(self, (a, b) =&gt; a.ParentId == b.Id)
    ///             .Select((a, b) =&gt; new { a.Id, a.Name, a.ParentId })), "MenuList") ...
    /// SQL:
    /// WITH RECURSIVE MenuList(Id,Name,ParentId) AS
    /// (
    ///     SELECT `Id`,`Name`,`ParentId` FROM `sys_menu` WHERE `Id`=1 UNION ALL
    ///     SELECT a.`Id`,a.`Name`,a.`ParentId` FROM `sys_menu` a INNER JOIN MenuList b ON a.`ParentId`=b.`Id`
    /// ) ...
    /// </code>
    /// </summary>
    /// <typeparam name="T">CTE With子句的临时实体类型，通常是一个匿名的</typeparam>
    /// <param name="cteSubQuery">CTE 查询子句</param>
    /// <param name="cteTableName">CTE表自身引用的表名称，如果在UnionRecursive/UnionAllRecursive方法中设置过了，此处无需设置</param>
    /// <param name="tableAsStart">表别名起始字母，默认值从字母a开始</param>
    /// <returns>返回查询对象</returns>
    //IQuery<T> FromWith<T>(Func<IFromQuery, IQuery<T>> cteSubQuery, string cteTableName = null, char tableAsStart = 'a');
    #endregion

    #region QueryFirst/Query
    /// <summary>
    /// 使用原始SQL语句rawSql和参数parameters查询数据，并返回满足条件的第一条记录，记录不存在时返回TEntity类型的默认值
    /// </summary>
    /// <typeparam name="TEntity">实体TEntity类型</typeparam>
    /// <param name="rawSql">原始SQL</param>
    /// <param name="parameters">参数，可以是命名对象、匿名对象或是Dictionary类型对象，parameters可以为null</param>
    /// <returns>返回查询结果，记录不存在时返回TEntity类型的默认值</returns>
    TEntity QueryFirst<TEntity>(string rawSql, object parameters = null);
    /// <summary>
    /// 使用原始SQL语句rawSql和参数parameters查询数据，并返回满足条件的第一条记录，记录不存在时返回TEntity类型的默认值
    /// </summary>
    /// <typeparam name="TEntity">返回的实体类型</typeparam>
    /// <param name="rawSql">查询SQL</param>
    /// <param name="parameters">参数，可以是命名对象、匿名对象或是Dictionary类型对象，可以为null</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回查询结果，记录不存在时返回TEntity类型的默认值</returns>
    Task<TEntity> QueryFirstAsync<TEntity>(string rawSql, object parameters = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// 从表TEntity中，查询与whereObj对象各属性值都相等的第一条记录，记录不存在时返回TEntity类型的默认值，用法：
    /// <code>
    /// repository.QueryFirst&lt;User&gt;(new { Id = 1, IsEnabled = true })
    /// SQL: SELECT `Id`,`Name`,`Gender`,`Age`,`CompanyId`,`GuidField`,`SomeTimes`,`SourceType`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt` FROM `sys_user` WHERE `Id`=@Id AND `IsEnabled`=@IsEnabled
    /// </code>
    /// </summary>
    /// <typeparam name="TEntity">实体TEntity类型</typeparam>
    /// <param name="whereObj">参数，可以是命名对象、匿名对象或是Dictionary类型对象，不能为null</param>
    /// <returns>返回查询结果，记录不存在时返回TEntity类型的默认值</returns>
    TEntity QueryFirst<TEntity>(object whereObj);
    /// <summary>
    /// 从表TEntity中，查询与whereObj对象各属性值都相等的第一条记录，记录不存在时返回TEntity类型的默认值，用法：
    /// <code>
    /// await repository.QueryFirstAsync&lt;User&gt;(new { Id = 1, IsEnabled = true })
    /// SQL: SELECT `Id`,`Name`,`Gender`,`Age`,`CompanyId`,`GuidField`,`SomeTimes`,`SourceType`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt` FROM `sys_user` WHERE `Id`=@Id AND `IsEnabled`=@IsEnabled
    /// </code>
    /// </summary>
    /// <typeparam name="TEntity">实体TEntity类型</typeparam>
    /// <param name="whereObj">参数，可以是命名对象、匿名对象或是Dictionary类型对象，不能为null</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回查询结果，记录不存在时返回TEntity类型的默认值</returns>
    Task<TEntity> QueryFirstAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default);
    /// <summary>
    /// 使用原始SQL语句rawSql和参数parameters查询数据，并返回满足条件的所有TEntity实体记录，记录不存在时返回没有任何元素的List&lt;TEntity&gt;类型空列表
    /// </summary>
    /// <typeparam name="TEntity">实体TEntity类型</typeparam>
    /// <param name="rawSql">原始SQL</param>
    /// <param name="parameters">参数，可以是命名对象、匿名对象或是Dictionary类型对象</param>
    /// <returns>返回查询结果，记录不存在时返回没有任何元素的List&lt;TEntity&gt;类型空列表</returns>
    List<TEntity> Query<TEntity>(string rawSql, object parameters = null);
    /// <summary>
    /// 使用原始SQL语句rawSql和参数parameters查询数据，并返回满足条件的所有TEntity实体记录，记录不存在时返回没有任何元素的List&lt;TEntity&gt;类型空列表
    /// </summary>
    /// <typeparam name="TEntity">实体TEntity类型</typeparam>
    /// <param name="rawSql">原始SQL</param>
    /// <param name="parameters">参数，可以是命名对象、匿名对象或是Dictionary类型对象</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回查询结果，记录不存在时返回没有任何元素的List&lt;TEntity&gt;类型空列表</returns>
    Task<List<TEntity>> QueryAsync<TEntity>(string rawSql, object parameters = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// 从表TEntity中，查询与whereObj对象各属性值都相等的所有记录，记录不存在时返回没有任何元素的List&lt;TEntity&gt;类型空列表，用法：
    /// <code>
    /// repository.Query&lt;User&gt;(new { Id = 1, IsEnabled = true })
    /// SQL: SELECT `Id`,`Name`,`Gender`,`Age`,`CompanyId`,`GuidField`,`SomeTimes`,`SourceType`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt` FROM `sys_user` WHERE `Id`=@Id AND `IsEnabled`=@IsEnabled
    /// </code>
    /// </summary>
    /// <typeparam name="TEntity">实体TEntity类型</typeparam>
    /// <param name="whereObj">参数，可以是命名对象、匿名对象或是Dictionary类型对象，不能为null</param>
    /// <returns>返回查询结果，记录不存在时返回没有任何元素的List&lt;TEntity&gt;类型空列表</returns>
    List<TEntity> Query<TEntity>(object whereObj);
    /// <summary>
    /// 从表TEntity中，查询与whereObj对象各属性值都相等的所有记录，记录不存在时返回没有任何元素的List&lt;TEntity&gt;类型空列表，用法：
    /// <code>
    /// await repository.QueryAsync&lt;User&gt;(new { Id = 1, IsEnabled = true })
    /// SQL: SELECT `Id`,`Name`,`Gender`,`Age`,`CompanyId`,`GuidField`,`SomeTimes`,`SourceType`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt` FROM `sys_user` WHERE `Id`=@Id AND `IsEnabled`=@IsEnabled
    /// </code>
    /// </summary>
    /// <typeparam name="TEntity">实体TEntity类型</typeparam>
    /// <param name="whereObj">参数，可以是命名对象、匿名对象或是Dictionary类型对象，不能为null</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回查询结果，记录不存在时返回没有任何元素的List&lt;TEntity&gt;类型空列表</returns>
    Task<List<TEntity>> QueryAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default);
    #endregion

    #region Get
    /// <summary>
    /// 根据主键信息查询表TEntity中数据，记录不存在时返回TEntity类型的默认值，用法：
    /// <code>
    /// repository.Get&lt;User&gt;(1) //或是
    /// repository.Get&lt;User&gt;(new { Id = 1 }) //或是
    /// var userInfo = new UserInfo { Id = 1, Name = "xxx" ... };
    /// repository.Get&lt;User&gt;(userInfo) //三种写法是等效的
    /// SQL: SELECT ... FROM `sys_user` WHERE `Id`=@Id
    /// </code>
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="whereObj">主键值或是包含主键的匿名对象或是已有对象，如：1，2或new { Id = 1}或是已有对象userInfo(包含主键栏位Id) </param>
    /// <returns>返回实体对象或是TEntity类型默认值</returns>
    TEntity Get<TEntity>(object whereObj);
    /// <summary>
    /// 根据主键信息查询表TEntity中数据，记录不存在时返回TEntity类型的默认值，用法：
    /// <code>
    /// await repository.GetAsync&lt;User&gt;(1) //或是
    /// await repository.GetAsync&lt;User&gt;(new { Id = 1 }) //或是
    /// var userInfo = new UserInfo { Id = 1, Name = "xxx" ...};
    /// await repository.GetAsync&lt;User&gt;(userInfo) //三种写法是等效的
    /// SQL: SELECT ... FROM `sys_user` WHERE `Id`=@Id
    /// </code>
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="whereObj">主键值或是包含主键的匿名对象或是已有对象，如：1，2或new { Id = 1}或是已有对象userInfo(包含主键栏位Id) </param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回实体对象或是TEntity类型默认值</returns>
    Task<TEntity> GetAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default);
    #endregion

    #region Exists
    /// <summary>
    /// 判断是否存在表TEntity中满足与whereObj对象各属性值都相等的记录，存在返回true，否则返回false。
    /// <code>
    /// repository.Exists&lt;User&gt;(new { Id = 1, IsEnabled = true })
    /// SQL: SELECT COUNT(1) FROM `sys_user` WHERE `Id`=@Id AND `IsEnabled`=@IsEnabled
    /// </code>
    /// </summary>
    /// <typeparam name="TEntity">实体对象类型</typeparam>
    /// <param name="whereObj">where条件对象，whereObj对象各属性值都参与相等比较,推荐使用匿名对象</param>
    /// <returns>返回是否存在的布尔值</returns>
    bool Exists<TEntity>(object whereObj);
    /// <summary>
    /// 判断是否存在表TEntity中满足与whereObj对象各属性值都相等的记录，存在返回true，否则返回false。
    /// <code>
    /// await repository.ExistsAsync&lt;User&gt;(new { Id = 1, IsEnabled = true })
    /// SQL: SELECT COUNT(1) FROM `sys_user` WHERE `Id`=@Id AND `IsEnabled`=@IsEnabled
    /// </code>
    /// </summary>
    /// <typeparam name="TEntity">实体对象类型</typeparam>
    /// <param name="whereObj">where条件对象，whereObj对象各属性值都参与相等比较,推荐使用匿名对象</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回是否存在的布尔值</returns>
    Task<bool> ExistsAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default);
    /// <summary>
    /// 判断TEntity表是否存在满足predicate条件的记录，存在返回true，否则返回false。
    /// </summary>
    /// <typeparam name="TEntity">实体对象类型</typeparam>
    /// <param name="predicate">where条件表达式</param>
    /// <returns>返回是否存在的布尔值</returns>
    bool Exists<TEntity>(Expression<Func<TEntity, bool>> predicate);
    /// <summary>
    /// 判断TEntity表是否存在满足predicate条件的记录，存在返回true，否则返回false。
    /// </summary>
    /// <typeparam name="TEntity">实体对象类型</typeparam>
    /// <param name="predicate">where条件表达式</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回是否存在的布尔值</returns>
    Task<bool> ExistsAsync<TEntity>(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    #endregion

    #region Create
    /// <summary>
    /// 创建TEntity类型插入对象
    /// </summary>
    /// <typeparam name="TEntity">插入实体类型</typeparam>
    /// <returns>返回插入对象</returns>
    ICreate<TEntity> Create<TEntity>();
    /// <summary>
    /// 使用插入对象部分字段插入，可单条也可多条数据插入，自动增长栏位，不需要传入，多条可分批次完成，每次插入bulkCount条数，批量插入,采用多表值方式，用法：
    /// <code>
    /// repository.Create&lt;User&gt;(new
    /// {
    ///     Name = "leafkevin",
    ///     Age = 25,
    ///     UpdatedAt = DateTime.Now,
    ///     UpdatedBy = 1
    /// });
    /// repository.Create&lt;Product&gt;(new []{ new { ... }, new { ... }, new { ... });
    /// SQL:
    /// INSERT INTO `sys_user` (`Name`,`Age`,`UpdatedAt`,`UpdatedBy`) VALUES(@Name,@Age,@UpdatedAt,@UpdatedBy)
    /// INSERT INTO [sys_product] ([ProductNo],[Name],...) VALUES (@ProductNo0,@Name0,...),(@ProductNo1,@Name1,...),(@ProductNo2,@Name2,...)...
    /// </code>
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="insertObjs">插入对象，可以是匿名对象、实体对象、字典，也可以是这些类型的IEnumerable类型，如：new { Value1 = 1, Value2 = "xxx" } 或 new Order{ ... }</param>
    /// <param name="bulkCount">单次插入最多的条数，根据插入对象大小找到最佳的设置阈值，默认值500</param>
    /// <returns>返回插入行数</returns>
    int Create<TEntity>(object insertObjs, int bulkCount = 500);
    /// <summary>
    /// 使用插入对象部分字段插入，可单条也可多条数据插入，自动增长栏位，不需要传入，多条可分批次完成，每次插入bulkCount条数，批量插入,采用多表值方式，用法：
    /// <code>
    /// await repository.CreateAsync&lt;User&gt;(new
    /// {
    ///     Name = "leafkevin",
    ///     Age = 25,
    ///     UpdatedAt = DateTime.Now,
    ///     UpdatedBy = 1
    /// });
    /// await repository.CreateAsync&lt;Product&gt;(new []{ new { ... }, new { ... }, new { ... });
    /// SQL:
    /// INSERT INTO `sys_user` (`Name`,`Age`,`UpdatedAt`,`UpdatedBy`) VALUES(@Name,@Age,@UpdatedAt,@UpdatedBy)
    /// INSERT INTO [sys_product] ([ProductNo],[Name],...) VALUES (@ProductNo0,@Name0,...),(@ProductNo1,@Name1,...),(@ProductNo2,@Name2,...)...
    /// </code>
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="insertObjs">插入对象，可以是匿名对象、实体对象、字典，也可以是这些类型的IEnumerable类型，如：new { Value1 = 1, Value2 = "xxx" } 或 new Order{ ... }</param>
    /// <param name="bulkCount">单次插入最多的条数，根据插入对象大小找到最佳的设置阈值，默认值500</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回插入行数</returns>
    Task<int> CreateAsync<TEntity>(object insertObjs, int bulkCount = 500, CancellationToken cancellationToken = default);
    /// <summary>
    ///  使用插入对象部分字段插入，并返回自增长ID，自动增长栏位，不需要传入
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="insertObj">插入对象，可以是匿名对象、实体对象、字典</param>
    /// <returns>返回自增长ID</returns>
    int CreateIdentity<TEntity>(object insertObj);
    /// <summary>
    ///  使用插入对象部分字段插入，并返回自增长ID，自动增长栏位，不需要传入
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="insertObj">插入对象，可以是匿名对象、实体对象、字典</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回自增长ID</returns>
    Task<int> CreateIdentityAsync<TEntity>(object insertObj, CancellationToken cancellationToken = default);
    /// <summary>
    ///  使用插入对象部分字段插入，并返回自增长ID，自动增长栏位，不需要传入
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="insertObj">插入对象，可以是匿名对象、实体对象、字典</param>
    /// <returns>返回自增长ID</returns>
    long CreateIdentityLong<TEntity>(object insertObj);
    /// <summary>
    ///  使用插入对象部分字段插入，并返回自增长ID，自动增长栏位，不需要传入
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="insertObj">插入对象，可以是匿名对象、实体对象、字典</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回自增长ID</returns>
    Task<long> CreateIdentityLongAsync<TEntity>(object insertObj, CancellationToken cancellationToken = default);
    #endregion

    #region Update
    /// <summary>
    /// 创建TEntity类型更新对象
    /// </summary>
    /// <typeparam name="TEntity">更新实体类型</typeparam>
    /// <returns>返回更新对象</returns>
    IUpdate<TEntity> Update<TEntity>();
    /// <summary>
    /// 使用更新对象updateObjs部分字段By主键更新，updateObjs对象内除主键字段外所有与当前实体表TEntity名称相同的栏位都将参与更新，updateObjs对象必须包含主键字段，可单条也可多条数据更新，多条可分批次完成，每次更新bulkCount条数，用法：
    /// <code>
    /// repository.Update&lt;User&gt;(new { Id = 1, Name = "kevin"});
    /// repository.Update&lt;User&gt;(new [] { new { Id = 1, Name = "kevin"}, new { Id = 2, Name = "cindy"} }, 200);
    /// SQL: 
    /// UPDATE `sys_user` SET `Name`=@Name WHERE `Id`=@Id
    /// UPDATE `sys_user` SET `Name`=@Name0 WHERE `Id`=@Id0;UPDATE `sys_user` SET `Name`=@Name1 WHERE `Id`=@Id1
    /// </code>
    /// </summary>
    /// <param name="updateObjs">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新</param>
    /// <returns>返回更新对象</returns> 
    int Update<TEntity>(object updateObjs, int bulkCount = 500);
    /// <summary>
    /// 使用更新对象updateObj部分字段By主键更新，updateObj对象内除主键字段外所有与当前实体表TEntity名称相同的栏位都将参与更新，updateObj对象必须包含主键字段，可单条也可多条数据更新，多条可分批次完成，每次更新bulkCount条数，用法：
    /// <code>
    /// repository.UpdateAsync&lt;User&gt;(new { Id = 1, Name = "kevin", SourceType = DBNull.Value});
    /// repository.UpdateAsync&lt;User&gt;(new [] { new { Id = 1, Name = "kevin"}, new { Id = 2, Name = "cindy"} }, 200);
    /// SQL: 
    /// UPDATE `sys_user` SET `Name`=@Name,SourceType=@SourceType WHERE `Id`=@Id
    /// UPDATE `sys_user` SET `Name`=@Name0 WHERE `Id`=@Id0;UPDATE `sys_user` SET `Name`=@Name1 WHERE `Id`=@Id1
    /// </code>
    /// </summary>
    /// <param name="updateObjs">部分字段更新对象参数，包含想要更新的必需栏位值，updateObj对象内的栏位都将参与更新</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回更新行数</returns>
    Task<int> UpdateAsync<TEntity>(object updateObjs, int bulkCount = 500, CancellationToken cancellationToken = default);
    #endregion

    #region Delete
    /// <summary>
    /// 创建TEntity类型删除对象
    /// </summary>
    /// <typeparam name="TEntity">删除实体类型</typeparam>
    /// <returns>返回删除对象</returns>
    IDelete<TEntity> Delete<TEntity>();
    /// <summary>
    /// 根据主键删除数据，可以删除一条也可以删除多条记录，keys可以是主键值也可以是包含主键值的匿名对象，用法：
    /// <code>
    /// 单个删除,下面两个方法等效
    /// repository.Delete&lt;User&gt;(1);
    /// repository.Delete&lt;User&gt;(new { Id = 1});
    /// 批量删除,下面两个方法等效
    /// repository.Delete&lt;User&gt;(new[] { 1, 2 });
    /// repository.Delete&lt;User&gt;(new[] { new { Id = 1 }, new { Id = 2 } });
    /// </code>
    /// </summary>
    /// <typeparam name="TEntity">要删除的实体类型</typeparam>
    /// <param name="whereKeys">主键值，可以是一个值或是一个匿名对象，也可以是多个值或是多个匿名对象</param>
    /// <returns>返回删除行数</returns>
    int Delete<TEntity>(object whereKeys);
    /// <summary>
    /// 根据主键删除数据，可以删除一条也可以删除多条记录，keys可以是主键值也可以是包含主键值的匿名对象，用法：
    /// <code>
    /// 单个删除,下面两个方法等效
    /// await repository.DeleteAsync&lt;User&gt;(1);
    /// await repository.DeleteAsync&lt;User&gt;(new { Id = 1});
    /// 批量删除,下面两个方法等效
    /// await repository.DeleteAsync&lt;User&gt;(new[] { 1, 2 });
    /// await repository.DeleteAsync&lt;User&gt;(new[] { new { Id = 1 }, new { Id = 2 } });
    /// </code>
    /// </summary>
    /// <typeparam name="TEntity">要删除的实体类型</typeparam>
    /// <param name="whereKeys">主键值，可以是一个值或是一个匿名对象，也可以是多个值或是多个匿名对象</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回删除行数</returns>
    Task<int> DeleteAsync<TEntity>(object whereKeys, CancellationToken cancellationToken = default);
    #endregion

    #region Execute
    /// <summary>
    /// 执行原始SQL，并返回影响行数
    /// </summary>
    /// <param name="rawSql">要执行的SQL</param>
    /// <param name="parameters">SQL中使用的参数，可以是已有对象、匿名对象或是Dictionary类型对象，可以为null</param>
    /// <returns>返回影响行数</returns>
    int Execute(string rawSql, object parameters = null);
    /// <summary>
    /// 执行原始SQL，并返回影响行数
    /// </summary>
    /// <param name="rawSql">要执行的SQL</param>
    /// <param name="parameters">SQL中使用的参数，可以是已有对象、匿名对象或是Dictionary类型对象，可以为null</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回影响行数</returns>
    Task<int> ExecuteAsync(string rawSql, object parameters = null, CancellationToken cancellationToken = default);
    #endregion

    #region QueryMultiple
    #region 不支持
    ///// <summary>
    ///// 多SQL语句一起执行，并返回多个结果集，根据SQL语句按顺序接收返回结果。
    ///// </summary>
    ///// <param name="rawSql">要执行的SQL</param>
    ///// <param name="parameters">SQL中使用的参数，可以是已有对象、匿名对象或是Dictionary类型对象，可以为null</param>
    ///// <returns>返回多结果集Reader对象</returns>
    //IMultiQueryReader QueryMultiple(string rawSql, object parameters = null);
    ///// <summary>
    ///// 多SQL语句一起执行，并返回多个结果集，根据SQL语句顺序接收返回结果。
    ///// </summary>
    ///// <param name="rawSql">要执行的SQL</param>
    ///// <param name="parameters">SQL中使用的参数，可以是已有对象、匿名对象或是Dictionary类型对象，可以为null</param>
    ///// <param name="cancellationToken">取消Token</param>
    ///// <returns>返回多结果集Reader对象</returns>
    //Task<IMultiQueryReader> QueryMultipleAsync(string rawSql, object parameters = null, CancellationToken cancellationToken = default);
    #endregion
    /// <summary>
    /// 使用IMultipleQuery操作生成多个SQL语句一起执行，并返回多个结果集，根据IMultipleQuery操作顺序接收返回结果。
    /// </summary>
    /// <param name="subQueries">多个SQL查询操作，不能为null</param>
    /// <returns>返回多结果集Reader对象</returns>
    IMultiQueryReader QueryMultiple(Action<IMultipleQuery> subQueries);
    /// <summary>
    /// 使用IMultipleQuery操作生成多个SQL语句一起执行，并返回多个结果集，根据IMultipleQuery操作顺序接收返回结果。
    /// </summary>
    /// <param name="subQueries">多个SQL查询操作，不能为null</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回多结果集Reader对象</returns>
    Task<IMultiQueryReader> QueryMultipleAsync(Action<IMultipleQuery> subQueries, CancellationToken cancellationToken = default);
    #endregion

    #region MultipleExecute
    void MultipleExecute(List<MultipleCommand> commands);
    Task MultipleExecuteAsync(List<MultipleCommand> commands, CancellationToken cancellationToken = default);
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