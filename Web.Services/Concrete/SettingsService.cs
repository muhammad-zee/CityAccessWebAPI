using Microsoft.EntityFrameworkCore;
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
    public class SettingsService: ISettingService
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
            var settings = this._settingRepo.Table.Where(x => x.OrganizationIdFk == OrgId && !x.IsDeleted).FirstOrDefault();
            if (settings != null)
            {
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = "Data Found", Body = settings };
            }
            else {
                return new BaseResponse() { Status = HttpStatusCode.NotFound, Message = "Data Not Found"};
            }
            
        }

    }
}
