using Backend_Crop_Insurrance.Configuration;
using Backend_Crop_Insurrance.DTOs.Ai_Analysis;
using Backend_Crop_Insurrance.Interfaces;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace Backend_Crop_Insurrance.Services
{
    public class AiAnalysisService : IAiAnalysisService
    {
        private readonly HttpClient _httpClient;
        private readonly GeminiSettings _settings;
        private readonly ILogger<AiAnalysisService> _logger;

        public AiAnalysisService(
            HttpClient httpClient,
            IOptions<GeminiSettings> settings,
            ILogger<AiAnalysisService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<string> AskGeminiAsync(string prompt)
        {
            var url = $"{_settings.BaseUrl}/{_settings.ModelId}:generateContent?key={_settings.ApiKey}";

            var payload = new
            {
                contents = new[]
                {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            },
                generationConfig = new
                {
                    temperature = 0.7,
                    maxOutputTokens = 1024
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation(
                "Sending prompt to Gemini model: {Model}", _settings.ModelId);

            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    "Gemini API error {Status}: {Body}",
                    response.StatusCode, errorBody);

                throw new InvalidOperationException(
                    $"Gemini API returned {response.StatusCode}. " +
                    "Please check your API key and model configuration.");
            }

            var responseBody = await response.Content.ReadAsStringAsync();

            // Parse Gemini response structure:
            // { candidates: [ { content: { parts: [ { text: "..." } ] } } ] }
            using var doc = JsonDocument.Parse(responseBody);

            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return text ?? throw new InvalidOperationException(
                "Gemini returned an empty response.");
        }

        public async Task<ClaimRiskAnalysisResponseDto> AnalyzeClaimRiskAsync(
    ClaimRiskAnalysisRequestDto request)
        {
            var prompt = $@"You are a senior crop insurance fraud analyst for an Indian agricultural insurance platform.
Analyze the following insurance claim and return a structured JSON risk assessment.

CLAIM DETAILS:
- Crop Name: {request.CropName}
- Season: {request.Season}
- Damage Type: {request.DamageType}
- Damage Description: {request.DamageDescription}
- Estimated Loss Amount: Rs.{request.EstimatedLossAmount:N2}
- Farm Location: {request.FarmLocation}
- Soil Type: {request.SoilType}
- Farm Area: {request.FarmAreaInAcres} acres
- Incident Date: {request.IncidentDate:dd MMM yyyy}
- Sowing Date: {request.SowingDate:dd MMM yyyy}
- Expected Harvest Date: {request.ExpectedHarvestDate:dd MMM yyyy}

INSTRUCTIONS:
Evaluate this claim for fraud risk, inconsistencies, and legitimacy.
Consider: damage type vs crop season alignment, estimated loss vs farm size,
incident timing vs crop lifecycle, description quality and specificity.

Return ONLY a valid JSON object with this exact structure (no markdown, no extra text):
{{
  ""riskScore"": 0,
  ""riskLevel"": ""Low or Medium or High or Critical"",
  ""recommendation"": ""Approve or Reject or Investigate"",
  ""reasoningSummary"": ""2-3 sentence explanation here"",
  ""redFlags"": [""flag1"", ""flag2""],
  ""positiveIndicators"": [""indicator1"", ""indicator2""]
}}";

            var raw = await AskGeminiAsync(prompt);

            // Strip markdown code fences if Gemini wraps in ```json ... ```
            var cleaned = raw
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            try
            {
                var parsed = JsonSerializer.Deserialize<ClaimRiskAnalysisResponseDto>(
                    cleaned,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? throw new InvalidOperationException("Null deserialization result.");

                parsed.RawAiResponse = raw;
                return parsed;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex,
                    "Failed to parse Gemini claim risk response. Raw: {Raw}", raw);

                // Graceful fallback — return raw text wrapped in response
                return new ClaimRiskAnalysisResponseDto
                {
                    RiskScore = 0,
                    RiskLevel = "Unknown",
                    Recommendation = "Investigate",
                    ReasoningSummary = "AI response could not be parsed. Review manually.",
                    RawAiResponse = raw
                };
            }
        }

        public async Task<CropAdvisoryResponseDto> GetCropAdvisoryAsync(
    CropAdvisoryRequestDto request)
        {
            // Build optional context block only if fields are provided
            var contextBlock = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(request.CropName))
                contextBlock.AppendLine($"- Crop: {request.CropName}");
            if (!string.IsNullOrWhiteSpace(request.Season))
                contextBlock.AppendLine($"- Season: {request.Season}");
            if (!string.IsNullOrWhiteSpace(request.District))
                contextBlock.AppendLine($"- District/Region: {request.District}");
            if (!string.IsNullOrWhiteSpace(request.SoilType))
                contextBlock.AppendLine($"- Soil Type: {request.SoilType}");

            var contextSection = contextBlock.Length > 0
                ? $"\nCONTEXT PROVIDED:\n{contextBlock}"
                : "\nNo additional context provided. Answer based on general Indian agriculture knowledge.";

            var prompt = $@"You are an expert agricultural advisor specializing in Indian farming,
crop insurance, and rural agronomy. You assist insurance administrators
and field agents with crop-related intelligence.

{contextSection}

QUESTION FROM ADMIN:
{request.Question}

INSTRUCTIONS:
- Answer clearly and practically for an Indian agricultural context.
- Be specific — mention crop varieties, seasons, regions where relevant.
- Keep the answer useful for insurance risk assessment purposes.

Return ONLY a valid JSON object with this exact structure (no markdown, no extra text):
{{
  ""answer"": ""clear 3-5 sentence main answer here"",
  ""keyPoints"": [""point1"", ""point2"", ""point3""],
  ""precautions"": [""precaution1"", ""precaution2""],
  ""disclaimer"": ""one sentence disclaimer about consulting local experts""
}}";

            var raw = await AskGeminiAsync(prompt);

            var cleaned = raw
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            try
            {
                var parsed = JsonSerializer.Deserialize<CropAdvisoryResponseDto>(
                    cleaned,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? throw new InvalidOperationException("Null deserialization result.");

                parsed.RawAiResponse = raw;
                return parsed;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex,
                    "Failed to parse Gemini crop advisory response. Raw: {Raw}", raw);

                return new CropAdvisoryResponseDto
                {
                    Answer = raw, // Return raw text as answer fallback
                    KeyPoints = new List<string>(),
                    Precautions = new List<string>(),
                    Disclaimer = "This response was not structured. Review manually.",
                    RawAiResponse = raw
                };
            }
        }

        public async Task<DamageReportResponseDto> SummarizeDamageReportAsync(
    DamageReportRequestDto request)
        {
            var prompt = $@"You are a professional agricultural insurance report writer for an Indian
crop insurance platform. Your job is to convert raw field inspection notes
written by insurance agents into clean, formal, professional damage assessment
reports suitable for admin review and archival.

INSPECTION DETAILS:
- Inspection Number : {request.InspectionNumber}
- Inspection Date   : {request.InspectionDate:dd MMM yyyy}
- Inspector/Agent   : {request.AgentName ?? "Field Agent"}
- Crop Name         : {request.CropName}
- Damage Type       : {request.DamageType}
- Damage Percentage : {request.DamagePercentage}%
- Recommended Payout: Rs.{request.RecommendedAmount:N2}
- Farm Location     : {request.FarmLocation}

RAW FIELD NOTES FROM AGENT:
""{request.RawFindings}""

INSTRUCTIONS:
- Convert the raw notes into a professional, formal report tone.
- Do not invent facts not present in the raw notes.
- Use precise insurance and agricultural terminology.
- Structure the output clearly for an admin audience.

Return ONLY a valid JSON object with this exact structure (no markdown, no extra text):
{{
  ""executiveSummary"": ""2 sentence high-level summary here"",
  ""damageAssessment"": ""detailed paragraph on damage observed here"",
  ""financialImpact"": ""paragraph on estimated loss and recommended payout here"",
  ""inspectorConclusion"": ""formal conclusion from inspector findings here"",
  ""keyObservations"": [""observation1"", ""observation2"", ""observation3""],
  ""recommendedAction"": ""Approve Claim or Reject Claim or Request Re-inspection or Partial Approval""
}}";

            var raw = await AskGeminiAsync(prompt);

            var cleaned = System.Text.RegularExpressions.Regex
             .Replace(raw, @"```(?:json)?", "")
             .Trim();

            try
            {
                var parsed = JsonSerializer.Deserialize<DamageReportResponseDto>(
                    cleaned,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? throw new InvalidOperationException("Null deserialization result.");

                parsed.RawAiResponse = raw;
                return parsed;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex,
                    "Failed to parse Gemini damage report response. Raw: {Raw}", raw);

                return new DamageReportResponseDto
                {
                    ExecutiveSummary = "AI response could not be structured. Raw output attached.",
                    DamageAssessment = raw,
                    FinancialImpact = string.Empty,
                    InspectorConclusion = string.Empty,
                    KeyObservations = new List<string>(),
                    RecommendedAction = "Request Re-inspection",
                    RawAiResponse = raw
                };
            }
        }
    }
}
