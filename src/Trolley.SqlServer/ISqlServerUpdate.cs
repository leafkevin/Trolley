using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trolley.SqlServer;

public interface ISqlServerUpdate<TEntity> : IUpdate<TEntity>
{
    #region From
    /// <summary>
    /// 连接表TSource获取更新数据
    /// </summary>
    /// <typeparam name="TSource">数据来源表TSource实体类型</typeparam>
    /// <returns>返回数据更新来源对象</returns>
    IUpdateFrom<TEntity, TSource> From<TSource>();
    /// <summary>
    /// 使用表T1, T2部分字段数据，更新当前表TEntity数据，仅限Sql Server数据库使用
    /// </summary>
    /// <typeparam name="T1">数据来源表T1实体类型</typeparam>
    /// <typeparam name="T2">数据来源表T2实体类型</typeparam>
    /// <returns>返回数据更新来源对象</returns>
    IUpdateFrom<TEntity, T1, T2> From<T1, T2>();
    /// <summary>
    /// 使用表T1, T2, T3部分字段数据，更新当前表TEntity数据，仅限Sql Server数据库使用
    /// </summary>
    /// <typeparam name="T1">数据来源表T1实体类型</typeparam>
    /// <typeparam name="T2">数据来源表T2实体类型</typeparam>
    /// <typeparam name="T3">数据来源表T3实体类型</typeparam>
    /// <returns>返回数据更新来源对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3> From<T1, T2, T3>();
    /// <summary>
    /// 使用表T1, T2, T3, T4部分字段数据，更新当前表TEntity数据，仅限Sql Server数据库使用
    /// </summary>
    /// <typeparam name="T1">数据来源表T1实体类型</typeparam>
    /// <typeparam name="T2">数据来源表T2实体类型</typeparam>
    /// <typeparam name="T3">数据来源表T3实体类型</typeparam>
    /// <typeparam name="T4">数据来源表T4实体类型</typeparam>
    /// <returns>返回数据更新来源对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3, T4> From<T1, T2, T3, T4>();
    /// <summary>
    /// 使用表T1, T2, T3, T4, T5部分字段数据，更新当前表TEntity数据，仅限Sql Server数据库使用
    /// </summary>
    /// <typeparam name="T1">数据来源表T1实体类型</typeparam>
    /// <typeparam name="T2">数据来源表T2实体类型</typeparam>
    /// <typeparam name="T3">数据来源表T3实体类型</typeparam>
    /// <typeparam name="T4">数据来源表T4实体类型</typeparam>
    /// <typeparam name="T5">数据来源表T5实体类型</typeparam>
    /// <returns>返回数据更新来源对象</returns>
    IUpdateFrom<TEntity, T1, T2, T3, T4, T5> From<T1, T2, T3, T4, T5>();
    #endregion
}
