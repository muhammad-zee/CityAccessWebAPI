using ElmahCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Web.API.Helper;
using Web.Model;
using Web.Model.Common;
using Web.Services.Interfaces;

namespace Web.API.Controllers
{
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



        [Description("Load Schedule")]
        [Route("Schedule/LoadSchedule")]
        [HttpPost, DisableRequestSizeLimit]
        public ActionResult LoadSchedule([FromBody] EditParams param, bool readOnly = false)
        {
            try
            {
                List<ScheduleEventData> _list = new List<ScheduleEventData>();
                ScheduleEventData obj = new ScheduleEventData();

                obj.Id = 1;
                obj.Subject = "Muhammad Masud";
                obj.IsAllDay = false;
                obj.StartTime = DateTime.Now;
                obj.EndTime = DateTime.Now.AddHours(8);
                obj.OwnerId = 1;
                _list.Add(obj);

                return Json(_list);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return Json("");
            }
        }

        [Description("Update Schedule")]
        [Route("Schedule/UpdateSchedule")]
        [HttpPost, DisableRequestSizeLimit]
        public ActionResult UpdateSchedule([FromBody] EditParams param)
        {
            try
            {
                List<ScheduleEventData> list = new List<ScheduleEventData>();
                ScheduleEventData obj = new ScheduleEventData();

                obj.Id = 1;
                obj.Subject = "Muhammad Masud";
                obj.IsAllDay = false;
                obj.StartTime = DateTime.Now;
                obj.EndTime = DateTime.Now.AddHours(10);
                obj.OwnerId = 1;
                list.Add(obj);

                return Json(list);
            }
            catch (Exception ex)
            {
                ElmahExtensions.RiseError(ex);
                _logger.LogExceptions(ex);
                return Json("");
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
    }
}
