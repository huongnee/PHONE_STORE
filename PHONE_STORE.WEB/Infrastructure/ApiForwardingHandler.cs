using System.Net.Http.Headers;

public sealed class ApiForwardingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _http;
    public ApiForwardingHandler(IHttpContextAccessor http) => _http = http;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var ctx = _http.HttpContext;

        // 1. Gắn JWT access token
        var token = ctx?.User?.FindFirst("access_token")?.Value
                    ?? ctx?.Request.Cookies["at"]; // fallback nếu bạn lưu token vào cookie "at"

        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 2. Gắn session id (dùng cho giỏ hàng)
        var sid = ctx?.Request.Cookies["sid"];
        if (!string.IsNullOrEmpty(sid) && !request.Headers.Contains("X-Session-Id"))
            request.Headers.Add("X-Session-Id", sid);

        return base.SendAsync(request, ct);
    }
}
