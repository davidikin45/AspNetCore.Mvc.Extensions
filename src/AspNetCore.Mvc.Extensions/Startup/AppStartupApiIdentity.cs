using AspNetCore.Mvc.Extensions.Settings;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AspNetCore.Mvc.Extensions
{
    public abstract class AppStartupApiIdentity<TIdentiyDbContext, TUser> : AppStartup
        where TIdentiyDbContext : DbContext
        where TUser : IdentityUser
    {
        public AppStartupApiIdentity(IConfiguration configuration, IWebHostEnvironment hostingEnvironment, Action<AppStartupOptions> config = null)
            :base(configuration, hostingEnvironment, config)
        {

        }

        public override void ConfigureIdentityServices(IServiceCollection services)
        {
            base.ConfigureIdentityServices(services);

            //This will override the authentication scheme

            var passwordSettings = GetSettings<PasswordSettings>("PasswordSettings");
            var userSettings = GetSettings<UserSettings>("UserSettings");
            var authenticationSettings = GetSettings<AuthenticationSettings>("AuthenticationSettings");

            if (authenticationSettings.Basic.Enable || authenticationSettings.JwtToken.Enable)
            {
                //https://github.com/aspnet/Identity/blob/8ef14785a4a1e416189ca1137eb13f43c2f4349d/src/Identity/IdentityServiceCollectionExtensions.cs
                //User AddIdentityCore if using identity with Api only.

                if(authenticationSettings.Basic.Enable)
                {
                    services.AddBasicAuth<TUser>();
                }

                //Should use services.AddIdentity OR services.AddAuthentication
                services.AddIdentityCore<TIdentiyDbContext, TUser, IdentityRole>(
                passwordSettings.MaxFailedAccessAttemptsBeforeLockout,
                passwordSettings.LockoutMinutes,
                passwordSettings.RequireDigit,
                passwordSettings.RequiredLength,
                passwordSettings.RequiredUniqueChars,
                passwordSettings.RequireLowercase,
                passwordSettings.RequireNonAlphanumeric,
                passwordSettings.RequireUppercase,
                userSettings.RequireConfirmedEmail,
                userSettings.RequireUniqueEmail,
                userSettings.RegistrationEmailConfirmationExprireDays,
                userSettings.ForgotPasswordEmailConfirmationExpireHours,
                userSettings.UserDetailsChangeLogoutMinutes);

                //Use cookie authentication without ASP.NET Core Identity
                //https://docs.microsoft.com/en-us/aspnet/core/security/authentication/cookie?view=aspnetcore-2.1&tabs=aspnetcore2x
                //https://wildermuth.com/2018/04/10/Using-JwtBearer-Authentication-in-an-API-only-ASP-NET-Core-Project
            }
        }
    }
}
