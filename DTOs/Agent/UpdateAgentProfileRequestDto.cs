namespace Authentication.DTOs.Agent;

public class UpdateAgentProfileRequestDto
{
    public string AssignedDistrict { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
}