using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Email
{
    public static class EmailTemplateCallbackExtensions
    {
        public static async Task SendWelcomeEmailAsync(this IEmailService emailService, string templatePath, string email)
        {
            var emailHtml = File.ReadAllText(templatePath);
            await emailService.SendEmailAsync(email, "Welcome", "", emailHtml);
        }

        public static async Task SendResetPasswordEmailAsync(this IEmailService emailService, IUrlHelper urlHelper, string templatePath, string email, string callbackUrl, string userId, string code, string scheme)
        {
            //var callbackUrl = urlHelper.ResetPasswordCallbackLink(userId, code, scheme);
            callbackUrl = $"{callbackUrl}?email={email}&code={WebUtility.UrlEncode(code)}";
            var emailHtml = File.ReadAllText(templatePath).Replace("{callbackUrl}", callbackUrl);
            await emailService.SendEmailAsync(email, "Reset Password", "", emailHtml);
        }
    }
}
