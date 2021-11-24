using System;
using Web.DLL;
using Web.DLL.Db_Context;
using Web.DLL.Generic_Repository;
using Web.Model;
using Web.Model.Common;

namespace Web.Data
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<T> GetRepository<T>() where T : BaseEntity;
        DbHRMSContext Context { get; }
        int Commit();
    }
}
