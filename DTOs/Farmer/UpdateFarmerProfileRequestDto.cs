namespace Authentication.DTOs.Farmer;

public class UpdateFarmerProfileRequestDto
{
    public string Village { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public decimal TotalLandAcres { get; set; }
}