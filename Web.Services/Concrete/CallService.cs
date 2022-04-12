using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
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
using static Twilio.Rest.Api.V2010.Account.CallResource;
using Client = Twilio.TwiML.Voice.Client;
using Newtonsoft.Json;

namespace Web.Services.Concrete
{
    public class CallService : TwilioController, ICallService
    {
        private RAQ_DbContext _dbContext;
        private readonly ICommunicationService _communicationService;
        private readonly IRepository<Ivrsetting> _ivrSettingsRepo;
        private readonly IRepository<InteractiveVoiceResponse> _IVRRepo;
        private readonly IRepository<ControlListDetail> _controlListDetailsRepo;
        private readonly IRepository<CallLog> _callLogRepo;
        private readonly IRepository<User> _userRepo;



        IConfiguration _config;
        private readonly UnitOfWork unitorWork;
        private string origin = "";
        private string Twilio_AccountSid;
        private string Twilio_AuthToken;
        private string Twillio_TwiMLAppSid;
        private string Twillio_VoiceApiKey;
        private string Twillio_VoiceApiKeySecret;


        public CallService(RAQ_DbContext dbContext,
            IConfiguration config,
            ICommunicationService communicationService,
            IRepository<Ivrsetting> ivrSettings,
            IRepository<InteractiveVoiceResponse> IVR,
            IRepository<ControlListDetail> controlListDetails,
            IRepository<CallLog> callLog,
            IRepository<User> userRepo)
        {

            this._dbContext = dbContext;
            this._config = config;
            this._communicationService = communicationService;
            this._ivrSettingsRepo = ivrSettings;
            this._IVRRepo = IVR;
            this._controlListDetailsRepo = controlListDetails;
            this._callLogRepo = callLog;
            this._userRepo = userRepo;
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


        public TwiMLResult Connect(string phoneNumber, string Twilio_PhoneNumber, string From, string CallSid, string CallStatus)
        {


            var response = new VoiceResponse();
            var CallbackStatusUrl = $"{origin}/Call/CallbackStatus";
            var statusCallbackEventList = new[]{
                //Number.EventEnum.Initiated,
                //Number.EventEnum.Ringing,
                Number.EventEnum.Answered,
                Number.EventEnum.Completed
            }.ToList();

            var statusCallbackEventList1 = new[]{
                //Number.EventEnum.Initiated,
                //Number.EventEnum.Ringing,
                Client.EventEnum.Answered,
                Client.EventEnum.Completed
            }.ToList();
            var dial = new Dial(callerId: phoneNumber.Contains("client")?From:Twilio_PhoneNumber/*, record: Dial.RecordEnum.RecordFromAnswer*/);
            if (phoneNumber.Contains("client"))
            {
                dial.Client(phoneNumber.Replace("client:", ""), statusCallback: new Uri(CallbackStatusUrl), statusCallbackMethod: Twilio.Http.HttpMethod.Post, statusCallbackEvent: statusCallbackEventList1);
            }
            else
            {
                dial.Number(phoneNumber: new PhoneNumber(phoneNumber), statusCallback: new Uri(CallbackStatusUrl), statusCallbackMethod: Twilio.Http.HttpMethod.Post, statusCallbackEvent: statusCallbackEventList);
            }
            response.Append(dial);



            //ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072; //TLS 1.2
            //var call = CallResource.Create(to, from, Twillio_AccountSID, sendDigits: pin, url: new Uri(url), statusCallback: new Uri(CallStatusUrl), record: true, timeout: 30);

            var call = new CallLogVM()
            {
                FromName = From,
                FromPhoneNumber = From,
                ToPhoneNumber = phoneNumber,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow,
                CallStatus = CallStatus,
                Direction = CallDirectionEnums.Outbound.ToString(),
                CreatedDate = DateTime.UtcNow,
                CallSid = CallSid,
                Duration = "0"
            };
            this.saveCallLog(call);
            return TwiML(response);
        }
        public TwiMLResult EnqueueCall(int serviceLineId)
        {
            var users = _dbContext.LoadStoredProcedure("md_getAllUsersByServiceLineId")
                        .WithSqlParam("@pServiceLineId", serviceLineId)
                        .ExecuteStoredProc<UserListVm>().ToList();

            var response = new VoiceResponse();
            var dial = new Dial();
            dial.Client(users.FirstOrDefault().UserUniqueId);
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
            //var To = new PhoneNumber("+923327097498");
            var To = new PhoneNumber("+923327097498");
            var From = new PhoneNumber("+17273867112");
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

        public string CallbackStatus(IFormCollection Request)
        {
            var Callsid = Request["CallSid"].ToString();
            var CallStatus = Request["CallStatus"].ToString();
            var Direction = Request["Direction"].ToString();

            if ( CallStatus == "in-progress")
            {
                string from = Request["From"].ToString();
                string channelSid = this._userRepo.Table.FirstOrDefault(u => u.UserUniqueId == from.Replace("client:", "")).UserChannelSid;
                var attributes = JsonConvert.SerializeObject(new Dictionary<string, Object>()
                                    {
                                        { "callSid", Request["ParentCallSid"].ToString()},
                                        { "eventType", CallEventEnums.Accepted.ToString()},
                                        { "type", "CallEvent"}
                                    });
                var push = this._communicationService.sendPushNotification(new ConversationMessageVM
                {
                    author = AuthorEnums.Call.ToString(),
                    body = CallEventEnums.Accepted.ToString(),
                    attributes = attributes,
                    channelSid = channelSid
                });
            }

            CallLogVM callRec = new();
            if (Direction == "inbound")
            {
                callRec.CallSid = Request["CallSid"].ToString();

            }
            else if (Direction == "outbound-dial")
            {
                callRec.CallSid = Request["ParentCallSid"].ToString();
            }

            callRec = this._dbContext.LoadStoredProcedure("md_getCallByCallSid")
               .WithSqlParam("@pCallSid", callRec.CallSid)
               .ExecuteStoredProc<CallLogVM>().FirstOrDefault();

            callRec.Duration = Request["CallDuration"].ToString();
            callRec.CallStatus = CallStatus;
            callRec.EndTime = DateTime.UtcNow;
            this.saveCallLog(callRec);
            return "ok";
        }
        public TwiMLResult CallConnected(string To, string From)
        { 
                 string OfficeOpenTime = "9:00 AM";
         string OfficeCloseTime = "7:00 PM";

            var dsTime = Convert.ToDateTime(OfficeOpenTime).TimeOfDay;
            var dcTime = Convert.ToDateTime(OfficeCloseTime).TimeOfDay;
            var callTime = DateTime.Now.TimeOfDay;
            var afterOpenTime = TimeSpan.Compare(callTime, dsTime);
            var beforeCloseTime = TimeSpan.Compare(dcTime, callTime);

            var serviceLineId = this._IVRRepo.Table.Where(i => i.IsDeleted!= true && ( i.LandlineNumber == To || i.LandlineNumber == From)).Select(i=>i.ServicelineIdFk).FirstOrDefault();

            var response = new VoiceResponse();
            //var GatherResponseUrl = $"https://" + origin + "/AutomatedCall/PatientResponse?PatientID=" + PatientID + "&AppointmentID=" + AppointmentID + "&Price=" + Price;
            var rootNode = this._dbContext.LoadStoredProcedure("md_getIvrNodesByParentNodeId")                       
                .WithSqlParam("@pParentNodeId", 0)
                .WithSqlParam("@pServiceLineId",serviceLineId)
                .ExecuteStoredProc<IvrSettingVM>().FirstOrDefault();
            if(rootNode != null)
            {
                response.Say(rootNode.Description);

                var childNodes = this._dbContext.LoadStoredProcedure("md_getIvrNodesByParentNodeId")
                    .WithSqlParam("@pParentNodeId", rootNode.IvrSettingsId)
                    .ExecuteStoredProc<IvrSettingVM>().ToList();
                IvrSettingVM childNode = null;
                //TimeSpan startTime = Convert(DateTime);
                if (afterOpenTime < 0 || beforeCloseTime < 0)
                {
                    //afterhours
                    childNode = childNodes.FirstOrDefault(n => n.NodeTypeId == IvrNodeTypeEnums.AfterHour.ToInt());
                }
                else
                {
                    //clinical hours
                    childNode = childNodes.FirstOrDefault(n => n.NodeTypeId == IvrNodeTypeEnums.ClinicalHour.ToInt());
                }
                var GatherResponseUrl = $"{origin}/Call/PromptResponse?parentNodeId={childNode.IvrSettingsId}&serviceLineId={serviceLineId}";
                var gather = new Gather(numDigits: 1, timeout: 10, action: new Uri(GatherResponseUrl)).Say(childNode.Description, language: "en");
                response.Append(gather);
                response.Say("You did not press any key,\n good bye.!");
            }
            else
            {
                response.Say("there is no I V R saved against number that you are calling ");
            }
            
            var xmlResponse = response.ToString();
            return TwiML(response);
        }
        public TwiMLResult PromptResponse(int Digits, int ParentNodeId,int serviceLineId)
        {
            var IvrSetting = this._dbContext.LoadStoredProcedure("md_getIvrNodesByParentNodeId")
               .WithSqlParam("@pParentNodeId", ParentNodeId)
               .WithSqlParam("@pServiceLineId", serviceLineId)
               .ExecuteStoredProc<IvrSettingVM>();
            int QueryDigit = Convert.ToInt32(Digits);

            IvrSettingVM ivrNode = null;
            var ivrParentNode = this._ivrSettingsRepo.Table.FirstOrDefault(i => i.IvrSettingsId == ParentNodeId && i.IsDeleted != true);

            var response = new VoiceResponse();
            if (ivrParentNode.NodeTypeId == IvrNodeTypeEnums.Gather.ToInt()||ivrParentNode.NodeTypeId == IvrNodeTypeEnums.AfterHour.ToInt()||ivrParentNode.NodeTypeId == IvrNodeTypeEnums.ClinicalHour.ToInt())
            {
                ivrNode = IvrSetting.FirstOrDefault(i => i.KeyPress == QueryDigit);
              
            }
            else if(ivrParentNode.NodeTypeId == IvrNodeTypeEnums.Say.ToInt())
            {
                ivrNode = IvrSetting.FirstOrDefault();
            }

            if (ivrNode != null)
            {
                if (ivrNode.NodeTypeId == IvrNodeTypeEnums.Gather.ToInt())
                {
                    var GatherResponseUrl = $"{origin}/Call/PromptResponse?parentNodeId={ivrNode.IvrSettingsId}&serviceLineId={serviceLineId}";
                    var gather = new Gather(numDigits: 1, timeout: 10, action: new Uri(GatherResponseUrl)).Pause(length: 3).Say(ivrNode.Description, language: "en");
                    response.Append(gather);
                    response.Say("You did not press any key,\n good bye.!");
                }
                else if (ivrNode.NodeTypeId == IvrNodeTypeEnums.Voicemail.ToInt())
                {
                    var RecordUrl = $"{origin}/Call/ReceiveVoicemail";
                    response.Say(ivrNode.Description).Pause(2).Say("press # key after recording message");
                    response.Record(action: new Uri(RecordUrl), finishOnKey: "#");
                    response.Say("I did not receive a recording");
                    response.Leave();
                }
                else if (ivrNode.NodeTypeId == IvrNodeTypeEnums.Say.ToInt())
                {
                    var RedirectUrl = $"{origin}/Call/PromptResponse?parentNodeId={ivrNode.IvrSettingsId}&serviceLineId={serviceLineId}";
                    response.Say(ivrNode.Description);
                    response.Redirect(url: new Uri(RedirectUrl));
                }
                else if(ivrNode.NodeTypeId == IvrNodeTypeEnums.Enqueue.ToInt())
                {
                    var enqueueCallUrl = $"{origin}/Call/EnqueueCall?parentNodeId={ivrNode.IvrSettingsId}&serviceLineId={serviceLineId}";
                    response.Say(ivrNode.Description);
                    response.Redirect(url: new Uri(enqueueCallUrl));
                }
            }
            else
            {
                var GatherResponseUrl = $"{origin}/Call/PromptResponse?parentNodeId={ivrParentNode.IvrSettingsId}&serviceLineId={serviceLineId}";
                var gather = new Gather(numDigits: 1, timeout: 10, action: new Uri(GatherResponseUrl)).Pause(length: 3).Say("You Pressed wrong key").Pause(length: 2).Say(ivrParentNode.Description, language: "en");
                response.Append(gather);

                response.Say("You did not press any key,\n good bye.!");
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

        #region [Calls Logging]

        public BaseResponse saveCallLog(CallLogVM log)
        {
            CallLog record = null;
            if (!string.IsNullOrEmpty(log.CallSid))
            {
                //record = this._callLogRepo.Table.Where(i => i.CallSid == log.CallSid).FirstOrDefault();
                //if (record != null)
                //{
                //    record.EndTime = log.CallStatus == "completed" ? DateTime.UtcNow : record.EndTime;
                //    record.CallStatus = log.CallStatus;
                //    record.Duration = log.Duration;

                //    this._callLogRepo.Update(record);


                //}
                //else
                //{
                //    record = new();
                //    record.CallLogId = log.CallLogId;
                //    record.StartTime = log.StartTime;
                //    record.EndTime = log.EndTime;
                //    record.Duration = "0";
                //    record.Direction = log.Direction;
                //    record.CallStatus = log.CallStatus;
                //    record.ToPhoneNumber = log.ToPhoneNumber;
                //    record.ToName = log.ToName;
                //    record.FromPhoneNumber = log.FromPhoneNumber;
                //    record.FromName = log.FromName;
                //    record.CallSid = log.CallSid;
                //    record.ParentCallSid = log.ParentCallSid;
                //    record.RecordingName = log.RecordingName;
                //    record.IsRecorded = log.IsRecorded;
                //    record.CreatedDate = DateTime.UtcNow;

                //    this._callLogRepo.Insert(record);
                //}

                int rowsAffected;
                string sql = "EXEC md_InsertUpdateCallLog " +
                    "@pStartTime, " +
                    "@pEndTime, " +
                    "@pDuration, " +
                    "@pDirection, " +
                    "@pCallStatus, " +
                    "@pToPhoneNumber, " +
                    //"@pToName, " +
                    "@pFromPhoneNumber, " +
                    "@pFromName, " +
                    "@pCallSid, " +
                    //"@pParentCallSid, " +
                    //"@pRecordingName, " +
                    //"@pIsRecorded, " +
                    "@pCreatedDate";

                List<SqlParameter> parms = new List<SqlParameter>
                    { 
                        // Create parameters    
                        new SqlParameter { ParameterName = "@pStartTime", Value = log.StartTime },
                        new SqlParameter { ParameterName = "@pEndTime", Value = DateTime.UtcNow },
                        new SqlParameter { ParameterName = "@pDuration", Value = log.Duration },
                        new SqlParameter { ParameterName = "@pDirection", Value = log.Direction },
                        new SqlParameter { ParameterName = "@pCallStatus", Value = log.CallStatus },
                        new SqlParameter { ParameterName = "@pToPhoneNumber", Value = log.ToPhoneNumber },
                        //new SqlParameter { ParameterName = "@pToName", Value = log.ToName },
                        new SqlParameter { ParameterName = "@pFromPhoneNumber", Value = log.FromPhoneNumber },
                        new SqlParameter { ParameterName = "@pFromName", Value = log.FromName },
                        new SqlParameter { ParameterName = "@pCallSid", Value = log.CallSid },
                        //new SqlParameter { ParameterName = "@pParentCallSid", Value = log.ParentCallSid },
                        //new SqlParameter { ParameterName = "@pRecordingname", Value = log.RecordingName },
                        //new SqlParameter { ParameterName = "@pIsRecorded", Value = log.IsRecorded },
                        new SqlParameter { ParameterName = "@pCreatedDate", Value = DateTime.UtcNow }
                    };

                rowsAffected = this._dbContext.Database.ExecuteSqlRaw(sql, parms.ToArray());
            }
            return new BaseResponse()
            {
                Status = HttpStatusCode.OK,
                Message = "Record Saved",
                Body = record
            };

        }
        public BaseResponse getPreviousCalls()
        {

            var calls = this._dbContext.LoadStoredProcedure("md_getPreviousCallsByUserUniqueId")
                .WithSqlParam("@pUserUniqueId", "")
                .ExecuteStoredProc_ToDictionary();
            return new BaseResponse()
            {
                Status = HttpStatusCode.OK,
                Message = "Record Saved",
                Body = calls
            };

        }
        #endregion


        #region IVR Settings
        //public BaseResponse getIvrTree()
        //{
        //    var IvrSetting = this._ivrSettingsRepo.Table.Where(i => i.IsDeleted!= true).ToList();
        //    if (IvrSetting.Count() > 0)
        //    {
        //        var treeItems = IvrSetting.Select(x => new IvrTreeVM()
        //        {
        //            key = x.IvrSettingsId.ToString(),
        //            ParentKey = x.IvrparentId,
        //            data = x.Description,
        //            label = x.Name,
        //            expandedIcon = x.Icon,
        //            collapsedIcon = x.Icon,
        //            icon = "pi pi-image",
        //            KeyPress = x.KeyPress,
        //            expanded = true
        //        }).ToList();
        //        var treeViewItems = treeItems.BuildIvrTree();
        //        return new BaseResponse { Status = HttpStatusCode.OK, Message = "IVR Returned", Body = treeViewItems };
        //    }
        //    else
        //    {
        //        return new BaseResponse { Status = HttpStatusCode.NotFound, Message = "IVR Not Found" };
        //    }
        //}
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
                    //expandedIcon = x.Icon,
                    //collapsedIcon = x.Icon,
                    icon = x.Icon,
                    NodeTypeId = x.NodeTypeId,
                    EnqueueToRoleIdFk = x.EnqueueToRoleIdFk,
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
        public BaseResponse GetNodeType(int Id)
        {
            var nodeTypes = _controlListDetailsRepo.Table.Where(x => x.IsDeleted != true && x.ControlListIdFk == Id).ToList();
            return new BaseResponse()
            {
                Status = HttpStatusCode.OK,
                Message = "Data Found",
                Body = nodeTypes
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
                    ivrNode.NodeTypeId = model.NodeTypeId;
                    ivrNode.KeyPress = model.KeyPress;
                    ivrNode.EnqueueToRoleIdFk = model.EnqueueToRoleIdFk;
                    ivrNode.ModifiedBy = model.ModifiedBy;
                    ivrNode.ModifiedDate = DateTime.UtcNow;
                    ivrNode.Icon = model.Icon;
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
                ivrNode.NodeTypeId = model.NodeTypeId;
                ivrNode.KeyPress = model.KeyPress;
                ivrNode.EnqueueToRoleIdFk = model.EnqueueToRoleIdFk;
                ivrNode.Icon = model.Icon;
                ivrNode.CreatedBy = model.CreatedBy;
                ivrNode.CreatedDate = DateTime.UtcNow;
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

        public BaseResponse DeleteIVRNode(int Id)
        {
            var IVRNode = _ivrSettingsRepo.Table.Where(x => x.IvrSettingsId == Id && x.IsDeleted != true).FirstOrDefault();
            if (IVRNode != null)
            {
                IVRNode.IsDeleted = true;
                IVRNode.ModifiedBy = ApplicationSettings.UserId;
                IVRNode.ModifiedDate = DateTime.UtcNow;
                this._ivrSettingsRepo.Update(IVRNode);
                this.deleteNodeChildren(Id);

                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Successfully Deleted" };
            }
            else
            {
                return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Record Not Found" };
            }

        }
        public int addIvrParentNodes(int IvrId)
        {
            Ivrsetting ivrNodeCallLanded = new();
            ivrNodeCallLanded.IvrIdFk = IvrId;
            ivrNodeCallLanded.IvrparentId = null;
            ivrNodeCallLanded.Name = "Call Landed";
            ivrNodeCallLanded.Description = "Welcome";
            ivrNodeCallLanded.NodeTypeId = IvrNodeTypeEnums.Say.ToInt();
            ivrNodeCallLanded.KeyPress = null;
            ivrNodeCallLanded.Icon = "pi pi-sitemap";
            ivrNodeCallLanded.CreatedBy = ApplicationSettings.UserId;
            ivrNodeCallLanded.CreatedDate = DateTime.UtcNow;
            ivrNodeCallLanded.IsDeleted = false;
            this._ivrSettingsRepo.Insert(ivrNodeCallLanded);


            Ivrsetting ivrNodeClinicalHour = new();
            ivrNodeClinicalHour.IvrIdFk = IvrId;
            ivrNodeClinicalHour.IvrparentId = ivrNodeCallLanded.IvrSettingsId;
            ivrNodeClinicalHour.Name = "Clinical Hours";
            ivrNodeClinicalHour.Description = "You are Calling in Clinical hours";
            ivrNodeClinicalHour.NodeTypeId = IvrNodeTypeEnums.ClinicalHour.ToInt();
            ivrNodeClinicalHour.KeyPress = null;
            ivrNodeClinicalHour.Icon = "pi pi-clock";
            ivrNodeClinicalHour.CreatedBy = ApplicationSettings.UserId;
            ivrNodeClinicalHour.CreatedDate = DateTime.UtcNow;
            ivrNodeClinicalHour.IsDeleted = false;


            Ivrsetting ivrNodeAfterHour = new();
            ivrNodeAfterHour.IvrIdFk = IvrId;
            ivrNodeAfterHour.IvrparentId = ivrNodeCallLanded.IvrSettingsId;
            ivrNodeAfterHour.Name = "After Hours";
            ivrNodeAfterHour.Description = "You are Calling after Clinical hours";
            ivrNodeAfterHour.NodeTypeId = IvrNodeTypeEnums.AfterHour.ToInt();
            ivrNodeAfterHour.KeyPress = null;
            ivrNodeAfterHour.Icon = "pi pi-clock";
            ivrNodeAfterHour.CreatedBy = ApplicationSettings.UserId;
            ivrNodeAfterHour.CreatedDate = DateTime.UtcNow;
            ivrNodeAfterHour.IsDeleted = false;

            List<Ivrsetting> childNodeList = new();
            childNodeList.Add(ivrNodeAfterHour);
            childNodeList.Add(ivrNodeClinicalHour);

            this._ivrSettingsRepo.Insert(childNodeList);
            if (ivrNodeCallLanded.IvrSettingsId > 0 && ivrNodeClinicalHour.IvrSettingsId > 0 && ivrNodeAfterHour.IvrSettingsId > 0)
            {

                return 1;
            }
            else
            {
                return 0;
            }
        }
        public void deleteNodeChildren(int Id)
        {
            if (this._ivrSettingsRepo.Table.Count(x => x.IvrparentId == Id && x.IsDeleted != true) > 0)
            {

                var childNodes = this._ivrSettingsRepo.Table.Where(x => x.IvrparentId == Id && x.IsDeleted != true).ToList();
                foreach (var node in childNodes)
                {
                    node.IsDeleted = true;
                    node.ModifiedBy = ApplicationSettings.UserId;
                    node.ModifiedDate = DateTime.UtcNow;

                    deleteNodeChildren(node.IvrSettingsId);
                }
                //childNodes.ForEach(x => { x.IsDeleted = true; x.ModifiedBy = ApplicationSettings.UserId; x.ModifiedDate = DateTime.UtcNow; });
                this._ivrSettingsRepo.Update(childNodes);
            }
        }

        public BaseResponse copyIvrSettings(int copyFromServiceLineId,int copyToServicelineId)
        {
            BaseResponse response = new();
            int rowsAffected;
            string sql = "EXEC md_CopyIvrSettings " +
                "@pCopyFromServiceLineId, " +
                "@pCopyToServiceLineId, " +
                "@pCreatedBy ";

            List<SqlParameter> parms = new List<SqlParameter>
            {
                new SqlParameter { ParameterName = "@pCopyFromServiceLineId", Value = copyFromServiceLineId },
                new SqlParameter { ParameterName = "@pCopyToServiceLineId", Value = copyToServicelineId },
                new SqlParameter { ParameterName = "@pCreatedBy",Value=ApplicationSettings.UserId }
            };

            rowsAffected = this._dbContext.Database.ExecuteSqlRaw(sql, parms.ToArray());
            if (rowsAffected > 0)
            {
                response.Status = HttpStatusCode.OK;
                response.Message = "IVR Settings Copied Successfully!"; 
            }
            else
            {
                response.Status = HttpStatusCode.BadRequest;
                response.Message = "Ivr Settings Not Copied";
            }
            return response;
        }
        #endregion


        #region IVR

        public BaseResponse getAllIvrs()
        {
            var IVRs = _IVRRepo.Table.Where(x => x.IsDeleted != true).ToList();
            var types = _controlListDetailsRepo.Table.Where(x => x.ControlListIdFk == UCLEnums.OrgType.ToInt()).Select(x => new { x.ControlListDetailId, x.Title });
            var IVRVMs = AutoMapperHelper.MapList<InteractiveVoiceResponse, IVRVM>(IVRs);
            //IVRVMs.ForEach(x => x.OrganizationType = types.Where(t => t.ControlListDetailId == x.OrganizationTypeIdFk).Select(ty => ty.Title).FirstOrDefault());
            return new BaseResponse()
            {
                Status = HttpStatusCode.OK,
                Message = "Data Found",
                Body = IVRVMs
            };
        }
        public BaseResponse getAllIvrsByOrgId(int orgId)
        {
            var IVRs = _IVRRepo.Table.Where(x => x.IsDeleted != true && x.OrganizationIdFk == orgId).ToList();
            var types = _controlListDetailsRepo.Table.Where(x => x.ControlListIdFk == UCLEnums.OrgType.ToInt()).Select(x => new { x.ControlListDetailId, x.Title });
            var IVRVMs = AutoMapperHelper.MapList<InteractiveVoiceResponse, IVRVM>(IVRs);
            //IVRVMs.ForEach(x => x.OrganizationType = types.Where(t => t.ControlListDetailId == x.OrganizationTypeIdFk).Select(ty => ty.Title).FirstOrDefault());
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
                    ivr.OrganizationIdFk = model.OrganizationIdFk;
                    ivr.ServicelineIdFk = model.ServiceLineIdFk;
                    ivr.Name = model.Name;
                    ivr.Description = model.Description;
                    ivr.LandlineNumber = model.LandlineNumber;
                    ivr.ModifiedBy = model.ModifiedBy;
                    ivr.ModifiedDate = DateTime.UtcNow;
                    ivr.IsDeleted = false;
                    this._IVRRepo.Update(ivr);
                }

            }
            else
            {
                ivr = new InteractiveVoiceResponse();
                ivr.OrganizationIdFk = model.OrganizationIdFk;
                ivr.ServicelineIdFk = model.ServiceLineIdFk;
                ivr.LandlineNumber = model.LandlineNumber;
                ivr.Name = model.Name;
                ivr.Description = model.Description;
                ivr.CreatedBy = model.CreatedBy;
                ivr.CreatedDate = DateTime.UtcNow;
                ivr.IsDeleted = false;
                this._IVRRepo.Insert(ivr);


                var saveRootNodes=this.addIvrParentNodes(ivr.IvrId);
            }
            return new BaseResponse()
            {
                Status = HttpStatusCode.OK,
                Message = "Node Saved",
            };
        }

        public BaseResponse DeleteIVR(int Id)
        {
            var IVRNode = this._IVRRepo.Table.Where(x => x.IvrId == Id && x.IsDeleted != true).FirstOrDefault();
            if (IVRNode != null)
            {
                IVRNode.IsDeleted = true;
                IVRNode.ModifiedBy = ApplicationSettings.UserId;
                IVRNode.ModifiedDate = DateTime.UtcNow;
                this._IVRRepo.Update(IVRNode);

                var childNodes = _ivrSettingsRepo.Table.Where(x => x.IvrIdFk == Id && x.IsDeleted != true).ToList();
                childNodes.ForEach(x => { x.IsDeleted = true; x.ModifiedBy = ApplicationSettings.UserId; x.ModifiedDate = DateTime.UtcNow; });

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
