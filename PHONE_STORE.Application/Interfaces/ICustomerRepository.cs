using PHONE_STORE.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHONE_STORE.Application.Interfaces;

public interface ICustomerRepository
{
    Task<List<CustomerDto>> SearchAsync(string? q, int top = 100, CancellationToken ct = default);
    Task<CustomerDto?> GetAsync(long id, CancellationToken ct = default);
    Task<long> CreateAsync(CustomerUpsertDto dto, CancellationToken ct = default);
    Task<bool> UpdateAsync(long id, CustomerUpsertDto dto, CancellationToken ct = default);
}