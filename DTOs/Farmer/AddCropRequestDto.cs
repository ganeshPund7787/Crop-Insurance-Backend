namespace Authentication.DTOs.Farmer;

public class AddCropRequestDto
{
    public string CropName { get; set; } = string.Empty;
    public string Season { get; set; } = string.Empty;
    public decimal ExpectedYieldTons { get; set; }
    public DateTime SowingDate { get; set; }
    public DateTime ExpectedHarvestDate { get; set; }
}