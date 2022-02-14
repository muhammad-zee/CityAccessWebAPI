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
    public class ConsultController : Controller
    {
        private readonly IConsultService _consultService;
        private IConfiguration _config;
        Logger _logger;
        private IWebHostEnvironment _hostEnvironment;
        public ConsultController(IConsultService consultService, IConfiguration config, IWebHostEnvironment environment)
        {
            _consultService = consultService;
            _config = config;
            _hostEnvironment = environment;
            _logger = new Logger(_hostEnvironment);
        }

        #region Consult Fields
        [Description("Get All Consult Feilds")]
        [HttpGet("consult/GetAllConsultFields")]
        public BaseResponse GetAllConsultFields()
        {
            try
            {
                return _consultService.GetAllConsultFields();
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Get Consult Feilds For Org")]
        [HttpGet("consult/GetConsultFeildsForOrg/{OrgId}")]
        public BaseResponse GetConsultFeildsForOrg(int OrgId)
        {
            try
            {
                return _consultService.GetConsultFeildsForOrg(OrgId);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Get Consult Feilds For Org")]
        [HttpGet("consult/GetConsultFormFieldByOrgId/{OrgId}")]
        public BaseResponse GetConsultFormFieldByOrgId(int OrgId)
        {
            try
            {
                return _consultService.GetConsultFormFieldByOrgId(OrgId);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Add Or Update Consult Feilds")]
        [HttpPost("consult/AddOrUpdateConsultFeilds")]
        public BaseResponse AddOrUpdateConsultFeilds([FromBody] ConsultFieldsVM consultField)
        {
            try
            {
                return _consultService.AddOrUpdateConsultFeilds(consultField);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        #endregion

        #region Organization Consult Fields

        [Description("Get Consult Feilds For Org")]
        [HttpPost("consult/AddOrUpdateOrgConsultFeilds")]
        public BaseResponse AddOrUpdateOrgConsultFeilds([FromBody] List<OrgConsultFieldsVM> orgConsultFields)
        {
            try
            {
                return _consultService.AddOrUpdateOrgConsultFeilds(orgConsultFields);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }

        }

        [Description("Get Consult Forms For Org")]
        [HttpGet("consult/GetConsultFormByOrgId/{orgId}")]
        public BaseResponse GetConsultFormByOrgId(int orgId)
        {
            try
            {
                return _consultService.GetConsultFormByOrgId(orgId);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        #endregion

        #region Consults

        [Description("Get All Consults")]
        [HttpGet("consult/GetAllConsults")]
        public BaseResponse GetAllConsults()
        {
            try
            {
                return _consultService.GetAllConsults();
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Get All Consult By Id")]
        [HttpGet("consult/GetConsultById/{Id}")]
        public BaseResponse GetConsultById(int Id)
        {
            try
            {
                return _consultService.GetConsultById(Id);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Add or Update Consult Form")]
        [HttpPost("consult/AddOrUpdateConsult")]
        public BaseResponse AddOrUpdateConsult(IDictionary<string, object> keyValues)
        {
            try
            {
                return _consultService.AddOrUpdateConsult(keyValues);
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
