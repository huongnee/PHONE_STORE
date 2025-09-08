using PHONE_STORE.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHONE_STORE.Application.Interfaces
{
    public interface IVariantRepository
    {
        Task<List<VariantDto>> ListByProductAsync(long productId, CancellationToken ct);
        Task<VariantDto?> GetAsync(long id, CancellationToken ct);
        Task<long> CreateAsync(VariantCreateDto dto, CancellationToken ct);
        Task<bool> UpdateAsync(long id, VariantUpdateDto dto, CancellationToken ct);
        Task<bool> DeleteAsync(long id, CancellationToken ct);
    }
}
