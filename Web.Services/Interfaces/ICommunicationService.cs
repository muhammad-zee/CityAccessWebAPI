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
        BaseResponse addUserToChannelUsingApi(string ChannelUniqueName, string ParticipantUniqueName, int UserId);
        BaseResponse updateConversationGroup(string FriendlyName, string ChannelSid);
        BaseResponse getAllConversationUsers();
        bool SendSms(string ToPhoneNumber, string SmsBody);
        bool SendEmail(string To, string Subject, string HtmlContent, byte[] ImageContent);
    }
}
