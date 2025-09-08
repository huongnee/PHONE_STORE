using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace PHONE_STORE.WEB.Infrastructure;

public class SessionForwardingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _http;
    public SessionForwardingHandler(IHttpContextAccessor http) => _http = http;

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var sid = _http.HttpContext?.Request.Cookies["sid"];
        if (!string.IsNullOrWhiteSpace(sid))
        {
            if (request.Headers.Contains("X-Session-Id"))
                request.Headers.Remove("X-Session-Id");
            request.Headers.Add("X-Session-Id", sid);
        }
        return base.SendAsync(request, cancellationToken);
    }
}
