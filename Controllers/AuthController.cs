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
    private readonly CookieHelper _cookieHelper;

    public AuthController(
        IAuthService authService,
        ICurrentUserService currentUserService,
        CookieHelper cookieHelper)
    {
        _authService = authService;
        _currentUserService = currentUserService;
        _cookieHelper = cookieHelper;
    }

    // ─── POST api/auth/register ────────────────────────────────────────────
    [HttpPost("register")]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequestDto request)
    {
        var tokens = await _authService.RegisterAsync(request);

        // Set tokens in HttpOnly cookies — JS cannot access these
        _cookieHelper.SetAuthCookies(
            Response,
            tokens.AccessToken,
            tokens.RefreshToken);

        // Return only user info — no tokens in body
        return Ok(ApiResponse<AuthResponseDto>.Ok(
            tokens.UserInfo,
            "Registration successful."));
    }

    // ─── POST api/auth/login ───────────────────────────────────────────────
    [HttpPost("login")]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequestDto request)
    {
        var tokens = await _authService.LoginAsync(request);

        _cookieHelper.SetAuthCookies(
            Response,
            tokens.AccessToken,
            tokens.RefreshToken);

        return Ok(ApiResponse<AuthResponseDto>.Ok(
            tokens.UserInfo,
            "Login successful."));
    }

    // ─── POST api/auth/refresh-token ───────────────────────────────────────
    // Reads refresh token from cookie — no body needed
    [HttpPost("refresh-token")]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> RefreshToken()
    {
        // Read refresh token from HttpOnly cookie
        var refreshToken = _cookieHelper.GetRefreshToken(Request);

        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized(ApiResponse<object>.Fail(
                "Refresh token cookie missing. Please login again."));

        var tokens = await _authService.RefreshTokenAsync(refreshToken);

        // Set new rotated cookies
        _cookieHelper.SetAuthCookies(
            Response,
            tokens.AccessToken,
            tokens.RefreshToken);

        return Ok(ApiResponse<AuthResponseDto>.Ok(
            tokens.UserInfo,
            "Token refreshed successfully."));
    }

    // ─── POST api/auth/logout ──────────────────────────────────────────────
    // Revokes refresh token + clears both cookies
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = _cookieHelper.GetRefreshToken(Request);

        // Revoke in DB if token exists in cookie
        if (!string.IsNullOrEmpty(refreshToken))
        {
            try
            {
                await _authService.RevokeTokenAsync(refreshToken);
            }
            catch
            {
                // Token already revoked or invalid — still clear cookies
            }
        }

        // Always clear cookies regardless
        _cookieHelper.ClearAuthCookies(Response);

        return Ok(ApiResponse<object>.Ok(
            null!,
            "Logged out successfully."));
    }

    // ─── PUT api/auth/change-password ──────────────────────────────────────
    [HttpPut("change-password")]
    [Authorize]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequestDto request)
    {
        await _authService.ChangePasswordAsync(
            _currentUserService.UserId,
            request);

        // Clear cookies — user must login again on all devices
        _cookieHelper.ClearAuthCookies(Response);

        return Ok(ApiResponse<object>.Ok(
            null!,
            "Password changed. Please login again."));
    }

    // ─── GET api/auth/me ───────────────────────────────────────────────────
    // React calls this on app load to check if user is still logged in
    // If cookie is valid JWT passes — returns user info
    // If cookie expired/missing — returns 401
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        return Ok(ApiResponse<object>.Ok(new
        {
            UserId = _currentUserService.UserId,
            Email = _currentUserService.Email,
            Role = _currentUserService.Role.ToString(),
            IsAuthenticated = _currentUserService.IsAuthenticated
        }));
    }
}