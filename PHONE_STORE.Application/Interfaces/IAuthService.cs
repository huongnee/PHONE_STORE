using PHONE_STORE.Application.Dtos;

namespace PHONE_STORE.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> LoginAsync(string username, string password, CancellationToken ct = default);
        Task<AuthResponseDto?> RefreshAsync(string refreshToken, CancellationToken ct = default);
        Task LogoutAsync(string refreshToken, CancellationToken ct = default);
        Task<(bool ok, string? error)> RegisterAsync(string email, string password, string? phone, CancellationToken ct = default);

        // NEW
        Task<(bool ok, string? error)> ChangePasswordAsync(long userId, string oldPassword, string newPassword, string? currentRefreshTokenJti, CancellationToken ct = default);
        Task<(bool ok, string? error)> StartPasswordResetAsync(string email, CancellationToken ct = default); // phát OTP
        Task<(bool ok, string? error)> CompletePasswordResetAsync(string email, string otp, string newPassword, CancellationToken ct = default); // xác minh OTP, đổi mật khẩu
    }
}
