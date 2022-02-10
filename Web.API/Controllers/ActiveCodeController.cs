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
            _logger = new Logger(_hostEnvironment);
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
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = ex.ToString() };
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
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = ex.ToString() };
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
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = ex.ToString() };
            }
        }

        #region Code Stroke

        [Description("Get All Stroke Data")]
        [HttpGet("stroke/GetAllStrokeData")]
        public BaseResponse GetAllStrokeData()
        {
            try
            {
                return _activeCodesService.GetAllStrokeCode();
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = ex.ToString() };
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
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = ex.ToString() };
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
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = ex.ToString() };
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
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = ex.ToString() };
            }
        }

        #endregion


        #region Code Sepsis

        [Description("Get All Sepsis Data")]
        [HttpGet("Sepsis/GetAllSepsisData")]
        public BaseResponse GetAllSepsisData()
        {
            try
            {
                return _activeCodesService.GetAllSepsisCode();
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = ex.ToString() };
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
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = ex.ToString() };
            }
        }


        [Description("Add Or Update Sepsis")]
        [HttpPost("Sepsis/AddOrUpdateSepsis")]
        public BaseResponse AddOrUpdateSepsis([FromBody] CodeSepsisVM codeSepsis)
        {
            try
            {
                return _activeCodesService.AddOrUpdateSepsisData(codeSepsis);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = ex.ToString() };
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
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = ex.ToString() };
            }
        }

        #endregion


        #region Code STEMI

        [Description("Get All STEMI Data")]
        [HttpGet("STEMI/GetAllSTEMIData")]
        public BaseResponse GetAllSTEMIData()
        {
            try
            {
                return _activeCodesService.GetAllSTEMICode();
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = ex.ToString() };
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
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = ex.ToString() };
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
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = ex.ToString() };
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
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = ex.ToString() };
            }
        }

        #endregion


        #region Code Truma

        [Description("Get All Truma Data")]
        [HttpGet("Truma/GetAllTrumaData")]
        public BaseResponse GetAllTrumaData()
        {
            try
            {
                return _activeCodesService.GetAllTrumaCode();
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = ex.ToString() };
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
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = ex.ToString() };
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
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = ex.ToString() };
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
                return new BaseResponse() { Status = HttpStatusCode.OK, Message = ex.ToString() };
            }
        }

        #endregion
    }
}
