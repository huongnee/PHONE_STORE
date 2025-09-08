using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHONE_STORE.Application.Dtos;

public record InventoryDto(long VariantId, long WarehouseId, int QtyOnHand, int QtyReserved, DateTime UpdatedAt);
public record StockMoveRequestDto(long VariantId, long WarehouseId, string MovementType, int Quantity,
                                  string? RefType, long? RefId, string? RefCode, string? Note,
                                  List<string>? ImeiList);
