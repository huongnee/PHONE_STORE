using Microsoft.EntityFrameworkCore;
using PHONE_STORE.Application.Dtos;
using PHONE_STORE.Application.Interfaces;
using PHONE_STORE.Infrastructure.Data;

namespace PHONE_STORE.Infrastructure.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly PhoneDbContext _db;
    public InventoryRepository(PhoneDbContext db) => _db = db;

    public async Task<InventoryDto?> GetAsync(long variantId, long warehouseId, CancellationToken ct = default)
        => await _db.Inventories
            .Where(i => i.VariantId == variantId && i.WarehouseId == warehouseId)
            .Select(i => new InventoryDto(i.VariantId, i.WarehouseId, i.QtyOnHand, i.QtyReserved, i.UpdatedAt))
            .FirstOrDefaultAsync(ct);

    public async Task<List<InventoryDto>> GetByVariantAsync(long variantId, CancellationToken ct = default)
        => await _db.Inventories
            .Where(i => i.VariantId == variantId)
            .OrderBy(i => i.WarehouseId)
            .Select(i => new InventoryDto(i.VariantId, i.WarehouseId, i.QtyOnHand, i.QtyReserved, i.UpdatedAt))
            .ToListAsync(ct);

    public async Task<List<InventoryDto>> GetByWarehouseAsync(long warehouseId, CancellationToken ct = default)
        => await _db.Inventories
            .Where(i => i.WarehouseId == warehouseId)
            .OrderBy(i => i.VariantId)
            .Select(i => new InventoryDto(i.VariantId, i.WarehouseId, i.QtyOnHand, i.QtyReserved, i.UpdatedAt))
            .ToListAsync(ct);
}
