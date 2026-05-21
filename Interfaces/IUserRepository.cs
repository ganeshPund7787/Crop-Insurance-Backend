using Authentication.Models;
using Authentication.Models.Enums;

namespace Authentication.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdWithProfileAsync(Guid id);
    Task<User?> GetByIdWithRefreshTokensAsync(Guid id);
    Task<IEnumerable<User>> GetAllByRoleAsync(UserRole role);
    Task<bool> EmailExistsAsync(string email);
}