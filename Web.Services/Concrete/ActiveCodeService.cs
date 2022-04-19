using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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
        private IRepository<CodeStrokeGroupMember> _StrokeCodeGroupMembersRepo;
        private IRepository<CodeStemigroupMember> _STEMICodeGroupMembersRepo;
        private IRepository<CodeSepsisGroupMember> _SepsisCodeGroupMembersRepo;
        private IRepository<CodeTraumaGroupMember> _TraumaCodeGroupMembersRepo;
        private IRepository<CodeBlueGroupMember> _BlueCodeGroupMembersRepo;
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
            IRepository<CodeStrokeGroupMember> StrokeCodeGroupMembersRepo,
            IRepository<CodeStemigroupMember> STEMICodeGroupMembersRepo,
            IRepository<CodeSepsisGroupMember> SepsisCodeGroupMembersRepo,
            IRepository<CodeTraumaGroupMember> TraumaCodeGroupMembersRepo,
            IRepository<CodeBlueGroupMember> BlueCodeGroupMembersRepo,
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

            this._StrokeCodeGroupMembersRepo = StrokeCodeGroupMembersRepo;
            this._STEMICodeGroupMembersRepo = STEMICodeGroupMembersRepo;
            this._SepsisCodeGroupMembersRepo = SepsisCodeGroupMembersRepo;
            this._TraumaCodeGroupMembersRepo = TraumaCodeGroupMembersRepo;
            this._BlueCodeGroupMembersRepo = BlueCodeGroupMembersRepo;

            this._codesServiceLinesMappingRepo = codesServiceLinesMappingRepo;
            this._RootPath = this._config["FilePath:Path"].ToString();
            this._GoogleApiKey = this._config["GoogleApi:Key"].ToString();
        }

        #region Active Code

        public BaseResponse GetActivatedCodesByOrgId(int orgId, bool status)
        {
            var codes = this._dbContext.LoadStoredProcedure("md_getActivatedCodesForOrg")
                .WithSqlParam("@pOrgId", orgId)
                .WithSqlParam("@pstatus", status)
                .ExecuteStoredProc<ActiveCodeVM>();
            foreach (var item in codes)
            {
                item.DefaultServiceLineTeamList = this._serviceLineRepo.Table.Where(x => !x.IsDeleted && item.DefaultServiceLineTeam.ToIntList().Contains(x.ServiceLineId)).Select(x => new ServiceLineVM() { ServiceLineId = x.ServiceLineId, ServiceName = x.ServiceName }).ToList();
                item.ServiceLineTeam1List = this._serviceLineRepo.Table.Where(x => !x.IsDeleted && item.ServiceLineTeam1.ToIntList().Contains(x.ServiceLineId)).Select(x => new ServiceLineVM() { ServiceLineId = x.ServiceLineId, ServiceName = x.ServiceName }).ToList();
                item.ServiceLineTeam2List = this._serviceLineRepo.Table.Where(x => !x.IsDeleted && item.ServiceLineTeam2.ToIntList().Contains(x.ServiceLineId)).Select(x => new ServiceLineVM() { ServiceLineId = x.ServiceLineId, ServiceName = x.ServiceName }).ToList();
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
                    //row.DefaultServiceLineId = item.DefaultServiceLineId;
                    row.DefaultServiceLineTeam = item.DefaultServiceLineTeam;
                    row.ServiceLineTeam1 = item.ServiceLineTeam1;
                    row.ServiceLineTeam2 = item.ServiceLineTeam2;
                    row.ModifiedBy = item.ModifiedBy;
                    row.ModifiedDate = DateTime.Now;
                    row.IsDeleted = false;
                    update.Add(row);
                }
                else
                {
                    var row = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == item.OrganizationIdFk && x.CodeIdFk == item.CodeIdFk && x.Type == item.Type && !x.IsDeleted).FirstOrDefault();
                    if (row != null)
                    {
                        var oldServicesDefault = new List<string>();
                        if (!string.IsNullOrEmpty(row.DefaultServiceLineTeam) && !string.IsNullOrWhiteSpace(row.DefaultServiceLineTeam))
                        {
                            oldServicesDefault = row.DefaultServiceLineTeam.Split(",").ToList();
                        }
                        var newServicesDefault = new List<string>();
                        if (!string.IsNullOrEmpty(item.DefaultServiceLineTeam) && !string.IsNullOrWhiteSpace(item.DefaultServiceLineTeam))
                        {
                            newServicesDefault = item.DefaultServiceLineTeam.Split(",").ToList();
                        }
                        oldServicesDefault.AddRange(newServicesDefault);
                        var oldServices1 = new List<string>();
                        if (!string.IsNullOrEmpty(row.ServiceLineTeam1) && !string.IsNullOrWhiteSpace(row.ServiceLineTeam1))
                        {
                            oldServices1 = row.ServiceLineTeam1.Split(",").ToList();
                        }
                        var newServices1 = new List<string>();
                        if (!string.IsNullOrEmpty(item.ServiceLineTeam1) && !string.IsNullOrWhiteSpace(item.ServiceLineTeam1))
                        {
                            newServices1 = item.ServiceLineTeam1.Split(",").ToList();
                        }
                        oldServices1.AddRange(newServices1);
                        var oldServices2 = new List<string>();
                        if (!string.IsNullOrEmpty(row.ServiceLineTeam2) && !string.IsNullOrWhiteSpace(row.ServiceLineTeam2))
                        {
                            oldServices2 = row.ServiceLineTeam2.Split(",").ToList();
                        }
                        var newServices2 = new List<string>();
                        if (!string.IsNullOrEmpty(item.ServiceLineTeam2) && !string.IsNullOrWhiteSpace(item.ServiceLineTeam2))
                        {
                            newServices2 = item.ServiceLineTeam2.Split(",").ToList();
                        }
                        oldServices2.AddRange(newServices2);

                        //row.DefaultServiceLineId = item.DefaultServiceLineId;
                        row.DefaultServiceLineTeam = string.Join(",", oldServicesDefault.Distinct());
                        row.ServiceLineTeam1 = string.Join(",", oldServices1.Distinct());
                        row.ServiceLineTeam2 = string.Join(",", oldServices2.Distinct());
                        row.ModifiedBy = item.CreatedBy;
                        row.ModifiedDate = DateTime.Now;
                        row.IsDeleted = false;
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

        public BaseResponse DetachActiveCodes(int activeCodeId, bool status)
        {
            var row = this._activeCodeRepo.Table.Where(x => x.ActiveCodeId == activeCodeId).FirstOrDefault();
            row.ModifiedBy = ApplicationSettings.UserId;
            row.ModifiedDate = DateTime.UtcNow;
            row.IsDeleted = status;
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
                    label = "Inhouse Codes",
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
                                                  label= "Inhouse Codes",
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
                var rootPath = this._codeStrokeRepo.Table.Where(x => x.CodeStrokeId == files.Id).Select(files.Type, "IsEMS", "OrganizationIdFk").FirstOrDefault();
                string path = _environment.WebRootFileProvider.GetFileInfo(rootPath + '/' + files.FileName)?.PhysicalPath;
                File.Delete(path);

                var UserChannelSid = (from u in this._userRepo.Table
                                      join gm in this._StrokeCodeGroupMembersRepo.Table on u.UserId equals gm.UserIdFk
                                      where gm.StrokeCodeIdFk == files.Id && !u.IsDeleted
                                      select u.UserChannelSid).Distinct().ToList();

                var notification = new PushNotificationVM()
                {
                    Id = files.Id,
                    OrgId = rootPath.OrganizationIdFk,
                    UserChannelSid = UserChannelSid,
                    From = AuthorEnums.Stroke.ToString(),
                    Msg = (rootPath.IsEms.HasValue && rootPath.IsEms.Value ? "EMS" : "Active") + " Code Stroke Form is Changed",
                    RouteLink1 = "/Home/Activate%20Code/code-strok-form",
                    RouteLink2 = "/Home/EMS/activateCode"
                };

                _communication.pushNotification(notification);
            }
            else if (files.CodeType == AuthorEnums.Sepsis.ToString())
            {
                var rootPath = this._codeSepsisRepo.Table.Where(x => x.CodeSepsisId == files.Id).Select(files.Type, "IsEMS", "OrganizationIdFk").FirstOrDefault();
                string path = _environment.WebRootFileProvider.GetFileInfo(rootPath + '/' + files.FileName)?.PhysicalPath;
                File.Delete(path);

                var UserChannelSid = (from u in this._userRepo.Table
                                      join gm in this._SepsisCodeGroupMembersRepo.Table on u.UserId equals gm.UserIdFk
                                      where gm.SepsisCodeIdFk == files.Id && !u.IsDeleted
                                      select u.UserChannelSid).Distinct().ToList();

                var notification = new PushNotificationVM()
                {
                    Id = files.Id,
                    OrgId = rootPath.OrganizationIdFk,
                    UserChannelSid = UserChannelSid,
                    From = AuthorEnums.Sepsis.ToString(),
                    Msg = (rootPath.IsEms.HasValue && rootPath.IsEms.Value ? "EMS" : "Active") + " Code Sepsis Form is Changed",
                    RouteLink1 = "/Home/Activate%20Code/code-sepsis-form",
                    RouteLink2 = "/Home/EMS/activateCode"
                };

                _communication.pushNotification(notification);
            }
            else if (files.CodeType == AuthorEnums.STEMI.ToString())
            {
                var rootPath = this._codeSTEMIRepo.Table.Where(x => x.CodeStemiid == files.Id).Select(files.Type, "IsEMS", "OrganizationIdFk").FirstOrDefault();
                string path = _environment.WebRootFileProvider.GetFileInfo(rootPath + '/' + files.FileName)?.PhysicalPath;
                File.Delete(path);
                var UserChannelSid = (from u in this._userRepo.Table
                                      join gm in this._STEMICodeGroupMembersRepo.Table on u.UserId equals gm.UserIdFk
                                      where gm.StemicodeIdFk == files.Id && !u.IsDeleted
                                      select u.UserChannelSid).Distinct().ToList();

                var notification = new PushNotificationVM()
                {
                    Id = files.Id,
                    OrgId = rootPath.OrganizationIdFk,
                    UserChannelSid = UserChannelSid,
                    From = AuthorEnums.STEMI.ToString(),
                    Msg = (rootPath.IsEms.HasValue && rootPath.IsEms.Value ? "EMS" : "Active") + " Code STEMI Form is Changed",
                    RouteLink1 = "/Home/Activate%20Code/code-stemi-form",
                    RouteLink2 = "/Home/EMS/activateCode"
                };

                _communication.pushNotification(notification);
            }
            else if (files.CodeType == AuthorEnums.Trauma.ToString())
            {
                var rootPath = this._codeTrumaRepo.Table.Where(x => x.CodeTraumaId == files.Id).Select(files.Type, "IsEMS", "OrganizationIdFk").FirstOrDefault();
                string path = _environment.WebRootFileProvider.GetFileInfo(rootPath + '/' + files.FileName)?.PhysicalPath;
                File.Delete(path);
                var UserChannelSid = (from u in this._userRepo.Table
                                      join gm in this._TraumaCodeGroupMembersRepo.Table on u.UserId equals gm.UserIdFk
                                      where gm.TraumaCodeIdFk == files.Id && !u.IsDeleted
                                      select u.UserChannelSid).Distinct().ToList();

                var notification = new PushNotificationVM()
                {
                    Id = files.Id,
                    OrgId = rootPath.OrganizationIdFk,
                    UserChannelSid = UserChannelSid,
                    From = AuthorEnums.Trauma.ToString(),
                    Msg = (rootPath.IsEms.HasValue && rootPath.IsEms.Value ? "EMS" : "Active") + " Code Trauma Form is Changed",
                    RouteLink1 = "/Home/Activate%20Code/code-trauma-form",
                    RouteLink2 = "/Home/EMS/activateCode"
                };

                _communication.pushNotification(notification);
            }
            else if (files.CodeType == AuthorEnums.Blue.ToString())
            {
                var rootPath = this._codeBlueRepo.Table.Where(x => x.CodeBlueId == files.Id).Select(files.Type, "IsEMS", "OrganizationIdFk").FirstOrDefault();
                string path = _environment.WebRootFileProvider.GetFileInfo(rootPath + '/' + files.FileName)?.PhysicalPath;
                File.Delete(path);
                var UserChannelSid = (from u in this._userRepo.Table
                                      join gm in this._BlueCodeGroupMembersRepo.Table on u.UserId equals gm.UserIdFk
                                      where gm.BlueCodeIdFk == files.Id && !u.IsDeleted
                                      select u.UserChannelSid).Distinct().ToList();

                var notification = new PushNotificationVM()
                {
                    Id = files.Id,
                    OrgId = rootPath.OrganizationIdFk,
                    UserChannelSid = UserChannelSid,
                    From = AuthorEnums.Blue.ToString(),
                    Msg = (rootPath.IsEms.HasValue && rootPath.IsEms.Value ? "EMS" : "Active") + " Code Blue Form is Changed",
                    RouteLink1 = "/Home/Activate%20Code/code-blue-form",
                    RouteLink2 = "/Home/EMS/activateCode"
                };

                _communication.pushNotification(notification);
            }


            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "File Deleted Successfully" };
        }

        #endregion
        #region Code Stroke

        public BaseResponse GetAllStrokeCode(ActiveCodeVM activeCode)
        {
            var objList = this._dbContext.LoadStoredProcedure("md_getAllActiveCodesOrEMS_Dynamic")
                            .WithSqlParam("@status", activeCode.Status)
                            .WithSqlParam("@codeName", UCLEnums.Stroke.ToString())
                            .WithSqlParam("@IsSuperAdmin", ApplicationSettings.isSuperAdmin)
                            .WithSqlParam("@showAll", activeCode.showAllActiveCodes)
                            .WithSqlParam("@userId", ApplicationSettings.UserId)
                            .WithSqlParam("@organizationId", activeCode.OrganizationIdFk)
                            .WithSqlParam("@page", activeCode.PageNumber)
                            .WithSqlParam("@size", activeCode.Rows)
                            .WithSqlParam("@sortOrder", activeCode.SortOrder)
                            .WithSqlParam("@sortCol", activeCode.SortCol)
                            .WithSqlParam("@filterVal", activeCode.FilterVal)
                            .ExecuteStoredProc<ActiveOrEMSCodesVM>();

            objList.ForEach(x =>
            {
                x.BloodThinnersTitle = new List<object>();
                x.BloodThinnersTitle.AddRange(_controlListDetailsRepo.Table.Where(b => x.BloodThinners.ToIntList().Contains(b.ControlListDetailId)).Select(b => new { Id = b.ControlListDetailId, b.Title }).ToList());
            });

            int totalRecords = 0;
            if (objList.Count > 0)
            {
                totalRecords = objList.Select(x => x.Total_Records).FirstOrDefault();
            }
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Returned", Body = new { totalRecords, objList } };

            //if (ApplicationSettings.isSuperAdmin)
            //{
            //    strokeData = this._codeStrokeRepo.Table.Where(x => x.OrganizationIdFk == activeCode.OrganizationIdFk && !x.IsDeleted).OrderByDescending(x => x.CodeStrokeId).ToList();
            //}
            //else if (activeCode.showAllActiveCodes)
            //{
            //    strokeData = this._codeStrokeRepo.Table.Where(x => x.OrganizationIdFk == activeCode.OrganizationIdFk && !x.IsDeleted).OrderByDescending(x => x.CodeStrokeId).ToList();
            //}
            //else
            //{
            //    strokeData = (from cs in this._codeStrokeRepo.Table
            //                  join agm in this._activeCodesGroupMembersRepo.Table on cs.CodeStrokeId equals agm.ActiveCodeIdFk
            //                  where agm.UserIdFk == ApplicationSettings.UserId && agm.ActiveCodeName == UCLEnums.Stroke.ToString() && !cs.IsDeleted
            //                  select cs).OrderByDescending(x => x.CodeStrokeId).AsQueryable().ToList();

            //    //strokeData = this._codeStrokeRepo.Table.Where(x => x.CreatedBy == ApplicationSettings.UserId && !x.IsDeleted).OrderByDescending(x => x.CodeStrokeId).ToList();
            //}
            //var strokeDataVM = AutoMapperHelper.MapList<CodeStroke, CodeStrokeVM>(strokeData);
            //var orgData = GetHosplitalAddressObject(activeCode.OrganizationIdFk);

            //strokeDataVM.ForEach(x =>
            //{
            //    x.AttachmentsPath = new List<string>();
            //    x.AudiosPath = new List<string>();
            //    x.VideosPath = new List<string>();
            //    x.OrganizationData = new object();
            //    x.BloodThinnersTitle = new List<object>();
            //    x.ServiceLines = new List<ServiceLineVM>();

            //    //if (!string.IsNullOrEmpty(x.Attachments) && !string.IsNullOrWhiteSpace(x.Attachments))
            //    //{
            //    //    string path = this._RootPath + x.Attachments; //this._RootPath  + x.Attachments;
            //    //    if (Directory.Exists(path))
            //    //    {
            //    //        DirectoryInfo AttachFiles = new DirectoryInfo(path);
            //    //        foreach (var item in AttachFiles.GetFiles())
            //    //        {
            //    //            x.AttachmentsPath.Add(x.Attachments + "/" + item.Name);
            //    //        }
            //    //    }
            //    //}

            //    //if (!string.IsNullOrEmpty(x.Audio) && !string.IsNullOrWhiteSpace(x.Audio))
            //    //{
            //    //    string path = this._RootPath + x.Audio; //this._RootPath  + x.Audio;
            //    //    if (Directory.Exists(path))
            //    //    {
            //    //        DirectoryInfo AudioFiles = new DirectoryInfo(path);
            //    //        foreach (var item in AudioFiles.GetFiles())
            //    //        {
            //    //            x.AudiosPath.Add(x.Audio + "/" + item.Name);
            //    //        }
            //    //    }
            //    //}

            //    //if (!string.IsNullOrEmpty(x.Video) && !string.IsNullOrWhiteSpace(x.Video))
            //    //{
            //    //    var path = this._RootPath + x.Video; //this._RootPath  + x.Video;
            //    //    if (Directory.Exists(path))
            //    //    {
            //    //        DirectoryInfo VideoFiles = new DirectoryInfo(path);
            //    //        foreach (var item in VideoFiles.GetFiles())
            //    //        {
            //    //            x.VideosPath.Add(x.Video + "/" + item.Name);
            //    //        }
            //    //    }
            //    //}


            //    //var serviceIds = this._activeCodeRepo.Table.Where(s => s.OrganizationIdFk == x.OrganizationIdFk && s.CodeIdFk == UCLEnums.Stroke.ToInt() && !s.IsDeleted).Select(s => new { s.ServiceLineIds, s.DefaultServiceLineId }).FirstOrDefault();
            //    //var serviceLineIds = (from s in this._codesServiceLinesMappingRepo.Table
            //    //                      where s.OrganizationIdFk == x.OrganizationIdFk && s.CodeIdFk == UCLEnums.Stroke.ToInt()
            //    //                      && s.ActiveCodeId == x.CodeStrokeId && s.ActiveCodeName == UCLEnums.Stroke.ToString() && s.ServiceLineIdFk != serviceIds.DefaultServiceLineId
            //    //                      select s.ServiceLineIdFk).ToList();
            //    //x.ServiceLines = this._serviceLineRepo.Table.Where(s => serviceIds.ServiceLineIds.ToIntList().Distinct().Contains(s.ServiceLineId) && s.ServiceLineId != serviceIds.DefaultServiceLineId && !s.IsDeleted).Select(s => new ServiceLineVM() { ServiceLineId = s.ServiceLineId, ServiceName = s.ServiceName, IsSelected = serviceLineIds.Contains(s.ServiceLineId) }).ToList();
            //    //x.DefaultServiceLineId = serviceIds.DefaultServiceLineId;
            //    //x.DefaultServiceLine = this._serviceLineRepo.Table.Where(s => s.ServiceLineId == serviceIds.DefaultServiceLineId && !s.IsDeleted).Select(s => new ServiceLineVM() { ServiceLineId = s.ServiceLineId, ServiceName = s.ServiceName }).FirstOrDefault();
            //    //x.SelectedServiceLineIds = string.Join(",", serviceLineIds);
            //    x.LastKnownWellStr = x.LastKnownWell?.ToString("yyyy-MM-dd hh:mm:ss tt");
            //    x.DobStr = x.Dob?.ToString("yyyy-MM-dd hh:mm:ss tt");
            //    //x.OrganizationData = orgData;
            //    x.GenderTitle = _controlListDetailsRepo.Table.Where(g => g.ControlListDetailId == x.Gender).Select(g => g.Title).FirstOrDefault();
            //    x.BloodThinnersTitle.AddRange(_controlListDetailsRepo.Table.Where(b => x.BloodThinners.ToIntList().Contains(b.ControlListDetailId)).Select(b => new { Id = b.ControlListDetailId, b.Title }).ToList());
            //});
            //return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Returned", Body = strokeDataVM };
        }

        public BaseResponse GetStrokeDataById(int strokeId)
        {
            var strokeData = this._codeStrokeRepo.Table.Where(x => x.CodeStrokeId == strokeId).FirstOrDefault();
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
                string Type = StrokeDataVM.IsEms.HasValue && StrokeDataVM.IsEms.Value ? "EMS Code" : "Inhouse Code";
                var serviceIds = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == strokeData.OrganizationIdFk && x.CodeIdFk == UCLEnums.Stroke.ToInt() && x.Type == Type && !x.IsDeleted).Select(x => new { x.DefaultServiceLineTeam, x.ServiceLineTeam1, x.ServiceLineTeam2 }).FirstOrDefault();
                var serviceLineIds = (from x in this._codesServiceLinesMappingRepo.Table
                                      where x.OrganizationIdFk == strokeData.OrganizationIdFk && x.CodeIdFk == UCLEnums.Stroke.ToInt()
                                      && x.ActiveCodeId == strokeData.CodeStrokeId && x.ActiveCodeName == UCLEnums.Stroke.ToString()
                                      select new { x.DefaultServiceLineIdFk, x.ServiceLineId1Fk, x.ServiceLineId2Fk }).FirstOrDefault();

                if (serviceIds != null && serviceLineIds != null)
                {
                    var defaultIds = serviceIds.DefaultServiceLineTeam.ToIntList().Where(x => serviceLineIds.DefaultServiceLineIdFk.ToIntList().Contains(x)).Distinct().ToList();
                    var team1 = serviceIds.ServiceLineTeam1.ToIntList().Where(x => serviceLineIds.ServiceLineId1Fk.ToIntList().Contains(x)).Distinct().ToList();
                    var team2 = serviceIds.ServiceLineTeam2.ToIntList().Where(x => serviceLineIds.ServiceLineId2Fk.ToIntList().Contains(x)).Distinct().ToList();
                    StrokeDataVM.DefaultServiceLineTeam = this._serviceLineRepo.Table.Where(x => serviceIds.DefaultServiceLineTeam.ToIntList().Distinct().Contains(x.ServiceLineId) && !x.IsDeleted).Select(x => new ServiceLineVM() { ServiceLineId = x.ServiceLineId, ServiceName = x.ServiceName, IsSelected = defaultIds.Contains(x.ServiceLineId) }).ToList();
                    StrokeDataVM.ServiceLineTeam1 = this._serviceLineRepo.Table.Where(x => serviceIds.ServiceLineTeam1.ToIntList().Distinct().Contains(x.ServiceLineId) && !x.IsDeleted).Select(x => new ServiceLineVM() { ServiceLineId = x.ServiceLineId, ServiceName = x.ServiceName, IsSelected = team1.Contains(x.ServiceLineId) }).ToList();
                    StrokeDataVM.ServiceLineTeam2 = this._serviceLineRepo.Table.Where(x => serviceIds.ServiceLineTeam2.ToIntList().Distinct().Contains(x.ServiceLineId) && !x.IsDeleted).Select(x => new ServiceLineVM() { ServiceLineId = x.ServiceLineId, ServiceName = x.ServiceName, IsSelected = team2.Contains(x.ServiceLineId) }).ToList();
                }

                if (StrokeDataVM.IsEms.HasValue && StrokeDataVM.IsEms.Value)
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

                if (codeStroke.DefaultServiceLineIds == null || codeStroke.DefaultServiceLineIds == "")
                {
                    string IsEMS = row.IsEms.HasValue && row.IsEms.Value ? "EMS Code" : "Inhouse Code";
                    codeStroke.DefaultServiceLineIds = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == codeStroke.OrganizationIdFk && x.CodeIdFk == UCLEnums.Stroke.ToInt() && x.Type == IsEMS && !x.IsDeleted).Select(x => x.DefaultServiceLineTeam).FirstOrDefault();
                }

                if (codeStroke.DefaultServiceLineIds != null && codeStroke.DefaultServiceLineIds != "")
                {
                    var DefaultServiceLineIds = codeStroke.DefaultServiceLineIds.ToIntList();
                    var ServiceLineTeam1Ids = codeStroke.ServiceLineTeam1Ids.ToIntList();
                    var ServiceLineTeam2Ids = codeStroke.ServiceLineTeam2Ids.ToIntList();

                    var codeServiceMapping = this._codesServiceLinesMappingRepo.Table.Where(x => x.OrganizationIdFk == row.OrganizationIdFk && x.CodeIdFk == UCLEnums.Stroke.ToInt() && x.ActiveCodeId == row.CodeStrokeId).ToList();
                    if (codeServiceMapping.Count > 0)
                        this._codesServiceLinesMappingRepo.DeleteRange(codeServiceMapping);

                    var codeService = new CodesServiceLinesMapping()
                    {
                        OrganizationIdFk = row.OrganizationIdFk,
                        CodeIdFk = UCLEnums.Stroke.ToInt(),
                        DefaultServiceLineIdFk = codeStroke.DefaultServiceLineIds,
                        ServiceLineId1Fk = codeStroke.ServiceLineTeam1Ids,
                        ServiceLineId2Fk = codeStroke.ServiceLineTeam2Ids,
                        ActiveCodeId = row.CodeStrokeId,
                        ActiveCodeName = UCLEnums.Stroke.ToString()
                    };
                    this._codesServiceLinesMappingRepo.Insert(codeService);

                    //var channel = this._StrokeCodeGroupMembersRepo.Table.Where(x => x.StrokeCodeIdFk == row.CodeStrokeId && !x.IsDeleted).ToList();

                    var UserChannelSid = (from us in this._userSchedulesRepo.Table
                                          join u in this._userRepo.Table on us.UserIdFk equals u.UserId
                                          where (DefaultServiceLineIds.Contains(us.ServiceLineIdFk.Value) || ServiceLineTeam1Ids.Contains(us.ServiceLineIdFk.Value) || ServiceLineTeam2Ids.Contains(us.ServiceLineIdFk.Value)) && us.ScheduleDateStart <= DateTime.UtcNow && us.ScheduleDateEnd >= DateTime.UtcNow && !us.IsDeleted && !u.IsDeleted
                                          select new { u.UserUniqueId, u.UserId }).Distinct().ToList();
                    var superAdmins = this._userRepo.Table.Where(x => x.IsInGroup && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId }).ToList();
                    UserChannelSid.AddRange(superAdmins);
                    var loggedInUser = this._userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId }).FirstOrDefault();
                    UserChannelSid.Add(loggedInUser);

                    if (row.ChannelSid != null && row.ChannelSid != "")
                    {
                        var channelSid = row.ChannelSid; //channel.Select(x => x.ChannelSid).FirstOrDefault();

                        var groupMembers = this._StrokeCodeGroupMembersRepo.Table.Where(x => x.StrokeCodeIdFk == row.CodeStrokeId).ToList();
                        this._StrokeCodeGroupMembersRepo.DeleteRange(groupMembers);
                        //this._StrokeCodeGroupMembersRepo.DeleteRange(channel);
                        bool isDeleted = _communication.DeleteUserToConversationChannel(channelSid);
                        List<CodeStrokeGroupMember> ACodeGroupMembers = new List<CodeStrokeGroupMember>();
                        foreach (var item in UserChannelSid.Distinct())
                        {
                            try
                            {
                                var codeGroupMember = new CodeStrokeGroupMember()
                                {
                                    UserIdFk = item.UserId,
                                    StrokeCodeIdFk = row.CodeStrokeId,
                                    //ActiveCodeName = UCLEnums.Stroke.ToString(),
                                    IsAcknowledge = false,
                                    CreatedBy = ApplicationSettings.UserId,
                                    CreatedDate = DateTime.UtcNow,
                                    IsDeleted = false
                                };
                                ACodeGroupMembers.Add(codeGroupMember);
                                _communication.addNewUserToConversationChannel(channelSid, item.UserUniqueId);
                            }
                            catch (Exception ex)
                            {
                                //ElmahExtensions.RiseError(ex);
                            }
                        }
                        this._StrokeCodeGroupMembersRepo.Insert(ACodeGroupMembers);

                    }


                    var notification = new PushNotificationVM()
                    {
                        Id = row.CodeStrokeId,
                        OrgId = row.OrganizationIdFk,
                        UserChannelSid = UserChannelSid.Select(x => x.UserUniqueId).Distinct().ToList(),
                        From = AuthorEnums.Stroke.ToString(),
                        Msg = (codeStroke.IsEms.HasValue && codeStroke.IsEms.Value ? "EMS" : "Inhouse") + " Code Stroke From is Changed",
                        RouteLink1 = "/Home/Inhouse%20Codes/code-strok-form",
                        RouteLink2 = "/Home/EMS/activateCode",
                    };

                    _communication.pushNotification(notification);

                }
                else
                {

                    var userIds = this._StrokeCodeGroupMembersRepo.Table.Where(x => x.StrokeCodeIdFk == row.CodeStrokeId).Select(x => x.UserIdFk).ToList();
                    var userUniqueIds = this._userRepo.Table.Where(x => userIds.Contains(x.UserId)).Select(x => x.UserUniqueId).Distinct().ToList();

                    var notification = new PushNotificationVM()
                    {
                        Id = row.CodeStrokeId,
                        OrgId = row.OrganizationIdFk,
                        UserChannelSid = userUniqueIds,
                        From = AuthorEnums.Stroke.ToString(),
                        Msg = (codeStroke.IsEms.HasValue && codeStroke.IsEms.Value ? "EMS" : "Inhouse") + " Code Stroke From is Changed",
                        RouteLink1 = "/Home/Inhouse%20Codes/code-strok-form",
                        RouteLink2 = "/Home/EMS/activateCode",
                    };

                    _communication.pushNotification(notification);
                }

                return GetStrokeDataById(row.CodeStrokeId);
            }
            else
            {
                if (codeStroke.OrganizationIdFk > 0)
                {
                    string IsEMS = codeStroke.IsEms.HasValue && codeStroke.IsEms.Value ? "EMS Code" : "Inhouse Code";
                    var DefaultServiceLineTeam = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == codeStroke.OrganizationIdFk && x.CodeIdFk == UCLEnums.Stroke.ToInt() && x.Type == IsEMS && !x.IsDeleted).Select(x => x.DefaultServiceLineTeam).FirstOrDefault();
                    if (DefaultServiceLineTeam != null && DefaultServiceLineTeam != "")
                    {
                        var DefaultServiceLineIds = DefaultServiceLineTeam.ToIntList();
                        //var ServiceLineTeam1Ids = codeStroke.ServiceLineTeam1Ids.ToIntList();
                        //var ServiceLineTeam2Ids = codeStroke.ServiceLineTeam2Ids.ToIntList();

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

                        var codeService = new CodesServiceLinesMapping()
                        {
                            OrganizationIdFk = stroke.OrganizationIdFk,
                            CodeIdFk = UCLEnums.Stroke.ToInt(),
                            DefaultServiceLineIdFk = DefaultServiceLineTeam,
                            ActiveCodeId = stroke.CodeStrokeId,
                            ActiveCodeName = UCLEnums.Stroke.ToString()
                        };
                        this._codesServiceLinesMappingRepo.Insert(codeService);

                        var UserChannelSid = (from us in this._userSchedulesRepo.Table
                                              join u in this._userRepo.Table on us.UserIdFk equals u.UserId
                                              where DefaultServiceLineIds.Contains(us.ServiceLineIdFk.Value) && us.ScheduleDateStart <= DateTime.UtcNow && us.ScheduleDateEnd >= DateTime.UtcNow && !us.IsDeleted && !u.IsDeleted
                                              select new { u.UserUniqueId, u.UserId }).Distinct().ToList();

                        var superAdmins = this._userRepo.Table.Where(x => x.IsInGroup && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId }).ToList();
                        UserChannelSid.AddRange(superAdmins);
                        var loggedUser = this._userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId }).FirstOrDefault();
                        UserChannelSid.Add(loggedUser);
                        var conversationChannelAttributes = JsonConvert.SerializeObject(new Dictionary<string, Object>()
                                    {
                                        {ChannelAttributeEnums.ChannelType.ToString(), ChannelTypeEnums.EMS.ToString()},
                                        {ChannelAttributeEnums.CodeType.ToString(), ChannelTypeEnums.Stroke.ToString()},
                                        {ChannelAttributeEnums.StrokeId.ToString(), stroke.CodeStrokeId}
                                    }, Formatting.Indented);
                        List<CodeStrokeGroupMember> ACodeGroupMembers = new List<CodeStrokeGroupMember>();
                        if (UserChannelSid != null && UserChannelSid.Count > 0)
                        {
                            string uniqueName = DateTime.Now.ToString("yyyyMMddHHmmssffff") + ApplicationSettings.UserId.ToString();
                            string friendlyName = stroke.IsEms.HasValue && stroke.IsEms.Value ? $"EMS Code {UCLEnums.Stroke.ToString()} {stroke.CodeStrokeId}" : $"Inhouse Code {UCLEnums.Stroke.ToString()} {stroke.CodeStrokeId}";
                            var channel = _communication.createConversationChannel(friendlyName, uniqueName, conversationChannelAttributes);
                            stroke.ChannelSid = channel.Sid;
                            this._codeStrokeRepo.Update(stroke);
                            UserChannelSid = UserChannelSid.Distinct().ToList();
                            foreach (var item in UserChannelSid)
                            {
                                try
                                {
                                    var codeGroupMember = new CodeStrokeGroupMember()
                                    {
                                        UserIdFk = item.UserId,
                                        StrokeCodeIdFk = stroke.CodeStrokeId,
                                        //ActiveCodeName = UCLEnums.Stroke.ToString(),
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

                                }
                            }
                            this._StrokeCodeGroupMembersRepo.Insert(ACodeGroupMembers);
                            var msg = new ConversationMessageVM();
                            msg.channelSid = channel.Sid;
                            msg.author = "System";
                            msg.attributes = "";
                            msg.body = $"<strong> {(codeStroke.IsEms.HasValue && codeStroke.IsEms.Value ? "EMS Code" : "Inhouse Code")} {UCLEnums.Stroke.ToString()} </strong></br></br>";
                            if (codeStroke.PatientName != null && codeStroke.PatientName != "")
                                msg.body += $"<strong>Patient Name: </strong> {codeStroke.PatientName} </br>";
                            msg.body += $"<strong>Dob: </strong> {codeStroke.Dob:MM-dd-yyyy} </br>";
                            msg.body += $"<strong>Last Well Known: </strong> {codeStroke.LastKnownWell:MM-dd-yyyy hh:mm tt} </br>";
                            if (codeStroke.ChiefComplant != null && codeStroke.ChiefComplant != "")
                                msg.body += $"<strong>Chief Complaint: </strong> {codeStroke.ChiefComplant} </br>";
                            if (codeStroke.Hpi != null && codeStroke.Hpi != "")
                                msg.body += $"<strong>Hpi: </strong> {codeStroke.Hpi} </br>";
                            var sendMsg = _communication.sendPushNotification(msg);

                            var showAllAccessUsers = this._dbContext.LoadStoredProcedure("md_getUsersOfComponentAccess")
                                                .WithSqlParam("@componentName", "Show All EMS,Show All Active Codes,Show Graphs,Show EMS,Show Active Codes")
                                                .WithSqlParam("@orgId", stroke.OrganizationIdFk)
                                                .ExecuteStoredProc<RegisterCredentialVM>().Select(x => new { x.UserUniqueId, x.UserId }).Distinct().ToList();
                            UserChannelSid.AddRange(showAllAccessUsers);
                            var notification = new PushNotificationVM()
                            {
                                Id = stroke.CodeStrokeId,
                                OrgId = stroke.OrganizationIdFk,
                                UserChannelSid = UserChannelSid.Select(x => x.UserUniqueId).Distinct().ToList(),
                                From = AuthorEnums.Stroke.ToString(),
                                Msg = (codeStroke.IsEms.HasValue && codeStroke.IsEms.Value ? "EMS" : "Inhouse") + " Code Stroke is update",
                                RouteLink3 = "/Home/EMS",
                                RouteLink4 = "/Home/Dashboard",
                                RouteLink5 = "/Home/Inhouse%20Codes"
                            };

                            _communication.pushNotification(notification);
                        }
                        return GetStrokeDataById(stroke.CodeStrokeId);
                    }
                    return new BaseResponse() { Status = HttpStatusCode.NotAcceptable, Message = "There is no Service Line in this organization related to Code Stroke" };
                }
                return new BaseResponse() { Status = HttpStatusCode.NotAcceptable, Message = "Organization is not selected" };
            }
        }

        public BaseResponse DeleteStroke(int strokeId, bool status)
        {
            //var row = this._codeStrokeRepo.Table.Where(x => x.CodeStrokeId == strokeId && !x.IsDeleted).FirstOrDefault();
            //row.IsDeleted = true;
            //row.ModifiedBy = ApplicationSettings.UserId;
            //row.ModifiedDate = DateTime.UtcNow;
            //this._codeStrokeRepo.Update(row);

            var sql = "EXEC md_ActiveOrInActiveInHouseCodesOrEMS @Status, @CodeName, @CodeId, @UserId";
            var parameters = new List<SqlParameter>() { new SqlParameter { ParameterName = "@Status", Value = status },
                                                        new SqlParameter { ParameterName = "@CodeName", Value = UCLEnums.Stroke.ToString() },
                                                        new SqlParameter { ParameterName = "@CodeId", Value = strokeId },
                                                        new SqlParameter { ParameterName = "@UserId", Value = ApplicationSettings.UserId }
                                                      };

            var isDeleted = this._dbContext.Database.ExecuteSqlRaw(sql, parameters);

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Deleted" };
        }

        public BaseResponse ActiveOrInActiveStroke(int strokeId, bool status)
        {
            //var row = this._codeStrokeRepo.Table.Where(x => x.CodeStrokeId == strokeId && !x.IsDeleted).FirstOrDefault();
            //row.IsDeleted = true;
            //row.ModifiedBy = ApplicationSettings.UserId;
            //row.ModifiedDate = DateTime.UtcNow;
            //this._codeStrokeRepo.Update(row);

            var sql = "EXEC md_ActiveOrInActiveInHouseCodesOrEMSDynamic @Status, @CodeName, @CodeId, @UserId";
            var parameters = new List<SqlParameter>() { new SqlParameter { ParameterName = "@Status", Value = status },
                                                        new SqlParameter { ParameterName = "@CodeName", Value = UCLEnums.Stroke.ToString() },
                                                        new SqlParameter { ParameterName = "@CodeId", Value = strokeId },
                                                        new SqlParameter { ParameterName = "@UserId", Value = ApplicationSettings.UserId }
                                                      };

            var isDeleted = this._dbContext.Database.ExecuteSqlRaw(sql, parameters);

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Deleted" };
        }

        #endregion

        #region Code Sepsis


        public BaseResponse GetAllSepsisCode(ActiveCodeVM activeCode)
        {

            var objList = this._dbContext.LoadStoredProcedure("md_getAllActiveCodesOrEMS_Dynamic")
                            .WithSqlParam("@status", activeCode.Status)
                            .WithSqlParam("@codeName", UCLEnums.Sepsis.ToString())
                            .WithSqlParam("@IsSuperAdmin", ApplicationSettings.isSuperAdmin)
                            .WithSqlParam("@showAll", activeCode.showAllActiveCodes)
                            .WithSqlParam("@userId", ApplicationSettings.UserId)
                            .WithSqlParam("@organizationId", activeCode.OrganizationIdFk)
                            .WithSqlParam("@page", activeCode.PageNumber)
                            .WithSqlParam("@size", activeCode.Rows)
                            .WithSqlParam("@sortOrder", activeCode.SortOrder)
                            .WithSqlParam("@sortCol", activeCode.SortCol)
                            .WithSqlParam("@filterVal", activeCode.FilterVal)
                           .ExecuteStoredProc<ActiveOrEMSCodesVM>();

            objList.ForEach(x =>
            {
                x.BloodThinnersTitle = new List<object>();
                x.BloodThinnersTitle.AddRange(_controlListDetailsRepo.Table.Where(b => x.BloodThinners.ToIntList().Contains(b.ControlListDetailId)).Select(b => new { Id = b.ControlListDetailId, b.Title }).ToList());
            });
            int totalRecords = 0;
            if (objList.Count > 0)
            {
                totalRecords = objList.Select(x => x.Total_Records).FirstOrDefault();
            }
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Returned", Body = new { totalRecords, objList } };

            //var SepsisData = new List<CodeSepsi>();
            //if (ApplicationSettings.isSuperAdmin)
            //{
            //    SepsisData = this._codeSepsisRepo.Table.Where(x => x.OrganizationIdFk == activeCode.OrganizationIdFk && !x.IsDeleted).OrderByDescending(x => x.CodeSepsisId).ToList();
            //}
            //else if (activeCode.showAllActiveCodes)
            //{
            //    SepsisData = this._codeSepsisRepo.Table.Where(x => x.OrganizationIdFk == activeCode.OrganizationIdFk && !x.IsDeleted).OrderByDescending(x => x.CodeSepsisId).ToList();
            //}
            //else
            //{
            //    SepsisData = (from cs in this._codeSepsisRepo.Table
            //                  join agm in this._activeCodesGroupMembersRepo.Table on cs.CodeSepsisId equals agm.ActiveCodeIdFk
            //                  where agm.UserIdFk == ApplicationSettings.UserId && agm.ActiveCodeName == UCLEnums.Sepsis.ToString() && !cs.IsDeleted
            //                  select cs).OrderByDescending(x => x.CodeSepsisId).AsQueryable().ToList();

            //    //SepsisData = this._codeSepsisRepo.Table.Where(x => x.CreatedBy == ApplicationSettings.UserId && !x.IsDeleted).OrderByDescending(x => x.CodeSepsisId).ToList();
            //}
            //var SepsisDataVM = AutoMapperHelper.MapList<CodeSepsi, CodeSepsisVM>(SepsisData);

            ////var orgData = GetHosplitalAddressObject(activeCode.OrganizationIdFk);

            //SepsisDataVM.ForEach(x =>
            //{
            //    x.AttachmentsPath = new List<string>();
            //    x.AudiosPath = new List<string>();
            //    x.VideosPath = new List<string>();
            //    x.BloodThinnersTitle = new List<object>();
            //    x.ServiceLines = new List<ServiceLineVM>();
            //    //x.OrganizationData == new object();

            //    //if (!string.IsNullOrEmpty(x.Attachments) && !string.IsNullOrWhiteSpace(x.Attachments))
            //    //{
            //    //    string path = this._RootPath + x.Attachments;
            //    //    if (Directory.Exists(path))
            //    //    {
            //    //        DirectoryInfo AttachFiles = new DirectoryInfo(path);
            //    //        foreach (var item in AttachFiles.GetFiles())
            //    //        {
            //    //            x.AttachmentsPath.Add(x.Attachments + "/" + item.Name);
            //    //        }
            //    //    }
            //    //}

            //    //if (!string.IsNullOrEmpty(x.Audio) && !string.IsNullOrWhiteSpace(x.Audio))
            //    //{
            //    //    string path = this._RootPath + x.Audio;
            //    //    if (Directory.Exists(path))
            //    //    {
            //    //        DirectoryInfo AudioFiles = new DirectoryInfo(path);
            //    //        foreach (var item in AudioFiles.GetFiles())
            //    //        {
            //    //            x.AudiosPath.Add(x.Audio + "/" + item.Name);
            //    //        }
            //    //    }
            //    //}

            //    //if (!string.IsNullOrEmpty(x.Video) && !string.IsNullOrWhiteSpace(x.Video))
            //    //{
            //    //    var path = this._RootPath + x.Video;
            //    //    if (Directory.Exists(path))
            //    //    {
            //    //        DirectoryInfo VideoFiles = new DirectoryInfo(path);
            //    //        foreach (var item in VideoFiles.GetFiles())
            //    //        {
            //    //            x.VideosPath.Add(x.Video + "/" + item.Name);
            //    //        }
            //    //    }
            //    //}
            //    //var serviceIds = this._activeCodeRepo.Table.Where(s => s.OrganizationIdFk == x.OrganizationIdFk && s.CodeIdFk == UCLEnums.Sepsis.ToInt() && !s.IsDeleted).Select(s => new { s.ServiceLineIds, s.DefaultServiceLineId }).FirstOrDefault();
            //    //var serviceLineIds = (from s in this._codesServiceLinesMappingRepo.Table
            //    //                      where s.OrganizationIdFk == x.OrganizationIdFk && s.CodeIdFk == UCLEnums.Sepsis.ToInt()
            //    //                      && s.ActiveCodeId == x.CodeSepsisId && s.ActiveCodeName == UCLEnums.Sepsis.ToString() && s.ServiceLineIdFk != serviceIds.DefaultServiceLineId
            //    //                      select s.ServiceLineIdFk).ToList();
            //    //x.ServiceLines = this._serviceLineRepo.Table.Where(s => serviceIds.ServiceLineIds.ToIntList().Distinct().Contains(s.ServiceLineId) && s.ServiceLineId != serviceIds.DefaultServiceLineId && !s.IsDeleted).Select(s => new ServiceLineVM() { ServiceLineId = s.ServiceLineId, ServiceName = s.ServiceName, IsSelected = serviceLineIds.Contains(s.ServiceLineId) }).ToList();
            //    //x.DefaultServiceLineId = serviceIds.DefaultServiceLineId;
            //    //x.DefaultServiceLine = this._serviceLineRepo.Table.Where(s => s.ServiceLineId == serviceIds.DefaultServiceLineId && !s.IsDeleted).Select(s => new ServiceLineVM() { ServiceLineId = s.ServiceLineId, ServiceName = s.ServiceName }).FirstOrDefault();
            //    //x.SelectedServiceLineIds = string.Join(",", serviceLineIds);
            //    x.LastKnownWellStr = x.LastKnownWell.ToString("yyyy-MM-dd hh:mm:ss tt");
            //    x.DobStr = x.Dob.ToString("yyyy-MM-dd hh:mm:ss tt");
            //    //x.OrganizationData = orgData;
            //    x.GenderTitle = _controlListDetailsRepo.Table.Where(g => g.ControlListDetailId == x.Gender).Select(g => g.Title).FirstOrDefault();
            //    x.BloodThinnersTitle.AddRange(_controlListDetailsRepo.Table.Where(b => x.BloodThinners.ToIntList().Contains(b.ControlListDetailId)).Select(b => new { Id = b.ControlListDetailId, b.Title }).ToList());
            //});
            //return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Returned", Body = SepsisDataVM };
        }

        public BaseResponse GetSepsisDataById(int SepsisId)
        {
            var SepsisData = this._codeSepsisRepo.Table.Where(x => x.CodeSepsisId == SepsisId).FirstOrDefault();
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
                string Type = SepsisDataVM.IsEms ? "EMS Code" : "Inhouse Code";
                var serviceIds = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == SepsisData.OrganizationIdFk && x.CodeIdFk == UCLEnums.Sepsis.ToInt() && x.Type == Type && !x.IsDeleted).Select(x => new { x.DefaultServiceLineTeam, x.ServiceLineTeam1, x.ServiceLineTeam2 }).FirstOrDefault();
                var serviceLineIds = (from x in this._codesServiceLinesMappingRepo.Table
                                      where x.OrganizationIdFk == SepsisData.OrganizationIdFk && x.CodeIdFk == UCLEnums.Sepsis.ToInt()
                                      && x.ActiveCodeId == SepsisData.CodeSepsisId && x.ActiveCodeName == UCLEnums.Sepsis.ToString()
                                      select new { x.DefaultServiceLineIdFk, x.ServiceLineId1Fk, x.ServiceLineId2Fk }).FirstOrDefault();
                if (serviceIds != null && serviceLineIds != null)
                {
                    var defaultIds = serviceIds.DefaultServiceLineTeam.ToIntList().Where(x => serviceLineIds.DefaultServiceLineIdFk.ToIntList().Contains(x)).Distinct().ToList();
                    var team1 = serviceIds.ServiceLineTeam1.ToIntList().Where(x => serviceLineIds.ServiceLineId1Fk.ToIntList().Contains(x)).Distinct().ToList();
                    var team2 = serviceIds.ServiceLineTeam2.ToIntList().Where(x => serviceLineIds.ServiceLineId2Fk.ToIntList().Contains(x)).Distinct().ToList();
                    SepsisDataVM.DefaultServiceLineTeam = this._serviceLineRepo.Table.Where(x => serviceIds.DefaultServiceLineTeam.ToIntList().Distinct().Contains(x.ServiceLineId) && !x.IsDeleted).Select(x => new ServiceLineVM() { ServiceLineId = x.ServiceLineId, ServiceName = x.ServiceName, IsSelected = defaultIds.Contains(x.ServiceLineId) }).ToList();
                    SepsisDataVM.ServiceLineTeam1 = this._serviceLineRepo.Table.Where(x => serviceIds.ServiceLineTeam1.ToIntList().Distinct().Contains(x.ServiceLineId) && !x.IsDeleted).Select(x => new ServiceLineVM() { ServiceLineId = x.ServiceLineId, ServiceName = x.ServiceName, IsSelected = team1.Contains(x.ServiceLineId) }).ToList();
                    SepsisDataVM.ServiceLineTeam2 = this._serviceLineRepo.Table.Where(x => serviceIds.ServiceLineTeam2.ToIntList().Distinct().Contains(x.ServiceLineId) && !x.IsDeleted).Select(x => new ServiceLineVM() { ServiceLineId = x.ServiceLineId, ServiceName = x.ServiceName, IsSelected = team2.Contains(x.ServiceLineId) }).ToList();
                }

                if (SepsisDataVM.IsEms)
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

                if (codeSepsis.DefaultServiceLineIds == null || codeSepsis.DefaultServiceLineIds == "")
                {
                    string IsEMS = row.IsEms ? "EMS Code" : "Inhouse Code";
                    codeSepsis.DefaultServiceLineIds = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == codeSepsis.OrganizationIdFk && x.CodeIdFk == UCLEnums.Sepsis.ToInt() && x.Type == IsEMS && !x.IsDeleted).Select(x => x.DefaultServiceLineTeam).FirstOrDefault();
                }

                if (codeSepsis.DefaultServiceLineIds != null && codeSepsis.DefaultServiceLineIds != "")
                {
                    var DefaultServiceLineIds = codeSepsis.DefaultServiceLineIds.ToIntList();
                    var ServiceLineTeam1Ids = codeSepsis.ServiceLineTeam1Ids.ToIntList();
                    var ServiceLineTeam2Ids = codeSepsis.ServiceLineTeam2Ids.ToIntList();

                    var codeServiceMapping = this._codesServiceLinesMappingRepo.Table.Where(x => x.OrganizationIdFk == row.OrganizationIdFk && x.CodeIdFk == UCLEnums.Sepsis.ToInt() && x.ActiveCodeId == row.CodeSepsisId).ToList();

                    if (codeServiceMapping.Count > 0)
                        this._codesServiceLinesMappingRepo.DeleteRange(codeServiceMapping);

                    var codeService = new CodesServiceLinesMapping()
                    {
                        OrganizationIdFk = row.OrganizationIdFk,
                        CodeIdFk = UCLEnums.Sepsis.ToInt(),
                        DefaultServiceLineIdFk = codeSepsis.DefaultServiceLineIds,
                        ServiceLineId1Fk = codeSepsis.ServiceLineTeam1Ids,
                        ServiceLineId2Fk = codeSepsis.ServiceLineTeam2Ids,
                        ActiveCodeId = row.CodeSepsisId,
                        ActiveCodeName = UCLEnums.Sepsis.ToString()
                    };
                    this._codesServiceLinesMappingRepo.Insert(codeService);

                    //var channel = this._SepsisCodeGroupMembersRepo.Table.Where(x => x.SepsisCodeIdFk == row.CodeSepsisId && !x.IsDeleted).ToList();

                    var UserChannelSid = (from us in this._userSchedulesRepo.Table
                                          join u in this._userRepo.Table on us.UserIdFk equals u.UserId
                                          where (DefaultServiceLineIds.Contains(us.ServiceLineIdFk.Value) || ServiceLineTeam1Ids.Contains(us.ServiceLineIdFk.Value) || ServiceLineTeam2Ids.Contains(us.ServiceLineIdFk.Value)) && us.ScheduleDateStart <= DateTime.UtcNow && us.ScheduleDateEnd >= DateTime.UtcNow && !us.IsDeleted && !u.IsDeleted
                                          select new { u.UserUniqueId, u.UserId }).Distinct().ToList();
                    var superAdmins = this._userRepo.Table.Where(x => x.IsInGroup && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId }).ToList();
                    UserChannelSid.AddRange(superAdmins);
                    var loggedInUser = this._userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId }).FirstOrDefault();
                    UserChannelSid.Add(loggedInUser);

                    if (row.ChannelSid != null && row.ChannelSid != "")
                    {
                        var channelSid = row.ChannelSid; //channel.Select(x => x.ChannelSid).FirstOrDefault();
                        var groupMembers = this._SepsisCodeGroupMembersRepo.Table.Where(x => x.SepsisCodeIdFk == row.CodeSepsisId).ToList();
                        this._SepsisCodeGroupMembersRepo.DeleteRange(groupMembers);
                        //this._SepsisCodeGroupMembersRepo.DeleteRange(channel);
                        bool isDeleted = _communication.DeleteUserToConversationChannel(channelSid);
                        List<CodeSepsisGroupMember> ACodeGroupMembers = new List<CodeSepsisGroupMember>();
                        foreach (var item in UserChannelSid.Distinct())
                        {
                            try
                            {
                                var codeGroupMember = new CodeSepsisGroupMember()
                                {
                                    UserIdFk = item.UserId,
                                    SepsisCodeIdFk = row.CodeSepsisId,
                                    //ActiveCodeName = UCLEnums.Sepsis.ToString(),
                                    IsAcknowledge = false,
                                    CreatedBy = ApplicationSettings.UserId,
                                    CreatedDate = DateTime.UtcNow,
                                    IsDeleted = false
                                };
                                ACodeGroupMembers.Add(codeGroupMember);
                                _communication.addNewUserToConversationChannel(channelSid, item.UserUniqueId);
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        this._SepsisCodeGroupMembersRepo.Insert(ACodeGroupMembers);
                    }

                    var notification = new PushNotificationVM()
                    {
                        Id = row.CodeSepsisId,
                        OrgId = row.OrganizationIdFk,
                        UserChannelSid = UserChannelSid.Select(x => x.UserUniqueId).Distinct().ToList(),
                        From = AuthorEnums.Sepsis.ToString(),
                        Msg = (codeSepsis.IsEms ? "EMS" : "Inhouse") + " Code Sepsis From is Changed",
                        RouteLink1 = "/Home/Inhouse%20Codes/code-sepsis-form",
                        RouteLink2 = "/Home/EMS/activateCode"
                    };

                    _communication.pushNotification(notification);

                }
                else
                {
                    var userIds = this._SepsisCodeGroupMembersRepo.Table.Where(x => x.SepsisCodeIdFk == row.CodeSepsisId).Select(x => x.UserIdFk).ToList();
                    var userUniqueIds = this._userRepo.Table.Where(x => userIds.Contains(x.UserId)).Select(x => x.UserUniqueId).Distinct().ToList();

                    var notification = new PushNotificationVM()
                    {
                        Id = row.CodeSepsisId,
                        OrgId = row.OrganizationIdFk,
                        UserChannelSid = userUniqueIds,
                        From = AuthorEnums.Sepsis.ToString(),
                        Msg = (codeSepsis.IsEms ? "EMS" : "Inhouse") + " Code Sepsis From is Changed",
                        RouteLink1 = "/Home/Inhouse%20Codes/code-sepsis-form",
                        RouteLink2 = "/Home/EMS/activateCode"
                    };

                    _communication.pushNotification(notification);
                }
                return GetSepsisDataById(row.CodeSepsisId);
            }
            else
            {

                if (codeSepsis.OrganizationIdFk > 0)
                {
                    string IsEMS = codeSepsis.IsEms ? "EMS Code" : "Inhouse Code";
                    var DefaultServiceLineTeam = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == codeSepsis.OrganizationIdFk && x.CodeIdFk == UCLEnums.Sepsis.ToInt() && x.Type == IsEMS && !x.IsDeleted).Select(x => x.DefaultServiceLineTeam).FirstOrDefault();
                    if (DefaultServiceLineTeam != null && DefaultServiceLineTeam != "")
                    {
                        var DefaultServiceLineIds = DefaultServiceLineTeam.ToIntList();
                        //var ServiceLineTeam1Ids = codeSepsis.ServiceLineTeam1Ids.ToIntList();
                        //var ServiceLineTeam2Ids = codeSepsis.ServiceLineTeam2Ids.ToIntList();

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

                        var codeService = new CodesServiceLinesMapping()
                        {
                            OrganizationIdFk = Sepsis.OrganizationIdFk,
                            CodeIdFk = UCLEnums.Sepsis.ToInt(),
                            DefaultServiceLineIdFk = DefaultServiceLineTeam,
                            ActiveCodeId = Sepsis.CodeSepsisId,
                            ActiveCodeName = UCLEnums.Sepsis.ToString()
                        };
                        this._codesServiceLinesMappingRepo.Insert(codeService);

                        var UserChannelSid = (from us in this._userSchedulesRepo.Table
                                              join u in this._userRepo.Table on us.UserIdFk equals u.UserId
                                              where DefaultServiceLineIds.Contains(us.ServiceLineIdFk.Value) && us.ScheduleDateStart <= DateTime.UtcNow && us.ScheduleDateEnd >= DateTime.UtcNow && !us.IsDeleted && !u.IsDeleted
                                              select new { u.UserUniqueId, u.UserId }).ToList();
                        var superAdmins = this._userRepo.Table.Where(x => x.IsInGroup && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId }).ToList();
                        UserChannelSid.AddRange(superAdmins);
                        var loggedUser = this._userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId }).FirstOrDefault();
                        UserChannelSid.Add(loggedUser);
                        List<CodeSepsisGroupMember> ACodeGroupMembers = new List<CodeSepsisGroupMember>();
                        if (UserChannelSid != null && UserChannelSid.Count > 0)
                        {
                            string uniqueName = DateTime.Now.ToString("yyyyMMddHHmmssffff") + ApplicationSettings.UserId.ToString();
                            string friendlyName = Sepsis.IsEms ? $"EMS Code {UCLEnums.Sepsis.ToString()} {Sepsis.CodeSepsisId}" : $"Inhouse Code {UCLEnums.Sepsis.ToString()} {Sepsis.CodeSepsisId}";
                            var conversationChannelAttributes = JsonConvert.SerializeObject(new Dictionary<string, Object>()
                                    {
                                        {ChannelAttributeEnums.ChannelType.ToString(), ChannelTypeEnums.EMS.ToString()},
                                        {ChannelAttributeEnums.CodeType.ToString(), ChannelTypeEnums.Sepsis.ToString()},
                                        {ChannelAttributeEnums.SepsisId.ToString(), Sepsis.CodeSepsisId}
                                    }, Formatting.Indented);
                            var channel = _communication.createConversationChannel(friendlyName, uniqueName, conversationChannelAttributes);
                            Sepsis.ChannelSid = channel.Sid;
                            this._codeSepsisRepo.Update(Sepsis);
                            UserChannelSid = UserChannelSid.Distinct().ToList();
                            foreach (var item in UserChannelSid)
                            {
                                try
                                {
                                    var codeGroupMember = new CodeSepsisGroupMember()
                                    {
                                        UserIdFk = item.UserId,
                                        SepsisCodeIdFk = Sepsis.CodeSepsisId,
                                        //ActiveCodeName = UCLEnums.Sepsis.ToString(),
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
                                    //ElmahExtensions.RiseError(ex);
                                }
                            }
                            this._SepsisCodeGroupMembersRepo.Insert(ACodeGroupMembers);

                            var msg = new ConversationMessageVM();
                            msg.channelSid = channel.Sid;
                            msg.author = "System";
                            msg.attributes = "";
                            msg.body = $"<strong> {(codeSepsis.IsEms ? "EMS Code" : "Inhouse Code")} {UCLEnums.Sepsis.ToString()} </strong></br></br>";
                            if (codeSepsis.PatientName != null && codeSepsis.PatientName != "")
                                msg.body += $"<strong>Patient Name: </strong> {codeSepsis.PatientName} </br>";
                            msg.body += $"<strong>Dob: </strong> {codeSepsis.Dob:MM-dd-yyyy} </br>";
                            msg.body += $"<strong>Last Well Known: </strong> {codeSepsis.LastKnownWell:MM-dd-yyyy hh:mm tt} </br>";
                            if (codeSepsis.ChiefComplant != null && codeSepsis.ChiefComplant != "")
                                msg.body += $"<strong>Chief Complaint: </strong> {codeSepsis.ChiefComplant} </br>";
                            if (codeSepsis.Hpi != null && codeSepsis.Hpi != "")
                                msg.body += $"<strong>Hpi: </strong> {codeSepsis.Hpi} </br>";

                            var sendMsg = _communication.sendPushNotification(msg);

                            var showAllAccessUsers = this._dbContext.LoadStoredProcedure("md_getUsersOfComponentAccess")
                                               .WithSqlParam("@componentName", "Show All EMS,Show All Active Codes,Show Graphs,Show EMS,Show Active Codes")
                                               .WithSqlParam("@orgId", Sepsis.OrganizationIdFk)
                                               .ExecuteStoredProc<RegisterCredentialVM>().Select(x => new { x.UserUniqueId, x.UserId }).Distinct().ToList();
                            UserChannelSid.AddRange(showAllAccessUsers);
                            var notification = new PushNotificationVM()
                            {
                                Id = Sepsis.CodeSepsisId,
                                OrgId = Sepsis.OrganizationIdFk,
                                UserChannelSid = UserChannelSid.Select(x => x.UserUniqueId).Distinct().ToList(),
                                From = AuthorEnums.Sepsis.ToString(),
                                Msg = (codeSepsis.IsEms ? "EMS" : "Inhouse") + " Code Sepsis is update",
                                RouteLink3 = "/Home/EMS",
                                RouteLink4 = "/Home/Dashboard",
                                RouteLink5 = "/Home/Inhouse%20Codes"
                            };

                            _communication.pushNotification(notification);
                        }
                        return GetSepsisDataById(Sepsis.CodeSepsisId);
                    }
                    return new BaseResponse() { Status = HttpStatusCode.NotAcceptable, Message = "There is no Service Line in this organization related to Code Sepsis" };
                }
                return new BaseResponse() { Status = HttpStatusCode.NotAcceptable, Message = "Organization is not selected" };
            }
        }

        public BaseResponse DeleteSepsis(int SepsisId, bool status)
        {
            //var row = this._codeSepsisRepo.Table.Where(x => x.CodeSepsisId == SepsisId && !x.IsDeleted).FirstOrDefault();
            //row.IsDeleted = true;
            //row.ModifiedBy = ApplicationSettings.UserId;
            //row.ModifiedDate = DateTime.UtcNow;
            //this._codeSepsisRepo.Update(row);

            var sql = "EXEC md_ActiveOrInActiveInHouseCodesOrEMS @Status, @CodeName, @CodeId, @UserId";
            var parameters = new List<SqlParameter>() { new SqlParameter { ParameterName = "@Status", Value = status },
                                                        new SqlParameter { ParameterName = "@CodeName", Value = UCLEnums.Sepsis.ToString() },
                                                        new SqlParameter { ParameterName = "@CodeId", Value = SepsisId },
                                                        new SqlParameter { ParameterName = "@UserId", Value = ApplicationSettings.UserId }
                                                      };

            var isDeleted = this._dbContext.Database.ExecuteSqlRaw(sql, parameters);

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Deleted" };
        }

        public BaseResponse ActiveOrInActiveSepsis(int SepsisId, bool status)
        {
            //var row = this._codeSepsisRepo.Table.Where(x => x.CodeSepsisId == SepsisId && !x.IsDeleted).FirstOrDefault();
            //row.IsDeleted = true;
            //row.ModifiedBy = ApplicationSettings.UserId;
            //row.ModifiedDate = DateTime.UtcNow;
            //this._codeSepsisRepo.Update(row);

            var sql = "EXEC md_ActiveOrInActiveInHouseCodesOrEMSDynamic @Status, @CodeName, @CodeId, @UserId";
            var parameters = new List<SqlParameter>() { new SqlParameter { ParameterName = "@Status", Value = status },
                                                        new SqlParameter { ParameterName = "@CodeName", Value = UCLEnums.Sepsis.ToString() },
                                                        new SqlParameter { ParameterName = "@CodeId", Value = SepsisId },
                                                        new SqlParameter { ParameterName = "@UserId", Value = ApplicationSettings.UserId }
                                                      };

            var isDeleted = this._dbContext.Database.ExecuteSqlRaw(sql, parameters);

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Deleted" };
        }


        #endregion

        #region Code STEMI

        public BaseResponse GetAllSTEMICode(ActiveCodeVM activeCode)
        {
            var objList = this._dbContext.LoadStoredProcedure("md_getAllActiveCodesOrEMS_Dynamic")
                                        .WithSqlParam("@status", activeCode.Status)
                                        .WithSqlParam("@codeName", UCLEnums.STEMI.ToString())
                                        .WithSqlParam("@IsSuperAdmin", ApplicationSettings.isSuperAdmin)
                                        .WithSqlParam("@showAll", activeCode.showAllActiveCodes)
                                        .WithSqlParam("@userId", ApplicationSettings.UserId)
                                        .WithSqlParam("@organizationId", activeCode.OrganizationIdFk)
                                        .WithSqlParam("@page", activeCode.PageNumber)
                                        .WithSqlParam("@size", activeCode.Rows)
                                        .WithSqlParam("@sortOrder", activeCode.SortOrder)
                                        .WithSqlParam("@sortCol", activeCode.SortCol)
                                        .WithSqlParam("@filterVal", activeCode.FilterVal)
                                       .ExecuteStoredProc<ActiveOrEMSCodesVM>();

            objList.ForEach(x =>
            {
                x.BloodThinnersTitle = new List<object>();
                x.BloodThinnersTitle.AddRange(_controlListDetailsRepo.Table.Where(b => x.BloodThinners.ToIntList().Contains(b.ControlListDetailId)).Select(b => new { Id = b.ControlListDetailId, b.Title }).ToList());
            });
            int totalRecords = 0;
            if (objList.Count > 0)
            {
                totalRecords = objList.Select(x => x.Total_Records).FirstOrDefault();
            }
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Returned", Body = new { totalRecords, objList } };

            //var STEMIData = new List<CodeStemi>();
            //if (ApplicationSettings.isSuperAdmin)
            //{
            //    STEMIData = this._codeSTEMIRepo.Table.Where(x => x.OrganizationIdFk == activeCode.OrganizationIdFk && !x.IsDeleted).OrderByDescending(x => x.CodeStemiid).ToList();
            //}
            //else if (activeCode.showAllActiveCodes)
            //{
            //    STEMIData = this._codeSTEMIRepo.Table.Where(x => x.OrganizationIdFk == activeCode.OrganizationIdFk && !x.IsDeleted).OrderByDescending(x => x.CodeStemiid).ToList();
            //}
            //else
            //{
            //    STEMIData = (from cs in this._codeSTEMIRepo.Table
            //                 join agm in this._activeCodesGroupMembersRepo.Table on cs.CodeStemiid equals agm.ActiveCodeIdFk
            //                 where agm.UserIdFk == ApplicationSettings.UserId && agm.ActiveCodeName == UCLEnums.STEMI.ToString() && !cs.IsDeleted
            //                 select cs).OrderByDescending(x => x.CodeStemiid).AsQueryable().ToList();


            //    //STEMIData = this._codeSTEMIRepo.Table.Where(x => x.CreatedBy == ApplicationSettings.UserId && !x.IsDeleted).OrderByDescending(x => x.CodeStemiid).ToList();
            //}
            //var STEMIDataVM = AutoMapperHelper.MapList<CodeStemi, CodeSTEMIVM>(STEMIData);

            ////var orgData = GetHosplitalAddressObject(activeCode.OrganizationIdFk);

            //STEMIDataVM.ForEach(x =>
            //{
            //    x.AttachmentsPath = new List<string>();
            //    x.AudiosPath = new List<string>();
            //    x.VideosPath = new List<string>();
            //    x.BloodThinnersTitle = new List<object>();
            //    x.ServiceLines = new List<ServiceLineVM>();

            //    //if (!string.IsNullOrEmpty(x.Attachments) && !string.IsNullOrWhiteSpace(x.Attachments))
            //    //{
            //    //    string path = this._RootPath + x.Attachments;
            //    //    if (Directory.Exists(path))
            //    //    {
            //    //        DirectoryInfo AttachFiles = new DirectoryInfo(path);
            //    //        foreach (var item in AttachFiles.GetFiles())
            //    //        {
            //    //            x.AttachmentsPath.Add(x.Attachments + "/" + item.Name);
            //    //        }
            //    //    }
            //    //}

            //    //if (!string.IsNullOrEmpty(x.Audio) && !string.IsNullOrWhiteSpace(x.Audio))
            //    //{
            //    //    string path = this._RootPath + x.Audio;
            //    //    if (Directory.Exists(path))
            //    //    {
            //    //        DirectoryInfo AudioFiles = new DirectoryInfo(path);
            //    //        foreach (var item in AudioFiles.GetFiles())
            //    //        {
            //    //            x.AudiosPath.Add(x.Audio + "/" + item.Name);
            //    //        }
            //    //    }
            //    //}

            //    //if (!string.IsNullOrEmpty(x.Video) && !string.IsNullOrWhiteSpace(x.Video))
            //    //{
            //    //    var path = this._RootPath + x.Video;
            //    //    if (Directory.Exists(path))
            //    //    {
            //    //        DirectoryInfo VideoFiles = new DirectoryInfo(path);
            //    //        foreach (var item in VideoFiles.GetFiles())
            //    //        {
            //    //            x.VideosPath.Add(x.Video + "/" + item.Name);
            //    //        }
            //    //    }
            //    //}

            //    //var serviceIds = this._activeCodeRepo.Table.Where(s => s.OrganizationIdFk == x.OrganizationIdFk && s.CodeIdFk == UCLEnums.STEMI.ToInt() && !s.IsDeleted).Select(s => new { s.ServiceLineIds, s.DefaultServiceLineId }).FirstOrDefault();
            //    //var serviceLineIds = (from s in this._codesServiceLinesMappingRepo.Table
            //    //                      where s.OrganizationIdFk == x.OrganizationIdFk && s.CodeIdFk == UCLEnums.STEMI.ToInt()
            //    //                      && s.ActiveCodeId == x.CodeStemiid && s.ActiveCodeName == UCLEnums.STEMI.ToString() && s.ServiceLineIdFk != serviceIds.DefaultServiceLineId
            //    //                      select s.ServiceLineIdFk).ToList();
            //    //x.ServiceLines = this._serviceLineRepo.Table.Where(s => serviceIds.ServiceLineIds.ToIntList().Distinct().Contains(s.ServiceLineId) && s.ServiceLineId != serviceIds.DefaultServiceLineId && !s.IsDeleted).Select(s => new ServiceLineVM() { ServiceLineId = s.ServiceLineId, ServiceName = s.ServiceName, IsSelected = serviceLineIds.Contains(s.ServiceLineId) }).ToList();
            //    //x.DefaultServiceLineId = serviceIds.DefaultServiceLineId;
            //    //x.DefaultServiceLine = this._serviceLineRepo.Table.Where(s => s.ServiceLineId == serviceIds.DefaultServiceLineId && !s.IsDeleted).Select(s => new ServiceLineVM() { ServiceLineId = s.ServiceLineId, ServiceName = s.ServiceName }).FirstOrDefault();
            //    //x.SelectedServiceLineIds = string.Join(",", serviceLineIds);
            //    x.LastKnownWellStr = x.LastKnownWell?.ToString("yyyy-MM-dd hh:mm:ss tt");
            //    x.DobStr = x.Dob?.ToString("yyyy-MM-dd hh:mm:ss tt");
            //    //x.OrganizationData = orgData;
            //    x.GenderTitle = _controlListDetailsRepo.Table.Where(g => g.ControlListDetailId == x.Gender).Select(g => g.Title).FirstOrDefault();
            //    x.BloodThinnersTitle.AddRange(_controlListDetailsRepo.Table.Where(b => x.BloodThinners.ToIntList().Contains(b.ControlListDetailId)).Select(b => new { Id = b.ControlListDetailId, b.Title }).ToList());
            //});
            //return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Returned", Body = STEMIDataVM };
        }

        public BaseResponse GetSTEMIDataById(int STEMIId)
        {
            var STEMIData = this._codeSTEMIRepo.Table.Where(x => x.CodeStemiid == STEMIId).FirstOrDefault();
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
                string Type = STEMIDataVM.IsEms.HasValue && STEMIDataVM.IsEms.Value ? "EMS Code" : "Inhouse Code";
                var serviceIds = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == STEMIData.OrganizationIdFk && x.CodeIdFk == UCLEnums.STEMI.ToInt() && x.Type == Type && !x.IsDeleted).Select(x => new { x.DefaultServiceLineTeam, x.ServiceLineTeam1, x.ServiceLineTeam2 }).FirstOrDefault();

                var serviceLineIds = (from x in this._codesServiceLinesMappingRepo.Table
                                      where x.OrganizationIdFk == STEMIData.OrganizationIdFk && x.CodeIdFk == UCLEnums.STEMI.ToInt()
                                      && x.ActiveCodeId == STEMIData.CodeStemiid && x.ActiveCodeName == UCLEnums.STEMI.ToString()
                                      select new { x.DefaultServiceLineIdFk, x.ServiceLineId1Fk, x.ServiceLineId2Fk }).FirstOrDefault();

                if (serviceIds != null && serviceLineIds != null)
                {
                    var defaultIds = serviceIds.DefaultServiceLineTeam.ToIntList().Where(x => serviceLineIds.DefaultServiceLineIdFk.ToIntList().Contains(x)).Distinct().ToList();
                    var team1 = serviceIds.ServiceLineTeam1.ToIntList().Where(x => serviceLineIds.ServiceLineId1Fk.ToIntList().Contains(x)).Distinct().ToList();
                    var team2 = serviceIds.ServiceLineTeam2.ToIntList().Where(x => serviceLineIds.ServiceLineId2Fk.ToIntList().Contains(x)).Distinct().ToList();
                    STEMIDataVM.DefaultServiceLineTeam = this._serviceLineRepo.Table.Where(x => serviceIds.DefaultServiceLineTeam.ToIntList().Distinct().Contains(x.ServiceLineId) && !x.IsDeleted).Select(x => new ServiceLineVM() { ServiceLineId = x.ServiceLineId, ServiceName = x.ServiceName, IsSelected = defaultIds.Contains(x.ServiceLineId) }).ToList();
                    STEMIDataVM.ServiceLineTeam1 = this._serviceLineRepo.Table.Where(x => serviceIds.ServiceLineTeam1.ToIntList().Distinct().Contains(x.ServiceLineId) && !x.IsDeleted).Select(x => new ServiceLineVM() { ServiceLineId = x.ServiceLineId, ServiceName = x.ServiceName, IsSelected = team1.Contains(x.ServiceLineId) }).ToList();
                    STEMIDataVM.ServiceLineTeam2 = this._serviceLineRepo.Table.Where(x => serviceIds.ServiceLineTeam2.ToIntList().Distinct().Contains(x.ServiceLineId) && !x.IsDeleted).Select(x => new ServiceLineVM() { ServiceLineId = x.ServiceLineId, ServiceName = x.ServiceName, IsSelected = team2.Contains(x.ServiceLineId) }).ToList();
                }

                STEMIDataVM.LastKnownWellStr = STEMIDataVM.LastKnownWell?.ToString("yyyy-MM-dd hh:mm:ss tt");
                STEMIDataVM.DobStr = STEMIDataVM.Dob?.ToString("yyyy-MM-dd hh:mm:ss tt");
                if (STEMIDataVM.IsEms.HasValue && STEMIDataVM.IsEms.Value)
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

                if (codeSTEMI.DefaultServiceLineIds == null || codeSTEMI.DefaultServiceLineIds == "")
                {
                    string IsEMS = row.IsEms.HasValue && row.IsEms.Value ? "EMS Code" : "Inhouse Code";
                    codeSTEMI.DefaultServiceLineIds = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == codeSTEMI.OrganizationIdFk && x.CodeIdFk == UCLEnums.STEMI.ToInt() && x.Type == IsEMS && !x.IsDeleted).Select(x => x.DefaultServiceLineTeam).FirstOrDefault();
                }

                if (codeSTEMI.DefaultServiceLineIds != null && codeSTEMI.DefaultServiceLineIds != "")
                {
                    var DefaultServiceLineIds = codeSTEMI.DefaultServiceLineIds.ToIntList();
                    var ServiceLineTeam1Ids = codeSTEMI.ServiceLineTeam1Ids.ToIntList();
                    var ServiceLineTeam2Ids = codeSTEMI.ServiceLineTeam2Ids.ToIntList();

                    var codeServiceMapping = this._codesServiceLinesMappingRepo.Table.Where(x => x.OrganizationIdFk == row.OrganizationIdFk && x.CodeIdFk == UCLEnums.STEMI.ToInt() && x.ActiveCodeId == row.CodeStemiid).ToList();

                    if (codeServiceMapping.Count > 0)
                        this._codesServiceLinesMappingRepo.DeleteRange(codeServiceMapping);

                    var codeService = new CodesServiceLinesMapping()
                    {
                        OrganizationIdFk = row.OrganizationIdFk,
                        CodeIdFk = UCLEnums.STEMI.ToInt(),
                        DefaultServiceLineIdFk = codeSTEMI.DefaultServiceLineIds,
                        ServiceLineId1Fk = codeSTEMI.ServiceLineTeam1Ids,
                        ServiceLineId2Fk = codeSTEMI.ServiceLineTeam2Ids,
                        ActiveCodeId = row.CodeStemiid,
                        ActiveCodeName = UCLEnums.STEMI.ToString()
                    };
                    this._codesServiceLinesMappingRepo.Insert(codeService);

                    //var channel = this._STEMICodeGroupMembersRepo.Table.Where(x => x.StemicodeIdFk == row.CodeStemiid && !x.IsDeleted).ToList();

                    var UserChannelSid = (from us in this._userSchedulesRepo.Table
                                          join u in this._userRepo.Table on us.UserIdFk equals u.UserId
                                          where (DefaultServiceLineIds.Contains(us.ServiceLineIdFk.Value) || ServiceLineTeam1Ids.Contains(us.ServiceLineIdFk.Value) || ServiceLineTeam2Ids.Contains(us.ServiceLineIdFk.Value)) && us.ScheduleDateStart <= DateTime.UtcNow && us.ScheduleDateEnd >= DateTime.UtcNow && !us.IsDeleted && !u.IsDeleted
                                          select new { u.UserUniqueId, u.UserId }).Distinct().ToList();
                    var superAdmins = this._userRepo.Table.Where(x => x.IsInGroup && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId }).ToList();
                    UserChannelSid.AddRange(superAdmins);
                    var loggedInUser = this._userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId }).FirstOrDefault();
                    UserChannelSid.Add(loggedInUser);

                    if (row.ChannelSid != null && row.ChannelSid != "")
                    {
                        var channelSid = row.ChannelSid; //channel.Select(x => x.ChannelSid).FirstOrDefault();

                        var groupMembers = this._STEMICodeGroupMembersRepo.Table.Where(x => x.StemicodeIdFk == row.CodeStemiid).ToList();
                        this._STEMICodeGroupMembersRepo.DeleteRange(groupMembers);
                        //this._STEMICodeGroupMembersRepo.DeleteRange(channel);
                        bool isDeleted = _communication.DeleteUserToConversationChannel(channelSid);
                        List<CodeStemigroupMember> ACodeGroupMembers = new List<CodeStemigroupMember>();
                        foreach (var item in UserChannelSid.Distinct())
                        {
                            try
                            {
                                var codeGroupMember = new CodeStemigroupMember()
                                {
                                    //ChannelSid = channelSid,
                                    UserIdFk = item.UserId,
                                    StemicodeIdFk = row.CodeStemiid,
                                    //ActiveCodeName = UCLEnums.STEMI.ToString(),
                                    IsAcknowledge = false,
                                    CreatedBy = ApplicationSettings.UserId,
                                    CreatedDate = DateTime.UtcNow,
                                    IsDeleted = false
                                };
                                ACodeGroupMembers.Add(codeGroupMember);
                                _communication.addNewUserToConversationChannel(channelSid, item.UserUniqueId);
                            }
                            catch (Exception ex)
                            {
                                //ElmahExtensions.RiseError(ex);
                            }
                        }
                        this._STEMICodeGroupMembersRepo.Insert(ACodeGroupMembers);
                    }
                    var notification = new PushNotificationVM()
                    {
                        Id = row.CodeStemiid,
                        OrgId = row.OrganizationIdFk,
                        UserChannelSid = UserChannelSid.Select(x => x.UserUniqueId).Distinct().ToList(),
                        From = AuthorEnums.STEMI.ToString(),
                        Msg = (codeSTEMI.IsEms.HasValue && codeSTEMI.IsEms.Value ? "EMS" : "Inhouse") + " Code STEMI From is Changed",
                        RouteLink1 = "/Home/Inhouse%20Codes/code-STEMI-form",
                        RouteLink2 = "/Home/EMS/activateCode"
                    };

                    _communication.pushNotification(notification);

                }
                else
                {
                    var userIds = this._STEMICodeGroupMembersRepo.Table.Where(x => x.StemicodeIdFk == row.CodeStemiid).Select(x => x.UserIdFk).ToList();
                    var userUniqueIds = this._userRepo.Table.Where(x => userIds.Contains(x.UserId)).Select(x => x.UserUniqueId).Distinct().ToList();

                    var notification = new PushNotificationVM()
                    {
                        Id = row.CodeStemiid,
                        OrgId = row.OrganizationIdFk,
                        UserChannelSid = userUniqueIds,
                        From = AuthorEnums.STEMI.ToString(),
                        Msg = (codeSTEMI.IsEms.HasValue && codeSTEMI.IsEms.Value ? "EMS" : "Inhouse") + " Code STEMI From is Changed",
                        RouteLink1 = "/Home/Inhouse%20Codes/code-STEMI-form",
                        RouteLink2 = "/Home/EMS/activateCode"
                    };

                    _communication.pushNotification(notification);
                }
                return GetSTEMIDataById(row.CodeStemiid);
            }
            else
            {
                if (codeSTEMI.OrganizationIdFk > 0)
                {
                    string IsEMS = codeSTEMI.IsEms.HasValue && codeSTEMI.IsEms.Value ? "EMS Code" : "Inhouse Code";
                    var DefaultServiceLineTeam = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == codeSTEMI.OrganizationIdFk && x.CodeIdFk == UCLEnums.STEMI.ToInt() && x.Type == IsEMS && !x.IsDeleted).Select(x => x.DefaultServiceLineTeam).FirstOrDefault();
                    if (DefaultServiceLineTeam != null && DefaultServiceLineTeam != "")
                    {
                        var DefaultServiceLineIds = DefaultServiceLineTeam.ToIntList();


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

                        var codeService = new CodesServiceLinesMapping()
                        {
                            OrganizationIdFk = STEMI.OrganizationIdFk,
                            CodeIdFk = UCLEnums.STEMI.ToInt(),
                            DefaultServiceLineIdFk = DefaultServiceLineTeam,
                            ActiveCodeId = STEMI.CodeStemiid,
                            ActiveCodeName = UCLEnums.STEMI.ToString()
                        };
                        this._codesServiceLinesMappingRepo.Insert(codeService);

                        var UserChannelSid = (from us in this._userSchedulesRepo.Table
                                              join u in this._userRepo.Table on us.UserIdFk equals u.UserId
                                              where DefaultServiceLineIds.Contains(us.ServiceLineIdFk.Value) && us.ScheduleDateStart <= DateTime.UtcNow && us.ScheduleDateEnd >= DateTime.UtcNow && !us.IsDeleted && !u.IsDeleted
                                              select new { u.UserUniqueId, u.UserId }).ToList();

                        var superAdmins = this._userRepo.Table.Where(x => x.IsInGroup && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId }).ToList();
                        UserChannelSid.AddRange(superAdmins);
                        var loggedUser = this._userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId }).FirstOrDefault();
                        UserChannelSid.Add(loggedUser);
                        List<CodeStemigroupMember> ACodeGroupMembers = new List<CodeStemigroupMember>();
                        if (UserChannelSid != null && UserChannelSid.Count > 0)
                        {
                            //string uniqueName = $"CONSULT_{Consult_Counter.Counter_Value.ToString()}";
                            string uniqueName = DateTime.Now.ToString("yyyyMMddHHmmssffff") + ApplicationSettings.UserId.ToString();
                            string friendlyName = STEMI.IsEms.HasValue && STEMI.IsEms.Value ? $"EMS Code {UCLEnums.STEMI.ToString()} {STEMI.CodeStemiid}" : $"Inhouse Code {UCLEnums.STEMI.ToString()} {STEMI.CodeStemiid}";
                            var conversationChannelAttributes = JsonConvert.SerializeObject(new Dictionary<string, Object>()
                                    {
                                        {ChannelAttributeEnums.ChannelType.ToString(), ChannelTypeEnums.EMS.ToString()},
                                        {ChannelAttributeEnums.CodeType.ToString(), ChannelTypeEnums.STEMI.ToString()},
                                        {ChannelAttributeEnums.STEMIId.ToString(), STEMI.CodeStemiid}
                                    }, Formatting.Indented);
                            var channel = _communication.createConversationChannel(friendlyName, uniqueName, conversationChannelAttributes);
                            STEMI.ChannelSid = channel.Sid;
                            this._codeSTEMIRepo.Update(STEMI);
                            UserChannelSid = UserChannelSid.Distinct().ToList();
                            foreach (var item in UserChannelSid)
                            {
                                try
                                {
                                    var codeGroupMember = new CodeStemigroupMember()
                                    {
                                        //ChannelSid = channel.Sid,
                                        UserIdFk = item.UserId,
                                        StemicodeIdFk = STEMI.CodeStemiid,
                                        //  ActiveCodeName = UCLEnums.STEMI.ToString(),
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
                                    //ElmahExtensions.RiseError(ex);
                                }
                            }
                            this._STEMICodeGroupMembersRepo.Insert(ACodeGroupMembers);

                            var msg = new ConversationMessageVM();
                            msg.channelSid = channel.Sid;
                            msg.author = "System";
                            msg.attributes = "";
                            msg.body = $"<strong> {(codeSTEMI.IsEms.HasValue && codeSTEMI.IsEms.Value ? "EMS Code" : "Inhouse Code")} {UCLEnums.STEMI.ToString()} </strong></br></br>";
                            if (codeSTEMI.PatientName != null && codeSTEMI.PatientName != "")
                                msg.body += $"<strong>Patient Name: </strong> {codeSTEMI.PatientName} </br>";
                            msg.body += $"<strong>Dob: </strong> {codeSTEMI.Dob:MM-dd-yyyy} </br>";
                            msg.body += $"<strong>Last Well Known: </strong> {codeSTEMI.LastKnownWell:MM-dd-yyyy hh:mm tt} </br>";
                            if (codeSTEMI.ChiefComplant != null && codeSTEMI.ChiefComplant != "")
                                msg.body += $"<strong>Chief Complaint: </strong> {codeSTEMI.ChiefComplant} </br>";
                            if (codeSTEMI.Hpi != null && codeSTEMI.Hpi != "")
                                msg.body += $"<strong>Hpi: </strong> {codeSTEMI.Hpi} </br>";
                            var sendMsg = _communication.sendPushNotification(msg);

                            var showAllAccessUsers = this._dbContext.LoadStoredProcedure("md_getUsersOfComponentAccess")
                                               .WithSqlParam("@componentName", "Show All EMS,Show All Active Codes,Show Graphs,Show EMS,Show Active Codes")
                                               .WithSqlParam("@orgId", STEMI.OrganizationIdFk)
                                               .ExecuteStoredProc<RegisterCredentialVM>().Select(x => new { x.UserUniqueId, x.UserId }).Distinct().ToList();
                            UserChannelSid.AddRange(showAllAccessUsers);
                            var notification = new PushNotificationVM()
                            {
                                Id = STEMI.CodeStemiid,
                                OrgId = STEMI.OrganizationIdFk,
                                UserChannelSid = UserChannelSid.Select(x => x.UserUniqueId).Distinct().ToList(),
                                From = AuthorEnums.STEMI.ToString(),
                                Msg = (codeSTEMI.IsEms.HasValue && codeSTEMI.IsEms.Value ? "EMS" : "Inhouse") + " Code Stemi is update",
                                RouteLink3 = "/Home/EMS",
                                RouteLink4 = "/Home/Dashboard",
                                RouteLink5 = "/Home/Inhouse%20Codes"
                            };

                            _communication.pushNotification(notification);
                        }
                        return GetSTEMIDataById(STEMI.CodeStemiid);
                    }
                    return new BaseResponse() { Status = HttpStatusCode.NotAcceptable, Message = "There is no Service Line in this organization related to Code STEMI" };
                }
                return new BaseResponse() { Status = HttpStatusCode.NotAcceptable, Message = "Organization is not selected" };
            }
        }

        public BaseResponse DeleteSTEMI(int STEMIId, bool status)
        {
            //var row = this._codeSTEMIRepo.Table.Where(x => x.CodeStemiid == STEMIId && !x.IsDeleted).FirstOrDefault();
            //row.IsDeleted = true;
            //row.ModifiedBy = ApplicationSettings.UserId;
            //row.ModifiedDate = DateTime.UtcNow;
            //this._codeSTEMIRepo.Update(row);

            var sql = "EXEC md_ActiveOrInActiveInHouseCodesOrEMS @Status, @CodeName, @CodeId, @UserId";
            var parameters = new List<SqlParameter>() { new SqlParameter { ParameterName = "@Status", Value = status },
                                                        new SqlParameter { ParameterName = "@CodeName", Value = UCLEnums.STEMI.ToString() },
                                                        new SqlParameter { ParameterName = "@CodeId", Value = STEMIId },
                                                        new SqlParameter { ParameterName = "@UserId", Value = ApplicationSettings.UserId }
                                                      };

            var isDeleted = this._dbContext.Database.ExecuteSqlRaw(sql, parameters);

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Deleted" };
        }

        public BaseResponse ActiceOrInActiveSTEMI(int STEMIId, bool status)
        {
            //var row = this._codeSTEMIRepo.Table.Where(x => x.CodeStemiid == STEMIId && !x.IsDeleted).FirstOrDefault();
            //row.IsDeleted = true;
            //row.ModifiedBy = ApplicationSettings.UserId;
            //row.ModifiedDate = DateTime.UtcNow;
            //this._codeSTEMIRepo.Update(row);

            var sql = "EXEC md_ActiveOrInActiveInHouseCodesOrEMSDynamic @Status, @CodeName, @CodeId, @UserId";
            var parameters = new List<SqlParameter>() { new SqlParameter { ParameterName = "@Status", Value = status },
                                                        new SqlParameter { ParameterName = "@CodeName", Value = UCLEnums.STEMI.ToString() },
                                                        new SqlParameter { ParameterName = "@CodeId", Value = STEMIId },
                                                        new SqlParameter { ParameterName = "@UserId", Value = ApplicationSettings.UserId }
                                                      };

            var isDeleted = this._dbContext.Database.ExecuteSqlRaw(sql, parameters);

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Deleted" };
        }



        #endregion

        #region Code Truma

        public BaseResponse GetAllTrumaCode(ActiveCodeVM activeCode)
        {
            var objList = this._dbContext.LoadStoredProcedure("md_getAllActiveCodesOrEMS_Dynamic")
                            .WithSqlParam("@status", activeCode.Status)
                            .WithSqlParam("@codeName", UCLEnums.Trauma.ToString())
                            .WithSqlParam("@IsSuperAdmin", ApplicationSettings.isSuperAdmin)
                            .WithSqlParam("@showAll", activeCode.showAllActiveCodes)
                            .WithSqlParam("@userId", ApplicationSettings.UserId)
                            .WithSqlParam("@organizationId", activeCode.OrganizationIdFk)
                            .WithSqlParam("@page", activeCode.PageNumber)
                            .WithSqlParam("@size", activeCode.Rows)
                            .WithSqlParam("@sortOrder", activeCode.SortOrder)
                            .WithSqlParam("@sortCol", activeCode.SortCol)
                            .WithSqlParam("@filterVal", activeCode.FilterVal)
                           .ExecuteStoredProc<ActiveOrEMSCodesVM>();

            objList.ForEach(x =>
            {
                x.BloodThinnersTitle = new List<object>();
                x.BloodThinnersTitle.AddRange(_controlListDetailsRepo.Table.Where(b => x.BloodThinners.ToIntList().Contains(b.ControlListDetailId)).Select(b => new { Id = b.ControlListDetailId, b.Title }).ToList());
            });
            int totalRecords = 0;
            if (objList.Count > 0)
            {
                totalRecords = objList.Select(x => x.Total_Records).FirstOrDefault();
            }
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Returned", Body = new { totalRecords, objList } };

            //var TrumaData = new List<CodeTrauma>();
            //if (ApplicationSettings.isSuperAdmin)
            //{
            //    TrumaData = this._codeTrumaRepo.Table.Where(x => x.OrganizationIdFk == activeCode.OrganizationIdFk && !x.IsDeleted).OrderByDescending(x => x.CodeTraumaId).ToList();
            //}
            //else if (activeCode.showAllActiveCodes)
            //{
            //    TrumaData = this._codeTrumaRepo.Table.Where(x => x.OrganizationIdFk == activeCode.OrganizationIdFk && !x.IsDeleted).OrderByDescending(x => x.CodeTraumaId).ToList();
            //}
            //else
            //{
            //    TrumaData = (from cs in this._codeTrumaRepo.Table
            //                 join agm in this._activeCodesGroupMembersRepo.Table on cs.CodeTraumaId equals agm.ActiveCodeIdFk
            //                 where agm.UserIdFk == ApplicationSettings.UserId && agm.ActiveCodeName == UCLEnums.Trauma.ToString() && !cs.IsDeleted
            //                 select cs).OrderByDescending(x => x.CodeTraumaId).AsQueryable().ToList();

            //    //TrumaData = this._codeTrumaRepo.Table.Where(x => x.CreatedBy == ApplicationSettings.UserId && !x.IsDeleted).OrderByDescending(x => x.CodeTraumaId).ToList();
            //}

            //var TrumaDataVM = AutoMapperHelper.MapList<CodeTrauma, CodeTrumaVM>(TrumaData);

            ////var orgData = GetHosplitalAddressObject(activeCode.OrganizationIdFk);

            //TrumaDataVM.ForEach(x =>
            //{
            //    x.AttachmentsPath = new List<string>();
            //    x.AudiosPath = new List<string>();
            //    x.VideosPath = new List<string>();
            //    x.BloodThinnersTitle = new List<object>();
            //    //if (!string.IsNullOrEmpty(x.Attachments) && !string.IsNullOrWhiteSpace(x.Attachments))
            //    //{
            //    //    string path = this._RootPath + x.Attachments;
            //    //    if (Directory.Exists(path))
            //    //    {
            //    //        DirectoryInfo AttachFiles = new DirectoryInfo(path);
            //    //        foreach (var item in AttachFiles.GetFiles())
            //    //        {
            //    //            x.AttachmentsPath.Add(x.Attachments + "/" + item.Name);
            //    //        }
            //    //    }
            //    //}

            //    //if (!string.IsNullOrEmpty(x.Audio) && !string.IsNullOrWhiteSpace(x.Audio))
            //    //{
            //    //    string path = this._RootPath + x.Audio;
            //    //    if (Directory.Exists(path))
            //    //    {
            //    //        DirectoryInfo AudioFiles = new DirectoryInfo(path);
            //    //        foreach (var item in AudioFiles.GetFiles())
            //    //        {
            //    //            x.AudiosPath.Add(x.Audio + "/" + item.Name);
            //    //        }
            //    //    }
            //    //}

            //    //if (!string.IsNullOrEmpty(x.Video) && !string.IsNullOrWhiteSpace(x.Video))
            //    //{
            //    //    var path = this._RootPath + x.Video;
            //    //    if (Directory.Exists(path))
            //    //    {
            //    //        DirectoryInfo VideoFiles = new DirectoryInfo(path);
            //    //        foreach (var item in VideoFiles.GetFiles())
            //    //        {
            //    //            x.VideosPath.Add(x.Video + "/" + item.Name);
            //    //        }
            //    //    }
            //    //}

            //    //var serviceIds = this._activeCodeRepo.Table.Where(s => s.OrganizationIdFk == x.OrganizationIdFk && s.CodeIdFk == UCLEnums.Trauma.ToInt() && !s.IsDeleted).Select(s => new { s.ServiceLineIds, s.DefaultServiceLineId }).FirstOrDefault();
            //    //var serviceLineIds = (from s in this._codesServiceLinesMappingRepo.Table
            //    //                      where s.OrganizationIdFk == x.OrganizationIdFk && s.CodeIdFk == UCLEnums.Trauma.ToInt()
            //    //                      && s.ActiveCodeId == x.CodeTraumaId && s.ActiveCodeName == UCLEnums.Trauma.ToString() && s.ServiceLineIdFk != serviceIds.DefaultServiceLineId
            //    //                      select s.ServiceLineIdFk).ToList();
            //    //x.ServiceLines = this._serviceLineRepo.Table.Where(s => serviceIds.ServiceLineIds.ToIntList().Distinct().Contains(s.ServiceLineId) && s.ServiceLineId != serviceIds.DefaultServiceLineId && !s.IsDeleted).Select(s => new ServiceLineVM() { ServiceLineId = s.ServiceLineId, ServiceName = s.ServiceName, IsSelected = serviceLineIds.Contains(s.ServiceLineId) }).ToList();
            //    //x.DefaultServiceLineId = serviceIds.DefaultServiceLineId;
            //    //x.DefaultServiceLine = this._serviceLineRepo.Table.Where(s => s.ServiceLineId == serviceIds.DefaultServiceLineId && !s.IsDeleted).Select(s => new ServiceLineVM() { ServiceLineId = s.ServiceLineId, ServiceName = s.ServiceName }).FirstOrDefault();
            //    //x.SelectedServiceLineIds = string.Join(",", serviceLineIds);

            //    x.LastKnownWellStr = x.LastKnownWell?.ToString("yyyy-MM-dd hh:mm:ss tt");
            //    x.DobStr = x.Dob?.ToString("yyyy-MM-dd hh:mm:ss tt");
            //    //x.OrganizationData = orgData;
            //    x.GenderTitle = _controlListDetailsRepo.Table.Where(g => g.ControlListDetailId == x.Gender).Select(g => g.Title).FirstOrDefault();
            //    x.BloodThinnersTitle.AddRange(_controlListDetailsRepo.Table.Where(b => x.BloodThinners.ToIntList().Contains(b.ControlListDetailId)).Select(b => new { Id = b.ControlListDetailId, b.Title }).ToList());
            //});
            //return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Returned", Body = TrumaDataVM };
        }

        public BaseResponse GetTrumaDataById(int TrumaId)
        {
            var TrumaData = this._codeTrumaRepo.Table.Where(x => x.CodeTraumaId == TrumaId).FirstOrDefault();
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
                string Type = TrumaDataVM.IsEms.HasValue && TrumaDataVM.IsEms.Value ? "EMS Code" : "Inhouse Code";
                var serviceIds = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == TrumaData.OrganizationIdFk && x.CodeIdFk == UCLEnums.Trauma.ToInt() && x.Type == Type && !x.IsDeleted).Select(x => new { x.DefaultServiceLineTeam, x.ServiceLineTeam1, x.ServiceLineTeam2 }).FirstOrDefault();


                var serviceLineIds = (from x in this._codesServiceLinesMappingRepo.Table
                                      where x.OrganizationIdFk == TrumaData.OrganizationIdFk && x.CodeIdFk == UCLEnums.Trauma.ToInt()
                                      && x.ActiveCodeId == TrumaData.CodeTraumaId && x.ActiveCodeName == UCLEnums.Trauma.ToString()
                                      select new { x.DefaultServiceLineIdFk, x.ServiceLineId1Fk, x.ServiceLineId2Fk }).FirstOrDefault();

                if (serviceIds != null && serviceLineIds != null)
                {
                    var defaultIds = serviceIds.DefaultServiceLineTeam.ToIntList().Where(x => serviceLineIds.DefaultServiceLineIdFk.ToIntList().Contains(x)).Distinct().ToList();
                    var team1 = serviceIds.ServiceLineTeam1.ToIntList().Where(x => serviceLineIds.ServiceLineId1Fk.ToIntList().Contains(x)).Distinct().ToList();
                    var team2 = serviceIds.ServiceLineTeam2.ToIntList().Where(x => serviceLineIds.ServiceLineId2Fk.ToIntList().Contains(x)).Distinct().ToList();
                    TrumaDataVM.DefaultServiceLineTeam = this._serviceLineRepo.Table.Where(x => serviceIds.DefaultServiceLineTeam.ToIntList().Distinct().Contains(x.ServiceLineId) && !x.IsDeleted).Select(x => new ServiceLineVM() { ServiceLineId = x.ServiceLineId, ServiceName = x.ServiceName, IsSelected = defaultIds.Contains(x.ServiceLineId) }).ToList();
                    TrumaDataVM.ServiceLineTeam1 = this._serviceLineRepo.Table.Where(x => serviceIds.ServiceLineTeam1.ToIntList().Distinct().Contains(x.ServiceLineId) && !x.IsDeleted).Select(x => new ServiceLineVM() { ServiceLineId = x.ServiceLineId, ServiceName = x.ServiceName, IsSelected = team1.Contains(x.ServiceLineId) }).ToList();
                    TrumaDataVM.ServiceLineTeam2 = this._serviceLineRepo.Table.Where(x => serviceIds.ServiceLineTeam2.ToIntList().Distinct().Contains(x.ServiceLineId) && !x.IsDeleted).Select(x => new ServiceLineVM() { ServiceLineId = x.ServiceLineId, ServiceName = x.ServiceName, IsSelected = team2.Contains(x.ServiceLineId) }).ToList();
                }

                if (TrumaDataVM.IsEms.HasValue && TrumaDataVM.IsEms.Value)
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

                if (codeTruma.DefaultServiceLineIds == null || codeTruma.DefaultServiceLineIds == "")
                {
                    string IsEMS = row.IsEms.HasValue && row.IsEms.Value ? "EMS Code" : "Inhouse Code";
                    codeTruma.DefaultServiceLineIds = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == codeTruma.OrganizationIdFk && x.CodeIdFk == UCLEnums.Trauma.ToInt() && x.Type == IsEMS && !x.IsDeleted).Select(x => x.DefaultServiceLineTeam).FirstOrDefault();
                }

                if (codeTruma.DefaultServiceLineIds != null && codeTruma.DefaultServiceLineIds != "")
                {
                    var DefaultServiceLineIds = codeTruma.DefaultServiceLineIds.ToIntList();
                    var ServiceLineTeam1Ids = codeTruma.ServiceLineTeam1Ids.ToIntList();
                    var ServiceLineTeam2Ids = codeTruma.ServiceLineTeam2Ids.ToIntList();

                    var codeServiceMapping = this._codesServiceLinesMappingRepo.Table.Where(x => x.OrganizationIdFk == row.OrganizationIdFk && x.CodeIdFk == UCLEnums.Trauma.ToInt() && x.ActiveCodeId == row.CodeTraumaId).ToList();
                    if (codeServiceMapping.Count > 0)
                        this._codesServiceLinesMappingRepo.DeleteRange(codeServiceMapping);
                    var codeService = new CodesServiceLinesMapping()
                    {
                        OrganizationIdFk = row.OrganizationIdFk,
                        CodeIdFk = UCLEnums.Trauma.ToInt(),
                        DefaultServiceLineIdFk = codeTruma.DefaultServiceLineIds,
                        ServiceLineId1Fk = codeTruma.ServiceLineTeam1Ids,
                        ServiceLineId2Fk = codeTruma.ServiceLineTeam2Ids,
                        ActiveCodeId = row.CodeTraumaId,
                        ActiveCodeName = UCLEnums.Trauma.ToString()
                    };
                    this._codesServiceLinesMappingRepo.Insert(codeService);

                    //var channel = this._TraumaCodeGroupMembersRepo.Table.Where(x => x.TraumaCodeIdFk == row.CodeTraumaId && !x.IsDeleted).ToList();

                    var UserChannelSid = (from us in this._userSchedulesRepo.Table
                                          join u in this._userRepo.Table on us.UserIdFk equals u.UserId
                                          where (DefaultServiceLineIds.Contains(us.ServiceLineIdFk.Value) || ServiceLineTeam1Ids.Contains(us.ServiceLineIdFk.Value) || ServiceLineTeam2Ids.Contains(us.ServiceLineIdFk.Value)) && us.ScheduleDateStart <= DateTime.UtcNow && us.ScheduleDateEnd >= DateTime.UtcNow && !us.IsDeleted && !u.IsDeleted
                                          select new { u.UserUniqueId, u.UserId }).Distinct().ToList();
                    var superAdmins = this._userRepo.Table.Where(x => x.IsInGroup && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId }).ToList();
                    UserChannelSid.AddRange(superAdmins);
                    var loggedInUser = this._userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId }).FirstOrDefault();
                    UserChannelSid.Add(loggedInUser);

                    if (row.ChannelSid != null && row.ChannelSid != "")
                    {
                        var channelSid = row.ChannelSid; //channel.Select(x => x.ChannelSid).FirstOrDefault();
                        var groupMembers = this._TraumaCodeGroupMembersRepo.Table.Where(x => x.TraumaCodeIdFk == row.CodeTraumaId).ToList();
                        this._TraumaCodeGroupMembersRepo.DeleteRange(groupMembers);
                        //this._TraumaCodeGroupMembersRepo.DeleteRange(channel);
                        bool isDeleted = _communication.DeleteUserToConversationChannel(channelSid);
                        List<CodeTraumaGroupMember> ACodeGroupMembers = new List<CodeTraumaGroupMember>();
                        foreach (var item in UserChannelSid.Distinct())
                        {
                            try
                            {
                                var codeGroupMember = new CodeTraumaGroupMember()
                                {
                                    //ChannelSid = channelSid,
                                    UserIdFk = item.UserId,
                                    TraumaCodeIdFk = row.CodeTraumaId,
                                    //ActiveCodeName = UCLEnums.Trauma.ToString(),
                                    IsAcknowledge = false,
                                    CreatedBy = ApplicationSettings.UserId,
                                    CreatedDate = DateTime.UtcNow,
                                    IsDeleted = false
                                };
                                ACodeGroupMembers.Add(codeGroupMember);
                                _communication.addNewUserToConversationChannel(channelSid, item.UserUniqueId);
                            }
                            catch (Exception ex)
                            {
                                //ElmahExtensions.RiseError(ex);
                            }
                        }
                        this._TraumaCodeGroupMembersRepo.Insert(ACodeGroupMembers);
                    }

                    var notification = new PushNotificationVM()
                    {
                        Id = row.CodeTraumaId,
                        OrgId = row.OrganizationIdFk,
                        UserChannelSid = UserChannelSid.Select(x => x.UserUniqueId).Distinct().ToList(),
                        From = AuthorEnums.Trauma.ToString(),
                        Msg = (codeTruma.IsEms.HasValue && codeTruma.IsEms.Value ? "EMS" : "Inhouse") + " Code Trauma From is Changed",
                        RouteLink1 = "/Home/Inhouse%20Codes/code-trauma-form",
                        RouteLink2 = "/Home/EMS/activateCode"
                    };

                    _communication.pushNotification(notification);

                }
                else
                {

                    var userIds = this._TraumaCodeGroupMembersRepo.Table.Where(x => x.TraumaCodeIdFk == row.CodeTraumaId).Select(x => x.UserIdFk).ToList();
                    var userUniqueIds = this._userRepo.Table.Where(x => userIds.Contains(x.UserId)).Select(x => x.UserUniqueId).Distinct().ToList();


                    var notification = new PushNotificationVM()
                    {
                        Id = row.CodeTraumaId,
                        OrgId = row.OrganizationIdFk,
                        UserChannelSid = userUniqueIds,
                        From = AuthorEnums.Trauma.ToString(),
                        Msg = (codeTruma.IsEms.HasValue && codeTruma.IsEms.Value ? "EMS" : "Inhouse") + " Code Trauma From is Changed",
                        RouteLink1 = "/Home/Inhouse%20Codes/code-trauma-form",
                        RouteLink2 = "/Home/EMS/activateCode"
                    };

                    _communication.pushNotification(notification);

                }
                return GetTrumaDataById(row.CodeTraumaId);
            }
            else
            {

                if (codeTruma.OrganizationIdFk > 0)
                {
                    string IsEMS = codeTruma.IsEms.HasValue && codeTruma.IsEms.Value ? "EMS Code" : "Inhouse Code";
                    var DefaultServiceLineTeam = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == codeTruma.OrganizationIdFk && x.CodeIdFk == UCLEnums.Trauma.ToInt() && x.Type == IsEMS && !x.IsDeleted).Select(x => x.DefaultServiceLineTeam).FirstOrDefault();
                    if (DefaultServiceLineTeam != null && DefaultServiceLineTeam != "")
                    {
                        var DefaultServiceLineIds = DefaultServiceLineTeam.ToIntList();

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

                        var codeService = new CodesServiceLinesMapping()
                        {
                            OrganizationIdFk = Truma.OrganizationIdFk,
                            CodeIdFk = UCLEnums.Trauma.ToInt(),
                            DefaultServiceLineIdFk = DefaultServiceLineTeam,
                            ActiveCodeId = Truma.CodeTraumaId,
                            ActiveCodeName = UCLEnums.Trauma.ToString()
                        };
                        this._codesServiceLinesMappingRepo.Insert(codeService);

                        var UserChannelSid = (from us in this._userSchedulesRepo.Table
                                              join u in this._userRepo.Table on us.UserIdFk equals u.UserId
                                              where DefaultServiceLineIds.Contains(us.ServiceLineIdFk.Value) && us.ScheduleDateStart <= DateTime.UtcNow && us.ScheduleDateEnd >= DateTime.UtcNow && !us.IsDeleted && !u.IsDeleted
                                              select new { u.UserUniqueId, u.UserId }).ToList();
                        var superAdmins = this._userRepo.Table.Where(x => x.IsInGroup && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId }).ToList();
                        UserChannelSid.AddRange(superAdmins);
                        var loggedUser = this._userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId }).FirstOrDefault();
                        UserChannelSid.Add(loggedUser);
                        List<CodeTraumaGroupMember> ACodeGroupMembers = new();
                        if (UserChannelSid != null && UserChannelSid.Count > 0)
                        {
                            string uniqueName = DateTime.Now.ToString("yyyyMMddHHmmssffff") + ApplicationSettings.UserId.ToString();
                            string friendlyName = Truma.IsEms.HasValue && Truma.IsEms.Value ? $"EMS Code {UCLEnums.Trauma.ToString()} {Truma.CodeTraumaId}" : $"Inhouse Code {UCLEnums.Trauma.ToString()} {Truma.CodeTraumaId}";
                            var conversationChannelAttributes = JsonConvert.SerializeObject(new Dictionary<string, Object>()
                                    {
                                        {ChannelAttributeEnums.ChannelType.ToString(), ChannelTypeEnums.EMS.ToString()},
                                        {ChannelAttributeEnums.CodeType.ToString(), ChannelTypeEnums.Trauma.ToString()},
                                        {ChannelAttributeEnums.TraumaId.ToString(), Truma.CodeTraumaId}
                                    }, Formatting.Indented);
                            var channel = _communication.createConversationChannel(friendlyName, uniqueName, conversationChannelAttributes);
                            Truma.ChannelSid = channel.Sid;
                            this._codeTrumaRepo.Update(Truma);
                            UserChannelSid = UserChannelSid.Distinct().ToList();
                            foreach (var item in UserChannelSid)
                            {
                                try
                                {
                                    var codeGroupMember = new CodeTraumaGroupMember()
                                    {
                                        //ChannelSid = channel.Sid,
                                        UserIdFk = item.UserId,
                                        TraumaCodeIdFk = Truma.CodeTraumaId,
                                        //ActiveCodeName = UCLEnums.Trauma.ToString(),
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
                                    //ElmahExtensions.RiseError(ex);
                                }
                            }
                            this._TraumaCodeGroupMembersRepo.Insert(ACodeGroupMembers);

                            var msg = new ConversationMessageVM();
                            msg.channelSid = channel.Sid;
                            msg.author = "System";
                            msg.attributes = "";
                            msg.body = $"<strong> {(codeTruma.IsEms.HasValue && codeTruma.IsEms.Value ? "EMS Code" : "Inhouse Code")} {UCLEnums.Trauma.ToString()} </strong></br></br>";
                            if (codeTruma.PatientName != null && codeTruma.PatientName != "")
                                msg.body += $"<strong>Patient Name: </strong> {codeTruma.PatientName} </br>";
                            msg.body += $"<strong>Dob: </strong> {codeTruma.Dob:MM-dd-yyyy} </br>";
                            msg.body += $"<strong>Last Well Known: </strong> {codeTruma.LastKnownWell:MM-dd-yyyy hh:mm tt} </br>";
                            if (codeTruma.ChiefComplant != null && codeTruma.ChiefComplant != "")
                                msg.body += $"<strong>Chief Complaint: </strong> {codeTruma.ChiefComplant} </br>";
                            if (codeTruma.Hpi != null && codeTruma.Hpi != "")
                                msg.body += $"<strong>Hpi: </strong> {codeTruma.Hpi} </br>";
                            var sendMsg = _communication.sendPushNotification(msg);

                            var showAllAccessUsers = this._dbContext.LoadStoredProcedure("md_getUsersOfComponentAccess")
                                               .WithSqlParam("@componentName", "Show All EMS,Show All Active Codes,Show Graphs,Show EMS,Show Active Codes")
                                               .WithSqlParam("@orgId", Truma.OrganizationIdFk)
                                               .ExecuteStoredProc<RegisterCredentialVM>().Select(x => new { x.UserUniqueId, x.UserId }).Distinct().ToList();
                            UserChannelSid.AddRange(showAllAccessUsers);
                            var notification = new PushNotificationVM()
                            {
                                Id = Truma.CodeTraumaId,
                                OrgId = Truma.OrganizationIdFk,
                                UserChannelSid = UserChannelSid.Select(x => x.UserUniqueId).Distinct().ToList(),
                                From = AuthorEnums.Trauma.ToString(),
                                Msg = (codeTruma.IsEms.HasValue && codeTruma.IsEms.Value ? "EMS" : "Inhouse") + " Code Trauma is update",
                                RouteLink3 = "/Home/EMS",
                                RouteLink4 = "/Home/Dashboard",
                                RouteLink5 = "/Home/Inhouse%20Codes"
                            };

                            _communication.pushNotification(notification);
                        }
                        return GetTrumaDataById(Truma.CodeTraumaId);
                    }
                    return new BaseResponse() { Status = HttpStatusCode.NotAcceptable, Message = "There is no Service Line in this organization related to Code Trauma" };
                }
                return new BaseResponse() { Status = HttpStatusCode.NotAcceptable, Message = "Organization is not selected" };
            }
        }

        public BaseResponse DeleteTruma(int TrumaId, bool status)
        {
            //var row = this._codeTrumaRepo.Table.Where(x => x.CodeTraumaId == TrumaId && !x.IsDeleted).FirstOrDefault();
            //row.IsDeleted = true;
            //row.ModifiedBy = ApplicationSettings.UserId;
            //row.ModifiedDate = DateTime.UtcNow;
            //this._codeTrumaRepo.Update(row);

            var sql = "EXEC md_ActiveOrInActiveInHouseCodesOrEMS @Status, @CodeName, @CodeId, @UserId";
            var parameters = new List<SqlParameter>() { new SqlParameter { ParameterName = "@Status", Value = status },
                                                        new SqlParameter { ParameterName = "@CodeName", Value = UCLEnums.Trauma.ToString() },
                                                        new SqlParameter { ParameterName = "@CodeId", Value = TrumaId },
                                                        new SqlParameter { ParameterName = "@UserId", Value = ApplicationSettings.UserId }
                                                      };

            var isDeleted = this._dbContext.Database.ExecuteSqlRaw(sql, parameters);

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Deleted" };
        }

        public BaseResponse ActiveOrInActiveTruma(int TrumaId, bool status)
        {
            //var row = this._codeTrumaRepo.Table.Where(x => x.CodeTraumaId == TrumaId && !x.IsDeleted).FirstOrDefault();
            //row.IsDeleted = true;
            //row.ModifiedBy = ApplicationSettings.UserId;
            //row.ModifiedDate = DateTime.UtcNow;
            //this._codeTrumaRepo.Update(row);

            var sql = "EXEC md_ActiveOrInActiveInHouseCodesOrEMSDynamic @Status, @CodeName, @CodeId, @UserId";
            var parameters = new List<SqlParameter>() { new SqlParameter { ParameterName = "@Status", Value = status },
                                                        new SqlParameter { ParameterName = "@CodeName", Value = UCLEnums.Trauma.ToString() },
                                                        new SqlParameter { ParameterName = "@CodeId", Value = TrumaId },
                                                        new SqlParameter { ParameterName = "@UserId", Value = ApplicationSettings.UserId }
                                                      };

            var isDeleted = this._dbContext.Database.ExecuteSqlRaw(sql, parameters);

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Deleted" };
        }


        #endregion

        #region Code Blue

        public BaseResponse GetAllBlueCode(ActiveCodeVM activeCode)
        {
            var objList = this._dbContext.LoadStoredProcedure("md_getAllActiveCodesOrEMS_Dynamic")
                            .WithSqlParam("@status", activeCode.Status)
                            .WithSqlParam("@codeName", UCLEnums.Blue.ToString())
                            .WithSqlParam("@IsSuperAdmin", ApplicationSettings.isSuperAdmin)
                            .WithSqlParam("@showAll", activeCode.showAllActiveCodes)
                            .WithSqlParam("@userId", ApplicationSettings.UserId)
                            .WithSqlParam("@organizationId", activeCode.OrganizationIdFk)
                            .WithSqlParam("@page", activeCode.PageNumber)
                            .WithSqlParam("@size", activeCode.Rows)
                            .WithSqlParam("@sortOrder", activeCode.SortOrder)
                            .WithSqlParam("@sortCol", activeCode.SortCol)
                            .WithSqlParam("@filterVal", activeCode.FilterVal)
                           .ExecuteStoredProc<ActiveOrEMSCodesVM>();

            objList.ForEach(x =>
            {
                x.BloodThinnersTitle = new List<object>();
                x.BloodThinnersTitle.AddRange(_controlListDetailsRepo.Table.Where(b => x.BloodThinners.ToIntList().Contains(b.ControlListDetailId)).Select(b => new { Id = b.ControlListDetailId, b.Title }).ToList());
            });
            int totalRecords = 0;
            if (objList.Count > 0)
            {
                totalRecords = objList.Select(x => x.Total_Records).FirstOrDefault();
            }
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Returned", Body = new { totalRecords, objList } };
            //var blueData = new List<CodeBlue>();
            //if (ApplicationSettings.isSuperAdmin)
            //{
            //    blueData = this._codeBlueRepo.Table.Where(x => x.OrganizationIdFk == activeCode.OrganizationIdFk && !x.IsDeleted).OrderByDescending(x => x.CodeBlueId).ToList();
            //}
            //if (activeCode.showAllActiveCodes)
            //{
            //    blueData = this._codeBlueRepo.Table.Where(x => x.OrganizationIdFk == activeCode.OrganizationIdFk && !x.IsDeleted).OrderByDescending(x => x.CodeBlueId).ToList();
            //}
            //else
            //{
            //    blueData = (from cs in this._codeBlueRepo.Table
            //                join agm in this._activeCodesGroupMembersRepo.Table on cs.CodeBlueId equals agm.ActiveCodeIdFk
            //                where agm.UserIdFk == ApplicationSettings.UserId && agm.ActiveCodeName == UCLEnums.Blue.ToString() && !cs.IsDeleted
            //                select cs).OrderByDescending(x => x.BloodThinners).AsQueryable().ToList();

            //    //blueData = this._codeBlueRepo.Table.Where(x => x.CreatedBy == ApplicationSettings.UserId && !x.IsDeleted).OrderByDescending(x => x.CodeBlueId).ToList();
            //}
            //var blueDataVM = AutoMapperHelper.MapList<CodeBlue, CodeBlueVM>(blueData);
            ////var orgData = GetHosplitalAddressObject(activeCode.OrganizationIdFk);

            //blueDataVM.ForEach(x =>
            //{
            //    x.AttachmentsPath = new List<string>();
            //    x.AudiosPath = new List<string>();
            //    x.VideosPath = new List<string>();
            //    x.OrganizationData = new object();
            //    x.BloodThinnersTitle = new List<object>();
            //    x.ServiceLines = new List<ServiceLineVM>();
            //    //if (!string.IsNullOrEmpty(x.Attachments) && !string.IsNullOrWhiteSpace(x.Attachments))
            //    //{
            //    //    string path = this._RootPath + x.Attachments; //this._RootPath  + x.Attachments;
            //    //    if (Directory.Exists(path))
            //    //    {
            //    //        DirectoryInfo AttachFiles = new DirectoryInfo(path);
            //    //        foreach (var item in AttachFiles.GetFiles())
            //    //        {
            //    //            x.AttachmentsPath.Add(x.Attachments + "/" + item.Name);
            //    //        }
            //    //    }
            //    //}

            //    //if (!string.IsNullOrEmpty(x.Audio) && !string.IsNullOrWhiteSpace(x.Audio))
            //    //{
            //    //    string path = this._RootPath + x.Audio; //this._RootPath  + x.Audio;
            //    //    if (Directory.Exists(path))
            //    //    {
            //    //        DirectoryInfo AudioFiles = new DirectoryInfo(path);
            //    //        foreach (var item in AudioFiles.GetFiles())
            //    //        {
            //    //            x.AudiosPath.Add(x.Audio + "/" + item.Name);
            //    //        }
            //    //    }
            //    //}

            //    //if (!string.IsNullOrEmpty(x.Video) && !string.IsNullOrWhiteSpace(x.Video))
            //    //{
            //    //    var path = this._RootPath + x.Video; //this._RootPath  + x.Video;
            //    //    if (Directory.Exists(path))
            //    //    {
            //    //        DirectoryInfo VideoFiles = new DirectoryInfo(path);
            //    //        foreach (var item in VideoFiles.GetFiles())
            //    //        {
            //    //            x.VideosPath.Add(x.Video + "/" + item.Name);
            //    //        }
            //    //    }
            //    //}
            //    //var serviceIds = this._activeCodeRepo.Table.Where(s => s.OrganizationIdFk == x.OrganizationIdFk && s.CodeIdFk == UCLEnums.Blue.ToInt() && !s.IsDeleted).Select(s => new { s.ServiceLineIds, s.DefaultServiceLineId }).FirstOrDefault();
            //    //var serviceLineIds = (from s in this._codesServiceLinesMappingRepo.Table
            //    //                      where s.OrganizationIdFk == x.OrganizationIdFk && s.CodeIdFk == UCLEnums.Blue.ToInt()
            //    //                      && s.ActiveCodeId == x.CodeBlueId && s.ActiveCodeName == UCLEnums.Blue.ToString() && s.ServiceLineIdFk != serviceIds.DefaultServiceLineId
            //    //                      select s.ServiceLineIdFk).ToList();
            //    //x.ServiceLines = this._serviceLineRepo.Table.Where(s => serviceIds.ServiceLineIds.ToIntList().Distinct().Contains(s.ServiceLineId) && s.ServiceLineId != serviceIds.DefaultServiceLineId && !s.IsDeleted).Select(s => new ServiceLineVM() { ServiceLineId = s.ServiceLineId, ServiceName = s.ServiceName, IsSelected = serviceLineIds.Contains(s.ServiceLineId) }).ToList();
            //    //x.DefaultServiceLineId = serviceIds.DefaultServiceLineId;
            //    //x.DefaultServiceLine = this._serviceLineRepo.Table.Where(s => s.ServiceLineId == serviceIds.DefaultServiceLineId && !s.IsDeleted).Select(s => new ServiceLineVM() { ServiceLineId = s.ServiceLineId, ServiceName = s.ServiceName }).FirstOrDefault();
            //    //x.SelectedServiceLineIds = string.Join(",", serviceLineIds);
            //    x.LastKnownWellStr = x.LastKnownWell?.ToString("yyyy-MM-dd hh:mm:ss tt");
            //    x.DobStr = x.Dob?.ToString("yyyy-MM-dd hh:mm:ss tt");
            //    //x.OrganizationData = orgData;
            //    x.GenderTitle = _controlListDetailsRepo.Table.Where(g => g.ControlListDetailId == x.Gender).Select(g => g.Title).FirstOrDefault();
            //    x.BloodThinnersTitle.AddRange(_controlListDetailsRepo.Table.Where(b => x.BloodThinners.ToIntList().Contains(b.ControlListDetailId)).Select(b => new { Id = b.ControlListDetailId, b.Title }).ToList());
            //});
            //return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Returned", Body = blueDataVM };
        }

        public BaseResponse GetBlueDataById(int blueId)
        {
            var blueData = this._codeBlueRepo.Table.Where(x => x.CodeBlueId == blueId).FirstOrDefault();
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
                string Type = BlueDataVM.IsEms.HasValue && BlueDataVM.IsEms.Value ? "EMS Code" : "Inhouse Code";
                var serviceIds = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == blueData.OrganizationIdFk && x.CodeIdFk == UCLEnums.Blue.ToInt() && x.Type == Type && !x.IsDeleted).Select(x => new { x.DefaultServiceLineTeam, x.ServiceLineTeam1, x.ServiceLineTeam2 }).FirstOrDefault();

                var serviceLineIds = (from x in this._codesServiceLinesMappingRepo.Table
                                      where x.OrganizationIdFk == blueData.OrganizationIdFk && x.CodeIdFk == UCLEnums.Blue.ToInt()
                                      && x.ActiveCodeId == blueData.CodeBlueId && x.ActiveCodeName == UCLEnums.Blue.ToString()
                                      select new { x.DefaultServiceLineIdFk, x.ServiceLineId1Fk, x.ServiceLineId2Fk }).FirstOrDefault();
                if (serviceIds != null && serviceLineIds != null)
                {
                    var defaultIds = serviceIds.DefaultServiceLineTeam.ToIntList().Where(x => serviceLineIds.DefaultServiceLineIdFk.ToIntList().Contains(x)).Distinct().ToList();
                    var team1 = serviceIds.ServiceLineTeam1.ToIntList().Where(x => serviceLineIds.ServiceLineId1Fk.ToIntList().Contains(x)).Distinct().ToList();
                    var team2 = serviceIds.ServiceLineTeam2.ToIntList().Where(x => serviceLineIds.ServiceLineId2Fk.ToIntList().Contains(x)).Distinct().ToList();
                    BlueDataVM.DefaultServiceLineTeam = this._serviceLineRepo.Table.Where(x => serviceIds.DefaultServiceLineTeam.ToIntList().Distinct().Contains(x.ServiceLineId) && !x.IsDeleted).Select(x => new ServiceLineVM() { ServiceLineId = x.ServiceLineId, ServiceName = x.ServiceName, IsSelected = defaultIds.Contains(x.ServiceLineId) }).ToList();
                    BlueDataVM.ServiceLineTeam1 = this._serviceLineRepo.Table.Where(x => serviceIds.ServiceLineTeam1.ToIntList().Distinct().Contains(x.ServiceLineId) && !x.IsDeleted).Select(x => new ServiceLineVM() { ServiceLineId = x.ServiceLineId, ServiceName = x.ServiceName, IsSelected = team1.Contains(x.ServiceLineId) }).ToList();
                    BlueDataVM.ServiceLineTeam2 = this._serviceLineRepo.Table.Where(x => serviceIds.ServiceLineTeam2.ToIntList().Distinct().Contains(x.ServiceLineId) && !x.IsDeleted).Select(x => new ServiceLineVM() { ServiceLineId = x.ServiceLineId, ServiceName = x.ServiceName, IsSelected = team2.Contains(x.ServiceLineId) }).ToList();
                }

                if (BlueDataVM.IsEms.HasValue && BlueDataVM.IsEms.Value)
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

                if (codeBlue.DefaultServiceLineIds == null || codeBlue.DefaultServiceLineIds == "")
                {
                    string IsEMS = row.IsEms.HasValue && row.IsEms.Value ? "EMS Code" : "Inhouse Code";
                    codeBlue.DefaultServiceLineIds = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == codeBlue.OrganizationIdFk && x.CodeIdFk == UCLEnums.Blue.ToInt() && x.Type == IsEMS && !x.IsDeleted).Select(x => x.DefaultServiceLineTeam).FirstOrDefault();
                }

                if (codeBlue.DefaultServiceLineIds != null && codeBlue.DefaultServiceLineIds != "")
                {
                    var DefaultServiceLineIds = codeBlue.DefaultServiceLineIds.ToIntList();
                    var ServiceLineTeam1Ids = codeBlue.ServiceLineTeam1Ids.ToIntList();
                    var ServiceLineTeam2Ids = codeBlue.ServiceLineTeam2Ids.ToIntList();
                    var codeServiceMapping = this._codesServiceLinesMappingRepo.Table.Where(x => x.OrganizationIdFk == row.OrganizationIdFk && x.CodeIdFk == UCLEnums.Blue.ToInt() && x.ActiveCodeId == row.CodeBlueId).ToList();
                    if (codeServiceMapping.Count > 0)
                        this._codesServiceLinesMappingRepo.DeleteRange(codeServiceMapping);
                    var codeService = new CodesServiceLinesMapping()
                    {
                        OrganizationIdFk = row.OrganizationIdFk,
                        CodeIdFk = UCLEnums.Blue.ToInt(),
                        DefaultServiceLineIdFk = codeBlue.DefaultServiceLineIds,
                        ServiceLineId1Fk = codeBlue.ServiceLineTeam1Ids,
                        ServiceLineId2Fk = codeBlue.ServiceLineTeam2Ids,
                        ActiveCodeId = row.CodeBlueId,
                        ActiveCodeName = UCLEnums.Blue.ToString()
                    };
                    this._codesServiceLinesMappingRepo.Insert(codeService);

                    //var channel = this._BlueCodeGroupMembersRepo.Table.Where(x => x.BlueCodeIdFk == row.CodeBlueId && !x.IsDeleted).ToList();

                    var UserChannelSid = (from us in this._userSchedulesRepo.Table
                                          join u in this._userRepo.Table on us.UserIdFk equals u.UserId
                                          where (DefaultServiceLineIds.Contains(us.ServiceLineIdFk.Value) || ServiceLineTeam1Ids.Contains(us.ServiceLineIdFk.Value) || ServiceLineTeam2Ids.Contains(us.ServiceLineIdFk.Value)) && us.ScheduleDateStart <= DateTime.UtcNow && us.ScheduleDateEnd >= DateTime.UtcNow && !us.IsDeleted && !u.IsDeleted
                                          select new { u.UserUniqueId, u.UserId }).Distinct().ToList();
                    var superAdmins = this._userRepo.Table.Where(x => x.IsInGroup && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId }).ToList();
                    UserChannelSid.AddRange(superAdmins);
                    var loggedInUser = this._userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId }).FirstOrDefault();
                    UserChannelSid.Add(loggedInUser);

                    if (row.ChannelSid != null && row.ChannelSid != "")
                    {
                        var channelSid = row.ChannelSid; //channel.Select(x => x.ChannelSid).FirstOrDefault();

                        var groupMembers = this._BlueCodeGroupMembersRepo.Table.Where(x => x.BlueCodeIdFk == row.CodeBlueId).ToList();
                        this._BlueCodeGroupMembersRepo.DeleteRange(groupMembers);

                        //this._BlueCodeGroupMembersRepo.DeleteRange(channel);
                        bool isDeleted = _communication.DeleteUserToConversationChannel(channelSid);
                        List<CodeBlueGroupMember> ACodeGroupMembers = new List<CodeBlueGroupMember>();
                        foreach (var item in UserChannelSid.Distinct())
                        {
                            try
                            {
                                var codeGroupMember = new CodeBlueGroupMember()
                                {
                                    //ChannelSid = channelSid,
                                    UserIdFk = item.UserId,
                                    BlueCodeIdFk = row.CodeBlueId,
                                    //ActiveCodeName = UCLEnums.Blue.ToString(),
                                    IsAcknowledge = false,
                                    CreatedBy = ApplicationSettings.UserId,
                                    CreatedDate = DateTime.UtcNow,
                                    IsDeleted = false
                                };
                                ACodeGroupMembers.Add(codeGroupMember);
                                _communication.addNewUserToConversationChannel(channelSid, item.UserUniqueId);
                            }
                            catch (Exception ex)
                            {
                                //ElmahExtensions.RiseError(ex);
                            }
                        }
                        this._BlueCodeGroupMembersRepo.Insert(ACodeGroupMembers);
                    }

                    var notification = new PushNotificationVM()
                    {
                        Id = row.CodeBlueId,
                        OrgId = row.OrganizationIdFk,
                        UserChannelSid = UserChannelSid.Select(x => x.UserUniqueId).Distinct().ToList(),
                        From = AuthorEnums.Blue.ToString(),
                        Msg = (codeBlue.IsEms.HasValue && codeBlue.IsEms.Value ? "EMS" : "Inhouse") + " Code Blue From is Changed",
                        RouteLink1 = "/Home/Activate%20Code/code-blue-form",
                        RouteLink2 = "/Home/EMS/activateCode"
                    };

                    _communication.pushNotification(notification);

                }
                else
                {
                    var userIds = this._BlueCodeGroupMembersRepo.Table.Where(x => x.BlueCodeIdFk == row.CodeBlueId).Select(x => x.UserIdFk).ToList();
                    var userUniqueIds = this._userRepo.Table.Where(x => userIds.Contains(x.UserId)).Select(x => x.UserUniqueId).Distinct().ToList();

                    var notification = new PushNotificationVM()
                    {
                        Id = row.CodeBlueId,
                        OrgId = row.OrganizationIdFk,
                        UserChannelSid = userUniqueIds,
                        From = AuthorEnums.Blue.ToString(),
                        Msg = (codeBlue.IsEms.HasValue && codeBlue.IsEms.Value ? "EMS" : "Inhouse") + " Code Blue From is Changed",
                        RouteLink1 = "/Home/Activate%20Code/code-blue-form",
                        RouteLink2 = "/Home/EMS/activateCode"
                    };

                    _communication.pushNotification(notification);

                }
                return GetBlueDataById(row.CodeBlueId);
            }
            else
            {
                if (codeBlue.OrganizationIdFk > 0)
                {
                    string IsEMS = codeBlue.IsEms.HasValue && codeBlue.IsEms.Value ? "EMS Code" : "Inhouse Code";
                    var DefaultServiceLineTeam = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == codeBlue.OrganizationIdFk && x.CodeIdFk == UCLEnums.Blue.ToInt() && x.Type == IsEMS && !x.IsDeleted).Select(x => x.DefaultServiceLineTeam).FirstOrDefault();
                    if (DefaultServiceLineTeam != null && DefaultServiceLineTeam != "")
                    {
                        var DefaultServiceLineIds = DefaultServiceLineTeam.ToIntList();

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

                        var codeService = new CodesServiceLinesMapping()
                        {
                            OrganizationIdFk = blue.OrganizationIdFk,
                            CodeIdFk = UCLEnums.Blue.ToInt(),
                            DefaultServiceLineIdFk = DefaultServiceLineTeam,
                            ActiveCodeId = blue.CodeBlueId,
                            ActiveCodeName = UCLEnums.Blue.ToString()
                        };
                        this._codesServiceLinesMappingRepo.Insert(codeService);

                        var UserChannelSid = (from us in this._userSchedulesRepo.Table
                                              join u in this._userRepo.Table on us.UserIdFk equals u.UserId
                                              where DefaultServiceLineIds.Contains(us.ServiceLineIdFk.Value) && us.ScheduleDateStart <= DateTime.UtcNow && us.ScheduleDateEnd >= DateTime.UtcNow && !us.IsDeleted && !u.IsDeleted
                                              select new { u.UserUniqueId, u.UserId }).ToList();
                        var superAdmins = this._userRepo.Table.Where(x => x.IsInGroup && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId }).ToList();
                        UserChannelSid.AddRange(superAdmins);
                        var loggedUser = this._userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId }).FirstOrDefault();
                        UserChannelSid.Add(loggedUser);
                        var conversationChannelAttributes = JsonConvert.SerializeObject(new Dictionary<string, Object>()
                                    {
                                        {ChannelAttributeEnums.ChannelType.ToString(), ChannelTypeEnums.EMS.ToString()},
                                        {ChannelAttributeEnums.CodeType.ToString(), ChannelTypeEnums.Blue.ToString()},
                                        {ChannelAttributeEnums.BlueId.ToString(), blue.CodeBlueId}
                                    }, Formatting.Indented);
                        List<CodeBlueGroupMember> ACodeGroupMembers = new List<CodeBlueGroupMember>();
                        if (UserChannelSid != null && UserChannelSid.Count > 0)
                        {
                            string uniqueName = DateTime.Now.ToString("yyyyMMddHHmmssffff") + ApplicationSettings.UserId.ToString();
                            string friendlyName = blue.IsEms.HasValue && blue.IsEms.Value ? $"EMS Code {UCLEnums.Blue.ToString()} {blue.CodeBlueId}" : $"Inhouse Code {UCLEnums.Blue.ToString()} {blue.CodeBlueId}";
                            var channel = _communication.createConversationChannel(friendlyName, uniqueName, conversationChannelAttributes);
                            blue.ChannelSid = channel.Sid;
                            this._codeBlueRepo.Update(blue);
                            UserChannelSid = UserChannelSid.Distinct().ToList();
                            foreach (var item in UserChannelSid)
                            {
                                try
                                {
                                    var codeGroupMember = new CodeBlueGroupMember()
                                    {
                                        //ChannelSid = channel.Sid,
                                        UserIdFk = item.UserId,
                                        BlueCodeIdFk = blue.CodeBlueId,
                                        // ActiveCodeName = UCLEnums.Blue.ToString(),
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
                                    //ElmahExtensions.RiseError(ex);
                                }
                            }
                            this._BlueCodeGroupMembersRepo.Insert(ACodeGroupMembers);
                            var msg = new ConversationMessageVM();
                            msg.channelSid = channel.Sid;
                            msg.author = "System";
                            msg.attributes = "";
                            msg.body = $"<strong> {(codeBlue.IsEms.HasValue && codeBlue.IsEms.Value ? "EMS Code" : "Inhouse Code")} {UCLEnums.Blue.ToString()} </strong></br></br>";
                            if (codeBlue.PatientName != null && codeBlue.PatientName != "")
                                msg.body += $"<strong>Patient Name: </strong> {codeBlue.PatientName} </br>";
                            msg.body += $"<strong>Dob: </strong> {codeBlue.Dob:MM-dd-yyyy} </br>";
                            msg.body += $"<strong>Last Well Known: </strong> {codeBlue.LastKnownWell:MM-dd-yyyy hh:mm tt} </br>";
                            if (codeBlue.ChiefComplant != null && codeBlue.ChiefComplant != "")
                                msg.body += $"<strong>Chief Complaint: </strong> {codeBlue.ChiefComplant} </br>";
                            if (codeBlue.Hpi != null && codeBlue.Hpi != "")
                                msg.body += $"<strong>Hpi: </strong> {codeBlue.Hpi} </br>";
                            var sendMsg = _communication.sendPushNotification(msg);

                            var showAllAccessUsers = this._dbContext.LoadStoredProcedure("md_getUsersOfComponentAccess")
                                               .WithSqlParam("@componentName", "Show All EMS,Show All Active Codes,Show Graphs,Show EMS,Show Active Codes")
                                               .WithSqlParam("@orgId", blue.OrganizationIdFk)
                                               .ExecuteStoredProc<RegisterCredentialVM>().Select(x => new { x.UserUniqueId, x.UserId }).Distinct().ToList();
                            UserChannelSid.AddRange(showAllAccessUsers);
                            var notification = new PushNotificationVM()
                            {
                                Id = blue.CodeBlueId,
                                OrgId = blue.OrganizationIdFk,
                                UserChannelSid = UserChannelSid.Select(x => x.UserUniqueId).Distinct().ToList(),
                                From = AuthorEnums.Blue.ToString(),
                                Msg = (codeBlue.IsEms.HasValue && codeBlue.IsEms.Value ? "EMS" : "Inhouse") + " Code Blue is update",
                                RouteLink3 = "/Home/EMS",
                                RouteLink4 = "/Home/Dashboard",
                                RouteLink5 = "/Home/Inhouse%20Codes"
                            };

                            _communication.pushNotification(notification);
                        }
                        return GetBlueDataById(blue.CodeBlueId);
                    }
                    return new BaseResponse() { Status = HttpStatusCode.NotAcceptable, Message = "There is no Service Line in this organization related to Code Blue" };
                }
                return new BaseResponse() { Status = HttpStatusCode.NotAcceptable, Message = "Organization is not selected" };
            }
        }

        public BaseResponse DeleteBlue(int blueId, bool status)
        {
            //var row = this._codeBlueRepo.Table.Where(x => x.CodeBlueId == blueId && !x.IsDeleted).FirstOrDefault();
            //row.IsDeleted = true;
            //row.ModifiedBy = ApplicationSettings.UserId;
            //row.ModifiedDate = DateTime.UtcNow;
            //this._codeBlueRepo.Update(row);

            var sql = "EXEC md_ActiveOrInActiveInHouseCodesOrEMS @Status, @CodeName, @CodeId, @UserId";
            var parameters = new List<SqlParameter>() { new SqlParameter { ParameterName = "@Status", Value = status },
                                                        new SqlParameter { ParameterName = "@CodeName", Value = UCLEnums.Blue.ToString() },
                                                        new SqlParameter { ParameterName = "@CodeId", Value = blueId },
                                                        new SqlParameter { ParameterName = "@UserId", Value = ApplicationSettings.UserId }
                                                      };

            var isDeleted = this._dbContext.Database.ExecuteSqlRaw(sql, parameters);

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Deleted" };
        }

        public BaseResponse ActiveOrInActiveBlue(int blueId, bool status)
        {
            //var row = this._codeBlueRepo.Table.Where(x => x.CodeBlueId == blueId && !x.IsDeleted).FirstOrDefault();
            //row.IsDeleted = true;
            //row.ModifiedBy = ApplicationSettings.UserId;
            //row.ModifiedDate = DateTime.UtcNow;
            //this._codeBlueRepo.Update(row);

            var sql = "EXEC md_ActiveOrInActiveInHouseCodesOrEMSDynamic @Status, @CodeName, @CodeId, @UserId";
            var parameters = new List<SqlParameter>() { new SqlParameter { ParameterName = "@Status", Value = status },
                                                        new SqlParameter { ParameterName = "@CodeName", Value = UCLEnums.Blue.ToString() },
                                                        new SqlParameter { ParameterName = "@CodeId", Value = blueId },
                                                        new SqlParameter { ParameterName = "@UserId", Value = ApplicationSettings.UserId }
                                                      };

            var isDeleted = this._dbContext.Database.ExecuteSqlRaw(sql, parameters);

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Deleted" };
        }

        #endregion


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
                item.DobStr = item.Dob?.ToString("yyyy-MM-dd hh:mm:ss tt");
                item.CreatedDateStr = item.CreatedDate.ToString("MM-dd-yyyy hh:mm:ss tt");
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
            if (results.Count > 0)
            {
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
            else
            {
                return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "State Not Found" };
            }

        }

        #endregion
    }
}
