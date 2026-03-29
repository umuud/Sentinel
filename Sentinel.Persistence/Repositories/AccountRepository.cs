using Microsoft.EntityFrameworkCore;
using Sentinel.Application.Interfaces;
using Sentinel.Domain.Entities;
using Sentinel.Persistence.Context;

namespace Sentinel.Persistence.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly SentinelDbContext _context;

    public AccountRepository(SentinelDbContext context)
    {
        _context = context;
    }

    public async Task<Account?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
    }

    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken)
    {
        return await _context.Accounts
            .AnyAsync(x => x.Email == email, cancellationToken);
    }

    public async Task AddAsync(Account account, CancellationToken cancellationToken)
    {
        await _context.Accounts.AddAsync(account, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}