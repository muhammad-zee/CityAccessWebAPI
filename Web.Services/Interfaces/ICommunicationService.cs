using Microsoft.AspNetCore.Http;
using Twilio.Rest.Chat.V2.Service;
using Twilio.Rest.Chat.V2.Service.Channel;
using Web.Model;
using Web.Model.Common;

namespace Web.Services.Interfaces
{
    public interface ICommunicationService
    {

        BaseResponse generateConversationToken(string Identity);
        BaseResponse pushNotification(PushNotificationVM model);
        BaseResponse sendPushNotification(ConversationMessageVM msg);
        BaseResponse ConversationCallbackEvent(string EventType);
        BaseResponse saveUserChannelSid(int UserId, string ChannelSid);
        BaseResponse saveConversationChannel(ConversationChannelVM channel);
        BaseResponse saveConversationChannelParticipants(ConversationChannelParticipantsVM channel);
        BaseResponse getConversationChannels(int UserId);
        BaseResponse deleteAllChannels(string key);
        BaseResponse deleteConflictedConversationChannel(string channelSid);
        BaseResponse deleteConversationChannel(string ChannelSid, int UserId);
        BaseResponse deleteConversationParticipant(string ChannelSid, string ParticipantUniqueName, int UserId);
        BaseResponse addUserToChannelUsingApi(string ChannelUniqueName, string ParticipantUniqueName, int UserId);
        BaseResponse updateConversationGroup(string FriendlyName, string ChannelSid);
        BaseResponse updateConversationUserSid(string UserSid, int userId);
        BaseResponse getAllConversationUsers();

        public BaseResponse getAllConversationUsersByOrgId(int orgid);
        BaseResponse getConversationUsersStatus(string UserSid);
        BaseResponse GetCurrentConversationParticipants(string channelSid);
        bool conversationUserIsOnline(string UserSid);
        ChannelResource createConversationChannel(string FriendlyName, string UniqueName, string Attributes);
        ChannelResource createNotificationChannel(string FriendlyName, string userUniqueId);
        UserResource createConversationUser(string Identity, string FriendlyName);
        MemberResource addNewUserToConversationChannel(string ChannelSid, string ParticipantUniqueName);
        BaseResponse createOrRemoveGroupMemberAsAdmin(bool isAdmin, string uniqueName, string channleSid);
        BaseResponse UploadAttachment(IFormFileCollection file);
        bool DeleteUserToConversationChannel(string ChannelSid);
        BaseResponse refreshConsversationUsers(string key);
        BaseResponse updateConsversationUserSidFromTwilio(string key);
        #region [Video Call]
        BaseResponse generateVideoCallToken(string identity);

        BaseResponse VideoRoomCallbackEvent(string EventType);
        BaseResponse dialVideoCall(DialVideoCallVM model);

        BaseResponse incomingCallEvent(string roomSid, string eventType, string channelSid);
        #endregion

        bool SendSms(string ToPhoneNumber, string SmsBody);
        bool SendEmail(string To, string Subject, string HtmlContent, byte[] ImageContent);

        #region Chat Settings
        BaseResponse GetTone(int Id);
        BaseResponse GetChatSetting(int UserId);
        #endregion

        public BaseResponse addChatSettings(AddChatSettingVM channel);


        #region Communication Log

        BaseResponse SaveCommunicatonLog(CommunicationLogVM log);

        BaseResponse GetCommunicationLogById(int logId, bool status);
        BaseResponse ActiveOrInActiveCommunicationlog(int logId, bool status);

        BaseResponse GetAllCommunicationlog(int orgId, string departmentIds, string serviceLineIds, bool showAllVoicemails);
        BaseResponse GetCallLog(int orgId, bool showAllCalls);
        ChannelResource getConversationChannelBySid(string channelSid);

        #endregion
    }
}
