using Authentication.Data;
using Authentication.Interfaces;
using Authentication.Models;
using Authentication.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Services;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .Include(x => x.RefreshTokens)
            .FirstOrDefaultAsync(x =>
                x.Email == email.ToLower().Trim()
                && !x.IsDeleted);
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users
            .FirstOrDefaultAsync(x =>
                x.Id == id
                && !x.IsDeleted);
    }

    public async Task<User?> GetByIdWithProfileAsync(Guid id)
    {
        return await _context.Users
            .Include(x => x.FarmerProfile)
                .ThenInclude(fp => fp!.Farms)
                    .ThenInclude(f => f.Crops)
            .Include(x => x.AgentProfile)
            .FirstOrDefaultAsync(x =>
                x.Id == id
                && !x.IsDeleted);
    }

    public async Task<User?> GetByIdWithRefreshTokensAsync(Guid id)
    {
        return await _context.Users
            .Include(x => x.RefreshTokens)
            .FirstOrDefaultAsync(x =>
                x.Id == id
                && !x.IsDeleted);
    }

    public async Task<IEnumerable<User>> GetAllByRoleAsync(UserRole role)
    {
        return await _context.Users
            .Where(x => x.Role == role && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users
            .AnyAsync(x =>
                x.Email == email.ToLower().Trim()
                && !x.IsDeleted);
    }
}