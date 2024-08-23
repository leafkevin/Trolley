using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

/// <summary>
/// 插入数据
/// </summary>
/// <typeparam name="TEntity">要插入的实体类型</typeparam>
public interface ICreate<TEntity>
{
    #region Properties
    DbContext DbContext { get; }
    ICreateVisitor Visitor { get; }
    #endregion

    #region Sharding
    /// <summary>
    /// 使用固定表名确定TEntity表分表名执行插入操作，完整的表名，如：.UseTable("sys_order_202001")，按月分表
    /// </summary>
    /// <param name="tableName">完整的表名，如：sys_order_202001，按月分表</param>
    /// <returns>返回插入对象</returns>
    ICreate<TEntity> UseTable(string tableName);
    /// <summary>
    /// 根据字段值确定TEntity表分表名执行插入操作，最多支持2个字段，字段值的顺序与配置的字段顺序保持一致，可多次调用
    /// </summary>
    /// <param name="field1Value">字段1值</param>
    /// <param name="field2Value">字段2值</param>
    /// <returns>返回插入对象</returns>
    ICreate<TEntity> UseTableBy(object field1Value, object field2Value = null);
    #endregion

    #region UseTableSchema
    /// <summary>
    /// 切换TableSchema，非默认TableSchema才有效
    /// </summary>
    /// <param name="tableSchema">指定TableSchema</param>
    /// <returns>返回插入对象</returns>
    ICreate<TEntity> UseTableSchema(string tableSchema);
    #endregion

    #region WithBy
    /// <summary>
    /// 使用插入对象部分字段插入，单个对象插入，命名或匿名对象都可以
    /// <para>自动增长的栏位，不需要传入，用法：</para>
    /// <code>
    /// repository.Create&lt;User&gt;()
    ///     .WithBy(new
    ///     {
    ///         Name = "leafkevin",
    ///         Age = 25,
    ///         ...
    ///     })
    ///     .Execute();
    /// SQL: INSERT INTO `sys_user` (`Name`,`Age`, ...) VALUES(@Name,@Age, ...)
    /// </code>
    /// </summary>
    /// <typeparam name="TInsertObject">插入对象类型</typeparam>
    /// <param name="insertObj">插入对象，包含想要插入的必需栏位值，命名或匿名对象都可以</param>
    /// <returns>返回插入对象</returns>
    IContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj);
    #endregion

    #region WithBulk
    /// <summary>
    /// 批量插入,采用多表值方式，生成的SQL:
    /// <code>
    /// INSERT INTO [sys_product] ([ProductNo],[Name], ...) VALUES (@ProductNo0,@Name0, ...),(@ProductNo1,@Name1, ...),(@ProductNo2,@Name2, ...)
    /// </code>
    /// </summary>
    /// <param name="insertObjs">插入的对象集合</param>
    /// <param name="bulkCount">单次插入最多的条数，根据插入对象大小找到最佳的设置阈值，默认值500</param>
    /// <returns>返回插入对象</returns>
    IContinuedCreate<TEntity> WithBulk(IEnumerable insertObjs, int bulkCount = 500);
    #endregion

    #region From
    /// <summary>
    /// 从表T中查询数据创建子查询对象，用法：
    /// <code>
    /// repository.From&lt;Menu&gt;() SQL:FROM `sys_menu`
    /// </code>
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// </param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T> From<T>();
    /// <summary>
    /// 使用2个表创建子查询对象
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2> From<T1, T2>();
    /// <summary>
    /// 使用3个表创建子查询对象
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <typeparam name="T3">表T3实体类型</typeparam>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3> From<T1, T2, T3>();
    /// <summary>
    /// 使用4个表创建子查询对象
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <typeparam name="T3">表T3实体类型</typeparam>
    /// <typeparam name="T4">表T4实体类型</typeparam>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4> From<T1, T2, T3, T4>();
    /// <summary>
    /// 使用5个表创建子查询对象
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <typeparam name="T3">表T3实体类型</typeparam>
    /// <typeparam name="T4">表T4实体类型</typeparam>
    /// <typeparam name="T5">表T5实体类型</typeparam>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>();
    /// <summary>
    /// 使用6个表创建子查询对象
    /// </summary>
    /// <typeparam name="T1">表T1实体类型</typeparam>
    /// <typeparam name="T2">表T2实体类型</typeparam>
    /// <typeparam name="T3">表T3实体类型</typeparam>
    /// <typeparam name="T4">表T4实体类型</typeparam>
    /// <typeparam name="T5">表T5实体类型</typeparam>
    /// <typeparam name="T6">表T6实体类型</typeparam>
    /// <returns>返回查询对象</returns>
    IFromCommand<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>();
    /// <summary>
    /// 使用子查询subQuery作为创建子查询对象，子查询subQuery也可以是CTE表，用法：
    /// <code>
    /// var subQuery = repository.From&lt;Menu&gt;() ... .Select( ...);
    /// repository.Create&lt;Function&gt;(subQuery).Select( ... )
    /// SQL: INSERT INTO `sys_menu` SELECT ... FROM ( ... )
    /// </code>
    /// </summary>
    /// <typeparam name="T">子查询返回的实体类型</typeparam>
    /// <param name="subQuery">子查询</param>
    /// <returns>返回查询对象</returns>
    IFromCommand<T> From<T>(IQuery<T> subQuery);
    #endregion
}
/// <summary>
/// 插入数据
/// </summary>
/// <typeparam name="TEntity">要插入的实体类型</typeparam>
public interface ICreated<TEntity>
{
    #region Properties
    DbContext DbContext { get; }
    ICreateVisitor Visitor { get; }
    #endregion

