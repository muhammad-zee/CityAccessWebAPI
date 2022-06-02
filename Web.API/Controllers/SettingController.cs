﻿using ElmahCore;
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
            _logger = new Logger(_hostEnvironment, config);
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
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        //new
        [Description("Get Settings By Org Id (Change Password)")]
        [HttpPost("settings/GetSettingsChangePasswordByOrgId")]
        public BaseResponse GetSettingsChangePasswordByOrgId([FromBody] SettingsVM settings)
        {
            try
            {
                return _settingService.GetSettingsChangePasswordByOrgId(settings);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Add or Update Org Settings")]
        [HttpPost("settings/AddOrUpdateOrgSettings")]
        public BaseResponse AddOrUpdateOrgSettings([FromBody] SettingsVM settings)
        {
            var state = ModelState;
            try
            {
                return _settingService.AddOrUpdateOrgSettings(settings);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        #region [Activity Log]
        [HttpPost("ActivityLog/GetActivityLog")]
        public BaseResponse GetActivityLog([FromBody] FilterActivityLogVM filter)
        {
            var state = ModelState;
            try
            {
                return _settingService.GetActivityLog(filter);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [HttpPost("ActivityLog/GetActivityLogPrimaryKeys")]
        public BaseResponse GetActivityLogPrimaryKeys([FromBody] FilterActivityLogVM filter)
        {
            var state = ModelState;
            try
            {
                return _settingService.GetActivityLogPrimaryKeys(filter);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [HttpGet("ActivityLog/ActivityLoggedOut")]
        public BaseResponse GetActivityLoggedOut()
        {
            var state = ModelState;
            try
            {
                return _settingService.GetActivityLoggedOut();
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
