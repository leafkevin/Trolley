using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Trolley;

public interface IPostgreSqlMultipleQuery : IMultipleQuery
{
    #region GetShardingTableNames
    /// <summary>
    /// 获取实体TEntity满足条件的所有分表名
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="tableNameSelector">分表名选择表达式</param>
    /// <param name="tableSchema">分表所在的TableSchema</param>
    /// <returns>返回满足条件的所有分表</returns>
    new IPostgreSqlMultipleQuery GetShardingTableNames<TEntity>(Func<string, bool> tableNameSelector = null, string tableSchema = null);
    /// <summary>
    /// 根据字段值确定TEntity表分表名，最多支持3个字段值，字段值的顺序与分表规则设置的顺序保持一致
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="fieldValues">字段值数组，字段值的顺序与分表规则设置的顺序保持一致，不可为null</param>
    /// <returns>返回满足条件的分表名</returns>
    new IPostgreSqlMultipleQuery GetShardingTableNameBy<TEntity>(params object[] fieldValues);
    #endregion

    #region From
    /// <summary>
    /// 从表T中查询数据，用法：
    /// <code>
    /// f.From&lt;Menu&gt;()
    /// SQL: FROM `sys_menu`
    /// </code>
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <param name="tableAsStart">表别名起始字母，默认从字母'a'开始</param>
    /// </param>
    /// <returns>返回查询对象</returns>
    new IPostgreSqlMultipleQuery<T> From<T>(char tableAsStart = 'a');
    /// <summary>
    /// 使用2个表创建查询对象
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <param name="tableAsStart">表别名起始字母，默认值从字母a开始</param>
    /// <returns>返回查询对象</returns>
    new IPostgreSqlMultipleQuery<T1, T2> From<T1, T2>(char tableAsStart = 'a');
    /// <summary>
    /// 使用3个表创建查询对象
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <typeparam name="T3">表T3实体类型</typeparam>
    /// <param name="tableAsStart">表别名起始字母，默认值从字母a开始</param>
    /// <returns>返回查询对象</returns>
    new IPostgreSqlMultipleQuery<T1, T2, T3> From<T1, T2, T3>(char tableAsStart = 'a');
    /// <summary>
    /// 使用4个表创建查询对象
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <typeparam name="T3">表T3实体类型</typeparam>
    /// <typeparam name="T4">表T4实体类型</typeparam>
    /// <param name="tableAsStart">表别名起始字母，默认值从字母a开始</param>
    /// <returns>返回查询对象</returns>
    new IPostgreSqlMultipleQuery<T1, T2, T3, T4> From<T1, T2, T3, T4>(char tableAsStart = 'a');
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
    new IPostgreSqlMultipleQuery<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>(char tableAsStart = 'a');
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
    new IPostgreSqlMultipleQuery<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>(char tableAsStart = 'a');
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
    new IPostgreSqlMultipleQuery<T1, T2, T3, T4, T5, T6, T7> From<T1, T2, T3, T4, T5, T6, T7>(char tableAsStart = 'a');
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
    new IPostgreSqlMultipleQuery<T1, T2, T3, T4, T5, T6, T7, T8> From<T1, T2, T3, T4, T5, T6, T7, T8>(char tableAsStart = 'a');
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
    new IPostgreSqlMultipleQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9> From<T1, T2, T3, T4, T5, T6, T7, T8, T9>(char tableAsStart = 'a');
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
    new IPostgreSqlMultipleQuery<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> From<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(char tableAsStart = 'a');
    #endregion

    #region FromQuery
    /// <summary>
    /// 从SQL子查询中查询数据，用法：
    /// <code>
    /// var subQuery = f.From&lt;Page, Menu&gt;('o')
    ///     .Where((a, b) =&gt; a.Id == b.PageId)
    ///     .Select((x, y) =&gt; new { y.Id, y.ParentId, x.Url });
    /// f.FromQuery(subQuery) ...
    /// SQL:
    /// ... FROM (SELECT p.`Id`,p.`ParentId`,o.`Url` FROM `sys_page` o,`sys_menu` p WHERE o.`Id`=p.`PageId`) ...
    /// </code>
    /// </summary>
    /// <typeparam name="T">表T实体类型</typeparam>
    /// <param name="subQuery">子查询</param>
    /// <returns>返回查询对象</returns>
    new IPostgreSqlMultipleQuery<T> FromQuery<T>(IQuery<T> subQuery);
    /// <summary>
    /// 从SQL子查询中查询数据，用法：
    /// <code>
    ///  t.From(f =&gt; f.From&lt;Page, Menu&gt;('o') ...
    ///       .Select((x, y) =&gt; new { ... }))
    /// SQL:
    /// ... FROM (SELECT ... FROM `sys_page` o,`sys_menu` p WHERE ...) ...
    /// </code>
    /// </summary>
    /// <typeparam name="T">表T实体类型</typeparam>
    /// <param name="subQueryExpr">子查询</param>
    /// <returns>返回查询对象</returns>
    new IPostgreSqlMultipleQuery<T> FromQuery<T>(Expression<Func<IFromQuery, IQuery<T>>> subQueryExpr);
    #endregion
}