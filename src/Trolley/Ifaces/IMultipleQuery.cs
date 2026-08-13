using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Trolley;

public interface IMultipleQuery : ICommandVisitor, IDisposable
{
    #region Properties
    DbContext DbContext { get; }
    List<ReaderAfter> ReaderAfters { get; }
    #endregion

    #region GetShardingTableName
    /// <summary>
    /// 根据字段值确定<typeparamref name="TEntity"/>表分表名，字段值的顺序与分表规则设置的顺序保持一致
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="fieldValues">字段值数组，字段值的顺序与分表规则设置的顺序保持一致，不可为null</param>
    /// <returns>返回满足条件的分表名</returns>
    IMultipleQuery GetShardingTableName<TEntity>(params object[] fieldValues);
    #endregion

    #region From
    /// <summary>
    /// 使用1个表创建查询对象
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

    #region FromQuery
    /// <summary>
    /// 从SQL子查询中查询数据，如：
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
    IMultiQuery<T> FromQuery<T>(IQuery<T> subQuery);
    /// <summary>
    /// 从SQL子查询中查询数据，如：
    /// <code>
    ///  t.From(f =&gt; f.From&lt;Page, Menu&gt;('o') ...
    ///       .Select((x, y) =&gt; new { ... }))
    /// SQL:
    /// ... FROM (SELECT ... FROM `sys_page` o,`sys_menu` p WHERE ...) ...
    /// </code>
    /// </summary>
    /// <typeparam name="T">表T实体类型</typeparam>
    /// <param name="subQueryExpr">子查询表达式</param>
    /// <returns>返回查询对象</returns>
    IMultiQuery<T> FromQuery<T>(Expression<Func<IFromQuery, IQuery<T>>> subQueryExpr);
    #endregion

    #region QueryScalar
    /// <summary>
    /// 指定原始SQL语句，查询单个值
    /// </summary>
    /// <typeparam name="TValue">返回值类型</typeparam>
    /// <param name="rawSql">原始SQL语句</param>
    /// <returns>返回单个值</returns>
    IMultipleQuery QueryScalar<TValue>(string rawSql);
    /// <summary>
    /// 指定原始SQL语句，查询单个值
    /// </summary>
    /// <typeparam name="TValue">返回值类型</typeparam>
    /// <param name="rawSql">原始SQL语句</param>
    /// <param name="parameters">参数，可以是命名对象、匿名对象或是字典类型对象，不可为null</param>
    /// <returns>返回单个值</returns>
    IMultipleQuery QueryScalar<TValue>(string rawSql, object parameters);
    /// <summary>
    /// 指定原始SQL语句，查询单个值
    /// </summary>
    /// <typeparam name="TValue">返回值类型</typeparam>
    /// <param name="rawSql">原始SQL语句</param>
    /// <param name="parameters">参数列表，不可为null</param>
    /// <returns>返回单个值</returns>
    IMultipleQuery QueryScalar<TValue>(string rawSql, List<IDbDataParameter> parameters);
    #endregion

    #region QueryById
    /// <summary>
    /// 根据主键信息查询表TEntity中数据，记录不存在时返回TEntity类型的默认值，不支持分表，如：
    /// <code>
    /// f.QueryById&lt;User&gt;(1) //或是
    /// f.QueryById&lt;User&gt;(new { Id = 1 }) //或是
    /// var userInfo = new UserInfo { Id = 1, Name = "xxx" ... };
    /// f.QueryById&lt;User&gt;(userInfo) //三种写法是等效的
    /// SQL: SELECT ... FROM `sys_user` a WHERE a.`Id`=@Id
    /// </code>
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="whereObj">主键值或是包含主键的匿名对象或是已有对象，如：1，2或new { Id = 1}或是已有对象userInfo(包含主键栏位Id) </param>
    /// <returns>返回实体对象或是TEntity类型默认值</returns>
    IMultipleQuery QueryById<TEntity>(object whereObj);
    #endregion

