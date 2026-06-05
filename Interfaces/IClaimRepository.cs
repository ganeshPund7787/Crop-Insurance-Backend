using Authentication.Models;
using Authentication.Models.Enums;

namespace Authentication.Interfaces;

public interface IClaimRepository : IRepository<InsuranceClaim>
{
    Task<InsuranceClaim?> GetByIdWithDetailsAsync(Guid claimId);
    Task<IEnumerable<InsuranceClaim>> GetByDistrictAsync(string district);
    Task<IEnumerable<InsuranceClaim>> GetByAgentIdAsync(Guid agentProfileId);
    Task<IEnumerable<InsuranceClaim>> GetByFarmerIdAsync(Guid farmerProfileId);
    Task<string> GenerateClaimNumberAsync();
}