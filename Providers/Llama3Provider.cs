using LocalRAG.Interfaces;
using System.Text.Json;

namespace LocalRAG.Providers;

public class Llama3Provider : ILlmProvider
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _baseUrl;
    private readonly string _model;

    public string ProviderName => "Llama3";

    public Llama3Provider(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _baseUrl = _configuration["LlmSettings:Llama3:BaseUrl"] ?? "http://localhost:11434";
        _model = _configuration["LlmSettings:Llama3:Model"] ?? "llama3";
    }

    public async Task<string> GenerateResponseAsync(string prompt, string? context = null, string? userContext = null)
    {
        try
        {
            var systemPrompt = @"You are a helpful AI assistant for Star Tour, a travel company.
Your instructions are absolute.
1. ALWAYS respond in Korean. No exceptions.
2. Base your answers strictly on the provided 'Context'.
3. If the context does not contain the answer, you MUST say '정보가 부족하여 답변할 수 없습니다.'";

            if (!string.IsNullOrEmpty(userContext))
            {
                systemPrompt += $"\nIMPORTANT: {userContext}";
            }

            var fullPrompt = context != null
                ? $"Context:\n---\n{context}\n---\nBased on the context above, answer the following question in Korean.\nQuestion: {prompt}"
                : $"Answer the following question in Korean.\nQuestion: {prompt}";

            var request = new { model = _model, prompt = fullPrompt, system = systemPrompt, stream = false };
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/generate", request);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
            var answer = result.GetProperty("response").GetString() ?? "답변을 생성할 수 없습니다";
            return answer.Trim();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to generate response from Llama3: {ex.Message}", ex);
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