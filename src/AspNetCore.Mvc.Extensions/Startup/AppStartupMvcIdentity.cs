using AspNetCore.Mvc.Extensions.Settings;
using EntityFrameworkCore.Initialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace AspNetCore.Mvc.Extensions
{
    public abstract class AppStartupMvcIdentity<TIdentiyDbContext, TUser> : AppStartup
        where TIdentiyDbContext : DbContext
        where TUser : IdentityUser
    {
        public AppStartupMvcIdentity(IConfiguration configuration, IWebHostEnvironment hostingEnvironment, Action<AppStartupOptions> config = null)
            :base(configuration, hostingEnvironment, config)
        {
            var types = new Type[] {
                typeof(Microsoft.IdentityModel.Protocols.AuthenticationProtocolMessage),
                typeof(Microsoft.IdentityModel.Protocols.OpenIdConnect.ActiveDirectoryOpenIdConnectEndpoints)
                };
        }

        public override void ConfigureIdentityServices(IServiceCollection services)
        {
            base.ConfigureIdentityServices(services);

            //This will override the authentication scheme

            var appSettings = GetSettings<AppSettings>("AppSettings");
            var passwordSettings = GetSettings<PasswordSettings>("PasswordSettings");
            var userSettings = GetSettings<UserSettings>("UserSettings");
            var authenticationSettings = GetSettings<AuthenticationSettings>("AuthenticationSettings");

            if (authenticationSettings.Basic.Enable || authenticationSettings.Application.Enable || authenticationSettings.JwtToken.Enable)
            {
                //https://github.com/aspnet/Identity/blob/8ef14785a4a1e416189ca1137eb13f43c2f4349d/src/Identity/IdentityServiceCollectionExtensions.cs
                //User AddIdentityCore if using identity with Api only.

                //Sets the following values
                //https://github.com/aspnet/announcements/issues/262
                //DefaultScheme: if specified, DefaultAuthenticateScheme, DefaultChallengeScheme and DefaultSignInScheme will fallback to this value.
                //DefaultAuthenticateScheme: How claims principal gets read/reconstructed on every request. If specified, AuthenticateAsync() will use this scheme, and also the AuthenticationMiddleware added by UseAuthentication() will use this scheme to set context.User automatically. (Corresponds to AutomaticAuthentication)
                //DefaultChallengeScheme: What happens when user tries to access a resource where authorization is required. e.g Redirect to Sign In. If specified, ChallengeAsync() will use this scheme, [Authorize] with policies that don't specify schemes will also use this
                //DefaultSignInScheme:  Persists claims principal to Cookie. to Is used by SignInAsync() and also by all of the remote auth schemes like Google/Facebook/OIDC/OAuth, typically this would be set to a cookie.
                //DefaultSignOutScheme: Deletes Cookie. is used by SignOutAsync() falls back to DefaultSignInScheme
                //DefaultForbidScheme: What happens when a user accesses a resource where authorization fails. e.g Redirect to Access Denied. is used by ForbidAsync(), falls back to DefaultChallengeScheme

                bool constants = false;
                if (constants)
                {
                    services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme; //"Identity.Application" - How claims principal gets reconstructed on every request
                        options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme; //"Identity.Application" - What happens when user tries to access a resource where authorization is required. e.g Redirect to Sign In
                        options.DefaultSignInScheme = IdentityConstants.ExternalScheme; //"Identity.External"

                        options.DefaultForbidScheme = IdentityConstants.ApplicationScheme; //"Identity.Application" - What happens when a user accesses a resource where authorization fails. e.g Redirect to Access Denied
                        options.DefaultSignOutScheme = IdentityConstants.ExternalScheme; //"Identity.External"
                    });
                }

                if (authenticationSettings.Basic.Enable)
                {
                    services.AddBasicAuth<TUser>();
                }

                //Should use services.AddIdentity OR services.AddAuthentication
                services.AddIdentity<TIdentiyDbContext, TUser, IdentityRole>(
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

                services.ConfigureApplicationCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.Cookie.Name = appSettings.CookieAuthName;
                });

                services.ConfigureExternalCookie(options =>
                {
                    options.Cookie.Name = appSettings.CookieExternalAuthName;
                });

                //Use cookie authentication without ASP.NET Core Identity
                //https://docs.microsoft.com/en-us/aspnet/core/security/authentication/cookie?view=aspnetcore-2.1&tabs=aspnetcore2x
                //https://wildermuth.com/2018/04/10/Using-JwtBearer-Authentication-in-an-API-only-ASP-NET-Core-Project
            }

        }

        public override void AddDatabases(IServiceCollection services, ConnectionStrings connectionStrings, string identityConnectionString, string hangfireConnectionString, string defaultConnectionString)
        {
            services.AddDbContext<TIdentiyDbContext>(identityConnectionString);
        }
    }
}
