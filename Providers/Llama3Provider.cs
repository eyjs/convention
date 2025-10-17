// ----------------- Llama3Provider.cs (수정된 최종 버전) -----------------

using LocalRAG.Interfaces;
using LocalRAG.Models;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace LocalRAG.Providers;

public class Llama3Provider : ILlmProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<Llama3Provider> _logger;

    private readonly string _baseUrl;
    private readonly string _model;
    private readonly double _temperature;
    private readonly double _topP;

    // 의도 분류 시 사용할 카테고리 목록 (관리 용이)
    private readonly List<string> _intentCategories = new() { "PersonalInfo", "PersonalSchedule", "Event", "General", "Unknown" };

    public string ProviderName => "Llama3";

    public Llama3Provider(HttpClient httpClient, IConfiguration configuration, ILogger<Llama3Provider> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        var settings = configuration.GetSection("LlmSettings:Llama3");
        _baseUrl = settings["BaseUrl"] ?? "http://localhost:11434";
        _model = settings["Model"] ?? "llama3";
        _temperature = double.Parse(settings["Temperature"] ?? "0.3");
        _topP = double.Parse(settings["TopP"] ?? "0.9");
    }

    public async Task<string> GenerateResponseAsync(string prompt, string? context = null, List<ChatRequestMessage>? history = null, string? systemInstructionOverride = null)
    {
        // 이 메서드는 기존과 동일하게 유지합니다.
        try
        {
            var effectiveSystemPrompt = systemInstructionOverride ?? @"You are a helpful AI assistant. Always respond in Korean.";
            var finalUserPrompt = prompt;
            if (!string.IsNullOrWhiteSpace(context))
            {
                finalUserPrompt = "Your task is to act as a simple information presenter. " +
                                  "You must deliver the information provided in the [Context] below to the user **exactly as it is, without any summarization, omission, or modification.** " +
                                  "Present all sections, including personal details, the full schedule, and additional information, in a clear and well-formatted response in Korean.\n\n" +
                                  $"[Context]\n{context}\n\n" +
                                  "---\n" +
                      "Based on the context above, respond to the user's question. Remember, do not summarize or leave anything out.\n" +
                      $"User Question: {prompt}";
            }
            var messages = BuildLlama3ChatMessages(effectiveSystemPrompt, finalUserPrompt, history);
            var request = new
            {
                model = _model,
                messages,
                stream = false,
                options = new { temperature = _temperature, top_p = _topP }
            };

            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/chat", request);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
            var answer = result.GetProperty("message").GetProperty("content").GetString() ?? "답변을 생성할 수 없습니다.";
            return answer.Trim();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate response from Llama3");
            throw new InvalidOperationException($"Failed to generate response from Llama3: {ex.Message}", ex);
        }
    }

    private List<object> BuildLlama3ChatMessages(string systemPrompt, string userPrompt, List<ChatRequestMessage>? history)
    {
        var messages = new List<object> { new { role = "system", content = systemPrompt } };
        if (history != null)
        {
            foreach (var message in history)
            {
                var role = message.Role.ToLower() == "bot" || message.Role.ToLower() == "assistant" ? "assistant" : "user";
                messages.Add(new { role, content = message.Content });
            }
        }
        messages.Add(new { role = "user", content = userPrompt });
        return messages;
    }

    
    public async Task<string> ClassifyIntentAsync(string question, List<ChatRequestMessage>? history = null)
    {
        try
        {
            var systemPrompt = @$"Your task is to classify the user's question into one of the following categories: {string.Join(", ", _intentCategories)}.
Respond with ONLY the category name. Do not add any explanation or punctuation.";
            var userPrompt = $"Classify the following question: \"{question}\"";

            var messages = new List<object>
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            };

            var request = new
            {
                model = _model,
                messages,
                stream = false,
                options = new { temperature = 0.0 }
            };

            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/chat", request);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
            var rawIntent = result.GetProperty("message").GetProperty("content").GetString()?.Trim() ?? "Unknown";

            _logger.LogInformation("Raw intent result: {RawIntent}", rawIntent); // 디버깅용 로그

            // 견고한 파싱 로직: 응답 텍스트에 카테고리 이름이 포함되어 있는지 확인
            var parsedIntent = _intentCategories.FirstOrDefault(cat =>
                rawIntent.Contains(cat, StringComparison.OrdinalIgnoreCase)) ?? "Unknown";

            _logger.LogInformation("Parsed intent: {ParsedIntent}", parsedIntent); // 디버깅용 로그

            return parsedIntent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to classify intent with Llama3");
            return "Unknown";
        }
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        // 이 메서드는 기존과 동일하게 유지합니다.
        try
        {
            var embeddingModel = "nomic-embed-text";
            var request = new { model = embeddingModel, prompt = text };
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/embeddings", request);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
            var embeddingArray = result.GetProperty("embedding").EnumerateArray().Select(x => (float)x.GetDouble()).ToArray();
            return embeddingArray;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate embedding from Llama3");
            throw new InvalidOperationException($"Failed to generate embedding from Llama3: {ex.Message}", ex);
        }
    }
}