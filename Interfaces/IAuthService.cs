using Authentication.DTOs.Auth;

namespace Authentication.Interfaces;

public interface IAuthService
{
    Task<AuthTokensDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthTokensDto> LoginAsync(LoginRequestDto request);
    Task<AuthTokensDto> RefreshTokenAsync(string refreshToken);
    Task RevokeTokenAsync(string refreshToken);
    Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request);
}