using ElmahCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
    public class ActiveCodeService : IActiveCodeService
    {
        private RAQ_DbContext _dbContext;
        private IHttpClient _httpClient;
        private IHostingEnvironment _environment;
        private ICommunicationService _communication;
        private IRepository<User> _userRepo;
        private IRepository<Organization> _orgRepo;
        private IRepository<ServiceLine> _serviceLineRepo;
        private IRepository<UsersSchedule> _userSchedulesRepo;
        private IRepository<ControlListDetail> _controlListDetailsRepo;
        private IRepository<ActiveCode> _activeCodeRepo;
        private IRepository<CodeStroke> _codeStrokeRepo;
        private IRepository<CodeSepsi> _codeSepsisRepo;
        private IRepository<CodeStemi> _codeSTEMIRepo;
        private IRepository<CodeTrauma> _codeTrumaRepo;
        private IRepository<CodeBlue> _codeBlueRepo;
        private IRepository<ActiveCodesGroupMember> _activeCodesGroupMembersRepo;
        private IRepository<CodesServiceLinesMapping> _codesServiceLinesMappingRepo;
        IConfiguration _config;
        private string _RootPath;
        private string _GoogleApiKey;
        public ActiveCodeService(RAQ_DbContext dbContext,
            IConfiguration config,
            IHttpClient httpClient,
            IHostingEnvironment environment,
            ICommunicationService communication,
            IRepository<User> userRepo,
            IRepository<Organization> orgRepo,
            IRepository<ServiceLine> serviceLineRepo,
            IRepository<UsersSchedule> userSchedulesRepo,
            IRepository<ControlListDetail> controlListDetailsRepo,
            IRepository<ActiveCode> activeCodeRepo,
            IRepository<CodeStroke> codeStrokeRepo,
            IRepository<CodeSepsi> codeSepsisRepo,
            IRepository<CodeStemi> codeSTEMIRepo,
            IRepository<CodeTrauma> codeTrumaRepo,
            IRepository<CodeBlue> codeBlueRepo,
            IRepository<ActiveCodesGroupMember> activeCodesGroupMembersRepo,
            IRepository<CodesServiceLinesMapping> codesServiceLinesMappingRepo)
        {
            this._config = config;
            this._httpClient = httpClient;
            this._dbContext = dbContext;
            this._environment = environment;
            this._communication = communication;
            this._userRepo = userRepo;
            this._orgRepo = orgRepo;
            this._serviceLineRepo = serviceLineRepo;
            this._userSchedulesRepo = userSchedulesRepo;
            this._controlListDetailsRepo = controlListDetailsRepo;
            this._activeCodeRepo = activeCodeRepo;
            this._codeStrokeRepo = codeStrokeRepo;
            this._codeSepsisRepo = codeSepsisRepo;
            this._codeSTEMIRepo = codeSTEMIRepo;
            this._codeTrumaRepo = codeTrumaRepo;
            this._codeBlueRepo = codeBlueRepo;
            this._activeCodesGroupMembersRepo = activeCodesGroupMembersRepo;
            this._codesServiceLinesMappingRepo = codesServiceLinesMappingRepo;
            this._RootPath = this._config["FilePath:Path"].ToString();
            this._GoogleApiKey = this._config["GoogleApi:Key"].ToString();
        }

        #region Active Code

        public BaseResponse GetActivatedCodesByOrgId(int orgId)
        {
            var codes = this._dbContext.LoadStoredProcedure("md_getActivatedCodesForOrg")
                .WithSqlParam("@pOrgId", orgId)
                .ExecuteStoredProc<ActiveCodeVM>();
            foreach (var item in codes)
            {
                item.serviceLines = this._serviceLineRepo.Table.Where(x => !x.IsDeleted && item.ServiceLineIds.ToIntList().Contains(x.ServiceLineId)).Select(x => new ServiceLineVM() { ServiceLineId = x.ServiceLineId, ServiceName = x.ServiceName }).ToList();
            }
            if (codes.Count > 0)
            {
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Returned", Body = codes };
            }
            else
            {
                return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "No Active Code Found", Body = codes };
            }
        }

        public BaseResponse MapActiveCodes(List<ActiveCodeVM> activeCodes)
        {
            List<ActiveCode> update = new();
            List<ActiveCode> insert = new();
            foreach (var item in activeCodes)
            {
                if (item.ActiveCodeId > 0)
                {
                    var row = this._activeCodeRepo.Table.Where(x => x.ActiveCodeId == item.ActiveCodeId && !x.IsDeleted).FirstOrDefault();
                    row.DefaultServiceLineId = item.DefaultServiceLineId;
                    row.ServiceLineIds = item.ServiceLineIds;
                    row.ModifiedBy = item.ModifiedBy;
                    row.ModifiedDate = DateTime.Now;
                    row.IsDeleted = false;
                    update.Add(row);
                }
                else
                {
                    var row = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == item.OrganizationIdFk && x.CodeIdFk == item.CodeIdFk && !x.IsDeleted).FirstOrDefault();
                    if (row != null)
                    {
                        var oldServices = new List<string>();
                        if (!string.IsNullOrEmpty(row.ServiceLineIds) && !string.IsNullOrWhiteSpace(row.ServiceLineIds))
                        {
                            oldServices = row.ServiceLineIds.Split(",").ToList();
                        }
                        var newServices = new List<string>();
                        if (!string.IsNullOrEmpty(row.ServiceLineIds) && !string.IsNullOrWhiteSpace(row.ServiceLineIds))
                        {
                            newServices = item.ServiceLineIds.Split(",").ToList();
                        }
                        oldServices.AddRange(newServices);
                        row.DefaultServiceLineId = item.DefaultServiceLineId;
                        row.ServiceLineIds = string.Join(",", oldServices);
                        row.ModifiedBy = item.CreatedBy;
                        row.ModifiedDate = DateTime.Now;
                        update.Add(row);
                    }
                    else
                    {
                        item.CreatedDate = DateTime.UtcNow;
                        var newRow = AutoMapperHelper.MapSingleRow<ActiveCodeVM, ActiveCode>(item);
                        insert.Add(newRow);
                    }
                }

            }
            if (update.Count > 0)
            {
                this._activeCodeRepo.Update(update);
            }
            if (insert.Count > 0)
            {
                this._activeCodeRepo.Insert(insert);
            }

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Saved" };
        }

        public BaseResponse DetachActiveCodes(int activeCodeId)
        {
            var row = this._activeCodeRepo.Table.Where(x => x.ActiveCodeId == activeCodeId && !x.IsDeleted).FirstOrDefault();
            row.ModifiedBy = ApplicationSettings.UserId;
            row.ModifiedDate = DateTime.UtcNow;
            row.IsDeleted = true;
            this._activeCodeRepo.Update(row);
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Deleted" };
        }

        public BaseResponse GetAllActiveCodes(int orgId)
        {
            var activeCodesData = this._dbContext.LoadStoredProcedure("md_getActiveCodesForDashboard")
                                    .WithSqlParam("@organizationId", orgId)
                                    .ExecuteStoredProc<CodeStrokeVM>();
            var orgDataList = new List<dynamic>();
            //foreach (var item in activeCodesData.Select(x => x.OrganizationIdFk).Distinct().ToList())
            //{
            //    var orgData = GetHosplitalAddressObject(item);
            //    if (orgData != null)
            //        orgDataList.Add(orgData);
            //}
            //foreach (var item in activeCodesData)
            //{
            //    item.OrganizationData = orgDataList.Where(x => x.OrganizationId == item.OrganizationIdFk).FirstOrDefault();

            //    item.AttachmentsPath = new List<string>();
            //    item.AudiosPath = new List<string>();
            //    item.VideosPath = new List<string>();
            //    item.BloodThinnersTitle = new List<object>();
            //    item.ServiceLines = new List<ServiceLineVM>();
            //    if (!string.IsNullOrEmpty(item.Attachments) && !string.IsNullOrWhiteSpace(item.Attachments))
            //    {
            //        string path = this._RootPath + item.Attachments;
            //        if (Directory.Exists(path))
            //        {
            //            DirectoryInfo AttachFiles = new DirectoryInfo(path);
            //            foreach (var file in AttachFiles.GetFiles())
            //            {
            //                item.AttachmentsPath.Add(item.Attachments + "/" + file.Name);
            //            }
            //        }
            //    }

            //    if (!string.IsNullOrEmpty(item.Audio) && !string.IsNullOrWhiteSpace(item.Audio))
            //    {
            //        string path = this._RootPath + item.Audio;
            //        if (Directory.Exists(path))
            //        {
            //            DirectoryInfo AudioFiles = new DirectoryInfo(path);
            //            foreach (var file in AudioFiles.GetFiles())
            //            {
            //                item.AudiosPath.Add(item.Audio + "/" + file.Name);
            //            }
            //        }
            //    }

            //    if (!string.IsNullOrEmpty(item.Video) && !string.IsNullOrWhiteSpace(item.Video))
            //    {
            //        var path = this._RootPath + item.Video;
            //        if (Directory.Exists(path))
            //        {
            //            DirectoryInfo VideoFiles = new DirectoryInfo(path);
            //            foreach (var file in VideoFiles.GetFiles())
            //            {
            //                item.VideosPath.Add(item.Video + "/" + file.Name);
            //            }
            //        }
            //    }

            //    var serviceIds = this._activeCodeRepo.Table.Where(s => s.OrganizationIdFk == item.OrganizationIdFk && s.CodeIdFk == item.CodeName.GetActiveCodeId() && !s.IsDeleted).Select(s => new { s.ServiceLineIds, s.DefaultServiceLineId }).FirstOrDefault();
            //    var serviceLineIds = (from s in this._codesServiceLinesMappingRepo.Table
            //                          where s.OrganizationIdFk == item.OrganizationIdFk && s.CodeIdFk == item.CodeName.GetActiveCodeId()
            //                          && s.ActiveCodeId == item.Id && s.ActiveCodeName == item.CodeName && s.ServiceLineIdFk != serviceIds.DefaultServiceLineId
            //                          select s.ServiceLineIdFk).ToList();
            //    item.ServiceLines = this._serviceLineRepo.Table.Where(s => serviceIds.ServiceLineIds.ToIntList().Distinct().Contains(s.ServiceLineId) && s.ServiceLineId != serviceIds.DefaultServiceLineId && !s.IsDeleted).Select(s => new ServiceLineVM() { ServiceLineId = s.ServiceLineId, ServiceName = s.ServiceName, IsSelected = serviceLineIds.Contains(s.ServiceLineId) }).ToList();
            //    item.DefaultServiceLineId = serviceIds.DefaultServiceLineId;
            //    item.DefaultServiceLine = this._serviceLineRepo.Table.Where(s => s.ServiceLineId == serviceIds.DefaultServiceLineId && !s.IsDeleted).Select(s => new ServiceLineVM() { ServiceLineId = s.ServiceLineId, ServiceName = s.ServiceName }).FirstOrDefault();
            //    item.SelectedServiceLineIds = string.Join(",", serviceLineIds);

            //    item.LastKnownWellStr = item.LastKnownWell?.ToString("yyyy-MM-dd hh:mm:ss tt");
            //    //new
            //    item.CreatedDateStr = item.CreatedDate.ToString("yyyy-MM-dd hh:mm:ss tt");
            //    item.DobStr = item.Dob?.ToString("yyyy-MM-dd hh:mm:ss tt");
            //    //item.OrganizationData = orgData;
            //    item.GenderTitle = _controlListDetailsRepo.Table.Where(g => g.ControlListDetailId == item.Gender).Select(g => g.Title).FirstOrDefault();
            //    item.BloodThinnersTitle.AddRange(_controlListDetailsRepo.Table.Where(b => item.BloodThinners.ToIntList().Contains(b.ControlListDetailId)).Select(b => new { Id = b.ControlListDetailId, b.Title }).ToList());

            //}
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Returned", Body = activeCodesData };
        }
        
        public BaseResponse GetEMSandActiveCodesForDashboard(int OrgId, int days = 6)
        {

            var today = DateTime.Today;
            var lastWeek = today.AddDays(-days);
            //     var thisWeekStart = DateTime.Today.AddDays(-1 * (int)(DateTime.Today.DayOfWeek)).AddDays(1);
            //   var thisWeekEnd = thisWeekStart.AddDays(7).AddSeconds(-1);
            var ActiveCodes = this._dbContext.LoadStoredProcedure("md_getEMSandActiveCodesGraphDataForDashboard")
                    .WithSqlParam("@OrganizationId", OrgId)
                    .WithSqlParam("@StartDate", lastWeek.ToString("yyyy-MM-dd"))
                    .WithSqlParam("@EndDate", today.ToString("yyyy-MM-dd"))
                    .ExecuteStoredProc<GraphVM>();

            List<string> Label = new();
            while (today.Date >= lastWeek.Date)
            {
                Label.Add(lastWeek.ToString("MMM-dd"));
                lastWeek = lastWeek.AddDays(1);
            }
            // thisWeekStart = DateTime.Today.AddDays(-1 * (int)(DateTime.Today.DayOfWeek)).AddDays(1);
            lastWeek = today.AddDays(-days);
            var datasets = new List<object>();
            if (ActiveCodes.Count < days)
            {
                List<int> EMS = new();
                List<int> activeCodes = new();
                while (today.Date >= lastWeek.Date)
                {
                    if (ActiveCodes.Any(x => x.CreatedDate.Date == lastWeek.Date))
                    {
                        EMS.Add(ActiveCodes.Where(x => x.CreatedDate.Date == lastWeek.Date).Select(x => x.EMS).FirstOrDefault());
                        activeCodes.Add(ActiveCodes.Where(x => x.CreatedDate.Date == lastWeek.Date).Select(x => x.ActiveCodes).FirstOrDefault());
                    }
                    else
                    {
                        EMS.Add(0);
                        activeCodes.Add(0);
                    }
                    lastWeek = lastWeek.AddDays(1);
                }
                datasets.Add(new
                {
                    label = "EMS",
                    backgroundColor = "#089bab",
                    borderColor = "#089bab",
                    data = EMS
                });
                datasets.Add(new
                {
                    label = "Active Codes",
                    backgroundColor = "#CEEBEE",
                    borderColor = "#CEEBEE",
                    data = activeCodes
                });

            }
            else
            {
                datasets = new List<object>() { new
                                                {
                                                  label= "EMS",
                                                  backgroundColor= "#089bab",
                                                  data= ActiveCodes.Select(c=>c.EMS).ToList()
                                                },
                                                new
                                                {
                                                  label= "Active Codes",
                                                  backgroundColor= "#CEEBEE",
                                                  data= ActiveCodes.Select(c=>c.ActiveCodes).ToList()
                                                }
                                            };
            }

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Found", Body = new { labels = Label, datasets } };

        }

        #endregion

        #region Delete File

        public BaseResponse DeleteFile(FilesVM files)
        {
            if (files.CodeType == AuthorEnums.Stroke.ToString())
            {
                var rootPath = this._codeStrokeRepo.Table.Where(x => x.CodeStrokeId == files.Id).Select(files.Type, "IsEMS").FirstOrDefault();
                string path = _environment.WebRootFileProvider.GetFileInfo(rootPath + '/' + files.FileName)?.PhysicalPath;
                File.Delete(path);
                var serviceLineIds = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == files.OrgId && x.CodeIdFk == UCLEnums.Stroke.ToInt() && !x.IsDeleted).Select(x => x.ServiceLineIds).FirstOrDefault();
                if (serviceLineIds != null && serviceLineIds != "")
                {
                    var UserChannelSid = (from us in this._userSchedulesRepo.Table
                                          join u in this._userRepo.Table on us.UserIdFk equals u.UserId
                                          where serviceLineIds.ToIntList().Contains(us.ServiceLineIdFk.Value) && us.ScheduleDateStart <= DateTime.Now && us.ScheduleDateEnd >= DateTime.Now && !us.IsDeleted && !u.IsDeleted
                                          select u.UserChannelSid).ToList();

                    var notification = new PushNotificationVM()
                    {
                        Id = files.Id,
                        OrgId = files.OrgId,
                        UserChannelSid = UserChannelSid,
                        From = AuthorEnums.Stroke.ToString(),
                        Msg = "Stroke From is Changed",
                        RouteLink = "/Home/Activate%20Code/code-strok-form"
                    };

                    _communication.pushNotification(notification);

                }
            }
            else if (files.CodeType == AuthorEnums.Sepsis.ToString())
            {
                var rootPath = this._codeSepsisRepo.Table.Where(x => x.CodeSepsisId == files.Id).Select(files.Type).FirstOrDefault();
                string path = _environment.WebRootFileProvider.GetFileInfo(rootPath + '/' + files.FileName)?.PhysicalPath;
                File.Delete(path);
                var serviceLineIds = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == files.OrgId && x.CodeIdFk == UCLEnums.Sepsis.ToInt() && !x.IsDeleted).Select(x => x.ServiceLineIds).FirstOrDefault();
                if (serviceLineIds != null && serviceLineIds != "")
                {
                    var UserChannelSid = (from us in this._userSchedulesRepo.Table
                                          join u in this._userRepo.Table on us.UserIdFk equals u.UserId
                                          where serviceLineIds.ToIntList().Contains(us.ServiceLineIdFk.Value) && us.ScheduleDateStart <= DateTime.Now && us.ScheduleDateEnd >= DateTime.Now && !us.IsDeleted && !u.IsDeleted
                                          select u.UserChannelSid).ToList();

                    var notification = new PushNotificationVM()
                    {
                        Id = files.Id,
                        OrgId = files.OrgId,
                        UserChannelSid = UserChannelSid,
                        From = AuthorEnums.Sepsis.ToString(),
                        Msg = "Sepsis From is Changed",
                        RouteLink = "/Home/Activate%20Code/code-sepsis-form"
                    };

                    _communication.pushNotification(notification);

                }
            }
            else if (files.CodeType == AuthorEnums.STEMI.ToString())
            {
                var rootPath = this._codeSTEMIRepo.Table.Where(x => x.CodeStemiid == files.Id).Select(files.Type).FirstOrDefault();
                string path = _environment.WebRootFileProvider.GetFileInfo(rootPath + '/' + files.FileName)?.PhysicalPath;
                File.Delete(path);
                var serviceLineIds = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == files.OrgId && x.CodeIdFk == UCLEnums.STEMI.ToInt() && !x.IsDeleted).Select(x => x.ServiceLineIds).FirstOrDefault();
                if (serviceLineIds != null && serviceLineIds != "")
                {
                    var UserChannelSid = (from us in this._userSchedulesRepo.Table
                                          join u in this._userRepo.Table on us.UserIdFk equals u.UserId
                                          where serviceLineIds.ToIntList().Contains(us.ServiceLineIdFk.Value) && us.ScheduleDateStart <= DateTime.Now && us.ScheduleDateEnd >= DateTime.Now && !us.IsDeleted && !u.IsDeleted
                                          select u.UserChannelSid).ToList();

                    var notification = new PushNotificationVM()
                    {
                        Id = files.Id,
                        OrgId = files.OrgId,
                        UserChannelSid = UserChannelSid,
                        From = AuthorEnums.STEMI.ToString(),
                        Msg = "STEMI From is Changed",
                        RouteLink = "/Home/Activate%20Code/code-STEMI-form"
                    };

                    _communication.pushNotification(notification);

                }
            }
            else if (files.CodeType == AuthorEnums.Trauma.ToString())
            {
                var rootPath = this._codeTrumaRepo.Table.Where(x => x.CodeTraumaId == files.Id).Select(files.Type).FirstOrDefault();
                string path = _environment.WebRootFileProvider.GetFileInfo(rootPath + '/' + files.FileName)?.PhysicalPath;
                File.Delete(path);
                var serviceLineIds = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == files.OrgId && x.CodeIdFk == UCLEnums.Trauma.ToInt() && !x.IsDeleted).Select(x => x.ServiceLineIds).FirstOrDefault();
                if (serviceLineIds != null && serviceLineIds != "")
                {
                    var UserChannelSid = (from us in this._userSchedulesRepo.Table
                                          join u in this._userRepo.Table on us.UserIdFk equals u.UserId
                                          where serviceLineIds.ToIntList().Contains(us.ServiceLineIdFk.Value) && us.ScheduleDateStart <= DateTime.Now && us.ScheduleDateEnd >= DateTime.Now && !us.IsDeleted && !u.IsDeleted
                                          select u.UserChannelSid).ToList();

                    var notification = new PushNotificationVM()
                    {
                        Id = files.Id,
                        OrgId = files.OrgId,
                        UserChannelSid = UserChannelSid,
                        From = AuthorEnums.Trauma.ToString(),
                        Msg = "Trauma From is Changed",
                        RouteLink = "/Home/Activate%20Code/code-trauma-form"
                    };

                    _communication.pushNotification(notification);

                }
            }
            else if (files.CodeType == AuthorEnums.Blue.ToString())
            {
                var rootPath = this._codeBlueRepo.Table.Where(x => x.CodeBlueId == files.Id).Select(files.Type).FirstOrDefault();
                string path = _environment.WebRootFileProvider.GetFileInfo(rootPath + '/' + files.FileName)?.PhysicalPath;
                File.Delete(path);
                var serviceLineIds = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == files.OrgId && x.CodeIdFk == UCLEnums.Blue.ToInt() && !x.IsDeleted).Select(x => x.ServiceLineIds).FirstOrDefault();
                if (serviceLineIds != null && serviceLineIds != "")
                {
                    var UserChannelSid = (from us in this._userSchedulesRepo.Table
                                          join u in this._userRepo.Table on us.UserIdFk equals u.UserId
                                          where serviceLineIds.ToIntList().Contains(us.ServiceLineIdFk.Value) && us.ScheduleDateStart <= DateTime.Now && us.ScheduleDateEnd >= DateTime.Now && !us.IsDeleted && !u.IsDeleted
                                          select u.UserChannelSid).ToList();

                    var notification = new PushNotificationVM()
                    {
                        Id = files.Id,
                        OrgId = files.OrgId,
                        UserChannelSid = UserChannelSid,
                        From = AuthorEnums.Blue.ToString(),
                        Msg = "Blue From is Changed",
                        RouteLink = "/Home/Activate%20Code/code-blue-form"
                    };

                    _communication.pushNotification(notification);

                }
            }


            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "File Deleted Successfully" };
        }

        #endregion


        #region Code Stroke

        public BaseResponse GetAllStrokeCode(ActiveCodeVM activeCode)
        {
            var strokeData = new List<CodeStroke>();

            if (ApplicationSettings.isSuperAdmin)
            {
                strokeData = this._codeStrokeRepo.Table.Where(x => x.OrganizationIdFk == activeCode.OrganizationIdFk && !x.IsDeleted).OrderByDescending(x => x.CodeStrokeId).ToList();
            }
            else if (activeCode.showAllActiveCodes)
            {
                strokeData = this._codeStrokeRepo.Table.Where(x => x.OrganizationIdFk == activeCode.OrganizationIdFk && !x.IsDeleted).OrderByDescending(x => x.CodeStrokeId).ToList();
            }
            else
            {
                strokeData = (from cs in this._codeStrokeRepo.Table
                              join agm in this._activeCodesGroupMembersRepo.Table on cs.CodeStrokeId equals agm.ActiveCodeIdFk
                              where agm.UserIdFk == ApplicationSettings.UserId && agm.ActiveCodeName == UCLEnums.Stroke.ToString() && !cs.IsDeleted
                              select cs).OrderByDescending(x => x.CodeStrokeId).AsQueryable().ToList();

                //strokeData = this._codeStrokeRepo.Table.Where(x => x.CreatedBy == ApplicationSettings.UserId && !x.IsDeleted).OrderByDescending(x => x.CodeStrokeId).ToList();
            }
            var strokeDataVM = AutoMapperHelper.MapList<CodeStroke, CodeStrokeVM>(strokeData);
            //var orgData = GetHosplitalAddressObject(activeCode.OrganizationIdFk);

            strokeDataVM.ForEach(x =>
            {
                x.AttachmentsPath = new List<string>();
                x.AudiosPath = new List<string>();
                x.VideosPath = new List<string>();
                x.OrganizationData = new object();
                x.BloodThinnersTitle = new List<object>();
                x.ServiceLines = new List<ServiceLineVM>();

                //if (!string.IsNullOrEmpty(x.Attachments) && !string.IsNullOrWhiteSpace(x.Attachments))
                //{
                //    string path = this._RootPath + x.Attachments; //this._RootPath  + x.Attachments;
                //    if (Directory.Exists(path))
                //    {
                //        DirectoryInfo AttachFiles = new DirectoryInfo(path);
                //        foreach (var item in AttachFiles.GetFiles())
                //        {
                //            x.AttachmentsPath.Add(x.Attachments + "/" + item.Name);
                //        }
                //    }
                //}

                //if (!string.IsNullOrEmpty(x.Audio) && !string.IsNullOrWhiteSpace(x.Audio))
                //{
                //    string path = this._RootPath + x.Audio; //this._RootPath  + x.Audio;
                //    if (Directory.Exists(path))
                //    {
                //        DirectoryInfo AudioFiles = new DirectoryInfo(path);
                //        foreach (var item in AudioFiles.GetFiles())
                //        {
                //            x.AudiosPath.Add(x.Audio + "/" + item.Name);
                //        }
                //    }
                //}

                //if (!string.IsNullOrEmpty(x.Video) && !string.IsNullOrWhiteSpace(x.Video))
                //{
                //    var path = this._RootPath + x.Video; //this._RootPath  + x.Video;
                //    if (Directory.Exists(path))
                //    {
                //        DirectoryInfo VideoFiles = new DirectoryInfo(path);
                //        foreach (var item in VideoFiles.GetFiles())
                //        {
                //            x.VideosPath.Add(x.Video + "/" + item.Name);
                //        }
                //    }
                //}


                //var serviceIds = this._activeCodeRepo.Table.Where(s => s.OrganizationIdFk == x.OrganizationIdFk && s.CodeIdFk == UCLEnums.Stroke.ToInt() && !s.IsDeleted).Select(s => new { s.ServiceLineIds, s.DefaultServiceLineId }).FirstOrDefault();
                //var serviceLineIds = (from s in this._codesServiceLinesMappingRepo.Table
                //                      where s.OrganizationIdFk == x.OrganizationIdFk && s.CodeIdFk == UCLEnums.Stroke.ToInt()
                //                      && s.ActiveCodeId == x.CodeStrokeId && s.ActiveCodeName == UCLEnums.Stroke.ToString() && s.ServiceLineIdFk != serviceIds.DefaultServiceLineId
                //                      select s.ServiceLineIdFk).ToList();
                //x.ServiceLines = this._serviceLineRepo.Table.Where(s => serviceIds.ServiceLineIds.ToIntList().Distinct().Contains(s.ServiceLineId) && s.ServiceLineId != serviceIds.DefaultServiceLineId && !s.IsDeleted).Select(s => new ServiceLineVM() { ServiceLineId = s.ServiceLineId, ServiceName = s.ServiceName, IsSelected = serviceLineIds.Contains(s.ServiceLineId) }).ToList();
                //x.DefaultServiceLineId = serviceIds.DefaultServiceLineId;
                //x.DefaultServiceLine = this._serviceLineRepo.Table.Where(s => s.ServiceLineId == serviceIds.DefaultServiceLineId && !s.IsDeleted).Select(s => new ServiceLineVM() { ServiceLineId = s.ServiceLineId, ServiceName = s.ServiceName }).FirstOrDefault();
                //x.SelectedServiceLineIds = string.Join(",", serviceLineIds);
                x.LastKnownWellStr = x.LastKnownWell?.ToString("yyyy-MM-dd hh:mm:ss tt");
                x.DobStr = x.Dob?.ToString("yyyy-MM-dd hh:mm:ss tt");
                //x.OrganizationData = orgData;
                x.GenderTitle = _controlListDetailsRepo.Table.Where(g => g.ControlListDetailId == x.Gender).Select(g => g.Title).FirstOrDefault();
                x.BloodThinnersTitle.AddRange(_controlListDetailsRepo.Table.Where(b => x.BloodThinners.ToIntList().Contains(b.ControlListDetailId)).Select(b => new { Id = b.ControlListDetailId, b.Title }).ToList());
            });
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Returned", Body = strokeDataVM };
        }

        public BaseResponse GetStrokeDataById(int strokeId)
        {
            var strokeData = this._codeStrokeRepo.Table.Where(x => x.CodeStrokeId == strokeId && !x.IsDeleted).FirstOrDefault();
            if (strokeData != null)
            {
                var StrokeDataVM = AutoMapperHelper.MapSingleRow<CodeStroke, CodeStrokeVM>(strokeData);
                StrokeDataVM.AttachmentsPath = new List<string>();
                StrokeDataVM.AudiosPath = new List<string>();
                StrokeDataVM.VideosPath = new List<string>();
                StrokeDataVM.BloodThinnersTitle = new List<object>();
                StrokeDataVM.OrganizationData = new object();
                StrokeDataVM.ServiceLines = new List<ServiceLineVM>();

                if (!string.IsNullOrEmpty(StrokeDataVM.Attachments) && !string.IsNullOrWhiteSpace(StrokeDataVM.Attachments))
                {
                    string path = this._RootPath + strokeData.Attachments; //_environment.WebRootFileProvider.GetFileInfo(StrokeDataVM.Attachments)?.PhysicalPath;
                    if (Directory.Exists(path))
                    {
                        DirectoryInfo AttachFiles = new DirectoryInfo(path);
                        foreach (var item in AttachFiles.GetFiles())
                        {
                            StrokeDataVM.AttachmentsPath.Add(StrokeDataVM.Attachments + "/" + item.Name);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(StrokeDataVM.Audio) && !string.IsNullOrWhiteSpace(StrokeDataVM.Audio))
                {
                    string path = this._RootPath + strokeData.Audio; //_environment.WebRootFileProvider.GetFileInfo(StrokeDataVM.Audio)?.PhysicalPath;
                    if (Directory.Exists(path))
                    {
                        DirectoryInfo AudioFiles = new DirectoryInfo(path);
                        foreach (var item in AudioFiles.GetFiles())
                        {
                            StrokeDataVM.AudiosPath.Add(StrokeDataVM.Audio + "/" + item.Name);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(StrokeDataVM.Video) && !string.IsNullOrWhiteSpace(StrokeDataVM.Video))
                {
                    var path = this._RootPath + strokeData.Video;  //_environment.WebRootFileProvider.GetFileInfo(StrokeDataVM.Video)?.PhysicalPath; //.GetFileInfo(StrokeDataVM.Video);//?.PhysicalPath;
                    if (Directory.Exists(path))
                    {
                        DirectoryInfo VideoFiles = new DirectoryInfo(path);
                        foreach (var item in VideoFiles.GetFiles())
                        {
                            StrokeDataVM.VideosPath.Add(StrokeDataVM.Video + "/" + item.Name);
                        }
                    }
                }

                var serviceIds = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == strokeData.OrganizationIdFk && x.CodeIdFk == UCLEnums.Stroke.ToInt() && !x.IsDeleted).Select(x => new { x.ServiceLineIds, x.DefaultServiceLineId }).FirstOrDefault();
                var serviceLineIds = (from x in this._codesServiceLinesMappingRepo.Table
                                      where x.OrganizationIdFk == strokeData.OrganizationIdFk && x.CodeIdFk == UCLEnums.Stroke.ToInt()
                                      && x.ActiveCodeId == strokeData.CodeStrokeId && x.ActiveCodeName == UCLEnums.Stroke.ToString() && x.ServiceLineIdFk != serviceIds.DefaultServiceLineId
                                      select x.ServiceLineIdFk).ToList();
                StrokeDataVM.ServiceLines = this._serviceLineRepo.Table.Where(x => serviceIds.ServiceLineIds.ToIntList().Distinct().Contains(x.ServiceLineId) && x.ServiceLineId != serviceIds.DefaultServiceLineId && !x.IsDeleted).Select(x => new ServiceLineVM() { ServiceLineId = x.ServiceLineId, ServiceName = x.ServiceName, IsSelected = serviceLineIds.Contains(x.ServiceLineId) }).ToList();
                StrokeDataVM.DefaultServiceLineId = serviceIds.DefaultServiceLineId;
                StrokeDataVM.DefaultServiceLine = this._serviceLineRepo.Table.Where(x => x.ServiceLineId == serviceIds.DefaultServiceLineId && !x.IsDeleted).Select(x => new ServiceLineVM() { ServiceLineId = x.ServiceLineId, ServiceName = x.ServiceName }).FirstOrDefault();
                StrokeDataVM.SelectedServiceLineIds = string.Join(",", serviceLineIds);
                StrokeDataVM.OrganizationData = GetHosplitalAddressObject(StrokeDataVM.OrganizationIdFk);
                StrokeDataVM.LastKnownWellStr = StrokeDataVM.LastKnownWell?.ToString("yyyy-MM-dd hh:mm:ss tt");
                StrokeDataVM.DobStr = StrokeDataVM.Dob?.ToString("yyyy-MM-dd hh:mm:ss tt");
                StrokeDataVM.GenderTitle = _controlListDetailsRepo.Table.Where(g => g.ControlListDetailId == StrokeDataVM.Gender).Select(g => g.Title).FirstOrDefault();
                StrokeDataVM.BloodThinnersTitle.AddRange(_controlListDetailsRepo.Table.Where(b => StrokeDataVM.BloodThinners.ToIntList().Contains(b.ControlListDetailId)).Select(b => new { Id = b.ControlListDetailId, b.Title }).ToList());
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Found", Body = StrokeDataVM };
            }
            return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Record Not Found" };
        }

        public BaseResponse AddOrUpdateStrokeData(CodeStrokeVM codeStroke)
        {
            if (codeStroke != null && !string.IsNullOrEmpty(codeStroke.LastKnownWellStr) && !string.IsNullOrWhiteSpace(codeStroke.LastKnownWellStr))
            {
                codeStroke.LastKnownWell = DateTime.Parse(codeStroke.LastKnownWellStr);
            }
            if (codeStroke != null && !string.IsNullOrEmpty(codeStroke.DobStr) && !string.IsNullOrWhiteSpace(codeStroke.DobStr))
            {
                codeStroke.Dob = DateTime.Parse(codeStroke.DobStr);
            }
            if (codeStroke.CodeStrokeId > 0)
            {
                var row = this._codeStrokeRepo.Table.Where(x => x.CodeStrokeId == codeStroke.CodeStrokeId && !x.IsDeleted).FirstOrDefault();

                row.OrganizationIdFk = codeStroke.OrganizationIdFk;
                row.PatientName = codeStroke.PatientName;
                row.Dob = codeStroke.Dob;
                row.Gender = codeStroke.Gender;
                row.ChiefComplant = codeStroke.ChiefComplant;
                row.LastKnownWell = codeStroke.LastKnownWell;
                row.Hpi = codeStroke.Hpi;
                row.BloodThinners = codeStroke.BloodThinners;
                row.FamilyContactName = codeStroke.FamilyContactName;
                row.FamilyContactNumber = codeStroke.FamilyContactNumber;
                row.IsEms = codeStroke.IsEms;
                //row.IsCompleted = codeStroke.IsCompleted;
                if (codeStroke.IsCompleted != null && codeStroke.IsCompleted == true && row.IsCompleted != true)
                {
                    row.IsCompleted = true;
                    row.EndTime = DateTime.UtcNow;
                    row.ActualTime = row.EndTime - row.CreatedDate;
                }
                row.ModifiedBy = codeStroke.ModifiedBy;
                row.ModifiedDate = DateTime.UtcNow;
                row.IsDeleted = false;

                if (codeStroke.Attachment != null && codeStroke.Attachment.Count > 0)
                {
                    var RootPath = this._RootPath + "/Organizations"; //this._RootPath + "/Organizations";
                    string FileRoot = null;
                    List<string> Attachments = new();
                    FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeStroke.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                    FileRoot = Path.Combine(RootPath, FileRoot);
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, UCLEnums.Stroke.ToString());
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, row.CodeStrokeId.ToString());
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, "Attachments");

                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    //else
                    //{
                    //    DirectoryInfo dir = new DirectoryInfo(FileRoot);
                    //    foreach (FileInfo fi in dir.GetFiles())
                    //    {
                    //        fi.Delete();
                    //    }
                    //}
                    foreach (var item in codeStroke.Attachment)
                    {
                        if (!string.IsNullOrEmpty(item.Base64Str))
                        {

                            var fileInfo = item.Base64Str.Split("base64,");
                            string fileExtension = fileInfo[0].GetFileExtenstion();
                            var ByteFile = Convert.FromBase64String(fileInfo[1]);
                            string FilePath = Path.Combine(FileRoot, item.FileName);

                            if (File.Exists(FilePath))
                            {
                                long existingFileSize = 0;
                                long newFileSize = ByteFile.LongLength;
                                FileInfo ExistingfileInfo = new FileInfo(FilePath);
                                existingFileSize = ExistingfileInfo.Length;

                                if (existingFileSize > 0 && newFileSize != existingFileSize)
                                {
                                    var alterFile = item.FileName.Split('.');
                                    string extention = alterFile.LastOrDefault();
                                    var alterFileName = alterFile.ToList();
                                    alterFileName.RemoveAt(alterFileName.Count - 1);
                                    string fileName = string.Join(".", alterFileName);
                                    fileName = fileName + "_" + HelperExtension.CreateRandomString(7) + "." + extention;
                                    FilePath = Path.Combine(FileRoot, fileName);
                                    using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        fs.Write(ByteFile);
                                    }
                                }
                                else
                                {
                                    using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        fs.Write(ByteFile);
                                    }
                                }
                            }
                            else
                            {
                                using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                {
                                    fs.Write(ByteFile);
                                }
                            }
                        }
                    }
                    if (FileRoot != null && FileRoot != "")
                    {
                        codeStroke.AttachmentsFolderRoot = FileRoot.Replace(this._RootPath, "").Replace("\\", "/");
                    }
                }
                if (codeStroke.Videos != null && codeStroke.Videos.Count > 0)
                {
                    var RootPath = this._RootPath + "/Organizations";
                    string FileRoot = null;
                    List<string> Attachments = new();
                    FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeStroke.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                    FileRoot = Path.Combine(RootPath, FileRoot);
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, UCLEnums.Stroke.ToString());
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, row.CodeStrokeId.ToString());
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, "Videos");

                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    //else
                    //{
                    //    DirectoryInfo dir = new DirectoryInfo(FileRoot);
                    //    foreach (FileInfo fi in dir.GetFiles())
                    //    {
                    //        fi.Delete();
                    //    }
                    //}
                    foreach (var item in codeStroke.Videos)
                    {
                        if (!string.IsNullOrEmpty(item.Base64Str))
                        {

                            var fileInfo = item.Base64Str.Split("base64,");
                            string fileExtension = fileInfo[0].GetFileExtenstion();
                            var ByteFile = Convert.FromBase64String(fileInfo[1]);
                            string FilePath = Path.Combine(FileRoot, item.FileName);

                            if (File.Exists(FilePath))
                            {
                                long existingFileSize = 0;
                                long newFileSize = ByteFile.LongLength;
                                FileInfo ExistingfileInfo = new FileInfo(FilePath);
                                existingFileSize = ExistingfileInfo.Length;

                                if (existingFileSize > 0 && newFileSize != existingFileSize)
                                {
                                    var alterFile = item.FileName.Split('.');
                                    string extention = alterFile.LastOrDefault();
                                    var alterFileName = alterFile.ToList();
                                    alterFileName.RemoveAt(alterFileName.Count - 1);
                                    string fileName = string.Join(".", alterFileName);
                                    fileName = fileName + "_" + HelperExtension.CreateRandomString(7) + "." + extention;
                                    FilePath = Path.Combine(FileRoot, fileName);
                                    using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        fs.Write(ByteFile);
                                    }
                                }
                                else
                                {
                                    using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        fs.Write(ByteFile);
                                    }
                                }
                            }
                            else
                            {
                                using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                {
                                    fs.Write(ByteFile);
                                }
                            }
                        }

                    }
                    if (FileRoot != null && FileRoot != "")
                    {
                        codeStroke.VideoFolderRoot = FileRoot.Replace(this._RootPath, "").Replace("\\", "/");
                    }
                }
                if (codeStroke.Audios != null && codeStroke.Audios.Count > 0)
                {
                    var RootPath = this._RootPath + "/Organizations";
                    string FileRoot = null;
                    List<string> Attachments = new();

                    FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeStroke.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                    FileRoot = Path.Combine(RootPath, FileRoot);
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, UCLEnums.Stroke.ToString());
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, row.CodeStrokeId.ToString());
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, "Audios");

                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    //else
                    //{
                    //    DirectoryInfo dir = new DirectoryInfo(FileRoot);
                    //    foreach (FileInfo fi in dir.GetFiles())
                    //    {
                    //        fi.Delete();
                    //    }
                    //}
                    foreach (var item in codeStroke.Audios)
                    {
                        if (!string.IsNullOrEmpty(item.Base64Str))
                        {

                            var fileInfo = item.Base64Str.Split("base64,");
                            string fileExtension = fileInfo[0].GetFileExtenstion();
                            var ByteFile = Convert.FromBase64String(fileInfo[1]);
                            string FilePath = Path.Combine(FileRoot, item.FileName);

                            if (File.Exists(FilePath))
                            {
                                long existingFileSize = 0;
                                long newFileSize = ByteFile.LongLength;
                                FileInfo ExistingfileInfo = new FileInfo(FilePath);
                                existingFileSize = ExistingfileInfo.Length;

                                if (existingFileSize > 0 && newFileSize != existingFileSize)
                                {
                                    var alterFile = item.FileName.Split('.');
                                    string extention = alterFile.LastOrDefault();
                                    var alterFileName = alterFile.ToList();
                                    alterFileName.RemoveAt(alterFileName.Count - 1);
                                    string fileName = string.Join(".", alterFileName);
                                    fileName = fileName + "_" + HelperExtension.CreateRandomString(7) + "." + extention;
                                    FilePath = Path.Combine(FileRoot, fileName);
                                    using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        fs.Write(ByteFile);
                                    }
                                }
                                else
                                {
                                    using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        fs.Write(ByteFile);
                                    }
                                }
                            }
                            else
                            {
                                using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                {
                                    fs.Write(ByteFile);
                                }
                            }
                        }


                    }
                    if (FileRoot != null && FileRoot != "")
                    {
                        codeStroke.AudioFolderRoot = FileRoot.Replace(this._RootPath, "").Replace("\\", "/");
                    }
                }

                if (codeStroke.AttachmentsFolderRoot != null)
                {
                    row.Attachments = codeStroke.AttachmentsFolderRoot;
                }
                if (codeStroke.VideoFolderRoot != null)
                {
                    row.Video = codeStroke.VideoFolderRoot;
                }
                if (codeStroke.AudioFolderRoot != null)
                {
                    row.Audio = codeStroke.AudioFolderRoot;
                }

                this._codeStrokeRepo.Update(row);

                //this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == row.OrganizationIdFk && x.CodeIdFk == UCLEnums.Stroke.ToInt() && !x.IsDeleted).Select(x => x.ServiceLineIds).FirstOrDefault();
                if (codeStroke.SelectedServiceLineIds != null && codeStroke.SelectedServiceLineIds != "")
                {
                    var serviceLineIds = codeStroke.SelectedServiceLineIds.ToIntList();

                    var codeServiceMapping = this._codesServiceLinesMappingRepo.Table.Where(x => x.OrganizationIdFk == row.OrganizationIdFk && x.CodeIdFk == UCLEnums.Stroke.ToInt() && x.ActiveCodeId == row.CodeStrokeId).ToList();
                    var rowsToDelete = codeServiceMapping.Where(x => !serviceLineIds.Contains(x.ServiceLineIdFk)).ToList();
                    if (rowsToDelete.Count > 0)
                        this._codesServiceLinesMappingRepo.DeleteRange(rowsToDelete);

                    serviceLineIds.RemoveAll(x => codeServiceMapping.Select(s => s.ServiceLineIdFk).Contains(x));
                    var codeServiceMappingList = new List<CodesServiceLinesMapping>();
                    foreach (var item in serviceLineIds)
                    {
                        var codeService = new CodesServiceLinesMapping()
                        {
                            OrganizationIdFk = row.OrganizationIdFk,
                            CodeIdFk = UCLEnums.Stroke.ToInt(),
                            ServiceLineIdFk = item,
                            ActiveCodeId = row.CodeStrokeId,
                            ActiveCodeName = UCLEnums.Stroke.ToString()
                        };
                        codeServiceMappingList.Add(codeService);
                    }
                    this._codesServiceLinesMappingRepo.Insert(codeServiceMappingList);


                    //var UserChannelSid = (from us in this._userSchedulesRepo.Table
                    //                      join u in this._userRepo.Table on us.UserIdFk equals u.UserId
                    //                      where codeStroke.SelectedServiceLineIds.ToIntList().Contains(us.ServiceLineIdFk.Value) && us.ScheduleDateStart <= DateTime.Now && us.ScheduleDateEnd >= DateTime.Now && !us.IsDeleted && !u.IsDeleted
                    //                      select u.UserChannelSid).ToList();

                    var UserChannelSid = (from u in this._userRepo.Table
                                      join gm in this._activeCodesGroupMembersRepo.Table on u.UserId equals gm.UserIdFk
                                      where gm.ActiveCodeIdFk == codeStroke.CodeStrokeId && gm.ActiveCodeName == UCLEnums.Stroke.ToString() && !u.IsDeleted
                                      select u.UserChannelSid).Distinct().ToList();

                    //var loggedInUserChannelId = this._userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && !x.IsDeleted).Select(x => x.UserChannelSid).FirstOrDefault();

                    //UserChannelSid.Add(loggedInUserChannelId);
                    //UserChannelSid = UserChannelSid.Distinct().ToList();

                    var notification = new PushNotificationVM()
                    {
                        Id = row.CodeStrokeId,
                        OrgId = row.OrganizationIdFk,
                        UserChannelSid = UserChannelSid,
                        From = AuthorEnums.Stroke.ToString(),
                        Msg = (codeStroke.IsEms.HasValue && codeStroke.IsEms.Value ? "EMS" : "Active") + " Code Stroke From is Changed",
                        RouteLink = "/Home/Activate%20Code/code-strok-form",
                        RouteLinkEMS = "/Home/EMS/activateCode"
                    };

                    _communication.pushNotification(notification);

                }

                return GetStrokeDataById(row.CodeStrokeId);
            }
            else
            {
                //var serviceLineIds = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == codeStroke.OrganizationIdFk && x.CodeIdFk == UCLEnums.Stroke.ToInt() && !x.IsDeleted).Select(x => x.ServiceLineIds).FirstOrDefault();
                if (codeStroke.SelectedServiceLineIds != null && codeStroke.SelectedServiceLineIds != "")
                {
                    var serviceLineIds = codeStroke.SelectedServiceLineIds.ToIntList();
                    codeStroke.CreatedDate = DateTime.UtcNow;
                    var stroke = AutoMapperHelper.MapSingleRow<CodeStrokeVM, CodeStroke>(codeStroke);

                    if (codeStroke.Attachment != null && codeStroke.Attachment.Count > 0)
                    {
                        var RootPath = this._RootPath + "/Organizations";
                        string FileRoot = null;
                        List<string> Attachments = new();


                        FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeStroke.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                        FileRoot = Path.Combine(RootPath, FileRoot);
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, UCLEnums.Stroke.ToString());
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, stroke.CodeStrokeId.ToString());
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, "Attachments");

                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        //else
                        //{
                        //    DirectoryInfo dir = new DirectoryInfo(FileRoot);
                        //    foreach (FileInfo fi in dir.GetFiles())
                        //    {
                        //        fi.Delete();
                        //    }
                        //}
                        foreach (var item in codeStroke.Attachment)
                        {
                            if (!string.IsNullOrEmpty(item.Base64Str))
                            {

                                var fileInfo = item.Base64Str.Split("base64,");
                                string fileExtension = fileInfo[0].GetFileExtenstion();
                                var ByteFile = Convert.FromBase64String(fileInfo[1]);
                                string FilePath = Path.Combine(FileRoot, item.FileName);

                                if (File.Exists(FilePath))
                                {
                                    long existingFileSize = 0;
                                    long newFileSize = ByteFile.LongLength;
                                    FileInfo ExistingfileInfo = new FileInfo(FilePath);
                                    existingFileSize = ExistingfileInfo.Length;

                                    if (existingFileSize > 0 && newFileSize != existingFileSize)
                                    {
                                        var alterFile = item.FileName.Split('.');
                                        string extention = alterFile.LastOrDefault();
                                        var alterFileName = alterFile.ToList();
                                        alterFileName.RemoveAt(alterFileName.Count - 1);
                                        string fileName = string.Join(".", alterFileName);
                                        fileName = fileName + "_" + HelperExtension.CreateRandomString(7) + "." + extention;
                                        FilePath = Path.Combine(FileRoot, fileName);
                                        using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                        {
                                            fs.Write(ByteFile);
                                        }
                                    }
                                    else
                                    {
                                        using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                        {
                                            fs.Write(ByteFile);
                                        }
                                    }
                                }
                                else
                                {
                                    using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        fs.Write(ByteFile);
                                    }
                                }
                            }


                        }
                        if (FileRoot != null && FileRoot != "")
                        {
                            codeStroke.AttachmentsFolderRoot = FileRoot.Replace(this._RootPath, "").Replace("\\", "/");
                        }
                    }
                    if (codeStroke.Videos != null && codeStroke.Videos.Count > 0)
                    {
                        var RootPath = this._RootPath + "/Organizations";
                        string FileRoot = null;
                        List<string> Attachments = new();


                        FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeStroke.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                        FileRoot = Path.Combine(RootPath, FileRoot);
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, UCLEnums.Stroke.ToString());
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, stroke.CodeStrokeId.ToString());
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, "Videos");

                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        //else
                        //{
                        //    DirectoryInfo dir = new DirectoryInfo(FileRoot);
                        //    foreach (FileInfo fi in dir.GetFiles())
                        //    {
                        //        fi.Delete();
                        //    }
                        //}
                        foreach (var item in codeStroke.Videos)
                        {
                            if (!string.IsNullOrEmpty(item.Base64Str))
                            {

                                var fileInfo = item.Base64Str.Split("base64,");
                                string fileExtension = fileInfo[0].GetFileExtenstion();
                                var ByteFile = Convert.FromBase64String(fileInfo[1]);
                                string FilePath = Path.Combine(FileRoot, item.FileName);

                                if (File.Exists(FilePath))
                                {
                                    long existingFileSize = 0;
                                    long newFileSize = ByteFile.LongLength;
                                    FileInfo ExistingfileInfo = new FileInfo(FilePath);
                                    existingFileSize = ExistingfileInfo.Length;

                                    if (existingFileSize > 0 && newFileSize != existingFileSize)
                                    {
                                        var alterFile = item.FileName.Split('.');
                                        string extention = alterFile.LastOrDefault();
                                        var alterFileName = alterFile.ToList();
                                        alterFileName.RemoveAt(alterFileName.Count - 1);
                                        string fileName = string.Join(".", alterFileName);
                                        fileName = fileName + "_" + HelperExtension.CreateRandomString(7) + "." + extention;
                                        FilePath = Path.Combine(FileRoot, fileName);
                                        using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                        {
                                            fs.Write(ByteFile);
                                        }
                                    }
                                    else
                                    {
                                        using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                        {
                                            fs.Write(ByteFile);
                                        }
                                    }
                                }
                                else
                                {
                                    using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        fs.Write(ByteFile);
                                    }
                                }
                            }


                        }
                        if (FileRoot != null && FileRoot != "")
                        {
                            codeStroke.VideoFolderRoot = FileRoot.Replace(this._RootPath, "").Replace("\\", "/");
                        }
                    }
                    if (codeStroke.Audios != null && codeStroke.Audios.Count > 0)
                    {
                        var RootPath = this._RootPath + "/Organizations";
                        string FileRoot = null;
                        List<string> Attachments = new();


                        FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeStroke.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                        FileRoot = Path.Combine(RootPath, FileRoot);
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, UCLEnums.Stroke.ToString());
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, stroke.CodeStrokeId.ToString());
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, "Audios");

                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        //else
                        //{
                        //    DirectoryInfo dir = new DirectoryInfo(FileRoot);
                        //    foreach (FileInfo fi in dir.GetFiles())
                        //    {
                        //        fi.Delete();
                        //    }
                        //}
                        foreach (var item in codeStroke.Audios)
                        {
                            if (!string.IsNullOrEmpty(item.Base64Str))
                            {

                                var fileInfo = item.Base64Str.Split("base64,");
                                string fileExtension = fileInfo[0].GetFileExtenstion();
                                var ByteFile = Convert.FromBase64String(fileInfo[1]);
                                string FilePath = Path.Combine(FileRoot, item.FileName);

                                if (File.Exists(FilePath))
                                {
                                    long existingFileSize = 0;
                                    long newFileSize = ByteFile.LongLength;
                                    FileInfo ExistingfileInfo = new FileInfo(FilePath);
                                    existingFileSize = ExistingfileInfo.Length;

                                    if (existingFileSize > 0 && newFileSize != existingFileSize)
                                    {
                                        var alterFile = item.FileName.Split('.');
                                        string extention = alterFile.LastOrDefault();
                                        var alterFileName = alterFile.ToList();
                                        alterFileName.RemoveAt(alterFileName.Count - 1);
                                        string fileName = string.Join(".", alterFileName);
                                        fileName = fileName + "_" + HelperExtension.CreateRandomString(7) + "." + extention;
                                        FilePath = Path.Combine(FileRoot, fileName);
                                        using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                        {
                                            fs.Write(ByteFile);
                                        }
                                    }
                                    else
                                    {
                                        using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                        {
                                            fs.Write(ByteFile);
                                        }
                                    }
                                }
                                else
                                {
                                    using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        fs.Write(ByteFile);
                                    }
                                }
                            }


                        }
                        if (FileRoot != null && FileRoot != "")
                        {
                            codeStroke.AudioFolderRoot = FileRoot.Replace(this._RootPath, "").Replace("\\", "/");
                        }
                    }

                    if (codeStroke.AttachmentsFolderRoot != null)
                    {
                        stroke.Attachments = codeStroke.AttachmentsFolderRoot;
                    }
                    if (codeStroke.VideoFolderRoot != null)
                    {
                        stroke.Video = codeStroke.VideoFolderRoot;
                    }
                    if (codeStroke.AudioFolderRoot != null)
                    {
                        stroke.Audio = codeStroke.AudioFolderRoot;
                    }

                    this._codeStrokeRepo.Insert(stroke);

                    var codeServiceMappingList = new List<CodesServiceLinesMapping>();
                    foreach (var item in serviceLineIds)
                    {
                        var codeServiceMapping = new CodesServiceLinesMapping()
                        {
                            OrganizationIdFk = stroke.OrganizationIdFk,
                            CodeIdFk = UCLEnums.Stroke.ToInt(),
                            ServiceLineIdFk = item,
                            ActiveCodeId = stroke.CodeStrokeId,
                            ActiveCodeName = UCLEnums.Stroke.ToString()
                        };
                        codeServiceMappingList.Add(codeServiceMapping);
                    }
                    this._codesServiceLinesMappingRepo.Insert(codeServiceMappingList);

                    var UserChannelSid = (from us in this._userSchedulesRepo.Table
                                          join u in this._userRepo.Table on us.UserIdFk equals u.UserId
                                          where serviceLineIds.Contains(us.ServiceLineIdFk.Value) && us.ScheduleDateStart <= DateTime.UtcNow && us.ScheduleDateEnd >= DateTime.UtcNow && !us.IsDeleted && !u.IsDeleted
                                          select new { u.UserUniqueId, u.UserId }).ToList();
                    var loggedUser = this._userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId }).FirstOrDefault();
                    UserChannelSid.Add(loggedUser);
                    var conversationChannelAttributes = JsonConvert.SerializeObject(new Dictionary<string, Object>()
                                    {
                                        {ChannelAttributeEnums.ChannelType.ToString(), ChannelTypeEnums.EMS.ToString()},
                                        {ChannelAttributeEnums.CodeType.ToString(), ChannelTypeEnums.Stroke.ToString()},
                                        {ChannelAttributeEnums.StrokeId.ToString(), stroke.CodeStrokeId}
                                    }, Formatting.Indented);
                    List<ActiveCodesGroupMember> ACodeGroupMembers = new List<ActiveCodesGroupMember>();
                    if (UserChannelSid != null && UserChannelSid.Count > 0)
                    {
                        string uniqueName = DateTime.Now.ToString("yyyyMMddHHmmssffff") + ApplicationSettings.UserId.ToString();
                        string friendlyName = stroke.IsEms.HasValue && stroke.IsEms.Value ? $"EMS Code {UCLEnums.Stroke.ToString()} {stroke.CodeStrokeId}" : $"Active Code {UCLEnums.Stroke.ToString()} {stroke.CodeStrokeId}";
                        var channel = _communication.createConversationChannel(friendlyName, uniqueName, conversationChannelAttributes);
                        UserChannelSid = UserChannelSid.Distinct().ToList();
                        foreach (var item in UserChannelSid)
                        {
                            try
                            {
                                var codeGroupMember = new ActiveCodesGroupMember()
                                {
                                    UserIdFk = item.UserId,
                                    ActiveCodeIdFk = stroke.CodeStrokeId,
                                    ActiveCodeName = UCLEnums.Stroke.ToString(),
                                    IsAcknowledge = false,
                                    CreatedBy = ApplicationSettings.UserId,
                                    CreatedDate = DateTime.UtcNow,
                                    IsDeleted = false
                                };
                                ACodeGroupMembers.Add(codeGroupMember);
                                _communication.addNewUserToConversationChannel(channel.Sid, item.UserUniqueId);
                            }
                            catch (Exception ex)
                            {
                                ElmahExtensions.RiseError(ex);
                            }
                        }
                        var isMembersAdded = AddGroupMembers(ACodeGroupMembers);
                        var msg = new ConversationMessageVM();
                        msg.channelSid = channel.Sid;
                        msg.author = "System";
                        msg.attributes = "";
                        msg.body = $"<strong> {(codeStroke.IsEms.HasValue && codeStroke.IsEms.Value ? "EMS Code" : "Active Code")} {UCLEnums.Stroke.ToString()} </strong></br></br>";
                        if (codeStroke.PatientName != null && codeStroke.PatientName != "")
                            msg.body += $"<strong>Patient Name: </strong> {codeStroke.PatientName} </br>";
                        msg.body += $"<strong>Dob: </strong> {codeStroke.Dob:MM-dd-yyyy} </br>";
                        msg.body += $"<strong>Last Well Known: </strong> {codeStroke.LastKnownWell:MM-dd-yyyy hh:mm tt} </br>";
                        if (codeStroke.ChiefComplant != null && codeStroke.ChiefComplant != "")
                            msg.body += $"<strong>Chief Complaint: </strong> {codeStroke.ChiefComplant} </br>";
                        if (codeStroke.Hpi != null && codeStroke.Hpi != "")
                            msg.body += $"<strong>Hpi: </strong> {codeStroke.Hpi} </br>";
                        var sendMsg = _communication.sendPushNotification(msg);
                    }
                    return GetStrokeDataById(stroke.CodeStrokeId);
                }
                return new BaseResponse() { Status = HttpStatusCode.NotAcceptable, Message = "There is no Service Line in this organization related to Code Stroke" };
            }
        }

        public BaseResponse DeleteStroke(int strokeId)
        {
            var row = this._codeStrokeRepo.Table.Where(x => x.CodeStrokeId == strokeId && !x.IsDeleted).FirstOrDefault();
            row.IsDeleted = true;
            row.ModifiedBy = ApplicationSettings.UserId;
            row.ModifiedDate = DateTime.UtcNow;
            this._codeStrokeRepo.Update(row);

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Deleted" };
        }

        #endregion

        #region Code Sepsis


        public BaseResponse GetAllSepsisCode(ActiveCodeVM activeCode)
        {
            var SepsisData = new List<CodeSepsi>();
            if (ApplicationSettings.isSuperAdmin)
            {
                SepsisData = this._codeSepsisRepo.Table.Where(x => x.OrganizationIdFk == activeCode.OrganizationIdFk && !x.IsDeleted).OrderByDescending(x => x.CodeSepsisId).ToList();
            }
            else if (activeCode.showAllActiveCodes)
            {
                SepsisData = this._codeSepsisRepo.Table.Where(x => x.OrganizationIdFk == activeCode.OrganizationIdFk && !x.IsDeleted).OrderByDescending(x => x.CodeSepsisId).ToList();
            }
            else
            {
                SepsisData = (from cs in this._codeSepsisRepo.Table
                              join agm in this._activeCodesGroupMembersRepo.Table on cs.CodeSepsisId equals agm.ActiveCodeIdFk
                              where agm.UserIdFk == ApplicationSettings.UserId && agm.ActiveCodeName == UCLEnums.Sepsis.ToString() && !cs.IsDeleted
                              select cs).OrderByDescending(x => x.CodeSepsisId).AsQueryable().ToList();

                //SepsisData = this._codeSepsisRepo.Table.Where(x => x.CreatedBy == ApplicationSettings.UserId && !x.IsDeleted).OrderByDescending(x => x.CodeSepsisId).ToList();
            }
            var SepsisDataVM = AutoMapperHelper.MapList<CodeSepsi, CodeSepsisVM>(SepsisData);

            //var orgData = GetHosplitalAddressObject(activeCode.OrganizationIdFk);

            SepsisDataVM.ForEach(x =>
            {
                x.AttachmentsPath = new List<string>();
                x.AudiosPath = new List<string>();
                x.VideosPath = new List<string>();
                x.BloodThinnersTitle = new List<object>();
                x.ServiceLines = new List<ServiceLineVM>();
                //x.OrganizationData == new object();

                //if (!string.IsNullOrEmpty(x.Attachments) && !string.IsNullOrWhiteSpace(x.Attachments))
                //{
                //    string path = this._RootPath + x.Attachments;
                //    if (Directory.Exists(path))
                //    {
                //        DirectoryInfo AttachFiles = new DirectoryInfo(path);
                //        foreach (var item in AttachFiles.GetFiles())
                //        {
                //            x.AttachmentsPath.Add(x.Attachments + "/" + item.Name);
                //        }
                //    }
                //}

                //if (!string.IsNullOrEmpty(x.Audio) && !string.IsNullOrWhiteSpace(x.Audio))
                //{
                //    string path = this._RootPath + x.Audio;
                //    if (Directory.Exists(path))
                //    {
                //        DirectoryInfo AudioFiles = new DirectoryInfo(path);
                //        foreach (var item in AudioFiles.GetFiles())
                //        {
                //            x.AudiosPath.Add(x.Audio + "/" + item.Name);
                //        }
                //    }
                //}

                //if (!string.IsNullOrEmpty(x.Video) && !string.IsNullOrWhiteSpace(x.Video))
                //{
                //    var path = this._RootPath + x.Video;
                //    if (Directory.Exists(path))
                //    {
                //        DirectoryInfo VideoFiles = new DirectoryInfo(path);
                //        foreach (var item in VideoFiles.GetFiles())
                //        {
                //            x.VideosPath.Add(x.Video + "/" + item.Name);
                //        }
                //    }
                //}
                //var serviceIds = this._activeCodeRepo.Table.Where(s => s.OrganizationIdFk == x.OrganizationIdFk && s.CodeIdFk == UCLEnums.Sepsis.ToInt() && !s.IsDeleted).Select(s => new { s.ServiceLineIds, s.DefaultServiceLineId }).FirstOrDefault();
                //var serviceLineIds = (from s in this._codesServiceLinesMappingRepo.Table
                //                      where s.OrganizationIdFk == x.OrganizationIdFk && s.CodeIdFk == UCLEnums.Sepsis.ToInt()
                //                      && s.ActiveCodeId == x.CodeSepsisId && s.ActiveCodeName == UCLEnums.Sepsis.ToString() && s.ServiceLineIdFk != serviceIds.DefaultServiceLineId
                //                      select s.ServiceLineIdFk).ToList();
                //x.ServiceLines = this._serviceLineRepo.Table.Where(s => serviceIds.ServiceLineIds.ToIntList().Distinct().Contains(s.ServiceLineId) && s.ServiceLineId != serviceIds.DefaultServiceLineId && !s.IsDeleted).Select(s => new ServiceLineVM() { ServiceLineId = s.ServiceLineId, ServiceName = s.ServiceName, IsSelected = serviceLineIds.Contains(s.ServiceLineId) }).ToList();
                //x.DefaultServiceLineId = serviceIds.DefaultServiceLineId;
                //x.DefaultServiceLine = this._serviceLineRepo.Table.Where(s => s.ServiceLineId == serviceIds.DefaultServiceLineId && !s.IsDeleted).Select(s => new ServiceLineVM() { ServiceLineId = s.ServiceLineId, ServiceName = s.ServiceName }).FirstOrDefault();
                //x.SelectedServiceLineIds = string.Join(",", serviceLineIds);
                x.LastKnownWellStr = x.LastKnownWell.ToString("yyyy-MM-dd hh:mm:ss tt");
                x.DobStr = x.Dob.ToString("yyyy-MM-dd hh:mm:ss tt");
                //x.OrganizationData = orgData;
                x.GenderTitle = _controlListDetailsRepo.Table.Where(g => g.ControlListDetailId == x.Gender).Select(g => g.Title).FirstOrDefault();
                x.BloodThinnersTitle.AddRange(_controlListDetailsRepo.Table.Where(b => x.BloodThinners.ToIntList().Contains(b.ControlListDetailId)).Select(b => new { Id = b.ControlListDetailId, b.Title }).ToList());
            });
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Returned", Body = SepsisDataVM };
        }

        public BaseResponse GetSepsisDataById(int SepsisId)
        {
            var SepsisData = this._codeSepsisRepo.Table.Where(x => x.CodeSepsisId == SepsisId && !x.IsDeleted).FirstOrDefault();
            if (SepsisData != null)
            {
                var SepsisDataVM = AutoMapperHelper.MapSingleRow<CodeSepsi, CodeSepsisVM>(SepsisData);
                SepsisDataVM.AttachmentsPath = new List<string>();
                SepsisDataVM.AudiosPath = new List<string>();
                SepsisDataVM.VideosPath = new List<string>();
                SepsisDataVM.BloodThinnersTitle = new List<object>();

                if (!string.IsNullOrEmpty(SepsisDataVM.Attachments) && !string.IsNullOrWhiteSpace(SepsisDataVM.Attachments))
                {
                    string path = this._RootPath + SepsisDataVM.Attachments; //_environment.WebRootFileProvider.GetFileInfo(SepsisDataVM.Attachments)?.PhysicalPath;
                    if (Directory.Exists(path))
                    {
                        DirectoryInfo AttachFiles = new DirectoryInfo(path);
                        foreach (var item in AttachFiles.GetFiles())
                        {
                            SepsisDataVM.AttachmentsPath.Add(SepsisDataVM.Attachments + "/" + item.Name);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(SepsisDataVM.Audio) && !string.IsNullOrWhiteSpace(SepsisDataVM.Audio))
                {
                    string path = this._RootPath + SepsisDataVM.Audio; //_environment.WebRootFileProvider.GetFileInfo(SepsisDataVM.Audio)?.PhysicalPath;
                    if (Directory.Exists(path))
                    {
                        DirectoryInfo AudioFiles = new DirectoryInfo(path);
                        foreach (var item in AudioFiles.GetFiles())
                        {
                            SepsisDataVM.AudiosPath.Add(SepsisDataVM.Audio + "/" + item.Name);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(SepsisDataVM.Video) && !string.IsNullOrWhiteSpace(SepsisDataVM.Video))
                {
                    var path = this._RootPath + SepsisDataVM.Video; //_environment.WebRootFileProvider.GetFileInfo(SepsisDataVM.Video)?.PhysicalPath; //.GetFileInfo(SepsisDataVM.Video);//?.PhysicalPath;
                    if (Directory.Exists(path))
                    {
                        DirectoryInfo VideoFiles = new DirectoryInfo(path);
                        foreach (var item in VideoFiles.GetFiles())
                        {
                            SepsisDataVM.VideosPath.Add(SepsisDataVM.Video + "/" + item.Name);
                        }
                    }
                }

                var serviceIds = this._activeCodeRepo.Table.Where(s => s.OrganizationIdFk == SepsisDataVM.OrganizationIdFk && s.CodeIdFk == UCLEnums.Sepsis.ToInt() && !s.IsDeleted).Select(s => new { s.ServiceLineIds, s.DefaultServiceLineId }).FirstOrDefault();
                var serviceLineIds = (from s in this._codesServiceLinesMappingRepo.Table
                                      where s.OrganizationIdFk == SepsisDataVM.OrganizationIdFk && s.CodeIdFk == UCLEnums.Sepsis.ToInt()
                                      && s.ActiveCodeId == SepsisDataVM.CodeSepsisId && s.ActiveCodeName == UCLEnums.Sepsis.ToString() && s.ServiceLineIdFk != serviceIds.DefaultServiceLineId
                                      select s.ServiceLineIdFk).ToList();
                SepsisDataVM.ServiceLines = this._serviceLineRepo.Table.Where(s => serviceIds.ServiceLineIds.ToIntList().Distinct().Contains(s.ServiceLineId) && s.ServiceLineId != serviceIds.DefaultServiceLineId && !s.IsDeleted).Select(s => new ServiceLineVM() { ServiceLineId = s.ServiceLineId, ServiceName = s.ServiceName, IsSelected = serviceLineIds.Contains(s.ServiceLineId) }).ToList();
                SepsisDataVM.DefaultServiceLineId = serviceIds.DefaultServiceLineId;
                SepsisDataVM.DefaultServiceLine = this._serviceLineRepo.Table.Where(s => s.ServiceLineId == serviceIds.DefaultServiceLineId && !s.IsDeleted).Select(s => new ServiceLineVM() { ServiceLineId = s.ServiceLineId, ServiceName = s.ServiceName }).FirstOrDefault();
                SepsisDataVM.SelectedServiceLineIds = string.Join(",", serviceLineIds);

                SepsisDataVM.OrganizationData = GetHosplitalAddressObject(SepsisDataVM.OrganizationIdFk);
                SepsisDataVM.LastKnownWellStr = SepsisDataVM.LastKnownWell.ToString("yyyy-MM-dd hh:mm:ss tt");
                SepsisDataVM.DobStr = SepsisDataVM.Dob.ToString("yyyy-MM-dd hh:mm:ss tt");
                SepsisDataVM.GenderTitle = _controlListDetailsRepo.Table.Where(g => g.ControlListDetailId == SepsisDataVM.Gender).Select(g => g.Title).FirstOrDefault();
                SepsisDataVM.BloodThinnersTitle.AddRange(_controlListDetailsRepo.Table.Where(b => SepsisDataVM.BloodThinners.ToIntList().Contains(b.ControlListDetailId)).Select(b => new { Id = b.ControlListDetailId, b.Title }).ToList());
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Found", Body = SepsisDataVM };
            }
            return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Record Not Found" };
        }

        public BaseResponse AddOrUpdateSepsisData(CodeSepsisVM codeSepsis)
        {
            if (codeSepsis != null && !string.IsNullOrEmpty(codeSepsis.LastKnownWellStr) && !string.IsNullOrWhiteSpace(codeSepsis.LastKnownWellStr))
            {
                codeSepsis.LastKnownWell = DateTime.Parse(codeSepsis.LastKnownWellStr);
            }
            if (codeSepsis != null && !string.IsNullOrEmpty(codeSepsis.DobStr) && !string.IsNullOrWhiteSpace(codeSepsis.DobStr))
            {
                codeSepsis.Dob = DateTime.Parse(codeSepsis.DobStr);
            }
            if (codeSepsis.CodeSepsisId > 0)
            {
                var row = this._codeSepsisRepo.Table.Where(x => x.CodeSepsisId == codeSepsis.CodeSepsisId && !x.IsDeleted).FirstOrDefault();

                row.PatientName = codeSepsis.PatientName;
                row.Dob = codeSepsis.Dob;
                row.Gender = codeSepsis.Gender;
                row.ChiefComplant = codeSepsis.ChiefComplant;
                row.LastKnownWell = codeSepsis.LastKnownWell;
                row.Hpi = codeSepsis.Hpi;
                row.BloodThinners = codeSepsis.BloodThinners;
                row.FamilyContactName = codeSepsis.FamilyContactName;
                row.FamilyContactNumber = codeSepsis.FamilyContactNumber;
                row.IsEms = codeSepsis.IsEms;
                //row.IsCompleted = codeSepsis.IsCompleted;
                if (codeSepsis.IsCompleted == true && row.IsCompleted != true)
                {
                    row.IsCompleted = true;
                    row.EndTime = DateTime.UtcNow;
                    row.ActualTime = row.EndTime - row.CreatedDate;
                }
                row.ModifiedBy = codeSepsis.ModifiedBy;
                row.ModifiedDate = DateTime.UtcNow;
                row.IsDeleted = false;


                if (codeSepsis.Attachment != null && codeSepsis.Attachment.Count > 0)
                {
                    var RootPath = this._RootPath + "/Organizations";
                    string FileRoot = null;
                    List<string> Attachments = new();

                    FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeSepsis.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                    FileRoot = Path.Combine(RootPath, FileRoot);
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, UCLEnums.Sepsis.ToString());
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, row.CodeSepsisId.ToString());
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, "Attachments");

                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    //else
                    //{
                    //    DirectoryInfo dir = new DirectoryInfo(FileRoot);
                    //    foreach (FileInfo fi in dir.GetFiles())
                    //    {
                    //        fi.Delete();
                    //    }
                    //}
                    foreach (var item in codeSepsis.Attachment)
                    {
                        if (!string.IsNullOrEmpty(item.Base64Str))
                        {

                            var fileInfo = item.Base64Str.Split("base64,");
                            string fileExtension = fileInfo[0].GetFileExtenstion();
                            var ByteFile = Convert.FromBase64String(fileInfo[1]);
                            string FilePath = Path.Combine(FileRoot, item.FileName);

                            if (File.Exists(FilePath))
                            {
                                long existingFileSize = 0;
                                long newFileSize = ByteFile.LongLength;
                                FileInfo ExistingfileInfo = new FileInfo(FilePath);
                                existingFileSize = ExistingfileInfo.Length;

                                if (existingFileSize > 0 && newFileSize != existingFileSize)
                                {
                                    var alterFile = item.FileName.Split('.');
                                    string extention = alterFile.LastOrDefault();
                                    var alterFileName = alterFile.ToList();
                                    alterFileName.RemoveAt(alterFileName.Count - 1);
                                    string fileName = string.Join(".", alterFileName);
                                    fileName = fileName + "_" + HelperExtension.CreateRandomString(7) + "." + extention;
                                    FilePath = Path.Combine(FileRoot, fileName);
                                    using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        fs.Write(ByteFile);
                                    }
                                }
                                else
                                {
                                    using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        fs.Write(ByteFile);
                                    }
                                }
                            }
                            else
                            {
                                using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                {
                                    fs.Write(ByteFile);
                                }
                            }
                        }


                    }
                    if (FileRoot != null && FileRoot != "")
                    {
                        codeSepsis.AttachmentsFolderRoot = FileRoot.Replace(this._RootPath, "").Replace("\\", "/");
                    }
                }
                if (codeSepsis.Videos != null && codeSepsis.Videos.Count > 0)
                {
                    var RootPath = this._RootPath + "/Organizations";
                    string FileRoot = null;
                    List<string> Attachments = new();


                    FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeSepsis.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                    FileRoot = Path.Combine(RootPath, FileRoot);
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, UCLEnums.Sepsis.ToString());
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, row.CodeSepsisId.ToString());
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, "Videos");

                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    //else
                    //{
                    //    DirectoryInfo dir = new DirectoryInfo(FileRoot);
                    //    foreach (FileInfo fi in dir.GetFiles())
                    //    {
                    //        fi.Delete();
                    //    }
                    //}
                    foreach (var item in codeSepsis.Videos)
                    {
                        if (!string.IsNullOrEmpty(item.Base64Str))
                        {

                            var fileInfo = item.Base64Str.Split("base64,");
                            string fileExtension = fileInfo[0].GetFileExtenstion();
                            var ByteFile = Convert.FromBase64String(fileInfo[1]);
                            string FilePath = Path.Combine(FileRoot, item.FileName);

                            if (File.Exists(FilePath))
                            {
                                long existingFileSize = 0;
                                long newFileSize = ByteFile.LongLength;
                                FileInfo ExistingfileInfo = new FileInfo(FilePath);
                                existingFileSize = ExistingfileInfo.Length;

                                if (existingFileSize > 0 && newFileSize != existingFileSize)
                                {
                                    var alterFile = item.FileName.Split('.');
                                    string extention = alterFile.LastOrDefault();
                                    var alterFileName = alterFile.ToList();
                                    alterFileName.RemoveAt(alterFileName.Count - 1);
                                    string fileName = string.Join(".", alterFileName);
                                    fileName = fileName + "_" + HelperExtension.CreateRandomString(7) + "." + extention;
                                    FilePath = Path.Combine(FileRoot, fileName);
                                    using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        fs.Write(ByteFile);
                                    }
                                }
                                else
                                {
                                    using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        fs.Write(ByteFile);
                                    }
                                }
                            }
                            else
                            {
                                using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                {
                                    fs.Write(ByteFile);
                                }
                            }
                        }

                    }
                    if (FileRoot != null && FileRoot != "")
                    {
                        codeSepsis.VideoFolderRoot = FileRoot.Replace(this._RootPath, "").Replace("\\", "/");
                    }
                }
                if (codeSepsis.Audios != null && codeSepsis.Audios.Count > 0)
                {
                    var RootPath = this._RootPath + "/Organizations";
                    string FileRoot = null;
                    List<string> Attachments = new();
                    //var outPath = Directory.GetCurrentDirectory();

                    FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeSepsis.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                    FileRoot = Path.Combine(RootPath, FileRoot);
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, UCLEnums.Sepsis.ToString());
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, row.CodeSepsisId.ToString());
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, "Audios");

                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    //else
                    //{
                    //    DirectoryInfo dir = new DirectoryInfo(FileRoot);
                    //    foreach (FileInfo fi in dir.GetFiles())
                    //    {
                    //        fi.Delete();
                    //    }
                    //}
                    foreach (var item in codeSepsis.Audios)
                    {
                        if (!string.IsNullOrEmpty(item.Base64Str))
                        {

                            var fileInfo = item.Base64Str.Split("base64,");
                            string fileExtension = fileInfo[0].GetFileExtenstion();
                            var ByteFile = Convert.FromBase64String(fileInfo[1]);
                            string FilePath = Path.Combine(FileRoot, item.FileName);

                            if (File.Exists(FilePath))
                            {
                                long existingFileSize = 0;
                                long newFileSize = ByteFile.LongLength;
                                FileInfo ExistingfileInfo = new FileInfo(FilePath);
                                existingFileSize = ExistingfileInfo.Length;

                                if (existingFileSize > 0 && newFileSize != existingFileSize)
                                {
                                    var alterFile = item.FileName.Split('.');
                                    string extention = alterFile.LastOrDefault();
                                    var alterFileName = alterFile.ToList();
                                    alterFileName.RemoveAt(alterFileName.Count - 1);
                                    string fileName = string.Join(".", alterFileName);
                                    fileName = fileName + "_" + HelperExtension.CreateRandomString(7) + "." + extention;
                                    FilePath = Path.Combine(FileRoot, fileName);
                                    using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        fs.Write(ByteFile);
                                    }
                                }
                                else
                                {
                                    using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        fs.Write(ByteFile);
                                    }
                                }
                            }
                            else
                            {
                                using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                {
                                    fs.Write(ByteFile);
                                }
                            }
                        }


                    }
                    if (FileRoot != null && FileRoot != "")
                    {
                        codeSepsis.AudioFolderRoot = FileRoot.Replace(this._RootPath, "").Replace("\\", "/");
                    }
                }

                if (codeSepsis.AttachmentsFolderRoot != null)
                {
                    row.Attachments = codeSepsis.AttachmentsFolderRoot;
                }
                if (codeSepsis.VideoFolderRoot != null)
                {
                    row.Video = codeSepsis.VideoFolderRoot;
                }
                if (codeSepsis.AudioFolderRoot != null)
                {
                    row.Audio = codeSepsis.AudioFolderRoot;
                }



                this._codeSepsisRepo.Update(row);

                //var serviceLineIds = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == row.OrganizationIdFk && x.CodeIdFk == UCLEnums.Sepsis.ToInt() && !x.IsDeleted).Select(x => x.ServiceLineIds).FirstOrDefault();
                if (codeSepsis.SelectedServiceLineIds != null && codeSepsis.SelectedServiceLineIds != "")
                {
                    var serviceLineIds = codeSepsis.SelectedServiceLineIds.ToIntList();
                    var codeServiceMapping = this._codesServiceLinesMappingRepo.Table.Where(x => x.OrganizationIdFk == row.OrganizationIdFk && x.CodeIdFk == UCLEnums.Sepsis.ToInt() && x.ActiveCodeId == row.CodeSepsisId).ToList();
                    var rowsToDelete = codeServiceMapping.Where(x => !serviceLineIds.Contains(x.ServiceLineIdFk)).ToList();
                    if (rowsToDelete.Count > 0)
                        this._codesServiceLinesMappingRepo.DeleteRange(rowsToDelete);

                    serviceLineIds.RemoveAll(x => codeServiceMapping.Select(s => s.ServiceLineIdFk).Contains(x));
                    var codeServiceMappingList = new List<CodesServiceLinesMapping>();
                    foreach (var item in serviceLineIds)
                    {
                        var codeService = new CodesServiceLinesMapping()
                        {
                            OrganizationIdFk = row.OrganizationIdFk,
                            CodeIdFk = UCLEnums.Stroke.ToInt(),
                            ServiceLineIdFk = item,
                            ActiveCodeId = row.CodeSepsisId,
                            ActiveCodeName = UCLEnums.Stroke.ToString()
                        };
                        codeServiceMappingList.Add(codeService);
                    }
                    this._codesServiceLinesMappingRepo.Insert(codeServiceMappingList);


                    //var UserChannelSid = (from us in this._userSchedulesRepo.Table
                    //                      join u in this._userRepo.Table on us.UserIdFk equals u.UserId
                    //                      where serviceLineIds.Contains(us.ServiceLineIdFk.Value) && us.ScheduleDateStart <= DateTime.UtcNow && us.ScheduleDateEnd >= DateTime.UtcNow && !us.IsDeleted && !u.IsDeleted
                    //                      select u.UserChannelSid).ToList();

                    var UserChannelSid = (from u in this._userRepo.Table
                                          join gm in this._activeCodesGroupMembersRepo.Table on u.UserId equals gm.UserIdFk
                                          where gm.ActiveCodeIdFk == codeSepsis.CodeSepsisId && gm.ActiveCodeName == UCLEnums.Sepsis.ToString() && !u.IsDeleted
                                          select u.UserChannelSid).Distinct().ToList();

                    var notification = new PushNotificationVM()
                    {
                        Id = row.CodeSepsisId,
                        OrgId = row.OrganizationIdFk,
                        UserChannelSid = UserChannelSid,
                        From = AuthorEnums.Sepsis.ToString(),
                        Msg = (codeSepsis.IsEms ? "EMS" : "Active") + " Code Sepsis From is Changed",
                        RouteLink = "/Home/Activate%20Code/code-sepsis-form",
                        RouteLinkEMS = "/Home/EMS/activateCode"
                    };

                    _communication.pushNotification(notification);

                }

                return GetSepsisDataById(row.CodeSepsisId);
            }
            else
            {

                //var serviceLineIds = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == codeSepsis.OrganizationIdFk && x.CodeIdFk == UCLEnums.Sepsis.ToInt() && !x.IsDeleted).Select(x => x.ServiceLineIds).FirstOrDefault();
                if (codeSepsis.SelectedServiceLineIds != null && codeSepsis.SelectedServiceLineIds != "")
                {
                    var serviceLineIds = codeSepsis.SelectedServiceLineIds.ToIntList();
                    codeSepsis.CreatedDate = DateTime.UtcNow;
                    var Sepsis = AutoMapperHelper.MapSingleRow<CodeSepsisVM, CodeSepsi>(codeSepsis);

                    if (codeSepsis.Attachment != null && codeSepsis.Attachment.Count > 0)
                    {
                        var RootPath = this._RootPath + "/Organizations";
                        string FileRoot = null;
                        List<string> Attachments = new();


                        FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeSepsis.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                        FileRoot = Path.Combine(RootPath, FileRoot);
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, UCLEnums.Sepsis.ToString());
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, Sepsis.CodeSepsisId.ToString());
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, "Attachments");

                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        //else
                        //{
                        //    DirectoryInfo dir = new DirectoryInfo(FileRoot);
                        //    foreach (FileInfo fi in dir.GetFiles())
                        //    {
                        //        fi.Delete();
                        //    }
                        //}
                        foreach (var item in codeSepsis.Attachment)
                        {
                            if (!string.IsNullOrEmpty(item.Base64Str))
                            {

                                var fileInfo = item.Base64Str.Split("base64,");
                                string fileExtension = fileInfo[0].GetFileExtenstion();
                                var ByteFile = Convert.FromBase64String(fileInfo[1]);
                                string FilePath = Path.Combine(FileRoot, item.FileName);

                                if (File.Exists(FilePath))
                                {
                                    long existingFileSize = 0;
                                    long newFileSize = ByteFile.LongLength;
                                    FileInfo ExistingfileInfo = new FileInfo(FilePath);
                                    existingFileSize = ExistingfileInfo.Length;

                                    if (existingFileSize > 0 && newFileSize != existingFileSize)
                                    {
                                        var alterFile = item.FileName.Split('.');
                                        string extention = alterFile.LastOrDefault();
                                        var alterFileName = alterFile.ToList();
                                        alterFileName.RemoveAt(alterFileName.Count - 1);
                                        string fileName = string.Join(".", alterFileName);
                                        fileName = fileName + "_" + HelperExtension.CreateRandomString(7) + "." + extention;
                                        FilePath = Path.Combine(FileRoot, fileName);
                                        using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                        {
                                            fs.Write(ByteFile);
                                        }
                                    }
                                    else
                                    {
                                        using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                        {
                                            fs.Write(ByteFile);
                                        }
                                    }
                                }
                                else
                                {
                                    using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        fs.Write(ByteFile);
                                    }
                                }
                            }

                        }
                        if (FileRoot != null && FileRoot != "")
                        {
                            codeSepsis.AttachmentsFolderRoot = FileRoot.Replace(this._RootPath, "").Replace("\\", "/");
                        }
                    }
                    if (codeSepsis.Videos != null && codeSepsis.Videos.Count > 0)
                    {
                        var RootPath = this._RootPath + "/Organizations";
                        string FileRoot = null;
                        List<string> Attachments = new();


                        FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeSepsis.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                        FileRoot = Path.Combine(RootPath, FileRoot);
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, UCLEnums.Sepsis.ToString());
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, Sepsis.CodeSepsisId.ToString());
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, "Videos");

                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        //else
                        //{
                        //    DirectoryInfo dir = new DirectoryInfo(FileRoot);
                        //    foreach (FileInfo fi in dir.GetFiles())
                        //    {
                        //        fi.Delete();
                        //    }
                        //}
                        foreach (var item in codeSepsis.Videos)
                        {
                            if (!string.IsNullOrEmpty(item.Base64Str))
                            {

                                var fileInfo = item.Base64Str.Split("base64,");
                                string fileExtension = fileInfo[0].GetFileExtenstion();
                                var ByteFile = Convert.FromBase64String(fileInfo[1]);
                                string FilePath = Path.Combine(FileRoot, item.FileName);

                                if (File.Exists(FilePath))
                                {
                                    long existingFileSize = 0;
                                    long newFileSize = ByteFile.LongLength;
                                    FileInfo ExistingfileInfo = new FileInfo(FilePath);
                                    existingFileSize = ExistingfileInfo.Length;

                                    if (existingFileSize > 0 && newFileSize != existingFileSize)
                                    {
                                        var alterFile = item.FileName.Split('.');
                                        string extention = alterFile.LastOrDefault();
                                        var alterFileName = alterFile.ToList();
                                        alterFileName.RemoveAt(alterFileName.Count - 1);
                                        string fileName = string.Join(".", alterFileName);
                                        fileName = fileName + "_" + HelperExtension.CreateRandomString(7) + "." + extention;
                                        FilePath = Path.Combine(FileRoot, fileName);
                                        using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                        {
                                            fs.Write(ByteFile);
                                        }
                                    }
                                    else
                                    {
                                        using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                        {
                                            fs.Write(ByteFile);
                                        }
                                    }
                                }
                                else
                                {
                                    using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        fs.Write(ByteFile);
                                    }
                                }
                            }

                        }
                        if (FileRoot != null && FileRoot != "")
                        {
                            codeSepsis.VideoFolderRoot = FileRoot.Replace(this._RootPath, "").Replace("\\", "/");
                        }
                    }
                    if (codeSepsis.Audios != null && codeSepsis.Audios.Count > 0)
                    {
                        var RootPath = this._RootPath + "/Organizations";
                        string FileRoot = null;
                        List<string> Attachments = new();
                        //var outPath = Directory.GetCurrentDirectory();

                        FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeSepsis.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                        FileRoot = Path.Combine(RootPath, FileRoot);
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, UCLEnums.Sepsis.ToString());
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, Sepsis.CodeSepsisId.ToString());
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, "Audios");

                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        //else
                        //{
                        //    DirectoryInfo dir = new DirectoryInfo(FileRoot);
                        //    foreach (FileInfo fi in dir.GetFiles())
                        //    {
                        //        fi.Delete();
                        //    }
                        //}
                        foreach (var item in codeSepsis.Audios)
                        {
                            if (!string.IsNullOrEmpty(item.Base64Str))
                            {

                                var fileInfo = item.Base64Str.Split("base64,");
                                string fileExtension = fileInfo[0].GetFileExtenstion();
                                var ByteFile = Convert.FromBase64String(fileInfo[1]);
                                string FilePath = Path.Combine(FileRoot, item.FileName);

                                if (File.Exists(FilePath))
                                {
                                    long existingFileSize = 0;
                                    long newFileSize = ByteFile.LongLength;
                                    FileInfo ExistingfileInfo = new FileInfo(FilePath);
                                    existingFileSize = ExistingfileInfo.Length;

                                    if (existingFileSize > 0 && newFileSize != existingFileSize)
                                    {
                                        var alterFile = item.FileName.Split('.');
                                        string extention = alterFile.LastOrDefault();
                                        var alterFileName = alterFile.ToList();
                                        alterFileName.RemoveAt(alterFileName.Count - 1);
                                        string fileName = string.Join(".", alterFileName);
                                        fileName = fileName + "_" + HelperExtension.CreateRandomString(7) + "." + extention;
                                        FilePath = Path.Combine(FileRoot, fileName);
                                        using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                        {
                                            fs.Write(ByteFile);
                                        }
                                    }
                                    else
                                    {
                                        using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                        {
                                            fs.Write(ByteFile);
                                        }
                                    }
                                }
                                else
                                {
                                    using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        fs.Write(ByteFile);
                                    }
                                }
                            }

                        }
                        if (FileRoot != null && FileRoot != "")
                        {
                            codeSepsis.AudioFolderRoot = FileRoot.Replace(this._RootPath, "").Replace("\\", "/");
                        }
                    }

                    if (codeSepsis.AttachmentsFolderRoot != null)
                    {
                        Sepsis.Attachments = codeSepsis.AttachmentsFolderRoot;
                    }
                    if (codeSepsis.VideoFolderRoot != null)
                    {
                        Sepsis.Video = codeSepsis.VideoFolderRoot;
                    }
                    if (codeSepsis.AudioFolderRoot != null)
                    {
                        Sepsis.Audio = codeSepsis.AudioFolderRoot;
                    }
                    this._codeSepsisRepo.Insert(Sepsis);

                    var codeServiceMappingList = new List<CodesServiceLinesMapping>();
                    foreach (var item in serviceLineIds)
                    {
                        var codeServiceMapping = new CodesServiceLinesMapping()
                        {
                            OrganizationIdFk = Sepsis.OrganizationIdFk,
                            CodeIdFk = UCLEnums.Sepsis.ToInt(),
                            ServiceLineIdFk = item,
                            ActiveCodeId = Sepsis.CodeSepsisId,
                            ActiveCodeName = UCLEnums.Sepsis.ToString()
                        };
                        codeServiceMappingList.Add(codeServiceMapping);
                    }
                    this._codesServiceLinesMappingRepo.Insert(codeServiceMappingList);

                    var UserChannelSid = (from us in this._userSchedulesRepo.Table
                                          join u in this._userRepo.Table on us.UserIdFk equals u.UserId
                                          where serviceLineIds.Contains(us.ServiceLineIdFk.Value) && us.ScheduleDateStart <= DateTime.UtcNow && us.ScheduleDateEnd >= DateTime.UtcNow && !us.IsDeleted && !u.IsDeleted
                                          select new { u.UserUniqueId, u.UserId }).ToList();
                    var loggedUser = this._userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId }).FirstOrDefault();
                    UserChannelSid.Add(loggedUser);
                    List<ActiveCodesGroupMember> ACodeGroupMembers = new List<ActiveCodesGroupMember>();
                    if (UserChannelSid != null && UserChannelSid.Count > 0)
                    {
                        //string uniqueName = $"CONSULT_{Consult_Counter.Counter_Value.ToString()}";
                        string uniqueName = DateTime.Now.ToString("yyyyMMddHHmmssffff") + ApplicationSettings.UserId.ToString();
                        string friendlyName = Sepsis.IsEms ? $"EMS Code {UCLEnums.Sepsis.ToString()} {Sepsis.CodeSepsisId}" : $"Active Code {UCLEnums.Sepsis.ToString()} {Sepsis.CodeSepsisId}";
                        var conversationChannelAttributes = JsonConvert.SerializeObject(new Dictionary<string, Object>()
                                    {
                                        {ChannelAttributeEnums.ChannelType.ToString(), ChannelTypeEnums.EMS.ToString()},
                                        {ChannelAttributeEnums.CodeType.ToString(), ChannelTypeEnums.Sepsis.ToString()},
                                        {ChannelAttributeEnums.SepsisId.ToString(), Sepsis.CodeSepsisId}
                                    }, Formatting.Indented);
                        var channel = _communication.createConversationChannel(friendlyName, uniqueName, conversationChannelAttributes);
                        UserChannelSid = UserChannelSid.Distinct().ToList();
                        foreach (var item in UserChannelSid)
                        {
                            try
                            {
                                var codeGroupMember = new ActiveCodesGroupMember()
                                {
                                    UserIdFk = item.UserId,
                                    ActiveCodeIdFk = Sepsis.CodeSepsisId,
                                    ActiveCodeName = UCLEnums.Sepsis.ToString(),
                                    IsAcknowledge = false,
                                    CreatedBy = ApplicationSettings.UserId,
                                    CreatedDate = DateTime.UtcNow,
                                    IsDeleted = false
                                };
                                ACodeGroupMembers.Add(codeGroupMember);
                                _communication.addNewUserToConversationChannel(channel.Sid, item.UserUniqueId);
                            }
                            catch (Exception ex)
                            {
                                ElmahExtensions.RiseError(ex);
                            }
                        }
                        var isMembersAdded = AddGroupMembers(ACodeGroupMembers);

                        var msg = new ConversationMessageVM();
                        msg.channelSid = channel.Sid;
                        msg.author = "System";
                        msg.attributes = "";
                        msg.body = $"<strong> {(codeSepsis.IsEms ? "EMS Code" : "Active Code")} {UCLEnums.Sepsis.ToString()} </strong></br></br>";
                        if (codeSepsis.PatientName != null && codeSepsis.PatientName != "")
                            msg.body += $"<strong>Patient Name: </strong> {codeSepsis.PatientName} </br>";
                        msg.body += $"<strong>Dob: </strong> {codeSepsis.Dob:MM-dd-yyyy} </br>";
                        msg.body += $"<strong>Last Well Known: </strong> {codeSepsis.LastKnownWell:MM-dd-yyyy hh:mm tt} </br>";
                        if (codeSepsis.ChiefComplant != null && codeSepsis.ChiefComplant != "")
                            msg.body += $"<strong>Chief Complaint: </strong> {codeSepsis.ChiefComplant} </br>";
                        if (codeSepsis.Hpi != null && codeSepsis.Hpi != "")
                            msg.body += $"<strong>Hpi: </strong> {codeSepsis.Hpi} </br>";

                        var sendMsg = _communication.sendPushNotification(msg);
                    }
                    return GetSepsisDataById(Sepsis.CodeSepsisId);
                }
                return new BaseResponse() { Status = HttpStatusCode.NotAcceptable, Message = "There is no Service Line in this organization related to Code Sepsis" };
            }
        }

        public BaseResponse DeleteSepsis(int SepsisId)
        {
            var row = this._codeSepsisRepo.Table.Where(x => x.CodeSepsisId == SepsisId && !x.IsDeleted).FirstOrDefault();
            row.IsDeleted = true;
            row.ModifiedBy = ApplicationSettings.UserId;
            row.ModifiedDate = DateTime.UtcNow;
            this._codeSepsisRepo.Update(row);

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Deleted" };
        }


        #endregion

        #region Code STEMI

        public BaseResponse GetAllSTEMICode(ActiveCodeVM activeCode)
        {
            var STEMIData = new List<CodeStemi>();
            if (ApplicationSettings.isSuperAdmin)
            {
                STEMIData = this._codeSTEMIRepo.Table.Where(x => x.OrganizationIdFk == activeCode.OrganizationIdFk && !x.IsDeleted).OrderByDescending(x => x.CodeStemiid).ToList();
            }
            else if (activeCode.showAllActiveCodes)
            {
                STEMIData = this._codeSTEMIRepo.Table.Where(x => x.OrganizationIdFk == activeCode.OrganizationIdFk && !x.IsDeleted).OrderByDescending(x => x.CodeStemiid).ToList();
            }
            else
            {
                STEMIData = (from cs in this._codeSTEMIRepo.Table
                             join agm in this._activeCodesGroupMembersRepo.Table on cs.CodeStemiid equals agm.ActiveCodeIdFk
                             where agm.UserIdFk == ApplicationSettings.UserId && agm.ActiveCodeName == UCLEnums.STEMI.ToString() && !cs.IsDeleted
                             select cs).OrderByDescending(x => x.CodeStemiid).AsQueryable().ToList();


                //STEMIData = this._codeSTEMIRepo.Table.Where(x => x.CreatedBy == ApplicationSettings.UserId && !x.IsDeleted).OrderByDescending(x => x.CodeStemiid).ToList();
            }
            var STEMIDataVM = AutoMapperHelper.MapList<CodeStemi, CodeSTEMIVM>(STEMIData);

            //var orgData = GetHosplitalAddressObject(activeCode.OrganizationIdFk);

            STEMIDataVM.ForEach(x =>
            {
                x.AttachmentsPath = new List<string>();
                x.AudiosPath = new List<string>();
                x.VideosPath = new List<string>();
                x.BloodThinnersTitle = new List<object>();
                x.ServiceLines = new List<ServiceLineVM>();

                //if (!string.IsNullOrEmpty(x.Attachments) && !string.IsNullOrWhiteSpace(x.Attachments))
                //{
                //    string path = this._RootPath + x.Attachments;
                //    if (Directory.Exists(path))
                //    {
                //        DirectoryInfo AttachFiles = new DirectoryInfo(path);
                //        foreach (var item in AttachFiles.GetFiles())
                //        {
                //            x.AttachmentsPath.Add(x.Attachments + "/" + item.Name);
                //        }
                //    }
                //}

                //if (!string.IsNullOrEmpty(x.Audio) && !string.IsNullOrWhiteSpace(x.Audio))
                //{
                //    string path = this._RootPath + x.Audio;
                //    if (Directory.Exists(path))
                //    {
                //        DirectoryInfo AudioFiles = new DirectoryInfo(path);
                //        foreach (var item in AudioFiles.GetFiles())
                //        {
                //            x.AudiosPath.Add(x.Audio + "/" + item.Name);
                //        }
                //    }
                //}

                //if (!string.IsNullOrEmpty(x.Video) && !string.IsNullOrWhiteSpace(x.Video))
                //{
                //    var path = this._RootPath + x.Video;
                //    if (Directory.Exists(path))
                //    {
                //        DirectoryInfo VideoFiles = new DirectoryInfo(path);
                //        foreach (var item in VideoFiles.GetFiles())
                //        {
                //            x.VideosPath.Add(x.Video + "/" + item.Name);
                //        }
                //    }
                //}

                //var serviceIds = this._activeCodeRepo.Table.Where(s => s.OrganizationIdFk == x.OrganizationIdFk && s.CodeIdFk == UCLEnums.STEMI.ToInt() && !s.IsDeleted).Select(s => new { s.ServiceLineIds, s.DefaultServiceLineId }).FirstOrDefault();
                //var serviceLineIds = (from s in this._codesServiceLinesMappingRepo.Table
                //                      where s.OrganizationIdFk == x.OrganizationIdFk && s.CodeIdFk == UCLEnums.STEMI.ToInt()
                //                      && s.ActiveCodeId == x.CodeStemiid && s.ActiveCodeName == UCLEnums.STEMI.ToString() && s.ServiceLineIdFk != serviceIds.DefaultServiceLineId
                //                      select s.ServiceLineIdFk).ToList();
                //x.ServiceLines = this._serviceLineRepo.Table.Where(s => serviceIds.ServiceLineIds.ToIntList().Distinct().Contains(s.ServiceLineId) && s.ServiceLineId != serviceIds.DefaultServiceLineId && !s.IsDeleted).Select(s => new ServiceLineVM() { ServiceLineId = s.ServiceLineId, ServiceName = s.ServiceName, IsSelected = serviceLineIds.Contains(s.ServiceLineId) }).ToList();
                //x.DefaultServiceLineId = serviceIds.DefaultServiceLineId;
                //x.DefaultServiceLine = this._serviceLineRepo.Table.Where(s => s.ServiceLineId == serviceIds.DefaultServiceLineId && !s.IsDeleted).Select(s => new ServiceLineVM() { ServiceLineId = s.ServiceLineId, ServiceName = s.ServiceName }).FirstOrDefault();
                //x.SelectedServiceLineIds = string.Join(",", serviceLineIds);
                x.LastKnownWellStr = x.LastKnownWell?.ToString("yyyy-MM-dd hh:mm:ss tt");
                x.DobStr = x.Dob?.ToString("yyyy-MM-dd hh:mm:ss tt");
                //x.OrganizationData = orgData;
                x.GenderTitle = _controlListDetailsRepo.Table.Where(g => g.ControlListDetailId == x.Gender).Select(g => g.Title).FirstOrDefault();
                x.BloodThinnersTitle.AddRange(_controlListDetailsRepo.Table.Where(b => x.BloodThinners.ToIntList().Contains(b.ControlListDetailId)).Select(b => new { Id = b.ControlListDetailId, b.Title }).ToList());
            });
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Returned", Body = STEMIDataVM };
        }

        public BaseResponse GetSTEMIDataById(int STEMIId)
        {
            var STEMIData = this._codeSTEMIRepo.Table.Where(x => x.CodeStemiid == STEMIId && !x.IsDeleted).FirstOrDefault();
            if (STEMIData != null)
            {
                var STEMIDataVM = AutoMapperHelper.MapSingleRow<CodeStemi, CodeSTEMIVM>(STEMIData);
                STEMIDataVM.AttachmentsPath = new List<string>();
                STEMIDataVM.AudiosPath = new List<string>();
                STEMIDataVM.VideosPath = new List<string>();
                STEMIDataVM.BloodThinnersTitle = new List<object>();
                STEMIDataVM.ServiceLines = new List<ServiceLineVM>();

                if (!string.IsNullOrEmpty(STEMIDataVM.Attachments) && !string.IsNullOrWhiteSpace(STEMIDataVM.Attachments))
                {
                    string path = this._RootPath + STEMIDataVM.Attachments; //_environment.WebRootFileProvider.GetFileInfo(STEMIDataVM.Attachments)?.PhysicalPath;
                    if (Directory.Exists(path))
                    {
                        DirectoryInfo AttachFiles = new DirectoryInfo(path);
                        foreach (var item in AttachFiles.GetFiles())
                        {
                            STEMIDataVM.AttachmentsPath.Add(STEMIDataVM.Attachments + "/" + item.Name);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(STEMIDataVM.Audio) && !string.IsNullOrWhiteSpace(STEMIDataVM.Audio))
                {
                    string path = this._RootPath + STEMIDataVM.Audio;  //_environment.WebRootFileProvider.GetFileInfo(STEMIDataVM.Audio)?.PhysicalPath;
                    if (Directory.Exists(path))
                    {
                        DirectoryInfo AudioFiles = new DirectoryInfo(path);
                        foreach (var item in AudioFiles.GetFiles())
                        {
                            STEMIDataVM.AudiosPath.Add(STEMIDataVM.Audio + "/" + item.Name);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(STEMIDataVM.Video) && !string.IsNullOrWhiteSpace(STEMIDataVM.Video))
                {
                    var path = this._RootPath + STEMIDataVM.Video; //_environment.WebRootFileProvider.GetFileInfo(STEMIDataVM.Video)?.PhysicalPath; //.GetFileInfo(STEMIDataVM.Video);//?.PhysicalPath;
                    if (Directory.Exists(path))
                    {
                        DirectoryInfo VideoFiles = new DirectoryInfo(path);
                        foreach (var item in VideoFiles.GetFiles())
                        {
                            STEMIDataVM.VideosPath.Add(STEMIDataVM.Video + "/" + item.Name);
                        }
                    }
                }
                var serviceIds = this._activeCodeRepo.Table.Where(s => s.OrganizationIdFk == STEMIDataVM.OrganizationIdFk && s.CodeIdFk == UCLEnums.STEMI.ToInt() && !s.IsDeleted).Select(s => new { s.ServiceLineIds, s.DefaultServiceLineId }).FirstOrDefault();
                var serviceLineIds = (from s in this._codesServiceLinesMappingRepo.Table
                                      where s.OrganizationIdFk == STEMIDataVM.OrganizationIdFk && s.CodeIdFk == UCLEnums.STEMI.ToInt()
                                      && s.ActiveCodeId == STEMIDataVM.CodeStemiid && s.ActiveCodeName == UCLEnums.STEMI.ToString() && s.ServiceLineIdFk != serviceIds.DefaultServiceLineId
                                      select s.ServiceLineIdFk).ToList();
                STEMIDataVM.ServiceLines = this._serviceLineRepo.Table.Where(s => serviceIds.ServiceLineIds.ToIntList().Distinct().Contains(s.ServiceLineId) && s.ServiceLineId != serviceIds.DefaultServiceLineId && !s.IsDeleted).Select(s => new ServiceLineVM() { ServiceLineId = s.ServiceLineId, ServiceName = s.ServiceName, IsSelected = serviceLineIds.Contains(s.ServiceLineId) }).ToList();
                STEMIDataVM.DefaultServiceLineId = serviceIds.DefaultServiceLineId;
                STEMIDataVM.DefaultServiceLine = this._serviceLineRepo.Table.Where(s => s.ServiceLineId == serviceIds.DefaultServiceLineId && !s.IsDeleted).Select(s => new ServiceLineVM() { ServiceLineId = s.ServiceLineId, ServiceName = s.ServiceName }).FirstOrDefault();
                STEMIDataVM.SelectedServiceLineIds = string.Join(",", serviceLineIds);

                STEMIDataVM.LastKnownWellStr = STEMIDataVM.LastKnownWell?.ToString("yyyy-MM-dd hh:mm:ss tt");
                STEMIDataVM.DobStr = STEMIDataVM.Dob?.ToString("yyyy-MM-dd hh:mm:ss tt");
                STEMIDataVM.OrganizationData = GetHosplitalAddressObject(STEMIDataVM.OrganizationIdFk);
                STEMIDataVM.GenderTitle = _controlListDetailsRepo.Table.Where(g => g.ControlListDetailId == STEMIDataVM.Gender).Select(g => g.Title).FirstOrDefault();
                STEMIDataVM.BloodThinnersTitle.AddRange(_controlListDetailsRepo.Table.Where(b => STEMIDataVM.BloodThinners.ToIntList().Contains(b.ControlListDetailId)).Select(b => new { Id = b.ControlListDetailId, b.Title }).ToList());
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Found", Body = STEMIDataVM };
            }
            return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Record Not Found" };
        }

        public BaseResponse AddOrUpdateSTEMIData(CodeSTEMIVM codeSTEMI)
        {
            if (codeSTEMI != null && !string.IsNullOrEmpty(codeSTEMI.LastKnownWellStr) && !string.IsNullOrWhiteSpace(codeSTEMI.LastKnownWellStr))
            {
                codeSTEMI.LastKnownWell = DateTime.Parse(codeSTEMI.LastKnownWellStr);
            }
            if (codeSTEMI != null && !string.IsNullOrEmpty(codeSTEMI.DobStr) && !string.IsNullOrWhiteSpace(codeSTEMI.DobStr))
            {
                codeSTEMI.Dob = DateTime.Parse(codeSTEMI.DobStr);
            }
            if (codeSTEMI.CodeStemiid > 0)
            {
                var row = this._codeSTEMIRepo.Table.Where(x => x.CodeStemiid == codeSTEMI.CodeStemiid && !x.IsDeleted).FirstOrDefault();

                row.PatientName = codeSTEMI.PatientName;
                row.Dob = codeSTEMI.Dob;
                row.Gender = codeSTEMI.Gender;
                row.ChiefComplant = codeSTEMI.ChiefComplant;
                row.LastKnownWell = codeSTEMI.LastKnownWell;
                row.Hpi = codeSTEMI.Hpi;
                row.BloodThinners = codeSTEMI.BloodThinners;
                row.FamilyContactName = codeSTEMI.FamilyContactName;
                row.FamilyContactNumber = codeSTEMI.FamilyContactNumber;
                row.IsEms = codeSTEMI.IsEms;
                //row.IsCompleted = codeSTEMI.IsCompleted;
                if (codeSTEMI.IsCompleted != null && codeSTEMI.IsCompleted == true && row.IsCompleted != true)
                {
                    row.IsCompleted = true;
                    row.EndTime = DateTime.UtcNow;
                    row.ActualTime = row.EndTime - row.CreatedDate;
                }
                row.ModifiedBy = codeSTEMI.ModifiedBy;
                row.ModifiedDate = DateTime.UtcNow;
                row.IsDeleted = false;


                if (codeSTEMI.Attachment != null && codeSTEMI.Attachment.Count > 0)
                {
                    var RootPath = this._RootPath + "/Organizations";
                    string FileRoot = null;
                    List<string> Attachments = new();
                    //var outPath = Directory.GetCurrentDirectory();

                    FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeSTEMI.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                    FileRoot = Path.Combine(RootPath, FileRoot);
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, UCLEnums.STEMI.ToString());
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, row.CodeStemiid.ToString());
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, "Attachments");

                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    //else
                    //{
                    //    DirectoryInfo dir = new DirectoryInfo(FileRoot);
                    //    foreach (FileInfo fi in dir.GetFiles())
                    //    {
                    //        fi.Delete();
                    //    }
                    //}
                    foreach (var item in codeSTEMI.Attachment)
                    {
                        if (!string.IsNullOrEmpty(item.Base64Str))
                        {

                            var fileInfo = item.Base64Str.Split("base64,");
                            string fileExtension = fileInfo[0].GetFileExtenstion();
                            var ByteFile = Convert.FromBase64String(fileInfo[1]);
                            string FilePath = Path.Combine(FileRoot, item.FileName);

                            if (File.Exists(FilePath))
                            {
                                long existingFileSize = 0;
                                long newFileSize = ByteFile.LongLength;
                                FileInfo ExistingfileInfo = new FileInfo(FilePath);
                                existingFileSize = ExistingfileInfo.Length;

                                if (existingFileSize > 0 && newFileSize != existingFileSize)
                                {
                                    var alterFile = item.FileName.Split('.');
                                    string extention = alterFile.LastOrDefault();
                                    var alterFileName = alterFile.ToList();
                                    alterFileName.RemoveAt(alterFileName.Count - 1);
                                    string fileName = string.Join(".", alterFileName);
                                    fileName = fileName + "_" + HelperExtension.CreateRandomString(7) + "." + extention;
                                    FilePath = Path.Combine(FileRoot, fileName);
                                    using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        fs.Write(ByteFile);
                                    }
                                }
                                else
                                {
                                    using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        fs.Write(ByteFile);
                                    }
                                }
                            }
                            else
                            {
                                using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                {
                                    fs.Write(ByteFile);
                                }
                            }
                        }

                    }
                    if (FileRoot != null && FileRoot != "")
                    {
                        codeSTEMI.AttachmentsFolderRoot = FileRoot.Replace(this._RootPath, "").Replace("\\", "/");
                    }
                }
                if (codeSTEMI.Videos != null && codeSTEMI.Videos.Count > 0)
                {
                    var RootPath = this._RootPath + "/Organizations";
                    string FileRoot = null;
                    List<string> Attachments = new();
                    //var outPath = Directory.GetCurrentDirectory();

                    FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeSTEMI.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                    FileRoot = Path.Combine(RootPath, FileRoot);
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, UCLEnums.STEMI.ToString());
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, row.CodeStemiid.ToString());
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, "Videos");

                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    //else
                    //{
                    //    DirectoryInfo dir = new DirectoryInfo(FileRoot);
                    //    foreach (FileInfo fi in dir.GetFiles())
                    //    {
                    //        fi.Delete();
                    //    }
                    //}
                    foreach (var item in codeSTEMI.Videos)
                    {
                        if (!string.IsNullOrEmpty(item.Base64Str))
                        {

                            var fileInfo = item.Base64Str.Split("base64,");
                            string fileExtension = fileInfo[0].GetFileExtenstion();
                            var ByteFile = Convert.FromBase64String(fileInfo[1]);
                            string FilePath = Path.Combine(FileRoot, item.FileName);

                            if (File.Exists(FilePath))
                            {
                                long existingFileSize = 0;
                                long newFileSize = ByteFile.LongLength;
                                FileInfo ExistingfileInfo = new FileInfo(FilePath);
                                existingFileSize = ExistingfileInfo.Length;

                                if (existingFileSize > 0 && newFileSize != existingFileSize)
                                {
                                    var alterFile = item.FileName.Split('.');
                                    string extention = alterFile.LastOrDefault();
                                    var alterFileName = alterFile.ToList();
                                    alterFileName.RemoveAt(alterFileName.Count - 1);
                                    string fileName = string.Join(".", alterFileName);
                                    fileName = fileName + "_" + HelperExtension.CreateRandomString(7) + "." + extention;
                                    FilePath = Path.Combine(FileRoot, fileName);
                                    using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        fs.Write(ByteFile);
                                    }
                                }
                                else
                                {
                                    using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        fs.Write(ByteFile);
                                    }
                                }
                            }
                            else
                            {
                                using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                {
                                    fs.Write(ByteFile);
                                }
                            }
                        }

                    }
                    if (FileRoot != null && FileRoot != "")
                    {
                        codeSTEMI.VideoFolderRoot = FileRoot.Replace(this._RootPath, "").Replace("\\", "/");
                    }
                }
                if (codeSTEMI.Audios != null && codeSTEMI.Audios.Count > 0)
                {
                    var RootPath = this._RootPath + "/Organizations";
                    string FileRoot = null;
                    List<string> Attachments = new();
                    //var outPath = Directory.GetCurrentDirectory();

                    FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeSTEMI.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                    FileRoot = Path.Combine(RootPath, FileRoot);
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, UCLEnums.STEMI.ToString());
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, row.CodeStemiid.ToString());
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, "Audios");

                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    //else
                    //{
                    //    DirectoryInfo dir = new DirectoryInfo(FileRoot);
                    //    foreach (FileInfo fi in dir.GetFiles())
                    //    {
                    //        fi.Delete();
                    //    }
                    //}
                    foreach (var item in codeSTEMI.Audios)
                    {
                        if (!string.IsNullOrEmpty(item.Base64Str))
                        {

                            var fileInfo = item.Base64Str.Split("base64,");
                            string fileExtension = fileInfo[0].GetFileExtenstion();
                            var ByteFile = Convert.FromBase64String(fileInfo[1]);
                            string FilePath = Path.Combine(FileRoot, item.FileName);

                            if (File.Exists(FilePath))
                            {
                                long existingFileSize = 0;
                                long newFileSize = ByteFile.LongLength;
                                FileInfo ExistingfileInfo = new FileInfo(FilePath);
                                existingFileSize = ExistingfileInfo.Length;

                                if (existingFileSize > 0 && newFileSize != existingFileSize)
                                {
                                    var alterFile = item.FileName.Split('.');
                                    string extention = alterFile.LastOrDefault();
                                    var alterFileName = alterFile.ToList();
                                    alterFileName.RemoveAt(alterFileName.Count - 1);
                                    string fileName = string.Join(".", alterFileName);
                                    fileName = fileName + "_" + HelperExtension.CreateRandomString(7) + "." + extention;
                                    FilePath = Path.Combine(FileRoot, fileName);
                                    using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        fs.Write(ByteFile);
                                    }
                                }
                                else
                                {
                                    using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        fs.Write(ByteFile);
                                    }
                                }
                            }
                            else
                            {
                                using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                {
                                    fs.Write(ByteFile);
                                }
                            }
                        }

                    }
                    if (FileRoot != null && FileRoot != "")
                    {
                        codeSTEMI.AudioFolderRoot = FileRoot.Replace(this._RootPath, "").Replace("\\", "/");
                    }
                }

                if (codeSTEMI.AttachmentsFolderRoot != null)
                {
                    row.Attachments = codeSTEMI.AttachmentsFolderRoot;
                }
                if (codeSTEMI.VideoFolderRoot != null)
                {
                    row.Video = codeSTEMI.VideoFolderRoot;
                }
                if (codeSTEMI.AudioFolderRoot != null)
                {
                    row.Audio = codeSTEMI.AudioFolderRoot;
                }

                this._codeSTEMIRepo.Update(row);

                //var serviceLineIds = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == row.OrganizationIdFk && x.CodeIdFk == UCLEnums.STEMI.ToInt() && !x.IsDeleted).Select(x => x.ServiceLineIds).FirstOrDefault();
                if (codeSTEMI.SelectedServiceLineIds != null && codeSTEMI.SelectedServiceLineIds != "")
                {

                    var serviceLineIds = codeSTEMI.SelectedServiceLineIds.ToIntList();
                    var codeServiceMapping = this._codesServiceLinesMappingRepo.Table.Where(x => x.OrganizationIdFk == row.OrganizationIdFk && x.CodeIdFk == UCLEnums.STEMI.ToInt() && x.ActiveCodeId == row.CodeStemiid).ToList();
                    var rowsToDelete = codeServiceMapping.Where(x => !serviceLineIds.Contains(x.ServiceLineIdFk)).ToList();
                    if (rowsToDelete.Count > 0)
                        this._codesServiceLinesMappingRepo.DeleteRange(rowsToDelete);

                    serviceLineIds.RemoveAll(x => codeServiceMapping.Select(s => s.ServiceLineIdFk).Contains(x));
                    var codeServiceMappingList = new List<CodesServiceLinesMapping>();
                    foreach (var item in serviceLineIds)
                    {
                        var codeService = new CodesServiceLinesMapping()
                        {
                            OrganizationIdFk = row.OrganizationIdFk,
                            CodeIdFk = UCLEnums.STEMI.ToInt(),
                            ServiceLineIdFk = item,
                            ActiveCodeId = row.CodeStemiid,
                            ActiveCodeName = UCLEnums.STEMI.ToString()
                        };
                        codeServiceMappingList.Add(codeService);
                    }
                    this._codesServiceLinesMappingRepo.Insert(codeServiceMappingList);

                    //var UserChannelSid = (from us in this._userSchedulesRepo.Table
                    //                      join u in this._userRepo.Table on us.UserIdFk equals u.UserId
                    //                      where serviceLineIds.Contains(us.ServiceLineIdFk.Value) && us.ScheduleDateStart <= DateTime.UtcNow && us.ScheduleDateEnd >= DateTime.UtcNow && !us.IsDeleted && !u.IsDeleted
                    //                      select u.UserChannelSid).ToList();

                    var UserChannelSid = (from u in this._userRepo.Table
                                          join gm in this._activeCodesGroupMembersRepo.Table on u.UserId equals gm.UserIdFk
                                          where gm.ActiveCodeIdFk == codeSTEMI.CodeStemiid && gm.ActiveCodeName == UCLEnums.STEMI.ToString() && !u.IsDeleted
                                          select u.UserChannelSid).Distinct().ToList();

                    var notification = new PushNotificationVM()
                    {
                        Id = row.CodeStemiid,
                        OrgId = row.OrganizationIdFk,
                        UserChannelSid = UserChannelSid,
                        From = AuthorEnums.STEMI.ToString(),
                        Msg = (codeSTEMI.IsEms.HasValue && codeSTEMI.IsEms.Value ? "EMS" : "Active") + " Code STEMI From is Changed",
                        RouteLink = "/Home/Activate%20Code/code-STEMI-form",
                        RouteLinkEMS = "/Home/EMS/activateCode"
                    };

                    _communication.pushNotification(notification);

                }

                //codeSTEMI.AttachmentsPath = new List<string>();
                //codeSTEMI.AudiosPath = new List<string>();
                //codeSTEMI.VideosPath = new List<string>();

                //if (!string.IsNullOrEmpty(codeSTEMI.AttachmentsFolderRoot) && !string.IsNullOrWhiteSpace(codeSTEMI.AttachmentsFolderRoot))
                //{
                //    string path = this._RootPath + codeSTEMI.AttachmentsFolderRoot;  //_environment.WebRootFileProvider.GetFileInfo(codeSTEMI.AttachmentsFolderRoot)?.PhysicalPath;
                //    if (Directory.Exists(path))
                //    {
                //        DirectoryInfo AttachFiles = new DirectoryInfo(path);
                //        foreach (var item in AttachFiles.GetFiles())
                //        {
                //            codeSTEMI.AttachmentsPath.Add(codeSTEMI.AttachmentsFolderRoot + "/" + item.Name);
                //        }
                //    }
                //}

                //if (!string.IsNullOrEmpty(codeSTEMI.AudioFolderRoot) && !string.IsNullOrWhiteSpace(codeSTEMI.AudioFolderRoot))
                //{
                //    string path = this._RootPath + codeSTEMI.AudioFolderRoot; //_environment.WebRootFileProvider.GetFileInfo(codeSTEMI.AudioFolderRoot)?.PhysicalPath;
                //    if (Directory.Exists(path))
                //    {
                //        DirectoryInfo AudioFiles = new DirectoryInfo(path);
                //        foreach (var item in AudioFiles.GetFiles())
                //        {
                //            codeSTEMI.AudiosPath.Add(codeSTEMI.AudioFolderRoot + "/" + item.Name);
                //        }
                //    }
                //}

                //if (!string.IsNullOrEmpty(codeSTEMI.VideoFolderRoot) && !string.IsNullOrWhiteSpace(codeSTEMI.VideoFolderRoot))
                //{
                //    var path = this._RootPath + codeSTEMI.VideoFolderRoot; //_environment.WebRootFileProvider.GetFileInfo(codeSTEMI.VideoFolderRoot)?.PhysicalPath;
                //    if (Directory.Exists(path))
                //    {
                //        DirectoryInfo VideoFiles = new DirectoryInfo(path);
                //        foreach (var item in VideoFiles.GetFiles())
                //        {
                //            codeSTEMI.VideosPath.Add(codeSTEMI.VideoFolderRoot + "/" + item.Name);
                //        }
                //    }
                //}




                return GetSTEMIDataById(row.CodeStemiid);
            }
            else
            {
                //var serviceLineIds = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == codeSTEMI.OrganizationIdFk && x.CodeIdFk == UCLEnums.STEMI.ToInt() && !x.IsDeleted).Select(x => x.ServiceLineIds).FirstOrDefault();
                if (codeSTEMI.SelectedServiceLineIds != null && codeSTEMI.SelectedServiceLineIds != "")
                {
                    var serviceLineIds = codeSTEMI.SelectedServiceLineIds.ToIntList();

                    codeSTEMI.CreatedDate = DateTime.UtcNow;
                    var STEMI = AutoMapperHelper.MapSingleRow<CodeSTEMIVM, CodeStemi>(codeSTEMI);

                    if (codeSTEMI.Attachment != null && codeSTEMI.Attachment.Count > 0)
                    {
                        var RootPath = this._RootPath + "/Organizations";
                        string FileRoot = null;
                        List<string> Attachments = new();
                        //var outPath = Directory.GetCurrentDirectory();

                        FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeSTEMI.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                        FileRoot = Path.Combine(RootPath, FileRoot);
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, UCLEnums.STEMI.ToString());
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, STEMI.CodeStemiid.ToString());
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, "Attachments");

                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        //else
                        //{
                        //    DirectoryInfo dir = new DirectoryInfo(FileRoot);
                        //    foreach (FileInfo fi in dir.GetFiles())
                        //    {
                        //        fi.Delete();
                        //    }
                        //}
                        foreach (var item in codeSTEMI.Attachment)
                        {
                            if (!string.IsNullOrEmpty(item.Base64Str))
                            {

                                var fileInfo = item.Base64Str.Split("base64,");
                                string fileExtension = fileInfo[0].GetFileExtenstion();
                                var ByteFile = Convert.FromBase64String(fileInfo[1]);
                                string FilePath = Path.Combine(FileRoot, item.FileName);

                                if (File.Exists(FilePath))
                                {
                                    long existingFileSize = 0;
                                    long newFileSize = ByteFile.LongLength;
                                    FileInfo ExistingfileInfo = new FileInfo(FilePath);
                                    existingFileSize = ExistingfileInfo.Length;

                                    if (existingFileSize > 0 && newFileSize != existingFileSize)
                                    {
                                        var alterFile = item.FileName.Split('.');
                                        string extention = alterFile.LastOrDefault();
                                        var alterFileName = alterFile.ToList();
                                        alterFileName.RemoveAt(alterFileName.Count - 1);
                                        string fileName = string.Join(".", alterFileName);
                                        fileName = fileName + "_" + HelperExtension.CreateRandomString(7) + "." + extention;
                                        FilePath = Path.Combine(FileRoot, fileName);
                                        using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                        {
                                            fs.Write(ByteFile);
                                        }
                                    }
                                    else
                                    {
                                        using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                        {
                                            fs.Write(ByteFile);
                                        }
                                    }
                                }
                                else
                                {
                                    using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        fs.Write(ByteFile);
                                    }
                                }
                            }

                        }
                        if (FileRoot != null && FileRoot != "")
                        {
                            codeSTEMI.AttachmentsFolderRoot = FileRoot.Replace(this._RootPath, "").Replace("\\", "/");
                        }
                    }
                    if (codeSTEMI.Videos != null && codeSTEMI.Videos.Count > 0)
                    {
                        var RootPath = this._RootPath + "/Organizations";
                        string FileRoot = null;
                        List<string> Attachments = new();
                        //var outPath = Directory.GetCurrentDirectory();

                        FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeSTEMI.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                        FileRoot = Path.Combine(RootPath, FileRoot);
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, UCLEnums.STEMI.ToString());
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, STEMI.CodeStemiid.ToString());
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, "Videos");

                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        //else
                        //{
                        //    DirectoryInfo dir = new DirectoryInfo(FileRoot);
                        //    foreach (FileInfo fi in dir.GetFiles())
                        //    {
                        //        fi.Delete();
                        //    }
                        //}
                        foreach (var item in codeSTEMI.Videos)
                        {
                            if (!string.IsNullOrEmpty(item.Base64Str))
                            {

                                var fileInfo = item.Base64Str.Split("base64,");
                                string fileExtension = fileInfo[0].GetFileExtenstion();
                                var ByteFile = Convert.FromBase64String(fileInfo[1]);
                                string FilePath = Path.Combine(FileRoot, item.FileName);

                                if (File.Exists(FilePath))
                                {
                                    long existingFileSize = 0;
                                    long newFileSize = ByteFile.LongLength;
                                    FileInfo ExistingfileInfo = new FileInfo(FilePath);
                                    existingFileSize = ExistingfileInfo.Length;

                                    if (existingFileSize > 0 && newFileSize != existingFileSize)
                                    {
                                        var alterFile = item.FileName.Split('.');
                                        string extention = alterFile.LastOrDefault();
                                        var alterFileName = alterFile.ToList();
                                        alterFileName.RemoveAt(alterFileName.Count - 1);
                                        string fileName = string.Join(".", alterFileName);
                                        fileName = fileName + "_" + HelperExtension.CreateRandomString(7) + "." + extention;
                                        FilePath = Path.Combine(FileRoot, fileName);
                                        using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                        {
                                            fs.Write(ByteFile);
                                        }
                                    }
                                    else
                                    {
                                        using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                        {
                                            fs.Write(ByteFile);
                                        }
                                    }
                                }
                                else
                                {
                                    using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        fs.Write(ByteFile);
                                    }
                                }
                            }

                        }
                        if (FileRoot != null && FileRoot != "")
                        {
                            codeSTEMI.VideoFolderRoot = FileRoot.Replace(this._RootPath, "").Replace("\\", "/");
                        }
                    }
                    if (codeSTEMI.Audios != null && codeSTEMI.Audios.Count > 0)
                    {
                        var RootPath = this._RootPath + "/Organizations";
                        string FileRoot = null;
                        List<string> Attachments = new();

                        //var outPath = Directory.GetCurrentDirectory();

                        FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeSTEMI.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                        FileRoot = Path.Combine(RootPath, FileRoot);
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, UCLEnums.STEMI.ToString());
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, STEMI.CodeStemiid.ToString());
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, "Audios");

                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        //else
                        //{
                        //    DirectoryInfo dir = new DirectoryInfo(FileRoot);
                        //    foreach (FileInfo fi in dir.GetFiles())
                        //    {
                        //        fi.Delete();
                        //    }
                        //}
                        foreach (var item in codeSTEMI.Audios)
                        {
                            if (!string.IsNullOrEmpty(item.Base64Str))
                            {

                                var fileInfo = item.Base64Str.Split("base64,");
                                string fileExtension = fileInfo[0].GetFileExtenstion();
                                var ByteFile = Convert.FromBase64String(fileInfo[1]);
                                string FilePath = Path.Combine(FileRoot, item.FileName);

                                if (File.Exists(FilePath))
                                {
                                    long existingFileSize = 0;
                                    long newFileSize = ByteFile.LongLength;
                                    FileInfo ExistingfileInfo = new FileInfo(FilePath);
                                    existingFileSize = ExistingfileInfo.Length;

                                    if (existingFileSize > 0 && newFileSize != existingFileSize)
                                    {
                                        var alterFile = item.FileName.Split('.');
                                        string extention = alterFile.LastOrDefault();
                                        var alterFileName = alterFile.ToList();
                                        alterFileName.RemoveAt(alterFileName.Count - 1);
                                        string fileName = string.Join(".", alterFileName);
                                        fileName = fileName + "_" + HelperExtension.CreateRandomString(7) + "." + extention;
                                        FilePath = Path.Combine(FileRoot, fileName);
                                        using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                        {
                                            fs.Write(ByteFile);
                                        }
                                    }
                                    else
                                    {
                                        using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                        {
                                            fs.Write(ByteFile);
                                        }
                                    }
                                }
                                else
                                {
                                    using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        fs.Write(ByteFile);
                                    }
                                }
                            }

                        }
                        if (FileRoot != null && FileRoot != "")
                        {
                            codeSTEMI.AudioFolderRoot = FileRoot.Replace(this._RootPath, "").Replace("\\", "/");
                        }
                    }
                    if (codeSTEMI.AttachmentsFolderRoot != null)
                    {
                        STEMI.Attachments = codeSTEMI.AttachmentsFolderRoot;
                    }
                    if (codeSTEMI.VideoFolderRoot != null)
                    {
                        STEMI.Video = codeSTEMI.VideoFolderRoot;
                    }
                    if (codeSTEMI.AudioFolderRoot != null)
                    {
                        STEMI.Audio = codeSTEMI.AudioFolderRoot;
                    }

                    this._codeSTEMIRepo.Insert(STEMI);

                    var codeServiceMappingList = new List<CodesServiceLinesMapping>();
                    foreach (var item in serviceLineIds)
                    {
                        var codeServiceMapping = new CodesServiceLinesMapping()
                        {
                            OrganizationIdFk = STEMI.OrganizationIdFk,
                            CodeIdFk = UCLEnums.STEMI.ToInt(),
                            ServiceLineIdFk = item,
                            ActiveCodeId = STEMI.CodeStemiid,
                            ActiveCodeName = UCLEnums.STEMI.ToString()
                        };
                        codeServiceMappingList.Add(codeServiceMapping);
                    }
                    this._codesServiceLinesMappingRepo.Insert(codeServiceMappingList);

                    var UserChannelSid = (from us in this._userSchedulesRepo.Table
                                          join u in this._userRepo.Table on us.UserIdFk equals u.UserId
                                          where serviceLineIds.Contains(us.ServiceLineIdFk.Value) && us.ScheduleDateStart <= DateTime.UtcNow && us.ScheduleDateEnd >= DateTime.UtcNow && !us.IsDeleted && !u.IsDeleted
                                          select new { u.UserUniqueId, u.UserId }).ToList();
                    var loggedUser = this._userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId }).FirstOrDefault();
                    UserChannelSid.Add(loggedUser);
                    List<ActiveCodesGroupMember> ACodeGroupMembers = new List<ActiveCodesGroupMember>();
                    if (UserChannelSid != null && UserChannelSid.Count > 0)
                    {
                        //string uniqueName = $"CONSULT_{Consult_Counter.Counter_Value.ToString()}";
                        string uniqueName = DateTime.Now.ToString("yyyyMMddHHmmssffff") + ApplicationSettings.UserId.ToString();
                        string friendlyName = STEMI.IsEms.HasValue && STEMI.IsEms.Value ? $"EMS Code {UCLEnums.STEMI.ToString()} {STEMI.CodeStemiid}" : $"Active Code {UCLEnums.STEMI.ToString()} {STEMI.CodeStemiid}";
                        var conversationChannelAttributes = JsonConvert.SerializeObject(new Dictionary<string, Object>()
                                    {
                                        {ChannelAttributeEnums.ChannelType.ToString(), ChannelTypeEnums.EMS.ToString()},
                                        {ChannelAttributeEnums.CodeType.ToString(), ChannelTypeEnums.STEMI.ToString()},
                                        {ChannelAttributeEnums.STEMIId.ToString(), STEMI.CodeStemiid}
                                    }, Formatting.Indented);
                        var channel = _communication.createConversationChannel(friendlyName, uniqueName, conversationChannelAttributes);

                        UserChannelSid = UserChannelSid.Distinct().ToList();
                        foreach (var item in UserChannelSid)
                        {
                            try
                            {
                                var codeGroupMember = new ActiveCodesGroupMember()
                                {
                                    UserIdFk = item.UserId,
                                    ActiveCodeIdFk = STEMI.CodeStemiid,
                                    ActiveCodeName = UCLEnums.STEMI.ToString(),
                                    IsAcknowledge = false,
                                    CreatedBy = ApplicationSettings.UserId,
                                    CreatedDate = DateTime.UtcNow,
                                    IsDeleted = false
                                };
                                ACodeGroupMembers.Add(codeGroupMember);
                                _communication.addNewUserToConversationChannel(channel.Sid, item.UserUniqueId);
                            }
                            catch (Exception ex)
                            {
                                ElmahExtensions.RiseError(ex);
                            }
                        }
                        var isMembersAdded = AddGroupMembers(ACodeGroupMembers);
                        var msg = new ConversationMessageVM();
                        msg.channelSid = channel.Sid;
                        msg.author = "System";
                        msg.attributes = "";
                        msg.body = $"<strong> {(codeSTEMI.IsEms.HasValue && codeSTEMI.IsEms.Value ? "EMS Code" : "Active Code")} {UCLEnums.STEMI.ToString()} </strong></br></br>";
                        if (codeSTEMI.PatientName != null && codeSTEMI.PatientName != "")
                            msg.body += $"<strong>Patient Name: </strong> {codeSTEMI.PatientName} </br>";
                        msg.body += $"<strong>Dob: </strong> {codeSTEMI.Dob:MM-dd-yyyy} </br>";
                        msg.body += $"<strong>Last Well Known: </strong> {codeSTEMI.LastKnownWell:MM-dd-yyyy hh:mm tt} </br>";
                        if (codeSTEMI.ChiefComplant != null && codeSTEMI.ChiefComplant != "")
                            msg.body += $"<strong>Chief Complaint: </strong> {codeSTEMI.ChiefComplant} </br>";
                        if (codeSTEMI.Hpi != null && codeSTEMI.Hpi != "")
                            msg.body += $"<strong>Hpi: </strong> {codeSTEMI.Hpi} </br>";
                        var sendMsg = _communication.sendPushNotification(msg);
                    }
                    return GetSTEMIDataById(STEMI.CodeStemiid);
                }
                return new BaseResponse() { Status = HttpStatusCode.NotAcceptable, Message = "There is no Service Line in this organization related to Code STEMI" };
            }
        }

        public BaseResponse DeleteSTEMI(int STEMIId)
        {
            var row = this._codeSTEMIRepo.Table.Where(x => x.CodeStemiid == STEMIId && !x.IsDeleted).FirstOrDefault();
            row.IsDeleted = true;
            row.ModifiedBy = ApplicationSettings.UserId;
            row.ModifiedDate = DateTime.UtcNow;
            this._codeSTEMIRepo.Update(row);

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Deleted" };
        }


        #endregion

        #region Code Truma

        public BaseResponse GetAllTrumaCode(ActiveCodeVM activeCode)
        {
            var TrumaData = new List<CodeTrauma>();
            if (ApplicationSettings.isSuperAdmin)
            {
                TrumaData = this._codeTrumaRepo.Table.Where(x => x.OrganizationIdFk == activeCode.OrganizationIdFk && !x.IsDeleted).OrderByDescending(x => x.CodeTraumaId).ToList();
            }
            else if (activeCode.showAllActiveCodes)
            {
                TrumaData = this._codeTrumaRepo.Table.Where(x => x.OrganizationIdFk == activeCode.OrganizationIdFk && !x.IsDeleted).OrderByDescending(x => x.CodeTraumaId).ToList();
            }
            else
            {
                TrumaData = (from cs in this._codeTrumaRepo.Table
                             join agm in this._activeCodesGroupMembersRepo.Table on cs.CodeTraumaId equals agm.ActiveCodeIdFk
                             where agm.UserIdFk == ApplicationSettings.UserId && agm.ActiveCodeName == UCLEnums.Trauma.ToString() && !cs.IsDeleted
                             select cs).OrderByDescending(x => x.CodeTraumaId).AsQueryable().ToList();

                //TrumaData = this._codeTrumaRepo.Table.Where(x => x.CreatedBy == ApplicationSettings.UserId && !x.IsDeleted).OrderByDescending(x => x.CodeTraumaId).ToList();
            }

            var TrumaDataVM = AutoMapperHelper.MapList<CodeTrauma, CodeTrumaVM>(TrumaData);

            //var orgData = GetHosplitalAddressObject(activeCode.OrganizationIdFk);

            TrumaDataVM.ForEach(x =>
            {
                x.AttachmentsPath = new List<string>();
                x.AudiosPath = new List<string>();
                x.VideosPath = new List<string>();
                x.BloodThinnersTitle = new List<object>();
                //if (!string.IsNullOrEmpty(x.Attachments) && !string.IsNullOrWhiteSpace(x.Attachments))
                //{
                //    string path = this._RootPath + x.Attachments;
                //    if (Directory.Exists(path))
                //    {
                //        DirectoryInfo AttachFiles = new DirectoryInfo(path);
                //        foreach (var item in AttachFiles.GetFiles())
                //        {
                //            x.AttachmentsPath.Add(x.Attachments + "/" + item.Name);
                //        }
                //    }
                //}

                //if (!string.IsNullOrEmpty(x.Audio) && !string.IsNullOrWhiteSpace(x.Audio))
                //{
                //    string path = this._RootPath + x.Audio;
                //    if (Directory.Exists(path))
                //    {
                //        DirectoryInfo AudioFiles = new DirectoryInfo(path);
                //        foreach (var item in AudioFiles.GetFiles())
                //        {
                //            x.AudiosPath.Add(x.Audio + "/" + item.Name);
                //        }
                //    }
                //}

                //if (!string.IsNullOrEmpty(x.Video) && !string.IsNullOrWhiteSpace(x.Video))
                //{
                //    var path = this._RootPath + x.Video;
                //    if (Directory.Exists(path))
                //    {
                //        DirectoryInfo VideoFiles = new DirectoryInfo(path);
                //        foreach (var item in VideoFiles.GetFiles())
                //        {
                //            x.VideosPath.Add(x.Video + "/" + item.Name);
                //        }
                //    }
                //}

                //var serviceIds = this._activeCodeRepo.Table.Where(s => s.OrganizationIdFk == x.OrganizationIdFk && s.CodeIdFk == UCLEnums.Trauma.ToInt() && !s.IsDeleted).Select(s => new { s.ServiceLineIds, s.DefaultServiceLineId }).FirstOrDefault();
                //var serviceLineIds = (from s in this._codesServiceLinesMappingRepo.Table
                //                      where s.OrganizationIdFk == x.OrganizationIdFk && s.CodeIdFk == UCLEnums.Trauma.ToInt()
                //                      && s.ActiveCodeId == x.CodeTraumaId && s.ActiveCodeName == UCLEnums.Trauma.ToString() && s.ServiceLineIdFk != serviceIds.DefaultServiceLineId
                //                      select s.ServiceLineIdFk).ToList();
                //x.ServiceLines = this._serviceLineRepo.Table.Where(s => serviceIds.ServiceLineIds.ToIntList().Distinct().Contains(s.ServiceLineId) && s.ServiceLineId != serviceIds.DefaultServiceLineId && !s.IsDeleted).Select(s => new ServiceLineVM() { ServiceLineId = s.ServiceLineId, ServiceName = s.ServiceName, IsSelected = serviceLineIds.Contains(s.ServiceLineId) }).ToList();
                //x.DefaultServiceLineId = serviceIds.DefaultServiceLineId;
                //x.DefaultServiceLine = this._serviceLineRepo.Table.Where(s => s.ServiceLineId == serviceIds.DefaultServiceLineId && !s.IsDeleted).Select(s => new ServiceLineVM() { ServiceLineId = s.ServiceLineId, ServiceName = s.ServiceName }).FirstOrDefault();
                //x.SelectedServiceLineIds = string.Join(",", serviceLineIds);

                x.LastKnownWellStr = x.LastKnownWell?.ToString("yyyy-MM-dd hh:mm:ss tt");
                x.DobStr = x.Dob?.ToString("yyyy-MM-dd hh:mm:ss tt");
                //x.OrganizationData = orgData;
                x.GenderTitle = _controlListDetailsRepo.Table.Where(g => g.ControlListDetailId == x.Gender).Select(g => g.Title).FirstOrDefault();
                x.BloodThinnersTitle.AddRange(_controlListDetailsRepo.Table.Where(b => x.BloodThinners.ToIntList().Contains(b.ControlListDetailId)).Select(b => new { Id = b.ControlListDetailId, b.Title }).ToList());
            });
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Returned", Body = TrumaDataVM };
        }

        public BaseResponse GetTrumaDataById(int TrumaId)
        {
            var TrumaData = this._codeTrumaRepo.Table.Where(x => x.CodeTraumaId == TrumaId && !x.IsDeleted).FirstOrDefault();
            if (TrumaData != null)
            {
                var TrumaDataVM = AutoMapperHelper.MapSingleRow<CodeTrauma, CodeTrumaVM>(TrumaData);
                TrumaDataVM.AttachmentsPath = new List<string>();
                TrumaDataVM.AudiosPath = new List<string>();
                TrumaDataVM.VideosPath = new List<string>();
                TrumaDataVM.BloodThinnersTitle = new List<object>();

                if (!string.IsNullOrEmpty(TrumaDataVM.Attachments) && !string.IsNullOrWhiteSpace(TrumaDataVM.Attachments))
                {
                    string path = this._RootPath + TrumaDataVM.Attachments; //_environment.WebRootFileProvider.GetFileInfo(TrumaDataVM.Attachments)?.PhysicalPath;
                    if (Directory.Exists(path))
                    {
                        DirectoryInfo AttachFiles = new DirectoryInfo(path);
                        foreach (var item in AttachFiles.GetFiles())
                        {
                            TrumaDataVM.AttachmentsPath.Add(TrumaDataVM.Attachments + "/" + item.Name);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(TrumaDataVM.Audio) && !string.IsNullOrWhiteSpace(TrumaDataVM.Audio))
                {
                    string path = this._RootPath + TrumaDataVM.Audio; //_environment.WebRootFileProvider.GetFileInfo(TrumaDataVM.Audio)?.PhysicalPath;
                    if (Directory.Exists(path))
                    {
                        DirectoryInfo AudioFiles = new DirectoryInfo(path);
                        foreach (var item in AudioFiles.GetFiles())
                        {
                            TrumaDataVM.AudiosPath.Add(TrumaDataVM.Audio + "/" + item.Name);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(TrumaDataVM.Video) && !string.IsNullOrWhiteSpace(TrumaDataVM.Video))
                {
                    var path = this._RootPath + TrumaDataVM.Video; //_environment.WebRootFileProvider.GetFileInfo(TrumaDataVM.Video)?.PhysicalPath; //.GetFileInfo(TrumaDataVM.Video);//?.PhysicalPath;
                    if (Directory.Exists(path))
                    {
                        DirectoryInfo VideoFiles = new DirectoryInfo(path);
                        foreach (var item in VideoFiles.GetFiles())
                        {
                            TrumaDataVM.VideosPath.Add(TrumaDataVM.Video + "/" + item.Name);
                        }
                    }
                }
                var serviceIds = this._activeCodeRepo.Table.Where(s => s.OrganizationIdFk == TrumaDataVM.OrganizationIdFk && s.CodeIdFk == UCLEnums.Trauma.ToInt() && !s.IsDeleted).Select(s => new { s.ServiceLineIds, s.DefaultServiceLineId }).FirstOrDefault();
                var serviceLineIds = (from s in this._codesServiceLinesMappingRepo.Table
                                      where s.OrganizationIdFk == TrumaDataVM.OrganizationIdFk && s.CodeIdFk == UCLEnums.Trauma.ToInt()
                                      && s.ActiveCodeId == TrumaDataVM.CodeTraumaId && s.ActiveCodeName == UCLEnums.Trauma.ToString() && s.ServiceLineIdFk != serviceIds.DefaultServiceLineId
                                      select s.ServiceLineIdFk).ToList();
                TrumaDataVM.ServiceLines = this._serviceLineRepo.Table.Where(s => serviceIds.ServiceLineIds.ToIntList().Distinct().Contains(s.ServiceLineId) && s.ServiceLineId != serviceIds.DefaultServiceLineId && !s.IsDeleted).Select(s => new ServiceLineVM() { ServiceLineId = s.ServiceLineId, ServiceName = s.ServiceName, IsSelected = serviceLineIds.Contains(s.ServiceLineId) }).ToList();
                TrumaDataVM.DefaultServiceLineId = serviceIds.DefaultServiceLineId;
                TrumaDataVM.DefaultServiceLine = this._serviceLineRepo.Table.Where(s => s.ServiceLineId == serviceIds.DefaultServiceLineId && !s.IsDeleted).Select(s => new ServiceLineVM() { ServiceLineId = s.ServiceLineId, ServiceName = s.ServiceName }).FirstOrDefault();
                TrumaDataVM.SelectedServiceLineIds = string.Join(",", serviceLineIds);

                TrumaDataVM.OrganizationData = GetHosplitalAddressObject(TrumaDataVM.OrganizationIdFk);
                TrumaDataVM.LastKnownWellStr = TrumaDataVM.LastKnownWell?.ToString("yyyy-MM-dd hh:mm:ss tt");
                TrumaDataVM.DobStr = TrumaDataVM.Dob?.ToString("yyyy-MM-dd hh:mm:ss tt");
                TrumaDataVM.GenderTitle = _controlListDetailsRepo.Table.Where(g => g.ControlListDetailId == TrumaDataVM.Gender).Select(g => g.Title).FirstOrDefault();
                TrumaDataVM.BloodThinnersTitle.AddRange(_controlListDetailsRepo.Table.Where(b => TrumaDataVM.BloodThinners.ToIntList().Contains(b.ControlListDetailId)).Select(b => new { Id = b.ControlListDetailId, b.Title }).ToList());
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Found", Body = TrumaDataVM };
            }
            return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Record Not Found" };
        }

        public BaseResponse AddOrUpdateTrumaData(CodeTrumaVM codeTruma)
        {
            if (codeTruma != null && !string.IsNullOrEmpty(codeTruma.LastKnownWellStr) && !string.IsNullOrWhiteSpace(codeTruma.LastKnownWellStr))
            {
                codeTruma.LastKnownWell = DateTime.Parse(codeTruma.LastKnownWellStr);
            }
            if (codeTruma != null && !string.IsNullOrEmpty(codeTruma.DobStr) && !string.IsNullOrWhiteSpace(codeTruma.DobStr))
            {
                codeTruma.Dob = DateTime.Parse(codeTruma.DobStr);
            }
            if (codeTruma.CodeTraumaId > 0)
            {
                var row = this._codeTrumaRepo.Table.Where(x => x.CodeTraumaId == codeTruma.CodeTraumaId && !x.IsDeleted).FirstOrDefault();

                row.PatientName = codeTruma.PatientName;
                row.Dob = codeTruma.Dob;
                row.Gender = codeTruma.Gender;
                row.ChiefComplant = codeTruma.ChiefComplant;
                row.LastKnownWell = codeTruma.LastKnownWell;
                row.Hpi = codeTruma.Hpi;
                row.BloodThinners = codeTruma.BloodThinners;
                row.FamilyContactName = codeTruma.FamilyContactName;
                row.FamilyContactNumber = codeTruma.FamilyContactNumber;
                row.IsEms = codeTruma.IsEms;
                //row.IsCompleted = codeTruma.IsCompleted;
                if (codeTruma.IsCompleted != null && codeTruma.IsCompleted == true && row.IsCompleted != true)
                {
                    row.IsCompleted = true;
                    row.EndTime = DateTime.UtcNow;
                    row.ActualTime = row.EndTime - row.CreatedDate;
                }
                row.ModifiedBy = codeTruma.ModifiedBy;
                row.ModifiedDate = DateTime.UtcNow;
                row.IsDeleted = false;


                if (codeTruma.Attachment != null && codeTruma.Attachment.Count > 0)
                {
                    var RootPath = this._RootPath + "/Organizations";
                    string FileRoot = null;
                    List<string> Attachments = new();
                    //var outPath = Directory.GetCurrentDirectory();

                    FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeTruma.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                    FileRoot = Path.Combine(RootPath, FileRoot);
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, UCLEnums.Trauma.ToString());
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, row.CodeTraumaId.ToString());
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, "Attachments");

                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    //else
                    //{
                    //    DirectoryInfo dir = new DirectoryInfo(FileRoot);
                    //    foreach (FileInfo fi in dir.GetFiles())
                    //    {
                    //        fi.Delete();
                    //    }
                    //}
                    foreach (var item in codeTruma.Attachment)
                    {
                        if (!string.IsNullOrEmpty(item.Base64Str))
                        {

                            var fileInfo = item.Base64Str.Split("base64,");
                            string fileExtension = fileInfo[0].GetFileExtenstion();
                            var ByteFile = Convert.FromBase64String(fileInfo[1]);
                            string FilePath = Path.Combine(FileRoot, item.FileName);

                            if (File.Exists(FilePath))
                            {
                                long existingFileSize = 0;
                                long newFileSize = ByteFile.LongLength;
                                FileInfo ExistingfileInfo = new FileInfo(FilePath);
                                existingFileSize = ExistingfileInfo.Length;

                                if (existingFileSize > 0 && newFileSize != existingFileSize)
                                {
                                    var alterFile = item.FileName.Split('.');
                                    string extention = alterFile.LastOrDefault();
                                    var alterFileName = alterFile.ToList();
                                    alterFileName.RemoveAt(alterFileName.Count - 1);
                                    string fileName = string.Join(".", alterFileName);
                                    fileName = fileName + "_" + HelperExtension.CreateRandomString(7) + "." + extention;
                                    FilePath = Path.Combine(FileRoot, fileName);
                                    using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        fs.Write(ByteFile);
                                    }
                                }
                                else
                                {
                                    using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        fs.Write(ByteFile);
                                    }
                                }
                            }
                            else
                            {
                                using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                {
                                    fs.Write(ByteFile);
                                }
                            }
                        }

                    }
                    if (FileRoot != null && FileRoot != "")
                    {
                        codeTruma.AttachmentsFolderRoot = FileRoot.Replace(this._RootPath, "").Replace("\\", "/");
                    }
                }
                if (codeTruma.Videos != null && codeTruma.Videos.Count > 0)
                {
                    var RootPath = this._RootPath + "/Organizations";
                    string FileRoot = null;
                    List<string> Attachments = new();
                    //var outPath = Directory.GetCurrentDirectory();

                    FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeTruma.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                    FileRoot = Path.Combine(RootPath, FileRoot);
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, UCLEnums.Trauma.ToString());
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, row.CodeTraumaId.ToString());
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, "Videos");

                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    //else
                    //{
                    //    DirectoryInfo dir = new DirectoryInfo(FileRoot);
                    //    foreach (FileInfo fi in dir.GetFiles())
                    //    {
                    //        fi.Delete();
                    //    }
                    //}
                    foreach (var item in codeTruma.Videos)
                    {
                        if (!string.IsNullOrEmpty(item.Base64Str))
                        {

                            var fileInfo = item.Base64Str.Split("base64,");
                            string fileExtension = fileInfo[0].GetFileExtenstion();
                            var ByteFile = Convert.FromBase64String(fileInfo[1]);
                            string FilePath = Path.Combine(FileRoot, item.FileName);

                            if (File.Exists(FilePath))
                            {
                                long existingFileSize = 0;
                                long newFileSize = ByteFile.LongLength;
                                FileInfo ExistingfileInfo = new FileInfo(FilePath);
                                existingFileSize = ExistingfileInfo.Length;

                                if (existingFileSize > 0 && newFileSize != existingFileSize)
                                {
                                    var alterFile = item.FileName.Split('.');
                                    string extention = alterFile.LastOrDefault();
                                    var alterFileName = alterFile.ToList();
                                    alterFileName.RemoveAt(alterFileName.Count - 1);
                                    string fileName = string.Join(".", alterFileName);
                                    fileName = fileName + "_" + HelperExtension.CreateRandomString(7) + "." + extention;
                                    FilePath = Path.Combine(FileRoot, fileName);
                                    using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        fs.Write(ByteFile);
                                    }
                                }
                                else
                                {
                                    using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        fs.Write(ByteFile);
                                    }
                                }
                            }
                            else
                            {
                                using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                {
                                    fs.Write(ByteFile);
                                }
                            }
                        }

                    }
                    if (FileRoot != null && FileRoot != "")
                    {
                        codeTruma.VideoFolderRoot = FileRoot.Replace(this._RootPath, "").Replace("\\", "/");
                    }
                }
                if (codeTruma.Audios != null && codeTruma.Audios.Count > 0)
                {
                    var RootPath = this._RootPath + "/Organizations";
                    string FileRoot = null;
                    List<string> Attachments = new();
                    //var outPath = Directory.GetCurrentDirectory();

                    FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeTruma.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                    FileRoot = Path.Combine(RootPath, FileRoot);
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, UCLEnums.Trauma.ToString());
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, row.CodeTraumaId.ToString());
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, "Audios");

                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    //else
                    //{
                    //    DirectoryInfo dir = new DirectoryInfo(FileRoot);
                    //    foreach (FileInfo fi in dir.GetFiles())
                    //    {
                    //        fi.Delete();
                    //    }
                    //}
                    foreach (var item in codeTruma.Audios)
                    {
                        if (!string.IsNullOrEmpty(item.Base64Str))
                        {

                            var fileInfo = item.Base64Str.Split("base64,");
                            string fileExtension = fileInfo[0].GetFileExtenstion();
                            var ByteFile = Convert.FromBase64String(fileInfo[1]);
                            string FilePath = Path.Combine(FileRoot, item.FileName);

                            if (File.Exists(FilePath))
                            {
                                long existingFileSize = 0;
                                long newFileSize = ByteFile.LongLength;
                                FileInfo ExistingfileInfo = new FileInfo(FilePath);
                                existingFileSize = ExistingfileInfo.Length;

                                if (existingFileSize > 0 && newFileSize != existingFileSize)
                                {
                                    var alterFile = item.FileName.Split('.');
                                    string extention = alterFile.LastOrDefault();
                                    var alterFileName = alterFile.ToList();
                                    alterFileName.RemoveAt(alterFileName.Count - 1);
                                    string fileName = string.Join(".", alterFileName);
                                    fileName = fileName + "_" + HelperExtension.CreateRandomString(7) + "." + extention;
                                    FilePath = Path.Combine(FileRoot, fileName);
                                    using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        fs.Write(ByteFile);
                                    }
                                }
                                else
                                {
                                    using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        fs.Write(ByteFile);
                                    }
                                }
                            }
                            else
                            {
                                using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                {
                                    fs.Write(ByteFile);
                                }
                            }
                        }

                    }
                    if (FileRoot != null && FileRoot != "")
                    {
                        codeTruma.AudioFolderRoot = FileRoot.Replace(this._RootPath, "").Replace("\\", "/");
                    }
                }

                if (codeTruma.AttachmentsFolderRoot != null)
                {
                    row.Attachments = codeTruma.AttachmentsFolderRoot;
                }
                if (codeTruma.VideoFolderRoot != null)
                {
                    row.Video = codeTruma.VideoFolderRoot;
                }
                if (codeTruma.AudioFolderRoot != null)
                {
                    row.Audio = codeTruma.AudioFolderRoot;
                }

                this._codeTrumaRepo.Update(row);

                //var serviceLineIds = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == row.OrganizationIdFk && x.CodeIdFk == UCLEnums.Trauma.ToInt() && !x.IsDeleted).Select(x => x.ServiceLineIds).FirstOrDefault();
                if (codeTruma.SelectedServiceLineIds != null && codeTruma.SelectedServiceLineIds != "")
                {

                    var serviceLineIds = codeTruma.SelectedServiceLineIds.ToIntList();
                    var codeServiceMapping = this._codesServiceLinesMappingRepo.Table.Where(x => x.OrganizationIdFk == row.OrganizationIdFk && x.CodeIdFk == UCLEnums.Trauma.ToInt() && x.ActiveCodeId == row.CodeTraumaId).ToList();
                    var rowsToDelete = codeServiceMapping.Where(x => !serviceLineIds.Contains(x.ServiceLineIdFk)).ToList();
                    if (rowsToDelete.Count > 0)
                        this._codesServiceLinesMappingRepo.DeleteRange(rowsToDelete);

                    serviceLineIds.RemoveAll(x => codeServiceMapping.Select(s => s.ServiceLineIdFk).Contains(x));
                    var codeServiceMappingList = new List<CodesServiceLinesMapping>();
                    foreach (var item in serviceLineIds)
                    {
                        var codeService = new CodesServiceLinesMapping()
                        {
                            OrganizationIdFk = row.OrganizationIdFk,
                            CodeIdFk = UCLEnums.Trauma.ToInt(),
                            ServiceLineIdFk = item,
                            ActiveCodeId = row.CodeTraumaId,
                            ActiveCodeName = UCLEnums.Trauma.ToString()
                        };
                        codeServiceMappingList.Add(codeService);
                    }
                    this._codesServiceLinesMappingRepo.Insert(codeServiceMappingList);
                    //var UserChannelSid = (from us in this._userSchedulesRepo.Table
                    //                      join u in this._userRepo.Table on us.UserIdFk equals u.UserId
                    //                      where serviceLineIds.Contains(us.ServiceLineIdFk.Value) && us.ScheduleDateStart <= DateTime.UtcNow && us.ScheduleDateEnd >= DateTime.UtcNow && !us.IsDeleted && !u.IsDeleted
                    //                      select u.UserChannelSid).ToList();

                    var UserChannelSid = (from u in this._userRepo.Table
                                          join gm in this._activeCodesGroupMembersRepo.Table on u.UserId equals gm.UserIdFk
                                          where gm.ActiveCodeIdFk == codeTruma.CodeTraumaId && gm.ActiveCodeName == UCLEnums.Trauma.ToString() && !u.IsDeleted
                                          select u.UserChannelSid).Distinct().ToList();

                    var notification = new PushNotificationVM()
                    {
                        Id = row.CodeTraumaId,
                        OrgId = row.OrganizationIdFk,
                        UserChannelSid = UserChannelSid,
                        From = AuthorEnums.Trauma.ToString(),
                        Msg = (codeTruma.IsEms.HasValue && codeTruma.IsEms.Value ? "EMS" : "Active") + " Code Trauma From is Changed",
                        RouteLink = "/Home/Activate%20Code/code-trauma-form",
                        RouteLinkEMS = "/Home/EMS/activateCode"
                    };

                    _communication.pushNotification(notification);

                }

                //codeTruma.AttachmentsPath = new List<string>();
                //codeTruma.AudiosPath = new List<string>();
                //codeTruma.VideosPath = new List<string>();

                //if (!string.IsNullOrEmpty(codeTruma.AttachmentsFolderRoot) && !string.IsNullOrWhiteSpace(codeTruma.AttachmentsFolderRoot))
                //{
                //    string path = this._RootPath + codeTruma.AttachmentsFolderRoot; //_environment.WebRootFileProvider.GetFileInfo(codeTruma.AttachmentsFolderRoot)?.PhysicalPath;
                //    if (Directory.Exists(path))
                //    {
                //        DirectoryInfo AttachFiles = new DirectoryInfo(path);
                //        foreach (var item in AttachFiles.GetFiles())
                //        {
                //            codeTruma.AttachmentsPath.Add(codeTruma.AttachmentsFolderRoot + "/" + item.Name);
                //        }
                //    }
                //}

                //if (!string.IsNullOrEmpty(codeTruma.AudioFolderRoot) && !string.IsNullOrWhiteSpace(codeTruma.AudioFolderRoot))
                //{
                //    string path = this._RootPath + codeTruma.AudioFolderRoot; //_environment.WebRootFileProvider.GetFileInfo(codeTruma.AudioFolderRoot)?.PhysicalPath;
                //    if (Directory.Exists(path))
                //    {
                //        DirectoryInfo AudioFiles = new DirectoryInfo(path);
                //        foreach (var item in AudioFiles.GetFiles())
                //        {
                //            codeTruma.AudiosPath.Add(codeTruma.AudioFolderRoot + "/" + item.Name);
                //        }
                //    }
                //}

                //if (!string.IsNullOrEmpty(codeTruma.VideoFolderRoot) && !string.IsNullOrWhiteSpace(codeTruma.VideoFolderRoot))
                //{
                //    var path = this._RootPath + codeTruma.VideoFolderRoot;  //_environment.WebRootFileProvider.GetFileInfo(codeTruma.VideoFolderRoot)?.PhysicalPath;
                //    if (Directory.Exists(path))
                //    {
                //        DirectoryInfo VideoFiles = new DirectoryInfo(path);
                //        foreach (var item in VideoFiles.GetFiles())
                //        {
                //            codeTruma.VideosPath.Add(codeTruma.VideoFolderRoot + "/" + item.Name);
                //        }
                //    }
                //}



                return GetTrumaDataById(row.CodeTraumaId);
            }
            else
            {

                //var serviceLineIds = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == codeTruma.OrganizationIdFk && x.CodeIdFk == UCLEnums.Trauma.ToInt() && !x.IsDeleted).Select(x => x.ServiceLineIds).FirstOrDefault();
                if (codeTruma.SelectedServiceLineIds != null && codeTruma.SelectedServiceLineIds != "")
                {
                    var serviceLineIds = codeTruma.SelectedServiceLineIds.ToIntList();
                    codeTruma.CreatedDate = DateTime.UtcNow;
                    var Truma = AutoMapperHelper.MapSingleRow<CodeTrumaVM, CodeTrauma>(codeTruma);


                    if (codeTruma.Attachment != null && codeTruma.Attachment.Count > 0)
                    {
                        var RootPath = this._RootPath + "/Organizations";
                        string FileRoot = null;
                        List<string> Attachments = new();
                        //var outPath = Directory.GetCurrentDirectory();

                        FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeTruma.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                        FileRoot = Path.Combine(RootPath, FileRoot);
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, UCLEnums.Trauma.ToString());
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, Truma.CodeTraumaId.ToString());
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, "Attachments");

                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        //else
                        //{
                        //    DirectoryInfo dir = new DirectoryInfo(FileRoot);
                        //    foreach (FileInfo fi in dir.GetFiles())
                        //    {
                        //        fi.Delete();
                        //    }
                        //}
                        foreach (var item in codeTruma.Attachment)
                        {
                            if (!string.IsNullOrEmpty(item.Base64Str))
                            {

                                var fileInfo = item.Base64Str.Split("base64,");
                                string fileExtension = fileInfo[0].GetFileExtenstion();
                                var ByteFile = Convert.FromBase64String(fileInfo[1]);
                                string FilePath = Path.Combine(FileRoot, item.FileName);

                                if (File.Exists(FilePath))
                                {
                                    long existingFileSize = 0;
                                    long newFileSize = ByteFile.LongLength;
                                    FileInfo ExistingfileInfo = new FileInfo(FilePath);
                                    existingFileSize = ExistingfileInfo.Length;

                                    if (existingFileSize > 0 && newFileSize != existingFileSize)
                                    {
                                        var alterFile = item.FileName.Split('.');
                                        string extention = alterFile.LastOrDefault();
                                        var alterFileName = alterFile.ToList();
                                        alterFileName.RemoveAt(alterFileName.Count - 1);
                                        string fileName = string.Join(".", alterFileName);
                                        fileName = fileName + "_" + HelperExtension.CreateRandomString(7) + "." + extention;
                                        FilePath = Path.Combine(FileRoot, fileName);
                                        using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                        {
                                            fs.Write(ByteFile);
                                        }
                                    }
                                    else
                                    {
                                        using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                        {
                                            fs.Write(ByteFile);
                                        }
                                    }
                                }
                                else
                                {
                                    using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        fs.Write(ByteFile);
                                    }
                                }
                            }

                        }
                        if (FileRoot != null && FileRoot != "")
                        {
                            codeTruma.AttachmentsFolderRoot = FileRoot.Replace(this._RootPath, "").Replace("\\", "/");
                        }
                    }
                    if (codeTruma.Videos != null && codeTruma.Videos.Count > 0)
                    {
                        var RootPath = this._RootPath + "/Organizations";
                        string FileRoot = null;
                        List<string> Attachments = new();
                        //var outPath = Directory.GetCurrentDirectory();

                        FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeTruma.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                        FileRoot = Path.Combine(RootPath, FileRoot);
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, UCLEnums.Trauma.ToString());
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, Truma.CodeTraumaId.ToString());
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, "Videos");

                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        //else
                        //{
                        //    DirectoryInfo dir = new DirectoryInfo(FileRoot);
                        //    foreach (FileInfo fi in dir.GetFiles())
                        //    {
                        //        fi.Delete();
                        //    }
                        //}
                        foreach (var item in codeTruma.Videos)
                        {
                            if (!string.IsNullOrEmpty(item.Base64Str))
                            {

                                var fileInfo = item.Base64Str.Split("base64,");
                                string fileExtension = fileInfo[0].GetFileExtenstion();
                                var ByteFile = Convert.FromBase64String(fileInfo[1]);
                                string FilePath = Path.Combine(FileRoot, item.FileName);

                                if (File.Exists(FilePath))
                                {
                                    long existingFileSize = 0;
                                    long newFileSize = ByteFile.LongLength;
                                    FileInfo ExistingfileInfo = new FileInfo(FilePath);
                                    existingFileSize = ExistingfileInfo.Length;

                                    if (existingFileSize > 0 && newFileSize != existingFileSize)
                                    {
                                        var alterFile = item.FileName.Split('.');
                                        string extention = alterFile.LastOrDefault();
                                        var alterFileName = alterFile.ToList();
                                        alterFileName.RemoveAt(alterFileName.Count - 1);
                                        string fileName = string.Join(".", alterFileName);
                                        fileName = fileName + "_" + HelperExtension.CreateRandomString(7) + "." + extention;
                                        FilePath = Path.Combine(FileRoot, fileName);
                                        using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                        {
                                            fs.Write(ByteFile);
                                        }
                                    }
                                    else
                                    {
                                        using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                        {
                                            fs.Write(ByteFile);
                                        }
                                    }
                                }
                                else
                                {
                                    using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        fs.Write(ByteFile);
                                    }
                                }
                            }

                        }
                        if (FileRoot != null && FileRoot != "")
                        {
                            codeTruma.VideoFolderRoot = FileRoot.Replace(this._RootPath, "").Replace("\\", "/");
                        }
                    }
                    if (codeTruma.Audios != null && codeTruma.Audios.Count > 0)
                    {
                        var RootPath = this._RootPath + "/Organizations";
                        string FileRoot = null;
                        List<string> Attachments = new();
                        //var outPath = Directory.GetCurrentDirectory();

                        FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeTruma.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                        FileRoot = Path.Combine(RootPath, FileRoot);
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, UCLEnums.Trauma.ToString());
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, Truma.CodeTraumaId.ToString());
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, "Audios");

                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        //else
                        //{
                        //    DirectoryInfo dir = new DirectoryInfo(FileRoot);
                        //    foreach (FileInfo fi in dir.GetFiles())
                        //    {
                        //        fi.Delete();
                        //    }
                        //}
                        foreach (var item in codeTruma.Audios)
                        {
                            if (!string.IsNullOrEmpty(item.Base64Str))
                            {

                                var fileInfo = item.Base64Str.Split("base64,");
                                string fileExtension = fileInfo[0].GetFileExtenstion();
                                var ByteFile = Convert.FromBase64String(fileInfo[1]);
                                string FilePath = Path.Combine(FileRoot, item.FileName);

                                if (File.Exists(FilePath))
                                {
                                    long existingFileSize = 0;
                                    long newFileSize = ByteFile.LongLength;
                                    FileInfo ExistingfileInfo = new FileInfo(FilePath);
                                    existingFileSize = ExistingfileInfo.Length;

                                    if (existingFileSize > 0 && newFileSize != existingFileSize)
                                    {
                                        var alterFile = item.FileName.Split('.');
                                        string extention = alterFile.LastOrDefault();
                                        var alterFileName = alterFile.ToList();
                                        alterFileName.RemoveAt(alterFileName.Count - 1);
                                        string fileName = string.Join(".", alterFileName);
                                        fileName = fileName + "_" + HelperExtension.CreateRandomString(7) + "." + extention;
                                        FilePath = Path.Combine(FileRoot, fileName);
                                        using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                        {
                                            fs.Write(ByteFile);
                                        }
                                    }
                                    else
                                    {
                                        using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                        {
                                            fs.Write(ByteFile);
                                        }
                                    }
                                }
                                else
                                {
                                    using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        fs.Write(ByteFile);
                                    }
                                }
                            }

                        }
                        if (FileRoot != null && FileRoot != "")
                        {
                            codeTruma.AudioFolderRoot = FileRoot.Replace(this._RootPath, "").Replace("\\", "/");
                        }
                    }
                    if (codeTruma.AttachmentsFolderRoot != null)
                    {
                        Truma.Attachments = codeTruma.AttachmentsFolderRoot;
                    }
                    if (codeTruma.VideoFolderRoot != null)
                    {
                        Truma.Video = codeTruma.VideoFolderRoot;
                    }
                    if (codeTruma.AudioFolderRoot != null)
                    {
                        Truma.Audio = codeTruma.AudioFolderRoot;
                    }
                    this._codeTrumaRepo.Insert(Truma);

                    var codeServiceMappingList = new List<CodesServiceLinesMapping>();
                    foreach (var item in serviceLineIds)
                    {
                        var codeServiceMapping = new CodesServiceLinesMapping()
                        {
                            OrganizationIdFk = Truma.OrganizationIdFk,
                            CodeIdFk = UCLEnums.Trauma.ToInt(),
                            ServiceLineIdFk = item,
                            ActiveCodeId = Truma.CodeTraumaId,
                            ActiveCodeName = UCLEnums.Trauma.ToString()
                        };
                        codeServiceMappingList.Add(codeServiceMapping);
                    }
                    this._codesServiceLinesMappingRepo.Insert(codeServiceMappingList);

                    var UserChannelSid = (from us in this._userSchedulesRepo.Table
                                          join u in this._userRepo.Table on us.UserIdFk equals u.UserId
                                          where serviceLineIds.Contains(us.ServiceLineIdFk.Value) && us.ScheduleDateStart <= DateTime.UtcNow && us.ScheduleDateEnd >= DateTime.UtcNow && !us.IsDeleted && !u.IsDeleted
                                          select new { u.UserUniqueId, u.UserId }).ToList();
                    var loggedUser = this._userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId }).FirstOrDefault();
                    UserChannelSid.Add(loggedUser);
                    List<ActiveCodesGroupMember> ACodeGroupMembers = new();
                    if (UserChannelSid != null && UserChannelSid.Count > 0)
                    {
                        //string uniqueName = $"CONSULT_{Consult_Counter.Counter_Value.ToString()}";
                        string uniqueName = DateTime.Now.ToString("yyyyMMddHHmmssffff") + ApplicationSettings.UserId.ToString();
                        string friendlyName = Truma.IsEms.HasValue && Truma.IsEms.Value ? $"EMS Code {UCLEnums.Trauma.ToString()} {Truma.CodeTraumaId}" : $"Active Code {UCLEnums.Trauma.ToString()} {Truma.CodeTraumaId}";
                        var conversationChannelAttributes = JsonConvert.SerializeObject(new Dictionary<string, Object>()
                                    {
                                        {ChannelAttributeEnums.ChannelType.ToString(), ChannelTypeEnums.EMS.ToString()},
                                        {ChannelAttributeEnums.CodeType.ToString(), ChannelTypeEnums.Trauma.ToString()},
                                        {ChannelAttributeEnums.TraumaId.ToString(), Truma.CodeTraumaId}
                                    }, Formatting.Indented);
                        var channel = _communication.createConversationChannel(friendlyName, uniqueName, conversationChannelAttributes);
                        UserChannelSid = UserChannelSid.Distinct().ToList();
                        foreach (var item in UserChannelSid)
                        {
                            try
                            {
                                var codeGroupMember = new ActiveCodesGroupMember()
                                {
                                    UserIdFk = item.UserId,
                                    ActiveCodeIdFk = Truma.CodeTraumaId,
                                    ActiveCodeName = UCLEnums.Trauma.ToString(),
                                    IsAcknowledge = false,
                                    CreatedBy = ApplicationSettings.UserId,
                                    CreatedDate = DateTime.UtcNow,
                                    IsDeleted = false
                                };
                                ACodeGroupMembers.Add(codeGroupMember);
                                _communication.addNewUserToConversationChannel(channel.Sid, item.UserUniqueId);
                            }
                            catch (Exception ex)
                            {
                                ElmahExtensions.RiseError(ex);
                            }
                        }
                        var isMembersAdded = AddGroupMembers(ACodeGroupMembers);
                        var msg = new ConversationMessageVM();
                        msg.channelSid = channel.Sid;
                        msg.author = "System";
                        msg.attributes = "";
                        msg.body = $"<strong> {(codeTruma.IsEms.HasValue && codeTruma.IsEms.Value ? "EMS Code" : "Active Code")} {UCLEnums.Trauma.ToString()} </strong></br></br>";
                        if (codeTruma.PatientName != null && codeTruma.PatientName != "")
                            msg.body += $"<strong>Patient Name: </strong> {codeTruma.PatientName} </br>";
                        msg.body += $"<strong>Dob: </strong> {codeTruma.Dob:MM-dd-yyyy} </br>";
                        msg.body += $"<strong>Last Well Known: </strong> {codeTruma.LastKnownWell:MM-dd-yyyy hh:mm tt} </br>";
                        if (codeTruma.ChiefComplant != null && codeTruma.ChiefComplant != "")
                            msg.body += $"<strong>Chief Complaint: </strong> {codeTruma.ChiefComplant} </br>";
                        if (codeTruma.Hpi != null && codeTruma.Hpi != "")
                            msg.body += $"<strong>Hpi: </strong> {codeTruma.Hpi} </br>";
                        var sendMsg = _communication.sendPushNotification(msg);
                    }
                    return GetTrumaDataById(Truma.CodeTraumaId);
                }
                return new BaseResponse() { Status = HttpStatusCode.NotAcceptable, Message = "There is no Service Line in this organization related to Code Trauma" };
            }
        }

        public BaseResponse DeleteTruma(int TrumaId)
        {
            var row = this._codeTrumaRepo.Table.Where(x => x.CodeTraumaId == TrumaId && !x.IsDeleted).FirstOrDefault();
            row.IsDeleted = true;
            row.ModifiedBy = ApplicationSettings.UserId;
            row.ModifiedDate = DateTime.UtcNow;
            this._codeTrumaRepo.Update(row);

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Deleted" };
        }


        #endregion

        #region Code Blue

        public BaseResponse GetAllBlueCode(ActiveCodeVM activeCode)
        {
            var blueData = new List<CodeBlue>();
            if (ApplicationSettings.isSuperAdmin)
            {
                blueData = this._codeBlueRepo.Table.Where(x => x.OrganizationIdFk == activeCode.OrganizationIdFk && !x.IsDeleted).OrderByDescending(x => x.CodeBlueId).ToList();
            }
            if (activeCode.showAllActiveCodes)
            {
                blueData = this._codeBlueRepo.Table.Where(x => x.OrganizationIdFk == activeCode.OrganizationIdFk && !x.IsDeleted).OrderByDescending(x => x.CodeBlueId).ToList();
            }
            else
            {
                blueData = (from cs in this._codeBlueRepo.Table
                            join agm in this._activeCodesGroupMembersRepo.Table on cs.CodeBlueId equals agm.ActiveCodeIdFk
                            where agm.UserIdFk == ApplicationSettings.UserId && agm.ActiveCodeName == UCLEnums.Blue.ToString() && !cs.IsDeleted
                            select cs).OrderByDescending(x => x.BloodThinners).AsQueryable().ToList();

                //blueData = this._codeBlueRepo.Table.Where(x => x.CreatedBy == ApplicationSettings.UserId && !x.IsDeleted).OrderByDescending(x => x.CodeBlueId).ToList();
            }
            var blueDataVM = AutoMapperHelper.MapList<CodeBlue, CodeBlueVM>(blueData);
            //var orgData = GetHosplitalAddressObject(activeCode.OrganizationIdFk);

            blueDataVM.ForEach(x =>
            {
                x.AttachmentsPath = new List<string>();
                x.AudiosPath = new List<string>();
                x.VideosPath = new List<string>();
                x.OrganizationData = new object();
                x.BloodThinnersTitle = new List<object>();
                x.ServiceLines = new List<ServiceLineVM>();
                //if (!string.IsNullOrEmpty(x.Attachments) && !string.IsNullOrWhiteSpace(x.Attachments))
                //{
                //    string path = this._RootPath + x.Attachments; //this._RootPath  + x.Attachments;
                //    if (Directory.Exists(path))
                //    {
                //        DirectoryInfo AttachFiles = new DirectoryInfo(path);
                //        foreach (var item in AttachFiles.GetFiles())
                //        {
                //            x.AttachmentsPath.Add(x.Attachments + "/" + item.Name);
                //        }
                //    }
                //}

                //if (!string.IsNullOrEmpty(x.Audio) && !string.IsNullOrWhiteSpace(x.Audio))
                //{
                //    string path = this._RootPath + x.Audio; //this._RootPath  + x.Audio;
                //    if (Directory.Exists(path))
                //    {
                //        DirectoryInfo AudioFiles = new DirectoryInfo(path);
                //        foreach (var item in AudioFiles.GetFiles())
                //        {
                //            x.AudiosPath.Add(x.Audio + "/" + item.Name);
                //        }
                //    }
                //}

                //if (!string.IsNullOrEmpty(x.Video) && !string.IsNullOrWhiteSpace(x.Video))
                //{
                //    var path = this._RootPath + x.Video; //this._RootPath  + x.Video;
                //    if (Directory.Exists(path))
                //    {
                //        DirectoryInfo VideoFiles = new DirectoryInfo(path);
                //        foreach (var item in VideoFiles.GetFiles())
                //        {
                //            x.VideosPath.Add(x.Video + "/" + item.Name);
                //        }
                //    }
                //}
                //var serviceIds = this._activeCodeRepo.Table.Where(s => s.OrganizationIdFk == x.OrganizationIdFk && s.CodeIdFk == UCLEnums.Blue.ToInt() && !s.IsDeleted).Select(s => new { s.ServiceLineIds, s.DefaultServiceLineId }).FirstOrDefault();
                //var serviceLineIds = (from s in this._codesServiceLinesMappingRepo.Table
                //                      where s.OrganizationIdFk == x.OrganizationIdFk && s.CodeIdFk == UCLEnums.Blue.ToInt()
                //                      && s.ActiveCodeId == x.CodeBlueId && s.ActiveCodeName == UCLEnums.Blue.ToString() && s.ServiceLineIdFk != serviceIds.DefaultServiceLineId
                //                      select s.ServiceLineIdFk).ToList();
                //x.ServiceLines = this._serviceLineRepo.Table.Where(s => serviceIds.ServiceLineIds.ToIntList().Distinct().Contains(s.ServiceLineId) && s.ServiceLineId != serviceIds.DefaultServiceLineId && !s.IsDeleted).Select(s => new ServiceLineVM() { ServiceLineId = s.ServiceLineId, ServiceName = s.ServiceName, IsSelected = serviceLineIds.Contains(s.ServiceLineId) }).ToList();
                //x.DefaultServiceLineId = serviceIds.DefaultServiceLineId;
                //x.DefaultServiceLine = this._serviceLineRepo.Table.Where(s => s.ServiceLineId == serviceIds.DefaultServiceLineId && !s.IsDeleted).Select(s => new ServiceLineVM() { ServiceLineId = s.ServiceLineId, ServiceName = s.ServiceName }).FirstOrDefault();
                //x.SelectedServiceLineIds = string.Join(",", serviceLineIds);
                x.LastKnownWellStr = x.LastKnownWell?.ToString("yyyy-MM-dd hh:mm:ss tt");
                x.DobStr = x.Dob?.ToString("yyyy-MM-dd hh:mm:ss tt");
                //x.OrganizationData = orgData;
                x.GenderTitle = _controlListDetailsRepo.Table.Where(g => g.ControlListDetailId == x.Gender).Select(g => g.Title).FirstOrDefault();
                x.BloodThinnersTitle.AddRange(_controlListDetailsRepo.Table.Where(b => x.BloodThinners.ToIntList().Contains(b.ControlListDetailId)).Select(b => new { Id = b.ControlListDetailId, b.Title }).ToList());
            });
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Returned", Body = blueDataVM };
        }

        public BaseResponse GetBlueDataById(int blueId)
        {
            var blueData = this._codeBlueRepo.Table.Where(x => x.CodeBlueId == blueId && !x.IsDeleted).FirstOrDefault();
            if (blueData != null)
            {
                var BlueDataVM = AutoMapperHelper.MapSingleRow<CodeBlue, CodeBlueVM>(blueData);
                BlueDataVM.AttachmentsPath = new List<string>();
                BlueDataVM.AudiosPath = new List<string>();
                BlueDataVM.VideosPath = new List<string>();
                BlueDataVM.BloodThinnersTitle = new List<object>();
                BlueDataVM.OrganizationData = new object();

                if (!string.IsNullOrEmpty(BlueDataVM.Attachments) && !string.IsNullOrWhiteSpace(BlueDataVM.Attachments))
                {
                    string path = this._RootPath + blueData.Attachments; //_environment.WebRootFileProvider.GetFileInfo(StrokeDataVM.Attachments)?.PhysicalPath;
                    if (Directory.Exists(path))
                    {
                        DirectoryInfo AttachFiles = new DirectoryInfo(path);
                        foreach (var item in AttachFiles.GetFiles())
                        {
                            BlueDataVM.AttachmentsPath.Add(BlueDataVM.Attachments + "/" + item.Name);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(BlueDataVM.Audio) && !string.IsNullOrWhiteSpace(BlueDataVM.Audio))
                {
                    string path = this._RootPath + blueData.Audio; //_environment.WebRootFileProvider.GetFileInfo(StrokeDataVM.Audio)?.PhysicalPath;
                    if (Directory.Exists(path))
                    {
                        DirectoryInfo AudioFiles = new DirectoryInfo(path);
                        foreach (var item in AudioFiles.GetFiles())
                        {
                            BlueDataVM.AudiosPath.Add(BlueDataVM.Audio + "/" + item.Name);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(BlueDataVM.Video) && !string.IsNullOrWhiteSpace(BlueDataVM.Video))
                {
                    var path = this._RootPath + blueData.Video;  //_environment.WebRootFileProvider.GetFileInfo(StrokeDataVM.Video)?.PhysicalPath; //.GetFileInfo(StrokeDataVM.Video);//?.PhysicalPath;
                    if (Directory.Exists(path))
                    {
                        DirectoryInfo VideoFiles = new DirectoryInfo(path);
                        foreach (var item in VideoFiles.GetFiles())
                        {
                            BlueDataVM.VideosPath.Add(BlueDataVM.Video + "/" + item.Name);
                        }
                    }
                }
                var serviceIds = this._activeCodeRepo.Table.Where(s => s.OrganizationIdFk == BlueDataVM.OrganizationIdFk && s.CodeIdFk == UCLEnums.Blue.ToInt() && !s.IsDeleted).Select(s => new { s.ServiceLineIds, s.DefaultServiceLineId }).FirstOrDefault();
                var serviceLineIds = (from s in this._codesServiceLinesMappingRepo.Table
                                      where s.OrganizationIdFk == BlueDataVM.OrganizationIdFk && s.CodeIdFk == UCLEnums.Blue.ToInt()
                                      && s.ActiveCodeId == BlueDataVM.CodeBlueId && s.ActiveCodeName == UCLEnums.Blue.ToString() && s.ServiceLineIdFk != serviceIds.DefaultServiceLineId
                                      select s.ServiceLineIdFk).ToList();
                BlueDataVM.ServiceLines = this._serviceLineRepo.Table.Where(s => serviceIds.ServiceLineIds.ToIntList().Distinct().Contains(s.ServiceLineId) && s.ServiceLineId != serviceIds.DefaultServiceLineId && !s.IsDeleted).Select(s => new ServiceLineVM() { ServiceLineId = s.ServiceLineId, ServiceName = s.ServiceName, IsSelected = serviceLineIds.Contains(s.ServiceLineId) }).ToList();
                BlueDataVM.DefaultServiceLineId = serviceIds.DefaultServiceLineId;
                BlueDataVM.DefaultServiceLine = this._serviceLineRepo.Table.Where(s => s.ServiceLineId == serviceIds.DefaultServiceLineId && !s.IsDeleted).Select(s => new ServiceLineVM() { ServiceLineId = s.ServiceLineId, ServiceName = s.ServiceName }).FirstOrDefault();
                BlueDataVM.SelectedServiceLineIds = string.Join(",", serviceLineIds);

                BlueDataVM.OrganizationData = GetHosplitalAddressObject(BlueDataVM.OrganizationIdFk);
                BlueDataVM.LastKnownWellStr = BlueDataVM.LastKnownWell?.ToString("yyyy-MM-dd hh:mm:ss tt");
                BlueDataVM.DobStr = BlueDataVM.Dob?.ToString("yyyy-MM-dd hh:mm:ss tt");
                BlueDataVM.GenderTitle = _controlListDetailsRepo.Table.Where(g => g.ControlListDetailId == BlueDataVM.Gender).Select(g => g.Title).FirstOrDefault();
                BlueDataVM.BloodThinnersTitle.AddRange(_controlListDetailsRepo.Table.Where(b => BlueDataVM.BloodThinners.ToIntList().Contains(b.ControlListDetailId)).Select(b => new { Id = b.ControlListDetailId, b.Title }).ToList());
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Found", Body = BlueDataVM };
            }
            return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Record Not Found" };
        }

        public BaseResponse AddOrUpdateBlueData(CodeBlueVM codeBlue)
        {
            if (codeBlue != null && !string.IsNullOrEmpty(codeBlue.LastKnownWellStr) && !string.IsNullOrWhiteSpace(codeBlue.LastKnownWellStr))
            {
                codeBlue.LastKnownWell = DateTime.Parse(codeBlue.LastKnownWellStr);
            }
            if (codeBlue != null && !string.IsNullOrEmpty(codeBlue.DobStr) && !string.IsNullOrWhiteSpace(codeBlue.DobStr))
            {
                codeBlue.Dob = DateTime.Parse(codeBlue.DobStr);
            }
            if (codeBlue.CodeBlueId > 0)
            {
                var row = this._codeBlueRepo.Table.Where(x => x.CodeBlueId == codeBlue.CodeBlueId && !x.IsDeleted).FirstOrDefault();

                row.OrganizationIdFk = codeBlue.OrganizationIdFk;
                row.PatientName = codeBlue.PatientName;
                row.Dob = codeBlue.Dob;
                row.Gender = codeBlue.Gender;
                row.ChiefComplant = codeBlue.ChiefComplant;
                row.LastKnownWell = codeBlue.LastKnownWell;
                row.Hpi = codeBlue.Hpi;
                row.BloodThinners = codeBlue.BloodThinners;
                row.FamilyContactName = codeBlue.FamilyContactName;
                row.FamilyContactNumber = codeBlue.FamilyContactNumber;
                row.IsEms = codeBlue.IsEms;
                //row.IsCompleted = codeStroke.IsCompleted;
                if (codeBlue.IsCompleted != null && codeBlue.IsCompleted == true && row.IsCompleted != true)
                {
                    row.IsCompleted = true;
                    row.EndTime = DateTime.UtcNow;
                    row.ActualTime = row.EndTime - row.CreatedDate;
                }
                row.ModifiedBy = codeBlue.ModifiedBy;
                row.ModifiedDate = DateTime.UtcNow;
                row.IsDeleted = false;

                if (codeBlue.Attachment != null && codeBlue.Attachment.Count > 0)
                {
                    var RootPath = this._RootPath + "/Organizations"; //this._RootPath + "/Organizations";
                    string FileRoot = null;
                    List<string> Attachments = new();
                    FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeBlue.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                    FileRoot = Path.Combine(RootPath, FileRoot);
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, UCLEnums.Blue.ToString());
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, row.CodeBlueId.ToString());
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, "Attachments");

                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    //else
                    //{
                    //    DirectoryInfo dir = new DirectoryInfo(FileRoot);
                    //    foreach (FileInfo fi in dir.GetFiles())
                    //    {
                    //        fi.Delete();
                    //    }
                    //}
                    foreach (var item in codeBlue.Attachment)
                    {
                        if (!string.IsNullOrEmpty(item.Base64Str))
                        {

                            var fileInfo = item.Base64Str.Split("base64,");
                            string fileExtension = fileInfo[0].GetFileExtenstion();
                            var ByteFile = Convert.FromBase64String(fileInfo[1]);
                            string FilePath = Path.Combine(FileRoot, item.FileName);

                            if (File.Exists(FilePath))
                            {
                                long existingFileSize = 0;
                                long newFileSize = ByteFile.LongLength;
                                FileInfo ExistingfileInfo = new FileInfo(FilePath);
                                existingFileSize = ExistingfileInfo.Length;

                                if (existingFileSize > 0 && newFileSize != existingFileSize)
                                {
                                    var alterFile = item.FileName.Split('.');
                                    string extention = alterFile.LastOrDefault();
                                    var alterFileName = alterFile.ToList();
                                    alterFileName.RemoveAt(alterFileName.Count - 1);
                                    string fileName = string.Join(".", alterFileName);
                                    fileName = fileName + "_" + HelperExtension.CreateRandomString(7) + "." + extention;
                                    FilePath = Path.Combine(FileRoot, fileName);
                                    using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        fs.Write(ByteFile);
                                    }
                                }
                                else
                                {
                                    using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        fs.Write(ByteFile);
                                    }
                                }
                            }
                            else
                            {
                                using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                {
                                    fs.Write(ByteFile);
                                }
                            }
                        }
                    }
                    if (FileRoot != null && FileRoot != "")
                    {
                        codeBlue.AttachmentsFolderRoot = FileRoot.Replace(this._RootPath, "").Replace("\\", "/");
                    }
                }
                if (codeBlue.Videos != null && codeBlue.Videos.Count > 0)
                {
                    var RootPath = this._RootPath + "/Organizations";
                    string FileRoot = null;
                    List<string> Attachments = new();
                    FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeBlue.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                    FileRoot = Path.Combine(RootPath, FileRoot);
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, UCLEnums.Blue.ToString());
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, row.CodeBlueId.ToString());
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, "Videos");

                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    //else
                    //{
                    //    DirectoryInfo dir = new DirectoryInfo(FileRoot);
                    //    foreach (FileInfo fi in dir.GetFiles())
                    //    {
                    //        fi.Delete();
                    //    }
                    //}
                    foreach (var item in codeBlue.Videos)
                    {
                        if (!string.IsNullOrEmpty(item.Base64Str))
                        {

                            var fileInfo = item.Base64Str.Split("base64,");
                            string fileExtension = fileInfo[0].GetFileExtenstion();
                            var ByteFile = Convert.FromBase64String(fileInfo[1]);
                            string FilePath = Path.Combine(FileRoot, item.FileName);

                            if (File.Exists(FilePath))
                            {
                                long existingFileSize = 0;
                                long newFileSize = ByteFile.LongLength;
                                FileInfo ExistingfileInfo = new FileInfo(FilePath);
                                existingFileSize = ExistingfileInfo.Length;

                                if (existingFileSize > 0 && newFileSize != existingFileSize)
                                {
                                    var alterFile = item.FileName.Split('.');
                                    string extention = alterFile.LastOrDefault();
                                    var alterFileName = alterFile.ToList();
                                    alterFileName.RemoveAt(alterFileName.Count - 1);
                                    string fileName = string.Join(".", alterFileName);
                                    fileName = fileName + "_" + HelperExtension.CreateRandomString(7) + "." + extention;
                                    FilePath = Path.Combine(FileRoot, fileName);
                                    using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        fs.Write(ByteFile);
                                    }
                                }
                                else
                                {
                                    using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        fs.Write(ByteFile);
                                    }
                                }
                            }
                            else
                            {
                                using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                {
                                    fs.Write(ByteFile);
                                }
                            }
                        }

                    }
                    if (FileRoot != null && FileRoot != "")
                    {
                        codeBlue.VideoFolderRoot = FileRoot.Replace(this._RootPath, "").Replace("\\", "/");
                    }
                }
                if (codeBlue.Audios != null && codeBlue.Audios.Count > 0)
                {
                    var RootPath = this._RootPath + "/Organizations";
                    string FileRoot = null;
                    List<string> Attachments = new();

                    FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeBlue.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                    FileRoot = Path.Combine(RootPath, FileRoot);
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, UCLEnums.Blue.ToString());
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, row.CodeBlueId.ToString());
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, "Audios");

                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    //else
                    //{
                    //    DirectoryInfo dir = new DirectoryInfo(FileRoot);
                    //    foreach (FileInfo fi in dir.GetFiles())
                    //    {
                    //        fi.Delete();
                    //    }
                    //}
                    foreach (var item in codeBlue.Audios)
                    {
                        if (!string.IsNullOrEmpty(item.Base64Str))
                        {

                            var fileInfo = item.Base64Str.Split("base64,");
                            string fileExtension = fileInfo[0].GetFileExtenstion();
                            var ByteFile = Convert.FromBase64String(fileInfo[1]);
                            string FilePath = Path.Combine(FileRoot, item.FileName);

                            if (File.Exists(FilePath))
                            {
                                long existingFileSize = 0;
                                long newFileSize = ByteFile.LongLength;
                                FileInfo ExistingfileInfo = new FileInfo(FilePath);
                                existingFileSize = ExistingfileInfo.Length;

                                if (existingFileSize > 0 && newFileSize != existingFileSize)
                                {
                                    var alterFile = item.FileName.Split('.');
                                    string extention = alterFile.LastOrDefault();
                                    var alterFileName = alterFile.ToList();
                                    alterFileName.RemoveAt(alterFileName.Count - 1);
                                    string fileName = string.Join(".", alterFileName);
                                    fileName = fileName + "_" + HelperExtension.CreateRandomString(7) + "." + extention;
                                    FilePath = Path.Combine(FileRoot, fileName);
                                    using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        fs.Write(ByteFile);
                                    }
                                }
                                else
                                {
                                    using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        fs.Write(ByteFile);
                                    }
                                }
                            }
                            else
                            {
                                using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                {
                                    fs.Write(ByteFile);
                                }
                            }
                        }


                    }
                    if (FileRoot != null && FileRoot != "")
                    {
                        codeBlue.AudioFolderRoot = FileRoot.Replace(this._RootPath, "").Replace("\\", "/");
                    }
                }

                if (codeBlue.AttachmentsFolderRoot != null)
                {
                    row.Attachments = codeBlue.AttachmentsFolderRoot;
                }
                if (codeBlue.VideoFolderRoot != null)
                {
                    row.Video = codeBlue.VideoFolderRoot;
                }
                if (codeBlue.AudioFolderRoot != null)
                {
                    row.Audio = codeBlue.AudioFolderRoot;
                }

                this._codeBlueRepo.Update(row);


                //var serviceLineIds = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == row.OrganizationIdFk && x.CodeIdFk == UCLEnums.Blue.ToInt() && !x.IsDeleted).Select(x => x.ServiceLineIds).FirstOrDefault();
                if (codeBlue.SelectedServiceLineIds != null && codeBlue.SelectedServiceLineIds != "")
                {
                    var serviceLineIds = codeBlue.SelectedServiceLineIds.ToIntList();
                    var codeServiceMapping = this._codesServiceLinesMappingRepo.Table.Where(x => x.OrganizationIdFk == row.OrganizationIdFk && x.CodeIdFk == UCLEnums.Blue.ToInt() && x.ActiveCodeId == row.CodeBlueId).ToList();
                    var rowsToDelete = codeServiceMapping.Where(x => !serviceLineIds.Contains(x.ServiceLineIdFk)).ToList();
                    if (rowsToDelete.Count > 0)
                        this._codesServiceLinesMappingRepo.DeleteRange(rowsToDelete);

                    serviceLineIds.RemoveAll(x => codeServiceMapping.Select(s => s.ServiceLineIdFk).Contains(x));
                    var codeServiceMappingList = new List<CodesServiceLinesMapping>();
                    foreach (var item in serviceLineIds)
                    {
                        var codeService = new CodesServiceLinesMapping()
                        {
                            OrganizationIdFk = row.OrganizationIdFk,
                            CodeIdFk = UCLEnums.Blue.ToInt(),
                            ServiceLineIdFk = item,
                            ActiveCodeId = row.CodeBlueId,
                            ActiveCodeName = UCLEnums.Blue.ToString()
                        };
                        codeServiceMappingList.Add(codeService);
                    }
                    this._codesServiceLinesMappingRepo.Insert(codeServiceMappingList);

                    //var UserChannelSid = (from us in this._userSchedulesRepo.Table
                    //                      join u in this._userRepo.Table on us.UserIdFk equals u.UserId
                    //                      where serviceLineIds.Contains(us.ServiceLineIdFk.Value) && us.ScheduleDateStart <= DateTime.Now && us.ScheduleDateEnd >= DateTime.Now && !us.IsDeleted && !u.IsDeleted
                    //                      select u.UserChannelSid).ToList();

                    var UserChannelSid = (from u in this._userRepo.Table
                                          join gm in this._activeCodesGroupMembersRepo.Table on u.UserId equals gm.UserIdFk
                                          where gm.ActiveCodeIdFk == codeBlue.CodeBlueId && gm.ActiveCodeName == UCLEnums.Blue.ToString() && !u.IsDeleted
                                          select u.UserChannelSid).Distinct().ToList();

                    var notification = new PushNotificationVM()
                    {
                        Id = row.CodeBlueId,
                        OrgId = row.OrganizationIdFk,
                        UserChannelSid = UserChannelSid,
                        From = AuthorEnums.Blue.ToString(),
                        Msg = (codeBlue.IsEms.HasValue && codeBlue.IsEms.Value ? "EMS" : "Active") + " Code Blue From is Changed",
                        RouteLink = "/Home/Activate%20Code/code-blue-form",
                        RouteLinkEMS = "/Home/EMS/activateCode"
                    };

                    _communication.pushNotification(notification);

                }

                //codeStroke.AttachmentsPath = new List<string>();
                //codeStroke.AudiosPath = new List<string>();
                //codeStroke.VideosPath = new List<string>();

                //if (!string.IsNullOrEmpty(codeStroke.AttachmentsFolderRoot) && !string.IsNullOrWhiteSpace(codeStroke.AttachmentsFolderRoot))
                //{
                //    string path = this._RootPath + codeStroke.AttachmentsFolderRoot; //_environment.WebRootFileProvider.GetFileInfo(codeStroke.AttachmentsFolderRoot)?.PhysicalPath;
                //    if (Directory.Exists(path))
                //    {
                //        DirectoryInfo AttachFiles = new DirectoryInfo(path);
                //        foreach (var item in AttachFiles.GetFiles())
                //        {
                //            codeStroke.AttachmentsPath.Add(codeStroke.AttachmentsFolderRoot + "/" + item.Name);
                //        }
                //    }
                //}

                //if (!string.IsNullOrEmpty(codeStroke.AudioFolderRoot) && !string.IsNullOrWhiteSpace(codeStroke.AudioFolderRoot))
                //{
                //    string path = this._RootPath + codeStroke.AudioFolderRoot;  //_environment.WebRootFileProvider.GetFileInfo(codeStroke.AudioFolderRoot)?.PhysicalPath;
                //    if (Directory.Exists(path))
                //    {
                //        DirectoryInfo AudioFiles = new DirectoryInfo(path);
                //        foreach (var item in AudioFiles.GetFiles())
                //        {
                //            codeStroke.AudiosPath.Add(codeStroke.AudioFolderRoot + "/" + item.Name);
                //        }
                //    }
                //}

                //if (!string.IsNullOrEmpty(codeStroke.VideoFolderRoot) && !string.IsNullOrWhiteSpace(codeStroke.VideoFolderRoot))
                //{
                //    var path = this._RootPath + codeStroke.VideoFolderRoot;  //_environment.WebRootFileProvider.GetFileInfo(codeStroke.VideoFolderRoot)?.PhysicalPath;
                //    if (Directory.Exists(path))
                //    {
                //        DirectoryInfo VideoFiles = new DirectoryInfo(path);
                //        foreach (var item in VideoFiles.GetFiles())
                //        {
                //            codeStroke.VideosPath.Add(codeStroke.VideoFolderRoot + "/" + item.Name);
                //        }
                //    }
                //}

                return GetBlueDataById(row.CodeBlueId);
            }
            else
            {
                //var serviceLineIds = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == codeBlue.OrganizationIdFk && x.CodeIdFk == UCLEnums.Blue.ToInt() && !x.IsDeleted).Select(x => x.ServiceLineIds).FirstOrDefault();
                if (codeBlue.SelectedServiceLineIds != null && codeBlue.SelectedServiceLineIds != "")
                {
                    var serviceLineIds = codeBlue.SelectedServiceLineIds.ToIntList();
                    codeBlue.CreatedDate = DateTime.UtcNow;
                    var blue = AutoMapperHelper.MapSingleRow<CodeBlueVM, CodeBlue>(codeBlue);

                    if (codeBlue.Attachment != null && codeBlue.Attachment.Count > 0)
                    {
                        var RootPath = this._RootPath + "/Organizations";
                        string FileRoot = null;
                        List<string> Attachments = new();


                        FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeBlue.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                        FileRoot = Path.Combine(RootPath, FileRoot);
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, UCLEnums.Blue.ToString());
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, blue.CodeBlueId.ToString());
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, "Attachments");

                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        //else
                        //{
                        //    DirectoryInfo dir = new DirectoryInfo(FileRoot);
                        //    foreach (FileInfo fi in dir.GetFiles())
                        //    {
                        //        fi.Delete();
                        //    }
                        //}
                        foreach (var item in codeBlue.Attachment)
                        {
                            if (!string.IsNullOrEmpty(item.Base64Str))
                            {

                                var fileInfo = item.Base64Str.Split("base64,");
                                string fileExtension = fileInfo[0].GetFileExtenstion();
                                var ByteFile = Convert.FromBase64String(fileInfo[1]);
                                string FilePath = Path.Combine(FileRoot, item.FileName);

                                if (File.Exists(FilePath))
                                {
                                    long existingFileSize = 0;
                                    long newFileSize = ByteFile.LongLength;
                                    FileInfo ExistingfileInfo = new FileInfo(FilePath);
                                    existingFileSize = ExistingfileInfo.Length;

                                    if (existingFileSize > 0 && newFileSize != existingFileSize)
                                    {
                                        var alterFile = item.FileName.Split('.');
                                        string extention = alterFile.LastOrDefault();
                                        var alterFileName = alterFile.ToList();
                                        alterFileName.RemoveAt(alterFileName.Count - 1);
                                        string fileName = string.Join(".", alterFileName);
                                        fileName = fileName + "_" + HelperExtension.CreateRandomString(7) + "." + extention;
                                        FilePath = Path.Combine(FileRoot, fileName);
                                        using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                        {
                                            fs.Write(ByteFile);
                                        }
                                    }
                                    else
                                    {
                                        using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                        {
                                            fs.Write(ByteFile);
                                        }
                                    }
                                }
                                else
                                {
                                    using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        fs.Write(ByteFile);
                                    }
                                }
                            }


                        }
                        if (FileRoot != null && FileRoot != "")
                        {
                            codeBlue.AttachmentsFolderRoot = FileRoot.Replace(this._RootPath, "").Replace("\\", "/");
                        }
                    }
                    if (codeBlue.Videos != null && codeBlue.Videos.Count > 0)
                    {
                        var RootPath = this._RootPath + "/Organizations";
                        string FileRoot = null;
                        List<string> Attachments = new();


                        FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeBlue.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                        FileRoot = Path.Combine(RootPath, FileRoot);
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, UCLEnums.Blue.ToString());
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, blue.CodeBlueId.ToString());
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, "Videos");

                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        //else
                        //{
                        //    DirectoryInfo dir = new DirectoryInfo(FileRoot);
                        //    foreach (FileInfo fi in dir.GetFiles())
                        //    {
                        //        fi.Delete();
                        //    }
                        //}
                        foreach (var item in codeBlue.Videos)
                        {
                            if (!string.IsNullOrEmpty(item.Base64Str))
                            {

                                var fileInfo = item.Base64Str.Split("base64,");
                                string fileExtension = fileInfo[0].GetFileExtenstion();
                                var ByteFile = Convert.FromBase64String(fileInfo[1]);
                                string FilePath = Path.Combine(FileRoot, item.FileName);

                                if (File.Exists(FilePath))
                                {
                                    long existingFileSize = 0;
                                    long newFileSize = ByteFile.LongLength;
                                    FileInfo ExistingfileInfo = new FileInfo(FilePath);
                                    existingFileSize = ExistingfileInfo.Length;

                                    if (existingFileSize > 0 && newFileSize != existingFileSize)
                                    {
                                        var alterFile = item.FileName.Split('.');
                                        string extention = alterFile.LastOrDefault();
                                        var alterFileName = alterFile.ToList();
                                        alterFileName.RemoveAt(alterFileName.Count - 1);
                                        string fileName = string.Join(".", alterFileName);
                                        fileName = fileName + "_" + HelperExtension.CreateRandomString(7) + "." + extention;
                                        FilePath = Path.Combine(FileRoot, fileName);
                                        using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                        {
                                            fs.Write(ByteFile);
                                        }
                                    }
                                    else
                                    {
                                        using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                        {
                                            fs.Write(ByteFile);
                                        }
                                    }
                                }
                                else
                                {
                                    using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        fs.Write(ByteFile);
                                    }
                                }
                            }


                        }
                        if (FileRoot != null && FileRoot != "")
                        {
                            codeBlue.VideoFolderRoot = FileRoot.Replace(this._RootPath, "").Replace("\\", "/");
                        }
                    }
                    if (codeBlue.Audios != null && codeBlue.Audios.Count > 0)
                    {
                        var RootPath = this._RootPath + "/Organizations";
                        string FileRoot = null;
                        List<string> Attachments = new();


                        FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeBlue.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                        FileRoot = Path.Combine(RootPath, FileRoot);
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, UCLEnums.Blue.ToString());
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, blue.CodeBlueId.ToString());
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, "Audios");

                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        //else
                        //{
                        //    DirectoryInfo dir = new DirectoryInfo(FileRoot);
                        //    foreach (FileInfo fi in dir.GetFiles())
                        //    {
                        //        fi.Delete();
                        //    }
                        //}
                        foreach (var item in codeBlue.Audios)
                        {
                            if (!string.IsNullOrEmpty(item.Base64Str))
                            {

                                var fileInfo = item.Base64Str.Split("base64,");
                                string fileExtension = fileInfo[0].GetFileExtenstion();
                                var ByteFile = Convert.FromBase64String(fileInfo[1]);
                                string FilePath = Path.Combine(FileRoot, item.FileName);

                                if (File.Exists(FilePath))
                                {
                                    long existingFileSize = 0;
                                    long newFileSize = ByteFile.LongLength;
                                    FileInfo ExistingfileInfo = new FileInfo(FilePath);
                                    existingFileSize = ExistingfileInfo.Length;

                                    if (existingFileSize > 0 && newFileSize != existingFileSize)
                                    {
                                        var alterFile = item.FileName.Split('.');
                                        string extention = alterFile.LastOrDefault();
                                        var alterFileName = alterFile.ToList();
                                        alterFileName.RemoveAt(alterFileName.Count - 1);
                                        string fileName = string.Join(".", alterFileName);
                                        fileName = fileName + "_" + HelperExtension.CreateRandomString(7) + "." + extention;
                                        FilePath = Path.Combine(FileRoot, fileName);
                                        using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                        {
                                            fs.Write(ByteFile);
                                        }
                                    }
                                    else
                                    {
                                        using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                        {
                                            fs.Write(ByteFile);
                                        }
                                    }
                                }
                                else
                                {
                                    using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                    {
                                        fs.Write(ByteFile);
                                    }
                                }
                            }


                        }
                        if (FileRoot != null && FileRoot != "")
                        {
                            codeBlue.AudioFolderRoot = FileRoot.Replace(this._RootPath, "").Replace("\\", "/");
                        }
                    }

                    if (codeBlue.AttachmentsFolderRoot != null)
                    {
                        blue.Attachments = codeBlue.AttachmentsFolderRoot;
                    }
                    if (codeBlue.VideoFolderRoot != null)
                    {
                        blue.Video = codeBlue.VideoFolderRoot;
                    }
                    if (codeBlue.AudioFolderRoot != null)
                    {
                        blue.Audio = codeBlue.AudioFolderRoot;
                    }

                    this._codeBlueRepo.Insert(blue);

                    var codeServiceMappingList = new List<CodesServiceLinesMapping>();
                    foreach (var item in serviceLineIds)
                    {
                        var codeServiceMapping = new CodesServiceLinesMapping()
                        {
                            OrganizationIdFk = codeBlue.OrganizationIdFk,
                            CodeIdFk = UCLEnums.Blue.ToInt(),
                            ServiceLineIdFk = item,
                            ActiveCodeId = blue.CodeBlueId,
                            ActiveCodeName = UCLEnums.Blue.ToString()
                        };
                        codeServiceMappingList.Add(codeServiceMapping);
                    }
                    this._codesServiceLinesMappingRepo.Insert(codeServiceMappingList);

                    var UserChannelSid = (from us in this._userSchedulesRepo.Table
                                          join u in this._userRepo.Table on us.UserIdFk equals u.UserId
                                          where serviceLineIds.Contains(us.ServiceLineIdFk.Value) && us.ScheduleDateStart <= DateTime.UtcNow && us.ScheduleDateEnd >= DateTime.UtcNow && !us.IsDeleted && !u.IsDeleted
                                          select new { u.UserUniqueId, u.UserId }).ToList();
                    var loggedUser = this._userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId }).FirstOrDefault();
                    UserChannelSid.Add(loggedUser);
                    var conversationChannelAttributes = JsonConvert.SerializeObject(new Dictionary<string, Object>()
                                    {
                                        {ChannelAttributeEnums.ChannelType.ToString(), ChannelTypeEnums.EMS.ToString()},
                                        {ChannelAttributeEnums.CodeType.ToString(), ChannelTypeEnums.Blue.ToString()},
                                        {ChannelAttributeEnums.BlueId.ToString(), blue.CodeBlueId}
                                    }, Formatting.Indented);
                    List<ActiveCodesGroupMember> ACodeGroupMembers = new List<ActiveCodesGroupMember>();
                    if (UserChannelSid != null && UserChannelSid.Count > 0)
                    {
                        //string uniqueName = $"CONSULT_{Consult_Counter.Counter_Value.ToString()}";
                        string uniqueName = DateTime.Now.ToString("yyyyMMddHHmmssffff") + ApplicationSettings.UserId.ToString();
                        string friendlyName = blue.IsEms.HasValue && blue.IsEms.Value ? $"EMS Code {UCLEnums.Blue.ToString()} {blue.CodeBlueId}" : $"Active Code {UCLEnums.Blue.ToString()} {blue.CodeBlueId}";
                        var channel = _communication.createConversationChannel(friendlyName, uniqueName, conversationChannelAttributes);
                        UserChannelSid = UserChannelSid.Distinct().ToList();
                        foreach (var item in UserChannelSid)
                        {
                            try
                            {
                                var codeGroupMember = new ActiveCodesGroupMember()
                                {
                                    UserIdFk = item.UserId,
                                    ActiveCodeIdFk = blue.CodeBlueId,
                                    ActiveCodeName = UCLEnums.Blue.ToString(),
                                    IsAcknowledge = false,
                                    CreatedBy = ApplicationSettings.UserId,
                                    CreatedDate = DateTime.UtcNow,
                                    IsDeleted = false
                                };
                                ACodeGroupMembers.Add(codeGroupMember);
                                _communication.addNewUserToConversationChannel(channel.Sid, item.UserUniqueId);
                            }
                            catch (Exception ex)
                            {
                                ElmahExtensions.RiseError(ex);
                            }
                        }
                        var isMembersAdded = AddGroupMembers(ACodeGroupMembers);
                        var msg = new ConversationMessageVM();
                        msg.channelSid = channel.Sid;
                        msg.author = "System";
                        msg.attributes = "";
                        msg.body = $"<strong> {(codeBlue.IsEms.HasValue && codeBlue.IsEms.Value ? "EMS Code" : "Active Code")} {UCLEnums.Blue.ToString()} </strong></br></br>";
                        if (codeBlue.PatientName != null && codeBlue.PatientName != "")
                            msg.body += $"<strong>Patient Name: </strong> {codeBlue.PatientName} </br>";
                        msg.body += $"<strong>Dob: </strong> {codeBlue.Dob:MM-dd-yyyy} </br>";
                        msg.body += $"<strong>Last Well Known: </strong> {codeBlue.LastKnownWell:MM-dd-yyyy hh:mm tt} </br>";
                        if (codeBlue.ChiefComplant != null && codeBlue.ChiefComplant != "")
                            msg.body += $"<strong>Chief Complaint: </strong> {codeBlue.ChiefComplant} </br>";
                        if (codeBlue.Hpi != null && codeBlue.Hpi != "")
                            msg.body += $"<strong>Hpi: </strong> {codeBlue.Hpi} </br>";
                        var sendMsg = _communication.sendPushNotification(msg);
                    }
                    return GetBlueDataById(blue.CodeBlueId);
                }
                return new BaseResponse() { Status = HttpStatusCode.NotAcceptable, Message = "There is no Service Line in this organization related to Code Trauma" };
            }
        }

        public BaseResponse DeleteBlue(int blueId)
        {
            var row = this._codeBlueRepo.Table.Where(x => x.CodeBlueId == blueId && !x.IsDeleted).FirstOrDefault();
            row.IsDeleted = true;
            row.ModifiedBy = ApplicationSettings.UserId;
            row.ModifiedDate = DateTime.UtcNow;
            this._codeBlueRepo.Update(row);

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Deleted" };
        }

        #endregion


        public BaseResponse AddGroupMembers(List<ActiveCodesGroupMember> activeCodesGroup)
        {
            if (activeCodesGroup.Count > 0)
            {
                this._activeCodesGroupMembersRepo.Insert(activeCodesGroup);
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Inserted" };
            }
            else
            {
                return new BaseResponse() { Status = HttpStatusCode.NotModified, Message = "No Data Inserted" };
            }
        }

        #region EMS

        public BaseResponse GetActiveEMS(bool showAll, bool fromDashboard = false)
        {
            var activeEMS = this._dbContext.LoadStoredProcedure("md_getActiveEMS")
                            .WithSqlParam("@IsFromDashboard", fromDashboard)
                            .WithSqlParam("@currentUserId", ApplicationSettings.UserId)
                            .WithSqlParam("@isSuperAdmin", ApplicationSettings.isSuperAdmin)
                            .WithSqlParam("@showAll", showAll)
                            .ExecuteStoredProc<CodeStrokeVM>();

            //var orgDataList = new List<dynamic>();
            //foreach (var item in activeEMS.Select(x => x.OrganizationIdFk).Distinct().ToList())
            //{
            //    var orgData = GetHosplitalAddressObject(item);
            //    if (orgData != null)
            //        orgDataList.Add(orgData);
            //}
            foreach (var item in activeEMS)
            {
                //item.OrganizationData = orgDataList.Where(x => x.OrganizationId == item.OrganizationIdFk).FirstOrDefault();

                item.AttachmentsPath = new List<string>();
                item.AudiosPath = new List<string>();
                item.VideosPath = new List<string>();
                item.BloodThinnersTitle = new List<object>();
                //if (!string.IsNullOrEmpty(item.Attachments) && !string.IsNullOrWhiteSpace(item.Attachments))
                //{
                //    string path = this._RootPath + item.Attachments;
                //    if (Directory.Exists(path))
                //    {
                //        DirectoryInfo AttachFiles = new DirectoryInfo(path);
                //        foreach (var file in AttachFiles.GetFiles())
                //        {
                //            item.AttachmentsPath.Add(item.Attachments + "/" + file.Name);
                //        }
                //    }
                //}

                //if (!string.IsNullOrEmpty(item.Audio) && !string.IsNullOrWhiteSpace(item.Audio))
                //{
                //    string path = this._RootPath + item.Audio;
                //    if (Directory.Exists(path))
                //    {
                //        DirectoryInfo AudioFiles = new DirectoryInfo(path);
                //        foreach (var file in AudioFiles.GetFiles())
                //        {
                //            item.AudiosPath.Add(item.Audio + "/" + file.Name);
                //        }
                //    }
                //}

                //if (!string.IsNullOrEmpty(item.Video) && !string.IsNullOrWhiteSpace(item.Video))
                //{
                //    var path = this._RootPath + item.Video;
                //    if (Directory.Exists(path))
                //    {
                //        DirectoryInfo VideoFiles = new DirectoryInfo(path);
                //        foreach (var file in VideoFiles.GetFiles())
                //        {
                //            item.VideosPath.Add(item.Video + "/" + file.Name);
                //        }
                //    }
                //}

                //var serviceIds = this._activeCodeRepo.Table.Where(s => s.OrganizationIdFk == item.OrganizationIdFk && s.CodeIdFk == item.CodeName.GetActiveCodeId() && !s.IsDeleted).Select(s => new { s.ServiceLineIds, s.DefaultServiceLineId }).FirstOrDefault();

                //var serviceLineIds = (from s in this._codesServiceLinesMappingRepo.Table
                //                      where s.OrganizationIdFk == item.OrganizationIdFk && s.CodeIdFk == item.CodeName.GetActiveCodeId()
                //                      && s.ActiveCodeId == item.Id && s.ActiveCodeName == item.CodeName && s.ServiceLineIdFk != serviceIds.DefaultServiceLineId
                //                      select s.ServiceLineIdFk).ToList();
                //item.ServiceLines = this._serviceLineRepo.Table.Where(s => serviceIds.ServiceLineIds.ToIntList().Distinct().Contains(s.ServiceLineId) && s.ServiceLineId != serviceIds.DefaultServiceLineId && !s.IsDeleted).Select(s => new ServiceLineVM() { ServiceLineId = s.ServiceLineId, ServiceName = s.ServiceName, IsSelected = serviceLineIds.Contains(s.ServiceLineId) }).ToList();
                //item.DefaultServiceLineId = serviceIds.DefaultServiceLineId;
                //item.DefaultServiceLine = this._serviceLineRepo.Table.Where(s => s.ServiceLineId == serviceIds.DefaultServiceLineId && !s.IsDeleted).Select(s => new ServiceLineVM() { ServiceLineId = s.ServiceLineId, ServiceName = s.ServiceName }).FirstOrDefault();
                //item.SelectedServiceLineIds = string.Join(",", serviceLineIds);

                item.LastKnownWellStr = item.LastKnownWell?.ToString("yyyy-MM-dd hh:mm:ss tt");
                //item.OrganizationData = orgData;
                item.GenderTitle = _controlListDetailsRepo.Table.Where(g => g.ControlListDetailId == item.Gender).Select(g => g.Title).FirstOrDefault();
                item.BloodThinnersTitle.AddRange(_controlListDetailsRepo.Table.Where(b => item.BloodThinners.ToIntList().Contains(b.ControlListDetailId)).Select(b => new { Id = b.ControlListDetailId, b.Title }).ToList());
            }
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Returned", Body = activeEMS };
        }

        #endregion


        #region Map & addresses

        public object GetHosplitalAddressObject(int orgId)
        {
            var org = this._orgRepo.Table.Where(o => o.OrganizationId == orgId && !o.IsDeleted).FirstOrDefault();
            if (org != null)
            {
                var state = this._controlListDetailsRepo.Table.Where(s => s.ControlListDetailId == org.StateIdFk).Select(s => new { Id = s.ControlListDetailId, s.Title, s.Description }).FirstOrDefault();
                if (state != null)
                {
                    string add = $"{org.PrimaryAddress} {org.City}, {state.Title} {org.Zip}";
                    string url = "https://maps.googleapis.com/maps/api/geocode/json?address=" + add.Replace(" ", "%20") + "&key=" + this._GoogleApiKey;
                    var googleApiLatLng = this._httpClient.GetAsync(url).Result;

                    dynamic Apiresults = googleApiLatLng["results"];
                    var formatted_address = Convert.ToString(Apiresults[0]["formatted_address"]);
                    var geometry = Apiresults[0]["geometry"];
                    var location = geometry["location"];
                    var longLat = new List<double> { Convert.ToDouble(location["lat"]), Convert.ToDouble(location["lng"]) };

                    return new { OrganizationId = org.OrganizationId, Address = formatted_address, DestinationCoords = string.Join(",", longLat), org.OrganizationName };
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public BaseResponse GetHospitalsOfStatesByCodeId(int codeId, string latlng)
        {
            var googleApiResult = this._httpClient.GetAsync("https://maps.googleapis.com/maps/api/geocode/json?latlng=" + latlng + "&key=" + this._GoogleApiKey).Result;

            dynamic results = googleApiResult["results"];
            var address = results[0]["address_components"];
            var StateName = "";
            for (int i = 0; i < address.Count; i++)
            {
                var addressType = address[i].types;
                for (int j = 0; j < addressType.Count; j++)
                {
                    if (addressType[j] == "administrative_area_level_1")
                    {
                        StateName = address[i].long_name;
                    }
                }
            }

            if (StateName != "")
            {
                var stateId = this._controlListDetailsRepo.Table.Where(x => x.Description.Contains(StateName) && !x.IsDeleted).Select(x => new { Id = x.ControlListDetailId, x.Title }).FirstOrDefault();
                if (stateId != null)
                {
                    var orgsAddress = this._orgRepo.Table.Where(x => x.ActiveCodes.Contains(codeId.ToString()) && x.StateIdFk == stateId.Id && !x.IsDeleted).ToList();

                    List<object> objList = new List<object>();

                    foreach (var item in orgsAddress)
                    {
                        string add = $"{item.PrimaryAddress} {item.City}, {stateId.Title} {item.Zip}";
                        string url = "https://maps.googleapis.com/maps/api/geocode/json?address=" + add.Replace(" ", "%20") + "&key=" + this._GoogleApiKey;
                        var googleApiLatLng = this._httpClient.GetAsync(url).Result;

                        dynamic Apiresults = googleApiLatLng["results"];
                        var formatted_address = Convert.ToString(Apiresults[0]["formatted_address"]);
                        var geometry = Apiresults[0]["geometry"];
                        var location = geometry["location"];
                        var longLat = new List<double> { Convert.ToDouble(location.lat), Convert.ToDouble(location.lng) };

                        objList.Add(new { OrganizationId = item.OrganizationId, Address = formatted_address, lat = longLat[0], lng = longLat[1], title = item.OrganizationName });
                    }
                    return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Addresses Returned", Body = objList };
                }
                else
                {
                    return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "State Not Found" };
                }
            }
            else
            {
                return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "State Not Found" };
            }
        }

        #endregion
    }
}
