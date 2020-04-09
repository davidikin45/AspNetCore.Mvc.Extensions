using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;

//https://www.blinkingcaret.com/2018/11/29/asp-net-core-web-api-antiforgery/
//https://dotnetthoughts.net/anti-forgery-validation-with-aspdotnet-core-and-angular/
namespace AspNetCore.Mvc.Extensions.Controllers.AntiForgery
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/antiforgery")]
    [ApiController]
    public class AntiForgeryController : Controller
    {
        private IAntiforgery _antiForgery;
        public AntiForgeryController(IAntiforgery antiForgery)
        {
            _antiForgery = antiForgery;
        }

        [HttpGet("")]
        [IgnoreAntiforgeryToken]
        public IActionResult GenerateAntiForgeryTokens()
        {
            var tokens = _antiForgery.GetAndStoreTokens(HttpContext);
            Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken, new Microsoft.AspNetCore.Http.CookieOptions
            {
                HttpOnly = false,
                IsEssential = true
            });
            return NoContent();
        }
    }
}