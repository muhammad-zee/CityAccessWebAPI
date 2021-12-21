using System;
using Twilio.AspNet.Core;
using Twilio.Rest.Api.V2010.Account;
using Web.Data.Models;
using Web.Model;
using Web.Model.Common;

namespace Web.Services.Interfaces
{
    public interface ICallService
    {
        BaseResponse GenerateToken(string Identity);
        TwiMLResult EnqueueCall();
        TwiMLResult Connect(string phoneNumber, string Twilio_PhoneNumber);
        CallResource Call();
        string CallbackStatus(string Callsid, string CallStatus);
        TwiMLResult CallConnected();
        TwiMLResult PromptResponse(int Digits);
        TwiMLResult ExceptionResponse(Exception ex);
        TwiMLResult ReceiveVoicemail(string RecordingUrl, string RecordingSid);

        BaseResponse getIvrTree();
        BaseResponse getIvrNodes();
        BaseResponse saveIvrNode(IvrSettingVM model);

    }
}
