using Authentication.Helpers;
using Authentication.Models;
using Authentication.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(
        AppDbContext context,
        ILogger logger)
    {
        try
        {
            // ── Apply any pending migrations automatically ──────────────────
            await context.Database.MigrateAsync();

            await SeedAdminAsync(context, logger);
            await SeedSampleAgentAsync(context, logger);

            logger.LogInformation("Database seeding completed.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during database seeding.");
            throw;
        }
    }

    // ─── Seed default admin account ────────────────────────────────────────
    // Only created if no admin exists — safe to run on every startup
    private static async Task SeedAdminAsync(
        AppDbContext context,
        ILogger logger)
    {
        var adminExists = await context.Users
            .AnyAsync(u => u.Role == UserRole.Admin);

        if (adminExists)
        {
            logger.LogInformation("Admin account already exists. Skipping.");
            return;
        }

        var admin = new User
        {
            FullName = "Super Admin",
            Email = "admin@cropinsurance.com",
            PhoneNumber = "+911234567890",
            Role = UserRole.Admin,
            IsActive = true,
            EmailVerified = true,
            PasswordHash = PasswordHelper.HashPassword(
                                "Admin@CropInsurance2025!")
        };

        await context.Users.AddAsync(admin);
        await context.SaveChangesAsync();

        logger.LogInformation(
            "Admin account seeded: {Email}", admin.Email);
    }

    // ─── Seed sample verified agent ────────────────────────────────────────
    private static async Task SeedSampleAgentAsync(
        AppDbContext context,
        ILogger logger)
    {
        var agentExists = await context.Users
            .AnyAsync(u => u.Role == UserRole.InsuranceAgent);

        if (agentExists)
        {
            logger.LogInformation(
                "Agent account already exists. Skipping.");
            return;
        }

        var agent = new User
        {
            FullName = "Sample Agent",
            Email = "agent@cropinsurance.com",
            PhoneNumber = "+919876543210",
            Role = UserRole.InsuranceAgent,
            IsActive = true,
            EmailVerified = true,
            PasswordHash = PasswordHelper.HashPassword(
                                "Agent@CropInsurance2025!")
        };

        agent.AgentProfile = new AgentProfile
        {
            AgentCode = "AGT-001",
            LicenseNumber = "LIC-MH-2025-001",
            AssignedDistrict = "Aurangabad",
            IsVerified = true,
            UserId = agent.Id
        };

        await context.Users.AddAsync(agent);
        await context.SaveChangesAsync();

        logger.LogInformation(
            "Sample agent seeded: {Email}", agent.Email);
    }
}