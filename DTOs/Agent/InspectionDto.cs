namespace Authentication.DTOs.Agent;

public class InspectionDto
{
    public Guid Id { get; set; }
    public string InspectionNumber { get; set; } = string.Empty;
    public DateTime ScheduledAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Findings { get; set; }
    public decimal? DamagePercentage { get; set; }
    public decimal? RecommendedAmount { get; set; }
    public string? InspectorNotes { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}