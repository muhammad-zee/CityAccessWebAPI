using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web.Data.Models;
using Web.Model.Common;

namespace Web.Services.Interfaces
{
    public interface IAdminService
    {
        IQueryable<City> GetAllcities();
        IQueryable<CommissionType> GetAllCommissionTypes();
        IQueryable<DynamicFieldAlternativeVM> GetAllDynamicFields();
    }
}
