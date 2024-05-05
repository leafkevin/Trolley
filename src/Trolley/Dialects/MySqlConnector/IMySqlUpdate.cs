using System;
using System.Collections;
using System.Linq.Expressions;

namespace Trolley.MySqlConnector;

public interface IMySqlUpdate<TEntity> : IUpdate<TEntity>
{
    #region InnerJoin
    /// <summary>
    /// InnerJoin内连接表TSource部分字段数据，更新当前表TEntity数据，仅限MySql,MariaDB,PostgreSQL数据库使用
    /// </summary>
    /// <typeparam name="TSource">数据来源表TSource实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回数据更新Join对象</returns>
    IUpdateJoin<TEntity, TSource> InnerJoin<TSource>(Expression<Func<TEntity, TSource, bool>> joinOn);
    #endregion

    #region LeftJoin
    /// <summary>
    /// LeftJoin左连接表TSource部分字段数据，更新当前表TEntity数据，仅限MySql,MariaDB,PostgreSQL数据库使用
    /// </summary>
    /// <typeparam name="TSource">数据来源表TSource实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回数据更新Join对象</returns>
    IUpdateJoin<TEntity, TSource> LeftJoin<TSource>(Expression<Func<TEntity, TSource, bool>> joinOn);
    #endregion

    #region WithBulkCopy
    /// <summary>
    /// 批量更新，采用SqlBulkCopy方式先插入数据到临时表，再Join临时表数据更新，不生成SQL，updateObjs可以不是TEntity类型，只要包含更新字段和主键栏位即可
    /// </summary>
    /// <param name="updateObjs">更新的对象集合，可以不是TEntity类型，包含更新字段和主键栏位即可</param>
    /// <param name="timeoutSeconds">超时时间，单位秒</param>
    /// <returns>返更新对象</returns>
    IMySqlUpdated<TEntity> WithBulkCopy(IEnumerable updateObjs, int? timeoutSeconds = null);
    #endregion
}