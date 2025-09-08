using System.Threading;
using System.Threading.Tasks;
using PHONE_STORE.Application.Dtos;

namespace PHONE_STORE.Application.Interfaces
{
    public interface IOrderService
    {
        Task<CheckoutPreviewResult> PreviewAsync(long userId, long addressId, string? sessionId = null, CancellationToken ct = default);
        Task<long?> SubmitAsync(long userId, long addressId, string? note, string? sessionId = null, CancellationToken ct = default);
    }
}
