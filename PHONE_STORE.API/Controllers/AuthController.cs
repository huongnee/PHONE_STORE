using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PHONE_STORE.API.Auth;
using PHONE_STORE.Application.Interfaces;

namespace PHONE_STORE.API.Controllers;



[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly LoginLockService _lock;

    public AuthController(IAuthService auth, LoginLockService @lock)
    {
        _auth = auth;
        _lock = @lock;
    }

    public record LoginRequest(string Username, string Password);

    public record ChangePasswordRequest(string OldPassword, string NewPassword, string? CurrentRefreshTokenJti);
    public record ForgotPasswordRequest(string Email);
    public record ResetPasswordRequest(string Email, string Otp, string NewPassword);

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req, CancellationToken ct)
    {
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0";
        long.TryParse(userIdStr, out var userId);

        var (ok, err) = await _auth.ChangePasswordAsync(userId, req.OldPassword, req.NewPassword, req.CurrentRefreshTokenJti, ct);
        if (!ok) return BadRequest(new { message = err });
        return Ok(new { message = "changed" });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req, CancellationToken ct)
    {
        var (ok, err) = await _auth.StartPasswordResetAsync(req.Email, ct);
        if (!ok) return BadRequest(new { message = err });
        return Ok(new { message = "otp_sent_if_exists" });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req, CancellationToken ct)
    {
        var (ok, err) = await _auth.CompletePasswordResetAsync(req.Email, req.Otp, req.NewPassword, ct);
        if (!ok) return BadRequest(new { message = err });
        return Ok(new { message = "password_reset" });
    }


    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        var username = (req.Username ?? "").Trim();
        if (await _lock.IsLockedAsync(username, ct))
            return StatusCode(423, new { message = "Tài khoản bị khoá tạm thời. Vui lòng thử lại sau." });

        var result = await _auth.LoginAsync(username, req.Password, ct);
        if (result is null)
        {
            await _lock.IncreaseFailAsync(username, ct);
            return Unauthorized(new { message = "Invalid username or password" });
        }

        await _lock.ClearAsync(username, ct);

        Response.Cookies.Append("at", result.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = new DateTimeOffset(result.AccessExpiresAt)
        });
        Response.Cookies.Append("rt", result.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = new DateTimeOffset(result.RefreshExpiresAt)
        });

        return Ok(result);
    }
    // THÊM record dưới LoginRequest:
    public record RefreshRequest(string RefreshToken);

    // SỬA /refresh: đọc RT từ body, trả về full AuthResponseDto
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest req, CancellationToken ct)
    {
        var result = await _auth.RefreshAsync(req.RefreshToken ?? "", ct);
        if (result is null) return Unauthorized();
        return Ok(result); // trả cả AT/RT mới + thời hạn
    }

    // Cho phép logout không cần AT; nhận RT từ body
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest req, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(req.RefreshToken))
            await _auth.LogoutAsync(req.RefreshToken, ct);
        return Ok(new { message = "logged_out" });
    }


    //[HttpPost("refresh")]
    //public async Task<IActionResult> Refresh(CancellationToken ct)
    //{
    //    var jti = Request.Cookies["rt"] ?? "";
    //    var result = await _auth.RefreshAsync(jti, ct);
    //    if (result is null) return Unauthorized();

    //    Response.Cookies.Append("at", result.AccessToken, new CookieOptions
    //    {
    //        HttpOnly = true,
    //        Secure = true,
    //        SameSite = SameSiteMode.Lax,
    //        Expires = new DateTimeOffset(result.AccessExpiresAt)
    //    });
    //    Response.Cookies.Append("rt", result.RefreshToken, new CookieOptions
    //    {
    //        HttpOnly = true,
    //        Secure = true,
    //        SameSite = SameSiteMode.Strict,
    //        Expires = new DateTimeOffset(result.RefreshExpiresAt)
    //    });

    //    return Ok(new { message = "refreshed" });
    //}

    // PHONE_STORE.API/Controllers/AuthController.cs (thêm bên dưới LoginRequest)
    public record RegisterRequest(string Email, string Password, string? Phone);

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req, CancellationToken ct)
    {
        var (ok, error) = await _auth.RegisterAsync(req.Email, req.Password, req.Phone, ct);
        if (!ok) return BadRequest(new { message = error });
        return Ok(new { message = "registered" });
    }


    //[Authorize]
    //[HttpPost("logout")]
    //public async Task<IActionResult> Logout(CancellationToken ct)
    //{
    //    var jti = Request.Cookies["rt"] ?? "";
    //    await _auth.LogoutAsync(jti, ct);

    //    Response.Cookies.Delete("at");
    //    Response.Cookies.Delete("rt");
    //    return Ok(new { message = "logged_out" });
    //}

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var id = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                 ?? User.FindFirst("sub")?.Value;
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var roles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(r => r.Value).ToArray();
        return Ok(new { userId = id, email, roles });
    }
}
