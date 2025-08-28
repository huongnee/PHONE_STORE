namespace PHONE_STORE.Application.Interfaces
{
    public interface IOtpStore
    {
        Task SaveAsync(string key, string code, TimeSpan ttl, CancellationToken ct = default);
        Task<string?> GetAsync(string key, CancellationToken ct = default);
        Task RemoveAsync(string key, CancellationToken ct = default);
    }
}
