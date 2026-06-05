namespace Backend_Crop_Insurrance.DTOs.Farmer
{
    public class AiRiskSummaryRequestDto
    {
        public string CropName { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Season { get; set; } = string.Empty;
        public decimal LandArea { get; set; }
        public string? ProblemDescription { get; set; }
    }

    public class AiRiskSummaryResponseDto
    {
        public string RiskLevel { get; set; } = string.Empty;
        public List<string> PossibleRisks { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
        public List<string> InsuranceSuggestions { get; set; } = new();
    }
}
