using System.Data.Common;
using Sentinel.Domain.Common;

namespace Sentinel.Domain.Entities;
public class Account : AuditableEntity
{
    public string Username { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public bool IsActive {get; private set;}
    private Account(){}

    public Account(string username, string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username boş olamaz");

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email boş olamaz");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password boş olamaz");

        Id = Guid.NewGuid();
        Username=username;
        Email=email;
        PasswordHash=passwordHash;
        IsActive=true;

        CreatedDate=DateTime.UtcNow;
        CreatedBy="system";
    }

    public void ChangeEmail(string newEmail)
    {
        if (string.IsNullOrWhiteSpace(newEmail))
            throw new ArgumentException("Email boş olamaz");

        Email = newEmail;
        LastModifiedDate = DateTime.UtcNow;
    }

    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("Password boş olamaz");

        PasswordHash = newPasswordHash;
        LastModifiedDate = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        LastModifiedDate = DateTime.UtcNow;
    }

}