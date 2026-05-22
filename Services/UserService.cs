using Authentication.DTOs.User;
using Authentication.Interfaces;
using Authentication.Models.Enums;
using AutoMapper;

namespace Authentication.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UserService(
        IUserRepository userRepository,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    // ─── Get profile ───────────────────────────────────────────────────────
    public async Task<UserProfileDto> GetProfileAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        return _mapper.Map<UserProfileDto>(user);
    }

    // ─── Update profile ────────────────────────────────────────────────────
    public async Task<UserProfileDto> UpdateProfileAsync(
        Guid userId,
        UpdateProfileRequestDto request)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        user.FullName = request.FullName.Trim();
        user.PhoneNumber = request.PhoneNumber.Trim();

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        return _mapper.Map<UserProfileDto>(user);
    }

    // ─── Deactivate user ───────────────────────────────────────────────────
    // Soft disable — user cannot login but data preserved
    public async Task<bool> DeactivateUserAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        if (!user.IsActive)
            throw new InvalidOperationException(
                "User is already deactivated.");

        user.IsActive = false;

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        return true;
    }

    // ─── Activate user ─────────────────────────────────────────────────────
    public async Task<bool> ActivateUserAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        if (user.IsActive)
            throw new InvalidOperationException(
                "User is already active.");

        user.IsActive = true;

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        return true;
    }

    // ─── Get all users by role ─────────────────────────────────────────────
    // Admin only — used for user management dashboard
    public async Task<IEnumerable<UserProfileDto>> GetAllUsersByRoleAsync(
        UserRole role)
    {
        var users = await _userRepository.GetAllByRoleAsync(role);

        return _mapper.Map<IEnumerable<UserProfileDto>>(users);
    }

    // ─── Soft delete user ──────────────────────────────────────────────────
    // Sets IsDeleted = true — global query filter hides them from all queries
    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        _userRepository.Delete(user);
        await _userRepository.SaveChangesAsync();

        return true;
    }
}