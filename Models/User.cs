using Authentication.Models.Enums;

namespace Authentication.Models;

public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Farmer;
    public bool IsActive { get; set; } = true;
    public bool EmailVerified { get; set; } = false;
    public DateTime? LastLoginAtUtc { get; set; }

    // Navigation
    public ICollection<RefreshToken> RefreshTokens { get; set; }
        = new List<RefreshToken>();

    public FarmerProfile? FarmerProfile { get; set; }
    public AgentProfile? AgentProfile { get; set; }
}