using System;

namespace Trolley.PostgreSql;

public interface IPostgreSqlRepository : IRepository
{
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
    /// </param>
    /// <returns>返回查询对象</returns>
    new IPostgreSqlQuery<T> From<T>(char tableAsStart = 'a');
    /// <summary>
    /// 使用2个表创建查询对象
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <param name="tableAsStart">表别名起始字母，默认值从字母a开始</param>
    /// <returns>返回查询对象</returns>
    new IPostgreSqlQuery<T1, T2> From<T1, T2>(char tableAsStart = 'a');
    /// <summary>
    /// 使用3个表创建查询对象
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <typeparam name="T3">表T3实体类型</typeparam>
    /// <param name="tableAsStart">表别名起始字母，默认值从字母a开始</param>
    /// <returns>返回查询对象</returns>
    new IPostgreSqlQuery<T1, T2, T3> From<T1, T2, T3>(char tableAsStart = 'a');
    /// <summary>
    /// 使用4个表创建查询对象
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <typeparam name="T3">表T3实体类型</typeparam>
    /// <typeparam name="T4">表T4实体类型</typeparam>
    /// <param name="tableAsStart">表别名起始字母，默认值从字母a开始</param>
    /// <returns>返回查询对象</returns>
    new IPostgreSqlQuery<T1, T2, T3, T4> From<T1, T2, T3, T4>(char tableAsStart = 'a');
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
    new IPostgreSqlQuery<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>(char tableAsStart = 'a');
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
    new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>(char tableAsStart = 'a');
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
    new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7> From<T1, T2, T3, T4, T5, T6, T7>(char tableAsStart = 'a');
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
    new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8> From<T1, T2, T3, T4, T5, T6, T7, T8>(char tableAsStart = 'a');
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
    new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T1, T2, T3, T4, T5, T6, T7, T8, T9>(char tableAsStart = 'a');
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
    new IPostgreSqlQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(char tableAsStart = 'a');
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
    /// <returns>返回查询对象</returns>
    new IPostgreSqlQuery<T> From<T>(IQuery<T> subQuery);
    /// <summary>
    /// 从SQL子查询中查询数据，用法：
    /// <code>
    /// repository
    ///     .From(f =&gt; f.From&lt;Page, Menu&gt;('o')
    ///         .Where((a, b) =&gt; a.Id == b.PageId)
    ///         .Select((x, y) =&gt; new { y.Id, y.ParentId, x.Url }))
    ///     ...
    /// SQL:
    /// ... FROM (SELECT p.`Id`,p.`ParentId`,o.`Url` FROM `sys_page` o,`sys_menu` p WHERE o.`Id`=p.`PageId`) ...
    /// </code>
    /// </summary>
    /// <typeparam name="T">表T实体类型</typeparam>
    /// <param name="subQuery">子查询</param>
    /// <returns>返回查询对象</returns>
    new IPostgreSqlQuery<T> From<T>(Func<IFromQuery, IQuery<T>> subQuery);
    #endregion

    #region Create
    /// <summary>
    /// 创建TEntity类型插入对象
    /// </summary>
    /// <typeparam name="TEntity">插入实体类型</typeparam>
    /// <returns>返回插入对象</returns>
    new IPostgreSqlCreate<TEntity> Create<TEntity>();
    #endregion

    #region Update
    /// <summary>
    /// 创建TEntity类型更新对象
    /// </summary>
    /// <typeparam name="TEntity">更新实体类型</typeparam>
    /// <returns>返回更新对象</returns>
    new IPostgreSqlUpdate<TEntity> Update<TEntity>();
    #endregion
}