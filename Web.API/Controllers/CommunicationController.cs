using ElmahCore;
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
    public class CommunicationController : Controller
    {
        private readonly ICommunicationService _communicaitonService;
        private IConfiguration _config;
        Logger _logger;
        private IWebHostEnvironment _hostEnvironment;
        public CommunicationController(ICommunicationService communicaitonService, IConfiguration config, IWebHostEnvironment environment)
        {
            this._communicaitonService = communicaitonService;
            _config = config;
            _hostEnvironment = environment;
            _logger = new Logger(_hostEnvironment);
        }

        [HttpGet("Communication/generateConversationToken")]
        public BaseResponse generateConversationToken(string Identity)
        {
            try
            {
                return this._communicaitonService.generateConversationToken(Identity);
            }
            catch(Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [HttpPost("Communication/sendPushNotification")]
        public BaseResponse sendPushNotification([FromBody] ConversationMessageVM msg)
        {
            try
            {
                return this._communicaitonService.sendPushNotification(msg);
            }
            catch(Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }
        [HttpGet("Communication/saveUserChannelSid")]
        public BaseResponse saveUserChannelSid(int UserId, string ChannelSid)
        {
            try
            {
                return this._communicaitonService.saveUserChannelSid(UserId, ChannelSid);
            }
            catch(Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }
        [HttpGet("Communication/daleteChannelBySid")]
        public BaseResponse daleteChannelBySid(string ChannelSid)
        {
            try
            {
                return this._communicaitonService.delateChatChannel( ChannelSid);
            }
            catch(Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }
    }
}
