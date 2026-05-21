namespace Authentication.Models;

public class FarmerProfile : BaseEntity
{
    public string Village { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string AadhaarNumber { get; set; } = string.Empty;
    public decimal TotalLandAcres { get; set; }
    public bool IsKycVerified { get; set; } = false;

    // FK
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    // Navigation
    public ICollection<Farm> Farms { get; set; } = new List<Farm>();
}