using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PHONE_STORE.Application.Dtos;
using PHONE_STORE.Application.Interfaces;

[ApiController]
[Route("api/admin/categories")]
[Authorize(Roles = "ADMIN,STAFF")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryRepository _repo;
    public CategoriesController(ICategoryRepository repo) => _repo = repo;

    [HttpGet]
    public Task<PagedResult<CategoryDto>> Search([FromQuery] string? q, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default)
        => _repo.SearchAsync(q, page, pageSize, ct);

    [HttpGet("{id:long}")]
    public Task<CategoryDto?> Get(long id, CancellationToken ct) => _repo.GetAsync(id, ct);

    [HttpGet("options")]
    public async Task<IActionResult> Options([FromQuery] long? excludeId, CancellationToken ct)
    {
        try
        {
            var list = await _repo.GetOptionsAsync(excludeId, ct);
            return Ok(list);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());              // log ra console để nhìn nguyên nhân
            return BadRequest(new { message = ex.Message }); // Web sẽ show message này
        }
    }


    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CategoryCreateDto dto, CancellationToken ct)
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
    public async Task<IActionResult> Update(long id, [FromBody] CategoryUpdateDto dto, CancellationToken ct)
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
    {
        try
        {
            var ok = await _repo.DeleteAsync(id, ct);
            return ok ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
        catch (DbUpdateException ex) { return BadRequest(new { message = ex.InnerException?.Message ?? ex.Message }); }
    }
}
