using Microsoft.AspNetCore.Http;
using System;
using Twilio.AspNet.Core;
using Twilio.Rest.Api.V2010.Account;
using Web.Model;
using Web.Model.Common;

namespace Web.Services.Interfaces
{
    public interface ICallService
    {
        BaseResponse GenerateToken(string Identity);
        TwiMLResult EnqueueCall(int parentNodeId, int serviceLineId, string CallSid);
        TwiMLResult Connect(string phoneNumber, string Twilio_PhoneNumber, string From, string CallSid, string CallStatus);
        CallResource Call();
        TwiMLResult CallConnected(string To, string From);
        TwiMLResult PromptResponse(int Digits, int ParentNodeId, int serviceLineId);
        TwiMLResult ExceptionResponse(Exception ex);
        TwiMLResult ReceiveVoicemail(string RecordingUrl, string RecordingSid);
        string CallbackStatus(IFormCollection Request);
        string InboundCallbackStatus(IFormCollection Request, int parentNodeId, int serviceLineId);
        string ConferenceParticipantCallbackStatus(IFormCollection Request, int roleId, int serviceLineId, string conferenceSid);
        //BaseResponse getIvrTree();
        BaseResponse getIvrNodes();
        BaseResponse getIvrTree(int Id);
        BaseResponse getIvrNodes(int Id);
        BaseResponse GetNodeType(int Id);
        BaseResponse saveIvrNode(IvrSettingVM model);
        BaseResponse getPreviousCalls();
        BaseResponse DeleteIVRNode(int Id);

        BaseResponse copyIvrSettings(int copyFromServiceLineId, int copyToServiceLineId);

        BaseResponse saveCallLog(CallLogVM log);


        #region IVR

        BaseResponse getAllIvrs(bool status);
        BaseResponse getAllIvrsByOrgId(int orgId);
        BaseResponse saveIVR(IVRVM model);
        BaseResponse DeleteIVR(int Id);
        BaseResponse ActiveOrInActiveIVR(int Id, bool status);

        #endregion

        #region Enqueue

        BaseResponse saveQueues(QueuesVM queue);
        BaseResponse DequeueCalls();
        #endregion

    }
}
