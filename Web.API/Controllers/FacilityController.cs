using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Web.API.Helper;
using Web.Services.Interfaces;

namespace Web.API.Controllers
{
    public class FacilityController : Controller
    {
        private readonly IFacilityService _facilityService;
        private IConfiguration _config;
        Logger _logger;
        private IWebHostEnvironment _hostEnvironment;
        public FacilityController(IConfiguration config, IWebHostEnvironment environment, IFacilityService facilityService)
        {
            this._config = config;
            this._hostEnvironment = environment;
            this._logger = new Logger(this._hostEnvironment);
            this._facilityService = facilityService;
        }
    }
}
