using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

/// <summary>
/// 仓储对象
/// </summary>
public interface IRepository
{
    #region Properties
    /// <summary>
    /// 获取或设置DbContext对象
    /// </summary>
    DbContext DbContext { get; set; }
    #endregion

    #region ShardingTableNames   
    /// <summary>
    /// 在当前数据库中创建实体TEntity的tableName分表，表结构与实体TEntity相同并生成所有索引等信息，fromTableSchema为TEntity表的Schema，为null时是默认当前Schema
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="tableName">分表名称</param>
    /// <param name="fromTableSchema">实体表所在的Schema，为null时是默认当前Schema</param>
    void CreateShardingTable<TEntity>(string tableName, string fromTableSchema = null);
    /// <summary>
    /// 在当前数据库中创建实体TEntity的tableName分表，表结构与实体TEntity相同并生成所有索引等信息，fromTableSchema为TEntity表的Schema，为null时是默认当前Schema
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="tableName">分表名称</param>
    /// <param name="fromTableSchema">实体表所在的TableSchema，为null时是默认当前Schema</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns></returns>
    Task CreateShardingTableAsync<TEntity>(string tableName, string fromTableSchema = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// 根据字段值确定<typeparamref name="TEntity"/>表分表名，字段值的顺序与分表规则设置的顺序保持一致
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="fieldValues">字段值数组，字段值的顺序与分表规则设置的顺序保持一致，不可为null</param>
    /// <returns>返回分表名</returns>
    string GetShardingTableName<TEntity>(params object[] fieldValues);
    /// <summary>
    /// 在当前数据库中创建实体TEntity的tableName分表，根据字段值确定<typeparamref name="TEntity"/>表分表名，字段值的顺序与配置的字段顺序保持一致，表结构与实体TEntity相同并生成所有索引等信息
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="fieldValues">字段值数组，字段值的顺序与分表规则设置的顺序保持一致，不可为null</param>
    /// <param name="fromTableSchema">实体表所在的TableSchema，为null时是默认当前Schema</param>
    void CreateShardingTable<TEntity>(object[] fieldValues, string fromTableSchema = null);
    /// <summary>
    /// 在当前数据库中创建实体TEntity的tableName分表，根据字段值确定<typeparamref name="TEntity"/>表分表名，字段值的顺序与配置的字段顺序保持一致，表结构与实体TEntity相同并生成所有索引等信息
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="fieldValues">字段值数组，字段值的顺序与分表规则设置的顺序保持一致，不可为null</param>
    /// <param name="fromTableSchema">实体表所在的TableSchema，为null时是默认当前Schema</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns></returns>
    Task CreateShardingTableAsync<TEntity>(object[] fieldValues, string fromTableSchema = null, CancellationToken cancellationToken = default);
    #endregion

    #region ShardingDatabase
    /// <summary>
    /// 强制使用主库查询数据，根据选择器依赖参数值确定主库，适用于多主库，多租户、多租户多主库等场景，需要提供连接串选择器依赖参数值，如：租户Id、租户Id+游戏ID...等分库，不传任何值，适用于多主库，则默认使用轮询选择主库
    /// </summary>
    /// <param name="selectorValues">选择器依赖参数值，参数值要与设置的选择器参数相同，多主库场景，可为null，使用轮询选择主库</param>
    /// <returns></returns>
    IRepository UseMaster(params object[] selectorValues);
    /// <summary>
    /// 强制使用从库查询数据，根据选择器依赖参数值确定从库，适用于多从库，多租户多从库等场景，需要提供连接串选择器依赖参数值，如：租户Id、租户Id+游戏ID...等分库，不传任何值，适用于多从库，则默认使用轮询选择从库
    /// </summary>
    /// <param name="selectorValues">选择器依赖参数值，参数值要与设置的选择器参数相同，多从库场景，可为null，使用轮询选择从库</param>
    /// <returns></returns>
    IRepository UseSlave(params object[] selectorValues);
    #endregion

    #region From
    /// <summary>
    /// 从表<typeparamref name="T"/>中查询数据，如：
    /// <code>
    /// repository.From&lt;Menu&gt;()
    /// SQL:FROM `sys_menu`
    /// </code>
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <param name="tableAsStart">表别名起始字母，默认从字母'a'开始</param>
    /// </param>
    /// <returns>返回查询对象</returns>
    IQuery<T> From<T>(char tableAsStart = 'a');
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

    #region FromQuery
    /// <summary>
    /// 从SQL子查询中查询数据，如：
    /// <code>
    /// var subQuery = repository.From&lt;Page, Menu&gt;('o')
    ///     .Where((a, b) =&gt; a.Id == b.PageId)
    ///     .Select((x, y) =&gt; new { y.Id, y.ParentId, x.Url });
    /// repository.FromQuery(subQuery) ...
    /// SQL:
    /// ... FROM (SELECT p.`Id`,p.`ParentId`,o.`Url` FROM `sys_page` o,`sys_menu` p WHERE o.`Id`=p.`PageId`) ...
    /// </code>
    /// </summary>
    /// <typeparam name="T">表T实体类型</typeparam>
    /// <param name="subQuery">子查询</param>
    /// <returns>返回查询对象</returns>
    IQuery<T> FromQuery<T>(IQuery<T> subQuery);
    /// <summary>
    /// 从SQL子查询中查询数据，如：
    /// <code>
    /// repository.FromQuery(f =&gt; f.From&lt;Page, Menu&gt;('o').Where(...)...)
    /// var subQuery = repository.From&lt;Page, Menu&gt;('o').Where(...)...
    /// repository.FromQuery(f =&gt; f.UseQuery(subQuery)) ...    
    /// SQL: ... FROM (SELECT ... FROM `sys_page` o,`sys_menu` p WHERE ...) ...
    /// </code>
    /// </summary>
    /// <typeparam name="T">表T实体类型</typeparam>
    /// <param name="subQueryExpr">子查询表达式</param>
    /// <returns>返回查询对象</returns>
    IQuery<T> FromQuery<T>(Expression<Func<IFromQuery, IQuery<T>>> subQueryExpr);
    #endregion

    #region QueryScalar
    /// <summary>
    /// 指定原始SQL语句，查询单个值
    /// </summary>
    /// <typeparam name="TValue">返回值类型</typeparam>
    /// <param name="commandType">rawSql原始语句的类型，默认是CommandType.Text</param>
    /// <param name="rawSql">原始SQL语句</param>
    /// <returns>返回单个值</returns>
    TValue QueryScalar<TValue>(string rawSql, CommandType commandType = CommandType.Text);
    /// <summary>
    /// 指定原始SQL语句，查询单个值
    /// </summary>
    /// <typeparam name="TValue">返回值类型</typeparam>
    /// <param name="rawSql">原始SQL语句</param>
    /// <param name="commandType">rawSql原始语句的类型，默认是CommandType.Text</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回单个值</returns>
    Task<TValue> QueryScalarAsync<TValue>(string rawSql, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default);
    /// <summary>
    /// 指定原始SQL语句，查询单个值
    /// </summary>
    /// <typeparam name="TValue">返回值类型</typeparam>
    /// <param name="rawSql">原始SQL语句</param>
    /// <param name="parameters">参数，可以是命名对象、匿名对象或是字典类型对象，不可为null</param>
    /// <param name="commandType">rawSql原始语句的类型，默认是CommandType.Text</param>
    /// <returns>返回单个值</returns>
    TValue QueryScalar<TValue>(string rawSql, object parameters, CommandType commandType = CommandType.Text);
    /// <summary>
    /// 指定原始SQL语句，查询单个值
    /// </summary>
    /// <typeparam name="TValue">返回值类型</typeparam>
    /// <param name="rawSql">原始SQL语句</param>
    /// <param name="parameters">参数，可以是命名对象、匿名对象或是字典类型对象，不可为null</param>
    /// <param name="commandType">rawSql原始语句的类型，默认是CommandType.Text</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回单个值</returns>
    Task<TValue> QueryScalarAsync<TValue>(string rawSql, object parameters, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default);
    /// <summary>
    /// 指定原始SQL语句，查询单个值
    /// </summary>
    /// <typeparam name="TValue">返回值类型</typeparam>
    /// <param name="rawSql">原始SQL语句</param>
    /// <param name="parameters">参数数组</param>
    /// <param name="commandType">rawSql原始语句的类型</param>
    /// <returns>返回单个值</returns>
    TValue QueryScalar<TValue>(string rawSql, List<IDbDataParameter> parameters, CommandType commandType = CommandType.Text);
    /// <summary>
    /// 指定原始SQL语句，查询单个值
    /// </summary>
    /// <typeparam name="TValue">返回值类型</typeparam>   
    /// <param name="rawSql">原始SQL语句</param>
    /// <param name="parameters">参数列表，不可为null</param>
    /// <param name="commandType">rawSql原始语句的类型</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回单个值</returns>
    Task<TValue> QueryScalarAsync<TValue>(string rawSql, List<IDbDataParameter> parameters, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default);
    #endregion

    #region QueryById
    /// <summary>
    /// 主键查询，不支持分表，如：.QueryById&lt;User&gt;(1) 或是 .QueryById&lt;User&gt;(new { Id = 1 })，whereKey不能为null
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="whereKey">主键值或是包含主键的对象，不能为null</param>
    /// <returns>返回查询结果，没有数据返回默认值null</returns>
    TEntity QueryById<TEntity>(object whereKey);
    /// <summary>
    /// 主键查询，不支持分表，如：.QueryByIdAsync&lt;User&gt;(1) 或是 .QueryByIdAsync&lt;User&gt;(new { Id = 1 })，whereKey不能为null
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="whereKey">主键值或是包含主键的对象，不能为null</param>   
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回查询结果，没有数据返回默认值null</returns>
    Task<TEntity> QueryByIdAsync<TEntity>(object whereKey, CancellationToken cancellationToken = default);
    #endregion

    #region QueryFirst
    /// <summary>
    /// 原始SQL查询，没有数据返回null，不支持分表
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="rawSql">原始SQL</param>
    /// <param name="commandType">命令类型，默认是文本</param>
    /// <returns>返回查询结果，没有数据返回默认值null</returns>
    TEntity QueryFirst<TEntity>(string rawSql, CommandType commandType = CommandType.Text);
    /// <summary>
    /// 原始SQL查询，没有数据返回null，不支持分表
    /// </summary>
    /// <typeparam name="TEntity">返回的实体类型</typeparam>
    /// <param name="rawSql">查询SQL</param>
    /// <param name="commandType">命令类型，默认是文本</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回查询结果，没有数据返回默认值null</returns>
    Task<TEntity> QueryFirstAsync<TEntity>(string rawSql, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default);
    /// <summary>
    /// 原始SQL查询，同名属性值作为参数，没有数据返回null，不支持分表，如：.QueryFirst&lt;User&gt;("SELECT * FROM sys_user WHERE Name=@Name", new { Name = "kevin" })
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="rawSql">原始SQL</param>
    /// <param name="parameters">参数，可以是命名对象、匿名对象或是字典类型对象，不可为null</param>
    /// <param name="commandType">命令类型，默认是文本</param>
    /// <returns>返回查询结果，没有数据返回默认值null</returns>
    TEntity QueryFirst<TEntity>(string rawSql, object parameters, CommandType commandType = CommandType.Text);
    /// <summary>
    /// 原始SQL查询，同名属性值作为参数，没有数据返回null，不支持分表，如：.QueryFirstAsync&lt;User&gt;("SELECT * FROM sys_user WHERE Name=@Name", new { Name = "kevin" })
    /// </summary>
    /// <typeparam name="TEntity">返回的实体类型</typeparam>
    /// <param name="rawSql">查询SQL</param>
    /// <param name="parameters">参数，可以是命名对象、匿名对象或是字典类型对象，不可为null</param>
    /// <param name="commandType">命令类型，默认是文本</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回查询结果，没有数据返回默认值null</returns>
    Task<TEntity> QueryFirstAsync<TEntity>(string rawSql, object parameters, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default);
    /// <summary>
    /// 原始SQL查询，使用原始参数，没有数据返回null，不支持分表
    /// </summary>
    /// <param name="rawSql">查询SQL</param>
    /// <param name="parameters">参数列表，不可为null</param>
    /// <param name="commandType">rawSql原始语句的类型</param>
    /// <returns>返回查询结果，没有数据返回默认值null</returns>
    TEntity QueryFirst<TEntity>(string rawSql, List<IDbDataParameter> parameters, CommandType commandType = CommandType.Text);
    /// <summary>
    /// 原始SQL查询，使用原始参数，没有数据返回null，不支持分表
    /// </summary>
    /// <param name="rawSql">查询SQL</param>
    /// <param name="parameters">参数列表，不可为null</param>
    /// <param name="commandType">rawSql原始语句的类型</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回查询结果，没有数据返回默认值null</returns>
    Task<TEntity> QueryFirstAsync<TEntity>(string rawSql, List<IDbDataParameter> parameters, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default);
    /// <summary>
    /// 条件查询，同名属性值作为查询条件，没有数据返回null，whereObj为null时返回表中第一条记录，不支持分表，如：.QueryFirst&lt;User&gt;(new { Name = "kevin" })
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="whereObj">条件参数，可以是命名对象、匿名对象或是字典类型对象，可为null</param>
    /// <returns>返回查询结果，没有数据返回默认值null</returns>
    TEntity QueryFirst<TEntity>(object whereObj = null);
    /// <summary>
    /// 条件查询，同名属性值作为查询条件，没有数据返回null，whereObj为null时返回表中第一条记录，不支持分表，如：.QueryFirstAsync&lt;User&gt;(new { Name = "kevin" })
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="whereObj">条件参数，可以是命名对象、匿名对象或是字典类型对象，可为null</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回查询结果，没有数据返回默认值null</returns>
    Task<TEntity> QueryFirstAsync<TEntity>(object whereObj = null, CancellationToken cancellationToken = default);
    #endregion

    #region QueryByIds
    /// <summary>
    /// 多主键查询，不支持分表，如：.QueryByIds&lt;User&gt;(new int[]{ 1, 2, 3 }) 或是 .QueryByIds&lt;User&gt;(new []{new { Id = 1 }, new { Id = 2 }, new { Id = 3 } })，whereKeys不能为null
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="whereKeys">多个主键值或是包含主键的对象集合，不能为null</param>
    /// <returns>返回查询结果，没有数据返回List&lt;<typeparamref name="TEntity"/>&gt;类型空列表</returns>
    List<TEntity> QueryByIds<TEntity>(IEnumerable whereKeys);
    /// <summary>
    /// 多主键查询，不支持分表，如：.QueryByIdsAsync&lt;User&gt;(new int[]{ 1, 2, 3 }) 或是 .QueryByIdsAsync&lt;User&gt;(new []{new { Id = 1 }, new { Id = 2 }, new { Id = 3 } })，whereKeys不能为null
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="whereKeys">多个主键值或是包含主键的对象集合，不能为null</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回查询结果，没有数据返回List&lt;<typeparamref name="TEntity"/>&gt;类型空列表</returns>
    Task<List<TEntity>> QueryByIdsAsync<TEntity>(IEnumerable whereKeys, CancellationToken cancellationToken = default);
    #endregion

    #region Query
    /// <summary>
    /// 原始SQL查询，没有数据返回List&lt;<typeparamref name="TEntity"/>&gt;类型空列表，不支持分表
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="rawSql">原始SQL</param>
    /// <param name="commandType">命令类型，默认是文本</param>
    /// <returns>返回查询结果，没有数据返回List&lt;<typeparamref name="TEntity"/>&gt;类型空列表</returns>
    List<TEntity> Query<TEntity>(string rawSql, CommandType commandType = CommandType.Text);
    /// <summary>
    /// 原始SQL查询，没有数据返回List&lt;<typeparamref name="TEntity"/>&gt;类型空列表，不支持分表
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="rawSql">原始SQL</param>
    /// <param name="commandType">命令类型，默认是文本</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回查询结果，没有数据返回List&lt;<typeparamref name="TEntity"/>&gt;类型空列表</returns>
    Task<List<TEntity>> QueryAsync<TEntity>(string rawSql, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default);
    /// <summary>
    /// 原始SQL查询，同名属性值作为参数，没有数据返回List&lt;<typeparamref name="TEntity"/>&gt;类型空列表，不支持分表，如：.Query&lt;User&gt;("SELECT * FROM sys_user WHERE Name=@Name", new { Name = "kevin" })
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="rawSql">原始SQL</param>
    /// <param name="parameters">参数，可以是命名对象、匿名对象或是字典类型对象，不可为null</param>
    /// <param name="commandType">命令类型，默认是文本</param>
    /// <returns>返回查询结果，没有数据返回List&lt;<typeparamref name="TEntity"/>&gt;类型空列表</returns>
    List<TEntity> Query<TEntity>(string rawSql, object parameters, CommandType commandType = CommandType.Text);
    /// <summary>
    /// 原始SQL查询，同名属性值作为参数，没有数据返回List&lt;<typeparamref name="TEntity"/>&gt;类型空列表，不支持分表，如：.QueryAsync&lt;User&gt;("SELECT * FROM sys_user WHERE Name=@Name", new { Name = "kevin" })
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="rawSql">原始SQL</param>
    /// <param name="parameters">参数，可以是命名对象、匿名对象或是字典类型对象，不可为null</param>
    /// <param name="commandType">命令类型，默认是文本</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回查询结果，没有数据返回List&lt;<typeparamref name="TEntity"/>&gt;类型空列表</returns>
    Task<List<TEntity>> QueryAsync<TEntity>(string rawSql, object parameters, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default);
    /// <summary>
    /// 原始SQL查询，没有数据返回List&lt;<typeparamref name="TEntity"/>&gt;类型空列表，不支持分表
    /// </summary>
    /// <param name="rawSql">原始SQL</param>
    /// <param name="parameters">参数列表，不可为null</param>
    /// <param name="commandType">rawSql原始语句的类型</param>
    /// <returns>返回查询结果，没有数据返回List&lt;<typeparamref name="TEntity"/>&gt;类型空列表</returns>
    List<TEntity> Query<TEntity>(string rawSql, List<IDbDataParameter> parameters, CommandType commandType = CommandType.Text);
    /// <summary>
    /// 原始SQL查询，没有数据返回List&lt;<typeparamref name="TEntity"/>&gt;类型空列表，不支持分表
    /// </summary>
    /// <param name="rawSql">原始SQL</param>
    /// <param name="parameters">参数列表，不可为null</param>
    /// <param name="commandType">rawSql原始语句的类型</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回查询结果，没有数据返回List&lt;<typeparamref name="TEntity"/>&gt;类型空列表</returns>
    Task<List<TEntity>> QueryAsync<TEntity>(string rawSql, List<IDbDataParameter> parameters, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default);
    /// <summary>
    /// 条件查询，同名属性值作为查询条件，没有数据返回List&lt;<typeparamref name="TEntity"/>&gt;类型空列表，whereObj为null时返回表中所有记录，不支持分表，如：.Query&lt;User&gt;(new { Name = "kevin" })
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，可以是命名对象、匿名对象或是字典类型对象，可为null</param>
    /// <returns>返回查询结果，没有数据返回List&lt;<typeparamref name="TEntity"/>&gt;类型空列表</returns>
    List<TEntity> Query<TEntity>(object whereObj = null);
    /// <summary>
    /// 条件查询，同名属性值作为查询条件，没有数据返回List&lt;<typeparamref name="TEntity"/>&gt;类型空列表，whereObj为null时返回表中所有记录，不支持分表，如：.QueryAsync&lt;User&gt;(new { Name = "kevin" })
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，可以是命名对象、匿名对象或是字典类型对象，可为null</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回查询结果，没有数据返回List&lt;<typeparamref name="TEntity"/>&gt;类型空列表</returns>
    Task<List<TEntity>> QueryAsync<TEntity>(object whereObj = null, CancellationToken cancellationToken = default);
    #endregion

    #region Exists
    /// <summary>
    /// 判断是否存在，同名属性值作为查询条件，不支持分表，如：.ExistsBy&lt;User&gt;(new { IsEnabled = true })，whereObj不能为null
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，不能为null</param>
    /// <returns>返回是否存在</returns>
    bool ExistsBy<TEntity>(object whereObj);
    /// <summary>
    /// 判断是否存在，同名属性值作为查询条件，不支持分表，如：.ExistsByAsync&lt;User&gt;(new { IsEnabled = true })，whereObj不能为null
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，不能为null</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回是否存在</returns>
    Task<bool> ExistsByAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default);
    /// <summary>
    /// 根据主键判断是否存在，不支持分表，如：.ExistsById&lt;User&gt;(1) 或是 .ExistsById&lt;User&gt;(new { Id = 1 })，whereKey不能为null
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="whereKey">主键值或是包含主键的对象，不能为null</param>
    /// <returns>返回是否存在</returns>
    bool ExistsById<TEntity>(object whereKey);
    /// <summary>
    /// 根据主键判断是否存在，不支持分表，如：.ExistsByIdAsync&lt;User&gt;(1) 或是 .ExistsByIdAsync&lt;User&gt;(new { Id = 1 })，whereKey不能为null
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="whereKey">主键值或是包含主键的对象，不能为null</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回是否存在</returns>
    Task<bool> ExistsByIdAsync<TEntity>(object whereKey, CancellationToken cancellationToken = default);
    /// <summary>
    /// 根据多主键判断是否存在，存在任意一条返回true，不支持分表，如：.ExistsByIds&lt;User&gt;(new int[]{ 1, 2, 3 }) 或是 .ExistsByIds&lt;User&gt;(new []{new { Id = 1 }, new { Id = 2 }, new { Id = 3 } })，whereKeys不能为null
    /// </summary>
    /// <param name="whereKeys">多个主键值或是包含主键的对象集合，不能为null</param>
    /// <returns>返回是否存在</returns>
    bool ExistsByIds<TEntity>(IEnumerable whereKeys);
    /// <summary>
    /// 根据多主键判断是否存在，存在任意一条返回true，不支持分表，如：.ExistsByIdsAsync&lt;User&gt;(new int[]{ 1, 2, 3 }) 或是 .ExistsByIdsAsync&lt;User&gt;(new []{new { Id = 1 }, new { Id = 2 }, new { Id = 3 } })，whereKeys不能为null
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="whereKeys">多个主键值或是包含主键的对象集合，不能为null</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回是否存在</returns>
    Task<bool> ExistsByIdsAsync<TEntity>(IEnumerable whereKeys, CancellationToken cancellationToken = default);
    #endregion

