using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PHONE_STORE.Application.Dtos;
using PHONE_STORE.Application.Interfaces;

[ApiController]
[Route("api/admin/brands")]
[Authorize(Roles = "ADMIN,STAFF")]
public class BrandsController : ControllerBase
{
    private readonly IBrandRepository _repo;
    public BrandsController(IBrandRepository repo) => _repo = repo;

    [HttpGet]
    public Task<PagedResult<BrandDto>> Search([FromQuery] string? q, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        => _repo.SearchAsync(q, page, pageSize, ct);

    [HttpGet("{id:long}")]
    public Task<BrandDto?> Get(long id, CancellationToken ct) => _repo.GetAsync(id, ct);
    [HttpGet("options")]
    public Task<List<IdNameDto>> Options(CancellationToken ct)
    => _repo.GetOptionsAsync(ct);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BrandCreateDto dto, CancellationToken ct)
    {
        try
        {
            var id = await _repo.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(Get), new { id }, new { id });
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        catch (DbUpdateException ex) { return BadRequest(new { message = ex.InnerException?.Message ?? ex.Message }); }
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] BrandUpdateDto dto, CancellationToken ct)
    {
        try
        {
            var ok = await _repo.UpdateAsync(id, dto, ct);
            return ok ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        catch (DbUpdateException ex) { return BadRequest(new { message = ex.InnerException?.Message ?? ex.Message }); }
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
        => await _repo.DeleteAsync(id, ct) ? NoContent() : NotFound();
}
