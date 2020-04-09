using AspNetCore.Mvc.Extensions.Email;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace DND.Web.Extensions
{
    public static class EmailSenderExtensions
    {
        public static Task SendEmailConfirmationAsync(this IEmailService emailSender, string email, string link)
        {
            return emailSender.SendEmailAsync(email, "Confirm your email",
                $"Please confirm your account by clicking this link: <a href='{HtmlEncoder.Default.Encode(link)}'>link</a>", true);
        }
    }
}
