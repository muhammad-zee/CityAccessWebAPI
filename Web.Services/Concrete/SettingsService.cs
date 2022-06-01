using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
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
    public class SettingsService : ISettingService
    {


        public string generateLogDesc(string tableName, int action, string jsonString)
        {
            var jobj = JObject.Parse(jsonString);
            string logDesc = "";
            //string userFullName = action == ActivityLogActionEnums.SignIn.ToInt() ? jobj["userFullName"].ToString() : ApplicationSettings.UserFullName;
            string userFullName = ApplicationSettings.UserFullName;
            string actionName = action == ActivityLogActionEnums.SignIn.ToInt() ? "Logged In" :
                action == ActivityLogActionEnums.Logout.ToInt() ? "Logged Out" :
                action == ActivityLogActionEnums.Create.ToInt() ? "Created" :
                action == ActivityLogActionEnums.Update.ToInt() ? "Updated" :
                action == ActivityLogActionEnums.Delete.ToInt() ? "Deleted" :
                action == ActivityLogActionEnums.Inactive.ToInt() ? "Deactivated" :
                action == ActivityLogActionEnums.FileUpload.ToInt() ? "Uploaded File" :
                action == ActivityLogActionEnums.FileDelete.ToInt() ? "Deleted File" :
                action == ActivityLogActionEnums.Acknowledge.ToInt() ? "Acknowledged" :
                action == ActivityLogActionEnums.Active.ToInt() ? "Activated" : "Action Performed";

            if (tableName == ActivityLogTableEnums.CodeStrokes.ToString() || tableName == ActivityLogTableEnums.CodeTraumas.ToString() || tableName == ActivityLogTableEnums.CodeBlues.ToString() ||
                tableName == ActivityLogTableEnums.CodeSepsis.ToString() || tableName == ActivityLogTableEnums.CodeSTEMIs.ToString())
            {
                if (action == ActivityLogActionEnums.Update.ToInt())
                {
                    var jobj1 = jobj.Properties().ToList();
                    string updatedField = "";
                    string updatedValue = "";
                    foreach (var i in jobj)
                    {
                        if (i.Value != null)
                        {
                            updatedField = i.Key;
                            updatedValue = i.Value.ToString();
                        }
                    }

                    logDesc = $"{userFullName} {actionName} {updatedField} To {updatedValue} In {tableName}";
                }
                else if (action == ActivityLogActionEnums.FileUpload.ToInt() || action == ActivityLogActionEnums.FileDelete.ToInt())
                {
                    logDesc = $"{userFullName} {actionName} In {tableName}";
                }
                else
                {
                    logDesc = $"{userFullName} {actionName} {tableName}";
                }

            }
            else if(tableName == ActivityLogTableEnums.Consults.ToString())
            {
                if (action == ActivityLogActionEnums.Acknowledge.ToInt())
                {

                }
                logDesc = $"{userFullName} {actionName} Record In {tableName}";

            }
            else
            {
                    logDesc = $"{userFullName} {actionName}";
            }

            return logDesc;
        }

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


        #endregion
    }
}
