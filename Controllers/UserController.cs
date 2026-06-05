using Authentication.DTOs.User;
using Authentication.Helpers;
using Authentication.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Authentication.Controllers;

[ApiController]
[Route("api/user")]
[Authorize]  // All endpoints require login
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUserService;

    public UserController(
        IUserService userService,
        ICurrentUserService currentUserService)
    {
        _userService = userService;
        _currentUserService = currentUserService;
    }



    // ─── GET api/user/debug-claims ─────────────────────────────────────────────
    // REMOVE THIS AFTER DEBUGGING
    [HttpGet("debug-claims")]
    [Authorize]
    public IActionResult DebugClaims()
    {
        var claims = User.Claims
            .Select(c => new { c.Type, c.Value })
            .ToList();

        return Ok(new
        {
            IsAuthenticated = User.Identity?.IsAuthenticated,
            UserId = _currentUserService.UserId,
            Email = _currentUserService.Email,
            Role = _currentUserService.Role.ToString(),
            AllClaims = claims
        });
    }

    // ─── GET api/user/profile ──────────────────────────────────────────────
    // Any authenticated user can get their own profile
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var result = await _userService
            .GetProfileAsync(_currentUserService.UserId);

        return Ok(ApiResponse<UserProfileDto>.Ok(result));
    }

    // ─── PUT api/user/profile ──────────────────────────────────────────────
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateProfileRequestDto request)
    {
        var result = await _userService
            .UpdateProfileAsync(_currentUserService.UserId, request);

        return Ok(ApiResponse<UserProfileDto>.Ok(
            result,
            "Profile updated successfully."));
    }
}