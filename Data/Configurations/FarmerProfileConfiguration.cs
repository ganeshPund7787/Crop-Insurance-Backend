using Authentication.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Authentication.Data.Configurations;

public class FarmerProfileConfiguration : IEntityTypeConfiguration<FarmerProfile>
{
    public void Configure(EntityTypeBuilder<FarmerProfile> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Village)
            .HasMaxLength(100);

        builder.Property(x => x.District)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.State)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.AadhaarNumber)
            .HasMaxLength(12);

        // ─── Unique Aadhaar ────────────────────────────────────────────────
        // One Aadhaar = one farmer account, enforced at DB level
        builder.HasIndex(x => x.AadhaarNumber)
            .IsUnique();

        builder.Property(x => x.TotalLandAcres)
            .HasColumnType("decimal(10,2)");

        builder.HasMany(x => x.Farms)
            .WithOne(x => x.FarmerProfile)
            .HasForeignKey(x => x.FarmerProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}