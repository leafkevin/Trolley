using System.Collections.Generic;

namespace Trolley;

public interface IPagedList<T>
{
    /// <summary>
    /// 数据总条数
    /// </summary>
    int TotalCount { get; set; }
    /// <summary>
    /// 当前页数据条数
    /// </summary>
    int Count { get; set; }
    /// <summary>
    /// 第几页，从1开始
    /// </summary>
    int PageNumber { get; set; }
    /// <summary>
    /// 每页条数
    /// </summary>
    int PageSize { get; set; }
    /// <summary>
    /// 当前页数据
    /// </summary>
    List<T> Data { get; set; }
}