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
    public class SettingController : Controller
    {
        private readonly ISettingService _settingService;
        private IConfiguration _config;
        Logger _logger;
        private IWebHostEnvironment _hostEnvironment;
        public SettingController(ISettingService settingService, IConfiguration config, IWebHostEnvironment environment)
        {
            _settingService = settingService;
            _config = config;
            _hostEnvironment = environment;
            _logger = new Logger(_hostEnvironment);
        }

        [Description("Get Settings By Org Id")]
        [HttpGet("settings/GetSettingsByOrgId/{OrgId}")]
        public BaseResponse GetSettingsByOrgId(int OrgId) 
        {
            try
            {
                return _settingService.GetSettingsByOrgId(OrgId);   
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