    #region Execute
    /// <summary>
    /// 执行插入操作，并返回插入行数
    /// </summary>
    /// <returns>返回插入行数</returns>
    int Execute();
    /// <summary>
    /// 执行插入操作，并返回插入行数
    /// </summary>
    /// <param name="cancellationToken">取消token</param>
    /// <returns>返回插入行数</returns>
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
    #endregion

    #region ExecuteIdentity
    /// <summary>
    /// 执行插入操作，并返回自增长主键值
    /// </summary>
    /// <returns>返回自增长主键值</returns>
    int ExecuteIdentity();
    /// <summary>
    /// 执行插入操作，并返回自增长主键值
    /// </summary>
    /// <param name="cancellationToken">取消token</param>
    /// <returns>返回自增长主键值</returns>
    Task<int> ExecuteIdentityAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// 执行插入操作，并返回自增长主键值
    /// </summary>
    /// <returns>返回自增长主键值</returns>
    long ExecuteIdentityLong();
    /// <summary>
    /// 执行插入操作，并返回自增长主键值
    /// </summary>
    /// <param name="cancellationToken">取消token</param>
    /// <returns>返回自增长主键值</returns>
    Task<long> ExecuteIdentityLongAsync(CancellationToken cancellationToken = default);
    #endregion

    #region ToMultipleCommand
    MultipleCommand ToMultipleCommand();
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
/// 插入数据
/// </summary>
/// <typeparam name="TEntity">要插入的实体类型</typeparam>
public interface IContinuedCreate<TEntity> : ICreated<TEntity>
{
    #region WithBy
    /// <summary>
    /// 使用插入对象部分字段插入，单个对象插入，可多次调用，自动增长的栏位，不需要传入，命名或匿名对象、字典都可以，用法：
    /// <code>
    /// repository.Create&lt;User&gt;()
    ///     .WithBy(new { ... })
    ///     .WithBy(new { Name = "kevin", Age = 25 }) ...
    /// SQL: INSERT INTO `sys_user` ( ..., `Name`,`Age`, ... ) VALUES(..., @Name,@Age, ... )
    /// </code>
    /// </summary>
    /// <typeparam name="TInsertObject">插入数据的对象类型</typeparam>
    /// <param name="insertObj">插入数据对象，包含想要插入的必需栏位值</param>
    /// <returns>返回插入对象</returns>
    IContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj);
    /// <summary>
    /// 判断condition布尔值，如果为true，使用插入对象部分字段插入，单个对象插入，可多次调用，自动增长的栏位，不需要传入，用法：
    /// <code>
    /// repository.Create&lt;User&gt;()
    ///     .WithBy(new { Name = "kevin", Age = 25 })
    ///     .WithBy(true, new { Gender = Gender.Male, ... })
    ///     .Execute();
    /// SQL: INSERT INTO `sys_user` (`Name`,`Age`,`Gender`, ... ) VALUES(@Name,@Age,@Gender, ... )
    /// </code>
    /// </summary>
    /// <typeparam name="TInsertObject">插入数据的对象类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="insertObj">插入数据对象，包含想要插入的必需栏位值</param>
    /// <returns>返回插入对象</returns>
    IContinuedCreate<TEntity> WithBy<TInsertObject>(bool condition, TInsertObject insertObj);
    /// <summary>
    /// 单个字段插入，可多次调用，用法：
    /// <code>
    /// repository.Create&lt;User&gt;()
    ///     .WithBy(new { Name = "kevin", Age = 25 })
    ///     .WithBy(f =&gt; f.Gender, Gender.Female)
    ///     ...
    ///     .Execute();
    /// SQL: INSERT INTO `sys_user` (`Name`,`Age`,`Gender`, ... ) VALUES(@Name,@Age,@Gender, ... )
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldSelector">字段选择表达式，只能选择单个字段</param>
    /// <param name="fieldValue">字段值</param>
    /// <returns>返回插入对象</returns>
    IContinuedCreate<TEntity> WithBy<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    /// <summary>
    /// 判断condition布尔值，如果为true，插入fieldSelector字段，为false则不插入，可多次调用，用法：
    /// <code>
    /// repository.Create&lt;User&gt;()
    ///     .WithBy(new { Name = "kevin", Age = 25 })
    ///     .WithBy(true, f =&gt; f.Gender, Gender.Female)
    ///     ...
    ///     .Execute();
    /// SQL: INSERT INTO `sys_user` (`Name`,`Age`,`Gender`, ... ) VALUES(@Name,@Age,@Gender, ... )
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="condition">判断条件</param>
    /// <param name="fieldSelector">字段选择表达式，只能选择单个字段</param>
    /// <param name="fieldValue">字段值</param>
    /// <returns>返回插入对象</returns>
    IContinuedCreate<TEntity> WithBy<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    #endregion

