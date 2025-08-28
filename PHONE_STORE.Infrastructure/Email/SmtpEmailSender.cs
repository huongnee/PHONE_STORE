using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PHONE_STORE.Application.Interfaces;
using PHONE_STORE.Application.Options;

namespace PHONE_STORE.Infrastructure.Email
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpOptions _opt;
        private readonly ILogger<SmtpEmailSender> _log;

        public SmtpEmailSender(IOptions<SmtpOptions> opt, ILogger<SmtpEmailSender> log)
        { _opt = opt.Value; _log = log; }

        public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
        {
            using var client = new SmtpClient(_opt.Host, _opt.Port)
            {
                EnableSsl = _opt.EnableSsl,
                Credentials = new NetworkCredential(_opt.Username, _opt.Password),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Timeout = 15000
            };

            using var msg = new MailMessage(new MailAddress(_opt.FromEmail, _opt.FromName), new MailAddress(toEmail))
            {
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            await client.SendMailAsync(msg);
            _log.LogInformation("SMTP sent to {Email} | {Subject}", toEmail, subject);
        }
    }
}
