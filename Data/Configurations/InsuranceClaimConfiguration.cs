using Authentication.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Authentication.Data.Configurations;

public class InsuranceClaimConfiguration : IEntityTypeConfiguration<InsuranceClaim>
{
    public void Configure(EntityTypeBuilder<InsuranceClaim> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ClaimNumber)
            .IsRequired()
            .HasMaxLength(30);

        builder.HasIndex(x => x.ClaimNumber)
            .IsUnique();

        builder.Property(x => x.DamageType)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(x => x.DamageDescription)
            .HasMaxLength(1000);

        builder.Property(x => x.EstimatedLossAmount)
            .HasColumnType("decimal(12,2)");

        builder.Property(x => x.ApprovedAmount)
            .HasColumnType("decimal(12,2)");

        builder.Property(x => x.RejectionReason)
            .HasMaxLength(500);

        builder.Property(x => x.AgentRemarks)
            .HasMaxLength(500);

        // ─── Relationships ─────────────────────────────────────────────────
        builder.HasOne(x => x.Crop)
            .WithMany(x => x.Claims)
            .HasForeignKey(x => x.CropId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.FarmerProfile)
            .WithMany(x => x.Claims)
            .HasForeignKey(x => x.FarmerProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AgentProfile)
            .WithMany(x => x.Claims)
            .HasForeignKey(x => x.AgentProfileId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.HasMany(x => x.Inspections)
            .WithOne(x => x.Claim)
            .HasForeignKey(x => x.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);

        // Global soft delete filter
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}