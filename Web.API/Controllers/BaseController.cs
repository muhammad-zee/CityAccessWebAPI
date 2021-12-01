using Microsoft.AspNetCore.Mvc;

namespace Web.API.Controllers
{
    public class BaseController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

    }
}
