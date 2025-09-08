using PHONE_STORE.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHONE_STORE.Application.Interfaces
{
    public interface IProductRepository
    {
        Task<PagedResult<ProductListItemDto>> SearchAsync(
            string? q,
            long? brandId,
            int page,
            int pageSize,
            CancellationToken ct);

        Task<ProductDto?> GetAsync(long id, CancellationToken ct);

        Task<long> CreateAsync(ProductCreateDto dto, CancellationToken ct);

        Task<bool> UpdateAsync(long id, ProductUpdateDto dto, CancellationToken ct);

        Task<bool> DeleteAsync(long id, CancellationToken ct);

        Task<bool> SlugExistsAsync(string slug, long? exceptId, CancellationToken ct);
    }
}
