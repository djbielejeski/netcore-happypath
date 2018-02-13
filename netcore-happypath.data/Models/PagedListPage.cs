using System;
using System.Collections.Generic;
using System.Text;

namespace netcore_happypath.data.Models
{
    public class PagedListPage
    {
        public int PageNumber { get; set; }
        public string PageDisplay { get; set; }
        public int PageRecordCount { get; set; }
    }
}
