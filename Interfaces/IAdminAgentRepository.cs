using Backend_Crop_Insurrance.DTOs.Admin;

namespace Backend_Crop_Insurrance.Interfaces
{
    public interface IAdminAgentRepository
    {
        Task<IEnumerable<AgentListItemDto>> GetAllAgentsAsync();
    }
}
