namespace Web.Services.Interfaces
{
    public interface ICommunicationService
    {
        bool SendSms(string ToPhoneNumber, string SmsBody);
        bool SendEmail(string To, string Subject, string HtmlContent, byte[] ImageContent);
    }
}
