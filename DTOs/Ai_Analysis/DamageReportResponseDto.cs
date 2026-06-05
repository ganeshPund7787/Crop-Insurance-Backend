namespace Backend_Crop_Insurrance.DTOs.Ai_Analysis
{
    public class DamageReportResponseDto
    {
        public string ExecutiveSummary { get; set; } = string.Empty;
        public string DamageAssessment { get; set; } = string.Empty;
        public string FinancialImpact { get; set; } = string.Empty;
        public string InspectorConclusion { get; set; } = string.Empty;
        public List<string> KeyObservations { get; set; } = new();
        public string RecommendedAction { get; set; } = string.Empty;
        public string RawAiResponse { get; set; } = string.Empty;
    }
}
