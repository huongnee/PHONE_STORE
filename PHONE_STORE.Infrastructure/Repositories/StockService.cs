using Microsoft.EntityFrameworkCore;
using PHONE_STORE.Application.Dtos;
using PHONE_STORE.Application.Interfaces;
using PHONE_STORE.Infrastructure.Data;
using PHONE_STORE.Infrastructure.Entities;

namespace PHONE_STORE.Infrastructure.Services;

public class StockService : IStockService
{
    private readonly PhoneDbContext _db;
    public StockService(PhoneDbContext db) => _db = db;

    public async Task<(bool ok, string? error)> MoveAsync(StockMoveRequestDto req, long? createdBy, CancellationToken ct = default)
    {
        var type = (req.MovementType ?? "").Trim().ToUpperInvariant();
        if (type is not ("IN" or "OUT" or "ADJUST"))
            return (false, "movement_type phải là IN/OUT/ADJUST");
        if (req.Quantity <= 0 && type != "ADJUST")
            return (false, "quantity phải > 0");

        // 9.1 gọn delta
        var delta = type switch
        {
            "IN" => req.Quantity,
            "OUT" => -req.Quantity,
            _ => req.Quantity // ADJUST: truyền thẳng, cho phép âm/dương
        };

        using var tx = await _db.Database.BeginTransactionAsync(ct);

        // Lấy (hoặc tạo) inventory
        var inv = await _db.Inventories
            .FirstOrDefaultAsync(i => i.VariantId == req.VariantId && i.WarehouseId == req.WarehouseId, ct);

        if (inv == null)
        {
            inv = new Inventory
            {
                VariantId = req.VariantId,
                WarehouseId = req.WarehouseId,
                QtyOnHand = 0,
                QtyReserved = 0,
                UpdatedAt = DateTime.UtcNow
            };
            _db.Inventories.Add(inv);
            await _db.SaveChangesAsync(ct);
        }

        // Check tồn
        var newOnHand = inv.QtyOnHand + delta;
        if (newOnHand < 0 || newOnHand < inv.QtyReserved)
            return (false, "Không đủ tồn (vướng QtyReserved).");

        inv.QtyOnHand = newOnHand;
        inv.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        // Ghi nhật ký
        var sm = new StockMovement
        {
            VariantId = req.VariantId,
            WarehouseId = req.WarehouseId,
            MovementType = type,
            QtyDelta = delta,
            RefType = req.RefType,
            RefId = req.RefId,
            RefCode = req.RefCode,
            Note = req.Note,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
        _db.StockMovements.Add(sm);
        await _db.SaveChangesAsync(ct);

        // (Tùy chọn) xử lý IMEI
        if (req.ImeiList is { Count: > 0 })
        {
            if (type == "IN")
            {
                foreach (var imei in req.ImeiList.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    _db.DeviceUnits.Add(new DeviceUnit
                    {
                        VariantId = req.VariantId,
                        Imei = imei.Trim(),
                        SerialNo = null,
                        Status = "IN_STOCK",
                        WarehouseId = req.WarehouseId,
                        ReceivedAt = DateTime.UtcNow
                    });
                }
                await _db.SaveChangesAsync(ct);
            }
            else if (type == "OUT")
            {
                // 9.2 Làm sạch & distinct IMEI
                var imeis = req.ImeiList?
                    .Select(x => x.Trim())
                    .Where(x => x.Length > 0)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList() ?? new();

                var units = await _db.DeviceUnits
                    .Where(u => u.VariantId == req.VariantId
                                && u.WarehouseId == req.WarehouseId
                                && u.Status == "IN_STOCK"
                                && imeis.Contains(u.Imei))
                    .ToListAsync(ct);

                if (units.Count != imeis.Count)
                    return (false, "Danh sách IMEI không hợp lệ hoặc không đủ trong kho.");

                foreach (var u in units)
                {
                    u.Status = "SOLD";
                    u.SoldAt = DateTime.UtcNow;
                    u.WarehouseId = null; // ra khỏi kho
                }
                await _db.SaveChangesAsync(ct);
            }
        }

        await tx.CommitAsync(ct);
        return (true, null);
    }

}