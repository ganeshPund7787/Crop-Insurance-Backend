using Authentication.DTOs.Auth;
using Authentication.Helpers;
using Authentication.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Authentication.Controllers;

[ApiController]
[Route("api/auth")]

public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUserService;

    public AuthController(
        IAuthService authService,
        ICurrentUserService currentUserService)
    {
        _authService = authService;
        _currentUserService = currentUserService;
    }

    // ─── POST api/auth/register ────────────────────────────────────────────
    // Public — no auth required
    // FluentValidation runs automatically before this method is called
    [HttpPost("register")]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequestDto request)
    {
        var result = await _authService.RegisterAsync(request);

        return Ok(ApiResponse<AuthResponseDto>.Ok(
            result,
            "Registration successful."));
    }

    // ─── POST api/auth/login ───────────────────────────────────────────────
    [HttpPost("login")]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);

        return Ok(ApiResponse<AuthResponseDto>.Ok(
            result,
            "Login successful."));
    }

    // ─── POST api/auth/refresh-token ───────────────────────────────────────
    // Public — expired access token is expected here
    [HttpPost("refresh-token")]
    [EnableRateLimiting("AuthPolicy")]

    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenRequestDto request)
    {
        var result = await _authService.RefreshTokenAsync(request);

        return Ok(ApiResponse<AuthResponseDto>.Ok(
            result,
            "Token refreshed successfully."));
    }

    // ─── POST api/auth/revoke-token ────────────────────────────────────────
    // Requires valid JWT — logout current session
    [HttpPost("revoke-token")]
    [Authorize]
    [EnableRateLimiting("AuthPolicy")]

    public async Task<IActionResult> RevokeToken(
        [FromBody] RevokeTokenRequestDto request)
    {
        await _authService.RevokeTokenAsync(request);

        return Ok(ApiResponse<object>.Ok(
            null!,
            "Token revoked successfully. Logged out."));
    }

    // ─── PUT api/auth/change-password ──────────────────────────────────────
    // Requires valid JWT — all sessions revoked after change
    [HttpPut("change-password")]
    [Authorize]
    [EnableRateLimiting("AuthPolicy")]

    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequestDto request)
    {
        await _authService.ChangePasswordAsync(
            _currentUserService.UserId,
            request);

        return Ok(ApiResponse<object>.Ok(
            null!,
            "Password changed. All sessions have been revoked."));
    }
}