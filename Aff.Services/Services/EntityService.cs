using Aff.DataAccess.Common;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Aff.Services
{
    public interface IEntityService<T>
    {
        /// <summary>
        /// Function use to get Object flow Id
        /// </summary>
        /// <param name="id">Primary key of Table current</param>
        /// <returns></returns>
        T GetById(int id);

        /// <summary>
        /// Get All list Object
        /// </summary>
        /// <returns></returns>
        IEnumerable<T> GetAll();

        /// <summary>
        /// Function use to Update Object 
        /// </summary>
        /// <param name="entity">Object is targer Update</param>
        /// <returns></returns>
        T Update(T entity);


        /// <summary>
        /// Function use to Insert Object 
        /// </summary>
        /// <param name="entity">Object is targer Update</param>
        /// <returns></returns>
        T Insert(T entity);

        /// <summary>
        /// Inserts the multiple entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        List<T> InsertMulti(List<T> entity);

        /// <summary>
        /// Function use to Remove Object in Database
        /// </summary>
        /// <param name="entity">Object is targer Update</param>
        /// <returns></returns>
        bool Delete(T entity);

        /// <summary>
        /// Function use to Remove Object in Database
        /// </summary>
        /// <param name="id">Id is identity</param>
        /// <returns></returns>
        bool Delete(dynamic id);

        /// <summary>
        /// Deletes the mullti.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        bool DeleteMulti(List<T> entity);

        T Find(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includes);

        List<T> FindAll(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includes);
    }


    public class EntityService<T> : IEntityService<T> where T : class
    {
        protected readonly IUnitOfWork UnitOfWork;
        readonly IBaseRepository<T> _repository;
        //protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        protected EntityService(IUnitOfWork unitOfWork, IBaseRepository<T> repository)
        {
            UnitOfWork = unitOfWork;
            _repository = repository;
        }

        public T Insert(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            _repository.Insert(entity);
            UnitOfWork.SaveChanges();

            return entity;
        }

        public List<T> InsertMulti(List<T> entity)
        {
            try
            {
                _repository.InsertMulti(entity);
                UnitOfWork.SaveChanges();

                return entity;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool Delete(T entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException("entity");

                _repository.Delete(entity);
                UnitOfWork.SaveChanges();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool Delete(dynamic id)
        {
            return _repository.Delete(id);
        }

        public bool DeleteMulti(List<T> entity)
        {
            try
            {
                _repository.DeleteMulti(entity);
                UnitOfWork.SaveChanges();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public T GetById(int id)
        {
            return _repository.GetById(id);
        }
        public T Find(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includes)
        {
            return _repository.Find(expression, includes);
        }
        public List<T> FindAll(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includes)
        {
            return _repository.FindAll(expression, includes);
        }
        public IEnumerable<T> GetAll()
        {
            return _repository.GetAll();
        }

        public T Update(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            _repository.Update(entity);
            UnitOfWork.SaveChanges();

            return entity;
        }
    }
}
