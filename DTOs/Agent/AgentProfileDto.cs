namespace Authentication.DTOs.Agent;

public class AgentProfileDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string AgentCode { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string AssignedDistrict { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public int TotalClaimsHandled { get; set; }
}