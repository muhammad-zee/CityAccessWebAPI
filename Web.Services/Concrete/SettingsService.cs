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
                OrganizationEmail = x.OrganizationEmail,
                TwoFactorAuthenticationExpiryMinutes = x.TwoFactorAuthenticationExpiryMinutes,
                TwoFactorEnable = x.TwoFactorEnable,
                VerifyForFutureDays = x.VerifyForFutureDays,
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
                setting.TwoFactorEnable = settings.TwoFactorEnable;
                setting.TwoFactorAuthenticationExpiryMinutes = settings.TwoFactorAuthenticationExpiryMinutes;
                setting.VerifyForFutureDays = settings.VerifyForFutureDays;
                setting.TokenExpiryTime = settings.TokenExpiryTime;
                setting.OrganizationEmail = settings.OrganizationEmail;
                setting.ModifiedBy = settings.ModifiedBy;
                setting.ModifiedDate = DateTime.UtcNow;
                setting.IsDeleted = false;
            }
            else
            {
                var setting = new Setting()
                {
                    OrganizationIdFk = settings.OrganizationIdFk,
                    TwoFactorEnable = settings.TwoFactorEnable,
                    TwoFactorAuthenticationExpiryMinutes = settings.TwoFactorAuthenticationExpiryMinutes,
                    VerifyForFutureDays = settings.VerifyForFutureDays,
                    TokenExpiryTime = settings.TokenExpiryTime,
                    OrganizationEmail = settings.OrganizationEmail,
                    CreatedBy = settings.CreatedBy,
                    CreatedDate = DateTime.UtcNow
                };
                this._settingRepo.Insert(setting);
            }

            return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Saved Successfully" };
        }

    }
}
