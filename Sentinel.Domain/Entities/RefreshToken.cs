using Sentinel.Domain.Common;

namespace Sentinel.Domain.Entities;

public class RefreshToken : AuditableEntity
{
    public Guid AccountId { get; private set; }
    public string Token { get; private set; }

    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }

    // Navigation
    public Account Account { get; private set; }

    private RefreshToken() { }

    public RefreshToken(Guid accountId, string token, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token boş olamaz");

        Id = Guid.NewGuid();
        AccountId = accountId;
        Token = token;
        ExpiresAt = expiresAt;
        IsRevoked = false;
    }

    public void Revoke()
    {
        IsRevoked = true;
    }

    public bool IsExpired()
    {
        return DateTime.UtcNow >= ExpiresAt;
    }
}