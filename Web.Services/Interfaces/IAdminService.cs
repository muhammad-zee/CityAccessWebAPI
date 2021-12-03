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
        BaseResponse GetComponentsByUserRoleId(int roleId, int userId);
        BaseResponse AddOrUpdateUserRoleComponentAccess(ComponentAccessUserRoleVM componentAccess);

        #endregion

        #region Users
        BaseResponse GetAllUsers();
        BaseResponse GetUserById(int Id);
        BaseResponse DeleteUser(int Id);

        #endregion

        #region Roles
        IQueryable<Role> getRoleList();
        IQueryable<UserRoleVM> getRoleListByUserId(int UserId);
        string SaveRole(RoleVM role);
        BaseResponse DeleteRole(int Id);

        #endregion

    }
}
