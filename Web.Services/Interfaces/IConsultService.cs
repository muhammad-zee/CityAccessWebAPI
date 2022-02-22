using System.Collections.Generic;
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
        BaseResponse GetConsultFormFieldByOrgId(int OrgId);
        BaseResponse AddOrUpdateConsultFeilds(ConsultFieldsVM consultField);
        #endregion

        #region Organization Consult Fields
        BaseResponse AddOrUpdateOrgConsultFeilds(List<OrgConsultFieldsVM> orgConsultFields);
        BaseResponse GetConsultFormByOrgId(int orgId);
        #endregion

        #region Map and Addresses

        BaseResponse GetHospitalsOfStatesByCodeId(int codeId, string latlng);

        #endregion

        #region Consults
        BaseResponse GetAllConsults();
        BaseResponse GetConsultById(int Id);
        BaseResponse AddOrUpdateConsult(IDictionary<string, object> keyValues);
        #endregion
    }
}
