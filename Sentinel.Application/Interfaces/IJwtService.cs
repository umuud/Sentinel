namespace Sentinel.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(Guid accountId, string email);
}