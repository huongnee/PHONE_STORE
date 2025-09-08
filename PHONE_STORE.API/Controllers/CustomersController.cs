using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PHONE_STORE.Application.Dtos;
using PHONE_STORE.Application.Interfaces;

namespace PHONE_STORE.API.Controllers;

[ApiController]
[Route("api/admin/customers")]
[Authorize(Roles = "ADMIN,STAFF")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerRepository _cus;
    private readonly IAddressRepository _addr;

    public CustomersController(ICustomerRepository cus, IAddressRepository addr)
    {
        _cus = cus; _addr = addr;
    }

    [HttpGet]
    public async Task<ActionResult<List<CustomerDto>>> Search([FromQuery] string? q, [FromQuery] int top = 100, CancellationToken ct = default)
        => await _cus.SearchAsync(q, top, ct);

    [HttpGet("{id:long}")]
    public async Task<ActionResult<CustomerDto?>> Get(long id, CancellationToken ct = default)
    {
        var dto = await _cus.GetAsync(id, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CustomerUpsertDto dto, CancellationToken ct = default)
    {
        var id = await _cus.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(Get), new { id }, new { id });
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] CustomerUpsertDto dto, CancellationToken ct = default)
    {
        var ok = await _cus.UpdateAsync(id, dto, ct);
        return ok ? Ok(new { message = "updated" }) : NotFound();
    }

    // ------ Addresses (nested) ------
    [HttpGet("{id:long}/addresses")]
    public async Task<ActionResult<List<AddressDto>>> ListAddresses(long id, CancellationToken ct = default)
        => await _addr.GetByCustomerAsync(id, ct);

    [HttpPost("{id:long}/addresses")]
    public async Task<IActionResult> CreateAddress(long id, [FromBody] AddressUpsertDto dto, CancellationToken ct = default)
    {
        var aid = await _addr.CreateAsync(id, dto, ct);
        return CreatedAtAction(nameof(GetAddress), new { id, addressId = aid }, new { id = aid });
    }

    [HttpGet("{id:long}/addresses/{addressId:long}")]
    public async Task<ActionResult<AddressDto?>> GetAddress(long id, long addressId, CancellationToken ct = default)
    {
        var dto = await _addr.GetAsync(addressId, ct);
        return dto is null || dto.CustomerId != id ? NotFound() : Ok(dto);
    }

    [HttpPut("{id:long}/addresses/{addressId:long}")]
    public async Task<IActionResult> UpdateAddress(long id, long addressId, [FromBody] AddressUpsertDto dto, CancellationToken ct = default)
    {
        var ok = await _addr.UpdateAsync(addressId, dto, ct);
        return ok ? Ok(new { message = "updated" }) : NotFound();
    }

    [HttpPost("{id:long}/addresses/{addressId:long}/set-default")]
    public async Task<IActionResult> SetDefault(long id, long addressId, [FromQuery] string type = "SHIPPING", CancellationToken ct = default)
    {
        var ok = await _addr.SetDefaultAsync(addressId, type, ct);
        return ok ? Ok(new { message = "set_default" }) : NotFound();
    }

    [HttpDelete("{id:long}/addresses/{addressId:long}")]
    public async Task<IActionResult> DeleteAddress(long id, long addressId, CancellationToken ct = default)
    {
        var ok = await _addr.DeleteAsync(addressId, ct);
        return ok ? Ok(new { message = "deleted" }) : NotFound();
    }
}