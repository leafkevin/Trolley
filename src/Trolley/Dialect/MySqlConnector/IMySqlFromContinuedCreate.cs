using System;
using System.Linq.Expressions;

namespace Trolley.MySqlConnector;

public interface IMySqlFromContinuedCreate<TEntity> : ICreated<TEntity>
{
    #region OnDuplicateKeyUpdate
    /// <summary>
    /// 相同主键或唯一索引存在时执行更新动作，INSERT INTO ... ON DUPLICATE KEY UPDATE
    /// </summary>
    /// <typeparam name="TUpdateFields">要更新的字段类型</typeparam>
    /// <param name="updateObj">更新实体对象</param>
    /// <returns>返回插入对象</returns>
    IMySqlFromContinuedCreate<TEntity> OnDuplicateKeyUpdate<TUpdateFields>(TUpdateFields updateObj);
    /// <summary>
    /// 相同主键或唯一索引存在时执行更新动作，INSERT INTO ... ON DUPLICATE KEY UPDATE
    /// </summary>
    /// <typeparam name="TUpdateFields">要更新的字段类型</typeparam>
    /// <param name="fieldsAssignment">要更新的字段赋值表达式</param>
    /// <returns>返回插入对象</returns>
    IMySqlFromContinuedCreate<TEntity> OnDuplicateKeyUpdate<TUpdateFields>(Expression<Func<IMySqlCreateDuplicateKeyUpdate<TEntity>, TUpdateFields>> fieldsAssignment);
    #endregion

    #region Returning
    /// <summary>
    /// 返回插入后想要返回字段的内容，仅mariadb数据库支持
    /// </summary>
    /// <typeparam name="TResult">返回类型</typeparam>
    /// <param name="fieldNames">字段名称列表</param>
    /// <returns>返回插入的部分字段</returns>
    IMySqlBulkCreated<TEntity, TResult> Returning<TResult>(string fieldNames);
    /// <summary>
    /// 返回插入后想要返回字段的内容，仅mariadb数据库支持
    /// </summary>
    /// <typeparam name="TResult">返回类型</typeparam>
    /// <param name="fieldsSelector">字段筛选表达式</param>
    /// <returns>返回插入的部分字段</returns>
    IMySqlBulkCreated<TEntity, TResult> Returning<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector);
    #endregion
}