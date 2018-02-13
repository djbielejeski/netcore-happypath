using System;
using System.Collections.Generic;
using System.Text;

namespace netcore_happypath.data.Models
{
    public class PagedList<T, TKey>
    {
        public PagedList()
        {
            PageList = new List<T>();
            Ids = new List<TKey>();
            TotalPages = new List<PagedListPage>();
        }

        public List<T> PageList { get; set; }
        public List<TKey> Ids { get; set; }
        public List<PagedListPage> TotalPages { get; set; }
        public long TotalRecordCount { get; set; }
        public int CurrentPage { get; set; }
        public int ResultSize { get; set; }

        public string DisplayString
        {
            get
            {
                string response = "Displaying ";
                if (TotalRecordCount < ResultSize)
                {
                    response += " 1 - " + TotalRecordCount + " of " + TotalRecordCount;
                }
                else
                {
                    response += ((CurrentPage * ResultSize) + 1) + " - ";

                    if (((CurrentPage + 1) * ResultSize) < TotalRecordCount)
                    {
                        response += ((CurrentPage + 1) * ResultSize);
                    }
                    else
                    {
                        response += TotalRecordCount;
                    }

                    response += " of " + TotalRecordCount;
                }

                return response;
            }
        }

        public PagedList<X, TKey> ConvertToNewType<X>()
        {
            PagedList<X, TKey> response = new PagedList<X, TKey>();
            response.CurrentPage = this.CurrentPage;
            response.Ids = this.Ids;
            response.ResultSize = this.ResultSize;
            response.TotalPages = this.TotalPages;
            response.TotalRecordCount = this.TotalRecordCount;

            return response;
        }
    }
}
