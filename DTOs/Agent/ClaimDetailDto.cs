namespace Authentication.DTOs.Agent;

public class ClaimDetailDto
{
    public Guid Id { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;

    // Farmer info
    public string FarmerName { get; set; } = string.Empty;
    public string FarmerEmail { get; set; } = string.Empty;
    public string FarmerPhone { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Village { get; set; } = string.Empty;
    public string AadhaarNumber { get; set; } = string.Empty;

    // Crop info
    public string CropName { get; set; } = string.Empty;
    public string Season { get; set; } = string.Empty;
    public decimal ExpectedYieldTons { get; set; }
    public DateTime SowingDate { get; set; }

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

    public IEnumerable<InspectionDto> Inspections { get; set; }
        = new List<InspectionDto>();
}