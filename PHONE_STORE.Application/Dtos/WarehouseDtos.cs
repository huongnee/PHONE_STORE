using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHONE_STORE.Application.Dtos;

public record WarehouseDto(long Id, string Code, string Name, string? AddressLine, string? District, string? Province, bool IsActive);
public record WarehouseUpsertDto(string Code, string Name, string? AddressLine, string? District, string? Province, bool IsActive);
public record WarehouseOptionDto(long Id, string Label);

