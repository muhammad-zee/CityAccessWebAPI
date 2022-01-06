using Web.Model;
using Web.Model.Common;

namespace Web.Services.Interfaces
{
    public interface ICommunicationService
    {
        BaseResponse generateConversationToken(string Identity);
        BaseResponse sendPushNotification(ConversationMessageVM msg);
        BaseResponse saveUserChannelSid(int UserId, string ChannelSid);
        BaseResponse saveConversationChannel(ConversationChannelVM channel);
        BaseResponse saveConversationChannelParticipants(ConversationChannelParticipantsVM channel);
        BaseResponse getConversationChannels(int UserId);
        BaseResponse deleteChatChannel(string ChannelSid);
        BaseResponse deleteConversationChannel(string ChannelSid, int UserId);
        BaseResponse deleteConversationParticipant(string ChannelSid, string ParticipantUniqueName, int UserId);
        BaseResponse getAllConversationUsers();
        bool SendSms(string ToPhoneNumber, string SmsBody);
        bool SendEmail(string To, string Subject, string HtmlContent, byte[] ImageContent);
    }
}
