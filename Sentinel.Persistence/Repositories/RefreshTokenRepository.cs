using Microsoft.EntityFrameworkCore;
using Sentinel.Application.Interfaces;
using Sentinel.Domain.Entities;
using Sentinel.Persistence.Context;

namespace Sentinel.Persistence.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly SentinelDbContext _context;

    public RefreshTokenRepository(SentinelDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
    {
        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == token, cancellationToken);
    }

    public async Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
    {
        _context.RefreshTokens.Update(refreshToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}