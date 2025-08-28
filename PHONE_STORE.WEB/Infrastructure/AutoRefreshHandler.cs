using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using PHONE_STORE.WEB.Models;

namespace PHONE_STORE.WEB.Infrastructure;

public class AutoRefreshHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _http;
    private readonly IHttpClientFactory _factory;
    private static readonly HttpRequestOptionsKey<bool> RetriedKey = new("X-Retried");

    public AutoRefreshHandler(IHttpContextAccessor http, IHttpClientFactory factory)
    {
        _http = http; _factory = factory;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var resp = await base.SendAsync(request, ct);
        if (resp.StatusCode != HttpStatusCode.Unauthorized) return resp;

        if (request.Options.TryGetValue(RetriedKey, out var retried) && retried) return resp;

        var ctx = _http.HttpContext;
        if (ctx == null || !ctx.Request.Cookies.TryGetValue("refresh_token", out var rt) || string.IsNullOrWhiteSpace(rt))
            return resp;

        var refreshClient = _factory.CreateClient("ApiNoAuth");
        var r = await refreshClient.PostAsJsonAsync("/api/auth/refresh", new { refreshToken = rt }, ct);
        if (!r.IsSuccessStatusCode) return resp;

        var data = await r.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken: ct);
        if (data == null || string.IsNullOrEmpty(data.AccessToken)) return resp;

        var atUtc = data.AccessExpiresAt.Kind == DateTimeKind.Utc ? data.AccessExpiresAt : DateTime.SpecifyKind(data.AccessExpiresAt, DateTimeKind.Utc);
        var rtUtc = data.RefreshExpiresAt.Kind == DateTimeKind.Utc ? data.RefreshExpiresAt : DateTime.SpecifyKind(data.RefreshExpiresAt, DateTimeKind.Utc);

        ctx.Response.Cookies.Append("access_token", data.AccessToken, new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Lax, Expires = new DateTimeOffset(atUtc) });
        ctx.Response.Cookies.Append("refresh_token", data.RefreshToken, new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict, Expires = new DateTimeOffset(rtUtc) });

        resp.Dispose();
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", data.AccessToken);
        request.Options.Set(RetriedKey, true);
        return await base.SendAsync(request, ct);
    }
}
