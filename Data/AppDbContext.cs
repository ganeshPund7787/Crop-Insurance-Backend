using Authentication.Models;
using Authentication.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {}

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<FarmerProfile> FarmerProfiles => Set<FarmerProfile>();
    public DbSet<AgentProfile> AgentProfiles => Set<AgentProfile>();
    public DbSet<Farm> Farms => Set<Farm>();
    public DbSet<Crop> Crops => Set<Crop>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Auto-apply all IEntityTypeConfiguration<T> in this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AppDbContext).Assembly);

        // ─── Global Soft Delete Filter ────────────────────────────────────
        // Every query automatically excludes IsDeleted = true records
        // No need to add .Where(x => !x.IsDeleted) anywhere in the app
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

    // ─── Auto-set UpdatedAtUtc on SaveChanges ─────────────────────────────────
    // Senior tip: never manually set UpdatedAtUtc in services
    // DbContext intercepts every save and stamps it automatically
    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAtUtc = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}