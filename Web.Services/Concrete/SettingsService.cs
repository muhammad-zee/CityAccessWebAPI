using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Text.RegularExpressions;
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
    public class SettingsService : ISettingService
    {


        IConfiguration _config;
        private RAQ_DbContext _dbContext;
        private readonly IRepository<Setting> _settingRepo;
        public readonly IRepository<ActivityLog> _activityLogRepo;

        public SettingsService(RAQ_DbContext dbContext,
            IConfiguration configuration,
            IRepository<Setting> settingsRepo,
            IRepository<ActivityLog> activityLogRepo)
        {
            this._dbContext = dbContext;
            this._config = configuration;
            this._settingRepo = settingsRepo;
            this._activityLogRepo = activityLogRepo;
        }



        public BaseResponse GetSettingsByOrgId(int OrgId)
        {
            var settings = this._settingRepo.Table.Where(x => x.OrganizationIdFk == OrgId && !x.IsDeleted).Select(x => new SettingsVM()
            {
                SettingId = x.SettingId,
                OrganizationIdFk = x.OrganizationIdFk,
                TwoFactorCodeExpiry = x.TwoFactorAuthenticationExpiryMinutes,
                TwoFactorEnable = x.TwoFactorEnable,
                VerifyCodeForFutureDays = x.VerifyForFutureDays,

                PasswordLength = x.PasswordLength,
                RequiredLowerCase = x.RequiredLowerCase,
                RequiredNonAlphaNumeric = x.RequiredNonAlphaNumeric,
                RequiredNumeric = x.RequiredNumeric,
                RequiredUpperCase = x.RequiredUpperCase,
                EnablePasswordAge = x.EnablePasswordAge,

                TokenExpiryTime = x.TokenExpiryTime,
                IsDeleted = x.IsDeleted
            }).FirstOrDefault();
            if (settings != null)
            {
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Found", Body = settings };
            }
            else
            {
                return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Data Not Found" };
            }

        }

        public BaseResponse GetSettingsChangePasswordByOrgId(SettingsVM settings)
        {
            var passwordSettings = this._dbContext.LoadStoredProcedure("md_getOrganizationSettings")
                                .WithSqlParam("@IsSuperAdmin", settings.UserIsSuperAdmin)
                                .WithSqlParam("@IsEMS", settings.UserIsEMS)
                                .WithSqlParam("@orgId", settings.OrganizationId)
                                .ExecuteStoredProc<Setting>().FirstOrDefault();
            if (passwordSettings != null)
            {
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Found", Body = passwordSettings };
            }
            else
            {
                return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Data Not Found" };
            }
        }

        public BaseResponse AddOrUpdateOrgSettings(SettingsVM settings)
        {
            if (settings.SettingId > 0)
            {
                var setting = this._settingRepo.Table.Where(x => x.SettingId == settings.SettingId && !x.IsDeleted).FirstOrDefault();
                setting.TwoFactorEnable = settings.TwoFactorEnable;
                setting.TwoFactorAuthenticationExpiryMinutes = settings.TwoFactorCodeExpiry.HasValue && settings.TwoFactorCodeExpiry.Value > 0 ? settings.TwoFactorCodeExpiry.Value : _config["TwoFactorAuthentication:TwoFactorAuthenticationExpiryMinutes"].ToInt();
                setting.VerifyForFutureDays = settings.VerifyCodeForFutureDays.HasValue && settings.VerifyCodeForFutureDays.Value > 0 ? settings.VerifyCodeForFutureDays.Value : _config["TwoFactorAuthentication:VerifyForFutureDays"].ToInt();
                setting.TokenExpiryTime = settings.TokenExpiryTime.Value;

                ////// Password Validations ///////////

                setting.PasswordLength = settings.PasswordLength.HasValue ? settings.PasswordLength : _config["PasswordSettings:PassLength"].ToInt();
                setting.RequiredLowerCase = settings.RequiredLowerCase;
                setting.RequiredNonAlphaNumeric = settings.RequiredNonAlphaNumeric;
                setting.RequiredNumeric = settings.RequiredNumeric;
                setting.RequiredUpperCase = settings.RequiredUpperCase;
                setting.EnablePasswordAge = settings.EnablePasswordAge.HasValue ? settings.EnablePasswordAge : _config["PasswordSettings:PassExpiryDays"].ToInt();

                //////////////////////////////////////

                setting.ModifiedBy = settings.ModifiedBy;
                setting.ModifiedDate = DateTime.UtcNow;
                setting.IsDeleted = false;
                this._settingRepo.Update(setting);
            }
            else
            {
                var setting = new Setting()
                {
                    OrganizationIdFk = settings.OrganizationIdFk.Value,
                    TwoFactorEnable = settings.TwoFactorEnable,
                    TwoFactorAuthenticationExpiryMinutes = settings.TwoFactorCodeExpiry.HasValue && settings.TwoFactorCodeExpiry.Value > 0 ? settings.TwoFactorCodeExpiry.Value : _config["TwoFactorAuthentication:TwoFactorAuthenticationExpiryMinutes"].ToInt(),
                    VerifyForFutureDays = settings.VerifyCodeForFutureDays.HasValue && settings.VerifyCodeForFutureDays.Value > 0 ? settings.VerifyCodeForFutureDays.Value : _config["TwoFactorAuthentication:VerifyForFutureDays"].ToInt(),
                    TokenExpiryTime = settings.TokenExpiryTime.Value,

                    ////// Password Validations ///////////

                    PasswordLength = settings.PasswordLength.HasValue ? settings.PasswordLength : _config["PasswordSettings:PassLength"].ToInt(),
                    RequiredLowerCase = settings.RequiredLowerCase,
                    RequiredNonAlphaNumeric = settings.RequiredNonAlphaNumeric,
                    RequiredNumeric = settings.RequiredNumeric,
                    RequiredUpperCase = settings.RequiredUpperCase,
                    EnablePasswordAge = settings.EnablePasswordAge.HasValue ? settings.EnablePasswordAge : _config["PasswordSettings:PassExpiryDays"].ToInt(),

                    //////////////////////////////////////

                    CreatedBy = settings.CreatedBy.Value,
                    CreatedDate = DateTime.UtcNow
                };
                this._settingRepo.Insert(setting);
            }

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Saved Successfully" };
        }


        #region [Activity Log]

        public BaseResponse GetActivityLog(FilterActivityLogVM filter)
        {
            var rec = this._dbContext.LoadStoredProcedure("md_getActivityLogReport")
                                .WithSqlParam("@pUserId", filter.UserId)
                                .WithSqlParam("@pModule", filter.ModuleId)
                                .WithSqlParam("@pCodeId", filter.CodeId)
                                .WithSqlParam("@pFromDate", filter.FromDate)
                                .WithSqlParam("@pToDate", filter.ToDate)
                                .WithSqlParam("@pLastRecordId", filter.LastRecordId)
                                .WithSqlParam("@pPageSize", filter.PageSize)
                                .ExecuteStoredProc<ActivityLogVm>().AsQueryable();
            foreach (var r in rec)
            {
                r.Description = generateLogDesc(r);

            }
            if (rec != null)
            {
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Found", Body = rec };
            }
            else
            {
                return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Data Not Found" };
            }
        }

        public BaseResponse GetActivityLogPrimaryKeys(FilterActivityLogVM filter)
        {
            var rec = this._dbContext.LoadStoredProcedure("md_getActivityLogPrimaryKeys")
                                 .WithSqlParam("@pUserId", filter.UserId)
                                 .WithSqlParam("@pModule", filter.ModuleId)
                                 .WithSqlParam("@pFromDate", filter.FromDate)
                                 .WithSqlParam("@pToDate", filter.ToDate)
                                 .ExecuteStoredProc<ActivityLogVm>().AsQueryable();

            if (rec != null)
            {
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Found", Body = rec };
            }
            else
            {
                return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Data Not Found" };
            }
        }

        public BaseResponse LogoutActivity()
        {
            this._dbContext.Log(new { }, ActivityLogTableEnums.Users.ToString(), ApplicationSettings.UserId, ActivityLogActionEnums.Logout.ToInt());
            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Found" };

        }

        public string generateLogDesc(ActivityLogVm model)
        {

            string logDesc = "";
            string recordName = "";
            bool dynamicDesc = true;
            if (model.TableName == ActivityLogTableEnums.CodeStrokes.ToString() || model.TableName == ActivityLogTableEnums.CodeTraumas.ToString() ||
            model.TableName == ActivityLogTableEnums.CodeBlues.ToString() || model.TableName == ActivityLogTableEnums.CodeSepsis.ToString() ||
            model.TableName == ActivityLogTableEnums.CodeSTEMIs.ToString())
            {
                recordName = "Code";
            }
            else if (model.TableName == ActivityLogTableEnums.Consults.ToString())
            {
                recordName = "Consult";
            }
            else if (model.TableName == ActivityLogTableEnums.UsersSchedule.ToString())
            {
                recordName = "Schedule";
                dynamicDesc = false;
            }
            if (!dynamicDesc)
            {
                logDesc = $"<b>{model.UserFullName}</b> {model.ActionName} {model.Description}";
            }
            if (dynamicDesc) { 
            if (model.Action == ActivityLogActionEnums.SignIn.ToInt() || model.Action == ActivityLogActionEnums.SignIn.ToInt())
            {
                logDesc = $"<b>{model.UserFullName}</b> {model.ActionName}";
            }
            else if (model.Action == ActivityLogActionEnums.Create.ToInt())
            {
                logDesc = $"<b>{model.UserFullName}</b> {model.ActionName} <b>{recordName}: {model.TablePrimaryKey}</b> in {model.TableName}";

            }
            else if (model.Action == ActivityLogActionEnums.Update.ToInt())
            {
                string changedFields = "";
                var jobj1 = JObject.Parse(model.PreviousValue);
                var jobj2 = JObject.Parse(model.Changeset);
                var jobj1Props = jobj1.Properties().ToList();
                foreach (var p in jobj1Props)
                {
                    var typeOfPrevObj = jobj1[p.Name].Type.ToString();
                    var typeOfUpdatedObj = jobj2[p.Name].Type.ToString();
                    if (typeOfPrevObj != "Array" && typeOfUpdatedObj != "Array")
                    {
                        changedFields = changedFields != "" ? changedFields + ", " : changedFields;
                        changedFields += $"{p.Name.SplitCamelCase()} {jobj1[p.Name]} to {jobj2[p.Name]}";
                    }

                }
                changedFields = changedFields != "" ? changedFields + " of" : changedFields;
                logDesc = $"<b>{model.UserFullName}</b> {model.ActionName} {changedFields} <b>{recordName}: {model.TablePrimaryKey}</b> In {model.TableName}";
            }
            else if (model.Action == ActivityLogActionEnums.Active.ToInt() || model.Action == ActivityLogActionEnums.Inactive.ToInt())
            {

                logDesc = $"<b>{model.UserFullName}</b> changed status of  <b>{recordName}: {model.TablePrimaryKey}</b> to {model.ActionName}";
            }
            else if (model.Action == ActivityLogActionEnums.FileUpload.ToInt() || model.Action == ActivityLogActionEnums.FileDelete.ToInt())
            {
                logDesc = $"<b>{model.UserFullName}</b> {model.ActionName} file in <b>{recordName}: {model.TablePrimaryKey}</b> {model.TableName}";
            }
            }
            return logDesc;
        }

        #endregion
    }
}
