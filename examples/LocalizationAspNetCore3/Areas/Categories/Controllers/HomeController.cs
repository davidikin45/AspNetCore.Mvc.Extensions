using Microsoft.AspNetCore.Mvc;

namespace LocalizationAspNetCore3.Areas.Categories.Controllers
{
    [Route("categories")]
    [Area("Categories")]
    public class HomeController : Controller
    {
        [Route("")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
