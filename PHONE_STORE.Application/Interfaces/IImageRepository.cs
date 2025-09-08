using PHONE_STORE.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHONE_STORE.Application.Interfaces
{
    public interface IImageRepository
    {
        Task<List<ImageDto>> ListForProductAsync(long productId, CancellationToken ct);
        Task<List<ImageDto>> ListForVariantAsync(long variantId, CancellationToken ct);
        Task<long> CreateAsync(ImageCreateDto dto, CancellationToken ct);
        Task<bool> DeleteAsync(long id, CancellationToken ct);
    }
}
