using Microsoft.EntityFrameworkCore;
using PHONE_STORE.Application.Dtos;
using PHONE_STORE.Application.Interfaces;
using PHONE_STORE.Infrastructure.Data;
using PHONE_STORE.Infrastructure.Entities;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

public class BrandRepository : IBrandRepository
{
    private readonly PhoneDbContext _db;
    public BrandRepository(PhoneDbContext db) => _db = db;

    public async Task<PagedResult<BrandDto>> SearchAsync(string? q, int page, int pageSize, CancellationToken ct)
    {
        var query = _db.Brands.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            var s = q.Trim().ToUpperInvariant();
            query = query.Where(x => x.Name.ToUpper().Contains(s) || x.Slug.ToUpper().Contains(s));
        }

        var total = await query.CountAsync(ct);
        var items = await query.OrderBy(x => x.Name)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(b => new BrandDto(b.Id, b.Name, b.Slug, b.IsActive))
            .ToListAsync(ct);

        return new(items, total, page, pageSize);
    }

    public async Task<BrandDto?> GetAsync(long id, CancellationToken ct)
        => await _db.Brands.Where(b => b.Id == id)
           .Select(b => new BrandDto(b.Id, b.Name, b.Slug, b.IsActive))
           .FirstOrDefaultAsync(ct);

    public async Task<long> CreateAsync(BrandCreateDto dto, CancellationToken ct)
    {
        var slug = MakeSlug(string.IsNullOrWhiteSpace(dto.Slug) ? dto.Name : dto.Slug!);

        // kiểm tra trước để tránh đụng unique constraint
        if (await SlugExistsAsync(slug, null, ct))
            throw new InvalidOperationException("Slug đã tồn tại.");

        var b = new Brand
        {
            Name = dto.Name.Trim(),
            Slug = slug,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };
        _db.Brands.Add(b);
        await _db.SaveChangesAsync(ct);
        return b.Id;
    }

    public async Task<bool> UpdateAsync(long id, BrandUpdateDto dto, CancellationToken ct)
    {
        var b = await _db.Brands.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (b is null) return false;

        var slug = MakeSlug(string.IsNullOrWhiteSpace(dto.Slug) ? dto.Name : dto.Slug!);
        if (await SlugExistsAsync(slug, id, ct))
            throw new InvalidOperationException("Slug đã tồn tại.");

        b.Name = dto.Name.Trim();
        b.Slug = slug;
        b.IsActive = dto.IsActive;

        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken ct)
    {
        var b = await _db.Brands.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (b is null) return false;
        _db.Brands.Remove(b);                         // hoặc b.IsActive = false; nếu muốn xoá mềm
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> SlugExistsAsync(string slug, long? exceptId, CancellationToken ct)
    {
        var q = _db.Brands.AsNoTracking()
                          .Where(b => b.Slug.ToLower() == slug.ToLower());
        if (exceptId.HasValue)
            q = q.Where(b => b.Id != exceptId.Value);

        var count = await q.CountAsync(ct);   // -> SELECT COUNT(*) ...
        return count > 0;                     // so sánh ở C#
    }

    private static string MakeSlug(string s)
    {
        var slug = s.Trim().ToLowerInvariant();
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");
        slug = Regex.Replace(slug, "-{2,}", "-").Trim('-');
        return slug;
    }

    public Task<List<IdNameDto>> GetOptionsAsync(CancellationToken ct)
        => _db.Brands.AsNoTracking()
            .OrderBy(b => b.Name)
            .Select(b => new IdNameDto(b.Id, b.Name))
            .ToListAsync(ct);
}
