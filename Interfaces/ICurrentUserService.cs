using Authentication.Models.Enums;

namespace Authentication.Interfaces;

// ─── Injected into services to get the logged-in user context ─────────────
// Never read HttpContext directly inside services — use this abstraction
public interface ICurrentUserService
{
    Guid UserId { get; }
    string Email { get; }
    UserRole Role { get; }
    bool IsAuthenticated { get; }
}