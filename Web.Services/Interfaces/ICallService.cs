using System;
using Twilio.AspNet.Core;
using Twilio.Rest.Api.V2010.Account;
using Web.Model;

namespace Web.Services.Interfaces
{
    public interface ICallService
    {
        BaseResponse GenerateToken(string Identity);
        CallResource Call();
        string CallbackStatus(string Callsid, string CallStatus);
        TwiMLResult CallConnected();
        TwiMLResult PromptResponse(int Digits);
        TwiMLResult ExceptionResponse(Exception ex);
        TwiMLResult ReceiveVoicemail(string RecordingUrl, string RecordingSid);

    }
}
