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
            _logger = new Logger(_hostEnvironment, config);
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
        [Description("Get Consult Fields For Org")]
        [HttpGet("consult/GetConsultGraphDataForOrg/{OrgId}/{days}")]
        public BaseResponse GetConsultGraphDataForOrg(int OrgId, int days = 6)
        {
            try
            {
                return _consultService.GetConsultGraphDataForOrg(OrgId, days);
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
        [ValidateAntiForgeryToken]
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

        [Description("Get Consults By Service Line Id")]
        [HttpPost("consult/GetConsultsByServiceLineId")]
        public BaseResponse GetConsultsByServiceLineId([FromBody] ConsultVM consult)
        {
            try
            {
                return _consultService.GetConsultsByServiceLineId(consult);
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
        public BaseResponse AddOrUpdateConsult([FromBody] IDictionary<string, object> keyValues)
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

        [Description("create Consult group")]
        [HttpPost("consult/CreateConsultGroup")]
        public BaseResponse CreateConsultGroup([FromBody]  IDictionary<string, object> keyValues)
        {
            try
            {
                return _consultService.CreateConsultGroup(keyValues);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Active Or InActive Consult Consult")]
        [HttpGet("consult/ActiveOrInActiveConsult/{consultId}/{status}")]
        public BaseResponse ActiveOrInActiveConsult(int consultId, bool status)
        {
            try
            {
                return _consultService.ActiveOrInActiveConsult(consultId, status);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Delete Consult")]
        [HttpGet("consult/DeleteConsult/{consultId}/{status}")]
        public BaseResponse DeleteConsult(int consultId, bool status)
        {
            try
            {
                return _consultService.DeleteConsult(consultId, status);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }


        #endregion

        #region Consult Acknowledgments

        [HttpGet("consult/GetAllConsultAcknowledgments")]
        public BaseResponse GetAllConsultAcknowledgments()
        {
            try
            {
                return _consultService.GetAllConsultAcknowledgments();
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [HttpGet("consult/GetConsultAcknowledgmentByConsultId/{consultId}")]
        public BaseResponse GetConsultAcknowledgmentByConsultId(int consultId)
        {
            try
            {
                return _consultService.GetConsultAcknowledgmentByConsultId(consultId);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [HttpGet("consult/GetConsultAcknowledgmentByUserId/{userId}")]
        public BaseResponse GetConsultAcknowledgmentByUserId(int userId)
        {
            try
            {
                return _consultService.GetConsultAcknowledgmentByUserId(userId);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }


        [HttpGet("consult/AcknowledgeConsult/{consultId}")]
        public BaseResponse AcknowledgeConsult(int consultId)
        {
            try
            {
                return _consultService.AcknowledgeConsult(consultId);
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
