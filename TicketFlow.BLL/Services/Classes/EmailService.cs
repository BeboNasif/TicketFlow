using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using TicketFlow.BLL.Services.Interfaces;
using TicketFlow.BLL.Settings;

namespace TicketFlow.BLL.Services.Classes
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string bodyHtml)
        {
            if (string.IsNullOrWhiteSpace(_emailSettings.SenderEmail))
            {
                throw new InvalidOperationException("EmailSettings:SenderEmail is not configured in appsettings.json.");
            }

            if (string.IsNullOrWhiteSpace(toEmail))
            {
                throw new ArgumentException("Recipient email address cannot be empty.", nameof(toEmail));
            }

            using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port)
            {
                Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.Password),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                Subject = subject,
                Body = bodyHtml,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
        }
    }
}