using ElmahCore;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Twilio;
using Twilio.AspNet.Common;
using Twilio.Jwt.AccessToken;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Rest.Chat.V2.Service;
using Twilio.Types;
using Web.Data.Models;
using Web.DLL.Generic_Repository;
using Web.Model;
using Web.Model.Common;
using Web.Services.Enums;
using Web.Services.Helper;
using Web.Services.Interfaces;

namespace Web.Services.Concrete
{

    public class CommunicationService : ICommunicationService
    {
        private string Twilio_AccountSid;
        private string Twilio_AuthToken;

        private string SendGrid_ApiKey;
        private string FromEmail;

        private string Twilio_ChatServiceSid;
        private string Twilio_ChatPushCredentialSid;
        private string Twilio_ChatApiKey;
        private string Twilio_ChatApiKeySecret;


        private IConfiguration _config;
        private RAQ_DbContext _dbContext;
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<ConversationChannel> _conversationChannelsRepo;
        private readonly IRepository<ConversationParticipant> _conversationParticipantsRepo;
        public CommunicationService(IConfiguration config,
             RAQ_DbContext dbContext,
        IRepository<User> userRepo,
            IRepository<ConversationChannel> conversationChannelsRepo,
            IRepository<ConversationParticipant> conversationParticipantsRepo)
        {

            this._config = config;
            this._dbContext = dbContext;
            this.Twilio_AccountSid = this._config["Twilio:AccountSid"].ToString();
            this.Twilio_AuthToken = this._config["Twilio:AuthToken"].ToString();
            this.SendGrid_ApiKey = this._config["SendGrid:ApiKey"].ToString();
            this.FromEmail = this._config["SendGrid:FromEmail"].ToString();

            this.Twilio_ChatServiceSid = this._config["Twilio:ChatServiceSid"].ToString();
            this.Twilio_ChatPushCredentialSid = this._config["Twilio:PushCredentialSid"].ToString();
            this.Twilio_ChatApiKey = this._config["Twilio:ChatApiKey"].ToString();
            this.Twilio_ChatApiKeySecret = this._config["Twilio:ChatApiKeySecret"].ToString();

            this._userRepo = userRepo;
            this._conversationChannelsRepo = conversationChannelsRepo;
            this._conversationParticipantsRepo = conversationParticipantsRepo;
        }

