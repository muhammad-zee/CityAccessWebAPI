using ElmahCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using Web.API.Helper;
using Web.Model;
using Web.Model.Common;
using Web.Services.Interfaces;

namespace Web.API.Controllers
{
    [Authorize]
    [RequestHandler]
    public class FacilityController : Controller
    {
        private readonly IFacilityService _facilityService;
        private IConfiguration _config;
        Logger _logger;
        private IWebHostEnvironment _hostEnvironment;
        public FacilityController(IConfiguration config, IWebHostEnvironment environment, IFacilityService facilityService)
        {
            this._config = config;
            this._hostEnvironment = environment;
            this._logger = new Logger(this._hostEnvironment, config);
            this._facilityService = facilityService;
        }

        #region Service Lines

        [Description("Get All Service Lines")]
        [HttpGet("service/GetAllServiceLines/{departmentId}/{status}")]
        public BaseResponse GetAllServiceLines(int? departmentId, bool? status)
        {
            if (status == null)
            {
                status = true;
            }
            try
            {
                BaseResponse res = null;
                if (departmentId != null)
                {
                    res = this._facilityService.GetAllServiceLinesByDepartmentId(Convert.ToInt32(departmentId), status.Value);
                }
                else
                {
                    res = this._facilityService.GetAllServiceLines();
                }

                return res;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Get Service Line By Id")]
        [HttpGet("service/GetServiceLineById/{Id}")]
        public BaseResponse GetServiceLineById(int Id)
        {
            try
            {
                var res = _facilityService.GetServiceLineById(Id);
                return res;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Get Services By Ids")]
        [HttpGet("service/getServicesByDepartmentIds/{departmentIds}")]
        public BaseResponse getServicesByDepartmentIds(string departmentIds)
        {
            try
            {
                var res = _facilityService.getServicesByDepartmentIds(departmentIds);
                return res;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Get Services By Organization ID")]
        [HttpGet("service/GetServicesByOrganizationId/{OrganizationId}")]
        public BaseResponse GetServicesByOrganizationId(int OrganizationId)
        {
            try
            {
                var res = _facilityService.GetServicesByOrganizationId(OrganizationId);
                return res;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Add Or Update Service Line")]
        [HttpPost("service/AddOrUpdateServiceLine")]
        public BaseResponse AddOrUpdateServiceLine([FromBody] List<ServiceLineVM> serviceLines)
        {
            try
            {
                var res = _facilityService.AddOrUpdateServiceLine(serviceLines);
                return res;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Delete Service Line")]
        [HttpGet("service/ActiveOrInactiveServiceLine/{serviceLineId}/{userId}/{status}")]
        public BaseResponse ActiveOrInactiveServiceLine(int serviceLineId, int userId, bool status)
        {
            try
            {
                var res = _facilityService.ActiveOrInactiveServiceLine(serviceLineId, userId, status);
                return res;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }


        [Description("Delete Service Line")]
        [HttpGet("service/DeleteServiceLine/{serviceLineId}/{userId}/{status}")]
        public BaseResponse DeleteServiceLine(int serviceLineId, int userId, bool status)
        {
            try
            {
                var res = _facilityService.DeleteServiceLine(serviceLineId, userId, status);
                return res;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        #endregion

        #region Departments

        [Description("Get All Departments")]
        [HttpGet("dpt/GetAllDepartments")]
        public BaseResponse GetAllDepartments(int? OrganizationId, bool? status = true)
        {
            try
            {
                if (status == null)
                {
                    status = true;
                }
                BaseResponse res = null;
                if (OrganizationId != null)
                {
                    res = this._facilityService.GetAllDepartmentsByOrganizationId(OrganizationId.Value, status.Value);
                }
                else
                {
                    res = this._facilityService.GetAllDepartments();
                }

                return res;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Get Department  By Id")]
        [HttpGet("dpt/GetDepartmentById/{Id}")]
        public BaseResponse GetDepartmentById(int Id)
        {
            try
            {
                var res = _facilityService.GetDepartmentById(Id);
                return res;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Get Departments By Ids")]
        [HttpGet("dpt/GetDepartmentsByIds/{Ids}")]
        public BaseResponse GetDepartmentsByIds(string Ids)
        {
            try
            {
                var res = _facilityService.GetDepartmentsByIds(Ids);
                return res;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Add Or Update Department ")]
        [HttpPost("dpt/AddOrUpdateDepartment")]
        public BaseResponse AddOrUpdateDepartment([FromBody] List<DepartmentVM> departments)
        {
            try
            {
                var res = _facilityService.AddOrUpdateDepartment(departments);
                return res;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Delete Department")]
        [HttpGet("dpt/DeleteDepartment/{departmentId}/{userId}/{status}")]
        public BaseResponse DeleteDepartment(int departmentId, int userId, bool status)
        {
            try
            {
                var res = _facilityService.DeleteDepartment(departmentId, userId, status);
                return res;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Active/InActive Department")]
        [HttpGet("dpt/ActiveOrInActiveDepartment/{departmentId}/{userId}/{status}")]
        public BaseResponse ActiveOrInActiveDepartment(int departmentId, int userId, bool status)
        {
            try
            {
                var res = _facilityService.ActiveOrInActiveDepartment(departmentId, userId, status);
                return res;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }
        #endregion

        #region Organizations

        [Description("Get All Organizations")]
        [HttpPost("org/GetAllOrganizations")]
        public BaseResponse GetAllOrganizations([FromBody] PaginationVM vM)
        {
            try
            {
                var res = _facilityService.GetAllOrganizations(vM);
                return res;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }


        [Description("Get All Organizations")]
        [HttpGet("org/GetAllOrganizations")]
        public BaseResponse GetAllOrganizations(bool? status = false)
        {

            if (status == null)
            {
                status = true;
            }
            try
            {
                var res = _facilityService.GetAllOrganizations(status.Value);
                return res;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Get All Organizations For Outpatient")]
        [HttpGet("org/GetAllOutpatientOrganization")]
        public BaseResponse GetOutPatientOrganization()
        {
            try
            {
                var res = _facilityService.GetOutpatientOrganizationsIvr();
                return res;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Get Organization  By Id")]
        [HttpGet("org/GetOrganizationById/{Id}")]
        public BaseResponse GetOrganizationById(int Id)
        {
            try
            {
                var res = _facilityService.GetOrganizationById(Id);
                return res;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Get Organization Association Tree By Id")]
        [HttpGet("org/GetOrganizationAssociationTreeByIds/{Ids}")]
        public BaseResponse GetOrgAssociationTree(string Ids)
        {
            try
            {
                var res = _facilityService.GetOrgAssociationTree(Ids);
                return res;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Add Or Update Organization ")]
        [HttpPost("org/AddOrUpdateOrganization")]
        public BaseResponse AddOrUpdateOrganization([FromBody] OrganizationVM organization)
        {
            try
            {
                var res = _facilityService.AddOrUpdateOrganization(organization);
                return res;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Delete Organization")]
        [HttpGet("org/DeleteOrganization")]
        public BaseResponse DeleteOrganization(int OrganizationId, bool status)
        {
            try
            {
                var res = _facilityService.DeleteOrganization(OrganizationId, status);
                return res;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Get Organization Type By Id")]
        [HttpGet("org/GetOrganizationTypeByOrgId/{orgId}")]
        public BaseResponse GetOrganizationTypeByOrgId(int orgId)
        {
            try
            {
                var res = _facilityService.GetOrganizationTypeByOrgId(orgId);
                return res;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }
        #endregion

        #region Clinical Hours

        [Description("Get All Clinical Hours")]
        [HttpGet("ClinicalHour/GetAllClinicalHours")]
        public BaseResponse GetAllClinicalHours()
        {
            try
            {
                var res = _facilityService.GetAllClinicalHours();
                return res;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Get Clinical Hours By Id")]
        [HttpGet("ClinicalHour/GetClinicalHourById/{Id}")]
        public BaseResponse GetClinicalHourById(int Id)
        {
            try
            {
                var res = _facilityService.GetClinicalHourById(Id);
                return res;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Get Clinical Hours By Service Line Id")]
        [HttpGet("ClinicalHour/GetClinicalHourByServiceLineId/{orgId}/{serviceLineId}")]
        public BaseResponse GetClinicalHourByServiceLineId(int orgId, int serviceLineId)
        {
            try
            {
                var res = _facilityService.GetClinicalHourByServiceLineId(orgId, serviceLineId);

                return res;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }


        [Description("Add Or Update Clinical Hours")]
        [HttpPost("ClinicalHour/AddOrUpdateClinicalHour")]
        public BaseResponse AddOrUpdateClinicalHour([FromBody] OrganizationSchedule clinicalHours)
        {
            try
            {
                var res = _facilityService.AddOrUpdateClinicalHour(clinicalHours);
                return res;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Delete Clinical Hour")]
        [HttpGet("ClinicalHour/DeleteClinicalHour")]
        public BaseResponse DeleteClinicalHour(int Id, int userId)
        {
            try
            {
                var res = _facilityService.DeleteClinicalHour(Id, userId);
                return res;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        #endregion

        #region Clinical Holidays

        [Description("Get Clinical Hours By Service Line Id")]
        [HttpGet("ClinicalHour/GetClinicalHolidayByServiceLineId/{serviceLineId}")]
        public BaseResponse GetClinicalHolidayByServiceLineId(int serviceLineId)
        {
            try
            {
                var res = _facilityService.GetClinicalHolidayByServiceLineId(serviceLineId);
                return res;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }


        [Description("Add Or Update Clinical Holidays")]
        [HttpPost("ClinicalHour/SaveClinicalHoliday")]
        public BaseResponse SaveClinicalHoliday([FromBody] ClinicalHolidayVM clinicalHoliday)
        {
            try
            {
                var res = _facilityService.SaveClinicalHoliday(clinicalHoliday);
                return res;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Delete Clinical Holidays")]
        [HttpGet("ClinicalHour/DeleteClinicalHoliday")]
        public BaseResponse DeleteClinicalHoliday(int clinicalHolidayId, int userId)
        {
            try
            {
                var res = _facilityService.DeleteClinicalHoliday(clinicalHolidayId, userId);
                return res;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }
        #endregion

    }
}
