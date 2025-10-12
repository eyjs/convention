using LocalRAG.Interfaces;
using System.Text;
using System.Text.Json;

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

    public async Task<string> GenerateResponseAsync(string prompt, string? context = null, string? userContext = null)
    {
        try
        {
            var systemInstruction = @"You are a helpful AI assistant for Star Tour, a travel company.
Your instructions are absolute.
1. You MUST respond in Korean. Absolutely no other languages.
2. Base your answers strictly on the provided 'Context'.
3. If the context does not contain the answer, you MUST respond with '정보가 부족하여 답변할 수 없습니다.'";

            if (!string.IsNullOrEmpty(userContext))
            {
                systemInstruction += $"\nIMPORTANT: {userContext}";
            }

            var fullPrompt = context != null
                ? $"Context:\n---\n{context}\n---\nBased on the context above, answer the following question in Korean.\nQuestion: {prompt}"
                : $"Answer the following question in Korean.\nQuestion: {prompt}";

            var request = new
            {
                system_instruction = new { parts = new[] { new { text = systemInstruction } } },
                contents = new[] { new { parts = new[] { new { text = fullPrompt } } } }
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/models/{_model}:generateContent?key={_apiKey}", content);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(jsonResponse);

            var candidates = result.GetProperty("candidates");
            if (candidates.GetArrayLength() > 0)
            {
                var parts = candidates[0].GetProperty("content").GetProperty("parts");
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
        // ... (이 메서드는 변경 없습니다)
        return Array.Empty<float>();
    }
}