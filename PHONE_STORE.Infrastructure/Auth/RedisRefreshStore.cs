using Microsoft.Extensions.Caching.Distributed;
using PHONE_STORE.Application.Interfaces;
using System.Text.Json;

namespace PHONE_STORE.Infrastructure.Auth;

public class RedisRefreshStore : IRefreshStore
{
    private readonly IDistributedCache _cache;
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public RedisRefreshStore(IDistributedCache cache) => _cache = cache;

    public Task SaveAsync(RefreshSession s, CancellationToken ct = default) =>
        _cache.SetStringAsync($"rt:{s.Jti}", JsonSerializer.Serialize(s, JsonOpts),
            new DistributedCacheEntryOptions { AbsoluteExpiration = s.ExpiresAt }, ct);

    public async Task<RefreshSession?> GetAsync(string jti, CancellationToken ct = default)
    {
        var json = await _cache.GetStringAsync($"rt:{jti}", ct);
        return json is null ? null : JsonSerializer.Deserialize<RefreshSession>(json, JsonOpts);
    }

    public Task RevokeAsync(string jti, CancellationToken ct = default) =>
        _cache.RemoveAsync($"rt:{jti}", ct);
}
