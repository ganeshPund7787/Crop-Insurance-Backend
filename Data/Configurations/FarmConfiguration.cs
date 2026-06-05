using Authentication.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Authentication.Data.Configurations;

public class FarmConfiguration : IEntityTypeConfiguration<Farm>
{
    public void Configure(EntityTypeBuilder<Farm> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FarmName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.AreaInAcres)
            .HasColumnType("decimal(10,2)");

        builder.Property(x => x.SoilType)
            .HasMaxLength(50);

        builder.Property(x => x.Location)
            .HasMaxLength(200);

        builder.HasMany(x => x.Crops)
            .WithOne(x => x.Farm)
            .HasForeignKey(x => x.FarmId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}