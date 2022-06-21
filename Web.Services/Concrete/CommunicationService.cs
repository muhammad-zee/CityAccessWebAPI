using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using ElmahCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Twilio;
using Twilio.AspNet.Common;
using Twilio.Jwt.AccessToken;
using Twilio.Rest.Chat.V2.Service;
using Twilio.Rest.Chat.V2.Service.Channel;
using Twilio.Rest.Video.V1;
using Twilio.Types;
using Web.Data.Models;
using Web.DLL.Generic_Repository;
using Web.Model;
using Web.Model.Common;
using Web.Services.Enums;
using Web.Services.Helper;
using Web.Services.Interfaces;
using static Twilio.Rest.Chat.V2.Service.ChannelResource;

namespace Web.Services.Concrete
{

    public class CommunicationService : ICommunicationService
    {
        private readonly string Twilio_AccountSid;
        private readonly string Twilio_AuthToken;

        private readonly string SendGrid_ApiKey;
        private readonly string FromEmail;

        private readonly string Twilio_ChatServiceSid;
        private readonly string Twilio_ChatPushCredentialSid;
        private readonly string Twilio_ChatApiKey;
        private readonly string Twilio_ChatApiKeySecret;

        private readonly string s3accessKey;
        private readonly string s3secretKey;
        private readonly string s3BucketName;


        private string origin = "";


        private IRepository<Role> _role;
        private IRepository<UserRole> _userRole;


        private string _RootPath;
        private IConfiguration _config;
        private RAQ_DbContext _dbContext;
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<ConversationChannel> _conversationChannelsRepo;
        private readonly IRepository<ChatSetting> _chatSettingRepo;
        private readonly IRepository<CommunicationLog> _communicationLog;
        private readonly IRepository<ConversationParticipant> _conversationParticipantsRepo;
        private IRepository<UsersRelation> _userRelationRepo;
        private IRepository<ServiceLine> _serviceLineRepo;
        private IRepository<Department> _dptRepo;
        private IRepository<Organization> _orgRepo;
        private IRepository<ControlList> _uclRepo;
        private IRepository<ControlListDetail> _uclDetailRepo;

        private IActiveCodeHelperService _activeCodeHelperService;


        private string _encryptionKey = "";

        public CommunicationService(IConfiguration config,
            RAQ_DbContext dbContext,
            IRepository<User> userRepo,
            IRepository<ConversationChannel> conversationChannelsRepo,
            IRepository<ChatSetting> chatSettingRepo,
            IRepository<ConversationParticipant> conversationParticipantsRepo,
            IRepository<Role> role,
            IRepository<CommunicationLog> communicationlog,
            IRepository<UserRole> userRole,
            IRepository<UsersRelation> userRelationRepo,
            IRepository<ServiceLine> serviceLineRepo,
            IRepository<Department> dptRepo,
            IRepository<Organization> orgRepo,
            IRepository<ControlList> uclRepo,
            IRepository<ControlListDetail> uclDetailRepo,
            IActiveCodeHelperService activeCodeHelperService
            )
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

            this.s3accessKey = this._config["AmazonS3:AccessKey"].ToString();
            this.s3secretKey = this._config["AmazonS3:SecretKey"].ToString();
            this.s3BucketName = this._config["AmazonS3:s3BucketName"].ToString();


            this._RootPath = this._config["FilePath:Path"].ToString();

            this.origin = this._config["Twilio:CallbackDomain"].ToString();

            this._userRepo = userRepo;
            this._conversationChannelsRepo = conversationChannelsRepo;
            this._conversationParticipantsRepo = conversationParticipantsRepo;
            this._chatSettingRepo = chatSettingRepo;
            this._communicationLog = communicationlog;

            this._role = role;
            this._userRole = userRole;
            this._userRelationRepo = userRelationRepo;
            this._serviceLineRepo = serviceLineRepo;
            this._dptRepo = dptRepo;
            this._orgRepo = orgRepo;
            this._uclRepo = uclRepo;
            this._uclDetailRepo = uclDetailRepo;

            this._activeCodeHelperService = activeCodeHelperService;


            this._encryptionKey = this._config["Encryption:key"].ToString();
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
                var messageResource = await Twilio.Rest.Api.V2010.Account.MessageResource.CreateAsync(
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
            try
            {
                TwilioClient.Init(this.Twilio_AccountSid, this.Twilio_AuthToken);
                var Notify = Twilio.Rest.Conversations.V1.Service.Conversation.MessageResource.Create(
                                           author: msg.author,
                                           body: Encryption.encryptData(msg.body, this._encryptionKey),
                                           attributes: msg.attributes,
                                           pathChatServiceSid: Twilio_ChatServiceSid,
                                           pathConversationSid: msg.channelSid
                                           );
                //var Notify = Twilio.Rest.Chat.V2.Service.Channel.MessageResource.Create(
                //                           from: msg.author,
                //                           body: msg.body,
                //                           attributes: msg.attributes,
                //                           pathServiceSid: this.Twilio_ChatServiceSid,
                //                           pathChannelSid: msg.channelSid
                //                           );
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Notification Sent", Body = Notify };
            }
            catch (Exception ex)
            {
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Notification Not Sent", Body = ex };
            }
        }
        public BaseResponse ConversationCallbackEvent(string EventType)
        {
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Event Received" };
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
            response.Message = "Notificaiton channel saved successfully.";
            response.Body = new { UserId = UserId, UserChannelSid = ChannelSid };
            return response;
        }
        public BaseResponse saveConversationChannel(ConversationChannelVM channel)
        {
            BaseResponse response = new BaseResponse();
            if (this._userRepo.Table.Count(u => u.IsDeleted == false && u.IsActive == true && u.UserUniqueId == channel.UniqueName) > 0)
            {
                //if channel is user's notification channel and there is and other user in that channel or notification channel is somehow saved in db
                var ch = ChannelResource.Fetch(pathServiceSid: this.Twilio_ChatServiceSid, pathSid: channel.ChannelSid);
                var members = MemberResource.Read(pathServiceSid: this.Twilio_ChatServiceSid, pathChannelSid: channel.ChannelSid);
                foreach (var m in members)
                {
                    if (m.Identity != channel.UniqueName)
                    {
                        var delete = MemberResource.Delete(pathServiceSid: this.Twilio_ChatServiceSid, pathChannelSid: channel.ChannelSid, pathSid: m.Sid);
                    }

                }
                var dbChannel = this._conversationChannelsRepo.Table.FirstOrDefault(ch => ch.ChannelSid == channel.ChannelSid);
                if (dbChannel != null)
                {
                    var participants = this._conversationParticipantsRepo.Table.Where(p => p.ConversationChannelIdFk == dbChannel.ConversationChannelId);
                    this._conversationParticipantsRepo.DeleteRange(participants);
                    this._conversationChannelsRepo.Delete(dbChannel);
                }
                response.Status = HttpStatusCode.OK;
            }
            else
            {
                if (!channel.IsGroup.Value && !channel.FriendlyName.Contains("S_"))
                {
                    channel.IsGroup = true;
                }
                var channelSetting = this._conversationChannelsRepo.Table.Count(ch => ch.ChannelSid == channel.ChannelSid && ch.IsDeleted != true) == 0;
                if (channelSetting)
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
                    response.Message = "Notificaiton channel saved successfully.";
                    response.Body = newChannel;
                }
                else
                {
                    response.Status = HttpStatusCode.OK;
                    response.Message = "Channel Already Exists";
                }
            }

