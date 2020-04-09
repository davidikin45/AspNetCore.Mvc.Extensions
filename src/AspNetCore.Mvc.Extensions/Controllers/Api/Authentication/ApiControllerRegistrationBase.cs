using AspNetCore.Mvc.Extensions.Authorization;
using AspNetCore.Mvc.Extensions.Context;
using AspNetCore.Mvc.Extensions.Email;
using AspNetCore.Mvc.Extensions.Settings;
using AspNetCore.Mvc.Extensions.Users;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Controllers.Api.Authentication
{
    [ResourceCollection(ResourceCollectionsCore.Auth.Name)]
    public abstract class ApiControllerRegistrationBase<TUser, TRegistrationDto> : ApiControllerAuthenticationBase<TUser>
        where TUser : IdentityUser
        where TRegistrationDto : RegisterDtoBase
    {
        private readonly string _welcomeEmailTemplate;

        public ApiControllerRegistrationBase(
            ControllerServicesContext context,
            RoleManager<IdentityRole> roleManager,
            UserManager<TUser> userManager,
            SignInManager<TUser> signInManager,
            TokenSettings tokenSettings,
            PasswordSettings passwordSettings,
            EmailTemplates emailTemplates)
            :base(context, roleManager, userManager, signInManager, tokenSettings, passwordSettings, emailTemplates)
        {
            _welcomeEmailTemplate = emailTemplates.Welcome;
        }

        #region Register
        [ResourceAuthorize(ResourceCollectionsCore.Auth.Operations.Register)]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] TRegistrationDto registerDto)
        {
            var user = Mapper.Map<TUser>(registerDto);
            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(_welcomeEmailTemplate))
                {
                    await EmailService.SendWelcomeEmailAsync(_welcomeEmailTemplate, user.Email);
                }
                return await GenerateJWTToken(user);
            }
            AddErrors(result);
            return ValidationErrors(ModelState);
        }
        #endregion
    }
}