    #region Create
    /// <summary>
    /// 创建<typeparamref name="entityType"/>类型匿名插入对象
    /// </summary>
    /// <param name="entityType"></param>
    /// <returns>返回插入对象</returns>
    ICreate Create(Type entityType);
    /// <summary>
    /// 创建<typeparamref name="TEntity"/>类型插入对象
    /// </summary>
    /// <typeparam name="TEntity">插入实体类型</typeparam>
    /// <returns>返回插入对象</returns>
    ICreate<TEntity> Create<TEntity>();
    /// <summary>
    /// 单条数据插入，自动增长栏位不需要传入，未列出属性不插入，不支持分表
    /// <code>
    /// .Create&lt;User&gt;(new
    /// {
    ///     Name = "leafkevin",
    ///     Age = 25,
    ///     UpdatedAt = DateTime.Now,
    ///     UpdatedBy = 1
    /// });
    /// SQL: INSERT INTO `sys_user` (`Name`,`Age`,`UpdatedAt`,`UpdatedBy`) VALUES(@Name,@Age,@UpdatedAt,@UpdatedBy)
    /// </code>
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="insertObj">插入对象，可以是命名对象、匿名对象或是字典类型对象，未列出属性不插入，不可为null</param>
    /// <returns>返回插入行数</returns>
    int Create<TEntity>(object insertObj);
    /// <summary>
    /// 单条数据插入，自动增长栏位不需要传入，未列出属性不插入，不支持分表
    /// <code>
    /// .CreateAsync&lt;User&gt;(new
    /// {
    ///     Name = "leafkevin",
    ///     Age = 25,
    ///     UpdatedAt = DateTime.Now,
    ///     UpdatedBy = 1
    /// });
    /// SQL: INSERT INTO `sys_user` (`Name`,`Age`,`UpdatedAt`,`UpdatedBy`) VALUES(@Name,@Age,@UpdatedAt,@UpdatedBy)
    /// </code>
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="insertObj">插入对象，未列出属性不插入，可以是命名对象、匿名对象或是字典类型对象，不可为null</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回插入行数</returns>
    Task<int> CreateAsync<TEntity>(object insertObj, CancellationToken cancellationToken = default);
    /// <summary>
    /// 多条数据插入，自动增长栏位不需要传入，未列出属性不插入，分批次完成，每次插入bulkCount条数，不支持分表
    /// <code>
    /// .Create&lt;Product&gt;(new []{ new { ... }, new { ... }, new { ... });
    /// SQL: INSERT INTO [sys_product] ([ProductNo],[Name],...) VALUES (@ProductNo0,@Name0,...),(@ProductNo1,@Name1,...),(@ProductNo2,@Name2,...)...
    /// </code>
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="insertObjs">插入对象，未列出属性不插入，可以是匿名对象、实体对象、字典等类型的IEnumerable类型，如：数组、列表、集合等</param>
    /// <param name="bulkCount">单次插入最多的条数，根据插入对象大小找到最佳的设置阈值</param>
    /// <returns>返回插入行数</returns>
    int Create<TEntity>(IEnumerable insertObjs, int bulkCount = 500);
    /// <summary>
    /// 多条数据插入，自动增长栏位不需要传入，未列出属性不插入，分批次完成，每次插入bulkCount条数，不支持分表
    /// <code>
    /// .CreateAsync&lt;Product&gt;(new []{ new { ... }, new { ... }, new { ... });
    /// SQL: INSERT INTO [sys_product] ([ProductNo],[Name],...) VALUES (@ProductNo0,@Name0,...),(@ProductNo1,@Name1,...),(@ProductNo2,@Name2,...)...
    /// </code>
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="insertObjs">插入对象，未列出属性不插入，可以是匿名对象、实体对象、字典等类型的IEnumerable类型，如：数组、列表、集合等</param>
    /// <param name="bulkCount">单次插入最多的条数，根据插入对象大小找到最佳的设置阈值</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回插入行数</returns>
    Task<int> CreateAsync<TEntity>(IEnumerable insertObjs, int bulkCount = 500, CancellationToken cancellationToken = default);

