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
            var systemPrompt = @"당신은 친절하고 전문적인 AI 어시스턴트입니다. 
모든 답변은 반드시 한국어로만 작성해야 합니다.
답변 시 다음 규칙을 따르세요:
1. 모든 답변은 한국어로만 작성
2. 정확하고 명확한 정보 제공
3. 컨텍스트에 있는 정보를 기반으로 답변
4. 컨텍스트에 없는 내용은 추측하지 말 것";

            var fullPrompt = context != null 
                ? $"{systemPrompt}\n\n컨텍스트: {context}\n\n질문: {prompt}\n\n한국어로 답변:"
                : $"{systemPrompt}\n\n질문: {prompt}\n\n한국어로 답변:";

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
            
            var answer = result.GetProperty("response").GetString() ?? "답변을 생성할 수 없습니다";
            
            return answer;
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
