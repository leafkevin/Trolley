using System.Collections.Generic;

namespace Trolley;

public class PagedList<T> : IPagedList<T>
{
    /// <summary>
    /// 数据总条数
    /// </summary>
    public int TotalCount { get; set; }
    /// <summary>
    /// 当前页数据条数
    /// </summary>
    public int Count { get; set; }
    /// <summary>
    /// 第几页，从1开始
    /// </summary>
    public int PageNumber { get; set; }
    /// <summary>
    /// 每页条数
    /// </summary>
    public int PageSize { get; set; }
    /// <summary>
    /// 当前页数据
    /// </summary>
    public List<T> Data { get; set; }
}