        #region [SMS sending]
        public bool SendSms(string ToPhoneNumber, string SmsBody)
        {
            ToPhoneNumber = "+923327097498";
            var smsParams = new SmsRequest
            {
                To = ToPhoneNumber,
                From = this._config["Twilio:PhoneNumber"],
                Body = SmsBody,
            };
            var MessageStatus = SendingSms(smsParams);
            var result = MessageStatus.Result;

            if (result != "failed" && result != "undelivered")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public async Task<string> SendingSms(SmsRequest sr)
        {
            try
            {
                TwilioClient.Init(this.Twilio_AccountSid, this.Twilio_AuthToken);
                var messageResource = await MessageResource.CreateAsync(
                    to: new PhoneNumber(sr.To),
                    from: new PhoneNumber(sr.From),
                    body: sr.Body
                    );
                string status = messageResource.Status.ToString();
                return status;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                return ex.Message;
            }

        }
        #endregion

        #region [Email Sending]

        public bool SendEmail(string To, string Subject, string HtmlContent, byte[] ImageContent)
        {
            SendGridMessage sgm = new SendGridMessage();
            sgm.ReplyTo = new EmailAddress(To);
            sgm.From = new EmailAddress(this.FromEmail);
            sgm.Subject = Subject;
            sgm.HtmlContent = "<strong>" + HtmlContent + "</strong>";
            sgm.PlainTextContent = HtmlContent;
            ImageContent = new byte[64];
            var ImageName = "";
            //if (Request.Files.Count > 0)
            //{
            //    foreach (string file in Request.Files)
            //    {
            //        var _file = Request.Files[file];
            //    }
            //}
            //if (ImageFile != null)
            //{
            //    ImageName = ImageFile.FileName;
            //    var ContentType = Path.GetExtension(ImageName);
            //    BinaryReader reader = new BinaryReader(ImageFile.InputStream);
            //    ImageContent = reader.ReadBytes(ImageFile.ContentLength);
            //}
            //NewSendEmail.SendingEmail();
            var res = SendingEmailAsync(sgm, ImageName, ImageContent);
            var result = res.Result;
            if (result == CommunicationEnums.Sent.ToString())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<string> SendingEmailAsync(SendGridMessage sgm, string ImageName, byte[] ImageFile)
        {
            string response = string.Empty;
            try
            {
                var client = new SendGridClient(this.SendGrid_ApiKey);
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                //ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                var msg = MailHelper.CreateSingleEmail(sgm.From, sgm.ReplyTo, sgm.Subject, sgm.PlainTextContent, sgm.HtmlContent);
                if (ImageName != "")
                {
                    var file = Convert.ToBase64String(ImageFile);
                    msg.AddAttachment(ImageName, file);
                }
                var SendEmail = await client.SendEmailAsync(msg).ConfigureAwait(false);
                var status = SendEmail.StatusCode.ToString();

                if (status == "Unauthorized")
                {
                    response = CommunicationEnums.NotSent.ToString();
                }
                else
                {
                    response = CommunicationEnums.Sent.ToString();
                }
                return response;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                response = ex.Message.ToString();
                return response;
            }
            //finally
            //{
            //    ComunicationException.ComunicationExceptionHandling(ce);
            //}
        }


        #endregion


        #region [Twilio Conversation]

        public BaseResponse generateConversationToken(string Identity)
        {
            BaseResponse response = new BaseResponse();
            TwilioClient.Init(this.Twilio_AccountSid, this.Twilio_AuthToken);
            var grants = new HashSet<IGrant>
            {
                new ChatGrant {
                    ServiceSid = this.Twilio_ChatServiceSid,
                    PushCredentialSid = this.Twilio_ChatPushCredentialSid
                }
            };

            var chatToken = new Token(
                this.Twilio_AccountSid,
                this.Twilio_ChatApiKey,
                this.Twilio_ChatApiKeySecret,
                Identity,
                grants: grants);

            response.Status = HttpStatusCode.OK;
            response.Message = "Token Generated";
            response.Body = new { identity = Identity, token = chatToken.ToJwt() };
            return response;
        }

        public BaseResponse sendPushNotification(ConversationMessageVM msg)
        {
            TwilioClient.Init(this.Twilio_AccountSid, this.Twilio_AuthToken);
            var Notify = Twilio.Rest.Conversations.V1.Service.Conversation.MessageResource.Create(
                                       author: msg.author,
                                       body: msg.body,
                                       attributes: msg.attributes,
                                       pathChatServiceSid: this.Twilio_ChatServiceSid,
                                       pathConversationSid: msg.channelSid
                                       );
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Notification Sent", Body = Notify };
        }


        public BaseResponse saveUserChannelSid(int UserId, string ChannelSid)
        {
            BaseResponse response = new BaseResponse();

            var user = this._userRepo.Table.FirstOrDefault(u => u.UserId == UserId && !u.IsDeleted);
            if (user != null)
            {
                user.UserChannelSid = ChannelSid;
                this._userRepo.Update(user);
            }
            response.Status = HttpStatusCode.OK;
            response.Message = "Notificaiton Channel Saved Successfully";
            response.Body = new { UserId = UserId, UserChannelSid = ChannelSid };
            return response;
        }
        public BaseResponse saveConversationChannel(ConversationChannelVM channel)
        {
            BaseResponse response = new BaseResponse();

            var channelNotExists = this._conversationChannelsRepo.Table.Count(ch => ch.ChannelSid == channel.ChannelSid && ch.IsDeleted != true) == 0;
            if (channelNotExists)
            {
                var newChannel = new ConversationChannel
                {
                    FriendlyName = channel.FriendlyName,
                    UniqueName = channel.UniqueName,
                    ChannelSid = channel.ChannelSid,
                    IsGroup = channel.IsGroup,
                    CreatedBy = channel.CreatedBy,
                    CreatedDate = DateTime.UtcNow,
                    IsDeleted = false,
                };
                this._conversationChannelsRepo.Insert(newChannel);
                response.Status = HttpStatusCode.OK;
                response.Message = "Notificaiton Channel Saved Successfully";
                response.Body = newChannel;
            }
            else
            {
                response.Status = HttpStatusCode.OK;
                response.Message = "Channel Already Exists";
            }
            return response;
        }
        public BaseResponse saveConversationChannelParticipants(ConversationChannelParticipantsVM model)
        {
            BaseResponse response = new BaseResponse();

            var channel = this._conversationChannelsRepo.Table.FirstOrDefault(ch => ch.ChannelSid == model.ChannelSid && ch.IsDeleted != true);
            if (channel != null)
            {
                List<ConversationParticipant> channelParticipantsList = new List<ConversationParticipant>();
                ConversationParticipant newParticipant = null;
                User user = null;
                foreach (var p in model.Participants)
                {
                    user = this._userRepo.Table.FirstOrDefault(u => u.IsDeleted != true && u.IsActive == true && u.UserUniqueId == p);
                    newParticipant = new ConversationParticipant()
                    {
                        FriendlyName = user.FirstName + " " + user.LastName,
                        UniqueName = p,
                        UserIdFk = user.UserId,
                        ConversationChannelIdFk = channel.ConversationChannelId,
                        CreatedBy = model.CreatedBy,
                        CreatedDate = DateTime.UtcNow,
                        IsDeleted = false

                    };
                    channelParticipantsList.Add(newParticipant);
                }
                this._conversationParticipantsRepo.Insert(channelParticipantsList);

                response.Status = HttpStatusCode.OK;
                response.Message = "Channel Participants Saved";
                //response.Body = newChannel;
            }
            else
            {
                response.Status = HttpStatusCode.OK;
                response.Message = "channgel not saved yet";
            }
            return response;
        }
        public BaseResponse getConversationChannels(int UserId)
        {
            BaseResponse response = new BaseResponse();

            var channels = this._dbContext.LoadStoredProc("raq_getConversationChannelsByUserId")
            .WithSqlParam("@pUserId", UserId)
            .ExecuteStoredProc<ConversationChannelsListVM>().Result.ToList();
            if (channels != null)
            {
                response.Status = HttpStatusCode.OK;
                response.Message = "Conversation Channels Found";
                response.Body = channels;
            }
            else
            {
                response.Status = HttpStatusCode.OK;
                response.Message = "Conversation Channels Not Found";
            }
            return response;
        }

        public BaseResponse deleteChatChannel(string ChannelSid)
        {
            TwilioClient.Init(this.Twilio_AccountSid, this.Twilio_AuthToken);
            var channels = ChannelResource.Read(pathServiceSid: this.Twilio_ChatServiceSid);
            foreach (var ch in channels)
            {
                var delete = ChannelResource.Delete(pathServiceSid: this.Twilio_ChatServiceSid, pathSid: ch.Sid);
                var dbChannel = this._conversationChannelsRepo.Table.FirstOrDefault(c => c.ChannelSid == ch.Sid && c.IsDeleted != true);
                if (dbChannel != null)
                {
                    dbChannel.IsDeleted = true;
                    this._conversationChannelsRepo.Update(dbChannel);
                    var channelParticipants = this._conversationParticipantsRepo.Table.Where(p => p.ConversationChannelIdFk == dbChannel.ConversationChannelId);
                    foreach (var p in channelParticipants)
                    {
                        p.IsDeleted = true;
                    }
                    this._conversationParticipantsRepo.Update(channelParticipants);
                }
            }
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Channels Found" };
        }

        public BaseResponse deleteConversationChannel(string ChannelSid,int UserId)
        {
            var dbChannel = this._conversationChannelsRepo.Table.FirstOrDefault(c => c.ChannelSid == ChannelSid && c.IsDeleted != true);
            if (dbChannel != null)
            {
                dbChannel.IsDeleted = true;
                dbChannel.ModifiedBy = UserId;
                dbChannel.ModifiedDate = DateTime.UtcNow;
                
                this._conversationChannelsRepo.Update(dbChannel);
                var channelParticipants = this._conversationParticipantsRepo.Table.Where(p => p.ConversationChannelIdFk == dbChannel.ConversationChannelId && p.IsDeleted != true);
                foreach (var p in channelParticipants)
                {
                    p.IsDeleted = true;
                    p.ModifiedBy = UserId;
                    p.ModifiedDate = DateTime.UtcNow;
                }
                this._conversationParticipantsRepo.Update(channelParticipants);
            }
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Conversation Deleted" };
        }

        public BaseResponse deleteConversationParticipant(string ChannelSid,string ParticipantUniqueName, int UserId)
        {
            var dbChannel = this._conversationChannelsRepo.Table.FirstOrDefault(c => c.ChannelSid == ChannelSid && c.IsDeleted != true);
            if (dbChannel != null)
            {
                var participant = this._conversationParticipantsRepo.Table.FirstOrDefault(p => p.UniqueName == ParticipantUniqueName && p.ConversationChannelIdFk == dbChannel.ConversationChannelId && p.IsDeleted != true);
                participant.IsDeleted = true;
                participant.ModifiedBy = UserId;
                participant.ModifiedDate = DateTime.UtcNow;

                this._conversationParticipantsRepo.Update(participant);
            }
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Participant Removed" };
        }
        public BaseResponse getAllConversationUsers()
        {
            var chatUsers = this._userRepo.Table.Where(u => u.IsDeleted != true && u.IsActive == true && !string.IsNullOrEmpty(u.UserChannelSid));
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Chat users found", Body = chatUsers };
        }

        #endregion
    }



}
