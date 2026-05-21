using Authentication.DTOs.Agent;

namespace Authentication.Interfaces;

public interface IAgentService
{
    Task<AgentProfileDto> GetAgentProfileAsync(Guid userId);
    Task<AgentProfileDto> UpdateAgentProfileAsync(Guid userId, UpdateAgentProfileRequestDto request);
    Task<bool> VerifyAgentAsync(Guid agentId);
}