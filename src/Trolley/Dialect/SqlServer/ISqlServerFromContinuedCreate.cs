using System;
using System.Linq.Expressions;

namespace Trolley.SqlServer;

public interface ISqlServerFromContinuedCreate<TEntity> : ICreated<TEntity>
{
    #region Output
    /// <summary>
    /// 返回插入后想要返回字段的内容
    /// </summary>
    /// <typeparam name="TResult">返回类型</typeparam>
    /// <param name="fieldNames">字段名称列表</param>
    /// <returns>返回插入的部分字段</returns>
    ISqlServerBulkCreated<TEntity, TResult> Output<TResult>(string fieldNames);
    /// <summary>
    /// 返回插入后想要返回字段的内容
    /// </summary>
    /// <typeparam name="TResult">返回类型</typeparam>
    /// <param name="fieldsSelector">字段筛选表达式</param>
    /// <returns>返回插入的部分字段</returns>
    ISqlServerBulkCreated<TEntity, TResult> Output<TResult>(Expression<Func<TEntity, TResult>> fieldsSelector);
    #endregion
}