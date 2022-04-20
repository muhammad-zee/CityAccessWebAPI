using System.Collections.Generic;
using System.Linq;
using Web.Data.Models;
using Web.Model;
using Web.Model.Common;

namespace Web.Services.Interfaces
{
    public interface IAdminService
    {
        #region Dashboard
        BaseResponse GetLabelCounts(int orgId);
        BaseResponse GetUsersForDashBoard(int orgId);
        BaseResponse GetSchedulesForCurrentDate(ScheduleVM schedule);

        #endregion


        #region Components

        BaseResponse AddOrUpdateComponent(List<ComponentVM> components);
        BaseResponse GetAllComponents();
        BaseResponse GetComponentById(int Id);
        BaseResponse GetComponentsTreeByRoleId(int Id);
        BaseResponse GetComponentsByUserRoleId(int roleId, int userId);
        BaseResponse GetComponentsTreeByUserRoleId(int roleId, int userId);
        BaseResponse AddOrUpdateUserRoleComponentAccess(ComponentAccessUserRoleVMUpdate componentAccess);

        #endregion

        #region Users
        BaseResponse GetAllUsers();
        BaseResponse GetAllUsersByOrganizationId(int OrganizationId, int UserRoleId);
        BaseResponse GetAllUsersByOrganizationId(RegisterCredentialVM model);
        BaseResponse GetAllUsersByServiceLineAndRoleId(string OrganizationId, string ServiceLineId, string RoleIds);
        BaseResponse getAllScheduleUsersByServiceAndRoleId(string OrganizationId, string ServiceLineId, string RoleIds);
        BaseResponse GetAllEMSUsers(bool status);
        BaseResponse GetUserById(int Id);
        BaseResponse GetUserAlreadyExist(string userName);
        BaseResponse DeleteUser(int Id);

        #endregion

        #region User Role
        BaseResponse GetUsersByRoleId(int roleId);
        #endregion

        #region Roles
        BaseResponse GetAllRoles(RoleVM role);
        IQueryable<Role> getRoleList();
        IQueryable<Role> getRoleListByOrganizationId(int OrganizationId,bool status);
        IQueryable<Role> getScheduleRoleListByOrganizationId(int OrganizationId);
        IQueryable<Role> getRoleListByOrganizationIds(string OrganizationIds);
        BaseResponse GetAllRolesByServiceLineId(int ServiceLineId);
        List<UserRoleVM> getRoleListByUserId(int UserId);
        string SaveRole(List<RoleVM> role);
        BaseResponse GetRoleById(int roleId);
        BaseResponse DeleteRole(int Id);

        BaseResponse ActiveInActiveRole(int Id,bool status);

        #endregion

        #region Control List and Details
        BaseResponse GetUCLDetails(List<int> Id);
        #endregion

    }
}
