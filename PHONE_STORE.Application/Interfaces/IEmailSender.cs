namespace PHONE_STORE.Application.Interfaces
{
    public interface IEmailSender
    {
        Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default);
    }
}
