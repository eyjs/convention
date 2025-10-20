using LocalRAG.Interfaces;
using LocalRAG.Models;
using System.Text.Json; // For potential metadata parsing if needed in the future
using LocalRAG.Models.DTOs; // Assuming SourceInfo is here

namespace LocalRAG.Services.Chat;

/// <summary>
/// RAG 검색 서비스 - Vector Store 검색을 담당하며, 검색 결과를 바탕으로 컨텍스트를 구축합니다.
/// </summary>
public class RagSearchService
{
    private readonly IEmbeddingService _embeddingService;
    private readonly IVectorStore _vectorStore;
    private readonly ILogger<RagSearchService> _logger;

    public RagSearchService(
        IEmbeddingService embeddingService,
        IVectorStore vectorStore,
        ILogger<RagSearchService> logger)
    {
        _embeddingService = embeddingService;
        _vectorStore = vectorStore;
        _logger = logger;
    }

    /// <summary>
    /// 질문과 가장 관련성이 높은 문서 조각들을 Vector Store에서 검색하여 LLM에게 제공할 컨텍스트를 생성합니다.
    /// </summary>
    public async Task<string?> BuildContextAsync(
        string question,
        int? conventionId,
        ChatUserContext? userContext)
    {
        try
        {
            var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(question);

            // 필터 구성: conventionId가 있을 때만 필터링하도록 수정
            Dictionary<string, object>? filter = null;
            if (conventionId.HasValue)
            {
                filter = new Dictionary<string, object> { { "convention_id", conventionId.Value } };
            }
            // (핵심 수정) 하드코딩된 { "type", "convention" } 필터를 제거하여
            // convention_info, daily_schedule 등 모든 관련 문서를 검색 대상으로 포함합니다.

            // Vector Store 검색 실행 (topK는 3~5개가 적절합니다)
            var results = await _vectorStore.SearchAsync(queryEmbedding, topK: 3, filter);

            if (!results.Any())
            {
                _logger.LogInformation("No relevant context found for question: {Question}", question);
                return null;
            }

            _logger.LogInformation("Found {Count} relevant documents for question", results.Count());

            // 검색된 컨텍스트 조각들을 명확한 구분자와 함께 조합합니다.
            return string.Join("\n\n---\n\n", results.Select(r => r.Content));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building RAG context");
            // 운영 환경에서는 사용자에게 직접적인 에러를 노출하지 않도록 null을 반환하거나,
            // 별도의 예외 처리 로직을 두는 것이 좋습니다.
            return "컨텍스트를 검색하는 중 오류가 발생했습니다.";
        }
    }

    /// <summary>
    /// 컨텍스트와 함께 출처(Source) 정보까지 반환하는 검색을 수행합니다.
    /// </summary>
    public async Task<(string? context, List<SourceInfo> sources)> BuildContextWithSourcesAsync(
        string question,
        int? conventionId,
        ChatUserContext? userContext)
    {
        try
        {
            var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(question);

            // BuildContextAsync와 동일하게 필터 로직 수정
            Dictionary<string, object>? filter = null;
            if (conventionId.HasValue)
            {
                filter = new Dictionary<string, object> { { "convention_id", conventionId.Value } };
            }

            var results = await _vectorStore.SearchAsync(queryEmbedding, topK: 3, filter);

            if (!results.Any())
            {
                return (null, new List<SourceInfo>());
            }

            var context = string.Join("\n\n---\n\n", results.Select(r => r.Content));

            var sources = results.Select(r => new SourceInfo
            {
                Content = r.Content,
                Similarity = r.Similarity,
                Type = r.Metadata?.GetValueOrDefault("type")?.ToString() ?? "unknown",
                ConventionId = ExtractConventionId(r.Metadata),
                ConventionTitle = r.Metadata?.GetValueOrDefault("title")?.ToString()
            }).ToList();

            return (context, sources);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building RAG context with sources");
            throw; // 컨트롤러 레벨에서 최종 예외 처리를 위해 re-throw
        }
    }

    /// <summary>
    /// 메타데이터에서 convention_id를 안전하게 추출합니다.
    /// </summary>
    private int? ExtractConventionId(Dictionary<string, object>? metadata)
    {
        if (metadata == null) return null;

        if (metadata.TryGetValue("convention_id", out var convIdObj))
        {
            // Vector Store에서 넘어온 데이터 타입에 따라 안전하게 변환
            if (convIdObj is JsonElement element && element.TryGetInt32(out var convId))
            {
                return convId;
            }
            if (convIdObj is int directInt)
            {
                return directInt;
            }
            if (convIdObj is long longInt && longInt <= int.MaxValue)
            {
                return (int)longInt;
            }
            if (int.TryParse(convIdObj.ToString(), out var parsedInt))
            {
                return parsedInt;
            }
        }

        return null;
    }
}