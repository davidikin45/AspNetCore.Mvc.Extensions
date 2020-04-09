using AspNetCore.Mvc.Extensions.Settings;
using AspNetCore.Mvc.Extensions.Validation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Email
{
    public class EmailServiceFileSystem : IEmailService
    {
        protected EmailSettings Options;
        private readonly ILogger _logger;

        public EmailServiceFileSystem(EmailSettings options, ILogger<EmailServiceFileSystem> logger)
        {
            Options = options;
            _logger = logger;
        }

        public async Task<Result> SendEmailMessageToAdminAsync(EmailMessage message)
        {
            message.ToEmail = Options.ToEmail;
            message.ToDisplayName = Options.ToDisplayName;
            return await SendEmailMessageAsync(message).ConfigureAwait(false);
        }


        public async Task<Result> SendEmailAsync(string email, string subject, string message, string htmlMessage)
        {
            var emailMessage = new EmailMessage
            {
                ToEmail = email,
                Subject = subject,
                PlainBody = message,
                HtmlBody = htmlMessage
            };

            return await SendEmailMessageAsync(emailMessage).ConfigureAwait(false);
        }

        public virtual async Task<Result> SendEmailMessageAsync(EmailMessage message, bool sendOverride = false)
        {
            try
            {
                if (Options.WriteEmailsToFileSystem || sendOverride)
                {
                    var from = new MailAddress(Options.FromEmail, Options.FromDisplayName);
                    var to = new MailAddress(message.ToEmail, message.ToDisplayName);

                    using (var smtpMessage = new MailMessage(from, to))
                    {
                        smtpMessage.Subject = message.Subject;
                        smtpMessage.SubjectEncoding = Encoding.UTF8;
                        smtpMessage.Body = message.PlainBody;
                        smtpMessage.IsBodyHtml = false;
                        smtpMessage.BodyEncoding = Encoding.UTF8;

                        if (!string.IsNullOrEmpty(message.ReplyEmail))
                        {
                            smtpMessage.ReplyToList.Add(new MailAddress(message.ReplyEmail, message.ReplyDisplayName));
                        }

                        await SendMailAsync(smtpMessage, message.HtmlBody);
                    }
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Email to {message.ToEmail} failed to send.");
            }

            return Result.Fail(ErrorType.EmailSendFailed);
        }

        public virtual async Task SendMailAsync(MailMessage message, string html)
        {
            if(!string.IsNullOrEmpty(html))
            {
                message.Body = html;
                message.IsBodyHtml = true;
            }

            using (var smtp = CreateSmtpClient())
            {
                await smtp.SendMailAsync(message).ConfigureAwait(false);

            }
        }

        public virtual SmtpClient CreateSmtpClient()
        {
            if (!Directory.Exists(Options.FileSystemFolder)) Directory.CreateDirectory(Options.FileSystemFolder);

            return new SmtpClient
            {
                DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
                PickupDirectoryLocation = Options.FileSystemFolder
            };
        }

        public bool SendEmailMessages(IList<EmailMessage> messages)
        {
            foreach (EmailMessage message in messages)
            {

            }
            return true;
        }
    }
}
