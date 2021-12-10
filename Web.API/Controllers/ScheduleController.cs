using ElmahCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Web.API.Helper;
using Web.Model;
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

        [Description("Import Schedule")]
        [Route("Schedule/Upload")]
        [HttpPost, DisableRequestSizeLimit]
        public async Task<BaseResponse> Upload()
        {
            try
            {
                var response = new BaseResponse();
                var file = Request.Form.Files[0];
                var folderName = Path.Combine("ImportSchecdule");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
                if (file.Length > 0)
                {
                    var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    var fullPath = Path.Combine(pathToSave, fileName);
                    var dbPath = Path.Combine(folderName, fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                    response.Status = HttpStatusCode.OK;
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
