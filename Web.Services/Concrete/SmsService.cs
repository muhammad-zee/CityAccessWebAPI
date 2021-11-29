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
    class SmsService : ISmsService
    {
        private string Twilio_AccountSid;
        private string Twilio_AuthToken;
        private IConfiguration _config;
        public SmsService(IConfiguration config) {
            this._config = config;
            this.Twilio_AccountSid = this._config["Twilio:AccountSid"].ToString();
            this.Twilio_AuthToken = this._config["Twilio:AuthToken"].ToString();
        }
        public string SendSms(SmsRequest sr)
        {
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