    /// <summary>
    /// 单条数据插入，并返回自增长ID，自动增长栏位不需要传入，不支持分表
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="insertObj">插入对象，未列出属性不插入，可以是命名对象、匿名对象或是字典类型对象，不可为null</param>
    /// <returns>返回自增长ID</returns>
    int CreateIdentity<TEntity>(object insertObj);
    /// <summary>
    /// 单条数据插入，并返回自增长ID，自动增长栏位不需要传入，不支持分表
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="insertObj">插入对象，未列出属性不插入，可以是命名对象、匿名对象或是字典类型对象，不可为null</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回自增长ID</returns>
    Task<int> CreateIdentityAsync<TEntity>(object insertObj, CancellationToken cancellationToken = default);
    /// <summary>
    /// 单条数据插入，并返回自增长ID，自动增长栏位不需要传入，不支持分表
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="insertObj">插入对象，未列出属性不插入，可以是命名对象、匿名对象或是字典类型对象，不可为null</param>
    /// <returns>返回自增长ID</returns>
    long CreateIdentityLong<TEntity>(object insertObj);
    /// <summary>
    /// 单条数据插入，并返回自增长ID，自动增长栏位不需要传入，不支持分表
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="insertObj">插入对象，未列出属性不插入，可以是命名对象、匿名对象或是字典类型对象，不可为null</param>
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
    /// 单条数据更新，updateObj对象内除主键字段外与实体同名属性参与更新，必须包含主键字段，不支持分表，如：
    /// <code>.Update&lt;User&gt;(new { Id = 1, Name = "kevin"});
    /// SQL: UPDATE `sys_user` SET `Name`=@Name WHERE `Id`=@Id
    /// </code>
    /// </summary>
    /// <param name="updateObj">更新对象，可以是匿名对象、实体对象、字典</param>
    /// <returns>返回更新行数</returns> 
    int Update<TEntity>(object updateObj);
    /// <summary>
    /// 单条数据更新，updateObj对象内除主键字段外与实体同名属性参与更新，必须包含主键字段，不支持分表，如：
    /// <code>
    /// repository.Update&lt;User&gt;(new { Id = 1, Name = "kevin"});
    /// SQL: UPDATE `sys_user` SET `Name`=@Name WHERE `Id`=@Id
    /// </code>
    /// </summary>
    /// <param name="updateObj">更新对象，可以是匿名对象、实体对象、字典</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回更新行数</returns>
    Task<int> UpdateAsync<TEntity>(object updateObj, CancellationToken cancellationToken = default);
    /// <summary>
    /// 多条数据更新，updateObjs单条对象内除主键字段外与实体同名属性参与更新，必须包含主键字段，分批次完成更新，每次更新bulkCount条数据，不支持分表，如：
    /// <code>
    /// repository.UpdateAsync&lt;User&gt;(new [] { new { Id = 1, Name = "kevin"}, new { Id = 2, Name = "cindy"} }, 200);
    /// SQL: UPDATE `sys_user` SET `Name`=@Name0 WHERE `Id`=@Id0;UPDATE `sys_user` SET `Name`=@Name1 WHERE `Id`=@Id1
    /// </code>
    /// </summary>
    /// <param name="updateObjs">更新对象，可以是匿名对象、实体对象、字典等类型的IEnumerable类型，如：数组、列表、集合等</param>
    /// <param name="bulkCount">单次插入最多的条数，根据插入对象大小找到最佳的设置阈值</param>
    /// <returns>返回更新行数</returns> 
    int Update<TEntity>(IEnumerable updateObjs, int bulkCount);
    /// <summary>
    /// 多条数据更新，updateObjs单条对象内除主键字段外与实体同名属性参与更新，必须包含主键字段，分批次完成更新，每次更新bulkCount条数据，不支持分表，如：
    /// <code>
    /// repository.UpdateAsync&lt;User&gt;(new [] { new { Id = 1, Name = "kevin"}, new { Id = 2, Name = "cindy"} }, 200);
    /// SQL: UPDATE `sys_user` SET `Name`=@Name0 WHERE `Id`=@Id0;UPDATE `sys_user` SET `Name`=@Name1 WHERE `Id`=@Id1
    /// </code>
    /// </summary>
    /// <param name="updateObjs">更新对象，可以是匿名对象、实体对象、字典等类型的IEnumerable类型，如：数组、列表、集合等</param>
    /// <param name="bulkCount">单次插入最多的条数，根据插入对象大小找到最佳的设置阈值</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回更新行数</returns>
    Task<int> UpdateAsync<TEntity>(IEnumerable updateObjs, int bulkCount, CancellationToken cancellationToken = default);
    #endregion

