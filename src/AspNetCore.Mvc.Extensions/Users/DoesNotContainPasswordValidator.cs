using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Users
{
    public class DoesNotContainPasswordValidator<TUser> : IPasswordValidator<TUser> where TUser : class
    {

        public async Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user, string password)
        {
            var username = await manager.GetUserNameAsync(user);

            if (username == password)
                return IdentityResult.Failed(new IdentityError { Description = "Password cannot contain username" });
            if (password.Contains("password"))
                return IdentityResult.Failed(new IdentityError { Description = "Password cannot contain password" });

            return IdentityResult.Success;
        }
    }
}
