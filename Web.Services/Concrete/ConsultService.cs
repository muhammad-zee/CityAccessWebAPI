using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Web.Data.Models;
using Web.DLL.Generic_Repository;
using Web.Model;
using Web.Model.Common;
using Web.Services.Enums;
using Web.Services.Extensions;
using Web.Services.Helper;
using Web.Services.Interfaces;

namespace Web.Services.Concrete
{
    public class ConsultService : IConsultService
    {
        private RAQ_DbContext _dbContext;
        private ICommunicationService _communicationService;
        private IRepository<User> _usersRepo;
        private IRepository<Organization> _orgRepo;
        private IRepository<Department> _dptRepo;
        private IRepository<ControlListDetail> _controlListDetailsRepo;
        private IRepository<ConsultField> _consultFieldRepo;
        private IRepository<OrganizationConsultField> _orgConsultRepo;
        private IRepository<ConsultAcknowledgment> _consultAcknowledgmentRepo;
        private IRepository<ServiceLine> _serviceLineRepo;
        IConfiguration _config;

        public ConsultService(RAQ_DbContext dbContext,
            ICommunicationService communicationService,
            IConfiguration config,
            IRepository<User> userRepo,
            IRepository<Organization> orgRepo,
            IRepository<Department> dptRepo,
            IRepository<ControlListDetail> controlListDetailsRepo,
            IRepository<ConsultField> consultFieldRepo,
            IRepository<OrganizationConsultField> orgConsultRepo,
            IRepository<ServiceLine> serviceLineRepo,
            IRepository<ConsultAcknowledgment> consultAcknowledgmentRepo)
        {
            this._dbContext = dbContext;
            this._config = config;
            this._communicationService = communicationService;
            this._usersRepo = userRepo;
            this._orgRepo = orgRepo;
            this._dptRepo = dptRepo;
            this._serviceLineRepo = serviceLineRepo;
            this._controlListDetailsRepo = controlListDetailsRepo;
            this._consultFieldRepo = consultFieldRepo;
            this._orgConsultRepo = orgConsultRepo;
            this._consultAcknowledgmentRepo = consultAcknowledgmentRepo;
        }

        #region Consult Fields

        public BaseResponse GetAllConsultFields()
        {
            var consultFields = this._consultFieldRepo.Table.Where(x => !x.IsDeleted).ToList();
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Found", Body = consultFields };
        }

        public BaseResponse GetConsultFeildsForOrg(int OrgId)
        {
            var consultFields = this._consultFieldRepo.Table.Where(x => !x.IsDeleted).ToList();
            var selectedConsultFields = this._orgConsultRepo.Table.Where(x => x.OrganizationIdFk == OrgId && !x.IsDeleted).Select(x => new { x.ConsultFieldIdFk, x.IsShowInTable, x.SortOrder, x.IsRequired }).ToList();

            var consultFieldVM = AutoMapperHelper.MapList<ConsultField, ConsultFieldsVM>(consultFields);

            foreach (var item in consultFieldVM)
            {
                if (selectedConsultFields.Select(x => x.ConsultFieldIdFk).Contains(item.ConsultFieldId))
                {
                    item.IsRequired = selectedConsultFields.Where(x => x.ConsultFieldIdFk == item.ConsultFieldId).Select(s => s.IsRequired).FirstOrDefault();
                    item.IsShowInTable = selectedConsultFields.Where(x => x.ConsultFieldIdFk == item.ConsultFieldId).Select(s => s.IsShowInTable).FirstOrDefault();
                    item.SortOrder = selectedConsultFields.Where(x => x.ConsultFieldIdFk == item.ConsultFieldId).Select(s => s.SortOrder.Value).FirstOrDefault();
                    item.IsSelected = true;
                }
                else
                {
                    item.IsSelected = false;
                }
            }

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Found", Body = consultFieldVM };
        }
        public BaseResponse GetConsultGraphDataForOrg(int OrgId, int days = 6)
        {

            var today = DateTime.Today;
            var lastweek = today.AddDays(-days);
            var consultFields = this._dbContext.LoadStoredProcedure("md_getConsultGraphDataForDashboard")
                .WithSqlParam("@OrganizationId", OrgId)
                .WithSqlParam("@StartDate", lastweek)
                .WithSqlParam("@EndDate", today)
                .ExecuteStoredProc<GraphVM>();
            var datasets = new List<object>();
            List<string> Label = new();
            while (today.Date >= lastweek.Date)
            {
                Label.Add(lastweek.ToString("MMM-dd"));
                lastweek = lastweek.AddDays(1);
            }
            lastweek = today.AddDays(-days);
            if (consultFields.Count < days)
            {
                List<int> Urgent = new();
                List<int> Routine = new();

                while (today.Date >= lastweek.Date)
                {
                    if (consultFields.Any(x => x.CreatedDate.Date == lastweek.Date))
                    {
                        Urgent.Add(consultFields.Where(x => x.CreatedDate.Date == lastweek.Date).Select(x => x.UrgentConsults).FirstOrDefault());
                        Routine.Add(consultFields.Where(x => x.CreatedDate.Date == lastweek.Date).Select(x => x.RoutineConsults).FirstOrDefault());
                    }
                    else
                    {
                        Urgent.Add(0);
                        Routine.Add(0);
                    }
                    lastweek = lastweek.AddDays(1);
                }

                datasets.Add(new
                {
                    label = "URGENT",
                    backgroundColor = "#089bab",
                    data = Urgent
                });
                datasets.Add(new
                {
                    label = "ROUTINE",
                    backgroundColor = "#CEEBEE",
                    data = Routine
                });
            }
            else
            {
                datasets = new List<object>(){ new
                                                {
                                                  label= "URGENT",
                                                  backgroundColor= "#089bab",
                                                  data= consultFields.Select(c=>c.UrgentConsults).ToList()
                                                },
                                                new
                                                {
                                                  label= "ROUTINE",
                                                  backgroundColor= "#CEEBEE",
                                                  data= consultFields.Select(c=>c.RoutineConsults).ToList()
                                                }
                                            };
            }


            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Found", Body = new { labels = Label, datasets } };
        }
        public BaseResponse GetConsultFormFieldByOrgId(int OrgId)
        {
            var formFields = (from cf in this._consultFieldRepo.Table
                              join ocf in this._orgConsultRepo.Table on cf.ConsultFieldId equals ocf.ConsultFieldIdFk
                              where ocf.OrganizationIdFk == OrgId && !ocf.IsDeleted && !cf.IsDeleted
                              select new
                              {
                                  cf.FieldLabel,
                                  cf.FieldName,
                                  cf.FieldType,
                                  cf.FieldData,
                                  cf.FieldDataType,
                                  cf.FieldDataLength,
                                  ocf.IsRequired
                              }).Distinct().ToList();
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Found", Body = formFields };
        }

