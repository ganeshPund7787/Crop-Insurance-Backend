namespace Backend_Crop_Insurrance.DTOs.Admin
{
    public class AgentListItemDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? AgentCode { get; set; }
        public string? LicenseNumber { get; set; }
        public string? AssignedDistrict { get; set; }
        public bool IsVerified { get; set; }
        public bool IsActive { get; set; }
        public int TotalClaimsHandled { get; set; }
        public DateTime? CreatedAtUtc { get; set; }
        public DateTime? LastLoginAtUtc { get; set; }
    }
}
