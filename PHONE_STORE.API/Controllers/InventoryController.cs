using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PHONE_STORE.Application.Dtos;
using PHONE_STORE.Application.Interfaces;

namespace PHONE_STORE.API.Controllers;

[ApiController]
[Route("api/admin/inventory")]
[Authorize(Roles = "ADMIN,STAFF")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryRepository _inv;
    private readonly IStockService _stock;

    public InventoryController(IInventoryRepository inv, IStockService stock)
    {
        _inv = inv;
        _stock = stock;
    }

    [HttpGet]
    public async Task<ActionResult<object>> Get([FromQuery] long? variantId, [FromQuery] long? warehouseId, CancellationToken ct)
    {
        if (variantId.HasValue && warehouseId.HasValue)
        {
            var one = await _inv.GetAsync(variantId.Value, warehouseId.Value, ct);
            return Ok(one);
        }
        if (variantId.HasValue)
            return Ok(await _inv.GetByVariantAsync(variantId.Value, ct));
        if (warehouseId.HasValue)
            return Ok(await _inv.GetByWarehouseAsync(warehouseId.Value, ct));
        return BadRequest(new { message = "Cần cung cấp variantId hoặc warehouseId" });
    }

    [HttpPost("move")]
    public async Task<IActionResult> Move([FromBody] StockMoveRequestDto req, CancellationToken ct)
    {
        long? userId = null;
        var idStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (long.TryParse(idStr, out var tmp)) userId = tmp;

        var (ok, err) = await _stock.MoveAsync(req, userId, ct);
        if (!ok) return BadRequest(new { message = err });
        return Ok(new { message = "moved" });
    }
}