        public BaseResponse AddOrUpdateConsultFeilds(ConsultFieldsVM consultField)
        {
            if (consultField.ConsultFieldId > 0)
            {
                var consultFeilds = this._consultFieldRepo.Table.Where(x => x.ConsultFieldId == consultField.ConsultFieldId && !x.IsDeleted).FirstOrDefault();
                if (consultFeilds != null)
                {
                    consultFeilds.FieldLabel = consultField.FieldLabel;
                    consultFeilds.FieldName = consultField.FieldName;
                    consultFeilds.FieldType = consultField.FieldType;
                    consultFeilds.FieldDataType = consultField.FieldDataType;
                    consultFeilds.FieldDataLength = consultField.FieldDataLength;
                    consultFeilds.ModifiedBy = consultField.ModifiedBy;
                    consultFeilds.ModifiedDate = DateTime.UtcNow;
                    consultFeilds.IsDeleted = false;

                    this._consultFieldRepo.Update(consultFeilds);
                    return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Updated Successfully" };
                }
                return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Record Not Found" };
            }
            else
            {
                var consultFields = new ConsultField()
                {
                    FieldLabel = consultField.FieldLabel,
                    FieldName = consultField.FieldName,
                    FieldType = consultField.FieldType,
                    FieldDataType = consultField.FieldDataType,
                    FieldDataLength = consultField.FieldDataLength,
                    CreatedBy = consultField.CreatedBy,
                    CreatedDate = DateTime.UtcNow,
                    IsDeleted = false
                };

                this._consultFieldRepo.Insert(consultFields);
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Added Successfully" };
            }
        }

        #endregion


        #region Organization Consult Fields

        public BaseResponse AddOrUpdateOrgConsultFeilds(List<OrgConsultFieldsVM> orgConsultFields)
        {

            if (orgConsultFields.Count == 1 && orgConsultFields.Select(x => x.ConsultFieldIdFk).FirstOrDefault() == 0)
            {
                var toBeDeletedRows = this._orgConsultRepo.Table.Where(x => x.OrganizationIdFk == orgConsultFields.Select(x => x.OrganizationIdFk).FirstOrDefault() && !x.IsDeleted).ToList();
                toBeDeletedRows.ForEach(x => { x.ModifiedBy = ApplicationSettings.UserId; x.ModifiedDate = DateTime.UtcNow; x.IsDeleted = true; });
                this._orgConsultRepo.Update(toBeDeletedRows);
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Deleted Successfully" };
            }
            else
            {
                var duplicateObj = orgConsultFields.Select(x => new { x.OrganizationIdFk, x.ConsultFieldIdFk }).ToList();

                var alreadyExistFields = this._orgConsultRepo.Table.Where(x => duplicateObj.Select(y => y.ConsultFieldIdFk).Contains(x.ConsultFieldIdFk) && duplicateObj.Select(y => y.OrganizationIdFk).Contains(x.OrganizationIdFk) && !x.IsDeleted).Distinct().ToList();

                var objsNeedToUpdate = alreadyExistFields.Where(x => duplicateObj.Select(c => c.ConsultFieldIdFk).Contains(x.ConsultFieldIdFk)).ToList();

                if (objsNeedToUpdate.Count > 0)
                {
                    foreach (var item in objsNeedToUpdate)
                    {
                        item.SortOrder = orgConsultFields.Where(x => x.ConsultFieldIdFk == item.ConsultFieldIdFk).Select(x => x.SortOrder).FirstOrDefault();
                        item.IsRequired = orgConsultFields.Where(x => x.ConsultFieldIdFk == item.ConsultFieldIdFk).Select(x => x.IsRequired).FirstOrDefault();
                        item.IsShowInTable = orgConsultFields.Where(x => x.ConsultFieldIdFk == item.ConsultFieldIdFk).Select(x => x.IsShowInTable).FirstOrDefault();
                    }
                    this._orgConsultRepo.Update(objsNeedToUpdate);
                }

                duplicateObj.RemoveAll(r => alreadyExistFields.Select(x => x.ConsultFieldIdFk).Contains(r.ConsultFieldIdFk));

                var orgConsults = AutoMapperHelper.MapList<OrgConsultFieldsVM, OrganizationConsultField>(orgConsultFields.Where(x => duplicateObj.Select(c => c.ConsultFieldIdFk).Contains(x.ConsultFieldIdFk)).ToList());

                if (orgConsults.Count > 0)
                {
                    this._orgConsultRepo.Insert(orgConsults);
                }

                alreadyExistFields = this._orgConsultRepo.Table.Where(x => duplicateObj.Select(y => y.ConsultFieldIdFk).Contains(x.ConsultFieldIdFk) && duplicateObj.Select(y => y.OrganizationIdFk).Contains(x.OrganizationIdFk) && !x.IsDeleted).Distinct().ToList();
                duplicateObj.RemoveAll(r => alreadyExistFields.Select(x => x.ConsultFieldIdFk).Contains(r.ConsultFieldIdFk));

                var deletedOnes = this._orgConsultRepo.Table.Where(x => !(orgConsultFields.Select(y => y.ConsultFieldIdFk).Contains(x.ConsultFieldIdFk)) && orgConsultFields.Select(y => y.OrganizationIdFk).Contains(x.OrganizationIdFk) && !x.IsDeleted).ToList();

                int? ModifiedBy = orgConsultFields.Select(x => x.ModifiedBy).FirstOrDefault();

                if (deletedOnes.Count > 0)
                {
                    deletedOnes.ForEach(x => { x.ModifiedBy = ModifiedBy; x.ModifiedDate = DateTime.UtcNow; x.IsDeleted = true; });
                    this._orgConsultRepo.Update(deletedOnes);
                }

                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Saved Successfully" };
            }
        }

