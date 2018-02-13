using Microsoft.Extensions.Logging;
using netcore_happypath.data.Entities;
using netcore_happypath.data.Models;
using netcore_happypath.data.Session;
using netcore_happypath.data.Utilities;
using netcore_happypath.services.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace netcore_happypath.services.DatabaseActivities
{
    public interface IBaseEntityService<T, TKey>
       where T : class, IBaseEntity<TKey>, new()
       where TKey : struct, IEquatable<TKey>
    {
        IQueryable<T> GetAll<TSort>(Expression<Func<T, bool>> whereExpression, Expression<Func<T, TSort>> sortExpression, ListSortDirection sortDirection);
        IQueryable<T> GetAll<TSort>(Expression<Func<T, TSort>> sortExpression, ListSortDirection sortDirection);
        IQueryable<T> GetAll(Expression<Func<T, bool>> whereExpression, string sortPropertyName, ListSortDirection sortDirection);
        IQueryable<T> GetAll(string sortPropertyName, ListSortDirection sortDirection);
        IQueryable<T> GetAll(Expression<Func<T, bool>> whereExpression);
        IQueryable<T> GetAll(Expression<Func<T, bool>> whereExpression, int count, int pageOffset);
        IQueryable<T> GetAll();
        IQueryable<T> GetAllDescending();
        IQueryable<T> GetAll<TF>(TF filter) where TF : BaseFilter<T, TKey>;

        PagedList<T, TKey> GetAllAsPagedList<TF>(TF filter, bool includeIds) where TF : BaseFilter<T, TKey>;

        T Get(Expression<Func<T, bool>> whereExpression);
        T Get(TKey id);
        void AddOrUpdate(T item);
        void Add(List<T> items);
        void Update(List<T> items);
        void Delete(TKey id);
        void Delete(List<TKey> ids);
        void Delete(Expression<Func<T, bool>> whereExpression);
        int Count();
        int Count(Expression<Func<T, bool>> whereExpression);

        UserPrincipalViewModel CurrentUser { get; }

    }

    public abstract class BaseEntityService<TEntity, TEntityKey> : IBaseEntityService<TEntity, TEntityKey>
        where TEntity : BaseEntity<TEntityKey>, new()
        where TEntityKey : struct, IEquatable<TEntityKey>
    {
        readonly INetCoreHappyPathSession _session;
        readonly ILogger<BaseEntityService<TEntity, TEntityKey>> _logger;
        readonly ICurrentUserService _currentUserService;

        public BaseEntityService(INetCoreHappyPathSession session, ICurrentUserService currentUserService)
            : this(session, currentUserService, null)
        {
        }

        public BaseEntityService(INetCoreHappyPathSession session, ICurrentUserService currentUserService, ILogger<BaseEntityService<TEntity, TEntityKey>> logger)
        {
            _session = session;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public UserPrincipalViewModel CurrentUser
        {
            get { return _currentUserService.CurrentUser; }
        }

        protected abstract bool IsAuthorized(TEntity originalEntity, TEntity newEntity);

        #region Paged List

        public virtual PagedList<TEntity, TEntityKey> GetAllAsPagedList<TFilter>(TFilter filter, bool includeIds)
            where TFilter : BaseFilter<TEntity, TEntityKey>
        {
            IQueryable<TEntity> list = GetAll(filter.Query(), filter.sortBy, filter.sortByDirection);
            return GetAllAsPagedList(list, filter.count, filter.offset, includeIds);
        }

        protected PagedList<TEntity, TEntityKey> GetAllAsPagedList(IQueryable<TEntity> list, int count, int pageOffset, bool includeIds)
        {
            if (pageOffset < 0)
            {
                throw new ArgumentException("Invalid value: '" + pageOffset.ToString() + "'.", "pageOffset");
            }

            long totalCount = list.LongCount();

            List<PagedListPage> totalPages = new List<PagedListPage>();
            if (count > 0)
            {
                int pageNumber = 0;
                for (int page = 0; page < totalCount; page += count)
                {
                    totalPages.Add(new PagedListPage()
                    {
                        PageNumber = pageNumber,
                        PageDisplay = (pageNumber + 1).ToString(),
                        PageRecordCount = (totalCount - page) < count ? (int)(totalCount - page) : count
                    });
                    pageNumber++;
                }
            }

            PagedList<TEntity, TEntityKey> pagedList = new PagedList<TEntity, TEntityKey>();
            if (includeIds)
            {
                pagedList.Ids = list.Select(x => x.Id).ToList();
            }

            // Only skip take if there is a count specified (paging).
            if (count > 0)
            {
                list = list.Skip(pageOffset * count).Take(count);
            }

            pagedList.PageList = list.ToList();
            pagedList.TotalPages = totalPages;
            pagedList.TotalRecordCount = totalCount;
            pagedList.CurrentPage = pageOffset;
            pagedList.ResultSize = count;

            return pagedList;
        }

        #endregion

        #region IQueryables
        public virtual IQueryable<TEntity> GetAll<TFilter>(TFilter filter)
            where TFilter : BaseFilter<TEntity, TEntityKey>
        {
            return GetAll(filter.Query());
        }

        public virtual IQueryable<TEntity> GetAll()
        {
            return GetAll(x => x.Id, ListSortDirection.Ascending);
        }

        public virtual IQueryable<TEntity> GetAllDescending()
        {
            return GetAll(x => x.Id, ListSortDirection.Descending);
        }

        public virtual IQueryable<TEntity> GetAll(Expression<Func<TEntity, bool>> whereExpression)
        {
            return GetAll(whereExpression, x => x.Id, ListSortDirection.Ascending);
        }

        public virtual IQueryable<TEntity> GetAll(Expression<Func<TEntity, bool>> whereExpression, int count, int pageOffset)
        {
            var query = _session.All<TEntity, TEntityKey>().OrderBy(x => x.Id).Where(whereExpression);

            if (count > 0)
            {
                query = query.Skip(count * pageOffset).Take(count);
            }

            return query;
        }

        public virtual IQueryable<TEntity> GetAll<TSort>(Expression<Func<TEntity, TSort>> sortExpression, ListSortDirection sortDirection)
        {
            PropertyInfo sortProperty = PropertyHelper.GetPropertyInfoCompilerProtected<TEntity, TSort>(sortExpression, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            bool isDBPropertyExpression = EntityToSqlScript.IsPropertyDBProperty(sortProperty);

            if (sortDirection == ListSortDirection.Ascending)
            {
                if (isDBPropertyExpression)
                {
                    return _session.All<TEntity, TEntityKey>().OrderBy(sortExpression);
                }
                else
                {
                    List<TEntity> list = _session.All<TEntity, TEntityKey>().ToList();
                    return list.OrderBy(sortExpression.Compile()).AsQueryable();
                }
            }
            else
            {
                if (isDBPropertyExpression)
                {
                    return _session.All<TEntity, TEntityKey>().OrderByDescending(sortExpression);
                }
                else
                {
                    List<TEntity> list = _session.All<TEntity, TEntityKey>().ToList();
                    return list.OrderByDescending(sortExpression.Compile()).AsQueryable();
                }
            }
        }

        public virtual IQueryable<TEntity> GetAll<TSort>(Expression<Func<TEntity, bool>> whereExpression, Expression<Func<TEntity, TSort>> sortExpression, ListSortDirection sortDirection)
        {
            PropertyInfo sortProperty = PropertyHelper.GetPropertyInfoCompilerProtected<TEntity, TSort>(sortExpression, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            bool isDBPropertyExpression = EntityToSqlScript.IsPropertyDBProperty(sortProperty);

            if (sortDirection == ListSortDirection.Ascending)
            {
                if (isDBPropertyExpression)
                {
                    return _session.All<TEntity, TEntityKey>().OrderBy(sortExpression).Where(whereExpression);
                }
                else
                {
                    List<TEntity> list = _session.All<TEntity, TEntityKey>().Where(whereExpression).ToList();
                    return list.OrderBy(sortExpression.Compile()).AsQueryable();
                }
            }
            else
            {
                if (isDBPropertyExpression)
                {
                    return _session.All<TEntity, TEntityKey>().OrderByDescending(sortExpression).Where(whereExpression);
                }
                else
                {
                    List<TEntity> list = _session.All<TEntity, TEntityKey>().Where(whereExpression).ToList();
                    return list.OrderByDescending(sortExpression.Compile()).AsQueryable();
                }
            }
        }

        public virtual IQueryable<TEntity> GetAll(string sortPropertyName, ListSortDirection sortDirection)
        {
            if (string.IsNullOrEmpty(sortPropertyName))
            {
                sortPropertyName = "Id";
            }

            PropertyInfo sortProperty = GetSortPropertyInfo(sortPropertyName);
            bool isDBPropertyExpression = EntityToSqlScript.IsPropertyDBProperty(sortProperty);

            MethodInfo internalGetAllMethod = this.GetType().GetMethod("InternalGetAll", BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo constructedInternalGetAllMethod = internalGetAllMethod.MakeGenericMethod(new Type[] { sortProperty.PropertyType });

            return (IQueryable<TEntity>)constructedInternalGetAllMethod.Invoke(this, new object[] { null, isDBPropertyExpression, sortPropertyName, sortDirection });
        }

        protected IQueryable<TEntity> InternalGetAll<TSort>(Expression<Func<TEntity, bool>> whereExpression, bool isDBPropertyExpression, string sortPropertyName, ListSortDirection sortDirection)
        {
            var param = Expression.Parameter(typeof(TEntity), "x");
            Expression<Func<TEntity, TSort>> sortExpression = GetSortExpression<TSort>(sortPropertyName);

            if (sortDirection == ListSortDirection.Ascending)
            {
                if (isDBPropertyExpression)
                {
                    if (whereExpression != null)
                    {
                        return _session.All<TEntity, TEntityKey>().OrderBy(sortExpression).Where(whereExpression);
                    }
                    else
                    {
                        return _session.All<TEntity, TEntityKey>().OrderBy(sortExpression);
                    }
                }
                else
                {
                    if (whereExpression != null)
                    {
                        List<TEntity> list = _session.All<TEntity, TEntityKey>().Where(whereExpression).ToList();
                        return list.OrderBy(sortExpression.Compile()).AsQueryable();
                    }
                    else
                    {
                        List<TEntity> list = _session.All<TEntity, TEntityKey>().ToList();
                        return list.OrderBy(sortExpression.Compile()).AsQueryable();
                    }
                }
            }
            else
            {
                if (isDBPropertyExpression)
                {
                    if (whereExpression != null)
                    {
                        return _session.All<TEntity, TEntityKey>().OrderByDescending(sortExpression).Where(whereExpression);
                    }
                    else
                    {
                        return _session.All<TEntity, TEntityKey>().OrderByDescending(sortExpression);
                    }
                }
                else
                {
                    if (whereExpression != null)
                    {
                        List<TEntity> list = _session.All<TEntity, TEntityKey>().Where(whereExpression).ToList();
                        return list.OrderByDescending(sortExpression.Compile()).AsQueryable();
                    }
                    else
                    {
                        List<TEntity> list = _session.All<TEntity, TEntityKey>().ToList();
                        return list.OrderByDescending(sortExpression.Compile()).AsQueryable();
                    }
                }
            }
        }

        public virtual IQueryable<TEntity> GetAll(Expression<Func<TEntity, bool>> whereExpression, string sortPropertyName, ListSortDirection sortDirection)
        {
            if (string.IsNullOrEmpty(sortPropertyName))
            {
                sortPropertyName = "Id";
            }

            PropertyInfo sortProperty = GetSortPropertyInfo(sortPropertyName);
            bool isDBPropertyExpression = EntityToSqlScript.IsPropertyDBProperty(sortProperty);

            MethodInfo internalGetAllMethod = this.GetType().GetMethod("InternalGetAll", BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo constructedInternalGetAllMethod = internalGetAllMethod.MakeGenericMethod(new Type[] { sortProperty.PropertyType });

            return (IQueryable<TEntity>)constructedInternalGetAllMethod.Invoke(this, new object[] { whereExpression, isDBPropertyExpression, sortPropertyName, sortDirection });
        }

        private PropertyInfo GetSortPropertyInfo(string sortPropertyName)
        {
            PropertyInfo sortProperty;

            List<string> sortProperties = sortPropertyName.Split('.').ToList();

            if (sortPropertyName.Contains("."))
            {
                // we need to split on the "." and follow the chain down
                Type itemType = (new TEntity()).GetType();

                PropertyInfo innerTypePropertyInfo = itemType.GetProperty(sortProperties[0], BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public);
                sortProperty = innerTypePropertyInfo.PropertyType.GetProperty(sortProperties[1], BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public);
            }
            else
            {
                sortProperty = PropertyHelper.GetPropertyInfoCompilerProtected<TEntity>(sortPropertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            }

            return sortProperty;
        }

        private Expression<Func<TEntity, TSort>> GetSortExpression<TSort>(string sortPropertyName)
        {
            List<string> sortProperties = sortPropertyName.Split('.').ToList();

            var param = Expression.Parameter(typeof(TEntity), "x");
            Expression parent = param;
            sortProperties.ForEach((property) =>
            {
                parent = Expression.Property(parent, property);
            });

            return Expression.Lambda<Func<TEntity, TSort>>(parent, param);
        }

        #endregion

        #region Single Operations

        public TEntity Get(Expression<Func<TEntity, bool>> whereExpression)
        {
            return _session.Single<TEntity, TEntityKey>(whereExpression);
        }

        public virtual TEntity Get(TEntityKey id)
        {
            return _session.Single<TEntity, TEntityKey>(x => x.Id.Equals(id));
        }

        public virtual int Count()
        {
            return _session.All<TEntity, TEntityKey>().Count();
        }

        public virtual int Count(Expression<Func<TEntity, bool>> whereExpression)
        {
            return _session.All<TEntity, TEntityKey>().Where(whereExpression).Count();
        }

        public virtual void AddOrUpdate(TEntity item)
        {
            BaseEntity<TEntityKey>.ThrowAndLogIfNotValid(item, _logger);

            bool isAuthorized = false;

            if (!item.Id.Equals(default(TEntityKey)))
            {
                TEntity originalItem = _session.Single<TEntity, TEntityKey>(x => x.Id.Equals(item.Id));

                isAuthorized = this.IsAuthorized(originalItem, item);

                item.UpdateUser = _currentUserService.CurrentUser.UserId.ToString();
            }
            else
            {
                isAuthorized = this.IsAuthorized(null, item);
                item.CreateUser = _currentUserService.CurrentUser.UserId.ToString();
            }

            if (isAuthorized)
            {
                try
                {
                    _session.AddOrUpdate<TEntity, TEntityKey>(item);
                    _session.CommitChanges();
                }
                catch (Exception ee)
                {
                    _logger.LogError("Error during AddOrUpdate Single<" + typeof(TEntity).Name + ">", ee);

                    throw ee;
                }
            }
            else
            {
                throw new Exception("Unauthorized.");
            }
        }

        public virtual void Add(List<TEntity> items)
        {
            if (items.Any())
            {
                BaseEntity<TEntityKey>.ThrowAndLogIfNotValid(items, _logger);

                if (items.All(x => !x.Id.Equals(default(TEntityKey))))
                {
                    throw new InvalidOperationException("Unable to Add items with ids");
                }

                bool isAuthorized = false;

                foreach (var item in items)
                {
                    isAuthorized = this.IsAuthorized(null, item);

                    if (!isAuthorized)
                    {
                        break;
                    }
                }

                if (isAuthorized)
                {
                    string userName = _currentUserService.CurrentUser.EmailAddress;
                    items.ForEach((item) =>
                    {
                        item.CreateUser = userName;
                    });

                    // Batch this in 5000 chunks
                    int batchSize = 5000;
                    for (var i = 0; i < items.Count; i += batchSize)
                    {
                        var batch = items.Skip(i).Take(batchSize).ToList();

                        try
                        {
                            _session.Add<TEntity, TEntityKey>(batch);
                            _session.CommitChanges();
                        }
                        catch (Exception ee)
                        {
                            _logger.LogError("Error during Add List<" + typeof(TEntity).Name + ">", ee);
                            throw ee;
                        }
                    }
                }
                else
                {
                    throw new Exception("Unauthorized.");
                }
            }
        }

        public virtual void Update(List<TEntity> items)
        {
            if (items.Any())
            {
                BaseEntity<TEntityKey>.ThrowAndLogIfNotValid(items, _logger);

                if (items.All(x => x.Id.Equals(default(TEntityKey))))
                {
                    throw new InvalidOperationException("Unable to Update items without ids");
                }

                // Get all original items
                List<TEntityKey> itemIds = items.Select(x => x.Id).ToList();
                List<TEntity> originalItems = this.GetAll(x => itemIds.Contains(x.Id)).ToList();

                bool isAuthorized = false;
                foreach (var item in items)
                {
                    TEntity originalItem = originalItems.First(x => x.Id.Equals(item.Id));
                    isAuthorized = this.IsAuthorized(originalItem, item);

                    if (!isAuthorized)
                    {
                        break;
                    }
                }

                if (isAuthorized)
                {

                    string userName = _currentUserService.CurrentUser.EmailAddress;

                    try
                    {
                        items.ForEach((item) =>
                        {
                            item.UpdateUser = userName;
                        });

                        _session.Update<TEntity, TEntityKey>(items);
                        _session.CommitChanges();
                    }
                    catch (Exception ee)
                    {
                        _logger.LogError("Error during Update List<" + typeof(TEntity).Name + ">", ee);
                        throw ee;
                    }
                }
                else
                {
                    throw new Exception("Unauthorized");
                }
            }
        }

        public virtual void Delete(Expression<Func<TEntity, bool>> whereExpression)
        {
            try
            {
                List<TEntity> originalItems = this.GetAll(whereExpression).ToList();

                bool isAuthorized = false;
                foreach (var originalItem in originalItems)
                {
                    isAuthorized = this.IsAuthorized(originalItem, null);

                    if (!isAuthorized)
                    {
                        break;
                    }
                }

                if (isAuthorized)
                {

                    _session.Delete<TEntity, TEntityKey>(whereExpression);
                    _session.CommitChanges();
                }
                else
                {
                    throw new Exception("Unauthorized.");
                }
            }
            catch (Exception ee)
            {
                _logger.LogError("Error deleting by Expression<" + typeof(TEntity).Name + ">", ee);

                throw ee;
            }
        }

        public virtual void Delete(TEntityKey id)
        {
            this.Delete(x => x.Id.Equals(id));
        }

        public virtual void Delete(List<TEntityKey> ids)
        {
            this.Delete(x => ids.Contains(x.Id));
        }

        #endregion
    }
}
