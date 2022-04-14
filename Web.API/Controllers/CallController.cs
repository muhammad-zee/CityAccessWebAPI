using ElmahCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
    [Authorize]
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

        [AllowAnonymous]
        [HttpPost("Call/EnqueueCall")]
        public TwiMLResult EnqueueCall(int parentNodeId, int serviceLineId, string CallSid)
        {
            try
            {
                return this._callService.EnqueueCall(parentNodeId, serviceLineId, CallSid);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return this._callService.ExceptionResponse(ex);
            }

        }

        [AllowAnonymous]
        [HttpPost("Call/Connect")]
        public TwiMLResult Connect(string phoneNumber, string Twilio_PhoneNumber, string From, string CallSid, string CallStatus)
        {
            try
            {
                return this._callService.Connect(phoneNumber, Twilio_PhoneNumber, From, CallSid, CallStatus);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return this._callService.ExceptionResponse(ex);
            }

        }

        [AllowAnonymous]
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




        [AllowAnonymous]
        [HttpPost("Call/CallConnected")]
        public TwiMLResult CallConnected(string To, string From)
        {
            try
            {
                return this._callService.CallConnected(To, From);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return this._callService.ExceptionResponse(ex);
            }
        }


        [AllowAnonymous]
        [HttpPost("Call/PromptResponse")]
        public TwiMLResult PromptResponse(int Digits, int ParentNodeId, int serviceLineId)
        {
            try
            {
                return this._callService.PromptResponse(Digits, ParentNodeId, serviceLineId);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return this._callService.ExceptionResponse(ex);
            }
        }


        [AllowAnonymous]
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

        #region [Call Status Events]

        [AllowAnonymous]
        [HttpPost("Call/CallbackStatus")]
        public string CallbackStatus()
        {
            try
            {
                return this._callService.CallbackStatus(HttpContext.Request.Form);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return ex.Message;
            }
        }
        [AllowAnonymous]
        [HttpPost("Call/InboundCallbackStatus")]
        public string InboundCallbackStatus(int parentNodeId, int serviceLineId)
        {
            try
            {
                return this._callService.InboundCallbackStatus(HttpContext.Request.Form, parentNodeId, serviceLineId);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return ex.Message;
            }
        }
        [AllowAnonymous]
        [HttpPost("Call/ConferenceParticipantCallbackStatus")]
        public string ConferenceParticipantCallbackStatus(int roleId, int serviceLineId, string conferenceSid)
        {
            try
            {
                return this._callService.ConferenceParticipantCallbackStatus(HttpContext.Request.Form, roleId, serviceLineId, conferenceSid);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return ex.Message;
            }
        }

        #endregion
        #region [Calls Logging]


        [HttpPost("CallLog/saveCallRecord")]
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

        [HttpGet("CallLog/getPreviousCalls")]
        public BaseResponse getPreviousCalls()
        {
            try
            {
                return this._callService.getPreviousCalls();
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }

        }
        #endregion

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

        [HttpGet("ivr/GetNodeType/{Id}")]
        public BaseResponse GetNodeType(int Id)
        {
            try
            {
                return this._callService.GetNodeType(Id);
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
                return this._callService.DeleteIVRNode(Id);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }

        }
        [AllowAnonymous]
        [HttpGet("ivr/copyIvrSettings/{copyFromIvrId}/{copytoIvrId}")]
        public BaseResponse copyIvrSettings(int copyFromIvrId, int copyToIvrId)
        {
            try
            {
                return this._callService.copyIvrSettings(copyFromIvrId, copyToIvrId);
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
        [HttpGet("ivr/getAllIvrsByOrgId/{orgId}")]
        public BaseResponse getAllIvrsByOrgId(int orgId)
        {
            try
            {
                return this._callService.getAllIvrsByOrgId(orgId);
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
                return this._callService.DeleteIVR(Id);
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
