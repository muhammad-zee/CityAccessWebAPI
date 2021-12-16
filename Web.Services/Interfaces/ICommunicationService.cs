using Web.Model;

namespace Web.Services.Interfaces
{
    public interface ICommunicationService
    {
        BaseResponse generateConversationToken(string Identity);
        bool SendSms(string ToPhoneNumber, string SmsBody);
        bool SendEmail(string To, string Subject, string HtmlContent, byte[] ImageContent);
    }
}
