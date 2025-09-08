using PHONE_STORE.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHONE_STORE.Application.Interfaces;

public interface ICartRepository
{
    Task<CartDto> GetOrCreateAsync(long? customerId, string? sessionId, CancellationToken ct);
    Task<CartDto> AddOrUpdateItemAsync(long? customerId, string? sessionId, CartItemUpsertDto dto, CancellationToken ct);
    Task<CartDto> UpdateQtyAsync(long itemId, int qty, long? customerId, string? sessionId, CancellationToken ct);
    Task RemoveItemAsync(long itemId, long? customerId, string? sessionId, CancellationToken ct);
    Task MergeAsync(string sessionId, long customerId, CancellationToken ct); // gọi sau khi login
    Task ClearAsync(long? customerId, string? sessionId, CancellationToken ct);
}