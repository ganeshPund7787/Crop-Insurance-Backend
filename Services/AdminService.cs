using Backend_Crop_Insurrance.DTOs.Admin;
using Backend_Crop_Insurrance.Interfaces;

namespace Backend_Crop_Insurrance.Services
{
    public class AdminService : IAdminService
    {
        private readonly IAdminAgentRepository _adminAgentRepository;

        public AdminService(IAdminAgentRepository adminAgentRepository)
        {
            _adminAgentRepository = adminAgentRepository;
        }

        public async Task<IEnumerable<AgentListItemDto>> GetAllAgentsAsync()
        {
            return await _adminAgentRepository.GetAllAgentsAsync();
        }
    }
}
