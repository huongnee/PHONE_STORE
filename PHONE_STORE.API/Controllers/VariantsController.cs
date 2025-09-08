using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PHONE_STORE.Application.Dtos;
using PHONE_STORE.Application.Interfaces;

namespace PHONE_STORE.API.Controllers
{
    [ApiController]
    [Route("api/admin/products/{productId:long}/variants")]
    [Authorize(Roles = "ADMIN,STAFF")]
    public class VariantsController : ControllerBase
    {
        private readonly IVariantRepository _repo;
        public VariantsController(IVariantRepository repo) => _repo = repo;

        [HttpGet]
        public Task<List<VariantDto>> List(long productId, CancellationToken ct)
            => _repo.ListByProductAsync(productId, ct);

        [HttpGet("{id:long}")]
        public Task<VariantDto?> Get(long id, CancellationToken ct)
            => _repo.GetAsync(id, ct);

        [HttpPost]
        public async Task<IActionResult> Create(long productId, [FromBody] VariantCreateDto dto, CancellationToken ct)
        {
            var id = await _repo.CreateAsync(dto with { ProductId = productId }, ct);
            return CreatedAtAction(nameof(Get), new { id, productId }, new { id, message = "Tạo biến thể thành công." });
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(long id, [FromBody] VariantUpdateDto dto, CancellationToken ct)
            => (await _repo.UpdateAsync(id, dto, ct)) ? NoContent() : NotFound();

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id, CancellationToken ct)
            => (await _repo.DeleteAsync(id, ct)) ? NoContent() : NotFound();
    }

}
