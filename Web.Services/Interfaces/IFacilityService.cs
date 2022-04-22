using System.Collections.Generic;
using Web.Model;
using Web.Model.Common;

namespace Web.Services.Interfaces
{
    public interface IFacilityService
    {
        #region Service Lines
        BaseResponse GetAllServiceLines();
        BaseResponse GetUsersServiceLines();
        BaseResponse GetAllServiceLinesByDepartmentId(int DepartmentId, bool status);
        BaseResponse GetServiceLineById(int Id);
        BaseResponse getServicesByDepartmentIds(string departmentIds);
        BaseResponse GetServicesByOrganizationId(int OrganizationId);
        BaseResponse AddOrUpdateServiceLine(List<ServiceLineVM> serviceLines);
        BaseResponse ActiveOrInactiveServiceLine(int serviceLineId, int userId, bool status);
        BaseResponse DeleteServiceLine(int serviceLineId, int userId, bool status);
        #endregion

        #region Department
        BaseResponse GetAllDepartments();
        BaseResponse GetUsersDepartments();
        BaseResponse GetAllDepartmentsByOrganizationId(int OrganizationId, bool status);
        BaseResponse GetDepartmentById(int Id);
        BaseResponse GetDepartmentsByIds(string Ids);
        BaseResponse AddOrUpdateDepartment(List<DepartmentVM> departments);
        BaseResponse DeleteDepartment(int departmentId, int userId, bool status);

        BaseResponse ActiveOrInActiveDepartment(int departmentId, int userId, bool status);
        #endregion

        #region Organization

        BaseResponse GetOutpatientOrganizationsIvr();
        BaseResponse GetAllOrganizations(PaginationVM vM);
        BaseResponse GetAllOrganizations(bool status);
        BaseResponse GetOrganizationById(int Id);
        BaseResponse GetOrgAssociationTree(string Ids);
        BaseResponse GetOrganizationTypeByOrgId(int orgId);
        BaseResponse AddOrUpdateOrganization(OrganizationVM organization);
        BaseResponse DeleteOrganization(int OrganizationId, bool status);

        BaseResponse ActiveOrInActiveOrganization(int OrganizationId, bool status);
        #endregion

        #region Clinical Hours

        BaseResponse GetAllClinicalHours();
        BaseResponse GetClinicalHourById(int Id);
        BaseResponse GetClinicalHourByServiceLineId(int orgId, int serviceLineId,bool status);
        BaseResponse AddOrUpdateClinicalHour(OrganizationSchedule clinicalHours);
        BaseResponse DeleteClinicalHour(int Id, int userId);

        BaseResponse ActiveOrInActiveClinicalHour(int Id, int userId, bool status);

        #endregion

        #region Cinical Holidays
        BaseResponse GetClinicalHolidayByServiceLineId(int serviceLineId);
        BaseResponse SaveClinicalHoliday(ClinicalHolidayVM clinicalHoliday);
        BaseResponse DeleteClinicalHoliday(int clinicalHolidayId, int userId);
        #endregion

    }
}
