using Authentication.Models.Enums;

namespace Authentication.Models;

public class Inspection : BaseEntity
{
    public string InspectionNumber { get; set; } = string.Empty;
    public DateTime ScheduledAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public InspectionStatus Status { get; set; }
        = InspectionStatus.Scheduled;

    public string Location { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    // ─── Findings (filled after inspection) ───────────────────────────────
    public string? Findings { get; set; }
    public decimal? DamagePercentage { get; set; }
    public decimal? RecommendedAmount { get; set; }
    public string? InspectorNotes { get; set; }

    // ─── FK ───────────────────────────────────────────────────────────────
    public Guid ClaimId { get; set; }
    public InsuranceClaim Claim { get; set; } = default!;

    public Guid AgentProfileId { get; set; }
    public AgentProfile AgentProfile { get; set; } = default!;
}