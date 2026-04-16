using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sentinel.Domain.Entities;

namespace Sentinel.Persistence.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts");

        // Primary Key
        builder.HasKey(x => x.Id);

        // Username
        builder.Property(x => x.Username)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(x => x.Username)
            .IsUnique();
            
        // Email
        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(x => x.Email)
            .IsUnique();

        // PasswordHash
        builder.Property(x => x.PasswordHash)
            .IsRequired();

        // IsActive
        builder.Property(x => x.IsActive)
            .IsRequired();

        // Auditable fields
        builder.Property(x => x.CreatedDate)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasMaxLength(100);

        builder.Property(x => x.LastModifiedDate);

        builder.Property(x => x.LastModifiedBy)
            .HasMaxLength(100);
    }
}