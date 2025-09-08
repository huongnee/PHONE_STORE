using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PHONE_STORE.Application.Dtos;
using PHONE_STORE.Application.Interfaces;

namespace PHONE_STORE.API.Controllers;

[ApiController]
[Route("api/admin/warehouses")]
[Authorize(Roles = "ADMIN,STAFF")]
public class WarehousesController : ControllerBase
{
    private readonly IWarehouseRepository _repo;
    public WarehousesController(IWarehouseRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<ActionResult<List<WarehouseDto>>> GetAll(CancellationToken ct)
        => await _repo.GetAllAsync(ct);

    [HttpGet("{id:long}")]
    public async Task<ActionResult<WarehouseDto?>> Get(long id, CancellationToken ct)
    {
        var dto = await _repo.GetAsync(id, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] WarehouseUpsertDto dto, CancellationToken ct)
    {
        var id = await _repo.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(Get), new { id }, new { id });
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] WarehouseUpsertDto dto, CancellationToken ct)
    {
        var ok = await _repo.UpdateAsync(id, dto, ct);
        return ok ? Ok(new { message = "updated" }) : NotFound();
    }

    [HttpGet("options")]
    public async Task<ActionResult<List<WarehouseOptionDto>>> Options(CancellationToken ct)
        => await _repo.GetOptionsAsync(ct);
}
