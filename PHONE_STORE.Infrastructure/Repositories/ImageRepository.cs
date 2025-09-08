using Microsoft.EntityFrameworkCore;
using PHONE_STORE.Application.Dtos;
using PHONE_STORE.Application.Interfaces;
using PHONE_STORE.Infrastructure.Data;
using PHONE_STORE.Infrastructure.Entities;

namespace PHONE_STORE.Infrastructure.Repositories;

public class ImageRepository : IImageRepository
{
    private readonly PhoneDbContext _db;
    public ImageRepository(PhoneDbContext db) => _db = db;

    public Task<List<ImageDto>> ListForProductAsync(long productId, CancellationToken ct)
        => _db.ProductImages.AsNoTracking()
           .Where(i => i.ProductId == productId)
           .OrderBy(i => i.SortOrder)
           .Select(i => new ImageDto(i.Id, i.ProductId, i.VariantId, i.ImageUrl, i.AltText, i.IsPrimary, i.SortOrder))
           .ToListAsync(ct);

    public Task<List<ImageDto>> ListForVariantAsync(long variantId, CancellationToken ct)
        => _db.ProductImages.AsNoTracking()
           .Where(i => i.VariantId == variantId)
           .OrderBy(i => i.SortOrder)
           .Select(i => new ImageDto(i.Id, i.ProductId, i.VariantId, i.ImageUrl, i.AltText, i.IsPrimary, i.SortOrder))
           .ToListAsync(ct);

    public async Task<long> CreateAsync(ImageCreateDto dto, CancellationToken ct)
    {
        if ((dto.ProductId.HasValue && dto.VariantId.HasValue) || (!dto.ProductId.HasValue && !dto.VariantId.HasValue))
            throw new InvalidOperationException("Phải chọn hoặc ProductId hoặc VariantId (không đồng thời).");

        if (dto.ProductId.HasValue && await _db.Products.CountAsync(p => p.Id == dto.ProductId.Value, ct) == 0)
            throw new InvalidOperationException("Product không tồn tại.");
        if (dto.VariantId.HasValue && await _db.ProductVariants.CountAsync(v => v.Id == dto.VariantId.Value, ct) == 0)
            throw new InvalidOperationException("Variant không tồn tại.");

        if (dto.IsPrimary)
        {
            if (dto.ProductId.HasValue)
                await _db.ProductImages.Where(i => i.ProductId == dto.ProductId.Value).ExecuteUpdateAsync(u => u.SetProperty(i => i.IsPrimary, false), ct);
            if (dto.VariantId.HasValue)
                await _db.ProductImages.Where(i => i.VariantId == dto.VariantId.Value).ExecuteUpdateAsync(u => u.SetProperty(i => i.IsPrimary, false), ct);
        }

        var img = new ProductImage
        {
            ProductId = dto.ProductId,
            VariantId = dto.VariantId,
            ImageUrl = dto.ImageUrl,
            AltText = dto.AltText,
            IsPrimary = dto.IsPrimary,
            SortOrder = dto.SortOrder
        };
        _db.ProductImages.Add(img);
        await _db.SaveChangesAsync(ct);
        return img.Id;
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken ct)
    {
        var it = await _db.ProductImages.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (it is null) return false;
        _db.ProductImages.Remove(it);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
