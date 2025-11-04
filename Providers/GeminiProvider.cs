using LocalRAG.DTOs.ChatModels;
using LocalRAG.Interfaces;
using LocalRAG.Entities;
using System.Text;
using System.Text.Json;

namespace LocalRAG.Providers;

public class GeminiProvider : ILlmProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _apiKey;
    private readonly string _model;

    public string ProviderName => "Gemini";

    // 기존 생성자 - DI 등록용 (IConfiguration 사용)
    public GeminiProvider(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _baseUrl = configuration["LlmSettings:Gemini:BaseUrl"] ?? "https://generativelanguage.googleapis.com/v1beta";
        _apiKey = configuration["LlmSettings:Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini API key not configured");
        _model = configuration["LlmSettings:Gemini:Model"] ?? "gemini-2.0-flash";
    }

    // DB 설정 기반 생성자 - 동적 생성용 (LlmSetting 사용)
    public GeminiProvider(HttpClient httpClient, LlmSetting setting)
    {
        _httpClient = httpClient;
        _baseUrl = setting.BaseUrl ?? "https://generativelanguage.googleapis.com/v1beta";
        _apiKey = setting.ApiKey ?? throw new InvalidOperationException("Gemini API key not configured in DB");
        _model = setting.ModelName ?? "gemini-2.0-flash";
    }

    public async Task<string> GenerateResponseAsync(string prompt, string? context = null, List<ChatRequestMessage>? history = null, string? systemInstructionOverride = null)
    {
        try
        {
            var effectiveSystemInstruction = systemInstructionOverride ?? @"You are a helpful AI assistant for Star Tour. Your instructions are absolute.
1. You MUST respond in Korean.
2. Base your answers strictly on the provided 'Context'.
3. If the context does not contain the answer, you MUST respond with '정보가 부족하여 답변할 수 없습니다.'";

            var fullPrompt = context != null
                ? $"Context:\n---\n{context}\n---\nBased on the context, answer the following question in Korean.\nQuestion: {prompt}"
                : $"Answer the following question in Korean.\nQuestion: {prompt}";

            var contents = new List<object>();
            if (history != null)
            {
                foreach (var message in history)
                {
                    contents.Add(new { role = message.Role == "assistant" ? "model" : "user", parts = new[] { new { text = message.Content } } });
                }
            }
            contents.Add(new { role = "user", parts = new[] { new { text = fullPrompt } } });

            var request = new
            {
                system_instruction = new { parts = new[] { new { text = effectiveSystemInstruction } } },
                contents
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

    public async Task<string> ClassifyIntentAsync(string question, List<ChatRequestMessage>? history = null)
    {
        try
        {
            var prompt = @$"Classify the user's question into ONE category. Respond with ONLY the category name.

Categories:
- 'personal_info': Questions about the user's own information (who am I, my name, my profile, 내 정보, 난 누구, 내 이름)
- 'personal_schedule': Questions about the user's schedule (my schedule, my events, my today, 내 일정, 내 스케줄, 오늘 내 일정, 나의 일정)
- 'event_query': Questions about the event/convention (event info, convention details, when is event, 행사 정보, 컨벤션 정보, 행사 언제)
- 'general_query': Everything else (weather, news, general knowledge, 날씨, 뉴스)

Examples:
- ""내 일정"" → personal_schedule
- ""내 일정 알려줘"" → personal_schedule
- ""오늘 내 일정은?"" → personal_schedule
- ""난 누구야?"" → personal_info
- ""내 정보 알려줘"" → personal_info
- ""행사 언제야?"" → event_query
- ""오늘 날씨"" → general_query

Question: ""{question}""

Category:";

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
                    var intentText = parts[0].GetProperty("text").GetString()?.Trim() ?? "general_query";
                    return intentText;
                }
            }
            return "general_query";
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to classify intent with Gemini: {ex.Message}", ex);
        }
    }
}
