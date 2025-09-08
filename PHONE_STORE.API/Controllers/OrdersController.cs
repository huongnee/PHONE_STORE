using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PHONE_STORE.Application.Dtos;
using PHONE_STORE.Infrastructure.Data;

namespace PHONE_STORE.API.Controllers;

[ApiController]
[Route("api/admin/orders")]
[Authorize(Roles = "ADMIN,STAFF")]
public class OrdersController : ControllerBase
{
    private readonly PhoneDbContext _db;
    public OrdersController(PhoneDbContext db) => _db = db;

    [HttpGet]
    public async Task<List<OrderListItemDto>> Search(
        [FromQuery] string? q, [FromQuery] string? status, [FromQuery] int top = 100, CancellationToken ct = default)
    {
        top = Math.Clamp(top, 1, 500);

        var query = _db.Orders.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(o => o.Status == status);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var s = q.Trim().ToLower();
            query = from o in query
                    join c in _db.Customers.AsNoTracking() on o.CustomerId equals c.Id into cc
                    from c in cc.DefaultIfEmpty()
                    where o.Code.ToLower().Contains(s)
                       || ((c.Email ?? "").ToLower().Contains(s))
                       || ((c.Phone ?? "").Contains(s))
                    select o;
        }

        return await query
            .OrderByDescending(o => o.PlacedAt)
            .Take(top)
            .Select(o => new OrderListItemDto(
                o.Id, o.Code, o.Status,
                _db.Customers.Where(c => c.Id == o.CustomerId).Select(c => c.Email).FirstOrDefault(),
                o.GrandTotal, o.PlacedAt))
            .ToListAsync(ct);
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<OrderDetailDto>> Get(long id, CancellationToken ct = default)
    {
        var o = await _db.Orders.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (o is null) return NotFound();

        var items = await _db.OrderItems.AsNoTracking()
            .Where(i => i.OrderId == id)
            .Select(i => new OrderItemLineDto(i.VariantId, i.ProductName, i.Sku,
                                              i.Quantity, i.UnitPrice, i.UnitPrice * i.Quantity))
            .ToListAsync(ct);

        return new OrderDetailDto(
            o.Id, o.Code, o.Status, o.CustomerId, o.ShippingAddressId,
            o.Subtotal, o.DiscountTotal, o.TaxTotal, o.ShippingFee, o.GrandTotal,
            o.Note, o.Currency, o.PlacedAt, o.UpdatedAt, items);
    }

    // --- Các thao tác trạng thái đơn ---

    [HttpPost("{id:long}/mark-paid")]
    public async Task<IActionResult> MarkPaid(long id, CancellationToken ct = default)
    {
        var o = await _db.Orders.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (o is null) return NotFound();
        o.Status = "PAID";
        o.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok(new { message = "paid" });
    }

    [HttpPost("{id:long}/ship")]
    public async Task<IActionResult> Ship(long id, CancellationToken ct = default)
    {
        var o = await _db.Orders.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (o is null) return NotFound();

        // Giảm tồn kho đơn giản: lấy dòng Inventory đầu tiên theo Variant
        foreach (var it in o.Items)
        {
            var inv = await _db.Inventories.FirstOrDefaultAsync(x => x.VariantId == it.VariantId, ct);
            if (inv != null)
            {
                inv.QtyReserved = Math.Max(0, inv.QtyReserved - it.Quantity);
                inv.QtyOnHand = Math.Max(0, inv.QtyOnHand - it.Quantity);
                inv.UpdatedAt = DateTime.UtcNow;
            }
        }

        o.Status = "SHIPPED";
        o.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok(new { message = "shipped" });
    }

    [HttpPost("{id:long}/cancel")]
    public async Task<IActionResult> Cancel(long id, CancellationToken ct = default)
    {
        var o = await _db.Orders.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (o is null) return NotFound();

        // Nhả tồn đã reserve
        foreach (var it in o.Items)
        {
            var inv = await _db.Inventories.FirstOrDefaultAsync(x => x.VariantId == it.VariantId, ct);
            if (inv != null)
            {
                inv.QtyReserved = Math.Max(0, inv.QtyReserved - it.Quantity);
                inv.UpdatedAt = DateTime.UtcNow;
            }
        }

        o.Status = "CANCELLED";
        o.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Ok(new { message = "cancelled" });
    }
}
