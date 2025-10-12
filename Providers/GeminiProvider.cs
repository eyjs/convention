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
            var systemInstruction = @"You are a helpful AI assistant for Star Tour. Your instructions are absolute.
1. You MUST respond in Korean.
2. Base your answers strictly on the provided 'Context'.
3. If the context does not contain the answer, you MUST respond with '정보가 부족하여 답변할 수 없습니다.'";

            if (!string.IsNullOrEmpty(userContext))
            {
                systemInstruction += $"\nIMPORTANT: {userContext}";
            }

            var fullPrompt = context != null
                ? $"Context:\n---\n{context}\n---\nBased on the context, answer the following question in Korean.\nQuestion: {prompt}"
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

    public async Task<string> ClassifyIntentAsync(string question)
    {
        try
        {
            var prompt = @$"Your task is to classify the user's question into one of the following categories: personal_info, personal_schedule, general_query.
Respond with ONLY the category name and nothing else.

- 'personal_info': Question about the user themselves (e.g., 'who am I', 'what is my info', '난 누구야', '내 정보').
- 'personal_schedule': Question about the user's own schedule (e.g., 'what is my schedule', 'my events today', '내 일정').
- 'general_query': Any other question (e.g., '행사 정보', '색인된 정보').

Question: ""{question}""";

            var request = new { contents = new[] { new { parts = new[] { new { text = prompt } } } } };
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
                    var intent = parts[0].GetProperty("text").GetString()?.Trim().ToLower() ?? "general_query";
                    if (intent.Contains("personal_info")) return "personal_info";
                    if (intent.Contains("personal_schedule")) return "personal_schedule";
                }
            }
            return "general_query";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Intent classification failed with Gemini: {ex.Message}");
            return "general_query";
        }
    }

    public Task<float[]> GenerateEmbeddingAsync(string text)
    {
        throw new NotImplementedException();
    }
}