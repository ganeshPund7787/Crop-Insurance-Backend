namespace Authentication.DTOs.Farmer;

public class AddFarmRequestDto
{
    public string FarmName { get; set; } = string.Empty;
    public decimal AreaInAcres { get; set; }
    public string SoilType { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}