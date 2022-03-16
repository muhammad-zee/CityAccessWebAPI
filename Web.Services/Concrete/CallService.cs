using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Twilio;
using Twilio.AspNet.Core;
using Twilio.Jwt.AccessToken;
using Twilio.Rest.Api.V2010.Account;
using Twilio.TwiML;
using Twilio.TwiML.Voice;
using Twilio.Types;
using Web.Data.Models;
using Web.DLL;
using Web.DLL.Generic_Repository;
using Web.Model;
using Web.Model.Common;
using Web.Services.Enums;
using Web.Services.Extensions;
using Web.Services.Helper;
using Web.Services.Interfaces;


namespace Web.Services.Concrete
{
    public class CallService : TwilioController, ICallService
    {
        private readonly ICommunicationService _communicationService;
        private readonly IRepository<Ivrsetting> _ivrSettingsRepo;
        private readonly IRepository<InteractiveVoiceResponse> _IVRRepo;
        private readonly IRepository<CallLog> _callLogRepo;
        private IRepository<ControlListDetail> _controlListDetailsRepo;
        IConfiguration _config;
        private readonly UnitOfWork unitorWork;
        private string origin = "";
        private string Twilio_AccountSid;
        private string Twilio_AuthToken;
        private string Twillio_TwiMLAppSid;
        private string Twillio_VoiceApiKey;
        private string Twillio_VoiceApiKeySecret;


        public CallService(IConfiguration config,
            ICommunicationService communicationService,
            IRepository<Ivrsetting> ivrSettings,
            IRepository<InteractiveVoiceResponse> IVR,
            IRepository<ControlListDetail> controlListDetails,
            IRepository<CallLog> callLog)
        {
            this._config = config;
            this._communicationService = communicationService;
            this._ivrSettingsRepo = ivrSettings;
            this._IVRRepo = IVR;
            this._callLogRepo = callLog;
            this._controlListDetailsRepo = controlListDetails;
            this.origin = this._config["Twilio:CallbackDomain"].ToString();

            //Twilio Credentials
            this.Twilio_AccountSid = this._config["Twilio:AccountSid"].ToString();
            this.Twilio_AuthToken = this._config["Twilio:AuthToken"].ToString();
            this.Twillio_TwiMLAppSid = this._config["Twilio:TwiMLAppSid"].ToString();
            this.Twillio_VoiceApiKey = this._config["Twilio:VoiceApiKey"].ToString();
            this.Twillio_VoiceApiKeySecret = this._config["Twilio:VoiceApiKeySecret"].ToString();

        }

        #region Generate Twilio Voice Capability Token
        public BaseResponse GenerateToken(string Identity)
        {
            BaseResponse response = new BaseResponse();
            TwilioClient.Init(this.Twilio_AccountSid, this.Twilio_AuthToken);
            // Create a Voice grant for this token
            var grant = new VoiceGrant();
            grant.OutgoingApplicationSid = Twillio_TwiMLAppSid;
            //grant.PushCredentialSid = PushCredentialSid;

            grant.IncomingAllow = true;
            var grants = new HashSet<IGrant>
                {
                    { grant }
                };

            // Create an Access Token generator
            var Expirydate = DateTime.Now.AddHours(23);
            if (this.Twillio_VoiceApiKey != null)
            {
                var token = new Token(
                            this.Twilio_AccountSid,
                            this.Twillio_VoiceApiKey,
                            this.Twillio_VoiceApiKeySecret,
                            Identity,
                            grants: grants,
                            expiration: Expirydate);

                var resposeData = new
                {
                    token = token.ToJwt(),
                    identity = Identity
                };
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Token Generated", Body = resposeData };
            }
            else
            {
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = "Failed To Generate Token" };

            }

        }

        #endregion


