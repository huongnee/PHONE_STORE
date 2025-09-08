using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PHONE_STORE.Application.Dtos;
using PHONE_STORE.Infrastructure.Data;
using System.Security.Claims;

namespace PHONE_STORE.API.Controllers;

[ApiController]
[Route("api/shop/me")]
[Authorize]
public class ShopMeController : ControllerBase
{
    private readonly PhoneDbContext _db;
    public ShopMeController(PhoneDbContext db) => _db = db;

    private long? GetUserId()
    {
        var s = User.FindFirstValue("sub")
                 ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? User.FindFirstValue("uid")
                 ?? User.FindFirstValue("userId");
        return long.TryParse(s, out var id) ? id : (long?)null;
    }

    [HttpGet("addresses")]
    public async Task<ActionResult<List<AddressDto>>> MyAddresses(CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var cusId = await _db.Customers.AsNoTracking()
            .Where(c => c.UserAccountId == userId.Value)
            .Select(c => c.Id)
            .FirstOrDefaultAsync(ct);

        if (cusId == 0) return Ok(new List<AddressDto>());

        var rows = await _db.Addresses.AsNoTracking()
            .Where(a => a.CustomerId == cusId)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.UpdatedAt ?? a.CreatedAt)
            .Select(a => new AddressDto(
                a.Id, a.CustomerId, a.Label, a.Recipient, a.Phone, a.Line1,
                a.Ward, a.District, a.Province, a.PostalCode,
                a.AddressType, a.IsDefault,
                a.CreatedAt, a.UpdatedAt))
            .ToListAsync(ct);

        return Ok(rows);
    }
}
