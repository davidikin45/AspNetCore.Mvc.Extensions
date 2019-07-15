using Microsoft.AspNetCore.Mvc;

namespace LocalizationAspNetCore3.Areas.Sales.Controllers
{
    [Route("sales")]
    [Area("Sales")]
    public class HomeController : Controller
    {
        [Route("")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
