namespace PHONE_STORE.Application.Interfaces;

public record RefreshSession(string Jti, long UserId, DateTime ExpiresAt, bool Revoked = false);

public interface IRefreshStore
{
    Task SaveAsync(RefreshSession session, CancellationToken ct = default);
    Task<RefreshSession?> GetAsync(string jti, CancellationToken ct = default);
    Task RevokeAsync(string jti, CancellationToken ct = default);
}
