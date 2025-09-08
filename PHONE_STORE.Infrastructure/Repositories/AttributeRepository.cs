using Microsoft.EntityFrameworkCore;
using PHONE_STORE.Application.Dtos;
using PHONE_STORE.Application.Interfaces;
using PHONE_STORE.Infrastructure.Data;
using PHONE_STORE.Infrastructure.Entities;

namespace PHONE_STORE.Infrastructure.Repositories;

public class AttributeRepository : IAttributeRepository
{
    private readonly PhoneDbContext _db;
    public AttributeRepository(PhoneDbContext db) => _db = db;

    public async Task<List<AttributeDto>> ListAsync(CancellationToken ct)
        => await _db.ProductAttributes.AsNoTracking()
            .OrderBy(a => a.Name)
            .Select(a => new AttributeDto(a.Id, a.Code, a.Name, a.DataType))
            .ToListAsync(ct);

    public async Task<long> CreateAsync(AttributeCreateDto dto, CancellationToken ct)
    {
        if (await _db.ProductAttributes.AnyAsync(a => a.Code.ToUpper() == dto.Code.ToUpper(), ct))
            throw new InvalidOperationException("Mã thuộc tính đã tồn tại.");
        var a = new ProductAttribute { Code = dto.Code.Trim(), Name = dto.Name.Trim(), DataType = dto.DataType.Trim().ToUpper() };
        _db.ProductAttributes.Add(a);
        await _db.SaveChangesAsync(ct);
        return a.Id;
    }

    public async Task<bool> UpdateAsync(long id, AttributeUpdateDto dto, CancellationToken ct)
    {
        var a = await _db.ProductAttributes.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (a is null) return false;
        a.Name = dto.Name.Trim();
        a.DataType = dto.DataType.Trim().ToUpper();
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken ct)
    {
        var a = await _db.ProductAttributes.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (a is null) return false;
        _db.ProductAttributes.Remove(a);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task UpsertValueAsync(long productId, AttributeValueUpsertDto dto, CancellationToken ct)
    {
        if (await _db.Products.CountAsync(p => p.Id == productId, ct) == 0)
            throw new InvalidOperationException("Product không tồn tại.");

        var attr = await _db.ProductAttributes.FirstOrDefaultAsync(a => a.Id == dto.AttributeId, ct)
                   ?? throw new InvalidOperationException("Thuộc tính không tồn tại.");

        // one-of-four: xóa các cột còn lại
        long? i = null; decimal? d = null; bool? b = null; string? t = null;
        switch (attr.DataType.ToUpper())
        {
            case "INT": i = dto.IntValue; break;
            case "DECIMAL": d = dto.DecValue; break;
            case "BOOL": b = dto.BoolValue; break;
            default: t = dto.TextValue; break;
        }

        var existing = await _db.ProductAttributeValues
            .FirstOrDefaultAsync(x => x.ProductId == productId && x.AttributeId == dto.AttributeId, ct);

        if (existing is null)
        {
            var v = new ProductAttributeValue
            {
                ProductId = productId,
                AttributeId = dto.AttributeId,
                IntValue = i,
                DecValue = d,
                BoolValue = b,
                TextValue = t
            };
            _db.ProductAttributeValues.Add(v);
        }
        else
        {
            existing.IntValue = i; existing.DecValue = d; existing.BoolValue = b; existing.TextValue = t;
        }
        await _db.SaveChangesAsync(ct);
    }

    public async Task<List<(AttributeDto Attr, object? Value)>> ListValuesAsync(long productId, CancellationToken ct)
    {
        var q = from v in _db.ProductAttributeValues.AsNoTracking()
                join a in _db.ProductAttributes.AsNoTracking() on v.AttributeId equals a.Id
                where v.ProductId == productId
                select new { a, v };
        var list = await q.ToListAsync(ct);

        return list.Select(x =>
        {
            object? val = x.a.DataType.ToUpper() switch
            {
                "INT" => x.v.IntValue,
                "DECIMAL" => x.v.DecValue,
                "BOOL" => x.v.BoolValue,
                _ => x.v.TextValue
            };
            return (new AttributeDto(x.a.Id, x.a.Code, x.a.Name, x.a.DataType), val);
        }).ToList();
    }
}
