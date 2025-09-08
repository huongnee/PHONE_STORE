using Microsoft.EntityFrameworkCore;
using PHONE_STORE.Application.Dtos;
using PHONE_STORE.Application.Interfaces;
using PHONE_STORE.Infrastructure.Data;
using PHONE_STORE.Infrastructure.Entities;

namespace PHONE_STORE.Infrastructure.Repositories;

public class PriceRepository : IPriceRepository
{
    private readonly PhoneDbContext _db;
    public PriceRepository(PhoneDbContext db) => _db = db;

    public async Task<List<PriceDto>> ListByVariantAsync(long variantId, CancellationToken ct)
        => await _db.ProductPrices.AsNoTracking()
            .Where(p => p.VariantId == variantId)
            .OrderByDescending(p => p.StartsAt)
            .Select(p => new PriceDto(p.Id, p.VariantId, p.ListPrice, p.SalePrice, p.Currency, p.StartsAt, p.EndsAt))
            .ToListAsync(ct);

    public async Task<long> CreateAsync(long variantId, PriceUpsertDto dto, CancellationToken ct)
    {
        if (await _db.ProductVariants.CountAsync(v => v.Id == variantId, ct) == 0)
            throw new InvalidOperationException("Variant không tồn tại.");
        if (dto.SalePrice.HasValue && dto.SalePrice.Value > dto.ListPrice)
            throw new InvalidOperationException("SalePrice phải ≤ ListPrice.");

        // 8.1 Check overlap
        var s = dto.StartsAt;
        var e = dto.EndsAt ?? DateTime.MaxValue;
        var overlap = await _db.ProductPrices.AnyAsync(p =>
            p.VariantId == variantId &&
            p.StartsAt <= e &&
            (p.EndsAt ?? DateTime.MaxValue) >= s
        , ct);
        if (overlap)
            throw new InvalidOperationException("Khoảng giá bị chồng lấn.");

        var p = new ProductPrice
        {
            VariantId = variantId,
            ListPrice = dto.ListPrice,
            SalePrice = dto.SalePrice,
            Currency = dto.Currency ?? "VND",
            StartsAt = dto.StartsAt,
            EndsAt = dto.EndsAt
        };
        _db.ProductPrices.Add(p);
        await _db.SaveChangesAsync(ct);
        return p.Id;
    }

    public async Task<bool> UpdateAsync(long id, PriceUpsertDto dto, CancellationToken ct)
    {
        var p = await _db.ProductPrices.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (p is null) return false;
        if (dto.SalePrice.HasValue && dto.SalePrice.Value > dto.ListPrice)
            throw new InvalidOperationException("SalePrice phải ≤ ListPrice.");

        // 8.2 Check overlap (trừ chính nó)
        var s = dto.StartsAt;
        var e = dto.EndsAt ?? DateTime.MaxValue;
        var overlap = await _db.ProductPrices.AnyAsync(x =>
            x.VariantId == p.VariantId &&
            x.Id != id &&
            x.StartsAt <= e &&
            (x.EndsAt ?? DateTime.MaxValue) >= s
        , ct);
        if (overlap)
            throw new InvalidOperationException("Khoảng giá bị chồng lấn.");

        p.ListPrice = dto.ListPrice;
        p.SalePrice = dto.SalePrice;
        p.Currency = dto.Currency ?? "VND";
        p.StartsAt = dto.StartsAt;
        p.EndsAt = dto.EndsAt;

        await _db.SaveChangesAsync(ct);
        return true;
    }


    public async Task<bool> DeleteAsync(long id, CancellationToken ct)
    {
        var p = await _db.ProductPrices.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (p is null) return false;
        _db.ProductPrices.Remove(p);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<PriceDto?> GetActiveAsync(long variantId, DateTime? at, CancellationToken ct)
    {
        var t = at ?? DateTime.UtcNow;
        return await _db.ProductPrices.AsNoTracking()
            .Where(p => p.VariantId == variantId
                     && p.StartsAt <= t
                     && (p.EndsAt == null || p.EndsAt >= t))
            .OrderByDescending(p => p.StartsAt)
            .Select(p => new PriceDto(p.Id, p.VariantId, p.ListPrice, p.SalePrice, p.Currency, p.StartsAt, p.EndsAt))
            .FirstOrDefaultAsync(ct);
    }
    public Task<long> UpsertAsync(PriceUpsertDto dto, CancellationToken ct)
    => CreateAsync(dto.VariantId, dto, ct);

}
