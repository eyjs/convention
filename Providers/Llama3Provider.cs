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

    public async Task<string> GenerateResponseAsync(string prompt, string? context = null)
    {
        try
        {
            var fullPrompt = context != null 
                ? $"Context: {context}\n\nQuestion: {prompt}\n\nAnswer based on the context provided:"
                : prompt;

            var request = new
            {
                model = _model,
                prompt = fullPrompt,
                stream = false,
                options = new
                {
                    temperature = 0.7,
                    top_p = 0.9,
                    max_tokens = 2048
                }
            };

            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/generate", request);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
            
            return result.GetProperty("response").GetString() ?? "No response generated";
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
            var request = new
            {
                model = _model,
                prompt = text
            };

            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/embeddings", request);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
            
            var embeddingArray = result.GetProperty("embedding").EnumerateArray()
                .Select(x => (float)x.GetDouble())
                .ToArray();
                
            return embeddingArray;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to generate embedding from Llama3: {ex.Message}", ex);
        }
    }
}