    #region Delete
    /// <summary>
    /// 创建<typeparamref name="TEntity"/>类型删除对象
    /// </summary>
    /// <typeparam name="TEntity">删除实体类型</typeparam>
    /// <returns>返回删除对象</returns>
    IDelete<TEntity> Delete<TEntity>();
    /// <summary>
    /// 条件删除，同名属性值作为查询条件，不支持分表，如：.DeleteBy&lt;User&gt;(new { IsEnabled = true })，whereObj不能为null
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，不能为null</param>
    /// <returns>返回删除行数</returns>
    int DeleteBy<TEntity>(object whereObj);
    /// <summary>
    /// 条件删除，同名属性值作为查询条件，不支持分表，如：.DeleteByAsync&lt;User&gt;(new { IsEnabled = true })，whereObj不能为null
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="whereObj">条件对象，同名属性值作为查询条件，不能为null</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回删除行数</returns>
    Task<int> DeleteByAsync<TEntity>(object whereObj, CancellationToken cancellationToken = default);
    /// <summary>
    /// 主键条件删除，不支持分表，如：.DeleteById&lt;User&gt;(1) 或是 .DeleteById&lt;User&gt;(new { Id = 1 })，whereKey不能为null
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="whereKey">主键值或是包含主键的对象，不能为null</param>
    /// <returns>返回删除行数</returns>
    int DeleteById<TEntity>(object whereKey);
    /// <summary>
    /// 主键条件删除，不支持分表，如：.DeleteByIdAsync&lt;User&gt;(1) 或是 .DeleteByIdAsync&lt;User&gt;(new { Id = 1 })，whereKey不能为null
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="whereKey">主键值或是包含主键的对象，不能为null</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回删除行数</returns>
    Task<int> DeleteByIdAsync<TEntity>(object whereKey, CancellationToken cancellationToken = default);
    /// <summary>
    /// 多主键条件删除，不支持分表，如：.DeleteByIds&lt;User&gt;(new int[]{ 1, 2, 3 }) 或是 .DeleteByIds&lt;User&gt;(new []{new { Id = 1 }, new { Id = 2 }, new { Id = 3 } })，whereKeys不能为null
    /// </summary>
    /// <param name="whereKeys">多个主键值或是包含主键的对象集合，不能为null</param>
    /// <returns>返回删除行数</returns>
    int DeleteByIds<TEntity>(IEnumerable whereKeys);
    /// <summary>
    /// 多主键条件删除，不支持分表，如：.DeleteByIdsAsync&lt;User&gt;(new int[]{ 1, 2, 3 }) 或是 .DeleteByIdsAsync&lt;User&gt;(new []{new { Id = 1 }, new { Id = 2 }, new { Id = 3 } })，whereKeys不能为null
    /// </summary>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <param name="whereKeys">多个主键值或是包含主键的对象集合，不能为null</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回删除行数</returns>
    Task<int> DeleteByIdsAsync<TEntity>(IEnumerable whereKeys, CancellationToken cancellationToken = default);
    #endregion

