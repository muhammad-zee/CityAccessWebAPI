﻿using Web.Model;
using Web.Model.Common;

namespace Web.Services.Interfaces
{
    public interface ISettingService
    {
        BaseResponse GetSettingsByOrgId(int OrgId);
        BaseResponse AddOrUpdateOrgSettings(SettingsVM settings);
    }
}
