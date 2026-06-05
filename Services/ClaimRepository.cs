using Authentication.Data;
using Authentication.Interfaces;
using Authentication.Models;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Services;

public class ClaimRepository : BaseRepository<InsuranceClaim>, IClaimRepository
{
    public ClaimRepository(AppDbContext context) : base(context) { }

    public async Task<InsuranceClaim?> GetByIdWithDetailsAsync(Guid claimId)
    {
        return await _context.Claims
            .Include(x => x.FarmerProfile)
                .ThenInclude(fp => fp.User)
            .Include(x => x.Crop)
                .ThenInclude(c => c.Farm)
            .Include(x => x.AgentProfile)
                .ThenInclude(ap => ap.User)
            .Include(x => x.Inspections)
            .FirstOrDefaultAsync(x =>
                x.Id == claimId && !x.IsDeleted);
    }

    public async Task<IEnumerable<InsuranceClaim>> GetByDistrictAsync(string district)
    {
        return await _context.Claims
            .Include(x => x.FarmerProfile)
                .ThenInclude(fp => fp.User)
            .Include(x => x.Crop)
            .Where(x =>
                x.FarmerProfile.District == district &&
                !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();
    }

    public async Task<IEnumerable<InsuranceClaim>> GetByAgentIdAsync(
        Guid agentProfileId)
    {
        return await _context.Claims
            .Include(x => x.FarmerProfile)
                .ThenInclude(fp => fp.User)
            .Include(x => x.Crop)
            .Where(x =>
                x.AgentProfileId == agentProfileId &&
                !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();
    }

    public async Task<IEnumerable<InsuranceClaim>> GetByFarmerIdAsync(
        Guid farmerProfileId)
    {
        return await _context.Claims
            .Include(x => x.Crop)
            .Include(x => x.Inspections)
            .Where(x =>
                x.FarmerProfileId == farmerProfileId &&
                !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();
    }

    // ─── Auto-generate claim number CLM-20250001 ───────────────────────────
    public async Task<string> GenerateClaimNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var count = await _context.Claims
            .CountAsync(x => x.CreatedAtUtc.Year == year);

        return $"CLM-{year}{(count + 1):D4}";
    }
}