using System.Net.Http.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PHONE_STORE.WEB.Models;
using System.Security.Claims;
using System.Text.Json;
using System.Linq;

namespace PHONE_STORE.WEB.Controllers;

public class AccountController : Controller
{
    private readonly IHttpClientFactory _http;
    public AccountController(IHttpClientFactory http) => _http = http;

    [HttpGet] public IActionResult Register() => View();

    [ValidateAntiForgeryToken, HttpPost]
    public async Task<IActionResult> Register(RegisterRequest model)
    {
        if (!ModelState.IsValid) return View(model);

        var api = _http.CreateClient("api");
        var resp = await api.PostAsJsonAsync("/api/auth/register", model);
        if (resp.IsSuccessStatusCode) { TempData["msg"] = "Đăng ký thành công, vui lòng đăng nhập."; return RedirectToAction(nameof(Login)); }

        var err = await resp.Content.ReadAsStringAsync();
        ModelState.AddModelError("", $"Đăng ký thất bại: {err}");
        return View(model);
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View(new LoginRequest());
    }

    [ValidateAntiForgeryToken, HttpPost]
    public async Task<IActionResult> Login(LoginRequest model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);

        var api = _http.CreateClient("api");
        var resp = await api.PostAsJsonAsync("/api/auth/login", model);
        if (!resp.IsSuccessStatusCode) { ModelState.AddModelError("", "Sai tài khoản hoặc mật khẩu."); return View(model); }

        var data = await resp.Content.ReadFromJsonAsync<AuthResponse>();
        if (data is null || string.IsNullOrEmpty(data.AccessToken)) { ModelState.AddModelError("", "Phản hồi đăng nhập không hợp lệ."); return View(model); }

        var accessUtc = data.AccessExpiresAt.Kind == DateTimeKind.Utc ? data.AccessExpiresAt : DateTime.SpecifyKind(data.AccessExpiresAt, DateTimeKind.Utc);
        var refreshUtc = data.RefreshExpiresAt.Kind == DateTimeKind.Utc ? data.RefreshExpiresAt : DateTime.SpecifyKind(data.RefreshExpiresAt, DateTimeKind.Utc);

        Response.Cookies.Append("access_token", data.AccessToken, new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Lax, Expires = new DateTimeOffset(accessUtc) });
        if (!string.IsNullOrEmpty(data.RefreshToken))
            Response.Cookies.Append("refresh_token", data.RefreshToken, new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict, Expires = new DateTimeOffset(refreshUtc) });

        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, data.UserId.ToString()), new Claim(ClaimTypes.Name, data.Email ?? data.UserId.ToString()) };
        foreach (var r in data.Roles) claims.Add(new Claim(ClaimTypes.Role, r));
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var authProps = new AuthenticationProperties { IsPersistent = true, AllowRefresh = true };
        if (model.RememberMe) authProps.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(14);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), authProps);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
        return RedirectToAction("Index", "Home");
    }

    [ValidateAntiForgeryToken, HttpPost]
    public async Task<IActionResult> Logout()
    {
        var rt = Request.Cookies["refresh_token"];
        if (!string.IsNullOrWhiteSpace(rt))
        {
            var api = _http.CreateClient("api");
            _ = await api.PostAsJsonAsync("/api/auth/logout", new { refreshToken = rt });
        }
        Response.Cookies.Delete("access_token");
        Response.Cookies.Delete("refresh_token");
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [Authorize, HttpGet]
    public async Task<IActionResult> Me()
    {
        var api = _http.CreateClient("api");
        var resp = await api.GetAsync("/api/auth/me");
        if (!resp.IsSuccessStatusCode) return Content("Gọi API /api/auth/me thất bại: " + resp.StatusCode);
        var json = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        ViewBag.Json = json;
        ViewBag.UserId = root.GetProperty("userId").GetString();
        ViewBag.Email = root.GetProperty("email").GetString();
        ViewBag.Roles = string.Join(", ", root.GetProperty("roles").EnumerateArray().Select(x => x.GetString()));
        return View();
    }

    [HttpGet] public IActionResult ForgotPassword() => View();

    [ValidateAntiForgeryToken, HttpPost]
    public async Task<IActionResult> ForgotPassword(string email)
    {
        var api = _http.CreateClient("api");
        var resp = await api.PostAsJsonAsync("/api/auth/forgot-password", new { Email = email });
        TempData["msg"] = resp.IsSuccessStatusCode ? "Nếu email tồn tại, mã OTP đã được gửi." : "Không gửi được OTP. Thử lại sau.";
        return RedirectToAction(nameof(ResetPassword));
    }

    [HttpGet] public IActionResult ResetPassword() => View();

    [ValidateAntiForgeryToken, HttpPost]
    public async Task<IActionResult> ResetPassword(string email, string otp, string newPassword)
    {
        var api = _http.CreateClient("api");
        var resp = await api.PostAsJsonAsync("/api/auth/reset-password", new { Email = email, Otp = otp, NewPassword = newPassword });
        if (resp.IsSuccessStatusCode) { TempData["msg"] = "Đổi mật khẩu thành công. Đăng nhập lại nhé."; return RedirectToAction(nameof(Login)); }
        ModelState.AddModelError("", await resp.Content.ReadAsStringAsync());
        return View();
    }

    [Authorize, HttpGet] public IActionResult ChangePassword() => View();

    [Authorize, ValidateAntiForgeryToken, HttpPost]
    public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword)
    {
        var rt = Request.Cookies["refresh_token"];
        var api = _http.CreateClient("api");
        var resp = await api.PostAsJsonAsync("/api/auth/change-password", new { OldPassword = oldPassword, NewPassword = newPassword, CurrentRefreshTokenJti = rt });
        if (resp.IsSuccessStatusCode) { TempData["msg"] = "Đã đổi mật khẩu."; return RedirectToAction("Index", "Home"); }
        ModelState.AddModelError("", await resp.Content.ReadAsStringAsync());
        return View();
    }
}
