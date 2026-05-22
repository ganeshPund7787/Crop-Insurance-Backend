using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Authentication.Interfaces;
using Authentication.Models.Enums;

namespace Authentication.Helpers;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CurrentUserService> _logger;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor,
        ILogger<CurrentUserService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    private ClaimsPrincipal? User =>
        _httpContextAccessor.HttpContext?.User;

    public Guid UserId
    {
        get
        {
            // ── Try Sub claim first ────────────────────────────────────────
            var sub = User?.FindFirstValue(JwtRegisteredClaimNames.Sub)
                   ?? User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(sub))
            {
                _logger.LogWarning(
                    "UserId claim missing from JWT token.");
                return Guid.Empty;
            }

            if (!Guid.TryParse(sub, out var userId))
            {
                _logger.LogWarning(
                    "UserId claim '{Sub}' is not a valid Guid.", sub);
                return Guid.Empty;
            }

            return userId;
        }
    }

    public string Email =>
        User?.FindFirstValue(JwtRegisteredClaimNames.Email)
        ?? User?.FindFirstValue(ClaimTypes.Email)
        ?? string.Empty;

    public UserRole Role
    {
        get
        {
            var role = User?.FindFirstValue(ClaimTypes.Role);
            return Enum.TryParse<UserRole>(role, out var result)
                ? result
                : UserRole.Farmer;
        }
    }

    public bool IsAuthenticated =>
        User?.Identity?.IsAuthenticated ?? false;
}