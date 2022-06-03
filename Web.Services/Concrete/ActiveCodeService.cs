using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        private ICommunicationService _communicationService;
        private IRepository<User> _userRepo;
        private IRepository<Organization> _orgRepo;
        private IRepository<ServiceLine> _serviceLineRepo;
        private IRepository<UsersSchedule> _userSchedulesRepo;
        private IRepository<ControlListDetail> _controlListDetailsRepo;
        private IRepository<InhouseCodesField> _InhouseCodeFeilds;
        private IRepository<OrganizationCodeStrokeField> _orgCodeStrokeFeilds;
        private IRepository<OrganizationCodeStemifield> _orgCodeSTEMIFeilds;
        private IRepository<OrganizationCodeTraumaField> _orgCodeTraumaFeilds;
        private IRepository<OrganizationCodeSepsisField> _orgCodeSepsisFeilds;
        private IRepository<OrganizationCodeBlueField> _orgCodeBlueFeilds;
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
        private IRepository<ConversationChannel> _conversationChannelsRepo;

        IConfiguration _config;
        private string _RootPath;
        private string _GoogleApiKey;
        public ActiveCodeService(RAQ_DbContext dbContext,
            IConfiguration config,
            IHttpClient httpClient,
            IHostingEnvironment environment,
            ICommunicationService communicationService,
            IRepository<User> userRepo,
            IRepository<Organization> orgRepo,
            IRepository<ServiceLine> serviceLineRepo,
            IRepository<UsersSchedule> userSchedulesRepo,
            IRepository<ControlListDetail> controlListDetailsRepo,
            IRepository<InhouseCodesField> InhouseCodeFeilds,
            IRepository<OrganizationCodeStrokeField> orgCodeStrokeFeilds,
            IRepository<OrganizationCodeStemifield> orgCodeSTEMIFeilds,
            IRepository<OrganizationCodeTraumaField> orgCodeTraumaFeilds,
            IRepository<OrganizationCodeSepsisField> orgCodeSepsisFeilds,
            IRepository<OrganizationCodeBlueField> orgCodeBlueFeilds,
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
            IRepository<CodesServiceLinesMapping> codesServiceLinesMappingRepo,
            IRepository<ConversationChannel> conversationChannelsRepo)
        {
            this._config = config;
            this._httpClient = httpClient;
            this._dbContext = dbContext;
            this._environment = environment;
            this._communicationService = communicationService;
            this._userRepo = userRepo;
            this._orgRepo = orgRepo;
            this._serviceLineRepo = serviceLineRepo;
            this._userSchedulesRepo = userSchedulesRepo;
            this._controlListDetailsRepo = controlListDetailsRepo;
            this._InhouseCodeFeilds = InhouseCodeFeilds;
            this._orgCodeStrokeFeilds = orgCodeStrokeFeilds;
            this._orgCodeSTEMIFeilds = orgCodeSTEMIFeilds;
            this._orgCodeSepsisFeilds = orgCodeSepsisFeilds;
            this._orgCodeTraumaFeilds = orgCodeTraumaFeilds;
            this._orgCodeBlueFeilds = orgCodeBlueFeilds;
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
            this._conversationChannelsRepo = conversationChannelsRepo;

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
                return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "No Code Found", Body = codes };
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
                        row.IsActive = true;
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
            row.IsActive = status;
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

            //    item.LastKnownWellStr = item.LastKnownWell?.ToTimezoneFromUtc("Eastern Standard Time").ToString("yyyy-MM-dd hh:mm:ss tt");
            //    //new
            //    item.CreatedDateStr = item.CreatedDate.ToString("yyyy-MM-dd hh:mm:ss tt");
            //    item.DobStr = item.Dob?.ToTimezoneFromUtc("Eastern Standard Time").ToString("yyyy-MM-dd hh:mm:ss tt");
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
                    label = UCLEnums.EMS.ToDescription(),
                    backgroundColor = "#089bab",
                    borderColor = "#089bab",
                    data = EMS
                });
                datasets.Add(new
                {
                    label = UCLEnums.InhouseCode.ToDescription(),
                    backgroundColor = "#CEEBEE",
                    borderColor = "#CEEBEE",
                    data = activeCodes
                });

            }
            else
            {
                datasets = new List<object>() { new
                                                {
                                                  label= UCLEnums.EMS.ToDescription(),
                                                  backgroundColor= "#089bab",
                                                  data= ActiveCodes.Select(c=>c.EMS).ToList()
                                                },
                                                new
                                                {
                                                  label= UCLEnums.InhouseCode.ToDescription(),
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
                var rootPath = this._codeStrokeRepo.Table.Where(x => x.CodeStrokeId == files.Id).Select($"new({files.Type},IsEms,OrganizationIdFk)").FirstOrDefault();
                var pathval = rootPath.GetType().GetProperty(files.Type).GetValue(rootPath, null);
                string path = _environment.WebRootFileProvider.GetFileInfo(pathval + '/' + files.FileName)?.PhysicalPath;
                File.Delete(path);

                var UserChannelSid = (from u in this._userRepo.Table
                                      join gm in this._StrokeCodeGroupMembersRepo.Table on u.UserId equals gm.UserIdFk
                                      where gm.StrokeCodeIdFk == files.Id && !u.IsDeleted
                                      select u.UserUniqueId).Distinct().ToList();
                var superAdmins = this._userRepo.Table.Where(x => x.IsInGroup && !x.IsDeleted).Select(x => x.UserUniqueId).ToList();
                UserChannelSid.AddRange(superAdmins);
                var loggedUser = this._userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && !x.IsDeleted).Select(x => x.UserUniqueId).FirstOrDefault();
                UserChannelSid.Add(loggedUser);
                var codeStroke = new CodeStrokeVM();
                object fieldValue = new();

                if (files.Type == "Video")
                {
                    codeStroke.VideoFolderRoot = pathval;
                    string VideoPath = this._RootPath + codeStroke.VideoFolderRoot;
                    if (Directory.Exists(VideoPath))
                    {
                        DirectoryInfo AttachFiles = new DirectoryInfo(VideoPath);
                        codeStroke.VideosPath = new List<string>();
                        foreach (var item in AttachFiles.GetFiles())
                        {
                            codeStroke.VideosPath.Add(codeStroke.VideoFolderRoot + "/" + item.Name);
                        }
                    }
                }

                if (files.Type == "Audio")
                {
                    codeStroke.AudioFolderRoot = pathval;
                    string AudioPath = this._RootPath + codeStroke.AudioFolderRoot;
                    if (Directory.Exists(AudioPath))
                    {
                        DirectoryInfo AttachFiles = new DirectoryInfo(AudioPath);
                        codeStroke.AudiosPath = new List<string>();
                        foreach (var item in AttachFiles.GetFiles())
                        {
                            codeStroke.AudiosPath.Add(codeStroke.AudioFolderRoot + "/" + item.Name);
                        }
                    }
                }

                if (files.Type == "Attachments")
                {
                    codeStroke.AttachmentsFolderRoot = pathval;
                    string AttachmentsPath = this._RootPath + codeStroke.AttachmentsFolderRoot;
                    if (Directory.Exists(AttachmentsPath))
                    {
                        DirectoryInfo AttachFiles = new DirectoryInfo(AttachmentsPath);
                        codeStroke.AttachmentsPath = new List<string>();
                        foreach (var item in AttachFiles.GetFiles())
                        {
                            codeStroke.AttachmentsPath.Add(codeStroke.AttachmentsFolderRoot + "/" + item.Name);
                        }
                    }
                }

                fieldValue = new { videosPath = codeStroke.VideosPath, audiosPath = codeStroke.AudiosPath, attachmentsPath = codeStroke.AttachmentsPath };
                this._dbContext.Log(new { }, ActivityLogTableEnums.CodeStrokes.ToString(), codeStroke.CodeStrokeId, ActivityLogActionEnums.FileDelete.ToInt());
                var notification = new PushNotificationVM()
                {
                    Id = files.Id,
                    OrgId = rootPath.OrganizationIdFk,
                    FieldName = (files.Type == "Video" || files.Type == "Audio" ? (files.Type + "s").ToLower() : (files.Type.Replace("s", "")).ToLower()),
                    FieldDataType = "file",
                    FieldValue = fieldValue,
                    UserChannelSid = UserChannelSid.Distinct().ToList(),
                    From = AuthorEnums.Stroke.ToString(),
                    Msg = (rootPath.IsEms != null && rootPath.IsEms ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription()) + " Stroke From is Changed",
                    RouteLink1 = RouteEnums.CodeStrokeForm.ToDescription(),
                    RouteLink2 = RouteEnums.EMSForms.ToDescription(),
                };

                _communicationService.pushNotification(notification);
            }
            else if (files.CodeType == AuthorEnums.Sepsis.ToString())
            {
                var rootPath = this._codeSepsisRepo.Table.Where(x => x.CodeSepsisId == files.Id).Select($"new({files.Type},IsEms,OrganizationIdFk)").FirstOrDefault();
                var pathval = rootPath.GetType().GetProperty(files.Type).GetValue(rootPath, null);
                string path = _environment.WebRootFileProvider.GetFileInfo(pathval + '/' + files.FileName)?.PhysicalPath;
                File.Delete(path);

                var UserChannelSid = (from u in this._userRepo.Table
                                      join gm in this._SepsisCodeGroupMembersRepo.Table on u.UserId equals gm.UserIdFk
                                      where gm.SepsisCodeIdFk == files.Id && !u.IsDeleted
                                      select u.UserUniqueId).Distinct().ToList();
                var superAdmins = this._userRepo.Table.Where(x => x.IsInGroup && !x.IsDeleted).Select(x => x.UserUniqueId).ToList();
                UserChannelSid.AddRange(superAdmins);
                var loggedUser = this._userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && !x.IsDeleted).Select(x => x.UserUniqueId).FirstOrDefault();
                UserChannelSid.Add(loggedUser);
                var codeSepsis = new CodeSepsisVM();
                object fieldValue = new();

                if (files.Type == "Video")
                {
                    codeSepsis.VideoFolderRoot = pathval;
                    string VideoPath = this._RootPath + codeSepsis.VideoFolderRoot;
                    if (Directory.Exists(VideoPath))
                    {
                        DirectoryInfo AttachFiles = new DirectoryInfo(VideoPath);
                        codeSepsis.VideosPath = new List<string>();
                        foreach (var item in AttachFiles.GetFiles())
                        {
                            codeSepsis.VideosPath.Add(codeSepsis.VideoFolderRoot + "/" + item.Name);
                        }
                    }
                }

                if (files.Type == "Audio")
                {
                    codeSepsis.AudioFolderRoot = pathval;
                    string AudioPath = this._RootPath + codeSepsis.AudioFolderRoot;
                    if (Directory.Exists(AudioPath))
                    {
                        DirectoryInfo AttachFiles = new DirectoryInfo(AudioPath);
                        codeSepsis.AudiosPath = new List<string>();
                        foreach (var item in AttachFiles.GetFiles())
                        {
                            codeSepsis.AudiosPath.Add(codeSepsis.AudioFolderRoot + "/" + item.Name);
                        }
                    }
                }

                if (files.Type == "Attachments")
                {
                    codeSepsis.AttachmentsFolderRoot = pathval;
                    string AttachmentsPath = this._RootPath + codeSepsis.AttachmentsFolderRoot;
                    if (Directory.Exists(AttachmentsPath))
                    {
                        DirectoryInfo AttachFiles = new DirectoryInfo(AttachmentsPath);
                        codeSepsis.AttachmentsPath = new List<string>();
                        foreach (var item in AttachFiles.GetFiles())
                        {
                            codeSepsis.AttachmentsPath.Add(codeSepsis.AttachmentsFolderRoot + "/" + item.Name);
                        }
                    }
                }

                fieldValue = new { videosPath = codeSepsis.VideosPath, audiosPath = codeSepsis.AudiosPath, attachmentsPath = codeSepsis.AttachmentsPath };
                this._dbContext.Log(new { }, ActivityLogTableEnums.CodeSepsis.ToString(), codeSepsis.CodeSepsisId, ActivityLogActionEnums.FileDelete.ToInt());
                var notification = new PushNotificationVM()
                {
                    Id = files.Id,
                    OrgId = rootPath.OrganizationIdFk,
                    FieldName = (files.Type == "Video" || files.Type == "Audio" ? (files.Type + "s").ToLower() : (files.Type.Replace("s", "")).ToLower()),
                    FieldDataType = "file",
                    FieldValue = fieldValue,
                    UserChannelSid = UserChannelSid.Distinct().ToList(),
                    From = AuthorEnums.Sepsis.ToString(),
                    Msg = (rootPath.IsEms ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription()) + " Sepsis From is Changed",
                    RouteLink1 = RouteEnums.CodeSepsisForm.ToDescription(),
                    RouteLink2 = RouteEnums.EMSForms.ToDescription(),
                };

                _communicationService.pushNotification(notification);
            }
            else if (files.CodeType == AuthorEnums.Stemi.ToString())
            {
                var rootPath = this._codeSTEMIRepo.Table.Where(x => x.CodeStemiid == files.Id).Select($"new({files.Type},IsEms,OrganizationIdFk)").FirstOrDefault();
                var pathval = rootPath.GetType().GetProperty(files.Type).GetValue(rootPath, null);
                string path = _environment.WebRootFileProvider.GetFileInfo(pathval + '/' + files.FileName)?.PhysicalPath;
                File.Delete(path);

                var UserChannelSid = (from u in this._userRepo.Table
                                      join gm in this._STEMICodeGroupMembersRepo.Table on u.UserId equals gm.UserIdFk
                                      where gm.StemicodeIdFk == files.Id && !u.IsDeleted
                                      select u.UserUniqueId).Distinct().ToList();
                var superAdmins = this._userRepo.Table.Where(x => x.IsInGroup && !x.IsDeleted).Select(x => x.UserUniqueId).ToList();
                UserChannelSid.AddRange(superAdmins);
                var loggedUser = this._userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && !x.IsDeleted).Select(x => x.UserUniqueId).FirstOrDefault();
                UserChannelSid.Add(loggedUser);
                var codeSTEMI = new CodeSTEMIVM();
                object fieldValue = new();

                if (files.Type == "Video")
                {
                    codeSTEMI.VideoFolderRoot = pathval;
                    string VideoPath = this._RootPath + codeSTEMI.VideoFolderRoot;
                    if (Directory.Exists(VideoPath))
                    {
                        DirectoryInfo AttachFiles = new DirectoryInfo(VideoPath);
                        codeSTEMI.VideosPath = new List<string>();
                        foreach (var item in AttachFiles.GetFiles())
                        {
                            codeSTEMI.VideosPath.Add(codeSTEMI.VideoFolderRoot + "/" + item.Name);
                        }
                    }
                }

                if (files.Type == "Audio")
                {
                    codeSTEMI.AudioFolderRoot = pathval;
                    string AudioPath = this._RootPath + codeSTEMI.AudioFolderRoot;
                    if (Directory.Exists(AudioPath))
                    {
                        DirectoryInfo AttachFiles = new DirectoryInfo(AudioPath);
                        codeSTEMI.AudiosPath = new List<string>();
                        foreach (var item in AttachFiles.GetFiles())
                        {
                            codeSTEMI.AudiosPath.Add(codeSTEMI.AudioFolderRoot + "/" + item.Name);
                        }
                    }
                }

                if (files.Type == "Attachments")
                {
                    codeSTEMI.AttachmentsFolderRoot = pathval;
                    string AttachmentsPath = this._RootPath + codeSTEMI.AttachmentsFolderRoot;
                    if (Directory.Exists(AttachmentsPath))
                    {
                        DirectoryInfo AttachFiles = new DirectoryInfo(AttachmentsPath);
                        codeSTEMI.AttachmentsPath = new List<string>();
                        foreach (var item in AttachFiles.GetFiles())
                        {
                            codeSTEMI.AttachmentsPath.Add(codeSTEMI.AttachmentsFolderRoot + "/" + item.Name);
                        }
                    }
                }

                fieldValue = new { videosPath = codeSTEMI.VideosPath, audiosPath = codeSTEMI.AudiosPath, attachmentsPath = codeSTEMI.AttachmentsPath };
                this._dbContext.Log(new { }, ActivityLogTableEnums.CodeSTEMIs.ToString(), codeSTEMI.CodeStemiid, ActivityLogActionEnums.FileDelete.ToInt());
                var notification = new PushNotificationVM()
                {
                    Id = files.Id,
                    OrgId = rootPath.OrganizationIdFk,
                    FieldName = (files.Type == "Video" || files.Type == "Audio" ? (files.Type + "s").ToLower() : (files.Type.Replace("s", "")).ToLower()),
                    FieldDataType = "file",
                    FieldValue = fieldValue,
                    UserChannelSid = UserChannelSid.Distinct().ToList(),
                    From = AuthorEnums.Stemi.ToString(),
                    Msg = (rootPath.IsEms != null && rootPath.IsEms ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription()) + " STEMI From is Changed",
                    RouteLink1 = RouteEnums.CodeStemiForm.ToDescription(),
                    RouteLink2 = RouteEnums.EMSForms.ToDescription(),
                };

                _communicationService.pushNotification(notification);
            }
            else if (files.CodeType == AuthorEnums.Trauma.ToString())
            {
                var rootPath = this._codeTrumaRepo.Table.Where(x => x.CodeTraumaId == files.Id).Select($"new({files.Type},IsEms,OrganizationIdFk)").FirstOrDefault();
                var pathval = rootPath.GetType().GetProperty(files.Type).GetValue(rootPath, null);
                string path = _environment.WebRootFileProvider.GetFileInfo(pathval + '/' + files.FileName)?.PhysicalPath;
                File.Delete(path);

                var UserChannelSid = (from u in this._userRepo.Table
                                      join gm in this._TraumaCodeGroupMembersRepo.Table on u.UserId equals gm.UserIdFk
                                      where gm.TraumaCodeIdFk == files.Id && !u.IsDeleted
                                      select u.UserUniqueId).Distinct().ToList();
                var superAdmins = this._userRepo.Table.Where(x => x.IsInGroup && !x.IsDeleted).Select(x => x.UserUniqueId).ToList();
                UserChannelSid.AddRange(superAdmins);
                var loggedUser = this._userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && !x.IsDeleted).Select(x => x.UserUniqueId).FirstOrDefault();
                UserChannelSid.Add(loggedUser);
                var codeTrauma = new CodeTrumaVM();
                object fieldValue = new();

                if (files.Type == "Video")
                {
                    codeTrauma.VideoFolderRoot = pathval;
                    string VideoPath = this._RootPath + codeTrauma.VideoFolderRoot;
                    if (Directory.Exists(VideoPath))
                    {
                        DirectoryInfo AttachFiles = new DirectoryInfo(VideoPath);
                        codeTrauma.VideosPath = new List<string>();
                        foreach (var item in AttachFiles.GetFiles())
                        {
                            codeTrauma.VideosPath.Add(codeTrauma.VideoFolderRoot + "/" + item.Name);
                        }
                    }
                }

                if (files.Type == "Audio")
                {
                    codeTrauma.AudioFolderRoot = pathval;
                    string AudioPath = this._RootPath + codeTrauma.AudioFolderRoot;
                    if (Directory.Exists(AudioPath))
                    {
                        DirectoryInfo AttachFiles = new DirectoryInfo(AudioPath);
                        codeTrauma.AudiosPath = new List<string>();
                        foreach (var item in AttachFiles.GetFiles())
                        {
                            codeTrauma.AudiosPath.Add(codeTrauma.AudioFolderRoot + "/" + item.Name);
                        }
                    }
                }

                if (files.Type == "Attachments")
                {
                    codeTrauma.AttachmentsFolderRoot = pathval;
                    string AttachmentsPath = this._RootPath + codeTrauma.AttachmentsFolderRoot;
                    if (Directory.Exists(AttachmentsPath))
                    {
                        DirectoryInfo AttachFiles = new DirectoryInfo(AttachmentsPath);
                        codeTrauma.AttachmentsPath = new List<string>();
                        foreach (var item in AttachFiles.GetFiles())
                        {
                            codeTrauma.AttachmentsPath.Add(codeTrauma.AttachmentsFolderRoot + "/" + item.Name);
                        }
                    }
                }

                fieldValue = new { videosPath = codeTrauma.VideosPath, audiosPath = codeTrauma.AudiosPath, attachmentsPath = codeTrauma.AttachmentsPath };
                this._dbContext.Log(new { }, ActivityLogTableEnums.CodeTraumas.ToString(), codeTrauma.CodeTraumaId, ActivityLogActionEnums.FileDelete.ToInt());
                var notification = new PushNotificationVM()
                {
                    Id = files.Id,
                    OrgId = rootPath.OrganizationIdFk,
                    FieldName = (files.Type == "Video" || files.Type == "Audio" ? (files.Type + "s").ToLower() : (files.Type.Replace("s", "")).ToLower()),
                    FieldDataType = "file",
                    FieldValue = fieldValue,
                    UserChannelSid = UserChannelSid.Distinct().ToList(),
                    From = AuthorEnums.Trauma.ToString(),
                    Msg = (rootPath.IsEms != null && rootPath.IsEms ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription()) + " Trauma From is Changed",
                    RouteLink1 = RouteEnums.CodeTraumaForm.ToDescription(),
                    RouteLink2 = RouteEnums.EMSForms.ToDescription(),
                };

                _communicationService.pushNotification(notification);
            }
            else if (files.CodeType == AuthorEnums.Blue.ToString())
            {
                var rootPath = this._codeBlueRepo.Table.Where(x => x.CodeBlueId == files.Id).Select($"new({files.Type},IsEms,OrganizationIdFk)").FirstOrDefault();
                var pathval = rootPath.GetType().GetProperty(files.Type).GetValue(rootPath, null);
                string path = _environment.WebRootFileProvider.GetFileInfo(pathval + '/' + files.FileName)?.PhysicalPath;
                File.Delete(path);

                var UserChannelSid = (from u in this._userRepo.Table
                                      join gm in this._BlueCodeGroupMembersRepo.Table on u.UserId equals gm.UserIdFk
                                      where gm.BlueCodeIdFk == files.Id && !u.IsDeleted
                                      select u.UserUniqueId).Distinct().ToList();
                var superAdmins = this._userRepo.Table.Where(x => x.IsInGroup && !x.IsDeleted).Select(x => x.UserUniqueId).ToList();
                UserChannelSid.AddRange(superAdmins);
                var loggedUser = this._userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && !x.IsDeleted).Select(x => x.UserUniqueId).FirstOrDefault();
                UserChannelSid.Add(loggedUser);
                var codeBlue = new CodeBlueVM();
                object fieldValue = new();

                if (files.Type == "Video")
                {
                    codeBlue.VideoFolderRoot = pathval;
                    string VideoPath = this._RootPath + codeBlue.VideoFolderRoot;
                    if (Directory.Exists(VideoPath))
                    {
                        DirectoryInfo AttachFiles = new DirectoryInfo(VideoPath);
                        codeBlue.VideosPath = new List<string>();
                        foreach (var item in AttachFiles.GetFiles())
                        {
                            codeBlue.VideosPath.Add(codeBlue.VideoFolderRoot + "/" + item.Name);
                        }
                    }
                }

                if (files.Type == "Audio")
                {
                    codeBlue.AudioFolderRoot = pathval;
                    string AudioPath = this._RootPath + codeBlue.AudioFolderRoot;
                    if (Directory.Exists(AudioPath))
                    {
                        DirectoryInfo AttachFiles = new DirectoryInfo(AudioPath);
                        codeBlue.AudiosPath = new List<string>();
                        foreach (var item in AttachFiles.GetFiles())
                        {
                            codeBlue.AudiosPath.Add(codeBlue.AudioFolderRoot + "/" + item.Name);
                        }
                    }
                }

                if (files.Type == "Attachments")
                {
                    codeBlue.AttachmentsFolderRoot = pathval;
                    string AttachmentsPath = this._RootPath + codeBlue.AttachmentsFolderRoot;
                    if (Directory.Exists(AttachmentsPath))
                    {
                        DirectoryInfo AttachFiles = new DirectoryInfo(AttachmentsPath);
                        codeBlue.AttachmentsPath = new List<string>();
                        foreach (var item in AttachFiles.GetFiles())
                        {
                            codeBlue.AttachmentsPath.Add(codeBlue.AttachmentsFolderRoot + "/" + item.Name);
                        }
                    }
                }

                fieldValue = new { videosPath = codeBlue.VideosPath, audiosPath = codeBlue.AudiosPath, attachmentsPath = codeBlue.AttachmentsPath };
                this._dbContext.Log(new { }, ActivityLogTableEnums.CodeBlues.ToString(), codeBlue.CodeBlueId, ActivityLogActionEnums.FileDelete.ToInt());
                var notification = new PushNotificationVM()
                {
                    Id = files.Id,
                    OrgId = rootPath.OrganizationIdFk,
                    FieldName = (files.Type == "Video" || files.Type == "Audio" ? (files.Type + "s").ToLower() : (files.Type.Replace("s", "")).ToLower()),
                    FieldDataType = "file",
                    FieldValue = fieldValue,
                    UserChannelSid = UserChannelSid.Distinct().ToList(),
                    From = AuthorEnums.Blue.ToString(),
                    Msg = (rootPath.IsEms != null && rootPath.IsEms ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription()) + " Blue From is Changed",
                    RouteLink1 = RouteEnums.CodeBlueForm.ToDescription(),
                    RouteLink2 = RouteEnums.EMSForms.ToDescription(),
                };

                _communicationService.pushNotification(notification);
            }


            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "File Deleted Successfully" };
        }

        #endregion


        #region Generic Methods For Codes

        public BaseResponse GetAllCodeData(ActiveCodeVM activeCode)
        {

            var gridColumns = GetInhouseCodeTableFeilds(activeCode.OrganizationIdFk, activeCode.CodeName);
            dynamic Fields = gridColumns.Body;
            if (Fields != null && Fields.FieldName != null)
            {
                string FieldNames = Convert.ToString(Fields.FieldName);
                var objList = this._dbContext.LoadStoredProcedure("md_getAllActiveCodesOrEMS_Dynamic")
                                .WithSqlParam("@status", activeCode.Status)
                                .WithSqlParam("@colName", FieldNames)
                                .WithSqlParam("@codeName", activeCode.CodeName)
                                .WithSqlParam("@IsSuperAdmin", ApplicationSettings.isSuperAdmin)
                                .WithSqlParam("@showAll", activeCode.showAllActiveCodes)
                                .WithSqlParam("@userId", ApplicationSettings.UserId)
                                .WithSqlParam("@organizationId", activeCode.OrganizationIdFk)
                                .WithSqlParam("@page", activeCode.PageNumber)
                                .WithSqlParam("@size", activeCode.Rows)
                                .WithSqlParam("@sortOrder", activeCode.SortOrder)
                                .WithSqlParam("@sortCol", activeCode.SortCol)
                                .WithSqlParam("@filterVal", activeCode.FilterVal)
                                .ExecuteStoredProc_ToDictionary();

                objList.ForEach(x =>
                {
                    var bloodThinnerIds = x.ContainsKey("bloodThinners") && x["bloodThinners"] != null && x["bloodThinners"].ToString() != "" ? x["bloodThinners"].ToString().ToIntList() : new List<int>();
                    var bloodThinners = _controlListDetailsRepo.Table.Where(b => bloodThinnerIds.Contains(b.ControlListDetailId)).Select(b => new { Id = b.ControlListDetailId, b.Title }).ToList();
                    x.Add("bloodThinnersTitle", bloodThinners);
                });

                int totalRecords = 0;
                if (objList.Count > 0)
                {
                    totalRecords = objList.FirstOrDefault()["totalRecords"].ToInt();
                }
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Returned", Body = new { totalRecords, objList, fields = gridColumns.Body } };

            }
            return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Fields Name Not Found" };
        }

        public BaseResponse GetCodeDataById(int codeId, string codeName)
        {
            string tbl_Name = $"Code{(codeName != UCLEnums.Sepsis.ToString() ? codeName + "s" : codeName)}";
            var tableInfo = this._dbContext.LoadStoredProcedure("md_getTableInfoByTableName")
                                .WithSqlParam("@tableName", tbl_Name)
                                .ExecuteStoredProc_ToDictionary().FirstOrDefault();
            var fieldNames = tableInfo["fieldName"].ToString().Split(",").ToList();
            var fieldDataTypes = tableInfo["fieldDataType"].ToString().Split(",").ToList();

            //string qry = $"Select * " +
            //             $"from {tbl_Name} " +
            //             $"WHERE Code{codeName}Id = {codeId} and IsDeleted = 0";
            //var codeDataVM = this._dbContext.LoadSQLQuery(qry).ExecuteStoredProc_ToDictionary();

            var codeDataVM = this._dbContext.LoadStoredProcedure("md_getCodeDataById")
                                 .WithSqlParam("@codeName", codeName)
                                 .WithSqlParam("@codeId", codeId)
                                 .ExecuteStoredProc_ToDictionary().FirstOrDefault();

            if (codeDataVM != null && codeDataVM.Count() > 0)
            {
                if (codeName == UCLEnums.Stemi.ToString()) 
                {
                    int Id = codeDataVM["codeSTEMIId"].ToString().ToInt();
                    codeDataVM.Remove("codeSTEMIId");
                    codeDataVM.Add("codeStemiId", Id);
                }

                if (codeDataVM.ContainsKey("attachments") && codeDataVM["attachments"] != null && codeDataVM["attachments"].ToString() != "")
                {
                    string Attachments = codeDataVM["attachments"].ToString();
                    if (!string.IsNullOrEmpty(Attachments) && !string.IsNullOrWhiteSpace(Attachments))
                    {
                        string path = this._RootPath + Attachments;
                        List<string> FilesList = new();
                        if (Directory.Exists(path))
                        {
                            DirectoryInfo AttachFiles = new DirectoryInfo(path);
                            foreach (var item in AttachFiles.GetFiles())
                            {
                                FilesList.Add(Attachments + "/" + item.Name);
                            }
                            codeDataVM.Add("attachmentsPath", FilesList);
                        }
                    }
                }

                if (codeDataVM.ContainsKey("audio") && codeDataVM["audio"] != null && codeDataVM["audio"].ToString() != "")
                {
                    string audio = codeDataVM["audio"].ToString();
                    if (!string.IsNullOrEmpty(audio) && !string.IsNullOrWhiteSpace(audio))
                    {
                        string path = this._RootPath + audio;
                        List<string> FilesList = new();
                        if (Directory.Exists(path))
                        {
                            DirectoryInfo AttachFiles = new DirectoryInfo(path);
                            foreach (var item in AttachFiles.GetFiles())
                            {
                                FilesList.Add(audio + "/" + item.Name);
                            }
                            codeDataVM.Add("audiosPath", FilesList);
                        }
                    }
                }

                if (codeDataVM.ContainsKey("video") && codeDataVM["video"] != null && codeDataVM["video"].ToString() != "")
                {
                    string video = codeDataVM["video"].ToString();
                    if (!string.IsNullOrEmpty(video) && !string.IsNullOrWhiteSpace(video))
                    {
                        string path = this._RootPath + video;
                        List<string> FilesList = new();
                        if (Directory.Exists(path))
                        {
                            DirectoryInfo AttachFiles = new DirectoryInfo(path);
                            foreach (var item in AttachFiles.GetFiles())
                            {
                                FilesList.Add(video + "/" + item.Name);
                            }
                            codeDataVM.Add("videosPath", FilesList);
                        }
                    }
                }

                string Type = codeDataVM["isEMS"] != null && codeDataVM["isEMS"].ToString().ToBool() ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription();
                
                var serviceIds = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == codeDataVM["organizationIdFk"].ToString().ToInt() && x.CodeIdFk == codeName.GetActiveCodeId() && x.Type == Type && x.IsActive.HasValue && x.IsActive.Value && !x.IsDeleted).Select(x => new { x.DefaultServiceLineTeam, x.ServiceLineTeam1, x.ServiceLineTeam2 }).FirstOrDefault();
                var serviceLineIds = (from x in this._codesServiceLinesMappingRepo.Table
                                      where x.OrganizationIdFk == codeDataVM["organizationIdFk"].ToString().ToInt() && x.CodeIdFk == codeName.GetActiveCodeId()
                                      && x.ActiveCodeId == codeId && x.ActiveCodeName == codeName
                                      select new { x.DefaultServiceLineIdFk, x.ServiceLineId1Fk, x.ServiceLineId2Fk }).FirstOrDefault();

                if (serviceIds != null)
                {
                    List<int> defaultIds = new();
                    List<int> team1 = new();
                    List<int> team2 = new();
                    if (serviceLineIds != null)
                    {
                        defaultIds = serviceIds.DefaultServiceLineTeam.ToIntList().Where(x => serviceLineIds.DefaultServiceLineIdFk.ToIntList().Contains(x)).Distinct().ToList();
                        team1 = serviceIds.ServiceLineTeam1.ToIntList().Where(x => serviceLineIds.ServiceLineId1Fk.ToIntList().Contains(x)).Distinct().ToList();
                        team2 = serviceIds.ServiceLineTeam2.ToIntList().Where(x => serviceLineIds.ServiceLineId2Fk.ToIntList().Contains(x)).Distinct().ToList();
                    }
                    var DefaultServiceLineTeam = this._serviceLineRepo.Table.Where(x => serviceIds.DefaultServiceLineTeam.ToIntList().Distinct().Contains(x.ServiceLineId) && !x.IsDeleted).Select(x => new ServiceLineVM() { ServiceLineId = x.ServiceLineId, ServiceName = x.ServiceName, IsSelected = defaultIds.Contains(x.ServiceLineId) }).ToList();
                    var ServiceLineTeam1 = this._serviceLineRepo.Table.Where(x => serviceIds.ServiceLineTeam1.ToIntList().Distinct().Contains(x.ServiceLineId) && !x.IsDeleted).Select(x => new ServiceLineVM() { ServiceLineId = x.ServiceLineId, ServiceName = x.ServiceName, IsSelected = team1.Contains(x.ServiceLineId) }).ToList();
                    var ServiceLineTeam2 = this._serviceLineRepo.Table.Where(x => serviceIds.ServiceLineTeam2.ToIntList().Distinct().Contains(x.ServiceLineId) && !x.IsDeleted).Select(x => new ServiceLineVM() { ServiceLineId = x.ServiceLineId, ServiceName = x.ServiceName, IsSelected = team2.Contains(x.ServiceLineId) }).ToList();

                    codeDataVM.Add("defaultServiceLineTeam", DefaultServiceLineTeam);
                    codeDataVM.Add("serviceLineTeam1", ServiceLineTeam1);
                    codeDataVM.Add("serviceLineTeam2", ServiceLineTeam2);
                }


                if (codeDataVM["isEMS"].ToString() != null && codeDataVM["isEMS"].ToString().ToBool())
                    codeDataVM.Add("organizationData", GetHosplitalAddressObject(codeDataVM["organizationIdFk"].ToString().ToInt()));

                var indeces = fieldDataTypes.Select((c, i) => new { character = c, index = i })
                         .Where(list => list.character.Trim() == "datetime")
                         .Select(x => x.index)
                         .ToList();

                foreach (var item in indeces)
                {
                    string field = fieldNames.ElementAt(item).ToCamelCase();
                    var fieldVal = codeDataVM[field] != null && codeDataVM[field].ToString() != "" ? DateTime.Parse(codeDataVM[field].ToString()).ToTimezoneFromUtc("Eastern Standard Time").ToString("yyyy-MM-dd hh:mm:ss tt") : "";
                    codeDataVM.Add(field + "Str", fieldVal);
                }

                var bloodThinnerIds = codeDataVM.ContainsKey("bloodThinners") && codeDataVM["bloodThinners"] != null && codeDataVM["bloodThinners"].ToString() != "" ? codeDataVM["bloodThinners"].ToString().ToIntList() : new List<int>();
                var bloodThinners = _controlListDetailsRepo.Table.Where(b => bloodThinnerIds.Contains(b.ControlListDetailId)).Select(b => new { Id = b.ControlListDetailId, b.Title }).ToList();
                codeDataVM.Add("bloodThinnersTitle", bloodThinners);

                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Found", Body = codeDataVM };
            }
            return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Record Not Found" };
        }
        public BaseResponse AddOrUpdateCodeData(IDictionary<string, object> codeData)
        {

            if (codeData.ContainsKey($"code{codeData["codeName"].ToString()}Id") && codeData[$"code{codeData["codeName"].ToString()}Id"].ToString().ToInt() > 0)
            {
                string fieldName = codeData["fieldName"].ToString();
                int codeId = codeData[$"code{codeData["codeName"]}Id"].ToString().ToInt();
                string fieldDataType = codeData["fieldDataType"].ToString();
                string codeName = codeData["codeName"].ToString();
                var row = new CodeStroke();
                object fieldValue = new();
                if (fieldDataType != "file")
                {
                    if (fieldDataType == "date" || fieldDataType == "datetime")
                    {
                        if (codeData.ContainsKey(fieldName + "Str") && codeData[fieldName + "Str"].ToString() != null)
                        {
                            fieldValue = DateTime.Parse(codeData[fieldName + "Str"].ToString()).ToUniversalTimeZone();
                        }
                    }
                    else
                    if (codeData.ContainsKey(fieldName) && codeData[fieldName].ToString() != null)
                    {
                        fieldValue = codeData[fieldName].ToString();
                    }


                    row = this._dbContext.LoadStoredProcedure("md_UpdateCodes")
                                             .WithSqlParam("codeName", codeName)
                                             .WithSqlParam("fieldName", fieldName)
                                             .WithSqlParam("fieldValue", fieldValue)
                                             .WithSqlParam("codeId", codeId)
                                             .WithSqlParam("modifiedBy", ApplicationSettings.UserId)
                                             .ExecuteStoredProc<CodeStroke>().FirstOrDefault();

                    //this._dbContext.Log(fieldName, TableEnums.CodeStrokes.ToString(), codeId, ActivityLogActionEnums.Update.ToInt());
                    //var userIds = this._StrokeCodeGroupMembersRepo.Table.Where(x => x.StrokeCodeIdFk == codeId).Select(x => x.UserIdFk).ToList();

                    string qry = $"Select UserIdFk From Code{codeName}GroupMembers where {codeName}CodeIdFk = {codeId}";
                    var userIds = this._dbContext.LoadSQLQuery(qry).ExecuteStoredProc<CodeStrokeGroupMember>().Select(x => x.UserIdFk).ToList();

                    var userUniqueIds = this._userRepo.Table.Where(x => userIds.Contains(x.UserId)).Select(x => x.UserUniqueId).Distinct().ToList();
                    var superAdmins = this._userRepo.Table.Where(x => x.IsInGroup && !x.IsDeleted).Select(x => x.UserUniqueId).ToList();
                    userUniqueIds.AddRange(superAdmins);
                    var loggedUser = this._userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && !x.IsDeleted).Select(x => x.UserUniqueId).FirstOrDefault();
                    userUniqueIds.Add(loggedUser);

                    var notification = new PushNotificationVM()
                    {
                        Id = codeId,
                        OrgId = row.OrganizationIdFk,
                        FieldName = fieldName,
                        FieldDataType = fieldDataType,
                        FieldValue = fieldValue,
                        UserChannelSid = userUniqueIds.Distinct().ToList(),
                        From = codeName,
                        Msg = (row.IsEms != null && row.IsEms.Value ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription()) + $" {codeName} From is Changed",
                        RouteLink1 = ($"Code{codeName}Form").GetEnumDescription<RouteEnums>(), //RouteEnums.CodeStrokeForm.ToDescription(), // "/Home/Inhouse%20Codes/code-strok-form",
                        RouteLink2 = ($"EMSForms").GetEnumDescription<RouteEnums>() //RouteEnums.EMSForms.ToDescription(), // RouteEnums.EMSForms.ToDescription(),
                    };

                    _communicationService.pushNotification(notification);

                    return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Updated Successfully" };
                }
                else
                {

                    string Qry = $"Select OrganizationIdFk, IsEms From Code{(codeName != UCLEnums.Sepsis.ToString() ? codeName + "s" : codeName)} WHERE Code{codeName}Id = {codeId} and IsDeleted = 0";
                    var code = this._dbContext.LoadSQLQuery(Qry).ExecuteStoredProc<CodeStroke>().FirstOrDefault();

                    //row = this._codeStrokeRepo.Table.Where(x => x.CodeStrokeId == codeId && !x.IsDeleted).FirstOrDefault();

                    //row.ModifiedBy = ApplicationSettings.UserId;
                    //row.ModifiedDate = DateTime.UtcNow;
                    //row.IsDeleted = false;
                    fieldName = fieldName.ToCamelCase();
                    string jobj = codeData[fieldName].ToString();
                    fieldValue = JsonConvert.DeserializeObject<List<FilesVM>>(jobj);
                    var filesList = (List<FilesVM>)fieldValue;
                    string FolderRootPath = null;
                    string fieldNameAltered = fieldName == "attachment" ? fieldName.ToTitleCase() + "s" : fieldName.Replace("s", "").ToTitleCase();
                    if (filesList != null && filesList.Count > 0)
                    {
                        var RootPath = this._RootPath + "/Organizations"; //this._RootPath + "/Organizations";
                        string FileRoot = null;
                        List<string> Attachments = new();
                        FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == code.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                        FileRoot = Path.Combine(RootPath, FileRoot);
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, codeName);
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, codeId.ToString());
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, fieldNameAltered);

                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }

                        foreach (var item in filesList)
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
                            code.GetType().GetProperty(fieldNameAltered).SetValue(code, FileRoot.Replace(this._RootPath, "").Replace("\\", "/"));
                            FolderRootPath = FileRoot.Replace(this._RootPath, "").Replace("\\", "/");
                        }
                    }


                    List<string> FilesPath = new();
                    if (FolderRootPath != null)
                    {
                        string path = this._RootPath + FolderRootPath;
                        if (Directory.Exists(path))
                        {
                            DirectoryInfo AttachFiles = new DirectoryInfo(path);
                            FilesPath = new List<string>();
                            foreach (var item in AttachFiles.GetFiles())
                            {
                                FilesPath.Add(FolderRootPath + "/" + item.Name);
                            }
                        }
                    }

                    //this._codeStrokeRepo.Update(row);

                    Qry = $"Update Code{(codeName != UCLEnums.Sepsis.ToString() ? codeName + "s" : codeName)}" +
                            $" Set ModifiedBy = {ApplicationSettings.UserId}," +
                            $" ModifiedDate = '{DateTime.UtcNow}'," +
                            $" IsDeleted = 0," +
                            $" {fieldNameAltered} = '{code.GetPropertyValueByName(fieldNameAltered)}'" +
                            $" WHERE Code{codeName}Id = {codeId}";

                    int rowEffect = this._dbContext.Database.ExecuteSqlRaw(Qry);

                    //this._dbContext.Log(row, TableEnums.CodeStrokes.ToString(), codeId, ActivityLogActionEnums.FileUpload.ToInt());

                    var returnVal = new Dictionary<string, object>();
                    returnVal.Add((fieldName == "attachment" ? fieldName + "s" : fieldName) + "Path", FilesPath);
                    fieldValue = returnVal;


                    //var userIds = this._StrokeCodeGroupMembersRepo.Table.Where(x => x.StrokeCodeIdFk == codeId).Select(x => x.UserIdFk).ToList();

                    string qry = $"Select UserIdFk From Code{codeName}GroupMembers where {codeName}CodeIdFk = {codeId}";
                    var userIds = this._dbContext.LoadSQLQuery(qry).ExecuteStoredProc<CodeStrokeGroupMember>().Select(x => x.UserIdFk).ToList();

                    var userUniqueIds = this._userRepo.Table.Where(x => userIds.Contains(x.UserId)).Select(x => x.UserUniqueId).Distinct().ToList();
                    var superAdmins = this._userRepo.Table.Where(x => x.IsInGroup && !x.IsDeleted).Select(x => x.UserUniqueId).ToList();
                    userUniqueIds.AddRange(superAdmins);
                    var loggedUser = this._userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && !x.IsDeleted).Select(x => x.UserUniqueId).FirstOrDefault();
                    userUniqueIds.Add(loggedUser);

                    var notification = new PushNotificationVM()
                    {
                        Id = codeId,
                        OrgId = code.OrganizationIdFk,
                        FieldName = fieldName,
                        FieldDataType = fieldDataType,
                        FieldValue = fieldValue,
                        UserChannelSid = userUniqueIds,
                        From = codeName,
                        Msg = (row.IsEms != null && row.IsEms.Value ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription()) + $" {codeName} From is Changed",
                        RouteLink1 = ($"Code{codeName}Form").GetEnumDescription<RouteEnums>(), //RouteEnums.CodeStrokeForm.ToDescription(), // "/Home/Inhouse%20Codes/code-strok-form",
                        RouteLink2 = ($"EMSForms").GetEnumDescription<RouteEnums>() //RouteEnums.EMSForms.ToDescription(), // RouteEnums.EMSForms.ToDescription(),
                    };

                    _communicationService.pushNotification(notification);

                    return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Updated Successfully" };
                }

            }
            else
            {
                if (codeData.ContainsKey("organizationIdFk") && codeData["organizationIdFk"].ToString().ToInt() > 0)
                {
                    string codeName = codeData["codeName"].ToString();
                    string IsEMS = codeData["IsEms"].ToString() != null && codeData["IsEms"].ToString().ToBool() ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription();
                    var DefaultServiceLineTeam = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == codeData["organizationIdFk"].ToString().ToInt() && x.CodeIdFk == codeName.GetActiveCodeId() && x.Type == IsEMS && x.IsActive.HasValue && x.IsActive.Value && !x.IsDeleted).Select(x => x.DefaultServiceLineTeam).FirstOrDefault();
                    if (DefaultServiceLineTeam != null && DefaultServiceLineTeam != "")
                    {
                        var DefaultServiceLineIds = DefaultServiceLineTeam.ToIntList();

                        string tbl_Name = $"Code{(codeName != UCLEnums.Sepsis.ToString() ? codeName + "s" : codeName)}";
                        var tableInfo = this._dbContext.LoadStoredProcedure("md_getTableInfoByTableName")
                                            .WithSqlParam("@tableName", tbl_Name)
                                            .ExecuteStoredProc_ToDictionary().FirstOrDefault();
                        var fieldNames = tableInfo["fieldName"].ToString().Split(",").ToList();
                        var fieldDataTypes = tableInfo["fieldDataType"].ToString().Split(",").ToList();

                        var indeces = fieldDataTypes.Select((c, i) => new { character = c, index = i })
                                                   .Where(list => list.character.Trim() == "datetime")
                                                   .Select(x => x.index)
                                                   .ToList();

                        foreach (var item in indeces)
                        {
                            string field = fieldNames.ElementAt(item).Trim();
                            if (codeData.ContainsKey(field) && codeData[field] != null && codeData[field].ToString() != "") 
                            {
                                codeData[field] = DateTime.Parse(codeData[field].ToString()).ToUniversalTimeZone();
                            }
                        }
                        var conterName = $"Code_{codeName}_Counter";
                        var Counter = this._dbContext.LoadStoredProcedure("md_getMDRouteCounter").WithSqlParam("@C_Name", conterName).ExecuteStoredProc<MDRoute_CounterVM>().Select(x => x.Counter_Value).FirstOrDefault();
                        string query = $"INSERT INTO [dbo].[{tbl_Name}] (";

                        var keys = codeData.Keys.ToList();
                        var values = codeData.Values.ToList();

                        for (int i = 0; i < keys.Count(); i++)
                        {
                            if (keys[i] != $"code{codeName}Id" && keys[i] != "CreatedBy" && keys[i] != "CreatedDate" && keys[i] != "FamilyContactNumber" && keys[i] != "codeName")
                            {
                                query += $"[{keys[i]}],";
                            }
                        }

                        query += codeData.ContainsKey("FamilyContactNumber") ? "[FamilyContactNumber]," : "";
                        query += $"[Code{codeName}Number],";
                        query += "[CreatedBy],";
                        query += "[CreatedDate],";
                        query += "[IsDeleted]";
                        query += ")";
                        query += " VALUES (";

                        for (int i = 0; i < values.Count; i++)
                        {
                            if (keys[i] != $"code{codeName}Id" && keys[i] != "CreatedBy" && keys[i] != "CreatedDate" && keys[i] != "FamilyContactNumber" && keys[i] != "codeName")
                            {
                                query += $"'{values[i]}',";
                            }
                        }

                        query += codeData.ContainsKey("FamilyContactNumber") && codeData["FamilyContactNumber"] != null ? (codeData["FamilyContactNumber"].ToString() != "(___) ___-____" ? "'" + codeData["FamilyContactNumber"] + "'," : "'',") : "";
                        query += $"{Counter},";
                        query += $"'{ApplicationSettings.UserId}',";
                        query += $"'{DateTime.UtcNow}',";
                        query += "'0'";
                        query += ")";

                        int Id = this._dbContext.ExecuteInsertQuery(query);

                        //this._dbContext.Log(stroke, TableEnums.CodeStrokes.ToString(), stroke.CodeStrokeId, ActivityLogActionEnums.Create.ToInt());


                        return GetCodeDataById(Id, codeName);
                    }
                    return new BaseResponse() { Status = HttpStatusCode.NotAcceptable, Message = $"There is no Service Line in this organization related to Code {codeName}" };
                }
            }
            return new BaseResponse();
        }


        #endregion


        #region Code Stroke

        public BaseResponse GetAllStrokeCode(ActiveCodeVM activeCode)
        {

            var gridColumns = GetInhouseCodeTableFeilds(activeCode.OrganizationIdFk, UCLEnums.Stroke.ToString());
            dynamic Fields = gridColumns.Body;
            if (Fields != null && Fields.FieldName != null)
            {
                string FieldNames = Convert.ToString(Fields.FieldName);
                var objList = this._dbContext.LoadStoredProcedure("md_getAllActiveCodesOrEMS_Dynamic")
                                .WithSqlParam("@status", activeCode.Status)
                                .WithSqlParam("@colName", FieldNames)
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
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Returned", Body = new { totalRecords, objList, fields = gridColumns.Body } };

            }
            return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Fields Name Not Found" };
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
                string Type = StrokeDataVM.IsEms.HasValue && StrokeDataVM.IsEms.Value ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription();
                var serviceIds = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == strokeData.OrganizationIdFk && x.CodeIdFk == UCLEnums.Stroke.ToInt() && x.Type == Type && x.IsActive.HasValue && x.IsActive.Value && !x.IsDeleted).Select(x => new { x.DefaultServiceLineTeam, x.ServiceLineTeam1, x.ServiceLineTeam2 }).FirstOrDefault();
                var serviceLineIds = (from x in this._codesServiceLinesMappingRepo.Table
                                      where x.OrganizationIdFk == strokeData.OrganizationIdFk && x.CodeIdFk == UCLEnums.Stroke.ToInt()
                                      && x.ActiveCodeId == strokeData.CodeStrokeId && x.ActiveCodeName == UCLEnums.Stroke.ToString()
                                      select new { x.DefaultServiceLineIdFk, x.ServiceLineId1Fk, x.ServiceLineId2Fk }).FirstOrDefault();

                if (serviceIds != null)
                {
                    List<int> defaultIds = new();
                    List<int> team1 = new();
                    List<int> team2 = new();
                    if (serviceLineIds != null)
                    {
                        defaultIds = serviceIds.DefaultServiceLineTeam.ToIntList().Where(x => serviceLineIds.DefaultServiceLineIdFk.ToIntList().Contains(x)).Distinct().ToList();
                        team1 = serviceIds.ServiceLineTeam1.ToIntList().Where(x => serviceLineIds.ServiceLineId1Fk.ToIntList().Contains(x)).Distinct().ToList();
                        team2 = serviceIds.ServiceLineTeam2.ToIntList().Where(x => serviceLineIds.ServiceLineId2Fk.ToIntList().Contains(x)).Distinct().ToList();
                    }
                    StrokeDataVM.DefaultServiceLineTeam = this._serviceLineRepo.Table.Where(x => serviceIds.DefaultServiceLineTeam.ToIntList().Distinct().Contains(x.ServiceLineId) && !x.IsDeleted).Select(x => new ServiceLineVM() { ServiceLineId = x.ServiceLineId, ServiceName = x.ServiceName, IsSelected = defaultIds.Contains(x.ServiceLineId) }).ToList();
                    StrokeDataVM.ServiceLineTeam1 = this._serviceLineRepo.Table.Where(x => serviceIds.ServiceLineTeam1.ToIntList().Distinct().Contains(x.ServiceLineId) && !x.IsDeleted).Select(x => new ServiceLineVM() { ServiceLineId = x.ServiceLineId, ServiceName = x.ServiceName, IsSelected = team1.Contains(x.ServiceLineId) }).ToList();
                    StrokeDataVM.ServiceLineTeam2 = this._serviceLineRepo.Table.Where(x => serviceIds.ServiceLineTeam2.ToIntList().Distinct().Contains(x.ServiceLineId) && !x.IsDeleted).Select(x => new ServiceLineVM() { ServiceLineId = x.ServiceLineId, ServiceName = x.ServiceName, IsSelected = team2.Contains(x.ServiceLineId) }).ToList();
                }


                if (StrokeDataVM.IsEms.HasValue && StrokeDataVM.IsEms.Value)
                    StrokeDataVM.OrganizationData = GetHosplitalAddressObject(StrokeDataVM.OrganizationIdFk);

                StrokeDataVM.LastKnownWellStr = StrokeDataVM.LastKnownWell?.ToTimezoneFromUtc("Eastern Standard Time").ToString("yyyy-MM-dd hh:mm:ss tt");
                StrokeDataVM.DobStr = StrokeDataVM.Dob?.ToTimezoneFromUtc("Eastern Standard Time").ToString("yyyy-MM-dd hh:mm:ss tt");
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
                codeStroke.LastKnownWell = DateTime.Parse(codeStroke.LastKnownWellStr).ToUniversalTimeZone();
            }
            if (codeStroke != null && !string.IsNullOrEmpty(codeStroke.DobStr) && !string.IsNullOrWhiteSpace(codeStroke.DobStr))
            {
                codeStroke.Dob = DateTime.Parse(codeStroke.DobStr).ToUniversalTimeZone();
            }
            if (codeStroke.CodeStrokeId > 0)
            {
                var row = new CodeStroke();
                object fieldValue = new();
                if (codeStroke.FieldDataType != "file")
                {
                    var fieldName = string.Empty;
                    if (codeStroke.FieldName == "HPI" || codeStroke.FieldName == "DOB")
                    {
                        fieldName = codeStroke.FieldName.ToCapitalize();
                    }
                    else
                    {
                        fieldName = codeStroke.FieldName;
                    }

                    fieldValue = codeStroke.GetPropertyValueByName(fieldName);

                    row = this._dbContext.LoadStoredProcedure("md_UpdateCodes")
                                             .WithSqlParam("codeName", UCLEnums.Stroke.ToString())
                                             .WithSqlParam("fieldName", codeStroke.FieldName)
                                             .WithSqlParam("fieldValue", fieldValue)
                                             .WithSqlParam("codeId", codeStroke.CodeStrokeId)
                                             .WithSqlParam("modifiedBy", ApplicationSettings.UserId)
                                             .ExecuteStoredProc<CodeStroke>().FirstOrDefault();
                    this._dbContext.Log(row.getChangedPropertyObject(codeStroke.FieldName), ActivityLogTableEnums.CodeStrokes.ToString(), row.CodeStrokeId, ActivityLogActionEnums.Update.ToInt());
                    var userIds = this._StrokeCodeGroupMembersRepo.Table.Where(x => x.StrokeCodeIdFk == row.CodeStrokeId).Select(x => x.UserIdFk).ToList();
                    var userUniqueIds = this._userRepo.Table.Where(x => userIds.Contains(x.UserId)).Select(x => x.UserUniqueId).Distinct().ToList();
                    var superAdmins = this._userRepo.Table.Where(x => x.IsInGroup && !x.IsDeleted).Select(x => x.UserUniqueId).ToList();
                    userUniqueIds.AddRange(superAdmins);
                    var loggedUser = this._userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && !x.IsDeleted).Select(x => x.UserUniqueId).FirstOrDefault();
                    userUniqueIds.Add(loggedUser);
                    if (codeStroke.FieldDataType == "date")
                    {
                        fieldValue = codeStroke.GetPropertyValueByName(fieldName + "Str");
                    }
                    var notification = new PushNotificationVM()
                    {
                        Id = row.CodeStrokeId,
                        OrgId = row.OrganizationIdFk,
                        FieldName = codeStroke.FieldName,
                        FieldDataType = codeStroke.FieldDataType,
                        FieldValue = fieldValue,
                        UserChannelSid = userUniqueIds.Distinct().ToList(),
                        From = AuthorEnums.Stroke.ToString(),
                        Msg = (row.IsEms != null && row.IsEms.Value ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription()) + " Stroke From is Changed",
                        RouteLink1 = RouteEnums.CodeStrokeForm.ToDescription(), // "/Home/Inhouse%20Codes/code-strok-form",
                        RouteLink2 = RouteEnums.EMSForms.ToDescription(), // RouteEnums.EMSForms.ToDescription(),
                    };

                    _communicationService.pushNotification(notification);

                    return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Updated Successfully" };
                }
                else
                {

                    row = this._codeStrokeRepo.Table.Where(x => x.CodeStrokeId == codeStroke.CodeStrokeId && !x.IsDeleted).FirstOrDefault();

                    row.ModifiedBy = codeStroke.ModifiedBy;
                    row.ModifiedDate = DateTime.UtcNow;
                    row.IsDeleted = false;

                    if (codeStroke.Attachment != null && codeStroke.Attachment.Count > 0)
                    {
                        var RootPath = this._RootPath + "/Organizations"; //this._RootPath + "/Organizations";
                        string FileRoot = null;
                        List<string> Attachments = new();
                        FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == row.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
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
                        FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == row.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
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

                        FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == row.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
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
                        string path = this._RootPath + codeStroke.AttachmentsFolderRoot;
                        if (Directory.Exists(path))
                        {
                            DirectoryInfo AttachFiles = new DirectoryInfo(path);
                            codeStroke.AttachmentsPath = new List<string>();
                            foreach (var item in AttachFiles.GetFiles())
                            {
                                codeStroke.AttachmentsPath.Add(codeStroke.AttachmentsFolderRoot + "/" + item.Name);
                            }
                        }
                    }
                    if (codeStroke.VideoFolderRoot != null)
                    {
                        row.Video = codeStroke.VideoFolderRoot;
                        var path = this._RootPath + codeStroke.VideoFolderRoot;
                        if (Directory.Exists(path))
                        {
                            DirectoryInfo VideoFiles = new DirectoryInfo(path);
                            codeStroke.VideosPath = new List<string>();
                            foreach (var item in VideoFiles.GetFiles())
                            {
                                codeStroke.VideosPath.Add(codeStroke.VideoFolderRoot + "/" + item.Name);
                            }
                        }
                    }
                    if (codeStroke.AudioFolderRoot != null)
                    {
                        row.Audio = codeStroke.AudioFolderRoot;
                        string path = this._RootPath + codeStroke.AudioFolderRoot;
                        if (Directory.Exists(path))
                        {
                            DirectoryInfo AudioFiles = new DirectoryInfo(path);
                            codeStroke.AudiosPath = new List<string>();
                            foreach (var item in AudioFiles.GetFiles())
                            {
                                codeStroke.AudiosPath.Add(codeStroke.AudioFolderRoot + "/" + item.Name);
                            }
                        }
                    }

                    this._codeStrokeRepo.Update(row);

                    this._dbContext.Log(row, ActivityLogTableEnums.CodeStrokes.ToString(), row.CodeStrokeId, ActivityLogActionEnums.FileUpload.ToInt());
                    fieldValue = new { videosPath = codeStroke.VideosPath, audiosPath = codeStroke.AudiosPath, attachmentsPath = codeStroke.AttachmentsPath };
                    var userIds = this._StrokeCodeGroupMembersRepo.Table.Where(x => x.StrokeCodeIdFk == row.CodeStrokeId).Select(x => x.UserIdFk).ToList();
                    var userUniqueIds = this._userRepo.Table.Where(x => userIds.Contains(x.UserId)).Select(x => x.UserUniqueId).Distinct().ToList();
                    var superAdmins = this._userRepo.Table.Where(x => x.IsInGroup && !x.IsDeleted).Select(x => x.UserUniqueId).ToList();
                    userUniqueIds.AddRange(superAdmins);
                    var loggedUser = this._userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && !x.IsDeleted).Select(x => x.UserUniqueId).FirstOrDefault();
                    userUniqueIds.Add(loggedUser);

                    var notification = new PushNotificationVM()
                    {
                        Id = row.CodeStrokeId,
                        OrgId = row.OrganizationIdFk,
                        FieldName = codeStroke.FieldName,
                        FieldDataType = codeStroke.FieldDataType,
                        FieldValue = fieldValue,
                        UserChannelSid = userUniqueIds,
                        From = AuthorEnums.Stroke.ToString(),
                        Msg = (row.IsEms != null && row.IsEms.Value ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription()) + " Stroke From is Changed",
                        RouteLink1 = RouteEnums.CodeStrokeForm.ToDescription(), // "/Home/Inhouse%20Codes/code-strok-form",
                        RouteLink2 = RouteEnums.EMSForms.ToDescription(), // RouteEnums.EMSForms.ToDescription(),
                    };

                    _communicationService.pushNotification(notification);

                    return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Updated Successfully" };
                }

            }
            else
            {
                if (codeStroke.OrganizationIdFk > 0)
                {
                    string IsEMS = codeStroke.IsEms.HasValue && codeStroke.IsEms.Value ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription();
                    var DefaultServiceLineTeam = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == codeStroke.OrganizationIdFk && x.CodeIdFk == UCLEnums.Stroke.ToInt() && x.Type == IsEMS && x.IsActive.HasValue && x.IsActive.Value && !x.IsDeleted).Select(x => x.DefaultServiceLineTeam).FirstOrDefault();
                    if (DefaultServiceLineTeam != null && DefaultServiceLineTeam != "")
                    {
                        var DefaultServiceLineIds = DefaultServiceLineTeam.ToIntList();
                        //var ServiceLineTeam1Ids = codeStroke.ServiceLineTeam1Ids.ToIntList();
                        //var ServiceLineTeam2Ids = codeStroke.ServiceLineTeam2Ids.ToIntList();

                        codeStroke.CreatedDate = DateTime.UtcNow;
                        codeStroke.FamilyContactNumber = codeStroke.FamilyContactNumber != null && codeStroke.FamilyContactNumber != "" && codeStroke.FamilyContactNumber != "(___) ___-____" ? codeStroke.FamilyContactNumber : "";
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
                        this._dbContext.Log(stroke, ActivityLogTableEnums.CodeStrokes.ToString(), stroke.CodeStrokeId, ActivityLogActionEnums.Create.ToInt());


                        return GetStrokeDataById(stroke.CodeStrokeId);
                    }
                    return new BaseResponse() { Status = HttpStatusCode.NotAcceptable, Message = "There is no Service Line in this organization related to Code Stroke" };
                }
                return new BaseResponse() { Status = HttpStatusCode.NotAcceptable, Message = "Organization is not selected" };
            }
        }

        public BaseResponse CreateStrokeGroup(CodeStrokeVM codeStroke)
        {
            var DefaultServiceLineIds = codeStroke.DefaultServiceLineIds.ToIntList();
            bool usersFound = false;
            string errorMsg = "";

            var UserChannelSid = (from us in this._userSchedulesRepo.Table
                                  join u in this._userRepo.Table on us.UserIdFk equals u.UserId
                                  where DefaultServiceLineIds.Contains(us.ServiceLineIdFk.Value) && us.ScheduleDateStart <= DateTime.UtcNow && us.ScheduleDateEnd >= DateTime.UtcNow && !us.IsDeleted && !u.IsDeleted
                                  select new { u.UserUniqueId, u.UserId, ServiceLineIdFk = us.ServiceLineIdFk.Value }).Distinct().ToList();


            if (UserChannelSid.Count > 0)
            {
                usersFound = true;
                var defaultExist = DefaultServiceLineIds.Select(d => new { ServiceLineId = d, IsExist = UserChannelSid.Select(x => x.ServiceLineIdFk).Contains(d) }).ToList();
                if (defaultExist.Count > 0)
                {
                    if (!defaultExist.Select(x => x.IsExist).All(x => x == true))
                    {
                        var notExisted = defaultExist.Where(x => !x.IsExist).Select(x => x.ServiceLineId).ToList();
                        DefaultServiceLineIds.RemoveAll(d => notExisted.Contains(d));
                        var services = this._serviceLineRepo.Table.Where(x => notExisted.Contains(x.ServiceLineId)).Select(x => new { x.ServiceLineId, x.ServiceName }).ToList();
                        foreach (var item in services)
                        {
                            errorMsg += Environment.NewLine + "No OnCall user found in " + item.ServiceName;
                        }
                    }
                }
                else
                {
                    var services = this._serviceLineRepo.Table.Where(x => DefaultServiceLineIds.Contains(x.ServiceLineId)).Select(x => new { x.ServiceLineId, x.ServiceName }).ToList();
                    foreach (var item in services)
                    {
                        errorMsg += Environment.NewLine + "No OnCall user found in " + item.ServiceName;
                    }
                    DefaultServiceLineIds = new List<int>();
                }

                if (DefaultServiceLineIds.Count > 0)
                {
                    var codeService = new CodesServiceLinesMapping()
                    {
                        OrganizationIdFk = codeStroke.OrganizationIdFk,
                        CodeIdFk = UCLEnums.Stroke.ToInt(),
                        DefaultServiceLineIdFk = string.Join(",", DefaultServiceLineIds),
                        ActiveCodeId = codeStroke.CodeStrokeId,
                        ActiveCodeName = UCLEnums.Stroke.ToString()
                    };
                    this._codesServiceLinesMappingRepo.Insert(codeService);

                }
            }
            else
            {
                DefaultServiceLineIds = new List<int>();
            }

            var superAdmins = this._userRepo.Table.Where(x => x.IsInGroup && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId, ServiceLineIdFk = 0 }).ToList();
            UserChannelSid.AddRange(superAdmins);
            var loggedUser = this._userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId, ServiceLineIdFk = 0 }).FirstOrDefault();
            UserChannelSid.Add(loggedUser);
            var conversationChannelAttributes = JsonConvert.SerializeObject(new Dictionary<string, Object>()
                                    {
                                        {ChannelAttributeEnums.ChannelType.ToString(), ChannelTypeEnums.EMS.ToString()},
                                        {ChannelAttributeEnums.CodeType.ToString(), ChannelTypeEnums.Stroke.ToString()},
                                        {ChannelAttributeEnums.StrokeId.ToString(), codeStroke.CodeStrokeId}
                                    }, Formatting.Indented);
            List<CodeStrokeGroupMember> ACodeGroupMembers = new List<CodeStrokeGroupMember>();
            if (UserChannelSid != null && UserChannelSid.Count > 0)
            {
                string uniqueName = DateTime.Now.ToString("yyyyMMddHHmmssffff") + ApplicationSettings.UserId.ToString();
                string friendlyName = codeStroke.IsEms.HasValue && codeStroke.IsEms.Value ? $"{UCLEnums.EMS.ToDescription()} {UCLEnums.Stroke.ToString()} {codeStroke.CodeStrokeId}" : $"{UCLEnums.InhouseCode.ToDescription()} {UCLEnums.Stroke.ToString()} {codeStroke.CodeStrokeId}";
                var channel = this._communicationService.createConversationChannel(friendlyName, uniqueName, conversationChannelAttributes);
                var stroke = this._codeStrokeRepo.Table.Where(x => x.CodeStrokeId == codeStroke.CodeStrokeId && x.IsActive == true && !x.IsDeleted).FirstOrDefault();
                if (stroke != null)
                {
                    stroke.ChannelSid = channel.Sid;
                    this._codeStrokeRepo.Update(stroke);
                }
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
                        _communicationService.addNewUserToConversationChannel(channel.Sid, item.UserUniqueId);
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
                msg.body = $"<strong> {(codeStroke.IsEms.HasValue && codeStroke.IsEms.Value ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription())} {UCLEnums.Stroke.ToString()} </strong></br></br>";
                if (codeStroke.PatientName != null && codeStroke.PatientName != "")
                    msg.body += $"<strong>Patient Name: </strong> {codeStroke.PatientName} </br>";
                if (codeStroke.Dob != null)
                    msg.body += $"<strong>Dob: </strong> {codeStroke.Dob:MM-dd-yyyy} </br>";
                if (codeStroke.LastKnownWell != null)
                    msg.body += $"<strong>Last Well Known: </strong> {codeStroke.LastKnownWell:MM-dd-yyyy hh:mm tt} </br>";
                if (codeStroke.ChiefComplant != null && codeStroke.ChiefComplant != "")
                    msg.body += $"<strong>Chief Complaint: </strong> {codeStroke.ChiefComplant} </br>";
                if (codeStroke.Hpi != null && codeStroke.Hpi != "")
                    msg.body += $"<strong>Hpi: </strong> {codeStroke.Hpi} </br>";

                var sendMsg = _communicationService.sendPushNotification(msg);

                var showAllAccessUsers = this._dbContext.LoadStoredProcedure("md_getUsersOfComponentAccess")
                                    .WithSqlParam("@componentName", "Show All EMS,Show All Active Codes,Show Graphs,Show EMS,Show Active Codes")
                                    .WithSqlParam("@orgId", stroke.OrganizationIdFk)
                                    .ExecuteStoredProc<RegisterCredentialVM>().Select(x => new { x.UserUniqueId, x.UserId, ServiceLineIdFk = 0 }).Distinct().ToList();
                UserChannelSid.AddRange(showAllAccessUsers);
                var notification = new PushNotificationVM()
                {
                    Id = stroke.CodeStrokeId,
                    OrgId = stroke.OrganizationIdFk,
                    UserChannelSid = UserChannelSid.Select(x => x.UserUniqueId).Distinct().ToList(),
                    From = AuthorEnums.Stroke.ToString(),
                    Msg = (codeStroke.IsEms.HasValue && codeStroke.IsEms.Value ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription()) + " Stroke is update",
                    RouteLink3 = RouteEnums.ActiveEMS.ToDescription(), // RouteEnums.ActiveEMS.ToDescription(),
                    RouteLink4 = RouteEnums.Dashboard.ToDescription(),
                    RouteLink5 = RouteEnums.InhouseCodeGrid.ToDescription(), // RouteEnums.InhouseCodeGrid.ToDescription()
                };

                _communicationService.pushNotification(notification);
            }

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = errorMsg, Body = new { serviceLineUsersFound = usersFound, DefaultServiceLineIds, ServiceLineTeam1Ids = new List<int>(), ServiceLineTeam2Ids = new List<int>() } };
        }

        public BaseResponse UpdateStrokeGroupMembers(CodeStrokeVM codeStroke)
        {

            if (codeStroke.DefaultServiceLineIds == null || codeStroke.DefaultServiceLineIds == "")
            {
                string IsEMS = codeStroke.IsEms.HasValue && codeStroke.IsEms.Value ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription();
                codeStroke.DefaultServiceLineIds = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == codeStroke.OrganizationIdFk && x.CodeIdFk == UCLEnums.Stroke.ToInt() && x.Type == IsEMS && x.IsActive.HasValue && x.IsActive.Value && !x.IsDeleted).Select(x => x.DefaultServiceLineTeam).FirstOrDefault();
            }

            if (codeStroke.DefaultServiceLineIds != null && codeStroke.DefaultServiceLineIds != "")
            {
                bool userNotFound = false;

                string msg = "";

                var DefaultServiceLineIds = codeStroke.DefaultServiceLineIds.ToIntList();
                var ServiceLineTeam1Ids = codeStroke.ServiceLineTeam1Ids.ToIntList();
                var ServiceLineTeam2Ids = codeStroke.ServiceLineTeam2Ids.ToIntList();

                var UserChannelSid = (from us in this._userSchedulesRepo.Table
                                      join u in this._userRepo.Table on us.UserIdFk equals u.UserId
                                      where (DefaultServiceLineIds.Contains(us.ServiceLineIdFk.Value) || ServiceLineTeam1Ids.Contains(us.ServiceLineIdFk.Value) || ServiceLineTeam2Ids.Contains(us.ServiceLineIdFk.Value)) && us.ScheduleDateStart <= DateTime.UtcNow && us.ScheduleDateEnd >= DateTime.UtcNow && !us.IsDeleted && !u.IsDeleted
                                      select new { u.UserUniqueId, u.UserId, ServiceLineIdFk = us.ServiceLineIdFk.Value }).Distinct().ToList();

                var codeServiceMapping = this._codesServiceLinesMappingRepo.Table.Where(x => x.OrganizationIdFk == codeStroke.OrganizationIdFk && x.CodeIdFk == UCLEnums.Stroke.ToInt() && x.ActiveCodeId == codeStroke.CodeStrokeId).ToList();
                if (codeServiceMapping.Count > 0)
                    this._codesServiceLinesMappingRepo.DeleteRange(codeServiceMapping);

                if (UserChannelSid.Count > 0)
                {
                    userNotFound = true;
                    var defaultExist = DefaultServiceLineIds.Select(d => new { ServiceLineId = d, IsExist = UserChannelSid.Select(x => x.ServiceLineIdFk).Contains(d) }).ToList();
                    if (defaultExist.Count > 0)
                    {
                        if (!defaultExist.Select(x => x.IsExist).All(x => x == true))
                        {
                            var notExisted = defaultExist.Where(x => !x.IsExist).Select(x => x.ServiceLineId).ToList();
                            DefaultServiceLineIds.RemoveAll(d => notExisted.Contains(d));
                            var services = this._serviceLineRepo.Table.Where(x => notExisted.Contains(x.ServiceLineId)).Select(x => new { x.ServiceLineId, x.ServiceName }).ToList();
                            foreach (var item in services)
                            {
                                msg += Environment.NewLine + "No OnCall user found in " + item.ServiceName;
                            }
                        }
                    }
                    else
                    {
                        var services = this._serviceLineRepo.Table.Where(x => DefaultServiceLineIds.Contains(x.ServiceLineId)).Select(x => new { x.ServiceLineId, x.ServiceName }).ToList();
                        foreach (var item in services)
                        {
                            msg += Environment.NewLine + "No OnCall user found in " + item.ServiceName;
                        }
                        DefaultServiceLineIds = new List<int>();
                    }

                    var serviceLineTeam1Exist = ServiceLineTeam1Ids.Select(d => new { ServiceLineId = d, IsExist = UserChannelSid.Select(x => x.ServiceLineIdFk).Contains(d) }).ToList();
                    if (serviceLineTeam1Exist.Count > 0)
                    {
                        if (!serviceLineTeam1Exist.Select(x => x.IsExist).All(x => x == true))
                        {
                            var notExisted = serviceLineTeam1Exist.Where(x => !x.IsExist).Select(x => x.ServiceLineId).ToList();
                            ServiceLineTeam1Ids.RemoveAll(d => notExisted.Contains(d));
                            var services = this._serviceLineRepo.Table.Where(x => notExisted.Contains(x.ServiceLineId)).Select(x => new { x.ServiceLineId, x.ServiceName }).ToList();
                            foreach (var item in services)
                            {
                                msg += Environment.NewLine + "No OnCall user found in " + item.ServiceName;
                            }
                        }
                    }
                    else
                    {
                        var services = this._serviceLineRepo.Table.Where(x => ServiceLineTeam1Ids.Contains(x.ServiceLineId)).Select(x => new { x.ServiceLineId, x.ServiceName }).ToList();
                        foreach (var item in services)
                        {
                            msg += Environment.NewLine + "No OnCall user found in " + item.ServiceName;
                        }
                        ServiceLineTeam1Ids = new List<int>();
                    }


                    var serviceLineTeam2Exist = ServiceLineTeam2Ids.Select(d => new { ServiceLineId = d, IsExist = UserChannelSid.Select(x => x.ServiceLineIdFk).Contains(d) }).ToList();
                    if (serviceLineTeam2Exist.Count > 0)
                    {
                        if (!serviceLineTeam2Exist.Select(x => x.IsExist).All(x => x == true))
                        {
                            var notExisted = serviceLineTeam2Exist.Where(x => !x.IsExist).Select(x => x.ServiceLineId).ToList();
                            ServiceLineTeam2Ids.RemoveAll(d => notExisted.Contains(d));
                            var services = this._serviceLineRepo.Table.Where(x => notExisted.Contains(x.ServiceLineId)).Select(x => new { x.ServiceLineId, x.ServiceName }).ToList();
                            foreach (var item in services)
                            {
                                msg += Environment.NewLine + "No OnCall user found in " + item.ServiceName;
                            }
                        }
                    }
                    else
                    {
                        var services = this._serviceLineRepo.Table.Where(x => ServiceLineTeam2Ids.Contains(x.ServiceLineId)).Select(x => new { x.ServiceLineId, x.ServiceName }).ToList();
                        foreach (var item in services)
                        {
                            msg += Environment.NewLine + "No OnCall user found in " + item.ServiceName;
                        }
                        ServiceLineTeam2Ids = new List<int>();
                    }



                    if (DefaultServiceLineIds.Count > 0 || ServiceLineTeam1Ids.Count > 0 || ServiceLineTeam2Ids.Count > 0)
                    {
                        var codeService = new CodesServiceLinesMapping()
                        {
                            OrganizationIdFk = codeStroke.OrganizationIdFk,
                            CodeIdFk = UCLEnums.Stroke.ToInt(),
                            DefaultServiceLineIdFk = DefaultServiceLineIds.Count > 0 ? string.Join(",", DefaultServiceLineIds) : null,
                            ServiceLineId1Fk = ServiceLineTeam1Ids.Count > 0 ? string.Join(",", ServiceLineTeam1Ids) : null,
                            ServiceLineId2Fk = ServiceLineTeam2Ids.Count > 0 ? string.Join(",", ServiceLineTeam2Ids) : null,
                            ActiveCodeId = codeStroke.CodeStrokeId,
                            ActiveCodeName = UCLEnums.Stroke.ToString()
                        };
                        this._codesServiceLinesMappingRepo.Insert(codeService);
                    }
                }
                else
                {
                    DefaultServiceLineIds = new List<int>();
                    ServiceLineTeam1Ids = new List<int>();
                    ServiceLineTeam2Ids = new List<int>();
                }


                var superAdmins = this._userRepo.Table.Where(x => x.IsInGroup && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId, ServiceLineIdFk = 0 }).ToList();
                UserChannelSid.AddRange(superAdmins);
                var loggedInUser = this._userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId, ServiceLineIdFk = 0 }).FirstOrDefault();
                UserChannelSid.Add(loggedInUser);

                if (codeStroke.ChannelSid != null && codeStroke.ChannelSid != "")
                {
                    var channelSid = codeStroke.ChannelSid; //channel.Select(x => x.ChannelSid).FirstOrDefault();

                    var groupMembers = this._StrokeCodeGroupMembersRepo.Table.Where(x => x.StrokeCodeIdFk == codeStroke.CodeStrokeId).ToList();
                    this._StrokeCodeGroupMembersRepo.DeleteRange(groupMembers);
                    //this._StrokeCodeGroupMembersRepo.DeleteRange(channel);
                    bool isDeleted = _communicationService.DeleteUserToConversationChannel(channelSid);
                    List<CodeStrokeGroupMember> ACodeGroupMembers = new List<CodeStrokeGroupMember>();
                    foreach (var item in UserChannelSid.Distinct())
                    {
                        try
                        {
                            var codeGroupMember = new CodeStrokeGroupMember()
                            {
                                UserIdFk = item.UserId,
                                StrokeCodeIdFk = codeStroke.CodeStrokeId,
                                //ActiveCodeName = UCLEnums.Stroke.ToString(),
                                IsAcknowledge = false,
                                CreatedBy = ApplicationSettings.UserId,
                                CreatedDate = DateTime.UtcNow,
                                IsDeleted = false
                            };
                            ACodeGroupMembers.Add(codeGroupMember);
                            _communicationService.addNewUserToConversationChannel(channelSid, item.UserUniqueId);
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
                    Id = codeStroke.CodeStrokeId,
                    OrgId = codeStroke.OrganizationIdFk,
                    UserChannelSid = UserChannelSid.Select(x => x.UserUniqueId).Distinct().ToList(),
                    From = AuthorEnums.Stroke.ToString(),
                    Msg = (codeStroke.IsEms.HasValue && codeStroke.IsEms.Value ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription()) + " Stroke From is Changed",
                    RouteLink1 = RouteEnums.CodeStrokeForm.ToDescription(), // "/Home/Inhouse%20Codes/code-strok-form",
                    RouteLink2 = RouteEnums.EMSForms.ToDescription(), // RouteEnums.EMSForms.ToDescription(),
                };

                _communicationService.pushNotification(notification);

                return new BaseResponse() { Status = HttpStatusCode.OK, Message = msg, Body = new { serviceLineUsersFound = userNotFound, DefaultServiceLineIds, ServiceLineTeam1Ids, ServiceLineTeam2Ids } };
            }
            return new BaseResponse() { Status = HttpStatusCode.NotModified, Message = "Default Service Lines Required" };
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
            this._dbContext.Log(new { }, ActivityLogTableEnums.CodeStrokes.ToString(), strokeId, ActivityLogActionEnums.Delete.ToInt());

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

            var rowsEffected = this._dbContext.Database.ExecuteSqlRaw(sql, parameters);
            if (rowsEffected > 0)
            {
                this._dbContext.Log(new { }, ActivityLogTableEnums.CodeStrokes.ToString(), strokeId, status == false ? ActivityLogActionEnums.Inactive.ToInt() : ActivityLogActionEnums.Active.ToInt());
                var userIds = this._dbContext.LoadStoredProcedure("md_getAllActiveCodesGroupUserIds")
                                                .WithSqlParam("@codeName", UCLEnums.Stroke.ToString())
                                                .WithSqlParam("@codeId", strokeId)
                                                .ExecuteStoredProc<CodeStrokeVM>();

                var notification = new PushNotificationVM()
                {
                    Id = strokeId,
                    OrgId = userIds.Select(x => x.OrganizationIdFk).FirstOrDefault(),
                    Type = "ChannelStatusChanged",
                    ChannelIsActive = status,
                    ChannelSid = userIds.Select(x => x.ChannelSid).FirstOrDefault(),
                    UserChannelSid = userIds.Select(x => x.UserUniqueId).Distinct().ToList(),
                    From = UCLEnums.Stroke.ToString(),
                    Msg = UCLEnums.Stroke.ToString() + " is " + (status ? "Activated" : "Inactivated"),
                    RouteLink3 = RouteEnums.ActiveEMS.ToDescription(), // RouteEnums.ActiveEMS.ToDescription(),
                    RouteLink4 = RouteEnums.Dashboard.ToDescription(), // RouteEnums.Dashboard.ToDescription(),
                    RouteLink5 = RouteEnums.InhouseCodeGrid.ToDescription(), // RouteEnums.InhouseCodeGrid.ToDescription()
                };

                _communicationService.pushNotification(notification);
            }

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Deleted" };
        }

        #endregion

        #region Code Sepsis


        public BaseResponse GetAllSepsisCode(ActiveCodeVM activeCode)
        {

            var gridColumns = GetInhouseCodeTableFeilds(activeCode.OrganizationIdFk, UCLEnums.Sepsis.ToString());
            dynamic Fields = gridColumns.Body;
            if (Fields != null && Fields.FieldName != null)
            {
                string FieldNames = Convert.ToString(Fields.FieldName);
                var objList = this._dbContext.LoadStoredProcedure("md_getAllActiveCodesOrEMS_Dynamic")
                                .WithSqlParam("@status", activeCode.Status)
                                .WithSqlParam("@colName", FieldNames)
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
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Returned", Body = new { totalRecords, objList, fields = gridColumns.Body } };

            }
            return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Fields Name Not Found" };
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
                string Type = SepsisDataVM.IsEms ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription();
                var serviceIds = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == SepsisData.OrganizationIdFk && x.CodeIdFk == UCLEnums.Sepsis.ToInt() && x.Type == Type && x.IsActive.HasValue && x.IsActive.Value && !x.IsDeleted).Select(x => new { x.DefaultServiceLineTeam, x.ServiceLineTeam1, x.ServiceLineTeam2 }).FirstOrDefault();
                var serviceLineIds = (from x in this._codesServiceLinesMappingRepo.Table
                                      where x.OrganizationIdFk == SepsisData.OrganizationIdFk && x.CodeIdFk == UCLEnums.Sepsis.ToInt()
                                      && x.ActiveCodeId == SepsisData.CodeSepsisId && x.ActiveCodeName == UCLEnums.Sepsis.ToString()
                                      select new { x.DefaultServiceLineIdFk, x.ServiceLineId1Fk, x.ServiceLineId2Fk }).FirstOrDefault();
                if (serviceIds != null)
                {
                    List<int> defaultIds = new();
                    List<int> team1 = new();
                    List<int> team2 = new();
                    if (serviceLineIds != null)
                    {
                        defaultIds = serviceIds.DefaultServiceLineTeam.ToIntList().Where(x => serviceLineIds.DefaultServiceLineIdFk.ToIntList().Contains(x)).Distinct().ToList();
                        team1 = serviceIds.ServiceLineTeam1.ToIntList().Where(x => serviceLineIds.ServiceLineId1Fk.ToIntList().Contains(x)).Distinct().ToList();
                        team2 = serviceIds.ServiceLineTeam2.ToIntList().Where(x => serviceLineIds.ServiceLineId2Fk.ToIntList().Contains(x)).Distinct().ToList();
                    }
                    SepsisDataVM.DefaultServiceLineTeam = this._serviceLineRepo.Table.Where(x => serviceIds.DefaultServiceLineTeam.ToIntList().Distinct().Contains(x.ServiceLineId) && !x.IsDeleted).Select(x => new ServiceLineVM() { ServiceLineId = x.ServiceLineId, ServiceName = x.ServiceName, IsSelected = defaultIds.Contains(x.ServiceLineId) }).ToList();
                    SepsisDataVM.ServiceLineTeam1 = this._serviceLineRepo.Table.Where(x => serviceIds.ServiceLineTeam1.ToIntList().Distinct().Contains(x.ServiceLineId) && !x.IsDeleted).Select(x => new ServiceLineVM() { ServiceLineId = x.ServiceLineId, ServiceName = x.ServiceName, IsSelected = team1.Contains(x.ServiceLineId) }).ToList();
                    SepsisDataVM.ServiceLineTeam2 = this._serviceLineRepo.Table.Where(x => serviceIds.ServiceLineTeam2.ToIntList().Distinct().Contains(x.ServiceLineId) && !x.IsDeleted).Select(x => new ServiceLineVM() { ServiceLineId = x.ServiceLineId, ServiceName = x.ServiceName, IsSelected = team2.Contains(x.ServiceLineId) }).ToList();
                }

                if (SepsisDataVM.IsEms)
                    SepsisDataVM.OrganizationData = GetHosplitalAddressObject(SepsisDataVM.OrganizationIdFk);
                SepsisDataVM.LastKnownWellStr = SepsisDataVM.LastKnownWell?.ToTimezoneFromUtc("Eastern Standard Time").ToString("yyyy-MM-dd hh:mm:ss tt");
                SepsisDataVM.DobStr = SepsisDataVM.Dob?.ToTimezoneFromUtc("Eastern Standard Time").ToString("yyyy-MM-dd hh:mm:ss tt");
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
                codeSepsis.LastKnownWell = DateTime.Parse(codeSepsis.LastKnownWellStr).ToUniversalTimeZone(); ;
            }
            if (codeSepsis != null && !string.IsNullOrEmpty(codeSepsis.DobStr) && !string.IsNullOrWhiteSpace(codeSepsis.DobStr))
            {
                codeSepsis.Dob = DateTime.Parse(codeSepsis.DobStr).ToUniversalTimeZone(); ;
            }
            if (codeSepsis.CodeSepsisId > 0)
            {
                var row = new CodeSepsi();
                object fieldValue = new();
                if (codeSepsis.FieldDataType != "file")
                {
                    var fieldName = string.Empty;
                    if (codeSepsis.FieldName == "HPI" || codeSepsis.FieldName == "DOB")
                    {
                        fieldName = codeSepsis.FieldName.ToCapitalize();
                    }
                    else
                    {
                        fieldName = codeSepsis.FieldName;
                    }

                    fieldValue = codeSepsis.GetPropertyValueByName(fieldName);

                    row = this._dbContext.LoadStoredProcedure("md_UpdateCodes")
                                             .WithSqlParam("codeName", UCLEnums.Sepsis.ToString())
                                             .WithSqlParam("fieldName", codeSepsis.FieldName)
                                             .WithSqlParam("fieldValue", fieldValue)
                                             .WithSqlParam("codeId", codeSepsis.CodeSepsisId)
                                             .WithSqlParam("modifiedBy", ApplicationSettings.UserId)
                                             .ExecuteStoredProc<CodeSepsi>().FirstOrDefault();
                    this._dbContext.Log(row.getChangedPropertyObject(fieldName), ActivityLogTableEnums.CodeSepsis.ToString(), row.CodeSepsisId, ActivityLogActionEnums.Update.ToInt());

                    var userIds = this._SepsisCodeGroupMembersRepo.Table.Where(x => x.SepsisCodeIdFk == row.CodeSepsisId).Select(x => x.UserIdFk).ToList();
                    var userUniqueIds = this._userRepo.Table.Where(x => userIds.Contains(x.UserId)).Select(x => x.UserUniqueId).Distinct().ToList();
                    var superAdmins = this._userRepo.Table.Where(x => x.IsInGroup && !x.IsDeleted).Select(x => x.UserUniqueId).ToList();
                    userUniqueIds.AddRange(superAdmins);
                    var loggedUser = this._userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && !x.IsDeleted).Select(x => x.UserUniqueId).FirstOrDefault();
                    userUniqueIds.Add(loggedUser);
                    if (codeSepsis.FieldDataType == "date")
                    {
                        fieldValue = codeSepsis.GetPropertyValueByName(fieldName + "Str");
                    }
                    var notification = new PushNotificationVM()
                    {
                        Id = row.CodeSepsisId,
                        OrgId = row.OrganizationIdFk,
                        FieldName = codeSepsis.FieldName,
                        FieldDataType = codeSepsis.FieldDataType,
                        FieldValue = fieldValue,
                        UserChannelSid = userUniqueIds.Distinct().ToList(),
                        From = AuthorEnums.Sepsis.ToString(),
                        Msg = (row.IsEms ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription()) + " Sepsis From is Changed",
                        RouteLink1 = RouteEnums.CodeSepsisForm.ToDescription(), // "/Home/Inhouse%20Codes/code-strok-form",
                        RouteLink2 = RouteEnums.EMSForms.ToDescription(), // RouteEnums.EMSForms.ToDescription(),
                    };

                    _communicationService.pushNotification(notification);
                    return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Updated Successfully" };
                }
                else
                {

                    row = this._codeSepsisRepo.Table.Where(x => x.CodeSepsisId == codeSepsis.CodeSepsisId && !x.IsDeleted).FirstOrDefault();

                    row.ModifiedBy = codeSepsis.ModifiedBy;
                    row.ModifiedDate = DateTime.UtcNow;
                    row.IsDeleted = false;

                    if (codeSepsis.Attachment != null && codeSepsis.Attachment.Count > 0)
                    {
                        var RootPath = this._RootPath + "/Organizations"; //this._RootPath + "/Organizations";
                        string FileRoot = null;
                        List<string> Attachments = new();
                        FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == row.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
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
                        FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == row.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
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

                        FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == row.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
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
                        string path = this._RootPath + codeSepsis.AttachmentsFolderRoot;
                        if (Directory.Exists(path))
                        {
                            DirectoryInfo AttachFiles = new DirectoryInfo(path);
                            codeSepsis.AttachmentsPath = new List<string>();
                            foreach (var item in AttachFiles.GetFiles())
                            {
                                codeSepsis.AttachmentsPath.Add(codeSepsis.AttachmentsFolderRoot + "/" + item.Name);
                            }
                        }
                    }
                    if (codeSepsis.VideoFolderRoot != null)
                    {
                        row.Video = codeSepsis.VideoFolderRoot;
                        var path = this._RootPath + codeSepsis.VideoFolderRoot;
                        if (Directory.Exists(path))
                        {
                            DirectoryInfo VideoFiles = new DirectoryInfo(path);
                            codeSepsis.VideosPath = new List<string>();
                            foreach (var item in VideoFiles.GetFiles())
                            {
                                codeSepsis.VideosPath.Add(codeSepsis.VideoFolderRoot + "/" + item.Name);
                            }
                        }
                    }
                    if (codeSepsis.AudioFolderRoot != null)
                    {
                        row.Audio = codeSepsis.AudioFolderRoot;
                        string path = this._RootPath + codeSepsis.AudioFolderRoot;
                        if (Directory.Exists(path))
                        {
                            DirectoryInfo AudioFiles = new DirectoryInfo(path);
                            codeSepsis.AudiosPath = new List<string>();
                            foreach (var item in AudioFiles.GetFiles())
                            {
                                codeSepsis.AudiosPath.Add(codeSepsis.AudioFolderRoot + "/" + item.Name);
                            }
                        }
                    }

                    this._codeSepsisRepo.Update(row);
                    this._dbContext.Log(row, ActivityLogTableEnums.CodeSepsis.ToString(), row.CodeSepsisId, ActivityLogActionEnums.FileUpload.ToInt());

                    fieldValue = new { videosPath = codeSepsis.VideosPath, audiosPath = codeSepsis.AudiosPath, attachmentsPath = codeSepsis.AttachmentsPath };
                    var userIds = this._SepsisCodeGroupMembersRepo.Table.Where(x => x.SepsisCodeIdFk == row.CodeSepsisId).Select(x => x.UserIdFk).ToList();
                    var userUniqueIds = this._userRepo.Table.Where(x => userIds.Contains(x.UserId)).Select(x => x.UserUniqueId).Distinct().ToList();
                    var superAdmins = this._userRepo.Table.Where(x => x.IsInGroup && !x.IsDeleted).Select(x => x.UserUniqueId).ToList();
                    userUniqueIds.AddRange(superAdmins);
                    var loggedUser = this._userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && !x.IsDeleted).Select(x => x.UserUniqueId).FirstOrDefault();
                    userUniqueIds.Add(loggedUser);

                    var notification = new PushNotificationVM()
                    {
                        Id = row.CodeSepsisId,
                        OrgId = row.OrganizationIdFk,
                        FieldName = codeSepsis.FieldName,
                        FieldDataType = codeSepsis.FieldDataType,
                        FieldValue = fieldValue,
                        UserChannelSid = userUniqueIds,
                        From = AuthorEnums.Sepsis.ToString(),
                        Msg = (row.IsEms ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription()) + " Sepsis From is Changed",
                        RouteLink1 = RouteEnums.CodeSepsisForm.ToDescription(), // "/Home/Inhouse%20Codes/code-strok-form",
                        RouteLink2 = RouteEnums.EMSForms.ToDescription(), // RouteEnums.EMSForms.ToDescription(),
                    };

                    _communicationService.pushNotification(notification);
                    return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Updated Successfully" };
                }

            }
            else
            {

                if (codeSepsis.OrganizationIdFk > 0)
                {
                    string IsEMS = codeSepsis.IsEms ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription();
                    var DefaultServiceLineTeam = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == codeSepsis.OrganizationIdFk && x.CodeIdFk == UCLEnums.Sepsis.ToInt() && x.Type == IsEMS && x.IsActive.HasValue && x.IsActive.Value && !x.IsDeleted).Select(x => x.DefaultServiceLineTeam).FirstOrDefault();
                    if (DefaultServiceLineTeam != null && DefaultServiceLineTeam != "")
                    {
                        var DefaultServiceLineIds = DefaultServiceLineTeam.ToIntList();
                        //var ServiceLineTeam1Ids = codeSepsis.ServiceLineTeam1Ids.ToIntList();
                        //var ServiceLineTeam2Ids = codeSepsis.ServiceLineTeam2Ids.ToIntList();

                        codeSepsis.CreatedDate = DateTime.UtcNow;
                        codeSepsis.FamilyContactNumber = codeSepsis.FamilyContactNumber != null && codeSepsis.FamilyContactNumber != "" && codeSepsis.FamilyContactNumber != "(___) ___-____" ? codeSepsis.FamilyContactNumber : "";
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
                        this._dbContext.Log(Sepsis, ActivityLogTableEnums.CodeSepsis.ToString(), Sepsis.CodeSepsisId, ActivityLogActionEnums.Create.ToInt());

                        var codeService = new CodesServiceLinesMapping()
                        {
                            OrganizationIdFk = Sepsis.OrganizationIdFk,
                            CodeIdFk = UCLEnums.Sepsis.ToInt(),
                            DefaultServiceLineIdFk = DefaultServiceLineTeam,
                            ActiveCodeId = Sepsis.CodeSepsisId,
                            ActiveCodeName = UCLEnums.Sepsis.ToString()
                        };
                        this._codesServiceLinesMappingRepo.Insert(codeService);

                        //var UserChannelSid = (from us in this._userSchedulesRepo.Table
                        //                      join u in this._userRepo.Table on us.UserIdFk equals u.UserId
                        //                      where DefaultServiceLineIds.Contains(us.ServiceLineIdFk.Value) && us.ScheduleDateStart <= DateTime.UtcNow && us.ScheduleDateEnd >= DateTime.UtcNow && !us.IsDeleted && !u.IsDeleted
                        //                      select new { u.UserUniqueId, u.UserId }).ToList();
                        //var superAdmins = this._userRepo.Table.Where(x => x.IsInGroup && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId }).ToList();
                        //UserChannelSid.AddRange(superAdmins);
                        //var loggedUser = this._userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId }).FirstOrDefault();
                        //UserChannelSid.Add(loggedUser);
                        //List<CodeSepsisGroupMember> ACodeGroupMembers = new List<CodeSepsisGroupMember>();
                        //if (UserChannelSid != null && UserChannelSid.Count > 0)
                        //{
                        //    string uniqueName = DateTime.Now.ToString("yyyyMMddHHmmssffff") + ApplicationSettings.UserId.ToString();
                        //    string friendlyName = Sepsis.IsEms ? $"{UCLEnums.EMS.ToDescription()} {UCLEnums.Sepsis.ToString()} {Sepsis.CodeSepsisId}" : $"{UCLEnums.InhouseCode.ToDescription()} {UCLEnums.Sepsis.ToString()} {Sepsis.CodeSepsisId}";
                        //    var conversationChannelAttributes = JsonConvert.SerializeObject(new Dictionary<string, Object>()
                        //            {
                        //                {ChannelAttributeEnums.ChannelType.ToString(), ChannelTypeEnums.EMS.ToString()},
                        //                {ChannelAttributeEnums.CodeType.ToString(), ChannelTypeEnums.Sepsis.ToString()},
                        //                {ChannelAttributeEnums.SepsisId.ToString(), Sepsis.CodeSepsisId}
                        //            }, Formatting.Indented);
                        //    var channel = _communicationService.createConversationChannel(friendlyName, uniqueName, conversationChannelAttributes);
                        //    Sepsis.ChannelSid = channel.Sid;
                        //    this._codeSepsisRepo.Update(Sepsis);
                        //    UserChannelSid = UserChannelSid.Distinct().ToList();
                        //    foreach (var item in UserChannelSid)
                        //    {
                        //        try
                        //        {
                        //            var codeGroupMember = new CodeSepsisGroupMember()
                        //            {
                        //                UserIdFk = item.UserId,
                        //                SepsisCodeIdFk = Sepsis.CodeSepsisId,
                        //                //ActiveCodeName = UCLEnums.Sepsis.ToString(),
                        //                IsAcknowledge = false,
                        //                CreatedBy = ApplicationSettings.UserId,
                        //                CreatedDate = DateTime.UtcNow,
                        //                IsDeleted = false
                        //            };
                        //            ACodeGroupMembers.Add(codeGroupMember);
                        //            _communicationService.addNewUserToConversationChannel(channel.Sid, item.UserUniqueId);
                        //        }
                        //        catch (Exception ex)
                        //        {
                        //            //ElmahExtensions.RiseError(ex);
                        //        }
                        //    }
                        //    this._SepsisCodeGroupMembersRepo.Insert(ACodeGroupMembers);

                        //    var msg = new ConversationMessageVM();
                        //    msg.channelSid = channel.Sid;
                        //    msg.author = "System";
                        //    msg.attributes = "";
                        //    msg.body = $"<strong> {(codeSepsis.IsEms ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription())} {UCLEnums.Sepsis.ToString()} </strong></br></br>";
                        //    if (codeSepsis.PatientName != null && codeSepsis.PatientName != "")
                        //        msg.body += $"<strong>Patient Name: </strong> {codeSepsis.PatientName} </br>";
                        //    msg.body += $"<strong>Dob: </strong> {codeSepsis.Dob:MM-dd-yyyy} </br>";
                        //    msg.body += $"<strong>Last Well Known: </strong> {codeSepsis.LastKnownWell:MM-dd-yyyy hh:mm tt} </br>";
                        //    if (codeSepsis.ChiefComplant != null && codeSepsis.ChiefComplant != "")
                        //        msg.body += $"<strong>Chief Complaint: </strong> {codeSepsis.ChiefComplant} </br>";
                        //    if (codeSepsis.Hpi != null && codeSepsis.Hpi != "")
                        //        msg.body += $"<strong>Hpi: </strong> {codeSepsis.Hpi} </br>";

                        //    var sendMsg = _communicationService.sendPushNotification(msg);

                        //    var showAllAccessUsers = this._dbContext.LoadStoredProcedure("md_getUsersOfComponentAccess")
                        //                       .WithSqlParam("@componentName", "Show All EMS,Show All Active Codes,Show Graphs,Show EMS,Show Active Codes")
                        //                       .WithSqlParam("@orgId", Sepsis.OrganizationIdFk)
                        //                       .ExecuteStoredProc<RegisterCredentialVM>().Select(x => new { x.UserUniqueId, x.UserId }).Distinct().ToList();
                        //    UserChannelSid.AddRange(showAllAccessUsers);
                        //    var notification = new PushNotificationVM()
                        //    {
                        //        Id = Sepsis.CodeSepsisId,
                        //        OrgId = Sepsis.OrganizationIdFk,
                        //        UserChannelSid = UserChannelSid.Select(x => x.UserUniqueId).Distinct().ToList(),
                        //        From = AuthorEnums.Sepsis.ToString(),
                        //        Msg = (codeSepsis.IsEms ? UCLEnums.EMS.ToDescription() : "{UCLEnums.InhouseCode.ToDescription()}) + " Code Sepsis is update",
                        //        RouteLink3 = RouteEnums.ActiveEMS.ToDescription(),
                        //        RouteLink4 = RouteEnums.Dashboard.ToDescription(),
                        //        RouteLink5 = RouteEnums.InhouseCodeGrid.ToDescription()
                        //    };

                        //    _communicationService.pushNotification(notification);
                        //}
                        return GetSepsisDataById(Sepsis.CodeSepsisId);
                    }
                    return new BaseResponse() { Status = HttpStatusCode.NotAcceptable, Message = "There is no Service Line in this organization related to Code Sepsis" };
                }
                return new BaseResponse() { Status = HttpStatusCode.NotAcceptable, Message = "Organization is not selected" };
            }
        }

        public BaseResponse UpdateSepsisGroupMembers(CodeSepsisVM codeSepsis)
        {

            if (codeSepsis.DefaultServiceLineIds == null || codeSepsis.DefaultServiceLineIds == "")
            {
                string IsEMS = codeSepsis.IsEms ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription();
                codeSepsis.DefaultServiceLineIds = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == codeSepsis.OrganizationIdFk && x.CodeIdFk == UCLEnums.Sepsis.ToInt() && x.Type == IsEMS && x.IsActive.HasValue && x.IsActive.Value && !x.IsDeleted).Select(x => x.DefaultServiceLineTeam).FirstOrDefault();
            }

            if (codeSepsis.DefaultServiceLineIds != null && codeSepsis.DefaultServiceLineIds != "")
            {
                bool userNotFound = false;

                string msg = "";

                var DefaultServiceLineIds = codeSepsis.DefaultServiceLineIds.ToIntList();
                var ServiceLineTeam1Ids = codeSepsis.ServiceLineTeam1Ids.ToIntList();
                var ServiceLineTeam2Ids = codeSepsis.ServiceLineTeam2Ids.ToIntList();

                var UserChannelSid = (from us in this._userSchedulesRepo.Table
                                      join u in this._userRepo.Table on us.UserIdFk equals u.UserId
                                      where (DefaultServiceLineIds.Contains(us.ServiceLineIdFk.Value) || ServiceLineTeam1Ids.Contains(us.ServiceLineIdFk.Value) || ServiceLineTeam2Ids.Contains(us.ServiceLineIdFk.Value)) && us.ScheduleDateStart <= DateTime.UtcNow && us.ScheduleDateEnd >= DateTime.UtcNow && !us.IsDeleted && !u.IsDeleted
                                      select new { u.UserUniqueId, u.UserId, ServiceLineIdFk = us.ServiceLineIdFk.Value }).Distinct().ToList();

                var codeServiceMapping = this._codesServiceLinesMappingRepo.Table.Where(x => x.OrganizationIdFk == codeSepsis.OrganizationIdFk && x.CodeIdFk == UCLEnums.Sepsis.ToInt() && x.ActiveCodeId == codeSepsis.CodeSepsisId).ToList();
                if (codeServiceMapping.Count > 0)
                    this._codesServiceLinesMappingRepo.DeleteRange(codeServiceMapping);

                if (UserChannelSid.Count > 0)
                {
                    userNotFound = true;
                    var defaultExist = DefaultServiceLineIds.Select(d => new { ServiceLineId = d, IsExist = UserChannelSid.Select(x => x.ServiceLineIdFk).Contains(d) }).ToList();
                    if (defaultExist.Count > 0)
                    {
                        if (!defaultExist.Select(x => x.IsExist).All(x => x == true))
                        {
                            var notExisted = defaultExist.Where(x => !x.IsExist).Select(x => x.ServiceLineId).ToList();
                            DefaultServiceLineIds.RemoveAll(d => notExisted.Contains(d));
                            var services = this._serviceLineRepo.Table.Where(x => notExisted.Contains(x.ServiceLineId)).Select(x => new { x.ServiceLineId, x.ServiceName }).ToList();
                            foreach (var item in services)
                            {
                                msg += Environment.NewLine + "No OnCall user found in " + item.ServiceName;
                            }
                        }
                    }
                    else
                    {
                        var services = this._serviceLineRepo.Table.Where(x => DefaultServiceLineIds.Contains(x.ServiceLineId)).Select(x => new { x.ServiceLineId, x.ServiceName }).ToList();
                        foreach (var item in services)
                        {
                            msg += Environment.NewLine + "No OnCall user found in " + item.ServiceName;
                        }
                        DefaultServiceLineIds = new List<int>();
                    }

                    var serviceLineTeam1Exist = ServiceLineTeam1Ids.Select(d => new { ServiceLineId = d, IsExist = UserChannelSid.Select(x => x.ServiceLineIdFk).Contains(d) }).ToList();
                    if (serviceLineTeam1Exist.Count > 0)
                    {
                        if (!serviceLineTeam1Exist.Select(x => x.IsExist).All(x => x == true))
                        {
                            var notExisted = serviceLineTeam1Exist.Where(x => !x.IsExist).Select(x => x.ServiceLineId).ToList();
                            ServiceLineTeam1Ids.RemoveAll(d => notExisted.Contains(d));
                            var services = this._serviceLineRepo.Table.Where(x => notExisted.Contains(x.ServiceLineId)).Select(x => new { x.ServiceLineId, x.ServiceName }).ToList();
                            foreach (var item in services)
                            {
                                msg += Environment.NewLine + "No OnCall user found in " + item.ServiceName;
                            }
                        }
                    }
                    else
                    {
                        var services = this._serviceLineRepo.Table.Where(x => ServiceLineTeam1Ids.Contains(x.ServiceLineId)).Select(x => new { x.ServiceLineId, x.ServiceName }).ToList();
                        foreach (var item in services)
                        {
                            msg += Environment.NewLine + "No OnCall user found in " + item.ServiceName;
                        }
                        ServiceLineTeam1Ids = new List<int>();
                    }


                    var serviceLineTeam2Exist = ServiceLineTeam2Ids.Select(d => new { ServiceLineId = d, IsExist = UserChannelSid.Select(x => x.ServiceLineIdFk).Contains(d) }).ToList();
                    if (serviceLineTeam2Exist.Count > 0)
                    {
                        if (!serviceLineTeam2Exist.Select(x => x.IsExist).All(x => x == true))
                        {
                            var notExisted = serviceLineTeam2Exist.Where(x => !x.IsExist).Select(x => x.ServiceLineId).ToList();
                            ServiceLineTeam2Ids.RemoveAll(d => notExisted.Contains(d));
                            var services = this._serviceLineRepo.Table.Where(x => notExisted.Contains(x.ServiceLineId)).Select(x => new { x.ServiceLineId, x.ServiceName }).ToList();
                            foreach (var item in services)
                            {
                                msg += Environment.NewLine + "No OnCall user found in " + item.ServiceName;
                            }
                        }
                    }
                    else
                    {
                        var services = this._serviceLineRepo.Table.Where(x => ServiceLineTeam2Ids.Contains(x.ServiceLineId)).Select(x => new { x.ServiceLineId, x.ServiceName }).ToList();
                        foreach (var item in services)
                        {
                            msg += Environment.NewLine + "No OnCall user found in " + item.ServiceName;
                        }
                        ServiceLineTeam2Ids = new List<int>();
                    }



                    if (DefaultServiceLineIds.Count > 0 || ServiceLineTeam1Ids.Count > 0 || ServiceLineTeam2Ids.Count > 0)
                    {
                        var codeService = new CodesServiceLinesMapping()
                        {
                            OrganizationIdFk = codeSepsis.OrganizationIdFk,
                            CodeIdFk = UCLEnums.Sepsis.ToInt(),
                            DefaultServiceLineIdFk = DefaultServiceLineIds.Count > 0 ? string.Join(",", DefaultServiceLineIds) : null,
                            ServiceLineId1Fk = ServiceLineTeam1Ids.Count > 0 ? string.Join(",", ServiceLineTeam1Ids) : null,
                            ServiceLineId2Fk = ServiceLineTeam2Ids.Count > 0 ? string.Join(",", ServiceLineTeam2Ids) : null,
                            ActiveCodeId = codeSepsis.CodeSepsisId,
                            ActiveCodeName = UCLEnums.Sepsis.ToString()
                        };
                        this._codesServiceLinesMappingRepo.Insert(codeService);
                    }
                }
                else
                {
                    DefaultServiceLineIds = new List<int>();
                    ServiceLineTeam1Ids = new List<int>();
                    ServiceLineTeam2Ids = new List<int>();
                }


                var superAdmins = this._userRepo.Table.Where(x => x.IsInGroup && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId, ServiceLineIdFk = 0 }).ToList();
                UserChannelSid.AddRange(superAdmins);
                var loggedInUser = this._userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId, ServiceLineIdFk = 0 }).FirstOrDefault();
                UserChannelSid.Add(loggedInUser);

                if (codeSepsis.ChannelSid != null && codeSepsis.ChannelSid != "")
                {
                    var channelSid = codeSepsis.ChannelSid; //channel.Select(x => x.ChannelSid).FirstOrDefault();

                    var groupMembers = this._SepsisCodeGroupMembersRepo.Table.Where(x => x.SepsisCodeIdFk == codeSepsis.CodeSepsisId).ToList();
                    this._SepsisCodeGroupMembersRepo.DeleteRange(groupMembers);
                    //this._SepsisCodeGroupMembersRepo.DeleteRange(channel);
                    bool isDeleted = _communicationService.DeleteUserToConversationChannel(channelSid);
                    List<CodeSepsisGroupMember> ACodeGroupMembers = new List<CodeSepsisGroupMember>();
                    foreach (var item in UserChannelSid.Distinct())
                    {
                        try
                        {
                            var codeGroupMember = new CodeSepsisGroupMember()
                            {
                                UserIdFk = item.UserId,
                                SepsisCodeIdFk = codeSepsis.CodeSepsisId,
                                //ActiveCodeName = UCLEnums.Sepsis.ToString(),
                                IsAcknowledge = false,
                                CreatedBy = ApplicationSettings.UserId,
                                CreatedDate = DateTime.UtcNow,
                                IsDeleted = false
                            };
                            ACodeGroupMembers.Add(codeGroupMember);
                            _communicationService.addNewUserToConversationChannel(channelSid, item.UserUniqueId);
                        }
                        catch (Exception ex)
                        {
                            //ElmahExtensions.RiseError(ex);
                        }
                    }
                    this._SepsisCodeGroupMembersRepo.Insert(ACodeGroupMembers);

                }


                var notification = new PushNotificationVM()
                {
                    Id = codeSepsis.CodeSepsisId,
                    OrgId = codeSepsis.OrganizationIdFk,
                    UserChannelSid = UserChannelSid.Select(x => x.UserUniqueId).Distinct().ToList(),
                    From = AuthorEnums.Sepsis.ToString(),
                    Msg = (codeSepsis.IsEms ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription()) + " Sepsis From is Changed",
                    RouteLink1 = RouteEnums.CodeSepsisForm.ToDescription(), // "/Home/Inhouse%20Codes/code-strok-form",
                    RouteLink2 = RouteEnums.EMSForms.ToDescription(), // RouteEnums.EMSForms.ToDescription(),
                };

                _communicationService.pushNotification(notification);

                return new BaseResponse() { Status = HttpStatusCode.OK, Message = msg, Body = new { serviceLineUsersFound = userNotFound, DefaultServiceLineIds, ServiceLineTeam1Ids, ServiceLineTeam2Ids } };
            }
            return new BaseResponse() { Status = HttpStatusCode.NotModified, Message = "Default Service Lines Required" };
        }

        public BaseResponse CreateSepsisGroup(CodeSepsisVM codeSepsis)
        {

            var DefaultServiceLineIds = codeSepsis.DefaultServiceLineIds.ToIntList();
            bool usersFound = false;
            string errorMsg = "";

            var UserChannelSid = (from us in this._userSchedulesRepo.Table
                                  join u in this._userRepo.Table on us.UserIdFk equals u.UserId
                                  where DefaultServiceLineIds.Contains(us.ServiceLineIdFk.Value) && us.ScheduleDateStart <= DateTime.UtcNow && us.ScheduleDateEnd >= DateTime.UtcNow && !us.IsDeleted && !u.IsDeleted
                                  select new { u.UserUniqueId, u.UserId, ServiceLineIdFk = us.ServiceLineIdFk.Value }).Distinct().ToList();


            if (UserChannelSid.Count > 0)
            {
                usersFound = true;
                var defaultExist = DefaultServiceLineIds.Select(d => new { ServiceLineId = d, IsExist = UserChannelSid.Select(x => x.ServiceLineIdFk).Contains(d) }).ToList();
                if (defaultExist.Count > 0)
                {
                    if (!defaultExist.Select(x => x.IsExist).All(x => x == true))
                    {
                        var notExisted = defaultExist.Where(x => !x.IsExist).Select(x => x.ServiceLineId).ToList();
                        DefaultServiceLineIds.RemoveAll(d => notExisted.Contains(d));
                        var services = this._serviceLineRepo.Table.Where(x => notExisted.Contains(x.ServiceLineId)).Select(x => new { x.ServiceLineId, x.ServiceName }).ToList();
                        foreach (var item in services)
                        {
                            errorMsg += Environment.NewLine + "No OnCall user found in " + item.ServiceName;
                        }
                    }
                }
                else
                {
                    var services = this._serviceLineRepo.Table.Where(x => DefaultServiceLineIds.Contains(x.ServiceLineId)).Select(x => new { x.ServiceLineId, x.ServiceName }).ToList();
                    foreach (var item in services)
                    {
                        errorMsg += Environment.NewLine + "No OnCall user found in " + item.ServiceName;
                    }
                    DefaultServiceLineIds = new List<int>();
                }

                if (DefaultServiceLineIds.Count > 0)
                {
                    var codeService = new CodesServiceLinesMapping()
                    {
                        OrganizationIdFk = codeSepsis.OrganizationIdFk,
                        CodeIdFk = UCLEnums.Sepsis.ToInt(),
                        DefaultServiceLineIdFk = string.Join(",", DefaultServiceLineIds),
                        ActiveCodeId = codeSepsis.CodeSepsisId,
                        ActiveCodeName = UCLEnums.Sepsis.ToString()
                    };
                    this._codesServiceLinesMappingRepo.Insert(codeService);
                }
            }
            else
            {
                DefaultServiceLineIds = new List<int>();
            }

            var superAdmins = this._userRepo.Table.Where(x => x.IsInGroup && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId, ServiceLineIdFk = 0 }).ToList();
            UserChannelSid.AddRange(superAdmins);
            var loggedUser = this._userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId, ServiceLineIdFk = 0 }).FirstOrDefault();
            UserChannelSid.Add(loggedUser);
            var conversationChannelAttributes = JsonConvert.SerializeObject(new Dictionary<string, Object>()
                                    {
                                        {ChannelAttributeEnums.ChannelType.ToString(), ChannelTypeEnums.EMS.ToString()},
                                        {ChannelAttributeEnums.CodeType.ToString(), ChannelTypeEnums.Sepsis.ToString()},
                                        {ChannelAttributeEnums.SepsisId.ToString(), codeSepsis.CodeSepsisId}
                                    }, Formatting.Indented);
            List<CodeSepsisGroupMember> ACodeGroupMembers = new List<CodeSepsisGroupMember>();
            if (UserChannelSid != null && UserChannelSid.Count > 0)
            {
                string uniqueName = DateTime.Now.ToString("yyyyMMddHHmmssffff") + ApplicationSettings.UserId.ToString();
                string friendlyName = codeSepsis.IsEms ? $"{UCLEnums.EMS.ToDescription()} {UCLEnums.Sepsis.ToString()} {codeSepsis.CodeSepsisId}" : $"{UCLEnums.InhouseCode.ToDescription()} {UCLEnums.Sepsis.ToString()} {codeSepsis.CodeSepsisId}";
                var channel = _communicationService.createConversationChannel(friendlyName, uniqueName, conversationChannelAttributes);
                var Sepsis = this._codeSepsisRepo.Table.Where(x => x.CodeSepsisId == codeSepsis.CodeSepsisId && x.IsActive == true && !x.IsDeleted).FirstOrDefault();
                if (Sepsis != null)
                {
                    Sepsis.ChannelSid = channel.Sid;
                    this._codeSepsisRepo.Update(Sepsis);
                }
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
                        _communicationService.addNewUserToConversationChannel(channel.Sid, item.UserUniqueId);
                    }
                    catch (Exception ex)
                    {

                    }
                }
                this._SepsisCodeGroupMembersRepo.Insert(ACodeGroupMembers);
                var msg = new ConversationMessageVM();
                msg.channelSid = channel.Sid;
                msg.author = "System";
                msg.attributes = "";
                msg.body = $"<strong> {(codeSepsis.IsEms ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription())} {UCLEnums.Sepsis.ToString()} </strong></br></br>";
                if (codeSepsis.PatientName != null && codeSepsis.PatientName != "")
                    msg.body += $"<strong>Patient Name: </strong> {codeSepsis.PatientName} </br>";
                if (codeSepsis.Dob != null)
                    msg.body += $"<strong>Dob: </strong> {codeSepsis.Dob:MM-dd-yyyy} </br>";
                if (codeSepsis.LastKnownWell != null)
                    msg.body += $"<strong>Last Well Known: </strong> {codeSepsis.LastKnownWell:MM-dd-yyyy hh:mm tt} </br>";
                if (codeSepsis.ChiefComplant != null && codeSepsis.ChiefComplant != "")
                    msg.body += $"<strong>Chief Complaint: </strong> {codeSepsis.ChiefComplant} </br>";
                if (codeSepsis.Hpi != null && codeSepsis.Hpi != "")
                    msg.body += $"<strong>Hpi: </strong> {codeSepsis.Hpi} </br>";

                var sendMsg = _communicationService.sendPushNotification(msg);

                var showAllAccessUsers = this._dbContext.LoadStoredProcedure("md_getUsersOfComponentAccess")
                                    .WithSqlParam("@componentName", "Show All EMS,Show All Active Codes,Show Graphs,Show EMS,Show Active Codes")
                                    .WithSqlParam("@orgId", Sepsis.OrganizationIdFk)
                                    .ExecuteStoredProc<RegisterCredentialVM>().Select(x => new { x.UserUniqueId, x.UserId, ServiceLineIdFk = 0 }).Distinct().ToList();
                UserChannelSid.AddRange(showAllAccessUsers);
                var notification = new PushNotificationVM()
                {
                    Id = Sepsis.CodeSepsisId,
                    OrgId = Sepsis.OrganizationIdFk,
                    UserChannelSid = UserChannelSid.Select(x => x.UserUniqueId).Distinct().ToList(),
                    From = AuthorEnums.Sepsis.ToString(),
                    Msg = (codeSepsis.IsEms ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription()) + " Sepsis is update",
                    RouteLink3 = RouteEnums.ActiveEMS.ToDescription(), // RouteEnums.ActiveEMS.ToDescription(),
                    RouteLink4 = RouteEnums.Dashboard.ToDescription(),
                    RouteLink5 = RouteEnums.InhouseCodeGrid.ToDescription(), // RouteEnums.InhouseCodeGrid.ToDescription()
                };

                _communicationService.pushNotification(notification);
            }

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = errorMsg, Body = new { serviceLineUsersFound = usersFound, DefaultServiceLineIds, ServiceLineTeam1Ids = new List<int>(), ServiceLineTeam2Ids = new List<int>() } };
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
            this._dbContext.Log(new { }, ActivityLogTableEnums.CodeSepsis.ToString(), SepsisId, ActivityLogActionEnums.Delete.ToInt());
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

            var rowsEffected = this._dbContext.Database.ExecuteSqlRaw(sql, parameters);
            if (rowsEffected > 0)
            {
                this._dbContext.Log(new { }, ActivityLogTableEnums.CodeSepsis.ToString(), SepsisId, status == false ? ActivityLogActionEnums.Inactive.ToInt() : ActivityLogActionEnums.Active.ToInt());

                var userIds = this._dbContext.LoadStoredProcedure("md_getAllActiveCodesGroupUserIds")
                                                .WithSqlParam("@codeName", UCLEnums.Sepsis.ToString())
                                                .WithSqlParam("@codeId", SepsisId)
                                                .ExecuteStoredProc<CodeStrokeVM>();

                var notification = new PushNotificationVM()
                {
                    Id = SepsisId,
                    OrgId = userIds.Select(x => x.OrganizationIdFk).FirstOrDefault(),
                    Type = "ChannelStatusChanged",
                    ChannelIsActive = status,
                    ChannelSid = userIds.Select(x => x.ChannelSid).FirstOrDefault(),
                    UserChannelSid = userIds.Select(x => x.UserUniqueId).Distinct().ToList(),
                    From = UCLEnums.Sepsis.ToString(),
                    Msg = UCLEnums.Sepsis.ToString() + " is " + (status ? "Activated" : "Inactivated"),
                    RouteLink3 = RouteEnums.ActiveEMS.ToDescription(),
                    RouteLink4 = RouteEnums.Dashboard.ToDescription(),
                    RouteLink5 = RouteEnums.InhouseCodeGrid.ToDescription()
                };

                _communicationService.pushNotification(notification);
            }

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Deleted" };
        }


        #endregion

        #region Code STEMI

        public BaseResponse GetAllSTEMICode(ActiveCodeVM activeCode)
        {
            var gridColumns = GetInhouseCodeTableFeilds(activeCode.OrganizationIdFk, UCLEnums.Stemi.ToString());
            dynamic Fields = gridColumns.Body;
            if (Fields != null && Fields.FieldName != null)
            {
                string FieldNames = Convert.ToString(Fields.FieldName);
                var objList = this._dbContext.LoadStoredProcedure("md_getAllActiveCodesOrEMS_Dynamic")
                                .WithSqlParam("@status", activeCode.Status)
                                .WithSqlParam("@colName", FieldNames)
                                .WithSqlParam("@codeName", UCLEnums.Stemi.ToString())
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
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Returned", Body = new { totalRecords, objList, fields = gridColumns.Body } };

            }
            return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Fields Name Not Found" };
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
                string Type = STEMIDataVM.IsEms.HasValue && STEMIDataVM.IsEms.Value ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription();
                var serviceIds = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == STEMIData.OrganizationIdFk && x.CodeIdFk == UCLEnums.Stemi.ToInt() && x.Type == Type && x.IsActive.HasValue && x.IsActive.Value && !x.IsDeleted).Select(x => new { x.DefaultServiceLineTeam, x.ServiceLineTeam1, x.ServiceLineTeam2 }).FirstOrDefault();

                var serviceLineIds = (from x in this._codesServiceLinesMappingRepo.Table
                                      where x.OrganizationIdFk == STEMIData.OrganizationIdFk && x.CodeIdFk == UCLEnums.Stemi.ToInt()
                                      && x.ActiveCodeId == STEMIData.CodeStemiid && x.ActiveCodeName == UCLEnums.Stemi.ToString()
                                      select new { x.DefaultServiceLineIdFk, x.ServiceLineId1Fk, x.ServiceLineId2Fk }).FirstOrDefault();

                if (serviceIds != null)
                {
                    List<int> defaultIds = new();
                    List<int> team1 = new();
                    List<int> team2 = new();
                    if (serviceLineIds != null)
                    {
                        defaultIds = serviceIds.DefaultServiceLineTeam.ToIntList().Where(x => serviceLineIds.DefaultServiceLineIdFk.ToIntList().Contains(x)).Distinct().ToList();
                        team1 = serviceIds.ServiceLineTeam1.ToIntList().Where(x => serviceLineIds.ServiceLineId1Fk.ToIntList().Contains(x)).Distinct().ToList();
                        team2 = serviceIds.ServiceLineTeam2.ToIntList().Where(x => serviceLineIds.ServiceLineId2Fk.ToIntList().Contains(x)).Distinct().ToList();
                    }
                    STEMIDataVM.DefaultServiceLineTeam = this._serviceLineRepo.Table.Where(x => serviceIds.DefaultServiceLineTeam.ToIntList().Distinct().Contains(x.ServiceLineId) && !x.IsDeleted).Select(x => new ServiceLineVM() { ServiceLineId = x.ServiceLineId, ServiceName = x.ServiceName, IsSelected = defaultIds.Contains(x.ServiceLineId) }).ToList();
                    STEMIDataVM.ServiceLineTeam1 = this._serviceLineRepo.Table.Where(x => serviceIds.ServiceLineTeam1.ToIntList().Distinct().Contains(x.ServiceLineId) && !x.IsDeleted).Select(x => new ServiceLineVM() { ServiceLineId = x.ServiceLineId, ServiceName = x.ServiceName, IsSelected = team1.Contains(x.ServiceLineId) }).ToList();
                    STEMIDataVM.ServiceLineTeam2 = this._serviceLineRepo.Table.Where(x => serviceIds.ServiceLineTeam2.ToIntList().Distinct().Contains(x.ServiceLineId) && !x.IsDeleted).Select(x => new ServiceLineVM() { ServiceLineId = x.ServiceLineId, ServiceName = x.ServiceName, IsSelected = team2.Contains(x.ServiceLineId) }).ToList();
                }

                STEMIDataVM.LastKnownWellStr = STEMIDataVM.LastKnownWell?.ToTimezoneFromUtc("Eastern Standard Time").ToString("yyyy-MM-dd hh:mm:ss tt");
                STEMIDataVM.DobStr = STEMIDataVM.Dob?.ToTimezoneFromUtc("Eastern Standard Time").ToString("yyyy-MM-dd hh:mm:ss tt");
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
                codeSTEMI.LastKnownWell = DateTime.Parse(codeSTEMI.LastKnownWellStr).ToUniversalTimeZone(); ;
            }
            if (codeSTEMI != null && !string.IsNullOrEmpty(codeSTEMI.DobStr) && !string.IsNullOrWhiteSpace(codeSTEMI.DobStr))
            {
                codeSTEMI.Dob = DateTime.Parse(codeSTEMI.DobStr).ToUniversalTimeZone(); ;
            }
            if (codeSTEMI.CodeStemiid > 0)
            {
                var row = new CodeStemi();
                object fieldValue = new();
                if (codeSTEMI.FieldDataType != "file")
                {
                    var fieldName = string.Empty;
                    if (codeSTEMI.FieldName == "HPI" || codeSTEMI.FieldName == "DOB")
                    {
                        fieldName = codeSTEMI.FieldName.ToCapitalize();
                    }
                    else
                    {
                        fieldName = codeSTEMI.FieldName;
                    }

                    fieldValue = codeSTEMI.GetPropertyValueByName(fieldName);

                    row = this._dbContext.LoadStoredProcedure("md_UpdateCodes")
                                             .WithSqlParam("codeName", UCLEnums.Stemi.ToString())
                                             .WithSqlParam("fieldName", codeSTEMI.FieldName)
                                             .WithSqlParam("fieldValue", fieldValue)
                                             .WithSqlParam("codeId", codeSTEMI.CodeStemiid)
                                             .WithSqlParam("modifiedBy", ApplicationSettings.UserId)
                                             .ExecuteStoredProc<CodeStemi>().FirstOrDefault();
                    this._dbContext.Log(row.getChangedPropertyObject(fieldName), ActivityLogTableEnums.CodeSTEMIs.ToString(), row.CodeStemiid, ActivityLogActionEnums.Update.ToInt());

                    var userIds = this._STEMICodeGroupMembersRepo.Table.Where(x => x.StemicodeIdFk == row.CodeStemiid).Select(x => x.UserIdFk).ToList();
                    var userUniqueIds = this._userRepo.Table.Where(x => userIds.Contains(x.UserId)).Select(x => x.UserUniqueId).Distinct().ToList();
                    var superAdmins = this._userRepo.Table.Where(x => x.IsInGroup && !x.IsDeleted).Select(x => x.UserUniqueId).ToList();
                    userUniqueIds.AddRange(superAdmins);
                    var loggedUser = this._userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && !x.IsDeleted).Select(x => x.UserUniqueId).FirstOrDefault();
                    userUniqueIds.Add(loggedUser);
                    if (codeSTEMI.FieldDataType == "date")
                    {
                        fieldValue = codeSTEMI.GetPropertyValueByName(fieldName + "Str");
                    }
                    var notification = new PushNotificationVM()
                    {
                        Id = row.CodeStemiid,
                        OrgId = row.OrganizationIdFk,
                        FieldName = codeSTEMI.FieldName,
                        FieldDataType = codeSTEMI.FieldDataType,
                        FieldValue = fieldValue,
                        UserChannelSid = userUniqueIds.Distinct().ToList(),
                        From = AuthorEnums.Stemi.ToString(),
                        Msg = (row.IsEms != null && row.IsEms.Value ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription()) + " STEMI From is Changed",
                        RouteLink1 = RouteEnums.CodeStemiForm.ToDescription(), // "/Home/Inhouse%20Codes/code-strok-form",
                        RouteLink2 = RouteEnums.EMSForms.ToDescription(), // RouteEnums.EMSForms.ToDescription(),
                    };

                    _communicationService.pushNotification(notification);
                    return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Updated Successfully" };
                }
                else
                {

                    row = this._codeSTEMIRepo.Table.Where(x => x.CodeStemiid == codeSTEMI.CodeStemiid && !x.IsDeleted).FirstOrDefault();

                    row.ModifiedBy = codeSTEMI.ModifiedBy;
                    row.ModifiedDate = DateTime.UtcNow;
                    row.IsDeleted = false;

                    if (codeSTEMI.Attachment != null && codeSTEMI.Attachment.Count > 0)
                    {
                        var RootPath = this._RootPath + "/Organizations"; //this._RootPath + "/Organizations";
                        string FileRoot = null;
                        List<string> Attachments = new();
                        FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == row.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                        FileRoot = Path.Combine(RootPath, FileRoot);
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, UCLEnums.Stemi.ToString());
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
                        FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == row.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                        FileRoot = Path.Combine(RootPath, FileRoot);
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, UCLEnums.Stemi.ToString());
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

                        FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == row.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                        FileRoot = Path.Combine(RootPath, FileRoot);
                        if (!Directory.Exists(FileRoot))
                        {
                            Directory.CreateDirectory(FileRoot);
                        }
                        FileRoot = Path.Combine(FileRoot, UCLEnums.Stemi.ToString());
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
                        string path = this._RootPath + codeSTEMI.AttachmentsFolderRoot;
                        if (Directory.Exists(path))
                        {
                            DirectoryInfo AttachFiles = new DirectoryInfo(path);
                            codeSTEMI.AttachmentsPath = new List<string>();
                            foreach (var item in AttachFiles.GetFiles())
                            {
                                codeSTEMI.AttachmentsPath.Add(codeSTEMI.AttachmentsFolderRoot + "/" + item.Name);
                            }
                        }
                    }
                    if (codeSTEMI.VideoFolderRoot != null)
                    {
                        row.Video = codeSTEMI.VideoFolderRoot;
                        var path = this._RootPath + codeSTEMI.VideoFolderRoot;
                        if (Directory.Exists(path))
                        {
                            DirectoryInfo VideoFiles = new DirectoryInfo(path);
                            codeSTEMI.VideosPath = new List<string>();
                            foreach (var item in VideoFiles.GetFiles())
                            {
                                codeSTEMI.VideosPath.Add(codeSTEMI.VideoFolderRoot + "/" + item.Name);
                            }
                        }
                    }
                    if (codeSTEMI.AudioFolderRoot != null)
                    {
                        row.Audio = codeSTEMI.AudioFolderRoot;
                        string path = this._RootPath + codeSTEMI.AudioFolderRoot;
                        if (Directory.Exists(path))
                        {
                            DirectoryInfo AudioFiles = new DirectoryInfo(path);
                            codeSTEMI.AudiosPath = new List<string>();
                            foreach (var item in AudioFiles.GetFiles())
                            {
                                codeSTEMI.AudiosPath.Add(codeSTEMI.AudioFolderRoot + "/" + item.Name);
                            }
                        }
                    }

                    this._codeSTEMIRepo.Update(row);
                    this._dbContext.Log(row, ActivityLogTableEnums.CodeSTEMIs.ToString(), row.CodeStemiid, ActivityLogActionEnums.FileUpload.ToInt());

                    fieldValue = new { videosPath = codeSTEMI.VideosPath, audiosPath = codeSTEMI.AudiosPath, attachmentsPath = codeSTEMI.AttachmentsPath };
                    var userIds = this._STEMICodeGroupMembersRepo.Table.Where(x => x.StemicodeIdFk == row.CodeStemiid).Select(x => x.UserIdFk).ToList();
                    var userUniqueIds = this._userRepo.Table.Where(x => userIds.Contains(x.UserId)).Select(x => x.UserUniqueId).Distinct().ToList();
                    var superAdmins = this._userRepo.Table.Where(x => x.IsInGroup && !x.IsDeleted).Select(x => x.UserUniqueId).ToList();
                    userUniqueIds.AddRange(superAdmins);
                    var loggedUser = this._userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && !x.IsDeleted).Select(x => x.UserUniqueId).FirstOrDefault();
                    userUniqueIds.Add(loggedUser);

                    var notification = new PushNotificationVM()
                    {
                        Id = row.CodeStemiid,
                        OrgId = row.OrganizationIdFk,
                        FieldName = codeSTEMI.FieldName,
                        FieldDataType = codeSTEMI.FieldDataType,
                        FieldValue = fieldValue,
                        UserChannelSid = userUniqueIds,
                        From = AuthorEnums.Stemi.ToString(),
                        Msg = (row.IsEms != null && row.IsEms.Value ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription()) + " STEMI From is Changed",
                        RouteLink1 = RouteEnums.CodeStemiForm.ToDescription(), // "/Home/Inhouse%20Codes/code-strok-form",
                        RouteLink2 = RouteEnums.EMSForms.ToDescription(), // RouteEnums.EMSForms.ToDescription(),
                    };

                    _communicationService.pushNotification(notification);
                    return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Updated Successfully" };
                }

            }
            else
            {
                if (codeSTEMI.OrganizationIdFk > 0)
                {
                    string IsEMS = codeSTEMI.IsEms.HasValue && codeSTEMI.IsEms.Value ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription();
                    var DefaultServiceLineTeam = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == codeSTEMI.OrganizationIdFk && x.CodeIdFk == UCLEnums.Stemi.ToInt() && x.Type == IsEMS && x.IsActive.HasValue && x.IsActive.Value && !x.IsDeleted).Select(x => x.DefaultServiceLineTeam).FirstOrDefault();
                    if (DefaultServiceLineTeam != null && DefaultServiceLineTeam != "")
                    {
                        var DefaultServiceLineIds = DefaultServiceLineTeam.ToIntList();


                        codeSTEMI.CreatedDate = DateTime.UtcNow;
                        codeSTEMI.FamilyContactNumber = codeSTEMI.FamilyContactNumber != null && codeSTEMI.FamilyContactNumber != "" && codeSTEMI.FamilyContactNumber != "(___) ___-____" ? codeSTEMI.FamilyContactNumber : "";
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
                            FileRoot = Path.Combine(FileRoot, UCLEnums.Stemi.ToString());
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
                            FileRoot = Path.Combine(FileRoot, UCLEnums.Stemi.ToString());
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
                            FileRoot = Path.Combine(FileRoot, UCLEnums.Stemi.ToString());
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
                        this._dbContext.Log(STEMI, ActivityLogTableEnums.CodeSTEMIs.ToString(), STEMI.CodeStemiid, ActivityLogActionEnums.Create.ToInt());
                        var codeService = new CodesServiceLinesMapping()
                        {
                            OrganizationIdFk = STEMI.OrganizationIdFk,
                            CodeIdFk = UCLEnums.Stemi.ToInt(),
                            DefaultServiceLineIdFk = DefaultServiceLineTeam,
                            ActiveCodeId = STEMI.CodeStemiid,
                            ActiveCodeName = UCLEnums.Stemi.ToString()
                        };
                        this._codesServiceLinesMappingRepo.Insert(codeService);

                        return GetSTEMIDataById(STEMI.CodeStemiid);
                    }
                    return new BaseResponse() { Status = HttpStatusCode.NotAcceptable, Message = "There is no Service Line in this organization related to Code STEMI" };
                }
                return new BaseResponse() { Status = HttpStatusCode.NotAcceptable, Message = "Organization is not selected" };
            }
        }

        public BaseResponse CreateSTEMIGroup(CodeSTEMIVM codeSTEMI)
        {

            var DefaultServiceLineIds = codeSTEMI.DefaultServiceLineIds.ToIntList();
            bool usersFound = false;
            string errorMsg = "";

            var UserChannelSid = (from us in this._userSchedulesRepo.Table
                                  join u in this._userRepo.Table on us.UserIdFk equals u.UserId
                                  where DefaultServiceLineIds.Contains(us.ServiceLineIdFk.Value) && us.ScheduleDateStart <= DateTime.UtcNow && us.ScheduleDateEnd >= DateTime.UtcNow && !us.IsDeleted && !u.IsDeleted
                                  select new { u.UserUniqueId, u.UserId, ServiceLineIdFk = us.ServiceLineIdFk.Value }).Distinct().ToList();


            if (UserChannelSid.Count > 0)
            {
                usersFound = true;
                var defaultExist = DefaultServiceLineIds.Select(d => new { ServiceLineId = d, IsExist = UserChannelSid.Select(x => x.ServiceLineIdFk).Contains(d) }).ToList();
                if (defaultExist.Count > 0)
                {
                    if (!defaultExist.Select(x => x.IsExist).All(x => x == true))
                    {
                        var notExisted = defaultExist.Where(x => !x.IsExist).Select(x => x.ServiceLineId).ToList();
                        DefaultServiceLineIds.RemoveAll(d => notExisted.Contains(d));
                        var services = this._serviceLineRepo.Table.Where(x => notExisted.Contains(x.ServiceLineId)).Select(x => new { x.ServiceLineId, x.ServiceName }).ToList();
                        foreach (var item in services)
                        {
                            errorMsg += Environment.NewLine + "No OnCall user found in " + item.ServiceName;
                        }
                    }
                }
                else
                {
                    var services = this._serviceLineRepo.Table.Where(x => DefaultServiceLineIds.Contains(x.ServiceLineId)).Select(x => new { x.ServiceLineId, x.ServiceName }).ToList();
                    foreach (var item in services)
                    {
                        errorMsg += Environment.NewLine + "No OnCall user found in " + item.ServiceName;
                    }
                    DefaultServiceLineIds = new List<int>();
                }

                if (DefaultServiceLineIds.Count > 0)
                {
                    var codeService = new CodesServiceLinesMapping()
                    {
                        OrganizationIdFk = codeSTEMI.OrganizationIdFk,
                        CodeIdFk = UCLEnums.Stemi.ToInt(),
                        DefaultServiceLineIdFk = string.Join(",", DefaultServiceLineIds),
                        ActiveCodeId = codeSTEMI.CodeStemiid,
                        ActiveCodeName = UCLEnums.Stemi.ToString()
                    };
                    this._codesServiceLinesMappingRepo.Insert(codeService);
                }
            }
            else
            {
                DefaultServiceLineIds = new List<int>();
            }

            var superAdmins = this._userRepo.Table.Where(x => x.IsInGroup && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId, ServiceLineIdFk = 0 }).ToList();
            UserChannelSid.AddRange(superAdmins);
            var loggedUser = this._userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId, ServiceLineIdFk = 0 }).FirstOrDefault();
            UserChannelSid.Add(loggedUser);
            var conversationChannelAttributes = JsonConvert.SerializeObject(new Dictionary<string, Object>()
                                    {
                                        {ChannelAttributeEnums.ChannelType.ToString(), ChannelTypeEnums.EMS.ToString()},
                                        {ChannelAttributeEnums.CodeType.ToString(), ChannelTypeEnums.Stemi.ToString()},
                                        {ChannelAttributeEnums.StemiId.ToString(), codeSTEMI.CodeStemiid}
                                    }, Formatting.Indented);
            List<CodeStemigroupMember> ACodeGroupMembers = new List<CodeStemigroupMember>();
            if (UserChannelSid != null && UserChannelSid.Count > 0)
            {
                string uniqueName = DateTime.Now.ToString("yyyyMMddHHmmssffff") + ApplicationSettings.UserId.ToString();
                string friendlyName = codeSTEMI.IsEms.HasValue && codeSTEMI.IsEms.Value ? $"{UCLEnums.EMS.ToDescription()} {UCLEnums.Stemi.ToString()} {codeSTEMI.CodeStemiid}" : $"{UCLEnums.InhouseCode.ToDescription()} {UCLEnums.Stemi.ToString()} {codeSTEMI.CodeStemiid}";
                var channel = _communicationService.createConversationChannel(friendlyName, uniqueName, conversationChannelAttributes);
                var STEMI = this._codeSTEMIRepo.Table.Where(x => x.CodeStemiid == codeSTEMI.CodeStemiid && x.IsActive == true && !x.IsDeleted).FirstOrDefault();
                if (STEMI != null)
                {
                    STEMI.ChannelSid = channel.Sid;
                    this._codeSTEMIRepo.Update(STEMI);
                }
                UserChannelSid = UserChannelSid.Distinct().ToList();
                foreach (var item in UserChannelSid)
                {
                    try
                    {
                        var codeGroupMember = new CodeStemigroupMember()
                        {
                            UserIdFk = item.UserId,
                            StemicodeIdFk = STEMI.CodeStemiid,
                            //ActiveCodeName = UCLEnums.Stemi.ToString(),
                            IsAcknowledge = false,
                            CreatedBy = ApplicationSettings.UserId,
                            CreatedDate = DateTime.UtcNow,
                            IsDeleted = false
                        };
                        ACodeGroupMembers.Add(codeGroupMember);
                        _communicationService.addNewUserToConversationChannel(channel.Sid, item.UserUniqueId);
                    }
                    catch (Exception ex)
                    {

                    }
                }
                this._STEMICodeGroupMembersRepo.Insert(ACodeGroupMembers);
                var msg = new ConversationMessageVM();
                msg.channelSid = channel.Sid;
                msg.author = "System";
                msg.attributes = "";
                msg.body = $"<strong> {(codeSTEMI.IsEms.HasValue && codeSTEMI.IsEms.Value ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription())} {UCLEnums.Stemi.ToString()} </strong></br></br>";
                if (codeSTEMI.PatientName != null && codeSTEMI.PatientName != "")
                    msg.body += $"<strong>Patient Name: </strong> {codeSTEMI.PatientName} </br>";
                if (codeSTEMI.Dob != null)
                    msg.body += $"<strong>Dob: </strong> {codeSTEMI.Dob:MM-dd-yyyy} </br>";
                if (codeSTEMI.LastKnownWell != null)
                    msg.body += $"<strong>Last Well Known: </strong> {codeSTEMI.LastKnownWell:MM-dd-yyyy hh:mm tt} </br>";
                if (codeSTEMI.ChiefComplant != null && codeSTEMI.ChiefComplant != "")
                    msg.body += $"<strong>Chief Complaint: </strong> {codeSTEMI.ChiefComplant} </br>";
                if (codeSTEMI.Hpi != null && codeSTEMI.Hpi != "")
                    msg.body += $"<strong>Hpi: </strong> {codeSTEMI.Hpi} </br>";

                var sendMsg = _communicationService.sendPushNotification(msg);

                var showAllAccessUsers = this._dbContext.LoadStoredProcedure("md_getUsersOfComponentAccess")
                                    .WithSqlParam("@componentName", "Show All EMS,Show All Active Codes,Show Graphs,Show EMS,Show Active Codes")
                                    .WithSqlParam("@orgId", STEMI.OrganizationIdFk)
                                    .ExecuteStoredProc<RegisterCredentialVM>().Select(x => new { x.UserUniqueId, x.UserId, ServiceLineIdFk = 0 }).Distinct().ToList();
                UserChannelSid.AddRange(showAllAccessUsers);
                var notification = new PushNotificationVM()
                {
                    Id = STEMI.CodeStemiid,
                    OrgId = STEMI.OrganizationIdFk,
                    UserChannelSid = UserChannelSid.Select(x => x.UserUniqueId).Distinct().ToList(),
                    From = AuthorEnums.Stemi.ToString(),
                    Msg = (codeSTEMI.IsEms.HasValue && codeSTEMI.IsEms.Value ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription()) + " STEMI is update",
                    RouteLink3 = RouteEnums.ActiveEMS.ToDescription(), // RouteEnums.ActiveEMS.ToDescription(),
                    RouteLink4 = RouteEnums.Dashboard.ToDescription(),
                    RouteLink5 = RouteEnums.InhouseCodeGrid.ToDescription(), // RouteEnums.InhouseCodeGrid.ToDescription()
                };

                _communicationService.pushNotification(notification);
            }

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = errorMsg, Body = new { serviceLineUsersFound = usersFound, DefaultServiceLineIds, ServiceLineTeam1Ids = new List<int>(), ServiceLineTeam2Ids = new List<int>() } };
        }

        public BaseResponse UpdateSTEMIGroupMembers(CodeSTEMIVM codeSTEMI)
        {

            if (codeSTEMI.DefaultServiceLineIds == null || codeSTEMI.DefaultServiceLineIds == "")
            {
                string IsEMS = codeSTEMI.IsEms.HasValue && codeSTEMI.IsEms.Value ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription();
                codeSTEMI.DefaultServiceLineIds = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == codeSTEMI.OrganizationIdFk && x.CodeIdFk == UCLEnums.Stemi.ToInt() && x.Type == IsEMS && x.IsActive.HasValue && x.IsActive.Value && !x.IsDeleted).Select(x => x.DefaultServiceLineTeam).FirstOrDefault();
            }

            if (codeSTEMI.DefaultServiceLineIds != null && codeSTEMI.DefaultServiceLineIds != "")
            {
                bool userNotFound = false;

                string msg = "";

                var DefaultServiceLineIds = codeSTEMI.DefaultServiceLineIds.ToIntList();
                var ServiceLineTeam1Ids = codeSTEMI.ServiceLineTeam1Ids.ToIntList();
                var ServiceLineTeam2Ids = codeSTEMI.ServiceLineTeam2Ids.ToIntList();

                var UserChannelSid = (from us in this._userSchedulesRepo.Table
                                      join u in this._userRepo.Table on us.UserIdFk equals u.UserId
                                      where (DefaultServiceLineIds.Contains(us.ServiceLineIdFk.Value) || ServiceLineTeam1Ids.Contains(us.ServiceLineIdFk.Value) || ServiceLineTeam2Ids.Contains(us.ServiceLineIdFk.Value)) && us.ScheduleDateStart <= DateTime.UtcNow && us.ScheduleDateEnd >= DateTime.UtcNow && !us.IsDeleted && !u.IsDeleted
                                      select new { u.UserUniqueId, u.UserId, ServiceLineIdFk = us.ServiceLineIdFk.Value }).Distinct().ToList();

                var codeServiceMapping = this._codesServiceLinesMappingRepo.Table.Where(x => x.OrganizationIdFk == codeSTEMI.OrganizationIdFk && x.CodeIdFk == UCLEnums.Stemi.ToInt() && x.ActiveCodeId == codeSTEMI.CodeStemiid).ToList();
                if (codeServiceMapping.Count > 0)
                    this._codesServiceLinesMappingRepo.DeleteRange(codeServiceMapping);

                if (UserChannelSid.Count > 0)
                {
                    userNotFound = true;
                    var defaultExist = DefaultServiceLineIds.Select(d => new { ServiceLineId = d, IsExist = UserChannelSid.Select(x => x.ServiceLineIdFk).Contains(d) }).ToList();
                    if (defaultExist.Count > 0)
                    {
                        if (!defaultExist.Select(x => x.IsExist).All(x => x == true))
                        {
                            var notExisted = defaultExist.Where(x => !x.IsExist).Select(x => x.ServiceLineId).ToList();
                            DefaultServiceLineIds.RemoveAll(d => notExisted.Contains(d));
                            var services = this._serviceLineRepo.Table.Where(x => notExisted.Contains(x.ServiceLineId)).Select(x => new { x.ServiceLineId, x.ServiceName }).ToList();
                            foreach (var item in services)
                            {
                                msg += Environment.NewLine + "No OnCall user found in " + item.ServiceName;
                            }
                        }
                    }
                    else
                    {
                        var services = this._serviceLineRepo.Table.Where(x => DefaultServiceLineIds.Contains(x.ServiceLineId)).Select(x => new { x.ServiceLineId, x.ServiceName }).ToList();
                        foreach (var item in services)
                        {
                            msg += Environment.NewLine + "No OnCall user found in " + item.ServiceName;
                        }
                        DefaultServiceLineIds = new List<int>();
                    }

                    var serviceLineTeam1Exist = ServiceLineTeam1Ids.Select(d => new { ServiceLineId = d, IsExist = UserChannelSid.Select(x => x.ServiceLineIdFk).Contains(d) }).ToList();
                    if (serviceLineTeam1Exist.Count > 0)
                    {
                        if (!serviceLineTeam1Exist.Select(x => x.IsExist).All(x => x == true))
                        {
                            var notExisted = serviceLineTeam1Exist.Where(x => !x.IsExist).Select(x => x.ServiceLineId).ToList();
                            ServiceLineTeam1Ids.RemoveAll(d => notExisted.Contains(d));
                            var services = this._serviceLineRepo.Table.Where(x => notExisted.Contains(x.ServiceLineId)).Select(x => new { x.ServiceLineId, x.ServiceName }).ToList();
                            foreach (var item in services)
                            {
                                msg += Environment.NewLine + "No OnCall user found in " + item.ServiceName;
                            }
                        }
                    }
                    else
                    {
                        var services = this._serviceLineRepo.Table.Where(x => ServiceLineTeam1Ids.Contains(x.ServiceLineId)).Select(x => new { x.ServiceLineId, x.ServiceName }).ToList();
                        foreach (var item in services)
                        {
                            msg += Environment.NewLine + "No OnCall user found in " + item.ServiceName;
                        }
                        ServiceLineTeam1Ids = new List<int>();
                    }


                    var serviceLineTeam2Exist = ServiceLineTeam2Ids.Select(d => new { ServiceLineId = d, IsExist = UserChannelSid.Select(x => x.ServiceLineIdFk).Contains(d) }).ToList();
                    if (serviceLineTeam2Exist.Count > 0)
                    {
                        if (!serviceLineTeam2Exist.Select(x => x.IsExist).All(x => x == true))
                        {
                            var notExisted = serviceLineTeam2Exist.Where(x => !x.IsExist).Select(x => x.ServiceLineId).ToList();
                            ServiceLineTeam2Ids.RemoveAll(d => notExisted.Contains(d));
                            var services = this._serviceLineRepo.Table.Where(x => notExisted.Contains(x.ServiceLineId)).Select(x => new { x.ServiceLineId, x.ServiceName }).ToList();
                            foreach (var item in services)
                            {
                                msg += Environment.NewLine + "No OnCall user found in " + item.ServiceName;
                            }
                        }
                    }
                    else
                    {
                        var services = this._serviceLineRepo.Table.Where(x => ServiceLineTeam2Ids.Contains(x.ServiceLineId)).Select(x => new { x.ServiceLineId, x.ServiceName }).ToList();
                        foreach (var item in services)
                        {
                            msg += Environment.NewLine + "No OnCall user found in " + item.ServiceName;
                        }
                        ServiceLineTeam2Ids = new List<int>();
                    }



                    if (DefaultServiceLineIds.Count > 0 || ServiceLineTeam1Ids.Count > 0 || ServiceLineTeam2Ids.Count > 0)
                    {
                        var codeService = new CodesServiceLinesMapping()
                        {
                            OrganizationIdFk = codeSTEMI.OrganizationIdFk,
                            CodeIdFk = UCLEnums.Stemi.ToInt(),
                            DefaultServiceLineIdFk = DefaultServiceLineIds.Count > 0 ? string.Join(",", DefaultServiceLineIds) : null,
                            ServiceLineId1Fk = ServiceLineTeam1Ids.Count > 0 ? string.Join(",", ServiceLineTeam1Ids) : null,
                            ServiceLineId2Fk = ServiceLineTeam2Ids.Count > 0 ? string.Join(",", ServiceLineTeam2Ids) : null,
                            ActiveCodeId = codeSTEMI.CodeStemiid,
                            ActiveCodeName = UCLEnums.Stemi.ToString()
                        };
                        this._codesServiceLinesMappingRepo.Insert(codeService);
                    }
                }
                else
                {
                    DefaultServiceLineIds = new List<int>();
                    ServiceLineTeam1Ids = new List<int>();
                    ServiceLineTeam2Ids = new List<int>();
                }


                var superAdmins = this._userRepo.Table.Where(x => x.IsInGroup && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId, ServiceLineIdFk = 0 }).ToList();
                UserChannelSid.AddRange(superAdmins);
                var loggedInUser = this._userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId, ServiceLineIdFk = 0 }).FirstOrDefault();
                UserChannelSid.Add(loggedInUser);

                if (codeSTEMI.ChannelSid != null && codeSTEMI.ChannelSid != "")
                {
                    var channelSid = codeSTEMI.ChannelSid; //channel.Select(x => x.ChannelSid).FirstOrDefault();

                    var groupMembers = this._STEMICodeGroupMembersRepo.Table.Where(x => x.StemicodeIdFk == codeSTEMI.CodeStemiid).ToList();
                    this._STEMICodeGroupMembersRepo.DeleteRange(groupMembers);
                    //this._STEMICodeGroupMembersRepo.DeleteRange(channel);
                    bool isDeleted = _communicationService.DeleteUserToConversationChannel(channelSid);
                    List<CodeStemigroupMember> ACodeGroupMembers = new List<CodeStemigroupMember>();
                    foreach (var item in UserChannelSid.Distinct())
                    {
                        try
                        {
                            var codeGroupMember = new CodeStemigroupMember()
                            {
                                UserIdFk = item.UserId,
                                StemicodeIdFk = codeSTEMI.CodeStemiid,
                                //ActiveCodeName = UCLEnums.Stemi.ToString(),
                                IsAcknowledge = false,
                                CreatedBy = ApplicationSettings.UserId,
                                CreatedDate = DateTime.UtcNow,
                                IsDeleted = false
                            };
                            ACodeGroupMembers.Add(codeGroupMember);
                            _communicationService.addNewUserToConversationChannel(channelSid, item.UserUniqueId);
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
                    Id = codeSTEMI.CodeStemiid,
                    OrgId = codeSTEMI.OrganizationIdFk,
                    UserChannelSid = UserChannelSid.Select(x => x.UserUniqueId).Distinct().ToList(),
                    From = AuthorEnums.Stemi.ToString(),
                    Msg = (codeSTEMI.IsEms.HasValue && codeSTEMI.IsEms.Value ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription()) + " STEMI From is Changed",
                    RouteLink1 = RouteEnums.CodeStemiForm.ToDescription(), // "/Home/Inhouse%20Codes/code-strok-form",
                    RouteLink2 = RouteEnums.EMSForms.ToDescription(), // RouteEnums.EMSForms.ToDescription(),
                };

                _communicationService.pushNotification(notification);

                return new BaseResponse() { Status = HttpStatusCode.OK, Message = msg, Body = new { serviceLineUsersFound = userNotFound, DefaultServiceLineIds, ServiceLineTeam1Ids, ServiceLineTeam2Ids } };
            }
            return new BaseResponse() { Status = HttpStatusCode.NotModified, Message = "Default Service Lines Required" };
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
                                                        new SqlParameter { ParameterName = "@CodeName", Value = UCLEnums.Stemi.ToString() },
                                                        new SqlParameter { ParameterName = "@CodeId", Value = STEMIId },
                                                        new SqlParameter { ParameterName = "@UserId", Value = ApplicationSettings.UserId }
                                                      };

            var isDeleted = this._dbContext.Database.ExecuteSqlRaw(sql, parameters);
            this._dbContext.Log(new { }, ActivityLogTableEnums.CodeSTEMIs.ToString(), STEMIId, ActivityLogActionEnums.Delete.ToInt());

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Deleted" };
        }

        public BaseResponse ActiveOrInActiveSTEMI(int STEMIId, bool status)
        {
            //var row = this._codeSTEMIRepo.Table.Where(x => x.CodeStemiid == STEMIId && !x.IsDeleted).FirstOrDefault();
            //row.IsDeleted = true;
            //row.ModifiedBy = ApplicationSettings.UserId;
            //row.ModifiedDate = DateTime.UtcNow;
            //this._codeSTEMIRepo.Update(row);

            var sql = "EXEC md_ActiveOrInActiveInHouseCodesOrEMSDynamic @Status, @CodeName, @CodeId, @UserId";
            var parameters = new List<SqlParameter>() { new SqlParameter { ParameterName = "@Status", Value = status },
                                                        new SqlParameter { ParameterName = "@CodeName", Value = UCLEnums.Stemi.ToString() },
                                                        new SqlParameter { ParameterName = "@CodeId", Value = STEMIId },
                                                        new SqlParameter { ParameterName = "@UserId", Value = ApplicationSettings.UserId }
                                                      };

            var rowsEffected = this._dbContext.Database.ExecuteSqlRaw(sql, parameters);
            if (rowsEffected > 0)
            {
                this._dbContext.Log(new { }, ActivityLogTableEnums.CodeSTEMIs.ToString(), STEMIId, status == false ? ActivityLogActionEnums.Inactive.ToInt() : ActivityLogActionEnums.Active.ToInt());

                var userIds = this._dbContext.LoadStoredProcedure("md_getAllActiveCodesGroupUserIds")
                                                .WithSqlParam("@codeName", UCLEnums.Stemi.ToString())
                                                .WithSqlParam("@codeId", STEMIId)
                                                .ExecuteStoredProc<CodeStrokeVM>();

                var notification = new PushNotificationVM()
                {
                    Id = STEMIId,
                    OrgId = userIds.Select(x => x.OrganizationIdFk).FirstOrDefault(),
                    Type = "ChannelStatusChanged",
                    ChannelIsActive = status,
                    ChannelSid = userIds.Select(x => x.ChannelSid).FirstOrDefault(),
                    UserChannelSid = userIds.Select(x => x.UserUniqueId).Distinct().ToList(),
                    From = UCLEnums.Stemi.ToString(),
                    Msg = UCLEnums.Stemi.ToString() + " is " + (status ? "Activated" : "Inactivated"),
                    RouteLink3 = RouteEnums.ActiveEMS.ToDescription(),
                    RouteLink4 = RouteEnums.Dashboard.ToDescription(),
                    RouteLink5 = RouteEnums.InhouseCodeGrid.ToDescription()
                };

                _communicationService.pushNotification(notification);
            }

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Deleted" };
        }



        #endregion

        #region Code Truma

        public BaseResponse GetAllTrumaCode(ActiveCodeVM activeCode)
        {
            var gridColumns = GetInhouseCodeTableFeilds(activeCode.OrganizationIdFk, UCLEnums.Trauma.ToString());
            dynamic Fields = gridColumns.Body;
            if (Fields != null && Fields.FieldName != null)
            {
                string FieldNames = Convert.ToString(Fields.FieldName);
                var objList = this._dbContext.LoadStoredProcedure("md_getAllActiveCodesOrEMS_Dynamic")
                                .WithSqlParam("@status", activeCode.Status)
                                .WithSqlParam("@colName", FieldNames)
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
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Returned", Body = new { totalRecords, objList, fields = gridColumns.Body } };

            }
            return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Fields Name Not Found" };
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
                string Type = TrumaDataVM.IsEms.HasValue && TrumaDataVM.IsEms.Value ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription();
                var serviceIds = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == TrumaData.OrganizationIdFk && x.CodeIdFk == UCLEnums.Trauma.ToInt() && x.Type == Type && x.IsActive.HasValue && x.IsActive.Value && !x.IsDeleted).Select(x => new { x.DefaultServiceLineTeam, x.ServiceLineTeam1, x.ServiceLineTeam2 }).FirstOrDefault();


                var serviceLineIds = (from x in this._codesServiceLinesMappingRepo.Table
                                      where x.OrganizationIdFk == TrumaData.OrganizationIdFk && x.CodeIdFk == UCLEnums.Trauma.ToInt()
                                      && x.ActiveCodeId == TrumaData.CodeTraumaId && x.ActiveCodeName == UCLEnums.Trauma.ToString()
                                      select new { x.DefaultServiceLineIdFk, x.ServiceLineId1Fk, x.ServiceLineId2Fk }).FirstOrDefault();

                if (serviceIds != null)
                {
                    List<int> defaultIds = new();
                    List<int> team1 = new();
                    List<int> team2 = new();
                    if (serviceLineIds != null)
                    {
                        defaultIds = serviceIds.DefaultServiceLineTeam.ToIntList().Where(x => serviceLineIds.DefaultServiceLineIdFk.ToIntList().Contains(x)).Distinct().ToList();
                        team1 = serviceIds.ServiceLineTeam1.ToIntList().Where(x => serviceLineIds.ServiceLineId1Fk.ToIntList().Contains(x)).Distinct().ToList();
                        team2 = serviceIds.ServiceLineTeam2.ToIntList().Where(x => serviceLineIds.ServiceLineId2Fk.ToIntList().Contains(x)).Distinct().ToList();
                    }
                    TrumaDataVM.DefaultServiceLineTeam = this._serviceLineRepo.Table.Where(x => serviceIds.DefaultServiceLineTeam.ToIntList().Distinct().Contains(x.ServiceLineId) && !x.IsDeleted).Select(x => new ServiceLineVM() { ServiceLineId = x.ServiceLineId, ServiceName = x.ServiceName, IsSelected = defaultIds.Contains(x.ServiceLineId) }).ToList();
                    TrumaDataVM.ServiceLineTeam1 = this._serviceLineRepo.Table.Where(x => serviceIds.ServiceLineTeam1.ToIntList().Distinct().Contains(x.ServiceLineId) && !x.IsDeleted).Select(x => new ServiceLineVM() { ServiceLineId = x.ServiceLineId, ServiceName = x.ServiceName, IsSelected = team1.Contains(x.ServiceLineId) }).ToList();
                    TrumaDataVM.ServiceLineTeam2 = this._serviceLineRepo.Table.Where(x => serviceIds.ServiceLineTeam2.ToIntList().Distinct().Contains(x.ServiceLineId) && !x.IsDeleted).Select(x => new ServiceLineVM() { ServiceLineId = x.ServiceLineId, ServiceName = x.ServiceName, IsSelected = team2.Contains(x.ServiceLineId) }).ToList();
                }

                if (TrumaDataVM.IsEms.HasValue && TrumaDataVM.IsEms.Value)
                    TrumaDataVM.OrganizationData = GetHosplitalAddressObject(TrumaDataVM.OrganizationIdFk);
                TrumaDataVM.LastKnownWellStr = TrumaDataVM.LastKnownWell?.ToTimezoneFromUtc("Eastern Standard Time").ToString("yyyy-MM-dd hh:mm:ss tt");
                TrumaDataVM.DobStr = TrumaDataVM.Dob?.ToTimezoneFromUtc("Eastern Standard Time").ToString("yyyy-MM-dd hh:mm:ss tt");
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
                codeTruma.LastKnownWell = DateTime.Parse(codeTruma.LastKnownWellStr).ToUniversalTimeZone(); ;
            }
            if (codeTruma != null && !string.IsNullOrEmpty(codeTruma.DobStr) && !string.IsNullOrWhiteSpace(codeTruma.DobStr))
            {
                codeTruma.Dob = DateTime.Parse(codeTruma.DobStr).ToUniversalTimeZone(); ;
            }
            if (codeTruma.CodeTraumaId > 0)
            {
                var row = new CodeTrauma();
                object fieldValue = new();
                if (codeTruma.FieldDataType != "file")
                {
                    var fieldName = string.Empty;
                    if (codeTruma.FieldName == "HPI" || codeTruma.FieldName == "DOB")
                    {
                        fieldName = codeTruma.FieldName.ToCapitalize();
                    }
                    else
                    {
                        fieldName = codeTruma.FieldName;
                    }

                    fieldValue = codeTruma.GetPropertyValueByName(fieldName);

                    row = this._dbContext.LoadStoredProcedure("md_UpdateCodes")
                                             .WithSqlParam("codeName", UCLEnums.Trauma.ToString())
                                             .WithSqlParam("fieldName", codeTruma.FieldName)
                                             .WithSqlParam("fieldValue", fieldValue)
                                             .WithSqlParam("codeId", codeTruma.CodeTraumaId)
                                             .WithSqlParam("modifiedBy", ApplicationSettings.UserId)
                                             .ExecuteStoredProc<CodeTrauma>().FirstOrDefault();
                    this._dbContext.Log(row.getChangedPropertyObject(fieldName), ActivityLogTableEnums.CodeTraumas.ToString(), row.CodeTraumaId, ActivityLogActionEnums.Update.ToInt());
                    var userIds = this._TraumaCodeGroupMembersRepo.Table.Where(x => x.TraumaCodeIdFk == row.CodeTraumaId).Select(x => x.UserIdFk).ToList();
                    var userUniqueIds = this._userRepo.Table.Where(x => userIds.Contains(x.UserId)).Select(x => x.UserUniqueId).Distinct().ToList();
                    var superAdmins = this._userRepo.Table.Where(x => x.IsInGroup && !x.IsDeleted).Select(x => x.UserUniqueId).ToList();
                    userUniqueIds.AddRange(superAdmins);
                    var loggedUser = this._userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && !x.IsDeleted).Select(x => x.UserUniqueId).FirstOrDefault();
                    userUniqueIds.Add(loggedUser);
                    if (codeTruma.FieldDataType == "date")
                    {
                        fieldValue = codeTruma.GetPropertyValueByName(fieldName + "Str");
                    }
                    var notification = new PushNotificationVM()
                    {
                        Id = row.CodeTraumaId,
                        OrgId = row.OrganizationIdFk,
                        FieldName = codeTruma.FieldName,
                        FieldDataType = codeTruma.FieldDataType,
                        FieldValue = fieldValue,
                        UserChannelSid = userUniqueIds.Distinct().ToList(),
                        From = AuthorEnums.Trauma.ToString(),
                        Msg = (row.IsEms != null && row.IsEms.Value ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription()) + " Truma From is Changed",
                        RouteLink1 = RouteEnums.CodeTraumaForm.ToDescription(), // "/Home/Inhouse%20Codes/code-strok-form",
                        RouteLink2 = RouteEnums.EMSForms.ToDescription(), // RouteEnums.EMSForms.ToDescription(),
                    };

                    _communicationService.pushNotification(notification);
                    return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Updated Successfully" };
                }
                else
                {

                    row = this._codeTrumaRepo.Table.Where(x => x.CodeTraumaId == codeTruma.CodeTraumaId && !x.IsDeleted).FirstOrDefault();

                    row.ModifiedBy = codeTruma.ModifiedBy;
                    row.ModifiedDate = DateTime.UtcNow;
                    row.IsDeleted = false;

                    if (codeTruma.Attachment != null && codeTruma.Attachment.Count > 0)
                    {
                        var RootPath = this._RootPath + "/Organizations"; //this._RootPath + "/Organizations";
                        string FileRoot = null;
                        List<string> Attachments = new();
                        FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == row.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
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
                        FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == row.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
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

                        FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == row.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
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
                        string path = this._RootPath + codeTruma.AttachmentsFolderRoot;
                        if (Directory.Exists(path))
                        {
                            DirectoryInfo AttachFiles = new DirectoryInfo(path);
                            codeTruma.AttachmentsPath = new List<string>();
                            foreach (var item in AttachFiles.GetFiles())
                            {
                                codeTruma.AttachmentsPath.Add(codeTruma.AttachmentsFolderRoot + "/" + item.Name);
                            }
                        }
                    }
                    if (codeTruma.VideoFolderRoot != null)
                    {
                        row.Video = codeTruma.VideoFolderRoot;
                        var path = this._RootPath + codeTruma.VideoFolderRoot;
                        if (Directory.Exists(path))
                        {
                            DirectoryInfo VideoFiles = new DirectoryInfo(path);
                            codeTruma.VideosPath = new List<string>();
                            foreach (var item in VideoFiles.GetFiles())
                            {
                                codeTruma.VideosPath.Add(codeTruma.VideoFolderRoot + "/" + item.Name);
                            }
                        }
                    }
                    if (codeTruma.AudioFolderRoot != null)
                    {
                        row.Audio = codeTruma.AudioFolderRoot;
                        string path = this._RootPath + codeTruma.AudioFolderRoot;
                        if (Directory.Exists(path))
                        {
                            DirectoryInfo AudioFiles = new DirectoryInfo(path);
                            codeTruma.AudiosPath = new List<string>();
                            foreach (var item in AudioFiles.GetFiles())
                            {
                                codeTruma.AudiosPath.Add(codeTruma.AudioFolderRoot + "/" + item.Name);
                            }
                        }
                    }

                    this._codeTrumaRepo.Update(row);
                    this._dbContext.Log(row, ActivityLogTableEnums.CodeTraumas.ToString(), row.CodeTraumaId, ActivityLogActionEnums.FileUpload.ToInt());
                    fieldValue = new { videosPath = codeTruma.VideosPath, audiosPath = codeTruma.AudiosPath, attachmentsPath = codeTruma.AttachmentsPath };
                    var userIds = this._TraumaCodeGroupMembersRepo.Table.Where(x => x.TraumaCodeIdFk == row.CodeTraumaId).Select(x => x.UserIdFk).ToList();
                    var userUniqueIds = this._userRepo.Table.Where(x => userIds.Contains(x.UserId)).Select(x => x.UserUniqueId).Distinct().ToList();
                    var superAdmins = this._userRepo.Table.Where(x => x.IsInGroup && !x.IsDeleted).Select(x => x.UserUniqueId).ToList();
                    userUniqueIds.AddRange(superAdmins);
                    var loggedUser = this._userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && !x.IsDeleted).Select(x => x.UserUniqueId).FirstOrDefault();
                    userUniqueIds.Add(loggedUser);

                    var notification = new PushNotificationVM()
                    {
                        Id = row.CodeTraumaId,
                        OrgId = row.OrganizationIdFk,
                        FieldName = codeTruma.FieldName,
                        FieldDataType = codeTruma.FieldDataType,
                        FieldValue = fieldValue,
                        UserChannelSid = userUniqueIds,
                        From = AuthorEnums.Trauma.ToString(),
                        Msg = (row.IsEms != null && row.IsEms.Value ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription()) + " Trauma From is Changed",
                        RouteLink1 = RouteEnums.CodeTraumaForm.ToDescription(), // "/Home/Inhouse%20Codes/code-strok-form",
                        RouteLink2 = RouteEnums.EMSForms.ToDescription(), // RouteEnums.EMSForms.ToDescription(),
                    };

                    _communicationService.pushNotification(notification);
                    return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Updated Successfully" };
                }

            }
            else
            {

                if (codeTruma.OrganizationIdFk > 0)
                {
                    string IsEMS = codeTruma.IsEms.HasValue && codeTruma.IsEms.Value ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription();
                    var DefaultServiceLineTeam = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == codeTruma.OrganizationIdFk && x.CodeIdFk == UCLEnums.Trauma.ToInt() && x.Type == IsEMS && x.IsActive.HasValue && x.IsActive.Value && !x.IsDeleted).Select(x => x.DefaultServiceLineTeam).FirstOrDefault();
                    if (DefaultServiceLineTeam != null && DefaultServiceLineTeam != "")
                    {
                        var DefaultServiceLineIds = DefaultServiceLineTeam.ToIntList();

                        codeTruma.CreatedDate = DateTime.UtcNow;
                        codeTruma.FamilyContactNumber = codeTruma.FamilyContactNumber != null && codeTruma.FamilyContactNumber != "" && codeTruma.FamilyContactNumber != "(___) ___-____" ? codeTruma.FamilyContactNumber : "";
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
                        this._dbContext.Log(Truma, ActivityLogTableEnums.CodeTraumas.ToString(), Truma.CodeTraumaId, ActivityLogActionEnums.Create.ToInt());

                        var codeService = new CodesServiceLinesMapping()
                        {
                            OrganizationIdFk = Truma.OrganizationIdFk,
                            CodeIdFk = UCLEnums.Trauma.ToInt(),
                            DefaultServiceLineIdFk = DefaultServiceLineTeam,
                            ActiveCodeId = Truma.CodeTraumaId,
                            ActiveCodeName = UCLEnums.Trauma.ToString()
                        };
                        this._codesServiceLinesMappingRepo.Insert(codeService);

                        //var UserChannelSid = (from us in this._userSchedulesRepo.Table
                        //                      join u in this._userRepo.Table on us.UserIdFk equals u.UserId
                        //                      where DefaultServiceLineIds.Contains(us.ServiceLineIdFk.Value) && us.ScheduleDateStart <= DateTime.UtcNow && us.ScheduleDateEnd >= DateTime.UtcNow && !us.IsDeleted && !u.IsDeleted
                        //                      select new { u.UserUniqueId, u.UserId }).ToList();
                        //var superAdmins = this._userRepo.Table.Where(x => x.IsInGroup && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId }).ToList();
                        //UserChannelSid.AddRange(superAdmins);
                        //var loggedUser = this._userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId }).FirstOrDefault();
                        //UserChannelSid.Add(loggedUser);
                        //List<CodeTraumaGroupMember> ACodeGroupMembers = new();
                        //if (UserChannelSid != null && UserChannelSid.Count > 0)
                        //{
                        //    string uniqueName = DateTime.Now.ToString("yyyyMMddHHmmssffff") + ApplicationSettings.UserId.ToString();
                        //    string friendlyName = Truma.IsEms.HasValue && Truma.IsEms.Value ? $"{UCLEnums.EMS.ToDescription()} {UCLEnums.Trauma.ToString()} {Truma.CodeTraumaId}" : $"{UCLEnums.InhouseCode.ToDescription()} {UCLEnums.Trauma.ToString()} {Truma.CodeTraumaId}";
                        //    var conversationChannelAttributes = JsonConvert.SerializeObject(new Dictionary<string, Object>()
                        //            {
                        //                {ChannelAttributeEnums.ChannelType.ToString(), ChannelTypeEnums.EMS.ToString()},
                        //                {ChannelAttributeEnums.CodeType.ToString(), ChannelTypeEnums.Trauma.ToString()},
                        //                {ChannelAttributeEnums.TraumaId.ToString(), Truma.CodeTraumaId}
                        //            }, Formatting.Indented);
                        //    var channel = _communicationService.createConversationChannel(friendlyName, uniqueName, conversationChannelAttributes);
                        //    Truma.ChannelSid = channel.Sid;
                        //    this._codeTrumaRepo.Update(Truma);
                        //    UserChannelSid = UserChannelSid.Distinct().ToList();
                        //    foreach (var item in UserChannelSid)
                        //    {
                        //        try
                        //        {
                        //            var codeGroupMember = new CodeTraumaGroupMember()
                        //            {
                        //                //ChannelSid = channel.Sid,
                        //                UserIdFk = item.UserId,
                        //                TraumaCodeIdFk = Truma.CodeTraumaId,
                        //                //ActiveCodeName = UCLEnums.Trauma.ToString(),
                        //                IsAcknowledge = false,
                        //                CreatedBy = ApplicationSettings.UserId,
                        //                CreatedDate = DateTime.UtcNow,
                        //                IsDeleted = false
                        //            };
                        //            ACodeGroupMembers.Add(codeGroupMember);
                        //            _communicationService.addNewUserToConversationChannel(channel.Sid, item.UserUniqueId);
                        //        }
                        //        catch (Exception ex)
                        //        {
                        //            //ElmahExtensions.RiseError(ex);
                        //        }
                        //    }
                        //    this._TraumaCodeGroupMembersRepo.Insert(ACodeGroupMembers);

                        //    var msg = new ConversationMessageVM();
                        //    msg.channelSid = channel.Sid;
                        //    msg.author = "System";
                        //    msg.attributes = "";
                        //    msg.body = $"<strong> {(codeTruma.IsEms.HasValue && codeTruma.IsEms.Value ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription())} {UCLEnums.Trauma.ToString()} </strong></br></br>";
                        //    if (codeTruma.PatientName != null && codeTruma.PatientName != "")
                        //        msg.body += $"<strong>Patient Name: </strong> {codeTruma.PatientName} </br>";
                        //    msg.body += $"<strong>Dob: </strong> {codeTruma.Dob:MM-dd-yyyy} </br>";
                        //    msg.body += $"<strong>Last Well Known: </strong> {codeTruma.LastKnownWell:MM-dd-yyyy hh:mm tt} </br>";
                        //    if (codeTruma.ChiefComplant != null && codeTruma.ChiefComplant != "")
                        //        msg.body += $"<strong>Chief Complaint: </strong> {codeTruma.ChiefComplant} </br>";
                        //    if (codeTruma.Hpi != null && codeTruma.Hpi != "")
                        //        msg.body += $"<strong>Hpi: </strong> {codeTruma.Hpi} </br>";
                        //    var sendMsg = _communicationService.sendPushNotification(msg);

                        //    var showAllAccessUsers = this._dbContext.LoadStoredProcedure("md_getUsersOfComponentAccess")
                        //                       .WithSqlParam("@componentName", "Show All EMS,Show All Active Codes,Show Graphs,Show EMS,Show Active Codes")
                        //                       .WithSqlParam("@orgId", Truma.OrganizationIdFk)
                        //                       .ExecuteStoredProc<RegisterCredentialVM>().Select(x => new { x.UserUniqueId, x.UserId }).Distinct().ToList();
                        //    UserChannelSid.AddRange(showAllAccessUsers);
                        //    var notification = new PushNotificationVM()
                        //    {
                        //        Id = Truma.CodeTraumaId,
                        //        OrgId = Truma.OrganizationIdFk,
                        //        UserChannelSid = UserChannelSid.Select(x => x.UserUniqueId).Distinct().ToList(),
                        //        From = AuthorEnums.Trauma.ToString(),
                        //        Msg = (codeTruma.IsEms.HasValue && codeTruma.IsEms.Value ? UCLEnums.EMS.ToDescription() : "{UCLEnums.InhouseCode.ToDescription()}) + " Code Trauma is update",
                        //        RouteLink3 = RouteEnums.ActiveEMS.ToDescription(),
                        //        RouteLink4 = RouteEnums.Dashboard.ToDescription(),
                        //        RouteLink5 = RouteEnums.InhouseCodeGrid.ToDescription()
                        //    };

                        //    _communicationService.pushNotification(notification);
                        //}
                        return GetTrumaDataById(Truma.CodeTraumaId);
                    }
                    return new BaseResponse() { Status = HttpStatusCode.NotAcceptable, Message = "There is no Service Line in this organization related to Code Trauma" };
                }
                return new BaseResponse() { Status = HttpStatusCode.NotAcceptable, Message = "Organization is not selected" };
            }
        }
        public BaseResponse CreateTrumaGroup(CodeTrumaVM codeTruma)
        {

            var DefaultServiceLineIds = codeTruma.DefaultServiceLineIds.ToIntList();
            bool usersFound = false;
            string errorMsg = "";

            var UserChannelSid = (from us in this._userSchedulesRepo.Table
                                  join u in this._userRepo.Table on us.UserIdFk equals u.UserId
                                  where DefaultServiceLineIds.Contains(us.ServiceLineIdFk.Value) && us.ScheduleDateStart <= DateTime.UtcNow && us.ScheduleDateEnd >= DateTime.UtcNow && !us.IsDeleted && !u.IsDeleted
                                  select new { u.UserUniqueId, u.UserId, ServiceLineIdFk = us.ServiceLineIdFk.Value }).Distinct().ToList();


            if (UserChannelSid.Count > 0)
            {
                usersFound = true;
                var defaultExist = DefaultServiceLineIds.Select(d => new { ServiceLineId = d, IsExist = UserChannelSid.Select(x => x.ServiceLineIdFk).Contains(d) }).ToList();
                if (defaultExist.Count > 0)
                {
                    if (!defaultExist.Select(x => x.IsExist).All(x => x == true))
                    {
                        var notExisted = defaultExist.Where(x => !x.IsExist).Select(x => x.ServiceLineId).ToList();
                        DefaultServiceLineIds.RemoveAll(d => notExisted.Contains(d));
                        var services = this._serviceLineRepo.Table.Where(x => notExisted.Contains(x.ServiceLineId)).Select(x => new { x.ServiceLineId, x.ServiceName }).ToList();
                        foreach (var item in services)
                        {
                            errorMsg += Environment.NewLine + "No OnCall user found in " + item.ServiceName;
                        }
                    }
                }
                else
                {
                    var services = this._serviceLineRepo.Table.Where(x => DefaultServiceLineIds.Contains(x.ServiceLineId)).Select(x => new { x.ServiceLineId, x.ServiceName }).ToList();
                    foreach (var item in services)
                    {
                        errorMsg += Environment.NewLine + "No OnCall user found in " + item.ServiceName;
                    }
                    DefaultServiceLineIds = new List<int>();
                }

                if (DefaultServiceLineIds.Count > 0)
                {
                    var codeService = new CodesServiceLinesMapping()
                    {
                        OrganizationIdFk = codeTruma.OrganizationIdFk,
                        CodeIdFk = UCLEnums.Trauma.ToInt(),
                        DefaultServiceLineIdFk = string.Join(",", DefaultServiceLineIds),
                        ActiveCodeId = codeTruma.CodeTraumaId,
                        ActiveCodeName = UCLEnums.Trauma.ToString()
                    };
                    this._codesServiceLinesMappingRepo.Insert(codeService);
                }
            }
            else
            {
                DefaultServiceLineIds = new List<int>();
            }

            var superAdmins = this._userRepo.Table.Where(x => x.IsInGroup && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId, ServiceLineIdFk = 0 }).ToList();
            UserChannelSid.AddRange(superAdmins);
            var loggedUser = this._userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId, ServiceLineIdFk = 0 }).FirstOrDefault();
            UserChannelSid.Add(loggedUser);
            var conversationChannelAttributes = JsonConvert.SerializeObject(new Dictionary<string, Object>()
                                    {
                                        {ChannelAttributeEnums.ChannelType.ToString(), ChannelTypeEnums.EMS.ToString()},
                                        {ChannelAttributeEnums.CodeType.ToString(), ChannelTypeEnums.Trauma.ToString()},
                                        {ChannelAttributeEnums.TraumaId.ToString(), codeTruma.CodeTraumaId}
                                    }, Formatting.Indented);
            List<CodeTraumaGroupMember> ACodeGroupMembers = new List<CodeTraumaGroupMember>();
            if (UserChannelSid != null && UserChannelSid.Count > 0)
            {
                string uniqueName = DateTime.Now.ToString("yyyyMMddHHmmssffff") + ApplicationSettings.UserId.ToString();
                string friendlyName = codeTruma.IsEms.HasValue && codeTruma.IsEms.Value ? $"{UCLEnums.EMS.ToDescription()} {UCLEnums.Trauma.ToString()} {codeTruma.CodeTraumaId}" : $"{UCLEnums.InhouseCode.ToDescription()} {UCLEnums.Trauma.ToString()} {codeTruma.CodeTraumaId}";
                var channel = _communicationService.createConversationChannel(friendlyName, uniqueName, conversationChannelAttributes);
                var Truma = this._codeTrumaRepo.Table.Where(x => x.CodeTraumaId == codeTruma.CodeTraumaId && x.IsActive == true && !x.IsDeleted).FirstOrDefault();
                if (Truma != null)
                {
                    Truma.ChannelSid = channel.Sid;
                    this._codeTrumaRepo.Update(Truma);
                }
                UserChannelSid = UserChannelSid.Distinct().ToList();
                foreach (var item in UserChannelSid)
                {
                    try
                    {
                        var codeGroupMember = new CodeTraumaGroupMember()
                        {
                            UserIdFk = item.UserId,
                            TraumaCodeIdFk = Truma.CodeTraumaId,
                            //ActiveCodeName = UCLEnums.Truma.ToString(),
                            IsAcknowledge = false,
                            CreatedBy = ApplicationSettings.UserId,
                            CreatedDate = DateTime.UtcNow,
                            IsDeleted = false
                        };
                        ACodeGroupMembers.Add(codeGroupMember);
                        _communicationService.addNewUserToConversationChannel(channel.Sid, item.UserUniqueId);
                    }
                    catch (Exception ex)
                    {

                    }
                }
                this._TraumaCodeGroupMembersRepo.Insert(ACodeGroupMembers);
                var msg = new ConversationMessageVM();
                msg.channelSid = channel.Sid;
                msg.author = "System";
                msg.attributes = "";
                msg.body = $"<strong> {(codeTruma.IsEms.HasValue && codeTruma.IsEms.Value ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription())} {UCLEnums.Trauma.ToString()} </strong></br></br>";
                if (codeTruma.PatientName != null && codeTruma.PatientName != "")
                    msg.body += $"<strong>Patient Name: </strong> {codeTruma.PatientName} </br>";
                if (codeTruma.Dob != null)
                    msg.body += $"<strong>Dob: </strong> {codeTruma.Dob:MM-dd-yyyy} </br>";
                if (codeTruma.LastKnownWell != null)
                    msg.body += $"<strong>Last Well Known: </strong> {codeTruma.LastKnownWell:MM-dd-yyyy hh:mm tt} </br>";
                if (codeTruma.ChiefComplant != null && codeTruma.ChiefComplant != "")
                    msg.body += $"<strong>Chief Complaint: </strong> {codeTruma.ChiefComplant} </br>";
                if (codeTruma.Hpi != null && codeTruma.Hpi != "")
                    msg.body += $"<strong>Hpi: </strong> {codeTruma.Hpi} </br>";

                var sendMsg = _communicationService.sendPushNotification(msg);

                var showAllAccessUsers = this._dbContext.LoadStoredProcedure("md_getUsersOfComponentAccess")
                                    .WithSqlParam("@componentName", "Show All EMS,Show All Active Codes,Show Graphs,Show EMS,Show Active Codes")
                                    .WithSqlParam("@orgId", Truma.OrganizationIdFk)
                                    .ExecuteStoredProc<RegisterCredentialVM>().Select(x => new { x.UserUniqueId, x.UserId, ServiceLineIdFk = 0 }).Distinct().ToList();
                UserChannelSid.AddRange(showAllAccessUsers);
                var notification = new PushNotificationVM()
                {
                    Id = Truma.CodeTraumaId,
                    OrgId = Truma.OrganizationIdFk,
                    UserChannelSid = UserChannelSid.Select(x => x.UserUniqueId).Distinct().ToList(),
                    From = AuthorEnums.Trauma.ToString(),
                    Msg = (codeTruma.IsEms.HasValue && codeTruma.IsEms.Value ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription()) + " Truma is update",
                    RouteLink3 = RouteEnums.ActiveEMS.ToDescription(), // RouteEnums.ActiveEMS.ToDescription(),
                    RouteLink4 = RouteEnums.Dashboard.ToDescription(),
                    RouteLink5 = RouteEnums.InhouseCodeGrid.ToDescription(), // RouteEnums.InhouseCodeGrid.ToDescription()
                };

                _communicationService.pushNotification(notification);
            }

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = errorMsg, Body = new { serviceLineUsersFound = usersFound, DefaultServiceLineIds, ServiceLineTeam1Ids = new List<int>(), ServiceLineTeam2Ids = new List<int>() } };
        }

        public BaseResponse UpdateTrumaGroupMembers(CodeTrumaVM codeTruma)
        {

            if (codeTruma.DefaultServiceLineIds == null || codeTruma.DefaultServiceLineIds == "")
            {
                string IsEMS = codeTruma.IsEms.HasValue && codeTruma.IsEms.Value ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription();
                codeTruma.DefaultServiceLineIds = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == codeTruma.OrganizationIdFk && x.CodeIdFk == UCLEnums.Trauma.ToInt() && x.Type == IsEMS && x.IsActive.HasValue && x.IsActive.Value && !x.IsDeleted).Select(x => x.DefaultServiceLineTeam).FirstOrDefault();
            }

            if (codeTruma.DefaultServiceLineIds != null && codeTruma.DefaultServiceLineIds != "")
            {
                bool userNotFound = false;

                string msg = "";

                var DefaultServiceLineIds = codeTruma.DefaultServiceLineIds.ToIntList();
                var ServiceLineTeam1Ids = codeTruma.ServiceLineTeam1Ids.ToIntList();
                var ServiceLineTeam2Ids = codeTruma.ServiceLineTeam2Ids.ToIntList();

                var UserChannelSid = (from us in this._userSchedulesRepo.Table
                                      join u in this._userRepo.Table on us.UserIdFk equals u.UserId
                                      where (DefaultServiceLineIds.Contains(us.ServiceLineIdFk.Value) || ServiceLineTeam1Ids.Contains(us.ServiceLineIdFk.Value) || ServiceLineTeam2Ids.Contains(us.ServiceLineIdFk.Value)) && us.ScheduleDateStart <= DateTime.UtcNow && us.ScheduleDateEnd >= DateTime.UtcNow && !us.IsDeleted && !u.IsDeleted
                                      select new { u.UserUniqueId, u.UserId, ServiceLineIdFk = us.ServiceLineIdFk.Value }).Distinct().ToList();

                var codeServiceMapping = this._codesServiceLinesMappingRepo.Table.Where(x => x.OrganizationIdFk == codeTruma.OrganizationIdFk && x.CodeIdFk == UCLEnums.Trauma.ToInt() && x.ActiveCodeId == codeTruma.CodeTraumaId).ToList();
                if (codeServiceMapping.Count > 0)
                    this._codesServiceLinesMappingRepo.DeleteRange(codeServiceMapping);

                if (UserChannelSid.Count > 0)
                {
                    userNotFound = true;
                    var defaultExist = DefaultServiceLineIds.Select(d => new { ServiceLineId = d, IsExist = UserChannelSid.Select(x => x.ServiceLineIdFk).Contains(d) }).ToList();
                    if (defaultExist.Count > 0)
                    {
                        if (!defaultExist.Select(x => x.IsExist).All(x => x == true))
                        {
                            var notExisted = defaultExist.Where(x => !x.IsExist).Select(x => x.ServiceLineId).ToList();
                            DefaultServiceLineIds.RemoveAll(d => notExisted.Contains(d));
                            var services = this._serviceLineRepo.Table.Where(x => notExisted.Contains(x.ServiceLineId)).Select(x => new { x.ServiceLineId, x.ServiceName }).ToList();
                            foreach (var item in services)
                            {
                                msg += Environment.NewLine + "No OnCall user found in " + item.ServiceName;
                            }
                        }
                    }
                    else
                    {
                        var services = this._serviceLineRepo.Table.Where(x => DefaultServiceLineIds.Contains(x.ServiceLineId)).Select(x => new { x.ServiceLineId, x.ServiceName }).ToList();
                        foreach (var item in services)
                        {
                            msg += Environment.NewLine + "No OnCall user found in " + item.ServiceName;
                        }
                        DefaultServiceLineIds = new List<int>();
                    }

                    var serviceLineTeam1Exist = ServiceLineTeam1Ids.Select(d => new { ServiceLineId = d, IsExist = UserChannelSid.Select(x => x.ServiceLineIdFk).Contains(d) }).ToList();
                    if (serviceLineTeam1Exist.Count > 0)
                    {
                        if (!serviceLineTeam1Exist.Select(x => x.IsExist).All(x => x == true))
                        {
                            var notExisted = serviceLineTeam1Exist.Where(x => !x.IsExist).Select(x => x.ServiceLineId).ToList();
                            ServiceLineTeam1Ids.RemoveAll(d => notExisted.Contains(d));
                            var services = this._serviceLineRepo.Table.Where(x => notExisted.Contains(x.ServiceLineId)).Select(x => new { x.ServiceLineId, x.ServiceName }).ToList();
                            foreach (var item in services)
                            {
                                msg += Environment.NewLine + "No OnCall user found in " + item.ServiceName;
                            }
                        }
                    }
                    else
                    {
                        var services = this._serviceLineRepo.Table.Where(x => ServiceLineTeam1Ids.Contains(x.ServiceLineId)).Select(x => new { x.ServiceLineId, x.ServiceName }).ToList();
                        foreach (var item in services)
                        {
                            msg += Environment.NewLine + "No OnCall user found in " + item.ServiceName;
                        }
                        ServiceLineTeam1Ids = new List<int>();
                    }


                    var serviceLineTeam2Exist = ServiceLineTeam2Ids.Select(d => new { ServiceLineId = d, IsExist = UserChannelSid.Select(x => x.ServiceLineIdFk).Contains(d) }).ToList();
                    if (serviceLineTeam2Exist.Count > 0)
                    {
                        if (!serviceLineTeam2Exist.Select(x => x.IsExist).All(x => x == true))
                        {
                            var notExisted = serviceLineTeam2Exist.Where(x => !x.IsExist).Select(x => x.ServiceLineId).ToList();
                            ServiceLineTeam2Ids.RemoveAll(d => notExisted.Contains(d));
                            var services = this._serviceLineRepo.Table.Where(x => notExisted.Contains(x.ServiceLineId)).Select(x => new { x.ServiceLineId, x.ServiceName }).ToList();
                            foreach (var item in services)
                            {
                                msg += Environment.NewLine + "No OnCall user found in " + item.ServiceName;
                            }
                        }
                    }
                    else
                    {
                        var services = this._serviceLineRepo.Table.Where(x => ServiceLineTeam2Ids.Contains(x.ServiceLineId)).Select(x => new { x.ServiceLineId, x.ServiceName }).ToList();
                        foreach (var item in services)
                        {
                            msg += Environment.NewLine + "No OnCall user found in " + item.ServiceName;
                        }
                        ServiceLineTeam2Ids = new List<int>();
                    }



                    if (DefaultServiceLineIds.Count > 0 || ServiceLineTeam1Ids.Count > 0 || ServiceLineTeam2Ids.Count > 0)
                    {
                        var codeService = new CodesServiceLinesMapping()
                        {
                            OrganizationIdFk = codeTruma.OrganizationIdFk,
                            CodeIdFk = UCLEnums.Trauma.ToInt(),
                            DefaultServiceLineIdFk = DefaultServiceLineIds.Count > 0 ? string.Join(",", DefaultServiceLineIds) : null,
                            ServiceLineId1Fk = ServiceLineTeam1Ids.Count > 0 ? string.Join(",", ServiceLineTeam1Ids) : null,
                            ServiceLineId2Fk = ServiceLineTeam2Ids.Count > 0 ? string.Join(",", ServiceLineTeam2Ids) : null,
                            ActiveCodeId = codeTruma.CodeTraumaId,
                            ActiveCodeName = UCLEnums.Trauma.ToString()
                        };
                        this._codesServiceLinesMappingRepo.Insert(codeService);
                    }
                }
                else
                {
                    DefaultServiceLineIds = new List<int>();
                    ServiceLineTeam1Ids = new List<int>();
                    ServiceLineTeam2Ids = new List<int>();
                }


                var superAdmins = this._userRepo.Table.Where(x => x.IsInGroup && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId, ServiceLineIdFk = 0 }).ToList();
                UserChannelSid.AddRange(superAdmins);
                var loggedInUser = this._userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId, ServiceLineIdFk = 0 }).FirstOrDefault();
                UserChannelSid.Add(loggedInUser);

                if (codeTruma.ChannelSid != null && codeTruma.ChannelSid != "")
                {
                    var channelSid = codeTruma.ChannelSid; //channel.Select(x => x.ChannelSid).FirstOrDefault();

                    var groupMembers = this._TraumaCodeGroupMembersRepo.Table.Where(x => x.TraumaCodeIdFk == codeTruma.CodeTraumaId).ToList();
                    this._TraumaCodeGroupMembersRepo.DeleteRange(groupMembers);
                    //this._TrumaCodeGroupMembersRepo.DeleteRange(channel);
                    bool isDeleted = _communicationService.DeleteUserToConversationChannel(channelSid);
                    List<CodeTraumaGroupMember> ACodeGroupMembers = new List<CodeTraumaGroupMember>();
                    foreach (var item in UserChannelSid.Distinct())
                    {
                        try
                        {
                            var codeGroupMember = new CodeTraumaGroupMember()
                            {
                                UserIdFk = item.UserId,
                                TraumaCodeIdFk = codeTruma.CodeTraumaId,
                                //ActiveCodeName = UCLEnums.Truma.ToString(),
                                IsAcknowledge = false,
                                CreatedBy = ApplicationSettings.UserId,
                                CreatedDate = DateTime.UtcNow,
                                IsDeleted = false
                            };
                            ACodeGroupMembers.Add(codeGroupMember);
                            _communicationService.addNewUserToConversationChannel(channelSid, item.UserUniqueId);
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
                    Id = codeTruma.CodeTraumaId,
                    OrgId = codeTruma.OrganizationIdFk,
                    UserChannelSid = UserChannelSid.Select(x => x.UserUniqueId).Distinct().ToList(),
                    From = AuthorEnums.Trauma.ToString(),
                    Msg = (codeTruma.IsEms.HasValue && codeTruma.IsEms.Value ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription()) + " Truma From is Changed",
                    RouteLink1 = RouteEnums.CodeTraumaForm.ToDescription(), // "/Home/Inhouse%20Codes/code-strok-form",
                    RouteLink2 = RouteEnums.EMSForms.ToDescription(), // RouteEnums.EMSForms.ToDescription(),
                };

                _communicationService.pushNotification(notification);

                return new BaseResponse() { Status = HttpStatusCode.OK, Message = msg, Body = new { serviceLineUsersFound = userNotFound, DefaultServiceLineIds, ServiceLineTeam1Ids, ServiceLineTeam2Ids } };
            }
            return new BaseResponse() { Status = HttpStatusCode.NotModified, Message = "Default Service Lines Required" };
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
            this._dbContext.Log(new { }, ActivityLogTableEnums.CodeTraumas.ToString(), TrumaId, ActivityLogActionEnums.Delete.ToInt());
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Deleted" };
        }

        public BaseResponse ActiveOrInActiveTruma(int TraumaId, bool status)
        {
            //var row = this._codeTrumaRepo.Table.Where(x => x.CodeTraumaId == TrumaId && !x.IsDeleted).FirstOrDefault();
            //row.IsDeleted = true;
            //row.ModifiedBy = ApplicationSettings.UserId;
            //row.ModifiedDate = DateTime.UtcNow;
            //this._codeTrumaRepo.Update(row);

            var sql = "EXEC md_ActiveOrInActiveInHouseCodesOrEMSDynamic @Status, @CodeName, @CodeId, @UserId";
            var parameters = new List<SqlParameter>() { new SqlParameter { ParameterName = "@Status", Value = status },
                                                        new SqlParameter { ParameterName = "@CodeName", Value = UCLEnums.Trauma.ToString() },
                                                        new SqlParameter { ParameterName = "@CodeId", Value = TraumaId },
                                                        new SqlParameter { ParameterName = "@UserId", Value = ApplicationSettings.UserId }
                                                      };

            var rowsEffected = this._dbContext.Database.ExecuteSqlRaw(sql, parameters);
            if (rowsEffected > 0)
            {
                this._dbContext.Log(new { }, ActivityLogTableEnums.CodeTraumas.ToString(), TraumaId, status == false ? ActivityLogActionEnums.Inactive.ToInt() : ActivityLogActionEnums.Active.ToInt());
                var userIds = this._dbContext.LoadStoredProcedure("md_getAllActiveCodesGroupUserIds")
                                                .WithSqlParam("@codeName", UCLEnums.Trauma.ToString())
                                                .WithSqlParam("@codeId", TraumaId)
                                                .ExecuteStoredProc<CodeStrokeVM>();

                var notification = new PushNotificationVM()
                {
                    Id = TraumaId,
                    OrgId = userIds.Select(x => x.OrganizationIdFk).FirstOrDefault(),
                    Type = "ChannelStatusChanged",
                    ChannelIsActive = status,
                    ChannelSid = userIds.Select(x => x.ChannelSid).FirstOrDefault(),
                    UserChannelSid = userIds.Select(x => x.UserUniqueId).Distinct().ToList(),
                    From = UCLEnums.Trauma.ToString(),
                    Msg = UCLEnums.Trauma.ToString() + " is " + (status ? "Activated" : "Inactivated"),
                    RouteLink3 = RouteEnums.ActiveEMS.ToDescription(),
                    RouteLink4 = RouteEnums.Dashboard.ToDescription(),
                    RouteLink5 = RouteEnums.InhouseCodeGrid.ToDescription()
                };

                _communicationService.pushNotification(notification);
            }

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Deleted" };
        }


        #endregion

        #region Code Blue

        public BaseResponse GetAllBlueCode(ActiveCodeVM activeCode)
        {
            var gridColumns = GetInhouseCodeTableFeilds(activeCode.OrganizationIdFk, UCLEnums.Blue.ToString());
            dynamic Fields = gridColumns.Body;
            if (Fields != null && Fields.FieldName != null)
            {
                string FieldNames = Convert.ToString(Fields.FieldName);
                var objList = this._dbContext.LoadStoredProcedure("md_getAllActiveCodesOrEMS_Dynamic")
                                .WithSqlParam("@status", activeCode.Status)
                                .WithSqlParam("@colName", FieldNames)
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
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Returned", Body = new { totalRecords, objList, fields = gridColumns.Body } };

            }
            return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Fields Name Not Found" };
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
                string Type = BlueDataVM.IsEms.HasValue && BlueDataVM.IsEms.Value ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription();
                var serviceIds = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == blueData.OrganizationIdFk && x.CodeIdFk == UCLEnums.Blue.ToInt() && x.Type == Type && x.IsActive.HasValue && x.IsActive.Value && !x.IsDeleted).Select(x => new { x.DefaultServiceLineTeam, x.ServiceLineTeam1, x.ServiceLineTeam2 }).FirstOrDefault();

                var serviceLineIds = (from x in this._codesServiceLinesMappingRepo.Table
                                      where x.OrganizationIdFk == blueData.OrganizationIdFk && x.CodeIdFk == UCLEnums.Blue.ToInt()
                                      && x.ActiveCodeId == blueData.CodeBlueId && x.ActiveCodeName == UCLEnums.Blue.ToString()
                                      select new { x.DefaultServiceLineIdFk, x.ServiceLineId1Fk, x.ServiceLineId2Fk }).FirstOrDefault();
                if (serviceIds != null)
                {
                    List<int> defaultIds = new();
                    List<int> team1 = new();
                    List<int> team2 = new();
                    if (serviceLineIds != null)
                    {
                        defaultIds = serviceIds.DefaultServiceLineTeam.ToIntList().Where(x => serviceLineIds.DefaultServiceLineIdFk.ToIntList().Contains(x)).Distinct().ToList();
                        team1 = serviceIds.ServiceLineTeam1.ToIntList().Where(x => serviceLineIds.ServiceLineId1Fk.ToIntList().Contains(x)).Distinct().ToList();
                        team2 = serviceIds.ServiceLineTeam2.ToIntList().Where(x => serviceLineIds.ServiceLineId2Fk.ToIntList().Contains(x)).Distinct().ToList();
                    }
                    BlueDataVM.DefaultServiceLineTeam = this._serviceLineRepo.Table.Where(x => serviceIds.DefaultServiceLineTeam.ToIntList().Distinct().Contains(x.ServiceLineId) && !x.IsDeleted).Select(x => new ServiceLineVM() { ServiceLineId = x.ServiceLineId, ServiceName = x.ServiceName, IsSelected = defaultIds.Contains(x.ServiceLineId) }).ToList();
                    BlueDataVM.ServiceLineTeam1 = this._serviceLineRepo.Table.Where(x => serviceIds.ServiceLineTeam1.ToIntList().Distinct().Contains(x.ServiceLineId) && !x.IsDeleted).Select(x => new ServiceLineVM() { ServiceLineId = x.ServiceLineId, ServiceName = x.ServiceName, IsSelected = team1.Contains(x.ServiceLineId) }).ToList();
                    BlueDataVM.ServiceLineTeam2 = this._serviceLineRepo.Table.Where(x => serviceIds.ServiceLineTeam2.ToIntList().Distinct().Contains(x.ServiceLineId) && !x.IsDeleted).Select(x => new ServiceLineVM() { ServiceLineId = x.ServiceLineId, ServiceName = x.ServiceName, IsSelected = team2.Contains(x.ServiceLineId) }).ToList();
                }

                if (BlueDataVM.IsEms.HasValue && BlueDataVM.IsEms.Value)
                    BlueDataVM.OrganizationData = GetHosplitalAddressObject(BlueDataVM.OrganizationIdFk);
                BlueDataVM.LastKnownWellStr = BlueDataVM.LastKnownWell?.ToTimezoneFromUtc("Eastern Standard Time").ToString("yyyy-MM-dd hh:mm:ss tt");
                BlueDataVM.DobStr = BlueDataVM.Dob?.ToTimezoneFromUtc("Eastern Standard Time").ToString("yyyy-MM-dd hh:mm:ss tt");
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
                codeBlue.LastKnownWell = DateTime.Parse(codeBlue.LastKnownWellStr).ToUniversalTimeZone(); ;
            }
            if (codeBlue != null && !string.IsNullOrEmpty(codeBlue.DobStr) && !string.IsNullOrWhiteSpace(codeBlue.DobStr))
            {
                codeBlue.Dob = DateTime.Parse(codeBlue.DobStr).ToUniversalTimeZone(); ;
            }
            if (codeBlue.CodeBlueId > 0)
            {
                var row = new CodeBlue();
                object fieldValue = new();
                if (codeBlue.FieldDataType != "file")
                {
                    var fieldName = string.Empty;
                    if (codeBlue.FieldName == "HPI" || codeBlue.FieldName == "DOB")
                    {
                        fieldName = codeBlue.FieldName.ToCapitalize();
                    }
                    else
                    {
                        fieldName = codeBlue.FieldName;
                    }

                    fieldValue = codeBlue.GetPropertyValueByName(fieldName);

                    row = this._dbContext.LoadStoredProcedure("md_UpdateCodes")
                                             .WithSqlParam("codeName", UCLEnums.Blue.ToString())
                                             .WithSqlParam("fieldName", codeBlue.FieldName)
                                             .WithSqlParam("fieldValue", fieldValue)
                                             .WithSqlParam("codeId", codeBlue.CodeBlueId)
                                             .WithSqlParam("modifiedBy", ApplicationSettings.UserId)
                                             .ExecuteStoredProc<CodeBlue>().FirstOrDefault();
                    this._dbContext.Log(row.getChangedPropertyObject(fieldName), ActivityLogTableEnums.CodeBlues.ToString(), row.CodeBlueId, ActivityLogActionEnums.Update.ToInt());
                    var userIds = this._BlueCodeGroupMembersRepo.Table.Where(x => x.BlueCodeIdFk == row.CodeBlueId).Select(x => x.UserIdFk).ToList();
                    var userUniqueIds = this._userRepo.Table.Where(x => userIds.Contains(x.UserId)).Select(x => x.UserUniqueId).Distinct().ToList();
                    var superAdmins = this._userRepo.Table.Where(x => x.IsInGroup && !x.IsDeleted).Select(x => x.UserUniqueId).ToList();
                    userUniqueIds.AddRange(superAdmins);
                    var loggedUser = this._userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && !x.IsDeleted).Select(x => x.UserUniqueId).FirstOrDefault();
                    userUniqueIds.Add(loggedUser);
                    if (codeBlue.FieldDataType == "date")
                    {
                        fieldValue = codeBlue.GetPropertyValueByName(fieldName + "Str");
                    }
                    var notification = new PushNotificationVM()
                    {
                        Id = row.CodeBlueId,
                        OrgId = row.OrganizationIdFk,
                        FieldName = codeBlue.FieldName,
                        FieldDataType = codeBlue.FieldDataType,
                        FieldValue = fieldValue,
                        UserChannelSid = userUniqueIds.Distinct().ToList(),
                        From = AuthorEnums.Blue.ToString(),
                        Msg = (row.IsEms != null && row.IsEms.Value ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription()) + " Blue From is Changed",
                        RouteLink1 = RouteEnums.CodeBlueForm.ToDescription(), // "/Home/Inhouse%20Codes/code-strok-form",
                        RouteLink2 = RouteEnums.EMSForms.ToDescription(), // RouteEnums.EMSForms.ToDescription(),
                    };

                    _communicationService.pushNotification(notification);
                    return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Updated Successfully" };
                }
                else
                {

                    row = this._codeBlueRepo.Table.Where(x => x.CodeBlueId == codeBlue.CodeBlueId && !x.IsDeleted).FirstOrDefault();

                    row.ModifiedBy = codeBlue.ModifiedBy;
                    row.ModifiedDate = DateTime.UtcNow;
                    row.IsDeleted = false;

                    if (codeBlue.Attachment != null && codeBlue.Attachment.Count > 0)
                    {
                        var RootPath = this._RootPath + "/Organizations"; //this._RootPath + "/Organizations";
                        string FileRoot = null;
                        List<string> Attachments = new();
                        FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == row.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
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
                        FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == row.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
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

                        FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == row.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
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
                        string path = this._RootPath + codeBlue.AttachmentsFolderRoot;
                        if (Directory.Exists(path))
                        {
                            DirectoryInfo AttachFiles = new DirectoryInfo(path);
                            codeBlue.AttachmentsPath = new List<string>();
                            foreach (var item in AttachFiles.GetFiles())
                            {
                                codeBlue.AttachmentsPath.Add(codeBlue.AttachmentsFolderRoot + "/" + item.Name);
                            }
                        }
                    }
                    if (codeBlue.VideoFolderRoot != null)
                    {
                        row.Video = codeBlue.VideoFolderRoot;
                        var path = this._RootPath + codeBlue.VideoFolderRoot;
                        if (Directory.Exists(path))
                        {
                            DirectoryInfo VideoFiles = new DirectoryInfo(path);
                            codeBlue.VideosPath = new List<string>();
                            foreach (var item in VideoFiles.GetFiles())
                            {
                                codeBlue.VideosPath.Add(codeBlue.VideoFolderRoot + "/" + item.Name);
                            }
                        }
                    }
                    if (codeBlue.AudioFolderRoot != null)
                    {
                        row.Audio = codeBlue.AudioFolderRoot;
                        string path = this._RootPath + codeBlue.AudioFolderRoot;
                        if (Directory.Exists(path))
                        {
                            DirectoryInfo AudioFiles = new DirectoryInfo(path);
                            codeBlue.AudiosPath = new List<string>();
                            foreach (var item in AudioFiles.GetFiles())
                            {
                                codeBlue.AudiosPath.Add(codeBlue.AudioFolderRoot + "/" + item.Name);
                            }
                        }
                    }

                    this._codeBlueRepo.Update(row);
                    this._dbContext.Log(row, ActivityLogTableEnums.CodeBlues.ToString(), row.CodeBlueId, ActivityLogActionEnums.FileUpload.ToInt());
                    fieldValue = new { videosPath = codeBlue.VideosPath, audiosPath = codeBlue.AudiosPath, attachmentsPath = codeBlue.AttachmentsPath };
                    var userIds = this._BlueCodeGroupMembersRepo.Table.Where(x => x.BlueCodeIdFk == row.CodeBlueId).Select(x => x.UserIdFk).ToList();
                    var userUniqueIds = this._userRepo.Table.Where(x => userIds.Contains(x.UserId)).Select(x => x.UserUniqueId).Distinct().ToList();
                    var superAdmins = this._userRepo.Table.Where(x => x.IsInGroup && !x.IsDeleted).Select(x => x.UserUniqueId).ToList();
                    userUniqueIds.AddRange(superAdmins);
                    var loggedUser = this._userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && !x.IsDeleted).Select(x => x.UserUniqueId).FirstOrDefault();
                    userUniqueIds.Add(loggedUser);

                    var notification = new PushNotificationVM()
                    {
                        Id = row.CodeBlueId,
                        OrgId = row.OrganizationIdFk,
                        FieldName = codeBlue.FieldName,
                        FieldDataType = codeBlue.FieldDataType,
                        FieldValue = fieldValue,
                        UserChannelSid = userUniqueIds,
                        From = AuthorEnums.Blue.ToString(),
                        Msg = (row.IsEms != null && row.IsEms.Value ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription()) + " Blue From is Changed",
                        RouteLink1 = RouteEnums.CodeBlueForm.ToDescription(), // "/Home/Inhouse%20Codes/code-strok-form",
                        RouteLink2 = RouteEnums.EMSForms.ToDescription(), // RouteEnums.EMSForms.ToDescription(),
                    };

                    _communicationService.pushNotification(notification);
                    return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Updated Successfully" };
                }

            }
            else
            {
                if (codeBlue.OrganizationIdFk > 0)
                {
                    string IsEMS = codeBlue.IsEms.HasValue && codeBlue.IsEms.Value ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription();
                    var DefaultServiceLineTeam = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == codeBlue.OrganizationIdFk && x.CodeIdFk == UCLEnums.Blue.ToInt() && x.Type == IsEMS && x.IsActive.HasValue && x.IsActive.Value && !x.IsDeleted).Select(x => x.DefaultServiceLineTeam).FirstOrDefault();
                    if (DefaultServiceLineTeam != null && DefaultServiceLineTeam != "")
                    {
                        var DefaultServiceLineIds = DefaultServiceLineTeam.ToIntList();

                        codeBlue.CreatedDate = DateTime.UtcNow;
                        codeBlue.FamilyContactNumber = codeBlue.FamilyContactNumber != null && codeBlue.FamilyContactNumber != "" && codeBlue.FamilyContactNumber != "(___) ___-____" ? codeBlue.FamilyContactNumber : "";
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
                        this._dbContext.Log(blue, ActivityLogTableEnums.CodeBlues.ToString(), blue.CodeBlueId, ActivityLogActionEnums.Create.ToInt());
                        var codeService = new CodesServiceLinesMapping()
                        {
                            OrganizationIdFk = blue.OrganizationIdFk,
                            CodeIdFk = UCLEnums.Blue.ToInt(),
                            DefaultServiceLineIdFk = DefaultServiceLineTeam,
                            ActiveCodeId = blue.CodeBlueId,
                            ActiveCodeName = UCLEnums.Blue.ToString()
                        };
                        this._codesServiceLinesMappingRepo.Insert(codeService);

                        //var UserChannelSid = (from us in this._userSchedulesRepo.Table
                        //                      join u in this._userRepo.Table on us.UserIdFk equals u.UserId
                        //                      where DefaultServiceLineIds.Contains(us.ServiceLineIdFk.Value) && us.ScheduleDateStart <= DateTime.UtcNow && us.ScheduleDateEnd >= DateTime.UtcNow && !us.IsDeleted && !u.IsDeleted
                        //                      select new { u.UserUniqueId, u.UserId }).ToList();
                        //var superAdmins = this._userRepo.Table.Where(x => x.IsInGroup && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId }).ToList();
                        //UserChannelSid.AddRange(superAdmins);
                        //var loggedUser = this._userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId }).FirstOrDefault();
                        //UserChannelSid.Add(loggedUser);
                        //var conversationChannelAttributes = JsonConvert.SerializeObject(new Dictionary<string, Object>()
                        //            {
                        //                {ChannelAttributeEnums.ChannelType.ToString(), ChannelTypeEnums.EMS.ToString()},
                        //                {ChannelAttributeEnums.CodeType.ToString(), ChannelTypeEnums.Blue.ToString()},
                        //                {ChannelAttributeEnums.BlueId.ToString(), blue.CodeBlueId}
                        //            }, Formatting.Indented);
                        //List<CodeBlueGroupMember> ACodeGroupMembers = new List<CodeBlueGroupMember>();
                        //if (UserChannelSid != null && UserChannelSid.Count > 0)
                        //{
                        //    string uniqueName = DateTime.Now.ToString("yyyyMMddHHmmssffff") + ApplicationSettings.UserId.ToString();
                        //    string friendlyName = blue.IsEms.HasValue && blue.IsEms.Value ? $"{UCLEnums.EMS.ToDescription()} {UCLEnums.Blue.ToString()} {blue.CodeBlueId}" : $"{UCLEnums.InhouseCode.ToDescription()} {UCLEnums.Blue.ToString()} {blue.CodeBlueId}";
                        //    var channel = _communicationService.createConversationChannel(friendlyName, uniqueName, conversationChannelAttributes);
                        //    blue.ChannelSid = channel.Sid;
                        //    this._codeBlueRepo.Update(blue);
                        //    UserChannelSid = UserChannelSid.Distinct().ToList();
                        //    foreach (var item in UserChannelSid)
                        //    {
                        //        try
                        //        {
                        //            var codeGroupMember = new CodeBlueGroupMember()
                        //            {
                        //                //ChannelSid = channel.Sid,
                        //                UserIdFk = item.UserId,
                        //                BlueCodeIdFk = blue.CodeBlueId,
                        //                // ActiveCodeName = UCLEnums.Blue.ToString(),
                        //                IsAcknowledge = false,
                        //                CreatedBy = ApplicationSettings.UserId,
                        //                CreatedDate = DateTime.UtcNow,
                        //                IsDeleted = false
                        //            };
                        //            ACodeGroupMembers.Add(codeGroupMember);
                        //            _communicationService.addNewUserToConversationChannel(channel.Sid, item.UserUniqueId);
                        //        }
                        //        catch (Exception ex)
                        //        {
                        //            //ElmahExtensions.RiseError(ex);
                        //        }
                        //    }
                        //    this._BlueCodeGroupMembersRepo.Insert(ACodeGroupMembers);
                        //    var msg = new ConversationMessageVM();
                        //    msg.channelSid = channel.Sid;
                        //    msg.author = "System";
                        //    msg.attributes = "";
                        //    msg.body = $"<strong> {(codeBlue.IsEms.HasValue && codeBlue.IsEms.Value ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription())} {UCLEnums.Blue.ToString()} </strong></br></br>";
                        //    if (codeBlue.PatientName != null && codeBlue.PatientName != "")
                        //        msg.body += $"<strong>Patient Name: </strong> {codeBlue.PatientName} </br>";
                        //    msg.body += $"<strong>Dob: </strong> {codeBlue.Dob:MM-dd-yyyy} </br>";
                        //    msg.body += $"<strong>Last Well Known: </strong> {codeBlue.LastKnownWell:MM-dd-yyyy hh:mm tt} </br>";
                        //    if (codeBlue.ChiefComplant != null && codeBlue.ChiefComplant != "")
                        //        msg.body += $"<strong>Chief Complaint: </strong> {codeBlue.ChiefComplant} </br>";
                        //    if (codeBlue.Hpi != null && codeBlue.Hpi != "")
                        //        msg.body += $"<strong>Hpi: </strong> {codeBlue.Hpi} </br>";
                        //    var sendMsg = _communicationService.sendPushNotification(msg);

                        //    var showAllAccessUsers = this._dbContext.LoadStoredProcedure("md_getUsersOfComponentAccess")
                        //                       .WithSqlParam("@componentName", "Show All EMS,Show All Active Codes,Show Graphs,Show EMS,Show Active Codes")
                        //                       .WithSqlParam("@orgId", blue.OrganizationIdFk)
                        //                       .ExecuteStoredProc<RegisterCredentialVM>().Select(x => new { x.UserUniqueId, x.UserId }).Distinct().ToList();
                        //    UserChannelSid.AddRange(showAllAccessUsers);
                        //    var notification = new PushNotificationVM()
                        //    {
                        //        Id = blue.CodeBlueId,
                        //        OrgId = blue.OrganizationIdFk,
                        //        UserChannelSid = UserChannelSid.Select(x => x.UserUniqueId).Distinct().ToList(),
                        //        From = AuthorEnums.Blue.ToString(),
                        //        Msg = (codeBlue.IsEms.HasValue && codeBlue.IsEms.Value ? UCLEnums.EMS.ToDescription() : "{UCLEnums.InhouseCode.ToDescription()}) + " Code Blue is update",
                        //        RouteLink3 = RouteEnums.ActiveEMS.ToDescription(),
                        //        RouteLink4 = RouteEnums.Dashboard.ToDescription(),
                        //        RouteLink5 = RouteEnums.InhouseCodeGrid.ToDescription()
                        //    };

                        //    _communicationService.pushNotification(notification);
                        //}
                        return GetBlueDataById(blue.CodeBlueId);
                    }
                    return new BaseResponse() { Status = HttpStatusCode.NotAcceptable, Message = "There is no Service Line in this organization related to Code Blue" };
                }
                return new BaseResponse() { Status = HttpStatusCode.NotAcceptable, Message = "Organization is not selected" };
            }
        }

        public BaseResponse CreateBlueGroup(CodeBlueVM codeBlue)
        {

            var DefaultServiceLineIds = codeBlue.DefaultServiceLineIds.ToIntList();
            bool usersFound = false;
            string errorMsg = "";

            var UserChannelSid = (from us in this._userSchedulesRepo.Table
                                  join u in this._userRepo.Table on us.UserIdFk equals u.UserId
                                  where DefaultServiceLineIds.Contains(us.ServiceLineIdFk.Value) && us.ScheduleDateStart <= DateTime.UtcNow && us.ScheduleDateEnd >= DateTime.UtcNow && !us.IsDeleted && !u.IsDeleted
                                  select new { u.UserUniqueId, u.UserId, ServiceLineIdFk = us.ServiceLineIdFk.Value }).Distinct().ToList();


            if (UserChannelSid.Count > 0)
            {
                usersFound = true;
                var defaultExist = DefaultServiceLineIds.Select(d => new { ServiceLineId = d, IsExist = UserChannelSid.Select(x => x.ServiceLineIdFk).Contains(d) }).ToList();
                if (defaultExist.Count > 0)
                {
                    if (!defaultExist.Select(x => x.IsExist).All(x => x == true))
                    {
                        var notExisted = defaultExist.Where(x => !x.IsExist).Select(x => x.ServiceLineId).ToList();
                        DefaultServiceLineIds.RemoveAll(d => notExisted.Contains(d));
                        var services = this._serviceLineRepo.Table.Where(x => notExisted.Contains(x.ServiceLineId)).Select(x => new { x.ServiceLineId, x.ServiceName }).ToList();
                        foreach (var item in services)
                        {
                            errorMsg += Environment.NewLine + "No OnCall user found in " + item.ServiceName;
                        }
                    }
                }
                else
                {
                    var services = this._serviceLineRepo.Table.Where(x => DefaultServiceLineIds.Contains(x.ServiceLineId)).Select(x => new { x.ServiceLineId, x.ServiceName }).ToList();
                    foreach (var item in services)
                    {
                        errorMsg += Environment.NewLine + "No OnCall user found in " + item.ServiceName;
                    }
                    DefaultServiceLineIds = new List<int>();
                }

                if (DefaultServiceLineIds.Count > 0)
                {
                    var codeService = new CodesServiceLinesMapping()
                    {
                        OrganizationIdFk = codeBlue.OrganizationIdFk,
                        CodeIdFk = UCLEnums.Blue.ToInt(),
                        DefaultServiceLineIdFk = string.Join(",", DefaultServiceLineIds),
                        ActiveCodeId = codeBlue.CodeBlueId,
                        ActiveCodeName = UCLEnums.Blue.ToString()
                    };
                    this._codesServiceLinesMappingRepo.Insert(codeService);
                }
            }
            else
            {
                DefaultServiceLineIds = new List<int>();
            }

            var superAdmins = this._userRepo.Table.Where(x => x.IsInGroup && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId, ServiceLineIdFk = 0 }).ToList();
            UserChannelSid.AddRange(superAdmins);
            var loggedUser = this._userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId, ServiceLineIdFk = 0 }).FirstOrDefault();
            UserChannelSid.Add(loggedUser);
            var conversationChannelAttributes = JsonConvert.SerializeObject(new Dictionary<string, Object>()
                                    {
                                        {ChannelAttributeEnums.ChannelType.ToString(), ChannelTypeEnums.EMS.ToString()},
                                        {ChannelAttributeEnums.CodeType.ToString(), ChannelTypeEnums.Blue.ToString()},
                                        {ChannelAttributeEnums.BlueId.ToString(), codeBlue.CodeBlueId}
                                    }, Formatting.Indented);
            List<CodeBlueGroupMember> ACodeGroupMembers = new List<CodeBlueGroupMember>();
            if (UserChannelSid != null && UserChannelSid.Count > 0)
            {
                string uniqueName = DateTime.Now.ToString("yyyyMMddHHmmssffff") + ApplicationSettings.UserId.ToString();
                string friendlyName = codeBlue.IsEms.HasValue && codeBlue.IsEms.Value ? $"{UCLEnums.EMS.ToDescription()} {UCLEnums.Blue.ToString()} {codeBlue.CodeBlueId}" : $"{UCLEnums.InhouseCode.ToDescription()} {UCLEnums.Blue.ToString()} {codeBlue.CodeBlueId}";
                var channel = _communicationService.createConversationChannel(friendlyName, uniqueName, conversationChannelAttributes);
                var Blue = this._codeBlueRepo.Table.Where(x => x.CodeBlueId == codeBlue.CodeBlueId && x.IsActive == true && !x.IsDeleted).FirstOrDefault();
                if (Blue != null)
                {
                    Blue.ChannelSid = channel.Sid;
                    this._codeBlueRepo.Update(Blue);
                }
                UserChannelSid = UserChannelSid.Distinct().ToList();
                foreach (var item in UserChannelSid)
                {
                    try
                    {
                        var codeGroupMember = new CodeBlueGroupMember()
                        {
                            UserIdFk = item.UserId,
                            BlueCodeIdFk = Blue.CodeBlueId,
                            //ActiveCodeName = UCLEnums.Blue.ToString(),
                            IsAcknowledge = false,
                            CreatedBy = ApplicationSettings.UserId,
                            CreatedDate = DateTime.UtcNow,
                            IsDeleted = false
                        };
                        ACodeGroupMembers.Add(codeGroupMember);
                        _communicationService.addNewUserToConversationChannel(channel.Sid, item.UserUniqueId);
                    }
                    catch (Exception ex)
                    {

                    }
                }
                this._BlueCodeGroupMembersRepo.Insert(ACodeGroupMembers);
                var msg = new ConversationMessageVM();
                msg.channelSid = channel.Sid;
                msg.author = "System";
                msg.attributes = "";
                msg.body = $"<strong> {(codeBlue.IsEms.HasValue && codeBlue.IsEms.Value ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription())} {UCLEnums.Blue.ToString()} </strong></br></br>";
                if (codeBlue.PatientName != null && codeBlue.PatientName != "")
                    msg.body += $"<strong>Patient Name: </strong> {codeBlue.PatientName} </br>";
                if (codeBlue.Dob != null)
                    msg.body += $"<strong>Dob: </strong> {codeBlue.Dob:MM-dd-yyyy} </br>";
                if (codeBlue.LastKnownWell != null)
                    msg.body += $"<strong>Last Well Known: </strong> {codeBlue.LastKnownWell:MM-dd-yyyy hh:mm tt} </br>";
                if (codeBlue.ChiefComplant != null && codeBlue.ChiefComplant != "")
                    msg.body += $"<strong>Chief Complaint: </strong> {codeBlue.ChiefComplant} </br>";
                if (codeBlue.Hpi != null && codeBlue.Hpi != "")
                    msg.body += $"<strong>Hpi: </strong> {codeBlue.Hpi} </br>";

                var sendMsg = _communicationService.sendPushNotification(msg);

                var showAllAccessUsers = this._dbContext.LoadStoredProcedure("md_getUsersOfComponentAccess")
                                    .WithSqlParam("@componentName", "Show All EMS,Show All Active Codes,Show Graphs,Show EMS,Show Active Codes")
                                    .WithSqlParam("@orgId", Blue.OrganizationIdFk)
                                    .ExecuteStoredProc<RegisterCredentialVM>().Select(x => new { x.UserUniqueId, x.UserId, ServiceLineIdFk = 0 }).Distinct().ToList();
                UserChannelSid.AddRange(showAllAccessUsers);
                var notification = new PushNotificationVM()
                {
                    Id = Blue.CodeBlueId,
                    OrgId = Blue.OrganizationIdFk,
                    UserChannelSid = UserChannelSid.Select(x => x.UserUniqueId).Distinct().ToList(),
                    From = AuthorEnums.Blue.ToString(),
                    Msg = (codeBlue.IsEms.HasValue && codeBlue.IsEms.Value ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription()) + " Blue is update",
                    RouteLink3 = RouteEnums.ActiveEMS.ToDescription(), // RouteEnums.ActiveEMS.ToDescription(),
                    RouteLink4 = RouteEnums.Dashboard.ToDescription(),
                    RouteLink5 = RouteEnums.InhouseCodeGrid.ToDescription(), // RouteEnums.InhouseCodeGrid.ToDescription()
                };

                _communicationService.pushNotification(notification);
            }

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = errorMsg, Body = new { serviceLineUsersFound = usersFound, DefaultServiceLineIds, ServiceLineTeam1Ids = new List<int>(), ServiceLineTeam2Ids = new List<int>() } };
        }

        public BaseResponse UpdateBlueGroupMembers(CodeBlueVM codeBlue)
        {

            if (codeBlue.DefaultServiceLineIds == null || codeBlue.DefaultServiceLineIds == "")
            {
                string IsEMS = codeBlue.IsEms.HasValue && codeBlue.IsEms.Value ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription();
                codeBlue.DefaultServiceLineIds = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == codeBlue.OrganizationIdFk && x.CodeIdFk == UCLEnums.Blue.ToInt() && x.Type == IsEMS && x.IsActive.HasValue && x.IsActive.Value && !x.IsDeleted).Select(x => x.DefaultServiceLineTeam).FirstOrDefault();
            }

            if (codeBlue.DefaultServiceLineIds != null && codeBlue.DefaultServiceLineIds != "")
            {
                bool userNotFound = false;

                string msg = "";

                var DefaultServiceLineIds = codeBlue.DefaultServiceLineIds.ToIntList();
                var ServiceLineTeam1Ids = codeBlue.ServiceLineTeam1Ids.ToIntList();
                var ServiceLineTeam2Ids = codeBlue.ServiceLineTeam2Ids.ToIntList();

                var UserChannelSid = (from us in this._userSchedulesRepo.Table
                                      join u in this._userRepo.Table on us.UserIdFk equals u.UserId
                                      where (DefaultServiceLineIds.Contains(us.ServiceLineIdFk.Value) || ServiceLineTeam1Ids.Contains(us.ServiceLineIdFk.Value) || ServiceLineTeam2Ids.Contains(us.ServiceLineIdFk.Value)) && us.ScheduleDateStart <= DateTime.UtcNow && us.ScheduleDateEnd >= DateTime.UtcNow && !us.IsDeleted && !u.IsDeleted
                                      select new { u.UserUniqueId, u.UserId, ServiceLineIdFk = us.ServiceLineIdFk.Value }).Distinct().ToList();

                var codeServiceMapping = this._codesServiceLinesMappingRepo.Table.Where(x => x.OrganizationIdFk == codeBlue.OrganizationIdFk && x.CodeIdFk == UCLEnums.Blue.ToInt() && x.ActiveCodeId == codeBlue.CodeBlueId).ToList();
                if (codeServiceMapping.Count > 0)
                    this._codesServiceLinesMappingRepo.DeleteRange(codeServiceMapping);

                if (UserChannelSid.Count > 0)
                {
                    userNotFound = true;
                    var defaultExist = DefaultServiceLineIds.Select(d => new { ServiceLineId = d, IsExist = UserChannelSid.Select(x => x.ServiceLineIdFk).Contains(d) }).ToList();
                    if (defaultExist.Count > 0)
                    {
                        if (!defaultExist.Select(x => x.IsExist).All(x => x == true))
                        {
                            var notExisted = defaultExist.Where(x => !x.IsExist).Select(x => x.ServiceLineId).ToList();
                            DefaultServiceLineIds.RemoveAll(d => notExisted.Contains(d));
                            var services = this._serviceLineRepo.Table.Where(x => notExisted.Contains(x.ServiceLineId)).Select(x => new { x.ServiceLineId, x.ServiceName }).ToList();
                            foreach (var item in services)
                            {
                                msg += Environment.NewLine + "No OnCall user found in " + item.ServiceName;
                            }
                        }
                    }
                    else
                    {
                        var services = this._serviceLineRepo.Table.Where(x => DefaultServiceLineIds.Contains(x.ServiceLineId)).Select(x => new { x.ServiceLineId, x.ServiceName }).ToList();
                        foreach (var item in services)
                        {
                            msg += Environment.NewLine + "No OnCall user found in " + item.ServiceName;
                        }
                        DefaultServiceLineIds = new List<int>();
                    }

                    var serviceLineTeam1Exist = ServiceLineTeam1Ids.Select(d => new { ServiceLineId = d, IsExist = UserChannelSid.Select(x => x.ServiceLineIdFk).Contains(d) }).ToList();
                    if (serviceLineTeam1Exist.Count > 0)
                    {
                        if (!serviceLineTeam1Exist.Select(x => x.IsExist).All(x => x == true))
                        {
                            var notExisted = serviceLineTeam1Exist.Where(x => !x.IsExist).Select(x => x.ServiceLineId).ToList();
                            ServiceLineTeam1Ids.RemoveAll(d => notExisted.Contains(d));
                            var services = this._serviceLineRepo.Table.Where(x => notExisted.Contains(x.ServiceLineId)).Select(x => new { x.ServiceLineId, x.ServiceName }).ToList();
                            foreach (var item in services)
                            {
                                msg += Environment.NewLine + "No OnCall user found in " + item.ServiceName;
                            }
                        }
                    }
                    else
                    {
                        var services = this._serviceLineRepo.Table.Where(x => ServiceLineTeam1Ids.Contains(x.ServiceLineId)).Select(x => new { x.ServiceLineId, x.ServiceName }).ToList();
                        foreach (var item in services)
                        {
                            msg += Environment.NewLine + "No OnCall user found in " + item.ServiceName;
                        }
                        ServiceLineTeam1Ids = new List<int>();
                    }


                    var serviceLineTeam2Exist = ServiceLineTeam2Ids.Select(d => new { ServiceLineId = d, IsExist = UserChannelSid.Select(x => x.ServiceLineIdFk).Contains(d) }).ToList();
                    if (serviceLineTeam2Exist.Count > 0)
                    {
                        if (!serviceLineTeam2Exist.Select(x => x.IsExist).All(x => x == true))
                        {
                            var notExisted = serviceLineTeam2Exist.Where(x => !x.IsExist).Select(x => x.ServiceLineId).ToList();
                            ServiceLineTeam2Ids.RemoveAll(d => notExisted.Contains(d));
                            var services = this._serviceLineRepo.Table.Where(x => notExisted.Contains(x.ServiceLineId)).Select(x => new { x.ServiceLineId, x.ServiceName }).ToList();
                            foreach (var item in services)
                            {
                                msg += Environment.NewLine + "No OnCall user found in " + item.ServiceName;
                            }
                        }
                    }
                    else
                    {
                        var services = this._serviceLineRepo.Table.Where(x => ServiceLineTeam2Ids.Contains(x.ServiceLineId)).Select(x => new { x.ServiceLineId, x.ServiceName }).ToList();
                        foreach (var item in services)
                        {
                            msg += Environment.NewLine + "No OnCall user found in " + item.ServiceName;
                        }
                        ServiceLineTeam2Ids = new List<int>();
                    }



                    if (DefaultServiceLineIds.Count > 0 || ServiceLineTeam1Ids.Count > 0 || ServiceLineTeam2Ids.Count > 0)
                    {
                        var codeService = new CodesServiceLinesMapping()
                        {
                            OrganizationIdFk = codeBlue.OrganizationIdFk,
                            CodeIdFk = UCLEnums.Blue.ToInt(),
                            DefaultServiceLineIdFk = DefaultServiceLineIds.Count > 0 ? string.Join(",", DefaultServiceLineIds) : null,
                            ServiceLineId1Fk = ServiceLineTeam1Ids.Count > 0 ? string.Join(",", ServiceLineTeam1Ids) : null,
                            ServiceLineId2Fk = ServiceLineTeam2Ids.Count > 0 ? string.Join(",", ServiceLineTeam2Ids) : null,
                            ActiveCodeId = codeBlue.CodeBlueId,
                            ActiveCodeName = UCLEnums.Blue.ToString()
                        };
                        this._codesServiceLinesMappingRepo.Insert(codeService);
                    }
                }
                else
                {
                    DefaultServiceLineIds = new List<int>();
                    ServiceLineTeam1Ids = new List<int>();
                    ServiceLineTeam2Ids = new List<int>();
                }


                var superAdmins = this._userRepo.Table.Where(x => x.IsInGroup && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId, ServiceLineIdFk = 0 }).ToList();
                UserChannelSid.AddRange(superAdmins);
                var loggedInUser = this._userRepo.Table.Where(x => x.UserId == ApplicationSettings.UserId && !x.IsDeleted).Select(x => new { x.UserUniqueId, x.UserId, ServiceLineIdFk = 0 }).FirstOrDefault();
                UserChannelSid.Add(loggedInUser);

                if (codeBlue.ChannelSid != null && codeBlue.ChannelSid != "")
                {
                    var channelSid = codeBlue.ChannelSid; //channel.Select(x => x.ChannelSid).FirstOrDefault();

                    var groupMembers = this._BlueCodeGroupMembersRepo.Table.Where(x => x.BlueCodeIdFk == codeBlue.CodeBlueId).ToList();
                    this._BlueCodeGroupMembersRepo.DeleteRange(groupMembers);
                    //this._BlueCodeGroupMembersRepo.DeleteRange(channel);
                    bool isDeleted = _communicationService.DeleteUserToConversationChannel(channelSid);
                    List<CodeBlueGroupMember> ACodeGroupMembers = new List<CodeBlueGroupMember>();
                    foreach (var item in UserChannelSid.Distinct())
                    {
                        try
                        {
                            var codeGroupMember = new CodeBlueGroupMember()
                            {
                                UserIdFk = item.UserId,
                                BlueCodeIdFk = codeBlue.CodeBlueId,
                                //ActiveCodeName = UCLEnums.Blue.ToString(),
                                IsAcknowledge = false,
                                CreatedBy = ApplicationSettings.UserId,
                                CreatedDate = DateTime.UtcNow,
                                IsDeleted = false
                            };
                            ACodeGroupMembers.Add(codeGroupMember);
                            _communicationService.addNewUserToConversationChannel(channelSid, item.UserUniqueId);
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
                    Id = codeBlue.CodeBlueId,
                    OrgId = codeBlue.OrganizationIdFk,
                    UserChannelSid = UserChannelSid.Select(x => x.UserUniqueId).Distinct().ToList(),
                    From = AuthorEnums.Blue.ToString(),
                    Msg = (codeBlue.IsEms.HasValue && codeBlue.IsEms.Value ? UCLEnums.EMS.ToDescription() : UCLEnums.InhouseCode.ToDescription()) + " Blue From is Changed",
                    RouteLink1 = RouteEnums.CodeBlueForm.ToDescription(), // "/Home/Inhouse%20Codes/code-strok-form",
                    RouteLink2 = RouteEnums.EMSForms.ToDescription(), // RouteEnums.EMSForms.ToDescription(),
                };

                _communicationService.pushNotification(notification);

                return new BaseResponse() { Status = HttpStatusCode.OK, Message = msg, Body = new { serviceLineUsersFound = userNotFound, DefaultServiceLineIds, ServiceLineTeam1Ids, ServiceLineTeam2Ids } };
            }
            return new BaseResponse() { Status = HttpStatusCode.NotModified, Message = "Default Service Lines Required" };
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

            var rowsEffected = this._dbContext.Database.ExecuteSqlRaw(sql, parameters);
            if (rowsEffected > 0)
            {
                var userIds = this._dbContext.LoadStoredProcedure("md_getAllActiveCodesGroupUserIds")
                                                .WithSqlParam("@codeName", UCLEnums.Blue.ToString())
                                                .WithSqlParam("@codeId", blueId)
                                                .ExecuteStoredProc<CodeStrokeVM>();

                var notification = new PushNotificationVM()
                {
                    Id = blueId,
                    OrgId = userIds.Select(x => x.OrganizationIdFk).FirstOrDefault(),
                    Type = "ChannelStatusChanged",
                    ChannelIsActive = status,
                    ChannelSid = userIds.Select(x => x.ChannelSid).FirstOrDefault(),
                    UserChannelSid = userIds.Select(x => x.UserUniqueId).Distinct().ToList(),
                    From = UCLEnums.Blue.ToString(),
                    Msg = UCLEnums.Blue.ToString() + " is " + (status ? "Activated" : "Inactivated"),
                    RouteLink3 = RouteEnums.ActiveEMS.ToDescription(),
                    RouteLink4 = RouteEnums.Dashboard.ToDescription(),
                    RouteLink5 = RouteEnums.InhouseCodeGrid.ToDescription()
                };

                _communicationService.pushNotification(notification);
            }

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

            var orgDataList = new List<dynamic>();
            foreach (var item in activeEMS.Select(x => x.OrganizationIdFk).Distinct().ToList())
            {
                var orgData = GetHosplitalAddressObject(item);
                if (orgData != null)
                    orgDataList.Add(orgData);
            }
            foreach (var item in activeEMS)
            {
                item.OrganizationData = orgDataList.Where(x => x.OrganizationId == item.OrganizationIdFk).FirstOrDefault();

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

                item.LastKnownWellStr = item.LastKnownWell?.ToTimezoneFromUtc("Eastern Standard Time").ToString("yyyy-MM-dd hh:mm:ss tt");
                //item.OrganizationData = orgData;
                item.DobStr = item.Dob?.ToTimezoneFromUtc("Eastern Standard Time").ToString("yyyy-MM-dd hh:mm:ss tt");
                item.CreatedDateStr = item.CreatedDate.ToString("MM-dd-yyyy hh:mm:ss tt");
                item.GenderTitle = _controlListDetailsRepo.Table.Where(g => g.ControlListDetailId == item.Gender).Select(g => g.Title).FirstOrDefault();
                item.BloodThinnersTitle.AddRange(_controlListDetailsRepo.Table.Where(b => item.BloodThinners.ToIntList().Contains(b.ControlListDetailId)).Select(b => new { Id = b.ControlListDetailId, b.Title }).ToList());
            }
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Returned", Body = activeEMS };
        }

        #endregion

        #region Inhouse Code Settings


        public BaseResponse GetAllInhouseCodeFeilds()
        {
            var feilds = this._InhouseCodeFeilds.Table.Where(x => !x.IsDeleted).ToList();
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Feilds returend", Body = feilds };
        }

        public BaseResponse GetInhouseCodeFeildsForOrg(int OrgId, string codeName)
        {
            var InhouseCodeFields = this._InhouseCodeFeilds.Table.Where(x => !x.IsDeleted).ToList();
            var qry = $"SELECT InhouseCodesFieldIdFk, IsRequired, SortOrder, IsShowInTable from OrganizationCode{codeName}Fields WHERE OrganizationIdFk = {OrgId} and IsDeleted = 0";

            var selectedInhouseCodeFields = this._dbContext.LoadSQLQuery(qry).ExecuteStoredProc<OrganizationCodeStrokeField>(); //this._orgInhouseCodeFeilds.Table.Where(x => x.OrganizationIdFk == OrgId && !x.IsDeleted).Select(x => new { x.InhouseCodesFieldIdFk, x.IsRequired, x.SortOrder }).ToList();

            var InhouseCodeFieldVM = AutoMapperHelper.MapList<InhouseCodesField, InhouseCodeFeildsVM>(InhouseCodeFields);

            foreach (var item in InhouseCodeFieldVM)
            {
                if (selectedInhouseCodeFields.Select(x => x.InhouseCodesFieldIdFk).Contains(item.InhouseCodesFieldId))
                {
                    item.IsRequired = selectedInhouseCodeFields.Where(x => x.InhouseCodesFieldIdFk == item.InhouseCodesFieldId).Select(s => s.IsRequired).FirstOrDefault();
                    item.IsShowInTable = selectedInhouseCodeFields.Where(x => x.InhouseCodesFieldIdFk == item.InhouseCodesFieldId).Select(s => s.IsShowInTable).FirstOrDefault();
                    item.SortOrder = selectedInhouseCodeFields.Where(x => x.InhouseCodesFieldIdFk == item.InhouseCodesFieldId).Select(s => s.SortOrder.Value).FirstOrDefault();
                    item.IsSelected = true;
                }
                else
                {
                    item.IsSelected = false;
                }
            }

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Found", Body = InhouseCodeFieldVM };
        }

        //public BaseResponse GetInhouseCodeFormFieldByOrgId(int OrgId, string codeName)
        //{
        //    var formFields = (from cf in this._InhouseCodeFeilds.Table
        //                      join ocf in this._orgInhouseCodeFeilds.Table on cf.InhouseCodesFieldId equals ocf.InhouseCodesFieldIdFk
        //                      where ocf.OrganizationIdFk == OrgId && !ocf.IsDeleted && !cf.IsDeleted
        //                      select new
        //                      {
        //                          cf.FieldLabel,
        //                          cf.FieldName,
        //                          cf.FieldType,
        //                          cf.FieldData,
        //                          cf.FieldDataType,
        //                          cf.FieldDataLength,
        //                          ocf.IsRequired
        //                      }).Distinct().ToList();
        //    return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Found", Body = formFields };
        //}

        #endregion

        #region Organization InhouseCode Fields

        public BaseResponse AddOrUpdateOrgCodeStrokeFeilds(List<OrgCodeStrokeFeildsVM> orgInhouseCodeFields)
        {

            if (orgInhouseCodeFields.Count == 1 && orgInhouseCodeFields.Select(x => x.InhouseCodesFieldIdFk).FirstOrDefault() == 0)
            {
                var toBeDeletedRows = this._orgCodeStrokeFeilds.Table.Where(x => x.OrganizationIdFk == orgInhouseCodeFields.Select(x => x.OrganizationIdFk).FirstOrDefault() && !x.IsDeleted).ToList();
                toBeDeletedRows.ForEach(x => { x.ModifiedBy = ApplicationSettings.UserId; x.ModifiedDate = DateTime.UtcNow; x.IsDeleted = true; });
                this._orgCodeStrokeFeilds.Update(toBeDeletedRows);
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Deleted Successfully" };
            }
            else
            {
                var duplicateObj = orgInhouseCodeFields.Select(x => new { x.OrganizationIdFk, x.InhouseCodesFieldIdFk }).ToList();

                var alreadyExistFields = this._orgCodeStrokeFeilds.Table.Where(x => duplicateObj.Select(y => y.InhouseCodesFieldIdFk).Contains(x.InhouseCodesFieldIdFk) && duplicateObj.Select(y => y.OrganizationIdFk).Contains(x.OrganizationIdFk) && !x.IsDeleted).Distinct().ToList();

                var objsNeedToUpdate = alreadyExistFields.Where(x => duplicateObj.Select(c => c.InhouseCodesFieldIdFk).Contains(x.InhouseCodesFieldIdFk)).ToList();

                if (objsNeedToUpdate.Count > 0)
                {
                    foreach (var item in objsNeedToUpdate)
                    {
                        item.SortOrder = orgInhouseCodeFields.Where(x => x.InhouseCodesFieldIdFk == item.InhouseCodesFieldIdFk).Select(x => x.SortOrder).FirstOrDefault();
                        item.IsRequired = orgInhouseCodeFields.Where(x => x.InhouseCodesFieldIdFk == item.InhouseCodesFieldIdFk).Select(x => x.IsRequired).FirstOrDefault();
                        item.IsShowInTable = orgInhouseCodeFields.Where(x => x.InhouseCodesFieldIdFk == item.InhouseCodesFieldIdFk).Select(x => x.IsShowInTable).FirstOrDefault();
                    }
                    this._orgCodeStrokeFeilds.Update(objsNeedToUpdate);
                }

                duplicateObj.RemoveAll(r => alreadyExistFields.Select(x => x.InhouseCodesFieldIdFk).Contains(r.InhouseCodesFieldIdFk));

                var orgInhouseCodes = AutoMapperHelper.MapList<OrgCodeStrokeFeildsVM, OrganizationCodeStrokeField>(orgInhouseCodeFields.Where(x => duplicateObj.Select(c => c.InhouseCodesFieldIdFk).Contains(x.InhouseCodesFieldIdFk)).ToList());

                if (orgInhouseCodes.Count > 0)
                {
                    this._orgCodeStrokeFeilds.Insert(orgInhouseCodes);
                }

                alreadyExistFields = this._orgCodeStrokeFeilds.Table.Where(x => duplicateObj.Select(y => y.InhouseCodesFieldIdFk).Contains(x.InhouseCodesFieldIdFk) && duplicateObj.Select(y => y.OrganizationIdFk).Contains(x.OrganizationIdFk) && !x.IsDeleted).Distinct().ToList();
                duplicateObj.RemoveAll(r => alreadyExistFields.Select(x => x.InhouseCodesFieldIdFk).Contains(r.InhouseCodesFieldIdFk));

                var deletedOnes = this._orgCodeStrokeFeilds.Table.Where(x => !(orgInhouseCodeFields.Select(y => y.InhouseCodesFieldIdFk).Contains(x.InhouseCodesFieldIdFk)) && orgInhouseCodeFields.Select(y => y.OrganizationIdFk).Contains(x.OrganizationIdFk) && !x.IsDeleted).ToList();

                int? ModifiedBy = orgInhouseCodeFields.Select(x => x.ModifiedBy).FirstOrDefault();

                if (deletedOnes.Count > 0)
                {
                    deletedOnes.ForEach(x => { x.ModifiedBy = ModifiedBy; x.ModifiedDate = DateTime.UtcNow; x.IsDeleted = true; });
                    this._orgCodeStrokeFeilds.Update(deletedOnes);
                }

                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Saved Successfully" };
            }
        }

        public BaseResponse AddOrUpdateOrgCodeSTEMIFeilds(List<OrgCodeSTEMIFeildsVM> orgInhouseCodeFields)
        {

            if (orgInhouseCodeFields.Count == 1 && orgInhouseCodeFields.Select(x => x.InhouseCodesFieldIdFk).FirstOrDefault() == 0)
            {
                var toBeDeletedRows = this._orgCodeSTEMIFeilds.Table.Where(x => x.OrganizationIdFk == orgInhouseCodeFields.Select(x => x.OrganizationIdFk).FirstOrDefault() && !x.IsDeleted).ToList();
                toBeDeletedRows.ForEach(x => { x.ModifiedBy = ApplicationSettings.UserId; x.ModifiedDate = DateTime.UtcNow; x.IsDeleted = true; });
                this._orgCodeSTEMIFeilds.Update(toBeDeletedRows);
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Deleted Successfully" };
            }
            else
            {
                var duplicateObj = orgInhouseCodeFields.Select(x => new { x.OrganizationIdFk, x.InhouseCodesFieldIdFk }).ToList();

                var alreadyExistFields = this._orgCodeSTEMIFeilds.Table.Where(x => duplicateObj.Select(y => y.InhouseCodesFieldIdFk).Contains(x.InhouseCodesFieldIdFk) && duplicateObj.Select(y => y.OrganizationIdFk).Contains(x.OrganizationIdFk) && !x.IsDeleted).Distinct().ToList();

                var objsNeedToUpdate = alreadyExistFields.Where(x => duplicateObj.Select(c => c.InhouseCodesFieldIdFk).Contains(x.InhouseCodesFieldIdFk)).ToList();

                if (objsNeedToUpdate.Count > 0)
                {
                    foreach (var item in objsNeedToUpdate)
                    {
                        item.SortOrder = orgInhouseCodeFields.Where(x => x.InhouseCodesFieldIdFk == item.InhouseCodesFieldIdFk).Select(x => x.SortOrder).FirstOrDefault();
                        item.IsRequired = orgInhouseCodeFields.Where(x => x.InhouseCodesFieldIdFk == item.InhouseCodesFieldIdFk).Select(x => x.IsRequired).FirstOrDefault();
                        item.IsShowInTable = orgInhouseCodeFields.Where(x => x.InhouseCodesFieldIdFk == item.InhouseCodesFieldIdFk).Select(x => x.IsShowInTable).FirstOrDefault();
                    }
                    this._orgCodeSTEMIFeilds.Update(objsNeedToUpdate);
                }

                duplicateObj.RemoveAll(r => alreadyExistFields.Select(x => x.InhouseCodesFieldIdFk).Contains(r.InhouseCodesFieldIdFk));

                var orgInhouseCodes = AutoMapperHelper.MapList<OrgCodeSTEMIFeildsVM, OrganizationCodeStemifield>(orgInhouseCodeFields.Where(x => duplicateObj.Select(c => c.InhouseCodesFieldIdFk).Contains(x.InhouseCodesFieldIdFk)).ToList());

                if (orgInhouseCodes.Count > 0)
                {
                    this._orgCodeSTEMIFeilds.Insert(orgInhouseCodes);
                }

                alreadyExistFields = this._orgCodeSTEMIFeilds.Table.Where(x => duplicateObj.Select(y => y.InhouseCodesFieldIdFk).Contains(x.InhouseCodesFieldIdFk) && duplicateObj.Select(y => y.OrganizationIdFk).Contains(x.OrganizationIdFk) && !x.IsDeleted).Distinct().ToList();
                duplicateObj.RemoveAll(r => alreadyExistFields.Select(x => x.InhouseCodesFieldIdFk).Contains(r.InhouseCodesFieldIdFk));

                var deletedOnes = this._orgCodeSTEMIFeilds.Table.Where(x => !(orgInhouseCodeFields.Select(y => y.InhouseCodesFieldIdFk).Contains(x.InhouseCodesFieldIdFk)) && orgInhouseCodeFields.Select(y => y.OrganizationIdFk).Contains(x.OrganizationIdFk) && !x.IsDeleted).ToList();

                int? ModifiedBy = orgInhouseCodeFields.Select(x => x.ModifiedBy).FirstOrDefault();

                if (deletedOnes.Count > 0)
                {
                    deletedOnes.ForEach(x => { x.ModifiedBy = ModifiedBy; x.ModifiedDate = DateTime.UtcNow; x.IsDeleted = true; });
                    this._orgCodeSTEMIFeilds.Update(deletedOnes);
                }

                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Saved Successfully" };
            }
        }

        public BaseResponse AddOrUpdateOrgCodeSepsisFeilds(List<OrgCodeSepsisFeildsVM> orgInhouseCodeFields)
        {

            if (orgInhouseCodeFields.Count == 1 && orgInhouseCodeFields.Select(x => x.InhouseCodesFieldIdFk).FirstOrDefault() == 0)
            {
                var toBeDeletedRows = this._orgCodeSepsisFeilds.Table.Where(x => x.OrganizationIdFk == orgInhouseCodeFields.Select(x => x.OrganizationIdFk).FirstOrDefault() && !x.IsDeleted).ToList();
                toBeDeletedRows.ForEach(x => { x.ModifiedBy = ApplicationSettings.UserId; x.ModifiedDate = DateTime.UtcNow; x.IsDeleted = true; });
                this._orgCodeSepsisFeilds.Update(toBeDeletedRows);
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Deleted Successfully" };
            }
            else
            {
                var duplicateObj = orgInhouseCodeFields.Select(x => new { x.OrganizationIdFk, x.InhouseCodesFieldIdFk }).ToList();

                var alreadyExistFields = this._orgCodeSepsisFeilds.Table.Where(x => duplicateObj.Select(y => y.InhouseCodesFieldIdFk).Contains(x.InhouseCodesFieldIdFk) && duplicateObj.Select(y => y.OrganizationIdFk).Contains(x.OrganizationIdFk) && !x.IsDeleted).Distinct().ToList();

                var objsNeedToUpdate = alreadyExistFields.Where(x => duplicateObj.Select(c => c.InhouseCodesFieldIdFk).Contains(x.InhouseCodesFieldIdFk)).ToList();

                if (objsNeedToUpdate.Count > 0)
                {
                    foreach (var item in objsNeedToUpdate)
                    {
                        item.SortOrder = orgInhouseCodeFields.Where(x => x.InhouseCodesFieldIdFk == item.InhouseCodesFieldIdFk).Select(x => x.SortOrder).FirstOrDefault();
                        item.IsRequired = orgInhouseCodeFields.Where(x => x.InhouseCodesFieldIdFk == item.InhouseCodesFieldIdFk).Select(x => x.IsRequired).FirstOrDefault();
                        item.IsShowInTable = orgInhouseCodeFields.Where(x => x.InhouseCodesFieldIdFk == item.InhouseCodesFieldIdFk).Select(x => x.IsShowInTable).FirstOrDefault();
                    }
                    this._orgCodeSepsisFeilds.Update(objsNeedToUpdate);
                }

                duplicateObj.RemoveAll(r => alreadyExistFields.Select(x => x.InhouseCodesFieldIdFk).Contains(r.InhouseCodesFieldIdFk));

                var orgInhouseCodes = AutoMapperHelper.MapList<OrgCodeSepsisFeildsVM, OrganizationCodeSepsisField>(orgInhouseCodeFields.Where(x => duplicateObj.Select(c => c.InhouseCodesFieldIdFk).Contains(x.InhouseCodesFieldIdFk)).ToList());

                if (orgInhouseCodes.Count > 0)
                {
                    this._orgCodeSepsisFeilds.Insert(orgInhouseCodes);
                }

                alreadyExistFields = this._orgCodeSepsisFeilds.Table.Where(x => duplicateObj.Select(y => y.InhouseCodesFieldIdFk).Contains(x.InhouseCodesFieldIdFk) && duplicateObj.Select(y => y.OrganizationIdFk).Contains(x.OrganizationIdFk) && !x.IsDeleted).Distinct().ToList();
                duplicateObj.RemoveAll(r => alreadyExistFields.Select(x => x.InhouseCodesFieldIdFk).Contains(r.InhouseCodesFieldIdFk));

                var deletedOnes = this._orgCodeSepsisFeilds.Table.Where(x => !(orgInhouseCodeFields.Select(y => y.InhouseCodesFieldIdFk).Contains(x.InhouseCodesFieldIdFk)) && orgInhouseCodeFields.Select(y => y.OrganizationIdFk).Contains(x.OrganizationIdFk) && !x.IsDeleted).ToList();

                int? ModifiedBy = orgInhouseCodeFields.Select(x => x.ModifiedBy).FirstOrDefault();

                if (deletedOnes.Count > 0)
                {
                    deletedOnes.ForEach(x => { x.ModifiedBy = ModifiedBy; x.ModifiedDate = DateTime.UtcNow; x.IsDeleted = true; });
                    this._orgCodeSepsisFeilds.Update(deletedOnes);
                }

                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Saved Successfully" };
            }
        }

        public BaseResponse AddOrUpdateOrgCodeTraumaFeilds(List<OrgCodeTraumaFeildsVM> orgInhouseCodeFields)
        {

            if (orgInhouseCodeFields.Count == 1 && orgInhouseCodeFields.Select(x => x.InhouseCodesFieldIdFk).FirstOrDefault() == 0)
            {
                var toBeDeletedRows = this._orgCodeTraumaFeilds.Table.Where(x => x.OrganizationIdFk == orgInhouseCodeFields.Select(x => x.OrganizationIdFk).FirstOrDefault() && !x.IsDeleted).ToList();
                toBeDeletedRows.ForEach(x => { x.ModifiedBy = ApplicationSettings.UserId; x.ModifiedDate = DateTime.UtcNow; x.IsDeleted = true; });
                this._orgCodeTraumaFeilds.Update(toBeDeletedRows);
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Deleted Successfully" };
            }
            else
            {
                var duplicateObj = orgInhouseCodeFields.Select(x => new { x.OrganizationIdFk, x.InhouseCodesFieldIdFk }).ToList();

                var alreadyExistFields = this._orgCodeTraumaFeilds.Table.Where(x => duplicateObj.Select(y => y.InhouseCodesFieldIdFk).Contains(x.InhouseCodesFieldIdFk) && duplicateObj.Select(y => y.OrganizationIdFk).Contains(x.OrganizationIdFk) && !x.IsDeleted).Distinct().ToList();

                var objsNeedToUpdate = alreadyExistFields.Where(x => duplicateObj.Select(c => c.InhouseCodesFieldIdFk).Contains(x.InhouseCodesFieldIdFk)).ToList();

                if (objsNeedToUpdate.Count > 0)
                {
                    foreach (var item in objsNeedToUpdate)
                    {
                        item.SortOrder = orgInhouseCodeFields.Where(x => x.InhouseCodesFieldIdFk == item.InhouseCodesFieldIdFk).Select(x => x.SortOrder).FirstOrDefault();
                        item.IsRequired = orgInhouseCodeFields.Where(x => x.InhouseCodesFieldIdFk == item.InhouseCodesFieldIdFk).Select(x => x.IsRequired).FirstOrDefault();
                        item.IsShowInTable = orgInhouseCodeFields.Where(x => x.InhouseCodesFieldIdFk == item.InhouseCodesFieldIdFk).Select(x => x.IsShowInTable).FirstOrDefault();
                    }
                    this._orgCodeTraumaFeilds.Update(objsNeedToUpdate);
                }

                duplicateObj.RemoveAll(r => alreadyExistFields.Select(x => x.InhouseCodesFieldIdFk).Contains(r.InhouseCodesFieldIdFk));

                var orgInhouseCodes = AutoMapperHelper.MapList<OrgCodeTraumaFeildsVM, OrganizationCodeTraumaField>(orgInhouseCodeFields.Where(x => duplicateObj.Select(c => c.InhouseCodesFieldIdFk).Contains(x.InhouseCodesFieldIdFk)).ToList());

                if (orgInhouseCodes.Count > 0)
                {
                    this._orgCodeTraumaFeilds.Insert(orgInhouseCodes);
                }

                alreadyExistFields = this._orgCodeTraumaFeilds.Table.Where(x => duplicateObj.Select(y => y.InhouseCodesFieldIdFk).Contains(x.InhouseCodesFieldIdFk) && duplicateObj.Select(y => y.OrganizationIdFk).Contains(x.OrganizationIdFk) && !x.IsDeleted).Distinct().ToList();
                duplicateObj.RemoveAll(r => alreadyExistFields.Select(x => x.InhouseCodesFieldIdFk).Contains(r.InhouseCodesFieldIdFk));

                var deletedOnes = this._orgCodeTraumaFeilds.Table.Where(x => !(orgInhouseCodeFields.Select(y => y.InhouseCodesFieldIdFk).Contains(x.InhouseCodesFieldIdFk)) && orgInhouseCodeFields.Select(y => y.OrganizationIdFk).Contains(x.OrganizationIdFk) && !x.IsDeleted).ToList();

                int? ModifiedBy = orgInhouseCodeFields.Select(x => x.ModifiedBy).FirstOrDefault();

                if (deletedOnes.Count > 0)
                {
                    deletedOnes.ForEach(x => { x.ModifiedBy = ModifiedBy; x.ModifiedDate = DateTime.UtcNow; x.IsDeleted = true; });
                    this._orgCodeTraumaFeilds.Update(deletedOnes);
                }

                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Saved Successfully" };
            }
        }

        public BaseResponse AddOrUpdateOrgCodeBlueFeilds(List<OrgCodeBlueFeildsVM> orgInhouseCodeFields)
        {

            if (orgInhouseCodeFields.Count == 1 && orgInhouseCodeFields.Select(x => x.InhouseCodesFieldIdFk).FirstOrDefault() == 0)
            {
                var toBeDeletedRows = this._orgCodeBlueFeilds.Table.Where(x => x.OrganizationIdFk == orgInhouseCodeFields.Select(x => x.OrganizationIdFk).FirstOrDefault() && !x.IsDeleted).ToList();
                toBeDeletedRows.ForEach(x => { x.ModifiedBy = ApplicationSettings.UserId; x.ModifiedDate = DateTime.UtcNow; x.IsDeleted = true; });
                this._orgCodeBlueFeilds.Update(toBeDeletedRows);
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Deleted Successfully" };
            }
            else
            {
                var duplicateObj = orgInhouseCodeFields.Select(x => new { x.OrganizationIdFk, x.InhouseCodesFieldIdFk }).ToList();

                var alreadyExistFields = this._orgCodeBlueFeilds.Table.Where(x => duplicateObj.Select(y => y.InhouseCodesFieldIdFk).Contains(x.InhouseCodesFieldIdFk) && duplicateObj.Select(y => y.OrganizationIdFk).Contains(x.OrganizationIdFk) && !x.IsDeleted).Distinct().ToList();

                var objsNeedToUpdate = alreadyExistFields.Where(x => duplicateObj.Select(c => c.InhouseCodesFieldIdFk).Contains(x.InhouseCodesFieldIdFk)).ToList();

                if (objsNeedToUpdate.Count > 0)
                {
                    foreach (var item in objsNeedToUpdate)
                    {
                        item.SortOrder = orgInhouseCodeFields.Where(x => x.InhouseCodesFieldIdFk == item.InhouseCodesFieldIdFk).Select(x => x.SortOrder).FirstOrDefault();
                        item.IsRequired = orgInhouseCodeFields.Where(x => x.InhouseCodesFieldIdFk == item.InhouseCodesFieldIdFk).Select(x => x.IsRequired).FirstOrDefault();
                        item.IsShowInTable = orgInhouseCodeFields.Where(x => x.InhouseCodesFieldIdFk == item.InhouseCodesFieldIdFk).Select(x => x.IsShowInTable).FirstOrDefault();
                    }
                    this._orgCodeBlueFeilds.Update(objsNeedToUpdate);
                }

                duplicateObj.RemoveAll(r => alreadyExistFields.Select(x => x.InhouseCodesFieldIdFk).Contains(r.InhouseCodesFieldIdFk));

                var orgInhouseCodes = AutoMapperHelper.MapList<OrgCodeBlueFeildsVM, OrganizationCodeBlueField>(orgInhouseCodeFields.Where(x => duplicateObj.Select(c => c.InhouseCodesFieldIdFk).Contains(x.InhouseCodesFieldIdFk)).ToList());

                if (orgInhouseCodes.Count > 0)
                {
                    this._orgCodeBlueFeilds.Insert(orgInhouseCodes);
                }

                alreadyExistFields = this._orgCodeBlueFeilds.Table.Where(x => duplicateObj.Select(y => y.InhouseCodesFieldIdFk).Contains(x.InhouseCodesFieldIdFk) && duplicateObj.Select(y => y.OrganizationIdFk).Contains(x.OrganizationIdFk) && !x.IsDeleted).Distinct().ToList();
                duplicateObj.RemoveAll(r => alreadyExistFields.Select(x => x.InhouseCodesFieldIdFk).Contains(r.InhouseCodesFieldIdFk));

                var deletedOnes = this._orgCodeBlueFeilds.Table.Where(x => !(orgInhouseCodeFields.Select(y => y.InhouseCodesFieldIdFk).Contains(x.InhouseCodesFieldIdFk)) && orgInhouseCodeFields.Select(y => y.OrganizationIdFk).Contains(x.OrganizationIdFk) && !x.IsDeleted).ToList();

                int? ModifiedBy = orgInhouseCodeFields.Select(x => x.ModifiedBy).FirstOrDefault();

                if (deletedOnes.Count > 0)
                {
                    deletedOnes.ForEach(x => { x.ModifiedBy = ModifiedBy; x.ModifiedDate = DateTime.UtcNow; x.IsDeleted = true; });
                    this._orgCodeBlueFeilds.Update(deletedOnes);
                }

                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Saved Successfully" };
            }
        }


        public BaseResponse GetInhouseCodeFormByOrgId(int orgId, string codeName)
        {
            var InhouseCodeFields = _dbContext.LoadStoredProcedure("md_getInhouseCodeFormByOrgId")
                                .WithSqlParam("@OrgId", orgId)
                                .WithSqlParam("@codeName", codeName)
                                .WithSqlParam("@IsEMSUser", ApplicationSettings.isEMS)
                                .ExecuteStoredProc<InhouseCodeFeildsVM>();

            var NonSortedFields = InhouseCodeFields.Where(x => x.SortOrder == 0).ToList();
            InhouseCodeFields.RemoveAll(x => x.SortOrder == 0);
            InhouseCodeFields.AddRange(NonSortedFields);


            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Returned", Body = InhouseCodeFields };
        }

        public BaseResponse GetInhouseCodeTableFeilds(int orgId, string codeName)
        {
            var InhouseCodeFields = _dbContext.LoadStoredProcedure("md_getShowInTableColumnsForInHouseCodes")
                                .WithSqlParam("@OrgId", orgId)
                                .WithSqlParam("@codeName", codeName)
                                .WithSqlParam("@IsEMSUser", ApplicationSettings.isEMS)
                                .ExecuteStoredProc<InhouseCodeFeildsVM>().FirstOrDefault();
            if (InhouseCodeFields != null && InhouseCodeFields.FieldName != null)
            {
                var fieldNames = InhouseCodeFields.FieldName.Split(",").ToList();
                if (fieldNames.IndexOf(" Gender") > -1)
                    fieldNames[fieldNames.IndexOf(" Gender")] = "GenderTitle";
                if (fieldNames.IndexOf(" BloodThinners") > -1)
                    fieldNames[fieldNames.IndexOf(" BloodThinners")] = "BloodThinnersTitle";

                InhouseCodeFields.FieldName = string.Join(",", fieldNames);

            }
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Returned", Body = InhouseCodeFields };
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

        #region Apis For Mobile App

        public BaseResponse GetAllCodesData(ActiveCodeVM activeCode)
        {
            var objList = this._dbContext.LoadStoredProcedure("md_getAllCodes")
                            .WithSqlParam("@status", activeCode.Status)
                            .WithSqlParam("@organizationId", activeCode.OrganizationIdFk)
                            .WithSqlParam("@currentUserId", ApplicationSettings.UserId)
                            .WithSqlParam("@IsSuperAdmin", ApplicationSettings.isSuperAdmin)
                            .WithSqlParam("@showAll", activeCode.showAllActiveCodes)
                            .WithSqlParam("@page", activeCode.PageNumber)
                            .WithSqlParam("@size", activeCode.Rows)
                            .WithSqlParam("@filterCol", activeCode.FilterCol)
                            .WithSqlParam("@filterVal", activeCode.FilterVal)
                           .ExecuteStoredProc_ToDictionary();

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = objList.Count > 0 ? "Data Returned" : "No Data Found", Body = objList };
        }

        #endregion

    }
}
