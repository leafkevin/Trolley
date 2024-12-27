using System;
using System.Linq.Expressions;

namespace Trolley.Sqlite;

public interface ISqliteFromContinuedCreate<TEntity> : ICreated<TEntity>
{
    #region Returning
    /// <summary>
    /// 返回插入后想要返回字段的内容，仅mariadb数据库支持
    /// </summary>
    /// <typeparam name="TResult">返回类型</typeparam>
    /// <param name="fieldNames">字段名称列表</param>
    /// <returns>返回插入的部分字段</returns>
    ISqliteBulkCreated<TEntity, TResult> Returning<TResult>(string fieldNames);
    /// <summary>
    /// 返回插入后想要返回字段的内容，仅mariadb数据库支持
    /// </summary>
    /// <typeparam name="TResult">返回类型</typeparam>
    /// <param name="fieldsSelector">字段筛选表达式</param>
    /// <returns>返回插入的部分字段</returns>
    ISqliteBulkCreated<TEntity, TResult> Returning<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector);
    #endregion
}