using Authentication.Data;
using Authentication.DTOs.Farmer;
using Authentication.Interfaces;
using Authentication.Models;
using Authentication.Models.Enums;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Services;

public class FarmerService : IFarmerService
{
    private readonly IUserRepository _userRepository;
    private readonly IRepository<FarmerProfile> _farmerRepository;
    private readonly IRepository<Farm> _farmRepository;
    private readonly IRepository<Crop> _cropRepository;
    private readonly IMapper _mapper;

    public FarmerService(
        IUserRepository userRepository,
        IRepository<FarmerProfile> farmerRepository,
        IRepository<Farm> farmRepository,
        IRepository<Crop> cropRepository,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _farmerRepository = farmerRepository;
        _farmRepository = farmRepository;
        _cropRepository = cropRepository;
        _mapper = mapper;
    }

    // ─── Get farmer profile ────────────────────────────────────────────────
    public async Task<FarmerProfileDto> GetFarmerProfileAsync(Guid userId)
    {
        // ── Guard against empty userId ─────────────────────────────────────
        if (userId == Guid.Empty)
            throw new UnauthorizedAccessException(
                "Invalid user session. Please login again.");

        var user = await _userRepository.GetByIdWithProfileAsync(userId)
            ?? throw new KeyNotFoundException(
                $"User with ID {userId} not found.");

        if (user.Role != UserRole.Farmer)
            throw new InvalidOperationException(
                "User is not a farmer.");

        if (user.FarmerProfile is null)
            throw new KeyNotFoundException(
                "Farmer profile not found. " +
                "Ensure the user was registered as a Farmer.");

        return _mapper.Map<FarmerProfileDto>(user.FarmerProfile);
    }
    // ─── Update farmer profile ─────────────────────────────────────────────
    public async Task<FarmerProfileDto> UpdateFarmerProfileAsync(
        Guid userId,
        UpdateFarmerProfileRequestDto request)
    {
        var user = await _userRepository.GetByIdWithProfileAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        if (user.FarmerProfile is null)
            throw new KeyNotFoundException("Farmer profile not found.");

        var profile = user.FarmerProfile;

        profile.Village = request.Village.Trim();
        profile.District = request.District.Trim();
        profile.State = request.State.Trim();
        profile.TotalLandAcres = request.TotalLandAcres;

        _farmerRepository.Update(profile);
        await _farmerRepository.SaveChangesAsync();

        return _mapper.Map<FarmerProfileDto>(profile);
    }

    // ─── Get all farms for a farmer ────────────────────────────────────────
    public async Task<IEnumerable<FarmDto>> GetFarmsAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdWithProfileAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        if (user.FarmerProfile is null)
            throw new KeyNotFoundException("Farmer profile not found.");

        return _mapper.Map<IEnumerable<FarmDto>>(
            user.FarmerProfile.Farms);
    }

    // ─── Add a farm ────────────────────────────────────────────────────────
    public async Task<FarmDto> AddFarmAsync(
        Guid userId,
        AddFarmRequestDto request)
    {
        var user = await _userRepository.GetByIdWithProfileAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        if (user.FarmerProfile is null)
            throw new KeyNotFoundException("Farmer profile not found.");

        var farm = _mapper.Map<Farm>(request);
        farm.FarmerProfileId = user.FarmerProfile.Id;

        await _farmRepository.AddAsync(farm);
        await _farmRepository.SaveChangesAsync();

        return _mapper.Map<FarmDto>(farm);
    }

    // ─── Update a farm ─────────────────────────────────────────────────────
    // Validates ownership — farmer can only update their own farms
    public async Task<FarmDto> UpdateFarmAsync(
        Guid userId,
        Guid farmId,
        AddFarmRequestDto request)
    {
        var farm = await GetOwnedFarmAsync(userId, farmId);

        farm.FarmName = request.FarmName.Trim();
        farm.AreaInAcres = request.AreaInAcres;
        farm.SoilType = request.SoilType.Trim();
        farm.Location = request.Location.Trim();
        farm.Latitude = request.Latitude;
        farm.Longitude = request.Longitude;

        _farmRepository.Update(farm);
        await _farmRepository.SaveChangesAsync();

        return _mapper.Map<FarmDto>(farm);
    }

    // ─── Delete a farm ─────────────────────────────────────────────────────
    public async Task<bool> DeleteFarmAsync(Guid userId, Guid farmId)
    {
        var farm = await GetOwnedFarmAsync(userId, farmId);

        _farmRepository.Delete(farm);
        await _farmRepository.SaveChangesAsync();

        return true;
    }

    // ─── Get crops for a farm ──────────────────────────────────────────────
    public async Task<IEnumerable<CropDto>> GetCropsAsync(Guid farmId)
    {
        var crops = await _cropRepository
            .FindAsync(c => c.FarmId == farmId);

        return _mapper.Map<IEnumerable<CropDto>>(crops);
    }

    // ─── Add a crop to a farm ──────────────────────────────────────────────
    public async Task<CropDto> AddCropAsync(
        Guid userId,
        Guid farmId,
        AddCropRequestDto request)
    {
        // Validate farmer owns this farm before adding crop
        var farm = await GetOwnedFarmAsync(userId, farmId);

        var crop = _mapper.Map<Crop>(request);
        crop.FarmId = farm.Id;
        crop.Status = CropStatus.Active;

        await _cropRepository.AddAsync(crop);
        await _cropRepository.SaveChangesAsync();

        return _mapper.Map<CropDto>(crop);
    }

    // ─── Private: ownership validation ────────────────────────────────────
    // Reused across all farm operations
    // Prevents farmers accessing other farmers' data
    private async Task<Farm> GetOwnedFarmAsync(Guid userId, Guid farmId)
    {
        var user = await _userRepository.GetByIdWithProfileAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        if (user.FarmerProfile is null)
            throw new KeyNotFoundException("Farmer profile not found.");

        // Check farm belongs to this farmer specifically
        var farm = user.FarmerProfile.Farms
            .FirstOrDefault(f => f.Id == farmId)
            ?? throw new KeyNotFoundException(
                "Farm not found or does not belong to you.");

        return farm;
    }
}