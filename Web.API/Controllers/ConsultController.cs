using ElmahCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Web.API.Helper;
using Web.Model;
using Web.Model.Common;
using Web.Services.Enums;
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
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = ex.ToString() };
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
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = ex.ToString() };
            }
        }

        [Description("Get Consult Feilds For Org")]
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
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = ex.ToString() };
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
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = ex.ToString() };
            }

        }


    }
}
