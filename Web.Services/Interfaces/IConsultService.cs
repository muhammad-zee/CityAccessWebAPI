using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Web.Model;
using Web.Model.Common;

namespace Web.Services.Interfaces
{
    public interface IConsultService
    {
        #region Consult Feilds
        BaseResponse GetAllConsultFields();
        BaseResponse GetConsultFeildsForOrg(int OrgId);
        BaseResponse AddOrUpdateConsultFeilds(ConsultFieldsVM consultField);
        #endregion

        #region Organization Consult Fields
        BaseResponse AddOrUpdateOrgConsultFeilds(List<OrgConsultFieldsVM> orgConsultFields);
        #endregion
    }
}
