using Twilio.AspNet.Common;

namespace Web.Services.Interfaces
{
    public interface ICommunicationService
    {
        string SendSms(SmsRequest sr);
    }
}
