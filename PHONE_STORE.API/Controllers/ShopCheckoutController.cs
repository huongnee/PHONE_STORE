using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PHONE_STORE.Application.Dtos;
using PHONE_STORE.Application.Interfaces;
using System.Security.Claims;

namespace PHONE_STORE.API.Controllers;

[ApiController]
[Route("api/shop/checkout")]
[Authorize]
public class ShopCheckoutController : ControllerBase
{
    private readonly IOrderService _orders;
    public ShopCheckoutController(IOrderService orders) => _orders = orders;

    string? SessionId =>
        Request.Headers["X-Session-Id"].FirstOrDefault()
        ?? Request.Cookies["sid"];

    // Helper đọc user id an toàn
    private long? GetUserId()
    {
        var s = User.FindFirstValue("sub")
                 ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? User.FindFirstValue("uid")
                 ?? User.FindFirstValue("userId");
        return long.TryParse(s, out var id) ? id : (long?)null;
    }

    [HttpPost("preview")]
    public Task<CheckoutPreviewResult> Preview([FromBody] CheckoutPreviewRequest req, CancellationToken ct)
    {
        var uid = GetUserId() ?? throw new UnauthorizedAccessException("Missing user id (sub).");
        return _orders.PreviewAsync(uid, req.AddressId, SessionId, ct);
    }

    [HttpPost("submit")]
    public async Task<IActionResult> Submit([FromBody] CheckoutSubmitRequest req, CancellationToken ct)
    {
        var uid = GetUserId();
        if (uid is null) return Unauthorized();

        var id = await _orders.SubmitAsync(uid.Value, req.AddressId, req.Note, SessionId, ct);
        if (id is null) return BadRequest(new { message = "Đơn rỗng hoặc không hợp lệ" });
        return Ok(new { id });
    }
}
