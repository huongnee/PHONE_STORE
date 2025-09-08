using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace PHONE_STORE.WEB.Infrastructure
{
    /// <summary>
    /// Nếu API trả 401 và có refresh_token trong cookie của WEB:
    /// - Gọi /api/auth/refresh (client "ApiNoAuth")
    /// - Cập nhật cookies access_token/refresh_token ở WEB
    /// - Gắn Authorization mới và retry đúng 1 lần
    /// </summary>
    public class AutoRefreshHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _http;
        private readonly IHttpClientFactory _factory;
        private readonly ILogger<AutoRefreshHandler> _logger;

        // Đánh dấu đã retry để tránh vòng lặp
        private static readonly HttpRequestOptionsKey<bool> RetriedKey = new("X-Retried");

        public AutoRefreshHandler(IHttpContextAccessor http, IHttpClientFactory factory, ILogger<AutoRefreshHandler> logger)
        {
            _http = http;
            _factory = factory;
            _logger = logger;
        }

        private sealed class AuthResponse
        {
            public string? AccessToken { get; set; }
            public DateTime AccessExpiresAt { get; set; }
            public string? RefreshToken { get; set; }
            public DateTime RefreshExpiresAt { get; set; }
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var resp = await base.SendAsync(request, ct);
            if (resp.StatusCode != HttpStatusCode.Unauthorized)
                return resp;

            if (request.Options.TryGetValue(RetriedKey, out var retried) && retried)
                return resp; // đã retry 1 lần

            var ctx = _http.HttpContext;
            if (ctx == null || !ctx.Request.Cookies.TryGetValue("refresh_token", out var rt) || string.IsNullOrWhiteSpace(rt))
            {
                _logger.LogWarning("401 and no refresh_token cookie. Skip auto-refresh for {Url}", request.RequestUri);
                return resp;
            }

            try
            {
                var noAuth = _factory.CreateClient("ApiNoAuth");
                var r = await noAuth.PostAsJsonAsync("/api/auth/refresh", new { refreshToken = rt }, ct);
                if (!r.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Refresh call failed {Status} for {Url}", r.StatusCode, request.RequestUri);
                    return resp;
                }

                var data = await r.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: ct);
                if (data == null || string.IsNullOrWhiteSpace(data.AccessToken))
                {
                    _logger.LogWarning("Refresh returned invalid payload for {Url}", request.RequestUri);
                    return resp;
                }

                var env = ctx.RequestServices.GetRequiredService<IWebHostEnvironment>();
                var isDev = env.IsDevelopment();

                // Cập nhật cookies trên WEB
                var atUtc = DateTime.SpecifyKind(data.AccessExpiresAt, DateTimeKind.Utc);
                var rtUtc = DateTime.SpecifyKind(data.RefreshExpiresAt, DateTimeKind.Utc);

                ctx.Response.Cookies.Append("access_token", data.AccessToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = !isDev,          // prod: true
                    SameSite = SameSiteMode.Lax,
                    Path = "/",
                    Expires = new DateTimeOffset(atUtc)
                });

                if (!string.IsNullOrWhiteSpace(data.RefreshToken))
                {
                    ctx.Response.Cookies.Append("refresh_token", data.RefreshToken!, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = !isDev,      // prod: true
                        SameSite = SameSiteMode.Lax,
                        Path = "/",
                        Expires = new DateTimeOffset(rtUtc)
                    });
                }

                // Retry request với token mới
                resp.Dispose();
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", data.AccessToken);
                request.Options.Set(RetriedKey, true);

                _logger.LogInformation("Auto-refreshed token and retried {Url}", request.RequestUri);
                return await base.SendAsync(request, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Auto refresh failed for {Url}", request.RequestUri);
                return resp;
            }
        }
    }
}
