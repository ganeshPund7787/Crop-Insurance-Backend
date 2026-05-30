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
    private readonly IClaimRepository _claimRepository;
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public FarmerService(
        IUserRepository userRepository,
        IRepository<FarmerProfile> farmerRepository,
        IRepository<Farm> farmRepository,
        IRepository<Crop> cropRepository,
        IClaimRepository claimRepository,
        AppDbContext context,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _farmerRepository = farmerRepository;
        _farmRepository = farmRepository;
        _cropRepository = cropRepository;
        _claimRepository = claimRepository;
        _context = context;
        _mapper = mapper;
    }

    // ─── Get farmer profile ────────────────────────────────────────────────
    public async Task<FarmerProfileDto> GetFarmerProfileAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new UnauthorizedAccessException(
                "Invalid user session. Please login again.");

        var user = await _userRepository.GetByIdWithProfileAsync(userId)
            ?? throw new KeyNotFoundException(
                $"User with ID {userId} not found.");

        if (user.Role != UserRole.Farmer)
            throw new InvalidOperationException("User is not a farmer.");

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

    // ─── Get all farms ─────────────────────────────────────────────────────
    public async Task<IEnumerable<FarmDto>> GetFarmsAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdWithProfileAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        if (user.FarmerProfile is null)
            throw new KeyNotFoundException("Farmer profile not found.");

        return _mapper.Map<IEnumerable<FarmDto>>(user.FarmerProfile.Farms);
    }

    // ─── Add farm ──────────────────────────────────────────────────────────
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

    // ─── Update farm ───────────────────────────────────────────────────────
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

    // ─── Delete farm ───────────────────────────────────────────────────────
    public async Task<bool> DeleteFarmAsync(Guid userId, Guid farmId)
    {
        var farm = await GetOwnedFarmAsync(userId, farmId);

        _farmRepository.Delete(farm);
        await _farmRepository.SaveChangesAsync();

        return true;
    }

    // ─── Get crops ─────────────────────────────────────────────────────────
    public async Task<IEnumerable<CropDto>> GetCropsAsync(Guid farmId)
    {
        var crops = await _cropRepository
            .FindAsync(c => c.FarmId == farmId);

        return _mapper.Map<IEnumerable<CropDto>>(crops);
    }

    // ─── Add crop ──────────────────────────────────────────────────────────
    public async Task<CropDto> AddCropAsync(
        Guid userId,
        Guid farmId,
        AddCropRequestDto request)
    {
        var farm = await GetOwnedFarmAsync(userId, farmId);

        var crop = _mapper.Map<Crop>(request);
        crop.FarmId = farm.Id;
        crop.Status = CropStatus.Active;

        await _cropRepository.AddAsync(crop);
        await _cropRepository.SaveChangesAsync();

        return _mapper.Map<CropDto>(crop);
    }

    // ══════════════════════════════════════════════════════════════════════
    // CLAIMS
    // ══════════════════════════════════════════════════════════════════════

    // ─── Submit claim ──────────────────────────────────────────────────────
    public async Task<FarmerClaimDetailDto> SubmitClaimAsync(
        Guid userId,
        SubmitClaimRequestDto request)
    {
        var farmerProfile = await GetFarmerProfileEntityAsync(userId);

        // ── Validate crop belongs to this farmer ───────────────────────────
        var crop = await _context.Crops
            .Include(c => c.Farm)
            .FirstOrDefaultAsync(c =>
                c.Id == request.CropId &&
                c.Farm.FarmerProfileId == farmerProfile.Id &&
                !c.IsDeleted)
            ?? throw new KeyNotFoundException(
                "Crop not found or does not belong to you.");

        // ── Prevent duplicate active claim on same crop ────────────────────
        var existingActiveClaim = await _context.Claims
            .AnyAsync(c =>
                c.CropId == request.CropId &&
                c.FarmerProfileId == farmerProfile.Id &&
                c.Status != ClaimStatus.Rejected &&
                c.Status != ClaimStatus.Cancelled &&
                !c.IsDeleted);

        if (existingActiveClaim)
            throw new InvalidOperationException(
                "An active claim already exists for this crop. " +
                "Wait for it to be resolved before submitting a new one.");

        // ── Validate incident date is within crop sowing period ────────────
        if (request.IncidentDate < crop.SowingDate)
            throw new InvalidOperationException(
                "Incident date cannot be before the crop sowing date.");

        var claimNumber = await _claimRepository.GenerateClaimNumberAsync();

        var claim = new InsuranceClaim
        {
            ClaimNumber = claimNumber,
            CropId = crop.Id,
            FarmerProfileId = farmerProfile.Id,
            DamageType = request.DamageType,
            DamageDescription = request.DamageDescription.Trim(),
            EstimatedLossAmount = request.EstimatedLossAmount,
            IncidentDate = request.IncidentDate,
            Status = ClaimStatus.Submitted
        };

        await _claimRepository.AddAsync(claim);
        await _claimRepository.SaveChangesAsync();

        // Reload with full details for response
        var created = await _claimRepository
            .GetByIdWithDetailsAsync(claim.Id);

        return MapToFarmerClaimDetail(created!);
    }

    // ─── Get all my claims ─────────────────────────────────────────────────
    public async Task<IEnumerable<FarmerClaimSummaryDto>> GetMyClaimsAsync(
        Guid userId)
    {
        var farmerProfile = await GetFarmerProfileEntityAsync(userId);

        var claims = await _claimRepository
            .GetByFarmerIdAsync(farmerProfile.Id);

        return claims.Select(c => new FarmerClaimSummaryDto
        {
            Id = c.Id,
            ClaimNumber = c.ClaimNumber,
            CropName = c.Crop.CropName,
            FarmName = c.Crop.Farm.FarmName,
            DamageType = c.DamageType.ToString(),
            EstimatedLossAmount = c.EstimatedLossAmount,
            ApprovedAmount = c.ApprovedAmount,
            IncidentDate = c.IncidentDate,
            Status = c.Status.ToString(),
            CreatedAtUtc = c.CreatedAtUtc
        });
    }

    // ─── Get claim detail ──────────────────────────────────────────────────
    public async Task<FarmerClaimDetailDto> GetClaimDetailAsync(
        Guid userId,
        Guid claimId)
    {
        var farmerProfile = await GetFarmerProfileEntityAsync(userId);
        var claim = await GetOwnedClaimAsync(claimId, farmerProfile.Id);

        return MapToFarmerClaimDetail(claim);
    }

    // ─── Track claim status ────────────────────────────────────────────────
    public async Task<ClaimStatusDto> GetClaimStatusAsync(
        Guid userId,
        Guid claimId)
    {
        var farmerProfile = await GetFarmerProfileEntityAsync(userId);
        var claim = await GetOwnedClaimAsync(claimId, farmerProfile.Id);

        return new ClaimStatusDto
        {
            Id = claim.Id,
            ClaimNumber = claim.ClaimNumber,
            Status = claim.Status.ToString(),
            StatusDescription = GetStatusDescription(claim.Status),
            EstimatedLossAmount = claim.EstimatedLossAmount,
            ApprovedAmount = claim.ApprovedAmount,
            RejectionReason = claim.RejectionReason,
            ReviewedAtUtc = claim.ReviewedAtUtc,
            HasInspection = claim.Inspections.Any(i => !i.IsDeleted),
            NextStep = GetNextStep(claim.Status)
        };
    }

    // ─── Cancel claim ──────────────────────────────────────────────────────
    public async Task<bool> CancelClaimAsync(Guid userId, Guid claimId)
    {
        var farmerProfile = await GetFarmerProfileEntityAsync(userId);
        var claim = await GetOwnedClaimAsync(claimId, farmerProfile.Id);

        // Can only cancel if not yet under active review
        if (claim.Status == ClaimStatus.Approved ||
            claim.Status == ClaimStatus.Rejected ||
            claim.Status == ClaimStatus.Cancelled)
            throw new InvalidOperationException(
                $"Cannot cancel a claim with status '{claim.Status}'.");

        if (claim.Status == ClaimStatus.InspectionDone)
            throw new InvalidOperationException(
                "Cannot cancel a claim after inspection is completed.");

        claim.Status = ClaimStatus.Cancelled;

        await _claimRepository.SaveChangesAsync();

        return true;
    }

    // ══════════════════════════════════════════════════════════════════════
    // PRIVATE HELPERS
    // ══════════════════════════════════════════════════════════════════════

    private async Task<Farm> GetOwnedFarmAsync(Guid userId, Guid farmId)
    {
        var user = await _userRepository.GetByIdWithProfileAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        if (user.FarmerProfile is null)
            throw new KeyNotFoundException("Farmer profile not found.");

        return user.FarmerProfile.Farms
            .FirstOrDefault(f => f.Id == farmId)
            ?? throw new KeyNotFoundException(
                "Farm not found or does not belong to you.");
    }

    private async Task<FarmerProfile> GetFarmerProfileEntityAsync(
        Guid userId)
    {
        if (userId == Guid.Empty)
            throw new UnauthorizedAccessException(
                "Invalid user session. Please login again.");

        var user = await _userRepository.GetByIdWithProfileAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        if (user.Role != UserRole.Farmer)
            throw new InvalidOperationException("User is not a farmer.");

        return user.FarmerProfile
            ?? throw new KeyNotFoundException("Farmer profile not found.");
    }

    private async Task<InsuranceClaim> GetOwnedClaimAsync(
        Guid claimId,
        Guid farmerProfileId)
    {
        var claim = await _claimRepository.GetByIdWithDetailsAsync(claimId)
            ?? throw new KeyNotFoundException("Claim not found.");

        if (claim.FarmerProfileId != farmerProfileId)
            throw new InvalidOperationException(
                "This claim does not belong to you.");

        return claim;
    }

    // ─── Map to farmer-facing claim detail ─────────────────────────────────
    // Farmer sees limited agent info — no internal agent details
    private static FarmerClaimDetailDto MapToFarmerClaimDetail(
        InsuranceClaim c)
    {
        return new FarmerClaimDetailDto
        {
            Id = c.Id,
            ClaimNumber = c.ClaimNumber,
            CropName = c.Crop.CropName,
            Season = c.Crop.Season,
            FarmName = c.Crop.Farm.FarmName,
            FarmLocation = c.Crop.Farm.Location,
            DamageType = c.DamageType.ToString(),
            DamageDescription = c.DamageDescription,
            EstimatedLossAmount = c.EstimatedLossAmount,
            ApprovedAmount = c.ApprovedAmount,
            IncidentDate = c.IncidentDate,
            Status = c.Status.ToString(),
            RejectionReason = c.RejectionReason,
            AgentRemarks = c.AgentRemarks,
            ReviewedAtUtc = c.ReviewedAtUtc,
            CreatedAtUtc = c.CreatedAtUtc,

            // Agent info — only name and code, no sensitive details
            AssignedAgentName = c.AgentProfile?.User?.FullName,
            AssignedAgentCode = c.AgentProfile?.AgentCode,

            // Only show completed inspections to farmer
            Inspections = c.Inspections
                .Where(i => !i.IsDeleted)
                .Select(i => new FarmerInspectionDto
                {
                    Id = i.Id,
                    InspectionNumber = i.InspectionNumber,
                    ScheduledAtUtc = i.ScheduledAtUtc,
                    CompletedAtUtc = i.CompletedAtUtc,
                    Status = i.Status.ToString(),
                    Location = i.Location,
                    DamagePercentage = i.DamagePercentage,
                    RecommendedAmount = i.RecommendedAmount,
                    // Findings only visible after inspection completed
                    Findings = i.Status == InspectionStatus.Completed
                        ? i.Findings
                        : null
                })
        };
    }

    // ─── Human-readable status descriptions ───────────────────────────────
    private static string GetStatusDescription(ClaimStatus status)
    {
        return status switch
        {
            ClaimStatus.Submitted =>
                "Your claim has been submitted and is waiting for an agent to review.",
            ClaimStatus.UnderReview =>
                "An agent has been assigned and is reviewing your claim.",
            ClaimStatus.InspectionScheduled =>
                "A field inspection has been scheduled for your farm.",
            ClaimStatus.InspectionDone =>
                "Field inspection is complete. Agent is preparing the report.",
            ClaimStatus.Approved =>
                "Your claim has been approved.",
            ClaimStatus.Rejected =>
                "Your claim has been rejected.",
            ClaimStatus.Cancelled =>
                "This claim was cancelled.",
            _ => "Unknown status."
        };
    }

    // ─── What farmer should do next ────────────────────────────────────────
    private static string? GetNextStep(ClaimStatus status)
    {
        return status switch
        {
            ClaimStatus.Submitted =>
                "Wait for an agent to be assigned. " +
                "You will be notified when review begins.",
            ClaimStatus.UnderReview =>
                "Agent is reviewing your claim. " +
                "An inspection may be scheduled soon.",
            ClaimStatus.InspectionScheduled =>
                "Please ensure farm access is available on the scheduled date.",
            ClaimStatus.InspectionDone =>
                "Inspection complete. Awaiting final decision.",
            ClaimStatus.Approved =>
                "Claim approved. " +
                "Contact your agent for payout processing details.",
            ClaimStatus.Rejected =>
                "Review the rejection reason. " +
                "You may submit a new claim with corrected information.",
            ClaimStatus.Cancelled => null,
            _ => null
        };
    }
}