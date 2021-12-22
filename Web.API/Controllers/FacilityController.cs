using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel;
using System.Net;
using Web.API.Helper;
using Web.Model;
using Web.Model.Common;
using Web.Services.Interfaces;

namespace Web.API.Controllers
{
    //[Authorize]
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
        [HttpGet("service/GetAllServiceLines")]
        public BaseResponse GetAllServiceLines()
        {
            try
            {
                var res = _facilityService.GetAllServiceLines();
                return res;
            }
            catch (Exception ex)
            {
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
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Delete Service Line")]
        [HttpGet("service/DeleteServiceLine/{Id}/{userId}")]
        public BaseResponse DeleteServiceLine(int Id, int userId)
        {
            try
            {
                var res = _facilityService.DeleteServiceLine(Id, userId);
                return res;
            }
            catch (Exception ex)
            {
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        #endregion


        #region Departments

        [Description("Get All Departments")]
        [HttpGet("dpt/GetAllDepartments")]
        public BaseResponse GetAllDepartments()
        {
            try
            {
                var res = _facilityService.GetAllDepartments();
                return res;
            }
            catch (Exception ex)
            {
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
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Delete Department")]
        [HttpGet("dpt/DeleteDepartment/{Id}/{userId}")]
        public BaseResponse DeleteDepartment(int Id, int userId)
        {
            try
            {
                var res = _facilityService.DeleteDepartment(Id, userId);
                return res;
            }
            catch (Exception ex)
            {
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }
        #endregion


        #region Organizations

        [Description("Get All Organizations")]
        [HttpGet("org/GetAllOrganizations")]
        public BaseResponse GetAllOrganizations()
        {
            try
            {
                var res = _facilityService.GetAllOrganizations();
                return res;
            }
            catch (Exception ex)
            {
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
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Delete Department")]
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
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        #endregion

    }
}
