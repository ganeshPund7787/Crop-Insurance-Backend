using Authentication.DTOs.User;
using Authentication.Helpers;
using Authentication.Interfaces;
using Authentication.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Authentication.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]  // Admin role only
public class AdminController : ControllerBase
{
    private readonly IUserService _userService;

    public AdminController(IUserService userService)
    {
        _userService = userService;
    }

    // ─── GET api/admin/users/farmers ──────────────────────────────────────
    [HttpGet("users/farmers")]
    public async Task<IActionResult> GetAllFarmers()
    {
        var result = await _userService
            .GetAllUsersByRoleAsync(UserRole.Farmer);

        return Ok(ApiResponse<IEnumerable<UserProfileDto>>.Ok(result));
    }

    // ─── GET api/admin/users/agents ───────────────────────────────────────
    [HttpGet("users/agents")]
    public async Task<IActionResult> GetAllAgents()
    {
        var result = await _userService
            .GetAllUsersByRoleAsync(UserRole.InsuranceAgent);

        return Ok(ApiResponse<IEnumerable<UserProfileDto>>.Ok(result));
    }

    // ─── PUT api/admin/users/{userId}/deactivate ──────────────────────────
    [HttpPut("users/{userId:guid}/deactivate")]
    public async Task<IActionResult> DeactivateUser(Guid userId)
    {
        await _userService.DeactivateUserAsync(userId);

        return Ok(ApiResponse<object>.Ok(
            null!,
            "User deactivated successfully."));
    }

    // ─── PUT api/admin/users/{userId}/activate ────────────────────────────
    [HttpPut("users/{userId:guid}/activate")]
    public async Task<IActionResult> ActivateUser(Guid userId)
    {
        await _userService.ActivateUserAsync(userId);

        return Ok(ApiResponse<object>.Ok(
            null!,
            "User activated successfully."));
    }

    // ─── DELETE api/admin/users/{userId} ──────────────────────────────────
    // Soft delete — user marked IsDeleted, excluded from all queries
    [HttpDelete("users/{userId:guid}")]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        await _userService.DeleteUserAsync(userId);

        return Ok(ApiResponse<object>.Ok(
            null!,
            "User deleted successfully."));
    }

    // ─── GET api/admin/users/{userId} ─────────────────────────────────────
    [HttpGet("users/{userId:guid}")]
    public async Task<IActionResult> GetUserById(Guid userId)
    {
        var result = await _userService.GetProfileAsync(userId);

        return Ok(ApiResponse<UserProfileDto>.Ok(result));
    }
}