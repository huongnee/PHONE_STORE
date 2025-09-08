using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PHONE_STORE.Application.Dtos;
using PHONE_STORE.Application.Interfaces;

namespace PHONE_STORE.API.Controllers
{
    [ApiController]
    [Route("api/admin/images")]
    [Authorize(Roles = "ADMIN,STAFF")]
    public class ImagesController : ControllerBase
    {
        private readonly IImageRepository _repo;
        public ImagesController(IImageRepository repo) => _repo = repo;

        [HttpGet("product/{productId:long}")]
        public Task<List<ImageDto>> ForProduct(long productId, CancellationToken ct)
            => _repo.ListForProductAsync(productId, ct);

        [HttpGet("variant/{variantId:long}")]
        public Task<List<ImageDto>> ForVariant(long variantId, CancellationToken ct)
            => _repo.ListForVariantAsync(variantId, ct);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ImageCreateDto dto, CancellationToken ct)
        {
            var id = await _repo.CreateAsync(dto, ct);
            return Created("", new { id, message = "Thêm ảnh thành công." });
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id, CancellationToken ct)
            => (await _repo.DeleteAsync(id, ct)) ? NoContent() : NotFound();
    }

}
