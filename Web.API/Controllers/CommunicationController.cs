using ElmahCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using Web.API.Helper;
using Web.Model;
using Web.Model.Common;
using Web.Services.Interfaces;

namespace Web.API.Controllers
{
    [Authorize]
    [RequestHandler]
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
            _logger = new Logger(_hostEnvironment, config);
        }

        [HttpGet("Conversation/generateConversationToken")]
        public BaseResponse generateConversationToken(string Identity)
        {
            try
            {
                return this._communicaitonService.generateConversationToken(Identity);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [AllowAnonymous]
        [HttpPost("Conversation/sendPushNotification")]
        public BaseResponse sendPushNotification([FromBody] ConversationMessageVM msg)
        {
            try
            {
                return this._communicaitonService.sendPushNotification(msg);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [HttpPost("Conversation/ConversationCallbackEvent")]
        public BaseResponse ConversationCallbackEvent(string EventType)
        {
            try
            {
                return this._communicaitonService.ConversationCallbackEvent(EventType);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }
        [HttpGet("Conversation/saveUserChannelSid")]
        public BaseResponse saveUserChannelSid(int UserId, string ChannelSid)
        {
            try
            {
                return this._communicaitonService.saveUserChannelSid(UserId, ChannelSid);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }
        [AllowAnonymous]
        [HttpGet("Conversation/daleteAllChannels")]
        public BaseResponse daleteAllChannels(string key)
        {
            try
            {
                return this._communicaitonService.deleteAllChannels(key);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }


        [HttpGet("Conversation/getAllConversationUsers")]
        public BaseResponse getAllChatUsers()
        {
            try
            {
                return this._communicaitonService.getAllConversationUsers();
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }


        [HttpGet("Conversation/getAllConversationUsersByOrgId")]
        public BaseResponse getAllUsersByOrgId(int orgId)
        {
            try
            {
                return this._communicaitonService.getAllConversationUsersByOrgId(orgId);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }


        [HttpPost("Conversation/SaveConversationChannel")]
        public BaseResponse SaveConversationChannel([FromBody] ConversationChannelVM channel)
        {
            try
            {
                return this._communicaitonService.saveConversationChannel(channel);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [HttpPost("Conversation/saveConversationChannelParticipants")]
        public BaseResponse saveConversationChannelParticipants([FromBody] ConversationChannelParticipantsVM channel)
        {
            try
            {
                var state = ModelState;
                return this._communicaitonService.saveConversationChannelParticipants(channel);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [HttpGet("Conversation/getConversationChannels")]
        public BaseResponse getConversationChannels(int UserId)
        {
            try
            {
                return this._communicaitonService.getConversationChannels(UserId);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [HttpGet("Conversation/deleteConversationChannel")]
        public BaseResponse deleteConversationChannel(string ChannelSid, int UserId)
        {
            try
            {
                return this._communicaitonService.deleteConversationChannel(ChannelSid, UserId);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [HttpGet("Conversation/deleteConversationParticipant")]
        public BaseResponse deleteConversationParticipant(string ChannelSid, string ParticipantUniqueName, int UserId)
        {
            try
            {
                return this._communicaitonService.deleteConversationParticipant(ChannelSid, ParticipantUniqueName, UserId);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }
        [HttpGet("Conversation/addUserToChannelUsingApi")]
        public BaseResponse addUserToChannelUsingApi(string ChannelUniqueName, string ParticipantUniqueName, int UserId)
        {
            try
            {
                return this._communicaitonService.addUserToChannelUsingApi(ChannelUniqueName, ParticipantUniqueName, UserId);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }
        [HttpGet("Conversation/updateConversationGroup")]
        public BaseResponse updateConversationGroup(string FriendlyName, string ChannelSid)
        {
            try
            {
                return this._communicaitonService.updateConversationGroup(FriendlyName, ChannelSid);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }
        [HttpGet("Conversation/updateConversationUserSid")]
        public BaseResponse updateConversationUserSid(string UserSid)
        {
            try
            {
                return this._communicaitonService.updateConversationUserSid(UserSid);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }
        [HttpGet("Conversation/getConversationUsersStatus")]
        public BaseResponse getConversationUsersStatus(string UserSid)
        {
            try
            {
                return this._communicaitonService.getConversationUsersStatus(UserSid);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [HttpGet("Conversation/CurrentConversationParticipants/{channelSid}")]
        public BaseResponse CurrentConversationParticipants(string channelSid)
        {
            try
            {
                return this._communicaitonService.GetCurrentConversationParticipants(channelSid);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [HttpGet("Conversation/CreateOrRemoveGroupMemberAsAdmin/{isAdmin}/{uniqueName}/{channleSid}")]
        public BaseResponse createOrRemoveGroupMemberAsAdmin(bool isAdmin, string uniqueName, string channleSid)
        {
            try
            {
                return this._communicaitonService.createOrRemoveGroupMemberAsAdmin(isAdmin, uniqueName, channleSid);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }
        
        [AllowAnonymous]
        [HttpPost("Conversation/UploadAttachment")]
        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        public BaseResponse UploadAttachment()
        {
            try
            {
                var file = HttpContext.Request.Form.Files;

                return this._communicaitonService.UploadAttachment(file);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        #region [video call]
        [HttpGet("VideoCall/generateVideoCallToken")]
        public BaseResponse generateVideoCallToken(string Identity)
        {
            try
            {
                return this._communicaitonService.generateVideoCallToken(Identity);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [HttpPost("VideoCall/VideoRoomCallbackEvent")]
        public BaseResponse VideoRoomCallbackEvent(string EventType)
        {
            try
            {
                return this._communicaitonService.VideoRoomCallbackEvent(EventType);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [HttpPost("VideoCall/dialVideoCall")]
        public BaseResponse dialVideoCall([FromBody] DialVideoCallVM model)
        {
            try
            {

                return this._communicaitonService.dialVideoCall(model);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }
        [HttpGet("VideoCall/incomingCallEvent")]
        public BaseResponse incomingCallEvent(string roomSid, string eventType, string channelSid)
        {
            try
            {

                return this._communicaitonService.incomingCallEvent(roomSid, eventType, channelSid);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }
        #endregion

        #region Chat Setting

        [HttpGet("ChatSetting/GetTones/{Id}")]
        public BaseResponse GetTones(int Id)
        {
            try
            {
                return this._communicaitonService.GetTone(Id);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [HttpGet("ChatSetting/GetChatSetting/{UserId}")]
        public BaseResponse GetChatSetting(int UserId)
        {
            try
            {
                return this._communicaitonService.GetChatSetting(UserId);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [HttpPost("ChatSetting/addChatSettings")]
        public BaseResponse addChatSettings([FromBody] AddChatSettingVM channel)
        {
            try
            {
                return this._communicaitonService.addChatSettings(channel);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }
        #endregion chat Setting


        #region Communication Log


        [HttpPost("CommunicationLog/SaveCommunicatonLog")]
        public BaseResponse SaveCommunicatonLog([FromBody] CommunicationLogVM log)
        {
            try
            {
                return this._communicaitonService.SaveCommunicatonLog(log);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }


        [HttpGet("CommunicationLog/GetCommunicationLog/{logId}/{status}")]
        public BaseResponse GetCommunicationLog(int logId, bool status = true)
        {
            try
            {
                return this._communicaitonService.GetCommunicationLogById(logId, status);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [HttpGet("CommunicationLog/GetAllCommunicationLog")]
        public BaseResponse GetAllCommunicationLog(int orgId, string departmentIds, string serviceLineIds, bool showAllVoicemails)
        {
            try
            {
                return this._communicaitonService.GetAllCommunicationlog(orgId, departmentIds, serviceLineIds, showAllVoicemails);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }
        [HttpGet("CommunicationLog/GetCallLog")]
        public BaseResponse GetCallLog(int orgId, bool showAllCalls)
        {
            try
            {
                return this._communicaitonService.GetCallLog(orgId, showAllCalls);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [HttpGet("CommunicationLog/ActiveOrInActiveCommunicationLog/{logId}/{status}")]
        public BaseResponse ActiveOrInActiveCommunicationLog(int logId, bool status)
        {
            try
            {
                return this._communicaitonService.ActiveOrInActiveCommunicationlog(logId, status);
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
