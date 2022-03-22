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
        #endregion

        #region Consults
        BaseResponse GetAllConsults();
        BaseResponse GetConsultGraphDataForOrg(int orgId, int days = 6);
        BaseResponse GetConsultById(int Id);
        BaseResponse GetConsultsByServiceLineId(ConsultVM consult);
        BaseResponse AddOrUpdateConsult(IDictionary<string, object> keyValues);
        BaseResponse DeleteConsult(int consultId);
        #endregion

        #region Consult Acknowledgments
        BaseResponse GetAllConsultAcknowledgments();
        BaseResponse GetConsultAcknowledgmentByConsultId(int consultId);
        BaseResponse GetConsultAcknowledgmentByUserId(int userId);
        BaseResponse AcknowledgeConsult(int consultId);
        #endregion
    }
}
