using System.Net.Http.Headers;

namespace PHONE_STORE.WEB.Infrastructure
{
    public class JwtCookieHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _http;
        private readonly ILogger<JwtCookieHandler> _logger;

        public JwtCookieHandler(IHttpContextAccessor http, ILogger<JwtCookieHandler> logger)
        {
            _http = http;
            _logger = logger;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var ctx = _http.HttpContext;
            if (ctx != null)
            {
                // Gắn Bearer từ cookie 'access_token'
                if (ctx.Request.Cookies.TryGetValue("access_token", out var at) && !string.IsNullOrWhiteSpace(at))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", at);
                    _logger.LogInformation("Added JWT token to request: {Url}", request.RequestUri);
                }
                else
                {
                    _logger.LogWarning("No access_token cookie found for request: {Url}", request.RequestUri);
                }

                // Gắn X-Session-Id từ cookie 'sid' để API biết giỏ hàng guest
                if (!request.Headers.Contains("X-Session-Id") &&
                    ctx.Request.Cookies.TryGetValue("sid", out var sid) && !string.IsNullOrWhiteSpace(sid))
                {
                    request.Headers.Add("X-Session-Id", sid);
                    _logger.LogInformation("Added session ID to request: {Url}", request.RequestUri);
                }
            }

            return base.SendAsync(request, ct);
        }
    }
}
