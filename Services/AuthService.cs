using Authentication.Configuration;
using Authentication.DTOs.Auth;
using Authentication.Helpers;
using Authentication.Interfaces;
using Authentication.Models;
using Authentication.Models.Enums;
using AutoMapper;
using Microsoft.Extensions.Options;

namespace Authentication.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        ITokenService tokenService,
        IMapper mapper,
        IOptions<JwtSettings> jwtOptions)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _tokenService = tokenService;
        _mapper = mapper;
        _jwtSettings = jwtOptions.Value;
    }

    // ─── Register ──────────────────────────────────────────────────────────
    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        // ── 1. Duplicate email check ───────────────────────────────────────
        if (await _userRepository.EmailExistsAsync(request.Email))
            throw new InvalidOperationException(
                "An account with this email already exists.");

        // ── 2. Build User entity ───────────────────────────────────────────
        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = request.Email.ToLower().Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            Role = request.Role,
            PasswordHash = PasswordHelper.HashPassword(request.Password),
            IsActive = true,
            EmailVerified = false
        };

        // ── 3. Create role-specific profile ────────────────────────────────
        // Each role gets its own profile table — separation of concerns
        // Profile is created atomically with the user in same SaveChanges
        switch (request.Role)
        {
            case UserRole.Farmer:
                user.FarmerProfile = new FarmerProfile
                {
                    Village = request.Village?.Trim() ?? string.Empty,
                    District = request.District?.Trim() ?? string.Empty,
                    State = request.State?.Trim() ?? string.Empty,
                    AadhaarNumber = request.AadhaarNumber?.Trim() ?? string.Empty,
                    TotalLandAcres = request.TotalLandAcres ?? 0,
                    IsKycVerified = false
                };
                break;

            case UserRole.InsuranceAgent:
                if (string.IsNullOrWhiteSpace(request.AgentCode))
                    throw new InvalidOperationException(
                        "Agent code is required for Insurance Agent registration.");

                if (string.IsNullOrWhiteSpace(request.LicenseNumber))
                    throw new InvalidOperationException(
                        "License number is required for Insurance Agent registration.");

                user.AgentProfile = new AgentProfile
                {
                    AgentCode = request.AgentCode.Trim(),
                    LicenseNumber = request.LicenseNumber.Trim(),
                    AssignedDistrict = request.AssignedDistrict?.Trim() ?? string.Empty,
                    IsVerified = false
                };
                break;

            case UserRole.Admin:
                // Admin accounts created by super admin only
                // No additional profile needed
                break;
        }

        // ── 4. Generate refresh token ──────────────────────────────────────
        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync(); // Save user first to get Id in DB

        var refreshToken = _tokenService.GenerateRefreshToken();
        refreshToken.UserId = user.Id;      // ✅ user.Id now exists in DB
        refreshToken.CreatedAtUtc = DateTime.UtcNow;

        await _refreshTokenRepository.AddAsync(refreshToken);
        await _refreshTokenRepository.SaveChangesAsync();
        // ── 6. Generate access token ───────────────────────────────────────
        var accessToken = _tokenService.GenerateAccessToken(user);

        return BuildAuthResponse(user, accessToken, refreshToken.Token);
    }

    // ─── Login ─────────────────────────────────────────────────────────────
    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        // ── 1. Find user ───────────────────────────────────────────────────────
        var user = await _userRepository.GetByEmailAsync(request.Email);

        // ── 2. Validate credentials ────────────────────────────────────────────
        if (user is null ||
            !PasswordHelper.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        // ── 3. Account status check ────────────────────────────────────────────
        if (!user.IsActive)
            throw new UnauthorizedAccessException(
                "Your account has been deactivated. Contact support.");

        // ── 4. Clean up expired tokens ─────────────────────────────────────────
        var expiredTokens = user.RefreshTokens
            .Where(t => t.IsExpired && !t.IsRevoked)
            .ToList();

        foreach (var expired in expiredTokens)
        {
            expired.IsRevoked = true;
            expired.RevokedAtUtc = DateTime.UtcNow;
            expired.ReasonRevoked = "Expired — cleaned up on login";
        }

        // ── 5. Track last login ────────────────────────────────────────────────
        user.LastLoginAtUtc = DateTime.UtcNow;

        // ── 6. Build and attach refresh token ──────────────────────────────────
        // ✅ Set UserId explicitly BEFORE adding to collection
        var refreshToken = _tokenService.GenerateRefreshToken();
        refreshToken.UserId = user.Id;
        refreshToken.CreatedAtUtc = DateTime.UtcNow;

        // ✅ Add directly to DbContext — do NOT use user.RefreshTokens.Add()
        // This avoids EF state confusion on the navigation collection
        await _refreshTokenRepository.AddAsync(refreshToken);

        // ── 7. Save all in one transaction ────────────────────────────────────
        await _userRepository.SaveChangesAsync();

        // ── 8. Generate access token ───────────────────────────────────────────
        var accessToken = _tokenService.GenerateAccessToken(user);

        return BuildAuthResponse(user, accessToken, refreshToken.Token);
    }
    // ─── Refresh Token ─────────────────────────────────────────────────────
    public async Task<AuthResponseDto> RefreshTokenAsync(
        RefreshTokenRequestDto request)
    {
        // ── 1. Find user by refresh token ──────────────────────────────────
        var user = await _refreshTokenRepository
            .GetUserByRefreshTokenAsync(request.RefreshToken);

        if (user is null)
            throw new UnauthorizedAccessException("Invalid refresh token.");

        // ── 2. Find the specific token record ──────────────────────────────
        var existingToken = user.RefreshTokens
            .SingleOrDefault(t => t.Token == request.RefreshToken);

        if (existingToken is null)
            throw new UnauthorizedAccessException("Invalid refresh token.");

        // ── 3. Detect token reuse attack ───────────────────────────────────
        // If a revoked token is used again — it was stolen
        // Revoke ALL tokens for this user immediately
        if (existingToken.IsRevoked)
        {
            await _refreshTokenRepository.RevokeAllUserTokensAsync(
                user.Id,
                "Suspicious activity — revoked token reuse detected");

            throw new UnauthorizedAccessException(
                "Refresh token has been revoked. All sessions terminated.");
        }

        // ── 4. Check token expiry ──────────────────────────────────────────
        if (existingToken.IsExpired)
            throw new UnauthorizedAccessException(
                "Refresh token has expired. Please login again.");

        // ── 5. Rotate — revoke old, issue new ─────────────────────────────
        // Token rotation: every refresh produces a brand new token
        // Old token immediately invalidated
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        newRefreshToken.UserId = user.Id;

        existingToken.IsRevoked = true;
        existingToken.RevokedAtUtc = DateTime.UtcNow;
        existingToken.ReasonRevoked = "Replaced by token rotation";
        existingToken.ReplacedByToken = newRefreshToken.Token;

        //user.RefreshTokens.Add(newRefreshToken);

        //await _userRepository.SaveChangesAsync();
        await _refreshTokenRepository.AddAsync(newRefreshToken);
        await _userRepository.SaveChangesAsync(); // saves revoked old token
        await _refreshTokenRepository.SaveChangesAsync();

        // ── 6. Issue new access token ──────────────────────────────────────
        var accessToken = _tokenService.GenerateAccessToken(user);

        return BuildAuthResponse(user, accessToken, newRefreshToken.Token);
    }

    // ─── Revoke Token ──────────────────────────────────────────────────────
    public async Task RevokeTokenAsync(RevokeTokenRequestDto request)
    {
        var user = await _refreshTokenRepository
            .GetUserByRefreshTokenAsync(request.RefreshToken);

        if (user is null)
            throw new UnauthorizedAccessException("Invalid refresh token.");

        var token = user.RefreshTokens
            .SingleOrDefault(t => t.Token == request.RefreshToken);

        if (token is null || token.IsRevoked)
            throw new InvalidOperationException(
                "Token is invalid or already revoked.");

        token.IsRevoked = true;
        token.RevokedAtUtc = DateTime.UtcNow;
        token.ReasonRevoked = "Revoked by user (logout)";

        await _userRepository.SaveChangesAsync();
    }

    // ─── Change Password ───────────────────────────────────────────────────
    public async Task<bool> ChangePasswordAsync(
        Guid userId,
        ChangePasswordRequestDto request)
    {
        // ── 1. Validate new passwords match ────────────────────────────────
        if (request.NewPassword != request.ConfirmNewPassword)
            throw new InvalidOperationException(
                "New password and confirmation do not match.");

        // ── 2. Load user with tokens ───────────────────────────────────────
        var user = await _userRepository
            .GetByIdWithRefreshTokensAsync(userId);

        if (user is null)
            throw new InvalidOperationException("User not found.");

        // ── 3. Verify current password ─────────────────────────────────────
        if (!PasswordHelper.VerifyPassword(
                request.CurrentPassword, user.PasswordHash))
        {
            throw new UnauthorizedAccessException(
                "Current password is incorrect.");
        }

        // ── 4. Hash and update new password ───────────────────────────────
        user.PasswordHash = PasswordHelper.HashPassword(request.NewPassword);
        user.UpdatedAtUtc = DateTime.UtcNow;

        // ── 5. Revoke ALL refresh tokens ───────────────────────────────────
        // Password change = all existing sessions invalidated
        // User must log in again on all devices
        await _refreshTokenRepository.RevokeAllUserTokensAsync(
            userId,
            "All sessions revoked after password change");

        await _userRepository.SaveChangesAsync();

        return true;
    }

    // ─── Private Helper ────────────────────────────────────────────────────
    private AuthResponseDto BuildAuthResponse(
        User user,
        string accessToken,
        string refreshToken)    
    {
        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresAtUtc = DateTime.UtcNow.AddMinutes(
                                         _jwtSettings.AccessTokenExpirationMinutes),
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }
}