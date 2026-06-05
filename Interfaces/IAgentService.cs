using Authentication.DTOs.Agent;

namespace Authentication.Interfaces;

public interface IAgentService
{
    // Profile
    Task<AgentProfileDto> GetAgentProfileAsync(Guid userId);
    Task<AgentProfileDto> UpdateAgentProfileAsync(
        Guid userId, UpdateAgentProfileRequestDto request);
    Task<bool> VerifyAgentAsync(Guid agentId);

    // Claims
    Task<IEnumerable<ClaimSummaryDto>> GetDistrictClaimsAsync(Guid userId);
    Task<ClaimDetailDto> GetClaimDetailAsync(Guid userId, Guid claimId);
    Task<ClaimDetailDto> AssignClaimAsync(Guid userId, Guid claimId);
    Task<ClaimDetailDto> ApproveClaimAsync(
        Guid userId, Guid claimId, ApproveClaimRequestDto request);
    Task<ClaimDetailDto> RejectClaimAsync(
        Guid userId, Guid claimId, RejectClaimRequestDto request);

    // Inspections
    Task<InspectionDto> CreateInspectionAsync(
        Guid userId, Guid claimId, CreateInspectionRequestDto request);
    Task<InspectionDto> UpdateInspectionAsync(
        Guid userId, Guid inspectionId, UpdateInspectionRequestDto request);
    Task<IEnumerable<InspectionDto>> GetMyInspectionsAsync(Guid userId);

    // Farmers
    Task<IEnumerable<DistrictFarmerDto>> GetDistrictFarmersAsync(Guid userId);
}