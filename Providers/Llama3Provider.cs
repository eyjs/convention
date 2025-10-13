using LocalRAG.Interfaces;
using LocalRAG.Models;
using System.Text.Json;

namespace LocalRAG.Providers;

public class Llama3Provider : ILlmProvider
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _baseUrl;
    private readonly string _model;

    public string ProviderName => "Llama3";

    private readonly ILogger<Llama3Provider> _logger;

    public Llama3Provider(HttpClient httpClient, IConfiguration configuration, ILogger<Llama3Provider> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _baseUrl = _configuration["LlmSettings:Llama3:BaseUrl"] ?? "http://localhost:11434";
        _model = _configuration["LlmSettings:Llama3:Model"] ?? "llama3";
    }

    public async Task<string> GenerateResponseAsync(string prompt, string? context = null, List<ChatMessage>? history = null)
    {
        try
        {
            var systemPrompt = @"You are a helpful AI assistant for Star Tour. Your instructions are absolute.
1. ALWAYS respond in Korean.
2. Base your answers strictly on the provided 'Context'.
3. If the context does not contain the answer, you MUST say '정보가 부족하여 답변할 수 없습니다.'";

            var fullPrompt = context != null
                ? $"Context:\n---\n{context}\n---\nBased on the context, answer the following question in Korean.\nQuestion: {prompt}"
                : $"Answer the following question in Korean.\nQuestion: {prompt}";

            var messages = new List<object>();
            messages.Add(new { role = "system", content = systemPrompt });

            if (history != null)
            {
                foreach (var message in history)
                {
                    messages.Add(new { role = message.Role, content = message.Content });
                }
            }
            messages.Add(new { role = "user", content = fullPrompt });

            var request = new { model = _model, messages, stream = false };
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/chat", request);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
            var answer = result.GetProperty("message").GetProperty("content").GetString() ?? "답변을 생성할 수 없습니다";
            return answer.Trim();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to generate response from Llama3: {ex.Message}", ex);
        }
    }

    public async Task<string> ClassifyIntentAsync(string question, List<ChatMessage>? history = null)
    {
        try
        {
            var systemPrompt = @"Your task is to classify the user's question into one of the following categories: personal_info, personal_schedule, general_query.
Respond with ONLY the category name and nothing else.

- 'personal_info': Question about the user themselves (e.g., 'who am I', 'what is my info', '난 누구야', '내 정보').
- 'personal_schedule': Question about the user's own schedule (e.g., 'what is my schedule', 'my events today', '내 일정').
- 'general_query': Any other question (e.g., '행사 정보', '색인된 정보').

Question: ";

            var request = new
            {
                model = _model,
                prompt = $"{systemPrompt}\"{question}\"",
                stream = false,
                options = new { temperature = 0.0 }
            };

            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/generate", request);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
            var intent = result.GetProperty("response").GetString()?.Trim().ToLower() ?? "general_query";

            if (intent.Contains("personal_info")) return "personal_info";
            if (intent.Contains("personal_schedule")) return "personal_schedule";
            return "general_query";
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to classify intent with Llama3: {ex.Message}", ex);
        }
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        try
        {
            var request = new { model = _model, prompt = text };
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/embeddings", request);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
            var embeddingArray = result.GetProperty("embedding").EnumerateArray().Select(x => (float)x.GetDouble()).ToArray();
            return embeddingArray;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to generate embedding from Llama3: {ex.Message}", ex);
        }
    }
}