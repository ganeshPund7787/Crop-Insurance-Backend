namespace Authentication.Models;

public class AgentProfile : BaseEntity
{
    public string AgentCode { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string AssignedDistrict { get; set; } = string.Empty;
    public bool IsVerified { get; set; } = false;
    public int TotalClaimsHandled { get; set; } = 0;

    // FK
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;

    // Navigation
    public ICollection<InsuranceClaim> Claims { get; set; }
        = new List<InsuranceClaim>();
    public ICollection<Inspection> Inspections { get; set; }
        = new List<Inspection>();
}