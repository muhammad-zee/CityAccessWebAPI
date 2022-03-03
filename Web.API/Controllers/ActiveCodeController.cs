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
        [HttpGet("activecode/GetActivatedCodesByOrgId/{orgId}")]
        public BaseResponse GetActivatedCodesByOrgId(int orgId)
        {
            try
            {
                return _activeCodesService.GetActivatedCodesByOrgId(orgId);
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
        [HttpGet("activecode/DetachActiveCodes/{activeCodeId}")]
        public BaseResponse DetachActiveCodes(int activeCodeId)
        {
            try
            {
                return _activeCodesService.DetachActiveCodes(activeCodeId);
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
        [HttpGet("activecode/GetEMSandActiveCodesForDashboard/{OrgId}")]
        public BaseResponse GetEMSandActiveCodesForDashboard(int OrgId)
        {
            try
            {
                return _activeCodesService.GetEMSandActiveCodesForDashboard(OrgId);
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
        [HttpGet("stroke/DeleteStrokeDataById/{strokeId}")]
        public BaseResponse DeleteStrokeDataById(int strokeId)
        {
            try
            {
                return _activeCodesService.DeleteStroke(strokeId);
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
        [HttpGet("Sepsis/DeleteSepsisDataById/{SepsisId}")]
        public BaseResponse DeleteSepsisDataById(int SepsisId)
        {
            try
            {
                return _activeCodesService.DeleteSepsis(SepsisId);
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
        [HttpGet("STEMI/DeleteSTEMIDataById/{STEMIId}")]
        public BaseResponse DeleteSTEMIDataById(int STEMIId)
        {
            try
            {
                return _activeCodesService.DeleteSTEMI(STEMIId);
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
        [HttpGet("Truma/DeleteTrumaDataById/{TrumaId}")]
        public BaseResponse DeleteTrumaDataById(int TrumaId)
        {
            try
            {
                return _activeCodesService.DeleteTruma(TrumaId);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }

        #endregion

        [Description("Get All Active EMS")]
        [HttpGet("EMS/GetActiveEMS")]
        public BaseResponse GetActiveEMS() 
        {
            try
            {
                return _activeCodesService.GetActiveEMS();
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.BadRequest, Message = ex.ToString() };
            }
        }
        #region EMS


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
