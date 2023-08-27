using System.Collections.Generic;

namespace Trolley;

public class PagedList<T> : IPagedList<T>
{
    public int TotalCount { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public List<T> Data { get; set; }
}
