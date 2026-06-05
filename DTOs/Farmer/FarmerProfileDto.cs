namespace Authentication.DTOs.Farmer;

public class FarmerProfileDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Village { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string AadhaarNumber { get; set; } = string.Empty;
    public decimal TotalLandAcres { get; set; }
    public bool IsKycVerified { get; set; }
    public IEnumerable<FarmDto> Farms { get; set; } = new List<FarmDto>();
}