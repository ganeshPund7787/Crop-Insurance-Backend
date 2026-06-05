using Authentication.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Authentication.Data.Configurations;

public class InspectionConfiguration : IEntityTypeConfiguration<Inspection>
{
    public void Configure(EntityTypeBuilder<Inspection> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.InspectionNumber)
            .IsRequired()
            .HasMaxLength(30);

        builder.HasIndex(x => x.InspectionNumber)
            .IsUnique();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.Location)
            .HasMaxLength(200);

        builder.Property(x => x.Findings)
            .HasMaxLength(2000);

        builder.Property(x => x.InspectorNotes)
            .HasMaxLength(1000);

        builder.Property(x => x.DamagePercentage)
            .HasColumnType("decimal(5,2)");

        builder.Property(x => x.RecommendedAmount)
            .HasColumnType("decimal(12,2)");

        builder.HasOne(x => x.AgentProfile)
            .WithMany(x => x.Inspections)
            .HasForeignKey(x => x.AgentProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}