        public BaseResponse GetConsultFormByOrgId(int orgId)
        {
            var consultFields = _dbContext.LoadStoredProcedure("md_getConsultFormByOrgId")
                                .WithSqlParam("@pOrganizationId", orgId)
                                .ExecuteStoredProc<ConsultFieldsVM>();

            //(from c in this._consultFieldRepo.Table
            //                 join oc in this._orgConsultRepo.Table on c.ConsultFieldId equals oc.ConsultFieldIdFk
            //                 where oc.OrganizationIdFk == orgId && !c.IsDeleted && !oc.IsDeleted
            //                 select new ConsultFieldsVM()
            //                 {
            //                     ConsultFieldId = c.ConsultFieldId,
            //                     FieldLabel = c.FieldLabel,
            //                     FieldName = c.FieldName,
            //                     FieldType = c.FieldType,
            //                     FieldData = c.FieldData,
            //                     FieldDataType = c.FieldDataType,
            //                     FieldDataLength = c.FieldDataLength,
            //                     SortOrder = oc.SortOrder,
            //                     IsRequired = c.IsRequired
            //                 }).Distinct().OrderBy(x => x.SortOrder).ToList();

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Returned", Body = consultFields };
        }

        #endregion

        #region Consults

        public BaseResponse GetAllConsults()
        {
            var consultData = _dbContext.LoadStoredProcedure("md_getAllConsults").ExecuteStoredProc_ToDictionary();

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Returned", Body = consultData };
        }
        public BaseResponse GetConsultsByServiceLineId(ConsultVM consult)
        {

            var fields = _dbContext.LoadStoredProcedure("md_getShowInTableColumnsForConsult")
                                    .WithSqlParam("@orgId", consult.OrganizationId)
                                    .ExecuteStoredProc<ConsultFieldsVM>().Select(x => new { x.FieldName, x.FieldDataType, x.FieldLabel }).FirstOrDefault();

            if (fields.FieldName != null && fields.FieldName != null)
            {
              var consultData = _dbContext.LoadStoredProcedure("md_getGetConsultsByServiceLineId_Daynamic")
                                        .WithSqlParam("@status", consult.Status)
                                        .WithSqlParam("@colName", fields.FieldName)
                                        .WithSqlParam("@organizationId", consult.OrganizationId)
                                        .WithSqlParam("@departmentIds", consult.DepartmentIds)
                                        .WithSqlParam("@serviceLineIds", consult.ServiceLineIds)
                                        .WithSqlParam("@userId", ApplicationSettings.UserId)
                                        .WithSqlParam("@showAllConsults", consult.showAllConsults)
                                        .WithSqlParam("@isFromDashboard", consult.IsFromDashboard)

                                        .WithSqlParam("@page", consult.PageNumber)
                                        .WithSqlParam("@size", consult.Rows)
                                        .WithSqlParam("@sortOrder", consult.sortOrder)
                                        .WithSqlParam("@sortCol", consult.SortCol)
                                        .WithSqlParam("@filterVal", consult.FilterVal)
                                        .ExecuteStoredProc_ToDictionary();

                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Returned", Body = new { consultData, fields } };
            }
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Fields Name Not Found", Body = new { consultData = new List<object>(), fields } };
        }
        public BaseResponse GetConsultById(int Id)
        {
            var consultData = _dbContext.LoadStoredProcedure("md_getConsultById")
                .WithSqlParam("@consultId", Id)
                .ExecuteStoredProc_ToDictionary();

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Returned", Body = consultData };
        }
        public BaseResponse AddOrUpdateConsult(IDictionary<string, object> keyValues)
        {
            var keys = keyValues.Keys.ToList();
            var values = keyValues.Values.ToList();

            bool isConsultIdExist = keyValues.ContainsKey("ConsultId");
            if (isConsultIdExist && keyValues["ConsultId"].ToString() == "0")
            {
                var Consult_Counter = _dbContext.LoadStoredProcedure("md_getMDRouteCounter").WithSqlParam("@C_Name", "Consult_Counter").ExecuteStoredProc<MDRoute_CounterVM>().FirstOrDefault();
                keyValues.Add("Consult_Counter", Consult_Counter.Counter_Value);
                string query = "INSERT INTO [dbo].[Consults] (";

                for (int i = 0; i < keys.Count(); i++)
                {
                    if (keys[i] != "ConsultId" && keys[i] != "CreatedBy" && keys[i] != "CreatedDate" && keys[i] != "CallbackNumber")
                    {
                        query += $"[{keys[i]}]";
                        if ((i + 1) == keys.Count)
                        {
                            //if (keyValues.ContainsKey("DateOfBirth"))
                            //    query += ",[DateOfBirth]";
                            query += keyValues.ContainsKey("CallbackNumber") ? ",[CallbackNumber]" : "";
                            query += ",[ConsultNumber]";
                            query += ",[CreatedBy]";
                            query += ",[CreatedDate]";
                            query += ",[IsDeleted]";
                            query += ")";
                        }
                        else
                        {
                            query += ",";
                        }
                    }
                }

                query += " VALUES (";

                for (int i = 0; i < values.Count; i++)
                {
                    if (keys[i] != "ConsultId" && keys[i] != "CreatedBy" && keys[i] != "CreatedDate" && keys[i] != "CallbackNumber")
                    {
                        query += $"'{values[i]}'";
                        if ((i + 1) == values.Count)
                        {
                            //if (keyValues.ContainsKey("DateOfBirth")) 
                            //{
                            //    var dob = DateTime.Parse(keyValues["DateOfBirth"].ToString()).ToString("MM-dd-yyyy hh:mm:ss");
                            //    query += $",'{dob}'";
                            //}
                            query += keyValues.ContainsKey("CallbackNumber") && keyValues["CallbackNumber"] != null && keyValues["CallbackNumber"].ToString() != "(___) ___-____" ? ",'" + keyValues["CallbackNumber"] + "'" : ",''";
                            query += $",'{Consult_Counter.Counter_Value}'";
                            query += $",'{ApplicationSettings.UserId}'";
                            query += $",'{DateTime.UtcNow}'";
                            query += ",'0'";
                            query += ")";
                        }
                        else
                        {
                            query += ",";
                        }
                    }
                }

                int rowsEffect = this._dbContext.Database.ExecuteSqlRaw(query);
                if (rowsEffect > 0)
                {
                    var consult = this._dbContext.LoadSQLQuery($"Select ConsultId from consults where consultNumber = { Consult_Counter.Counter_Value.ToInt()}").ExecuteStoredProc<Consult>().FirstOrDefault();

                    this._dbContext.Log(keyValues, ActivityLogTableEnums.Consults.ToString(), consult.ConsultId.ToInt(), ActivityLogActionEnums.Create.ToInt());

                    //    if (keys.Contains("ServiceLineIdFk") && keyValues["ServiceLineIdFk"].ToString() != "0")
                    //    {
                    //        var serviceLineId = keyValues["ServiceLineIdFk"].ToString().ToInt();

                    //        var users = _dbContext.LoadStoredProcedure("md_getAvailableUserOnSchedule")
                    //                    .WithSqlParam("@servicelineIdFk", serviceLineId)
                    //                    .WithSqlParam("@dayOfWeek", DateTime.UtcNow.DayOfWeek.ToString())
                    //                    .ExecuteStoredProc<RegisterCredentialVM>();

                    //        var consultType = _controlListDetailsRepo.Table.Where(x => x.ControlListDetailId == keyValues["ConsultType"].ToString().ToInt()).Select(x => x.Title).FirstOrDefault();

                    //        var conversationChannelAttributes = JsonConvert.SerializeObject(new Dictionary<string, Object>()
                    //                    {
                    //                        {ChannelAttributeEnums.ChannelType.ToString(), ChannelTypeEnums.Consult.ToString()},
                    //                        {ChannelAttributeEnums.ConsultId.ToString(), Consult_Counter.Counter_Value}
                    //                    }, Formatting.Indented);

                    //        var superAdmins = this._usersRepo.Table.Where(x => x.IsInGroup && !x.IsDeleted).Select(x => new RegisterCredentialVM() { UserUniqueId = x.UserUniqueId, UserId = x.UserId }).ToList();
                    //        users.AddRange(superAdmins);
                    //        var loggedUser = (from u in this._usersRepo.Table
                    //                          where u.UserId == ApplicationSettings.UserId
                    //                          select new RegisterCredentialVM
                    //                          {
                    //                              UserUniqueId = u.UserUniqueId,
                    //                              UserId = u.UserId
                    //                          }).FirstOrDefault();
                    //        users.Add(loggedUser);

                    //        if (users != null && users.Count > 0 && users.FirstOrDefault().IsAfterHours == true)
                    //        {
                    //            if (keys.Contains("ConsultType") && keyValues["ConsultType"].ToString() != null && keyValues["ConsultType"].ToString() != "")
                    //            {

                    //                if (consultType != null && consultType == "Urgent")
                    //                {
                    //                    //string uniqueName = $"CONSULT_{Consult_Counter.Counter_Value.ToString()}";
                    //                    string ServiceName = this._serviceLineRepo.Table.Where(x => x.ServiceLineId == serviceLineId && !x.IsDeleted).Select(x => x.ServiceName).FirstOrDefault();
                    //                    string uniqueName = DateTime.Now.ToString("yyyyMMddHHmmssffff") + ApplicationSettings.UserId.ToString();
                    //                    string friendlyName = $"{consultType} {ServiceName} Consult {Consult_Counter.Counter_Value}";
                    //                    var channel = _communicationService.createConversationChannel(friendlyName, uniqueName, conversationChannelAttributes);

                    //                    //////////////// Update Consult For ChannelSid ////////////////////////////

                    //                    string ConsultId = keyValues["ConsultId"].ToString();
                    //                    string qry = $"UPDATE [dbo].[Consults] SET [ChannelSid] = '{channel.Sid}'";

                    //                    qry += $" WHERE ConsultNumber = '{Consult_Counter.Counter_Value}'";

                    //                    int rowUpdate = this._dbContext.Database.ExecuteSqlRaw(qry);

                    //                    ///////////////////////////////////////////////////////////////////////////


                    //                    List<ConsultAcknowledgment> consultAcknowledgmentList = new();
                    //                    users = users.Distinct().ToList();
                    //                    var distinctUsers = users.Select(x => new { x.UserUniqueId, x.UserId }).Distinct().ToList();

                    //                    foreach (var item in distinctUsers)
                    //                    {
                    //                        try
                    //                        {
                    //                            this._communicationService.addNewUserToConversationChannel(channel.Sid, item.UserUniqueId);
                    //                            var acknowledgeConsult = new ConsultAcknowledgment
                    //                            {
                    //                                IsAcknowledge = false,
                    //                                ConsultIdFk = Consult_Counter.Counter_Value,
                    //                                UserIdFk = item.UserId,
                    //                                CreatedBy = ApplicationSettings.UserId,
                    //                                CreatedDate = DateTime.UtcNow
                    //                            };
                    //                            consultAcknowledgmentList.Add(acknowledgeConsult);
                    //                        }
                    //                        catch (Exception ex)
                    //                        {

                    //                        }
                    //                    }
                    //                    this._consultAcknowledgmentRepo.Insert(consultAcknowledgmentList);
                    //                    var msg = new ConversationMessageVM();
                    //                    msg.channelSid = channel.Sid;
                    //                    msg.author = "System";
                    //                    msg.attributes = "";
                    //                    msg.body = $"<strong> {consultType} {ServiceName} Consult</strong></br></br>";
                    //                    if (keyValues.ContainsKey("PatientFirstName") && keyValues.ContainsKey("PatientLastName"))
                    //                    {
                    //                        msg.body += $"<strong>Patient Name:</strong> {keyValues["PatientFirstName"].ToString()} {keyValues["PatientLastName"].ToString()} </br>";
                    //                    }
                    //                    else
                    //                    {
                    //                        if (keyValues.ContainsKey("PatientFirstName"))
                    //                        {
                    //                            msg.body += $"<strong>Patient Name:</strong> {keyValues["PatientFirstName"].ToString()} </br>";
                    //                        }
                    //                        if (keyValues.ContainsKey("PatientLastName"))
                    //                        {
                    //                            msg.body += $"<strong>Patient Name:</strong> {keyValues["PatientLastName"].ToString()} </br>";
                    //                        }
                    //                    }
                    //                    if (keyValues.ContainsKey("DateOfBirth"))
                    //                    {
                    //                        DateTime dob = DateTime.Parse(keyValues["DateOfBirth"].ToString());
                    //                        msg.body += $"<strong>Dob:</strong> {dob:MM-dd-yyyy} </br>";
                    //                    }
                    //                    msg.body += keyValues.ContainsKey("MedicalRecordNumber") ? $"<strong>Medical Record Number:</strong> {keyValues["MedicalRecordNumber"].ToString()} </br>" : "";
                    //                    msg.body += keyValues.ContainsKey("CallbackNumber") && keyValues["CallbackNumber"].ToString() != "(___) ___-____" ? $"<strong>Callback Number:</strong> {keyValues["CallbackNumber"].ToString()} </br>" : "";
                    //                    _communicationService.sendPushNotification(msg);

                    //                    var orgByServiceId = _dptRepo.Table.Where(x => !x.IsDeleted && x.DepartmentId == _serviceLineRepo.Table.Where(x => !x.IsDeleted && x.ServiceLineId == serviceLineId).Select(x => x.DepartmentIdFk).FirstOrDefault()).Select(x => x.OrganizationIdFk).FirstOrDefault();
                    //                    var showAllAccessUsers = this._dbContext.LoadStoredProcedure("md_getUsersOfComponentAccess")
                    //                                                                   .WithSqlParam("@componentName", "Show Consults,Show All Consults,Show Graphs")
                    //                                                                   .WithSqlParam("@orgId", orgByServiceId.Value)
                    //                                                                   .ExecuteStoredProc<RegisterCredentialVM>(); //.Select(x => new { x.UserUniqueId, x.UserId }).Distinct().ToList();
                    //                    users.AddRange(showAllAccessUsers);
                    //                    distinctUsers = users.Select(x => new { x.UserUniqueId, x.UserId }).Distinct().ToList();
                    //                    var notification = new PushNotificationVM()
                    //                    {
                    //                        Id = keyValues["CallbackNumber"].ToString().ToInt(),
                    //                        OrgId = orgByServiceId.Value,
                    //                        UserChannelSid = users.Select(x => x.UserUniqueId).Distinct().ToList(),
                    //                        From = "Consult",
                    //                        Msg = "New Consult is Created",
                    //                        RouteLink1 = "/Home/Dashboard",
                    //                        RouteLink2 = "/Home/Consult"
                    //                    };

                    //                    _communicationService.pushNotification(notification);
                    //                }
                    //            }
                    //        }
                    //        else if (users != null && users.Count > 0)
                    //        {
                    //            //string uniqueName = $"CONSULT_{Consult_Counter.Counter_Value.ToString()}";
                    //            string ServiceName = this._serviceLineRepo.Table.Where(x => x.ServiceLineId == serviceLineId && !x.IsDeleted).Select(x => x.ServiceName).FirstOrDefault();
                    //            string uniqueName = DateTime.Now.ToString("yyyyMMddHHmmssffff") + ApplicationSettings.UserId.ToString();
                    //            string friendlyName = $"{consultType} {ServiceName} Consult {Consult_Counter.Counter_Value}";
                    //            var channel = _communicationService.createConversationChannel(friendlyName, uniqueName, conversationChannelAttributes);

                    //            //////////////// Update Consult For ChannelSid ////////////////////////////

                    //            string ConsultId = keyValues["ConsultId"].ToString();
                    //            string qry = $"UPDATE [dbo].[Consults] SET [ChannelSid] = '{channel.Sid}'";

                    //            qry += $" WHERE ConsultNumber = '{Consult_Counter.Counter_Value}'";

                    //            int rowUpdate = this._dbContext.Database.ExecuteSqlRaw(qry);

                    //            ///////////////////////////////////////////////////////////////////////////

                    //            List<ConsultAcknowledgment> consultAcknowledgmentList = new();
                    //            var distinctUsers = users.Select(x => new { x.UserUniqueId, x.UserId }).Distinct().ToList();
                    //            foreach (var item in distinctUsers)
                    //            {
                    //                try
                    //                {
                    //                    this._communicationService.addNewUserToConversationChannel(channel.Sid, item.UserUniqueId);
                    //                    var acknowledgeConsult = new ConsultAcknowledgment
                    //                    {
                    //                        IsAcknowledge = false,
                    //                        ConsultIdFk = Consult_Counter.Counter_Value,
                    //                        UserIdFk = item.UserId,
                    //                        CreatedBy = ApplicationSettings.UserId,
                    //                        CreatedDate = DateTime.UtcNow
                    //                    };
                    //                    consultAcknowledgmentList.Add(acknowledgeConsult);
                    //                }
                    //                catch (Exception ex)
                    //                {

                    //                }
                    //            }
                    //            this._consultAcknowledgmentRepo.Insert(consultAcknowledgmentList);
                    //            var msg = new ConversationMessageVM();
                    //            msg.author = "System";
                    //            msg.attributes = "";
                    //            msg.body = $"<strong>{consultType} {ServiceName} Consult </strong> </br></br>";
                    //            if (keyValues.ContainsKey("PatientFirstName") && keyValues.ContainsKey("PatientLastName"))
                    //            {
                    //                msg.body += $"<strong>Patient Name:</strong> {keyValues["PatientFirstName"].ToString()} {keyValues["PatientLastName"].ToString()} </br>";
                    //            }
                    //            else
                    //            {
                    //                if (keyValues.ContainsKey("PatientFirstName"))
                    //                {
                    //                    msg.body += $"<strong>Patient Name:</strong> {keyValues["PatientFirstName"].ToString()} </br>";
                    //                }
                    //                if (keyValues.ContainsKey("PatientLastName"))
                    //                {
                    //                    msg.body += $"<strong>Patient Name:</strong> {keyValues["PatientLastName"].ToString()} </br>";
                    //                }
                    //            }
                    //            if (keyValues.ContainsKey("DateOfBirth"))
                    //            {
                    //                DateTime dob = DateTime.Parse(keyValues["DateOfBirth"].ToString());
                    //                msg.body += $"<strong>Dob:</strong> {dob:MM-dd-yyyy} </br>";
                    //            }
                    //            msg.body += keyValues.ContainsKey("MedicalRecordNumber") ? $"<strong>Medical Record Number:</strong> {keyValues["MedicalRecordNumber"].ToString()} </br>" : "";
                    //            msg.body += keyValues.ContainsKey("CallbackNumber") && keyValues["CallbackNumber"].ToString() != "(___) ___-____" ? $"<strong>Callback Number:</strong> {keyValues["CallbackNumber"].ToString()} </br>" : "";
                    //            msg.channelSid = channel.Sid;

                    //            var sendMsg = _communicationService.sendPushNotification(msg);

                    //            var orgByServiceId = _dptRepo.Table.Where(x => !x.IsDeleted && x.DepartmentId == _serviceLineRepo.Table.Where(x => !x.IsDeleted && x.ServiceLineId == serviceLineId).Select(x => x.DepartmentIdFk).FirstOrDefault()).Select(x => x.OrganizationIdFk).FirstOrDefault();
                    //            var showAllAccessUsers = this._dbContext.LoadStoredProcedure("md_getUsersOfComponentAccess")
                    //                               .WithSqlParam("@componentName", "Show Consults,Show All Consults,Show Graphs")
                    //                               .WithSqlParam("@orgId", orgByServiceId.Value)
                    //                               .ExecuteStoredProc<RegisterCredentialVM>(); //.Select(x => new { x.UserUniqueId, x.UserId }).Distinct().ToList();
                    //            users.AddRange(showAllAccessUsers);
                    //            distinctUsers = users.Select(x => new { x.UserUniqueId, x.UserId }).Distinct().ToList();
                    //            var notification = new PushNotificationVM()
                    //            {
                    //                Id = keyValues["CallbackNumber"].ToString().ToInt(),
                    //                OrgId = orgByServiceId.Value,
                    //                UserChannelSid = distinctUsers.Select(x => x.UserUniqueId).Distinct().ToList(),
                    //                From = "Consult",
                    //                Msg = "New Consult is Created",
                    //                RouteLink1 = "/Home/Dashboard",
                    //                RouteLink2 = "/Home/Consult"
                    //            };

                    //            _communicationService.pushNotification(notification);
                    //        }
                    //    }

                    return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Added Successfully", Body = keyValues };
                }
                else
                {
                    return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = "No Record Added" };
                }
            }
            if (isConsultIdExist && keyValues["ConsultId"].ToString() != "0")
            {
                string ConsultId = keyValues["ConsultId"].ToString();
                string query = "UPDATE [dbo].[Consults] SET ";

                for (int i = 0; i < keys.Count; i++)
                {
                    if (keys[i] != "ConsultId" && keys[i] != "ModifiedBy" && keys[i] != "ModifiedDate" && keys[i] != "CallbackNumber")
                    {
                        query += $"[{keys[i]}] = '{values[i]}'";
                        if (i < keys.Count)
                        {
                            query += ",";
                        }
                    }
                }
                query += keyValues.ContainsKey("CallbackNumber") && keyValues["CallbackNumber"].ToString() != "(___) ___-____" ? " [CallbackNumber] ='" + keyValues["CallbackNumber"].ToString() + "'," : "";
                query += $" [ModifiedBy] = '{ApplicationSettings.UserId}', [ModifiedDate] = '{DateTime.UtcNow.ToString("MM-dd-yyyy hh:mm:ss")}'";
                query += $" WHERE ConsultId = '{ConsultId.ToInt()}'";

                int rowsEffect = this._dbContext.Database.ExecuteSqlRaw(query);
                if (rowsEffect > 0)
                {
                    this._dbContext.Log(keyValues, ActivityLogTableEnums.Consults.ToString(), keyValues["ConsultId"].ToString().ToInt(), ActivityLogActionEnums.Update.ToInt());
                    return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Updated Successfully" };
                }
                else
                {
                    return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = "No Record Updated" };
                }
            }


