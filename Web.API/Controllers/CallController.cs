using ElmahCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using Twilio.AspNet.Core;
using Twilio.Rest.Api.V2010.Account;
using Web.API.Helper;
using Web.Model;
using Web.Model.Common;
using Web.Services.Interfaces;

namespace Web.API.Controllers
{
    //[Authorize]
    [RequestHandler]
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
            this._logger = new Logger(this._hostEnvironment, config);
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

        [HttpPost("Call/EnqueueCall")]
        public TwiMLResult EnqueueCall()
        {
            try
            {
                return this._callService.EnqueueCall();
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return this._callService.ExceptionResponse(ex);
            }

        }

        [HttpPost("Call/Connect")]
        public TwiMLResult Connect(string phoneNumber, string Twilio_PhoneNumber, string From, string CallSid, string CallStatus)
        {
            try
            {
                return this._callService.Connect(phoneNumber, Twilio_PhoneNumber,From,CallSid,CallStatus);
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
        public string CallbackStatus(string CallSid, string CallStatus )
        {
            try
            {
                return this._callService.CallbackStatus(CallSid, CallStatus);
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
                return this._callService.ReceiveVoicemail(RecordingUrl, RecordingSid);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return this._callService.ExceptionResponse(ex);
            }
        }

        [HttpPost("Call/saveCallRecord")]
        public BaseResponse saveCallLog([FromBody] CallLogVM log)
        {
            try
            {
                return this._callService.saveCallLog(log);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }

        }


        #region IVR Settings

        [HttpGet("ivr/getIvrTree/{Id}")]
        public BaseResponse getIvrTree(int Id)
        {
            try
            {
                return this._callService.getIvrTree(Id);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }

        }

        [HttpGet("ivr/getIvrNodes/{Id}")]
        public BaseResponse getIvrNodes(int Id)
        {
            try
            {
                return this._callService.getIvrNodes(Id);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }

        }

        [HttpPost("ivr/saveIvrNode")]
        public BaseResponse saveIvrNode([FromBody] IvrSettingVM model)
        {
            try
            {
                return this._callService.saveIvrNode(model);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }

        }

        [HttpGet("ivr/deleteIVRNode/{Id}/{userId}")]
        public BaseResponse DeleteIVRNode(int Id, int userId)
        {
            try
            {
                return this._callService.DeleteIVRNode(Id, userId);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }

        }

        #endregion

        #region IVR

        [HttpGet("ivr/getAllIvrs")]
        public BaseResponse getAllIvrs()
        {
            try
            {
                return this._callService.getAllIvrs();
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }

        }

        [HttpPost("ivr/saveIvr")]
        public BaseResponse saveIVR([FromBody] IVRVM model)
        {
            try
            {
                return this._callService.saveIVR(model);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }

        }

        [HttpGet("ivr/deleteIVR/{Id}/{userId}")]
        public BaseResponse DeleteIVR(int Id, int userId)
        {
            try
            {
                return this._callService.DeleteIVR(Id, userId);
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
