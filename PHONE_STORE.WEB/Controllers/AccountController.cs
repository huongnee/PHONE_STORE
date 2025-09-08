using System.Net.Http.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PHONE_STORE.WEB.Models;
using System.Security.Claims;
using System.Text.Json;
using System.Linq;
using System.Net.Http.Headers; // ✅ THÊM

namespace PHONE_STORE.WEB.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IHttpClientFactory http, ILogger<AccountController> logger)
        {
            _http = http;
            _logger = logger;
        }

        private HttpClient Api => _http.CreateClient("api");             // có JwtCookieHandler
        private HttpClient ApiNoAuth => _http.CreateClient("ApiNoAuth"); // không có handler

        // 👉 Helper đọc message từ API
        private static async Task<string> ReadApiMessage(HttpResponseMessage res)
        {
            try
            {
                var obj = await res.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                if (obj != null && obj.TryGetValue("message", out var m) && !string.IsNullOrWhiteSpace(m))
                    return m;
            }
            catch { }
            return await res.Content.ReadAsStringAsync();
        }

        [HttpGet] public IActionResult Register() => View();

        [ValidateAntiForgeryToken, HttpPost]
        public async Task<IActionResult> Register(RegisterRequest model)
        {
            if (!ModelState.IsValid) return View(model);

            var resp = await ApiNoAuth.PostAsJsonAsync("/api/auth/register", model);
            if (resp.IsSuccessStatusCode)
            {
                TempData["msg"] = "Đăng ký thành công, vui lòng đăng nhập.";
                return RedirectToAction(nameof(Login));
            }

            var err = await ReadApiMessage(resp);
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

            var apiNoAuth = _http.CreateClient("ApiNoAuth");
            var resp = await apiNoAuth.PostAsJsonAsync("/api/auth/login", model);
            if (!resp.IsSuccessStatusCode)
            {
                // 👉 Sử dụng helper mới
                ModelState.AddModelError("", await ReadApiMessage(resp));
                return View(model);
            }

            var data = await resp.Content.ReadFromJsonAsync<AuthResponse>();
            if (data is null || string.IsNullOrWhiteSpace(data.AccessToken))
            {
                _logger.LogError("Login response invalid: {Data}", data);
                ModelState.AddModelError("", "Phản hồi đăng nhập không hợp lệ.");
                return View(model);
            }

            _logger.LogInformation("Login successful for user {UserId}, token length: {TokenLength}",
                data.UserId, data.AccessToken.Length);

            // 1) set token cookies cho origin WEB
            var accessExp = data.AccessExpiresAt == default ? DateTime.UtcNow.AddMinutes(30)
                                                            : DateTime.SpecifyKind(data.AccessExpiresAt, DateTimeKind.Utc);
            var refreshExp = data.RefreshExpiresAt == default ? DateTime.UtcNow.AddDays(7)
                                                             : DateTime.SpecifyKind(data.RefreshExpiresAt, DateTimeKind.Utc);

            var isDev = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment();
            Response.Cookies.Append("access_token", data.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = !isDev,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                Expires = new DateTimeOffset(accessExp)
            });
            if (!string.IsNullOrWhiteSpace(data.RefreshToken))
                Response.Cookies.Append("refresh_token", data.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = !isDev,
                    SameSite = SameSiteMode.Lax,
                    Path = "/",
                    Expires = new DateTimeOffset(refreshExp)
                });

            // 2) cookie MVC
            var claims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, data.UserId.ToString()),
                new Claim(ClaimTypes.Name, data.Email ?? data.UserId.ToString())
            };
            foreach (var r in data.Roles ?? Array.Empty<string>()) claims.Add(new Claim(ClaimTypes.Role, r));
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var props = new AuthenticationProperties { IsPersistent = model.RememberMe, AllowRefresh = true };
            if (model.RememberMe) props.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(14);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), props);

            // 3) MERGE giỏ guest (sid) -> giỏ user
            // ❗GỌI BẰNG CLIENT KHÔNG HANDLER + GẮN BEARER THỦ CÔNG (cookie vừa set chưa áp dụng cho request này)
            var sid = Request.Cookies["sid"];
            if (!string.IsNullOrWhiteSpace(sid))
            {
                var mergeClient = ApiNoAuth; // dùng client không có JwtCookieHandler
                mergeClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", data.AccessToken);
                mergeClient.DefaultRequestHeaders.Remove("X-Session-Id");
                mergeClient.DefaultRequestHeaders.Add("X-Session-Id", sid);

                try
                {
                    var mergeRes = await mergeClient.PostAsync("/api/shop/cart/merge", null);
                    _logger.LogInformation("Cart merge after login: {Status}", mergeRes.StatusCode);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Cart merge failed");
                }
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
            return RedirectToAction("Index", "Home");
        }

        [ValidateAntiForgeryToken, HttpPost]
        public async Task<IActionResult> Logout()
        {
            var rt = Request.Cookies["refresh_token"];
            if (!string.IsNullOrWhiteSpace(rt))
            {
                _ = await ApiNoAuth.PostAsJsonAsync("/api/auth/logout", new { RefreshToken = rt });
            }

            Response.Cookies.Delete("access_token");
            Response.Cookies.Delete("refresh_token");
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [Authorize, HttpGet]
        public async Task<IActionResult> Me()
        {
            var resp = await Api.GetAsync("/api/auth/me");
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
            var resp = await ApiNoAuth.PostAsJsonAsync("/api/auth/forgot-password", new { Email = email });
            TempData["msg"] = resp.IsSuccessStatusCode ? "Nếu email tồn tại, mã OTP đã được gửi." : "Không gửi được OTP. Thử lại sau.";
            return RedirectToAction(nameof(ResetPassword));
        }

        [HttpGet] public IActionResult ResetPassword() => View();

        [ValidateAntiForgeryToken, HttpPost]
        public async Task<IActionResult> ResetPassword(string email, string otp, string newPassword)
        {
            var resp = await ApiNoAuth.PostAsJsonAsync("/api/auth/reset-password", new { Email = email, Otp = otp, NewPassword = newPassword });
            if (resp.IsSuccessStatusCode)
            {
                TempData["msg"] = "Đổi mật khẩu thành công. Đăng nhập lại nhé.";
                return RedirectToAction(nameof(Login));
            }
            ModelState.AddModelError("", await ReadApiMessage(resp));
            return View();
        }

        [Authorize, HttpGet] public IActionResult ChangePassword() => View();

        [Authorize, ValidateAntiForgeryToken, HttpPost]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword)
        {
            var resp = await Api.PostAsJsonAsync("/api/auth/change-password", new { OldPassword = oldPassword, NewPassword = newPassword });
            if (resp.IsSuccessStatusCode)
            {
                TempData["msg"] = "Đã đổi mật khẩu.";
                return RedirectToAction("Index", "Home");
            }
            ModelState.AddModelError("", await ReadApiMessage(resp));
            return View();
        }
    }
}
