using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web.Data.Models;
using Web.Model;
using Web.Model.Common;

namespace Web.Services.Interfaces
{
    public interface IAdminService
    {
        BaseResponse AddOrUpdateComponent(List<ComponentVM> components);
        BaseResponse GetAllComponents();
        BaseResponse GetComponentById(int Id);
        BaseResponse GetComponentsByRoleId(int Id);

        IQueryable<Role> getRoleList();
    }
}
