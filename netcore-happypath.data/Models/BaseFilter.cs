using LinqKit;
using netcore_happypath.data.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Text;

namespace netcore_happypath.data.Models
{
    public abstract class BaseFilter<T, TKey>
        where T : class, IBaseEntity<TKey>, new()
        where TKey : struct, IEquatable<TKey>
    {
        public BaseFilter()
        {
            descending = false;
            offset = 0;
            count = 10000;
        }

        public int offset { get; set; }
        public int count { get; set; }
        public string sortBy { get; set; }
        public bool descending { get; set; }
        public ListSortDirection sortByDirection
        {
            get
            {
                return descending ? ListSortDirection.Descending : ListSortDirection.Ascending;
            }
        }


        protected abstract void InternalQuery(ref Expression<Func<T, bool>> predicate);

        public Expression<Func<T, bool>> Query()
        {
            Expression<Func<T, bool>> predicate = PredicateBuilder.New<T>(true);

            this.InternalQuery(ref predicate);

            return predicate.Expand();
        }
    }
}
