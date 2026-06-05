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
    public async Task<AuthTokensDto> RegisterAsync(RegisterRequestDto request)
    {
        if (await _userRepository.EmailExistsAsync(request.Email))
            throw new InvalidOperationException(
                "An account with this email already exists.");

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
                        "Agent code is required for agent registration.");

                if (string.IsNullOrWhiteSpace(request.LicenseNumber))
                    throw new InvalidOperationException(
                        "License number is required for agent registration.");

                user.AgentProfile = new AgentProfile
                {
                    AgentCode = request.AgentCode.Trim(),
                    LicenseNumber = request.LicenseNumber.Trim(),
                    AssignedDistrict = request.AssignedDistrict?.Trim()
                                       ?? string.Empty,
                    IsVerified = false
                };
                break;

            case UserRole.Admin:
                break;
        }

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        var refreshToken = _tokenService.GenerateRefreshToken();
        refreshToken.UserId = user.Id;
        refreshToken.CreatedAtUtc = DateTime.UtcNow;

        await _refreshTokenRepository.AddAsync(refreshToken);
        await _refreshTokenRepository.SaveChangesAsync();

        var accessToken = _tokenService.GenerateAccessToken(user);

        // ── Return tokens + user info — controller sets cookies ────────────
        return BuildTokensDto(user, accessToken, refreshToken.Token);
    }

    // ─── Login ─────────────────────────────────────────────────────────────
    public async Task<AuthTokensDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user is null ||
            !PasswordHelper.VerifyPassword(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        if (!user.IsActive)
            throw new UnauthorizedAccessException(
                "Your account has been deactivated. Contact support.");

        // Clean up expired tokens
        var expiredTokens = user.RefreshTokens
            .Where(t => t.IsExpired && !t.IsRevoked)
            .ToList();

        foreach (var expired in expiredTokens)
        {
            expired.IsRevoked = true;
            expired.RevokedAtUtc = DateTime.UtcNow;
            expired.ReasonRevoked = "Expired — cleaned up on login";
        }

        user.LastLoginAtUtc = DateTime.UtcNow;

        var refreshToken = _tokenService.GenerateRefreshToken();
        refreshToken.UserId = user.Id;
        refreshToken.CreatedAtUtc = DateTime.UtcNow;

        await _refreshTokenRepository.AddAsync(refreshToken);
        await _userRepository.SaveChangesAsync();

        var accessToken = _tokenService.GenerateAccessToken(user);

        return BuildTokensDto(user, accessToken, refreshToken.Token);
    }

    // ─── Refresh Token ─────────────────────────────────────────────────────
    public async Task<AuthTokensDto> RefreshTokenAsync(string refreshToken)
    {
        var user = await _refreshTokenRepository
            .GetUserByRefreshTokenAsync(refreshToken);

        if (user is null)
            throw new UnauthorizedAccessException("Invalid refresh token.");

        var existingToken = user.RefreshTokens
            .SingleOrDefault(t => t.Token == refreshToken);

        if (existingToken is null)
            throw new UnauthorizedAccessException("Invalid refresh token.");

        // Detect reuse attack — revoke all sessions
        if (existingToken.IsRevoked)
        {
            await _refreshTokenRepository.RevokeAllUserTokensAsync(
                user.Id,
                "Suspicious activity — revoked token reuse detected");

            throw new UnauthorizedAccessException(
                "Security violation detected. All sessions terminated.");
        }

        if (existingToken.IsExpired)
            throw new UnauthorizedAccessException(
                "Refresh token expired. Please login again.");

        // Rotate token
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        newRefreshToken.UserId = user.Id;
        newRefreshToken.CreatedAtUtc = DateTime.UtcNow;

        existingToken.IsRevoked = true;
        existingToken.RevokedAtUtc = DateTime.UtcNow;
        existingToken.ReasonRevoked = "Replaced by token rotation";
        existingToken.ReplacedByToken = newRefreshToken.Token;

        await _refreshTokenRepository.AddAsync(newRefreshToken);
        await _userRepository.SaveChangesAsync();
        await _refreshTokenRepository.SaveChangesAsync();

        var accessToken = _tokenService.GenerateAccessToken(user);

        return BuildTokensDto(user, accessToken, newRefreshToken.Token);
    }

    // ─── Revoke Token ──────────────────────────────────────────────────────
    public async Task RevokeTokenAsync(string refreshToken)
    {
        var user = await _refreshTokenRepository
            .GetUserByRefreshTokenAsync(refreshToken);

        if (user is null)
            throw new UnauthorizedAccessException("Invalid refresh token.");

        var token = user.RefreshTokens
            .SingleOrDefault(t => t.Token == refreshToken);

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
        if (request.NewPassword != request.ConfirmNewPassword)
            throw new InvalidOperationException(
                "Passwords do not match.");

        var user = await _userRepository
            .GetByIdWithRefreshTokensAsync(userId)
            ?? throw new InvalidOperationException("User not found.");

        if (!PasswordHelper.VerifyPassword(
                request.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException(
                "Current password is incorrect.");

        user.PasswordHash = PasswordHelper.HashPassword(request.NewPassword);
        user.UpdatedAtUtc = DateTime.UtcNow;

        await _refreshTokenRepository.RevokeAllUserTokensAsync(
            userId, "All sessions revoked after password change");

        await _userRepository.SaveChangesAsync();

        return true;
    }

    // ─── Internal DTO — carries tokens to controller ───────────────────────
    // Controller reads these and sets HttpOnly cookies
    // Tokens never leave the server in response body
    private AuthTokensDto BuildTokensDto(
        User user,
        string accessToken,
        string refreshToken)
    {
        return new AuthTokensDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresAtUtc = DateTime.UtcNow.AddMinutes(
                                          _jwtSettings.AccessTokenExpirationMinutes),
            UserInfo = new AuthResponseDto
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString(),
                AccessTokenExpiresAtUtc = DateTime.UtcNow.AddMinutes(
                                              _jwtSettings.AccessTokenExpirationMinutes)
            }
        };
    }
}