using System.Collections.Generic;

namespace Trolley;

public interface IPagedList<T>
{
    int RecordsTotal { get; }
    List<T> Items { get; }
}
