using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Authentication.Configuration;
using Authentication.Interfaces;
using Authentication.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Authentication.Services;

public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;

    public TokenService(IOptions<JwtSettings> jwtOptions)
    {
        _jwtSettings = jwtOptions.Value;
    }

    // ─── Access Token ──────────────────────────────────────────────────────
    // Short-lived (15 min) JWT containing user identity + role
    // Stateless — validated on every request without DB hit
    public string GenerateAccessToken(User user)
    {
        var claims = new List<Claim>
        {
            // Standard JWT claims
            new(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64),

            // App-specific claims
            new(ClaimTypes.Role,               user.Role.ToString()),
            new("FullName",                    user.FullName),
            new("IsActive",                    user.IsActive.ToString()),
            new("EmailVerified",               user.EmailVerified.ToString())
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(
                                    _jwtSettings.AccessTokenExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // ─── Refresh Token ─────────────────────────────────────────────────────
    // Long-lived (7 days) opaque random token stored in DB
    // Rotated on every use — old token revoked, new one issued
    public RefreshToken GenerateRefreshToken()
    {
        return new RefreshToken
        {
            Token = GenerateSecureToken(),
            ExpiresAtUtc = DateTime.UtcNow.AddDays(
                               _jwtSettings.RefreshTokenExpirationDays),
            IsRevoked = false
        };
    }

    // ─── Extract claims from expired access token ──────────────────────────
    // Used in /refresh-token endpoint
    // Validates signature but ignores expiry — this is intentional
    // We verify the refresh token separately to authorize the new access token
    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = false, // ← intentionally ignore expiry

            ValidIssuer = _jwtSettings.Issuer,
            ValidAudience = _jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                                   Encoding.UTF8.GetBytes(_jwtSettings.SecretKey))
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var principal = tokenHandler.ValidateToken(
                token,
                tokenValidationParameters,
                out var securityToken);

            // Make sure it's actually a JWT with correct algorithm
            if (securityToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(
                    SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }

    // ─── Extract UserId from token ─────────────────────────────────────────
    // Used in middleware and services to identify user
    public Guid GetUserIdFromToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var jwtToken = tokenHandler.ReadJwtToken(token);

            var sub = jwtToken.Claims
                .FirstOrDefault(x =>
                    x.Type == JwtRegisteredClaimNames.Sub)?.Value;

            return Guid.TryParse(sub, out var userId)
                ? userId
                : Guid.Empty;
        }
        catch
        {
            return Guid.Empty;
        }
    }

    // ─── Cryptographically secure random token ─────────────────────────────
    // 64 bytes = 512 bits of entropy — unguessable
    private static string GenerateSecureToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}