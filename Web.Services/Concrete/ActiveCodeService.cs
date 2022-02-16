using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
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
        IConfiguration _config;
        public ActiveCodeService(RAQ_DbContext dbContext,
            IConfiguration config,
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
            IRepository<CodeTrauma> codeTrumaRepo)
        {
            this._config = config;
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
        }

        #region Active Code

        public BaseResponse GetActivatedCodesByOrgId(int orgId)
        {
            //var codes = (from c in this._activeCodeRepo.Table
            //             join ucl in this._controlListDetailsRepo.Table on c.CodeIdFk equals ucl.ControlListDetailId
            //             where c.OrganizationIdFk == orgId && !c.IsDeleted
            //             select new ActiveCodeVM()
            //             {

            //                 ActiveCodeId = c.ActiveCodeId,
            //                 OrganizationIdFk = c.OrganizationIdFk,
            //                 ActiveCodeName = ucl.Title,
            //                 CodeIdFk = c.CodeIdFk,
            //                 ServiceLineIds = c.ServiceLineIds,
            //                 //serviceLines = this._serviceLineRepo.Table.Where(x => !x.IsDeleted && c.ServiceLineIds.ToIntList().Contains(x.ServiceLineId)).Select(x => new ServiceLineVM() { ServiceLineId = x.ServiceLineId, ServiceName = x.ServiceName }).ToList()

            //             }).Distinct().ToList();

            var codes = this._dbContext.LoadStoredProcedure("raq_getActivatedCodesForOrg")
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
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "No Active Code Found", Body = codes };
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
                    row.ServiceLineIds = item.ServiceLineIds;
                    row.ModifiedBy = item.ModifiedBy;
                    row.ModifiedDate = DateTime.Now;
                    row.IsDeleted = false;
                    update.Add(row);
                }
                else
                {
                    item.CreatedDate = DateTime.UtcNow;
                    var row = AutoMapperHelper.MapSingleRow<ActiveCodeVM, ActiveCode>(item);
                    insert.Add(row);
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


        #endregion

        #region Delete File

        public BaseResponse DeleteFile(FilesVM files)
        {
            if (files.CodeType == "Stroke")
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
            else if (files.CodeType == "Sepsis")
            {
                var rootPath = this._codeSepsisRepo.Table.Where(x => x.CodeSepsisId == files.Id).Select(files.Type).FirstOrDefault();
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
                        From = AuthorEnums.Sepsis.ToString(),
                        Msg = "Sepsis From is Changed",
                        RouteLink = "/Home/Activate%20Code/code-sepsis-form"
                    };

                    _communication.pushNotification(notification);

                }
            }
            else if (files.CodeType == "STEMI")
            {
                var rootPath = this._codeSTEMIRepo.Table.Where(x => x.CodeStemiid == files.Id).Select(files.Type).FirstOrDefault();
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
                        From = AuthorEnums.STEMI.ToString(),
                        Msg = "STEMI From is Changed",
                        RouteLink = "/Home/Activate%20Code/code-STEMI-form"
                    };

                    _communication.pushNotification(notification);

                }
            }
            else if (files.CodeType == "Trauma")
            {
                var rootPath = this._codeTrumaRepo.Table.Where(x => x.CodeTraumaId == files.Id).Select(files.Type).FirstOrDefault();
                string path = _environment.WebRootFileProvider.GetFileInfo(rootPath+'/'+files.FileName)?.PhysicalPath;
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
                        From = AuthorEnums.Trauma.ToString(),
                        Msg = "Trauma From is Changed",
                        RouteLink = "/Home/Activate%20Code/code-trauma-form"
                    };

                    _communication.pushNotification(notification);

                }
            }

           
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "File Deleted Successfully" };
        }

        #endregion


        #region Code Stroke

        public BaseResponse GetAllStrokeCode(int orgId)
        {
            var strokeData = this._codeStrokeRepo.Table.Where(x => x.OrganizationIdFk == orgId && !x.IsDeleted).ToList();
            var strokeDataVM = AutoMapperHelper.MapList<CodeStroke, CodeStrokeVM>(strokeData);
            strokeDataVM.ForEach(x =>
            {
                x.AttachmentsPath = new List<string>();
                x.AudiosPath = new List<string>();
                x.VideosPath = new List<string>();

                if (!string.IsNullOrEmpty(x.Attachments) && !string.IsNullOrWhiteSpace(x.Attachments))
                {
                    string path = _environment.WebRootFileProvider.GetFileInfo(x.Attachments)?.PhysicalPath;
                    if (Directory.Exists(path))
                    {
                        DirectoryInfo AttachFiles = new DirectoryInfo(path);
                        foreach (var item in AttachFiles.GetFiles())
                        {
                            x.AttachmentsPath.Add(x.Attachments + "/" + item.Name);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(x.Audio) && !string.IsNullOrWhiteSpace(x.Audio))
                {
                    string path = _environment.WebRootFileProvider.GetFileInfo(x.Audio)?.PhysicalPath;
                    if (Directory.Exists(path))
                    {
                        DirectoryInfo AudioFiles = new DirectoryInfo(path);
                        foreach (var item in AudioFiles.GetFiles())
                        {
                            x.AudiosPath.Add(x.Audio + "/" + item.Name);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(x.Video) && !string.IsNullOrWhiteSpace(x.Video))
                {
                    var path = _environment.WebRootFileProvider.GetFileInfo(x.Video)?.PhysicalPath; //.GetFileInfo(x.Video);//?.PhysicalPath;
                    if (Directory.Exists(path))
                    {
                        DirectoryInfo VideoFiles = new DirectoryInfo(path);
                        foreach (var item in VideoFiles.GetFiles())
                        {
                            x.VideosPath.Add(x.Video + "/" + item.Name);
                        }
                    }
                }
                x.GenderTitle = _controlListDetailsRepo.Table.Where(g => g.ControlListDetailId == x.Gender).Select(g => g.Title).FirstOrDefault();
                x.BloodThinnersTitle = _controlListDetailsRepo.Table.Where(b => b.ControlListDetailId == x.BloodThinners).Select(b => b.Title).FirstOrDefault();
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

                if (!string.IsNullOrEmpty(StrokeDataVM.Attachments) && !string.IsNullOrWhiteSpace(StrokeDataVM.Attachments))
                {
                    string path = _environment.WebRootFileProvider.GetFileInfo(StrokeDataVM.Attachments)?.PhysicalPath;
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
                    string path = _environment.WebRootFileProvider.GetFileInfo(StrokeDataVM.Audio)?.PhysicalPath;
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
                    var path = _environment.WebRootFileProvider.GetFileInfo(StrokeDataVM.Video)?.PhysicalPath; //.GetFileInfo(StrokeDataVM.Video);//?.PhysicalPath;
                    if (Directory.Exists(path))
                    {
                        DirectoryInfo VideoFiles = new DirectoryInfo(path);
                        foreach (var item in VideoFiles.GetFiles())
                        {
                            StrokeDataVM.VideosPath.Add(StrokeDataVM.Video + "/" + item.Name);
                        }
                    }
                }
                StrokeDataVM.GenderTitle = _controlListDetailsRepo.Table.Where(g => g.ControlListDetailId == StrokeDataVM.Gender).Select(g => g.Title).FirstOrDefault();
                StrokeDataVM.BloodThinnersTitle = _controlListDetailsRepo.Table.Where(b => b.ControlListDetailId == StrokeDataVM.BloodThinners).Select(b => b.Title).FirstOrDefault();
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Found", Body = StrokeDataVM };
            }
            return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Record Not Found" };
        }

        public BaseResponse AddOrUpdateStrokeData(CodeStrokeVM codeStroke)
        {
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
                row.IsCompleted = codeStroke.IsCompleted;
                row.ModifiedBy = codeStroke.ModifiedBy;
                row.ModifiedDate = DateTime.UtcNow;
                row.IsDeleted = false;

                if (codeStroke.Attachment != null && codeStroke.Attachment.Count > 0)
                {
                    var RootPath = this._environment.WebRootPath;
                    string FileRoot = null;
                    List<string> Attachments = new();
                    FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeStroke.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                    FileRoot = Path.Combine(RootPath, FileRoot);
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, "Stroke");
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
                            if (fileExtension != null)
                            {
                                var ByteFile = Convert.FromBase64String(fileInfo[1]);
                                string FilePath = Path.Combine(FileRoot, item.FileName);
                                using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                {
                                    fs.Write(ByteFile);
                                }

                            }
                        }

                    }
                    if (FileRoot != null && FileRoot != "")
                    {
                        codeStroke.AttachmentsFolderRoot = FileRoot.Replace(RootPath, "").Replace("\\", "/");
                    }
                }
                if (codeStroke.Videos != null && codeStroke.Videos.Count > 0)
                {
                    var RootPath = this._environment.WebRootPath;
                    string FileRoot = null;
                    List<string> Attachments = new();
                    FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeStroke.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                    FileRoot = Path.Combine(RootPath, FileRoot);
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, "Stroke");
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
                            if (fileExtension != null)
                            {
                                var ByteFile = Convert.FromBase64String(fileInfo[1]);
                                string FilePath = Path.Combine(FileRoot, item.FileName);
                                using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                {
                                    fs.Write(ByteFile);
                                }

                            }
                        }

                    }
                    if (FileRoot != null && FileRoot != "")
                    {
                        codeStroke.VideoFolderRoot = FileRoot.Replace(RootPath, "").Replace("\\", "/");
                    }
                }
                if (codeStroke.Audios != null && codeStroke.Audios.Count > 0)
                {
                    var RootPath = this._environment.WebRootPath;
                    string FileRoot = null;
                    List<string> Attachments = new();

                    FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeStroke.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                    FileRoot = Path.Combine(RootPath, FileRoot);
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, "Stroke");
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
                            if (fileExtension != null)
                            {
                                var ByteFile = Convert.FromBase64String(fileInfo[1]);
                                string FilePath = Path.Combine(FileRoot, item.FileName);
                                using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                {
                                    fs.Write(ByteFile);
                                }

                            }
                        }


                    }
                    if (FileRoot != null && FileRoot != "")
                    {
                        codeStroke.AudioFolderRoot = FileRoot.Replace(RootPath, "").Replace("\\", "/");
                    }
                }

                row.Attachments = codeStroke.AttachmentsFolderRoot;
                row.Video = codeStroke.VideoFolderRoot;
                row.Audio = codeStroke.AudioFolderRoot;

                this._codeStrokeRepo.Update(row);

                if (row.IsEms!= null && row.IsEms.Value) 
                {
                    var serviceLineIds = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == row.OrganizationIdFk && x.CodeIdFk == UCLEnums.Stroke.ToInt() && !x.IsDeleted).Select(x => x.ServiceLineIds).FirstOrDefault();
                    if (serviceLineIds != null && serviceLineIds != "")
                    {
                        var UserChannelSid = (from us in this._userSchedulesRepo.Table
                                              join u in this._userRepo.Table on us.UserIdFk equals u.UserId
                                              where serviceLineIds.ToIntList().Contains(us.ServiceLineIdFk.Value) && us.ScheduleDateStart <= DateTime.Now && us.ScheduleDateEnd >= DateTime.Now && !us.IsDeleted && !u.IsDeleted
                                              select u.UserChannelSid).ToList();

                        var notification = new PushNotificationVM()
                        {
                            Id = row.CodeStrokeId,
                            OrgId = row.OrganizationIdFk,
                            UserChannelSid = UserChannelSid,
                            From = AuthorEnums.Stroke.ToString(),
                            Msg = "Stroke From is Changed",
                            RouteLink = "/Home/Activate%20Code/code-strok-form"
                        };

                        _communication.pushNotification(notification);

                    }
                }

                codeStroke.AttachmentsPath = new List<string>();
                codeStroke.AudiosPath = new List<string>();
                codeStroke.VideosPath = new List<string>();

                if (!string.IsNullOrEmpty(codeStroke.AttachmentsFolderRoot) && !string.IsNullOrWhiteSpace(codeStroke.AttachmentsFolderRoot))
                {
                    string path = _environment.WebRootFileProvider.GetFileInfo(codeStroke.AttachmentsFolderRoot)?.PhysicalPath;
                    if (Directory.Exists(path))
                    {
                        DirectoryInfo AttachFiles = new DirectoryInfo(path);
                        foreach (var item in AttachFiles.GetFiles())
                        {
                            codeStroke.AttachmentsPath.Add(codeStroke.AttachmentsFolderRoot + "/" + item.Name);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(codeStroke.Audio) && !string.IsNullOrWhiteSpace(codeStroke.AudioFolderRoot))
                {
                    string path = _environment.WebRootFileProvider.GetFileInfo(codeStroke.AudioFolderRoot)?.PhysicalPath;
                    if (Directory.Exists(path))
                    {
                        DirectoryInfo AudioFiles = new DirectoryInfo(path);
                        foreach (var item in AudioFiles.GetFiles())
                        {
                            codeStroke.AudiosPath.Add(codeStroke.AudioFolderRoot + "/" + item.Name);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(codeStroke.Video) && !string.IsNullOrWhiteSpace(codeStroke.VideoFolderRoot))
                {
                    var path = _environment.WebRootFileProvider.GetFileInfo(codeStroke.VideoFolderRoot)?.PhysicalPath;
                    if (Directory.Exists(path))
                    {
                        DirectoryInfo VideoFiles = new DirectoryInfo(path);
                        foreach (var item in VideoFiles.GetFiles())
                        {
                            codeStroke.VideosPath.Add(codeStroke.VideoFolderRoot + "/" + item.Name);
                        }
                    }
                }

                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Modified", Body = codeStroke };
            }
            else
            {

                codeStroke.CreatedDate = DateTime.UtcNow;
                var stroke = AutoMapperHelper.MapSingleRow<CodeStrokeVM, CodeStroke>(codeStroke);

                if (codeStroke.Attachment != null && codeStroke.Attachment.Count > 0)
                {
                    var RootPath = this._environment.WebRootPath;
                    string FileRoot = null;
                    List<string> Attachments = new();


                    FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeStroke.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                    FileRoot = Path.Combine(RootPath, FileRoot);
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, "Stroke");
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
                            if (fileExtension != null)
                            {
                                var ByteFile = Convert.FromBase64String(fileInfo[1]);
                                string FilePath = Path.Combine(FileRoot, item.FileName);
                                using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                {
                                    fs.Write(ByteFile);
                                }

                            }
                        }


                    }
                    if (FileRoot != null && FileRoot != "")
                    {
                        codeStroke.AttachmentsFolderRoot = FileRoot.Replace(RootPath, "").Replace("\\", "/");
                    }
                }
                if (codeStroke.Videos != null && codeStroke.Videos.Count > 0)
                {
                    var RootPath = this._environment.WebRootPath;
                    string FileRoot = null;
                    List<string> Attachments = new();


                    FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeStroke.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                    FileRoot = Path.Combine(RootPath, FileRoot);
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, "Stroke");
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
                            if (fileExtension != null)
                            {
                                var ByteFile = Convert.FromBase64String(fileInfo[1]);
                                string FilePath = Path.Combine(FileRoot, item.FileName);
                                using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                {
                                    fs.Write(ByteFile);
                                }

                            }
                        }


                    }
                    if (FileRoot != null && FileRoot != "")
                    {
                        codeStroke.VideoFolderRoot = FileRoot.Replace(RootPath, "").Replace("\\", "/");
                    }
                }
                if (codeStroke.Audios != null && codeStroke.Audios.Count > 0)
                {
                    var RootPath = this._environment.WebRootPath;
                    string FileRoot = null;
                    List<string> Attachments = new();


                    FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeStroke.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                    FileRoot = Path.Combine(RootPath, FileRoot);
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, "Stroke");
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
                            if (fileExtension != null)
                            {
                                var ByteFile = Convert.FromBase64String(fileInfo[1]);
                                string FilePath = Path.Combine(FileRoot, item.FileName);
                                using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                {
                                    fs.Write(ByteFile);
                                }

                            }
                        }


                    }
                    if (FileRoot != null && FileRoot != "")
                    {
                        codeStroke.AudioFolderRoot = FileRoot.Replace(RootPath, "").Replace("\\", "/");
                    }
                }

                stroke.Attachments = codeStroke.AttachmentsFolderRoot;
                stroke.Video = codeStroke.VideoFolderRoot;
                stroke.Audio = codeStroke.AudioFolderRoot;

                this._codeStrokeRepo.Insert(stroke);

                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Added", Body = stroke };
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


        public BaseResponse GetAllSepsisCode(int orgId)
        {
            var SepsisData = this._codeSepsisRepo.Table.Where(x => x.OrganizationIdFk == orgId && !x.IsDeleted).ToList();
            var SepsisDataVM = AutoMapperHelper.MapList<CodeSepsi, CodeSepsisVM>(SepsisData);
            SepsisDataVM.ForEach(x =>
            {
                x.AttachmentsPath = new List<string>();
                x.AudiosPath = new List<string>();
                x.VideosPath = new List<string>();

                if (!string.IsNullOrEmpty(x.Attachments) && !string.IsNullOrWhiteSpace(x.Attachments))
                {
                    string path = _environment.WebRootFileProvider.GetFileInfo(x.Attachments)?.PhysicalPath;
                    if (Directory.Exists(path))
                    {
                        DirectoryInfo AttachFiles = new DirectoryInfo(path);
                        foreach (var item in AttachFiles.GetFiles())
                        {
                            x.AttachmentsPath.Add(x.Attachments + "/" + item.Name);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(x.Audio) && !string.IsNullOrWhiteSpace(x.Audio))
                {
                    string path = _environment.WebRootFileProvider.GetFileInfo(x.Audio)?.PhysicalPath;
                    if (Directory.Exists(path))
                    {
                        DirectoryInfo AudioFiles = new DirectoryInfo(path);
                        foreach (var item in AudioFiles.GetFiles())
                        {
                            x.AudiosPath.Add(x.Audio + "/" + item.Name);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(x.Video) && !string.IsNullOrWhiteSpace(x.Video))
                {
                    var path = _environment.WebRootFileProvider.GetFileInfo(x.Video)?.PhysicalPath; //.GetFileInfo(x.Video);//?.PhysicalPath;
                    if (Directory.Exists(path))
                    {
                        DirectoryInfo VideoFiles = new DirectoryInfo(path);
                        foreach (var item in VideoFiles.GetFiles())
                        {
                            x.VideosPath.Add(x.Video + "/" + item.Name);
                        }
                    }
                }
                x.GenderTitle = _controlListDetailsRepo.Table.Where(g => g.ControlListDetailId == x.Gender).Select(g => g.Title).FirstOrDefault();
                x.BloodThinnersTitle = _controlListDetailsRepo.Table.Where(b => b.ControlListDetailId == x.BloodThinners).Select(b => b.Title).FirstOrDefault();
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

                if (!string.IsNullOrEmpty(SepsisDataVM.Attachments) && !string.IsNullOrWhiteSpace(SepsisDataVM.Attachments))
                {
                    string path = _environment.WebRootFileProvider.GetFileInfo(SepsisDataVM.Attachments)?.PhysicalPath;
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
                    string path = _environment.WebRootFileProvider.GetFileInfo(SepsisDataVM.Audio)?.PhysicalPath;
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
                    var path = _environment.WebRootFileProvider.GetFileInfo(SepsisDataVM.Video)?.PhysicalPath; //.GetFileInfo(SepsisDataVM.Video);//?.PhysicalPath;
                    if (Directory.Exists(path))
                    {
                        DirectoryInfo VideoFiles = new DirectoryInfo(path);
                        foreach (var item in VideoFiles.GetFiles())
                        {
                            SepsisDataVM.VideosPath.Add(SepsisDataVM.Video + "/" + item.Name);
                        }
                    }
                }

                SepsisDataVM.GenderTitle = _controlListDetailsRepo.Table.Where(g => g.ControlListDetailId == SepsisDataVM.Gender).Select(g => g.Title).FirstOrDefault();
                SepsisDataVM.BloodThinnersTitle = _controlListDetailsRepo.Table.Where(b => b.ControlListDetailId == SepsisDataVM.BloodThinners).Select(b => b.Title).FirstOrDefault();
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Found", Body = SepsisDataVM };
            }
            return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Record Not Found" };
        }

        public BaseResponse AddOrUpdateSepsisData(CodeSepsisVM codeSepsis)
        {
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
                row.IsCompleted = codeSepsis.IsCompleted;
                row.ModifiedBy = codeSepsis.ModifiedBy;
                row.ModifiedDate = DateTime.UtcNow;
                row.IsDeleted = false;


                if (codeSepsis.Attachment != null && codeSepsis.Attachment.Count > 0)
                {
                    var RootPath = this._environment.WebRootPath;
                    string FileRoot = null;
                    List<string> Attachments = new();

                    FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeSepsis.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                    FileRoot = Path.Combine(RootPath, FileRoot);
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, "Sepsis");
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
                            if (fileExtension != null)
                            {
                                var ByteFile = Convert.FromBase64String(fileInfo[1]);
                                string FilePath = Path.Combine(FileRoot, item.FileName);
                                using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                {
                                    fs.Write(ByteFile);
                                }

                            }
                        }


                    }
                    if (FileRoot != null && FileRoot != "")
                    {
                        codeSepsis.AttachmentsFolderRoot = FileRoot.Replace(RootPath, "").Replace("\\", "/");
                    }
                }
                if (codeSepsis.Videos != null && codeSepsis.Videos.Count > 0)
                {
                    var RootPath = this._environment.WebRootPath;
                    string FileRoot = null;
                    List<string> Attachments = new();


                    FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeSepsis.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                    FileRoot = Path.Combine(RootPath, FileRoot);
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, "Sepsis");
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
                            if (fileExtension != null)
                            {
                                var ByteFile = Convert.FromBase64String(fileInfo[1]);
                                string FilePath = Path.Combine(FileRoot, item.FileName);
                                using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                {
                                    fs.Write(ByteFile);
                                }

                            }
                        }

                    }
                    if (FileRoot != null && FileRoot != "")
                    {
                        codeSepsis.VideoFolderRoot = FileRoot.Replace(RootPath, "").Replace("\\", "/");
                    }
                }
                if (codeSepsis.Audios != null && codeSepsis.Audios.Count > 0)
                {
                    var RootPath = this._environment.WebRootPath;
                    string FileRoot = null;
                    List<string> Attachments = new();
                    //var outPath = Directory.GetCurrentDirectory();

                    FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeSepsis.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                    FileRoot = Path.Combine(RootPath, FileRoot);
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, "Sepsis");
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
                            if (fileExtension != null)
                            {
                                var ByteFile = Convert.FromBase64String(fileInfo[1]);
                                string FilePath = Path.Combine(FileRoot, item.FileName);
                                using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                {
                                    fs.Write(ByteFile);
                                }

                            }
                        }


                    }
                    if (FileRoot != null && FileRoot != "")
                    {
                        codeSepsis.AudioFolderRoot = FileRoot.Replace(RootPath, "").Replace("\\", "/");
                    }
                }

                row.Attachments = codeSepsis.AttachmentsFolderRoot;
                row.Video = codeSepsis.VideoFolderRoot;
                row.Audio = codeSepsis.AudioFolderRoot;



                this._codeSepsisRepo.Update(row);

                if (row.IsEms) 
                {
                    var serviceLineIds = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == row.OrganizationIdFk && x.CodeIdFk == UCLEnums.Sepsis.ToInt() && !x.IsDeleted).Select(x => x.ServiceLineIds).FirstOrDefault();
                    if (serviceLineIds != null && serviceLineIds != "")
                    {
                        var UserChannelSid = (from us in this._userSchedulesRepo.Table
                                              join u in this._userRepo.Table on us.UserIdFk equals u.UserId
                                              where serviceLineIds.ToIntList().Contains(us.ServiceLineIdFk.Value) && us.ScheduleDateStart <= DateTime.UtcNow && us.ScheduleDateEnd >= DateTime.UtcNow && !us.IsDeleted && !u.IsDeleted
                                              select u.UserChannelSid).ToList();

                        var notification = new PushNotificationVM()
                        {
                            Id = row.CodeSepsisId,
                            OrgId = row.OrganizationIdFk,
                            UserChannelSid = UserChannelSid,
                            From = AuthorEnums.Sepsis.ToString(),
                            Msg = "Sepsis From is Changed",
                            RouteLink = "/Home/Activate%20Code/code-sepsis-form"
                        };

                        _communication.pushNotification(notification);

                    }
                }

                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Modified", Body = row };
            }
            else
            {

                codeSepsis.CreatedDate = DateTime.UtcNow;
                var Sepsis = AutoMapperHelper.MapSingleRow<CodeSepsisVM, CodeSepsi>(codeSepsis);

                if (codeSepsis.Attachment != null && codeSepsis.Attachment.Count > 0)
                {
                    var RootPath = this._environment.WebRootPath;
                    string FileRoot = null;
                    List<string> Attachments = new();


                    FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeSepsis.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                    FileRoot = Path.Combine(RootPath, FileRoot);
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, "Sepsis");
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
                            if (fileExtension != null)
                            {
                                var ByteFile = Convert.FromBase64String(fileInfo[1]);
                                string FilePath = Path.Combine(FileRoot, item.FileName);
                                using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                {
                                    fs.Write(ByteFile);
                                }

                            }
                        }

                    }
                    if (FileRoot != null && FileRoot != "")
                    {
                        codeSepsis.AttachmentsFolderRoot = FileRoot.Replace(RootPath, "").Replace("\\", "/");
                    }
                }
                if (codeSepsis.Videos != null && codeSepsis.Videos.Count > 0)
                {
                    var RootPath = this._environment.WebRootPath;
                    string FileRoot = null;
                    List<string> Attachments = new();


                    FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeSepsis.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                    FileRoot = Path.Combine(RootPath, FileRoot);
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, "Sepsis");
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
                            if (fileExtension != null)
                            {
                                var ByteFile = Convert.FromBase64String(fileInfo[1]);
                                string FilePath = Path.Combine(FileRoot, item.FileName);
                                using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                {
                                    fs.Write(ByteFile);
                                }

                            }
                        }

                    }
                    if (FileRoot != null && FileRoot != "")
                    {
                        codeSepsis.VideoFolderRoot = FileRoot.Replace(RootPath, "").Replace("\\", "/");
                    }
                }
                if (codeSepsis.Audios != null && codeSepsis.Audios.Count > 0)
                {
                    var RootPath = this._environment.WebRootPath;
                    string FileRoot = null;
                    List<string> Attachments = new();
                    //var outPath = Directory.GetCurrentDirectory();

                    FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeSepsis.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                    FileRoot = Path.Combine(RootPath, FileRoot);
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, "Sepsis");
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
                            if (fileExtension != null)
                            {
                                var ByteFile = Convert.FromBase64String(fileInfo[1]);
                                string FilePath = Path.Combine(FileRoot, item.FileName);
                                using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                {
                                    fs.Write(ByteFile);
                                }

                            }
                        }

                    }
                    if (FileRoot != null && FileRoot != "")
                    {
                        codeSepsis.AudioFolderRoot = FileRoot.Replace(RootPath, "").Replace("\\", "/");
                    }
                }

                Sepsis.Attachments = codeSepsis.AttachmentsFolderRoot;
                Sepsis.Video = codeSepsis.VideoFolderRoot;
                Sepsis.Audio = codeSepsis.AudioFolderRoot;

                this._codeSepsisRepo.Insert(Sepsis);

                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Added", Body = Sepsis };
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

        public BaseResponse GetAllSTEMICode(int orgId)
        {
            var STEMIData = this._codeSTEMIRepo.Table.Where(x => x.OrganizationIdFk == orgId && !x.IsDeleted).ToList();
            var STEMIDataVM = AutoMapperHelper.MapList<CodeStemi, CodeSTEMIVM>(STEMIData);
            STEMIDataVM.ForEach(x =>
            {
                x.AttachmentsPath = new List<string>();
                x.AudiosPath = new List<string>();
                x.VideosPath = new List<string>();

                if (!string.IsNullOrEmpty(x.Attachments) && !string.IsNullOrWhiteSpace(x.Attachments))
                {
                    string path = _environment.WebRootFileProvider.GetFileInfo(x.Attachments)?.PhysicalPath;
                    if (Directory.Exists(path))
                    {
                        DirectoryInfo AttachFiles = new DirectoryInfo(path);
                        foreach (var item in AttachFiles.GetFiles())
                        {
                            x.AttachmentsPath.Add(x.Attachments + "/" + item.Name);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(x.Audio) && !string.IsNullOrWhiteSpace(x.Audio))
                {
                    string path = _environment.WebRootFileProvider.GetFileInfo(x.Audio)?.PhysicalPath;
                    if (Directory.Exists(path))
                    {
                        DirectoryInfo AudioFiles = new DirectoryInfo(path);
                        foreach (var item in AudioFiles.GetFiles())
                        {
                            x.AudiosPath.Add(x.Audio + "/" + item.Name);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(x.Video) && !string.IsNullOrWhiteSpace(x.Video))
                {
                    var path = _environment.WebRootFileProvider.GetFileInfo(x.Video)?.PhysicalPath; //.GetFileInfo(x.Video);//?.PhysicalPath;
                    if (Directory.Exists(path))
                    {
                        DirectoryInfo VideoFiles = new DirectoryInfo(path);
                        foreach (var item in VideoFiles.GetFiles())
                        {
                            x.VideosPath.Add(x.Video + "/" + item.Name);
                        }
                    }
                }
                x.GenderTitle = _controlListDetailsRepo.Table.Where(g => g.ControlListDetailId == x.Gender).Select(g => g.Title).FirstOrDefault();
                x.BloodThinnersTitle = _controlListDetailsRepo.Table.Where(b => b.ControlListDetailId == x.BloodThinners).Select(b => b.Title).FirstOrDefault();
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

                if (!string.IsNullOrEmpty(STEMIDataVM.Attachments) && !string.IsNullOrWhiteSpace(STEMIDataVM.Attachments))
                {
                    string path = _environment.WebRootFileProvider.GetFileInfo(STEMIDataVM.Attachments)?.PhysicalPath;
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
                    string path = _environment.WebRootFileProvider.GetFileInfo(STEMIDataVM.Audio)?.PhysicalPath;
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
                    var path = _environment.WebRootFileProvider.GetFileInfo(STEMIDataVM.Video)?.PhysicalPath; //.GetFileInfo(STEMIDataVM.Video);//?.PhysicalPath;
                    if (Directory.Exists(path))
                    {
                        DirectoryInfo VideoFiles = new DirectoryInfo(path);
                        foreach (var item in VideoFiles.GetFiles())
                        {
                            STEMIDataVM.VideosPath.Add(STEMIDataVM.Video + "/" + item.Name);
                        }
                    }
                }
                STEMIDataVM.GenderTitle = _controlListDetailsRepo.Table.Where(g => g.ControlListDetailId == STEMIDataVM.Gender).Select(g => g.Title).FirstOrDefault();
                STEMIDataVM.BloodThinnersTitle = _controlListDetailsRepo.Table.Where(b => b.ControlListDetailId == STEMIDataVM.BloodThinners).Select(b => b.Title).FirstOrDefault();
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Found", Body = STEMIDataVM };
            }
            return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Record Not Found" };
        }

        public BaseResponse AddOrUpdateSTEMIData(CodeSTEMIVM codeSTEMI)
        {
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
                row.IsCompleted = codeSTEMI.IsCompleted;
                row.ModifiedBy = codeSTEMI.ModifiedBy;
                row.ModifiedDate = DateTime.UtcNow;
                row.IsDeleted = false;


                if (codeSTEMI.Attachment != null && codeSTEMI.Attachment.Count > 0)
                {
                    var RootPath = this._environment.WebRootPath;
                    string FileRoot = null;
                    List<string> Attachments = new();
                    //var outPath = Directory.GetCurrentDirectory();

                    FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeSTEMI.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                    FileRoot = Path.Combine(RootPath, FileRoot);
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, "STEMI");
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
                            if (fileExtension != null)
                            {
                                var ByteFile = Convert.FromBase64String(fileInfo[1]);
                                string FilePath = Path.Combine(FileRoot, item.FileName);
                                using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                {
                                    fs.Write(ByteFile);
                                }

                            }
                        }

                    }
                    if (FileRoot != null && FileRoot != "")
                    {
                        codeSTEMI.AttachmentsFolderRoot = FileRoot.Replace(RootPath, "").Replace("\\", "/");
                    }
                }
                if (codeSTEMI.Videos != null && codeSTEMI.Videos.Count > 0)
                {
                    var RootPath = this._environment.WebRootPath;
                    string FileRoot = null;
                    List<string> Attachments = new();
                    //var outPath = Directory.GetCurrentDirectory();

                    FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeSTEMI.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                    FileRoot = Path.Combine(RootPath, FileRoot);
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, "STEMI");
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
                            if (fileExtension != null)
                            {
                                var ByteFile = Convert.FromBase64String(fileInfo[1]);
                                string FilePath = Path.Combine(FileRoot, item.FileName);
                                using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                {
                                    fs.Write(ByteFile);
                                }

                            }
                        }

                    }
                    if (FileRoot != null && FileRoot != "")
                    {
                        codeSTEMI.VideoFolderRoot = FileRoot.Replace(RootPath, "").Replace("\\", "/");
                    }
                }
                if (codeSTEMI.Audios != null && codeSTEMI.Audios.Count > 0)
                {
                    var RootPath = this._environment.WebRootPath;
                    string FileRoot = null;
                    List<string> Attachments = new();
                    //var outPath = Directory.GetCurrentDirectory();

                    FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeSTEMI.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                    FileRoot = Path.Combine(RootPath, FileRoot);
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, "STEMI");
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
                            if (fileExtension != null)
                            {
                                var ByteFile = Convert.FromBase64String(fileInfo[1]);
                                string FilePath = Path.Combine(FileRoot, item.FileName);
                                using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                {
                                    fs.Write(ByteFile);
                                }

                            }
                        }

                    }
                    if (FileRoot != null && FileRoot != "")
                    {
                        codeSTEMI.AudioFolderRoot = FileRoot.Replace(RootPath, "").Replace("\\", "/");
                    }
                }

                row.Attachments = codeSTEMI.AttachmentsFolderRoot;
                row.Video = codeSTEMI.VideoFolderRoot;
                row.Audio = codeSTEMI.AudioFolderRoot;

                this._codeSTEMIRepo.Update(row);
                if (row.IsEms != null && row.IsEms.Value) 
                {
                    var serviceLineIds = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == row.OrganizationIdFk && x.CodeIdFk == UCLEnums.STEMI.ToInt() && !x.IsDeleted).Select(x => x.ServiceLineIds).FirstOrDefault();
                    if (serviceLineIds != null && serviceLineIds != "")
                    {
                        var UserChannelSid = (from us in this._userSchedulesRepo.Table
                                              join u in this._userRepo.Table on us.UserIdFk equals u.UserId
                                              where serviceLineIds.ToIntList().Contains(us.ServiceLineIdFk.Value) && us.ScheduleDateStart <= DateTime.UtcNow && us.ScheduleDateEnd >= DateTime.UtcNow && !us.IsDeleted && !u.IsDeleted
                                              select u.UserChannelSid).ToList();

                        var notification = new PushNotificationVM()
                        {
                            Id = row.CodeStemiid,
                            OrgId = row.OrganizationIdFk,
                            UserChannelSid = UserChannelSid,
                            From = AuthorEnums.STEMI.ToString(),
                            Msg = "STEMI From is Changed",
                            RouteLink = "/Home/Activate%20Code/code-STEMI-form"
                        };

                        _communication.pushNotification(notification);

                    }
                }

                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Modified", Body = row };
            }
            else
            {

                codeSTEMI.CreatedDate = DateTime.UtcNow;
                var STEMI = AutoMapperHelper.MapSingleRow<CodeSTEMIVM, CodeStemi>(codeSTEMI);

                if (codeSTEMI.Attachment != null && codeSTEMI.Attachment.Count > 0)
                {
                    var RootPath = this._environment.WebRootPath;
                    string FileRoot = null;
                    List<string> Attachments = new();
                    //var outPath = Directory.GetCurrentDirectory();

                    FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeSTEMI.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                    FileRoot = Path.Combine(RootPath, FileRoot);
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, "STEMI");
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
                            if (fileExtension != null)
                            {
                                var ByteFile = Convert.FromBase64String(fileInfo[1]);
                                string FilePath = Path.Combine(FileRoot, item.FileName);
                                using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                {
                                    fs.Write(ByteFile);
                                }

                            }
                        }

                    }
                    if (FileRoot != null && FileRoot != "")
                    {
                        codeSTEMI.AttachmentsFolderRoot = FileRoot.Replace(RootPath, "").Replace("\\", "/");
                    }
                }
                if (codeSTEMI.Videos != null && codeSTEMI.Videos.Count > 0)
                {
                    var RootPath = this._environment.WebRootPath;
                    string FileRoot = null;
                    List<string> Attachments = new();
                    //var outPath = Directory.GetCurrentDirectory();

                    FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeSTEMI.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                    FileRoot = Path.Combine(RootPath, FileRoot);
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, "STEMI");
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
                            if (fileExtension != null)
                            {
                                var ByteFile = Convert.FromBase64String(fileInfo[1]);
                                string FilePath = Path.Combine(FileRoot, item.FileName);
                                using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                {
                                    fs.Write(ByteFile);
                                }

                            }
                        }

                    }
                    if (FileRoot != null && FileRoot != "")
                    {
                        codeSTEMI.VideoFolderRoot = FileRoot.Replace(RootPath, "").Replace("\\", "/");
                    }
                }
                if (codeSTEMI.Audios != null && codeSTEMI.Audios.Count > 0)
                {
                    var RootPath = this._environment.WebRootPath;
                    string FileRoot = null;
                    List<string> Attachments = new();

                    //var outPath = Directory.GetCurrentDirectory();

                    FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeSTEMI.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                    FileRoot = Path.Combine(RootPath, FileRoot);
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, "STEMI");
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
                            if (fileExtension != null)
                            {
                                var ByteFile = Convert.FromBase64String(fileInfo[1]);
                                string FilePath = Path.Combine(FileRoot, item.FileName);
                                using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                {
                                    fs.Write(ByteFile);
                                }

                            }
                        }

                    }
                    if (FileRoot != null && FileRoot != "")
                    {
                        codeSTEMI.AudioFolderRoot = FileRoot.Replace(RootPath, "").Replace("\\", "/");
                    }
                }

                STEMI.Attachments = codeSTEMI.AttachmentsFolderRoot;
                STEMI.Video = codeSTEMI.VideoFolderRoot;
                STEMI.Audio = codeSTEMI.AudioFolderRoot;

                this._codeSTEMIRepo.Insert(STEMI);

                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Added", Body = STEMI };
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

        public BaseResponse GetAllTrumaCode(int orgId)
        {
            var TrumaData = this._codeTrumaRepo.Table.Where(x => x.OrganizationIdFk == orgId && !x.IsDeleted).ToList();
            var TrumaDataVM = AutoMapperHelper.MapList<CodeTrauma, CodeTrumaVM>(TrumaData);
            TrumaDataVM.ForEach(x =>
            {
                x.AttachmentsPath = new List<string>();
                x.AudiosPath = new List<string>();
                x.VideosPath = new List<string>();

                if (!string.IsNullOrEmpty(x.Attachments) && !string.IsNullOrWhiteSpace(x.Attachments))
                {
                    string path = _environment.WebRootFileProvider.GetFileInfo(x.Attachments)?.PhysicalPath;
                    if (Directory.Exists(path))
                    {
                        DirectoryInfo AttachFiles = new DirectoryInfo(path);
                        foreach (var item in AttachFiles.GetFiles())
                        {
                            x.AttachmentsPath.Add(x.Attachments + "/" + item.Name);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(x.Audio) && !string.IsNullOrWhiteSpace(x.Audio))
                {
                    string path = _environment.WebRootFileProvider.GetFileInfo(x.Audio)?.PhysicalPath;
                    if (Directory.Exists(path))
                    {
                        DirectoryInfo AudioFiles = new DirectoryInfo(path);
                        foreach (var item in AudioFiles.GetFiles())
                        {
                            x.AudiosPath.Add(x.Audio + "/" + item.Name);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(x.Video) && !string.IsNullOrWhiteSpace(x.Video))
                {
                    var path = _environment.WebRootFileProvider.GetFileInfo(x.Video)?.PhysicalPath; //.GetFileInfo(x.Video);//?.PhysicalPath;
                    if (Directory.Exists(path))
                    {
                        DirectoryInfo VideoFiles = new DirectoryInfo(path);
                        foreach (var item in VideoFiles.GetFiles())
                        {
                            x.VideosPath.Add(x.Video + "/" + item.Name);
                        }
                    }
                }
                x.GenderTitle = _controlListDetailsRepo.Table.Where(g => g.ControlListDetailId == x.Gender).Select(g => g.Title).FirstOrDefault();
                x.BloodThinnersTitle = _controlListDetailsRepo.Table.Where(b => b.ControlListDetailId == x.BloodThinners).Select(b => b.Title).FirstOrDefault();
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

                if (!string.IsNullOrEmpty(TrumaDataVM.Attachments) && !string.IsNullOrWhiteSpace(TrumaDataVM.Attachments))
                {
                    string path = _environment.WebRootFileProvider.GetFileInfo(TrumaDataVM.Attachments)?.PhysicalPath;
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
                    string path = _environment.WebRootFileProvider.GetFileInfo(TrumaDataVM.Audio)?.PhysicalPath;
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
                    var path = _environment.WebRootFileProvider.GetFileInfo(TrumaDataVM.Video)?.PhysicalPath; //.GetFileInfo(TrumaDataVM.Video);//?.PhysicalPath;
                    if (Directory.Exists(path))
                    {
                        DirectoryInfo VideoFiles = new DirectoryInfo(path);
                        foreach (var item in VideoFiles.GetFiles())
                        {
                            TrumaDataVM.VideosPath.Add(TrumaDataVM.Video + "/" + item.Name);
                        }
                    }
                }
                TrumaDataVM.GenderTitle = _controlListDetailsRepo.Table.Where(g => g.ControlListDetailId == TrumaDataVM.Gender).Select(g => g.Title).FirstOrDefault();
                TrumaDataVM.BloodThinnersTitle = _controlListDetailsRepo.Table.Where(b => b.ControlListDetailId == TrumaDataVM.BloodThinners).Select(b => b.Title).FirstOrDefault();
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Found", Body = TrumaDataVM };
            }
            return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Record Not Found" };
        }

        public BaseResponse AddOrUpdateTrumaData(CodeTrumaVM codeTruma)
        {
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
                row.IsCompleted = codeTruma.IsCompleted;
                row.ModifiedBy = codeTruma.ModifiedBy;
                row.ModifiedDate = DateTime.UtcNow;
                row.IsDeleted = false;


                if (codeTruma.Attachment != null && codeTruma.Attachment.Count > 0)
                {
                    var RootPath = this._environment.WebRootPath;
                    string FileRoot = null;
                    List<string> Attachments = new();
                    //var outPath = Directory.GetCurrentDirectory();

                    FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeTruma.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                    FileRoot = Path.Combine(RootPath, FileRoot);
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, "Truma");
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
                            if (fileExtension != null)
                            {
                                var ByteFile = Convert.FromBase64String(fileInfo[1]);
                                string FilePath = Path.Combine(FileRoot, item.FileName);
                                using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                {
                                    fs.Write(ByteFile);
                                }

                            }
                        }

                    }
                    if (FileRoot != null && FileRoot != "")
                    {
                        codeTruma.AttachmentsFolderRoot = FileRoot.Replace(RootPath, "").Replace("\\", "/");
                    }
                }
                if (codeTruma.Videos != null && codeTruma.Videos.Count > 0)
                {
                    var RootPath = this._environment.WebRootPath;
                    string FileRoot = null;
                    List<string> Attachments = new();
                    //var outPath = Directory.GetCurrentDirectory();

                    FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeTruma.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                    FileRoot = Path.Combine(RootPath, FileRoot);
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, "Truma");
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
                            if (fileExtension != null)
                            {
                                var ByteFile = Convert.FromBase64String(fileInfo[1]);
                                string FilePath = Path.Combine(FileRoot, item.FileName);
                                using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                {
                                    fs.Write(ByteFile);
                                }

                            }
                        }

                    }
                    if (FileRoot != null && FileRoot != "")
                    {
                        codeTruma.VideoFolderRoot = FileRoot.Replace(RootPath, "").Replace("\\", "/");
                    }
                }
                if (codeTruma.Audios != null && codeTruma.Audios.Count > 0)
                {
                    var RootPath = this._environment.WebRootPath;
                    string FileRoot = null;
                    List<string> Attachments = new();
                    //var outPath = Directory.GetCurrentDirectory();

                    FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeTruma.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                    FileRoot = Path.Combine(RootPath, FileRoot);
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, "Truma");
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
                            if (fileExtension != null)
                            {
                                var ByteFile = Convert.FromBase64String(fileInfo[1]);
                                string FilePath = Path.Combine(FileRoot, item.FileName);
                                using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                {
                                    fs.Write(ByteFile);
                                }

                            }
                        }

                    }
                    if (FileRoot != null && FileRoot != "")
                    {
                        codeTruma.AudioFolderRoot = FileRoot.Replace(RootPath, "").Replace("\\", "/");
                    }
                }

                row.Attachments = codeTruma.AttachmentsFolderRoot;
                row.Video = codeTruma.VideoFolderRoot;
                row.Audio = codeTruma.AudioFolderRoot;

                this._codeTrumaRepo.Update(row);

                if (row.IsEms != null && row.IsEms.Value) 
                {
                    var serviceLineIds = this._activeCodeRepo.Table.Where(x => x.OrganizationIdFk == row.OrganizationIdFk && x.CodeIdFk == UCLEnums.Trauma.ToInt() && !x.IsDeleted).Select(x => x.ServiceLineIds).FirstOrDefault();
                    if (serviceLineIds != null && serviceLineIds != "")
                    {
                        var UserChannelSid = (from us in this._userSchedulesRepo.Table
                                              join u in this._userRepo.Table on us.UserIdFk equals u.UserId
                                              where serviceLineIds.ToIntList().Contains(us.ServiceLineIdFk.Value) && us.ScheduleDateStart <= DateTime.UtcNow && us.ScheduleDateEnd >= DateTime.UtcNow && !us.IsDeleted && !u.IsDeleted
                                              select u.UserChannelSid).ToList();

                        var notification = new PushNotificationVM()
                        {
                            Id = row.CodeTraumaId,
                            OrgId = row.OrganizationIdFk,
                            UserChannelSid = UserChannelSid,
                            From = AuthorEnums.Trauma.ToString(),
                            Msg = "Trauma From is Changed",
                            RouteLink = "/Home/Activate%20Code/code-trauma-form"
                        };

                        _communication.pushNotification(notification);

                    }
                }
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Modified", Body = row };
            }
            else
            {

                codeTruma.CreatedDate = DateTime.UtcNow;
                var Truma = AutoMapperHelper.MapSingleRow<CodeTrumaVM, CodeTrauma>(codeTruma);


                if (codeTruma.Attachment != null && codeTruma.Attachment.Count > 0)
                {
                    var RootPath = this._environment.WebRootPath;
                    string FileRoot = null;
                    List<string> Attachments = new();
                    //var outPath = Directory.GetCurrentDirectory();

                    FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeTruma.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                    FileRoot = Path.Combine(RootPath, FileRoot);
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, "Truma");
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
                            if (fileExtension != null)
                            {
                                var ByteFile = Convert.FromBase64String(fileInfo[1]);
                                string FilePath = Path.Combine(FileRoot, item.FileName);
                                using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                {
                                    fs.Write(ByteFile);
                                }

                            }
                        }

                    }
                    if (FileRoot != null && FileRoot != "")
                    {
                        codeTruma.AttachmentsFolderRoot = FileRoot.Replace(RootPath, "").Replace("\\", "/");
                    }
                }
                if (codeTruma.Videos != null && codeTruma.Videos.Count > 0)
                {
                    var RootPath = this._environment.WebRootPath;
                    string FileRoot = null;
                    List<string> Attachments = new();
                    //var outPath = Directory.GetCurrentDirectory();

                    FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeTruma.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                    FileRoot = Path.Combine(RootPath, FileRoot);
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, "Truma");
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
                            if (fileExtension != null)
                            {
                                var ByteFile = Convert.FromBase64String(fileInfo[1]);
                                string FilePath = Path.Combine(FileRoot, item.FileName);
                                using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                {
                                    fs.Write(ByteFile);
                                }

                            }
                        }

                    }
                    if (FileRoot != null && FileRoot != "")
                    {
                        codeTruma.VideoFolderRoot = FileRoot.Replace(RootPath, "").Replace("\\", "/");
                    }
                }
                if (codeTruma.Audios != null && codeTruma.Audios.Count > 0)
                {
                    var RootPath = this._environment.WebRootPath;
                    string FileRoot = null;
                    List<string> Attachments = new();
                    //var outPath = Directory.GetCurrentDirectory();

                    FileRoot = this._orgRepo.Table.Where(x => x.OrganizationId == codeTruma.OrganizationIdFk && !x.IsDeleted).Select(x => x.OrganizationName).FirstOrDefault();
                    FileRoot = Path.Combine(RootPath, FileRoot);
                    if (!Directory.Exists(FileRoot))
                    {
                        Directory.CreateDirectory(FileRoot);
                    }
                    FileRoot = Path.Combine(FileRoot, "Truma");
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
                            if (fileExtension != null)
                            {
                                var ByteFile = Convert.FromBase64String(fileInfo[1]);
                                string FilePath = Path.Combine(FileRoot, item.FileName);
                                using (FileStream fs = new(FilePath, FileMode.Create, FileAccess.Write))
                                {
                                    fs.Write(ByteFile);
                                }

                            }
                        }

                    }
                    if (FileRoot != null && FileRoot != "")
                    {
                        codeTruma.AudioFolderRoot = FileRoot.Replace(RootPath, "").Replace("\\", "/");
                    }
                }

                Truma.Attachments = codeTruma.AttachmentsFolderRoot;
                Truma.Video = codeTruma.VideoFolderRoot;
                Truma.Audio = codeTruma.AudioFolderRoot;

                this._codeTrumaRepo.Insert(Truma);

                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Record Added", Body = Truma };
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

    }
}
