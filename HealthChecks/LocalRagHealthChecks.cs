using Microsoft.Extensions.Diagnostics.HealthChecks;
using LocalRAG.Interfaces;

namespace LocalRAG.HealthChecks;

public class LlmProviderHealthCheck : IHealthCheck
{
    private readonly ILlmProvider _llmProvider;
    private readonly ILogger<LlmProviderHealthCheck> _logger;

    public LlmProviderHealthCheck(ILlmProvider llmProvider, ILogger<LlmProviderHealthCheck> logger)
    {
        _llmProvider = llmProvider;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // 간단한 테스트 프롬프트로 헬스 체크
            var response = await _llmProvider.GenerateResponseAsync("Hello");
            
            if (string.IsNullOrEmpty(response))
            {
                return HealthCheckResult.Unhealthy($"LLM Provider '{_llmProvider.ProviderName}' returned empty response");
            }

            return HealthCheckResult.Healthy($"LLM Provider '{_llmProvider.ProviderName}' is working");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LLM Provider health check failed");
            return HealthCheckResult.Unhealthy($"LLM Provider '{_llmProvider.ProviderName}' failed: {ex.Message}");
        }
    }
}

public class VectorStoreHealthCheck : IHealthCheck
{
    private readonly IVectorStore _vectorStore;
    private readonly ILogger<VectorStoreHealthCheck> _logger;

    public VectorStoreHealthCheck(IVectorStore vectorStore, ILogger<VectorStoreHealthCheck> logger)
    {
        _vectorStore = vectorStore;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var documentCount = await _vectorStore.GetDocumentCountAsync();
            return HealthCheckResult.Healthy($"Vector Store is working with {documentCount} documents");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Vector Store health check failed");
            return HealthCheckResult.Unhealthy($"Vector Store failed: {ex.Message}");
        }
    }
}

public class EmbeddingServiceHealthCheck : IHealthCheck
{
    private readonly IEmbeddingService _embeddingService;
    private readonly ILogger<EmbeddingServiceHealthCheck> _logger;

    public EmbeddingServiceHealthCheck(IEmbeddingService embeddingService, ILogger<EmbeddingServiceHealthCheck> logger)
    {
        _embeddingService = embeddingService;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var embedding = await _embeddingService.GenerateEmbeddingAsync("test");
            
            if (embedding == null || embedding.Length != _embeddingService.EmbeddingDimensions)
            {
                return HealthCheckResult.Unhealthy("Embedding Service returned invalid embedding");
            }

            return HealthCheckResult.Healthy($"Embedding Service is working (dimensions: {_embeddingService.EmbeddingDimensions})");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Embedding Service health check failed");
            return HealthCheckResult.Unhealthy($"Embedding Service failed: {ex.Message}");
        }
    }
}
