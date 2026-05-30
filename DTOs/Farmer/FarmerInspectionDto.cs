namespace Authentication.DTOs.Farmer;

public class FarmerInspectionDto
{
    public Guid Id { get; set; }
    public string InspectionNumber { get; set; } = string.Empty;
    public DateTime ScheduledAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal? DamagePercentage { get; set; }
    public decimal? RecommendedAmount { get; set; }

    // Farmer sees findings only after inspection is completed
    public string? Findings { get; set; }
}