            return response;
        }
        public BaseResponse saveConversationChannelParticipants(ConversationChannelParticipantsVM model)
        {
            BaseResponse response = new BaseResponse();

            var channel = this._conversationChannelsRepo.Table.FirstOrDefault(ch => ch.ChannelSid == model.ChannelSid);
            if (channel != null)
            {
                List<ConversationParticipant> channelParticipantsList = new List<ConversationParticipant>();
                ConversationParticipant newParticipant = null;
                User user = null;
                foreach (var p in model.Participants)
                {
                    user = this._userRepo.Table.FirstOrDefault(u => u.IsDeleted != true && u.IsActive == true && u.UserUniqueId == p.UniqueName);
                    if (user != null)
                    {
                        bool participantExists = this._conversationParticipantsRepo.Table.Count(cp => cp.IsDeleted != true && cp.UserIdFk == user.UserId && cp.ConversationChannelIdFk == channel.ConversationChannelId && cp.UniqueName == p.UniqueName) > 0;
                        if (!participantExists)
                        {
                            newParticipant = new ConversationParticipant()
                            {
                                FriendlyName = user.FirstName + " " + user.LastName,
                                UniqueName = p.UniqueName,
                                UserIdFk = user.UserId,
                                ConversationChannelIdFk = channel.ConversationChannelId,
                                IsAdmin = p.IsAdmin,
                                CreatedBy = model.CreatedBy,
                                CreatedDate = DateTime.UtcNow,
                                IsDeleted = false

                            };

                            channelParticipantsList.Add(newParticipant);
                        }
                    }
                }
                if (channelParticipantsList.Count() > 0)
                {
                    this._conversationParticipantsRepo.Insert(channelParticipantsList);
                    this._activeCodeHelperService.MemberAddedToConversationChannel(channelParticipantsList, model.ChannelSid);
                }



                response.Status = HttpStatusCode.OK;
                response.Message = "Channel Participants Saved";
                //response.Body = newChannel;
            }
            else
            {
                response.Status = HttpStatusCode.OK;
                response.Message = "channel not saved yet";
            }
            return response;
        }
        public BaseResponse getConversationChannels(int UserId)
        {
            BaseResponse response = new BaseResponse();

            var channels = this._dbContext.LoadStoredProcedure("md_getConversationChannelsByUserId")
            .WithSqlParam("@pUserId", UserId)
            .ExecuteStoredProc<ConversationChannelsListVM>();
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

        public BaseResponse deleteAllChannels(string key)
        {
            if (key == "qw4hddqcrg")
            {
                TwilioClient.Init(this.Twilio_AccountSid, this.Twilio_AuthToken);
                var channels = ChannelResource.Read(pathServiceSid: this.Twilio_ChatServiceSid, pageSize: 50);
                var totalChannels = channels.Count();
                if (totalChannels > 0)
                {

                    foreach (var ch in channels)
                    {
                        var delete = ChannelResource.Delete(pathServiceSid: this.Twilio_ChatServiceSid, pathSid: ch.Sid);
                        var dbChannel = this._conversationChannelsRepo.Table.FirstOrDefault(c => c.ChannelSid == ch.Sid);
                        if (dbChannel != null)
                        {
                            //dbChannel.IsDeleted = true;
                            var channelParticipants = this._conversationParticipantsRepo.Table.Where(p => p.ConversationChannelIdFk == dbChannel.ConversationChannelId);
                            //foreach (var p in channelParticipants)
                            //{
                            //    p.IsDeleted = true;
                            //}
                            this._conversationParticipantsRepo.DeleteRange(channelParticipants);
                            this._conversationChannelsRepo.Delete(dbChannel);
                        }
                    }
                }
                else
                {
                    var convParticipantTable = this._conversationParticipantsRepo.Table.AsEnumerable();
                    if (convParticipantTable.Count() > 0)
                    {
                        this._conversationParticipantsRepo.DeleteRange(convParticipantTable);
                    }
                    var convTable = this._conversationChannelsRepo.Table.AsEnumerable();
                    if (convTable.Count() > 0)
                    {
                        this._conversationChannelsRepo.DeleteRange(convTable);
                    }


                    return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Conversations channels deleted completely from Twilio Server & Database" };
                }
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Hit API Again to delete All channels" };
            }
            else
            {
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = "Incorrect Key" };
            }

        }
        public BaseResponse deleteConflictedConversationChannel(string channelSid)
        {

            var dbChannel = this._conversationChannelsRepo.Table.FirstOrDefault(c => c.ChannelSid == channelSid);
            if (dbChannel != null)
            {
                //dbChannel.IsDeleted = true;
                var channelParticipants = this._conversationParticipantsRepo.Table.Where(p => p.ConversationChannelIdFk == dbChannel.ConversationChannelId);
                //foreach (var p in channelParticipants)
                //{
                //    p.IsDeleted = true;
                //}
                this._conversationParticipantsRepo.DeleteRange(channelParticipants);
                this._conversationChannelsRepo.Delete(dbChannel);
            }

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Hit API Again to delete All channels" };

        }




