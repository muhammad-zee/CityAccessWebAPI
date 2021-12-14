using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using Twilio;
using Twilio.AspNet.Core;
using Twilio.Rest.Api.V2010.Account;
using Twilio.TwiML;
using Twilio.TwiML.Voice;
using Twilio.Types;
using Web.DLL;
using Web.Services.Interfaces;


namespace Web.Services.Concrete
{
    public class CallService : TwilioController,ICallService
    {
        private readonly ICommunicationService _communicationService;
        IConfiguration _config;
        private readonly UnitOfWork unitorWork;
        private string origin = "";
        private string Twilio_AccountSid;
        private string Twilio_AuthToken;



        public CallService(IConfiguration config,
            ICommunicationService communicationService)
        {
            this._config = config;
            this._communicationService = communicationService;
            this.Twilio_AccountSid = this._config["Twilio:AccountSid"].ToString();
            this.Twilio_AuthToken = this._config["Twilio:AuthToken"].ToString();
            this.origin= this._config["Twilio:CallbackDomain"].ToString();

        }
        public CallResource Call()
        {
            var CallConnectedUrl = $"{origin}/Call/CallConnected";
            var StatusCallbackUrl = $"{origin}/Call/CallbackStatus";
            //var CallConnectedUrl = $"{origin}/Call/CallConnected";
            //var StatusCallbackUrl = $"{origin}/Call/CallbackStatus";
            //url = url.Replace(" ", "%20");
            var To = new PhoneNumber("+923327097498");
            //var From = new PhoneNumber("(616) 449-2720");
            var From = new PhoneNumber("(931) 548-6906");
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; //TLS 1.2                                                                             
            TwilioClient.Init(this.Twilio_AccountSid, this.Twilio_AuthToken);
            var call = CallResource.Create(to: To,
                from: From,
                url: new Uri(CallConnectedUrl),
                method: Twilio.Http.HttpMethod.Post,
                statusCallback: new Uri(StatusCallbackUrl),
                statusCallbackMethod: Twilio.Http.HttpMethod.Post);
            return call;
        }

        public string CallbackStatus(string Callsid, string CallStatus)
        {
            return "ok";
        }
        public TwiMLResult CallConnected()
        {
            var response = new VoiceResponse();
            //var GatherResponseUrl = $"https://" + origin + "/AutomatedCall/PatientResponse?PatientID=" + PatientID + "&AppointmentID=" + AppointmentID + "&Price=" + Price;
            var GatherResponseUrl = $"{origin}/Call/PromptResponse";

            var gather = new Gather(numDigits: 1, timeout: 10, action: new Uri(GatherResponseUrl)).Pause(length:3)
                                          .Say("Press one to do  talk to Bilal.", language: "en").Pause(length: 1)
                                          .Say("Press two to talk to an Zee.", language: "en").Pause(length:1)
                                          .Say("Press three to send voice mail");
            response.Append(gather);
            response.Say("You did not press any key,\n good bye.!");
            var xmlResponse = response.ToString();
            return TwiML(response);
        }
        public TwiMLResult PromptResponse(int Digits)
        {
            int QueryDigit = Convert.ToInt32(Digits);
            var response = new VoiceResponse();
            if (QueryDigit == 1)
            {
                response.Say("Sorry, Bilal is not available right now");

            }
            else if (QueryDigit == 2)
            {
                response.Say("sorry to say zee is not here. we will get back to you when zee will be back");
            }
            else if(QueryDigit == 3)
            {
                var RecordUrl = $"{origin}/Call/ReceiveVoicemail";

                response
                    .Say("Please leave a message at the beep.");
                response.Record(action: new Uri(RecordUrl));
                response.Say("I did not receive a recording");
                response.Leave();

            }
            else
            {
                response.Say("You Pressed wrong key");
            }
            return TwiML(response);
        }
        public TwiMLResult ReceiveVoicemail(string RecordingUrl, string RecordingSid)
        {

            VoiceResponse response = new VoiceResponse();
            response.Say("Thankyou , Your Voicemail is received.");
            return TwiML(response);
        }

        public TwiMLResult ForwardCallToAgent(string CallSid, string From)
        {
            var response = new VoiceResponse();
            var dial = new Dial();
            dial.Number("+16784263023");
            response.Append(dial);
            string responseXML = response.ToString();
            return TwiML(response);
        }



        public TwiMLResult ExceptionResponse(Exception ex)
        {
            VoiceResponse response = new VoiceResponse();
            response.Say(ex.Message.ToString());
            return TwiML(response);
        }
    }
}
