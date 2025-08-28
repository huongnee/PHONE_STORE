using Microsoft.Extensions.Logging;
using PHONE_STORE.Application.Interfaces;

namespace PHONE_STORE.Infrastructure.Email
{
    public class ConsoleEmailSender : IEmailSender
    {
        private readonly ILogger<ConsoleEmailSender> _log;
        public ConsoleEmailSender(ILogger<ConsoleEmailSender> log) => _log = log;
        public Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
        {
            _log.LogInformation("SEND MAIL to {Email} | {Subject} | Body: {Body}", toEmail, subject, htmlBody);
            return Task.CompletedTask;
        }
    }
}
