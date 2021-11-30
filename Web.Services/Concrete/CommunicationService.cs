using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twilio;
using Twilio.AspNet.Common;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Web.Services.Interfaces;

namespace Web.Services.Concrete
{
    public class CommunicationService : ICommunicationService
    {
        private string Twilio_AccountSid;
        private string Twilio_AuthToken;
        private IConfiguration _config;
        public CommunicationService(IConfiguration config) {
            this._config = config;
            this.Twilio_AccountSid = this._config["Twilio:AccountSid"].ToString();
            this.Twilio_AuthToken = this._config["Twilio:AuthToken"].ToString();
        }
        public string SendSms(SmsRequest sr)
        {
            sr.From = this._config["Twilio:PhoneNumber"];
            TwilioClient.Init(this.Twilio_AccountSid, this.Twilio_AuthToken);
            var messageResource = MessageResource.Create(
                to:new PhoneNumber(sr.To),
                from: new PhoneNumber(sr.From),
                body: sr.Body
                );
            return messageResource.Status.ToString(); ;

        }
    }
}
