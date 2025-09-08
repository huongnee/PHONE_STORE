using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PHONE_STORE.Application.Dtos;
using PHONE_STORE.Application.Interfaces;

namespace PHONE_STORE.API.Controllers
{
    [ApiController]
    [Route("api/admin/variants/{variantId:long}/prices")]
    [Authorize(Roles = "ADMIN,STAFF")]
    public class PricesController : ControllerBase
    {
        private readonly IPriceRepository _repo;
        public PricesController(IPriceRepository repo) => _repo = repo;

        [HttpGet]
        public Task<List<PriceDto>> List(long variantId, CancellationToken ct)
            => _repo.ListByVariantAsync(variantId, ct);

        [HttpGet("active")]
        public Task<PriceDto?> Active(long variantId, [FromQuery] DateTime? at, CancellationToken ct)
            => _repo.GetActiveAsync(variantId, at, ct);

        [HttpPost]
        public async Task<IActionResult> Create(long variantId, [FromBody] PriceUpsertDto dto, CancellationToken ct)
        {
            var id = await _repo.CreateAsync(variantId, dto, ct);
            return CreatedAtAction(nameof(List), new { variantId }, new { id, message = "Tạo giá thành công." });
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(long id, [FromBody] PriceUpsertDto dto, CancellationToken ct)
            => (await _repo.UpdateAsync(id, dto, ct)) ? NoContent() : NotFound();

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id, CancellationToken ct)
            => (await _repo.DeleteAsync(id, ct)) ? NoContent() : NotFound();
    }

}
