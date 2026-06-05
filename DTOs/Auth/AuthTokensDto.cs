namespace Authentication.DTOs.Auth;

// ─── Internal only — never serialized to client ────────────────────────────
// Carries tokens from AuthService to controller
// Controller puts tokens in cookies, sends only UserInfo to client
public class AuthTokensDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiresAtUtc { get; set; }
    public AuthResponseDto UserInfo { get; set; } = default!;
}