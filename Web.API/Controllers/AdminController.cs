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
    [Route("admin/")]
    public class AdminController : Controller
    {
        private readonly Logger _logger;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _hostEnvironment;

        private IAdminService _adminService;
        public AdminController(IConfiguration config, IWebHostEnvironment environment, IAdminService adminService)
        {
            this._config = config;
            this._hostEnvironment = environment;
            this._logger = new Logger(_hostEnvironment, config);
            this._adminService = adminService;
        }
        [HttpGet("GetAllcities")]
        public BaseResponse GetAllcities()
        {
            try
            {
                var citiesList = this._adminService.GetAllcities();
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Cities list returned", Body = citiesList };

            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.Message.ToString(), Body = ex.ToString() };
            }
        }
        [HttpGet("GetAllCommissionTypes")]
        public BaseResponse GetAllCommissionTypes()
        {
            try
            {
                var citiesList = this._adminService.GetAllCommissionTypes();
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Commission types returned", Body = citiesList };

            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.Message.ToString(), Body = ex.ToString() };
            }
        }
        [HttpGet("GetAllDynamicFields")]
        public BaseResponse GetAllDynamicFields()
        {
            try
            {
                var citiesList = this._adminService.GetAllDynamicFields();
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Cities list returned", Body = citiesList };

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
