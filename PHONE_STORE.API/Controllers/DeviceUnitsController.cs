using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PHONE_STORE.Application.Dtos;
using PHONE_STORE.Infrastructure.Data;

namespace PHONE_STORE.API.Controllers;

[ApiController]
[Route("api/admin/device-units")]
[Authorize(Roles = "ADMIN,STAFF")]
public class DeviceUnitsController : ControllerBase
{
    private readonly PhoneDbContext _db;
    public DeviceUnitsController(PhoneDbContext db) => _db = db;

    // GET /api/admin/device-units?imei=...&variantId=...&warehouseId=...&top=100
    [HttpGet]
    public async Task<ActionResult<List<DeviceUnitDto>>> Search(
        [FromQuery] string? imei, [FromQuery] long? variantId,
        [FromQuery] long? warehouseId, [FromQuery] int top = 100,
        CancellationToken ct = default)
    {
        var q = _db.DeviceUnits.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(imei))
        {
            var key = imei.Trim();
            q = q.Where(x => x.Imei.Contains(key));
        }
        if (variantId.HasValue) q = q.Where(x => x.VariantId == variantId.Value);
        if (warehouseId.HasValue) q = q.Where(x => x.WarehouseId == warehouseId.Value);

        top = Math.Clamp(top, 1, 500);
        var list = await q.OrderByDescending(x => x.ReceivedAt).Take(top)
            .Select(u => new DeviceUnitDto(u.Id, u.VariantId, u.Imei, u.SerialNo, u.Status,
                                           u.WarehouseId, u.ReceivedAt, u.SoldAt, u.ReturnedAt))
            .ToListAsync(ct);
        return Ok(list);
    }

    // GET /api/admin/device-units/123
    [HttpGet("{id:long}")]
    public async Task<ActionResult<DeviceUnitDto>> Get(long id, CancellationToken ct = default)
    {
        var u = await _db.DeviceUnits.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (u == null) return NotFound();
        return new DeviceUnitDto(u.Id, u.VariantId, u.Imei, u.SerialNo, u.Status,
                                 u.WarehouseId, u.ReceivedAt, u.SoldAt, u.ReturnedAt);
    }
}
