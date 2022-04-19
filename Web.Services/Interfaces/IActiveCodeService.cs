using System.Collections.Generic;
using Web.Model;
using Web.Model.Common;

namespace Web.Services.Interfaces
{
    public interface IActiveCodeService
    {
        #region Active Codes
        BaseResponse GetActivatedCodesByOrgId(int orgId, bool status);
        BaseResponse MapActiveCodes(List<ActiveCodeVM> activeCodes);
        BaseResponse DetachActiveCodes(int activeCodeId, bool status);
        BaseResponse GetAllActiveCodes(int orgId);
        BaseResponse GetEMSandActiveCodesForDashboard(int OrgId, int days = 6);
        #endregion

        #region Delete Files
        BaseResponse DeleteFile(FilesVM files);
        #endregion

        #region Code Stroke
        BaseResponse GetAllStrokeCode(ActiveCodeVM activeCode);
        BaseResponse GetStrokeDataById(int strokeId);
        BaseResponse AddOrUpdateStrokeData(CodeStrokeVM codeStroke);
        BaseResponse DeleteStroke(int strokeId, bool status);
        #endregion


        #region Code Sepsis
        BaseResponse GetAllSepsisCode(ActiveCodeVM activeCode);
        BaseResponse GetSepsisDataById(int SepsisId);
        BaseResponse AddOrUpdateSepsisData(CodeSepsisVM codeSepsis);
        BaseResponse DeleteSepsis(int SepsisId, bool status);
        #endregion

        #region Code STEMI
        BaseResponse GetAllSTEMICode(ActiveCodeVM activeCode);
        BaseResponse GetSTEMIDataById(int STEMIId);
        BaseResponse AddOrUpdateSTEMIData(CodeSTEMIVM codeSTEMI);
        BaseResponse DeleteSTEMI(int STEMIId, bool status);
        #endregion

        #region Code Truma
        BaseResponse GetAllTrumaCode(ActiveCodeVM activeCode);
        BaseResponse GetTrumaDataById(int TrumaId);
        BaseResponse AddOrUpdateTrumaData(CodeTrumaVM codeTruma);
        BaseResponse DeleteTruma(int TrumaId, bool status);
        #endregion

        #region Code Blue

        BaseResponse GetAllBlueCode(ActiveCodeVM activeCode);
        BaseResponse GetBlueDataById(int blueId);
        BaseResponse AddOrUpdateBlueData(CodeBlueVM codeBlue);
        BaseResponse DeleteBlue(int blueId, bool status);


        #endregion

        #region EMS
        BaseResponse GetActiveEMS(bool showAll, bool fromDashboard = false);
        #endregion

        #region Map and Addresses

        BaseResponse GetHospitalsOfStatesByCodeId(int codeId, string latlng);

        #endregion
    }
}
