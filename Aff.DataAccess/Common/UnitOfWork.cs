using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Transactions;

namespace Aff.DataAccess.Common
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly TimaAffiliateEntities _context;
        private readonly Dictionary<Type, object> _repository;
        private bool _disposed;

        public UnitOfWork(TimaAffiliateEntities entities)
        {
            _context = entities;
            _context.Configuration.ProxyCreationEnabled = false;
            _repository = new Dictionary<Type, object>();
            _disposed = false;
        }

        /// <summary>
        /// Function us to Get instance of a Object on Database
        /// </summary>
        /// <typeparam name="TEntity">Object is target</typeparam>
        /// <returns></returns>
        public IBaseRepository<TEntity> GetRepository<TEntity>() where TEntity : class
        {
            //Check if the Dictionary key contains the Model class
            if (_repository.Keys.Contains(typeof(TEntity)))
            {
                return _repository[typeof(TEntity)] as IBaseRepository<TEntity>;
            }
            // If the repository for that Model class doesn't exist, create it
            var repository = new BaseRepository<TEntity>(_context);

            _repository.Add(typeof(TEntity), repository);
            return repository;
        }

        public void TransactionSaveChanges()
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required,
                                         new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                try
                {
                    _context.SaveChanges();
                    scope.Complete();
                }
                catch (Exception ex)
                {
                    scope.Dispose();
                    throw ex;
                }
            }
        }

        /// <summary>
        /// Function use to Save all Object is changed into Database 
        /// </summary>
        public void SaveChanges()
        {
            try
            {
                _context.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                var newException = new FormattedDbEntityValidationException(_context, e);
                throw new Exception(newException.Message);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
