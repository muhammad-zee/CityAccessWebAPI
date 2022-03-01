﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
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
        private IRepository<ControlListDetail> _controlListDetailsRepo;
        private IRepository<ConsultField> _consultFieldRepo;
        private IRepository<OrganizationConsultField> _orgConsultRepo;
        private IRepository<ConsultAcknowledgment> _consultAcknowledgmentRepo;
        IConfiguration _config;

        public ConsultService(RAQ_DbContext dbContext,
            ICommunicationService communicationService,
            IConfiguration config,
            IRepository<User> userRepo,
            IRepository<Organization> orgRepo,
            IRepository<ControlListDetail> controlListDetailsRepo,
            IRepository<ConsultField> consultFieldRepo,
            IRepository<OrganizationConsultField> orgConsultRepo,
            IRepository<ConsultAcknowledgment> consultAcknowledgmentRepo)
        {
            this._dbContext = dbContext;
            this._config = config;
            this._communicationService = communicationService;
            this._usersRepo = userRepo;
            this._orgRepo = orgRepo;
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
            var selectedConsultFields = this._orgConsultRepo.Table.Where(x => x.OrganizationIdFk == OrgId && !x.IsDeleted).Select(x => new { x.ConsultFieldIdFk, x.IsRequired, x.SortOrder }).ToList();

            var consultFieldVM = AutoMapperHelper.MapList<ConsultField, ConsultFieldsVM>(consultFields);

            foreach (var item in consultFieldVM)
            {
                if (selectedConsultFields.Select(x => x.ConsultFieldIdFk).Contains(item.ConsultFieldId))
                {
                    item.IsRequired = selectedConsultFields.Where(x => x.ConsultFieldIdFk == item.ConsultFieldId).Select(s => s.IsRequired).FirstOrDefault();
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

        public BaseResponse GetConsultsByServiceLineId(ConsultFieldsVM consult)
        {
            var consultData = _dbContext.LoadStoredProcedure("md_getGetConsultsByServiceLineId")
                .WithSqlParam("@organizationId", consult.OrganizationId)
                .WithSqlParam("@departmentIds", consult.DepartmentIds)
                .WithSqlParam("@serviceLineIds", consult.ServiceLineIds)
                .WithSqlParam("@userId", ApplicationSettings.UserId)
                .WithSqlParam("@showAllConsults", consult.showAllConsults)
                .ExecuteStoredProc_ToDictionary();

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Returned", Body = consultData };
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
                var Consult_Counter = _dbContext.LoadStoredProcedure("md_getMDRouteCounter").WithSqlParam("@C_Initails", "CC").ExecuteStoredProc<MDRoute_CounterVM>().FirstOrDefault();

                string query = "INSERT INTO [dbo].[Consults] (";

                for (int i = 0; i < keys.Count(); i++)
                {
                    if (keys[i] != "ConsultId" && keys[i] != "CreatedBy" && keys[i] != "CreatedDate")
                    {
                        query += $"[{keys[i]}]";
                        if ((i + 1) == keys.Count)
                        {
                            //if (keyValues.ContainsKey("DateOfBirth"))
                            //    query += ",[DateOfBirth]";

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
                    if (keys[i] != "ConsultId" && keys[i] != "CreatedBy" && keys[i] != "CreatedDate")
                    {
                        query += $"'{values[i]}'";
                        if ((i + 1) == values.Count)
                        {
                            //if (keyValues.ContainsKey("DateOfBirth")) 
                            //{
                            //    var dob = DateTime.Parse(keyValues["DateOfBirth"].ToString()).ToString("MM-dd-yyyy hh:mm:ss");
                            //    query += $",'{dob}'";
                            //}
                            query += $",'{Consult_Counter.Counter_Value}'";
                            query += $",'{ApplicationSettings.UserId}'";
                            query += $",'{DateTime.UtcNow.ToString("MM-dd-yyyy hh:mm:ss")}'";
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

                    if (keys.Contains("ServiceLineIdFk") && keyValues["ServiceLineIdFk"].ToString() != "0")
                    {
                        var serviceLineId = keyValues["ServiceLineIdFk"].ToString().ToInt();

                        var users = _dbContext.LoadStoredProcedure("md_getAvailableUserOnSchedule")
                                    .WithSqlParam("@servicelineIdFk", serviceLineId)
                                    .WithSqlParam("@dayOfWeek", DateTime.UtcNow.DayOfWeek.ToString())
                                    .ExecuteStoredProc<RegisterCredentialVM>();

                        var consultType = _controlListDetailsRepo.Table.Where(x => x.ControlListDetailId == keyValues["ConsultType"].ToString().ToInt()).Select(x => x.Title).FirstOrDefault();

                        var conversationChannelAttributes = JsonConvert.SerializeObject(new Dictionary<string, Object>()
                                    {
                                        {ChannelAttributeEnums.ChannelType.ToString(), ChannelTypeEnums.Consult.ToString()},
                                        {ChannelAttributeEnums.ConsultId.ToString(), Consult_Counter.Counter_Value}
                                    }, Formatting.Indented);

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
                                    //string uniqueName = $"CONSULT_{Consult_Counter.Counter_Value.ToString()}";
                                    string uniqueName = DateTime.Now.ToString("yyyyMMddHHmmssffff") + ApplicationSettings.UserId.ToString();
                                    string friendlyName = $"Consult_{Consult_Counter.Counter_Value}_{consultType}";
                                    var channel = _communicationService.createConversationChannel(friendlyName, uniqueName, conversationChannelAttributes);
                                    List<ConsultAcknowledgment> consultAcknowledgmentList = new();
                                    foreach (var item in users.Distinct())
                                    {
                                        this._communicationService.addNewUserToConversationChannel(channel.Sid, item.UserUniqueId);
                                        var acknowledgeConsult = new ConsultAcknowledgment
                                        {
                                            IsAcknowledge = false,
                                            ConsultIdFk = Consult_Counter.Counter_Value,
                                            UserIdFk = item.UserId,
                                            CreatedBy = ApplicationSettings.UserId,
                                            CreatedDate = DateTime.UtcNow
                                        };
                                        consultAcknowledgmentList.Add(acknowledgeConsult);
                                    }
                                    this._consultAcknowledgmentRepo.Insert(consultAcknowledgmentList);
                                    var msg = new ConversationMessageVM()
                                    {
                                        author = "System",
                                        attributes = "",
                                        body = "This Group created by system for consult purpose",
                                        channelSid = channel.Sid
                                    };
                                    _communicationService.sendPushNotification(msg);
                                }
                            }
                        }
                        else if (users != null && users.Count > 0)
                        {
                            //string uniqueName = $"CONSULT_{Consult_Counter.Counter_Value.ToString()}";
                            string uniqueName = DateTime.Now.ToString("yyyyMMddHHmmssffff") + ApplicationSettings.UserId.ToString();
                            string friendlyName = $"Consult_{Consult_Counter.Counter_Value}_{consultType}";
                            var channel = _communicationService.createConversationChannel(friendlyName, uniqueName, conversationChannelAttributes);
                            List<ConsultAcknowledgment> consultAcknowledgmentList = new();
                            foreach (var item in users.Distinct())
                            {
                                _communicationService.addNewUserToConversationChannel(channel.Sid, item.UserUniqueId);
                                var acknowledgeConsult = new ConsultAcknowledgment
                                {
                                    IsAcknowledge = false,
                                    ConsultIdFk = Consult_Counter.Counter_Value,
                                    UserIdFk = item.UserId,
                                    CreatedBy = ApplicationSettings.UserId,
                                    CreatedDate = DateTime.UtcNow
                                };
                                consultAcknowledgmentList.Add(acknowledgeConsult);
                            }
                            this._consultAcknowledgmentRepo.Insert(consultAcknowledgmentList);
                            var msg = new ConversationMessageVM()
                            {
                                author = "System",
                                attributes = "",
                                body = "This Group created by system for consult purpose",
                                channelSid = channel.Sid
                            };
                            var sendMsg = _communicationService.sendPushNotification(msg);
                        }
                    }

                    return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Added Successfully" };
                }
                else
                {
                    return new BaseResponse() { Status = HttpStatusCode.OK, Message = "No Record Added" };
                }
            }
            if (isConsultIdExist && keyValues["ConsultId"].ToString() != "0")
            {
                string ConsultId = keyValues["ConsultId"].ToString();
                string query = "UPDATE [dbo].[Consults] SET ";

                for (int i = 0; i < keys.Count; i++)
                {
                    if (keys[i] != "ConsultId" && keys[i] != "ModifiedBy" && keys[i] != "ModifiedDate")
                    {
                        query += $"[{keys[i]}] = '{values[i]}'";
                        if (i < keys.Count)
                        {
                            query += ",";
                        }
                    }
                }
                query += $" [ModifiedBy] = '{ApplicationSettings.UserId}', [ModifiedDate] = '{DateTime.UtcNow.ToString("MM-dd-yyyy hh:mm:ss")}'";
                query += $" WHERE ConsultId = '{ConsultId.ToInt()}'";

                int rowsEffect = this._dbContext.Database.ExecuteSqlRaw(query);
                if (rowsEffect > 0)
                {
                    return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Updated Successfully" };
                }
                else
                {
                    return new BaseResponse() { Status = HttpStatusCode.OK, Message = "No Record Updated" };
                }
            }


            return new BaseResponse() { Status = HttpStatusCode.NotModified, Message = "Consult Id Column is not exist" };
        }

        public BaseResponse DeleteConsult(int consultId)
        {
            var isDeleted = this._dbContext.Database.ExecuteSqlRaw($"Update Consults SET IsDeleted = 1, ModifiedBy = '{ApplicationSettings.UserId}', ModifiedDate = '{DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss")}' WHERE ConsultId = '{consultId}'");
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

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Acknowledged Consult" };
        }

        #endregion

    }
}
