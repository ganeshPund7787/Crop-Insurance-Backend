namespace Authentication.DTOs.Farmer;

public class FarmerClaimSummaryDto
{
    public Guid Id { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public string CropName { get; set; } = string.Empty;
    public string FarmName { get; set; } = string.Empty;
    public string DamageType { get; set; } = string.Empty;
    public decimal EstimatedLossAmount { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public DateTime IncidentDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}