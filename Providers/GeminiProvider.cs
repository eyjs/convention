using LocalRAG.Interfaces;
using System.Text.Json;
using System.Text;

namespace LocalRAG.Providers;

public class GeminiProvider : ILlmProvider
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _baseUrl;
    private readonly string _apiKey;
    private readonly string _model;

    public string ProviderName => "Gemini";

    public GeminiProvider(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _baseUrl = _configuration["LlmSettings:Gemini:BaseUrl"] ?? "https://generativelanguage.googleapis.com/v1beta";
        _apiKey = _configuration["LlmSettings:Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini API key not configured");
        _model = _configuration["LlmSettings:Gemini:Model"] ?? "gemini-1.5-flash";
    }

    public async Task<string> GenerateResponseAsync(string prompt, string? context = null)
    {
        try
        {
            var systemInstruction = @"당신은 친절하고 전문적인 AI 어시스턴트입니다. 
모든 답변은 반드시 한국어로만 작성해야 합니다.
답변 시 다음 규칙을 따르세요:
1. 모든 답변은 한국어로만 작성
2. 정확하고 명확한 정보 제공
3. 컨텍스트에 있는 정보를 기반으로 답변
4. 컨텍스트에 없는 내용은 추측하지 말 것";

            var fullPrompt = context != null 
                ? $"컨텍스트: {context}\n\n질문: {prompt}\n\n한국어로 답변:"
                : $"질문: {prompt}\n\n한국어로 답변:";

            var request = new
            {
                system_instruction = new
                {
                    parts = new[]
                    {
                        new { text = systemInstruction }
                    }
                },
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = fullPrompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7,
                    topP = 0.9,
                    maxOutputTokens = 2048
                }
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                $"{_baseUrl}/models/{_model}:generateContent?key={_apiKey}", 
                content
            );
            
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
            
            var candidates = result.GetProperty("candidates");
            if (candidates.GetArrayLength() > 0)
            {
                var firstCandidate = candidates[0];
                var content_result = firstCandidate.GetProperty("content");
                var parts = content_result.GetProperty("parts");
                if (parts.GetArrayLength() > 0)
                {
                    return parts[0].GetProperty("text").GetString() ?? "답변을 생성할 수 없습니다";
                }
            }

            return "답변을 생성할 수 없습니다";
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to generate response from Gemini: {ex.Message}", ex);
        }
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        try
        {
            var request = new
            {
                model = "models/embedding-001",
                content = new
                {
                    parts = new[]
                    {
                        new { text = text }
                    }
                }
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                $"{_baseUrl}/models/embedding-001:embedContent?key={_apiKey}", 
                content
            );
            
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
            
            var embedding = result.GetProperty("embedding").GetProperty("values");
            var embeddingArray = embedding.EnumerateArray()
                .Select(x => (float)x.GetDouble())
                .ToArray();
                
            return embeddingArray;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to generate embedding from Gemini: {ex.Message}", ex);
        }
    }
}
