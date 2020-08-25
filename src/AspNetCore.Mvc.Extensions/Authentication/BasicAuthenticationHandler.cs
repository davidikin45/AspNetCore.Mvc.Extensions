using AspNetCore.Mvc.Extensions.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Authentication
{
    public class BasicAuthenticationDefaults
    {
        public const string AuthenticationScheme = "Basic";
    }

    public class BasicAuthenticationHandler<TUser> : BasicAuthenticationHandler
   where TUser : IdentityUser
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<TUser> _userManager;
        private readonly SignInManager<TUser> _signInManager;

        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            RoleManager<IdentityRole> roleManager,
            UserManager<TUser> userManager,
            SignInManager<TUser> signInManager)
            : base(options, logger, encoder, clock)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        protected override async Task<ClaimsIdentity> GetIdentityAsync(string username, string password)
        {
            var user = await _userManager.FindByEmailAsync(username);
            if (user != null)
            {
                var result = await _signInManager.CheckPasswordSignInAsync(user, password, true);

                if (result.Succeeded)
                {
                    var rolesAndScopes = await AuthenticationHelper.GetRolesAndScopesAsync(user, _userManager, _roleManager);
                    var roles = rolesAndScopes.Roles;
                    var scopes = rolesAndScopes.Scopes;

                    var claims = JwtTokenHelper.GetClaims(user.Id, user.UserName, user.Email, roles, scopes.ToArray());
                    var identity = new ClaimsIdentity(claims, Scheme.Name);
                    return identity;
                }
            }

            return null;
        }
    }

    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization") || !Request.Headers["Authorization"].ToString().StartsWith("Basic "))
            {
                return AuthenticateResult.Fail("Missing Authorization header");
            }

            try
            {
                var authenticationHeader = AuthenticationHeaderValue.Parse(
                    Request.Headers["Authorization"]);
                var credentialBytes = Convert.FromBase64String(authenticationHeader.Parameter);
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':');
                var username = credentials[0];
                var password = credentials[1];

                var identity = await GetIdentityAsync(username, password);
                if (identity != null)
                {
                    var principal = new ClaimsPrincipal(identity);
                    var ticket = new AuthenticationTicket(principal, Scheme.Name);

                    return AuthenticateResult.Success(ticket);
                }

                return AuthenticateResult.Fail("Invalid username or password");
            }
            catch
            {
                return AuthenticateResult.Fail("Invalid Authorization header");
            }
        }

        protected virtual Task<ClaimsIdentity> GetIdentityAsync(string username, string password)
        {
            if (username.Equals("username", StringComparison.InvariantCultureIgnoreCase) && password.Equals("password"))
            {
                var claims = new[] {
                        new Claim(ClaimTypes.NameIdentifier, username)
                };
                var identity = new ClaimsIdentity(claims, Scheme.Name);
                return Task.FromResult(identity);
            }

            return null;
        }
    }
}
