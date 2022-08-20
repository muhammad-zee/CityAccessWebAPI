using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Web.Data.Models;

namespace Web.DLL.Generic_Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly CityAccess_DbContext _appContext;
        private DbSet<T> _entities;
        private UnitOfWork _unitOfWork;

        #region CTOR
        public GenericRepository(CityAccess_DbContext appContext)
        {
            _appContext = appContext;
        }

        public GenericRepository(UnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }
        #endregion

        #region Properties
        public virtual IQueryable<T> TableNoTracking
        {
            get
            {
                return this.Entities.AsNoTracking();
            }
        }
        protected virtual DbSet<T> Entities
        {
            get
            {
                if (_entities == null)
                    _entities = _appContext.Set<T>();
                return _entities;
            }
        }
        public IQueryable<T> Table
        {
            get
            {
                return this.Entities;
            }
        }
        #endregion


        #region Methods

        public IQueryable<T> GetList()
        {
            return this.Entities;
        }
        public T GetById(int Id)
        {
            object[] Ids = new object[Id];
            return _entities.FindAsync(Ids).Result;
        }
        public IQueryable<T> GetList(Expression<Func<T, bool>> predicate)
        {
            return this.Entities.Where(predicate);
        }
        public T GetByPreducate(Expression<Func<T, bool>> predicate)
        {
            return this.Entities.FirstOrDefault(predicate);
        }
        public void Insert(T entity)
        {
            try
            {
                if (entity == null) throw new ArgumentNullException("Entity");

                this.Entities.Add(entity);
                this._appContext.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void Insert(IEnumerable<T> entities)
        {
            try
            {
                if (entities == null) throw new ArgumentNullException("entities");

                this.Entities.AddRange(entities);

                this._appContext.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void Update(T entity)
        {
            try
            {
                if (entity == null) throw new ArgumentNullException("entity");
                this._appContext.Update(entity);
                this._appContext.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void Update(IEnumerable<T> entities)
        {
            try
            {
                if (entities == null) throw new ArgumentNullException("entities");

                this.Entities.UpdateRange(entities);
                this._appContext.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void Delete(T entity)
        {
            try
            {
                if (entity == null) throw new ArgumentNullException("entity");
                this.Entities.Remove(entity);
                this._appContext.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void DeleteRange(IEnumerable<T> entities)
        {
            try
            {
                if (entities == null) throw new ArgumentNullException("entities");
                this.Entities.RemoveRange(entities);
                this._appContext.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IQueryable<T> GetAllIncluding(params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> queryable = Entities;
            foreach (System.Linq.Expressions.Expression<Func<T, object>> includeProperty in includeProperties)
            {
                queryable = queryable.Include<T, object>(includeProperty);
            }

            return queryable;
        }

        public IQueryable<T> ExcuteSql(string Qry)
        {
            return this.Entities.FromSqlRaw($"{Qry}");
        }


        #endregion
    }
}