            return new BaseResponse() { Status = HttpStatusCode.NotModified, Message = "Consult Id Column is not exist" };
        }
        public BaseResponse CreateConsultGroup(IDictionary<string, object> keyValues)
        {

            var keys = keyValues.Keys.ToList();
            var values = keyValues.Values.ToList();
            bool usersFound = false;
            var Consult_Counter = keyValues["Consult_Counter"].ToString().ToLong();
            if (keys.Contains("ServiceLineIdFk") && keyValues["ServiceLineIdFk"].ToString() != "0")
            {
                var serviceLineId = keyValues["ServiceLineIdFk"].ToString().ToInt();

                var users = _dbContext.LoadStoredProcedure("md_getAvailableUserOnSchedule")
                            .WithSqlParam("@servicelineIdFk", serviceLineId)
                            .WithSqlParam("@dayOfWeek", DateTime.UtcNow.DayOfWeek.ToString())
                            .ExecuteStoredProc<RegisterCredentialVM>();
                usersFound = users.Count() > 0;
                var consultType = _controlListDetailsRepo.Table.Where(x => x.ControlListDetailId == keyValues["ConsultType"].ToString().ToInt()).Select(x => x.Title).FirstOrDefault();

                var conversationChannelAttributes = JsonConvert.SerializeObject(new Dictionary<string, Object>()
                                    {
                                        {ChannelAttributeEnums.ChannelType.ToString(), ChannelTypeEnums.Consult.ToString()},
                                        {ChannelAttributeEnums.ConsultId.ToString(), Consult_Counter}
                                    }, Formatting.Indented);

                var superAdmins = this._usersRepo.Table.Where(x => x.IsInGroup && !x.IsDeleted).Select(x => new RegisterCredentialVM() { UserUniqueId = x.UserUniqueId, UserId = x.UserId }).ToList();
                users.AddRange(superAdmins);
                var loggedUser = (from u in this._usersRepo.Table
                                  where u.UserId == ApplicationSettings.UserId
                                  select new RegisterCredentialVM
                                  {
                                      UserUniqueId = u.UserUniqueId,
                                      UserId = u.UserId
                                  }).FirstOrDefault();
                users.Add(loggedUser);

                if (users != null && users.Count > 0 && users.FirstOrDefault().IsAfterHours == true)
                {
                    if (keys.Contains("ConsultType") && keyValues["ConsultType"].ToString() != null && keyValues["ConsultType"].ToString() != "")
                    {

                        if (consultType != null && consultType == "Urgent")
                        {
                            //string uniqueName = $"CONSULT_{Consult_Counter.ToString()}";
                            string ServiceName = this._serviceLineRepo.Table.Where(x => x.ServiceLineId == serviceLineId && !x.IsDeleted).Select(x => x.ServiceName).FirstOrDefault();
                            string uniqueName = DateTime.Now.ToString("yyyyMMddHHmmssffff") + ApplicationSettings.UserId.ToString();
                            string friendlyName = $"{consultType} {ServiceName} Consult {Consult_Counter}";
                            var channel = _communicationService.createConversationChannel(friendlyName, uniqueName, conversationChannelAttributes);

                            ////////////// Update Consult For ChannelSid ////////////////////////////

                            string ConsultId = keyValues["ConsultId"].ToString();
                            string qry = $"UPDATE [dbo].[Consults] SET [ChannelSid] = '{channel.Sid}'";

                            qry += $" WHERE ConsultNumber = '{Consult_Counter}'";

                            int rowUpdate = this._dbContext.Database.ExecuteSqlRaw(qry);

                            /////////////////////////////////////////////////////////////////////////


                            List<ConsultAcknowledgment> consultAcknowledgmentList = new();
                            users = users.Distinct().ToList();
                            var distinctUsers = users.Select(x => new { x.UserUniqueId, x.UserId }).Distinct().ToList();

                            foreach (var item in distinctUsers)
                            {
                                try
                                {
                                    this._communicationService.addNewUserToConversationChannel(channel.Sid, item.UserUniqueId);
                                    var acknowledgeConsult = new ConsultAcknowledgment
                                    {
                                        IsAcknowledge = false,
                                        ConsultIdFk = Consult_Counter,
                                        UserIdFk = item.UserId,
                                        CreatedBy = ApplicationSettings.UserId,
                                        CreatedDate = DateTime.UtcNow
                                    };
                                    consultAcknowledgmentList.Add(acknowledgeConsult);
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                            this._consultAcknowledgmentRepo.Insert(consultAcknowledgmentList);
                            var msg = new ConversationMessageVM();
                            msg.channelSid = channel.Sid;
                            msg.author = "System";
                            msg.attributes = "";
                            msg.body = $"<strong> {consultType} {ServiceName} Consult</strong></br></br>";
                            if (keyValues.ContainsKey("PatientFirstName") && keyValues.ContainsKey("PatientLastName"))
                            {
                                msg.body += $"<strong>Patient Name:</strong> {keyValues["PatientFirstName"].ToString()} {keyValues["PatientLastName"].ToString()} </br>";
                            }
                            else
                            {
                                if (keyValues.ContainsKey("PatientFirstName"))
                                {
                                    msg.body += $"<strong>Patient Name:</strong> {keyValues["PatientFirstName"].ToString()} </br>";
                                }
                                if (keyValues.ContainsKey("PatientLastName"))
                                {
                                    msg.body += $"<strong>Patient Name:</strong> {keyValues["PatientLastName"].ToString()} </br>";
                                }
                            }
                            if (keyValues.ContainsKey("DateOfBirth"))
                            {
                                DateTime dob = DateTime.Parse(keyValues["DateOfBirth"].ToString());
                                msg.body += $"<strong>Dob:</strong> {dob:MM-dd-yyyy} </br>";
                            }
                            msg.body += keyValues.ContainsKey("MedicalRecordNumber") ? $"<strong>Medical Record Number:</strong> {keyValues["MedicalRecordNumber"].ToString()} </br>" : "";
                            msg.body += keyValues.ContainsKey("CallbackNumber") && keyValues["CallbackNumber"].ToString() != "(___) ___-____" ? $"<strong>Callback Number:</strong> {keyValues["CallbackNumber"].ToString()} </br>" : "";
                            _communicationService.sendPushNotification(msg);

                            var orgByServiceId = _dptRepo.Table.Where(x => !x.IsDeleted && x.DepartmentId == _serviceLineRepo.Table.Where(x => !x.IsDeleted && x.ServiceLineId == serviceLineId).Select(x => x.DepartmentIdFk).FirstOrDefault()).Select(x => x.OrganizationIdFk).FirstOrDefault();
                            var showAllAccessUsers = this._dbContext.LoadStoredProcedure("md_getUsersOfComponentAccess")
                                                                           .WithSqlParam("@componentName", "Show Consults,Show All Consults,Show Graphs")
                                                                           .WithSqlParam("@orgId", orgByServiceId.Value)
                                                                           .ExecuteStoredProc<RegisterCredentialVM>(); //.Select(x => new { x.UserUniqueId, x.UserId }).Distinct().ToList();
                            users.AddRange(showAllAccessUsers);
                            distinctUsers = users.Select(x => new { x.UserUniqueId, x.UserId }).Distinct().ToList();
                            var notification = new PushNotificationVM()
                            {
                                Id = keyValues["CallbackNumber"].ToString().ToInt(),
                                OrgId = orgByServiceId.Value,
                                UserChannelSid = users.Select(x => x.UserUniqueId).Distinct().ToList(),
                                From = "Consult",
                                Msg = "New Consult is Created",
                                RouteLink1 = "/Home/Dashboard",
                                RouteLink2 = "/Home/Consult"
                            };

                            _communicationService.pushNotification(notification);
                        }
                    }
                }
                else if (users != null && users.Count > 0)
                {
                    //string uniqueName = $"CONSULT_{Consult_Counter.ToString()}";
                    string ServiceName = this._serviceLineRepo.Table.Where(x => x.ServiceLineId == serviceLineId && !x.IsDeleted).Select(x => x.ServiceName).FirstOrDefault();
                    string uniqueName = DateTime.Now.ToString("yyyyMMddHHmmssffff") + ApplicationSettings.UserId.ToString();
                    string friendlyName = $"{consultType} {ServiceName} Consult {Consult_Counter}";
                    var channel = _communicationService.createConversationChannel(friendlyName, uniqueName, conversationChannelAttributes);

                    ////////////// Update Consult For ChannelSid ////////////////////////////

                    string ConsultId = keyValues["ConsultId"].ToString();
                    string qry = $"UPDATE [dbo].[Consults] SET [ChannelSid] = '{channel.Sid}'";

                    qry += $" WHERE ConsultNumber = '{Consult_Counter}'";

                    int rowUpdate = this._dbContext.Database.ExecuteSqlRaw(qry);

                    /////////////////////////////////////////////////////////////////////////

                    List<ConsultAcknowledgment> consultAcknowledgmentList = new();
                    var distinctUsers = users.Select(x => new { x.UserUniqueId, x.UserId }).Distinct().ToList();
                    foreach (var item in distinctUsers)
                    {
                        try
                        {
                            this._communicationService.addNewUserToConversationChannel(channel.Sid, item.UserUniqueId);
                            var acknowledgeConsult = new ConsultAcknowledgment
                            {
                                IsAcknowledge = false,
                                ConsultIdFk = Consult_Counter,
                                UserIdFk = item.UserId,
                                CreatedBy = ApplicationSettings.UserId,
                                CreatedDate = DateTime.UtcNow
                            };
                            consultAcknowledgmentList.Add(acknowledgeConsult);
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    this._consultAcknowledgmentRepo.Insert(consultAcknowledgmentList);
                    var msg = new ConversationMessageVM();
                    msg.author = "System";
                    msg.attributes = "";
                    msg.body = $"<strong>{consultType} {ServiceName} Consult </strong> </br></br>";
                    if (keyValues.ContainsKey("PatientFirstName") && keyValues.ContainsKey("PatientLastName"))
                    {
                        msg.body += $"<strong>Patient Name:</strong> {keyValues["PatientFirstName"].ToString()} {keyValues["PatientLastName"].ToString()} </br>";
                    }
                    else
                    {
                        if (keyValues.ContainsKey("PatientFirstName"))
                        {
                            msg.body += $"<strong>Patient Name:</strong> {keyValues["PatientFirstName"].ToString()} </br>";
                        }
                        if (keyValues.ContainsKey("PatientLastName"))
                        {
                            msg.body += $"<strong>Patient Name:</strong> {keyValues["PatientLastName"].ToString()} </br>";
                        }
                    }
                    if (keyValues.ContainsKey("DateOfBirth"))
                    {
                        DateTime dob = DateTime.Parse(keyValues["DateOfBirth"].ToString());
                        msg.body += $"<strong>Dob:</strong> {dob:MM-dd-yyyy} </br>";
                    }
                    msg.body += keyValues.ContainsKey("MedicalRecordNumber") ? $"<strong>Medical Record Number:</strong> {keyValues["MedicalRecordNumber"].ToString()} </br>" : "";
                    msg.body += keyValues.ContainsKey("CallbackNumber") && keyValues["CallbackNumber"].ToString() != "(___) ___-____" ? $"<strong>Callback Number:</strong> {keyValues["CallbackNumber"].ToString()} </br>" : "";
                    msg.channelSid = channel.Sid;

                    var sendMsg = _communicationService.sendPushNotification(msg);

                    var orgByServiceId = _dptRepo.Table.Where(x => !x.IsDeleted && x.DepartmentId == _serviceLineRepo.Table.Where(x => !x.IsDeleted && x.ServiceLineId == serviceLineId).Select(x => x.DepartmentIdFk).FirstOrDefault()).Select(x => x.OrganizationIdFk).FirstOrDefault();
                    var showAllAccessUsers = this._dbContext.LoadStoredProcedure("md_getUsersOfComponentAccess")
                                       .WithSqlParam("@componentName", "Show Consults,Show All Consults,Show Graphs")
                                       .WithSqlParam("@orgId", orgByServiceId.Value)
                                       .ExecuteStoredProc<RegisterCredentialVM>(); //.Select(x => new { x.UserUniqueId, x.UserId }).Distinct().ToList();
                    users.AddRange(showAllAccessUsers);
                    distinctUsers = users.Select(x => new { x.UserUniqueId, x.UserId }).Distinct().ToList();
                    var notification = new PushNotificationVM()
                    {
                        Id = keyValues["CallbackNumber"].ToString().ToInt(),
                        OrgId = orgByServiceId.Value,
                        UserChannelSid = distinctUsers.Select(x => x.UserUniqueId).Distinct().ToList(),
                        From = "Consult",
                        Msg = "New Consult is Created",
                        RouteLink1 = "/Home/Dashboard",
                        RouteLink2 = "/Home/Consult"
                    };

                    _communicationService.pushNotification(notification);
                }
            }

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Group Created Successfully", Body = new { serviceLineUsersFound = usersFound } };

        }
        public BaseResponse ActiveOrInActiveConsult(int consultId, bool status)
        {
            var sql = "EXEC md_ActiveOrInActiveConsult @status, @userId, @consultId";
            var parameters = new List<SqlParameter>() { new SqlParameter { ParameterName = "@status", Value = status },
                                                        new SqlParameter { ParameterName = "@userId", Value = ApplicationSettings.UserId },
                                                        new SqlParameter { ParameterName = "@consultId", Value = consultId }
                                                      };

            var rowsEffected = this._dbContext.Database.ExecuteSqlRaw(sql, parameters);
            if (rowsEffected > 0)
            {
                this._dbContext.Log(new { }, ActivityLogTableEnums.Consults.ToString(), consultId, status == false ? ActivityLogActionEnums.Inactive.ToInt() : ActivityLogActionEnums.Active.ToInt());

                var userIds = this._dbContext.LoadStoredProcedure("md_getConsultGroupUsersId")
                                             .WithSqlParam("@consultId", consultId)
                                             .ExecuteStoredProc<ConsultAcknowledgmentVM>();

                var notification = new PushNotificationVM()
                {
                    Id = consultId,
                    //OrgId = orgByServiceId.Value,
                    Type = "ChannelStatusChanged",
                    ChannelIsActive = status,
                    ChannelSid = userIds.Select(x => x.channelSid).FirstOrDefault(),
                    UserChannelSid = userIds.Select(x => x.userUniqueId).Distinct().ToList(),
                    From = "Consult",
                    Msg = "Consult is " + (status ? "Activated" : "Inactivated"),
                    RouteLink1 = "/Home/Dashboard",
                    RouteLink2 = "/Home/Consult"
                };

                _communicationService.pushNotification(notification);

                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Consult Deleted" };
            }
            else
            {
                return new BaseResponse() { Status = HttpStatusCode.NotModified, Message = "No Recored deleted" };
            }
        }
        public BaseResponse DeleteConsult(int consultId, bool status)
        {
            var sql = "EXEC md_DeleteConsultAndGroup @status, @userId, @consultId";
            var parameters = new List<SqlParameter>() { new SqlParameter { ParameterName = "@status", Value = status },
                                                        new SqlParameter { ParameterName = "@userId", Value = ApplicationSettings.UserId },
                                                        new SqlParameter { ParameterName = "@consultId", Value = consultId }
                                                      };

            var isDeleted = this._dbContext.Database.ExecuteSqlRaw(sql, parameters);
            if (isDeleted > 0)
            {
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Consult Deleted" };
            }
            else
            {
                return new BaseResponse() { Status = HttpStatusCode.NotModified, Message = "No Recored deleted" };
            }
        }

        #endregion


        #region Consult Acknowledgments

        public BaseResponse GetAllConsultAcknowledgments()
        {
            var consultAcknowledgments = this._consultAcknowledgmentRepo.Table.Where(x => !x.IsDeleted).ToList();
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Returned", Body = consultAcknowledgments };
        }

        public BaseResponse GetConsultAcknowledgmentByConsultId(int consultId)
        {

            var consultAcknowledgment = _dbContext.LoadStoredProcedure("md_getConsultAcknowledgmentByConsultId")
             .WithSqlParam("@ConsultId", consultId)
             .ExecuteStoredProc<ConsultAcknowledgmentVM>();

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Returned", Body = consultAcknowledgment };
        }

        public BaseResponse GetConsultAcknowledgmentByUserId(int userId)
        {
            var consultAknowledge = this._consultAcknowledgmentRepo.Table.Where(x => x.UserIdFk == userId && !x.IsAcknowledge && !x.IsDeleted).ToList();
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Returned", Body = consultAknowledge };


        }

        public BaseResponse AcknowledgeConsult(int consultId)
        {
            var consultAknow = this._consultAcknowledgmentRepo.Table.Where(x => x.ConsultIdFk == consultId && x.UserIdFk == ApplicationSettings.UserId && !x.IsDeleted).FirstOrDefault();
            consultAknow.IsAcknowledge = true;
            consultAknow.ModifiedBy = ApplicationSettings.UserId;
            consultAknow.ModifiedDate = DateTime.UtcNow;

            this._consultAcknowledgmentRepo.Update(consultAknow);
            this._dbContext.Log(new { }, ActivityLogTableEnums.Consults.ToString(), consultId, ActivityLogActionEnums.Acknowledge.ToInt());
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Acknowledged Consult" };
        }

        #endregion

    }
}
