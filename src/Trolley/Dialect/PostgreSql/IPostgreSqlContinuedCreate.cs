using System;
using System.Linq.Expressions;

namespace Trolley.PostgreSql;

public interface IPostgreSqlContinuedCreate<TEntity> : IContinuedCreate<TEntity>
{
    #region WithBy
    /// <summary>
    /// 单个字段插入，可多次调用，如：.WithBy(f =&gt; f.Gender, Gender.Female)
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldSelector">字段选择表达式，只能选择单个字段</param>
    /// <param name="fieldValue">字段值</param>
    /// <returns>返回插入对象</returns>
    new IPostgreSqlContinuedCreate<TEntity> WithBy<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    /// <summary>
    /// 单个字段插入，可多次调用，condition为true生效，如：.WithBy(f =&gt; f.Gender, Gender.Female)
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="condition">判断条件，为true时生效</param>
    /// <param name="fieldSelector">字段选择表达式，只能选择单个字段</param>
    /// <param name="fieldValue">字段值</param>
    /// <returns>返回插入对象</returns>
    new IPostgreSqlContinuedCreate<TEntity> WithBy<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    #endregion   

    #region OnConflict
    /// <summary>
    /// 插入或更新，相同主键或唯一索引存在时执行更新动作，也可以忽略插入，INSERT INTO ... ON CONFLICT(...) DO UPDATE，如：
    /// </summary>
    /// <typeparam name="TUpdateFields">要更新的字段类型</typeparam>
    /// <param name="fieldsAssignment">要更新的字段赋值表达式</param>
    /// <returns>返回插入对象</returns>
    IPostgreSqlContinuedCreate<TEntity> OnConflict<TUpdateFields>(Expression<Func<IPostgreSqlCreateConflictDoUpdate<TEntity>, TUpdateFields>> fieldsAssignment);
    #endregion

    #region Returning
    /// <summary>
    /// 返回插入数据
    /// </summary>
    /// <typeparam name="TResult">返回类型</typeparam>
    /// <param name="fieldNames">返回字段列表, 如果有函数调用、表达式或是常量值需要带有AS子句</param>
    /// <returns>返回插入的部分对象值</returns>
    IPostgreSqlCreated<TEntity, TResult> Returning<TResult>(string fieldNames);
    /// <summary>
    /// 返回插入数据
    /// </summary>
    /// <typeparam name="TResult">返回类型</typeparam>
    /// <param name="fieldsSelector">返回字段名称列表</param>
    /// <returns>返回插入的选择字段值</returns>
    IPostgreSqlCreated<TEntity, TResult> Returning<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector);
    #endregion
}
public interface IPostgreSqlBulkContinuedCreate<TEntity> : IBulkContinuedCreate<TEntity>
{
    #region WithBy
    /// 单条数据插入，可多次调用，自动增长栏位不需要传入，未列出属性不插入，如：.WithBy(new { Name = "kevin", Age = 25 })
    /// </summary>
    /// <typeparam name="TInsertObject">插入数据的对象类型</typeparam>
    /// <param name="insertObj">插入对象，可以是命名对象、匿名对象或是字典类型对象，未列出属性不插入，不可为null</param>
    /// <returns>返回插入对象</returns>
    new IPostgreSqlBulkContinuedCreate<TEntity> WithBy<TInsertObject>(TInsertObject insertObj);
    /// <summary>
    /// 单条数据插入，可多次调用，自动增长栏位不需要传入，未列出属性不插入，condition为true生效，如：.WithBy(true, new { Gender = Gender.Male, ... })
    /// </summary>
    /// </summary>
    /// <typeparam name="TInsertObject">插入数据的对象类型</typeparam>
    /// <param name="condition">判断条件，为true时生效</param>
    /// <param name="insertObj">插入对象，可以是命名对象、匿名对象或是字典类型对象，未列出属性不插入，不可为null</param>
    /// <returns>返回插入对象</returns>
    new IPostgreSqlBulkContinuedCreate<TEntity> WithBy<TInsertObject>(bool condition, TInsertObject insertObj);
    /// <summary>
    /// 单个字段插入，可多次调用，如：.WithBy(f =&gt; f.Gender, Gender.Female)
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="fieldSelector">字段选择表达式，只能选择单个字段</param>
    /// <param name="fieldValue">字段值</param>
    /// <returns>返回插入对象</returns>
    new IPostgreSqlBulkContinuedCreate<TEntity> WithBy<TField>(Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    /// <summary>
    /// 单个字段插入，可多次调用，condition为true生效，如：.WithBy(f =&gt; f.Gender, Gender.Female)
    /// </summary>
    /// <typeparam name="TField">字段类型</typeparam>
    /// <param name="condition">判断条件，为true时生效</param>
    /// <param name="fieldSelector">字段选择表达式，只能选择单个字段</param>
    /// <param name="fieldValue">字段值</param>
    /// <returns>返回插入对象</returns>
    new IPostgreSqlBulkContinuedCreate<TEntity> WithBy<TField>(bool condition, Expression<Func<TEntity, TField>> fieldSelector, TField fieldValue);
    #endregion

    #region OnConflict
    /// <summary>
    /// 插入或更新，相同主键或唯一索引存在时执行更新动作，也可以忽略插入，INSERT INTO ... ON CONFLICT(...) DO UPDATE，如：
    /// </summary>
    /// <typeparam name="TUpdateFields">要更新的字段类型</typeparam>
    /// <param name="fieldsAssignment">要更新的字段赋值表达式</param>
    /// <returns>返回插入对象</returns>
    IPostgreSqlBulkContinuedCreate<TEntity> OnConflict<TUpdateFields>(Expression<Func<IPostgreSqlCreateConflictDoUpdate<TEntity>, TUpdateFields>> fieldsAssignment);
    #endregion

    #region Returning
    /// <summary>
    /// 返回插入数据
    /// </summary>
    /// <typeparam name="TResult">返回类型</typeparam>
    /// <param name="fieldNames">字段名称列表</param>
    /// <returns>返回插入对象</returns>
    IPostgreSqlBulkCreated<TEntity, TResult> Returning<TResult>(string fieldNames);
    /// <summary>
    /// 返回插入数据
    /// </summary>
   	/// <typeparam name="TResult">返回类型</typeparam>
    /// <param name="fieldsSelector">字段筛选表达式</param>
    /// <returns>返回插入对象</returns>
    IPostgreSqlBulkCreated<TEntity, TResult> Returning<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector);
    #endregion
}