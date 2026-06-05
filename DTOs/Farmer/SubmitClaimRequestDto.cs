using Authentication.Models.Enums;

namespace Authentication.DTOs.Farmer;

public class SubmitClaimRequestDto
{
    public Guid CropId { get; set; }
    public DamageType DamageType { get; set; }
    public string DamageDescription { get; set; } = string.Empty;
    public decimal EstimatedLossAmount { get; set; }
    public DateTime IncidentDate { get; set; }
}