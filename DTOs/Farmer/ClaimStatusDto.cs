namespace Authentication.DTOs.Farmer;

public class ClaimStatusDto
{
    public Guid Id { get; set; }
    public string ClaimNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusDescription { get; set; } = string.Empty;
    public decimal EstimatedLossAmount { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? ReviewedAtUtc { get; set; }
    public bool HasInspection { get; set; }
    public string? NextStep { get; set; }
}