using Authentication.Models.Enums;

namespace Authentication.DTOs.Auth;

public class RegisterRequestDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Farmer;

    // Farmer-specific (optional)
    public string? Village { get; set; }
    public string? District { get; set; }
    public string? State { get; set; }
    public string? AadhaarNumber { get; set; }
    public decimal? TotalLandAcres { get; set; }

    // Agent-specific (optional)
    public string? AgentCode { get; set; }
    public string? LicenseNumber { get; set; }
    public string? AssignedDistrict { get; set; }
}