    #region QueryByIds
    /// <summary>
    /// 根据多个主键信息查询表TEntity中数据，记录不存在时返回没有任何元素的List&lt;TEntity&gt;类型空列表，不支持分表，如：
    /// <code>
    /// f.QueryByIds&lt;User&gt;(new []{1 ,2, 3}) //或是
    /// f.QueryByIds&lt;User&gt;(new []{{ Id = 1 }, { Id = 2 }, { Id = 3 }}) //或是
    /// var userInfo = new UserInfo { Id = 1, Name = "xxx" ... };
    /// f.QueryByIds&lt;User&gt;(new List&lt;UserInfo&gt;{userInfo}) //三种写法是等效的
    /// SQL: SELECT ... FROM `sys_user` a WHERE a.`Id` in (@Id0,@Id1,@Id2)
    /// </code>
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="whereKeys">主键值或是包含主键的匿名对象或是已有对象，如：1，2或new { Id = 1}或是已有对象userInfo(包含主键栏位Id) </param>
    /// <returns>返回查询结果，记录不存在时返回没有任何元素的List&lt;TEntity&gt;类型空列表</returns>
    IMultipleQuery QueryByIds<TEntity>(IEnumerable whereKeys);
    #endregion

    #region QueryFirst
    /// <summary>
    /// 使用原始SQL语句rawSql查询数据，并返回满足条件的第一条记录，记录不存在时返回TEntity类型的默认值，不支持分表
    /// </summary>
    /// <typeparam name="TEntity">实体TEntity类型</typeparam>
    /// <param name="rawSql">原始SQL</param>
    /// <returns>返回查询结果，记录不存在时返回TEntity类型的默认值</returns>
    IMultipleQuery QueryFirst<TEntity>(string rawSql);
    /// <summary>
    /// 使用原始SQL语句rawSql和参数parameters查询数据，并返回满足条件的第一条记录，记录不存在时返回TEntity类型的默认值，不支持分表
    /// </summary>
    /// <typeparam name="TEntity">实体TEntity类型</typeparam>
    /// <param name="rawSql">原始SQL</param>
    /// <param name="parameters">参数，可以是命名对象、匿名对象或是字典类型对象，不可为null</param>
    /// <returns>返回查询结果，记录不存在时返回TEntity类型的默认值</returns>
    IMultipleQuery QueryFirst<TEntity>(string rawSql, object parameters);
    /// <summary>
    /// 执行原始SQL，并返回影响行数
    /// </summary>
    /// <param name="rawSql">要执行的SQL</param>
    /// <param name="parameters">参数列表，不可为null</param>
    /// <returns>返回查询结果，记录不存在时返回TEntity类型的默认值</returns>
    IMultipleQuery QueryFirst<TEntity>(string rawSql, List<IDbDataParameter> parameters);
    /// <summary>
    /// 从表TEntity中，查询与whereObj对象各属性值都相等的第一条记录，记录不存在时返回TEntity类型的默认值，不支持分表，如：
    /// <code>
    /// f.QueryFirst&lt;User&gt;(new { Id = 1, IsEnabled = true })
    /// SQL: SELECT a.`Id`,a.`Name`, ... FROM `sys_user` a WHERE a.`Id`=@Id AND a.`IsEnabled`=@IsEnabled
    /// </code>
    /// </summary>
    /// <typeparam name="TEntity">实体TEntity类型</typeparam>
    /// <param name="whereObj">参数，可以是命名对象、匿名对象或是字典类型对象，不可为null</param>
    /// <returns>返回查询结果，记录不存在时返回TEntity类型的默认值</returns>
    IMultipleQuery QueryFirst<TEntity>(object whereObj);
    #endregion

