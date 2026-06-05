using Authentication.Data;
using Authentication.Interfaces;
using Authentication.Models;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Services;

public class InspectionRepository
    : BaseRepository<Inspection>, IInspectionRepository
{
    public InspectionRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Inspection>> GetByAgentIdAsync(
        Guid agentProfileId)
    {
        return await _context.Inspections
            .Include(x => x.Claim)
                .ThenInclude(c => c.FarmerProfile)
                    .ThenInclude(fp => fp.User)
            .Where(x =>
                x.AgentProfileId == agentProfileId &&
                !x.IsDeleted)
            .OrderByDescending(x => x.ScheduledAtUtc)
            .ToListAsync();
    }

    public async Task<IEnumerable<Inspection>> GetByClaimIdAsync(Guid claimId)
    {
        return await _context.Inspections
            .Where(x =>
                x.ClaimId == claimId &&
                !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();
    }

    // ─── Auto-generate inspection number INS-20250001 ──────────────────────
    public async Task<string> GenerateInspectionNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var count = await _context.Inspections
            .CountAsync(x => x.CreatedAtUtc.Year == year);

        return $"INS-{year}{(count + 1):D4}";
    }
}