using Sentinel.Domain.Entities;

namespace Sentinel.Application.Interfaces;

public interface IAccountRepository
{
    Task<Account?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);

    Task AddAsync(Account account, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}