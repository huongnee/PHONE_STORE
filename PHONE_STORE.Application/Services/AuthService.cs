using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PHONE_STORE.Application.Dtos;
using PHONE_STORE.Application.Interfaces;
using PHONE_STORE.Application.Options;
using System.Security.Cryptography;


namespace PHONE_STORE.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly ITokenService _tokenService;
    private readonly IRefreshStore _refreshStore;
    private readonly JwtOptions _jwt;
    private readonly ILogger<AuthService> _log;

    private const int DefaultRefreshDays = 14;
    private readonly IOtpStore _otpStore;
    private readonly IEmailSender _email;

    public AuthService(
       IUserRepository userRepo,
       ITokenService tokenService,
       IRefreshStore refreshStore,
       IOptions<JwtOptions> jwtOpt,
       ILogger<AuthService> log,
       IOtpStore otpStore,
       IEmailSender email)
    {
        _userRepo = userRepo;
        _tokenService = tokenService;
        _refreshStore = refreshStore;
        _jwt = jwtOpt.Value;
        _log = log;
        _otpStore = otpStore ?? throw new ArgumentNullException(nameof(otpStore));
        _email = email ?? throw new ArgumentNullException(nameof(email));
    }

    public async Task<(bool ok, string? error)> ChangePasswordAsync(long userId, string oldPassword, string newPassword, string? currentRefreshTokenJti, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            return (false, "Mật khẩu mới tối thiểu 6 ký tự.");

        var u = await _userRepo.GetAuthUserByIdAsync(userId, ct);
        if (u is null || string.IsNullOrEmpty(u.PasswordHash)) return (false, "User không tồn tại hoặc không hợp lệ.");

        if (!BCrypt.Net.BCrypt.Verify(oldPassword, u.PasswordHash))
            return (false, "Mật khẩu cũ không đúng.");

        var hash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _userRepo.UpdatePasswordHashAsync(userId, hash, ct);

        // Revoke RT hiện tại (nếu phía WEB gửi kèm jti)
        if (!string.IsNullOrWhiteSpace(currentRefreshTokenJti))
            await _refreshStore.RevokeAsync(currentRefreshTokenJti, ct);

        return (true, null);
    }

    public async Task<(bool ok, string? error)> StartPasswordResetAsync(string email, CancellationToken ct = default)
    {
        var em = (email ?? "").Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(em)) return (false, "Email bắt buộc.");

        // Không tiết lộ user tồn tại hay không (tránh enumerate)
        // Nhưng vẫn tạo OTP nếu có user
        var user = await _userRepo.GetByUsernameAsync(em, ct); // GetByUsernameAsync hỗ trợ email/phone; email đã lower
        if (user is not null)
        {
            var otp = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            await _otpStore.SaveAsync($"fp:{em}", otp, TimeSpan.FromMinutes(10), ct);
            await _email.SendAsync(em, "Mã đặt lại mật khẩu", $"<p>Mã OTP của bạn là <b>{otp}</b>, hiệu lực 10 phút.</p>", ct);
        }
        return (true, null); // luôn trả OK để tránh lộ thông tin
    }

    public async Task<(bool ok, string? error)> CompletePasswordResetAsync(string email, string otp, string newPassword, CancellationToken ct = default)
    {
        var em = (email ?? "").Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(em)) return (false, "Email bắt buộc.");
        if (string.IsNullOrWhiteSpace(otp)) return (false, "OTP bắt buộc.");
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6) return (false, "Mật khẩu mới tối thiểu 6 ký tự.");

        var saved = await _otpStore.GetAsync($"fp:{em}", ct);
        if (saved is null || !string.Equals(saved, otp, StringComparison.Ordinal))
            return (false, "OTP không hợp lệ hoặc đã hết hạn.");

        var u = await _userRepo.GetAuthUserAsync(em, ct);
        if (u is null) return (false, "Tài khoản không tồn tại.");

        var hash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _userRepo.UpdatePasswordHashAsync(u.Id, hash, ct);

        await _otpStore.RemoveAsync($"fp:{em}", ct);
        // (tuỳ chọn) revoke RT hiện tại nếu bạn lưu danh sách RT theo user
        return (true, null);
    }
   

    public async Task<AuthResponseDto?> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        var u = await _userRepo.GetAuthUserAsync(username, ct);
        if (u is null || string.IsNullOrEmpty(u.PasswordHash))
        {
            _log.LogWarning("Login failed: user not found [{Username}]", username);
            return null;
        }
        if (!BCrypt.Net.BCrypt.Verify(password, u.PasswordHash))
        {
            _log.LogWarning("Login failed: wrong password [{UserId}]", u.Id);
            return null;
        }
        if (!string.Equals(u.Status, "ACTIVE", StringComparison.OrdinalIgnoreCase))
        {
            _log.LogWarning("Login failed: inactive user [{UserId}]", u.Id);
            return null;
        }

        var roles = await _userRepo.GetRolesAsync(u.Id, ct);

        var nowUtc = DateTime.UtcNow;
        var access = _tokenService.CreateAccessToken(u.Id, u.Email, roles);
        var accessExp = nowUtc.AddMinutes(_jwt.AccessTokenMinutes);

        var jti = NewJti();
        var refreshExp = nowUtc.AddDays(DefaultRefreshDays);
        await _refreshStore.SaveAsync(new RefreshSession(jti, u.Id, refreshExp), ct);

        _log.LogInformation("Login success [{UserId}]", u.Id);

        return new AuthResponseDto
        {
            AccessToken = access,
            AccessExpiresAt = accessExp,
            RefreshToken = jti,
            RefreshExpiresAt = refreshExp,
            UserId = u.Id,
            Email = u.Email,
            Roles = roles
        };
    }

    public async Task<(bool ok, string? error)> RegisterAsync(string email, string password, string? phone, CancellationToken ct = default)
    {
        var em = (email ?? "").Trim().ToLowerInvariant();
        var ph = (phone ?? "").Trim();

        if (string.IsNullOrWhiteSpace(em)) return (false, "Email là bắt buộc");
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6) return (false, "Mật khẩu tối thiểu 6 ký tự.");

        if (await _userRepo.EmailExistsAsync(em, ct)) return (false, "Email đã tồn tại.");
        if (!string.IsNullOrEmpty(ph) && await _userRepo.PhoneExistsAsync(ph, ct)) return (false, "Số điện thoại đã tồn tại.");

        var hash = BCrypt.Net.BCrypt.HashPassword(password);
        var userId = await _userRepo.CreateAsync(em, hash, string.IsNullOrEmpty(ph) ? null : ph, ct);

        //await _userRepo.AddRoleAsync(userId, "USER", ct);
        //await _userRepo.AddRoleAsync(userId, "CUSTOMER", ct);
        await _userRepo.AddRoleAsync(userId, RoleCodes.Customer, ct);

        _log.LogInformation("Register success [{UserId}]", userId);

        return (true, null);
    }

    public async Task<AuthResponseDto?> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            _log.LogWarning("Refresh denied: empty token");
            return null;
        }

        var session = await _refreshStore.GetAsync(refreshToken, ct);
        if (session is null || session.Revoked || session.ExpiresAt <= DateTime.UtcNow)
        {
            _log.LogWarning("Refresh denied: invalid/expired/revoked RT");
            return null;
        }

        var user = await _userRepo.GetByIdAsync(session.UserId, ct);
        if (user is null || !string.Equals(user.Status, "ACTIVE", StringComparison.OrdinalIgnoreCase))
        {
            _log.LogWarning("Refresh denied: user inactive/missing [{UserId}]", session.UserId);
            return null;
        }

        var roles = await _userRepo.GetRolesAsync(user.Id, ct);

        var nowUtc = DateTime.UtcNow;
        var access = _tokenService.CreateAccessToken(user.Id, user.Email, roles);
        var accessExp = nowUtc.AddMinutes(_jwt.AccessTokenMinutes);

        await _refreshStore.RevokeAsync(refreshToken, ct);

        var newJti = NewJti();
        var newExp = nowUtc.AddDays(DefaultRefreshDays);
        await _refreshStore.SaveAsync(new RefreshSession(newJti, user.Id, newExp), ct);

        _log.LogInformation("Refresh success: rotate RT for user {UserId}", user.Id);

        return new AuthResponseDto
        {
            AccessToken = access,
            AccessExpiresAt = accessExp,
            RefreshToken = newJti,
            RefreshExpiresAt = newExp,
            UserId = user.Id,
            Email = user.Email,
            Roles = roles
        };
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(refreshToken)) return;

        await _refreshStore.RevokeAsync(refreshToken, ct);
        _log.LogInformation("Logout: revoked RT");
    }

    private static string NewJti()
    {
        // URL-safe base64, không có '='
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }
}
