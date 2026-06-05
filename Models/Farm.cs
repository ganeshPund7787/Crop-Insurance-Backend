namespace Authentication.Models;

public class Farm : BaseEntity
{
    public string FarmName { get; set; } = string.Empty;
    public decimal AreaInAcres { get; set; }
    public string SoilType { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    // FK
    public Guid FarmerProfileId { get; set; }
    public FarmerProfile FarmerProfile { get; set; } = default!;

    // Navigation
    public ICollection<Crop> Crops { get; set; } = new List<Crop>();
}