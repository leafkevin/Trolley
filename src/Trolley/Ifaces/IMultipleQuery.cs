﻿using System;
using System.Linq.Expressions;

namespace Trolley;

public interface IMultipleQuery
{
    #region UseMaster
    /// <summary>
    /// 使用主库查询
    /// </summary>
    /// <param name="isUseMaster">使用主库为true，默认值为true</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery UseMaster(bool isUseMaster = true);
    #endregion

    #region From
    /// <summary>
    /// 查询数据，用法：
    /// <code>
    /// f.From&lt;Menu&gt;()
    /// SQL: FROM `sys_menu`
    /// </code>
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <param name="tableAsStart">表别名起始字母，默认从字母'a'开始</param>
    /// </param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T> From<T>(char tableAsStart = 'a');
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
    #endregion

    #region From SubQuery
    /// <summary>
    /// 从SQL子查询中查询数据，用法：
    /// <code>
    /// repository
    ///     .From(f =&gt; f.From&lt;Page, Menu&gt;('o') ...
    ///         .Select((x, y) =&gt; new { ... }))
    /// SQL:
    /// ... FROM (SELECT ... FROM `sys_page` o,`sys_menu` p WHERE ...) ...
    /// </code>
    /// </summary>
    /// <typeparam name="T">表T实体类型</typeparam>
    /// <param name="subQuery">子查询</param>
    /// <param name="tableAsStart">表别名起始字母，默认值从字母a开始</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T> From<T>(Func<IFromQuery, IQuery<T>> subQuery, char tableAsStart = 'a');
    #endregion

    #region QueryFirst/Query
    /// <summary>
    /// 使用原始SQL语句rawSql查询数据，并返回满足条件的第一条记录，记录不存在时返回TEntity类型的默认值
    /// </summary>
    /// <typeparam name="TEntity">实体TEntity类型</typeparam>
    /// <param name="rawSql">原始SQL</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery QueryFirst<TEntity>(string rawSql);
    /// <summary>
    /// 使用原始SQL语句rawSql和参数parameters查询数据，并返回满足条件的第一条记录，记录不存在时返回TEntity类型的默认值
    /// </summary>
    /// <typeparam name="TEntity">实体TEntity类型</typeparam>
    /// <param name="rawSql">原始SQL</param>
    /// <param name="parameters">参数，可以是命名对象、匿名对象或是Dictionary类型对象，parameters不可为null</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery QueryFirst<TEntity>(string rawSql, object parameters);
    /// <summary>
    ///从表TEntity中，查询与whereObj对象各属性值都相等的第一条记录，记录不存在时返回TEntity类型的默认值，用法：
    /// <code>
    /// f.QueryFirst&lt;User&gt;(new { Id = 1, IsEnabled = true })
    /// SQL: SELECT `Id`,`Name`,`Gender`,`Age`,`CompanyId`,`GuidField`,`SomeTimes`,`SourceType`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt` FROM `sys_user` WHERE `Id`=@Id AND `IsEnabled`=@IsEnabled
    /// </code>
    /// </summary>
    /// <typeparam name="TEntity">实体TEntity类型</typeparam>
    /// <param name="whereObj">参数，可以是命名对象、匿名对象或是Dictionary类型对象</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery QueryFirst<TEntity>(object whereObj);
    /// <summary>
    /// 使用原始SQL语句rawSql查询数据，并返回满足条件的所有TEntity实体记录，记录不存在时返回没有任何元素的List&lt;TEntity&gt;类型空列表
    /// </summary>
    /// <typeparam name="TEntity">实体TEntity类型</typeparam>
    /// <param name="rawSql">原始SQL</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Query<TEntity>(string rawSql);
    /// <summary>
    /// 使用原始SQL语句rawSql和参数parameters查询数据，并返回满足条件的所有TEntity实体记录，记录不存在时返回没有任何元素的List&lt;TEntity&gt;类型空列表
    /// </summary>
    /// <typeparam name="TEntity">实体TEntity类型</typeparam>
    /// <param name="rawSql">原始SQL</param>
    /// <param name="parameters">参数，可以是命名对象、匿名对象或是Dictionary类型对象，参数parameters不能为null</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Query<TEntity>(string rawSql, object parameters);
    /// <summary>
    /// 从表TEntity中，查询与whereObj对象各属性值都相等的所有记录，记录不存在时返回没有任何元素的List&lt;TEntity&gt;类型空列表，用法：
    /// <code>
    /// f.Query&lt;User&gt;(new { Id = 1, IsEnabled = true })
    /// SQL: SELECT `Id`,`Name`,`Gender`,`Age`,`CompanyId`,`GuidField`,`SomeTimes`,`SourceType`,`IsEnabled`,`CreatedBy`,`CreatedAt`,`UpdatedBy`,`UpdatedAt` FROM `sys_user` WHERE `Id`=@Id AND `IsEnabled`=@IsEnabled
    /// </code>
    /// </summary>
    /// <typeparam name="TEntity">实体TEntity类型</typeparam>
    /// <param name="whereObj">参数，可以是命名对象、匿名对象或是Dictionary类型对象，不能为null</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Query<TEntity>(object whereObj);
    #endregion

    #region Get
    /// <summary>
    /// 根据主键信息查询表TEntity中数据，记录不存在时返回TEntity类型的默认值，用法：
    /// <code>
    /// f.Get&lt;User&gt;(1) //或是
    /// f.Get&lt;User&gt;(new { Id = 1 }) //或是
    /// var userInfo = new UserInfo { Id = 1, Name = "xxx" ... };
    /// f.Get&lt;User&gt;(userInfo) //三种写法是等效的
    /// SQL: SELECT ... FROM `sys_user` WHERE `Id`=@Id
    /// </code>
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="whereObj">主键值或是包含主键的匿名对象或是已有对象，如：1，2或new { Id = 1}或是已有对象userInfo(包含主键栏位Id) </param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Get<TEntity>(object whereObj);
    #endregion

    #region Exists
    /// <summary>
    /// 判断是否存在表TEntity中满足与whereObj对象各属性值都相等的记录，存在返回true，否则返回false。
    /// <code>
    /// f.Exists&lt;User&gt;(new { Id = 1, IsEnabled = true })
    /// SQL: SELECT COUNT(1) FROM `sys_user` WHERE `Id`=@Id AND `IsEnabled`=@IsEnabled
    /// </code>
    /// </summary>
    /// <typeparam name="TEntity">实体对象类型</typeparam>
    /// <param name="whereObj">where条件对象，whereObj对象各属性值都参与相等比较,推荐使用匿名对象</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Exists<TEntity>(object whereObj);
    /// <summary>
    /// 判断TEntity表是否存在满足predicate条件的记录，存在返回true，否则返回false。
    /// </summary>
    /// <typeparam name="TEntity">实体对象类型</typeparam>
    /// <param name="predicate">where条件表达式</param>
    /// <returns>返回查询对象</returns>
    IMultipleQuery Exists<TEntity>(Expression<Func<TEntity, bool>> predicate = null);
    #endregion
}
