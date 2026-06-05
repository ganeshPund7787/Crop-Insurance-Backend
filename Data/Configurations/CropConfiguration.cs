using Authentication.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Authentication.Data.Configurations;

public class CropConfiguration : IEntityTypeConfiguration<Crop>
{
    public void Configure(EntityTypeBuilder<Crop> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.CropName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Season)
            .HasMaxLength(20);

        builder.Property(x => x.ExpectedYieldTons)
            .HasColumnType("decimal(10,2)");

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20);
    }
}