using Backend_Crop_Insurrance.DTOs.Ai_Analysis;

namespace Backend_Crop_Insurrance.Interfaces
{
    public interface IAiAnalysisService
    {
        /// <summary>
        /// Sends a prompt to Gemini and returns the raw text response.
        /// All feature-specific methods will call this internally.
        /// </summary>
        Task<string> AskGeminiAsync(string prompt);
        Task<ClaimRiskAnalysisResponseDto> AnalyzeClaimRiskAsync(
       ClaimRiskAnalysisRequestDto request);
        Task<CropAdvisoryResponseDto> GetCropAdvisoryAsync(
       CropAdvisoryRequestDto request);

        Task<DamageReportResponseDto> SummarizeDamageReportAsync(
     DamageReportRequestDto request);
    }
}
