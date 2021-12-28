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

        private readonly IRepository<User> _userRepo;
        public CommunicationService(IConfiguration config, IRepository<User> userRepo)
        {

            this._config = config;
            this.Twilio_AccountSid = this._config["Twilio:AccountSid"].ToString();
            this.Twilio_AuthToken = this._config["Twilio:AuthToken"].ToString();
            this.SendGrid_ApiKey = this._config["SendGrid:ApiKey"].ToString();
            this.FromEmail = this._config["SendGrid:FromEmail"].ToString();

            this.Twilio_ChatServiceSid = this._config["Twilio:ChatServiceSid"].ToString();
            this.Twilio_ChatPushCredentialSid = this._config["Twilio:PushCredentialSid"].ToString();
            this.Twilio_ChatApiKey = this._config["Twilio:ChatApiKey"].ToString();
            this.Twilio_ChatApiKeySecret = this._config["Twilio:ChatApiKeySecret"].ToString();

            this._userRepo = userRepo;
        }

        #region SMS sending
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

        #region Email Sending

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


        #region Twilio Conversation

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
                                       author:msg.author,
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

        public BaseResponse delateChatChannel(string ChannelSid)
        {
            TwilioClient.Init(this.Twilio_AccountSid, this.Twilio_AuthToken);
            var channels = ChannelResource.Read(pathServiceSid: this.Twilio_ChatServiceSid);
            foreach (var ch in channels)
            {
                //var delete = ChannelResource.Delete(pathServiceSid: this.Twilio_ChatServiceSid, pathSid: ch.Sid);
            }
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Channels Deleted" };
        }

        #endregion
    }



}