        public TwiMLResult Connect(string phoneNumber, string Twilio_PhoneNumber)
        {
            var response = new VoiceResponse();
            var dial = new Dial(callerId: Twilio_PhoneNumber/*, record: Dial.RecordEnum.RecordFromAnswer*/);
            if (phoneNumber.Contains("client"))
            {
                dial.Client(phoneNumber.Replace("client:", ""));
            }
            else
            {
                dial.Number(phoneNumber);
            }

            response.Append(dial);
            return TwiML(response);
        }
        public TwiMLResult EnqueueCall()
        {
            var response = new VoiceResponse();
            var dial = new Dial();
            dial.Client("X72ZHE3KSEB49TR");
            response.Append(dial);
            return TwiML(response);
        }
        public CallResource Call()
        {
            var CallConnectedUrl = $"{origin}/Call/CallConnected";
            var StatusCallbackUrl = $"{origin}/Call/CallbackStatus";
            //var CallConnectedUrl = $"{origin}/Call/CallConnected";
            //var StatusCallbackUrl = $"{origin}/Call/CallbackStatus";
            //url = url.Replace(" ", "%20");
            var To = new PhoneNumber("+923327097498");
            var From = new PhoneNumber("(616) 449-2720");
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

            var gather = new Gather(numDigits: 1, timeout: 10, action: new Uri(GatherResponseUrl)).Pause(length: 3)
                                          .Say("Press one to  talk to Bilal.", language: "en").Pause(length: 1)
                                          .Say("Press two to talk to an Zee.", language: "en").Pause(length: 1)
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
            else if (QueryDigit == 3)
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

        public BaseResponse saveCallLog(CallLogVM log)
        {
            CallLog record = null;
            if (log.CallLogId > 0)
            {
                record = this._callLogRepo.Table.Where(i => i.CallSid == log.CallSid ).FirstOrDefault();
                if (record != null)
                {
                    record.CallLogId = log.CallLogId;
                    record.StartTime = log.StartTime;
                    record.EndTime = log.EndTime;
                    record.Duration = log.Duration;
                    record.Direction = log.Direction;
                    record.CallStatus = log.CallStatus;
                    record.ToPhoneNumber = log.ToPhoneNumber;
                    record.ToName = log.ToName;
                    record.FromPhoneNumber = log.FromPhoneNumber;
                    record.FromName = log.FromName;
                    record.ParentCallSid = log.ParentCallSid;
                    record.RecordingName = log.RecordingName;
                    record.IsRecorded = log.IsRecorded;
                    record.CreatedDate = DateTime.UtcNow;

                    this._callLogRepo.Update(record);
                }

            }
            else
            {
                record.CallLogId = log.CallLogId;
                record.StartTime = log.StartTime;
                record.EndTime = log.EndTime;
                record.Duration = log.Duration;
                record.Direction = log.Direction;
                record.CallStatus = log.CallStatus;
                record.ToPhoneNumber = log.ToPhoneNumber;
                record.ToName = log.ToName;
                record.FromPhoneNumber = log.FromPhoneNumber;
                record.FromName = log.FromName;
                record.ParentCallSid = log.ParentCallSid;
                record.RecordingName = log.RecordingName;
                record.IsRecorded = log.IsRecorded;
                record.CreatedDate = DateTime.UtcNow;

                this._callLogRepo.Insert(record);
            }
            return new BaseResponse()
            {
                Status = HttpStatusCode.OK,
                Message = "Record Saved",
                Body = record
            };
        }

        #region IVR Settings
        public BaseResponse getIvrTree()
        {
            var IvrSetting = this._ivrSettingsRepo.Table.Where(i => !i.IsDeleted).ToList();
            if (IvrSetting.Count() > 0)
            {
                var treeItems = IvrSetting.Select(x => new IvrTreeVM()
                {
                    key = x.IvrSettingsId.ToString(),
                    ParentKey = x.IvrparentId,
                    data = x.Description,
                    label = x.Name,
                    expandedIcon = x.Icon,
                    collapsedIcon = x.Icon,
                    KeyPress = x.KeyPress,
                    expanded = true
                }).ToList();
                var treeViewItems = treeItems.BuildIvrTree();
                return new BaseResponse { Status = HttpStatusCode.OK, Message = "IVR Returned", Body = treeViewItems };
            }
            else
            {
                return new BaseResponse { Status = HttpStatusCode.NotFound, Message = "IVR Not Found" };
            }
        }
        public BaseResponse getIvrTree(int Id)
        {
            var IvrSetting = this._ivrSettingsRepo.Table.Where(i => !i.IsDeleted && i.IvrIdFk == Id).ToList();
            if (IvrSetting.Count() > 0)
            {
                var treeItems = IvrSetting.Select(x => new IvrTreeVM()
                {
                    key = x.IvrSettingsId.ToString(),
                    ParentKey = x.IvrparentId,
                    data = x.Description,
                    label = x.Name,
                    expandedIcon = x.Icon,
                    collapsedIcon = x.Icon,
                    KeyPress = x.KeyPress,
                    expanded = true
                }).ToList();
                var treeViewItems = treeItems.BuildIvrTree();
                return new BaseResponse { Status = HttpStatusCode.OK, Message = "IVR Returned", Body = treeViewItems };
            }
            else
            {
                return new BaseResponse { Status = HttpStatusCode.NotFound, Message = "IVR Not Found" };
            }
        }

        public BaseResponse getIvrNodes()
        {
            var IVRs = _ivrSettingsRepo.Table.Where(x => x.IsDeleted != true).ToList();
            return new BaseResponse()
            {
                Status = HttpStatusCode.OK,
                Message = "Data Found",
                Body = IVRs
            };
        }
        public BaseResponse getIvrNodes(int Id)
        {
            var IVRs = _ivrSettingsRepo.Table.Where(x => x.IsDeleted != true && x.IvrIdFk == Id).ToList();
            return new BaseResponse()
            {
                Status = HttpStatusCode.OK,
                Message = "Data Found",
                Body = IVRs
            };
        }
        public BaseResponse saveIvrNode(IvrSettingVM model)
        {
            Ivrsetting ivrNode = null;
            if (model.IvrSettingsId > 0)
            {
                ivrNode = this._ivrSettingsRepo.Table.Where(i => i.IvrSettingsId == model.IvrSettingsId && !i.IsDeleted).FirstOrDefault();
                if (ivrNode != null)
                {
                    ivrNode.IvrIdFk = model.IvrIdFk;
                    ivrNode.IvrparentId = model.IvrparentId;
                    ivrNode.Name = model.Name;
                    ivrNode.Description = model.Description;
                    ivrNode.KeyPress = model.KeyPress;
                    ivrNode.ModifiedBy = model.ModifiedBy;
                    ivrNode.ModifiedDate = DateTime.UtcNow;
                    ivrNode.IsDeleted = false;
                    this._ivrSettingsRepo.Update(ivrNode);
                }

            }
            else
            {
                ivrNode = new Ivrsetting();
                ivrNode.IvrIdFk = model.IvrIdFk;
                ivrNode.IvrparentId = model.IvrparentId == 0 ? null : model.IvrparentId;
                ivrNode.Name = model.Name;
                ivrNode.Description = model.Description;
                ivrNode.KeyPress = model.KeyPress;
                ivrNode.Icon = model.Icon;
                ivrNode.CreatedBy = model.CreatedBy;
                ivrNode.CreatedDate = DateTime.UtcNow;
                //ivrNode.ModifiedBy = model.ModifiedBy;
                //ivrNode.ModifiedDate = DateTime.UtcNow;
                ivrNode.IsDeleted = false;
                this._ivrSettingsRepo.Insert(ivrNode);
            }
            return new BaseResponse()
            {
                Status = HttpStatusCode.OK,
                Message = "Node Saved",
                Body = ivrNode
            };
        }

        public BaseResponse DeleteIVRNode(int Id, int userId)
        {
            var IVRNode = _ivrSettingsRepo.Table.Where(x => x.IvrSettingsId == Id && x.IsDeleted != true).FirstOrDefault();
            if (IVRNode != null)
            {
                IVRNode.IsDeleted = true;
                IVRNode.ModifiedBy = userId;
                IVRNode.ModifiedDate = DateTime.UtcNow;
                _ivrSettingsRepo.Update(IVRNode);

                var childNodes = _ivrSettingsRepo.Table.Where(x => x.IvrparentId == Id && x.IsDeleted != true).ToList();
                childNodes.ForEach(x => { x.IsDeleted = true; x.ModifiedBy = userId; x.ModifiedDate = DateTime.UtcNow; });

                _ivrSettingsRepo.Update(childNodes);

                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Successfully Deleted" };
            }
            else
            {
                return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Record Not Found" };
            }

        }

        #endregion


        #region IVR

        public BaseResponse getAllIvrs()
        {
            var IVRs = _IVRRepo.Table.Where(x => x.IsDeleted != true).ToList();
            var types = _controlListDetailsRepo.Table.Where(x => x.ControlListIdFk == UCLEnums.OrgType.ToInt()).Select(x => new { x.ControlListDetailId, x.Title });
            var IVRVMs = AutoMapperHelper.MapList<InteractiveVoiceResponse, IVRVM>(IVRs);
            IVRVMs.ForEach(x => x.OrganizationType = types.Where(t => t.ControlListDetailId == x.OrganizationTypeIdFk).Select(ty => ty.Title).FirstOrDefault());
            return new BaseResponse()
            {
                Status = HttpStatusCode.OK,
                Message = "Data Found",
                Body = IVRVMs
            };
        }
        public BaseResponse saveIVR(IVRVM model)
        {
            InteractiveVoiceResponse ivr = null;
            if (model.IvrId > 0)
            {
                ivr = this._IVRRepo.Table.Where(i => i.IvrId == model.IvrId && !i.IsDeleted).FirstOrDefault();
                if (ivr != null)
                {
                    ivr.OrganizationTypeIdFk = model.OrganizationTypeIdFk;
                    ivr.Name = model.Name;
                    ivr.Description = model.Description;
                    ivr.ModifiedBy = model.ModifiedBy;
                    ivr.ModifiedDate = DateTime.UtcNow;
                    ivr.IsDeleted = false;
                    this._IVRRepo.Update(ivr);
                }

            }
            else
            {
                ivr = new InteractiveVoiceResponse();
                ivr.OrganizationTypeIdFk = model.OrganizationTypeIdFk;
                ivr.Name = model.Name;
                ivr.Description = model.Description;
                ivr.CreatedBy = model.CreatedBy;
                ivr.CreatedDate = DateTime.UtcNow;
                ivr.IsDeleted = false;
                this._IVRRepo.Insert(ivr);
            }
            return new BaseResponse()
            {
                Status = HttpStatusCode.OK,
                Message = "Node Saved",
                Body = ivr
            };
        }

        public BaseResponse DeleteIVR(int Id, int userId)
        {
            var IVRNode = this._IVRRepo.Table.Where(x => x.IvrId == Id && x.IsDeleted != true).FirstOrDefault();
            if (IVRNode != null)
            {
                IVRNode.IsDeleted = true;
                IVRNode.ModifiedBy = userId;
                IVRNode.ModifiedDate = DateTime.UtcNow;
                this._IVRRepo.Update(IVRNode);

                var childNodes = _ivrSettingsRepo.Table.Where(x => x.IvrIdFk == Id && x.IsDeleted != true).ToList();
                childNodes.ForEach(x => { x.IsDeleted = true; x.ModifiedBy = userId; x.ModifiedDate = DateTime.UtcNow; });

                _ivrSettingsRepo.Update(childNodes);

                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Successfully Deleted" };
            }
            else
            {
                return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Record Not Found" };
            }

        }

        #endregion
    }
}
