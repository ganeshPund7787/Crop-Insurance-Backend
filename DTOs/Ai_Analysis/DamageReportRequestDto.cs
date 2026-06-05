namespace Backend_Crop_Insurrance.DTOs.Ai_Analysis
{
    public class DamageReportRequestDto
    {
        public string RawFindings { get; set; } = string.Empty; // Agent's raw notes
        public string CropName { get; set; } = string.Empty;
        public string DamageType { get; set; } = string.Empty;
        public decimal DamagePercentage { get; set; }
        public decimal RecommendedAmount { get; set; }
        public string FarmLocation { get; set; } = string.Empty;
        public string InspectionNumber { get; set; } = string.Empty;
        public DateTime InspectionDate { get; set; }
        public string? AgentName { get; set; }
    }
}
