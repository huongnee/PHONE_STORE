using PHONE_STORE.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHONE_STORE.Application.Interfaces
{
    public interface IPriceRepository
    {
        Task<List<PriceDto>> ListByVariantAsync(long variantId, CancellationToken ct);
        Task<long> CreateAsync(long variantId, PriceUpsertDto dto, CancellationToken ct);
        Task<bool> UpdateAsync(long id, PriceUpsertDto dto, CancellationToken ct);
        Task<bool> DeleteAsync(long id, CancellationToken ct);
        Task<PriceDto?> GetActiveAsync(long variantId, DateTime? at, CancellationToken ct);
        
            // trả về giá đang hiệu lực cho 1 variant (nếu có)
            //Task<PriceDto?> GetActiveAsync(long variantId, CancellationToken ct);

            // tạo/cập nhật 1 khoảng giá (tuỳ bạn implement)
            Task<long> UpsertAsync(PriceUpsertDto dto, CancellationToken ct);

    }
}
