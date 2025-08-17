using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Trolley;

public interface IMultipleQuery : IDisposable
{
    #region Properties
    DbContext DbContext { get; }
    IDbCommand Command { get; }
    List<ReaderAfter> ReaderAfters { get; }
    #endregion

    #region GetShardingTableNames
    /// <summary>
    /// 获取实体<typeparamref name="TEntity"/>满足条件的所有分表名
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="tableNameSelector">分表名选择表达式</param>
    /// <param name="tableSchema">分表所在的TableSchema</param>
    /// <returns>返回满足条件的所有分表</returns>
    IMultipleQuery GetShardingTableNames<TEntity>(Func<string, bool> tableNameSelector = null, string tableSchema = null);
    /// <summary>
    /// 根据字段值确定<typeparamref name="TEntity"/>表分表名，最多支持3个字段值，字段值的顺序与分表规则设置的顺序保持一致
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="fieldValues">字段值数组，字段值的顺序与分表规则设置的顺序保持一致，不可为null</param>
    /// <returns>返回满足条件的分表名</returns>
    IMultipleQuery GetShardingTableNameBy<TEntity>(params object[] fieldValues);
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
    /// <param name="parameters">参数，可以是命名对象、匿名对象或是Dictionary类型对象，不可为null</param>
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

    #region GetById
    /// <summary>
    /// 根据主键信息查询表TEntity中数据，记录不存在时返回TEntity类型的默认值，不支持分表，如：
    /// <code>
    /// f.GetById&lt;User&gt;(1) //或是
    /// f.GetById&lt;User&gt;(new { Id = 1 }) //或是
    /// var userInfo = new UserInfo { Id = 1, Name = "xxx" ... };
    /// f.GetById&lt;User&gt;(userInfo) //三种写法是等效的
    /// SQL: SELECT ... FROM `sys_user` a WHERE a.`Id`=@Id
    /// </code>
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="whereObj">主键值或是包含主键的匿名对象或是已有对象，如：1，2或new { Id = 1}或是已有对象userInfo(包含主键栏位Id) </param>
    /// <returns>返回实体对象或是TEntity类型默认值</returns>
    IMultipleQuery GetById<TEntity>(object whereObj);
    #endregion

    #region GetByIds
    /// <summary>
    /// 根据多个主键信息查询表TEntity中数据，记录不存在时返回没有任何元素的List&lt;TEntity&gt;类型空列表，不支持分表，如：
    /// <code>
    /// f.GetByIds&lt;User&gt;(new []{1 ,2, 3}) //或是
    /// f.GetByIds&lt;User&gt;(new []{{ Id = 1 }, { Id = 2 }, { Id = 3 }}) //或是
    /// var userInfo = new UserInfo { Id = 1, Name = "xxx" ... };
    /// f.GetByIds&lt;User&gt;(new List&lt;UserInfo&gt;{userInfo}) //三种写法是等效的
    /// SQL: SELECT ... FROM `sys_user` a WHERE a.`Id` in (@Id0,@Id1,@Id2)
    /// </code>
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="whereKeys">主键值或是包含主键的匿名对象或是已有对象，如：1，2或new { Id = 1}或是已有对象userInfo(包含主键栏位Id) </param>
    /// <returns>返回查询结果，记录不存在时返回没有任何元素的List&lt;TEntity&gt;类型空列表</returns>
    IMultipleQuery GetByIds<TEntity>(IEnumerable whereKeys);
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
    /// <param name="parameters">参数，可以是命名对象、匿名对象或是Dictionary类型对象，不可为null</param>
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
    /// <param name="whereObj">参数，可以是命名对象、匿名对象或是Dictionary类型对象，不可为null</param>
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
    /// <param name="parameters">参数，可以是命名对象、匿名对象或是Dictionary类型对象，不可为null</param>
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
    /// <param name="whereObj">参数，可以是命名对象、匿名对象或是Dictionary类型对象，不可为null</param>
    /// <returns>返回查询结果，记录不存在时返回没有任何元素的List&lt;TEntity&gt;类型空列表</returns>
    IMultipleQuery Query<TEntity>(object whereObj);
    #endregion

    #region Exists
    /// <summary>
    /// 判断是否存在表TEntity中满足与whereObj对象各属性值都相等的记录，存在返回true，否则返回false，不支持分表
    /// <code>
    /// f.Exists&lt;User&gt;(new { Id = 1, IsEnabled = true })
    /// SQL: SELECT COUNT(1) FROM `sys_user` WHERE `Id`=@Id AND `IsEnabled`=@IsEnabled
    /// </code>
    /// </summary>
    /// <typeparam name="TEntity">实体对象类型</typeparam>
    /// <param name="whereObj">where条件对象，whereObj对象各属性值都参与相等比较,推荐使用匿名对象</param>
    /// <returns>返回是否存在，布尔值</returns>
    IMultipleQuery Exists<TEntity>(object whereObj);
    /// <summary>
    /// 判断TEntity表是否存在满足wherePredicate条件的记录，存在返回true，否则返回false，不支持分表
    /// </summary>
    /// <typeparam name="TEntity">实体对象类型</typeparam>
    /// <param name="wherePredicate">where条件表达式</param>
    /// <returns>返回是否存在，布尔值</returns>
    IMultipleQuery Exists<TEntity>(Expression<Func<TEntity, bool>> wherePredicate = null);
    #endregion

    #region AddReader/BuildSql
    void AddReader(Type targetType, string sql, bool isSingle, IQueryVisitor queryVisitor = null, int pageNumber = 0, int pageSize = 0);
    string BuildSql(out List<ReaderAfter> readerAfters);
    #endregion
}
public class ReaderAfter
{
    public Type TargetType { get; set; }
    public IQueryVisitor QueryVisitor { get; set; }
    public bool IsSingle { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}