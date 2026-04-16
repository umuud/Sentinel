using Microsoft.EntityFrameworkCore;
using Sentinel.Application.Interfaces;
using Sentinel.Domain.Common;
using Sentinel.Domain.Entities;

namespace Sentinel.Persistence.Context;

public class SentinelDbContext : DbContext
{
    private readonly ICurrentUserService _currentUserService;
    public SentinelDbContext(DbContextOptions<SentinelDbContext> options, ICurrentUserService currentUserService):base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SentinelDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<AuditableEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedDate = DateTime.UtcNow;
                entry.Entity.CreatedBy = _currentUserService.UserId.ToString() ?? "system";
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.LastModifiedDate = DateTime.UtcNow;
                entry.Entity.LastModifiedBy = _currentUserService.UserId.ToString() ?? "system";
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}

