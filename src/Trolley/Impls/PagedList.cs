using System.Collections.Generic;

namespace Trolley;

public class PagedList<T> : IPagedList<T>
{
    public int RecordsTotal { get; set; }
    public List<T> Items { get; set; }
    public PagedList() { }
    public PagedList(int recordsTotal, List<T> items)
    {
        RecordsTotal = recordsTotal;
        Items = items;
    }
}
