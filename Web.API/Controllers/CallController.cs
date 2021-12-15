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
using Twilio.AspNet.Core;
using Twilio.Rest.Api.V2010.Account;
using Twilio.TwiML;
using Web.API.Helper;
using Web.Model;
using Web.Model.Common;
using Web.Services.Enums;
using Web.Services.Interfaces;

namespace Web.API.Controllers
{
    public class CallController : TwilioController
    {
        private readonly ICallService _callService;
        private IConfiguration _config;
        Logger _logger;
        private IWebHostEnvironment _hostEnvironment;
        public CallController(ICallService callService, IConfiguration config, IWebHostEnvironment environment)
        {
            this._callService = callService;
            this._config = config;
            this._hostEnvironment = environment;
            this._logger = new Logger(this._hostEnvironment);
        }

        [HttpGet("Call/Token")]

        public BaseResponse Token(string Identity)
        {
            try
            {
                return this._callService.GenerateToken(Identity);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }

        }

        [HttpPost("Call/Connect")]
        public TwiMLResult Connect(string phoneNumber, string Twilio_PhoneNumber)
        {
            try
            {
                return this._callService.Connect(phoneNumber,Twilio_PhoneNumber
);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return this._callService.ExceptionResponse(ex);
            }

        }
        [HttpGet("Call/Call")]

        public CallResource Call()
        {
            try
            {
                return this._callService.Call();
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                throw ex;
            }

        }

        [HttpPost("Call/CallbackStatus")]
        public string CallbackStatus(string Callsid, string CallStatus)
        {
            try
            {
                return this._callService.CallbackStatus(Callsid, CallStatus);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return ex.Message;
            }
        }

        [HttpPost("Call/CallConnected")]
        public TwiMLResult CallConnected()
        {
            try
            {
                return this._callService.CallConnected();
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return this._callService.ExceptionResponse(ex);
            }
        }

        [HttpPost("Call/PromptResponse")]
        public TwiMLResult PromptResponse(int Digits)
        {
            try
            {
                return this._callService.PromptResponse(Digits);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return this._callService.ExceptionResponse(ex);
            }
        }
        [HttpPost("Call/ReceiveVoicemail")]
        public TwiMLResult ReceiveVoicemail(string RecordingUrl, string RecordingSid)
        {
            try
            {
                return this._callService.ReceiveVoicemail( RecordingUrl,  RecordingSid);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return this._callService.ExceptionResponse(ex);
            }
        }
    }
}
