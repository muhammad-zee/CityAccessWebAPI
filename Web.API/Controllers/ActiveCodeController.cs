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
using Web.Services.Enums;
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
        //[ValidateAntiForgeryToken]
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
        //[ValidateAntiForgeryToken]
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


        [Description("Update EMS Ambulance Data")]
        [HttpPost("EMS/UpdateEMSAmbulanceData")]
        public BaseResponse UpdateEMSAmbulanceData([FromBody] IDictionary<string, object> keyValues) 
        {
            try
            {
                return _activeCodesService.UpdateEMSAmbulanceData(keyValues);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        #region Code Stroke

        [Description("Get All Stroke Data")]
        [HttpPost("stroke/GetAllStrokeData")]
        public BaseResponse GetAllStrokeData([FromBody] ActiveCodeVM activeCode)
        {
            try
            {
                activeCode.CodeName = UCLEnums.Stroke.ToString();
                return _activeCodesService.GetAllCodeData(activeCode);
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
                return _activeCodesService.GetCodeDataById(strokeId, UCLEnums.Stroke.ToString());
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
        public BaseResponse AddOrUpdateStroke([FromBody] IDictionary<string, object> codeStroke)
        {
            try
            {
                return _activeCodesService.AddOrUpdateCodeData(codeStroke);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Create Group for Code Stroke")]
        [HttpPost("stroke/createGroupForCodeStroke")]

        public BaseResponse CreateStrokeGroup([FromBody] CodeStrokeVM codeStroke)
        {
            try
            {
                return _activeCodesService.CreateStrokeGroup(codeStroke);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Update Group for Code Stroke")]
        [HttpPost("stroke/UpdateGroupForCodeStroke")]

        public BaseResponse UpdateStrokeGroup([FromBody] CodeStrokeVM codeStroke)
        {
            try
            {
                return _activeCodesService.UpdateStrokeGroupMembers(codeStroke);
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
        //[ValidateAntiForgeryToken]
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
                activeCode.CodeName = UCLEnums.Sepsis.ToString();
                return _activeCodesService.GetAllCodeData(activeCode);
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
                return _activeCodesService.GetCodeDataById(SepsisId, UCLEnums.Sepsis.ToString());
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
        public BaseResponse AddOrUpdateSepsis([FromBody] IDictionary<string, object> codeSepsis)
        {
            try
            {
                var state = ModelState;
                return _activeCodesService.AddOrUpdateCodeData(codeSepsis);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Create Group for Code Sepsis")]
        [HttpPost("Sepsis/createGroupForCodeSepsis")]

        public BaseResponse CreateSepsisGroup([FromBody] CodeSepsisVM codeSepsis)
        {
            try
            {
                return _activeCodesService.CreateSepsisGroup(codeSepsis);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Update Group for Code Sepsis")]
        [HttpPost("Sepsis/UpdateGroupForCodeSepsis")]

        public BaseResponse UpdateSepsisGroup([FromBody] CodeSepsisVM codeSepsis)
        {
            try
            {
                return _activeCodesService.UpdateSepsisGroupMembers(codeSepsis);
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
       // [ValidateAntiForgeryToken]
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
                activeCode.CodeName = UCLEnums.Stemi.ToString();
                return _activeCodesService.GetAllCodeData(activeCode);
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
                return _activeCodesService.GetCodeDataById(STEMIId, UCLEnums.Stemi.ToString());
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

        public BaseResponse AddOrUpdateSTEMI([FromBody] IDictionary<string, object> codeSTEMI)
        {
            try
            {
                return _activeCodesService.AddOrUpdateCodeData(codeSTEMI);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Create Group for Code STEMI")]
        [HttpPost("STEMI/createGroupForCodeSTEMI")]
        public BaseResponse CreateSTEMIGroup([FromBody] CodeSTEMIVM codeSTEMI)
        {
            try
            {
                return _activeCodesService.CreateSTEMIGroup(codeSTEMI);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Update Group for Code STEMI")]
        [HttpPost("STEMI/UpdateGroupForCodeSTEMI")]

        public BaseResponse UpdateSTEMIGroup([FromBody] CodeSTEMIVM codeSTEMI)
        {
            try
            {
                return _activeCodesService.UpdateSTEMIGroupMembers(codeSTEMI);
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
        //[ValidateAntiForgeryToken]
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

        #region Code Trauma

        [Description("Get All Trauma Data")]
        [HttpPost("Trauma/GetAllTraumaData")]
        public BaseResponse GetAllTraumaData([FromBody] ActiveCodeVM activeCode)
        {
            try
            {
                activeCode.CodeName = UCLEnums.Trauma.ToString();
                return _activeCodesService.GetAllCodeData(activeCode);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex); _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Get Trauma Data By Id")]
        [HttpGet("Trauma/GetTraumaDataById/{TraumaId}")]
        public BaseResponse GetTraumaDataById(int TraumaId)
        {
            try
            {
                return _activeCodesService.GetCodeDataById(TraumaId, UCLEnums.Trauma.ToString());
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Add Or Update Trauma")]
        [HttpPost("Trauma/AddOrUpdateTrauma")]
        public BaseResponse AddOrUpdateTrauma([FromBody] IDictionary<string, object> codeTrauma)
        {
            try
            {
                return _activeCodesService.AddOrUpdateCodeData(codeTrauma);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Create Group for Code Trauma")]
        [HttpPost("Trauma/createGroupForCodeTrauma")]
        public BaseResponse CreateTraumaGroup([FromBody] CodeTrumaVM codeTrauma)
        {
            try
            {
                return _activeCodesService.CreateTrumaGroup(codeTrauma);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Update Group for Code Trauma")]
        [HttpPost("Trauma/UpdateGroupForCodeTrauma")]

        public BaseResponse UpdateTrumaGroup([FromBody] CodeTrumaVM codeTruma)
        {
            try
            {
                return _activeCodesService.UpdateTrumaGroupMembers(codeTruma);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Delete Trauma Data By Id")]
        [HttpGet("Trauma/DeleteTraumaDataById/{TraumaId}/{status}")]
       // [ValidateAntiForgeryToken]
        public BaseResponse DeleteTraumaDataById(int TraumaId, bool status)
        {
            try
            {
                return _activeCodesService.DeleteTruma(TraumaId, status);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Active/InActive Trauma Data By Id")]
        [HttpGet("Trauma/ActiveOrInActiveTraumaDataById/{TraumaId}/{status}")]

        public BaseResponse ActiveOrInActiveTraumaDataById(int TraumaId, bool status)
        {
            try
            {
                return _activeCodesService.ActiveOrInActiveTruma(TraumaId, status);
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
                activeCode.CodeName = UCLEnums.Blue.ToString();
                return _activeCodesService.GetAllCodeData(activeCode);
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
                return _activeCodesService.GetCodeDataById(blueId, UCLEnums.Blue.ToString());
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
        public BaseResponse AddOrUpdateBlue([FromBody] IDictionary<string, object> codeBlue)
        {
            try
            {
                return _activeCodesService.AddOrUpdateCodeData(codeBlue);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Create Group for Code Blue")]
        [HttpPost("Blue/createGroupForCodeBlue")]

        public BaseResponse CreateBlueGroup([FromBody] CodeBlueVM codeBlue)
        {
            try
            {
                return _activeCodesService.CreateBlueGroup(codeBlue);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        [Description("Update Group for Code Blue")]
        [HttpPost("Blue/UpdateGroupForCodeBlue")]

        public BaseResponse UpdateBlueGroup([FromBody] CodeBlueVM codeBlue)
        {
            try
            {
                return _activeCodesService.UpdateBlueGroupMembers(codeBlue);
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
       // [ValidateAntiForgeryToken]
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
        [HttpGet("EMS/GetActiveEMS/{orgId}/{showAll}/{fromDashbord}")]
        public BaseResponse GetActiveEMS(int orgId, bool showAll, bool fromDashbord = false)
        {
            try
            {
                return _activeCodesService.GetActiveEMS(orgId, showAll, fromDashbord);
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
        //[ValidateAntiForgeryToken]
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

        public BaseResponse AddOrUpdateOrgCodeSepsisFeilds([FromBody] List<OrgCodeSepsisFeildsVM> orgInhouseCodeFields)
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

        #region Apis For Mobile App

        [Description("Get All Codes Data")]
        [HttpPost("activecode/GetAllCodesData")]
        //[ValidateAntiForgeryToken]
        public BaseResponse GetAllCodesData([FromBody] ActiveCodeVM activeCode)
        {
            try
            {
                var m_state = ModelState;
                return _activeCodesService.GetAllCodesData(activeCode);
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
