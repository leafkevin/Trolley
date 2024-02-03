﻿using System;
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
    IFromCommand<T> From<T>();
    IFromCommand<T1, T2> From<T1, T2>();
    IFromCommand<T1, T2, T3> From<T1, T2, T3>();
    IFromCommand<T1, T2, T3, T4> From<T1, T2, T3, T4>();
    IFromCommand<T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>();
    IFromCommand<T1, T2, T3, T4, T5, T6> From<T1, T2, T3, T4, T5, T6>();
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
    /// 判断condition布尔值，如果为true，使用fieldValue单个字段插入，用法：
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
