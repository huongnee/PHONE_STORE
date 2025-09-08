using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHONE_STORE.Application.Dtos;

public record DeviceUnitDto(long Id, long VariantId, string Imei, string? SerialNo, string Status, long? WarehouseId,
                            DateTime ReceivedAt, DateTime? SoldAt, DateTime? ReturnedAt);
public record DeviceUnitCreateDto(long VariantId, long WarehouseId, List<(string Imei, string? SerialNo)> Items);