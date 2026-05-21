using Authentication.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Authentication.Data.Configurations;

public class AgentProfileConfiguration : IEntityTypeConfiguration<AgentProfile>
{
    public void Configure(EntityTypeBuilder<AgentProfile> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AgentCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(x => x.AgentCode)
            .IsUnique();

        builder.Property(x => x.LicenseNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.LicenseNumber)
            .IsUnique();

        builder.Property(x => x.AssignedDistrict)
            .HasMaxLength(100);
    }
}