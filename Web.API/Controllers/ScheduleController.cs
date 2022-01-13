using ElmahCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Web.API.Helper;
using Web.Model;
using Web.Model.Common;
using Web.Services;
using Web.Services.Interfaces;

namespace Web.API.Controllers
{
    [Authorize]
    [RequestHandler]
    public class ScheduleController : Controller
    {
        private readonly IScheduleService _scheduleService;
        private IConfiguration _config;
        Logger _logger;
        private IWebHostEnvironment _hostEnvironment;

        public ScheduleController(IConfiguration config, IWebHostEnvironment environment, IScheduleService scheduleService)
        {
            this._config = config;
            this._hostEnvironment = environment;
            this._logger = new Logger(this._hostEnvironment);
            this._scheduleService = scheduleService;
        }


        [AllowAnonymous]
        [Description("Load Schedule")]
        [Route("Schedule/LoadSchedule")]
        [HttpPost, DisableRequestSizeLimit]
        public ActionResult LoadSchedule([FromBody] EditParams param)
        {
            try
            {
                var model = ModelState;
                var response = _scheduleService.getSchedule(param);

                return Json(response.Body);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return Json("");
            }
        }

        [AllowAnonymous]
        [Description("Load Schedule")]
        [Route("Schedule/AddOrUpdateScheduleFromSchedule")]
        [HttpPost, DisableRequestSizeLimit]
        public ActionResult AddOrUpdateScheduleFromSchedule([FromBody] EditParams param)
        {
            try
            {
                BaseResponse response = this._scheduleService.AddUpdateUserSchedule(param);
                var jsonResponse = Json(response.Body);
                return jsonResponse;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return Json(param);
            }
        }


        [Description("Get Schedule")]
        [Route("Schedule/GetScheduleList")]
        [HttpPost, DisableRequestSizeLimit]
        public BaseResponse GetScheduleList([FromBody] ScheduleVM param)
        {
            try
            {
                var model = ModelState;
                var response = _scheduleService.GetScheduleList(param);
                return response;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse()
                {
                    Status = HttpStatusCode.BadRequest,
                    Message = ex.ToString(),
                    Body = null
                };
            }
        }

        [Description("Add Or Update Schedule")]
        [Route("Schedule/AddOrUpdateSchedule")]
        [HttpPost, DisableRequestSizeLimit]
        public BaseResponse UpdateSchedule([FromBody] ScheduleVM param)
        {
            try
            {
                var model = ModelState;
                var response = _scheduleService.SaveSchedule(param);
                return response;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse()
                {
                    Status = HttpStatusCode.BadRequest,
                    Message = ex.ToString(),
                    Body = null
                };
            }
        }

        [Description("Import Schedule")]
        [Route("Schedule/Upload")]
        [HttpPost, DisableRequestSizeLimit]
        public async Task<BaseResponse> Upload([FromBody] ImportCSVFileVM fileVM)
        {
            try
            {
                var response = new BaseResponse();
                //var file = Request.Form.Files[0];

                if (!string.IsNullOrEmpty(fileVM.Base64CSV))
                {
                    var folderName = Path.Combine("ImportSchecdule");
                    var pathToSave = Path.Combine(this._hostEnvironment.WebRootPath, folderName);
                    if (!Directory.Exists(pathToSave))
                    {
                        Directory.CreateDirectory(pathToSave);
                    }
                    var fileName = "ImportSchecduleFile_" + DateTime.UtcNow.ToString("MMddyyyyhhmmss") + ".csv"; //ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    var fullPath = Path.Combine(pathToSave, fileName);
                    var dbPath = Path.Combine(folderName, fileName);
                    var fileInBytes = Convert.FromBase64String(fileVM.Base64CSV.Split("base64,")[1]);
                    using (var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
                    {
                        // file.CopyTo(stream);
                        stream.Write(fileInBytes);
                    }
                    fileVM.FilePath = fullPath;

                    response = _scheduleService.ImportCSV(fileVM);
                    return response;
                }
                else
                {
                    return response;
                }
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse()
                {
                    Status = HttpStatusCode.BadRequest,
                    Message = ex.ToString(),
                    Body = null
                };
            }
        }


        [Description("Get Schedule Template")]
        [HttpGet("Schedule/GetScheduleTemplate/{serviceLineId}")]
        public BaseResponse GetScheduleTemplate(int serviceLineId)
        {
            try
            {
                var response = this._scheduleService.GetScheduleTemplate(serviceLineId);
                return response;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse()
                {
                    Status = HttpStatusCode.BadRequest,
                    Message = ex.ToString(),
                    Body = null
                };
            }
        }

        [Description("Delete Schedule")]
        [HttpGet("Schedule/DeleteSchedule/{scheduleId}/{userId}")]
        public BaseResponse DeleteSchedule(int scheduleId, int userId)
        {
            try
            {
                var response = this._scheduleService.DeleteSchedule(scheduleId, userId);
                return response;
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return new BaseResponse()
                {
                    Status = HttpStatusCode.BadRequest,
                    Message = ex.ToString(),
                    Body = null
                };
            }
        }
    }
}
