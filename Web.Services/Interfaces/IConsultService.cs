using System.Collections.Generic;
using Web.Model;
using Web.Model.Common;

namespace Web.Services.Interfaces
{
    public interface IConsultService
    {
        #region Consult Feilds
        BaseResponse GetAllConsultFields();
        BaseResponse GetConsultFeildsForOrg(int OrgId);
        BaseResponse GetConsultFormFieldByOrgId(int OrgId);
        BaseResponse AddOrUpdateConsultFeilds(ConsultFieldsVM consultField);
        #endregion

        #region Organization Consult Fields
        BaseResponse AddOrUpdateOrgConsultFeilds(List<OrgConsultFieldsVM> orgConsultFields);
        BaseResponse GetConsultFormByOrgId(int orgId);

        BaseResponse AddOrUpdateConsult(IDictionary<string, object> keyValues);
        #endregion
    }
}
