using Authentication.DTOs.Farmer;

namespace Authentication.Interfaces;

public interface IFarmerService
{
    Task<FarmerProfileDto> GetFarmerProfileAsync(Guid userId);
    Task<FarmerProfileDto> UpdateFarmerProfileAsync(Guid userId, UpdateFarmerProfileRequestDto request);
    Task<FarmDto> AddFarmAsync(Guid userId, AddFarmRequestDto request);
    Task<IEnumerable<FarmDto>> GetFarmsAsync(Guid userId);
    Task<FarmDto> UpdateFarmAsync(Guid userId, Guid farmId, AddFarmRequestDto request);
    Task<bool> DeleteFarmAsync(Guid userId, Guid farmId);
    Task<CropDto> AddCropAsync(Guid userId, Guid farmId, AddCropRequestDto request);
    Task<IEnumerable<CropDto>> GetCropsAsync(Guid farmId);
}