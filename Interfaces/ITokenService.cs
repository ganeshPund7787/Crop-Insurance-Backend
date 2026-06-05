using Authentication.Models;
using System.Security.Claims;

namespace Authentication.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    RefreshToken GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    Guid GetUserIdFromToken(string token);
}