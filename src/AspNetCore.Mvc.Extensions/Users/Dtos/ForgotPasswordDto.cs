using System.ComponentModel.DataAnnotations;

namespace AspNetCore.Mvc.Extensions.Users
{
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
