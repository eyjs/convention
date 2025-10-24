using Microsoft.AspNetCore.Mvc;
using LocalRAG.Interfaces;
using LocalRAG.Providers;
using LocalRAG.DTOs.AiModels;

namespace LocalRAG.Controllers.System;

[ApiController]
[Route("api/[controller]")]
public class LlmController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LlmController> _logger;

    public LlmController(IServiceProvider serviceProvider, ILogger<LlmController> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    [HttpPost("test")]
    public async Task<ActionResult<TestLlmResponse>> TestLlm([FromBody] TestLlmRequest request)
    {
        try
        {
            ILlmProvider provider;
            
            if (!string.IsNullOrEmpty(request.Provider))
            {
                // 특정 프로바이더 지정
                provider = request.Provider.ToLower() switch
                {
                    "llama3" => _serviceProvider.GetRequiredService<Llama3Provider>(),
                    "gemini" => _serviceProvider.GetRequiredService<GeminiProvider>(),
                    _ => throw new ArgumentException($"Unknown provider: {request.Provider}")
                };
            }
            else
            {
                // 기본 프로바이더 사용
                provider = _serviceProvider.GetRequiredService<ILlmProvider>();
            }

            var response = await provider.GenerateResponseAsync(request.Prompt);
            
            return Ok(new TestLlmResponse(
                response,
                provider.ProviderName,
                true
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to test LLM with provider: {Provider}", request.Provider);
            
            return Ok(new TestLlmResponse(
                string.Empty,
                request.Provider ?? "Unknown",
                false,
                ex.Message
            ));
        }
    }

    [HttpPost("test-embedding")]
    public async Task<ActionResult<object>> TestEmbedding([FromBody] string text)
    {
        try
        {
            var embeddingService = _serviceProvider.GetRequiredService<IEmbeddingService>();
            var embedding = await embeddingService.GenerateEmbeddingAsync(text);
            
            return Ok(new
            {
                Text = text,
                EmbeddingDimensions = embedding.Length,
                Embedding = embedding.Take(10).ToArray(), // 처음 10개만 표시
                Success = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate embedding");
            
            return Ok(new
            {
                Text = text,
                Success = false,
                Error = ex.Message
            });
        }
    }

    [HttpGet("providers")]
    public ActionResult<object> GetAvailableProviders()
    {
        return Ok(new
        {
            Providers = new[] { "llama3", "gemini" },
            Current = _serviceProvider.GetRequiredService<ILlmProvider>().ProviderName
        });
    }
}
