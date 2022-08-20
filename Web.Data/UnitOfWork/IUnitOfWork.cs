using System;
using Web.Data.Models;
using Web.DLL.Generic_Repository;
using Web.Model.Common;

namespace Web.Data
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<T> GetRepository<T>() where T : BaseEntity;
        CityAccess_DbContext Context { get; }
        int Commit();
    }
}
