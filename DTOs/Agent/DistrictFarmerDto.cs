namespace Authentication.DTOs.Agent;

public class DistrictFarmerDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Village { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public decimal TotalLandAcres { get; set; }
    public bool IsKycVerified { get; set; }
    public int TotalFarms { get; set; }
    public int TotalClaims { get; set; }
}