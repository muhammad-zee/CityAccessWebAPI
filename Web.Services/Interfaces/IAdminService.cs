using System.Collections.Generic;
using System.Linq;
using Web.Data.Models;
using Web.Model;
using Web.Model.Common;

namespace Web.Services.Interfaces
{
    public interface IAdminService
    {
        #region Components

        BaseResponse AddOrUpdateComponent(List<ComponentVM> components);
        BaseResponse GetAllComponents();
        BaseResponse GetComponentById(int Id);
        BaseResponse GetComponentsByRoleId(int Id);

        #endregion

        #region Users
        BaseResponse GetAllUsers();
        BaseResponse GetUserById(int Id);
        #endregion

        IQueryable<Role> getRoleList();
        string SaveRole(RoleVM role);
    }
}
