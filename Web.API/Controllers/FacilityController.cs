using ElmahCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel;
using System.Net;
using Web.API.Helper;
using Web.Model;
using Web.Model.Common;
using Web.Services;
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
            this._logger = new Logger(this._hostEnvironment);
            this._facilityService = facilityService;
        }

        #region Service Lines

        [Description("Get All Service Lines")]
        [HttpGet("service/GetAllServiceLines/{departmentId}")]
        public BaseResponse GetAllServiceLines(int? departmentId)
        {
            try
            {
                BaseResponse res = null;
                if (departmentId != null)
                {
                    res = this._facilityService.GetAllServiceLinesByDepartmentId(Convert.ToInt32(departmentId));
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
        [HttpGet("service/GetServicesByIds/{Ids}")]
        public BaseResponse GetServicesByIds(string Ids)
        {
            try
            {
                var res = _facilityService.GetServicesByIds(Ids);
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
        public BaseResponse AddOrUpdateServiceLine([FromBody] ServiceLineVM serviceLine)
        {
            try
            {
                var res = _facilityService.AddOrUpdateServiceLine(serviceLine);
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
        [HttpGet("service/DeleteServiceLine/{serviceLineId}/{userId}")]
        public BaseResponse DeleteServiceLine(int serviceLineId, int userId)
        {
            try
            {
                var res = _facilityService.DeleteServiceLine(serviceLineId, userId);
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
        public BaseResponse GetAllDepartments(int? OrganizationId)
        {
            try
            {
                BaseResponse res = null;
                if (OrganizationId != null)
                {
                    res = this._facilityService.GetAllDepartmentsByOrganizationId(OrganizationId.Value);
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
        public BaseResponse AddOrUpdateDepartment([FromBody] DepartmentVM department)
        {
            try
            {
                var res = _facilityService.AddOrUpdateDepartment(department);
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
        [HttpGet("dpt/DeleteDepartment/{departmentId}/{userId}")]
        public BaseResponse DeleteDepartment(int departmentId, int userId)
        {
            try
            {
                var res = _facilityService.DeleteDepartment(departmentId, userId);
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
        [HttpGet("org/GetAllOrganizations")]
        public BaseResponse GetAllOrganizations(int RoleId)
        {
            try
            {
                var res = _facilityService.GetAllOrganizations(RoleId);
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
        public BaseResponse DeleteOrganization(int Id, int userId)
        {
            try
            {
                var res = _facilityService.DeleteOrganization(Id, userId);
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
        public BaseResponse DeleteClinicalHoliday(int clinicalHolidayId,int userId)
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