    #region Query
    /// <summary>
    /// 使用原始SQL语句rawSql查询数据，并返回满足条件的所有TEntity实体记录，记录不存在时返回没有任何元素的List&lt;TEntity&gt;类型空列表，不支持分表
    /// </summary>
    /// <typeparam name="TEntity">实体TEntity类型</typeparam>
    /// <param name="rawSql">原始SQL</param>
    /// <returns>返回查询结果，记录不存在时返回没有任何元素的List&lt;TEntity&gt;类型空列表</returns>
    IMultipleQuery Query<TEntity>(string rawSql);
    /// <summary>
    /// 使用原始SQL语句rawSql和参数parameters查询数据，并返回满足条件的所有TEntity实体记录，记录不存在时返回没有任何元素的List&lt;TEntity&gt;类型空列表，不支持分表
    /// </summary>
    /// <typeparam name="TEntity">实体TEntity类型</typeparam>
    /// <param name="rawSql">原始SQL</param>
    /// <param name="parameters">参数，可以是命名对象、匿名对象或是字典类型对象，不可为null</param>
    /// <returns>返回查询结果，记录不存在时返回没有任何元素的List&lt;TEntity&gt;类型空列表</returns>
    IMultipleQuery Query<TEntity>(string rawSql, object parameters);
    /// <summary>
    /// 执行原始SQL，并返回影响行数
    /// </summary>
    /// <param name="rawSql">要执行的SQL</param>
    /// <param name="parameters">参数列表，不可为null</param>
    /// <returns>返回查询结果，记录不存在时返回TEntity类型的默认值</returns>
    IMultipleQuery Query<TEntity>(string rawSql, List<IDbDataParameter> parameters);
    /// <summary>
    /// 从表TEntity中，查询与whereObj对象各属性值都相等的所有记录，记录不存在时返回没有任何元素的List&lt;TEntity&gt;类型空列表，不支持分表，如：
    /// <code>
    /// f.Query&lt;User&gt;(new { Id = 1, IsEnabled = true })
    /// SQL: SELECT a.`Id`,a.`Name`, ... FROM `sys_user` a WHERE a.`Id`=@Id AND a.`IsEnabled`=@IsEnabled
    /// </code>
    /// </summary>
    /// <typeparam name="TEntity">实体TEntity类型</typeparam>
    /// <param name="whereObj">参数，可以是命名对象、匿名对象或是字典类型对象，不可为null</param>
    /// <returns>返回查询结果，记录不存在时返回没有任何元素的List&lt;TEntity&gt;类型空列表</returns>
    IMultipleQuery Query<TEntity>(object whereObj);
    #endregion

    #region Exists
    /// <summary>
    /// 判断是否存在，同名属性值作为查询条件，不支持分表，如：.ExistsBy&lt;User&gt;(new { IsEnabled = true })，whereObj不能为null
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，不能为null</param>
    /// <returns>返回是否存在</returns>
    IMultipleQuery ExistsBy<TEntity>(object whereObj);
    /// <summary>
    /// 根据主键判断是否存在，不支持分表，如：.ExistsById&lt;User&gt;(1) 或是 .ExistsById&lt;User&gt;(new { Id = 1 })，whereKey不能为null
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="whereKey">主键值或是包含主键的对象，不能为null</param>
    /// <returns>返回是否存在</returns>
    IMultipleQuery ExistsById<TEntity>(object whereKey);
    /// <summary>
    /// 根据多主键判断是否存在，存在任意一条返回true，不支持分表，如：.ExistsByIds&lt;User&gt;(new int[]{ 1, 2, 3 }) 或是 .ExistsByIds&lt;User&gt;(new []{new { Id = 1 }, new { Id = 2 }, new { Id = 3 } })，whereKeys不能为null
    /// </summary>
    /// <param name="whereKeys">多个主键值或是包含主键的对象集合，不能为null</param>
    /// <returns>返回是否存在</returns>
    IMultipleQuery ExistsByIds<TEntity>(IEnumerable whereKeys);
    /// <summary>
    /// 判断TEntity表是否存在满足wherePredicate条件的记录，存在返回true，否则返回false，不支持分表
    /// </summary>
    /// <typeparam name="TEntity">实体对象类型</typeparam>
    /// <param name="wherePredicate">where条件表达式</param>
    /// <returns>返回是否存在，布尔值</returns>
    IMultipleQuery Exists<TEntity>(Expression<Func<TEntity, bool>> wherePredicate = null);
    #endregion

    #region AddReader/BuildSql
    void AddReader(Type targetType, string sql, ReaderResultType resultType, bool isExists = false, IQueryVisitor visitor = null);
    string BuildSql(out List<ReaderAfter> readerAfters);
    #endregion
}
public class ReaderAfter
{
    public Type TargetType { get; set; }
    public IQueryVisitor Visitor { get; set; }
    public ReaderResultType ResultType { get; set; }
    public bool IsExists { get; set; }
}
public enum ReaderResultType
{
    Value = 1,
    Entity,
    List
}