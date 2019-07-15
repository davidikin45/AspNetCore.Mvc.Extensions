using Microsoft.AspNetCore.Mvc;

namespace Localization.Areas.Products.Controllers
{
    [Route("products")]
    [Area("Products")]
    public class HomeController : Controller
    {
        [Route("")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("products")]
        public IActionResult Products()
        {
            return View();
        }
    }
}
