using System;
using System.Linq.Expressions;

namespace Trolley.MySqlConnector;

public interface IMySqlContinuedCreate<TEntity> : IContinuedCreate<TEntity>
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
    new IMySqlContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj);
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
    new IMySqlContinuedCreate<TEntity> WithBy<TInsertObject>(bool condition, TInsertObject insertObj);
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
    new IMySqlContinuedCreate<TEntity> WithBy<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    /// <summary>
    /// 忽略字段，实体属性名称，如：IgnoreFields("Name") | IgnoreFields("Name", "CreatedAt")
    /// </summary>
    /// <param name="fieldNames">忽略的字段数组，不可为null</param>
    /// <returns></returns>
    new IMySqlContinuedCreate<TEntity> IgnoreFields(params string[] fieldNames);
    /// <summary>
    /// 忽略字段，如：IgnoreFields(f =&gt; f.Name) | IgnoreFields(f =&gt; new {f.Name, f.CreatedAt})
    /// </summary>
    /// <typeparam name="TFields">一个或多个字段类型</typeparam>
    /// <param name="fieldsSelector">忽略的字段选择表达式，不可为null</param>
    /// <returns></returns>
    new IMySqlContinuedCreate<TEntity> IgnoreFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector);
    /// <summary>
    /// 只插入字段，实体属性名称，如：OnlyFields("Name") | OnlyFields("Name", "CreatedAt")
    /// </summary>
    /// <param name="fieldNames"></param>
    /// <returns></returns>
    new IMySqlContinuedCreate<TEntity> OnlyFields(params string[] fieldNames);
    /// <summary>
    /// 只插入字段，如：OnlyFields(f =&gt; f.Name) | OnlyFields(f =&gt; new {f.Name, f.CreatedAt})
    /// </summary>
    /// <typeparam name="TFields">一个或多个字段类型</typeparam>
    /// <param name="fieldsSelector">只插入的字段选择表达式，不可为null</param>
    /// <returns></returns>
    new IMySqlContinuedCreate<TEntity> OnlyFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector);
    #endregion

    #region OnDuplicateKeyUpdate
    /// <summary>
    /// 相同主键或唯一索引存在时执行更新动作，INSERT INTO ... ON DUPLICATE KEY UPDATE
    /// </summary>
    /// <typeparam name="TUpdateFields">要更新的字段类型</typeparam>
    /// <param name="updateObj">更新实体对象</param>
    /// <returns>返回插入对象</returns>
    IMySqlContinuedCreate<TEntity> OnDuplicateKeyUpdate<TUpdateFields>(TUpdateFields updateObj);
    /// <summary>
    /// 相同主键或唯一索引存在时执行更新动作，INSERT INTO ... ON DUPLICATE KEY UPDATE
    /// </summary>
    /// <typeparam name="TUpdateFields">要更新的字段类型</typeparam>
    /// <param name="fieldsAssignment">要更新的字段赋值表达式</param>
    /// <returns>返回插入对象</returns>
    IMySqlContinuedCreate<TEntity> OnDuplicateKeyUpdate<TUpdateFields>(Expression<Func<IMySqlCreateDuplicateKeyUpdate<TEntity>, TUpdateFields>> fieldsAssignment);
    #endregion
}