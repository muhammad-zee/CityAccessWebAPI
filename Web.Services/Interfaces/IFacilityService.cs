using Web.Data.Models;
using Web.Model;
using Web.Model.Common;

namespace Web.Services.Interfaces
{
    public interface IFacilityService
    {
        #region Service Lines
        BaseResponse GetAllServiceLines();
        BaseResponse GetServiceLineById(int Id);
        BaseResponse GetServicesByIds(string Ids);
        BaseResponse AddOrUpdateServiceLine(ServiceLineVM serviceLine);
        BaseResponse DeleteServiceLine(int Id, int userId);
        #endregion

        #region Department
        BaseResponse GetAllDepartments();
        BaseResponse GetDepartmentById(int Id);
        BaseResponse GetDepartmentsByIds(string Ids);
        BaseResponse AddOrUpdateDepartment(DepartmentVM department);
        BaseResponse DeleteDepartment(int Id, int userId);
        #endregion

        #region Organization
        BaseResponse GetAllOrganizations();
        BaseResponse GetOrganizationById(int Id);
        BaseResponse AddOrUpdateOrganization(OrganizationVM organization);
        BaseResponse DeleteOrganization(int Id, int userId);
        #endregion
    }
}
