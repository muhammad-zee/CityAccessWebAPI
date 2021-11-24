using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Web.Model.Common;

namespace Web.DLL.Generic_Repository
{
    public interface IRepository<T> where T : BaseEntity
    {
        IQueryable<T> GetList();
        IQueryable<T> GetList(Expression<Func<T, bool>> predicate);
        T GetByPreducate(Expression<Func<T, bool>> predicate);
        void Insert(T entity);
        void Insert(IEnumerable<T> entities);
        void Update(T entity);
        void Update(IEnumerable<T> entities);
        void Delete(T entity);
        IQueryable<T> Table { get; }
        IQueryable<T> GetAllIncluding(params Expression<Func<T, object>>[] includeProperties);
        void DeleteRange(IEnumerable<T> entities);
    }
}
