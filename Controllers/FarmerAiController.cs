using Backend_Crop_Insurrance.DTOs.Farmer;
using Backend_Crop_Insurrance.Helpers.Validators;
using Backend_Crop_Insurrance.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend_Crop_Insurrance.API.Controllers.Farmer;

[ApiController]
[Route("api/farmer")]
[Authorize(Roles = "Farmer")]
public class FarmerAiController : ControllerBase
{
    private readonly IAiRiskSummaryService _aiService;

    public FarmerAiController(IAiRiskSummaryService aiService)
    {
        _aiService = aiService;
    }

    /// <summary>
    /// POST /api/farmer/ai-risk-summary
    /// Analyzes crop risk using Gemini AI and returns structured risk assessment.
    /// </summary>
    [HttpPost("ai-risk-summary")]
    public async Task<IActionResult> GetAiRiskSummary(
        [FromBody] AiRiskSummaryRequestDto request,
        CancellationToken cancellationToken)
    {
        // Validate
        var validator = new AiRiskSummaryValidator();
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return BadRequest(new
            {
                success = false,
                message = "Validation failed.",
                data = (object?)null,
                errors = validation.Errors.Select(e => e.ErrorMessage).ToList()
            });
        }

        var result = await _aiService.GetRiskSummaryAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            return StatusCode(503, new
            {
                success = false,
                message = result.Error,
                data = (object?)null,
                errors = new[] { result.Error }
            });
        }

        return Ok(new
        {
            success = true,
            message = "Risk analysis completed successfully.",
            data = result.Value,
            errors = Array.Empty<string>()
        });
    }
}