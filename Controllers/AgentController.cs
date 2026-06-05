using Authentication.DTOs.Agent;
using Authentication.Helpers;
using Authentication.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Authentication.Controllers;

[ApiController]
[Route("api/agent")]
[Authorize(Roles = "InsuranceAgent")]
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
            result, "Profile updated successfully."));
    }

    // ─── GET api/agent/claims ──────────────────────────────────────────────
    [HttpGet("claims")]
    public async Task<IActionResult> GetDistrictClaims()
    {
        var result = await _agentService
            .GetDistrictClaimsAsync(_currentUserService.UserId);

        return Ok(ApiResponse<IEnumerable<ClaimSummaryDto>>.Ok(result));
    }

    // ─── GET api/agent/claims/{claimId} ───────────────────────────────────
    [HttpGet("claims/{claimId:guid}")]
    public async Task<IActionResult> GetClaimDetail(Guid claimId)
    {
        var result = await _agentService
            .GetClaimDetailAsync(_currentUserService.UserId, claimId);

        return Ok(ApiResponse<ClaimDetailDto>.Ok(result));
    }

    // ─── PUT api/agent/claims/{claimId}/assign ─────────────────────────────
    [HttpPut("claims/{claimId:guid}/assign")]
    public async Task<IActionResult> AssignClaim(Guid claimId)
    {
        var result = await _agentService
            .AssignClaimAsync(_currentUserService.UserId, claimId);

        return Ok(ApiResponse<ClaimDetailDto>.Ok(
            result, "Claim assigned successfully."));
    }

    // ─── POST api/agent/claims/{claimId}/inspection ────────────────────────
    [HttpPost("claims/{claimId:guid}/inspection")]
    public async Task<IActionResult> CreateInspection(
        Guid claimId,
        [FromBody] CreateInspectionRequestDto request)
    {
        var result = await _agentService
            .CreateInspectionAsync(
                _currentUserService.UserId, claimId, request);

        return Created(
            $"api/agent/inspections/{result.Id}",
            ApiResponse<InspectionDto>.Ok(
                result, "Inspection scheduled successfully."));
    }

    // ─── PUT api/agent/claims/{claimId}/approve ────────────────────────────
    [HttpPut("claims/{claimId:guid}/approve")]
    public async Task<IActionResult> ApproveClaim(
        Guid claimId,
        [FromBody] ApproveClaimRequestDto request)
    {
        var result = await _agentService
            .ApproveClaimAsync(
                _currentUserService.UserId, claimId, request);

        return Ok(ApiResponse<ClaimDetailDto>.Ok(
            result, "Claim approved successfully."));
    }

    // ─── PUT api/agent/claims/{claimId}/reject ─────────────────────────────
    [HttpPut("claims/{claimId:guid}/reject")]
    public async Task<IActionResult> RejectClaim(
        Guid claimId,
        [FromBody] RejectClaimRequestDto request)
    {
        var result = await _agentService
            .RejectClaimAsync(
                _currentUserService.UserId, claimId, request);

        return Ok(ApiResponse<ClaimDetailDto>.Ok(
            result, "Claim rejected."));
    }

    // ─── GET api/agent/inspections ─────────────────────────────────────────
    [HttpGet("inspections")]
    public async Task<IActionResult> GetMyInspections()
    {
        var result = await _agentService
            .GetMyInspectionsAsync(_currentUserService.UserId);

        return Ok(ApiResponse<IEnumerable<InspectionDto>>.Ok(result));
    }

    // ─── PUT api/agent/inspections/{inspectionId} ──────────────────────────
    [HttpPut("inspections/{inspectionId:guid}")]
    public async Task<IActionResult> UpdateInspection(
        Guid inspectionId,
        [FromBody] UpdateInspectionRequestDto request)
    {
        var result = await _agentService
            .UpdateInspectionAsync(
                _currentUserService.UserId, inspectionId, request);

        return Ok(ApiResponse<InspectionDto>.Ok(
            result, "Inspection updated successfully."));
    }

    // ─── GET api/agent/farmers ─────────────────────────────────────────────
    [HttpGet("farmers")]
    public async Task<IActionResult> GetDistrictFarmers()
    {
        var result = await _agentService
            .GetDistrictFarmersAsync(_currentUserService.UserId);

        return Ok(ApiResponse<IEnumerable<DistrictFarmerDto>>.Ok(result));
    }
}