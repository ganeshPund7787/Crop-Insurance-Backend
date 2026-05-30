namespace Authentication.DTOs.Agent;

public class ClaimSummaryDto
{
    public Guid Id { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public string FarmerName { get; set; } = string.Empty;
    public string FarmerPhone { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string CropName { get; set; } = string.Empty;
    public string DamageType { get; set; } = string.Empty;
    public decimal EstimatedLossAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime IncidentDate { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}