        public BaseResponse deleteConversationChannel(string ChannelSid, int UserId)
        {
            var dbChannel = this._conversationChannelsRepo.Table.FirstOrDefault(c => c.ChannelSid == ChannelSid);
            if (dbChannel != null)
            {
                dbChannel.IsDeleted = true;
                dbChannel.ModifiedBy = UserId;
                dbChannel.ModifiedDate = DateTime.UtcNow;

                this._conversationChannelsRepo.Update(dbChannel);
                var channelParticipants = this._conversationParticipantsRepo.Table.Where(p => p.ConversationChannelIdFk == dbChannel.ConversationChannelId);
                foreach (var p in channelParticipants)
                {
                    p.IsDeleted = true;
                    p.ModifiedBy = UserId;
                    p.ModifiedDate = DateTime.UtcNow;
                }
                this._conversationParticipantsRepo.Update(channelParticipants);
                //this._conversationParticipantsRepo.DeleteRange(channelParticipants);
                //this._conversationChannelsRepo.Delete(dbChannel);
            }
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Conversation Deleted" };
        }

        public BaseResponse deleteConversationParticipant(string ChannelSid, string ParticipantUniqueName, int UserId)
        {
            var dbChannel = this._conversationChannelsRepo.Table.FirstOrDefault(c => c.ChannelSid == ChannelSid && c.IsDeleted != true);
            if (dbChannel != null)
            {
                var participant = this._conversationParticipantsRepo.Table.FirstOrDefault(p => p.UniqueName == ParticipantUniqueName && p.ConversationChannelIdFk == dbChannel.ConversationChannelId && p.IsDeleted != true);
                if (participant != null)
                {
                    participant.IsDeleted = true;
                    participant.ModifiedBy = UserId;
                    participant.ModifiedDate = DateTime.UtcNow;
                    this._conversationParticipantsRepo.Update(participant);
                }
            }
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Participant Removed" };
        }
        public BaseResponse addUserToChannelUsingApi(string ChannelUniqueName, string ParticipantUniqueName, int UserId)
        {
            var dbChannel = this._conversationChannelsRepo.Table.FirstOrDefault(c => c.UniqueName == ChannelUniqueName && c.IsDeleted != true);
            if (dbChannel != null)
            {
                var participant = this._userRepo.Table.FirstOrDefault(u => u.IsDeleted != true && u.UserUniqueId == ParticipantUniqueName);
                TwilioClient.Init(this.Twilio_AccountSid, this.Twilio_AuthToken);

                //var conversations = ConversationResource.Fetch(pathSid: dbChannel.ChannelSid);
                var channel = ChannelResource.Fetch(pathServiceSid: this.Twilio_ChatServiceSid, pathSid: dbChannel.ChannelSid);


                //         var addParticipant = ParticipantResource.Create(
                //    identity: ParticipantUniqueName,
                //    pathConversationSid: conversations.Sid
                //);
                var addParticipant = MemberResource.Create(
                            identity: ParticipantUniqueName,
                            pathServiceSid: this.Twilio_ChatServiceSid,
                            pathChannelSid: channel.Sid
                );

                var newParticipant = new ConversationChannelParticipantsVM
                {
                    UserId = participant.UserId,
                    Participants = new List<ParticipantVM> { new ParticipantVM { UniqueName = participant.UserUniqueId } },
                    ChannelSid = channel.Sid,
                    CreatedBy = UserId
                };
                var insertParticipantIntoDb = this.saveConversationChannelParticipants(newParticipant);
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Participant Added" };
            }
            else
            {
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = "Channel Not Found" };
            }

        }
        public BaseResponse updateConversationGroup(string FriendlyName, string ChannelSid)
        {
            var dbChannel = this._conversationChannelsRepo.Table.FirstOrDefault(c => c.ChannelSid == ChannelSid && c.IsDeleted != true);
            if (dbChannel != null)
            {
                dbChannel.FriendlyName = FriendlyName;
                dbChannel.ModifiedBy = ApplicationSettings.UserId;
                dbChannel.ModifiedDate = DateTime.UtcNow;
                this._conversationChannelsRepo.Update(dbChannel);
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Group Updated Successfully" };
            }
            else
            {
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = "Channel Not Found" };
            }

        }
        public BaseResponse updateConversationUserSid(string UserSid, int userId)
        {
            BaseResponse response = new BaseResponse();

            var user = this._userRepo.Table.FirstOrDefault(u => u.UserId == ApplicationSettings.UserId && !u.IsDeleted);

            //if (string.IsNullOrEmpty(UserSid))
            //{
            //    try
            //    {
            //        var chatUser = this.createConversationUser(user.UserUniqueId, user.FirstName + " " + user.LastName);
            //    }
            //    catch(Exception e)
            //    {

            //    }

            //    //var chatUser = UserResource.Read(pathServiceSid: this.Twilio_ChatServiceSid,)
            //}
            if (!string.IsNullOrEmpty(UserSid))
            {

                if (user != null)
                {
                    user.ConversationUserSid = UserSid;
                    this._userRepo.Update(user);
                }
            }
            response.Status = HttpStatusCode.OK;
            response.Message = "User sid saved successfully.";
            response.Body = new { UserId = ApplicationSettings.UserId, UserChannelSid = UserSid };
            return response;

        }
        public BaseResponse getAllConversationUsers()
        {

            //var chatusers = this._userrepo.table.where(u => u.isdeleted != true && u.isactive == true && !string.isnullorempty(u.userchannelsid));
            var chatUsers = this._dbContext.LoadStoredProcedure("md_getAllConversationUsers")
             .WithSqlParam("@pUserId", ApplicationSettings.UserId)
             .WithSqlParam("@pIsSuperAdmin", ApplicationSettings.isSuperAdmin)
             .ExecuteStoredProc<ChatUsersVM>();
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Chat users found", Body = chatUsers };
        }

        public BaseResponse getAllConversationUsersByOrgId(int orgid)
        {

            //var chatusers = this._userrepo.table.where(u => u.isdeleted != true && u.isactive == true && !string.isnullorempty(u.userchannelsid));
            var chatUsers = this._dbContext.LoadStoredProcedure("md_getAllUsersByOrganizationIdforchat")
             .WithSqlParam("@pOrganizationId", orgid)
             .WithSqlParam("@pIsSuperAdmin", ApplicationSettings.isSuperAdmin)
             .ExecuteStoredProc<ChatUsersVM>();
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Chat users found", Body = chatUsers };
        }

        public ChannelResource createConversationChannel(string FriendlyName, string UniqueName, string Attrubutes)
        {
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; //TLS 1.2
            TwilioClient.Init(this.Twilio_AccountSid, this.Twilio_AuthToken);
            string userUniqueId = this._userRepo.Table.Where(u => u.IsDeleted != true && u.UserId == ApplicationSettings.UserId && !string.IsNullOrEmpty(u.ConversationUserSid)).Select(x => x.UserUniqueId).FirstOrDefault();
            userUniqueId = string.IsNullOrEmpty(userUniqueId) ? "system" : userUniqueId;
            var channel = ChannelResource.Create(pathServiceSid: this.Twilio_ChatServiceSid, friendlyName: FriendlyName, uniqueName: UniqueName, attributes: Attrubutes, type: ChannelTypeEnum.Private, createdBy: userUniqueId);
            //var channel = Twilio.Rest.Conversations.V1.ConversationResource.Create(servives: this.Twilio_ChatServiceSid, friendlyName: FriendlyName, uniqueName: UniqueName);
            return channel;
        }

        public ChannelResource createNotificationChannel(string FriendlyName, string userUniqueId)
        {
            try
            {

                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; //TLS 1.2
                TwilioClient.Init(this.Twilio_AccountSid, this.Twilio_AuthToken);
                var channel = ChannelResource.Create(pathServiceSid: this.Twilio_ChatServiceSid, friendlyName: FriendlyName, uniqueName: userUniqueId, type: ChannelTypeEnum.Private);
                //var channel = Twilio.Rest.Conversations.V1.ConversationResource.Create(servives: this.Twilio_ChatServiceSid, friendlyName: FriendlyName, uniqueName: UniqueName);
                return channel;
            }
            catch (Exception e)
            {
                return null;
            }

        }

        public UserResource createConversationUser(string Identity, string FriendlyName)
        {
            try
            {

                TwilioClient.Init(this.Twilio_AccountSid, this.Twilio_AuthToken);
                var user = UserResource.Create(pathServiceSid: this.Twilio_ChatServiceSid, identity: Identity, friendlyName: FriendlyName);
                return user;
            }
            catch (Exception e)
            {
                return null;
            }

        }
        public MemberResource addNewUserToConversationChannel(string ChannelSid, string ParticipantUniqueName)
        {
            try
            {
                TwilioClient.Init(this.Twilio_AccountSid, this.Twilio_AuthToken);
                var addParticipant = MemberResource.Create(identity: ParticipantUniqueName, pathServiceSid: this.Twilio_ChatServiceSid, pathChannelSid: ChannelSid);
                return addParticipant;
            }
            catch (Exception ex)
            {

                return null;
            }

        }
        public bool DeleteUserToConversationChannel(string ChannelSid)
        {
            try
            {
                TwilioClient.Init(this.Twilio_AccountSid, this.Twilio_AuthToken);
                var Participants = MemberResource.Read(pathServiceSid: this.Twilio_ChatServiceSid, pathChannelSid: ChannelSid);
                bool delete = false;
                var channelId = this._conversationChannelsRepo.Table.Where(x => x.ChannelSid == ChannelSid && !x.IsDeleted).Select(x => x.ConversationChannelId).FirstOrDefault();
                var participantsList = new List<ConversationParticipant>();
                foreach (var item in Participants)
                {
                    var participant = new ConversationParticipant();
                    delete = MemberResource.Delete(pathServiceSid: this.Twilio_ChatServiceSid, pathChannelSid: ChannelSid, pathSid: item.Sid);
                    participant = this._conversationParticipantsRepo.Table.Where(x => x.ConversationChannelIdFk == channelId && !x.IsDeleted).FirstOrDefault();
                    if (participant != null)
                    {
                        participant.IsDeleted = true;
                        participant.ModifiedBy = ApplicationSettings.UserId;
                        participant.ModifiedDate = DateTime.UtcNow;
                        participantsList.Add(participant);
                    }
                }
                if (participantsList.Count > 0)
                    this._conversationParticipantsRepo.Update(participantsList);

                return delete;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
        public BaseResponse getConversationUsersStatus(string UserSid)
        {
            List<ConversationUserStatus> statusList = new();
            if (!string.IsNullOrEmpty(UserSid))
            {
                string[] userSidList = UserSid.Split(",");
                var status = false;
                foreach (var sid in userSidList)
                {
                    if (sid != "")
                    {
                        status = conversationUserIsOnline(sid);
                        statusList.Add(new ConversationUserStatus() { ConversationUserSid = sid, IsOnline = status });
                    }
                }
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Chat users found", Body = statusList };
            }
            else
            {
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "User not found", Body = statusList };
            }
        }
        public BaseResponse GetCurrentConversationParticipants(string channelSid)
        {
            if (!string.IsNullOrEmpty(channelSid) && !string.IsNullOrWhiteSpace(channelSid))
            {
                var ConversationParticipants = this._dbContext.LoadStoredProcedure("md_getCurrentConversationParticipants")
                    .WithSqlParam("@channelSid", channelSid)
                    .ExecuteStoredProc<ConversationParticipant>();
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Returned", Body = ConversationParticipants };
            }
            else
            {
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Channel Sid Null or Empty" };
            }
        }
        public bool conversationUserIsOnline(string UserSid)
        {
            try
            {
                TwilioClient.Init(this.Twilio_AccountSid, this.Twilio_AuthToken);
                var user = UserResource.Fetch(this.Twilio_ChatServiceSid, UserSid);
                if (user.IsOnline == null)
                {
                    return false;
                }
                else
                {
                    return user.IsOnline.Value;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public BaseResponse createOrRemoveGroupMemberAsAdmin(bool isAdmin, string uniqueName, string channleSid)
        {
            HttpStatusCode status = HttpStatusCode.NotFound;
            string message = "Member not found.";
            var row = (from cp in this._conversationParticipantsRepo.Table
                       join c in this._conversationChannelsRepo.Table on cp.ConversationChannelIdFk equals c.ConversationChannelId
                       where cp.UniqueName == uniqueName && c.ChannelSid == channleSid && !cp.IsDeleted && !c.IsDeleted
                       select cp).FirstOrDefault();

            if (row != null)
            {
                status = HttpStatusCode.OK;
                message = isAdmin ? $"{row.FriendlyName} is admin now." : $"{row.FriendlyName} is not an admin now.";
                row.IsAdmin = isAdmin;
                row.ModifiedBy = ApplicationSettings.UserId;
                row.ModifiedDate = DateTime.UtcNow;
                this._conversationParticipantsRepo.Update(row);
            }


            return new BaseResponse() { Status = status, Message = message };
        }

        public BaseResponse UploadAttachment(IFormFileCollection file)
        {
            if (file.Count() > 0)
            {
                RegionEndpoint regionEndpoint = RegionEndpoint.USEast2;
                var s3Client = new AmazonS3Client(awsAccessKeyId: this.s3accessKey, awsSecretAccessKey: s3secretKey, region: regionEndpoint);
                var fileTransferUtility = new TransferUtility(s3Client);


                var attachment = file.FirstOrDefault();
                var extension = Path.GetExtension(attachment.FileName);
                string fileActualName = attachment.FileName;
                string contentType = attachment.ContentType;
                string fileUniqueName = DateTime.Now.ToString("yyyyMMddHHmmssffff") + "-" + ApplicationSettings.UserId + extension;


                var RootPath = this._RootPath;
                string FilePath = "conversationAttachments";
                var targetPath = Path.Combine(RootPath, FilePath);

                if (!Directory.Exists(targetPath))
                {
                    Directory.CreateDirectory(targetPath);
                }
                targetPath += "/" + $"{fileUniqueName}";
                using (var ms = new MemoryStream())
                {
                    attachment.CopyTo(ms);
                    var fileBytes = ms.ToArray();
                    using (FileStream fs = new FileStream(targetPath, FileMode.Create, FileAccess.Write))
                    {
                        fs.Write(fileBytes);
                    }
                }
                try
                {
                    var fileTransferUtilityRequest = new TransferUtilityUploadRequest
                    {
                        BucketName = this.s3BucketName,
                        FilePath = targetPath,
                        StorageClass = S3StorageClass.StandardInfrequentAccess,
                        PartSize = 6291456, // 6 MB.  
                        Key = fileUniqueName,//filename which u want to save in bucket
                        CannedACL = S3CannedACL.PublicRead,
                        //InputStream = fs,
                    };
                    fileTransferUtility.UploadAsync(fileTransferUtilityRequest).GetAwaiter().GetResult();
                    File.Delete(targetPath);
                    //To upload without asynchronous
                    //fileTransferUtility.Upload(filePath, bucketName, "SampleAudio.wav");
                    //fileTransferUtility.Dispose();
                }
                catch (AmazonS3Exception ex)
                {

                }
            //
                //origin = origin.Contains("ngrok.io") ? "http://localhost:60113" : origin;
                //var MediaUrl = $"{origin}/{FilePath}/{fileUniqueName}";
                 var MediaUrl = $"https://{s3BucketName}.s3.amazonaws.com/{fileUniqueName}";
                //string extension = Path.GetExtension(ImageFile.FileName);

                ////ImageFile.SaveAs(path + filename);
                ///       
                return new BaseResponse
                {
                    Status = HttpStatusCode.OK,
                    Message = "File Uploaded...!",
                    Body = new
                    {
                        media = new ConversationAttachmentVM { fileName = fileActualName, contentType = contentType, mediaUrl = MediaUrl }
                    }
                };
            }
            return new BaseResponse
            {
                Status = HttpStatusCode.BadRequest,
                Message = "File Not Uploaded...!",
            };



        }


        #endregion


        #region [Video Call]
        public BaseResponse generateVideoCallToken(string identity)
        {
            BaseResponse response = new BaseResponse();
            TwilioClient.Init(this.Twilio_AccountSid, this.Twilio_AuthToken);



            // Create a video grant for the token
            var grant = new VideoGrant();
            grant.Room = "321654987";
            var grants = new HashSet<IGrant> { grant };

            // Create an Access Token generator
            var token = new Token(this.Twilio_AccountSid,
                this.Twilio_ChatApiKey,
                this.Twilio_ChatApiKeySecret,
                identity: identity,
                grants: grants,
                expiration: DateTime.Now.AddHours(2));

            response.Status = HttpStatusCode.OK;
            response.Message = "Token Generated";
            response.Body = new { identity = identity, token = token.ToJwt() };
            return response;
        }

        public BaseResponse VideoRoomCallbackEvent(string EventType)
        {
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Event Received" };
        }

        public BaseResponse dialVideoCall(DialVideoCallVM model)
        {
            string UserChannelSid = this._userRepo.Table.FirstOrDefault(u => u.UserUniqueId == model.to && u.IsDeleted != true).UserChannelSid;

            var attributes = JsonConvert.SerializeObject(new Dictionary<string, Object>()
                                    {
                                        {"from", model.from},
                                        {"isVideo", model.isVideo},
                                        {"roomName", model.roomName},
                                        {"roomSid", model.roomSid},
                                        {"type", "VideoCall"}
                                    }, Formatting.Indented);
            this.sendPushNotification(new ConversationMessageVM
            {
                author = AuthorEnums.VideoCall.ToString(),
                body = "Video Call",
                attributes = attributes,
                channelSid = UserChannelSid
            });
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Call Dialed" };
        }
        public BaseResponse pushNotification(PushNotificationVM model)
        {
            //var UserChannelSid = this._userRepo.Table.Where(u => model.UserIds.Contains(u.UserId) && u.UserChannelSid != null && u.IsDeleted != true).Select(x => x.UserChannelSid).ToList();

            var attributes = JsonConvert.SerializeObject(new Dictionary<string, Object>()
                                    {
                                        { "id", model.Id},
                                        { "orgId", model.OrgId},
                                        { "type", model.Type },
                                        { "channelIsActive", model.ChannelIsActive },
                                        { "fieldName", model.FieldName },
                                        { "fieldValue", model.FieldValue },
                                        { "fieldDataType", model.FieldDataType },
                                        { "channelSid", model.ChannelSid},
                                        { "forEmsLocationUpdate", model.ForEmsLocationUpdate },
                                        { "routerLink1", model.RouteLink1},
                                        { "routerLink2", model.RouteLink2 },
                                        { "routerLink3", model.RouteLink3 },
                                        { "routerLink4", model.RouteLink4 },
                                        { "routerLink5", model.RouteLink5 },
                                    }, Formatting.Indented);
            foreach (var item in model.UserChannelSid)
            {
                if (item != null)
                {
                    this.sendPushNotification(new ConversationMessageVM
                    {
                        author = model.From,
                        body = model.Msg,
                        attributes = attributes,
                        channelSid = item
                    });
                }
            }

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Message Send" };
        }
        public BaseResponse incomingCallEvent(string roomSid, string eventType, string channelSid)
        {
            var attributes = JsonConvert.SerializeObject(new Dictionary<string, Object>()
                                    {
                                        { "eventType", eventType},
                                        { "type", "VideoCallEvent"}
                                    }, Formatting.Indented);
            if (roomSid != "" && eventType == CallEventEnums.Rejected.ToString())
            {
                var room = RoomResource.Update(
                status: RoomResource.RoomStatusEnum.Completed,
                pathSid: roomSid
                );
            }
            var push = this.sendPushNotification(new ConversationMessageVM
            {
                author = AuthorEnums.VideoCall.ToString(),
                body = eventType,
                attributes = attributes,
                channelSid = channelSid
            });
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Event Sent" };
        }
        #endregion


        #region Chat Settings

        public BaseResponse GetTone(int Id)
        {

            var UCLDetails = (from ucl in _uclRepo.Table
                              join ucld in _uclDetailRepo.Table on ucl.ControlListId equals ucld.ControlListIdFk
                              where ucl.ControlListId == Id && ucl.IsDeleted == false && ucld.IsDeleted == false
                              && ucl.ControlListIsActive == true && ucld.IsActive == true
                              select new
                              {
                                  ParentId = ucl.ControlListId,
                                  ParetntTitle = ucl.ControlListTitle,
                                  Id = ucld.ControlListDetailId,
                                  Title = ucld.Title,
                                  Description = ucld.Description,
                                  ImageHtml = ucld.ImageHtml
                              }).ToList();

            var returnObj = new Dictionary<string, object>();
            foreach (var item in UCLDetails)
            {
                var paths = new List<object>();
                string path = this._RootPath + item.ImageHtml;
                if (Directory.Exists(path))
                {
                    DirectoryInfo AttachFiles = new DirectoryInfo(path);
                    foreach (var itemfile in AttachFiles.GetFiles())
                    {
                        paths.Add(new { path = path.Replace(this._RootPath, "") + '/' + itemfile.Name, name = itemfile.Name.Split(".")[0] });
                    }

                    returnObj.Add(item.Title.Replace(" ", ""), paths);
                }
            }
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Returned", Body = returnObj };
        }

        public BaseResponse GetChatSetting(int UserId)
        {
            var chatData = this._chatSettingRepo.Table.FirstOrDefault(x => x.UserIdFk == ApplicationSettings.UserId && x.IsDeleted != true);
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Chat data", Body = chatData };
        }
        #endregion

        public BaseResponse addChatSettings(AddChatSettingVM channel)
        {
            var channelSetting = this._chatSettingRepo.Table.FirstOrDefault(ch => ch.UserIdFk == channel.UserIdFk && ch.IsDeleted != true);
            BaseResponse response = new BaseResponse();
            if (channelSetting == null)
            {
                var newChannel = new ChatSetting
                {
                    //ChatSettingId = channel.ChatSettingId,
                    UserIdFk = channel.UserIdFk,
                    IsMute = channel.IsMute,
                    CallSound = channel.CallSound.Replace(this._RootPath, ""),
                    MessageSound = channel.MessageSound.Replace(this._RootPath, ""),
                    Wallpaper = channel.Wallpaper,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = ApplicationSettings.UserId,
                    FontSize = channel.FontSize,
                    IsDeleted = false,
                };
                if (channel.WallpaperObj != null && !string.IsNullOrEmpty(channel.WallpaperObj.Base64Str))
                {
                    var GetUserInfo = _userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && x.IsDeleted == false).Select(x => new { x.UserId, x.FirstName, x.LastName }).FirstOrDefault();
                    //var outPath = Directory.GetCurrentDirectory();
                    var RootPath = this._RootPath;
                    string FilePath = "Wallpapers";
                    var targetPath = Path.Combine(RootPath, FilePath);

                    if (!Directory.Exists(targetPath))
                    {
                        Directory.CreateDirectory(targetPath);
                    }
                    var UserImageByte = Convert.FromBase64String(channel.WallpaperObj.Base64Str.Split("base64,")[1]);
                    targetPath += "/" + $"{GetUserInfo.FirstName}-{GetUserInfo.LastName}_{GetUserInfo.UserId}_{DateTime.UtcNow.ToString("yyyyMMddHHmmssffff")}.png";
                    using (FileStream fs = new FileStream(targetPath, FileMode.Create, FileAccess.Write))
                    {
                        fs.Write(UserImageByte);
                    }
                    newChannel.Wallpaper = targetPath.Replace(RootPath, "").Replace("\\", "/");
                }
                this._chatSettingRepo.Insert(newChannel);
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Chat settings saved successfully.", Body = newChannel };
            }
            else
            {
                //ChatSettingId = channel.ChatSettingId,
                channelSetting.UserIdFk = channel.UserIdFk;
                channelSetting.IsMute = channel.IsMute;
                channelSetting.CallSound = channel.CallSound.Replace(this._RootPath, "");
                channelSetting.MessageSound = channel.MessageSound.Replace(this._RootPath, "");
                channelSetting.Wallpaper = channel.Wallpaper;
                channelSetting.CreatedDate = DateTime.UtcNow;
                channelSetting.CreatedBy = ApplicationSettings.UserId;
                channelSetting.FontSize = channel.FontSize;
                channelSetting.IsDeleted = false;
                if (channel.WallpaperObj != null && !string.IsNullOrEmpty(channel.WallpaperObj.Base64Str))
                {
                    var GetUserInfo = _userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && x.IsDeleted == false).Select(x => new { x.UserId, x.FirstName, x.LastName }).FirstOrDefault();
                    //var outPath = Directory.GetCurrentDirectory();
                    var RootPath = this._RootPath;
                    string FilePath = "Wallpapers";
                    var targetPath = Path.Combine(RootPath, FilePath);

                    if (!Directory.Exists(targetPath))
                    {
                        Directory.CreateDirectory(targetPath);
                    }
                    else
                    {
                        var allFiles = new DirectoryInfo(targetPath).GetFiles();
                        var existingWallpaper = allFiles.Where(i => i.FullName.Contains($"{GetUserInfo.FirstName}-{GetUserInfo.LastName}_{GetUserInfo.UserId}"));
                        foreach (var file in existingWallpaper)
                        {
                            file.Delete();
                        }

                    }
                    var UserImageByte = Convert.FromBase64String(channel.WallpaperObj.Base64Str.Split("base64,")[1]);
                    targetPath += "/" + $"{GetUserInfo.FirstName}-{GetUserInfo.LastName}_{GetUserInfo.UserId}_{DateTime.UtcNow.ToString("yyyyMMddHHmmssffff")}.png";
                    using (FileStream fs = new FileStream(targetPath, FileMode.Create, FileAccess.Write))
                    {
                        fs.Write(UserImageByte);
                    }
                    channelSetting.Wallpaper = targetPath.Replace(RootPath, "").Replace("\\", "/");
                }
                this._chatSettingRepo.Update(channelSetting);
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Chat Setting Updated Successfully", Body = channelSetting };
            }
        }


        public BaseResponse refreshConsversationUsers(string key)
        {
            if (key == "qw4hddqcrg")
            {
                var dbUsers = this._userRepo.Table.Where(x => x.IsDeleted == false && x.IsActive == true /*&& !string.IsNullOrEmpty(x.ConversationUserSid)*/).ToList();
                List<User> usersToUpdate = new();
                TwilioClient.Init(this.Twilio_AccountSid, this.Twilio_AuthToken);
                foreach (var u in dbUsers)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(u.ConversationUserSid))
                        {
                            var newUser = this.createConversationUser(u.UserUniqueId, $"{u.FirstName} {u.LastName}");
                            if (newUser != null)
                            {
                                u.ConversationUserSid = newUser.Sid;
                            }
                            else
                            {
                                u.ConversationUserSid = "";
                            }

                            usersToUpdate.Add(u);
                        }
                        else
                        {
                            var twilioUser = UserResource.Fetch(pathServiceSid: this.Twilio_ChatServiceSid, pathSid: u.ConversationUserSid);
                        }

                    }
                    catch (Exception e)
                    {
                        var newUser = this.createConversationUser(u.UserUniqueId, $"{u.FirstName} {u.LastName}");
                        if (newUser != null)
                        {
                            u.ConversationUserSid = newUser.Sid;
                        }
                        else
                        {
                            u.ConversationUserSid = "";
                        }

                        usersToUpdate.Add(u);
                    }

                    //var createUser = UserResource.Create(pathServiceSid: this.Twilio_ChatServiceSid, identity: Identity, friendlyName: FriendlyName);
                }
                if (usersToUpdate.Count() > 0)
                {
                    this._userRepo.Update(usersToUpdate);
                }
                return new BaseResponse { Status = HttpStatusCode.OK, Message = "conversation users refreshed" };
            }
            else
            {
                return new BaseResponse { Status = HttpStatusCode.OK, Message = "Incorrect Key" };
            }
        }

        public BaseResponse updateConsversationUserSidFromTwilio(string key)
        {
            if (key == "qw4hddqcrg")
            {
                var dbUsersList = this._userRepo.Table.Where(x => x.IsDeleted == false && x.IsActive == true && string.IsNullOrEmpty(x.ConversationUserSid)).ToList();
                User dbUser = null;
                List<User> usersToUpdate = new();
                System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072; //TLS 1.2
                TwilioClient.Init(Twilio_AccountSid, Twilio_AuthToken);

                var users = UserResource.Read(
                    pathServiceSid: Twilio_ChatServiceSid,
                    limit: 1000
                );
                foreach (var user in users)
                {
                    dbUser = dbUsersList.FirstOrDefault(u => u.UserUniqueId == user.Identity);
                    if (dbUser != null)
                    {
                        if (string.IsNullOrEmpty(dbUser.ConversationUserSid))
                        {

                            dbUser.ConversationUserSid = user.Sid;
                            usersToUpdate.Add(dbUser);
                        }

                    }
                }
                if (usersToUpdate.Count() > 0)
                {
                    this._userRepo.Update(usersToUpdate);
                }

                return new BaseResponse { Status = HttpStatusCode.OK, Message = "User Sids Updated", Body = new { TwilioUsers = users.Count(), UpdatedUsers = usersToUpdate.Count() } };
            }
            else
            {
                return new BaseResponse { Status = HttpStatusCode.OK, Message = "Incorrect Key" };
            }
        }



        public BaseResponse SaveCommunicatonLog(CommunicationLogVM log)
        {
            var communicationLog = this._communicationLog.Table.FirstOrDefault(ch => ch.CommunicationLogId == log.CommunicationLogId && ch.IsDelete != true);
            BaseResponse response = new BaseResponse();
            if (communicationLog == null)
            {
                var newLog = new CommunicationLog
                {
                    //ChatSettingId = channel.ChatSettingId,
                    Title = log.Title,
                    Description = log.Description,
                    SentFrom = log.SentFrom,
                    SentTo = log.SentTo,
                    ServiceLineIdFk = log.ServiceLineIdFk,
                    LogType = log.LogType,
                    Direction = log.Direction,
                    MediaUrl = log.MediaUrl,
                    UniqueSid = log.UniqueSid,
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true,
                    IsDelete = false
                };
                this._communicationLog.Insert(newLog);
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Communication log saved successfully.", Body = newLog };
            }
            else
            {
                //ChatSettingId = channel.ChatSettingId,
                communicationLog.Title = log.Title;
                communicationLog.Description = log.Description;
                communicationLog.SentFrom = log.SentFrom;
                communicationLog.SentTo = log.SentTo;
                communicationLog.LogType = log.LogType;
                communicationLog.Direction = log.Direction;
                communicationLog.MediaUrl = log.MediaUrl;
                communicationLog.UniqueSid = log.UniqueSid;
                communicationLog.CreatedDate = log.CreatedDate;
                communicationLog.ModifiedDate = log.ModifiedDate;
                communicationLog.IsActive = log.IsActive;
                communicationLog.IsDelete = log.IsDelete;
            }
            this._communicationLog.Update(communicationLog);
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Communication Log Updated Successfully", Body = communicationLog };

        }


        public BaseResponse GetCommunicationLogById(int logId, bool status)
        {
            var communicationLog = this._communicationLog.Table.FirstOrDefault(x => x.CommunicationLogId == logId && x.IsDelete != true && x.IsActive == status);
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Communication Log data", Body = communicationLog };
        }


        public BaseResponse ActiveOrInActiveCommunicationlog(int logId, bool status)
        {
            var communicationLog = this._communicationLog.Table.FirstOrDefault(x => x.CommunicationLogId == logId && x.IsDelete != true);

            if (communicationLog != null)
            {
                communicationLog.IsActive = status;
                communicationLog.ModifiedDate = DateTime.UtcNow;

                _communicationLog.Update(communicationLog);


            }


            return new BaseResponse() { Status = HttpStatusCode.OK, Message = (status ? "Active" : "InActive") + "Successfully", Body = communicationLog };
        }


        public BaseResponse GetAllCommunicationlog(int orgId, string departmentIds, string serviceLineIds, bool showAllVoicemails)
        {
            var communicationLog = this._dbContext.LoadStoredProcedure("md_getCommunicationLog")
            .WithSqlParam("@pOrganizationId", orgId)
            .WithSqlParam("@pDepartmentIds", departmentIds)
            .WithSqlParam("@pServiceLineIds", serviceLineIds)
            .WithSqlParam("@pShowAllVoicemails", showAllVoicemails)
            .WithSqlParam("@pUserId", ApplicationSettings.UserId)
            .ExecuteStoredProc<CommunicationLogVM>();

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "CommunicationLog Data", Body = communicationLog };
        }


        public BaseResponse GetCallLog(int orgId, bool showAllCalls)
        {
            var communicationLog = this._dbContext.LoadStoredProcedure("md_getCallLog")
            .WithSqlParam("@pOrganizationId", orgId)
            .WithSqlParam("@pUserId", ApplicationSettings.UserId)
            .WithSqlParam("@pShowAllCalls", showAllCalls)
            .ExecuteStoredProc<CallLogVM>();

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "CommunicationLog Data", Body = communicationLog };
        }

        public ChannelResource getConversationChannelBySid(string channelSid)
        {
            try
            {
                var channel = ChannelResource.Fetch(pathServiceSid: this.Twilio_ChatServiceSid, pathSid: channelSid);
                return channel;
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }





}
