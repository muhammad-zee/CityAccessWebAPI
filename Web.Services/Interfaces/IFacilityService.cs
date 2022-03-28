using System.Collections.Generic;
using Web.Model;
using Web.Model.Common;

namespace Web.Services.Interfaces
{
    public interface IFacilityService
    {
        #region Service Lines
        BaseResponse GetAllServiceLines();
        BaseResponse GetAllServiceLinesByDepartmentId(int DepartmentId);
        BaseResponse GetServiceLineById(int Id);
        BaseResponse getServicesByDepartmentIds(string departmentIds);
        BaseResponse GetServicesByOrganizationId(int OrganizationId);
        BaseResponse AddOrUpdateServiceLine(List<ServiceLineVM> serviceLines);
        BaseResponse DeleteServiceLine(int serviceLineId, int userId);
        #endregion

        #region Department
        BaseResponse GetAllDepartments();
        BaseResponse GetAllDepartmentsByOrganizationId(int OrganizationId);
        BaseResponse GetDepartmentById(int Id);
        BaseResponse GetDepartmentsByIds(string Ids);
        BaseResponse AddOrUpdateDepartment(List<DepartmentVM> departments);
        BaseResponse DeleteDepartment(int departmentId, int userId);
        #endregion

        #region Organization
        BaseResponse GetAllOrganizations(PaginationVM vM);
        BaseResponse GetAllOrganizations(int RoleId);
        BaseResponse GetOrganizationById(int Id);
        BaseResponse GetOrgAssociationTree(string Ids);
        BaseResponse GetOrganizationTypeByOrgId(int orgId);
        BaseResponse AddOrUpdateOrganization(OrganizationVM organization);
        BaseResponse DeleteOrganization(int Id, int userId);
        #endregion

        #region Clinical Hours

        BaseResponse GetAllClinicalHours();
        BaseResponse GetClinicalHourById(int Id);
        BaseResponse GetClinicalHourByServiceLineId(int orgId, int serviceLineId);
        BaseResponse AddOrUpdateClinicalHour(OrganizationSchedule clinicalHours);
        BaseResponse DeleteClinicalHour(int Id, int userId);

        #endregion

        #region Cinical Holidays
        BaseResponse GetClinicalHolidayByServiceLineId(int serviceLineId);
        BaseResponse SaveClinicalHoliday(ClinicalHolidayVM clinicalHoliday);
        BaseResponse DeleteClinicalHoliday(int clinicalHolidayId, int userId);
        #endregion

    }
}
