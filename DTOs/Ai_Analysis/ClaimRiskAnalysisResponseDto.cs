namespace Backend_Crop_Insurrance.DTOs.Ai_Analysis
{
    public class ClaimRiskAnalysisResponseDto
    {
        public int RiskScore { get; set; }  // 0–100
        public string RiskLevel { get; set; } = string.Empty; // Low / Medium / High / Critical
        public string Recommendation { get; set; } = string.Empty; // Approve / Reject / Investigate
        public string ReasoningSummary { get; set; } = string.Empty;
        public List<string> RedFlags { get; set; } = new();
        public List<string> PositiveIndicators { get; set; } = new();
        public string RawAiResponse { get; set; } = string.Empty;
    }
}