    #region IgnoreFields
    /// <summary>
    /// 忽略字段，实体属性名称，如：IgnoreFields("Name") | IgnoreFields("Name", "CreatedAt")
    /// </summary>
    /// <param name="fieldNames">忽略的字段数组，不可为null</param>
    /// <returns></returns>
    IContinuedCreate<TEntity> IgnoreFields(params string[] fieldNames);
    /// <summary>
    /// 忽略字段，如：IgnoreFields(f =&gt; f.Name) | IgnoreFields(f =&gt; new {f.Name, f.CreatedAt})
    /// </summary>
    /// <typeparam name="TFields">一个或多个字段类型</typeparam>
    /// <param name="fieldsSelector">忽略的字段选择表达式，不可为null</param>
    /// <returns></returns>
    IContinuedCreate<TEntity> IgnoreFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector);
    #endregion

    #region OnlyFields
    /// <summary>
    /// 只插入字段，实体属性名称，如：OnlyFields("Name") | OnlyFields("Name", "CreatedAt")
    /// </summary>
    /// <param name="fieldNames"></param>
    /// <returns></returns>
    IContinuedCreate<TEntity> OnlyFields(params string[] fieldNames);
    /// <summary>
    /// 只插入字段，如：OnlyFields(f =&gt; f.Name) | OnlyFields(f =&gt; new {f.Name, f.CreatedAt})
    /// </summary>
    /// <typeparam name="TFields">一个或多个字段类型</typeparam>
    /// <param name="fieldsSelector">只插入的字段选择表达式，不可为null</param>
    /// <returns></returns>
    IContinuedCreate<TEntity> OnlyFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector);
    #endregion
}
