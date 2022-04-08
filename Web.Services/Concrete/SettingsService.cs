using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using Web.Data.Models;
using Web.DLL.Generic_Repository;
using Web.Model;
using Web.Model.Common;
using Web.Services.Interfaces;

namespace Web.Services.Concrete
{
    public class SettingsService : ISettingService
    {
        IConfiguration _config;
        private RAQ_DbContext _dbContext;
        private readonly IRepository<Setting> _settingRepo;

        public SettingsService(RAQ_DbContext dbContext,
            IConfiguration configuration,
            IRepository<Setting> settingsRepo)
        {
            this._dbContext = dbContext;
            this._config = configuration;
            this._settingRepo = settingsRepo;
        }


        public BaseResponse GetSettingsByOrgId(int OrgId)
        {
            var settings = this._settingRepo.Table.Where(x => x.OrganizationIdFk == OrgId && !x.IsDeleted).Select(x => new SettingsVM()
            {
                SettingId = x.SettingId,
                OrganizationIdFk = x.OrganizationIdFk,
                TwoFactorCodeExpiry = x.TwoFactorAuthenticationExpiryMinutes,
                TwoFactorEnabled = x.TwoFactorEnable,
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

        public BaseResponse AddOrUpdateOrgSettings(SettingsVM settings)
        {
            if (settings.SettingId > 0)
            {
                var setting = this._settingRepo.Table.Where(x => x.SettingId == settings.SettingId && !x.IsDeleted).FirstOrDefault();
                setting.TwoFactorEnable = settings.TwoFactorEnabled;
                setting.TwoFactorAuthenticationExpiryMinutes = settings.TwoFactorCodeExpiry;
                setting.VerifyForFutureDays = settings.VerifyCodeForFutureDays;
                setting.TokenExpiryTime = settings.TokenExpiryTime;

                ////// Password Validations ///////////

                setting.PasswordLength = settings.PasswordLength;
                setting.RequiredLowerCase = settings.RequiredLowerCase;
                setting.RequiredNonAlphaNumeric = settings.RequiredNonAlphaNumeric;
                setting.RequiredNumeric = settings.RequiredNumeric;
                setting.RequiredUpperCase = settings.RequiredUpperCase;
                setting.EnablePasswordAge = settings.EnablePasswordAge;

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
                    OrganizationIdFk = settings.OrganizationIdFk,
                    TwoFactorEnable = settings.TwoFactorEnabled,
                    TwoFactorAuthenticationExpiryMinutes = settings.TwoFactorCodeExpiry,
                    VerifyForFutureDays = settings.VerifyCodeForFutureDays,
                    TokenExpiryTime = settings.TokenExpiryTime,

                    ////// Password Validations ///////////

                    PasswordLength = settings.PasswordLength,
                    RequiredLowerCase = settings.RequiredLowerCase,
                    RequiredNonAlphaNumeric = settings.RequiredNonAlphaNumeric,
                    RequiredNumeric = settings.RequiredNumeric,
                    RequiredUpperCase = settings.RequiredUpperCase,
                    EnablePasswordAge = settings.EnablePasswordAge,

                    //////////////////////////////////////
                    
                    CreatedBy = settings.CreatedBy,
                    CreatedDate = DateTime.UtcNow
                };
                this._settingRepo.Insert(setting);
            }

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Saved Successfully" };
        }

    }
}
