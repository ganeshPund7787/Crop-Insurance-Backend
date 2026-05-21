using Authentication.DTOs.Auth;

namespace Authentication.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);
    Task RevokeTokenAsync(RevokeTokenRequestDto request);
    Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request);
}