using Microsoft.Extensions.Caching.Distributed;
using PHONE_STORE.Application.Interfaces;

namespace PHONE_STORE.Infrastructure.Auth
{
    public class RedisOtpStore : IOtpStore
    {
        private readonly IDistributedCache _cache;
        public RedisOtpStore(IDistributedCache cache) => _cache = cache;

        public Task SaveAsync(string key, string code, TimeSpan ttl, CancellationToken ct = default)
            => _cache.SetStringAsync($"otp:{key}", code, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            }, ct);

        public Task<string?> GetAsync(string key, CancellationToken ct = default)
            => _cache.GetStringAsync($"otp:{key}", ct);

        public Task RemoveAsync(string key, CancellationToken ct = default)
            => _cache.RemoveAsync($"otp:{key}", ct);
    }
}
