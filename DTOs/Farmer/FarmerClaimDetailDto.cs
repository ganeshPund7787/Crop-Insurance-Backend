namespace Authentication.DTOs.Farmer;

public class FarmerClaimDetailDto
{
    public Guid Id { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;

    // Crop + Farm info
    public string CropName { get; set; } = string.Empty;
    public string Season { get; set; } = string.Empty;
    public string FarmName { get; set; } = string.Empty;
    public string FarmLocation { get; set; } = string.Empty;

    // Claim info
    public string DamageType { get; set; } = string.Empty;
    public string DamageDescription { get; set; } = string.Empty;
    public decimal EstimatedLossAmount { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public DateTime IncidentDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public string? AgentRemarks { get; set; }
    public DateTime? ReviewedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    // Agent info (null if not yet assigned)
    public string? AssignedAgentName { get; set; }
    public string? AssignedAgentCode { get; set; }

    // Inspections
    public IEnumerable<FarmerInspectionDto> Inspections { get; set; }
        = new List<FarmerInspectionDto>();
}