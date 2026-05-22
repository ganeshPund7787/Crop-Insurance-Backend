using Authentication.DTOs.Agent;
using Authentication.Helpers;
using Authentication.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Authentication.Controllers;

[ApiController]
[Route("api/agent")]
[Authorize(Roles = "InsuranceAgent")]  // Agent role only
public class AgentController : ControllerBase
{
    private readonly IAgentService _agentService;
    private readonly ICurrentUserService _currentUserService;

    public AgentController(
        IAgentService agentService,
        ICurrentUserService currentUserService)
    {
        _agentService = agentService;
        _currentUserService = currentUserService;
    }

    // ─── GET api/agent/profile ─────────────────────────────────────────────
    [HttpGet("profile")]
    public async Task<IActionResult> GetAgentProfile()
    {
        var result = await _agentService
            .GetAgentProfileAsync(_currentUserService.UserId);

        return Ok(ApiResponse<AgentProfileDto>.Ok(result));
    }

    // ─── PUT api/agent/profile ─────────────────────────────────────────────
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateAgentProfile(
        [FromBody] UpdateAgentProfileRequestDto request)
    {
        var result = await _agentService
            .UpdateAgentProfileAsync(_currentUserService.UserId, request);

        return Ok(ApiResponse<AgentProfileDto>.Ok(
            result,
            "Agent profile updated successfully."));
    }
}