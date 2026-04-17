namespace Sentinel.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(Guid accountId, string email, string username);
    string? GetJtiFromToken(string token);
    DateTime? GetExpiryFromToken(string token);
}