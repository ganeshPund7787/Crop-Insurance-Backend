namespace Backend_Crop_Insurrance.DTOs.Ai_Analysis
{
    public class ClaimRiskAnalysisRequestDto
    {
        public string CropName { get; set; } = string.Empty;
        public string Season { get; set; } = string.Empty;
        public string DamageType { get; set; } = string.Empty;
        public string DamageDescription { get; set; } = string.Empty;
        public decimal EstimatedLossAmount { get; set; }
        public string FarmLocation { get; set; } = string.Empty;
        public string SoilType { get; set; } = string.Empty;
        public decimal FarmAreaInAcres { get; set; }
        public DateTime IncidentDate { get; set; }
        public DateTime SowingDate { get; set; }
        public DateTime ExpectedHarvestDate { get; set; }
    }
}
