using Authentication.Models.Enums;

namespace Authentication.Models;

public class Crop : BaseEntity
{
    public string CropName { get; set; } = string.Empty;
    public string Season { get; set; } = string.Empty;
    public decimal ExpectedYieldTons { get; set; }
    public DateTime SowingDate { get; set; }
    public DateTime ExpectedHarvestDate { get; set; }
    public CropStatus Status { get; set; } = CropStatus.Active;

    // FK
    public Guid FarmId { get; set; }
    public Farm Farm { get; set; } = default!;
}