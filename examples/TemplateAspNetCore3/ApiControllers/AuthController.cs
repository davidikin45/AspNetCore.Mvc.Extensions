using AspNetCore.Mvc.Extensions.Controllers.Api.Authentication;
using AspNetCore.Mvc.Extensions.Email;
using AspNetCore.Mvc.Extensions.Settings;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using TemplateAspNetCore3.Models;

namespace TemplateAspNetCore3.ApiControllers
{
    [AllowAnonymous]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/auth")]
    public class AuthController : ApiControllerAuthenticationBase<User>
    {
        public AuthController(
            RoleManager<IdentityRole> roleManager,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            TokenSettings tokenSettings,
            LinkGenerator linkGenerator,
            IEmailService emailSender,
            IMapper mapper,
            PasswordSettings passwordSettings,
            EmailTemplates emailTemplates,
            AppSettings appSettings)
            : base(roleManager, userManager, signInManager, tokenSettings, linkGenerator, emailSender, mapper, passwordSettings, emailTemplates, appSettings)
        {

        }
    }
}
