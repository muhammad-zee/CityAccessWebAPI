﻿using ElmahCore;
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
            _logger = new Logger(_hostEnvironment);
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
        [HttpGet("Conversation/daleteChannelBySid")]
        public BaseResponse daleteChannelBySid(string ChannelSid)
        {
            try
            {
                return this._communicaitonService.deleteChatChannel(ChannelSid);
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
    }
}
