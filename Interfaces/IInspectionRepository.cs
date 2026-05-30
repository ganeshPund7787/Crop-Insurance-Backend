using Authentication.Models;

namespace Authentication.Interfaces;

public interface IInspectionRepository : IRepository<Inspection>
{
    Task<IEnumerable<Inspection>> GetByAgentIdAsync(Guid agentProfileId);
    Task<IEnumerable<Inspection>> GetByClaimIdAsync(Guid claimId);
    Task<string> GenerateInspectionNumberAsync();
}