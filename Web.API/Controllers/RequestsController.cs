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
    public class RequestsController : Controller
    {
        private readonly Logger _logger;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _hostEnvironment;

        private IRequestsService _requestsService;
        public RequestsController(IConfiguration config, IWebHostEnvironment environment, IRequestsService requestsService)
        {
            this._config = config;
            this._hostEnvironment = environment;
            this._logger = new Logger(_hostEnvironment, config);
            this._requestsService = requestsService;
        }

        [HttpPost("request/GetBookedServices")]
        public BaseResponse GetBookedServices([FromBody]RequestsFilterVM filter)
        {
            try
            {
                return this._requestsService.GetBookedServices(filter);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.Message.ToString(), Body = ex.ToString() };
            }
        }

        [HttpPost("request/SaveBookingRequest")]
        public BaseResponse SaveBookingRequest([FromBody]RequestVM req)
        {
            try
            {
                var state = ModelState;
                return this._requestsService.SaveBookingRequest(req);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.Message.ToString(), Body = ex.ToString() };
            }
        }
    }
}
