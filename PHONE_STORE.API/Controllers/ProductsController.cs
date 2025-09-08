using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PHONE_STORE.Application.Dtos;
using PHONE_STORE.Application.Interfaces;

namespace PHONE_STORE.API.Controllers
{
    [ApiController]
    [Route("api/admin/products")]
    [Authorize(Roles = "ADMIN,STAFF")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _repo;
        public ProductsController(IProductRepository repo) => _repo = repo;

        [HttpGet]
        public Task<PagedResult<ProductListItemDto>> Search([FromQuery] string? q, [FromQuery] long? brandId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
            => _repo.SearchAsync(q, brandId, page, pageSize, ct);

        [HttpGet("{id:long}")]
        public Task<ProductDto?> Get(long id, CancellationToken ct) => _repo.GetAsync(id, ct);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductCreateDto dto, CancellationToken ct)
        {
            var id = await _repo.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(Get), new { id }, new { id, message = "Tạo sản phẩm thành công." });
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(long id, [FromBody] ProductUpdateDto dto, CancellationToken ct)
            => (await _repo.UpdateAsync(id, dto, ct)) ? NoContent() : NotFound();

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id, CancellationToken ct)
            => (await _repo.DeleteAsync(id, ct)) ? NoContent() : NotFound();
    }

}
