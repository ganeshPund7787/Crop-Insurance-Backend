using Authentication.Models.Enums;

namespace Authentication.Models;

public class InsuranceClaim : BaseEntity
{
    public string ClaimNumber { get; set; } = string.Empty;
    public DamageType DamageType { get; set; }
    public string DamageDescription { get; set; } = string.Empty;
    public decimal EstimatedLossAmount { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public DateTime IncidentDate { get; set; }
    public ClaimStatus Status { get; set; } = ClaimStatus.Submitted;
    public string? RejectionReason { get; set; }
    public string? AgentRemarks { get; set; }
    public DateTime? ReviewedAtUtc { get; set; }

    // ─── FK to Farmer's Crop ───────────────────────────────────────────────
    public Guid CropId { get; set; }
    public Crop Crop { get; set; } = default!;

    // ─── FK to Farmer ──────────────────────────────────────────────────────
    public Guid FarmerProfileId { get; set; }
    public FarmerProfile FarmerProfile { get; set; } = default!;

    // ─── FK to Agent (nullable — not assigned yet on submission) ──────────
    public Guid? AgentProfileId { get; set; }
    public AgentProfile? AgentProfile { get; set; }

    // ─── Navigation ────────────────────────────────────────────────────────
    public ICollection<Inspection> Inspections { get; set; }
        = new List<Inspection>();
}