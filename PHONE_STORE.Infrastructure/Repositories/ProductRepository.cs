using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using PHONE_STORE.Application.Dtos;
using PHONE_STORE.Application.Interfaces;
using PHONE_STORE.Infrastructure.Data;
using PHONE_STORE.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace PHONE_STORE.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly PhoneDbContext _db;
    public ProductRepository(PhoneDbContext db) => _db = db;

    public async Task<PagedResult<ProductListItemDto>> SearchAsync(string? q, long? brandId, int page, int pageSize, CancellationToken ct)
    {
        var query = _db.Products.AsNoTracking()
            .Include(p => p.Brand)
            .Include(p => p.DefaultCategory)
            .Include(p => p.Variants.Where(v => v.IsActive))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var s = q.Trim().ToUpperInvariant();
            query = query.Where(x => x.Name.ToUpper().Contains(s) || x.Slug.ToUpper().Contains(s));
        }
        if (brandId.HasValue) query = query.Where(x => x.BrandId == brandId.Value);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(x => x.IsActive).ThenBy(x => x.Name)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new ProductListItemDto(
                x.Id,
                x.Name,
                x.Slug,
                x.Brand!.Name,
                x.DefaultCategory != null ? x.DefaultCategory.Name : null, // -> CategoryName
                x.IsActive,
                x.Variants.Count                                        // -> ActiveVariantCount
            ))

            .ToListAsync(ct);

        return new(items, total, page, pageSize);
    }

    public async Task<ProductDto?> GetAsync(long id, CancellationToken ct)
        => await _db.Products.AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new ProductDto(
                p.Id, p.BrandId, p.DefaultCategoryId, p.Name, p.Slug,
                p.Description, p.SpecJson, p.IsActive))
            .FirstOrDefaultAsync(ct);

    public async Task<long> CreateAsync(ProductCreateDto dto, CancellationToken ct)
    {
        // FK tồn tại (COUNT để tránh TRUE/FALSE)
        if (await _db.Brands.CountAsync(b => b.Id == dto.BrandId, ct) == 0)
            throw new InvalidOperationException("Brand không tồn tại.");
        if (dto.DefaultCategoryId.HasValue &&
            await _db.Categories.CountAsync(c => c.Id == dto.DefaultCategoryId.Value, ct) == 0)
            throw new InvalidOperationException("Category mặc định không tồn tại.");

        var slug = Slugify(string.IsNullOrWhiteSpace(dto.Slug) ? dto.Name : dto.Slug!);
        if (await SlugExistsAsync(slug, null, ct)) throw new InvalidOperationException("Slug đã tồn tại.");

        if (!string.IsNullOrWhiteSpace(dto.SpecJson))
            _ = JsonDocument.Parse(dto.SpecJson); // validate JSON

        var p = new Product
        {
            BrandId = dto.BrandId,
            DefaultCategoryId = dto.DefaultCategoryId,
            Name = dto.Name.Trim(),
            Slug = slug,
            Description = dto.Description,
            SpecJson = dto.SpecJson,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.Products.Add(p);
        await _db.SaveChangesAsync(ct);
        return p.Id;
    }

    public async Task<bool> UpdateAsync(long id, ProductUpdateDto dto, CancellationToken ct)
    {
        var p = await _db.Products.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (p is null) return false;

        if (await _db.Brands.CountAsync(b => b.Id == dto.BrandId, ct) == 0)
            throw new InvalidOperationException("Brand không tồn tại.");
        if (dto.DefaultCategoryId.HasValue &&
            await _db.Categories.CountAsync(c => c.Id == dto.DefaultCategoryId.Value, ct) == 0)
            throw new InvalidOperationException("Category mặc định không tồn tại.");

        var slug = Slugify(string.IsNullOrWhiteSpace(dto.Slug) ? dto.Name : dto.Slug!);
        if (await SlugExistsAsync(slug, id, ct)) throw new InvalidOperationException("Slug đã tồn tại.");

        if (!string.IsNullOrWhiteSpace(dto.SpecJson))
            _ = JsonDocument.Parse(dto.SpecJson);

        p.BrandId = dto.BrandId;
        p.DefaultCategoryId = dto.DefaultCategoryId;
        p.Name = dto.Name.Trim();
        p.Slug = slug;
        p.Description = dto.Description;
        p.SpecJson = dto.SpecJson;
        p.IsActive = dto.IsActive;

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken ct)
    {
        // Có thể cấm xoá nếu còn variants
        if (await _db.ProductVariants.AnyAsync(v => v.ProductId == id, ct))
            throw new InvalidOperationException("Không thể xoá vì còn biến thể (SKU).");

        var p = await _db.Products.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (p is null) return false;
        _db.Products.Remove(p);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> SlugExistsAsync(string slug, long? exceptId, CancellationToken ct)
    {
        var q = _db.Products.AsNoTracking().Where(x => x.Slug.ToLower() == slug.ToLower());
        if (exceptId.HasValue) q = q.Where(x => x.Id != exceptId.Value);
        return (await q.CountAsync(ct)) > 0;
    }

    private static string Slugify(string s)
    {
        var slug = s.Trim().ToLowerInvariant();
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");
        slug = Regex.Replace(slug, "-{2,}", "-").Trim('-');
        return slug;
    }
}
