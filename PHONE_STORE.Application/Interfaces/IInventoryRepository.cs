using PHONE_STORE.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHONE_STORE.Application.Interfaces;

public interface IInventoryRepository
{
    Task<InventoryDto?> GetAsync(long variantId, long warehouseId, CancellationToken ct = default);
    Task<List<InventoryDto>> GetByVariantAsync(long variantId, CancellationToken ct = default);
    Task<List<InventoryDto>> GetByWarehouseAsync(long warehouseId, CancellationToken ct = default);
}