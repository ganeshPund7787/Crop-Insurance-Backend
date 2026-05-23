namespace Authentication.Configuration;

public class CookieSettings
{
    public string AccessTokenCookieName { get; set; }
        = "crop_access_token";
    public string RefreshTokenCookieName { get; set; }
        = "crop_refresh_token";
    public bool SecureOnly { get; set; } = true;
    public string SameSite { get; set; } = "Strict";
}