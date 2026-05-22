using Authentication.Data;
using Authentication.Interfaces;
using Authentication.Models;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Services;

public class RefreshTokenRepository
    : BaseRepository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(AppDbContext context) : base(context) { }

    // ─── Find token record by token string ────────────────────────────────
    // Used in RefreshToken endpoint — fast lookup via indexed column
    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Token == token);
    }

    // ─── Get full user object via refresh token ────────────────────────────
    // Used to validate token belongs to a real active user
    public async Task<User?> GetUserByRefreshTokenAsync(string token)
    {
        return await _context.Users
            .Include(x => x.RefreshTokens)
            .FirstOrDefaultAsync(x =>
                x.RefreshTokens.Any(rt => rt.Token == token));
    }

    // ─── Revoke all tokens for a user ─────────────────────────────────────
    // Used on password change, suspicious activity, or admin action
    public async Task RevokeAllUserTokensAsync(Guid userId, string reason)
    {
        var activeTokens = await _context.RefreshTokens
            .Where(x =>
                x.UserId == userId &&
                !x.IsRevoked &&
                x.ExpiresAtUtc > DateTime.UtcNow)
            .ToListAsync();

        foreach (var token in activeTokens)
        {
            token.IsRevoked = true;
            token.RevokedAtUtc = DateTime.UtcNow;
            token.ReasonRevoked = reason;
        }

        await _context.SaveChangesAsync();
    }

    // ─── Get all active (non-expired, non-revoked) tokens for a user ──────
    // Used by Admin for security monitoring
    public async Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(
        Guid userId)
    {
        return await _context.RefreshTokens
            .Where(x =>
                x.UserId == userId &&
                !x.IsRevoked &&
                x.ExpiresAtUtc > DateTime.UtcNow)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();
    }
}