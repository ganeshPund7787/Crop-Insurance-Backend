using Authentication.Data;
using Authentication.DTOs.Agent;
using Authentication.Interfaces;
using Authentication.Models;
using Authentication.Models.Enums;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Services;

public class AgentService : IAgentService
{
    private readonly IUserRepository _userRepository;
    private readonly IRepository<AgentProfile> _agentRepository;
    private readonly IClaimRepository _claimRepository;
    private readonly IInspectionRepository _inspectionRepository;
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public AgentService(
        IUserRepository userRepository,
        IRepository<AgentProfile> agentRepository,
        IClaimRepository claimRepository,
        IInspectionRepository inspectionRepository,
        AppDbContext context,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _agentRepository = agentRepository;
        _claimRepository = claimRepository;
        _inspectionRepository = inspectionRepository;
        _context = context;
        _mapper = mapper;
    }

    // ─── Get agent profile ─────────────────────────────────────────────────
    public async Task<AgentProfileDto> GetAgentProfileAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdWithProfileAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        if (user.Role != UserRole.InsuranceAgent)
            throw new InvalidOperationException(
                "User is not an insurance agent.");

        if (user.AgentProfile is null)
            throw new KeyNotFoundException("Agent profile not found.");

        return _mapper.Map<AgentProfileDto>(user.AgentProfile);
    }

    // ─── Update agent profile ──────────────────────────────────────────────
    public async Task<AgentProfileDto> UpdateAgentProfileAsync(
        Guid userId,
        UpdateAgentProfileRequestDto request)
    {
        var user = await _userRepository.GetByIdWithProfileAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        if (user.AgentProfile is null)
            throw new KeyNotFoundException("Agent profile not found.");

        var profile = user.AgentProfile;
        profile.AssignedDistrict = request.AssignedDistrict.Trim();
        profile.LicenseNumber = request.LicenseNumber.Trim();

        _agentRepository.Update(profile);
        await _agentRepository.SaveChangesAsync();

        return _mapper.Map<AgentProfileDto>(profile);
    }

    // ─── Verify agent (Admin action) ───────────────────────────────────────
    public async Task<bool> VerifyAgentAsync(Guid agentId)
    {
        var profile = await _agentRepository.GetByIdAsync(agentId)
            ?? throw new KeyNotFoundException("Agent profile not found.");

        if (profile.IsVerified)
            throw new InvalidOperationException("Agent is already verified.");

        profile.IsVerified = true;

        _agentRepository.Update(profile);
        await _agentRepository.SaveChangesAsync();

        return true;
    }

    // ─── Get all claims in agent's assigned district ───────────────────────
    public async Task<IEnumerable<ClaimSummaryDto>> GetDistrictClaimsAsync(
        Guid userId)
    {
        var agent = await GetVerifiedAgentAsync(userId);

        var claims = await _claimRepository
            .GetByDistrictAsync(agent.AssignedDistrict);

        return claims.Select(c => new ClaimSummaryDto
        {
            Id = c.Id,
            ClaimNumber = c.ClaimNumber,
            FarmerName = c.FarmerProfile.User.FullName,
            FarmerPhone = c.FarmerProfile.User.PhoneNumber,
            District = c.FarmerProfile.District,
            CropName = c.Crop.CropName,
            DamageType = c.DamageType.ToString(),
            EstimatedLossAmount = c.EstimatedLossAmount,
            Status = c.Status.ToString(),
            IncidentDate = c.IncidentDate,
            CreatedAtUtc = c.CreatedAtUtc
        });
    }

    // ─── Get single claim detail ───────────────────────────────────────────
    public async Task<ClaimDetailDto> GetClaimDetailAsync(
        Guid userId,
        Guid claimId)
    {
        var agent = await GetVerifiedAgentAsync(userId);
        var InsuranceClaim = await GetDistrictClaimAsync(claimId, agent);

        return MapToClaimDetail(InsuranceClaim);
    }

    // ─── Assign claim to self ──────────────────────────────────────────────
    public async Task<ClaimDetailDto> AssignClaimAsync(
        Guid userId,
        Guid claimId)
    {
        var agent = await GetVerifiedAgentAsync(userId);
        var InsuranceClaim = await GetDistrictClaimAsync(claimId, agent);

        if (InsuranceClaim.Status != ClaimStatus.Submitted)
            throw new InvalidOperationException(
                "Only submitted claims can be assigned.");

        if (InsuranceClaim.AgentProfileId is not null)
            throw new InvalidOperationException(
                "Claim is already assigned to an agent.");

        InsuranceClaim.AgentProfileId = agent.Id;
        InsuranceClaim.Status = ClaimStatus.UnderReview;
        InsuranceClaim.ReviewedAtUtc = DateTime.UtcNow;

        await _claimRepository.SaveChangesAsync();

        return MapToClaimDetail(
            await _claimRepository.GetByIdWithDetailsAsync(claimId)!);
    }

    // ─── Create inspection for a claim ────────────────────────────────────
    public async Task<InspectionDto> CreateInspectionAsync(
        Guid userId,
        Guid claimId,
        CreateInspectionRequestDto request)
    {
        var agent = await GetVerifiedAgentAsync(userId);
        var InsuranceClaim = await GetAssignedClaimAsync(claimId, agent);

        if (InsuranceClaim.Status == ClaimStatus.Approved ||
            InsuranceClaim.Status == ClaimStatus.Rejected)
            throw new InvalidOperationException(
                "Cannot create inspection for a closed claim.");

        var inspectionNumber = await _inspectionRepository
            .GenerateInspectionNumberAsync();

        var inspection = new Inspection
        {
            InspectionNumber = inspectionNumber,
            ClaimId = InsuranceClaim.Id,
            AgentProfileId = agent.Id,
            ScheduledAtUtc = request.ScheduledAtUtc,
            Location = request.Location.Trim(),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Status = InspectionStatus.Scheduled
        };

        // Update claim status
        InsuranceClaim.Status = ClaimStatus.InspectionScheduled;

        await _inspectionRepository.AddAsync(inspection);
        await _inspectionRepository.SaveChangesAsync();
        await _claimRepository.SaveChangesAsync();

        return MapToInspectionDto(inspection);
    }

    // ─── Update inspection with findings ──────────────────────────────────
    public async Task<InspectionDto> UpdateInspectionAsync(
        Guid userId,
        Guid inspectionId,
        UpdateInspectionRequestDto request)
    {
        var agent = await GetVerifiedAgentAsync(userId);

        var inspection = await _context.Inspections
            .Include(x => x.Claim)
            .FirstOrDefaultAsync(x =>
                x.Id == inspectionId &&
                x.AgentProfileId == agent.Id &&
                !x.IsDeleted)
            ?? throw new KeyNotFoundException("Inspection not found.");

        if (inspection.Status == InspectionStatus.Cancelled)
            throw new InvalidOperationException(
                "Cannot update a cancelled inspection.");

        inspection.Findings = request.Findings.Trim();
        inspection.DamagePercentage = request.DamagePercentage;
        inspection.RecommendedAmount = request.RecommendedAmount;
        inspection.InspectorNotes = request.InspectorNotes.Trim();
        inspection.Status = InspectionStatus.Completed;
        inspection.CompletedAtUtc = DateTime.UtcNow;

        // Update claim status
        inspection.Claim.Status = ClaimStatus.InspectionDone;

        await _inspectionRepository.SaveChangesAsync();

        return MapToInspectionDto(inspection);
    }

    // ─── Approve claim ─────────────────────────────────────────────────────
    public async Task<ClaimDetailDto> ApproveClaimAsync(
        Guid userId,
        Guid claimId,
        ApproveClaimRequestDto request)
    {
        var agent = await GetVerifiedAgentAsync(userId);
        var InsuranceClaim = await GetAssignedClaimAsync(claimId, agent);

        if (InsuranceClaim.Status != ClaimStatus.InspectionDone &&
            InsuranceClaim.Status != ClaimStatus.UnderReview)
            throw new InvalidOperationException(
                "Claim must be under review or inspection done to approve.");

        if (request.ApprovedAmount <= 0)
            throw new InvalidOperationException(
                "Approved amount must be greater than zero.");

        if (request.ApprovedAmount > InsuranceClaim.EstimatedLossAmount)
            throw new InvalidOperationException(
                "Approved amount cannot exceed estimated loss amount.");

        InsuranceClaim.Status = ClaimStatus.Approved;
        InsuranceClaim.ApprovedAmount = request.ApprovedAmount;
        InsuranceClaim.AgentRemarks = request.AgentRemarks.Trim();
        InsuranceClaim.ReviewedAtUtc = DateTime.UtcNow;

        // Increment agent's handled claims count
        agent.TotalClaimsHandled++;

        _agentRepository.Update(agent);
        await _claimRepository.SaveChangesAsync();

        return MapToClaimDetail(
            await _claimRepository.GetByIdWithDetailsAsync(claimId)!);
    }

    // ─── Reject claim ──────────────────────────────────────────────────────
    public async Task<ClaimDetailDto> RejectClaimAsync(
        Guid userId,
        Guid claimId,
        RejectClaimRequestDto request)
    {
        var agent = await GetVerifiedAgentAsync(userId);
        var claim = await GetAssignedClaimAsync(claimId, agent);

        if (claim.Status == ClaimStatus.Approved ||
            claim.Status == ClaimStatus.Rejected)
            throw new InvalidOperationException(
                "Claim is already closed.");

        claim.Status = ClaimStatus.Rejected;
        claim.RejectionReason = request.RejectionReason.Trim();
        claim.ReviewedAtUtc = DateTime.UtcNow;

        agent.TotalClaimsHandled++;

        _agentRepository.Update(agent);
        await _claimRepository.SaveChangesAsync();

        return MapToClaimDetail(
            await _claimRepository.GetByIdWithDetailsAsync(claimId)!);
    }

    // ─── Get all my inspections ────────────────────────────────────────────
    public async Task<IEnumerable<InspectionDto>> GetMyInspectionsAsync(
        Guid userId)
    {
        var agent = await GetVerifiedAgentAsync(userId);

        var inspections = await _inspectionRepository
            .GetByAgentIdAsync(agent.Id);

        return inspections.Select(MapToInspectionDto);
    }

    // ─── Get farmers in assigned district ─────────────────────────────────
    public async Task<IEnumerable<DistrictFarmerDto>> GetDistrictFarmersAsync(
        Guid userId)
    {
        var agent = await GetVerifiedAgentAsync(userId);

        var farmers = await _context.FarmerProfiles
            .Include(x => x.User)
            .Include(x => x.Farms)
            .Include(x => x.Claims)
            .Where(x =>
                x.District == agent.AssignedDistrict &&
                !x.IsDeleted)
            .OrderBy(x => x.User.FullName)
            .ToListAsync();

        return farmers.Select(f => new DistrictFarmerDto
        {
            UserId = f.UserId,
            FullName = f.User.FullName,
            Email = f.User.Email,
            PhoneNumber = f.User.PhoneNumber,
            Village = f.Village,
            District = f.District,
            TotalLandAcres = f.TotalLandAcres,
            IsKycVerified = f.IsKycVerified,
            TotalFarms = f.Farms.Count(x => !x.IsDeleted),
            TotalClaims = f.Claims.Count(x => !x.IsDeleted)
        });
    }

    // ─── Private Helpers ───────────────────────────────────────────────────

    private async Task<AgentProfile> GetVerifiedAgentAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdWithProfileAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        if (user.AgentProfile is null)
            throw new KeyNotFoundException("Agent profile not found.");

        if (!user.AgentProfile.IsVerified)
            throw new InvalidOperationException(
                "Your agent account is not yet verified. " +
                "Contact admin.");

        return user.AgentProfile;
    }

    private async Task<InsuranceClaim> GetDistrictClaimAsync(
        Guid claimId,
        AgentProfile agent)
    {
        var claim = await _claimRepository
            .GetByIdWithDetailsAsync(claimId)
            ?? throw new KeyNotFoundException("Claim not found.");

        if (claim.FarmerProfile.District != agent.AssignedDistrict)
            throw new InvalidOperationException(
                "This claim is not in your assigned district.");

        return claim;
    }

    private async Task<InsuranceClaim> GetAssignedClaimAsync(
        Guid claimId,
        AgentProfile agent)
    {
        var claim = await _claimRepository
            .GetByIdWithDetailsAsync(claimId)
            ?? throw new KeyNotFoundException("Claim not found.");

        if (claim.AgentProfileId != agent.Id)
            throw new InvalidOperationException(
                "This claim is not assigned to you.");

        return claim;
    }

    private static ClaimDetailDto MapToClaimDetail(InsuranceClaim c)
    {
        return new ClaimDetailDto
        {
            Id = c.Id,
            ClaimNumber = c.ClaimNumber,
            FarmerName = c.FarmerProfile.User.FullName,
            FarmerEmail = c.FarmerProfile.User.Email,
            FarmerPhone = c.FarmerProfile.User.PhoneNumber,
            District = c.FarmerProfile.District,
            Village = c.FarmerProfile.Village,
            AadhaarNumber = c.FarmerProfile.AadhaarNumber,
            CropName = c.Crop.CropName,
            Season = c.Crop.Season,
            ExpectedYieldTons = c.Crop.ExpectedYieldTons,
            SowingDate = c.Crop.SowingDate,
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
            Inspections = c.Inspections
                .Where(i => !i.IsDeleted)
                .Select(MapToInspectionDto)
        };
    }

    private static InspectionDto MapToInspectionDto(Inspection i)
    {
        return new InspectionDto
        {
            Id = i.Id,
            InspectionNumber = i.InspectionNumber,
            ScheduledAtUtc = i.ScheduledAtUtc,
            CompletedAtUtc = i.CompletedAtUtc,
            Status = i.Status.ToString(),
            Location = i.Location,
            Latitude = i.Latitude,
            Longitude = i.Longitude,
            Findings = i.Findings,
            DamagePercentage = i.DamagePercentage,
            RecommendedAmount = i.RecommendedAmount,
            InspectorNotes = i.InspectorNotes,
            ClaimNumber = i.Claim?.ClaimNumber ?? string.Empty,
            CreatedAtUtc = i.CreatedAtUtc
        };
    }
}