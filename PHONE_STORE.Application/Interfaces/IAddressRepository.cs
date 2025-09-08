using PHONE_STORE.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHONE_STORE.Application.Interfaces;

public interface IAddressRepository
{
    Task<List<AddressDto>> GetByCustomerAsync(long customerId, CancellationToken ct = default);
    Task<AddressDto?> GetAsync(long id, CancellationToken ct = default);
    Task<long> CreateAsync(long customerId, AddressUpsertDto dto, CancellationToken ct = default);
    Task<bool> UpdateAsync(long id, AddressUpsertDto dto, CancellationToken ct = default);
    Task<bool> DeleteAsync(long id, CancellationToken ct = default);
    Task<bool> SetDefaultAsync(long id, string addressType, CancellationToken ct = default);
}