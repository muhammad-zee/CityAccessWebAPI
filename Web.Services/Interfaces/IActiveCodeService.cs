using System.Collections.Generic;
using Web.Model;
using Web.Model.Common;

namespace Web.Services.Interfaces
{
    public interface IActiveCodeService
    {
        #region Active Codes
        BaseResponse GetActivatedCodesByOrgId(int orgId);
        BaseResponse MapActiveCodes(List<ActiveCodeVM> activeCodes);
        BaseResponse DetachActiveCodes(int activeCodeId);
        #endregion

        #region Delete Files
        BaseResponse DeleteFile(FilesVM files);
        #endregion

        #region Code Stroke
        BaseResponse GetAllStrokeCode(int orgId);
        BaseResponse GetStrokeDataById(int strokeId);
        BaseResponse AddOrUpdateStrokeData(CodeStrokeVM codeStroke);
        BaseResponse DeleteStroke(int strokeId);
        #endregion


        #region Code Sepsis
        BaseResponse GetAllSepsisCode(int orgId);
        BaseResponse GetSepsisDataById(int SepsisId);
        BaseResponse AddOrUpdateSepsisData(CodeSepsisVM codeSepsis);
        BaseResponse DeleteSepsis(int SepsisId);
        #endregion

        #region Code STEMI
        BaseResponse GetAllSTEMICode(int orgId);
        BaseResponse GetSTEMIDataById(int STEMIId);
        BaseResponse AddOrUpdateSTEMIData(CodeSTEMIVM codeSTEMI);
        BaseResponse DeleteSTEMI(int STEMIId);
        #endregion

        #region Code Truma
        BaseResponse GetAllTrumaCode(int orgId);
        BaseResponse GetTrumaDataById(int TrumaId);
        BaseResponse AddOrUpdateTrumaData(CodeTrumaVM codeTruma);
        BaseResponse DeleteTruma(int TrumaId);
        #endregion

        #region EMS
        BaseResponse GetActiveEMS();
        #endregion

        #region Map and Addresses

        BaseResponse GetHospitalsOfStatesByCodeId(int codeId, string latlng);

        #endregion
    }
}
