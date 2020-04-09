using System.ComponentModel.DataAnnotations;

namespace AspNetCore.Mvc.Extensions.Users
{
    public class AuthenticateDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
