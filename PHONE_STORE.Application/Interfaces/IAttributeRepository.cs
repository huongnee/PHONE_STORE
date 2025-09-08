using PHONE_STORE.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHONE_STORE.Application.Interfaces
{
    public interface IAttributeRepository
    {
        Task<List<AttributeDto>> ListAsync(CancellationToken ct);
        Task<long> CreateAsync(AttributeCreateDto dto, CancellationToken ct);
        Task<bool> UpdateAsync(long id, AttributeUpdateDto dto, CancellationToken ct);
        Task<bool> DeleteAsync(long id, CancellationToken ct);

        Task UpsertValueAsync(long productId, AttributeValueUpsertDto dto, CancellationToken ct);
        Task<List<(AttributeDto Attr, object? Value)>> ListValuesAsync(long productId, CancellationToken ct);
    }
}
