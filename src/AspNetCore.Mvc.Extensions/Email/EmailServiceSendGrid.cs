using AspNetCore.Mvc.Extensions.Settings;
using AspNetCore.Mvc.Extensions.Validation;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Net.Mail;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Email
{
    public class EmailServiceSendGrid : EmailServiceFileSystem
    {
        private EmailServiceFileSystem EmailServiceFileSystem;
        public EmailServiceSendGrid(EmailSettings options, ILogger<EmailServiceSmtp> logger)
            : base(options, logger)
        {
            EmailServiceFileSystem = new EmailServiceFileSystem(options, logger);
        }

        public async override Task<Result> SendEmailMessageAsync(EmailMessage message, bool sendOverride)
        {
            //Send Smtp Email
            var result = await base.SendEmailMessageAsync(message, true);

            //May also want to save a copy to disk
            if (result.IsSuccess && Options.WriteEmailsToFileSystem)
            {
                await EmailServiceFileSystem.SendEmailMessageAsync(message);
            }

            return result;
        }

        public async override Task SendMailAsync(MailMessage message, string html)
        {
            var client = new SendGridClient(Options.SendGridApiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress(message.From.Address, message.From.DisplayName),
                Subject = message.Subject,
                PlainTextContent = message.Body,
                HtmlContent = html
            };

            foreach (var to in message.To)
            {
                msg.AddTo(new EmailAddress(to.Address, to.DisplayName));
            }

            // Disable click tracking.
            // See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
            msg.SetClickTracking(false, false);

            await client.SendEmailAsync(msg);
        }

        public override SmtpClient CreateSmtpClient()
        {
            throw new NotImplementedException();
        }
    }
}
