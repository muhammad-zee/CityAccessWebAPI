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
    public class ServicesController : Controller
    {
        private readonly Logger _logger;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _hostEnvironment;

        private IServicesService _servicesService;
        public ServicesController(IConfiguration config, IWebHostEnvironment environment, IServicesService servicesService)
        {
            this._config = config;
            this._hostEnvironment = environment;
            this._logger = new Logger(_hostEnvironment, config);
            this._servicesService = servicesService;
        }
        [HttpGet("services/GetAllService")]
        public BaseResponse GetAllService()
        {
            try
            {
                var responseList = this._servicesService.GetAllService();
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Sevice list return", Body = responseList };
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.Message.ToString(), Body = ex.ToString() };
            }
        }
        [HttpGet("services/GetServiceDetails")]
        public BaseResponse GetServiceDetails(int ServiceId)
        {
            try
            {
                var serviceDetail = this._servicesService.GetServiceDetails(ServiceId);
                return new BaseResponse
                {
                    Status = HttpStatusCode.OK,
                    Message = "Service detail returned",
                    Body = serviceDetail
                };
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.Message.ToString(), Body = ex.ToString() };
            }
        }
        [HttpPost("services/SaveService")]
        public BaseResponse SaveService([FromBody]ServicesVM service)
        {
            try
            {
                return this._servicesService.SaveService(service);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.Message.ToString(), Body = ex.ToString() };
            }
        }
        [HttpGet("services/DeleteService")]
        public BaseResponse DeleteServices(int ServiceId)
        {
            try
            {
                return this._servicesService.DeleteServices(ServiceId);
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
