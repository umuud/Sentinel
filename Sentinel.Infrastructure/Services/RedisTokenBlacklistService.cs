using StackExchange.Redis;
using Sentinel.Application.Interfaces;

namespace Sentinel.Infrastructure.Services;

public class RedisTokenBlacklistService : ITokenBlacklistService
{
    private readonly IDatabase _db;
    private const string Prefix = "blacklist:";

    public RedisTokenBlacklistService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task AddToBlacklistAsync(string jti, TimeSpan expiry)
    {
        await _db.StringSetAsync(Prefix + jti, "revoked", expiry);
    }

    public async Task<bool> IsBlacklistedAsync(string jti)
    {
        return await _db.KeyExistsAsync(Prefix + jti);
    }
}