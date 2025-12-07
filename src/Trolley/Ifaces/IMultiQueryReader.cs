using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public interface IMultiQueryReader : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// 读取单个T类型对象或值，记录不存在时返回T类型的默认值
    /// </summary>
    /// <typeparam name="T">实体类型或是值类型</typeparam>
    /// <returns>返回T类型对象或值，记录不存在时返回T类型的默认值</returns>
    T ReadFirst<T>();
    /// <summary>
    /// 读取T类型对象列表或值列表，记录不存在时返回没有任何元素的空列表
    /// </summary>
    /// <typeparam name="T">实体类型或是值类型</typeparam>
    /// <returns>返回T类型对象列表或值列表，记录不存在时返回没有任何元素的空列表</returns>
    List<T> Read<T>();
    /// <summary>
    /// 读取当前列表，并转化为IPagedListy&lt;T&gt;分页列表
    /// </summary>
    /// <typeparam name="T">实体类型或是值类型</typeparam>
    /// <returns>返回T类型对象分页列表或值分页列表，记录不存在时返回没有任何元素的空分页列表</returns>
    IPagedList<T> ReadPageList<T>();
    /// <summary>
    /// 读取单个T类型对象或值，记录不存在时返回T类型的默认值
    /// </summary>
    /// <typeparam name="T">实体类型或是值类型</typeparam>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回T类型对象或值，记录不存在时返回T类型的默认值</returns>
    Task<T> ReadFirstAsync<T>(CancellationToken cancellationToken = default);
    /// <summary>
    /// 读取T类型对象列表或值列表，记录不存在时返回没有任何元素的空列表
    /// </summary>
    /// <typeparam name="T">实体类型或是值类型</typeparam>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回T类型对象列表或值列表，记录不存在时返回没有任何元素的空列表</returns>
    Task<List<T>> ReadAsync<T>(CancellationToken cancellationToken = default);
    /// <summary>
    /// 读取当前列表，并转化为IPagedListy&lt;T&gt;分页列表
    /// </summary>
    /// <typeparam name="T">实体类型或是值类型</typeparam>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回T类型对象分页列表或值分页列表，记录不存在时返回没有任何元素的空分页列表</returns>
    Task<IPagedList<T>> ReadPageListAsync<T>(CancellationToken cancellationToken = default);
    /// <summary>
    /// 返回当前查询的SQL和参数列表
    /// </summary>
    /// <param name="dbParameters">参数列表</param>
    /// <returns>当前查询的SQL</returns>
    string ToSql(out List<IDbDataParameter> dbParameters);
}