using Authentication.Data;
using Authentication.DTOs.Agent;
using Authentication.Interfaces;
using Authentication.Models;
using Authentication.Models.Enums;
using AutoMapper;

namespace Authentication.Services;

public class AgentService : IAgentService
{
    private readonly IUserRepository _userRepository;
    private readonly IRepository<AgentProfile> _agentRepository;
    private readonly IMapper _mapper;

    public AgentService(
        IUserRepository userRepository,
        IRepository<AgentProfile> agentRepository,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _agentRepository = agentRepository;
        _mapper = mapper;
    }

    // ─── Get agent profile ─────────────────────────────────────────────────
    public async Task<AgentProfileDto> GetAgentProfileAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdWithProfileAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        if (user.Role != UserRole.InsuranceAgent)
            throw new InvalidOperationException(
                "User is not an insurance agent.");

        if (user.AgentProfile is null)
            throw new KeyNotFoundException("Agent profile not found.");

        return _mapper.Map<AgentProfileDto>(user.AgentProfile);
    }

    // ─── Update agent profile ──────────────────────────────────────────────
    public async Task<AgentProfileDto> UpdateAgentProfileAsync(
        Guid userId,
        UpdateAgentProfileRequestDto request)
    {
        var user = await _userRepository.GetByIdWithProfileAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        if (user.AgentProfile is null)
            throw new KeyNotFoundException("Agent profile not found.");

        var profile = user.AgentProfile;

        profile.AssignedDistrict = request.AssignedDistrict.Trim();
        profile.LicenseNumber = request.LicenseNumber.Trim();

        _agentRepository.Update(profile);
        await _agentRepository.SaveChangesAsync();

        return _mapper.Map<AgentProfileDto>(profile);
    }

    // ─── Verify agent ──────────────────────────────────────────────────────
    // Admin action — marks agent as verified to handle claims
    public async Task<bool> VerifyAgentAsync(Guid agentId)
    {
        var profile = await _agentRepository.GetByIdAsync(agentId)
            ?? throw new KeyNotFoundException("Agent profile not found.");

        if (profile.IsVerified)
            throw new InvalidOperationException(
                "Agent is already verified.");

        profile.IsVerified = true;

        _agentRepository.Update(profile);
        await _agentRepository.SaveChangesAsync();

        return true;
    }
}