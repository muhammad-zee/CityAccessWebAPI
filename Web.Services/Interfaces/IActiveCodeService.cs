﻿using System.Collections.Generic;
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

        #region Generic Methods For Codes
        BaseResponse GetAllCodeData(ActiveCodeVM activeCode);
        BaseResponse GetCodeDataById(int codeId, string codeName);
        BaseResponse AddOrUpdateCodeData(IDictionary<string, object> codeData);

        #endregion

        #region Code Stroke
        BaseResponse GetAllStrokeCode(ActiveCodeVM activeCode);
        BaseResponse GetStrokeDataById(int strokeId);
        BaseResponse AddOrUpdateStrokeData(CodeStrokeVM codeStroke);
        BaseResponse CreateStrokeGroup(CodeStrokeVM codeStroke);
        BaseResponse UpdateStrokeGroupMembers(CodeStrokeVM codeStroke);
        BaseResponse DeleteStroke(int strokeId, bool status);
        BaseResponse ActiveOrInActiveStroke(int strokeId, bool status);
        #endregion


        #region Code Sepsis
        BaseResponse GetAllSepsisCode(ActiveCodeVM activeCode);
        BaseResponse GetSepsisDataById(int SepsisId);
        BaseResponse AddOrUpdateSepsisData(CodeSepsisVM codeSepsis);
        BaseResponse CreateSepsisGroup(CodeSepsisVM codeSepsis);
        BaseResponse UpdateSepsisGroupMembers(CodeSepsisVM codeSepsis);
        BaseResponse DeleteSepsis(int SepsisId, bool status);

        BaseResponse ActiveOrInActiveSepsis(int SepsisId, bool status);
        #endregion

        #region Code STEMI
        BaseResponse GetAllSTEMICode(ActiveCodeVM activeCode);
        BaseResponse GetSTEMIDataById(int STEMIId);
        BaseResponse AddOrUpdateSTEMIData(CodeSTEMIVM codeSTEMI);
        BaseResponse CreateSTEMIGroup(CodeSTEMIVM codeSTEMI);
        BaseResponse UpdateSTEMIGroupMembers(CodeSTEMIVM codeSTEMI);
        BaseResponse DeleteSTEMI(int STEMIId, bool status);

        BaseResponse ActiveOrInActiveSTEMI(int STEMIId, bool status);
        #endregion

        #region Code Truma
        BaseResponse GetAllTrumaCode(ActiveCodeVM activeCode);
        BaseResponse GetTrumaDataById(int TrumaId);
        BaseResponse AddOrUpdateTrumaData(CodeTrumaVM codeTruma);
        BaseResponse CreateTrumaGroup(CodeTrumaVM codeTruma);
        BaseResponse UpdateTrumaGroupMembers(CodeTrumaVM codeTruma);
        BaseResponse DeleteTruma(int TrumaId, bool status);

        BaseResponse ActiveOrInActiveTruma(int TrumaId, bool status);
        #endregion

        #region Code Blue

        BaseResponse GetAllBlueCode(ActiveCodeVM activeCode);
        BaseResponse GetBlueDataById(int blueId);
        BaseResponse AddOrUpdateBlueData(CodeBlueVM codeBlue);
        BaseResponse CreateBlueGroup(CodeBlueVM codeBlue);
        BaseResponse UpdateBlueGroupMembers(CodeBlueVM codeBlue);
        BaseResponse DeleteBlue(int blueId, bool status);

        BaseResponse ActiveOrInActiveBlue(int blueId, bool status);


        #endregion

        #region EMS
        BaseResponse GetActiveEMS(bool showAll, bool fromDashboard = false);
        #endregion

        #region Inhouse Code Settings

        BaseResponse GetAllInhouseCodeFeilds();
        BaseResponse GetInhouseCodeFeildsForOrg(int OrgId, string codeName);
        //BaseResponse GetInhouseCodeFormFieldByOrgId(int OrgId, string codeName);

        #endregion

        #region Organization InhouseCode Fields

        BaseResponse AddOrUpdateOrgCodeStrokeFeilds(List<OrgCodeStrokeFeildsVM> orgInhouseCodeFields);
        BaseResponse AddOrUpdateOrgCodeSTEMIFeilds(List<OrgCodeSTEMIFeildsVM> orgInhouseCodeFields);
        BaseResponse AddOrUpdateOrgCodeSepsisFeilds(List<OrgCodeSepsisFeildsVM> orgInhouseCodeFields);
        BaseResponse AddOrUpdateOrgCodeTraumaFeilds(List<OrgCodeTraumaFeildsVM> orgInhouseCodeFields);
        BaseResponse AddOrUpdateOrgCodeBlueFeilds(List<OrgCodeBlueFeildsVM> orgInhouseCodeFields);
        BaseResponse GetInhouseCodeFormByOrgId(int orgId, string codeName);

        #endregion

        #region Map and Addresses

        BaseResponse GetHospitalsOfStatesByCodeId(int codeId, string latlng);

        #endregion

        #region Apis for Mobile App
        BaseResponse GetAllCodesData(ActiveCodeVM activeCode);
        #endregion

    }
}