    #region Execute
    /// <summary>
    /// 执行原始SQL，并返回影响行数
    /// </summary>
    /// <param name="rawSql">要执行的SQL</param>
    /// <param name="parameters">SQL中使用的参数，可以是已有对象、匿名对象或是字典类型对象，可以为null</param>
    /// <param name="commandType">命令类型，默认是文本</param>
    /// <returns>返回影响行数</returns>
    int Execute(string rawSql, object parameters = null, CommandType commandType = CommandType.Text);
    /// <summary>
    /// 执行原始SQL，并返回影响行数
    /// </summary>
    /// <param name="rawSql">要执行的SQL</param>
    /// <param name="parameters">SQL中使用的参数，可以是已有对象、匿名对象或是字典类型对象，可以为null</param>
    /// <param name="commandType">命令类型，默认是文本</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回影响行数</returns>
    Task<int> ExecuteAsync(string rawSql, object parameters = null, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default);
    /// <summary>
    /// 执行原始SQL，并返回影响行数
    /// </summary>
    /// <param name="commandType">命令类型</param>
    /// <param name="rawSql">要执行的SQL</param>
    /// <param name="parameters">参数数组</param>
    /// <returns>返回影响行数</returns>
    int Execute(string rawSql, List<IDbDataParameter> parameters, CommandType commandType = CommandType.Text);
    /// <summary>
    /// 执行原始SQL，并返回影响行数
    /// </summary>
    /// <param name="commandType">命令类型</param>
    /// <param name="rawSql">要执行的SQL</param>
    /// <param name="parameters">参数数组</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回影响行数</returns>
    Task<int> ExecuteAsync(string rawSql, List<IDbDataParameter> parameters, CommandType commandType = CommandType.Text, CancellationToken cancellationToken = default);
    #endregion

    #region QueryMultiple
    /// <summary>
    /// 使用IMultipleQuery操作生成多个SQL语句一起执行，并返回多个结果集，根据IMultipleQuery操作顺序接收返回结果。
    /// </summary>
    /// <param name="subQueries">多个SQL查询操作，不可为null</param>
    /// <returns>返回多结果集Reader对象</returns>
    IMultiQueryReader QueryMultiple(Action<IMultipleQuery> subQueries);
    /// <summary>
    /// 使用IMultipleQuery操作生成多个SQL语句一起执行，并返回多个结果集，根据IMultipleQuery操作顺序接收返回结果。
    /// </summary>
    /// <param name="subQueries">多个SQL查询操作，不可为null</param>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回多结果集Reader对象</returns>
    Task<IMultiQueryReader> QueryMultipleAsync(Action<IMultipleQuery> subQueries, CancellationToken cancellationToken = default);
    #endregion

    #region Transaction
    void BeginTransaction();
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    void Commit();
    Task CommitAsync(CancellationToken cancellationToken = default);
    void Rollback();
    Task RollbackAsync(CancellationToken cancellationToken = default);
    #endregion

    #region Other
    IRepository WithTimeout(int seconds);
    #endregion
}