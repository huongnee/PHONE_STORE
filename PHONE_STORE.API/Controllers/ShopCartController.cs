using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PHONE_STORE.Application.Dtos;
using PHONE_STORE.Application.Interfaces;
using PHONE_STORE.Infrastructure.Data;
using PHONE_STORE.Infrastructure.Entities;
using System.Security.Claims;

[ApiController]
[Route("api/shop/cart")]
public class ShopCartController : ControllerBase
{
    private readonly ICartRepository _repo;
    private readonly PhoneDbContext _db;
    private readonly ILogger<ShopCartController> _log;

    public ShopCartController(ICartRepository repo, PhoneDbContext db, ILogger<ShopCartController> log)
    {
        _repo = repo; _db = db; _log = log;
    }

    string? SessionId =>
        Request.Headers["X-Session-Id"].FirstOrDefault()
        ?? Request.Cookies["sid"];

    private long? GetUserId()
    {
        var s = User.FindFirstValue("sub")
                 ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? User.FindFirstValue("uid")
                 ?? User.FindFirstValue("userId");
        return long.TryParse(s, out var id) ? id : (long?)null;
    }

    private async Task<long?> GetOrCreateCustomerIdAsync(CancellationToken ct)
    {
        var uid = GetUserId();
        if (uid is null)
        {
            _log.LogInformation("GetCustomerId: no auth (sub missing). Using guest cart. sid={Sid}", SessionId);
            return null; // khách vãng lai
        }

        var cid = await _db.Customers.AsNoTracking()
                   .Where(c => c.UserAccountId == uid)
                   .Select(c => (long?)c.Id)
                   .FirstOrDefaultAsync(ct);

        if (cid is null)
        {
            var c = new Customer { UserAccountId = uid.Value, CreatedAt = DateTime.UtcNow };
            _db.Customers.Add(c);
            await _db.SaveChangesAsync(ct);
            cid = c.Id;
            _log.LogInformation("Auto-created Customer row id={CustomerId} for user id={UserId}", cid, uid);
        }

        return cid;
    }

    [HttpGet]
    public async Task<CartDto> Get(CancellationToken ct)
    {
        var customerId = await GetOrCreateCustomerIdAsync(ct);
        _log.LogInformation("Cart GET: customerId={CustomerId}, sid={Sid}", customerId, SessionId);
        return await _repo.GetOrCreateAsync(customerId, SessionId, ct);
    }

    [HttpPost("items")]
    public async Task<CartDto> Add([FromBody] CartItemUpsertDto dto, CancellationToken ct)
    {
        var customerId = await GetOrCreateCustomerIdAsync(ct);
        _log.LogInformation("Cart ADD: variant={Variant}, qty={Qty}, customerId={CustomerId}, sid={Sid}",
            dto.VariantId, dto.Quantity, customerId, SessionId);
        var cart = await _repo.AddOrUpdateItemAsync(customerId, SessionId, dto, ct);
        _log.LogInformation("Cart ADD result: cartId={CartId}, items={ItemCount}, customerId={CustomerId}, sid={Sid}",
            cart.Id, cart.Items.Count, cart.CustomerId, cart.SessionId);
        return cart;
    }

    [HttpPut("items/{itemId:long}")]
    public async Task<CartDto> Update(long itemId, [FromQuery] int qty, CancellationToken ct)
    {
        var customerId = await GetOrCreateCustomerIdAsync(ct);
        return await _repo.UpdateQtyAsync(itemId, qty, customerId, SessionId, ct);
    }

    [HttpDelete("items/{itemId:long}")]
    public async Task Remove(long itemId, CancellationToken ct)
    {
        var customerId = await GetOrCreateCustomerIdAsync(ct);
        await _repo.RemoveItemAsync(itemId, customerId, SessionId, ct);
    }

    [Authorize]
    [HttpPost("merge")]
    public async Task<IActionResult> Merge(CancellationToken ct)
    {
        var sid = SessionId;
        if (string.IsNullOrWhiteSpace(sid)) return Ok();

        // Lấy CUSTOMER ID (không phải userId)
        var customerId = await GetOrCreateCustomerIdAsync(ct);
        if (customerId is null) return Ok();

        await _repo.MergeAsync(sid, customerId.Value, ct); // ✅ truyền customerId
        return Ok();
    }
}
