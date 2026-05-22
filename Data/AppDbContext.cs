using Authentication.Models;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<FarmerProfile> FarmerProfiles => Set<FarmerProfile>();
    public DbSet<AgentProfile> AgentProfiles => Set<AgentProfile>();
    public DbSet<Farm> Farms => Set<Farm>();
    public DbSet<Crop> Crops => Set<Crop>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly);

        // ── Global soft delete filters ─────────────────────────────────────
        modelBuilder.Entity<User>()
            .HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<RefreshToken>()
            .HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<FarmerProfile>()
            .HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<AgentProfile>()
            .HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Farm>()
            .HasQueryFilter(x => !x.IsDeleted);
        modelBuilder.Entity<Crop>()
            .HasQueryFilter(x => !x.IsDeleted);
    }

    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                // ✅ Only set UpdatedAtUtc on Modified — never on Added
                case EntityState.Modified:
                    entry.Entity.UpdatedAtUtc = DateTime.UtcNow;
                    break;

                // ✅ Ensure CreatedAtUtc is set on new entities
                case EntityState.Added:
                    if (entry.Entity.CreatedAtUtc == default)
                        entry.Entity.CreatedAtUtc = DateTime.UtcNow;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}