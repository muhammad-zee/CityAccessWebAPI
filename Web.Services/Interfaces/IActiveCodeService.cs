using System.Collections.Generic;
using Web.Model;
using Web.Model.Common;

namespace Web.Services.Interfaces
{
    public interface IActiveCodeService
    {
        BaseResponse MapActiveCodes(List<ActiveCodeVM> activeCodes);
        BaseResponse DetachActiveCodes(int activeCodeId);

        #region Code Stroke
        BaseResponse GetAllStrokeCode();
        BaseResponse GetStrokeDataById(int strokeId);
        BaseResponse AddOrUpdateStrokeData(CodeStrokeVM codeStroke);
        BaseResponse DeleteStroke(int strokeId);
        #endregion


        #region Code Sepsis
        BaseResponse GetAllSepsisCode();
        BaseResponse GetSepsisDataById(int SepsisId);
        BaseResponse AddOrUpdateSepsisData(CodeSepsisVM codeSepsis);
        BaseResponse DeleteSepsis(int SepsisId);
        #endregion

        #region Code STEMI
        BaseResponse GetAllSTEMICode();
        BaseResponse GetSTEMIDataById(int STEMIId);
        BaseResponse AddOrUpdateSTEMIData(CodeSTEMIVM codeSTEMI);
        BaseResponse DeleteSTEMI(int STEMIId);
        #endregion

        #region Code Truma
        BaseResponse GetAllTrumaCode();
        BaseResponse GetTrumaDataById(int TrumaId);
        BaseResponse AddOrUpdateTrumaData(CodeTrumaVM codeTruma);
        BaseResponse DeleteTruma(int TrumaId);
        #endregion
    }
}
