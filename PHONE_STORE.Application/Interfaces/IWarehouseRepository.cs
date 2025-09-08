using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PHONE_STORE.Application.Dtos;
//using PHONE_STORE.Infrastructure.Entities;

namespace PHONE_STORE.Application.Interfaces;

public interface IWarehouseRepository
{
    Task<List<WarehouseDto>> GetAllAsync(CancellationToken ct = default);
    Task<WarehouseDto?> GetAsync(long id, CancellationToken ct = default);
    Task<long> CreateAsync(WarehouseUpsertDto dto, CancellationToken ct = default);
    Task<bool> UpdateAsync(long id, WarehouseUpsertDto dto, CancellationToken ct = default);
    Task<List<WarehouseOptionDto>> GetOptionsAsync(CancellationToken ct = default);
}