namespace Authentication.Helpers;

// ─── BCrypt wrapper ────────────────────────────────────────────────────────
// Work factor 12 = ~300ms per hash on modern hardware
// Fast enough for users, brutal for attackers brute-forcing
// Never store plain passwords — always hash before touching DB
public static class PasswordHelper
{
    private const int WorkFactor = 12;

    public static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(
            password,
            WorkFactor
        );
    }

    public static bool VerifyPassword(
        string password,
        string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }

    public static bool IsStrongPassword(string password)
    {
        // Min 8 chars, 1 upper, 1 lower, 1 digit, 1 special
        if (password.Length < 8) return false;
        if (!password.Any(char.IsUpper)) return false;
        if (!password.Any(char.IsLower)) return false;
        if (!password.Any(char.IsDigit)) return false;
        if (!password.Any(c => !char.IsLetterOrDigit(c))) return false;
        return true;
    }
}