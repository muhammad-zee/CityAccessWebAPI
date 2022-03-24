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
        TwiMLResult EnqueueCall();
        TwiMLResult Connect(string phoneNumber, string Twilio_PhoneNumber, string From, string CallSid, string CallStatus);
        CallResource Call();
        string CallbackStatus(IFormCollection Request);
        TwiMLResult CallConnected();
        TwiMLResult PromptResponse(int Digits, int ParentNodeId);
        TwiMLResult ExceptionResponse(Exception ex);
        TwiMLResult ReceiveVoicemail(string RecordingUrl, string RecordingSid);

        //BaseResponse getIvrTree();
        BaseResponse getIvrNodes();
        BaseResponse getIvrTree(int Id);
        BaseResponse getIvrNodes(int Id);
        BaseResponse GetNodeType(int Id);
        BaseResponse saveIvrNode(IvrSettingVM model);
        BaseResponse getPreviousCalls();
        BaseResponse DeleteIVRNode(int Id, int userId);

        BaseResponse saveCallLog(CallLogVM log);


        #region IVR

        BaseResponse getAllIvrs();
        BaseResponse saveIVR(IVRVM model);
        BaseResponse DeleteIVR(int Id, int userId);

        #endregion


    }
}
