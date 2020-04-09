using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;

namespace AspNetCore.Mvc.Extensions.Users
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClaimsPrincipal User { get; }
        public string UserId { get; } = null;
        public string UserName { get; } = null;

        public UserService(IHttpContextAccessor httpContextAccessor)
        {
            // service is scoped, created once for each request => we only need
            // to fetch the info in the constructor
            _httpContextAccessor = httpContextAccessor
                ?? throw new ArgumentNullException(nameof(httpContextAccessor));

            var currentContext = _httpContextAccessor.HttpContext;
            if (currentContext == null)
            {
                return;
            }

            User = currentContext.User;

            if (!User.Identity.IsAuthenticated)
            {
                return;
            }

            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
                UserId = claim.Value;
            }

            claim = User.FindFirst(ClaimTypes.Name);
            if (claim != null)
            {
                UserName = claim.Value;
            }
        }
    }
}
