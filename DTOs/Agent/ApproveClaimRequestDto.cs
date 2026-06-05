namespace Authentication.DTOs.Agent;

public class ApproveClaimRequestDto
{
    public decimal ApprovedAmount { get; set; }
    public string AgentRemarks { get; set; } = string.Empty;
}