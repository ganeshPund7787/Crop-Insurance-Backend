using Authentication.Models;

namespace Authentication.Interfaces;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task<User?> GetUserByRefreshTokenAsync(string token);
    Task RevokeAllUserTokensAsync(Guid userId, string reason);
    Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId);
}