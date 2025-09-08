using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PHONE_STORE.Infrastructure.Data;

namespace PHONE_STORE.API.Controllers;

[ApiController]
[Route("api/shop/products")]
public class ShopProductsController : ControllerBase
{
    private readonly PhoneDbContext _db;
    public ShopProductsController(PhoneDbContext db) => _db = db;

    // Kiểu trả về cho danh sách (đơn giản, đủ dùng cho storefront)
    public record ShopProductListItem(long Id, string Name, string Slug, string? ImageUrl, decimal MinPrice);

    [HttpGet]
    public async Task<ActionResult<List<ShopProductListItem>>> List(
    [FromQuery] string? q, [FromQuery] long? catId,
    [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
    CancellationToken ct = default)
    {
        page = Math.Max(1, page); pageSize = Math.Clamp(pageSize, 1, 50);
        var now = DateTime.UtcNow;

        var baseQ = _db.Products.AsNoTracking().Where(p => p.IsActive == true);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var s = q.Trim().ToLower();
            baseQ = baseQ.Where(p => p.Name.ToLower().Contains(s) || p.Slug.ToLower().Contains(s));
        }
        if (catId.HasValue) baseQ = baseQ.Where(p => p.DefaultCategoryId == catId);

        // 1) lấy page sản phẩm
        var pageRows = await baseQ
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(p => new { p.Id, p.Name, p.Slug })
            .ToListAsync(ct);

        if (pageRows.Count == 0) return Ok(new List<ShopProductListItem>());

        var ids = pageRows.Select(x => x.Id).ToList();

        // 2) ảnh đại diện theo product
        // 2) ảnh đại diện theo product
        var imgRows = await _db.ProductImages.AsNoTracking()
            .Where(i => i.ProductId != null && ids.Contains(i.ProductId.Value))
            .GroupBy(i => i.ProductId!.Value)                   // key = long (không nullable)
            .Select(g => new
            {
                ProductId = g.Key,
                Url = g.OrderBy(i => i.SortOrder).Select(i => i.ImageUrl).FirstOrDefault()
            })
            .ToListAsync(ct);

        var imgMap = imgRows.ToDictionary(x => x.ProductId, x => x.Url);


        // 3) giá min hiện tại theo product
        var priceRows = await (
            from v in _db.ProductVariants.AsNoTracking().Where(v => ids.Contains(v.ProductId))
            join pr in _db.ProductPrices.AsNoTracking() on v.Id equals pr.VariantId
            where pr.StartsAt <= now && (pr.EndsAt == null || pr.EndsAt > now)
            select new { v.ProductId, Price = pr.SalePrice ?? pr.ListPrice }
        )
        .GroupBy(x => x.ProductId)
        .Select(g => new { ProductId = g.Key, MinPrice = (decimal?)g.Min(x => x.Price) })
        .ToListAsync(ct);
        var priceMap = priceRows.ToDictionary(x => x.ProductId, x => x.MinPrice ?? 0m);

        // 4) ghép kết quả
        var result = pageRows.Select(p => new ShopProductListItem(
            p.Id,
            p.Name,
            p.Slug,
            imgMap.TryGetValue(p.Id, out var u) ? u : null,
            priceMap.TryGetValue(p.Id, out var mp) ? mp : 0m
        )).ToList();

        return Ok(result);
    }


    // Chi tiết: product + variants (giá hiện tại) + ảnh
    public record ShopVariant(long Id, string Sku, string? Color, int? StorageGb, decimal Price);
    public record ShopImage(long Id, string Url, string? Alt, int Sort);
    public record ShopProductDetail(long Id, string Name, string Slug, string? Description,
                                    List<ShopVariant> Variants, List<ShopImage> Images);

    [HttpGet("{slug}")]
    public async Task<ActionResult<ShopProductDetail>> Details(string slug, CancellationToken ct)
    {
        var p = await _db.Products.AsNoTracking()
            .Include(x => x.Variants)
            .Include(x => x.Images)
            .FirstOrDefaultAsync(x => x.Slug == slug && x.IsActive == true, ct);

        if (p is null) return NotFound();

        var now = DateTime.UtcNow;
        var priceMap = await _db.ProductPrices.AsNoTracking()
            .Where(pr => p.Variants.Select(v => v.Id).Contains(pr.VariantId)
                         && pr.StartsAt <= now && (pr.EndsAt == null || pr.EndsAt > now))
            .GroupBy(pr => pr.VariantId)
            .Select(g => new { VariantId = g.Key, Price = (decimal)g.OrderByDescending(x => x.StartsAt).Select(x => x.SalePrice ?? x.ListPrice).FirstOrDefault() })
            .ToListAsync(ct);

        var variants = p.Variants.Select(v =>
            new ShopVariant(v.Id, v.Sku, v.Color, v.StorageGb,
                priceMap.FirstOrDefault(x => x.VariantId == v.Id)?.Price ?? 0)).ToList();

        var images = p.Images
                        .OrderBy(i => i.SortOrder)
                        .Select(i => new ShopImage(i.Id, i.ImageUrl, i.AltText, i.SortOrder))
                        .ToList();

        var dto = new ShopProductDetail(p.Id, p.Name, p.Slug, p.Description, variants, images);
        return Ok(dto);
    }
}
