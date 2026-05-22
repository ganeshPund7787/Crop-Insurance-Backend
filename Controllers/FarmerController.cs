using Authentication.DTOs.Farmer;
using Authentication.Helpers;
using Authentication.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Authentication.Controllers;

[ApiController]
[Route("api/farmer")]
[Authorize(Roles = "Farmer")]  // Farmer role only
public class FarmerController : ControllerBase
{
    private readonly IFarmerService _farmerService;
    private readonly ICurrentUserService _currentUserService;

    public FarmerController(
        IFarmerService farmerService,
        ICurrentUserService currentUserService)
    {
        _farmerService = farmerService;
        _currentUserService = currentUserService;
    }

    // ─── GET api/farmer/profile ────────────────────────────────────────────
    [HttpGet("profile")]
    public async Task<IActionResult> GetFarmerProfile()
    {
        var result = await _farmerService
            .GetFarmerProfileAsync(_currentUserService.UserId);

        return Ok(ApiResponse<FarmerProfileDto>.Ok(result));
    }

    // ─── PUT api/farmer/profile ────────────────────────────────────────────
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateFarmerProfile(
        [FromBody] UpdateFarmerProfileRequestDto request)
    {
        var result = await _farmerService
            .UpdateFarmerProfileAsync(_currentUserService.UserId, request);

        return Ok(ApiResponse<FarmerProfileDto>.Ok(
            result,
            "Farmer profile updated successfully."));
    }

    // ─── GET api/farmer/farms ──────────────────────────────────────────────
    [HttpGet("farms")]
    public async Task<IActionResult> GetFarms()
    {
        var result = await _farmerService
            .GetFarmsAsync(_currentUserService.UserId);

        return Ok(ApiResponse<IEnumerable<FarmDto>>.Ok(result));
    }

    // ─── POST api/farmer/farms ─────────────────────────────────────────────
    [HttpPost("farms")]
    public async Task<IActionResult> AddFarm(
        [FromBody] AddFarmRequestDto request)
    {
        var result = await _farmerService
            .AddFarmAsync(_currentUserService.UserId, request);

        return Created(
            $"api/farmer/farms/{result.Id}",
            ApiResponse<FarmDto>.Ok(result, "Farm added successfully."));
    }

    // ─── PUT api/farmer/farms/{farmId} ─────────────────────────────────────
    [HttpPut("farms/{farmId:guid}")]
    public async Task<IActionResult> UpdateFarm(
        Guid farmId,
        [FromBody] AddFarmRequestDto request)
    {
        var result = await _farmerService
            .UpdateFarmAsync(_currentUserService.UserId, farmId, request);

        return Ok(ApiResponse<FarmDto>.Ok(
            result,
            "Farm updated successfully."));
    }

    // ─── DELETE api/farmer/farms/{farmId} ──────────────────────────────────
    [HttpDelete("farms/{farmId:guid}")]
    public async Task<IActionResult> DeleteFarm(Guid farmId)
    {
        await _farmerService
            .DeleteFarmAsync(_currentUserService.UserId, farmId);

        return Ok(ApiResponse<object>.Ok(
            null!,
            "Farm deleted successfully."));
    }

    // ─── GET api/farmer/farms/{farmId}/crops ───────────────────────────────
    [HttpGet("farms/{farmId:guid}/crops")]
    public async Task<IActionResult> GetCrops(Guid farmId)
    {
        var result = await _farmerService.GetCropsAsync(farmId);

        return Ok(ApiResponse<IEnumerable<CropDto>>.Ok(result));
    }

    // ─── POST api/farmer/farms/{farmId}/crops ──────────────────────────────
    [HttpPost("farms/{farmId:guid}/crops")]
    public async Task<IActionResult> AddCrop(
        Guid farmId,
        [FromBody] AddCropRequestDto request)
    {
        var result = await _farmerService
            .AddCropAsync(_currentUserService.UserId, farmId, request);

        return Created(
            $"api/farmer/farms/{farmId}/crops/{result.Id}",
            ApiResponse<CropDto>.Ok(result, "Crop added successfully."));
    }
}