using AspNetCore.Mvc.Extensions.Validation;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Email
{
    public interface IEmailService
    {
        Task<Result> SendEmailMessageAsync(EmailMessage message, bool sendOverride = false);
        Task<Result> SendEmailAsync(string email, string subject, string message, string htmlMessage);
        bool SendEmailMessages(IList<EmailMessage> messages);
        Task<Result> SendEmailMessageToAdminAsync(EmailMessage message);
    }
}
