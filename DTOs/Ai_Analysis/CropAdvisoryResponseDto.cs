namespace Backend_Crop_Insurrance.DTOs.Ai_Analysis
{
    public class CropAdvisoryResponseDto
    {
        public string Answer { get; set; } = string.Empty;
        public List<string> KeyPoints { get; set; } = new();
        public List<string> Precautions { get; set; } = new();
        public string Disclaimer { get; set; } = string.Empty;
        public string RawAiResponse { get; set; } = string.Empty;
    }
}
