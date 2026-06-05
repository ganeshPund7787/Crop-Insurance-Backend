using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Backend_Crop_Insurrance.Application.Common;        // ✅ was AgroShield
using Backend_Crop_Insurrance.DTOs.Farmer;  // ✅ was AgroShield
using Backend_Crop_Insurrance.Interfaces; // ✅ was AgroShield
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Backend_Crop_Insurrance.Services; // ✅ was AgroShield

public class AiRiskSummaryService : IAiRiskSummaryService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AiRiskSummaryService> _logger;

    // Gemini endpoint template
    private const string GeminiEndpoint =
        "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={0}";

    public AiRiskSummaryService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<AiRiskSummaryService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public AiRiskSummaryService()
    {
    }

    public async Task<Result<AiRiskSummaryResponseDto>> GetRiskSummaryAsync(
        AiRiskSummaryRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var apiKey = _configuration["Gemini:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogError("Gemini API key is not configured.");
            return Result<AiRiskSummaryResponseDto>.Failure("AI service is not configured.");
        }

        var prompt = BuildPrompt(request);
        var requestBody = BuildGeminiRequest(prompt);
        var url = string.Format(GeminiEndpoint, apiKey);

        try
        {
            var httpResponse = await _httpClient.PostAsJsonAsync(url, requestBody, cancellationToken);
            var responseBody = await httpResponse.Content.ReadAsStringAsync();

            _logger.LogError(
                "Gemini Error: Status={Status}, Body={Body}",
                httpResponse.StatusCode,
                responseBody
            );
            httpResponse.EnsureSuccessStatusCode();

            var rawJson = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
            var parsed = ParseGeminiResponse(rawJson);

            if (parsed is null)
                return Result<AiRiskSummaryResponseDto>.Failure("Failed to parse AI response.");

            return Result<AiRiskSummaryResponseDto>.Success(parsed);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Gemini API request failed.");
            return Result<AiRiskSummaryResponseDto>.Failure("AI service is temporarily unavailable.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in AiRiskSummaryService.");
            return Result<AiRiskSummaryResponseDto>.Failure("An unexpected error occurred.");
        }
    }

    // ── Prompt builder ─────────────────────────────────────
    private static string BuildPrompt(AiRiskSummaryRequestDto r)
    {
        var sb = new StringBuilder();
        sb.AppendLine("You are an expert agricultural risk analyst for Indian farmers.");
        sb.AppendLine("Analyze the following farm situation and respond ONLY with a valid JSON object.");
        sb.AppendLine("Do NOT include markdown, code fences, or any explanation outside the JSON.");
        sb.AppendLine();
        sb.AppendLine("Farm Details:");
        sb.AppendLine($"- Crop: {r.CropName}");
        sb.AppendLine($"- District: {r.District}");
        sb.AppendLine($"- Season: {r.Season}");
        sb.AppendLine($"- Land Area: {r.LandArea} acres");
        if (!string.IsNullOrWhiteSpace(r.ProblemDescription))
            sb.AppendLine($"- Problem Description: {r.ProblemDescription}");
        sb.AppendLine();
        sb.AppendLine("Respond with this exact JSON structure:");
        sb.AppendLine("""
        {
          "riskLevel": "Low | Medium | High | Critical",
          "possibleRisks": ["risk1", "risk2", "risk3"],
          "recommendations": ["recommendation1", "recommendation2", "recommendation3"],
          "insuranceSuggestions": ["suggestion1", "suggestion2"]
        }
        """);
        // Fix 3: in BuildPrompt, replace the Rules section at the bottom
        sb.AppendLine("Rules:");
        sb.AppendLine("- riskLevel: exactly one of Low, Medium, High, Critical");
        sb.AppendLine("- possibleRisks: exactly 3 short risks (max 15 words each)");
        sb.AppendLine("- recommendations: exactly 3 short steps (max 20 words each)");
        sb.AppendLine("- insuranceSuggestions: exactly 2 scheme names with one-line descriptions");
        sb.AppendLine("- Keep ALL strings concise. Total response must fit within 400 tokens.");

        return sb.ToString();
    }

    // ── Gemini request body ────────────────────────────────
    // Fix 1: increase maxOutputTokens
private static object BuildGeminiRequest(string prompt) => new
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
        temperature = 0.3,
        maxOutputTokens = 2048  // ✅ was 1024 — too small, JSON was getting cut off
    }
};

    // ── Parse Gemini response ──────────────────────────────
    // Fix 2: robust JSON extraction in ParseGeminiResponse
    private AiRiskSummaryResponseDto? ParseGeminiResponse(string rawJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(rawJson);
            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrWhiteSpace(text))
            {
                _logger.LogError("Gemini returned empty text.");
                return null;
            }

            // ✅ Extract only the JSON object — handles extra prose, markdown fences
            var start = text.IndexOf('{');
            var end = text.LastIndexOf('}');

            if (start == -1 || end == -1 || end <= start)
            {
                _logger.LogError("No valid JSON object found in Gemini response. Raw: {Text}", text);
                return null;
            }

            var jsonOnly = text[start..(end + 1)].Trim();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<AiRiskSummaryResponseDto>(jsonOnly, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse Gemini JSON response.");
            return null;
        }
    }
}