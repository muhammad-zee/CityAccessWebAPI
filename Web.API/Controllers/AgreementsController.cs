using ElmahCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Web.API.Helper;
using Web.Model;
using Web.Model.Common;
using Web.Services.Interfaces;

namespace Web.API.Controllers
{
    [Authorize]
    [RequestHandler]
    public class AgreementsController : Controller
    {
        private readonly Logger _logger;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _hostEnvironment;

        private IAgreementsService _agreementsService;
        public AgreementsController(IConfiguration config, IWebHostEnvironment environment, IAgreementsService agreementsService)
        {
            this._config = config;
            this._hostEnvironment = environment;
            this._logger = new Logger(_hostEnvironment, config);
            this._agreementsService = agreementsService;
        }
        [HttpGet("agreements/GetServices")]
        public BaseResponse GetServices()
        {
            try
            {
                return this._agreementsService.GetServices();
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.Message.ToString(), Body = ex.ToString() };
            }
        }
        [HttpPost("agreements/GetAgreements")]
        public BaseResponse GetAgreements([FromBody]AgreementsFilterVM filter)
        {
            try
            {
                return this._agreementsService.GetAgreements(filter);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.Message.ToString(), Body = ex.ToString() };
            }
        }
        [HttpGet("agreements/GetAgreementDetailsByAgreementId")]
        public BaseResponse GetAgreementDetailsByAgreementId(int agreementId)
        {
            try
            {
                return this._agreementsService.GetAgreementDetailsByAgreementId(agreementId);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.Message.ToString(), Body = ex.ToString() };
            }
        }
        [HttpPost("agreements/SaveAgreement")]
        public BaseResponse SaveAgreemen([FromBody] AgreementVM agreement)
        {
            try
            {
                return this._agreementsService.SaveAgreement(agreement);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.Message.ToString(),Body=ex.ToString() };
            }
        }
    }
}
