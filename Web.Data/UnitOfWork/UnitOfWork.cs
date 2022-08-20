using System;
using System.Collections.Generic;
using Web.Data;
using Web.Data.Models;
using Web.DLL.Generic_Repository;
using Web.Model.Common;

namespace Web.DLL
{
    public class UnitOfWork : IUnitOfWork
    {
        private CityAccess_DbContext _context;
        public UnitOfWork(CityAccess_DbContext context)
        {
            _context = context;
        }

        public CityAccess_DbContext Context
        {
            get
            {
                return _context;
            }
        }

        ~UnitOfWork()
        {
            this.Dispose(false);
        }

        #region IUnitofWork Members

        public int Commit()
        {
            return _context.SaveChanges();
        }


        #endregion

        #region Repositories

        Dictionary<Type, object> _repo = null;
        public IGenericRepository<T> GetRepository<T>() where T : BaseEntity
        {
            if (this._repo == null)
                this._repo = new Dictionary<Type, object>();

            if (!this._repo.ContainsKey(typeof(T)))
                this._repo.Add(typeof(T), new GenericRepository<T>(this));

            return (IGenericRepository<T>)this._repo[typeof(T)];
        }

        #endregion

        #region Disposing logic

        #region IDisposable Members

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }

            this.disposed = true;
        }

        #endregion

    }
}
