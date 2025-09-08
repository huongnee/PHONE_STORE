using PHONE_STORE.Application.Dtos;
using PHONE_STORE.Application.Interfaces;
using PHONE_STORE.Infrastructure.Data;
using PHONE_STORE.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PHONE_STORE.Infrastructure.Repositories
{
    public class VariantRepository : IVariantRepository
    {
        private readonly PhoneDbContext _db;
        public VariantRepository(PhoneDbContext db) => _db = db;

        public async Task<List<VariantDto>> ListByProductAsync(long productId, CancellationToken ct)
            => await _db.ProductVariants.AsNoTracking()
                .Where(v => v.ProductId == productId)
                .OrderByDescending(v => v.IsActive).ThenBy(v => v.Sku)
                .Select(v => new VariantDto(v.Id, v.ProductId, v.Sku, v.Color, v.StorageGb, v.Barcode, v.WeightGram, v.IsActive))
                .ToListAsync(ct);

        public async Task<VariantDto?> GetAsync(long id, CancellationToken ct)
            => await _db.ProductVariants.AsNoTracking()
                .Where(v => v.Id == id)
                .Select(v => new VariantDto(v.Id, v.ProductId, v.Sku, v.Color, v.StorageGb, v.Barcode, v.WeightGram, v.IsActive))
                .FirstOrDefaultAsync(ct);

        public async Task<long> CreateAsync(VariantCreateDto dto, CancellationToken ct)
        {
            if (await _db.Products.CountAsync(p => p.Id == dto.ProductId, ct) == 0)
                throw new InvalidOperationException("Product không tồn tại.");

            if (await _db.ProductVariants.AnyAsync(v => v.Sku.ToUpper() == dto.Sku.ToUpper(), ct))
                throw new InvalidOperationException("SKU đã tồn tại.");

            var v = new ProductVariant
            {
                ProductId = dto.ProductId,
                Sku = dto.Sku.Trim(),
                Color = dto.Color,
                StorageGb = dto.StorageGb,
                Barcode = dto.Barcode,
                WeightGram = dto.WeightGram,
                IsActive = dto.IsActive
            };
            _db.ProductVariants.Add(v);
            await _db.SaveChangesAsync(ct);
            return v.Id;
        }

        public async Task<bool> UpdateAsync(long id, VariantUpdateDto dto, CancellationToken ct)
        {
            var v = await _db.ProductVariants.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (v is null) return false;

            if (await _db.ProductVariants.AnyAsync(x => x.Id != id && x.Sku.ToUpper() == dto.Sku.ToUpper(), ct))
                throw new InvalidOperationException("SKU đã tồn tại.");

            v.Sku = dto.Sku.Trim();
            v.Color = dto.Color;
            v.StorageGb = dto.StorageGb;
            v.Barcode = dto.Barcode;
            v.WeightGram = dto.WeightGram;
            v.IsActive = dto.IsActive;

            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteAsync(long id, CancellationToken ct)
        {
            // cấm xoá nếu còn price đang hiệu lực (tuỳ chính sách)
            var v = await _db.ProductVariants.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (v is null) return false;
            _db.ProductVariants.Remove(v);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
