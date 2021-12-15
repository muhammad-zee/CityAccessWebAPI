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
        BaseResponse AddOrUpdateServiceLine(ServiceLineVM serviceLine);
        #endregion

        #region Department
        BaseResponse GetAllDepartments();
        BaseResponse GetDepartmentById(int Id);
        BaseResponse AddOrUpdateDepartment(DepartmentVM department);
        #endregion

        #region Organization
        BaseResponse GetAllOrganizations();
        BaseResponse GetOrganizationById(int Id);
        BaseResponse AddOrUpdateOrganization(OrganizationVM organization);
        #endregion
    }
}
