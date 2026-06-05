using Authentication.DTOs.User;
using Authentication.Models.Enums;

namespace Authentication.Interfaces;

public interface IUserService
{
    Task<UserProfileDto> GetProfileAsync(Guid userId);
    Task<UserProfileDto> UpdateProfileAsync(Guid userId, UpdateProfileRequestDto request);
    Task<bool> DeactivateUserAsync(Guid userId);
    Task<bool> ActivateUserAsync(Guid userId);
    Task<IEnumerable<UserProfileDto>> GetAllUsersByRoleAsync(UserRole role);
    Task<bool> DeleteUserAsync(Guid userId);
}