using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using netcore_happypath.data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace netcore_happypath.data.Session
{
    public interface INetCoreHappyPathSession : IDisposable
    {
        // Commit
        void CommitChanges();
        Task CommitChangesAsync();
        void ClearContext();

        // Get
        T Single<T, TKey>(Expression<Func<T, bool>> expression)
        where T : class, IBaseEntity<TKey>, new()
        where TKey : struct, IEquatable<TKey>;

        IQueryable<T> All<T, TKey>()
        where T : class, IBaseEntity<TKey>, new()
        where TKey : struct, IEquatable<TKey>;

        // Add or update
        void AddOrUpdate<T, TKey>(T item)
        where T : class, IBaseEntity<TKey>, new()
        where TKey : struct, IEquatable<TKey>;

        void Add<T, TKey>(IEnumerable<T> items)
        where T : class, IBaseEntity<TKey>, new()
        where TKey : struct, IEquatable<TKey>;

        void Update<T, TKey>(IEnumerable<T> items)
        where T : class, IBaseEntity<TKey>, new()
        where TKey : struct, IEquatable<TKey>;

        // Delete
        void Delete<T, TKey>(Expression<Func<T, bool>> expression)
        where T : class, IBaseEntity<TKey>, new()
        where TKey : struct, IEquatable<TKey>;
        void Delete<T, TKey>(T item)
        where T : class, IBaseEntity<TKey>, new()
        where TKey : struct, IEquatable<TKey>;

        void DeleteAll<T, TKey>()
        where T : class, IBaseEntity<TKey>, new()
        where TKey : struct, IEquatable<TKey>;

        void TransactionalDeleteUpdateAdd<T, TKey>(List<TKey> itemIdsToDelete, List<T> itemsToUpdate, List<T> itemsToAdd)
        where T : class, IBaseEntity<TKey>, new()
        where TKey : struct, IEquatable<TKey>;
    }
    public class NetCoreHappyPathSession : INetCoreHappyPathSession
    {
        readonly DbContext _context;
        readonly ILogger<NetCoreHappyPathSession> _logger;
        public NetCoreHappyPathSession(DbContext context, ILogger<NetCoreHappyPathSession> logger)
        {
            _context = context;
            _logger = logger;
        }

        public void TransactionalDeleteUpdateAdd<T, TKey>(List<TKey> itemIdsToDelete, List<T> itemsToUpdate, List<T> itemsToAdd)
        where T : class, IBaseEntity<TKey>, new()
        where TKey : struct, IEquatable<TKey>
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    this.Delete<T, TKey>(x => itemIdsToDelete.Contains(x.Id));

                    itemsToUpdate.ForEach((item) =>
                    {
                        this.UpdateItem<T, TKey>(item);
                    });

                    this.AddItems<T, TKey>(itemsToAdd);

                    _context.SaveChanges();

                    transaction.Commit();
                }
                catch (Exception ee)
                {
                    transaction.Rollback();
                    throw ee;
                }
            }
        }

        public void ClearContext()
        {
            foreach (EntityEntry entry in _context.ChangeTracker.Entries())
            {
                entry.State = EntityState.Unchanged;
            }
        }

        // Commit
        public void CommitChanges()
        {
            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                // if we are catching a
                //if (ex is BadRequestException)
                //{
                throw ex;
                //}

                //// check if we are a validation exception
                //if (ex is DbEntityValidationException)
                //{
                //    List<ListValidationResult> validationResults = new List<ListValidationResult>();

                //    DbEntityValidationException validationException = ex as DbEntityValidationException;

                //    foreach (DbEntityValidationResult result in validationException.EntityValidationErrors)
                //    {
                //        ListValidationResult validationResult = new ListValidationResult();
                //        validationResult.Index = 0;

                //        foreach (DbValidationError error in result.ValidationErrors)
                //        {
                //            validationResult.ValidationResultMessages.Add(error.PropertyName + ": " + error.ErrorMessage);
                //        }

                //        validationResults.Add(validationResult);
                //    }

                //    // translate to a BadRequestException
                //    throw new BadRequestException(ex.Message, validationResults);
                //}

                //// check if we have violated a unique index or foreign key
                //Exception exception = ex;
                //SqlException sqlException = null;

                //while (exception != null)
                //{
                //    sqlException = exception as SqlException;
                //    if (sqlException != null)
                //    {
                //        if (sqlException.Number == 2601)
                //        {
                //            throw new BadRequestException("Duplicate records are not allowed.", BadRequestType.UniqueConstraintViolation);
                //        }

                //        if (sqlException.Number == 547)
                //        {
                //            throw new BadRequestException((sqlException as Exception).FullExceptionMessage(), BadRequestType.ForeignKeyViolation);
                //        }
                //    }

                //    exception = exception.InnerException;
                //}

                //// check to see if we have a concurrency issue
                //exception = ex;
                //OptimisticConcurrencyException concurrencyException = null;

                //while (exception != null)
                //{
                //    concurrencyException = exception as OptimisticConcurrencyException;
                //    if (concurrencyException != null)
                //    {
                //        throw new BadRequestException("The record has been changed.", BadRequestType.ConcurrencyIssue);
                //    }

                //    exception = exception.InnerException;
                //}

                //// log the exception - only if it isn't a recoverable exception
                //_logger.Error(ex, "CommitChanges Fail");

                //ClearContext();

                //// rethrow
                //throw;
            }
        }

        public async Task CommitChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CommitChanges Fail");

                throw;
            }
        }

        // Get
        public T Single<T, TEntityKey>(Expression<Func<T, bool>> expression)
        where T : class, IBaseEntity<TEntityKey>, new()
        where TEntityKey : struct, IEquatable<TEntityKey>
        {
            IQueryable<T> all = All<T, TEntityKey>();
            if (all == null || !all.Any())
            {
                return null;
            }

            return all.FirstOrDefault(expression);
        }

        public IQueryable<T> All<T, TEntityKey>()
        where T : class, IBaseEntity<TEntityKey>, new()
        where TEntityKey : struct, IEquatable<TEntityKey>
        {
            return InternalAll<T, TEntityKey>();
        }

        protected virtual IQueryable<T> InternalAll<T, TEntityKey>()
        where T : class, IBaseEntity<TEntityKey>, new()
        where TEntityKey : struct, IEquatable<TEntityKey>
        {
            return _context.Set<T>().AsQueryable<T>();
        }

        // Add or update
        public void AddOrUpdate<T, TEntityKey>(T item)
        where T : class, IBaseEntity<TEntityKey>, new()
        where TEntityKey : struct, IEquatable<TEntityKey>
        {
            if (item.Id.Equals(default(TEntityKey)))
            {
                AddItem<T, TEntityKey>(item);
            }
            else
            {
                UpdateItem<T, TEntityKey>(item);
            }
        }

        public void Add<T, TEntityKey>(IEnumerable<T> items)
        where T : class, IBaseEntity<TEntityKey>, new()
        where TEntityKey : struct, IEquatable<TEntityKey>
        {
            List<T> itemsToAdd = items.Where(x => x.Id.Equals(default(TEntityKey))).ToList();

            System.Diagnostics.Debug.Assert((itemsToAdd.Count) == items.Count());

            if (itemsToAdd.Any())
            {
                AddItems<T, TEntityKey>(itemsToAdd);
            }
        }

        public void Update<T, TEntityKey>(IEnumerable<T> items)
        where T : class, IBaseEntity<TEntityKey>, new()
        where TEntityKey : struct, IEquatable<TEntityKey>
        {
            List<T> itemsToUpdate = items.Where(x => !x.Id.Equals(default(TEntityKey))).ToList();

            System.Diagnostics.Debug.Assert((itemsToUpdate.Count) == items.Count());

            if (itemsToUpdate.Any())
            {
                UpdateItems<T, TEntityKey>(itemsToUpdate);
            }
        }

        // Private add or update functions
        private void AddItem<T, TEntityKey>(T entity)
        where T : class, IBaseEntity<TEntityKey>
        where TEntityKey : struct, IEquatable<TEntityKey>
        {
            entity.CreateDateTime = DateTime.Now;
            _context.Set<T>().Add(entity);
        }

        private void AddItems<T, TEntityKey>(IEnumerable<T> entities)
        where T : class, IBaseEntity<TEntityKey>
        where TEntityKey : struct, IEquatable<TEntityKey>
        {
            entities.ToList().ForEach(x => x.CreateDateTime = DateTime.Now);
            _context.Set<T>().AddRange(entities);
        }

        private void UpdateItem<T, TEntityKey>(T entity)
        where T : class, IBaseEntity<TEntityKey>
        where TEntityKey : struct, IEquatable<TEntityKey>
        {
            entity.UpdateDateTime = DateTime.Now;

            T original = _context.Set<T>().Find(entity.Id);
            EntityEntry<T> entry = _context.Entry<T>(original);

            this.SetValues<T, TEntityKey>(entity, entry);
        }

        private void UpdateItems<T, TEntityKey>(IEnumerable<T> entities)
        where T : class, IBaseEntity<TEntityKey>
        where TEntityKey : struct, IEquatable<TEntityKey>
        {
            List<EntityEntry<T>> trackedEntities = _context.ChangeTracker.Entries<T>().ToList();
            entities.ToList().ForEach((entity) =>
            {
                entity.UpdateDateTime = DateTime.Now;

                EntityEntry<T> trackedEntity = trackedEntities.First(x => x.Entity.Id.Equals(entity.Id));

                if (trackedEntity == null)
                {
                    T original = _context.Set<T>().Find(entity.Id);
                    trackedEntity = _context.Entry<T>(original);
                }

                this.SetValues<T, TEntityKey>(entity, trackedEntity);
            });
        }

        private void SetValues<T, TEntityKey>(T entity, EntityEntry<T> originalEntity)
        where T : class, IBaseEntity<TEntityKey>
        where TEntityKey : struct, IEquatable<TEntityKey>
        {
            if (originalEntity.State != EntityState.Modified)
            {
                // Preserve the CreateDateTime and CreateUser and never let our values overwrite the originals.
                entity.CreateDateTime = originalEntity.Entity.CreateDateTime;
                entity.CreateUser = originalEntity.Entity.CreateUser;

                originalEntity.CurrentValues.SetValues(entity);
            }
        }

        // Delete
        public void Delete<T, TEntityKey>(Expression<Func<T, bool>> expression)
        where T : class, IBaseEntity<TEntityKey>, new()
        where TEntityKey : struct, IEquatable<TEntityKey>
        {
            var query = All<T, TEntityKey>().Where(expression);
            _context.Set<T>().RemoveRange(query);
        }

        public void Delete<T, TEntityKey>(T item)
        where T : class, IBaseEntity<TEntityKey>, new()
        where TEntityKey : struct, IEquatable<TEntityKey>
        {
            _context.Set<T>().Remove(item);
        }

        public void DeleteAll<T, TEntityKey>()
        where T : class, IBaseEntity<TEntityKey>, new()
        where TEntityKey : struct, IEquatable<TEntityKey>
        {
            var query = All<T, TEntityKey>();
            _context.Set<T>().RemoveRange(query);
        }

        // IDisposable
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
