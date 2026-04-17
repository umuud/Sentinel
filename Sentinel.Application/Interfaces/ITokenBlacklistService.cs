namespace Sentinel.Application.Interfaces;

public interface ITokenBlacklistService
{
    Task AddToBlacklistAsync(string jti, TimeSpan expiry);
    Task<bool> IsBlacklistedAsync(string jti);
}