using System.Collections.Generic;

namespace Trolley
{
    public class PagedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int PageSize { get; private set; }
    }
}
