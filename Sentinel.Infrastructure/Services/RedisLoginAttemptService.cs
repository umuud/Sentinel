using StackExchange.Redis;
using Sentinel.Application.Interfaces;

namespace Sentinel.Infrastructure.Services;

public class RedisLoginAttemptService : ILoginAttemptService
{
    private readonly IDatabase _db;
    private const string Prefix = "login_attempt:";
    private const int MaxAttempts = 5;
    private static readonly TimeSpan BlockDuration = TimeSpan.FromMinutes(15);

    public RedisLoginAttemptService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task RecordFailedAttemptAsync(string email)
    {
        var key = Prefix + email;
        var attempts = await _db.StringIncrementAsync(key);

        if (attempts == 1)
            await _db.KeyExpireAsync(key, BlockDuration);
    }

    public async Task<bool> IsBlockedAsync(string email)
    {
        var key = Prefix + email;
        var attempts = await _db.StringGetAsync(key);

        if (!attempts.HasValue)
            return false;

        return (int)attempts >= MaxAttempts;
    }

    public async Task ResetAttemptsAsync(string email)
    {
        await _db.KeyDeleteAsync(Prefix + email);
    }
}