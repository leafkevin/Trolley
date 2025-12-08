using System;
using System.Linq.Expressions;

namespace Trolley.MySqlConnector;

public interface IMySqlContinuedCreate<TEntity> : IContinuedCreate<TEntity>
{
    #region WithBy
    /// <summary>
    /// 单条数据插入，可多次调用，自动增长栏位不需要传入，未列出属性不插入，如：.WithBy(new { Name = "kevin", Age = 25 })
    /// </summary>
    /// <typeparam name="TInsertObject">插入数据的对象类型</typeparam>
    /// <param name="insertObj">插入对象，可以是命名对象、匿名对象或是字典类型对象，未列出属性不插入，不可为null</param>
    /// <returns>返回插入对象</returns>
    new IMySqlContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj);
    /// <summary>
    /// 单条数据插入，可多次调用，自动增长栏位不需要传入，未列出属性不插入，condition为true生效，如：.WithBy(true, new { Gender = Gender.Male, ... })
    /// </summary>
    /// <typeparam name="TInsertObject">插入数据的对象类型</typeparam>
    /// <param name="condition">判断条件，为true时生效</param>
    /// <param name="insertObj">插入对象，可以是命名对象、匿名对象或是字典类型对象，未列出属性不插入，不可为null</param>
    /// <returns>返回插入对象</returns>
    new IMySqlContinuedCreate<TEntity> WithBy<TInsertObject>(bool condition, TInsertObject insertObj);
    /// <summary>
    /// 单个字段插入，可多次调用，如：.WithBy(f =&gt; f.Gender, Gender.Female)
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldSelector">字段选择表达式，只能选择单个字段</param>
    /// <param name="fieldValue">字段值</param>
    /// <returns>返回插入对象</returns>
    new IMySqlContinuedCreate<TEntity> WithBy<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    /// <summary>
    /// 单个字段插入，可多次调用，condition为true生效，如：.WithBy(f =&gt; f.Gender, Gender.Female)
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="condition">判断条件，为true时生效</param>
    /// <param name="fieldSelector">字段选择表达式，只能选择单个字段</param>
    /// <param name="fieldValue">字段值</param>
    /// <returns>返回插入对象</returns>
    new IMySqlContinuedCreate<TEntity> WithBy<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    #endregion

    #region IgnoreFields
    /// <summary>
    /// 忽略字段，实体属性名称，列出属性不插入，如：.IgnoreFields("Name") 或是 .IgnoreFields("Name", "CreatedAt")
    /// </summary>
    /// <param name="fieldNames">实体成员名称数组，不可为null</param>
    /// <returns>返回插入对象</returns>
    new IMySqlContinuedCreate<TEntity> IgnoreFields(params string[] fieldNames);
    /// <summary>
    /// 忽略字段，实体属性名称，列出属性不插入，如：.IgnoreFields("Name") 或是 .IgnoreFields("Name", "CreatedAt")
    /// </summary>
    /// <typeparam name="TFields">字段类型</typeparam>
    /// <param name="fieldsSelector">字段选择表达式，单个或多个字段，不可为null</param>
    /// <returns>返回插入对象</returns>
    new IMySqlContinuedCreate<TEntity> IgnoreFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector);
    #endregion

    #region OnlyFields
    /// <summary>
    /// 插入字段，实体属性名称，未列出属性不插入，如：.OnlyFields("Name") 或是 .OnlyFields("Name", "CreatedAt")
    /// </summary>
    /// <param name="fieldNames">实体成员名称数组，不可为null</param>
    /// <returns>返回插入对象</returns>
    new IMySqlContinuedCreate<TEntity> OnlyFields(params string[] fieldNames);
    /// <summary>
    /// 插入字段，未列出属性不插入，如：.OnlyFields(f =&gt; f.Name) 或是 .OnlyFields(f =&gt; new {f.Name, f.CreatedAt})
    /// </summary>
    /// <typeparam name="TFields">字段类型</typeparam>
    /// <param name="fieldsSelector">字段选择表达式，单个或多个字段，不可为null</param>
    /// <returns>返回插入对象</returns>
    new IMySqlContinuedCreate<TEntity> OnlyFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector);
    #endregion

    #region OnDuplicateKeyUpdate
    /// <summary>
    /// 插入或更新，相同主键或唯一索引存在时执行更新，INSERT INTO ... ON DUPLICATE KEY UPDATE，如：
    /// .OnDuplicateKeyUpdate(x =&gt; x.Set(f =&gt; new { TotalAmount = x.Values(f.TotalAmount) })) 
    /// 或是 .OnDuplicateKeyUpdate(x =&gt; x.Set(f =&gt; f.TotalAmount, f =&gt; f.TotalAmount + 500))
    /// </summary>
    /// <typeparam name="TUpdateFields">要更新的字段类型</typeparam>
    /// <param name="fieldsAssignment">要更新的字段赋值表达式</param>
    /// <returns>返回插入对象</returns>
    IMySqlContinuedCreate<TEntity> OnDuplicateKeyUpdate<TUpdateFields>(Expression<Func<IMySqlCreateDuplicateKeyUpdate<TEntity>, TUpdateFields>> fieldsAssignment);
    #endregion

    #region Returning
    /// <summary>
    /// 返回插入数据，仅mariadb数据库支持
    /// </summary>
    /// <typeparam name="TResult">返回类型</typeparam>
    /// <param name="fieldNames">返回字段列表, 如果有函数调用、表达式或是常量值需要带有AS子句</param>
    /// <returns>返回插入的部分对象值</returns>
    IMySqlCreated<TEntity, TResult> Returning<TResult>(string fieldNames);
    /// <summary>
    /// 返回插入数据，仅mariadb数据库支持
    /// </summary>
    /// <typeparam name="TResult">返回类型</typeparam>
    /// <param name="fieldsSelector">返回字段名称列表</param>
    /// <returns>返回插入的选择字段值</returns>
    IMySqlCreated<TEntity, TResult> Returning<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector);
    #endregion
}
public interface IMySqlBulkContinuedCreate<TEntity> : IContinuedCreate<TEntity>
{
    #region WithBy
    /// 单条数据插入，可多次调用，自动增长栏位不需要传入，未列出属性不插入，如：.WithBy(new { Name = "kevin", Age = 25 })
    /// </summary>
    /// <typeparam name="TInsertObject">插入数据的对象类型</typeparam>
    /// <param name="insertObj">插入对象，可以是命名对象、匿名对象或是字典类型对象，未列出属性不插入，不可为null</param>
    /// <returns>返回插入对象</returns>
    new IMySqlBulkContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj);
    /// <summary>
    /// 单条数据插入，可多次调用，自动增长栏位不需要传入，未列出属性不插入，condition为true生效，如：.WithBy(true, new { Gender = Gender.Male, ... })
    /// </summary>
    /// <typeparam name="TInsertObject">插入数据的对象类型</typeparam>
    /// <param name="condition">判断条件，为true时生效</param>
    /// <param name="insertObj">插入对象，可以是命名对象、匿名对象或是字典类型对象，未列出属性不插入，不可为null</param>
    /// <returns>返回插入对象</returns>
    new IMySqlBulkContinuedCreate<TEntity> WithBy<TInsertObject>(bool condition, TInsertObject insertObj);
    /// <summary>
    /// 单个字段插入，可多次调用，如：.WithBy(f =&gt; f.Gender, Gender.Female)
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldSelector">字段选择表达式，只能选择单个字段</param>
    /// <param name="fieldValue">字段值</param>
    /// <returns>返回插入对象</returns>
    new IMySqlBulkContinuedCreate<TEntity> WithBy<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    /// <summary>
    /// 单个字段插入，可多次调用，condition为true生效，如：.WithBy(f =&gt; f.Gender, Gender.Female)
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="condition">判断条件，为true时生效</param>
    /// <param name="fieldSelector">字段选择表达式，只能选择单个字段</param>
    /// <param name="fieldValue">字段值</param>
    /// <returns>返回插入对象</returns>
    new IMySqlBulkContinuedCreate<TEntity> WithBy<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    #endregion

    #region IgnoreFields
    /// <summary>
    /// 忽略字段，实体属性名称，列出属性不插入，如：.IgnoreFields("Name") 或是 .IgnoreFields("Name", "CreatedAt")
    /// </summary>
    /// <param name="fieldNames">实体成员名称数组，不可为null</param>
    /// <returns>返回插入对象</returns>
    new IMySqlBulkContinuedCreate<TEntity> IgnoreFields(params string[] fieldNames);
    /// <summary>
    /// 忽略字段，列出属性不插入，如：.IgnoreFields(f =&gt; f.Name) 或是 .IgnoreFields(f =&gt; new {f.Name, f.CreatedAt})
    /// </summary>
    /// <typeparam name="TFields">字段类型</typeparam>
    /// <param name="fieldsSelector">字段选择表达式，单个或多个字段，不可为null</param>
    /// <returns>返回插入对象</returns>
    new IMySqlBulkContinuedCreate<TEntity> IgnoreFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector);
    #endregion

    #region OnlyFields
    /// <summary>
    /// 插入字段，实体属性名称，未列出属性不插入，如：.OnlyFields("Name") 或是 .OnlyFields("Name", "CreatedAt")
    /// </summary>
    /// <param name="fieldNames">实体成员名称数组，不可为null</param>
    /// <returns>返回插入对象</returns>
    new IMySqlBulkContinuedCreate<TEntity> OnlyFields(params string[] fieldNames);
    /// <summary>
    /// 插入字段，未列出属性不插入，如：.OnlyFields(f =&gt; f.Name) 或是 .OnlyFields(f =&gt; new {f.Name, f.CreatedAt})
    /// </summary>
    /// <typeparam name="TFields">字段类型</typeparam>
    /// <param name="fieldsSelector">字段选择表达式，单个或多个字段，不可为null</param>
    /// <returns>返回插入对象</returns>
    new IMySqlBulkContinuedCreate<TEntity> OnlyFields<TFields>(Expression<Func<TEntity, TFields>> fieldsSelector);
    #endregion

    #region OnDuplicateKeyUpdate
    /// <summary>
    /// 插入或更新，相同主键或唯一索引存在时执行更新，INSERT INTO ... ON DUPLICATE KEY UPDATE，如：
    /// .OnDuplicateKeyUpdate(x =&gt; x.Set(f =&gt; new { TotalAmount = x.Values(f.TotalAmount) })) 
    /// 或是 .OnDuplicateKeyUpdate(x =&gt; x.Set(f =&gt; f.TotalAmount, f =&gt; f.TotalAmount + 500))
    /// </summary>
    /// <typeparam name="TUpdateFields">要更新的字段类型</typeparam>
    /// <param name="fieldsAssignment">要更新的字段赋值表达式</param>
    /// <returns>返回插入对象</returns>
    IMySqlBulkContinuedCreate<TEntity> OnDuplicateKeyUpdate<TUpdateFields>(Expression<Func<IMySqlCreateDuplicateKeyUpdate<TEntity>, TUpdateFields>> fieldsAssignment);
    #endregion

    #region Returning
    /// <summary>
    /// 返回插入数据，仅mariadb数据库支持
    /// </summary>
    /// <typeparam name="TResult">返回类型</typeparam>
    /// <param name="fieldNames">字段名称列表</param>
    /// <returns>返回插入对象</returns>
    IMySqlBulkCreated<TEntity, TResult> Returning<TResult>(string fieldNames);
    /// <summary>
    /// 返回插入数据，仅mariadb数据库支持
    /// </summary>
    /// <typeparam name="TResult">返回类型</typeparam>
    /// <param name="fieldsSelector">字段筛选表达式</param>
    /// <returns>返回插入对象</returns>
    IMySqlBulkCreated<TEntity, TResult> Returning<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector);
    #endregion
}