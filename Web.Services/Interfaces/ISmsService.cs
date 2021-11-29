using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twilio.AspNet.Common;

namespace Web.Services.Interfaces
{
    public interface ISmsService
    {
         string SendSms(SmsRequest sr);
    }
}
