using Authentication.Helpers;
using Backend_Crop_Insurrance.DTOs.Ai_Analysis;
using Backend_Crop_Insurrance.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend_Crop_Insurrance.Controllers
{
    [ApiController]
    [Route("api/ai")]
    [Authorize(Roles = "Admin")]
    public class AiAnalysisController : ControllerBase
    {
        private readonly IAiAnalysisService _aiService;
        private readonly ILogger<AiAnalysisController> _logger;

        public AiAnalysisController(
            IAiAnalysisService aiService,
            ILogger<AiAnalysisController> logger)
        {
            _aiService = aiService;
            _logger = logger;
        }

        // Feature endpoints will be added here in subsequent steps.
        // Step 2 → POST api/ai/claim-risk
        // ─── POST api/ai/claim-risk ────────────────────────────────────────────────
        [HttpPost("claim-risk")]
        public async Task<IActionResult> AnalyzeClaimRisk(
            [FromBody] ClaimRiskAnalysisRequestDto request)
        {
            _logger.LogInformation(
                "Admin requested claim risk analysis for crop: {Crop}",
                request.CropName);

            var result = await _aiService.AnalyzeClaimRiskAsync(request);

            return Ok(ApiResponse<ClaimRiskAnalysisResponseDto>.Ok(
                result, "Claim risk analysis completed."));
        }
        // Step 3 → POST api/ai/crop-advisory
        // ─── POST api/ai/crop-advisory ─────────────────────────────────────────────
        [HttpPost("crop-advisory")]
        public async Task<IActionResult> GetCropAdvisory(
            [FromBody] CropAdvisoryRequestDto request)
        {
            _logger.LogInformation(
                "Admin requested crop advisory. Question: {Question}",
                request.Question);

            var result = await _aiService.GetCropAdvisoryAsync(request);

            return Ok(ApiResponse<CropAdvisoryResponseDto>.Ok(
                result, "Crop advisory generated successfully."));
        }
        // Step 4 → POST api/ai/damage-summary
        // ─── POST api/ai/damage-summary ────────────────────────────────────────────
        [HttpPost("damage-summary")]
        public async Task<IActionResult> SummarizeDamageReport(
            [FromBody] DamageReportRequestDto request)
        {
            _logger.LogInformation(
                "Admin requested damage report summary for inspection: {InspectionNumber}",
                request.InspectionNumber);

            var result = await _aiService.SummarizeDamageReportAsync(request);

            return Ok(ApiResponse<DamageReportResponseDto>.Ok(
                result, "Damage report summarized successfully."));
        }
        // Step 5 → POST api/ai/farmer-risk
        // Step 6 → POST api/ai/policy-recommendation
    }


}
