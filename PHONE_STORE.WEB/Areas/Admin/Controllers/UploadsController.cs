using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PHONE_STORE.WEB.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "ADMIN,STAFF")]
[Route("Admin/[controller]")]
public class UploadsController : Controller
{
    private readonly IWebHostEnvironment _env;
    public UploadsController(IWebHostEnvironment env) => _env = env;

    [HttpPost("Image")]
    [RequestSizeLimit(10_000_000)] // 10MB
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Image(IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0) return BadRequest(new { message = "File trống." });
        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Chỉ chấp nhận ảnh." });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allow = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        if (!allow.Contains(ext)) return BadRequest(new { message = "Định dạng ảnh không hỗ trợ." });

        var now = DateTime.UtcNow;
        var relFolder = Path.Combine("uploads", "products", now.ToString("yyyy"), now.ToString("MM"));
        var absFolder = Path.Combine(_env.WebRootPath, relFolder);
        Directory.CreateDirectory(absFolder);

        var safeName = $"{Guid.NewGuid():N}{ext}";
        var absPath = Path.Combine(absFolder, safeName);
        await using (var fs = System.IO.File.Create(absPath))
            await file.CopyToAsync(fs, ct);

        // URL tuyệt đối để chèn vào DB
        var url = $"{Request.Scheme}://{Request.Host}/{relFolder.Replace("\\", "/")}/{safeName}";
        return Ok(new { url });
    }
}
