namespace Authentication.DTOs.Agent;

public class UpdateInspectionRequestDto
{
    public string Findings { get; set; } = string.Empty;
    public decimal DamagePercentage { get; set; }
    public decimal RecommendedAmount { get; set; }
    public string InspectorNotes { get; set; } = string.Empty;
}