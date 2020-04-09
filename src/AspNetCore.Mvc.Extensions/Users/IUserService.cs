using System.Security.Claims;

namespace AspNetCore.Mvc.Extensions.Users
{
    public interface IUserService
    {
        ClaimsPrincipal User { get; }
        string UserId { get; }
        string UserName { get; }
    }
}
