using Authentication.Models;
using Authentication.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Authentication.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FullName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(150);

        // ─── Unique index on Email ─────────────────────────────────────────
        // Prevents duplicate accounts at DB level, not just app level
        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.Property(x => x.PasswordHash)
            .IsRequired();

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(15);

        // ─── Store enum as string ──────────────────────────────────────────
        // "Farmer" in DB instead of 0 — readable, migration-safe
        builder.Property(x => x.Role)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.Property(x => x.EmailVerified)
            .HasDefaultValue(false);

        // ─── Relationships ─────────────────────────────────────────────────
        builder.HasMany(x => x.RefreshTokens)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.FarmerProfile)
            .WithOne(x => x.User)
            .HasForeignKey<FarmerProfile>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.AgentProfile)
            .WithOne(x => x.User)
            .HasForeignKey<AgentProfile>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}