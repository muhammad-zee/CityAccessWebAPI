using ElmahCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using Web.API.Helper;
using Web.Model;
using Web.Model.Common;
using Web.Services.Interfaces;

namespace Web.API.Controllers
{
    [Authorize]
    [RequestHandler]
    public class ActiveCodeController : Controller
    {
        private readonly IActiveCodeService _activeCodesService;
        private IConfiguration _config;
        Logger _logger;
        private IWebHostEnvironment _hostEnvironment;
        public ActiveCodeController(IActiveCodeService activeCodesService, IConfiguration config, IWebHostEnvironment environment)
        {
            _activeCodesService = activeCodesService;
            _config = config;
            _hostEnvironment = environment;
            _logger = new Logger(_hostEnvironment, config);
        }


        [Description("Get Activated Codes By Org Id")]
        [HttpGet("activecode/GetActivatedCodesByOrgId/{orgId}/{status}")]
        public BaseResponse GetActivatedCodesByOrgId(int orgId, bool status)
        {
            try
            {
                return _activeCodesService.GetActivatedCodesByOrgId(orgId, status);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Add Or Update Stroke")]
        [HttpPost("activecode/MapActiveCodes")]
        public BaseResponse MapActiveCodes([FromBody] List<ActiveCodeVM> activeCode)
        {
            try
            {
                return _activeCodesService.MapActiveCodes(activeCode);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Add Or Update Stroke")]
        [HttpGet("activecode/DetachActiveCodes/{activeCodeId}/{status}")]
        public BaseResponse DetachActiveCodes(int activeCodeId, bool status)
        {
            try
            {
                return _activeCodesService.DetachActiveCodes(activeCodeId, status);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Get All Active Codes")]
        [HttpGet("activecode/GetAllActiveCodes/{orgId}")]
        public BaseResponse GetAllActiveCodes(int orgId)
        {
            try
            {
                return _activeCodesService.GetAllActiveCodes(orgId);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Get All Active Codes")]
        [HttpGet("activecode/GetEMSandActiveCodesForDashboard/{OrgId}/{days}")]
        public BaseResponse GetEMSandActiveCodesForDashboard(int OrgId, int days = 6)
        {
            try
            {
                return _activeCodesService.GetEMSandActiveCodesForDashboard(OrgId, days);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }
        #region Delete Files


        [Description("Delete File")]
        [HttpPost("activecode/DeleteFile")]
        public BaseResponse DeleteFile([FromBody] FilesVM files)
        {
            try
            {
                return _activeCodesService.DeleteFile(files);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        #endregion  

        #region Code Stroke

        [Description("Get All Stroke Data")]
        [HttpPost("stroke/GetAllStrokeData")]
        public BaseResponse GetAllStrokeData([FromBody] ActiveCodeVM activeCode)
        {
            try
            {
                return _activeCodesService.GetAllStrokeCode(activeCode);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Get Stroke Data By Id")]
        [HttpGet("stroke/GetStrokeDataById/{strokeId}")]
        public BaseResponse GetStrokeDataById(int strokeId)
        {
            try
            {
                return _activeCodesService.GetStrokeDataById(strokeId);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }


        [Description("Add Or Update Stroke")]
        [HttpPost("stroke/AddOrUpdateStroke")]
        public BaseResponse AddOrUpdateStroke([FromBody] CodeStrokeVM codeStroke)
        {
            try
            {
                return _activeCodesService.AddOrUpdateStrokeData(codeStroke);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }


        [Description("Delete Stroke Data By Id")]
        [HttpGet("stroke/DeleteStrokeDataById/{strokeId}/{status}")]
        public BaseResponse DeleteStrokeDataById(int strokeId, bool status)
        {
            try
            {
                return _activeCodesService.DeleteStroke(strokeId, status);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Active/InActive Stroke Data By Id")]
        [HttpGet("stroke/ActiveOrInActiveStrokeDataById/{strokeId}/{status}")]
        public BaseResponse ActiveOrInActiveStrokeDataById(int strokeId, bool status)
        {
            try
            {
                return _activeCodesService.ActiveOrInActiveStroke(strokeId, status);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        #endregion


        #region Code Sepsis

        [Description("Get All Sepsis Data")]
        [HttpPost("Sepsis/GetAllSepsisData")]
        public BaseResponse GetAllSepsisData([FromBody] ActiveCodeVM activeCode)
        {
            try
            {
                return _activeCodesService.GetAllSepsisCode(activeCode);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Get Sepsis Data By Id")]
        [HttpGet("Sepsis/GetSepsisDataById/{SepsisId}")]
        public BaseResponse GetSepsisDataById(int SepsisId)
        {
            try
            {
                return _activeCodesService.GetSepsisDataById(SepsisId);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }


        [Description("Add Or Update Sepsis")]
        [HttpPost("Sepsis/AddOrUpdateSepsis")]
        public BaseResponse AddOrUpdateSepsis([FromBody] CodeSepsisVM codeSepsis)
        {
            try
            {
                var state = ModelState;
                return _activeCodesService.AddOrUpdateSepsisData(codeSepsis);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }


        [Description("Delete Sepsis Data By Id")]
        [HttpGet("Sepsis/DeleteSepsisDataById/{SepsisId}/{status}")]
        public BaseResponse DeleteSepsisDataById(int SepsisId, bool status)
        {
            try
            {
                return _activeCodesService.DeleteSepsis(SepsisId, status);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Active Or InActive Sepsis Data By Id")]
        [HttpGet("Sepsis/ActiveOrInActiveSepsisDataById/{SepsisId}/{status}")]
        public BaseResponse ActiveOrInActiveSepsisDataById(int SepsisId, bool status)
        {
            try
            {
                return _activeCodesService.ActiveOrInActiveSepsis(SepsisId, status);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        #endregion


        #region Code STEMI

        [Description("Get All STEMI Data")]
        [HttpPost("STEMI/GetAllSTEMIData")]
        public BaseResponse GetAllSTEMIData([FromBody] ActiveCodeVM activeCode)
        {
            try
            {
                return _activeCodesService.GetAllSTEMICode(activeCode);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Get STEMI Data By Id")]
        [HttpGet("STEMI/GetSTEMIDataById/{STEMIId}")]
        public BaseResponse GetSTEMIDataById(int STEMIId)
        {
            try
            {
                return _activeCodesService.GetSTEMIDataById(STEMIId);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }


        [Description("Add Or Update STEMI")]
        [HttpPost("STEMI/AddOrUpdateSTEMI")]
        public BaseResponse AddOrUpdateSTEMI([FromBody] CodeSTEMIVM codeSTEMI)
        {
            try
            {
                return _activeCodesService.AddOrUpdateSTEMIData(codeSTEMI);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }


        [Description("Delete STEMI Data By Id")]
        [HttpGet("STEMI/DeleteSTEMIDataById/{STEMIId}/{status}")]
        public BaseResponse DeleteSTEMIDataById(int STEMIId, bool status)
        {
            try
            {
                return _activeCodesService.DeleteSTEMI(STEMIId, status);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Active Or InActive STEMI Data By Id")]
        [HttpGet("STEMI/ActiveOrInActiveSTEMIDataById/{STEMIId}/{status}")]
        public BaseResponse ActiveOrInActiveSTEMIDataById(int STEMIId, bool status)
        {
            try
            {
                return _activeCodesService.ActiveOrInActiveSTEMI(STEMIId, status);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        #endregion


        #region Code Truma

        [Description("Get All Truma Data")]
        [HttpPost("Truma/GetAllTrumaData")]
        public BaseResponse GetAllTrumaData([FromBody] ActiveCodeVM activeCode)
        {
            try
            {
                return _activeCodesService.GetAllTrumaCode(activeCode);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex); _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Get Truma Data By Id")]
        [HttpGet("Truma/GetTrumaDataById/{TrumaId}")]
        public BaseResponse GetTrumaDataById(int TrumaId)
        {
            try
            {
                return _activeCodesService.GetTrumaDataById(TrumaId);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }


        [Description("Add Or Update Truma")]
        [HttpPost("Truma/AddOrUpdateTruma")]
        public BaseResponse AddOrUpdateTruma([FromBody] CodeTrumaVM codeTruma)
        {
            try
            {
                return _activeCodesService.AddOrUpdateTrumaData(codeTruma);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }


        [Description("Delete Truma Data By Id")]
        [HttpGet("Truma/DeleteTrumaDataById/{TrumaId}/{status}")]
        public BaseResponse DeleteTrumaDataById(int TrumaId, bool status)
        {
            try
            {
                return _activeCodesService.DeleteTruma(TrumaId, status);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Active/InActive Truma Data By Id")]
        [HttpGet("Truma/ActiveOrInActiveTrumaDataById/{TrumaId}/{status}")]
        public BaseResponse ActiveOrInActiveTrumaDataById(int TrumaId, bool status)
        {
            try
            {
                return _activeCodesService.ActiveOrInActiveTruma(TrumaId, status);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        #endregion

        #region Code Blue


        [Description("Get All Blue Data")]
        [HttpPost("blue/GetAllBlueData")]
        public BaseResponse GetAllBlueData([FromBody] ActiveCodeVM activeCode)
        {
            try
            {
                return _activeCodesService.GetAllBlueCode(activeCode);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Get Blue Data By Id")]
        [HttpGet("blue/GetBlueDataById/{blueId}")]
        public BaseResponse GetBlueDataById(int blueId)
        {
            try
            {
                return _activeCodesService.GetBlueDataById(blueId);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }


        [Description("Add Or Update blue")]
        [HttpPost("blue/AddOrUpdateBlue")]
        public BaseResponse AddOrUpdateBlue([FromBody] CodeBlueVM codeBlue)
        {
            try
            {
                return _activeCodesService.AddOrUpdateBlueData(codeBlue);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }


        [Description("Delete Blue Data By Id")]
        [HttpGet("blue/DeleteBlueDataById/{blueId}/{status}")]
        public BaseResponse DeleteBlueDataById(int blueId, bool status)
        {
            try
            {
                return _activeCodesService.DeleteBlue(blueId, status);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Active Or InActive Blue Data By Id")]
        [HttpGet("blue/ActiveOrInActiveBlueDataById/{blueId}/{status}")]
        public BaseResponse ActiveInActiveBlueDataById(int blueId, bool status)
        {
            try
            {
                return _activeCodesService.ActiveOrInActiveBlue(blueId, status);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        #endregion

        #region EMS

        [Description("Get All Active EMS")]
        [HttpGet("EMS/GetActiveEMS/{showAll}/{fromDashbord}")]
        public BaseResponse GetActiveEMS(bool showAll, bool fromDashbord = false)
        {
            try
            {
                return _activeCodesService.GetActiveEMS(showAll, fromDashbord);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        #endregion


        #region Inhouse Code Settings

        [Description("Get All Inhouse Code Feilds")]
        [HttpGet("InhouseCode/GetAllInhouseCodeFeilds")]
        public BaseResponse GetAllInhouseCodeFeilds()
        {
            try
            {
                return _activeCodesService.GetAllInhouseCodeFeilds();
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Get All Inhouse Code Feilds")]
        [HttpGet("InhouseCode/GetInhouseCodeFeildsForOrg/{OrgId}/{codeName}")]
        public BaseResponse GetInhouseCodeFeildsForOrg(int OrgId, string codeName)
        {
            try
            {
                return _activeCodesService.GetInhouseCodeFeildsForOrg(OrgId, codeName);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }
        //public BaseResponse GetInhouseCodeFormFieldByOrgId(int OrgId, string codeName) 
        //{
        //    try
        //    {
        //        return _activeCodesService.GetInhouseCodeFormFieldByOrgId(OrgId, codeName);
        //    }
        //    catch (Exception ex)
        //    {
        //        ElmahExtensions.RiseError(ex);
        //        _logger.LogExceptions(ex);
        //        return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
        //    }

        //}

        #endregion

        #region Organization InhouseCode Fields


        [Description("Get All Inhouse Code Feilds")]
        [HttpPost("InhouseCode/AddOrUpdateOrgCodeStrokeFeilds")]
        public BaseResponse AddOrUpdateOrgCodeStrokeFeilds([FromBody] List<OrgCodeStrokeFeildsVM> orgInhouseCodeFields)
        {
            try
            {
                return _activeCodesService.AddOrUpdateOrgCodeStrokeFeilds(orgInhouseCodeFields);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Get All Inhouse Code Feilds")]
        [HttpPost("InhouseCode/AddOrUpdateOrgCodeSTEMIFeilds")]
        public BaseResponse AddOrUpdateOrgCodeSTEMIFeilds([FromBody] List<OrgCodeSTEMIFeildsVM> orgInhouseCodeFields)
        {
            try
            {
                return _activeCodesService.AddOrUpdateOrgCodeSTEMIFeilds(orgInhouseCodeFields);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Get All Inhouse Code Feilds")]
        [HttpPost("InhouseCode/AddOrUpdateOrgCodeSepsisFeilds")]
        public BaseResponse AddOrUpdateOrgCodeSepsisFeilds(List<OrgCodeSepsisFeildsVM> orgInhouseCodeFields)
        {
            try
            {
                return _activeCodesService.AddOrUpdateOrgCodeSepsisFeilds(orgInhouseCodeFields);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Get All Inhouse Code Feilds")]
        [HttpPost("InhouseCode/AddOrUpdateOrgCodeTraumaFeilds")]
        public BaseResponse AddOrUpdateOrgCodeTraumaFeilds([FromBody] List<OrgCodeTraumaFeildsVM> orgInhouseCodeFields)
        {
            try
            {
                return _activeCodesService.AddOrUpdateOrgCodeTraumaFeilds(orgInhouseCodeFields);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Get All Inhouse Code Feilds")]
        [HttpPost("InhouseCode/AddOrUpdateOrgCodeBlueFeilds")]
        public BaseResponse AddOrUpdateOrgCodeBlueFeilds([FromBody] List<OrgCodeBlueFeildsVM> orgInhouseCodeFields)
        {
            try
            {
                return _activeCodesService.AddOrUpdateOrgCodeBlueFeilds(orgInhouseCodeFields);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Get All Inhouse Code Feilds")]
        [HttpGet("InhouseCode/GetInhouseCodeFormByOrgId/{OrgId}/{codeName}")]
        public BaseResponse GetInhouseCodeFormByOrgId(int orgId, string codeName)
        {
            try
            {
                return _activeCodesService.GetInhouseCodeFormByOrgId(orgId, codeName);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        #endregion


        #region Map & Addresses

        [HttpGet("map/GetHospitalsOfStatesByCodeId/{codeId}/{coordinates}")]
        public BaseResponse GetHospitalsOfStatesByCodeId(int codeId, string coordinates)
        {
            try
            {
                return _activeCodesService.GetHospitalsOfStatesByCodeId(codeId, coordinates);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        #endregion

    }
}
