using System;
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
    /// <returns>返回数据更新来源对象</returns>
    IUpdateJoin<TEntity, TSource> InnerJoin<TSource>(Expression<Func<TEntity, TSource, bool>> joinOn);
    #endregion

    #region LeftJoin
    /// <summary>
    /// LeftJoin左连接表TSource部分字段数据，更新当前表TEntity数据，仅限MySql,MariaDB,PostgreSQL数据库使用
    /// </summary>
    /// <typeparam name="TSource">数据来源表TSource实体类型</typeparam>
    /// <param name="joinOn">关联条件表达式</param>
    /// <returns>返回数据更新来源对象</returns>
    IUpdateJoin<TEntity, TSource> LeftJoin<TSource>(Expression<Func<TEntity, TSource, bool>> joinOn);
    #endregion
}