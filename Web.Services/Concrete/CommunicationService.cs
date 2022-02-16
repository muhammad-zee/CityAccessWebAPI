using ElmahCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
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

        private IRepository<Role> _role;
        private IRepository<UserRole> _userRole;



        private IConfiguration _config;
        private RAQ_DbContext _dbContext;
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<ConversationChannel> _conversationChannelsRepo;
        private readonly IRepository<ConversationParticipant> _conversationParticipantsRepo;
        private IRepository<UsersRelation> _userRelationRepo;
        private IRepository<ServiceLine> _serviceLineRepo;
        private IRepository<Department> _dptRepo;
        private IRepository<Organization> _orgRepo;

        public CommunicationService(IConfiguration config,
            RAQ_DbContext dbContext,
            IRepository<User> userRepo,
            IRepository<ConversationChannel> conversationChannelsRepo,
            IRepository<ConversationParticipant> conversationParticipantsRepo,
            IRepository<Role> role,
            IRepository<UserRole> userRole,
            IRepository<UsersRelation> userRelationRepo,
            IRepository<ServiceLine> serviceLineRepo,
            IRepository<Department> dptRepo,
            IRepository<Organization> orgRepo
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

            this._userRepo = userRepo;
            this._conversationChannelsRepo = conversationChannelsRepo;
            this._conversationParticipantsRepo = conversationParticipantsRepo;


            this._role = role;
            this._userRole = userRole;
            this._userRelationRepo = userRelationRepo;
            this._serviceLineRepo = serviceLineRepo;
            this._dptRepo = dptRepo;
            this._orgRepo = orgRepo;

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
            TwilioClient.Init(this.Twilio_AccountSid, this.Twilio_AuthToken);
            //var Notify = conversationResource.Service.Conversation.MessageResource.Create(
            //                           author: msg.author,
            //                           body: msg.body,
            //                           attributes: msg.attributes,
            //                           pathChatServiceSid: this.Twilio_ChatServiceSid,
            //                           pathConversationSid: msg.channelSid
            //                           );
            var Notify = Twilio.Rest.Chat.V2.Service.Channel.MessageResource.Create(
                                       from: msg.author,
                                       body: msg.body,
                                       attributes: msg.attributes,
                                       pathServiceSid: this.Twilio_ChatServiceSid,
                                       pathChannelSid: msg.channelSid
                                       );
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Notification Sent", Body = Notify };
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
            response.Message = "Notificaiton Channel Saved Successfully";
            response.Body = new { UserId = UserId, UserChannelSid = ChannelSid };
            return response;
        }
        public BaseResponse saveConversationChannel(ConversationChannelVM channel)
        {
            BaseResponse response = new BaseResponse();
            if (!channel.IsGroup.Value && !channel.FriendlyName.Contains("S_"))
            {
                channel.IsGroup = true;
            }
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
                    bool participantExists = this._conversationParticipantsRepo.Table.Count(cp => cp.IsDeleted != true && cp.UserIdFk == user.UserId && cp.ConversationChannelIdFk == channel.ConversationChannelId && cp.UniqueName == p) > 0;
                    if (!participantExists)
                    {
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

            var channels = this._dbContext.LoadStoredProcedure("raq_getConversationChannelsByUserId")
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

        public BaseResponse deleteConversationChannel(string ChannelSid, int UserId)
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
                    Participants = new List<string> { participant.UserUniqueId },
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
        public BaseResponse updateConversationUserSid(string UserSid)
        {
            BaseResponse response = new BaseResponse();

            var user = this._userRepo.Table.FirstOrDefault(u => u.UserId == ApplicationSettings.UserId && !u.IsDeleted);
            if (user != null)
            {
                user.ConversationUserSid = UserSid;
                this._userRepo.Update(user);
            }
            response.Status = HttpStatusCode.OK;
            response.Message = "User Sid Saved Successfully";
            response.Body = new { UserId = ApplicationSettings.UserId, UserChannelSid = UserSid };
            return response;

        }
        public BaseResponse getAllConversationUsers()
        {

            //var chatusers = this._userrepo.table.where(u => u.isdeleted != true && u.isactive == true && !string.isnullorempty(u.userchannelsid));
            var chatUsers = this._dbContext.LoadStoredProcedure("raq_getAllConversationUsers")
             .WithSqlParam("@proleid", ApplicationSettings.RoleIds)
             .ExecuteStoredProc<ChatUsersVM>();
            foreach (var user in chatUsers)
            {
                user.UserRoles = (from ur in this._userRole.Table
                                  join r in this._role.Table on ur.RoleIdFk equals r.RoleId
                                  where ur.UserIdFk == user.UserId && !r.IsDeleted
                                  select new UserRoleVM
                                  {
                                      RoleId = ur.RoleIdFk,
                                      RoleName = r.RoleName,
                                      OrganizationIdFk = r.OrganizationIdFk
                                  }).ToList();

                user.ServiceLines = (from sl in this._serviceLineRepo.Table
                                     join ur in this._userRelationRepo.Table
                                     on sl.ServiceLineId equals ur.ServiceLineIdFk
                                     where sl.IsDeleted != true && ur.UserIdFk == user.UserId
                                     select new ServiceLineVM
                                     {
                                         ServiceLineId = sl.ServiceLineId,
                                         ServiceName = sl.ServiceName,
                                         DepartmentIdFk = sl.DepartmentIdFk,
                                     }).ToList();
                user.Departments = this._dptRepo.Table.Where(x => !x.IsDeleted && user.ServiceLines.Select(y => y.DepartmentIdFk).Contains(x.DepartmentId)).Select(x => new DepartmentVM() { DepartmentId = x.DepartmentId, DepartmentName = x.DepartmentName, OrganizationIdFk = x.OrganizationIdFk }).ToList();
                user.Organizations = user.Departments.Count > 0 ? this._orgRepo.Table.Where(x => !x.IsDeleted && user.Departments.Select(y => y.OrganizationIdFk).Contains(x.OrganizationId)).Select(x => new OrganizationVM() { OrganizationId = x.OrganizationId, OrganizationName = x.OrganizationName }).ToList() : this._orgRepo.Table.Where(x => !x.IsDeleted && user.UserRoles.Select(y => y.OrganizationIdFk).Contains(x.OrganizationId)).Select(x => new OrganizationVM() { OrganizationId = x.OrganizationId, OrganizationName = x.OrganizationName }).ToList();
            }

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Chat users found", Body = chatUsers };
        }

        public ChannelResource createConversationChannel(string FriendlyName, string UniqueName)
        {
            TwilioClient.Init(this.Twilio_AccountSid, this.Twilio_AuthToken);
            var channel = ChannelResource.Create(pathServiceSid: this.Twilio_ChatServiceSid, friendlyName: FriendlyName, uniqueName: UniqueName);
            //var channel = Twilio.Rest.Conversations.V1.ConversationResource.Create(pathServiceSid: this.Twilio_ChatServiceSid,friendlyName:FriendlyName,uniqueName: UniqueName);
            return channel;
        }

        public UserResource createConversationUser(string Identity, string FriendlyName)
        {
            TwilioClient.Init(this.Twilio_AccountSid, this.Twilio_AuthToken);
            var user = UserResource.Create(pathServiceSid: this.Twilio_ChatServiceSid, identity: Identity, friendlyName: FriendlyName);
            return user;

        }
        public MemberResource addNewUserToConversationChannel(string ChannelSid, string ParticipantUniqueName)
        {
            TwilioClient.Init(this.Twilio_AccountSid, this.Twilio_AuthToken);
            var addParticipant = MemberResource.Create(identity: ParticipantUniqueName, pathServiceSid: this.Twilio_ChatServiceSid, pathChannelSid: ChannelSid);
            return addParticipant;

        }
        public BaseResponse getConversationUsersStatus(string UserSid)
        {
            List<ConversationUserStatus> statusList = new();
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
        #endregion


        #region [Video Call]
        public BaseResponse generateVideoCallToken(string identity)
        {
            BaseResponse response = new BaseResponse();
            TwilioClient.Init(this.Twilio_AccountSid, this.Twilio_AuthToken);



            // Create a video grant for the token
            var grant = new VideoGrant();
            grant.Room = null;//Request.QueryString["room"];
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
                                        {"type", "Video Call"}
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
                                        { "routerLink", model.RouteLink}
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
        public BaseResponse rejectIncomingCall(string roomSid)
        {
            var room = RoomResource.Update(
            status: RoomResource.RoomStatusEnum.Completed,
            pathSid: roomSid
        );

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Room Completed" };
        }
        #endregion
    }



}
