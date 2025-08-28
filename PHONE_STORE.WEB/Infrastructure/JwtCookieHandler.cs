using System.Net.Http.Headers;
using System.Reflection.PortableExecutable;

namespace PHONE_STORE.WEB.Infrastructure;
//tự động gắn JWT từ cookie access_token vào header Authorization mỗi khi Web gọi API.
public class JwtCookieHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _http;
    public JwtCookieHandler(IHttpContextAccessor http) => _http = http;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Headers.Authorization == null) // <-- thêm điều kiện này
        {
            var ctx = _http.HttpContext;
            if (ctx != null && ctx.Request.Cookies.TryGetValue("access_token", out var token) && !string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
        return base.SendAsync(request, cancellationToken);
    }

}
