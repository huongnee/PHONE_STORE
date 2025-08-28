using StackExchange.Redis;

namespace PHONE_STORE.API.Auth;

/// Chặn brute-force: nếu sai >= MaxFails trong WindowMinutes → khoá tạm.
/// Dùng Redis INCR + EXPIRE nên nguyên tử và nhanh.
public class LoginLockService
{
    private readonly IDatabase _db;
    private readonly TimeSpan _window;
    private readonly int _maxFails;

    private static string FailKey(string u) => $"ps:login:fail:{u.ToLowerInvariant()}";
    private static string LockKey(string u) => $"ps:login:lock:{u.ToLowerInvariant()}";

    public LoginLockService(IConnectionMultiplexer mux, IConfiguration cfg)
    {
        _db = mux.GetDatabase();
        _window = TimeSpan.FromMinutes(cfg.GetValue("LoginLock:WindowMinutes", 10));
        _maxFails = cfg.GetValue("LoginLock:MaxFails", 5);
    }

    public Task<bool> IsLockedAsync(string userName, CancellationToken _ = default)
        => _db.KeyExistsAsync(LockKey(userName));

    public async Task IncreaseFailAsync(string userName, CancellationToken _ = default)
    {
        var fKey = FailKey(userName);
        var count = await _db.StringIncrementAsync(fKey); // ++ nguyên tử
        if (count == 1) await _db.KeyExpireAsync(fKey, _window); // set TTL lần đầu

        if (count >= _maxFails)
        {
            // đặt cờ khoá, TTL bằng cửa sổ
            await _db.StringSetAsync(LockKey(userName), "1", _window);
        }
    }

    public Task ClearAsync(string userName, CancellationToken _ = default)
        => _db.KeyDeleteAsync(new RedisKey[] { FailKey(userName), LockKey(userName) });
}
