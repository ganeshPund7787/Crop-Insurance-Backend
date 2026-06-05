using Authentication.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Authentication.Data.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Token)
            .IsRequired()
            .HasMaxLength(256);

        // ─── Index for fast token lookup ───────────────────────────────────
        // Login/refresh hits this index on every request — must be fast
        builder.HasIndex(x => x.Token)
            .IsUnique();

        builder.Property(x => x.ExpiresAtUtc)
            .IsRequired();

        builder.Property(x => x.IsRevoked)
            .HasDefaultValue(false);

        builder.Property(x => x.ReplacedByToken)
            .HasMaxLength(256);

        builder.Property(x => x.ReasonRevoked)
            .HasMaxLength(200);

        // Ignore computed properties — not mapped to DB columns
        builder.Ignore(x => x.IsExpired);
        builder.Ignore(x => x.IsActive);
    }
}