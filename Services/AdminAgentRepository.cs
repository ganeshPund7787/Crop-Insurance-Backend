using Authentication.Data;
using Authentication.Models.Enums;
using Backend_Crop_Insurrance.DTOs.Admin;
using Backend_Crop_Insurrance.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend_Crop_Insurrance.Services
{
    public class AdminAgentRepository : IAdminAgentRepository
    {
        private readonly AppDbContext _context;

        public AdminAgentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AgentListItemDto>> GetAllAgentsAsync()
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.Role == UserRole.InsuranceAgent && !u.IsDeleted)
                .Include(u => u.AgentProfile)
                .OrderByDescending(u => u.CreatedAtUtc)
                .Select(u => new AgentListItemDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    AgentCode = u.AgentProfile != null ? u.AgentProfile.AgentCode : null,
                    LicenseNumber = u.AgentProfile != null ? u.AgentProfile.LicenseNumber : null,
                    AssignedDistrict = u.AgentProfile != null ? u.AgentProfile.AssignedDistrict : null,
                    IsVerified = u.AgentProfile != null && u.AgentProfile.IsVerified,
                    IsActive = u.IsActive,
                    TotalClaimsHandled = u.AgentProfile != null ? u.AgentProfile.TotalClaimsHandled : 0,
                    CreatedAtUtc = u.CreatedAtUtc,
                    LastLoginAtUtc = u.LastLoginAtUtc
                })
                .ToListAsync();
        }
    }
}
