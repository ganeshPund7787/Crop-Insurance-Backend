using Authentication.Configuration;
using Microsoft.Extensions.Options;

namespace Authentication.Helpers;

// ─── All cookie read/write logic in one place ──────────────────────────────
// Controllers call this — never set cookies manually in controllers
public class CookieHelper
{
    private readonly CookieSettings _cookieSettings;
    private readonly JwtSettings _jwtSettings;

    public CookieHelper(
        IOptions<CookieSettings> cookieOptions,
        IOptions<JwtSettings> jwtOptions)
    {
        _cookieSettings = cookieOptions.Value;
        _jwtSettings = jwtOptions.Value;
    }

    // ─── Set both tokens as HttpOnly cookies ───────────────────────────────
    public void SetAuthCookies(
        HttpResponse response,
        string accessToken,
        string refreshToken)
    {
        SetAccessTokenCookie(response, accessToken);
        SetRefreshTokenCookie(response, refreshToken);
    }

    // ─── Set access token cookie ───────────────────────────────────────────
    // Short-lived — matches JWT expiry exactly
    public void SetAccessTokenCookie(
        HttpResponse response,
        string accessToken)
    {
        response.Cookies.Append(
            _cookieSettings.AccessTokenCookieName,
            accessToken,
            BuildCookieOptions(
                DateTime.UtcNow.AddMinutes(
                    _jwtSettings.AccessTokenExpirationMinutes)));
    }

    // ─── Set refresh token cookie ──────────────────────────────────────────
    // Long-lived — matches refresh token expiry
    public void SetRefreshTokenCookie(
        HttpResponse response,
        string refreshToken)
    {
        response.Cookies.Append(
            _cookieSettings.RefreshTokenCookieName,
            refreshToken,
            BuildCookieOptions(
                DateTime.UtcNow.AddDays(
                    _jwtSettings.RefreshTokenExpirationDays)));
    }

    // ─── Clear both cookies on logout ─────────────────────────────────────
    // Expired cookies force browser to delete them immediately
    public void ClearAuthCookies(HttpResponse response)
    {
        response.Cookies.Append(
            _cookieSettings.AccessTokenCookieName,
            string.Empty,
            BuildCookieOptions(DateTime.UtcNow.AddDays(-1)));

        response.Cookies.Append(
            _cookieSettings.RefreshTokenCookieName,
            string.Empty,
            BuildCookieOptions(DateTime.UtcNow.AddDays(-1)));
    }

    // ─── Read access token from cookie ────────────────────────────────────
    public string? GetAccessToken(HttpRequest request)
    {
        return request.Cookies.TryGetValue(
            _cookieSettings.AccessTokenCookieName,
            out var token)
            ? token
            : null;
    }

    // ─── Read refresh token from cookie ───────────────────────────────────
    public string? GetRefreshToken(HttpRequest request)
    {
        return request.Cookies.TryGetValue(
            _cookieSettings.RefreshTokenCookieName,
            out var token)
            ? token
            : null;
    }

    // ─── Build consistent cookie options ──────────────────────────────────
    private CookieOptions BuildCookieOptions(DateTime expires)
    {
        var sameSite = _cookieSettings.SameSite switch
        {
            "Strict" => SameSiteMode.Strict,
            "None" => SameSiteMode.None,
            _ => SameSiteMode.Lax
        };

        return new CookieOptions
        {
            HttpOnly = true,                       // JS cannot read
            Secure = _cookieSettings.SecureOnly, // HTTPS only in production
            SameSite = sameSite,                   // CSRF protection
            Expires = expires,
            Path = "/"                         // Available to all routes
        };
    }
}