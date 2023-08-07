using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Trolley;

public interface IMultiQueryReader : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// 读取单个动态类型对象或值，记录不存在时返回动态类型的默认值
    /// </summary>
    /// <returns>返回动态类型对象或值，记录不存在时返回动态类型的默认值</returns>
    dynamic ReadFirst();
    /// <summary>
    /// 读取单个T类型对象或值，记录不存在时返回T类型的默认值
    /// </summary>
    /// <typeparam name="T">实体类型或是值类型</typeparam>
    /// <returns>返回T类型对象或值，记录不存在时返回T类型的默认值</returns>
    T ReadFirst<T>();
    /// <summary>
    /// 读取动态类型对象列表或值列表，记录不存在时返回没有任何元素的空列表
    /// </summary>
    /// <returns>返回动态类型对象列表或值列表，记录不存在时返回没有任何元素的空列表</returns>
    List<dynamic> Read();
    /// <summary>
    /// 读取T类型对象列表或值列表，记录不存在时返回没有任何元素的空列表
    /// </summary>
    /// <typeparam name="T">实体类型或是值类型</typeparam>
    /// <returns>返回T类型对象列表或值列表，记录不存在时返回没有任何元素的空列表</returns>
    List<T> Read<T>();
    /// <summary>
    /// 读取当前列表，并转化为IPagedListy&lt;dynamic&gt;分页列表
    /// </summary>
    /// <returns>返回动态类型对象分页列表或值分页列表，记录不存在时返回没有任何元素的空分页列表</returns>
    IPagedList<dynamic> ReadPageList();
    /// <summary>
    /// 读取当前列表，并转化为IPagedListy&lt;T&gt;分页列表
    /// </summary>
    /// <typeparam name="T">实体类型或是值类型</typeparam>
    /// <returns>返回T类型对象分页列表或值分页列表，记录不存在时返回没有任何元素的空分页列表</returns>
    IPagedList<T> ReadPageList<T>();
    /// <summary>
    /// 读取当前列表，并转化为Dictionary&lt;TKey, TValue&gt;的字典，记录不存在时返回没有任何元素的空字典
    /// </summary>
    /// <typeparam name="TKey">字典的键，一定要唯一，通常是当前实体类型中的某个唯一字段</typeparam>
    /// <typeparam name="TValue">字典的值，通常是当前实体类型或是其中一部分字段</typeparam>
    /// <returns>返回Dictionary&lt;TKey, TValue&gt;类型字典</returns>
    Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>();
    /// <summary>
    /// 读取单个动态类型对象或值，记录不存在时返回动态类型的默认值
    /// </summary>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回动态类型对象或值，记录不存在时返回动态类型的默认值</returns>
    Task<dynamic> ReadFirstAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// 读取单个T类型对象或值，记录不存在时返回T类型的默认值
    /// </summary>
    /// <typeparam name="T">实体类型或是值类型</typeparam>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回T类型对象或值，记录不存在时返回T类型的默认值</returns>
    Task<T> ReadFirstAsync<T>(CancellationToken cancellationToken = default);
    /// <summary>
    /// 读取动态类型对象列表或值列表，记录不存在时返回没有任何元素的空列表
    /// </summary>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回动态类型对象列表或值列表，记录不存在时返回没有任何元素的空列表</returns>
    Task<List<dynamic>> ReadAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// 读取T类型对象列表或值列表，记录不存在时返回没有任何元素的空列表
    /// </summary>
    /// <typeparam name="T">实体类型或是值类型</typeparam>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回T类型对象列表或值列表，记录不存在时返回没有任何元素的空列表</returns>
    Task<List<T>> ReadAsync<T>(CancellationToken cancellationToken = default);
    /// <summary>
    /// 读取当前列表，并转化为IPagedListy&lt;dynamic&gt;分页列表
    /// </summary>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回动态类型对象分页列表或值分页列表，记录不存在时返回没有任何元素的空分页列表</returns>
    Task<IPagedList<dynamic>> ReadPageListAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// 读取当前列表，并转化为IPagedListy&lt;T&gt;分页列表
    /// </summary>
    /// <typeparam name="T">实体类型或是值类型</typeparam>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回T类型对象分页列表或值分页列表，记录不存在时返回没有任何元素的空分页列表</returns>
    Task<IPagedList<T>> ReadPageListAsync<T>(CancellationToken cancellationToken = default);
    /// <summary>
    /// 读取当前列表，并转化为Dictionary&lt;TKey, TValue&gt;的字典，记录不存在时返回没有任何元素的空字典
    /// </summary>
    /// <typeparam name="TKey">字典的键，一定要唯一，通常是当前实体类型中的某个唯一字段</typeparam>
    /// <typeparam name="TValue">字典的值，通常是当前实体类型或是其中一部分字段</typeparam>
    /// <param name="cancellationToken">取消Token</param>
    /// <returns>返回Dictionary&lt;TKey, TValue&gt;类型字典</returns>
    Task<Dictionary<TKey, TValue>> ReadDictionaryAsync<TKey, TValue>(CancellationToken cancellationToken = default);
}
