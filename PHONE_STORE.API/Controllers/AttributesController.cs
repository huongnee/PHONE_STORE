using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PHONE_STORE.Application.Dtos;
using PHONE_STORE.Application.Interfaces;

[ApiController]
[Route("api/admin/attributes")]
[Authorize(Roles = "ADMIN,STAFF")]
public class AttributesController : ControllerBase
{
    private readonly IAttributeRepository _repo;
    public AttributesController(IAttributeRepository repo) => _repo = repo;

    [HttpGet]
    public Task<List<AttributeDto>> List(CancellationToken ct) => _repo.ListAsync(ct);

    [HttpPost]
    public async Task<IActionResult> Create(AttributeCreateDto dto, CancellationToken ct)
    {
        var id = await _repo.CreateAsync(dto, ct);
        return Created("", new { id, message = "Created" });
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, AttributeUpdateDto dto, CancellationToken ct)
        => (await _repo.UpdateAsync(id, dto, ct)) ? NoContent() : NotFound();

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
        => (await _repo.DeleteAsync(id, ct)) ? NoContent() : NotFound();
}

[ApiController]
[Route("api/admin/products/{productId:long}/attributes")]
[Authorize(Roles = "ADMIN,STAFF")]
public class ProductAttributesController : ControllerBase
{
    private readonly IAttributeRepository _repo;
    public ProductAttributesController(IAttributeRepository repo) => _repo = repo;

    public record ProductAttrValueDto(AttributeDto Attr, object? Value);

    [HttpGet]
    public async Task<List<ProductAttrValueDto>> List(long productId, CancellationToken ct)
    {
        var list = await _repo.ListValuesAsync(productId, ct);
        return list.Select(x => new ProductAttrValueDto(x.Attr, x.Value)).ToList();
    }

    [HttpPost("upsert")]
    public async Task<IActionResult> Upsert(long productId, AttributeValueUpsertDto dto, CancellationToken ct)
    {
        await _repo.UpsertValueAsync(productId, dto, ct);
        return Ok(new { message = "Updated" });
    }
}
