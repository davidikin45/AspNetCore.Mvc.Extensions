using AspNetCore.Mvc.Extensions.Settings;
using AspNetCore.Mvc.Extensions.Validation;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Email
{
    public class EmailServiceSmtp : EmailServiceFileSystem
    {
        private EmailServiceFileSystem EmailServiceFileSystem;
        public EmailServiceSmtp(EmailSettings options, ILogger<EmailServiceSmtp> logger)
            :base(options, logger)
        {
            EmailServiceFileSystem = new EmailServiceFileSystem(options, logger);
        }

        public async override Task<Result> SendEmailMessageAsync(EmailMessage message, bool sendOverride)
        {
            //Send Smtp Email
            var result = await base.SendEmailMessageAsync(message, true);

            //May also want to save a copy to disk
            if(result.IsSuccess && Options.WriteEmailsToFileSystem)
            {
                await EmailServiceFileSystem.SendEmailMessageAsync(message);
            }

            return result;
        }

        public override SmtpClient CreateSmtpClient()
        {
            return new SmtpClient
            {
                Host = Options.Host,
                Port = Options.Port,
                EnableSsl = Options.Ssl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(Options.Username, Options.Password)
            };
        }
    }
}
