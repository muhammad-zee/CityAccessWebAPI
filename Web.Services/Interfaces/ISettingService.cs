using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web.Model;

namespace Web.Services.Interfaces
{
    public interface ISettingService
    {
        BaseResponse GetSettingsByOrgId(int OrgId);
    }
}
