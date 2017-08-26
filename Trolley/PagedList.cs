using System.Collections.Generic;

namespace Trolley
{
    public class PagedList<T>
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int PageTotal { get; set; }
        public int RecordsTotal { get; set; }
        public List<T> Data { get; set; }
    }
}
