using System.Collections.Generic;
using System.Security.Claims;

namespace AspNetCore.Mvc.Extensions.Users
{
    public class MockUserService : IUserService
    {
        public MockUserService()
        {
            InitializeReturnValue();
        }

        private List<Claim> _Claims;
        private List<Claim> Claims
        {
            get
            {
                if (_Claims == null)
                {
                    _Claims = new List<Claim>();
                }

                return _Claims;
            }
        }

        public ClaimsPrincipal User { get; private set; }

        public string UserId { get; private set; }

        public string UserName { get; private set; }

        public void AddRole(string role)
        {
            AddClaim(ClaimTypes.Role, role);
        }

        public void AddClaim(string claimType, string claimValue)
        {
            Claims.Add(new Claim(claimType, claimValue));

            InitializeReturnValue();
        }

        private void InitializeReturnValue()
        {
            var identity = new ClaimsIdentity(Claims);

            User = new ClaimsPrincipal(identity);

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
