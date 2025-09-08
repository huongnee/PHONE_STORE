using Microsoft.EntityFrameworkCore;
using PHONE_STORE.Application.Dtos;
using PHONE_STORE.Application.Interfaces;
using PHONE_STORE.Infrastructure.Data;
using PHONE_STORE.Infrastructure.Entities;

namespace PHONE_STORE.Infrastructure.Repositories;

public class WarehouseRepository : IWarehouseRepository
{
    private readonly PhoneDbContext _db;
    public WarehouseRepository(PhoneDbContext db) => _db = db;

    public async Task<List<WarehouseDto>> GetAllAsync(CancellationToken ct = default)
        => await _db.Warehouses
            .OrderByDescending(x => x.CreatedAt)
            .Select(w => new WarehouseDto(w.Id, w.Code, w.Name, w.AddressLine, w.District, w.Province, w.IsActive))
            .ToListAsync(ct);

    public async Task<WarehouseDto?> GetAsync(long id, CancellationToken ct = default)
        => await _db.Warehouses.Where(w => w.Id == id)
            .Select(w => new WarehouseDto(w.Id, w.Code, w.Name, w.AddressLine, w.District, w.Province, w.IsActive))
            .FirstOrDefaultAsync(ct);

    public async Task<long> CreateAsync(WarehouseUpsertDto dto, CancellationToken ct = default)
    {
        var e = new Warehouse
        {
            Code = dto.Code.Trim(),
            Name = dto.Name.Trim(),
            AddressLine = dto.AddressLine?.Trim(),
            District = dto.District?.Trim(),
            Province = dto.Province?.Trim(),
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };
        _db.Warehouses.Add(e);
        await _db.SaveChangesAsync(ct);
        return e.Id;
    }

    public async Task<bool> UpdateAsync(long id, WarehouseUpsertDto dto, CancellationToken ct = default)
    {
        var e = await _db.Warehouses.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e == null) return false;
        e.Code = dto.Code.Trim();
        e.Name = dto.Name.Trim();
        e.AddressLine = dto.AddressLine?.Trim();
        e.District = dto.District?.Trim();
        e.Province = dto.Province?.Trim();
        e.IsActive = dto.IsActive;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<List<WarehouseOptionDto>> GetOptionsAsync(CancellationToken ct = default)
        => await _db.Warehouses
            .Where(w => w.IsActive)
            .OrderBy(w => w.Name)
            .Select(w => new WarehouseOptionDto(w.Id, $"{w.Code} - {w.Name}"))
            .ToListAsync(ct);
}