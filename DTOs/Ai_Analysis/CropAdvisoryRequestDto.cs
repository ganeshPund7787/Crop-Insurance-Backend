namespace Backend_Crop_Insurrance.DTOs.Ai_Analysis
{
    public class CropAdvisoryRequestDto
    {
        public string Question { get; set; } = string.Empty; // Free-text question
        public string? CropName { get; set; }                 // Optional context
        public string? Season { get; set; }                 // Optional context
        public string? District { get; set; }                 // Optional context
        public string? SoilType { get; set; }                 // Optional context
    }
}
